using A1RConsole.Bases;
using A1RConsole.Core;
using A1RConsole.DB;
using A1RConsole.Models.Categories;
using A1RConsole.Models.Discounts;
using A1RConsole.Models.Orders;
using Microsoft.Practices.Prism.Commands;
using MsgBox;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;


namespace A1RConsole.ViewModels.Discounts
{
    public class DiscountViewModel : ViewModelBase
    {
        private int customerId;
        private List<Category> _categories;
        private ObservableCollection<DiscountStructure> _discountStructure;
        private bool canExecute;
        public event Action<ObservableCollection<DiscountStructure>> Closed;
        private DelegateCommand _closeCommand;
        private ICommand _submitCommand;
        private ICommand _selectionChangedCommand;

        public DiscountViewModel(int cid, ObservableCollection<DiscountStructure> dsL)
        {
            customerId = cid;
            canExecute = true;
            DiscountStructure = new ObservableCollection<DiscountStructure>();
            ObservableCollection<DiscountStructure> ds = new ObservableCollection<DiscountStructure>();
            Categories = new List<Category>();
            Categories=DBAccess.GetCategories();
            
            ds = dsL;

            var data = UserData.MetaData.FirstOrDefault(x => x.KeyName.Equals("customer_exclusive_discount"));
            if (data != null)
            {
                string[] str1 = data.Description.Split('|');//"1840[2:55]|4871[3:65]|563[3:65]|539[2:55,11:45]".Split('|');
                if (str1 != null)
                {
                    foreach (var item in str1)
                    {
                        string[] str2 = item.Split('[');
                        if (str2 != null && int.Parse(str2[0]) == customerId)
                        {
                            string[] str3 = str2[1].Replace("]", string.Empty).Split(',');
                            if (str3 != null)
                            {
                                foreach (var item1 in str3)
                                {
                                    string[] str4 = item1.Split(':');
                                    if (str4 != null)
                                    {
                                        var cat = Categories.FirstOrDefault(x => x.CategoryID == int.Parse(str4[0]));
                                        if (cat != null)
                                        {
                                            cat.Discounts.Add(int.Parse(str4[1]));
                                            cat.Discounts = cat.Discounts.OrderByDescending(x => x).ToList();
                                        }
                                    }
                                }
                            }
                            break;
                        }
                    }
                }

            }

            foreach (var item in Categories)
            {
                if (item.CategoryID != 8 && item.CategoryID != 9)
                {                        
                    var z = ds !=null ? ds.FirstOrDefault(x => x.Category.CategoryID == item.CategoryID) : null;

                    if (z != null)
                    {
                        DiscountStructure.Add(new DiscountStructure()
                        {
                            CustomerID = cid,
                            Category = new Category()
                            {
                                CategoryID = item.CategoryID,
                                CategoryName = item.CategoryName,
                                CategoryDescription = item.CategoryDescription,
                                DocumentPath = item.DocumentPath,
                                Discounts = item.Discounts
                            },
                            UpdatedBy = UserData.FirstName + " " + UserData.LastName,
                            UpdatedDate = DateTime.Now,
                            Discount = z.Discount,
                            DiscountLabelVisibility = z.DiscountLabelVisibility,
                            TimeStamp = z.TimeStamp
                        });
                    }
                    else
                    {
                        DiscountStructure.Add(new DiscountStructure()
                        {
                            CustomerID = cid,
                            Category = new Category()
                            {
                                CategoryID = item.CategoryID,
                                CategoryName = item.CategoryName,
                                CategoryDescription = item.CategoryDescription,
                                DocumentPath = item.DocumentPath,
                                Discounts = item.Discounts
                            },
                            UpdatedBy = UserData.FirstName + " " + UserData.LastName,
                            UpdatedDate = DateTime.Now,
                            Discount = 0,
                            DiscountLabelVisibility = "Collapsed"
                        });
                    }

                }
            }            

            _closeCommand = new DelegateCommand(CloseFormNoParam);
        }

        private void SubmitSupplier()
        {
            if (customerId > 0)
            {
                CloseForm();
            }
            else
            {
                CloseForm();
            }
        }

        private void CloseForm()
        {
            if (Closed != null)
            {   
                Closed(DiscountStructure);               
            }
        }

        private void CloseFormNoParam()
        {
            if (Closed != null)
            {
                Closed(new ObservableCollection<DiscountStructure>());
            }
        }

        private void DiscountChanged()
        {
            foreach (var item in DiscountStructure)
            {
                if (item.Category.CategoryID == 3)
                {
                    if (item.Discount == 60)
                    {
                        item.DiscountLabelVisibility = "Visible";
                    }
                    else if (item.Discount == 58)
                    {
                        item.DiscountLabelVisibility = "Visible";
                    }
                    else
                    {
                        item.DiscountLabelVisibility = "Collapsed";
                    }
                }
                else
                {
                    item.DiscountLabelVisibility = "Collapsed";
                }
            }
        }

        #region PUBLIC_PROPERTIES

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

        public List<Category> Categories
        {
            get
            {
                return _categories;
            }
            set
            {
                _categories = value;
                RaisePropertyChanged("Categories");
            }
        }

        #endregion

        public DelegateCommand CloseCommand
        {
            get { return _closeCommand; }
        }

        public ICommand SubmitCommand
        {
            get
            {
                return _submitCommand ?? (_submitCommand = new A1RConsole.Commands.CommandHandler(() => SubmitSupplier(), canExecute));
            }
        }

        public ICommand SelectionChangedCommand
        {
            get
            {
                return _selectionChangedCommand ?? (_selectionChangedCommand = new A1RConsole.Commands.CommandHandler(() => DiscountChanged(), canExecute));
            }
        }
    }
}
