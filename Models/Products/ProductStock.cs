using A1RConsole.Bases;
using A1RConsole.Models.Stock;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A1RConsole.Models.Products
{
    public class ProductStock : ViewModelBase
    {
        private int _iD;
        private StockLocation _stockLocation;
        private SupplierProduct _supplierProduct;        
        private decimal _netDemand;
        private decimal _orderQty { get; set; }        
        private decimal _totalSupply;
        private decimal _qtyAvailable;
        private decimal _qtyOnHold;
        private Product _product;
        private bool _isExpanded;
        private string _purchaseButtonVisible;
        private string _productionButtonVisible;
        private string _timeStamp;
        private DateTime _lastUpdatedDate;
        private string _updatedBy;
        private string _editingBy;
        private bool _isEditing;

        public int ID
        {
            get
            {
                return _iD;
            }
            set
            {
                _iD = value;
                RaisePropertyChanged("ID");
            }
        }

        public StockLocation StockLocation
        {
            get
            {
                return _stockLocation;
            }
            set
            {
                _stockLocation = value;
                RaisePropertyChanged("StockLocation");
            }
        }

        public SupplierProduct SupplierProduct
        {
            get
            {
                return _supplierProduct;
            }
            set
            {
                _supplierProduct = value;
                RaisePropertyChanged("SupplierProduct");
            }
        }

        public bool IsEditing
        {
            get
            {
                return _isEditing;
            }
            set
            {
                _isEditing = value;
                RaisePropertyChanged("IsEditing");
            }
        }

        public string EditingBy
        {
            get
            {
                return _editingBy;
            }
            set
            {
                _editingBy = value;
                RaisePropertyChanged("EditingBy");
            }
        }

        public string UpdatedBy
        {
            get
            {
                return _updatedBy;
            }
            set
            {
                _updatedBy = value;
                RaisePropertyChanged("UpdatedBy");
            }
        }

        public DateTime LastUpdatedDate
        {
            get
            {
                return _lastUpdatedDate;
            }
            set
            {
                _lastUpdatedDate = value;
                RaisePropertyChanged("LastUpdatedDate");
            }
        }

        public string TimeStamp
        {
            get
            {
                return _timeStamp;
            }
            set
            {
                _timeStamp = value;
                RaisePropertyChanged("TimeStamp");
            }
        }

        public decimal NetDemand
        {
            get
            {
                return _netDemand;
            }
            set
            {
                _netDemand = value;
                RaisePropertyChanged("NetDemand");
            }
        }

        public decimal OrderQty
        {
            get
            {
                return _orderQty;
            }
            set
            {
                _orderQty = value;
                RaisePropertyChanged("OrderQty");
            }
        }

        public decimal TotalSupply
        {
            get
            {
                return _totalSupply;
            }
            set
            {
                _totalSupply = value;
                RaisePropertyChanged("TotalSupply");
            }
        }

        public decimal QtyOnHold
        {
            get
            {
                return _qtyOnHold;
            }
            set
            {
                _qtyOnHold = value;
                RaisePropertyChanged("QtyOnHold");
            }
        }

        public decimal QtyAvailable
        {
            get
            {
                return _qtyAvailable;
            }
            set
            {
                _qtyAvailable = value;
                RaisePropertyChanged("QtyAvailable");
            }
        }

        public Product Product
        {
            get
            {
                return _product;
            }
            set
            {
                _product = value;
                RaisePropertyChanged("Product");
            }
        }

        public string ProductionButtonVisible
        {
            get
            {
                return _productionButtonVisible;
            }
            set
            {
                _productionButtonVisible = value;
                RaisePropertyChanged("ProductionButtonVisible");
            }
        }

        public string PurchaseButtonVisible
        {
            get
            {
                return _purchaseButtonVisible;
            }
            set
            {
                _purchaseButtonVisible = value;
                RaisePropertyChanged("PurchaseButtonVisible");
            }
        }

        private decimal _projectedAvailable;
        public decimal ProjectedAvailable
        {
            get
            {
                return _projectedAvailable;
            }
            set
            {
                _projectedAvailable = value;
                RaisePropertyChanged("ProjectedAvailable");
                
            }
        }

        private string _projAvaBackground;
        public string ProjAvaBackground
        {
            get { return _projAvaBackground; }
            set
            {
                _projAvaBackground = value;

                RaisePropertyChanged("ProjAvaBackground");
            }
        }

        private string _qtyAvaForeground;
        public string QtyAvaForeground
        {
            get { return _qtyAvaForeground; }
            set
            {
                _qtyAvaForeground = value;

                RaisePropertyChanged("QtyAvaForeground");
            }
        }

        private string _qtyAvaBackground;
        public string QtyAvaBackground
        {
            get { return _qtyAvaBackground; }
            set
            {
                _qtyAvaBackground = value;

                RaisePropertyChanged("QtyAvaBackground");
            }
        }

        private string _projAvaForeground;
        public string ProjAvaForeground
        {
            get { return _qtyAvaBackground; }
            set
            {
                _projAvaForeground = value;

                RaisePropertyChanged("ProjAvaForeground");
            }
        }

        public bool IsExpanded
        {
            get
            {
                return _isExpanded;
            }
            set
            {
                _isExpanded = value;
                RaisePropertyChanged("IsExpanded");
            }
        }


        public object Clone()
        {
            return MemberwiseClone();
        }
    }

}
