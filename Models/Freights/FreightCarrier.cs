using A1RConsole.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A1RConsole.Models.Freights
{
    public class FreightCarrier : ViewModelBase
    {
        public int Id { get; set; }
        public decimal FreightPrice { get; set; }
        
        private string _freightDescription;
        public string FreightDescription
        {
            get
            {
                return _freightDescription;
            }
            set
            {
                _freightDescription = value;
                RaisePropertyChanged("FreightDescription");
            }
        }

        private string _freightName;
        public string FreightName
        {
            get
            {
                return _freightName;
            }
            set
            {
                _freightName = value;
                RaisePropertyChanged("FreightName");
            }
        }

        private string _timeStamp;
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

        private bool _active;
        public bool Active
        {
            get
            {
                return _active;
            }
            set
            {
                _active = value;
                RaisePropertyChanged("Active");
            }
        }
    }
}
