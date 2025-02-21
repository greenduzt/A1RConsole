using A1RConsole.Bases;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A1RConsole.Models.Categories
{
    public class CategoryCollection : ViewModelBase
    {
        private Category _selectedCategory;
        public ObservableCollection<Category> CategoryList { get; set; }
        public CategoryCollection(ObservableCollection<Category> cat)
        {
            CategoryList = cat;
            SelectedCategory = new Category() { CategoryName = "Select" };
        }

        public Category SelectedCategory
        {
            get
            {
                return _selectedCategory;
            }
            set
            {
                _selectedCategory = value;

                RaisePropertyChanged("SelectedCategory");
            }
        }
    }
}
