using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A1RConsole.Models.Products
{
    public class RawProduct
    {

        public int RawProductID { get; set; }
        public int SalesID { get; set; }
        public string RawProductCode { get; set; }
        public string RawProductName { get; set; }
        public string Description { get; set; }
        public string RawProductType { get; set; }
        public decimal Cost { get; set; }

    }
}
