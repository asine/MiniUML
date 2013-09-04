using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Linq;
using MiniUML.Diagnostics;
using MiniUML.Framework;
using MiniUML.Model.ViewModels;
using MiniUML.View.Controls;

namespace MiniUML.View.Views
{
    public delegate void LayoutUpdatedHandler();

    /// <summary>
    /// Interaction logic for CanvasView.xaml
    /// </summary>
    public partial class CanvasView : UserControl, ICanvasViewMouseHandler
    {
        public CanvasView()
        {
            this.InitializeComponent();

            this.DataContextChanged += delegate(object sender, DependencyPropertyChangedEventArgs e)
            {
                if (_CanvasViewModel != null) _CanvasViewModel.SelectionChanged -= model_SelectionChanged;
                _CanvasViewModel = (CanvasViewModel)DataContext;
                if (_CanvasViewModel != null) _CanvasViewModel.SelectionChanged += model_SelectionChanged;
            };
        }

        public CanvasViewModel _CanvasViewModel { get; private set; }

        #region Drag/Drop functionality

        /// <summary>
        /// Handles the Drop event of the canvas.
        /// When a IDragableCommand, which represents an item from a ribbon gallery, is dropped on the canvas, its OnDragDropExecute is called.
        /// </summary>
        protected override void OnDrop(DragEventArgs e)
        {
            base.OnDrop(e);

            IDragableCommandModel cmd = (IDragableCommandModel)e.Data.GetData(typeof(IDragableCommandModel));

            if (cmd != null)
            {
                cmd.OnDragDropExecute(e.GetPosition(_itemsControl));
                e.Handled = true;
                return;
            }

            string fileName = IsSingleFile(e);
            if (fileName != null)
            {
                //Check if the datamodel is ready
                if (!(_CanvasViewModel._DocumentViewModel.dm_DocumentDataModel.State == DataModel.ModelState.Ready ||
                   _CanvasViewModel._DocumentViewModel.dm_DocumentDataModel.State == DataModel.ModelState.Invalid))
                    return;

                Application.Current.MainWindow.Activate();
                if (!_CanvasViewModel._DocumentViewModel.QuerySaveChanges()) return;

                try
                {
                    // Open the document.
                    _CanvasViewModel._DocumentViewModel.LoadFile(fileName);
                }
                catch (Exception ex)
                {
                    ExceptionManager.Register(ex,
                        "Operation aborted.",
                        "An error occured while opening the file " + fileName + ".");

                    ExceptionManager.ShowErrorDialog(true);
                }
                return;
            }
        }

        protected override void OnDragOver(DragEventArgs e)
        {
            base.OnDragOver(e);
            e.Effects = DragDropEffects.None;
            
            if(e.Data.GetDataPresent(typeof(IDragableCommandModel)))
            {
                e.Effects = DragDropEffects.Copy;
            }
            else if (IsSingleFile(e) != null)
            {
                e.Effects = DragDropEffects.Copy;
            }

            e.Handled = true;
        }

        // Standard method to check the drag data; found in documentation.
        // If the data object in args is a single file, this method will return the filename.
        // Otherwise, it returns null.
        private string IsSingleFile(DragEventArgs args)
        {
            // Check for files in the hovering data object.
            if (args.Data.GetDataPresent(DataFormats.FileDrop, true))
            {
                string[] fileNames = args.Data.GetData(DataFormats.FileDrop, true) as string[];
                // Check fo a single file or folder.
                if (fileNames.Length == 1)
                {
                    // Check for a file (a directory will return false).
                    if (File.Exists(fileNames[0]))
                    {
                        // At this point we know there is a single file.
                        return fileNames[0];
                    }
                }
            }
            return null;
        }


        #endregion

        #region Select / move functionality

        public static readonly DependencyProperty CustomDragProperty
            = DependencyProperty.RegisterAttached("CustomDrag", typeof(bool), typeof(CanvasView),
            new FrameworkPropertyMetadata(false));

        public static void SetCustomDrag(UIElement element, bool value)
        {
            element.SetValue(CustomDragProperty, value);
        }

        public static bool GetCustomDrag(UIElement element)
        {
            return (bool)element.GetValue(CustomDragProperty);
        }

        /// <summary>
        /// Handles the PreviewMouseDown event of the canvas.
        /// If CanvasViewMouseHandler is set, we handle all mouse button events. Otherwise, only left-clicks.
        /// </summary>
        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            if (_gotMouseDown || (_CanvasViewModel.CanvasViewMouseHandler == null && e.ChangedButton != MouseButton.Left))
            {
                base.OnPreviewMouseDown(e);
                return;
            }

            beginMouseOperation();

            _dragStart = e.GetPosition(_itemsControl);
            _dragShape = GetShapeAt(_dragStart);

            e.Handled = (_CanvasViewModel.CanvasViewMouseHandler != null) || (_CanvasViewModel.prop_SelectedShapes.Count > 1);
        }

