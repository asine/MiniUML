using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Linq;
using MiniUML.Framework;
using MiniUML.View.Utilities;
using MiniUML.View.Views;

namespace MiniUML.Plugins.Home.Controls.Shapes
{
    /// <summary>
    /// Interaction logic for Line.xaml
    /// </summary>
    public partial class Line : UserControl, INotifyPropertyChanged
    {
        #region Dependency properties

        public static readonly DependencyProperty FromXProperty = DependencyProperty.Register(
            "FromX", typeof(Double), typeof(Line),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(propChanged))
        );

        public Double FromX
        {
            get { return (Double)GetValue(FromXProperty); }
            set { SetValue(FromXProperty, value); }
        }

        public static readonly DependencyProperty FromYProperty = DependencyProperty.Register(
            "FromY", typeof(Double), typeof(Line),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(propChanged))
        );

        public Double FromY
        {
            get { return (Double)GetValue(FromYProperty); }
            set { SetValue(FromYProperty, value); }
        }

        public static readonly DependencyProperty ToXProperty = DependencyProperty.Register(
            "ToX", typeof(Double), typeof(Line),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(propChanged))
        );

        public Double ToX
        {
            get { return (Double)GetValue(ToXProperty); }
            set { SetValue(ToXProperty, value); }
        }

        public static readonly DependencyProperty ToYProperty = DependencyProperty.Register(
            "ToY", typeof(Double), typeof(Line),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(propChanged))
        );

        public Double ToY
        {
            get { return (Double)GetValue(ToYProperty); }
            set { SetValue(ToYProperty, value); }
        }

        public static readonly DependencyProperty StrokeProperty = DependencyProperty.Register(
            "Stroke", typeof(Brush), typeof(Line),
            new FrameworkPropertyMetadata(Brushes.Black, FrameworkPropertyMetadataOptions.AffectsRender)
        );

        public Brush Stroke
        {
            get { return (Brush)GetValue(StrokeProperty); }
            set { SetValue(StrokeProperty, value); }
        }

        #endregion

        public Line()
        {
            InitializeComponent();
            ((ShapeIdToControlConverter)Resources["ShapeIdToControlConverter"]).ReferenceControl = this;
        }

        public double FromAngleInDegrees
        {
            get { return FrameworkUtilities.RadiansToDegrees(new Vector(FromX - ToX, FromY - ToY).GetAngularCoordinate()); }
        }

        public double ToAngleInDegrees
        {
            get { return FrameworkUtilities.RadiansToDegrees(new Vector(ToX - FromX, ToY - FromY).GetAngularCoordinate()); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void lineMoved()
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("FromAngleInDegrees"));
                PropertyChanged(this, new PropertyChangedEventArgs("ToAngleInDegrees"));
                PropertyChanged(this, new PropertyChangedEventArgs("TextAngleInDegrees"));
                PropertyChanged(this, new PropertyChangedEventArgs("TextWidth"));
            }

            _textBlock.Width = new Vector(FromX - ToX, FromY - ToY).Length;

            double angle = ToAngleInDegrees;
            if (angle >= -90 && angle <= 90)
            {
                _textCanvas.SetValue(Canvas.LeftProperty, FromX);
                _textCanvas.SetValue(Canvas.TopProperty, FromY);
                ((RotateTransform)_textCanvas.RenderTransform).Angle = angle;
            }
            else
            {
                _textCanvas.SetValue(Canvas.LeftProperty, ToX);
                _textCanvas.SetValue(Canvas.TopProperty, ToY);
                ((RotateTransform)_textCanvas.RenderTransform).Angle = angle - 180;
            }
        }

        private static void propChanged(DependencyObject o, DependencyPropertyChangedEventArgs a)
        {
            ((Line)o).lineMoved();
        }

        /// <summary>
        /// Auto-position end-points (this feature designed for use with circles).
        /// </summary>
        protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
        {
            base.OnMouseDoubleClick(e);
            
            Point fromP = getControlMiddle(_fromAnchor.SnapTarget);
            Point toP = getControlMiddle(_toAnchor.SnapTarget);
            Vector v = toP - fromP;
            v.Normalize();

            if (Double.IsNaN(v.X) || Double.IsNaN(v.Y)) return;

            _fromAnchor.MoveTo(fromP + v);
            _toAnchor.MoveTo(toP - v);
        }

        /// <summary>
        /// Returns the mid-point coordinates of the given control (assuming it's a shape on a canvas).
        /// </summary>
        private Point getControlMiddle(object control)
        {
            var cv = CanvasView.GetCanvasView(this);
            if (cv == null) return new Point(double.NaN, double.NaN);

            FrameworkElement felm = control as FrameworkElement;
            XElement xelm = cv.ElementFromControl(felm);

            if (xelm == null || felm == null) return new Point(double.NaN, double.NaN);

            return xelm.GetPositionAttributes() + new Vector(felm.ActualWidth / 2, felm.ActualHeight / 2);
        }
    }
}
