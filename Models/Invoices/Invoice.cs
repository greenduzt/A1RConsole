using A1RConsole.Models.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A1RConsole.Models.Invoices
{
    public class Invoice : SalesOrder
    {

        public Int16 InvoiceNo { get; set; }
        //public SalesOrder SalesOrder { get; set; }
        //public bool ExportToMyOb { get; set; }
        //public bool ExportedToMyOb { get; set; }
        public string ExportedToMyObStr { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime InvoicedDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public string CompletedBy { get; set; }
        public bool IsActive { get; set; }
        public bool IsTaken { get; set; }

        private bool _exportToMyOb;
        public bool ExportToMyOb
        {
            get
            {
                return _exportToMyOb;
            }
            set
            {
                _exportToMyOb = value;
                RaisePropertyChanged("ExportToMyOb");
            }
        }

        private bool _exportedToMyOb;
        public bool ExportedToMyOb
        {
            get
            {
                return _exportedToMyOb;
            }
            set
            {
                _exportedToMyOb = value;
                RaisePropertyChanged("ExportedToMyOb");
            }
        }

        private bool _sendToMyObEnabled;
        public bool SendToMyObEnabled
        {
            get
            {
                return _sendToMyObEnabled;
            }
            set
            {
                _sendToMyObEnabled = value;
                RaisePropertyChanged("SendToMyObEnabled");
            }
        }

        private bool _printInvoiceActive;
        public bool PrintInvoiceActive
        {
            get
            {
                return _printInvoiceActive;
            }
            set
            {
                _printInvoiceActive = value;
                RaisePropertyChanged("PrintInvoiceActive");
            }
        }


        private string _printInvoiceBackGroundColour;
        public string PrintInvoiceBackGroundColour
        {
            get
            {
                return _printInvoiceBackGroundColour;
            }
            set
            {
                _printInvoiceBackGroundColour = value;
                RaisePropertyChanged("PrintInvoiceBackGroundColour");
            }
        }

        private string _orderStatusVisibility;
        public string OrderStatusVisibility
        {
            get
            {
                return _orderStatusVisibility;
            }
            set
            {
                _orderStatusVisibility = value;
                RaisePropertyChanged("OrderStatusVisibility");
            }
        }

        private bool _prepareInvoiceActive;
        public bool PrepareInvoiceActive
        {
            get
            {
                return _prepareInvoiceActive;
            }
            set
            {
                _prepareInvoiceActive = value;
                RaisePropertyChanged("PrepareInvoiceActive");
            }
        }
        private string _prepareInvoiceBackGround;
        public string PrepareInvoiceBackGround
        {
            get
            {
                return _prepareInvoiceBackGround;
            }
            set
            {
                _prepareInvoiceBackGround = value;
                RaisePropertyChanged("PrepareInvoiceBackGround");
            }
        }

        //private string _paymentFinalisedBackGround;
        //public string PaymentFinalisedBackGround
        //{
        //    get
        //    {
        //        return _paymentFinalisedBackGround;
        //    }
        //    set
        //    {
        //        _paymentFinalisedBackGround = value;
        //        RaisePropertyChanged("PaymentFinalisedBackGround");
        //    }
        //}

        //private string _paymentFinalisedForeGround;
        //public string PaymentFinalisedForeGround
        //{
        //    get
        //    {
        //        return _paymentFinalisedForeGround;
        //    }
        //    set
        //    {
        //        _paymentFinalisedForeGround = value;
        //        RaisePropertyChanged("PaymentFinalisedForeGround");
        //    }
        //}

    }
}
