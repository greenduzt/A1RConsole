using A1RConsole.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A1RConsole.Models.Products
{
    public class ProductType : ViewModelBase
    {
        //public Int16 ProductTypeID { get; set; }
        //public string Type { get; set; }
        //public string Description { get; set; }

        private Int16 _productTypeID;
        public Int16 ProductTypeID
        {
            get
            {
                return _productTypeID;
            }
            set
            {
                _productTypeID = value;
                RaisePropertyChanged("ProductTypeID");
            }
        }


        private string _type;
        public string Type
        {
            get
            {
                return _type;
            }
            set
            {
                _type = value;
                RaisePropertyChanged("Type");
            }
        }

        private string _description;
        public string Description
        {
            get
            {
                return _description;
            }
            set
            {
                _description = value;
                RaisePropertyChanged("Description");
            }
        }
    }
}
