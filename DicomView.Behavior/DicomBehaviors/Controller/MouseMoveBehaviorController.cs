using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;

namespace DicomView.Behaviors.DicomBehaviors.Controller
{
    public class MouseMoveBehaviorController : DependencyObject
    {
        public static readonly DependencyProperty DistanceThresholdProperty = DependencyProperty.Register("DistanceThreshold", typeof(double), typeof(MouseMoveBehaviorController));
        public static MouseMoveBehaviorController Default = new MouseMoveBehaviorController() { DistanceThreshold = 10 };
        public double DistanceThreshold
        {
            get { return (double)GetValue(DistanceThresholdProperty); }
            set
            {
                SetValue(DistanceThresholdProperty, value);
            }
        }

        private double _distanceCount = 0;
        private Vector _vectorCount = new Vector(0, 0);
        public Vector VectorCount
        {
            get { return _vectorCount; }
        }
        public bool DistanceThresholdIsReach(Vector vector)
        {
            _vectorCount += vector;
            _distanceCount += vector.Length;
            if (_distanceCount >= DistanceThreshold)
            {
                return true;
            }
            return false;
        }

        public void ResetDistanceThreshold()
        {
            _distanceCount = 0;
            _vectorCount = new Vector(0, 0);
        }
    }
}
