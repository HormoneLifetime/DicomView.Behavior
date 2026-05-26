using DicomView.Behaviors.DicomBehaviors.Controller;
using DicomView.Behaviors.DicomInterface;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace DicomView.Behaviors.DicomBehaviors.BehaviorCore
{
    public abstract class BehaviorCoreBase : DependencyObject
    {
        public static readonly DependencyProperty TriggerButtonProperty = DependencyProperty.Register("TriggerButton", typeof(TriggerButton), typeof(BehaviorCoreBase));
        public TriggerButton TriggerButton
        {
            get { return (TriggerButton)GetValue(TriggerButtonProperty); }
            set
            {
                SetValue(TriggerButtonProperty, value);
            }
        }

        public static readonly DependencyProperty IsActiveProperty = DependencyProperty.Register("IsActive", typeof(bool), typeof(BehaviorCoreBase), new FrameworkPropertyMetadata(false, IsActivePropertyChangedCallback));
        public bool IsActive
        {
            get { return (bool)GetValue(IsActiveProperty); }
            set { SetValue(IsActiveProperty, value); }
        }

        private static void IsActivePropertyChangedCallback(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var isActive = (bool)args.NewValue;
            var core = obj as BehaviorCoreBase;
            core.OnIsActiveChanged(isActive);
        }

        protected abstract void OnIsActiveChanged(bool isActive);

        public static readonly DependencyProperty EnableProperty = DependencyProperty.Register("Enable", typeof(bool), typeof(BehaviorCoreBase), new FrameworkPropertyMetadata(true));
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
        public string Tag { get; set; }
    }

    interface IAssociatedBehaviorCore<Associate, Attach> where Attach : UIElement where Associate : UIElement
    {
        Associate AssociatedObject { get; set; }
        void OnAssociatedObjectChanged(Associate oldValue, Associate newValue);
    }

    public abstract class BehaviorCore<T> : BehaviorCoreBase where T : UIElement
    {
        protected override void OnIsActiveChanged(bool isActive)
        {
            OnActiveChanged?.Invoke(this);
        }

        public Action<BehaviorCore<T>> OnActiveChanged;
        protected UIElement _triggerElement;
        protected T _target;

        public void AttachTo(T target)
        {
            if (target != null && target != _target)
            {
                SetAttachElement(target);
                SubAttach();
            }
        }

        public virtual void Detach()
        {
            if (_target != null)
            {
                SubDetach();
                _target = null;
            }
        }

        public void Detach(T target)
        {
            SetAttachElement(target);
            SubDetach();
            _target = null;
        }

        public abstract void SetAttachElement(T target);
        protected abstract void SubAttach();

        protected abstract void SubDetach();
    }
}
