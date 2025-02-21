using A1RConsole.Bases;
using A1RConsole.Models.Comments;
using A1RConsole.Models.Customers;
using A1RConsole.Models.Dispatch;
using A1RConsole.Models.Freights;
using A1RConsole.Models.Invoices;
using A1RConsole.Models.Stock;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A1RConsole.Models.Orders
{
    public class SalesOrder : NewOrderPDFM
    {
        public Int32 SalesOrderNo { get; set; }
        public Int32 InvoiceNo { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime PaymentDueDate { get; set; }
        public int OrderPriority { get; set; }
        public List<Comment> Comments { get; set; }
        public string SalesMadeBy { get; set; }
        public int QuoteNo { get; set; }
        public string LastModifiedBy { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public DispatchOrder DispatchOrder { get; set; }
        public Invoice Invoice { get; set; }
        public string SearchString { get; set; }
        public string StatusForeGroundCol { get; set; }
        public string StatusBackgroundCol { get; set; }
        public string DaysRemBackgroundCol { get; set; }
        public string DaysRemForeGroundCol { get; set; }
        public string TimeStamp { get; set; }

        private FreightCarrier _freightCarrier;
        private string _salesCompletedBy;
        private string _prepaidCustomerName;
        private string _pickupTime;
        private string _customerOrderNo;
        private string _billTo;
        private string _shipTo;
        private Customer _customer;
        private DateTime? _desiredDispatchDate;
        private ObservableCollection<SalesOrderDetails> _salesOrderDetails;
        private BindingList<FreightDetails> _freightDetails;
        private string _orderStatus;
        private string _orderAction;
        private decimal _listPriceTotal;
        private decimal _freightTotal;
        private decimal _totalAmount;
        private decimal _gst;
        private string _termsID;
        private StockLocation _stockLocation;
        private string _stockReserved;
        private string _daysRemaining;
        private bool _paymentRecieved;
        private bool _gstEnabled;

        public event EventHandler Changed;

        public FreightCarrier FreightCarrier
        {
            get
            {
                return _freightCarrier;
            }
            set
            {
                _freightCarrier = value;
                RaisePropertyChanged("FreightCarrier");
            }
        }

        public string StockReserved
        {
            get
            {
                return _stockReserved;
            }
            set
            {
                _stockReserved = value;
                RaisePropertyChanged("StockReserved");
            }
        }

        public bool PaymentRecieved
        {
            get
            {
                return _paymentRecieved;
            }
            set
            {
                _paymentRecieved = value;
                RaisePropertyChanged("PaymentRecieved");
            }
        }

        public string BillTo
        {
            get
            {
                return _billTo;
            }
            set
            {
                _billTo = value;
                RaisePropertyChanged("BillTo");
            }
        }

        public string ShipTo
        {
            get
            {
                return _shipTo;
            }
            set
            {
                _shipTo = value;
                RaisePropertyChanged("ShipTo");
            }
        }

        public string CustomerOrderNo
        {
            get
            {
                return _customerOrderNo;
            }
            set
            {
                _customerOrderNo = value;
                RaisePropertyChanged("CustomerOrderNo");
            }
        }

        public string OrderStatus
        {
            get
            {
                return _orderStatus;
            }
            set
            {
                _orderStatus = value;
                RaisePropertyChanged("OrderStatus");
            }
        }

        public string OrderAction
        {
            get
            {
                return _orderAction;
            }
            set
            {
                _orderAction = value;
                RaisePropertyChanged("OrderAction");

            }
        }

        public Customer Customer
        {
            get
            {
                return _customer;
            }
            set
            {
                _customer = value;
                RaisePropertyChanged("Customer");
            }
        }

        public string PrepaidCustomerName
        {
            get
            {
                return _prepaidCustomerName;
            }
            set
            {
                _prepaidCustomerName = value;
                RaisePropertyChanged("PrepaidCustomerName");
            }
        }

        public DateTime? DesiredDispatchDate
        {
            get
            {
                return _desiredDispatchDate;
            }
            set
            {
                _desiredDispatchDate = value;
                RaisePropertyChanged("DesiredDispatchDate");
            }
        }

        public ObservableCollection<SalesOrderDetails> SalesOrderDetails
        {
            get
            {
                return _salesOrderDetails;
            }
            set
            {
                _salesOrderDetails = value;
                RaisePropertyChanged("SalesOrderDetails");
            }
        }

        public BindingList<FreightDetails> FreightDetails
        {
            get
            {
                return _freightDetails;
            }
            set
            {
                _freightDetails = value;
                RaisePropertyChanged("FreightDetails");

            }
        }

        public string PickupTime
        {
            get
            {
                return _pickupTime;
            }
            set
            {
                _pickupTime = value;
                RaisePropertyChanged("PickupTime");
            }
        }

        public decimal ListPriceTotal
        {
            get
            {
                return _listPriceTotal;
            }
            set
            {
                _listPriceTotal = value;
                RaisePropertyChanged("ListPriceTotal");
            }
        }

        public decimal FreightTotal
        {
            get
            {
                return _freightTotal;
            }
            set
            {
                _freightTotal = value;
                RaisePropertyChanged("FreightTotal");
            }
        }



        public decimal TotalAmount
        {
            get
            {
                return _totalAmount;
            }
            set
            {
                _totalAmount = value;
                RaisePropertyChanged("TotalAmount");
            }
        }

        public decimal GST
        {
            get
            {
                return _gst;
            }
            set
            {
                _gst = value;
                RaisePropertyChanged("GST");
            }
        }

        public string TermsID
        {
            get
            {
                return _termsID;
            }
            set
            {
                _termsID = value;
                RaisePropertyChanged("TermsID");
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

        public string DaysRemaining
        {
            get
            {
                return _daysRemaining;
            }
            set
            {
                _daysRemaining = value;
                RaisePropertyChanged("DaysRemaining");
            }
        }

        public bool GSTEnabled
        {
            get
            {
                return _gstEnabled;
            }
            set
            {
                _gstEnabled = value;
                RaisePropertyChanged("GSTEnabled");
            }
        }
        private string _paymentFinalisedBackGround;
        public string PaymentFinalisedBackGround
        {
            get
            {
                return _paymentFinalisedBackGround;
            }
            set
            {
                _paymentFinalisedBackGround = value;
                RaisePropertyChanged("PaymentFinalisedBackGround");
            }
        }

        private string _paymentFinalisedForeGround;
        public string PaymentFinalisedForeGround
        {
            get
            {
                return _paymentFinalisedForeGround;
            }
            set
            {
                _paymentFinalisedForeGround = value;
                RaisePropertyChanged("PaymentFinalisedForeGround");
            }
        }


        public string SalesCompletedBy
        {
            get
            {
                return _salesCompletedBy;
            }
            set
            {
                _salesCompletedBy = value;
                RaisePropertyChanged("SalesCompletedBy");

            }
        }

        protected override void OnPropertyChanged(string propertyName)
        {
            var hander = this.Changed;
            if (hander != null)
            {
                hander(this, EventArgs.Empty);
            }
        }
    }
}
