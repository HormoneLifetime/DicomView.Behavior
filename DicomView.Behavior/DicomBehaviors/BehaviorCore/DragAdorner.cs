// Licensed to the MinFound under one or more agreements.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace DicomView.Behaviors.DicomBehaviors.BehaviorCore
{
    public class DragAdorner : Adorner
    {
        private VisualBrush visualBrush = new VisualBrush();
        private TranslateTransform _translateTransform = new TranslateTransform();
        public DragAdorner(UIElement adornedElement, double opacity = 1) : base(adornedElement)
        {
            
            this.IsHitTestVisible = false;
            visualBrush.Visual = adornedElement;
            visualBrush.Opacity = opacity;
        }
        protected override void OnRender(DrawingContext drawingContext)
        {
            drawingContext.DrawRectangle(visualBrush, null, new Rect(AdornedElement.TranslatePoint(new Point(0, 0), Parent as UIElement), AdornedElement.RenderSize));
            base.OnRender(drawingContext);
        }

        public override GeneralTransform GetDesiredTransform(GeneralTransform transform)
        {
            return _translateTransform;
        }

        public void UpdateLocation(double offsetX, double offsetY)
        {
            _translateTransform.X = offsetX;
            _translateTransform.Y = offsetY;
        }
    }
}
