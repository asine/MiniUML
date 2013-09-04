using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using MiniUML.Framework;
using MiniUML.View.Controls;
using MiniUML.View.Utilities;

namespace MiniUML.Plugins.UmlClassDiagram.Resources.Shapes
{
    /// <summary>
    /// Interaction logic for Association.xaml
    /// </summary>
    public partial class GenericUmlAssociation : UserControl, INotifyPropertyChanged, ISnapTarget
    {
        #region Dependency properties (FromName, FromArrow, ToName, ToArrow)

        public static readonly DependencyProperty FromNameProperty = DependencyProperty.Register(
            "FromName",
            typeof(String),
            typeof(GenericUmlAssociation),
            new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.AffectsRender)
        );

        public String FromName
        {
            get { return (String)GetValue(FromNameProperty); }
            set { SetValue(FromNameProperty, value); }
        }

        public static readonly DependencyProperty ToNameProperty = DependencyProperty.Register(
            "ToName",
            typeof(String),
            typeof(GenericUmlAssociation),
            new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.AffectsRender)
        );

        public String ToName
        {
            get { return (String)GetValue(ToNameProperty); }
            set { SetValue(ToNameProperty, value); }
        }

        public static readonly DependencyProperty FromArrowProperty = DependencyProperty.Register(
            "FromArrow",
            typeof(String),
            typeof(GenericUmlAssociation),
            new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.AffectsRender)
        );

        public String FromArrow
        {
            get { return (String)GetValue(FromArrowProperty); }
            set { SetValue(FromArrowProperty, value); }
        }

        public static readonly DependencyProperty ToArrowProperty = DependencyProperty.Register(
            "ToArrow",
            typeof(String),
            typeof(GenericUmlAssociation),
            new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.AffectsRender)
        );

        public String ToArrow
        {
            get { return (String)GetValue(ToArrowProperty); }
            set { SetValue(ToArrowProperty, value); }
        }

        public static readonly DependencyProperty StrokeProperty = DependencyProperty.Register(
            "Stroke", typeof(Brush), typeof(GenericUmlAssociation),
            new FrameworkPropertyMetadata(Brushes.Black, FrameworkPropertyMetadataOptions.AffectsRender)
        );

        public Brush Stroke
        {
            get { return (Brush)GetValue(StrokeProperty); }
            set { SetValue(StrokeProperty, value); }
        }

        #endregion

        #region INotifyPropertyChanged implementation

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        public GenericUmlAssociation()
        {
            InitializeComponent();
            ((ShapeIdToControlConverter)Resources["ShapeIdToControlConverter"]).ReferenceControl = this;
        }

        #region ISnapTarget Members

        public void SnapPoint(ref Point snapPosition, out double snapAngle)
        {
            tpl.SnapPoint(ref snapPosition, out snapAngle);
        }

        public event SnapTargetUpdateHandler SnapTargetUpdate;

        public void NotifySnapTargetUpdate(SnapTargetUpdateEventArgs e)
        {
            if (SnapTargetUpdate != null) SnapTargetUpdate(this, e);
        }

        #endregion

        private void snapTargetUpdate(ISnapTarget source, SnapTargetUpdateEventArgs e)
        {
            NotifySnapTargetUpdate(e);
        }

        public ShapeIdToControlConverter ShapeIdToControlConverter
        {
            get { return new ShapeIdToControlConverter() { ReferenceControl = this }; }
        }
    }

    /************************************************************************/

    public class ThreePieceLine : UserControl, INotifyPropertyChanged, ISnapTarget
    {
        private Polyline line;
        private Polyline lineb;

        public ThreePieceLine()
        {
            line = new Polyline();

            lineb = new Polyline()
            {
                StrokeThickness = 7,
                Stroke = new SolidColorBrush(new Color() { A = 192, R = 255, G = 255, B = 255 })
            };

            Canvas canvas = new Canvas();
            canvas.Children.Add(lineb);
            canvas.Children.Add(line);
            this.Content = canvas;
        }

        public static readonly DependencyProperty FromXProperty = DependencyProperty.Register(
            "FromX", typeof(Double), typeof(ThreePieceLine),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(propChanged))
        );

        public Double FromX
        {
            get { return (Double)GetValue(FromXProperty); }
            set { SetValue(FromXProperty, value); }
        }

        public static readonly DependencyProperty FromYProperty = DependencyProperty.Register(
            "FromY", typeof(Double), typeof(ThreePieceLine),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(propChanged))
        );

        public Double FromY
        {
            get { return (Double)GetValue(FromYProperty); }
            set { SetValue(FromYProperty, value); }
        }

        public static readonly DependencyProperty FromOrientationProperty = DependencyProperty.Register(
            "FromOrientation", typeof(Orientation), typeof(ThreePieceLine),
            new FrameworkPropertyMetadata(Orientation.Horizontal, FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(propChanged))
        );

        public Orientation FromOrientation
        {
            get { return (Orientation)GetValue(FromOrientationProperty); }
            set { SetValue(FromOrientationProperty, value); }
        }

        public static readonly DependencyProperty ToXProperty = DependencyProperty.Register(
            "ToX", typeof(Double), typeof(ThreePieceLine),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(propChanged))
        );

        public Double ToX
        {
            get { return (Double)GetValue(ToXProperty); }
            set { SetValue(ToXProperty, value); }
        }

        public static readonly DependencyProperty ToYProperty = DependencyProperty.Register(
            "ToY", typeof(Double), typeof(ThreePieceLine),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(propChanged))
        );

        public Double ToY
        {
            get { return (Double)GetValue(ToYProperty); }
            set { SetValue(ToYProperty, value); }
        }

        public static readonly DependencyProperty ToOrientationProperty = DependencyProperty.Register(
            "ToOrientation", typeof(Orientation), typeof(ThreePieceLine),
            new FrameworkPropertyMetadata(Orientation.Horizontal, FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(propChanged))
        );

        public Orientation ToOrientation
        {
            get { return (Orientation)GetValue(ToOrientationProperty); }
            set { SetValue(ToOrientationProperty, value); }
        }

        private static void propChanged(DependencyObject o, DependencyPropertyChangedEventArgs a)
        {
            ((ThreePieceLine)o).rerouteLine();
        }

        private double fromAngleInDegrees;
        private double toAngleInDegrees;

        // The angle of the line joining the origin, in degrees.
        public double FromAngleInDegrees { get { return fromAngleInDegrees; } }

        // The angle of the line joining the destination, in degrees.
        public double ToAngleInDegrees { get { return toAngleInDegrees; } }

        private void rerouteLine()
        {
            Point from = new Point(FromX, FromY);
            Point to = new Point(ToX, ToY);

            line.Points = lineb.Points;
            line.Points.Clear();
            line.Points.Add(from);

            if (FromOrientation != ToOrientation)
            {
                // Fine, we'll just do a two-piece line, then.
                if (FromOrientation == Orientation.Horizontal)
                    line.Points.Add(new Point(to.X, from.Y));
                else line.Points.Add(new Point(from.X, to.Y));
            }
            else if (FromOrientation /* == ToOrientation */ == Orientation.Horizontal)
            {
                double mid = from.X + (to.X - from.X) / 2;

                line.Points.Add(new Point(mid, from.Y));
                line.Points.Add(new Point(mid, to.Y));
            }
            else /* FromOrientation == ToOrientation == Orientation.Vertical */
            {
                double mid = from.Y + (to.Y - from.Y) / 2;

                line.Points.Add(new Point(from.X, mid));
                line.Points.Add(new Point(to.X, mid));
            }

            line.Points.Add(to);

            Vector firstSegment = from - line.Points[1];
            Vector lastSegment = to - line.Points[line.Points.Count() - 2];
            fromAngleInDegrees = FrameworkUtilities.RadiansToDegrees(firstSegment.GetAngularCoordinate());
            toAngleInDegrees = FrameworkUtilities.RadiansToDegrees(lastSegment.GetAngularCoordinate());
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("FromAngleInDegrees"));
                PropertyChanged(this, new PropertyChangedEventArgs("ToAngleInDegrees"));
            }
            NotifySnapTargetUpdate(new SnapTargetUpdateEventArgs());
        }

        #region ISnapTarget Members

        public void SnapPoint(ref Point snapPosition, out double snapAngle)
        {
            snapAngle = 0;

            PointCollection points = line.Points;
            if (points.Count < 2) return;

            Point bestSnapPoint = new Point(Double.NaN, Double.NaN);
            double bestSnapLengthSq = double.PositiveInfinity;

            Point lastPoint = points[0];
            for (int i = 1; i < points.Count; i++)
            {
                Point curPoint = points[i];
                AnchorPoint.SnapToLineSegment(lastPoint, curPoint, snapPosition, ref bestSnapLengthSq, ref bestSnapPoint, ref snapAngle);
                lastPoint = curPoint;
            }

            snapPosition = bestSnapPoint;
        }

        public event SnapTargetUpdateHandler SnapTargetUpdate;

        public void NotifySnapTargetUpdate(SnapTargetUpdateEventArgs e)
        {
            if (SnapTargetUpdate != null) SnapTargetUpdate(this, e);
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
