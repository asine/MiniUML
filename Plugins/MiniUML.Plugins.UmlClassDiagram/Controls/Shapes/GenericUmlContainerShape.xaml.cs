using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml.Linq;
using MiniUML.Framework;
using MiniUML.View.Controls;
using MiniUML.View.Views;

namespace MiniUML.Plugins.UmlClassDiagram.Resources.Shapes
{
    /// <summary>
    /// Interaction logic for GenericUmlContainerShape.xaml
    /// </summary>
    public partial class GenericUmlContainerShape : Control, ISnapTarget
    {
        public GenericUmlContainerShape()
        {
            InitializeComponent();
            ContextMenu = new ContextMenu();
        }

        protected void AddMenuItem(string text, string id)
        {
            MenuItem menuItem = new MenuItem() { Header = text, Tag = id };
            menuItem.Click += menuItem_Click;
            ContextMenu.Items.Add(menuItem);
        }

        protected void AddZOrderMenuItems()
        {
            ContextMenu.Items.Add(new Separator());
            AddMenuItem("BringToFront", "BringToFront");
            AddMenuItem("SendToBack", "SendToBack");
        }
        
        protected void menuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            XElement element = DataContext as XElement;
            XElement root = element.Parent;

            switch ((string)menuItem.Tag)
            {
                case "AddMethod":
                    element.Add(new XElement("Uml.Members.Method", new XAttribute("Name", "<new method>")));
                    break;

                case "AddProperty":
                    element.Add(new XElement("Uml.Members.Property", new XAttribute("Name", "<new property>")));
                    break;

                case "AddField":
                    element.Add(new XElement("Uml.Members.Field", new XAttribute("Name", "<new field>")));
                    break;

                case "AddEvent":
                    element.Add(new XElement("Uml.Members.Event", new XAttribute("Name", "<new event>")));
                    break;

                case "AddMember":
                    element.Add(new XElement("Uml.Members.Member", new XAttribute("Name", "<new member>")));
                    break;

                case "AddClass":
                    element.Add(new XElement("Uml.Members.Type", new XAttribute("Name", "<new class>"), new XAttribute("Description", "Class")));
                    break;

                case "Delete":
                    element.Remove();
                    break;

                case "BringToFront":
                    element.Remove();
                    root.Add(element);
                    //TODO: Add workaround for databinding bug.
                    break;

                case "SendToBack":
                    element.Remove();
                    root.AddFirst(element);
                    //TODO: Add workaround for databinding bug.
                    break;
            }

            ListBox listBox = this.Template.FindName("_listBox", this) as ListBox;
            if (listBox != null) listBox.Focus();
        }

        private void listBox_LostFocus(object sender, RoutedEventArgs e)
        {
            ListBox listBox = sender as ListBox;
            if (listBox != null && !listBox.IsKeyboardFocusWithin) listBox.SelectedIndex = -1;
        }

        #region Snapping stuff

        public void SnapPoint(ref Point snapPosition, out double snapAngle)
        {
            snapAngle = 0;

            var cv = CanvasView.GetCanvasView(this);
            if (cv == null) return;

            Point pos = cv.ElementFromControl(this).GetPositionAttributes();

            // An array of line segments, each line segment represented as four double values.
            double[] shape = {
                pos.X, pos.Y + CornerRadius,
                pos.X, pos.Y + ActualHeight - CornerRadius,

                pos.X + CornerRadius, pos.Y + ActualHeight,
                pos.X + ActualWidth - CornerRadius, pos.Y + ActualHeight,

                pos.X + ActualWidth, pos.Y + ActualHeight - CornerRadius,
                pos.X + ActualWidth, pos.Y + CornerRadius,

                pos.X + ActualWidth - CornerRadius, pos.Y,
                pos.X + CornerRadius, pos.Y,
            };

            Point bestSnapPoint = new Point(Double.NaN, Double.NaN);
            double bestSnapLengthSq = double.PositiveInfinity;

            for (int i = 0; i < shape.Length; i += 4)
            {
                Point from = new Point(shape[i], shape[i + 1]);
                Point to = new Point(shape[i + 2], shape[i + 3]);

                AnchorPoint.SnapToLineSegment(from, to, snapPosition, ref bestSnapLengthSq, ref bestSnapPoint, ref snapAngle);
            }

            snapPosition = bestSnapPoint;
        }

        public event SnapTargetUpdateHandler SnapTargetUpdate;

        public void NotifySnapTargetUpdate(SnapTargetUpdateEventArgs e)
        {
            if (SnapTargetUpdate != null) SnapTargetUpdate(this, e);
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            NotifySnapTargetUpdate(new SnapTargetUpdateEventArgs());
        }

        #endregion

        #region GroupHeaderBackgroundBrush property

        public Brush GroupHeaderBackgroundBrush
        {
            get { return (Brush)GetValue(GroupHeaderBackgroundBrushProperty); }
            set { SetValue(GroupHeaderBackgroundBrushProperty, value); }
        }

        public static readonly DependencyProperty GroupHeaderBackgroundBrushProperty
            = DependencyProperty.Register("GroupHeaderBackgroundBrush", typeof(Brush), typeof(GenericUmlContainerShape),
            new FrameworkPropertyMetadata(Brushes.LightGray, FrameworkPropertyMetadataOptions.AffectsRender));

        #endregion

        #region CornerRadius property

        public double CornerRadius
        {
            get { return (double)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }

        public static readonly DependencyProperty CornerRadiusProperty
            = DependencyProperty.Register("CornerRadius", typeof(double), typeof(GenericUmlContainerShape),
            new FrameworkPropertyMetadata(10.0, FrameworkPropertyMetadataOptions.AffectsRender));

        #endregion

    }
}
