using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace DicomView.Behaviors.DicomBehaviors.BehaviorCore
{
    public class ItemsAdjustWindowCore : AdjustWindowCore
    {
        public Action<Vector> AdjustWindowAction { get; set; }

        private Vector _count = new Vector();

        protected override void OnPreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var point = e.GetPosition(_triggerElement);
                var vector = point - _prePoint;
                if (MouseMoveController.DistanceThresholdIsReach(vector))
                {
                    AdjustWindow();
                    _count += vector;
                    MouseMoveController.ResetDistanceThreshold();
                }
                _prePoint = point;
            }
        }

        protected override void OnPreviewMouseLeftButtonUp(object sender, MouseEventArgs e)
        {
            //鼠标释放后修改所有选中项，当前对象会多修改一次，所以反向调节一次
            base.OnPreviewMouseLeftButtonUp(sender, e);
            AdjustWindowAction?.Invoke(_count);
            _target.AdjustWindow(-_count);
            _count = new Vector();
        }
    }
}
