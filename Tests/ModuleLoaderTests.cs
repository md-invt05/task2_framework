using ModuleEngine;

namespace Tests;

public sealed class ModuleLoaderTests
{
    [Fact]
    public void LoadFromDirectory_ShouldFindModulesInExternalDlls()
    {
        var tempDirectory = Path.Combine(Path.GetTempPath(), $"module-loader-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDirectory);

        try
        {
            File.Copy(typeof(CoreModule.CoreModule).Assembly.Location, Path.Combine(tempDirectory, "CoreModule.dll"), overwrite: true);
            File.Copy(typeof(LoggingModule.LoggingModule).Assembly.Location, Path.Combine(tempDirectory, "LoggingModule.dll"), overwrite: true);

            var loader = new ModuleLoader();

            var modules = loader.LoadFromDirectory(tempDirectory);

            Assert.Contains(modules, module => string.Equals(module.Name, "Core", StringComparison.OrdinalIgnoreCase));
            Assert.Contains(modules, module => string.Equals(module.Name, "Logging", StringComparison.OrdinalIgnoreCase));
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, recursive: true);
            }
        }
    }
}