        /// <summary>
        /// Handles the PreviewMouseUp event of the canvas.
        /// </summary>
        protected override void OnPreviewMouseMove(MouseEventArgs e)
        {
            if (!_gotMouseDown) return;

            Point position = e.GetPosition(_itemsControl);
            Vector dragDelta = position - _dragStart;

            if (!IsMouseCaptured)
            {
                if (e.LeftButton != MouseButtonState.Pressed) return; // We're not dragging anything.

                //
                if (IsMouseCaptureWithin) return; // This CanvasView is not responsible for the dragging.

                if (Math.Abs(dragDelta.X) < SystemParameters.MinimumHorizontalDragDistance &&
                    Math.Abs(dragDelta.Y) < SystemParameters.MinimumVerticalDragDistance) return;

                _currentMouseHandler.OnShapeDragBegin(position, _dragShape);

                CaptureMouse();
            }

            _currentMouseHandler.OnShapeDragUpdate(position, dragDelta);

            // The new "drag start" is the current mouse position.
            _dragStart = position;
        }

        protected override void OnPreviewMouseUp(MouseButtonEventArgs e)
        {
            if (!_gotMouseDown) return;

            try
            {
                Point position = e.GetPosition(_itemsControl);

                if (!IsMouseCaptured)
                {
                    _currentMouseHandler.OnShapeClick(_dragShape);
                    return;
                }

                ReleaseMouseCapture();

                _currentMouseHandler.OnShapeDragUpdate(position, position - _dragStart);
                _currentMouseHandler.OnShapeDragEnd(position, GetShapeAt(position));
            }
            finally
            {
                endMouseOperation();
            }

            // HACK: Work-around for bug 4
            this._CanvasViewModel._DocumentViewModel.dm_DocumentDataModel.Undo();
            this._CanvasViewModel._DocumentViewModel.dm_DocumentDataModel.Redo();
        }

        private void beginMouseOperation()
        {
            DebugUtilities.Assert(_gotMouseDown == false, "beginMouseOperation called when already in mouse operation");
            _gotMouseDown = true;

            // Use the handler specified on Model, if not null. Otherwise, use ourself.
            _currentMouseHandler = _CanvasViewModel.CanvasViewMouseHandler != null ? _CanvasViewModel.CanvasViewMouseHandler : this;

            // Don't create undo states at every drag update.
            _CanvasViewModel._DocumentViewModel.dm_DocumentDataModel.BeginOperation("CanvasView mouse operation");
        }

        private void endMouseOperation()
        {
            _gotMouseDown = false;
            _currentMouseHandler = null;

            // Re-enable the data model.
            _CanvasViewModel._DocumentViewModel.dm_DocumentDataModel.EndOperation("CanvasView mouse operation");
        }

        /// <summary>
        /// Handles the SelectionChanged event of the view model.
        /// When the collection of selected shapes changes, the Selector.IsSelectedProperty is updated for all elements on the canvas.
        /// </summary>
        private void model_SelectionChanged(object sender, EventArgs e)
        {
            foreach (XElement shape in _itemsControl.Items)
            {
                _itemsControl.ItemContainerGenerator.ContainerFromItem(shape).SetValue(
                    Selector.IsSelectedProperty, _CanvasViewModel.prop_SelectedShapes.Contains(shape));
            }
        }

        private bool _gotMouseDown = false;
        private ICanvasViewMouseHandler _currentMouseHandler = null;
        private Point _dragStart; // Mouse position at drag start / last drag update.
        private XElement _dragShape; // Shape under mouse at drag start.

        #endregion
    
        #region ICanvasViewMouseHandler Members

