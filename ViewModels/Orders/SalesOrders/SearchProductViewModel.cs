using A1RConsole.Bases;
using A1RConsole.Commands;
using A1RConsole.Core;
using A1RConsole.DB;
using A1RConsole.Models.Customers;
using A1RConsole.Models.Discounts;
using A1RConsole.Models.Meta;
using A1RConsole.Models.Orders;
using A1RConsole.Models.Products;
using A1RConsole.Models.Users;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;

namespace A1RConsole.ViewModels.Orders.SalesOrders
{
    public class SearchProductViewModel : ViewModelBase
    {
        private string viewToOpen;
        private string openType;
        private string userName;
        private string state;
        private List<UserPrivilages> privilages;
        private List<MetaData> metaData;
       
        private List<DiscountStructure> discountList;
        private SalesOrder salesOrder;
        public event Action<Product> Closed;
        List<Product> _product;
        private string _searchString;
        private ICollectionView _itemsView;
        private ICommand _closeCommand;
        private ICommand _addProductCommand;

        //public SearchProductViewModel(A1QSystem.PageSwitcher ps)
        public SearchProductViewModel(string un, string s, List<UserPrivilages> p, List<MetaData> md,  SalesOrder so, List<DiscountStructure> dsList, string ot)
        {
            userName = un;
            state = s;
            privilages = p;
            metaData = md;           
            discountList = dsList;
            salesOrder = so;
            openType = ot;
            _closeCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(CloseForm);
            Products = new List<Product>(DBAccess.GetAllProds(false));
            _itemsView = CollectionViewSource.GetDefaultView(Products);
            _itemsView.Filter = x => Filter(x as Product);
        }


        private void CloseForm()
        {
            Product p = new Product();
            Closed(p);
        }


        private bool Filter(Product model)
        {
            var searchstring = (SearchString ?? string.Empty).ToLower();
            return model != null &&
                 ((model.ProductDescription ?? string.Empty).ToLower().Contains(searchstring)) ||
                 ((model.CommodityCode ?? string.Empty).ToLower().Contains(searchstring));
        }

        //private void Close()
        //{
        //    for (int i = 0; i < pageSwitcher.Container.Children.Count; i++)
        //    {
        //        if (pageSwitcher.Container.Children[i].Name == "SearchProductCode")
        //        {
        //            pageSwitcher.Container.Children[i].Close();
        //        }
        //    }
        //}

        private void AddProduct(object parameter)
        {
            int index = Products.IndexOf(parameter as Product);
            if (index > -1 && index < Products.Count)
            {
                ObservableCollection<Customer> CustomerList = new ObservableCollection<Customer>();
                BackgroundWorker worker = new BackgroundWorker();
                ChildWindow LoadingScreen = new ChildWindow();
                LoadingScreen.ShowWaitingScreen("Loading");
                worker.DoWork += (_, __) =>
                {
                    if (openType != "ProductMaintenance")
                    {
                        CustomerList = LoadCustomers();
                    }
                };
                worker.RunWorkerCompleted += delegate(object s, RunWorkerCompletedEventArgs args)
                {
                    LoadingScreen.CloseWaitingScreen();


                    if (openType == "New" || openType == "ProductMaintenance")
                    {
                        Closed(Products[index]);
                    }
                    else
                    {
                        

                            foreach (var item in salesOrder.SalesOrderDetails)
                            {
                                if (item.IsEditing)
                                {
                                    item.Product = Products[index];
                                    item.IsEditing = false;
                                }
                            }

                            ChildWindow showUpdateSalesScreen = new ChildWindow();
                            showUpdateSalesScreen.ShowUpdateSalesOrderView(userName, state, privilages, metaData, salesOrder, discountList, "SearchProduct", CustomerList);
                            showUpdateSalesScreen.updateSalesOrder_Closed += (r =>
                            {
                                //Console.WriteLine(r.ToString());
                                //SalesOrders.Clear();
                                //IsToBeDispatched = true;
                            });
                        
                    }

                };
                worker.RunWorkerAsync();
            }
        }

        private ObservableCollection<Customer> LoadCustomers()
        {
            return DBAccess.GetCustomerData();
        }

        private bool CanExecute(object parameter)
        {
            return true;
        }

        public string SearchString
        {
            get { return _searchString; }
            set
            {
                _searchString = value;
                RaisePropertyChanged("SearchString");
                ItemsView.Refresh();

            }
        }

        public ICollectionView ItemsView
        {
            get { return _itemsView; }
        }

        public List<Product> Products
        {
            get
            {
                return _product;
            }
            set
            {
                _product = value;
                RaisePropertyChanged("Products");
            }
        }

        #region Commands

        public ICommand CloseCommand
        {
            get
            {
                return _closeCommand ?? (_closeCommand = new CommandHandler(() => CloseForm(), true));
            }
        }



        public ICommand AddProductCommand
        {
            get
            {
                if (_addProductCommand == null)
                {
                    _addProductCommand = new DelegateCommand(CanExecute, AddProduct);
                }
                return _addProductCommand;
            }
        }

        #endregion
    }
}
