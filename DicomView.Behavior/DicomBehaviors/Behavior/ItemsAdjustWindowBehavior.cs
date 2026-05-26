using DicomView.Behaviors.DicomBehaviors.Controller;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace DicomView.Behaviors.DicomBehaviors
{
    public class ItemsAdjustWindowBehavior : MutualExclusionBehaviorBase<DicomImageShell>
    {
        public Action<Vector> AdjustWindowAction { get; set; }
        public MouseMoveBehaviorController MouseMoveController { get; set; } = MouseMoveBehaviorController.Default;
        public ItemsAdjustWindowBehavior()
        { 
            Tag = "WW\\WL";
            TriggerButton = TriggerButton.Left;
        }

        private Vector _count = new Vector();
        private Point _prePoint;
        protected override bool OnSubBehaviorAttached()
        {
            TriggerElement = AssociatedObject.triggerPanel;
            TriggerElement.PreviewMouseLeftButtonDown += OnPreviewMouseLeftButtonDown;
            TriggerElement.PreviewMouseLeftButtonUp += OnPreviewMouseLeftButtonUp;
            return true;
        }

        protected override void OnSubBehaviorDetaching()
        {
            TriggerElement.PreviewMouseLeftButtonDown -= OnPreviewMouseLeftButtonDown;
            TriggerElement.PreviewMouseLeftButtonUp -= OnPreviewMouseLeftButtonUp;
        }

        private void OnPreviewMouseLeftButtonDown(object sender, MouseEventArgs e)
        {
            if (TriggerElement.CaptureMouse())
            {
                _prePoint = e.GetPosition(TriggerElement);
                TriggerElement.PreviewMouseMove += OnPreviewMouseMove;
            }
        }

        private void OnPreviewMouseLeftButtonUp(object sender, MouseEventArgs e)
        {
            TriggerElement.ReleaseMouseCapture();
            TriggerElement.PreviewMouseMove -= OnPreviewMouseMove;
            AdjustWindowAction?.Invoke(_count);
            AssociatedObject.AdjustWindow(-_count);
            _count = new Vector();
        }

        private void OnPreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var point = e.GetPosition(TriggerElement);
                var vector = point - _prePoint;
                if (MouseMoveController.DistanceThresholdIsReach(vector))
                {
                    AssociatedObject.AdjustWindow(MouseMoveController.VectorCount);
                    _count += vector;
                    MouseMoveController.ResetDistanceThreshold();
                }
                _prePoint = point;
            }
        }
    }
}