        // Encapsulates functionality of shape selection by mouse (e.g. using ctrl to toggle selection).
        void ICanvasViewMouseHandler.OnShapeClick(XElement shape)
        {
            if ((Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
            {
                if (shape == null) return;

                if (_CanvasViewModel.prop_SelectedShapes.Contains(shape))
                    _CanvasViewModel.prop_SelectedShapes.Remove(shape);
                else _CanvasViewModel.prop_SelectedShapes.Add(shape);
            }
            else _CanvasViewModel.SelectShape(shape);
        }

        void ICanvasViewMouseHandler.OnShapeDragBegin(Point position, XElement shape)
        {
            if (!_CanvasViewModel.prop_SelectedShapes.Contains(_dragShape))
                ((ICanvasViewMouseHandler)this).OnShapeClick(_dragShape);

            /*
            for (int i = Model.prop_SelectedShapes.Count - 1; i >= 0; i--)
            {
                XElement e = Model.prop_SelectedShapes[i];
                if ((bool)ControlFromElement(e).GetValue(CanvasView.CustomDragProperty))
                    Model.prop_SelectedShapes.RemoveAt(i);
            }
             */
        }

        void ICanvasViewMouseHandler.OnShapeDragUpdate(Point position, Vector delta)
        {
            foreach (XElement shape in _CanvasViewModel.prop_SelectedShapes)
            {
                FrameworkElement control = ControlFromElement(shape);

                if ((bool)control.GetValue(CanvasView.CustomDragProperty)) continue;

                Point origin = shape.GetPositionAttributes();

                Point p = origin + delta;

                if (p.X < 0) p.X = 0;
                else if (p.X + control.ActualWidth > _itemsControl.ActualWidth) p.X = _itemsControl.ActualWidth - control.ActualWidth;

                if (p.Y < 0) p.Y = 0;
                else if (p.Y + control.ActualHeight > _itemsControl.ActualHeight) p.Y = _itemsControl.ActualHeight - control.ActualHeight;

                shape.SetPositionAttributes(p);

                ISnapTarget ist = control as ISnapTarget;
                if (ist != null) ist.NotifySnapTargetUpdate(new SnapTargetUpdateEventArgs(p - origin));
            }
        }

        void ICanvasViewMouseHandler.OnShapeDragEnd(Point position, XElement element)
        {
            CanvasViewModel viewModel = this.DataContext as CanvasViewModel;
        }

        void ICanvasViewMouseHandler.OnCancelMouseHandler()
        {
 	        throw new NotImplementedException();
        }

        #endregion
        
        #region Coercion

        public void NotifyOnLayoutUpdated(LayoutUpdatedHandler handler)
        {
            _layoutUpdatedHandlers.Add(handler);
        }

        private void canvas_LayoutUpdated(object sender, EventArgs e)
        {
            if (_CanvasViewModel == null) return;

            HashSet<LayoutUpdatedHandler> set = _layoutUpdatedHandlers;
            if (set.Count == 0) return;
            _layoutUpdatedHandlers = new HashSet<LayoutUpdatedHandler>();

            try
            {
                _CanvasViewModel._DocumentViewModel.dm_DocumentDataModel.BeginOperation("CanvasView.canvas_LayoutUpdated");

                foreach (LayoutUpdatedHandler handler in set) handler();
            }
            finally
            {
                _CanvasViewModel._DocumentViewModel.dm_DocumentDataModel.EndOperationWithoutCreatingUndoState("CanvasView.canvas_LayoutUpdated");
            }
        }

        private HashSet<LayoutUpdatedHandler> _layoutUpdatedHandlers = new HashSet<LayoutUpdatedHandler>();

        #endregion

        #region Utility methods

        public static CanvasView GetCanvasView(DependencyObject obj)
        {
            while (obj != null)
            {
                CanvasView cv = obj as CanvasView;
                if (cv != null) return cv;
                obj = VisualTreeHelper.GetParent(obj);
            }

            return null;
        }

        public XElement GetShapeAt(Point p)
        {
            DependencyObject hitObject = (DependencyObject)_itemsControl.InputHitTest(p);

            // Workaround: For reasons unknown, InputHitTest sometimes return null when it clearly should not. This appears to be a framework bug.
            if (hitObject == null) { return null; }

            // If hitObject is not a visual, we need to find the visual parent.
            // Thus we loop as long as we're dealing with a FrameworkContentElement.
            // Only FrameworkContentElements expose a Parent property, so we cast.
            // (If we find a generic ContentElement, something has gone horribly wrong.)
            while (hitObject is FrameworkContentElement)
                hitObject = ((FrameworkContentElement)hitObject).Parent;

            ContentPresenter presenter = null;

            do
            {
                if (hitObject is ContentPresenter) presenter = (ContentPresenter)hitObject;
                hitObject = VisualTreeHelper.GetParent(hitObject);
            }
            while (hitObject != _itemsControl);

            // Something's wrong: We clicked a control not wrapped in a ContentPresenter... Never mind, then.
            if (presenter == null) return null; 

            var element = _itemsControl.ItemContainerGenerator.ItemFromContainer(presenter);
            return (element == DependencyProperty.UnsetValue) ? null : (XElement)element;
        }

        public UIElement PresenterFromElement(XElement element)
        {
            if (element == null) return null;
            return (UIElement)_itemsControl.ItemContainerGenerator.ContainerFromItem(element);
        }

        public FrameworkElement ControlFromElement(XElement element)
        {
            if (element == null) return null;
            DependencyObject dob = _itemsControl.ItemContainerGenerator.ContainerFromItem(element);
            if (dob == null) return null;
            DebugUtilities.Assert(VisualTreeHelper.GetChildrenCount(dob) == 1);
            return (FrameworkElement)VisualTreeHelper.GetChild(dob, 0);
        }

        public XElement ElementFromControl(DependencyObject shape)
        {
            while (shape != null)
            {
                XElement item = _itemsControl.ItemContainerGenerator.ItemFromContainer(shape) as XElement;
                if (item != null) return item;
                shape = VisualTreeHelper.GetParent(shape);
            }

            return null;
        }

        #endregion
    }
}
