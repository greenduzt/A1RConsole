using A1RConsole.Bases;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A1RConsole.Models.Vjs
{
    public class VjsOrder: ViewModelBase
    {
        private string _state;
        private string _orderID;
        private DateTime _orderDate;
        private DateTime _desiredShippingDate;
        private string _customerName;
        private string _orderStatus;
        private decimal _totalAmount;
        private string _notes;
        private ObservableCollection<VjsOrderDetails> _vjsOrderDetails;
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

        public string State
        {
            get
            {
                return _state;
            }
            set
            {
                _state = value;
                RaisePropertyChanged("State");
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

        public DateTime DesiredShippingDate
        {
            get
            {
                return _desiredShippingDate;
            }
            set
            {
                _desiredShippingDate = value;
                RaisePropertyChanged("DesiredShippingDate");
            }
        }

        public DateTime OrderDate
        {
            get
            {
                return _orderDate;
            }
            set
            {
                _orderDate = value;
                RaisePropertyChanged("OrderDate");
            }
        }

        public string CustomerName
        {
            get
            {
                return _customerName;
            }
            set
            {
                _customerName = value;
                RaisePropertyChanged("CustomerName");
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

        public string Notes
        {
            get
            {
                return _notes;
            }
            set
            {
                _notes = value;
                RaisePropertyChanged("Notes");
            }
        }

        public ObservableCollection<VjsOrderDetails> VjsOrderDetails
        {
            get
            {
                return _vjsOrderDetails;
            }
            set
            {
                _vjsOrderDetails = value;
                RaisePropertyChanged("VjsOrderDetails");
            }
        }
    }
}
