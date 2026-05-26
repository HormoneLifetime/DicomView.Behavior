using DicomView.Behaviors.DicomInterface;
using DicomView.Behaviors.Fo_DicomOverride;
using FellowOakDicom;
using FellowOakDicom.Imaging;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;

namespace DicomView.Behaviors.FellowOakDicomImage
{
    public class Fo_DicomCTImage : Fo_DicomImageBase, IROIFeature
    {
        public Fo_DicomCTImage(DicomFile file) : base(file) { }
        public Fo_DicomCTImage(DicomDataset set) : base(set) { }

        protected DicomImageOverride _imageCore;
        protected override void Init(DicomDataset set)
        {
            base.Init(set);
            _imageCore = new DicomImageOverride(set);
            _imageCore.CacheMode |= CacheType.Display;
        }

        protected override void RenderImage()
        {
            DicomImage = _imageCore.RenderImage(_imageCore.CurrentFrame).AsWriteableBitmap();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void AdjustWindow(Vector vector)
        {
            vector *= 0.5;
            var windowWdith = WindowWidth + vector.X;
            var windowCenter = WindowCenter - vector.Y;
            if (windowWdith <  1)
                windowWdith = 1;
            _imageCore.WindowWidth = windowWdith;
            _imageCore.WindowCenter = windowCenter;
            WindowWidth = windowWdith;
            WindowCenter = windowCenter;
            _imageCore.RenderImageOverride();
        }
    }
}
