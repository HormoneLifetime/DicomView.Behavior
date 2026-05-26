using DicomView.Behaviors.DicomBehaviors.BehaviorCore;
using DicomView.Behaviors.DicomBehaviors.Controller;
using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Forms.Design.Behavior;

namespace DicomView.Behaviors.DicomBehaviors
{
    public abstract class MutualExclusionBehaviorBase<T> : Behavior<T> where T : DependencyObject
    {
        public static readonly DependencyProperty TriggerButtonProperty = DependencyProperty.Register("TriggerButton", typeof(TriggerButton), typeof(MutualExclusionBehaviorBase<T>));
        public TriggerButton TriggerButton
        {
            get { return (TriggerButton)GetValue(TriggerButtonProperty); }
            set
            {
                SetValue(TriggerButtonProperty, value);
            }
        }

        public static readonly DependencyProperty IsActiveProperty = DependencyProperty.Register("IsActive", typeof(bool), typeof(MutualExclusionBehaviorBase<T>), new FrameworkPropertyMetadata(false, IsActivePropertyChangedCallback));
        public bool IsActive
        {
            get { return (bool)GetValue(IsActiveProperty); }
            set { SetValue(IsActiveProperty, value); }
        }

        public static readonly DependencyProperty EnableProperty = DependencyProperty.Register("Enable", typeof(bool), typeof(MutualExclusionBehaviorBase<T>), new FrameworkPropertyMetadata(true));
        public bool Enable
        {
            get { return (bool)GetValue(EnableProperty); }
            set
            {
                SetValue(EnableProperty, value);
                if (!value)
                {
                    SetValue(IsActiveProperty, value);
                }
            }
        }

        private static void IsActivePropertyChangedCallback(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var isActive = (bool)args.NewValue;
            var behavior = obj as MutualExclusionBehaviorBase<T>;
            behavior.OnIsActiveChanged(isActive);
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            if (!Enable || !IsActive)
                return;
            OnSubBehaviorAttached();
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            OnSubBehaviorDetaching();
        }

        protected abstract bool OnSubBehaviorAttached();
        protected abstract void OnSubBehaviorDetaching();

        protected UIElement TriggerElement { get; set; }

        private void OnIsActiveChanged(bool isActive)
        {
            if (isActive)
            {
                if (!Enable || !IsActive)
                    return;
                OnSubBehaviorAttached();
            }
            else
            {
                OnTnactive();
            }
        }

        protected virtual void OnTnactive()
        {
            if (AssociatedObject != null)
                OnSubBehaviorDetaching();
        }

        public string Tag { get; set; }
    }
}
