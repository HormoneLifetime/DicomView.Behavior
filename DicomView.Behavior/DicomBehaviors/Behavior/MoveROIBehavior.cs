using DicomView.Behaviors;
using DicomView.Behaviors.ROI.Shapes;
using FellowOakDicom.Imaging;
using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace DicomView.Behaviors.DicomBehaviors
{
    public class MoveROIBehavior : Behavior<DicomImageShell>
    {
        private DicomROIBaseShape _shape;
        private Point _prePoint;
        private Canvas _canvas;
        protected override void OnAttached()
        {
            base.OnAttached();
            _canvas = AssociatedObject.GetCanvas();
            AssociatedObject.PreviewMouseLeftButtonDown += OnPreviewMouseLeftButtonDown;
            AssociatedObject.PreviewMouseLeftButtonUp += OnPreviewMouseLeftButtonUp;
            AssociatedObject.PreviewMouseMove += OnPreviewMouseMove;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.PreviewMouseLeftButtonDown -= OnPreviewMouseLeftButtonDown;
            AssociatedObject.PreviewMouseLeftButtonUp -= OnPreviewMouseLeftButtonUp;
            AssociatedObject.PreviewMouseRightButtonDown -= OnPreviewMouseMove;
        }

        private void OnPreviewMouseLeftButtonDown(object sender, MouseEventArgs e)
        {
            if (_canvas.Children.Count == 0)
                return;
            _prePoint = e.GetPosition(_canvas);
            //用canvas可以减少一计算深度
            VisualTreeHelper.HitTest(_canvas, HitTestFilter, HitTestResult, new PointHitTestParameters(_prePoint));
            if (_shape != null && _shape.IsRendered)
            {
                _shape.CaptureMouse();
                e.Handled = true;
            }
        }

        private void OnPreviewMouseLeftButtonUp(object sender, MouseEventArgs e)
        {
            if (_shape != null)
            {
                _shape.ReleaseMouseCapture();
                _shape = null;
                e.Handled = true;
            }
        }

        private void OnPreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (_shape != null && e.LeftButton == MouseButtonState.Pressed)
            {
                var point = e.GetPosition(_canvas);
                var left = Canvas.GetLeft(_shape);
                var top = Canvas.GetTop(_shape);
                left += point.X - _prePoint.X;
                top += point.Y - _prePoint.Y;
                Canvas.SetLeft(_shape, left);
                Canvas.SetTop(_shape, top);
                _prePoint = point;
                e.Handled = true;
            }
        }

        internal HitTestFilterBehavior HitTestFilter(DependencyObject o)
        {
            if (o is DicomROIBaseShape)
                return HitTestFilterBehavior.ContinueSkipChildren;
            return HitTestFilterBehavior.Continue;
        }

        internal HitTestResultBehavior HitTestResult(HitTestResult result)
        {
            _shape = result.VisualHit as DicomROIBaseShape;
            return HitTestResultBehavior.Stop;
        }
    }
}
