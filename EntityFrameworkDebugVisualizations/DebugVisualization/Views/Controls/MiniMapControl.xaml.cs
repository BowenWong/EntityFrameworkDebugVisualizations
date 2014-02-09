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
            if (ContentVisual == null || ZoomControl == null || ContentVisual.ActualWidth <= 0)
                return;

            MiniMapContentBounds.Width = GetWidth(ZoomControl.ActualWidth, MiniMapScale);
            MiniMapContentBounds.Height = GetHeight(ZoomControl.ActualHeight, MiniMapScale);

#warning it seems the ContentVisual doesn't correctly grow / update always (drag a vertex to the right..) - why?
            MiniMapContent.Width = GetContentWidth(MiniMapContentBounds.ActualWidth, MiniMapContentBounds.ActualHeight, ContentVisual.ActualWidth, ContentVisual.ActualHeight);
            MiniMapContent.Height = GetContentHeight(MiniMapContentBounds.ActualWidth, MiniMapContentBounds.ActualHeight, ContentVisual.ActualWidth, ContentVisual.ActualHeight);

#warning it seems the VisibleAreaIndicator size grows/shrinks with the size of the ContentVisual - why?
            VisibleAreaIndicator.Width = ZoomControl.ActualWidth*MiniMapScale/ZoomControl.Zoom;
            VisibleAreaIndicator.Height = ZoomControl.ActualHeight*MiniMapScale/ZoomControl.Zoom;

            double translateX = -ZoomControl.TranslateX * (MiniMapContent.ActualWidth / ContentVisual.ActualWidth);
            double translateY = -ZoomControl.TranslateY * (MiniMapContent.ActualHeight / ContentVisual.ActualHeight);
            //double translateX = -ZoomControl.TranslateX*MiniMapScale;
            //double translateY = -ZoomControl.TranslateY*MiniMapScale;

            var transformGroup = new TransformGroup();
            transformGroup.Children.Add(new TranslateTransform(translateX, translateY));
            VisibleAreaIndicator.RenderTransform = transformGroup;
        }

        public static double GetWidth(double zoomControlWidth, double miniMapScale)
        {
            return zoomControlWidth*miniMapScale;
        }

        public static double GetHeight(double zoomControlHeight, double miniMapScale)
        {
            return zoomControlHeight*miniMapScale;
        }

        public static double GetContentWidth(double miniMapContentWidth, double miniMapContentHeight, double contentVisualWidth, double contentVisualHeight)
        {
            if (miniMapContentWidth <= miniMapContentHeight && contentVisualWidth >= contentVisualHeight)
                return miniMapContentWidth;

            if (miniMapContentWidth < miniMapContentHeight && contentVisualHeight > contentVisualWidth)
                return miniMapContentWidth;

            return contentVisualWidth/contentVisualHeight*miniMapContentHeight;
        }

        public static double GetContentHeight(double miniMapContentWidth, double miniMapContentHeight, double contentVisualWidth, double contentVisualHeight)
        {
            if (miniMapContentHeight <= miniMapContentWidth && contentVisualHeight >= contentVisualWidth)
                return miniMapContentHeight;

            if (miniMapContentHeight < miniMapContentWidth && contentVisualWidth > contentVisualHeight)
                return miniMapContentHeight;

            return contentVisualHeight/contentVisualWidth*miniMapContentWidth;
        }
    }
}
