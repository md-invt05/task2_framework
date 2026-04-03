using ModuleContracts;

namespace ModuleEngine;

public sealed class ModuleCatalog
{
    public IReadOnlyList<IAppModule> BuildExecutionPlan(
        IEnumerable<IAppModule> discoveredModules,
        IEnumerable<string> enabledModuleNames)
    {
        ArgumentNullException.ThrowIfNull(discoveredModules);
        ArgumentNullException.ThrowIfNull(enabledModuleNames);

        var availableModules = discoveredModules.ToList();
        var requestedNames = enabledModuleNames
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .ToList();

        if (requestedNames.Count == 0)
        {
            throw new ModuleLoadException("В appsettings.json не указан ни один модуль для запуска.");
        }

        var availableByName = BuildAvailableModulesDictionary(availableModules);
        var selectedByName = new Dictionary<string, IAppModule>(StringComparer.OrdinalIgnoreCase);
        var selectedModules = new List<IAppModule>();

        foreach (var requestedName in requestedNames)
        {
            if (!availableByName.TryGetValue(requestedName, out var module))
            {
                throw new ModuleLoadException($"Модуль не найден, имя модуля {requestedName}");
            }

            if (selectedByName.ContainsKey(module.Name))
            {
                continue;
            }

            selectedByName[module.Name] = module;
            selectedModules.Add(module);
        }

        foreach (var module in selectedModules)
        {
            foreach (var dependency in module.Requires)
            {
                if (string.IsNullOrWhiteSpace(dependency))
                {
                    throw new ModuleLoadException($"У модуля {module.Name} указана пустая зависимость.");
                }

                if (!selectedByName.ContainsKey(dependency))
                {
                    throw new ModuleLoadException(
                        $"Не хватает модуля для зависимости, модуль {module.Name} требует {dependency}");
                }
            }
        }

        var orderedModules = new List<IAppModule>();
        var states = new Dictionary<string, VisitState>(StringComparer.OrdinalIgnoreCase);
        var stack = new List<string>();

        foreach (var module in selectedModules)
        {
            Visit(module);
        }

        return orderedModules;

        void Visit(IAppModule module)
        {
            if (states.TryGetValue(module.Name, out var state))
            {
                if (state == VisitState.Visited)
                {
                    return;
                }

                if (state == VisitState.Visiting)
                {
                    var cycleStartIndex = stack.FindIndex(name => string.Equals(name, module.Name, StringComparison.OrdinalIgnoreCase));
                    var cycleModules = cycleStartIndex >= 0
                        ? stack.Skip(cycleStartIndex).ToArray()
                        : [module.Name];

                    throw new ModuleLoadException(
                        $"Обнаружена циклическая зависимость модулей: {string.Join(", ", cycleModules)}");
                }
            }

            states[module.Name] = VisitState.Visiting;
            stack.Add(module.Name);

            foreach (var dependencyName in module.Requires)
            {
                Visit(selectedByName[dependencyName]);
            }

            stack.RemoveAt(stack.Count - 1);
            states[module.Name] = VisitState.Visited;
            orderedModules.Add(module);
        }
    }

    private static Dictionary<string, IAppModule> BuildAvailableModulesDictionary(IEnumerable<IAppModule> discoveredModules)
    {
        var duplicateNames = discoveredModules
            .GroupBy(module => module.Name, StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() > 1)
            .Select(group => group.First().Name)
            .ToArray();

        if (duplicateNames.Length > 0)
        {
            throw new ModuleLoadException(
                $"Обнаружены модули с одинаковым именем: {string.Join(", ", duplicateNames)}");
        }

        var modulesByName = new Dictionary<string, IAppModule>(StringComparer.OrdinalIgnoreCase);

        foreach (var module in discoveredModules)
        {
            if (string.IsNullOrWhiteSpace(module.Name))
            {
                throw new ModuleLoadException("Обнаружен модуль с пустым именем.");
            }

            modulesByName[module.Name] = module;
        }

        return modulesByName;
    }

    private enum VisitState
    {
        Visiting,
        Visited
    }
}
