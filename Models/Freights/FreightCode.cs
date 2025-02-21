using A1RConsole.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A1RConsole.Models.Freights
{
    public class FreightCode : ViewModelBase
    {
        public Int32 ID { get; set; }      
        public string Unit { get; set; }     
        private int _freightCodeID;
        private string _code;
        private decimal _price;
        private decimal _freightTotal;
        private bool _priceEnabled;
        private string _timeStamp;
        private bool _active;
        private string _description;

        public decimal Price
        {
            get { return _price; }
            set
            {
                _price = value;
                RaisePropertyChanged("Price");
            }
        }

        public decimal FreightTotal
        {
            get { return _freightTotal; }
            set
            {
                _freightTotal = value;

                RaisePropertyChanged("FreightTotal");
            }
        }

        public bool PriceEnabled
        {
            get { return _priceEnabled; }
            set
            {
                _priceEnabled = value;

                RaisePropertyChanged("PriceEnabled");
            }
        }

        public string Code
        {
            get { return _code; }
            set
            {
                _code = value;

                RaisePropertyChanged("Code");                
            }
        }

        public int FreightCodeID
        {
            get { return _freightCodeID; }
            set
            {
                _freightCodeID = value;

                RaisePropertyChanged("FreightCodeID");
            }
        }

        public string TimeStamp
        {
            get { return _timeStamp; }
            set
            {
                _timeStamp = value;

                RaisePropertyChanged("TimeStamp");
            }
        }

        public bool Active
        {
            get { return _active; }
            set
            {
                _active = value;

                RaisePropertyChanged("Active");
            }
        }

        public string Description
        {
            get { return _description; }
            set
            {
                _description = value;

                RaisePropertyChanged("Description");
                               
            }
        }
        
    }
}
