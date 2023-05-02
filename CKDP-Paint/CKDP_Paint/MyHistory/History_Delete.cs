using MyContract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CKDP_Paint.MyHistory
{
    public class History_Delete : History
    {
        public string Name => "Delete";
        public UIElement Object { get; set; }
        public IShape ObjectShape { get; set; }

        public History_Delete(UIElement _object, IShape _objectShape)
        {
            Object = _object;
            ObjectShape = _objectShape;
        }
    }
}
