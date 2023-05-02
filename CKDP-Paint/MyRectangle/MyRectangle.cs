using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using MyContract;
using System.Windows.Shapes;

namespace MyRectangle
{
    public class MyRectangle : IShape
    {
        public Point Start { get; set; }
        public Point End { get; set; }
        public string Name => "Rectangle";
        public string Icon => "Images/rectangle.png";
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
            double width = Math.Abs(End.X - Start.X);
            double height = Math.Abs(End.Y - Start.Y);

            var shape = new Rectangle()
            {
                Width = width,
                Height = height,
                Stroke = stroke,
                StrokeDashArray = strokeDashArray,
                StrokeDashCap = strokeDashCap,
                StrokeThickness = thickness
            };

            Canvas.SetLeft(shape, Math.Min(Start.X, End.X));
            Canvas.SetTop(shape, Math.Min(Start.Y, End.Y));
            return shape;
        }

        public object Clone()
        {
            return new MyRectangle()
            {
                Start = new Point(Start.X, Start.Y),
                End = new Point(End.X, End.Y),
                stroke = stroke,
                strokeDashArray = strokeDashArray,
                strokeDashCap = strokeDashCap,
                thickness = thickness,
            };
        }
    }
}
