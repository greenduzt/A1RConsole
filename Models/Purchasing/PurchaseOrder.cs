using A1RConsole.Bases;
using A1RConsole.Models.Deliveries;
using A1RConsole.Models.Suppliers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A1RConsole.Models.Purchasing
{
    public class PurchaseOrder : ViewModelBase
    {

        //public PurchaseOrder()
        //{
        //    PurchaseOrderDetails = new ObservableCollection<PurchaseOrderDetails>();
        //}

        public Int32 PurchasingOrderNo { get; set; }
        public Supplier Supplier { get; set; }
        //public DateTime OrderDate { get; set; }
        public TimeSpan OrderTIme { get; set; }
        //public DateTime RecieveOnDate { get; set; }
        public string Status { get; set; }
        public string SupplierQuoteReference { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public string LastModifiedBy { get; set; }
        //public string PurchaseFrom { get; set; }
        //public string ShipTo { get; set; }
        //public DateTime PromisedDate { get; set; }
        public DateTime LastRecievedDate { get; set; }
        public Delivery Delivery { get; set; }
        public bool Completed { get; set; }

        private ObservableCollection<PurchaseOrderDetails> _purchaseOrderDetails;
        private decimal _subTotal;
        private decimal _totalAmount;
        private string _purchaseFrom;

        public ObservableCollection<PurchaseOrderDetails> PurchaseOrderDetails
        {
            get
            {
                return _purchaseOrderDetails;
            }
            set
            {
                _purchaseOrderDetails = value;
                RaisePropertyChanged("PurchaseOrderDetails");
            }
        }

        public decimal SubTotal
        {
            get
            {
                return _subTotal;
            }
            set
            {
                _subTotal = value;
                RaisePropertyChanged("SubTotal");
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

        private decimal _tax;
        public decimal Tax
        {
            get
            {
                return _tax;
            }
            set
            {
                _tax = value;
                RaisePropertyChanged("Tax");
            }
        }

        private string _daysToReceive;
        public string DaysToReceive
        {
            get { return _daysToReceive; }
            set
            {
                _daysToReceive = value;

                RaisePropertyChanged("DaysToReceive");
            }
        }


        private string _daysToReceiveBackground;
        public string DaysToReceiveBackground
        {
            get { return _daysToReceiveBackground; }
            set
            {
                _daysToReceiveBackground = value;

                RaisePropertyChanged("DaysToReceiveBackground");
            }
        }

        private string _daysToReceiveForeground;
        public string DaysToReceiveForeground
        {
            get { return _daysToReceiveForeground; }
            set
            {
                _daysToReceiveForeground = value;

                RaisePropertyChanged("DaysToReceiveForeground");
            }
        }


        private string _statusBackground;
        public string StatusBackground
        {
            get { return _statusBackground; }
            set
            {
                _statusBackground = value;

                RaisePropertyChanged("StatusBackground");
            }
        }

        private string _statusForeground;
        public string StatusForeground
        {
            get { return _statusForeground; }
            set
            {
                _statusForeground = value;

                RaisePropertyChanged("StatusForeground");
            }
        }


        private DateTime _receivedDate;
        public DateTime ReceivedDate
        {
            get { return _receivedDate; }
            set
            {
                _receivedDate = value;

                RaisePropertyChanged("ReceivedDate");
            }
        }

        private bool _ticked;
        public bool Ticked
        {
            get { return _ticked; }
            set
            {
                _ticked = value;

                RaisePropertyChanged("Ticked");
            }
        }

        private bool _isExpanded;
        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                _isExpanded = value;

                RaisePropertyChanged("IsExpanded");
            }
        }

        public string PurchaseFrom
        {
            get { return _purchaseFrom; }
            set
            {
                _purchaseFrom = value;

                RaisePropertyChanged("PurchaseFrom");
            }
        }

        private string _shipTo;
        public string ShipTo
        {
            get { return _shipTo; }
            set
            {
                _shipTo = value;

                RaisePropertyChanged("ShipTo");
            }
        }

        private string _notes;
        public string Notes
        {
            get { return _notes; }
            set
            {
                _notes = value;

                RaisePropertyChanged("Notes");
            }
        }

        private DateTime _orderDate;
        public DateTime OrderDate
        {
            get { return _orderDate; }
            set
            {
                _orderDate = value;

                RaisePropertyChanged("OrderDate");
            }
        }

        private DateTime _recieveOnDate;
        public DateTime RecieveOnDate
        {
            get { return _recieveOnDate; }
            set
            {
                _recieveOnDate = value;

                RaisePropertyChanged("RecieveOnDate");
            }
        }




        public object Clone()
        {
            return MemberwiseClone();
        }


    }
}
