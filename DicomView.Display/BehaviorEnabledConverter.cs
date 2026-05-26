using DicomView.Behaviors.DicomBehaviors;
using DicomView.Behaviors.DicomBehaviors.BehaviorCore;
using DicomView.Behaviors.ROI;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Data;

namespace DicomView.Display
{
    public class BehaviorActiveConverter : StructCheckedConverter<BehaviorType>
    {
        public override object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool isChecked = (bool)value;
            if (!isChecked)
            {
                return BehaviorType.None;
            }
            return (BehaviorType)parameter;
        }
    }
}
