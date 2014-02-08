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
                prop.RemoveValueChanged(e.OldValue, miniMapControl.UpdateVisibleAreaIndicator);
            prop.AddValueChanged(miniMapControl.ZoomControl, miniMapControl.UpdateVisibleAreaIndicator);

            prop = DependencyPropertyDescriptor.FromProperty(ZoomControl.TranslateYProperty, typeof(ZoomControl));
            if (e.OldValue != null)
                prop.RemoveValueChanged(e.OldValue, miniMapControl.UpdateVisibleAreaIndicator);
            prop.AddValueChanged(miniMapControl.ZoomControl, miniMapControl.UpdateVisibleAreaIndicator);

            prop = DependencyPropertyDescriptor.FromProperty(ZoomControl.ZoomProperty, typeof(ZoomControl));
            if (e.OldValue != null)
                prop.RemoveValueChanged(e.OldValue, miniMapControl.UpdateVisibleAreaIndicator);
            prop.AddValueChanged(miniMapControl.ZoomControl, miniMapControl.UpdateVisibleAreaIndicator);
        }

        private void UpdateVisibleAreaIndicator(object sender, EventArgs e)
        {
            VisibleAreaIndicator.Width = Math.Min(MiniMapContent.ActualWidth, MiniMapContent.ActualWidth / ZoomControl.Zoom);
            VisibleAreaIndicator.Height = Math.Min(MiniMapContent.ActualHeight, MiniMapContent.ActualHeight / ZoomControl.Zoom);

            double translateX = -ZoomControl.TranslateX*(MiniMapContent.ActualWidth/ZoomControl.ActualWidth);
            double translateY = -ZoomControl.TranslateY*(MiniMapContent.ActualHeight/ZoomControl.ActualHeight);

            var transformGroup = new TransformGroup();
            transformGroup.Children.Add(new TranslateTransform(translateX, translateY));
            VisibleAreaIndicator.RenderTransform = transformGroup;
        }
    }
}
