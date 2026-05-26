using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace DicomView.Behaviors.ROI.Shapes
{
    public abstract class DicomROIBaseShape : Shape, ICloneable
    {
        public bool IsRendered { get; set; }

        public double SpacingX { get; set; } = 1;
        public double SpacingY { get; set; } = 1;

        public abstract object Clone();
    }
}
