using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using MyContract;
using System.Windows.Shapes;

namespace RectangleAbility
{
    public class MyRectangle : IShape
    {
        public Point Start { get; set; }
        public Point End { get; set; }

        public string Name => "Rectangle";
        public string Icon => "Images/rectangle.png";
        public Color? color { get; set; } = null;
        public int? thickness { get; set; } = null;

        public void UpdateStart(Point p)
        {
            Start = p;
        }
        public void UpdateEnd(Point p)
        {
            End = p;
        }

        public UIElement Draw(Color color, int thickness)
        {
            if (this.color == null)
            {
                this.color = color;
            }


            double width = Math.Abs(End.X - Start.X);
            double height = Math.Abs(End.Y - Start.Y);

            var shape = new Rectangle()
            {
                Width = width,
                Height = height,
                Stroke = new SolidColorBrush((Color)this.color),
                StrokeThickness = thickness
            };

            Canvas.SetLeft(shape, Start.X);
            Canvas.SetTop(shape, Start.Y);
            return shape;
        }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
