using System;
using System.Windows;

namespace MiniUML.Framework
{
    /// <summary>
    /// Base class from which all plugins must derive in order to be discovered.
    /// This class serves as a common interface to the basic elements of a plugin.
    /// </summary>
    public abstract class PluginModel
    {
        public PluginModel()
        {
            _resources = loadResourceDictionary("/SharedResources.xaml");
        }

        /// <summary>
        /// Gets the name of the plugin.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Gets the "view" trough which the user interacts with the plugin.
        /// </summary>
        public abstract FrameworkElement View { get; }

        /// <summary>
        /// Gets the resources needed for the plugin to function in the host application.
        /// </summary>
        public ResourceDictionary Resources
        {
            get { return _resources; }
        }

        /// <summary>
        /// Utility method used to load resource dictionaries from the assembly.
        /// </summary>
        /// <param name="uri">A relative path to the resource dictionary.</param>
        /// <returns>An instance of the specified resource dictionary.</returns>
        private ResourceDictionary loadResourceDictionary(string uri)
        {
            // Get a relative path to the resource dictionary.
            string assemblyName = this.GetType().Assembly.FullName;
            Uri resourceDictionaryUri = new Uri(String.Format(@"{0};component/{1}", assemblyName, uri), UriKind.Relative);

            // Load the resources.
            return Application.LoadComponent(resourceDictionaryUri) as ResourceDictionary;
        }

        private ResourceDictionary _resources;
    }
}
