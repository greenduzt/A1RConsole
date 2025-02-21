using A1RConsole.Models.Production.Grading;
using A1RConsole.Models.Products;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A1RConsole.Models.Production
{
    public class SlitPeel
    {
        public int ID { get; set; }
        public int ProdTimeTableID { get; set; }
        public int SalesOrderID { get; set; }
        public int ProductID { get; set; }
        public int RawProductID { get; set; }
        public int OrdertypeID { get; set; }
        public decimal QtyToMake { get; set; }
        public decimal QtyMade { get; set; }
        public decimal DollarValue { get; set; }
        public decimal Thickness { get; set; }
        public decimal ProductPrice { get; set; }
        public string ProductUnit { get; set; }
        public decimal MaxItemsPer { get; set; }
        public decimal OriginalQty { get; set; }
        public decimal OriginalBlockLogs { get; set; }
        public int Shift { get; set; }
        public string ProductionDate { get; set; }
        public string Status { get; set; }
        public List<GradingProduction> GradingProduction { get; set; }
        public Product Product { get; set; }
        public string Type { get; set; }
        //public string SlitPeelType { get; set; }
    }
}
