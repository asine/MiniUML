using MiniUML.Framework;

namespace MiniUML.Model.ViewModels
{
    public class RibbonViewModel : ViewModel
    {
        public RibbonViewModel(MainWindowViewModel windowViewModel)
        {
            // Store a reference to the parent view model.
            _WindowViewModel = windowViewModel;
        }

        #region View models

        public MainWindowViewModel _WindowViewModel { get; private set; }

        #endregion
    }
}
