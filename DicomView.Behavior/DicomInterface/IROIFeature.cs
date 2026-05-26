using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Shapes;

namespace DicomView.Behaviors.DicomInterface
{
    public interface IROIFeature
    {
        void AddROI(Shape shape);

        void RemoveROI(Shape shape);

        int ROICount();
    }
}
