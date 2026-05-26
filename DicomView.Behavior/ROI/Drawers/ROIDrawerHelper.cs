using DicomView.Behaviors.DicomInterface;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace DicomView.Behaviors.ROI.Drawers
{
    public static class ROIDrawerHelper
    {
        public static void SetAttach(this Shape shape, IROIFeature dicomImage)
        {
            //shape.Style = Style; 自定义实现，搜索Style资源，主题切换等
            Canvas.SetLeft(shape, 0);
            Canvas.SetTop(shape, 0);
            AttachContextMenu(shape, dicomImage);
        }

        private static void AttachContextMenu(this Shape shape, IROIFeature dicomImage)
        {
            ContextMenu menu = new ContextMenu();
            MenuItem deleteItem = new MenuItem() { Header = "Delete" };
            deleteItem.Click += (s, e) =>
            {
                dicomImage.RemoveROI(shape);
            };
            menu.Items.Add(deleteItem);
            shape.ContextMenu = menu;
        }
    }
}
