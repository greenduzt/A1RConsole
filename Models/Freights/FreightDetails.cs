using A1RConsole.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A1RConsole.Models.Freights
{
    public class FreightDetails : ViewModelBase//, IEquatable<FreightDetails>
    {
        public string TimeStamp { get; set; }
        public Int32 SalesNo { get; set; }
        private bool _freightCodeEnabled;
        private bool _freightPriceReadOnly;
        private FreightCode _freightCodeDetails;
        private decimal _total;
        private decimal _pallets;
        private string _palletsStr;
        private int _discount;
        private bool _disEnable;
        private string _dummyDescription;
        private decimal _dummyPrice;
        private int _prefix;
        //public FreightDetails() {  }

        private void CalculateTotal()
        {
            if (FreightCodeDetails != null)
            {
                decimal subTotal = 0;
                decimal disTotal = 0;

                subTotal = Pallets * FreightCodeDetails.Price;
                disTotal = (subTotal * Discount) / 100;
                Total = subTotal - disTotal;
                              
            }
        }


        public FreightCode FreightCodeDetails
        {
            get
            {
                return _freightCodeDetails;
            }
            set
            {
                _freightCodeDetails = value;
                RaisePropertyChanged("FreightCodeDetails");

                CalculateTotal();

                if (FreightCodeDetails != null)
                {
                    //if (DummyPrice == 0 && FreightCodeDetails.Code != "Select")
                    //{
                    //    FreightCodeDetails.PriceEnabled = true;
                    //    FreightPriceReadOnly = false;
                    //}
                    //else
                    //{
                    //    FreightPriceReadOnly = true;
                    //    FreightCodeDetails.PriceEnabled = false;
                    //}

                    if (FreightCodeDetails != null && (FreightCodeDetails.ID != 37 && FreightCodeDetails.ID != 70))
                    {
                        DisEnable = false;
                        Discount = 0;
                    }
                    else
                    {
                        DisEnable = true;
                    }
                }
            }
        }

        public int Prefix
        {
            get
            {
                return _prefix;
            }
            set
            {
                _prefix = value;
                RaisePropertyChanged("Prefix");
            }
        }

        public string DummyDescription
        {
            get
            {
                return _dummyDescription;
            }
            set
            {
                _dummyDescription = value;
                RaisePropertyChanged("DummyDescription");
            }
        }

        public decimal DummyPrice
        {
            get
            {
                return _dummyPrice;
            }
            set
            {
                _dummyPrice = value;
                RaisePropertyChanged("DummyPrice");
            }
        }

        

        public bool FreightPriceReadOnly
        {
            get
            {
                return _freightPriceReadOnly;
            }
            set
            {
                _freightPriceReadOnly = value;
                RaisePropertyChanged("FreightPriceReadOnly");
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
                RaisePropertyChanged("this.Total");
            }
        }

        public string PalletsStr
        {
            get
            {
                return _palletsStr;
            }
            set
            {
                _palletsStr = value;
                RaisePropertyChanged("PalletsStr");

                if (String.IsNullOrWhiteSpace(PalletsStr))
                {
                    Pallets = 0;
                }
                else
                {
                    Pallets = Convert.ToDecimal(PalletsStr);
                }
            }
        }

        public decimal Pallets
        {
            get
            {
                
                return _pallets;
            }
            set
            {
                _pallets = value;
                RaisePropertyChanged("Pallets");
                
                CalculateTotal();
                if (Pallets > 0)
                {
                    FreightCodeEnabled = true;
                }
                else
                {
                    FreightCodeEnabled = false;
                    //FreightCodeDetails = null;
                }
            }
        }

        public bool FreightCodeEnabled
        {
            get
            {
                return _freightCodeEnabled;
            }
            set
            {
                _freightCodeEnabled = value;
                RaisePropertyChanged("FreightCodeEnabled");
            }
        }

        public bool DisEnable
        {
            get
            {
                return _disEnable;
            }
            set
            {
                _disEnable = value;
                RaisePropertyChanged("DisEnable");
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

                RaisePropertyChanged("Discount");
                if (Discount > 100)
                {
                    Discount = 100;
                }
                else if (Discount < 0)
                {
                    Discount = 0;
                }

               

                CalculateTotal();
            }
        }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}

