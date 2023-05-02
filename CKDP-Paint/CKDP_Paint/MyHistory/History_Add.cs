using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CKDP_Paint.MyHistory
{
    public class History_Add:History
    {
        public string Name => "Add";
        public UIElement Object { get; set; }

        public History_Add(UIElement _object)
        {
            Object = _object;
        }
    }
}
