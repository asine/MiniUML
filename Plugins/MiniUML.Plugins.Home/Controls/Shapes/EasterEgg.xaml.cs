using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MiniUML.Framework;
using MiniUML.View.Controls;
using MiniUML.View.Views;

namespace MiniUML.Plugins.Home.Controls.Shapes
{
    /// <summary>
    /// Interaction logic for EasterEgg.xaml
    /// </summary>
    public partial class EasterEgg : UserControl, ISnapTarget
    {
        public EasterEgg()
        {
            InitializeComponent();
        }

        #region ISnapTarget Members

        public void SnapPoint(ref Point p, out double snapAngle)
        {
            double bestSnapLengthSq = double.MaxValue;
            Point bestSnapPoint = p;
            snapAngle = 0;

            snapToPolygon(poly1.Points, p, ref bestSnapLengthSq, ref bestSnapPoint, ref snapAngle);
            snapToPolygon(poly2.Points, p, ref bestSnapLengthSq, ref bestSnapPoint, ref snapAngle);

            p = bestSnapPoint; 
        }

        private void snapToPolygon(PointCollection points, Point origin, ref double bestSnapLengthSq, ref Point bestSnapPoint, ref double snapAngle)
        {
            var cv = CanvasView.GetCanvasView(this);
            if (cv == null) return;

            Vector thisOffset = cv.ElementFromControl(this).GetPositionAttributes() - new Point(0, 0);

            for (int i = 0; i < points.Count; i++)
            {
                Point from = points[i] + thisOffset;
                Point to = points[(i + 1) % points.Count] + thisOffset;
                AnchorPoint.SnapToLineSegment(from, to, origin, ref bestSnapLengthSq, ref bestSnapPoint, ref snapAngle);
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
