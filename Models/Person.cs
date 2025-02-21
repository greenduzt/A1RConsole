using A1RConsole.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A1RConsole.Models
{
    public class Person : ViewModelBase
    {
        public event EventHandler Changed;

        public Person(string name, DateTime birthDate, string address)
        {
            this.Name = name;
            this.BirthDate = birthDate;
            this.Address = address;
        }

        private string _name = string.Empty;
        public string Name
        {
            get
            {
                return this._name;
            }
            set
            {
                this._name = value;
                RaisePropertyChanged("Name");
            }
        }

        private DateTime _birthDate;
        public DateTime BirthDate
        {
            get
            {
                return this._birthDate;
            }
            set
            {
                this._birthDate = value;
                RaisePropertyChanged("BirthDate");
            }
        }

        private string _address = string.Empty;
        public string Address
        {
            get
            {
                return this._address;
            }
            set
            {
                this._address = value;
                RaisePropertyChanged("Address");
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
