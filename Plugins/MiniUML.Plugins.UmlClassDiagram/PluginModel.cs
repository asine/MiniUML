using System.Windows;
using MiniUML.Model.ViewModels;

namespace MiniUML.Plugins.UmlClassDiagram
{
    public class PluginModel : MiniUML.Framework.PluginModel
    {
        public PluginModel(MainWindowViewModel windowViewModel)
        {
            _pluginView = new PluginView();
            _pluginView.DataContext = new PluginViewModel(windowViewModel);
        }

        public override string Name
        {
            get { return "UML Diagram"; }
        }

        public override FrameworkElement View
        {
            get { return _pluginView; }
        }

        private FrameworkElement _pluginView;
    }
}
