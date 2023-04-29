using System;
using System.Windows;
using System.Windows.Media;

namespace MyContract
{
    public interface IShape : ICloneable
    {
        string Name { get; }
        string Icon { get; }
        Color? color { get; set; }
        int? thickness { get; set; }
        

           
        void UpdateStart(System.Windows.Point p);
        void UpdateEnd(System.Windows.Point p);
        UIElement Draw(System.Windows.Media.Color color, int thickness);
    }
}
