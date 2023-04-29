using MyContract;
using System;
using System.Collections.Generic;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace DemoPaint
{
    class Prototype
    {
        public IShape shape { get; set; } = null;
        public String type { get; set; } = "Line";
    }
}
