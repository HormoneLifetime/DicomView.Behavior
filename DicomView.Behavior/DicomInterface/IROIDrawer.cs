using DicomView.Behaviors.ROI.Shapes;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace DicomView.Behaviors.DicomInterface
{
    public interface IROIDrawer
    {
        void OnPreviewMouseLeftButtonDown(object sender, MouseEventArgs e);
        void OnPreviewMouseLeftButtonUp(object sender, MouseEventArgs e);
        void OnPreviewMouseRightButtonDown(object sender, MouseEventArgs e);
        void OnMouseLeave(object sender, MouseEventArgs e);
        void SetSpacing(double spacingX, double spacingY);
        void SetDrawAttach(UIElement triggerElement, IROIFeature dicomImage);
        DicomROIBaseShape GetRenderedROI();
        void RemoveCurrent();
    }
}
