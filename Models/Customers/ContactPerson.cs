using A1RConsole.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A1RConsole.Models.Customers
{
    public class ContactPerson : ViewModelBase
    {
        private Int32 _contactPersonID;
        private Int32 _customerID;
        private string _contactPersonName;
        private string _phoneNumber1;
        private string _phoneNumber2;
        private string _email;
        //public Int32 ContactPersonID { get; set; }
        //public Int32 CustomerID { get; set; }
        
        //public string PhoneNumber1 { get; set; }
        //public string PhoneNumber2 { get; set; }
        //public string Email { get; set; }
        public bool Active { get; set; }
        public string TimeStamp { get; set; }


        public Int32 ContactPersonID
        {
            get
            {
                return _contactPersonID;
            }
            set
            {
                _contactPersonID = value;
                RaisePropertyChanged("ContactPersonID");
            }
        }

        public Int32 CustomerID
        {
            get
            {
                return _customerID;
            }
            set
            {
                _customerID = value;
                RaisePropertyChanged("CustomerID");
            }
        }


        public string ContactPersonName
        {
            get
            {
                return _contactPersonName;
            }
            set
            {
                _contactPersonName = value;
                RaisePropertyChanged("ContactPersonName");
            }
        }

        public string PhoneNumber1
        {
            get
            {
                return _phoneNumber1;
            }
            set
            {
                _phoneNumber1 = value;
                RaisePropertyChanged("PhoneNumber1");
            }
        }

        public string PhoneNumber2
        {
            get
            {
                return _phoneNumber2;
            }
            set
            {
                _phoneNumber2 = value;
                RaisePropertyChanged("PhoneNumber2");
            }
        }

        public string Email
        {
            get
            {
                return _email;
            }
            set
            {
                _email = value;
                RaisePropertyChanged("Email");
            }
        }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
