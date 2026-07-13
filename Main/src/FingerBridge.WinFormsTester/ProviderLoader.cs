using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using FingerBridge;
using FingerBridge.Providers.Mock;

namespace FingerBridge.WinFormsTester
{
    internal static class ProviderLoader
    {
        public static IList<ProviderItem> LoadProviders()
        {
            var providers = new List<ProviderItem>();
            AddFactory(providers, new MockBiometricServiceFactory(), null);

            foreach (var candidate in EnumeratePluginCandidates())
                TryLoadPlugin(providers, candidate);

            return providers
                .GroupBy(x => x.Factory == null ? x.DisplayName : x.Factory.ProviderKey, StringComparer.OrdinalIgnoreCase)
                .Select(x => x.First())
                .OrderBy(x => x.DisplayName)
                .ToList();
        }

        private static IEnumerable<string> EnumeratePluginCandidates()
        {
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var folders = new[]
            {
                baseDirectory,
                Path.Combine(baseDirectory, "plugins")
            };

            var fixedCandidates = new[]
            {
                Path.Combine(baseDirectory, "FingerBridge.Providers.FutronicFs80h.dll"),
                Path.Combine(baseDirectory, "FingerBridge.Providers.NitgenHamsterDx.dll"),
                Path.Combine(baseDirectory, "plugins", "FingerBridge.Providers.FutronicFs80h.dll"),
                Path.Combine(baseDirectory, "plugins", "FingerBridge.Providers.NitgenHamsterDx.dll")
            };

            foreach (var candidate in fixedCandidates)
                yield return candidate;

            foreach (var folder in folders)
            {
                if (!Directory.Exists(folder))
                    continue;

                foreach (var file in Directory.GetFiles(folder, "*.Biometrics.*.dll"))
                    yield return file;
                foreach (var file in Directory.GetFiles(folder, "FingerBridge.*.dll"))
                    yield return file;
            }
        }

        private static void TryLoadPlugin(List<ProviderItem> providers, string assemblyPath)
        {
            if (!File.Exists(assemblyPath))
                return;

            try
            {
                var assembly = Assembly.LoadFrom(assemblyPath);
                var factoryType = typeof(IBiometricServiceFactory);

                foreach (var type in assembly.GetTypes())
                {
                    if (type.IsAbstract || type.IsInterface)
                        continue;

                    if (!factoryType.IsAssignableFrom(type))
                        continue;

                    var factory = (IBiometricServiceFactory)Activator.CreateInstance(type);
                    AddFactory(providers, factory, assemblyPath);
                }
            }
            catch (ReflectionTypeLoadException ex)
            {
                var loaderMessages = ex.LoaderExceptions == null
                    ? ex.Message
                    : string.Join(" | ", ex.LoaderExceptions.Where(e => e != null).Select(e => e.Message).ToArray());

                providers.Add(new ProviderItem(null, Path.GetFileNameWithoutExtension(assemblyPath) + " - erro ao carregar plugin", false, loaderMessages, assemblyPath));
            }
            catch (Exception ex)
            {
                providers.Add(new ProviderItem(null, Path.GetFileNameWithoutExtension(assemblyPath) + " - erro ao carregar plugin", false, ex.Message, assemblyPath));
            }
        }

        private static void AddFactory(List<ProviderItem> providers, IBiometricServiceFactory factory, string assemblyPath)
        {
            string reason;
            bool available = factory.IsAvailable(out reason);

            providers.Add(new ProviderItem(factory, factory.DisplayName, available, reason, assemblyPath));
        }
    }

    internal sealed class ProviderItem
    {
        public ProviderItem(IBiometricServiceFactory factory, string displayName, bool available, string reason, string assemblyPath)
        {
            Factory = factory;
            DisplayName = displayName;
            Available = available;
            Reason = reason;
            AssemblyPath = assemblyPath;
        }

        public IBiometricServiceFactory Factory { get; private set; }
        public string DisplayName { get; private set; }
        public bool Available { get; private set; }
        public string Reason { get; private set; }
        public string AssemblyPath { get; private set; }

        public override string ToString()
        {
            return Available ? DisplayName : DisplayName + " (indisponivel)";
        }
    }
}
