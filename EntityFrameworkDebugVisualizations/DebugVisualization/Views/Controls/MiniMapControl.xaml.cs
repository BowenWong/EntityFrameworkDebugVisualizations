using System;
using System.ComponentModel;
using System.Windows;

namespace EntityFramework.Debug.DebugVisualization.Views.Controls
{
    public partial class MiniMapControl
    {
        #region ContentVisual

        public static readonly DependencyProperty ContentVisualProperty = DependencyProperty.Register(
                "ContentVisual", typeof (UIElement), typeof (MiniMapControl), new PropertyMetadata(default(UIElement)));

        public UIElement ContentVisual
        {
            get { return (UIElement) GetValue(ContentVisualProperty); }
            set { SetValue(ContentVisualProperty, value); }
        }

        #endregion

        #region ZoomControl

        public static readonly DependencyProperty ZoomControlProperty = DependencyProperty.Register(
                "ZoomControl", typeof (ZoomControl), typeof (MiniMapControl), new PropertyMetadata(default(ZoomControl), OnZoomControlChanged));

        public ZoomControl ZoomControl
        {
            get { return (ZoomControl) GetValue(ZoomControlProperty); }
            set { SetValue(ZoomControlProperty, value); }
        }

        #endregion

        #region ViewportLeft/-Top

        public static readonly DependencyProperty ViewportLeftProperty = DependencyProperty.Register(
                "ViewportLeft", typeof (double), typeof (MiniMapControl), new PropertyMetadata(default(double)));

        public double ViewportLeft
        {
            get { return (double) GetValue(ViewportLeftProperty); }
            set { SetValue(ViewportLeftProperty, value); }
        }

        public static readonly DependencyProperty ViewportTopProperty = DependencyProperty.Register(
                "ViewportTop", typeof (double), typeof (MiniMapControl), new PropertyMetadata(default(double)));

        public double ViewportTop
        {
            get { return (double) GetValue(ViewportTopProperty); }
            set { SetValue(ViewportTopProperty, value); }
        }

        #endregion

        #region ViewportWidth/-Height

        public static readonly DependencyProperty ViewportWidthProperty = DependencyProperty.Register(
                "ViewportWidth", typeof (double), typeof (MiniMapControl), new PropertyMetadata(default(double)));

        public double ViewportWidth
        {
            get { return (double) GetValue(ViewportWidthProperty); }
            set { SetValue(ViewportWidthProperty, value); }
        }

        public static readonly DependencyProperty ViewportHeightProperty = DependencyProperty.Register(
                "ViewportHeight", typeof (double), typeof (MiniMapControl), new PropertyMetadata(default(double)));

        public double ViewportHeight
        {
            get { return (double) GetValue(ViewportHeightProperty); }
            set { SetValue(ViewportHeightProperty, value); }
        }

        #endregion

        public MiniMapControl()
        {
            InitializeComponent();
        }

        private static void OnZoomControlChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var miniMapControl = (MiniMapControl) d;

            DependencyPropertyDescriptor prop = DependencyPropertyDescriptor.FromProperty(ZoomControl.TranslateXProperty, typeof(ZoomControl));
            if (e.OldValue != null)
                prop.RemoveValueChanged(e.OldValue, miniMapControl.OnZoomControlTranslateXChanged);
            prop.AddValueChanged(miniMapControl.ZoomControl, miniMapControl.OnZoomControlTranslateXChanged);

            prop = DependencyPropertyDescriptor.FromProperty(ZoomControl.TranslateYProperty, typeof(ZoomControl));
            if (e.OldValue != null)
                prop.RemoveValueChanged(e.OldValue, miniMapControl.OnZoomControlTranslateYChanged);
            prop.AddValueChanged(miniMapControl.ZoomControl, miniMapControl.OnZoomControlTranslateYChanged);
        }

        private double GetScaleFactor()
        {
            var widthScale = ZoomControl.ActualWidth/ActualWidth;
            var heightScale = ZoomControl.ActualHeight/ActualHeight;
            return (widthScale < heightScale) ? widthScale : heightScale;
        }

        private void OnZoomControlTranslateXChanged(object sender, EventArgs e)
        {
            var scale = GetScaleFactor();
            ViewportLeft = ZoomControl.TranslateX / scale;
        }

        private void OnZoomControlTranslateYChanged(object sender, EventArgs e)
        {
            var scale = GetScaleFactor();
            ViewportTop = ZoomControl.TranslateY / scale;
        }

#warning TODO: set ViewportWidth/-Height (depending on size of Zoom- and MiniMapControl)
#warning TODO: update ViewportLeft/-Top if ScaleFactor changes (depends on size of Zoom- and MiniMapControl)
    }
}
