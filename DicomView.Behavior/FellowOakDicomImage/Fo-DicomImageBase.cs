using FellowOakDicom;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;
using DicomView.Behaviors.DicomInterface;
using System.Collections.ObjectModel;
using System.Windows.Shapes;

namespace DicomView.Behaviors.FellowOakDicomImage
{
    public abstract class Fo_DicomImageBase : BindableBase, IDicomImage
    {
        protected double windowWidth;
        public double WindowWidth
        {
            get { return windowWidth; }
            set { SetProperty(ref windowWidth, value); }
        }
        protected double windowCenter;
        public double WindowCenter
        {
            get { return windowCenter; }
            set { SetProperty(ref windowCenter, value); }
        }
        protected double spacingX;
        public double SpacingX
        {
            get { return spacingX; }
            protected set { SetProperty(ref spacingX, value); }
        }
        protected double spacingY;
        public double SpacingY
        {
            get { return spacingY; }
            protected set { SetProperty(ref spacingY, value); }
        }
        protected ushort cols;
        public ushort Cols
        {
            get { return cols; }
            protected set { SetProperty(ref cols, value); }
        }
        protected ushort rows;
        public ushort Rows
        {
            get { return rows; }
            protected set { SetProperty(ref rows, value); }
        }
        protected BitmapSource dicomImage;
        public BitmapSource DicomImage
        {
            get { return dicomImage; }
            protected set { SetProperty(ref dicomImage, value); }
        }

        public ObservableCollection<Shape> rois;
        public ObservableCollection<Shape> ROIs
        {
            get { return rois; }
            protected set { SetProperty(ref rois, value); }
        }

        protected DicomDataset _core;
        public Fo_DicomImageBase(DicomFile file)
            : this(file.Dataset)
        {
        }

        public Fo_DicomImageBase(DicomDataset set)
        {
            Init(set);
            RenderImage();
        }

        protected virtual void Init(DicomDataset set)
        {
            _core = set;
            var spacing = _core.GetValues<double>(DicomTag.PixelSpacing);
            SpacingX = spacing[0];
            SpacingY = spacing[1];
            Cols = _core.GetSingleValue<ushort>(DicomTag.Columns);
            Rows = _core.GetSingleValue<ushort>(DicomTag.Rows);
            WindowWidth = _core.GetSingleValue<double>(DicomTag.WindowWidth);
            WindowCenter = _core.GetSingleValue<double>(DicomTag.WindowCenter);
            ROIs = new ObservableCollection<Shape>();
        }

        public void AddROI(Shape shape)
        {
            ROIs.Add(shape);
        }

        public void RemoveROI(Shape shape)
        {
            ROIs.Remove(shape);
        }

        public int ROICount()
        {
            return ROIs.Count;
        }

        protected abstract void RenderImage();
        public abstract void AdjustWindow(Vector vector);
    }
}
