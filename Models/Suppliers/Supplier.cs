using A1RConsole.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A1RConsole.Models.Suppliers
{
    public class Supplier : ViewModelBase
    {
        
        public string SupplierCode { get; set; }
        public string SupplierAddress { get; set; }
        public string SupplierSuburb { get; set; }
        public string SupplierPostCode { get; set; }
        public string SupplierState { get; set; }
        public string SupplierCountry { get; set; }
        public string SupplierTelephone { get; set; }
        public string SupplierFax { get; set; }
        public string SupplierEmail { get; set; }
        public string SupplierWebUrl { get; set; }
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
        public int LeadTime { get; set; }
        public string LastUpdatedBy { get; set; }
        public DateTime LastUpdatedDateTime { get; set; }
        public string LastUpdatedString { get; set; }
        public bool Active { get; set; }
        public string TimeStamp { get; set; }

        private int _supplierID;
        private string _supplierName;

        public string SupplierName
        {
            get
            {
                return _supplierName;
            }
            set
            {
                _supplierName = value;
                RaisePropertyChanged("SupplierName");
            }
        }

        public int SupplierID
        {
            get
            {
                return _supplierID;
            }
            set
            {
                _supplierID = value;
                RaisePropertyChanged("SupplierID");
            }
        }
    }
}
