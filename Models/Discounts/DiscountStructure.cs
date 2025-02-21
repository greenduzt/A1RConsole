using A1RConsole.Bases;
using A1RConsole.Models.Categories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A1RConsole.Models.Discounts
{
    public class DiscountStructure : ViewModelBase
    {
        public int ID { get; set; }
        public int CustomerID { get; set; }

        public string Comment { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string UpdatedBy { get; set; }
        public string TimeStamp { get; set; }

        private Category _category;
        private string _discountLabelVisibility;
        private string _discountLabelText;
        private int _discount;
        private string _discountStr;

        public string DiscountLabelVisibility
        {
            get
            {
                return _discountLabelVisibility;
            }
            set
            {
                _discountLabelVisibility = value;
                RaisePropertyChanged("DiscountLabelVisibility");
            }
        }

        public string DiscountLabelText
        {
            get
            {
                return _discountLabelText;
            }
            set
            {
                _discountLabelText = value;
                RaisePropertyChanged("DiscountLabelText");
            }
        }

        public Category Category
        {
            get
            {
                return _category;
            }
            set
            {
                _category = value;
                RaisePropertyChanged("Category");
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

            }
        }

        public string DiscountStr
        {
            get
            {
                return _discountStr;
            }
            set
            {
                _discountStr = value;
                RaisePropertyChanged("DiscountStr");

            }
        }

    }
}
