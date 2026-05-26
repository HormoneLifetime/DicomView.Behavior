using DicomView.Behaviors.ROI.Drawers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace DicomView.Behaviors.ROI
{
    public enum ROIType
    {
        [ReflectionType(typeof(ROILengthLineDrawer))]
        Line,
        [ReflectionType(typeof(ROILengthLineDrawer))]
        Ellipse
    }

    [AttributeUsage(AttributeTargets.Field)]
    internal class ReflectionTypeAttribute : Attribute
    {
        public Type  RefType{ get; }

        public ReflectionTypeAttribute(Type type)
        {
            RefType = type;
        }
    }

    public class ROITypeConverter : StructCheckedConverter<ROIType?>
    {

    }

    public abstract class StructCheckedConverter<T> : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            T t = (T)value;
            return t.Equals((T)parameter);//不适用于引用类型，但是约束无法指定struct?
        }

        public virtual object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool isChecked = (bool)value;
            if (!isChecked)
            {
                return null;
            }
            return (T)parameter;
        }
    }
}
