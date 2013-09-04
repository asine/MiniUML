using MiniUML.Framework;
using MiniUML.Model.ViewModels;

namespace MiniUML.Plugins.Home
{
    public class PluginViewModel : ViewModel
    {
        public PluginViewModel(MainWindowViewModel windowViewModel)
        {
            // Store a reference to the parent view model.
            _WindowViewModel = windowViewModel;
        }

        #region View models

        public MainWindowViewModel _WindowViewModel { get; private set; }

        #endregion
    }
}
