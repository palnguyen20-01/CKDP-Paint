using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CKDP_Paint.MyHistory
{
    public class History_Move : History
    {
        public string Name => "Move";
        public Point Position { get; set; }
        public UIElement Object { get; set; }

        public History_Move(UIElement _object, Point _position)
        {
            Position = _position;
            Object = _object;
        }
    }
}
