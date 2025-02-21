using A1RConsole.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A1RConsole.Models.Invoices
{
    public class OrdersNotInvoiced : ViewModelBase
    {
        public string SalesID { get; set; }
        public string ShipperID { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime ShippedDate { get; set; }
        public string Name { get; set; }
        public string ShipperStatus { get; set; }
        public string ShipperType { get; set; }
        public string PartID { get; set; }
        public string Description { get; set; }
        public decimal OrderQty { get; set; }
        public string Unit { get; set; }
        public decimal UnitCost { get; set; }
        public decimal Total { get; set; }
        public string TotalItems { get; set; }

        public int OrderFormat { get; set; }
        public string State { get; set; }

        private string _totalByState;
        public string TotalByState
        {
            get
            {
                return _totalByState;
            }
            set
            {
                _totalByState = value;
                RaisePropertyChanged("TotalByState");
            }
        }
    }
}
