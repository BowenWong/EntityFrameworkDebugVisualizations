using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

namespace EntityFramework.Debug.DebugVisualization.Views.Controls
{
    public partial class MiniMapControl
    {
        #region ContentVisual

        public static readonly DependencyProperty ContentVisualProperty = DependencyProperty.Register(
                "ContentVisual", typeof(FrameworkElement), typeof(MiniMapControl), new PropertyMetadata(default(FrameworkElement)));

        public FrameworkElement ContentVisual
        {
            get { return (FrameworkElement)GetValue(ContentVisualProperty); }
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

        public MiniMapControl()
        {
            InitializeComponent();
        }

        private static void OnZoomControlChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var miniMapControl = (MiniMapControl) d;

            DependencyPropertyDescriptor prop = DependencyPropertyDescriptor.FromProperty(ZoomControl.TranslateXProperty, typeof(ZoomControl));
            if (e.OldValue != null)
                prop.RemoveValueChanged(e.OldValue, miniMapControl.UpdateViewportLayout);
            prop.AddValueChanged(miniMapControl.ZoomControl, miniMapControl.UpdateViewportLayout);

            prop = DependencyPropertyDescriptor.FromProperty(ZoomControl.TranslateYProperty, typeof(ZoomControl));
            if (e.OldValue != null)
                prop.RemoveValueChanged(e.OldValue, miniMapControl.UpdateViewportLayout);
            prop.AddValueChanged(miniMapControl.ZoomControl, miniMapControl.UpdateViewportLayout);

            prop = DependencyPropertyDescriptor.FromProperty(ZoomControl.ZoomProperty, typeof(ZoomControl));
            if (e.OldValue != null)
                prop.RemoveValueChanged(e.OldValue, miniMapControl.UpdateViewportLayout);
            prop.AddValueChanged(miniMapControl.ZoomControl, miniMapControl.UpdateViewportLayout);
        }

        private void UpdateViewportLayout(object sender, EventArgs e)
        {
            Viewport.Width = ActualWidth/ZoomControl.Zoom;
            Viewport.Height = ActualHeight/ZoomControl.Zoom;

            var transformGroup = new TransformGroup();

#warning clip to bounds?
            double translateX = -ZoomControl.TranslateX*ActualWidth/ContentVisual.ActualWidth;
            double translateY = -ZoomControl.TranslateY*ActualHeight/ContentVisual.ActualHeight;
            transformGroup.Children.Add(new TranslateTransform(translateX, translateY));

            Viewport.RenderTransform = transformGroup;
        }
    }
}
