using DicomView.Behaviors.DicomBehaviors.Controller;
using DicomView.Behaviors.DicomInterface;
using DicomView.Behaviors.ROI;
using DicomView.Behaviors.ROI.Shapes;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Caching;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace DicomView.Behaviors.DicomBehaviors
{
    public class ItemsROIBehavior : MutualExclusionBehaviorBase<DicomImageShell>
    {
        public static readonly DependencyProperty ROITypeProperty = DependencyProperty.Register("ROIType", typeof(ROIType?), typeof(ItemsROIBehavior), new FrameworkPropertyMetadata(null, ROITypePropertyChangedCallback));
        public ROIType? ROIType
        {
            get { return (ROIType?)GetValue(ROITypeProperty); }
            set
            {
                SetValue(ROITypeProperty, value);
            }
        }

        private static void ROITypePropertyChangedCallback(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            (obj as ItemsROIBehavior).OnROITypeChanged((ROIType?)args.NewValue);
        }

        private void OnROITypeChanged(ROIType? roiType)
        {
            if (roiType == null)
            {
                if (IsActive != false)
                    IsActive = false;
                return;
            }
            var key = roiType.ToString();
            var o = _drawerCache.Get(key);
            if (o != null)
            {
                _currentDrawer = o as IROIDrawer;
            }
            else
            {
                _currentDrawer = GenerateDrawer(key);
                _cachePolicy.AbsoluteExpiration = DateTime.Now.AddSeconds(60);
                _drawerCache.Add(key, _currentDrawer, _cachePolicy);
            }
            SetDrawAttach();
            IsActive = true;
        }

        private IROIDrawer GenerateDrawer(string type)
        {
            var attribute = typeof(ROIType).GetField(type).GetCustomAttribute<ReflectionTypeAttribute>();
            return Activator.CreateInstance(attribute.RefType) as IROIDrawer;
        }

        protected override void OnTnactive()
        {
            base.OnTnactive();
            if (ROIType != null)
            {
                ROIType = null;
            }
        }

        private DicomROIBaseShape _added;
        public Action<DicomROIBaseShape> AddROIAction { get; set; }
        private ObjectCache _drawerCache = new MemoryCache("Drawer");
        private CacheItemPolicy _cachePolicy = new CacheItemPolicy();
        private IROIDrawer _currentDrawer;
        public ItemsROIBehavior()
        {
            Tag = "ROI";
            TriggerButton = TriggerButton.Left;
        }

        protected override bool OnSubBehaviorAttached()
        {
            if (AssociatedObject.DataContext is not IROIFeature)
                return false;
            TriggerElement = AssociatedObject.triggerPanel;
            //_currentDrawer = new ROILengthLineDrawer(AssociatedObject.GetCanvas(), TriggerElement, AssociatedObject);
            TriggerElement.PreviewMouseLeftButtonDown += OnPreviewMouseLeftButtonDown;
            TriggerElement.PreviewMouseLeftButtonUp += OnPreviewMouseLeftButtonUp;
            TriggerElement.PreviewMouseRightButtonDown += OnPreviewMouseRightButtonDown;
            TriggerElement.MouseLeave += OnMouseLeave;
            return true;
        }

        private void SetDrawAttach()
        {
            if (_currentDrawer != null)
                _currentDrawer.SetDrawAttach(AssociatedObject.triggerPanel, AssociatedObject.DataContext as IROIFeature);
        }

        protected override void OnSubBehaviorDetaching()
        {
            TriggerElement.PreviewMouseLeftButtonDown -= OnPreviewMouseLeftButtonDown;
            TriggerElement.PreviewMouseLeftButtonUp -= OnPreviewMouseLeftButtonUp;
            TriggerElement.PreviewMouseRightButtonDown -= OnPreviewMouseRightButtonDown;
            TriggerElement.MouseLeave -= OnMouseLeave;
        }

        private void OnPreviewMouseLeftButtonDown(object sender, MouseEventArgs e)
        {
            var dicomImage = (AssociatedObject.DataContext as IDicomImage);
            _currentDrawer.SetSpacing(dicomImage.SpacingX, dicomImage.SpacingY);
            _currentDrawer.OnPreviewMouseLeftButtonDown(sender, e);
        }

        private void OnPreviewMouseLeftButtonUp(object sender, MouseEventArgs e)
        {
            _currentDrawer.OnPreviewMouseLeftButtonUp(sender, e);
            AddROI();
        }

        private void OnPreviewMouseRightButtonDown(object sender, MouseEventArgs e)
        {
            _currentDrawer.OnPreviewMouseRightButtonDown(sender, e);
        }

        private void OnMouseLeave(object sender, MouseEventArgs e)
        {
            _currentDrawer.OnMouseLeave(sender, e);
            AddROI();
        }

        private void AddROI()
        {
            var shape = _currentDrawer.GetRenderedROI();
            if (_added != shape && shape.IsRendered)
            {
                _added = shape;
                _currentDrawer.RemoveCurrent();
                AddROIAction?.Invoke(shape);
            }
        }
    }
}
