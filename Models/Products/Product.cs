using A1RConsole.Bases;
using A1RConsole.Models.Categories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A1RConsole.Models.Products
{
    public class Product : ViewModelBase
    {
        public int ProductID { get; set; }
        public Category Category { get; set; }
        public ProductType ProductType { get; set; }
        public RawProduct RawProduct { get; set; }
        public string CommodityCode { get; set; }
        public string Type { get; set; }
        public string ProductName { get; set; }
        //public string ProductCode { get; set; }
        //public string ProductDescription { get; set; }
        public string Size { get; set; }
        public string ProductUnit { get; set; }
        
        
           
        //public int LeadTime { get; set; }
        
        public string Density { get; set; }
        //public decimal Thickness { get; set; }
        public decimal Width { get; set; }
        //public decimal Height { get; set; }
        //public decimal MaxYield { get; set; }
        //public decimal MinYield { get; set; }
        public decimal MinCutLength { get; set; }       
        
        public string MouldType { get; set; }
        public bool IsCustomReRoll { get; set; }
        public string LogoPath { get; set; }
        public string QRVideoPath { get; set; }
        public string QRPDFPath { get; set; }
        public bool Active { get; set; }
        public bool IsManufactured { get; set; }
        public bool IsPurchased { get; set; }
        //public string LastModifiedBy { get; set; }
        public DateTime? LastModifiedDate { get; set; }
        public bool IsRawMaterial { get; set; }
        public bool IsAutoOrder { get; set; }
        public Tile Tile { get; set; }

        private decimal _materialCost;
        private string _productCode;
        private decimal _unitPrice;
        private decimal _unitCost;
        private string _timeStamp;
        private decimal _unitsPerPack;
        private decimal _orderPoint;
        private decimal _safetyStockQty;
        private decimal _minimumOrderQty;
        private decimal _orderInMultiplesOf;
        private string _productDescription;
        private bool _priceEditEnabled;
        private string _locationType;

        public decimal OrderInMultiplesOf
        {
            get
            {
                return _orderInMultiplesOf;
            }
            set
            {
                _orderInMultiplesOf = value;
                RaisePropertyChanged("OrderInMultiplesOf");
            }
        }

        public decimal MinimumOrderQty
        {
            get
            {
                return _minimumOrderQty;
            }
            set
            {
                _minimumOrderQty = value;
                RaisePropertyChanged("MinimumOrderQty");
            }
        }

        public decimal SafetyStockQty
        {
            get
            {
                return _safetyStockQty;
            }
            set
            {
                _safetyStockQty = value;
                RaisePropertyChanged("SafetyStockQty");
            }
        }

        public decimal OrderPoint
        {
            get
            {
                return _orderPoint;
            }
            set
            {
                _orderPoint = value;
                RaisePropertyChanged("OrderPoint");
            }
        }

        public decimal UnitsPerPack
        {
            get
            {
                return _unitsPerPack;
            }
            set
            {
                _unitsPerPack = value;
                RaisePropertyChanged("UnitsPerPack");
            }
        }

        public decimal UnitCost
        {
            get
            {
                return _unitCost;
            }
            set
            {
                _unitCost = value;
                RaisePropertyChanged("UnitCost");
            }
        }

        public decimal UnitPrice
        {
            get
            {
                return _unitPrice;
            }
            set
            {
                _unitPrice = value;
                RaisePropertyChanged("UnitPrice");

            }
        }

        public decimal MaterialCost
        {
            get
            {
                return _materialCost;
            }
            set
            {
                _materialCost = value;
                RaisePropertyChanged("MaterialCost");
            }
        }

        public string ProductCode
        {
            get
            {
                return _productCode;
            }
            set
            {
                _productCode = value;
                RaisePropertyChanged("ProductCode");
            }
        }

        private string _lastModifiedBy;
        public string LastModifiedBy
        {
            get
            {
                return _lastModifiedBy;
            }
            set
            {
                _lastModifiedBy = value;
                RaisePropertyChanged("LastModifiedBy");
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

        public string ProductDescription
        {
            get
            {
                return _productDescription;
            }
            set
            {
                _productDescription = value;
                RaisePropertyChanged("ProductDescription");
            }
        }

        public bool PriceEditEnabled
        {
            get
            {
                return _priceEditEnabled;
            }
            set
            {
                _priceEditEnabled = value;
                RaisePropertyChanged("PriceEditEnabled");
            }
        }


        public string LocationType
        {
            get
            {
                return _locationType;
            }
            set
            {
                _locationType = value;
                RaisePropertyChanged("LocationType");
            }
        }

        
    }
}

