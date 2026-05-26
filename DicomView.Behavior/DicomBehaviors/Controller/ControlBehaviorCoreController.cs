using DicomView.Behaviors.DicomBehaviors.BehaviorCore;
using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DicomView.Behaviors.DicomBehaviors.Controller
{
    public class DicomImageBehaviorCoreController : SingleControlBehaviorCoreController<DicomImageShell>
    {

    }

    public class SingleControlBehaviorCoreController<Attach> : ControlBehaviorCoreController<Attach, Attach> where Attach : UIElement
    {
        protected override void StayUnique(BehaviorCore<Attach> core)
        {
            if (core.IsActive)
            {
                if (_controller.Remove(core.TriggerButton, out var lastCore) && AutoActive)
                    lastCore.IsActive = false;
                _controller.TryAdd(core.TriggerButton, core);
                core.AttachTo(AssociatedObject);
            }
        }
    }


    public interface IBehaviorController<T> where T : UIElement
    {
        FreezableCollection<BehaviorCore<T>> BehaviorCores { get; }
    }

    [Flags]
    public enum TriggerButton
    {
        None = 0,
        Left = 1,
        Middle = 1 << 1,
        Right = 1 << 2,
        XButton1 = 1 << 3,
        XButton2 = 1 << 4
    }

    public abstract class ControlBehaviorCoreController<Control, Attach> : Behavior<Control>, IBehaviorController<Attach> where Control : UIElement where Attach : UIElement
    {
        public static readonly DependencyProperty AutoActiveProperty = DependencyProperty.Register("AutoActive", typeof(bool), typeof(ControlBehaviorCoreController<Control, Attach>));
        public bool AutoActive
        {
            get { return (bool)GetValue(AutoActiveProperty); }
            set
            {
                SetValue(AutoActiveProperty, value);
            }
        }

        protected Dictionary<TriggerButton, BehaviorCore<Attach>> _controller = new Dictionary<TriggerButton, BehaviorCore<Attach>>();

        private FreezableCollection<BehaviorCore<Attach>> _behaviorCores = new FreezableCollection<BehaviorCore<Attach>>();
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public FreezableCollection<BehaviorCore<Attach>> BehaviorCores { get { return _behaviorCores; } }

        protected override void OnAttached()
        {
            base.OnAttached();
            BeginObserve();
        }

        private void BeginObserve()
        {
            foreach (var core in _behaviorCores)
            {
                AddNew(core);
            }
            (_behaviorCores as INotifyCollectionChanged).CollectionChanged += OnBehaviorsCollectionChanged;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            EndObserve();
        }

        protected virtual void AddNew(BehaviorCore<Attach> core)
        {
            core.OnActiveChanged += OnActiveChanged;
            StayUnique(core);
        }

        protected void OnActiveChanged(BehaviorCore<Attach> core)
        {
            if (core.IsActive)
            {
                StayUnique(core);
            }
            else
            {
                OnTnactive(core);
            }
        }

        protected virtual void OnTnactive(BehaviorCore<Attach> core)
        {
            core.Detach();
        }

        protected virtual void StayUnique(BehaviorCore<Attach> core)
        {
            if (core.IsActive)
            {
                if (_controller.Remove(core.TriggerButton, out var lastCore) && AutoActive)
                    lastCore.IsActive = false;
                _controller.TryAdd(core.TriggerButton, core);
            }
        }


        protected virtual void RemoveOld(BehaviorCore<Attach> core)
        {
            core.IsActive = false;
        }

        private void EndObserve()
        {
            (_behaviorCores as INotifyCollectionChanged).CollectionChanged -= OnBehaviorsCollectionChanged;
            _controller.Clear();
        }

        private void OnBehaviorsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewItems != null)
                        foreach (var core in e.NewItems)
                        {
                            AddNew(core as BehaviorCore<Attach>);
                        }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    if (e.OldItems != null)
                        foreach (var core in e.OldItems)
                        {
                            RemoveOld(core as BehaviorCore<Attach>);
                        }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    if (e.NewItems != null)
                        foreach (var core in e.NewItems)
                        {
                            AddNew(core as BehaviorCore<Attach>);
                        }
                    if (e.OldItems != null)
                        foreach (var core in e.OldItems)
                        {
                            RemoveOld(core as BehaviorCore<Attach>);
                        }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    if (_behaviorCores != null)
                        foreach (var core in _behaviorCores)
                        {
                            AddNew(core);
                        }
                    break;
            }
        }
    }
}
