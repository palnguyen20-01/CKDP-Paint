using System;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows;
using MyContract;

namespace MyLine
{
    public class MyLine : IShape
    {
        public Point Start { get; set; }
        public Point End { get; set; }
        public string Name => "Line";
        public string Icon => "Images/line.png";
        public SolidColorBrush stroke { get; set; } = new SolidColorBrush(Colors.Black);
        public DoubleCollection strokeDashArray { get; set; } = new DoubleCollection(2);
        public PenLineCap strokeDashCap { get; set; } = PenLineCap.Flat;
        public double thickness { get; set; } = 2;

        public void UpdateStart(Point p)
        {
            Start = p;
        }
        public void UpdateEnd(Point p)
        {
            End = p;
        }

        public UIElement Draw()
        {
            return new Line()
            {
                X1 = Start.X,
                Y1 = Start.Y,
                X2 = End.X,
                Y2 = End.Y,
                Stroke = stroke,
                StrokeDashArray = strokeDashArray,
                StrokeDashCap = strokeDashCap,
                StrokeThickness = thickness
            };
        }

        public object Clone()
        {
            return new MyLine()
            {
                stroke = new SolidColorBrush(Colors.Black),
                strokeDashArray = new DoubleCollection(2),
                strokeDashCap = PenLineCap.Flat,
                thickness = 2,
            };
        }

    }
}
