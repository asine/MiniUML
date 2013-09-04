using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MiniUML.Model.ViewModels;

namespace MiniUML.View.Views
{
    /// <summary>
    /// Interaction logic for DocumentView.xaml
    /// </summary>
    public partial class DocumentView : UserControl
    {
        public DocumentView()
        {
            InitializeComponent();
            
            base.DataContextChanged += delegate(object sender, DependencyPropertyChangedEventArgs e)
            {
                DocumentViewModel viewModel = e.NewValue as DocumentViewModel;

                if (viewModel == null) return;
                
                // Pass a reference to the Visual representing the document to the view model.
                viewModel.v_CanvasView = this._documentVisual;
            };

            this._scrollViewer.PreviewMouseWheel += delegate(object sender, MouseWheelEventArgs e)
            {
                if (Keyboard.Modifiers == ModifierKeys.Control)
                {
                    this._zoomSlider.Value += e.Delta / 1000.0;
                    e.Handled = true;
                }
            };
        }

        private void _zoomSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (Math.Abs(e.NewValue - 1) < 0.09)
                _zoomSlider.Value = 1;

            this._zoomTextBlock.Text = (int)(_zoomSlider.Value * 100) + "%";
        }
    }
}