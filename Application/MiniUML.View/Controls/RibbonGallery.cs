using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MiniUML.Framework;

namespace MiniUML.View.Controls
{
    /// <summary>
    /// Interaction logic for RibbonGallery.xaml
    /// </summary>
    public class RibbonGallery : ListBox
    {
        static RibbonGallery()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RibbonGallery), new FrameworkPropertyMetadata(typeof(RibbonGallery)));
        }
        
        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseDown(e);
            _startPoint = e.GetPosition(null);
            
            //TODO: This works, but it's a bit too fragile...
            this.SelectedItem = e.Source;
        }

        protected override void OnPreviewMouseMove(MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && !_isDragging)
            {
                CommandButton selectedItem = this.SelectedItem as CommandButton;
                if (selectedItem == null) return;

                IDragableCommandModel cmd = selectedItem.CommandModel as IDragableCommandModel;
                if (cmd == null) return;

                Point position = e.GetPosition(null);

                if (Math.Abs(position.X - _startPoint.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(position.Y - _startPoint.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    DoDragDrop(e, cmd);
                    this.SelectedIndex = -1;
                }
            }
        }

        private void DoDragDrop(MouseEventArgs e, IDragableCommandModel cmd)
        {
            _isDragging = true;
            DataObject data = new DataObject(typeof(IDragableCommandModel), cmd);
            DragDropEffects de = DragDrop.DoDragDrop(this, data, DragDropEffects.Copy);
            _isDragging = false;
        }

        protected override void OnPreviewMouseUp(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseUp(e);
            this.SelectedIndex = -1;
        }

        private bool _isDragging;
        private Point _startPoint;
    }
}
