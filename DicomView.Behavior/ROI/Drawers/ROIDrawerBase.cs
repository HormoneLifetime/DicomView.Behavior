using DicomView.Behaviors.DicomInterface;
using DicomView.Behaviors.ROI.Shapes;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;

namespace DicomView.Behaviors.ROI.Drawers
{
    internal abstract class ROIDrawerBase<T> where T : DicomROIBaseShape, ICloneable
    {
        protected UIElement _triggerElement;
        protected IROIFeature _dicomImage;
        protected double _spacingX;
        protected double _spacingY;
        protected MouseEventHandler PreviewMouseMove;
        protected T _roiShape;

        internal ROIDrawerBase()
        {
            PreviewMouseMove = OnPreviewMouseMove;
        }

        public void SetDrawAttach(UIElement triggerElement, IROIFeature dicomImage)
        {
            _triggerElement = triggerElement;
            _dicomImage = dicomImage;
        }

        protected abstract void OnPreviewMouseMove(object sender, MouseEventArgs e);

        public virtual void OnRenderStart(Shape shape)
        {
            shape.SetAttach(_dicomImage);
        }

        public void SetSpacing(double spacingX, double spacingY)
        {
            _spacingX = spacingX;
            _spacingY = spacingY;
        }

        public DicomROIBaseShape GetRenderedROI()
        {
            return _roiShape;
        }

        public void RemoveCurrent()
        {
            if (_roiShape.IsRendered)
                _dicomImage.RemoveROI(_roiShape);
        }
    }
}
