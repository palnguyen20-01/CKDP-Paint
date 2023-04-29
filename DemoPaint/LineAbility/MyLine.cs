using System;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows;
using MyContract;

namespace LineAbility
{
    public class MyLine : IShape
    {
        public Point Start { get; set; }
        public Point End { get; set; }

        public string Name => "Line";
        public string Icon => "Images/line.png";
        public Color? color { get; set; } = null;
        public int? thickness { get; set; } =null;
       
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
          if(this.color == null)
            {
                this.color = color;
            }

            return new Line()
            {
                X1 = Start.X,
                Y1 = Start.Y,
                X2 = End.X,
                Y2 = End.Y,
                Stroke = new SolidColorBrush((Color)this.color),
                StrokeThickness = (int)thickness
            };
        }

        public object Clone()
        {
            return MemberwiseClone();
        }

    }
}
