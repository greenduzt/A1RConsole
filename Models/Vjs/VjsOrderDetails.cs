using A1RConsole.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A1RConsole.Models.Vjs
{
    public class VjsOrderDetails: ViewModelBase
    {
        private string _orderID;
        private int _lineNo;
        private string _partID;
        private string _description;
        private decimal _orderQty;
        private string _unit;
        private decimal _unitPrice;
        private decimal _orderAmount;
        private decimal _discount;
        private string _productString;

        public string OrderID
        {
            get
            {
                return _orderID;
            }
            set
            {
                _orderID = value;
                RaisePropertyChanged("OrderID");
            }
        }

        public decimal Discount
        {
            get
            {
                return _discount;
            }
            set
            {
                _discount = value;
                RaisePropertyChanged("Discount");
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
                RaisePropertyChanged("LineNo");
            }
        }

        public string PartID
        {
            get
            {
                return _partID;
            }
            set
            {
                _partID = value;
                RaisePropertyChanged("PartID");
            }
        }

        public string Description
        {
            get
            {
                return _description;
            }
            set
            {
                _description = value;
                RaisePropertyChanged("Description");
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

        public string Unit
        {
            get
            {
                return _unit;
            }
            set
            {
                _unit = value;
                RaisePropertyChanged("Unit");
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

        public decimal OrderAmount
        {
            get
            {
                return _orderAmount;
            }
            set
            {
                _orderAmount = value;
                RaisePropertyChanged("OrderAmount");
            }
        }

        public string ProductString
        {
            get
            {
                return _productString;
            }
            set
            {
                _productString = value;
                RaisePropertyChanged("ProductString");
            }
        }
        

    }
}
