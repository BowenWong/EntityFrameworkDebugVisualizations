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
                "ContentVisual", typeof(FrameworkElement), typeof(MiniMapControl), new PropertyMetadata(default(FrameworkElement), OnContentVisualChanged));

        public FrameworkElement ContentVisual
        {
            get { return (FrameworkElement)GetValue(ContentVisualProperty); }
            set { SetValue(ContentVisualProperty, value); }
        }

        private static void OnContentVisualChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var miniMapControl = (MiniMapControl)d;

            DependencyPropertyDescriptor prop = DependencyPropertyDescriptor.FromProperty(ActualWidthProperty, typeof(FrameworkElement));
            if (e.OldValue != null)
                prop.RemoveValueChanged(e.OldValue, miniMapControl.UpdateVisibleAreaIndicator);
            prop.AddValueChanged(miniMapControl.ContentVisual, miniMapControl.UpdateVisibleAreaIndicator);

            prop = DependencyPropertyDescriptor.FromProperty(ActualHeightProperty, typeof(FrameworkElement));
            if (e.OldValue != null)
                prop.RemoveValueChanged(e.OldValue, miniMapControl.UpdateVisibleAreaIndicator);
            prop.AddValueChanged(miniMapControl.ContentVisual, miniMapControl.UpdateVisibleAreaIndicator);
        }

        #endregion

        #region MiniMapScale

        public static readonly DependencyProperty MiniMapScaleProperty = DependencyProperty.Register(
                "MiniMapScale", typeof(double), typeof(MiniMapControl), new PropertyMetadata(default(double), OnMiniMapScaleChanged));

        public double MiniMapScale
        {
            get { return (double)GetValue(MiniMapScaleProperty); }
            set { SetValue(MiniMapScaleProperty, value); }
        }

        private static void OnMiniMapScaleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var miniMapControl = (MiniMapControl)d;
            miniMapControl.UpdateVisibleAreaIndicator(miniMapControl, EventArgs.Empty);
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

        private static void OnZoomControlChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var miniMapControl = (MiniMapControl)d;

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

            prop = DependencyPropertyDescriptor.FromProperty(ActualWidthProperty, typeof(ZoomControl));
            if (e.OldValue != null)
                prop.RemoveValueChanged(e.OldValue, miniMapControl.UpdateVisibleAreaIndicator);
            prop.AddValueChanged(miniMapControl.ZoomControl, miniMapControl.UpdateVisibleAreaIndicator);

            prop = DependencyPropertyDescriptor.FromProperty(ActualHeightProperty, typeof(ZoomControl));
            if (e.OldValue != null)
                prop.RemoveValueChanged(e.OldValue, miniMapControl.UpdateVisibleAreaIndicator);
            prop.AddValueChanged(miniMapControl.ZoomControl, miniMapControl.UpdateVisibleAreaIndicator);
        }

        #endregion

        public MiniMapControl()
        {
            InitializeComponent();
        }

        private void UpdateVisibleAreaIndicator(object sender, EventArgs e)
        {
            if (ContentVisual == null || ZoomControl == null)
                return;

            Width = ZoomControl.ActualWidth * MiniMapScale;
            Height = ZoomControl.ActualHeight * MiniMapScale;

            double widthScale = ContentVisual.ActualWidth > 0.0 ? ZoomControl.ActualWidth / ContentVisual.ActualWidth : 1.0;
            VisibleAreaIndicator.Width = Math.Min(MiniMapContent.ActualWidth, MiniMapContent.ActualWidth / ZoomControl.Zoom * widthScale);
            double heightScale = ContentVisual.ActualHeight > 0.0 ? ZoomControl.ActualHeight / ContentVisual.ActualHeight : 1.0;
            VisibleAreaIndicator.Height = Math.Min(MiniMapContent.ActualHeight, MiniMapContent.ActualHeight / ZoomControl.Zoom * heightScale);

            double translateX = -ZoomControl.TranslateX*(MiniMapContent.ActualWidth/ContentVisual.ActualWidth);
            double translateY = -ZoomControl.TranslateY*(MiniMapContent.ActualHeight/ContentVisual.ActualHeight);

            var transformGroup = new TransformGroup();
            transformGroup.Children.Add(new TranslateTransform(translateX, translateY));
            VisibleAreaIndicator.RenderTransform = transformGroup;
        }
    }
}
