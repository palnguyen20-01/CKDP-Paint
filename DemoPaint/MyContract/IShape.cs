using System;
using System.Windows;
using System.Windows.Media;

namespace MyContract
{
    public interface IShape : ICloneable
    {
        string Name { get; }
        string Icon { get; }
        public SolidColorBrush stroke { get; set; }
        public DoubleCollection strokeDashArray { get; set; }
        public PenLineCap strokeDashCap { get; set; }
        public double thickness { get; set; }
        void UpdateStart(System.Windows.Point p);
        void UpdateEnd(System.Windows.Point p);
        UIElement Draw();
    }
}
