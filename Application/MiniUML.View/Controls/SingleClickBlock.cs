using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MiniUML.View.Controls
{
    /// <summary>
    /// Interaction logic for SingleClickBlock.xaml
    /// </summary>
    public class SingleClickBlock : ContentControl
    {
        static SingleClickBlock()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SingleClickBlock), new FrameworkPropertyMetadata(typeof(SingleClickBlock)));
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed || this.IsKeyboardFocusWithin || _isRoutingClick) return;

            _isRoutingClick = true;

            try
            {
                RaiseEvent(e);
            }
            finally
            {
                _isRoutingClick = false;
            }

            base.OnMouseDown(e);
            this.Focus();
            e.Handled = true;
        }

        private bool _isRoutingClick;
    }
}