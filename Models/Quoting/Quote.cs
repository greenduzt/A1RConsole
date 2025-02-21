using A1RConsole.Bases;
using A1RConsole.Models.Customers;
using A1RConsole.Models.Freights;
using A1RConsole.Models.Users;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A1RConsole.Models.Quoting
{
    public class Quote : ViewModelBase
    {
        private Int32 _quoteNo;
        private Customer _customer;
        private string _projectName;
        private decimal _listPriceTotal;
        private decimal _discountedTotal;
        private decimal _freightTotal;
        private decimal _gst;
        private decimal _totalAmount;
        private User _user;
        private DateTime _quoteDate;
        private string _instructions;
        private string _internalComments;
        private DateTime _lastUpdatedDate;
        private User _lastUpdatedBy;
        private string _timeStamp;
        private decimal _unitsPerPack;
        private decimal _totalBeforeTax;
        private string _shipTo;
        private ObservableCollection<QuoteDetails> _quoteDetails;
        private FreightCarrier _freightCarrier;
        private BindingList<FreightDetails> _freightDetails;
        private ContactPerson _contactPerson;
        private string _fileName;
        private bool _gSTActive;
        private string _quoteCourierName;

        public Int32 QuoteNo
        {
            get
            {
                return _quoteNo;
            }
            set
            {
                _quoteNo = value;
                RaisePropertyChanged("QuoteNo");
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

        public string ProjectName
        {
            get
            {
                return _projectName;
            }
            set
            {
                _projectName = value;
                RaisePropertyChanged("ProjectName");
            }
        }

        public DateTime QuoteDate
        {
            get
            {
                return _quoteDate;
            }
            set
            {
                _quoteDate = value;
                RaisePropertyChanged("QuoteDate");
            }
        }

        public User User
        {
            get
            {
                return _user;
            }
            set
            {
                _user = value;
                RaisePropertyChanged("User");
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

        public ObservableCollection<QuoteDetails> QuoteDetails
        {
            get
            {
                return _quoteDetails;
            }
            set
            {
                _quoteDetails = value;
                RaisePropertyChanged("QuoteDetails");
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

        public decimal Gst
        {
            get
            {
                //_gst = GSTActive ? ((TotalAmount + FreightTotal) * 10) / 100 : 0;

                return _gst;
            }
            set
            {
                _gst = value;
                RaisePropertyChanged("Gst");
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

        public decimal TotalBeforeTax
        {
            get
            {
                return _totalBeforeTax;
            }
            set
            {
                _totalBeforeTax = value;
                RaisePropertyChanged("TotalBeforeTax");
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

        public decimal DiscountedTotal
        {
            get
            {
                return _discountedTotal;
            }
            set
            {
                _discountedTotal = value;
                RaisePropertyChanged("DiscountedTotal");
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

        public string Instructions
        {
            get
            {
                return _instructions;
            }
            set
            {
                _instructions = value;
                RaisePropertyChanged("Instructions");
            }
        }

        public string InternalComments
        {
            get
            {
                return _internalComments;
            }
            set
            {
                _internalComments = value;
                RaisePropertyChanged("InternalComments");
            }
        }

        public bool GSTActive
        {
            get
            {
                return _gSTActive;
            }
            set
            {
                _gSTActive = value;
                RaisePropertyChanged("GSTActive");
                //CalculateFinalTotal();
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

        public User LastUpdatedBy
        {
            get
            {
                return _lastUpdatedBy;
            }
            set
            {
                _lastUpdatedBy = value;
                RaisePropertyChanged("LastUpdatedBy");
            }
        }

        //private void CalculateFinalTotal()
        //{
        //    ListPriceTotal = FreightDetails == null ? 0 : QuoteDetails.Sum(x => x.ProductTotalBeforeDis);
        //    DiscountedTotal = FreightDetails == null ? 0 : QuoteDetails.Sum(x => x.DiscountedTotal);
        //    FreightTotal = FreightDetails == null ? 0 : FreightDetails.Sum(x => x.Total);

        //    if (GSTActive)
        //    {
        //        Gst = GSTActive ? (((ListPriceTotal -DiscountedTotal) + FreightTotal) * 10) / 100 : 0;
        //    }
        //    else
        //    {
        //        Gst = 0;
        //    }
        //    TotalBeforeTax = ListPriceTotal - DiscountedTotal;
        //    TotalAmount = (ListPriceTotal - DiscountedTotal) + FreightTotal + Gst;

        //}

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

        public ContactPerson ContactPerson
        {
            get
            {
                return _contactPerson;
            }
            set
            {
                _contactPerson = value;
                RaisePropertyChanged("ContactPerson");
            }
        }

        public string FileName
        {
            get
            {
                return _fileName;
            }
            set
            {
                _fileName = value;
                RaisePropertyChanged("FileName");
            }
        }

        public string QuoteCourierName
        {
            get
            {
                return _quoteCourierName;
            }
            set
            {
                _quoteCourierName = value;
                RaisePropertyChanged("QuoteCourierName");
            }
        }



        public object Clone()
        {
            return MemberwiseClone();
        }

    }
}
