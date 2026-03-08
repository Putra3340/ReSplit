using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using ReSplit.Models;
using ReSplit.Models.Form;
using ReSplit.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Threading.Tasks;

namespace ReSplit.Plugins
{
    public interface IReSplitHost
    {
        ObservableCollection<SplitsModel> Splits { get; }
        string IdentifierPath { get; }
        void SetStatus(string text);
        void UpdateIGT(TimeSpan value);
        void StartOrSplit();
        void Reset();

        void Shutdown(string dllPath);
    }
    public class ReSplitHost : IReSplitHost 
    {
        public string DllPath;
        public ReSplitHost(string dllPath = "")
        {
            DllPath = dllPath;
        }
        public ObservableCollection<SplitsModel> Splits => StaticBinding.Splits;


        string IReSplitHost.IdentifierPath => DllPath;

        public void Reset()
        {
            //CentralControls.ResetRun();
        }

        public void SetStatus(string text)
        {
            Dispatcher.UIThread.Post(() =>
            {
                MainWindow.Instance.Lbl_Status.Text = text;
            });
        }

        public void Shutdown(string dllPath)
        {
            PluginLoader.UnloadPlugin(dllPath);
        }

        public void StartOrSplit()
        {
            CentralControls.StartNewAttempt();
        }

        public void UpdateIGT(TimeSpan value)
        {
            Dispatcher.UIThread.Post(() =>
            {
                MainWindow.Instance.Lbl_IGT.Text = TimeSpanFormat.FormatNewTime(value);
            });
        }
    }

    public static class PluginLoader
    {
        private static readonly Dictionary<string, LoadedPlugin> _plugins = new(StringComparer.OrdinalIgnoreCase);

        public static IReadOnlyCollection<string> LoadedPluginPaths => _plugins.Keys.ToArray();

        public static async Task LoadAndInitialize(Window owner)
        {
            try
            {
                var files = await owner.StorageProvider.OpenFilePickerAsync(
                    new FilePickerOpenOptions
                    {
                        Title = "Select ReSplit Plugin(s)",
                        AllowMultiple = true,
                        FileTypeFilter = new[]
                        {
                            new FilePickerFileType("DLL")
                            {
                                Patterns = new[] { "*.dll" }
                            }
                        }
                    });

                if (files.Count == 0)
                    return;

                foreach (var file in files)
                {
                    string dllPath = file.Path.LocalPath;
                    await LoadPlugin(dllPath);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to pick plugin(s):");
                Debug.WriteLine(ex);
            }
        }

        public static Task LoadPlugin(string dllPath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dllPath))
                    throw new ArgumentException("DLL path is empty.", nameof(dllPath));

                dllPath = Path.GetFullPath(dllPath);

                if (_plugins.ContainsKey(dllPath))
                {
                    Debug.WriteLine($"Plugin already loaded: {dllPath}");
                    return Task.CompletedTask;
                }

                var loadedPlugin = LoadedPlugin.Load(dllPath);

                var host = new ReSplitHost(dllPath);
                loadedPlugin.Initialize(host);

                _plugins[dllPath] = loadedPlugin;

                Debug.WriteLine($"Loaded plugin: {loadedPlugin.PluginType.FullName}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to load plugin: {dllPath}");
                Debug.WriteLine(ex);
            }

