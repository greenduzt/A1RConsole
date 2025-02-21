using A1RConsole.Bases;
using A1RConsole.Interfaces;
using A1RConsole.Models.Products;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A1RConsole.Models.Orders
{
    public class OrderDetails : ObservableObject, ISequencedObject
    {
        private int _salesOrderDetailsID;
        private int p_SequenceNumber;
        private int _orderLine;
        private string _lineStatus;
        private Product _product;
        private string _productNameString;
        private string _reRollingVisible;
        private string _customProductVisible;
        private string _quantityStr;
        private decimal _quantity;
        private decimal _blocksLogsToMake;
        private decimal _qtyInStock;
        private decimal _qtyToMake;
        private int _discount;
        private int _maxDiscount;
        private decimal _total;
        private string _toMakeCellBack;
        private string _toMakeCellFore;
        private string _inStockCellBack;
        private string _inStockCellFore;
        private string _productSource;
        private bool _productCodeEnabled;
        private decimal _totalKg;
        private decimal _productTotalBeforeDis;
        private string _sODTimeStamp;
        private string _pSTimeStamp;
        private decimal _discountedTotal;
        private decimal _quoteUnitPrice;
        private string _quoteProductDescription;
        private string _isQtyReserved;
        private bool _isDiscountEnabled;
        private int _prefix;

        public bool IsEditing { get; set; }

        public OrderDetails()
        {
            Quantity = 0;
            ProductCodeEnabled = false;
            CustomProductVisible = "Collapsed";
            ReRollingVisible = "Collapsed";
            ToMakeCellBack = "White";
            ToMakeCellFore = "Black";
            InStockCellBack = "White";
            InStockCellFore = "Black";
            IsEditing = false;

        }

        #region PUBLIC PROPERTIES

        public int Prefix
        {
            get { return _prefix; }

            set
            {
                _prefix = value;
                RaisePropertyChanged(() => this.Prefix);

            }
        }

        public string IsQtyReserved
        {
            get { return _isQtyReserved; }

            set
            {
                _isQtyReserved = value;
                RaisePropertyChanged(() => this.IsQtyReserved);
                
            }
        }

        public int SequenceNumber
        {
            get { return p_SequenceNumber; }

            set
            {
                p_SequenceNumber = value;
                RaisePropertyChanged(() => this.SequenceNumber);
                OrderLine = SequenceNumber;
            }
        }


        public decimal DiscountedTotal
        {
            get { return _discountedTotal; }
            set
            {
                _discountedTotal = value;

                RaisePropertyChanged(() => this.DiscountedTotal);
            }
        }

        

        public int OrderLine
        {
            get { return _orderLine; }
            set
            {
                _orderLine = value;

                RaisePropertyChanged(() => this.OrderLine);
            }
        }

        public string LineStatus
        {
            get { return _lineStatus; }
            set
            {
                _lineStatus = value;

                RaisePropertyChanged(() => this.LineStatus);
            }
        }



        public bool ProductCodeEnabled
        {
            get { return _productCodeEnabled; }
            set
            {
                _productCodeEnabled = value;

                RaisePropertyChanged(() => this.ProductCodeEnabled);
            }
        }

        public string InStockCellBack
        {
            get { return _inStockCellBack; }
            set
            {
                _inStockCellBack = value;

                RaisePropertyChanged(() => this.InStockCellBack);
            }
        }

        public string InStockCellFore
        {
            get { return _inStockCellFore; }
            set
            {
                _inStockCellFore = value;

                RaisePropertyChanged(() => this.InStockCellFore);
            }
        }


        public decimal BlocksLogsToMake
        {
            get { return _blocksLogsToMake; }
            set
            {
                _blocksLogsToMake = value;

                RaisePropertyChanged(() => this.BlocksLogsToMake);
            }
        }
        public decimal QtyToMake
        {
            get { return _qtyToMake; }
            set
            {
                _qtyToMake = value;

                RaisePropertyChanged(() => this.QtyToMake);
            }
        }

        public decimal ProductTotalBeforeDis
        {
            get { return _productTotalBeforeDis; }
            set
            {
                _productTotalBeforeDis = value;

                RaisePropertyChanged(() => this.ProductTotalBeforeDis);
            }
        }

        //public decimal QtyInStock
        //{
        //    get { return _qtyInStock; }
        //    set
        //    {
        //        _qtyInStock = value;

        //        RaisePropertyChanged(() => this.QtyInStock);
        //    }
        //}

        public string ProductSource
        {
            get { return _productSource; }
            set
            {
                _productSource = value;

                RaisePropertyChanged(() => this.ProductSource);
            }
        }

        public string ProductNameString
        {
            get { return _productNameString; }
            set
            {
                _productNameString = value;

                RaisePropertyChanged(() => this.ProductNameString);
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

                RaisePropertyChanged(() => this.Product);

                if (Product != null)
                {
                    string density = string.Empty;
                    string thickness = string.Empty;
                    string length = string.Empty;
                    string size = string.Empty;
                    string width = string.Empty;
                    string height = string.Empty;
                   
                }
                //CalculateTotal();
            }
        }


        public string CustomProductVisible
        {
            get { return _customProductVisible; }
            set
            {
                _customProductVisible = value;

                RaisePropertyChanged(() => this.CustomProductVisible);
            }
        }

        public string ReRollingVisible
        {
            get { return _reRollingVisible; }
            set
            {
                _reRollingVisible = value;

                RaisePropertyChanged(() => this.ReRollingVisible);
            }
        }

        public string QuantityStr
        {
            get
            {
                return _quantityStr;
            }
            set
            {
                _quantityStr = value;

                RaisePropertyChanged(() => this.QuantityStr);

                if (String.IsNullOrWhiteSpace(QuantityStr))
                {
                    Quantity = 0;
                }
                else
                {
                    Quantity = Convert.ToDecimal(QuantityStr);
                }
            }
        }

        public decimal Quantity
        {
            get
            {
                return _quantity;
            }
            set
            {
                _quantity = value;

                RaisePropertyChanged(() => this.Quantity);
                if (Quantity > 0)
                {
                    ProductCodeEnabled = true;
                }
                else
                {
                    QtyInStock = 0;
                    QtyToMake = 0;
                    Discount = 0;
                    Total = 0;

                    InStockCellBack = "White";
                    InStockCellFore = "Black";

                    ToMakeCellBack = "White";
                    ToMakeCellFore = "Black";

                    ProductCodeEnabled = false;
                    Product = null;
                }
                //CalculateTotal();
            }
        }




        public decimal QtyInStock
        {
            get { return _qtyInStock; }
            set
            {
                _qtyInStock = value;

                RaisePropertyChanged(() => this.QtyInStock);
            }
        }

        public int Discount
        {
            get
            {
                return _discount;
            }
            set
            {
                _discount = value;

                RaisePropertyChanged(() => this.Discount);

                if (Discount > 100)
                {
                    
                    Discount = Convert.ToInt16(Discount.ToString().Remove(Discount.ToString().Length - 1, 1));
                    
                }
                else if (Discount < 0)
                {
                    Discount = 0;
                }
                else
                {
                    if (Product != null && Product.Category != null && Product.Category.CategoryID == 11)
                    {
                        if (Discount > MaxDiscount)
                        {
                            //Discount = Convert.ToInt16(Discount.ToString().Remove(Discount.ToString().Length - 1, 1));
                            Discount = MaxDiscount;
                        }
                    }                    
                }
            }
        }

        public decimal Total
        {
            get
            {
                return _total;
            }
            set
            {
                _total = value;
                RaisePropertyChanged(() => this.Total);
            }
        }

        public string ToMakeCellBack
        {
            get
            {
                return _toMakeCellBack;
            }
            set
            {
                _toMakeCellBack = value;
                RaisePropertyChanged(() => this.ToMakeCellBack);
            }
        }

        public string ToMakeCellFore
        {
            get
            {
                return _toMakeCellFore;
            }
            set
            {
                _toMakeCellFore = value;
                RaisePropertyChanged(() => this.ToMakeCellFore);
            }
        }

        public decimal TotalKg
        {
            get
            {
                return _totalKg;
            }
            set
            {
                _totalKg = value;
                RaisePropertyChanged(() => this.TotalKg);
            }
        }

        public string SODTimeStamp
        {
            get
            {
                return _sODTimeStamp;
            }
            set
            {
                _sODTimeStamp = value;
                RaisePropertyChanged(() => this.SODTimeStamp);
            }
        }

        public string PSTimeStamp
        {
            get
            {
                return _pSTimeStamp;
            }
            set
            {
                _pSTimeStamp = value;
                RaisePropertyChanged(() => this.PSTimeStamp);
            }
        }

        public decimal QuoteUnitPrice
        {
            get
            {
                return _quoteUnitPrice;
            }
            set
            {
                _quoteUnitPrice = value;
                RaisePropertyChanged(() => this.QuoteUnitPrice);
            }
        }

        public string QuoteProductDescription
        {
            get
            {
                return _quoteProductDescription;
            }
            set
            {
                _quoteProductDescription = value;
                RaisePropertyChanged(() => this.QuoteProductDescription);
            }
        }


        public int SalesOrderDetailsID
        {
            get
            {
                return _salesOrderDetailsID;
            }
            set
            {
                _salesOrderDetailsID = value;
                RaisePropertyChanged(() => this.SalesOrderDetailsID);
            }
        }

        public bool IsDiscountEnabled
        {
            get
            {
                return _isDiscountEnabled;
            }
            set
            {
                _isDiscountEnabled = value;
                RaisePropertyChanged(() => this.IsDiscountEnabled);
            }
        }

        public int MaxDiscount
        {
            get
            {
                return _maxDiscount;
            }
            set
            {
                _maxDiscount = value;
                RaisePropertyChanged(() => this.MaxDiscount);
            }
        }
        

        #endregion
    }
}

