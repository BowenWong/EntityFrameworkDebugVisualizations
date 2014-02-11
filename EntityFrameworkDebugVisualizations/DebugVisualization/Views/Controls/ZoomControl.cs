using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace EntityFramework.Debug.DebugVisualization.Views.Controls
{
    public class ZoomControl : ScrollViewer
    {
        #region Min-/MaxZoom

        public static readonly DependencyProperty MaxZoomProperty =
            DependencyProperty.Register("MaxZoom", typeof(double), typeof(ZoomControl), new UIPropertyMetadata(100.0));

        public static readonly DependencyProperty MinZoomProperty =
            DependencyProperty.Register("MinZoom", typeof(double), typeof(ZoomControl), new UIPropertyMetadata(0.01));

        public double MinZoom
        {
            get { return (double)GetValue(MinZoomProperty); }
            set { SetValue(MinZoomProperty, value); }
        }

        public double MaxZoom
        {
            get { return (double)GetValue(MaxZoomProperty); }
            set { SetValue(MaxZoomProperty, value); }
        }

        #endregion

        #region TranslateX/-Y

        public static readonly DependencyProperty TranslateXProperty =
            DependencyProperty.Register("TranslateX", typeof(double), typeof(ZoomControl),
                                        new UIPropertyMetadata(0.0, TranslateXPropertyChanged));

        public static readonly DependencyProperty TranslateYProperty =
            DependencyProperty.Register("TranslateY", typeof(double), typeof(ZoomControl),
                                        new UIPropertyMetadata(0.0, TranslateYPropertyChanged));

        public double TranslateX
        {
            get { return (double)GetValue(TranslateXProperty); }
            set
            {
                BeginAnimation(TranslateXProperty, null);
                SetValue(TranslateXProperty, value);
            }
        }

        public double TranslateY
        {
            get { return (double)GetValue(TranslateYProperty); }
            set
            {
                BeginAnimation(TranslateYProperty, null);
                SetValue(TranslateYProperty, value);
            }
        }

        #endregion

        #region Mode / IsPanning

        public static readonly DependencyProperty ModeProperty =
            DependencyProperty.Register("Mode", typeof(ZoomControlModes), typeof(ZoomControl),
                                        new UIPropertyMetadata(ZoomControlModes.Custom, ModePropertyChanged));

        public ZoomControlModes Mode
        {
            get { return (ZoomControlModes)GetValue(ModeProperty); }
            set { SetValue(ModeProperty, value); }
        }

        public static readonly DependencyProperty IsPanningProperty = DependencyProperty.Register(
                "IsPanning", typeof (bool), typeof (ZoomControl), new PropertyMetadata(default(bool)));

        public bool IsPanning
        {
            get { return (bool) GetValue(IsPanningProperty); }
            set { SetValue(IsPanningProperty, value); }
        }

        #endregion

        #region Zoom

        public static readonly DependencyProperty ZoomProperty =
            DependencyProperty.Register("Zoom", typeof(double), typeof(ZoomControl),
                                        new UIPropertyMetadata(1.0, ZoomPropertyChanged));

        public double Zoom
        {
            get { return (double)GetValue(ZoomProperty); }
            set
            {
                if (value == (double)GetValue(ZoomProperty))
                    return;
                BeginAnimation(ZoomProperty, null);
                SetValue(ZoomProperty, value);
            }
        }

        #endregion

        private Point _mouseDownPos;

        private ScaleTransform _scaleTransform;
        private Vector _startTranslate;

        private int _zoomAnimCount;
        private bool _isZooming;

        static ZoomControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ZoomControl), new FrameworkPropertyMetadata(typeof(ScrollViewer)));
        }

        public ZoomControl()
        {
            PreviewMouseWheel += ZoomControlMouseWheel;
            PreviewMouseLeftButtonDown += OnMouseDown;
            MouseUp += ZoomControlMouseUp;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            SizeChanged += (s, e) =>
            {
                if (Mode == ZoomControlModes.Fill)
                    DoZoomToFill();
            };

            ZoomToFill();

            var content = Content as FrameworkElement;
            if (content == null)
                throw new Exception("No content found or Content isn't a FrameworkElement.");

            _scaleTransform = new ScaleTransform();
            content.LayoutTransform = _scaleTransform;
            content.RenderTransformOrigin = new Point(0.5, 0.5);
        }

        protected override void OnChildDesiredSizeChanged(UIElement child)
        {
            base.OnChildDesiredSizeChanged(child);

            if (Mode == ZoomControlModes.Fill)
                DoZoomToFill();
        }

        private void ZoomControlMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!IsPanning)
                return;

            IsPanning = false;
            PreviewMouseMove -= ZoomControlPreviewMouseMove;
            ReleaseMouseCapture();
        }

        private void ZoomControlPreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (!IsPanning)
                return;

            var translate = _startTranslate + (_mouseDownPos - e.GetPosition(this));
            TranslateX = translate.X;
            TranslateY = translate.Y;
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (IsPanning)
                return;

            IsPanning = true;

            _mouseDownPos = e.GetPosition(this);
            _startTranslate = new Vector(HorizontalOffset, VerticalOffset);
            Mouse.Capture(this);
            PreviewMouseMove += ZoomControlPreviewMouseMove;
        }
        
        private void ZoomControlMouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;
            var origoPosition = new Point(ActualWidth / 2, ActualHeight / 2);
            Point mousePosition = e.GetPosition(this);

            var deltaZoom = Math.Max(0.2, Math.Min(2.0, e.Delta / 300.0 + 1));
            DoZoom(deltaZoom, origoPosition, mousePosition, mousePosition);
        }

        private static void TranslateXPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var zc = (ZoomControl)d;
            zc.ScrollToHorizontalOffset((double)e.NewValue);
            if (!zc._isZooming)
                zc.Mode = ZoomControlModes.Custom;
        }

        private static void TranslateYPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var zc = (ZoomControl)d;
            zc.ScrollToVerticalOffset((double)e.NewValue);
            if (!zc._isZooming)
                zc.Mode = ZoomControlModes.Custom;
        }

        private static void ModePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var zc = (ZoomControl)d;
            switch ((ZoomControlModes)e.NewValue)
            {
                case ZoomControlModes.Fill:
                    zc.DoZoomToFill();
                    break;
                case ZoomControlModes.Original:
                    zc.DoZoomToOriginal();
                    break;
            }
        }

        private static void ZoomPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var zc = (ZoomControl)d;
            if (zc._scaleTransform == null)
                return;

            var zoom = (double)e.NewValue;
            zc._scaleTransform.ScaleX = zoom;
            zc._scaleTransform.ScaleY = zoom;
            if (!zc._isZooming)
                zc.Mode = ZoomControlModes.Custom;
        }

        private void DoZoom(double deltaZoom, Point origoPosition, Point startHandlePosition, Point targetHandlePosition)
        {
            double currentZoom = Zoom * deltaZoom;
            currentZoom = Math.Max(MinZoom, Math.Min(MaxZoom, currentZoom));

            var endTranslate = (targetHandlePosition - origoPosition) - (startHandlePosition - origoPosition);

            double transformX = TranslateX + endTranslate.X;
            double transformY = TranslateY + endTranslate.Y;

            DoZoomAnimation(currentZoom, transformX, transformY);
            Mode = ZoomControlModes.Custom;
        }

        private void DoZoomAnimation(double targetZoom, double transformX, double transformY)
        {
            _isZooming = true;
            var duration = new Duration(TimeSpan.FromMilliseconds(500));
            StartAnimation(TranslateXProperty, transformX, duration);
            StartAnimation(TranslateYProperty, transformY, duration);
            StartAnimation(ZoomProperty, targetZoom, duration);
        }

        private void StartAnimation(DependencyProperty dp, double toValue, Duration duration)
        {
            if (double.IsNaN(toValue) || double.IsInfinity(toValue))
            {
                if (dp == ZoomProperty)
                    _isZooming = false;

                return;
            }
            var animation = new DoubleAnimation(toValue, duration);
            if (dp == ZoomProperty)
            {
                _zoomAnimCount++;
                animation.Completed += (s, args) =>
                                           {
                                               _zoomAnimCount--;
                                               if (_zoomAnimCount > 0)
                                                   return;

                                               var zoom = Zoom;
                                               BeginAnimation(ZoomProperty, null);
                                               SetValue(ZoomProperty, zoom);
                                               _isZooming = false;
                                           };
            }
            BeginAnimation(dp, animation, HandoffBehavior.Compose);
        }

        public void ZoomToOriginal()
        {
            Mode = ZoomControlModes.Original;
        }

        public void ZoomToFill()
        {
            Mode = ZoomControlModes.Fill;
        }

        private void DoZoomToOriginal()
        {
            var initialTranslate = GetInitialTranslate();
            DoZoomAnimation(1.0, initialTranslate.X, initialTranslate.Y);
        }

        private void DoZoomToFill()
        {
            var initialTranslate = GetInitialTranslate();
            var deltaZoom = Math.Min(ActualWidth / ExtentWidth, ActualHeight / ExtentHeight);
            DoZoomAnimation(deltaZoom, initialTranslate.X, initialTranslate.Y);
        }

        private Vector GetInitialTranslate()
        {
            var tX = -(ActualWidth - ExtentWidth) / 2.0;
            var tY = -(ActualHeight - ExtentHeight) / 2.0;
            return new Vector(tX, tY);
        }
    }
}
