using System.Windows;
using System.Windows.Controls;

namespace EntityFramework.Debug.DebugVisualization.Views.Controls
{
    public class ZoomContentControl : ContentControl
    {
        protected override Size MeasureOverride(Size constraint)
        {
            base.MeasureOverride(new Size(double.PositiveInfinity, double.PositiveInfinity));

            const double max = 1e9;
            var x = double.IsInfinity(constraint.Width) ? max : constraint.Width;
            var y = double.IsInfinity(constraint.Height) ? max : constraint.Height;
            return new Size(x, y);
        }

        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            var child = Content as UIElement;
            if (child == null)
                return arrangeBounds;

            child.Arrange(new Rect(new Point(250, 250), child.DesiredSize));

            return arrangeBounds;
        }
    }
}
