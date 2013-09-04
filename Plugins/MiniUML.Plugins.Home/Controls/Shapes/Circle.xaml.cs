using System.Windows;
using System.Windows.Controls;
using MiniUML.Framework;
using MiniUML.View.Controls;
using MiniUML.View.Views;

namespace MiniUML.Plugins.Home.Controls.Shapes
{
    /// <summary>
    /// Interaction logic for Circle.xaml
    /// </summary>
    public partial class Circle : UserControl, ISnapTarget
    {
        public Circle()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty DiameterProperty =
            DependencyProperty.Register("Diameter", typeof(double), typeof(Circle), new UIPropertyMetadata(0.0));

        public double Diameter
        {
            get { return (double)GetValue(DiameterProperty); }
            set { SetValue(DiameterProperty, value); }
        }

        #region ISnapTarget Members

        public void SnapPoint(ref Point p, out double snapAngle)
        {
            var cv = CanvasView.GetCanvasView(this);
            if (cv == null) { snapAngle = 0; return; }

            Point myPos = cv.ElementFromControl(this).GetPositionAttributes();

            Vector v = p - myPos;
            if (v.LengthSquared == 0)
            {
                p = myPos + new Vector(Diameter / 2, 0);
                snapAngle = 0;
            }
            else
            {
                v.Normalize();

                p = myPos + v * Diameter / 2;
                snapAngle = v.GetAngularCoordinate();
            }
        }

        public event SnapTargetUpdateHandler SnapTargetUpdate;

        public void NotifySnapTargetUpdate(SnapTargetUpdateEventArgs e)
        {
            if (SnapTargetUpdate != null) SnapTargetUpdate(this, e);
        }

        #endregion
    }
}
