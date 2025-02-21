using A1RConsole.Bases;
using A1RConsole.Models.Categories;
using A1RConsole.Models.Discounts;
using A1RConsole.Models.Products;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A1RConsole.Models.Customers
{
    public class Customer : ViewModelBase
    {
        public int CustomerId { get; set; }        
        public string CompanyAddress { get; set; }
        public string CompanyCity { get; set; }
        public string CompanyState { get; set; }
        public string CompanyPostCode { get; set; }
        public string CompanyCountry { get; set; }
        public string CompanyEmail { get; set; }
        public string CompanyTelephone { get; set; }
        public string CompanyFax { get; set; }
        public string Designation1 { get; set; }
        public string FirstName1 { get; set; }
        public string LastName1 { get; set; }
        public string Telephone1 { get; set; }
        public string Mobile1 { get; set; }
        public string Fax1 { get; set; }
        public string Email1 { get; set; }
        public string Designation2 { get; set; }
        public string FirstName2 { get; set; }
        public string LastName2 { get; set; }
        public string Telephone2 { get; set; }
        public string Mobile2 { get; set; }
        public string Fax2 { get; set; }
        public string Email2 { get; set; }
        public string Designation3 { get; set; }
        public string FirstName3 { get; set; }
        public string LastName3 { get; set; }
        public string Telephone3 { get; set; }
        public string Mobile3 { get; set; }
        public string Fax3 { get; set; }
        public string Email3 { get; set; }
        public decimal CreditLimit { get; set; }
        public decimal CreditRemaining { get; set; }
        public decimal Debt { get; set; }
        public decimal CreditOwed { get; set; }      
        
        public string LastUpdatedBy { get; set; }
        public DateTime LastUpdatedDateTime { get; set; }
        public bool Active { get; set; }
        public string TimeStamp { get; set; }
        public CustomerCreditHistory CustomerCreditHistory { get; set; }
        

        private string _customerType;
        public string CustomerType
        {
            get
            {
                return _customerType;
            }
            set
            {
                _customerType = value;
                RaisePropertyChanged("CustomerType");
            }
        }

        private string _companyName;
        public string CompanyName
        {
            get
            {
                return _companyName;
            }
            set
            {
                _companyName = value;
                RaisePropertyChanged("CompanyName");
            }
        }

        private ObservableCollection<DiscountStructure> _discountStructure;
        public ObservableCollection<DiscountStructure> DiscountStructure
        {
            get
            {
                return _discountStructure;
            }
            set
            {
                _discountStructure = value;
                RaisePropertyChanged("DiscountStructure");
            }
        }

        private string _stopCredit;
        public string StopCredit
        {
            get
            {
                return _stopCredit;
            }
            set
            {
                _stopCredit = value;
                RaisePropertyChanged("StopCredit");
            }
        }

        private ObservableCollection<CustomerNote> _customerNotes;
        public ObservableCollection<CustomerNote> CustomerNotes
        {
            get
            {
                return _customerNotes;
            }
            set
            {
                _customerNotes = value;
                RaisePropertyChanged("CustomerNotes");
            }
        }

        private Category _primaryBusiness;
        public Category PrimaryBusiness
        {
            get
            {
                return _primaryBusiness;
            }
            set
            {
                _primaryBusiness = value;
                RaisePropertyChanged("PrimaryBusiness");
            }
        }

        private ObservableCollection<ProductType> _productsInstalled;
        public ObservableCollection<ProductType> ProductsInstalled
        {
            get
            {
                return _productsInstalled;
            }
            set
            {
                _productsInstalled = value;
                RaisePropertyChanged("ProductsInstalled");
            }
        }

        private string _shipAddress;
        public string ShipAddress
        {
            get
            {
                return _shipAddress;
            }
            set
            {
                _shipAddress = value;
                RaisePropertyChanged("ShipAddress");
            }
        }


        private string _shipCity;
        public string ShipCity
        {
            get
            {
                return _shipCity;
            }
            set
            {
                _shipCity = value;
                RaisePropertyChanged("ShipCity");
            }
        }

        private string _shipState;
        public string ShipState
        {
            get
            {
                return _shipState;
            }
            set
            {
                _shipState = value;
                RaisePropertyChanged("ShipState");
            }
        }

        private string _shipPostCode;
        public string ShipPostCode
        {
            get
            {
                return _shipPostCode;
            }
            set
            {
                _shipPostCode = value;
                RaisePropertyChanged("ShipPostCode");
            }
        }

        private string _shipCountry;
        public string ShipCountry
        {
            get
            {
                return _shipCountry;
            }
            set
            {
                _shipCountry = value;
                RaisePropertyChanged("ShipCountry");
            }
        }


        private List<ContactPerson> _contactPerson;
        public List<ContactPerson> ContactPerson
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

       
        

    }
}
