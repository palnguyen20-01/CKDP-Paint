using MyContract;
using System;
using System.Collections.Generic;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Shapes;

namespace DemoPaint
{
    class Prototype
    {
        public IShape shape { get; set; } = null;
        public Format format { get; set; } = new Format();
        public String type { get; set; } = "Line";

        public void applyFormat()
        {
            shape.stroke.Color = format.stroke.Color;
            shape.strokeDashArray = format.strokeDashArray;
            shape.strokeDashCap = format.strokeDashCap;
            shape.thickness = format.thickness;
        }
    }
}
