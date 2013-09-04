using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MiniUML.Framework;
using MiniUML.Model.DataModels;
using MiniUML.Model.ViewModels;

namespace MiniUML.View.Views
{
    /// <summary>
    /// Interaction logic for XmlView.xaml
    /// </summary>
    public partial class XmlView : UserControl
    {
        public XmlView()
        {
            InitializeComponent();
        }

        private void documentDataModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            DocumentDataModel dataModel = sender as DocumentDataModel;

            if (e.PropertyName != "ObservableDocumentRoot")
                return;
            
            XmlViewModel viewModel = base.DataContext as XmlViewModel;

            if (dataModel.State == DataModel.ModelState.Invalid && viewModel.prop_DocumentChanged)
                return;
            
            _xmlTextBox.Text = dataModel.DocumentRoot.ToString();

            if (viewModel != null)
                viewModel.prop_DocumentChanged = false;
        }

        private void expander_Expanded(object sender, RoutedEventArgs e)
        {
            _updateDesignerButton.Visibility = Visibility.Visible;

            XmlViewModel viewModel = base.DataContext as XmlViewModel;

            if (viewModel != null)
            {
                _xmlTextBox.Text = viewModel._DocumentViewModel.dm_DocumentDataModel.DocumentRoot.ToString();
                viewModel._DocumentViewModel.dm_DocumentDataModel.PropertyChanged += documentDataModel_PropertyChanged;
            }

        }

        private void expander_Collapsed(object sender, RoutedEventArgs e)
        {
            _updateDesignerButton.Visibility = Visibility.Hidden;

            XmlViewModel viewModel = base.DataContext as XmlViewModel;

            if (viewModel != null) return;
            {
                _xmlTextBox.Text = string.Empty;
                viewModel._DocumentViewModel.dm_DocumentDataModel.PropertyChanged -= documentDataModel_PropertyChanged;
                viewModel.prop_DocumentChanged = false;
            }

        }

        private void xmlTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            XmlViewModel viewModel = base.DataContext as XmlViewModel;

            if (viewModel != null)
                viewModel.prop_DocumentChanged = true;
        }

        private void Thumb_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            double maxHeight = ((FrameworkElement) Parent).ActualHeight - 20; // 20 = estimated expander height.
            if (maxHeight < 0) maxHeight = 0;

            double height = _xmlTextBox.Height - e.VerticalChange;

            if (height > maxHeight) height = maxHeight;
            else if (height <= 25) height = 25;
            
            _xmlTextBox.Height = height;
        }

        private void updateDesignerButtonContainer_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }
    }
}
