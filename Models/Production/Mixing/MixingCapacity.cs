using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A1RConsole.Models.Production.Mixing
{
    public class MixingCapacity
    {
        public int ID { get; set; }
        public int ProdTimeTableID { get; set; }
        public int MixingTimeTableID { get; set; }
        public int SalesId { get; set; }
        public int RawProductID { get; set; }
        public decimal BlockLogQty { get; set; }
        public int OrderType { get; set; }
        //public int Shift { get; set; }
        public decimal MaxMixes { get; set; }
        //public int Rank { get; set; }
        //public bool ActiveOrder { get; set; }
    }
}
