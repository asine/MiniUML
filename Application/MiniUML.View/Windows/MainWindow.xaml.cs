using System.ComponentModel;
using System.Windows;
using MiniUML.Framework;
using MiniUML.Model.ViewModels;

namespace MiniUML.View.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MainWindowViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();
            
            base.DataContextChanged += delegate(object sender, DependencyPropertyChangedEventArgs e)
            {
                _viewModel = e.NewValue as MainWindowViewModel;

                if (_viewModel == null) return;

                // Create command bindings.
                CreateCommandBinding.SetCommands(this, 
                    _viewModel.vm_DocumentViewModel.cmd_New,
                    _viewModel.vm_DocumentViewModel.cmd_Open,
                    _viewModel.vm_DocumentViewModel.cmd_Save,
                    _viewModel.vm_DocumentViewModel.cmd_SaveAs,
                    _viewModel.vm_DocumentViewModel.cmd_Print,
                    _viewModel.vm_DocumentViewModel.cmd_Undo,
                    _viewModel.vm_DocumentViewModel.cmd_Redo,
                    _viewModel.vm_DocumentViewModel.vm_CanvasViewModel.cmd_Copy,
                    _viewModel.vm_DocumentViewModel.vm_CanvasViewModel.cmd_Cut,
                    _viewModel.vm_DocumentViewModel.vm_CanvasViewModel.cmd_Paste,
                    _viewModel.vm_DocumentViewModel.vm_CanvasViewModel.cmd_Delete,
                    _viewModel.vm_DocumentViewModel.vm_CanvasViewModel.cmd_Select);
            };
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (_viewModel != null && !_viewModel.vm_DocumentViewModel.QuerySaveChanges())
                e.Cancel = true;
            else
                base.OnClosing(e);
        }
    }

    public class MainWindowFactory : IFactory
    {
        private static MainWindow mainWindow = null;

        /// <summary>
        /// Creates the application main window, or returns a reference to the existing window if already created. Not thread-safe.
        /// </summary>
        public object CreateObject()
        {
            if (mainWindow == null) mainWindow = new MainWindow();
            return mainWindow;
        }
    }
}
