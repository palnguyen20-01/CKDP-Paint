using System.Drawing;
using System.Windows;

namespace IContract
{
    public interface IShape : ICloneable
    {
        string Name { get; }
        void UpdateStart(Point p);
        void UpdateEnd(Point p);
        UIElement Draw(Color color, int thickness);
    }

}