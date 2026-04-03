using System.Reflection;
using System.Runtime.Loader;
using ModuleContracts;

namespace ModuleEngine;

public sealed class ModuleLoader
{
    public IReadOnlyCollection<IAppModule> LoadFromDirectory(string modulesDirectory)
    {
        if (!Directory.Exists(modulesDirectory))
        {
            throw new ModuleLoadException($"Каталог модулей не найден: {modulesDirectory}");
        }

        var modules = new List<IAppModule>();
        var dllFiles = Directory
            .EnumerateFiles(modulesDirectory, "*.dll", SearchOption.TopDirectoryOnly)
            .OrderBy(Path.GetFileName, StringComparer.OrdinalIgnoreCase);

        foreach (var dllFile in dllFiles)
        {
            var assembly = LoadAssembly(dllFile);

            foreach (var moduleType in GetModuleTypes(assembly))
            {
                if (moduleType.GetConstructor(Type.EmptyTypes) is null)
                {
                    throw new ModuleLoadException(
                        $"Тип модуля {moduleType.FullName} должен иметь публичный конструктор без параметров.");
                }

                if (Activator.CreateInstance(moduleType) is not IAppModule module)
                {
                    throw new ModuleLoadException(
                        $"Не удалось создать экземпляр модуля {moduleType.FullName}.");
                }

                modules.Add(module);
            }
        }

        return modules;
    }

    private static Assembly LoadAssembly(string dllFile)
    {
        var assemblyName = AssemblyName.GetAssemblyName(dllFile);
        var alreadyLoaded = AssemblyLoadContext.Default.Assemblies.FirstOrDefault(
            assembly => AssemblyName.ReferenceMatchesDefinition(assembly.GetName(), assemblyName));

        return alreadyLoaded ?? AssemblyLoadContext.Default.LoadFromAssemblyPath(Path.GetFullPath(dllFile));
    }

    private static IEnumerable<Type> GetModuleTypes(Assembly assembly)
    {
        try
        {
            return assembly
                .GetTypes()
                .Where(type => typeof(IAppModule).IsAssignableFrom(type) && type is { IsAbstract: false, IsInterface: false })
                .ToArray();
        }
        catch (ReflectionTypeLoadException exception)
        {
            return exception.Types
                .Where(type => type is not null && typeof(IAppModule).IsAssignableFrom(type) && type is { IsAbstract: false, IsInterface: false })
                .Cast<Type>()
                .ToArray();
        }
    }
}
