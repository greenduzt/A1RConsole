using A1RConsole.Bases;
using A1RConsole.Models.Suppliers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A1RConsole.Models.Products
{
    public class SupplierProduct : ViewModelBase
    {
        public int ID { get; set; }
        public int ProductID { get; set; }
        public string TimeStamp { get; set; }
       
        private Supplier _supplier;
        private int _leadTime;

        public Supplier Supplier
        {
            get
            {
                return _supplier;
            }
            set
            {
                _supplier = value;
                RaisePropertyChanged("Supplier");

            }
        }

        public int LeadTime
        {
            get
            {
                return _leadTime;
            }
            set
            {
                _leadTime = value;
                RaisePropertyChanged("LeadTime");

            }
        }
    }
}
