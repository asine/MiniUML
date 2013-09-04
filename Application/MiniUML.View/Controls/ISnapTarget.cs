using System;
using System.Windows;

namespace MiniUML.View.Controls
{
    public class SnapTargetUpdateEventArgs : EventArgs
    {
        public Vector moveDelta;
        public bool isMoveUpdate = false;

        public SnapTargetUpdateEventArgs() { }

        public SnapTargetUpdateEventArgs(Vector moveDelta)
        {
            isMoveUpdate = true;
            this.moveDelta = moveDelta;
        }
    }

    public delegate void SnapTargetUpdateHandler(ISnapTarget source, SnapTargetUpdateEventArgs e);

    public interface ISnapTarget
    {
        /** Snaps p to the SnapTarget, and sets snapAngle to the angle of the snap target line (if any). */
        void SnapPoint(ref Point p, out double snapAngle);

        event SnapTargetUpdateHandler SnapTargetUpdate;

        void NotifySnapTargetUpdate(SnapTargetUpdateEventArgs e);
    }
}
