using System;
using System.IO;
using System.Reflection;
using System.Windows;
using MiniUML.Diagnostics;
using MiniUML.Framework;
using MiniUML.Model.ViewModels;

namespace MiniUML
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
 	            base.OnStartup(e);

                // Initialize SettingsManager.
                SettingsManager.Settings = MiniUML.Properties.Settings.Default;

                // Load theme.
                ThemeLoader.LoadThemeAssembly(MiniUML.Properties.Settings.Default.ThemeAssembly);
                DocumentViewModel.LoadThemeAssemblyDelegate = new LoadThemeAssemblyDelegate(ThemeLoader.LoadThemeAssembly);

                // Initialize models.
                vm_WindowViewModel = new MainWindowViewModel();
                
                // Load plugins.
                PluginLoader.LoadPlugins(MiniUML.Properties.Settings.Default.PluginDirectory, vm_WindowViewModel);

                // Create and show main window.
                IFactory mainWindowFactory = Application.Current.Resources["MainWindowFactory"] as IFactory;
                Window mainWindow = mainWindowFactory.CreateObject() as Window;
                mainWindow.DataContext = vm_WindowViewModel;
                mainWindow.Show();

                // If exceptions occured while loading, show them now.
                ExceptionManager.ShowErrorDialog(true);
            }
            catch (NotImplementedException ex)
            {
                // Catch and show unhandled exceptions before killing the process.
                ExceptionManager.RegisterCritical(ex,
                    "An error occured while starting the program.");
            }
        }

        #region ViewModels

        public MainWindowViewModel vm_WindowViewModel { get; private set; }

        #endregion

        private static class ThemeLoader
        {
            public static bool LoadThemeAssembly(string assemblyFile)
            {
                ResourceDictionary resourceDictionary;
                
                try
                {
                    // Load the theme assembly.
                    Assembly assembly = Assembly.LoadFrom(assemblyFile);
                    string packUri = String.Format(@"/{0};component/{1}", assembly.FullName, "SharedResources.xaml");
                    resourceDictionary = Application.LoadComponent(new Uri(packUri, UriKind.Relative)) as ResourceDictionary;
                }
                catch (Exception ex)
                {
                    ExceptionManager.Register(ex,
                        "No theme loaded.",
                        "A error occured while loading the theme resource dictionary.");

                    return false;
                }
                
                // Replace the current theme resource dictionary.
                if (currentThemeResourceDictionary != null)
                    Application.Current.Resources.MergedDictionaries.Remove(currentThemeResourceDictionary);
                currentThemeResourceDictionary = resourceDictionary;
                Application.Current.Resources.MergedDictionaries.Add(resourceDictionary);
                
                return true;
            }

            private static ResourceDictionary currentThemeResourceDictionary;
        }

        private static class PluginLoader
        {
            public static void LoadPlugins(string pluginDirectory, MainWindowViewModel windowViewModel)
            {
                string[] assemblyFiles = { };

                try
                {
                    try
                    {
                        // Get the names of all assembly files in the plugin directory.
                        assemblyFiles = Directory.GetFiles(pluginDirectory, "*.dll", SearchOption.AllDirectories);
                    }
                    catch (DirectoryNotFoundException ex)
                    {
                        // Plugin directory not was not found; create it.
                        Directory.CreateDirectory(pluginDirectory);

                        ExceptionManager.Register(ex,
                            "Plugin directory created; no plugins loaded.",
                            "The plugin directory was not found.");

                        return;
                    }
                }
                catch (Exception ex)
                {
                    ExceptionManager.RegisterCritical(ex,
                        "A error occured while accessing the plugin directory.");

                    return;
                }

                // Try to load plugins from each assembly.
                foreach (string assemblyFile in assemblyFiles)
                    loadPluginAssembly(assemblyFile, windowViewModel);

                if (PluginManager.PluginModels.Count == 0)
                    ExceptionManager.RegisterCritical(new Exception("No plugins loaded."), "Could not locate and/or load any plugins.");
            }

            private static void loadPluginAssembly(string assemblyFile, MainWindowViewModel windowViewModel)
            {
                Assembly assembly;

                try
                {
                    // Load the plugin assembly.
                    assembly = Assembly.LoadFrom(assemblyFile);

                    // Add an instance of each PluginModel found in the assembly to the plugin collection 
                    // and merge its resources into the plugin resource dictionary.
                    foreach (Type type in assembly.GetTypes())
                    {
                        if (!type.IsAbstract && typeof(PluginModel).IsAssignableFrom(type))
                        {
                            try
                            {
                                // Create PluginModel instance.
                                PluginModel pluginModel = Activator.CreateInstance(type, windowViewModel) as PluginModel;

                                // Plugin names must be unique
                                foreach (PluginModel p in PluginManager.PluginModels)
                                {
                                    if (p.Name == pluginModel.Name)
                                        throw new Exception("A plugin with the specified name has already been loaded.");
                                }

                                // Get the shared resources from the plugin.
                                ResourceDictionary sharedResources = pluginModel.Resources;

                                // If we got any resources, merge them into our plugin resource dictionary.
                                if (sharedResources != null)
                                    PluginManager.PluginResources.MergedDictionaries.Add(sharedResources);

                                // Add the plugin to our plugin collection.
                                PluginManager.PluginModels.Add(pluginModel);

                            }
                            catch (Exception ex)
                            {
                                ExceptionManager.Register(ex,
                                    "Plugin not loaded.",
                                    "An error occured while initializing a plugin found in assembly " + assemblyFile + ".");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    ExceptionManager.Register(ex,
                        "Plugins from the assembly was not loaded.",
                        "An error occured while loading plugin assembly " + assemblyFile + ".");

                    return;
                }
            }
        }
    }
}
