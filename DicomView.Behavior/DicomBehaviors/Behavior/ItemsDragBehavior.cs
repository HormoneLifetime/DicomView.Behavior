using DicomView.Behaviors.DicomBehaviors.BehaviorCore;
using DicomView.Behaviors.DicomBehaviors.Controller;
using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace DicomView.Behaviors.DicomBehaviors
{
    public abstract class ItemsDragBehaviorBase<T, Trigger> : MutualExclusionBehaviorBase<T> where T : ItemsControl where Trigger : FrameworkElement
    {
        protected DragAdorner _dragAdorner;
        protected AdornerLayer _layer;
        protected UIElement _holder;
        protected Trigger _testResult;

        public static readonly DependencyProperty AllowOverBoundsProperty = DependencyProperty.Register("AllowOverBounds", typeof(bool), typeof(ItemsDragBehaviorBase<T, Trigger>), new FrameworkPropertyMetadata(false, AllowOverBoundsPropertyChangedCallback));

        public bool AllowOverBounds
        {
            get { return (bool)GetValue(AllowOverBoundsProperty); }
            set
            {
                SetValue(AllowOverBoundsProperty, value);
            }
        }

        private static void AllowOverBoundsPropertyChangedCallback(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var behavior = obj as ItemsDragBehaviorBase<T, Trigger>;
            if (behavior != null)
            {
                behavior.InitAdornerLayer((bool)args.NewValue);
            }
        }

        protected override bool OnSubBehaviorAttached()
        {
            AssociatedObject.SetValue(UIElement.AllowDropProperty, true);
            if (AssociatedObject.IsLoaded)
            {                 
                InitAdornerLayer(AllowOverBounds);
                return true;
            }
            else
            {
                AssociatedObject.Loaded += OnLoaded;
            }
            return true;
        }

        protected override void OnSubBehaviorDetaching()
        {
            AssociatedObject.ClearValue(UIElement.AllowDropProperty);
            AssociatedObject.Loaded -= OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            InitAdornerLayer(AllowOverBounds);
        }

        protected void InitAdornerLayer(bool alowOverBounds)
        {
            if (_layer == null)
            {
                var window = Window.GetWindow(AssociatedObject);
                _layer = GetCurrentAdornerLayer(window);
                if (alowOverBounds)
                {
                    _holder = window;
                    window.SetCurrentValue(UIElement.AllowDropProperty, true);
                }
                else
                {
                    _holder = AssociatedObject;
                }
            }
        }

        protected AdornerLayer GetCurrentAdornerLayer(Visual visual)
        {
            var child = VisualTreeHelper.GetChild(visual, 0);
            while (child != null)
            {
                if (child is AdornerDecorator)
                    return ((AdornerDecorator)child).AdornerLayer;
                if (child is ScrollContentPresenter)
                    return ((ScrollContentPresenter)child).AdornerLayer;

                child = VisualTreeHelper.GetChild(child, 0);
            }
            return null;
        }

        protected HitTestFilterBehavior HitTestFilter(DependencyObject o)
        {
            if (o is Trigger)
            {
                return HitTestFilterBehavior.ContinueSkipChildren;
            }
            return HitTestFilterBehavior.Continue;
        }

        protected HitTestResultBehavior HitTestResult(HitTestResult result)
        {
            _testResult = result.VisualHit as Trigger;
            return HitTestResultBehavior.Stop;
        }
    }
    public class ItemsDragBehavior : ItemsDragBehaviorBase<ItemsControl, DicomImageShell>
    {
        private MouseEventHandler _previewMouseMove;
        private DragEventHandler _drop;
        private DragEventHandler _dragOver;
        public Action<FrameworkElement, FrameworkElement> ItemsMoved { get; set; }
        public ItemsDragBehavior()
        {
            _previewMouseMove = OnMouseMove;
            _drop = OnDrop;
            _dragOver = OnDragOver;
            TriggerButton = TriggerButton.Left;
        }

        protected override bool OnSubBehaviorAttached()
        {
            if (base.OnSubBehaviorAttached())
            {
                AssociatedObject.PreviewMouseMove += _previewMouseMove;
                AssociatedObject.Drop += _drop;
                return true;
            }
            return false;
        }

        protected override void OnSubBehaviorDetaching()
        {
            base.OnSubBehaviorDetaching();
            AssociatedObject.PreviewMouseMove -= _previewMouseMove;
            AssociatedObject.Drop -= _drop;
        }

        private void OnDragOver(object sender, DragEventArgs e)
        {
            var point = e.GetPosition(_dragAdorner.AdornedElement);
            _dragAdorner.UpdateLocation(point.X, point.Y);
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            //textbox控件拖动有特殊处理，不屏蔽会报错(或者自己做别的处理)
            if (e.OriginalSource is TextBoxBase) return;
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var pos = e.GetPosition(AssociatedObject);
                _testResult = null;
                VisualTreeHelper.HitTest(AssociatedObject, HitTestFilter, HitTestResult, new PointHitTestParameters(pos));
                if (_testResult == null)
                {
                    return;
                }
                _dragAdorner = new DragAdorner(_testResult);
                _layer.Add(_dragAdorner);
                _holder.DragOver += _dragOver;
                _testResult.SetValue(UIElement.OpacityProperty, 0.5);
                DragDrop.DoDragDrop(_holder, _testResult, DragDropEffects.Move);
                //drop后再移除一次，防止拖动到控件外释放鼠标后_dragAdorner停留的问题
                _holder.DragOver -= _dragOver;
                _layer.Remove(_dragAdorner);
            }
        }

        private void OnDrop(object sender, DragEventArgs e)
        {
            _layer.Remove(_dragAdorner);//先移除，否则Hit到的是_dragAdorner
            //查找元数据
            var source = e.Data.GetData(typeof(DicomImageShell)) as FrameworkElement;
            if (source == null)
            {
                return;
            }
            _testResult.ClearValue(UIElement.OpacityProperty);
            var pos = e.GetPosition(AssociatedObject);
            _testResult = null;
            VisualTreeHelper.HitTest(AssociatedObject, HitTestFilter, HitTestResult, new PointHitTestParameters(pos));
            if (_testResult == null)
            {
                return;
            }
            if (ReferenceEquals(_testResult, source))
            {
                return;
            }
            e.Handled = true;
            ItemsMoved.Invoke(source, _testResult);
        }
    }
}
