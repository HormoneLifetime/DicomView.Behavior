using DicomView.Behaviors.DicomBehaviors;
using DicomView.Behaviors.DicomInterface;
using DicomView.Behaviors.ROI;
using FellowOakDicom.Imaging;
using Microsoft.Xaml.Behaviors;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace DicomView.Behaviors
{
    /// <summary>
    /// DicomImageShell.xaml 的交互逻辑
    /// </summary>
    public partial class DicomImageShell : UserControl, IDrawable
    {
        #region useless
        //public static readonly DependencyProperty WindowWidthProperty = DependencyProperty.Register("WindowWidth", typeof(double), typeof(DicomImageShell));
        //public static readonly DependencyProperty WindowCenterProperty = DependencyProperty.Register("WindowCenter", typeof(double), typeof(DicomImageShell));
        //public static readonly DependencyProperty SpacingXProperty = DependencyProperty.Register("SpacingX", typeof(double), typeof(DicomImageShell));
        //public static readonly DependencyProperty SpacingYProperty = DependencyProperty.Register("SpacingY", typeof(double), typeof(DicomImageShell));
        //public static readonly DependencyProperty RowsProperty = DependencyProperty.Register("Rows", typeof(ushort), typeof(DicomImageShell));
        //public static readonly DependencyProperty ColsProperty = DependencyProperty.Register("Cols", typeof(ushort), typeof(DicomImageShell));
        public static readonly DependencyProperty ROISourceProperty = DependencyProperty.Register("ROISource", typeof(IEnumerable), typeof(DicomImageShell), new FrameworkPropertyMetadata(null, OnROISourceChanged));
        public IEnumerable ROISource
        {
            get
            {
                return (IEnumerable)GetValue(ROISourceProperty);
            }
            set
            {
                if (value == null)
                {
                    ClearValue(ROISourceProperty);
                }
                else
                {
                    SetValue(ROISourceProperty, value);
                }
            }
        }
        #region 集合变化处理

        private static void OnROISourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DicomImageShell canvas)
            {
                canvas.OnROISourceChanged(e.OldValue, e.NewValue);
            }
        }

        private void OnROISourceChanged(object oldValue, object newValue)
        {
            // 移除旧集合的监听
            if (oldValue is INotifyCollectionChanged oldCollection)
            {
                oldCollection.CollectionChanged -= OnCollectionChanged;
            }

            // 清除现有 Shapes
            ClearShapes();

            // 添加新集合的监听
            if (newValue is INotifyCollectionChanged newCollection)
            {
                newCollection.CollectionChanged += OnCollectionChanged;
            }

            // 添加现有项
            if (newValue != null)
            {
                foreach (var item in newValue as IEnumerable)
                {
                    if (item is Shape shape)
                    {
                        AddShape(shape);
                    }
                }
            }
        }

        private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewItems != null)
                    {
                        foreach (var item in e.NewItems)
                        {
                            if (item is Shape shape)
                            {
                                canvas.Children.Add(shape);
                            }
                        }
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    if (e.OldItems != null)
                    {
                        foreach (var item in e.OldItems)
                        {
                            if (item is Shape shape)
                            {
                                RemoveShape(shape);
                            }
                        }
                    }
                    break;

                case NotifyCollectionChangedAction.Replace:
                    if (e.OldItems != null)
                    {
                        foreach (var item in e.OldItems)
                        {
                            if (item is Shape shape)
                            {
                                RemoveShape(shape);
                            }
                        }
                    }
                    if (e.NewItems != null)
                    {
                        foreach (var item in e.NewItems)
                        {
                            if (item is Shape shape)
                            {
                                AddShape(shape);
                            }
                        }
                    }
                    break;

                case NotifyCollectionChangedAction.Reset:
                    ClearShapes();
                    if (ROISource != null)
                    {
                        foreach (var item in ROISource)
                        {
                            if (item is Shape shape)
                            {
                                AddShape(shape);
                            }
                        }
                    }
                    break;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddShape(Shape shape)
        {
            canvas.Children.Add(shape);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RemoveShape(Shape shape)
        {
            canvas.Children.Remove(shape);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ClearShapes()
        {
            canvas.Children.Clear();
        }
        #endregion

        //public double WindowWidth
        //{
        //    get { return (double)GetValue(WindowWidthProperty); }
        //    set
        //    {
        //        SetValue(WindowWidthProperty, value);
        //    }
        //}
        //public double WindowCenter
        //{
        //    get { return (double)GetValue(WindowCenterProperty); }
        //    set
        //    {
        //        SetValue(WindowCenterProperty, value);
        //    }
        //}
        //public double SpacingX
        //{
        //    get { return (double)GetValue(SpacingXProperty); }
        //    set
        //    {
        //        SetValue(SpacingXProperty, value);
        //    }
        //}
        //public double SpacingY
        //{
        //    get { return (double)GetValue(SpacingYProperty); }
        //    set
        //    {
        //        SetValue(SpacingYProperty, value);
        //    }
        //}
        //public ushort Rows
        //{
        //    get { return (ushort)GetValue(RowsProperty); }
        //    set
        //    {
        //        SetValue(RowsProperty, value);
        //    }
        //}
        //public ushort Cols
        //{
        //    get { return (ushort)GetValue(ColsProperty); }
        //    set
        //    {
        //        SetValue(ColsProperty, value);
        //    }
        //}
        #endregion
        public DicomImageShell()
        {
            InitializeComponent();
        }

        private void shell_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null && e.NewValue is not IDicomImage)
                throw new InvalidOperationException("DataContext must be of type IDicomImage");
        }

        public void AdjustWindow(Vector vector)
        {
            (DataContext as IDicomImage).AdjustWindow(vector);
        }

        public Canvas GetCanvas()
        {
            return canvas;
        }

        protected override HitTestResult HitTestCore(PointHitTestParameters hitTestParameters)
        {
            return new PointHitTestResult(this, hitTestParameters.HitPoint);
        }
    }
}
