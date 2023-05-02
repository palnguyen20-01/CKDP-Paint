using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using MyContract;
using System.Windows.Shapes;

namespace MyRectangle
{
    public class MySquare : IShape
    {
        public Point Start { get; set; }
        public Point End { get; set; }
        public string Name => "Square";
        public string Icon => "Images/square.png";
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
            double width = Math.Min(Math.Abs(End.X - Start.X), Math.Abs(End.Y - Start.Y));

            int x_sign;
            int y_sign;
            if (End.X > Start.X)
            {
                x_sign = 1;
            }
            else
            {
                x_sign = -1;
            }

            if (End.Y > Start.Y)
            {
                y_sign = 1;
            }
            else
            {
                y_sign = -1;
            }

            var shape = new Rectangle()
            {
                Width = width,
                Height = width,
                Stroke = stroke,
                StrokeDashArray = strokeDashArray,
                StrokeDashCap = strokeDashCap,
                StrokeThickness = thickness
            };

            Canvas.SetLeft(shape, Math.Min(Start.X, Start.X + x_sign * width));
            Canvas.SetTop(shape, Math.Min(Start.Y, Start.Y + y_sign * width));
            return shape;
        }

        public object Clone()
        {
            return new MySquare()
            {
                Start = new Point(Start.X, Start.Y),
                End = new Point(End.X, End.Y),
                stroke = stroke.Clone(),
                strokeDashArray = strokeDashArray,
                strokeDashCap = strokeDashCap,
                thickness = thickness,
            };
        }
    }
}
