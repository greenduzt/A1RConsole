using A1RConsole.Bases;
using A1RConsole.Models.Products;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A1RConsole.Models.Stock
{
    public class ProductStockMaintenance : ViewModelBase
    {
        public StockLocation StockLocation { get; set; }
        //public Product Product { get; set; }
        //public ProductStock ProductStock { get; set; }
        public string ProjAvaCellBack { get; set; }
        public string ProjAvaCellFore { get; set; }

        private Product _product;
        private ProductStock _ProductStock;
        private string _viewEditButtonVisibility;
        private bool _qtyAvailbleEnabled;

        public Product Product
        {
            get { return _product; }
            set
            {
                _product = value;
                RaisePropertyChanged("Product");
            }
        }

        public ProductStock ProductStock
        {
            get { return _ProductStock; }
            set
            {
                _ProductStock = value;
                RaisePropertyChanged("ProductStock");
            }
        }

        public string ViewEditButtonVisibility
        {
            get { return _viewEditButtonVisibility; }
            set
            {
                _viewEditButtonVisibility = value;
                RaisePropertyChanged("ViewEditButtonVisibility");
            }
        }

        public bool QtyAvailbleEnabled
        {
            get { return _qtyAvailbleEnabled; }
            set
            {
                _qtyAvailbleEnabled = value;
                RaisePropertyChanged("QtyAvailbleEnabled");
            }
        }
    }
}
