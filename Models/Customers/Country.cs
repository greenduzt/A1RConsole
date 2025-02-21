using A1RConsole.Bases;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace A1RConsole.Models.Customers
{
    public class Country : ViewModelBase
    {
        private int _iD;
        private string _countryName;
        private bool _isFavourite;
        private bool _isEnabled;

        public bool IsEnabled
        {
            get
            {
                return _isEnabled;
            }
            set
            {
                _isEnabled = value;
                RaisePropertyChanged("IsEnabled");
            }
        }

        public int ID
        {
            get
            {
                return _iD;
            }
            set
            {
                _iD = value;
                RaisePropertyChanged("ID");
            }
        }

        public string CountryName
        {
            get
            {
                return _countryName;
            }
            set
            {
                  _countryName = value;
                RaisePropertyChanged("CountryName");
            }
        }

        public bool IsFavourite
        {
            get
            {
                return _isFavourite;
            }
            set
            {
                _isFavourite = value;
                RaisePropertyChanged("IsFavourite");
            }
        }
    }
}
