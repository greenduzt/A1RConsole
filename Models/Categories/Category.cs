using A1RConsole.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A1RConsole.Models.Categories
{
    public class Category : ViewModelBase
    {
       

        private List<int> _discounts;
        public List<int> Discounts
        {
            get
            {
                return _discounts;
            }
            set
            {
                _discounts = value;
                RaisePropertyChanged("Discounts");
            }
        }

        private int _categoryID;
        public int CategoryID
        {
            get
            {
                return _categoryID;
            }
            set
            {
                _categoryID = value;
                RaisePropertyChanged("CategoryID");
            }
        }

        private string _categoryName;
        public string CategoryName
        {
            get
            {
                return _categoryName;
            }
            set
            {
                _categoryName = value;
                RaisePropertyChanged("CategoryName");
            }
        }

        private string _documentPath;
        public string DocumentPath
        {
            get
            {
                return _documentPath;
            }
            set
            {
                _documentPath = value;
                RaisePropertyChanged("DocumentPath");
            }
        }
        private string _categoryDescription;
        public string CategoryDescription
        {
            get
            {
                return _categoryDescription;
            }
            set
            {
                _categoryDescription = value;
                RaisePropertyChanged("CategoryDescription");
            }
        }
    }
}