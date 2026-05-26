using DicomView.Behaviors.DicomInterface;
using DicomView.Behaviors.ROI.Shapes;
using FellowOakDicom.Imaging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DicomView.Behaviors.ROI.Drawers
{
    internal class ROILengthLineDrawer : ROIDrawerBase<LengthLine>, IROIDrawer
    {
        public void OnPreviewMouseLeftButtonDown(object sender, MouseEventArgs e)
        {
            var point = e.GetPosition(_triggerElement);
            _roiShape = new LengthLine()
            {
                StartPoint = point,
                EndPoint = point,
                SpacingX = _spacingX,
                SpacingY = _spacingY,
                LengthUnit = "mm",
            };
            OnRenderStart(_roiShape);
            _dicomImage.AddROI(_roiShape);
            _triggerElement.PreviewMouseMove += OnPreviewMouseMove;
            e.Handled = true;
        }

        public void OnPreviewMouseLeftButtonUp(object sender, MouseEventArgs e)
        {
            if (_roiShape != null && !_roiShape.IsRendered)
            {
                var end = e.GetPosition(_triggerElement);
                if (end == _roiShape.StartPoint)
                {
                    _dicomImage.RemoveROI(_roiShape);
                }
                RenderEnded();
                e.Handled = true;
            }
        }

        private void RenderEnded()
        {
            _roiShape.IsRendered = true;
            _triggerElement.PreviewMouseMove -= OnPreviewMouseMove;
        }

        public void OnPreviewMouseRightButtonDown(object sender, MouseEventArgs e)
        {
            if (_roiShape != null && !_roiShape.IsRendered)
            {
                _dicomImage.RemoveROI(_roiShape);
                RenderEnded();
                e.Handled = true;
            }
        }

        public void OnMouseLeave(object sender, MouseEventArgs e)
        {
            if (_roiShape != null && !_roiShape.IsRendered)
            {
                RenderEnded();
            }
        }

        protected override void OnPreviewMouseMove(object sender, MouseEventArgs e)
        {
            _roiShape.EndPoint = e.GetPosition(_triggerElement);
        }
    }
}
