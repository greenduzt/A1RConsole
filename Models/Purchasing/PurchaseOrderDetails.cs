using A1RConsole.Bases;
using A1RConsole.Interfaces;
using A1RConsole.Models.Products;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A1RConsole.Models.Purchasing
{
    public class PurchaseOrderDetails : ObservableObject, ISequencedObject
    {
        //private decimal originalReceivedQty;
        public decimal originalReceivedQty { get; set; }
        private decimal _enteredQty;
        private decimal _recievedQty;
        private Product _product;
        private int p_SequenceNumber;
        private int _lineNo;
        private decimal _total;
        private decimal _orderQty;
        private decimal _totalRecieved;
        private bool _productCodeEnabled;
        private bool _rowEnable;
        private string _result;

        public PurchaseOrderDetails()
        {
            ProductCodeEnabled = false;
            LineStatus = "Open";
        }

        private void CalculateTotal()
        {
            if (Product != null)
            {
                Total = Product.MaterialCost * OrderQty;
            }
        }

        public int LineNo
        {
            get
            {
                return _lineNo;
            }
            set
            {
                _lineNo = value;
                RaisePropertyChanged(() => this.LineNo);
            }
        }

        public int SequenceNumber
        {
            get { return p_SequenceNumber; }

            set
            {
                p_SequenceNumber = value;
                RaisePropertyChanged(() => this.LineNo);
                LineNo = SequenceNumber;
            }
        }

        public Product Product
        {
            get { return _product; }

            set
            {
                _product = value;
                RaisePropertyChanged(() => this.Product);
                CalculateTotal();
            }
        }

        public decimal Total
        {
            get { return _total; }

            set
            {
                _total = value;
                RaisePropertyChanged(() => this.Total);

            }
        }

        public decimal OrderQty
        {
            get { return _orderQty; }

            set
            {
                _orderQty = value;
                RaisePropertyChanged(() => this.OrderQty);
                if (OrderQty > 0)
                {
                    ProductCodeEnabled = true;
                }
                else
                {
                    Total = 0;

                    ProductCodeEnabled = false;
                    Product = null;
                }
                CalculateTotal();
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




        public decimal EnteredQty
        {
            get { return _enteredQty; }
            set
            {
                if (value <= (OrderQty - RecievedQty < 0 ? 0 : OrderQty - RecievedQty))
                {
                    _enteredQty = value;
                }
                RaisePropertyChanged(() => this.EnteredQty);
            }
        }

        public decimal RecievedQty
        {
            get { return _recievedQty; }
            set
            {
                _recievedQty = value;

                RaisePropertyChanged(() => this.RecievedQty);
            }
        }

        public decimal TotalRecieved
        {
            get { return _totalRecieved; }
            set
            {
                _totalRecieved = value;

                RaisePropertyChanged(() => this.TotalRecieved);
            }
        }

        public bool RowEnable
        {
            get { return _rowEnable; }
            set
            {
                _rowEnable = value;

                RaisePropertyChanged(() => this.RowEnable);
            }
        }

        public string Result
        {
            get { return _result; }
            set
            {
                _result = value;

                RaisePropertyChanged(() => this.Result);

            }
        }

        public Int32 ID { get; set; }
        public Int32 PurchaseOrderID { get; set; }
        //public int LineNo { get; set; }
        public string LineStatus { get; set; }
        public DateTime LineDesiredRecieveDate { get; set; }
        //public decimal OrderQty { get; set; }
        //public decimal RecievedQty { get; set; }
        public string LastUpdatedBy { get; set; }
        public DateTime LastUpdatedDate { get; set; }
        //public Product Product { get; set; }
        //public decimal Total { get; set; }
    }
}
