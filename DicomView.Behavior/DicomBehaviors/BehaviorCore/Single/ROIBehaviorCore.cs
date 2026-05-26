using DicomView.Behaviors;
using DicomView.Behaviors.DicomBehaviors.BehaviorCore;
using DicomView.Behaviors.DicomBehaviors.Controller;
using DicomView.Behaviors.DicomInterface;
using DicomView.Behaviors.ROI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Caching;
using System.Security.AccessControl;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace DicomView.Behaviors.DicomBehaviors.BehaviorCore
{
    public class ROIBehaviorCore : BehaviorCore<DicomImageShell>, IToMenuItem
    {
        private MouseButtonEventHandler _previewMouseLeftButtonDown;
        private MouseButtonEventHandler _previewMouseLeftButtonUp;
        private MouseButtonEventHandler _previewMouseRightButtonDown;
        private MouseEventHandler _mouseLeave;
        public static readonly DependencyProperty ROITypeProperty = DependencyProperty.Register("ROIType", typeof(ROIType?), typeof(ROIBehaviorCore), new FrameworkPropertyMetadata(null, ROITypePropertyChangedCallback));
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
            (obj as ROIBehaviorCore).OnROITypeChanged((ROIType?)args.NewValue);
        }

        private void OnROITypeChanged(ROIType? roiType)
        {
            if (roiType == null)
            {
                if (IsActive != false)
                    IsActive = false;
                return;
            }
            IsActive = true;
            var key = roiType.ToString();
            var o = _drawerCache.Get(key);
            if (o != null)
            {
                _currentDrawer = o as IROIDrawer;
            }
            else
            {
                var attribute = typeof(ROIType).GetField(key).GetCustomAttribute<ReflectionTypeAttribute>();
                _currentDrawer = GenerateDrawer(key);
                _cachePolicy.AbsoluteExpiration = DateTime.Now.AddSeconds(60);
                _drawerCache.Add(key, _currentDrawer, _cachePolicy);
            }
            SetDrawAttach(_target);
        }

        private ObjectCache _drawerCache = new MemoryCache("Drawer");
        private CacheItemPolicy _cachePolicy = new CacheItemPolicy();
        protected IROIDrawer _currentDrawer;
        public ROIBehaviorCore()
        {
            Tag = "ROI";
            TriggerButton = TriggerButton.Left;
            _previewMouseLeftButtonDown = OnPreviewMouseLeftButtonDown;
            _previewMouseRightButtonDown = OnPreviewMouseRightButtonDown;
            _previewMouseLeftButtonUp = OnPreviewMouseLeftButtonUp;
            _mouseLeave = OnMouseLeave;
        }

        private IROIDrawer GenerateDrawer(string type)
        {
            var attribute = typeof(ROIType).GetField(type).GetCustomAttribute<ReflectionTypeAttribute>();
            return Activator.CreateInstance(attribute.RefType) as IROIDrawer;
        }

        private void SetDrawAttach(DicomImageShell target)
        {
            if (target != null && _currentDrawer != null)
                _currentDrawer.SetDrawAttach(target.triggerPanel, target.DataContext as IROIFeature);
        }

        public override void SetAttachElement(DicomImageShell target)
        {
            _target = target;
            _triggerElement = _target.triggerPanel;
        }

        protected override void SubAttach()
        {
            if (_target.DataContext is not IROIFeature)
                return;
            //_currentDrawer = new ROILengthLineDrawer(AssociatedObject.GetCanvas(), TriggerElement, AssociatedObject);
            _triggerElement.PreviewMouseLeftButtonDown += _previewMouseLeftButtonDown;
            _triggerElement.PreviewMouseLeftButtonUp += _previewMouseLeftButtonUp;
            _triggerElement.PreviewMouseRightButtonDown += _previewMouseRightButtonDown;
            _triggerElement.MouseLeave += _mouseLeave;
            SetDrawAttach(_target);
        }

        protected override void SubDetach()
        {
            if (ROIType != null)
            {
                ROIType = null;
            }
            _triggerElement.PreviewMouseLeftButtonDown -= _previewMouseLeftButtonDown;
            _triggerElement.PreviewMouseLeftButtonUp -= _previewMouseLeftButtonUp;
            _triggerElement.PreviewMouseRightButtonDown -= _previewMouseRightButtonDown;
            _triggerElement.MouseLeave -= _mouseLeave;
        }

        private void OnPreviewMouseLeftButtonDown(object sender, MouseEventArgs e)
        {
            var dicomImage = (_target.DataContext as IDicomImage);
            _currentDrawer.SetSpacing(dicomImage.SpacingX, dicomImage.SpacingY);
            _currentDrawer.OnPreviewMouseLeftButtonDown(sender, e);
        }

        protected virtual void OnPreviewMouseLeftButtonUp(object sender, MouseEventArgs e)
        {
            _currentDrawer.OnPreviewMouseLeftButtonUp(sender, e);
        }

        protected virtual void OnPreviewMouseRightButtonDown(object sender, MouseEventArgs e)
        {
            _currentDrawer.OnPreviewMouseRightButtonDown(sender, e);
        }

        protected virtual void OnMouseLeave(object sender, MouseEventArgs e)
        {
            _currentDrawer.OnMouseLeave(sender, e);
        }

        public MenuItem ToMenuItem()
        {
            //var first = base.ToMenuItem();
            var first = new MenuItem() { Header = Tag, IsCheckable = false };
            var binding = new Binding();
            binding.Path = new PropertyPath(nameof(Enable));
            binding.Source = this;
            binding.Mode = BindingMode.TwoWay;
            first.SetBinding(MenuItem.IsEnabledProperty, binding);
            var converter = new ROITypeConverter();
            foreach (var roi in Enum.GetValues(typeof(ROIType)))
            {
                var item = new MenuItem() { Header = roi, IsCheckable = true };
                binding = new Binding();
                binding.Path = new PropertyPath(nameof(ROIType));
                binding.Source = this;
                binding.Mode = BindingMode.TwoWay;
                binding.Converter = converter;
                binding.ConverterParameter = roi;
                item.SetBinding(MenuItem.IsCheckedProperty, binding);
                first.Items.Add(item);
            }
            return first;
        }
    }
}
