using DicomView.Behaviors.DicomBehaviors.Controller;
using DicomView.Behaviors.DicomInterface;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace DicomView.Behaviors.DicomBehaviors.BehaviorCore
{
    public abstract class ItemsDragBehaviorCoreBase<Associate, Attach> : BehaviorCore<Attach>, IAssociatedBehaviorCore<Associate, Attach> where Attach : FrameworkElement where Associate : FrameworkElement
    {
        protected DragAdorner _dragAdorner;
        protected AdornerLayer _layer;
        protected UIElement _holder;
        protected Attach _testResult;

        private Associate _associatedObject;
        public Associate AssociatedObject
        {
            get { return _associatedObject; }
            set
            {
                OnAssociatedObjectChanged(_associatedObject, value);
                _associatedObject = value;
            }
        }

        public static readonly DependencyProperty AllowOverBoundsProperty = DependencyProperty.Register("AllowOverBounds", typeof(bool), typeof(ItemsDragBehaviorCoreBase<Associate, Attach>), new FrameworkPropertyMetadata(false, AllowOverBoundsPropertyChangedCallback));

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
            var core = obj as ItemsDragBehaviorCoreBase<Associate, Attach>;
            if (core != null && core.AssociatedObject != null)
            {
                core.InitAdornerLayer((bool)args.NewValue);
            }
        }

        public void OnAssociatedObjectChanged(Associate oldValue, Associate newValue)
        {
            if (newValue != null)
                newValue.Loaded += OnLoaded;
            if (oldValue != null)
                oldValue.Loaded -= OnLoaded;
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
            if (o is Attach)
            {
                return HitTestFilterBehavior.ContinueSkipChildren;
            }
            return HitTestFilterBehavior.Continue;
        }

        protected HitTestResultBehavior HitTestResult(HitTestResult result)
        {
            _testResult = result.VisualHit as Attach;
            return HitTestResultBehavior.Stop;
        }
    }

    public class ItemsDragBehaviorCore : ItemsDragBehaviorCoreBase<ItemsControl, DicomImageShell>, IToMenuItem
    {
        private MouseEventHandler _previewMouseMove;
        private DragEventHandler _drop;
        private DragEventHandler _dragOver;
        public Action<FrameworkElement, FrameworkElement> ItemsMoved { get; set; }
        public ItemsDragBehaviorCore()
        {
            _previewMouseMove = OnMouseMove;
            _drop = OnDrop;
            _dragOver = OnDragOver;
            Tag = "Move";
            TriggerButton = TriggerButton.Left;
            OnActiveChanged += (e) => 
            {
                if (e.IsActive)
                    AssociatedObject.AllowDrop = true; 
            };
        }

        public override void SetAttachElement(DicomImageShell target)
        {
            _target = target;
        }

        protected override void SubAttach()
        {
            _target.PreviewMouseMove += _previewMouseMove;
            AssociatedObject.Drop += _drop;
        }

        protected override void SubDetach()
        {
            _target.PreviewMouseMove -= _previewMouseMove;
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
                _dragAdorner = new DragAdorner(_target);
                _layer.Add(_dragAdorner);
                _holder.DragOver += _dragOver;
                _target.SetValue(UIElement.OpacityProperty, 0.5);
                DragDrop.DoDragDrop(_holder, _target, DragDropEffects.Move);
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
            _target.ClearValue(UIElement.OpacityProperty);
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
            //ItemMovedEventArgs args = new ItemMovedEventArgs(this, source, _testResult);
            //AssociatedObject.RaiseEvent(args);
        }

        public MenuItem ToMenuItem()
        {
            return this.ToDefaultMenuItem();
        }
    }
}
