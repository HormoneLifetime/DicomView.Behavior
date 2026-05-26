using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;

namespace DicomView.Behaviors.DicomBehaviors.BehaviorCore
{
    static class BehaviorCoreHelper
    {
        public static MenuItem ToDefaultMenuItem(this BehaviorCoreBase behaviorCoreBase)
        {
            Binding binding = new Binding();
            binding.Path = new PropertyPath(nameof(BehaviorCoreBase.IsActive));
            binding.Source = behaviorCoreBase;
            binding.Mode = BindingMode.TwoWay;
            var item = new MenuItem() { Header = behaviorCoreBase.Tag, IsCheckable = true };
            item.SetBinding(MenuItem.IsCheckedProperty, binding);
            binding = new Binding();
            binding.Path = new PropertyPath(nameof(BehaviorCoreBase.Enable));
            binding.Source = behaviorCoreBase;
            binding.Mode = BindingMode.TwoWay;
            item.SetBinding(MenuItem.IsEnabledProperty, binding);
            return item;
        }
    }
}
