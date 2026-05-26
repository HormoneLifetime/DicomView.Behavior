using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace DicomView.Behaviors.ROI.Shapes
{
    internal class LengthLine : DicomROIBaseShape
    {
        static LengthLine()
        {
            // OverrideMetadata must be called once per type — do it in static ctor
            Shape.StrokeProperty.OverrideMetadata(typeof(LengthLine), new FrameworkPropertyMetadata(Brushes.Green));
            Shape.StrokeThicknessProperty.OverrideMetadata(typeof(LengthLine), new FrameworkPropertyMetadata(5.0));
        }

        public LengthLine()
        {
            GetTextCenterPoint = CalculateCenter;
        }
        #region 依赖属性

        // 起点
        public static readonly DependencyProperty StartPointProperty =
            DependencyProperty.Register("StartPoint", typeof(Point), typeof(LengthLine),
                new FrameworkPropertyMetadata(new Point(0, 0),
                    FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

        public Point StartPoint
        {
            get => (Point)GetValue(StartPointProperty);
            set => SetValue(StartPointProperty, value);
        }

        // 终点
        public static readonly DependencyProperty EndPointProperty =
            DependencyProperty.Register("EndPoint", typeof(Point), typeof(LengthLine),
                new FrameworkPropertyMetadata(new Point(100, 0),
                    FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

        public Point EndPoint
        {
            get => (Point)GetValue(EndPointProperty);
            set => SetValue(EndPointProperty, value);
        }

        // 文字位置
        public static readonly DependencyProperty TextPositionProperty =
            DependencyProperty.Register("TextPosition", typeof(TextPosition), typeof(LengthLine),
                new FrameworkPropertyMetadata(TextPosition.Center,
                    FrameworkPropertyMetadataOptions.AffectsRender, OnTextPositionChanged));

        private static void OnTextPositionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var line = d as LengthLine;
            line.OnTextPositionChanged((TextPosition)e.NewValue);
        }

        public TextPosition TextPosition
        {
            get => (TextPosition)GetValue(TextPositionProperty);
            set => SetValue(TextPositionProperty, value);
        }

        // 文字偏移距离
        public static readonly DependencyProperty TextOffsetProperty =
            DependencyProperty.Register("TextOffset", typeof(double), typeof(LengthLine),
                new FrameworkPropertyMetadata(10.0,
                    FrameworkPropertyMetadataOptions.AffectsRender));

        public double TextOffset
        {
            get => (double)GetValue(TextOffsetProperty);
            set => SetValue(TextOffsetProperty, value);
        }

        // 文字
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(LengthLine),
                new FrameworkPropertyMetadata(string.Empty,
                    FrameworkPropertyMetadataOptions.AffectsRender));

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        // 自动计算长度作为文字
        public static readonly DependencyProperty AutoLengthTextProperty =
            DependencyProperty.Register("AutoLengthText", typeof(bool), typeof(LengthLine),
                new FrameworkPropertyMetadata(true,
                    FrameworkPropertyMetadataOptions.AffectsRender));

        public bool AutoLengthText
        {
            get => (bool)GetValue(AutoLengthTextProperty);
            set => SetValue(AutoLengthTextProperty, value);
        }

        // 长度格式
        public static readonly DependencyProperty LengthFormatProperty =
            DependencyProperty.Register("LengthFormat", typeof(string), typeof(LengthLine),
                new FrameworkPropertyMetadata("F2",
                    FrameworkPropertyMetadataOptions.AffectsRender));

        public string LengthFormat
        {
            get => (string)GetValue(LengthFormatProperty);
            set => SetValue(LengthFormatProperty, value);
        }

        // 文字单位
        public static readonly DependencyProperty LengthUnitProperty =
            DependencyProperty.Register("LengthUnit", typeof(string), typeof(LengthLine),
                new FrameworkPropertyMetadata("",
                    FrameworkPropertyMetadataOptions.AffectsRender));

        public string LengthUnit
        {
            get => (string)GetValue(LengthUnitProperty);
            set => SetValue(LengthUnitProperty, value);
        }

        // 文字前景色
        public static readonly DependencyProperty TextForegroundProperty =
            DependencyProperty.Register("TextForeground", typeof(Brush), typeof(LengthLine),
                new FrameworkPropertyMetadata(Brushes.Black,
                    FrameworkPropertyMetadataOptions.AffectsRender));

        public Brush TextForeground
        {
            get => (Brush)GetValue(TextForegroundProperty);
            set => SetValue(TextForegroundProperty, value);
        }

        // 文字背景色
        public static readonly DependencyProperty TextBackgroundProperty =
            DependencyProperty.Register("TextBackground", typeof(Brush), typeof(LengthLine),
                new FrameworkPropertyMetadata(Brushes.White,
                    FrameworkPropertyMetadataOptions.AffectsRender));

        public Brush TextBackground
        {
            get => (Brush)GetValue(TextBackgroundProperty);
            set => SetValue(TextBackgroundProperty, value);
        }

        // 文字大小
        public static readonly DependencyProperty TextFontSizeProperty =
            DependencyProperty.Register("TextFontSize", typeof(double), typeof(LengthLine),
                new FrameworkPropertyMetadata(12.0,
                    FrameworkPropertyMetadataOptions.AffectsRender));

        public double TextFontSize
        {
            get => (double)GetValue(TextFontSizeProperty);
            set => SetValue(TextFontSizeProperty, value);
        }

        // 文字字体
        public static readonly DependencyProperty TextFontFamilyProperty =
            DependencyProperty.Register("TextFontFamily", typeof(FontFamily), typeof(LengthLine),
                new FrameworkPropertyMetadata(new FontFamily("Arial"),
                    FrameworkPropertyMetadataOptions.AffectsRender));

        public FontFamily TextFontFamily
        {
            get => (FontFamily)GetValue(TextFontFamilyProperty);
            set => SetValue(TextFontFamilyProperty, value);
        }

        // 文字边框
        public static readonly DependencyProperty TextBorderBrushProperty =
            DependencyProperty.Register("TextBorderBrush", typeof(Brush), typeof(LengthLine),
                new FrameworkPropertyMetadata(Brushes.Gray,
                    FrameworkPropertyMetadataOptions.AffectsRender));

        public Brush TextBorderBrush
        {
            get => (Brush)GetValue(TextBorderBrushProperty);
            set => SetValue(TextBorderBrushProperty, value);
        }

        // 文字边框粗细
        public static readonly DependencyProperty TextBorderThicknessProperty =
            DependencyProperty.Register("TextBorderThickness", typeof(double), typeof(LengthLine),
                new FrameworkPropertyMetadata(1.0,
                    FrameworkPropertyMetadataOptions.AffectsRender));

        public double TextBorderThickness
        {
            get => (double)GetValue(TextBorderThicknessProperty);
            set => SetValue(TextBorderThicknessProperty, value);
        }

        #endregion

        #region 重写方法

        protected override Geometry DefiningGeometry
        {
            get
            {
                // 创建一个包含直线路径的几何体
                StreamGeometry geometry = new StreamGeometry();
                using (StreamGeometryContext ctx = geometry.Open())
                {
                    ctx.BeginFigure(StartPoint, false, false);
                    ctx.LineTo(EndPoint, true, false);
                }
                return geometry;
            }
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            // 绘制文字
            if (!string.IsNullOrEmpty(Text) || AutoLengthText)
            {
                DrawText(drawingContext);
            }
        }

        protected override Size MeasureOverride(Size constraint)
        {
            // 计算控件所需的大小
            double minX = Math.Min(StartPoint.X, EndPoint.X);
            double maxX = Math.Max(StartPoint.X, EndPoint.X);
            double minY = Math.Min(StartPoint.Y, EndPoint.Y);
            double maxY = Math.Max(StartPoint.Y, EndPoint.Y);

            // 考虑文字的边界
            //var text = GetDisplayText();
            //if (!string.IsNullOrEmpty(text))
            //{
            //    var formattedText = CreateFormattedText(text);
            //    double textWidth = formattedText.Width;
            //    double textHeight = formattedText.Height;

            //    // 根据文字位置调整边界
            //    var textCenter = GetTextCenterPoint();
            //    minX = Math.Min(minX, textCenter.X - textWidth / 2);
            //    maxX = Math.Max(maxX, textCenter.X + textWidth / 2);
            //    minY = Math.Min(minY, textCenter.Y - textHeight / 2);
            //    maxY = Math.Max(maxY, textCenter.Y + textHeight / 2);
            //}

            return new Size(maxX - minX, maxY - minY);
        }

        #endregion

        #region 私有方法

        private void DrawText(DrawingContext drawingContext)
        {
            var text = GetDisplayText();
            if (string.IsNullOrEmpty(text))
                return;

            var formattedText = CreateFormattedText(text);
            var textCenter = GetTextCenterPoint();

            // 计算文字绘制位置（文字左上角）
            Point textPosition = new Point(textCenter.X - formattedText.Width / 2, textCenter.Y - formattedText.Height / 2);

            // 绘制文字背景
            Rect textRect = new Rect(textPosition,
                new Size(formattedText.Width, formattedText.Height));

            // 添加内边距
            double padding = 4;
            textRect.Inflate(padding, padding);

            // 绘制圆角矩形背景
            drawingContext.DrawRoundedRectangle(
                TextBackground,
                new Pen(TextBorderBrush, TextBorderThickness),
                textRect, 3, 3);

            // 绘制文字
            drawingContext.DrawText(formattedText, textPosition);
        }

        private FormattedText CreateFormattedText(string text)
        {
            return new FormattedText(
                text,
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(TextFontFamily, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal),
                TextFontSize,
                TextForeground,
                VisualTreeHelper.GetDpi(this).PixelsPerDip);
        }

        private string GetDisplayText()
        {
            if (!string.IsNullOrEmpty(Text) && !AutoLengthText)
                return Text;

            double length = CalculateLength();
            string lengthText = length.ToString(LengthFormat);

            if (!string.IsNullOrEmpty(LengthUnit))
                return $"{lengthText} {LengthUnit}";

            return lengthText;
        }

        private double CalculateLength()
        {
            double dx = (EndPoint.X - StartPoint.X) * SpacingX;
            double dy = (EndPoint.Y - StartPoint.Y) * SpacingY;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        private void OnTextPositionChanged(TextPosition position)
        {
            GetTextCenterPoint = position switch
            {
                TextPosition.Center => CalculateCenter,
                TextPosition.Left => CalculateLeft,
                TextPosition.Right => CalculateRight,
                TextPosition.Start => CalculateStart,
                TextPosition.End => CalculateEnd,
                _ => CalculateCenter
            };
        }

        private Func<Point> GetTextCenterPoint;

        private Point CalculateCenter()
        {
            return new Point((StartPoint.X + EndPoint.X) / 2, (StartPoint.Y + EndPoint.Y) / 2);
        }

        private Point CalculateLeft()
        {
            Vector vector = StartPoint - EndPoint;//方向向量
            Vector verticalVector = new Vector(-vector.Y, vector.X);//逆时针90°的垂直向量
            verticalVector.Normalize();
            var center = CalculateCenter();
            return center + verticalVector * TextOffset;
        }

        private Point CalculateRight()
        {
            Vector vector = StartPoint - EndPoint;//方向向量
            Vector verticalVector = new Vector(vector.Y, -vector.X);//顺时针90°的垂直向量
            verticalVector.Normalize();
            var center = CalculateCenter();
            return center + verticalVector * TextOffset;
        }

        private Point CalculateStart()
        {
            Vector vector = (StartPoint - EndPoint);//方向向量
            vector.Normalize();
            return StartPoint + vector * TextOffset;
        }

        private Point CalculateEnd()
        {
            Vector vector = (EndPoint - StartPoint);//方向向量
            vector.Normalize();
            return EndPoint + vector * TextOffset;
        }
        #endregion

        public override object Clone()
        {
            return new LengthLine
            {
                StartPoint = this.StartPoint,
                EndPoint = this.EndPoint,
                TextPosition = this.TextPosition,
                TextOffset = this.TextOffset,
                Text = this.Text,
                AutoLengthText = this.AutoLengthText,
                LengthFormat = this.LengthFormat,
                LengthUnit = this.LengthUnit,
                TextForeground = this.TextForeground.Clone(),
                TextBackground = this.TextBackground.Clone(),
                TextFontSize = this.TextFontSize,
                TextFontFamily = this.TextFontFamily,
                TextBorderBrush = this.TextBorderBrush.Clone(),
                TextBorderThickness = this.TextBorderThickness,
                SpacingX = this.SpacingX,
                SpacingY = this.SpacingY,
                IsRendered = this.IsRendered
            };
        }
    }

    #region 枚举

    public enum TextPosition
    {
        Center,    // 在直线上
        Left,      // 左侧
        Right,     // 右侧
        Start,     // 起点
        End      // 终点
    }

    #endregion
}