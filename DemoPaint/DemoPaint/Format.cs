using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace DemoPaint
{
    class Format
    {
        public SolidColorBrush stroke { get; set; } = new SolidColorBrush(Colors.Black);
        public DoubleCollection strokeDashArray { get; set; } = new DoubleCollection(2);
        public PenLineCap strokeDashCap { get; set; } = PenLineCap.Flat;
        public double thickness { get; set; } = 2;
    }
}
