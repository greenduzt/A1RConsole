using A1RConsole.Bases;
using A1RConsole.Core;
using A1RConsole.DB;
using A1RConsole.Interfaces;
using A1RConsole.Models.Categories;
using A1RConsole.Models.Discounts;
using A1RConsole.Models.Orders;
using Microsoft.Practices.Prism.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace A1RConsole.ViewModels.Discounts
{
    public class DiscountNoOverlayViewModel : ViewModelBase
    {
        private int customerId;
        private int _commercial;
        private int _fitness;
        private int _acoustic;
        private int _recreational;
        private int _sports;
        private int _general;
        private int _adheisives;
        private int _animal;
        private bool canExecute;
        private SalesOrder salesOrder;
        private List<DiscountStructure> discountList;

        public event EventHandler RequestClose;
        private DelegateCommand _closeCommand;
        private ICommand _submitCommand;

        public DiscountNoOverlayViewModel(int cid, SalesOrder so)
        {
            canExecute = true;
            customerId = cid;
            salesOrder = so;
            discountList = new List<DiscountStructure>();
            List<DiscountStructure> ds = DBAccess.GetDiscount(cid);
            foreach (var item in ds)
            {
                if (item.Category.CategoryID == 1)
                {
                    Commercial = Convert.ToInt16(item.Discount);
                }
                else if (item.Category.CategoryID == 2)
                {
                    Fitness = Convert.ToInt16(item.Discount);
                }
                else if (item.Category.CategoryID == 3)
                {
                    Acoustic = Convert.ToInt16(item.Discount);
                }
                else if (item.Category.CategoryID == 4)
                {
                    Recreational = Convert.ToInt16(item.Discount);
                }
                else if (item.Category.CategoryID == 5)
                {
                    Sports = Convert.ToInt16(item.Discount);
                }
                else if (item.Category.CategoryID == 6)
                {
                    General = Convert.ToInt16(item.Discount);
                }
                else if (item.Category.CategoryID == 7)
                {
                    Adheisives = Convert.ToInt16(item.Discount);
                }
                else if (item.Category.CategoryID == 10)
                {
                    Animal = Convert.ToInt16(item.Discount);
                }
            }
            _closeCommand = new DelegateCommand(OnRequestClose);
        }

        private void SubmitSupplier()
        {
            discountList.Add(new DiscountStructure() { CustomerID = customerId, Category = new Category() { CategoryID = 1 }, Discount = Commercial, UpdatedDate = DateTime.Now, UpdatedBy = UserData.UserName });
            discountList.Add(new DiscountStructure() { CustomerID = customerId, Category = new Category() { CategoryID = 2 }, Discount = Fitness, UpdatedDate = DateTime.Now, UpdatedBy = UserData.UserName });
            discountList.Add(new DiscountStructure() { CustomerID = customerId, Category = new Category() { CategoryID = 3 }, Discount = Acoustic, UpdatedDate = DateTime.Now, UpdatedBy = UserData.UserName });
            discountList.Add(new DiscountStructure() { CustomerID = customerId, Category = new Category() { CategoryID = 4 }, Discount = Recreational, UpdatedDate = DateTime.Now, UpdatedBy = UserData.UserName });
            discountList.Add(new DiscountStructure() { CustomerID = customerId, Category = new Category() { CategoryID = 5 }, Discount = Sports, UpdatedDate = DateTime.Now, UpdatedBy = UserData.UserName });
            discountList.Add(new DiscountStructure() { CustomerID = customerId, Category = new Category() { CategoryID = 6 }, Discount = General, UpdatedDate = DateTime.Now, UpdatedBy = UserData.UserName });
            discountList.Add(new DiscountStructure() { CustomerID = customerId, Category = new Category() { CategoryID = 7 }, Discount = Adheisives, UpdatedDate = DateTime.Now, UpdatedBy = UserData.UserName });
            discountList.Add(new DiscountStructure() { CustomerID = customerId, Category = new Category() { CategoryID = 10 }, Discount = Animal, UpdatedDate = DateTime.Now, UpdatedBy = UserData.UserName });

            int res1 = DBAccess.InsertUpdateDiscount(discountList);
            if (res1 > 0)
            {
                //string s = string.Empty;

                //foreach (var item in salesOrder.SalesOrderDetails)
                //{
                //    foreach (var items in discountList)
                //    {
                //        if (items.Discount != item.Discount && item.Product.Category.CategoryID == items.Category.CategoryID)
                //        {
                //            s += item.Product.ProductDescription + System.Environment.NewLine;
                //        }
                //    }
                //}

                //if (!String.IsNullOrWhiteSpace(s))
                //{
                    MessageBox.Show("Discount(s) updated successfully", "Discount(s) Updated", MessageBoxButton.OK, MessageBoxImage.Information);
                //}
                //else
                //{
                //    //If SalesOrder is in update mode
                //    if (salesOrder.SalesOrderNo > 0)
                //    {
                //        if (MessageBox.Show("Discount(s) updated successfully" + System.Environment.NewLine + "Would you like to add this discount to the selected products?" + System.Environment.NewLine + System.Environment.NewLine + s, "Discount(s) Updated", MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.No)
                //        {
                //            discountList.Clear();
                //            //int res2 = DBAccess.UpdateSalesOrderDiscount(dsList, salesOrder);
                //            //if (res2 == 0)
                //            //{
                //            //    Msg.Show("No changes were made", "No Changes Were Made", MsgBoxButtons.OK, MsgBoxImage.Information);
                //            //}
                //        }
                //    }
                //}                
            }
            else if (res1 == 0)
            {
                MessageBox.Show("You haven't made any changes", "No Changes Were Made", MessageBoxButton.OK, MessageBoxImage.Information);                
            }
            OnRequestClose();
        }

        protected void OnRequestClose()
        {
            EventHandler handler = this.RequestClose;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        #region PUBLIC_PROPERTIES

        public int Commercial
        {
            get
            {
                return _commercial;
            }
            set
            {
                _commercial = value;
                RaisePropertyChanged("Commercial");
            }
        }

        public int Fitness
        {
            get
            {
                return _fitness;
            }
            set
            {
                _fitness = value;
                RaisePropertyChanged("Fitness");
            }
        }

        public int Acoustic
        {
            get
            {
                return _acoustic;
            }
            set
            {
                _acoustic = value;
                RaisePropertyChanged("Acoustic");
            }
        }

        public int Recreational
        {
            get
            {
                return _recreational;
            }
            set
            {
                _recreational = value;
                RaisePropertyChanged("Recreational");
            }
        }

        public int Sports
        {
            get
            {
                return _sports;
            }
            set
            {
                _sports = value;
                RaisePropertyChanged("Sports");
            }
        }

        public int General
        {
            get
            {
                return _general;
            }
            set
            {
                _general = value;
                RaisePropertyChanged("General");
            }
        }

        public int Adheisives
        {
            get
            {
                return _adheisives;
            }
            set
            {
                _adheisives = value;
                RaisePropertyChanged("Adheisives");
            }
        }

        public int Animal
        {
            get
            {
                return _animal;
            }
            set
            {
                _animal = value;
                RaisePropertyChanged("Animal");
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
    }
}
