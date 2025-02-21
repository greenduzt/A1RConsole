using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A1RConsole.Core
{
    public class OrderOrigin<T1, T2>
    {
        public T1 OrderType { get; set; }
        public T2 Origin { get; set; }
    }
}
