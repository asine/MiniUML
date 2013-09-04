using System.Collections.ObjectModel;
using System.Windows;

namespace MiniUML.Framework
{
    public static class PluginManager
    {
        public static ObservableCollection<PluginModel> PluginModels
        {
            get { return _pluginModels; }
        }

        public static ResourceDictionary PluginResources
        {
            get { return _pluginResources; }
        }

        private static ObservableCollection<PluginModel> _pluginModels = new ObservableCollection<PluginModel>();
        private static ResourceDictionary _pluginResources = new ResourceDictionary();
    }
}
