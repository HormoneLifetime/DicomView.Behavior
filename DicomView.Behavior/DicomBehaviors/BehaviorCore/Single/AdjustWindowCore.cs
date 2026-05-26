using DicomView.Behaviors.DicomBehaviors.Controller;
using DicomView.Behaviors.DicomInterface;
using FellowOakDicom.Imaging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DicomView.Behaviors.DicomBehaviors.BehaviorCore
{
    public class AdjustWindowCore : BehaviorCore<DicomImageShell>, IToMenuItem
    {
        public MouseMoveBehaviorController MouseMoveController { get; set; } = MouseMoveBehaviorController.Default;
        protected Point _prePoint;
        private MouseButtonEventHandler _previewMouseLeftButtonDown;
        private MouseButtonEventHandler _previewMouseLeftButtonUp;
        private MouseEventHandler _previewMouseMove;

        public AdjustWindowCore()
        {
            Tag = "WW\\WL";
            TriggerButton = TriggerButton.Left;
            _previewMouseLeftButtonDown = OnPreviewMouseLeftButtonDown;
            _previewMouseLeftButtonUp = OnPreviewMouseLeftButtonUp;
            _previewMouseMove = OnPreviewMouseMove;
        }

        public override void SetAttachElement(DicomImageShell target)
        {
            _target = target;
            _triggerElement = _target.triggerPanel;
        }

        protected override void SubAttach()
        {
            _triggerElement.PreviewMouseLeftButtonDown += _previewMouseLeftButtonDown;
            _triggerElement.PreviewMouseLeftButtonUp += _previewMouseLeftButtonUp;
        }
        protected override void SubDetach()
        {
            _triggerElement.PreviewMouseLeftButtonDown -= _previewMouseLeftButtonDown;
            _triggerElement.PreviewMouseLeftButtonUp -= _previewMouseLeftButtonUp;
            _triggerElement.PreviewMouseMove -= _previewMouseMove;
        }


        private void OnPreviewMouseLeftButtonDown(object sender, MouseEventArgs e)
        {
            if (_triggerElement.CaptureMouse())
            {
                _prePoint = e.GetPosition(_triggerElement);
                _triggerElement.PreviewMouseMove += _previewMouseMove;
            }
        }

        protected virtual void OnPreviewMouseLeftButtonUp(object sender, MouseEventArgs e)
        {
            _triggerElement.ReleaseMouseCapture();
            _triggerElement.PreviewMouseMove -= _previewMouseMove;
        }

        protected virtual void OnPreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var point = e.GetPosition(_triggerElement);
                var vector = point - _prePoint;
                if (MouseMoveController.DistanceThresholdIsReach(vector))
                {
                    AdjustWindow();
                    MouseMoveController.ResetDistanceThreshold();
                }
                _prePoint = point;
            }
        }

        protected virtual void AdjustWindow()
        {
            _target.AdjustWindow(MouseMoveController.VectorCount);
        }

        public MenuItem ToMenuItem()
        {
            return this.ToDefaultMenuItem();
        }
    }
}
