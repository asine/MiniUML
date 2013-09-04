using System.ComponentModel;
using System.Windows;
using MiniUML.Framework;

namespace MiniUML.Model.ViewModels
{
    /// <summary>
    /// Class representing the view model for the main window of the application.
    /// This is the root of the view model tree.
    /// </summary>
    public class MainWindowViewModel : ViewModel
    {
        public MainWindowViewModel()
        {
            // Create the view models.
            vm_RibbonViewModel = new RibbonViewModel(this);
            vm_DocumentViewModel = new DocumentViewModel(this);

            updateWindowTitle();

            vm_DocumentViewModel.PropertyChanged += delegate(object sender, PropertyChangedEventArgs e)
            {
                if (e.PropertyName == "prop_DocumentFileName") updateWindowTitle();
            };

            vm_DocumentViewModel.dm_DocumentDataModel.PropertyChanged += delegate(object sender, PropertyChangedEventArgs e)
            {
                if (e.PropertyName == "HasUnsavedData") updateWindowTitle();
            };
        }

        public string prop_WindowTitle
        {
            get { return _windowTitle; }
        }

        private void updateWindowTitle()
        {
            string fileName = vm_DocumentViewModel.prop_DocumentFileName;

            // If the current document has unsaved changes, add a '*' at the end of the file name.
            if (vm_DocumentViewModel.dm_DocumentDataModel.HasUnsavedData)
                fileName += "*";

            _windowTitle = fileName + " - " + Application.Current.Resources["ApplicationName"];

            SendPropertyChanged("prop_WindowTitle");
        }

        private string _windowTitle;

        #region View models

        public RibbonViewModel vm_RibbonViewModel { get; private set; }

        public DocumentViewModel vm_DocumentViewModel { get; private set; }

        #endregion
    }
}
