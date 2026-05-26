using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;

namespace DicomView.Behaviors.DicomInterface
{
    internal interface IDicomImage
    {
        public double WindowWidth { get; set; }
        public double WindowCenter { get; set; }
        public double SpacingX { get; }
        public double SpacingY { get; }
        public ushort Cols { get; }
        public ushort Rows { get; }

        public BitmapSource DicomImage { get; }
        public void AdjustWindow(Vector vector);
    }
}
