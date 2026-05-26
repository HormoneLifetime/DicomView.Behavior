using DicomView.Behaviors.DicomBehaviors.BehaviorCore;
using DicomView.Behaviors.DicomInterface;
using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace DicomView.Behaviors.DicomBehaviors.Controller
{
    public class DicomImagesBehaviorCoreController : ItemsControlBehaviorCoreController<ItemsControl, DicomImageShell>
    {

    }

    public class ItemsControlBehaviorCoreController<Items, Control> : ControlBehaviorCoreController<Items, Control> where Control : FrameworkElement where Items : ItemsControl
    {
        private Dictionary<BehaviorCore<Control>, List<Control>> _coreElements = new Dictionary<BehaviorCore<Control>, List<Control>>();

        public void SetAttach(Control attachedElement, TriggerButton changedButton)
        {
            if (_controller.TryGetValue(changedButton, out var lastCore))
            {
                var triggers = ItemsControlBehaviorCoreController<ItemsControl, Control>.GetAttachedTriggers(attachedElement);
                if (!triggers.HasFlag(changedButton))
                {
                    lastCore.AttachTo(attachedElement);
                    triggers |= changedButton;
                    _coreElements[lastCore].Add(attachedElement);
                    ItemsControlBehaviorCoreController<ItemsControl, Control>.SetAttachedTriggers(attachedElement, triggers);
                }
            }
        }

        protected override void OnTnactive(BehaviorCore<Control> core)
        {
            var elements = _coreElements[core];
            for (int i = 0; i < elements.Count; ++i)
            {
                var element = elements[i];
                var triggers = ItemsControlBehaviorCoreController<ItemsControl, Control>.GetAttachedTriggers(element);
                triggers &= ~core.TriggerButton;
                ItemsControlBehaviorCoreController<ItemsControl, Control>.SetAttachedTriggers(element, triggers);
                core.Detach(element);
            }
            elements.Clear();
        }

        protected override void AddNew(BehaviorCore<Control> core)
        {
            base.AddNew(core);
            if (core is IAssociatedBehaviorCore<Items, Control> itemsCore)
                itemsCore.AssociatedObject = AssociatedObject;
            _coreElements[core] = new List<Control>(AssociatedObject.Items.Count < 16 ? AssociatedObject.Items.Count : AssociatedObject.Items.Count / 5);
        }


        protected override void RemoveOld(BehaviorCore<Control> core)
        {
            base.RemoveOld(core);
            if (core is IAssociatedBehaviorCore<Items, Control> itemsCore)
                itemsCore.AssociatedObject = null;
            _coreElements.Remove(core);
        }
        
        private static readonly DependencyProperty AttachedTriggersProperty = DependencyProperty.RegisterAttached("AttachedTriggers", typeof(TriggerButton), typeof(ItemsControlBehaviorCoreController<ItemsControl, Control>), new FrameworkPropertyMetadata(TriggerButton.None));
        public static TriggerButton GetAttachedTriggers(FrameworkElement element)
        {
            return (TriggerButton)element.GetValue(AttachedTriggersProperty);
        }

        public static void SetAttachedTriggers(FrameworkElement element, TriggerButton trigger)
        {
            element.SetValue(AttachedTriggersProperty, trigger);
        }
    }
}