            return Task.CompletedTask;
        }

        public static async Task ReloadPlugin(string dllPath)
        {
            if (string.IsNullOrWhiteSpace(dllPath))
                return;

            dllPath = Path.GetFullPath(dllPath);

            await UnloadPlugin(dllPath);
            await LoadPlugin(dllPath);
        }

        public static Task UnloadPlugin(string dllPath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dllPath))
                    return Task.CompletedTask;

                dllPath = Path.GetFullPath(dllPath);

                if (!_plugins.TryGetValue(dllPath, out var plugin))
                    return Task.CompletedTask;

                plugin.Dispose();
                _plugins.Remove(dllPath);

                ForceUnload();

                Debug.WriteLine($"Unloaded plugin: {dllPath}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to unload plugin: {dllPath}");
                Debug.WriteLine(ex);
            }

            return Task.CompletedTask;
        }

        public static Task UnloadAllPlugins()
        {
            foreach (var plugin in _plugins.Values.ToList())
            {
                try
                {
                    plugin.Dispose();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Failed while unloading plugin:");
                    Debug.WriteLine(ex);
                }
            }

            _plugins.Clear();
            ForceUnload();

            Debug.WriteLine("All plugins unloaded.");
            return Task.CompletedTask;
        }

        public static bool IsLoaded(string dllPath)
        {
            if (string.IsNullOrWhiteSpace(dllPath))
                return false;

            return _plugins.ContainsKey(Path.GetFullPath(dllPath));
        }

        private static void ForceUnload()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        private sealed class LoadedPlugin : IDisposable
        {
            public string OriginalPath { get; }
            public string ShadowCopyDirectory { get; }
            public string ShadowCopyDllPath { get; }
            public PluginLoadContext LoadContext { get; }
            public Assembly Assembly { get; }
            public Type PluginType { get; }
            private MethodInfo InitializeMethod { get; }

            private LoadedPlugin(
                string originalPath,
                string shadowCopyDirectory,
                string shadowCopyDllPath,
                PluginLoadContext loadContext,
                Assembly assembly,
                Type pluginType,
                MethodInfo initializeMethod)
            {
                OriginalPath = originalPath;
                ShadowCopyDirectory = shadowCopyDirectory;
                ShadowCopyDllPath = shadowCopyDllPath;
                LoadContext = loadContext;
                Assembly = assembly;
                PluginType = pluginType;
                InitializeMethod = initializeMethod;
            }

            public static LoadedPlugin Load(string dllPath)
            {
                if (!File.Exists(dllPath))
                    throw new FileNotFoundException("Plugin DLL not found.", dllPath);

                string tempDir = Path.Combine(
                    Path.GetTempPath(),
                    "LiveGuidePlugins",
                    Guid.NewGuid().ToString("N"));

                Directory.CreateDirectory(tempDir);

                string shadowDllPath = Path.Combine(tempDir, Path.GetFileName(dllPath));
                File.Copy(dllPath, shadowDllPath, overwrite: true);

                var loadContext = new PluginLoadContext(shadowDllPath);
                Assembly asm = loadContext.LoadFromAssemblyPath(shadowDllPath);

                Type? pluginType = asm.GetTypes().FirstOrDefault(IsValidPluginType);

                if (pluginType == null)
                {
                    loadContext.Unload();
                    throw new Exception(
                        "No valid plugin found. Expected a class with public static Initialize(IReSplitHost host).");
                }

                MethodInfo? init = pluginType.GetMethod(
                    "Initialize",
                    BindingFlags.Public | BindingFlags.Static,
                    binder: null,
                    types: new[] { typeof(IReSplitHost) },
                    modifiers: null);

                if (init == null)
                {
                    loadContext.Unload();
                    throw new Exception("Plugin has no valid Initialize(IReSplitHost host) method.");
                }

                return new LoadedPlugin(
                    dllPath,
                    tempDir,
                    shadowDllPath,
                    loadContext,
                    asm,
                    pluginType,
                    init);
            }

            public void Initialize(IReSplitHost host)
            {
                InitializeMethod.Invoke(null, new object[] { host });
            }

            public void Dispose()
            {
                try
                {
                    LoadContext.Unload();
                }
                finally
                {
                    TryDeleteDirectory(ShadowCopyDirectory);
                }
            }

            private static bool IsValidPluginType(Type type)
            {
                if (!type.IsClass)
                    return false;

                var init = type.GetMethod(
                    "Initialize",
                    BindingFlags.Public | BindingFlags.Static,
                    binder: null,
                    types: new[] { typeof(IReSplitHost) },
                    modifiers: null);

                if (init == null || init.ReturnType != typeof(void))
                    return false;

                var nameProp = type.GetProperty("Name", BindingFlags.Public | BindingFlags.Static);
                var descProp = type.GetProperty("Description", BindingFlags.Public | BindingFlags.Static);

                return nameProp?.PropertyType == typeof(string)
                    && descProp?.PropertyType == typeof(string);
            }

            private static void TryDeleteDirectory(string path)
            {
                try
                {
                    if (Directory.Exists(path))
                        Directory.Delete(path, recursive: true);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Failed to delete plugin temp directory:");
                    Debug.WriteLine(ex);
                }
            }
        }

        private sealed class PluginLoadContext : AssemblyLoadContext
        {
            private readonly AssemblyDependencyResolver _resolver;

            public PluginLoadContext(string pluginPath) : base(isCollectible: true)
            {
                _resolver = new AssemblyDependencyResolver(pluginPath);
            }

            protected override Assembly? Load(AssemblyName assemblyName)
            {
                string? assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
                if (assemblyPath != null)
                    return LoadFromAssemblyPath(assemblyPath);

                return null;
            }
        }
    }
}
