using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows;
using System.Text;
using System.Windows.Media.Imaging;
using System.Windows.Controls;

namespace DicomView.Behaviors.DicomInterface
{
    internal interface IDrawable
    {
        public Canvas GetCanvas();
    }
}
