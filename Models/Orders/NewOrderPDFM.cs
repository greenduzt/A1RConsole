using A1RConsole.Models.Quoting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A1RConsole.Models.Orders
{
    public class NewOrderPDFM : Quote
    {      

        private int _id;
        private string _quoteNoStr;
        private string _siteContactPhone;
        private string _siteContactName;
        private DateTime? _requiredDate;
        private string _dateTypeRequired;
        private string _freightType;
        private string _orderTruck;
        private string _unloadType;
        private string _courierName;
        private string _customerToChargedFreight;

        public DateTime? RequiredDate
        {
            get { return _requiredDate; }
            set
            {
                _requiredDate = value;
                RaisePropertyChanged("RequiredDate");
            }
        }

        public string SiteContactName
        {
            get { return _siteContactName; }
            set
            {
                _siteContactName = value;
                RaisePropertyChanged("SiteContactName");
            }
        }

        public string SiteContactPhone
        {
            get { return _siteContactPhone; }
            set
            {
                _siteContactPhone = value;
                RaisePropertyChanged("SiteContactPhone");
            }
        }

        public string DateTypeRequired
        {
            get { return _dateTypeRequired; }
            set
            {
                _dateTypeRequired = value;
                RaisePropertyChanged("DateTypeRequired");
            }
        }

        public string QuoteNoStr
        {
            get { return _quoteNoStr; }
            set
            {
                _quoteNoStr = value;
                RaisePropertyChanged("QuoteNoStr");
            }
        }        

        public string FreightType
        {
            get { return _freightType; }
            set
            {
                _freightType = value;
                RaisePropertyChanged("FreightType");
            }
        }

        public string OrderTruck
        {
            get { return _orderTruck; }
            set
            {
                _orderTruck = value;
                RaisePropertyChanged("OrderTruck");
            }
        }

        public string CustomerToChargedFreight
        {
            get { return _customerToChargedFreight; }
            set
            {
                _customerToChargedFreight = value;
                RaisePropertyChanged("CustomerToChargedFreight");
            }
        }

        public int ID
        {
            get { return _id; }
            set
            {
                _id = value;
                RaisePropertyChanged("ID");
            }
        }

        public string UnloadType
        {
            get { return _unloadType; }
            set
            {
                _unloadType = value;
                RaisePropertyChanged("UnloadType");
            }
        }

        public string CourierName
        {
            get { return _courierName; }
            set
            {
                _courierName = value;
                RaisePropertyChanged("CourierName");
            }
        }
        
        
    }
}
