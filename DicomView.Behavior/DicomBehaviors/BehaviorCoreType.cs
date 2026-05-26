using System;
using System.Collections.Generic;
using System.Text;

namespace DicomView.Behaviors.DicomBehaviors
{
    public enum BehaviorType
    {
        None = 0,
        WindowLevel = 1,
        Zoom = 1 << 1,
        ROI = 1 << 2,
        Move = 1 << 3,
    }
}
