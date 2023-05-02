using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CKDP_Paint.MyHistory
{
    public interface  History
    {
        public string Name { get; }
        public UIElement Object { get; set; }
    }
}
