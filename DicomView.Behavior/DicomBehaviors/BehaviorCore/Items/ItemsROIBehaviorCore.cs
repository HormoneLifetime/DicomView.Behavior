using DicomView.Behaviors.DicomBehaviors.BehaviorCore;
using DicomView.Behaviors.ROI;
using DicomView.Behaviors.ROI.Shapes;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using System.Windows.Shapes;

namespace DicomView.Behaviors.DicomBehaviors.BehaviorCore
{
    public class ItemsROIBehaviorCore : ROIBehaviorCore
    {
        private DicomROIBaseShape _added;
        public Action<DicomROIBaseShape> AddROIAction { get; set; }
        protected override void OnPreviewMouseLeftButtonUp(object sender, MouseEventArgs e)
        {
            base.OnPreviewMouseLeftButtonUp(sender, e);
            AddROI();
        }

        protected override void OnPreviewMouseRightButtonDown(object sender, MouseEventArgs e)
        {
            base.OnPreviewMouseRightButtonDown(sender, e);
        }

        protected override void OnMouseLeave(object sender, MouseEventArgs e)
        {
            base.OnMouseLeave(sender, e);
            AddROI();
        }

        private void AddROI()
        {
            var shape = _currentDrawer.GetRenderedROI();
            if (_added != shape && shape.IsRendered)
            {
                _added = shape;
                _currentDrawer.RemoveCurrent();
                AddROIAction?.Invoke(shape);
            }
        }
    }
}
