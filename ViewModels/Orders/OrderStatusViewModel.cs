using A1RConsole.Bases;
using A1RConsole.Commands;
using A1RConsole.Core;
using A1RConsole.DB;
using A1RConsole.Interfaces;
using A1RConsole.Models.Customers;
using A1RConsole.Models.Discounts;
using A1RConsole.Models.Invoices;
using A1RConsole.Models.Meta;
using A1RConsole.Models.Orders;
using A1RConsole.Models.Users;
using MsgBox;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace A1RConsole.ViewModels.Orders
{
    public class OrderStatusViewModel : ViewModelBase, IContent
    {
        private ObservableCollection<SalesOrder> _salesOrders;
        private ICollectionView _itemsView;
        public string UserName { get; set; }
        public string State { get; set; }
        private bool _isToBeDispatched;
        private bool _isDispatched;
        private bool _isCancelled;
        private bool _isAll;
        private bool _isSearch;
        private bool _isReturned;
        private bool _isHeld;
        private string _searchString;
        public List<UserPrivilages> Privilages { get; set; }
        private List<MetaData> metaData;
        private string _version;
        private bool canExecute;
        private string loadingStatus;
        private List<string> openedChildren;
        private bool IsDirty { get; set; }
        public event EventHandler Closing;
        public RelayCommand CloseCommand { get; private set; }
                
        private ICommand _viewUpdateCancelOrderCommand;
        private ICommand _prepareInvoiceCommand;
        private ICommand _refreshDataCommand;

        public OrderStatusViewModel()
        {
            UserName = UserData.UserName;
            State = UserData.State;
            Privilages = UserData.UserPrivilages;
            metaData = UserData.MetaData;
            openedChildren = new List<string>();

            //var data = metaData.SingleOrDefault(x => x.KeyName == "version");
            //Version = data.Description;
            SalesOrders = new ObservableCollection<SalesOrder>();
            IsToBeDispatched = true;
            canExecute = true;

            this.CloseCommand = new RelayCommand(CloseWindow);
        }

        private void GetSalesOrders()
        {
            if (!string.IsNullOrWhiteSpace(SearchString))
            {
                SearchString = string.Empty;
            }

            //BackgroundWorker worker = new BackgroundWorker();
            //ChildWindowView LoadingScreen = new ChildWindowView();_itemsView
            //LoadingScreen.ShowWaitingScreen("Loading");

            //worker.DoWork += (_, __) =>
            //{
                SalesOrders = SalesOrderManager.LoadSalesOrders(loadingStatus,0);
            //};

            //worker.RunWorkerCompleted += delegate(object s, RunWorkerCompletedEventArgs args)
            //{
            //    LoadingScreen.CloseWaitingScreen();
                _itemsView = CollectionViewSource.GetDefaultView(SalesOrders);
                _itemsView.Filter = x => Filter(x as SalesOrder);
            //};
            //worker.RunWorkerAsync();
        }

        private bool Filter(SalesOrder model)
        {
            var searchstring = (SearchString ?? string.Empty).ToLower();
            return model != null &&
                 ((model.Customer.CompanyName ?? string.Empty).ToLower().Contains(searchstring)) ||
                 ((model.SalesOrderNo).ToString() ?? string.Empty).Contains(searchstring) ||
                 ((model.CustomerOrderNo).ToString() ?? string.Empty).ToLower().Contains(searchstring) ||
                 ((model.DesiredDispatchDate ?? null).ToString() ?? string.Empty).Contains(searchstring);
        }

        public string Title
        {
            get
            {
                return "Order Status";
            }
        }

        private void CloseWindow(object p)
        {
            if (this.CanClose)
            {
                var hander = this.Closing;
                if (hander != null)
                {
                    hander(this, EventArgs.Empty);
                }
            }
        }

        private void ViewUpdateOrder(Object parameter)
        {
            int index = SalesOrders.IndexOf(parameter as SalesOrder);
            if (index >= 0)
            {
                //Check if the sales order still exist
                bool exist = DBAccess.CheckRecordExist(SalesOrders[index].SalesOrderNo, "SalesOrder");

                if (exist)
                {
                    ObservableCollection<Customer> CustomerList = new ObservableCollection<Customer>();
                    BackgroundWorker worker = new BackgroundWorker();
                    ChildWindow LoadingScreen = new ChildWindow();
                    LoadingScreen.ShowWaitingScreen("Loading");
                    worker.DoWork += (_, __) =>
                    {
                        CustomerList=LoadCustomers();
                    };
                    worker.RunWorkerCompleted += delegate(object s, RunWorkerCompletedEventArgs args)
                    {
                        LoadingScreen.CloseWaitingScreen();

                        List<DiscountStructure> discountList = new List<DiscountStructure>();
                        ChildWindow showUpdateSalesScreen = new ChildWindow();
                        showUpdateSalesScreen.ShowUpdateSalesOrderView(UserName, State, Privilages, metaData, SalesOrders[index], discountList, "", CustomerList);
                        showUpdateSalesScreen.updateSalesOrder_Closed += (r =>
                        {
                            //Console.WriteLine(r.ToString());
                            SalesOrders.Clear();
                            IsToBeDispatched = true;
                            GetSalesOrders();
                        });
                    };
                    worker.RunWorkerAsync();                
                }
                else
                {
                    MessageBox.Show("This sales order no longer exist", "Sales Order Does Not Exist", MessageBoxButton.OK, MessageBoxImage.Information);
                    GetSalesOrders();
                }
            }
        }

        private ObservableCollection<Customer> LoadCustomers()
        {          
            return DBAccess.GetCustomerData();
        }

        private void PrepareInvoice(Object parameter)
        {
            ////Print Invoice
            //int index = SalesOrders.IndexOf(parameter as SalesOrder);
            //if (index >= 0)
            //{
            //    Invoice inv = DBAccess.InsertUpdateInvoice(SalesOrders[index].SalesOrderNo, SalesOrders[index].PaymentDueDate);
            //    SalesOrders[index].InvoiceNo = inv.InvoiceNo;
            //    if (SalesOrders[index].InvoiceNo > 0)
            //    {
            //        SalesOrders[index].Invoice.InvoicedDate = inv.InvoicedDate;
            //        var childWindow = new ChildWindow();                   
            //        childWindow.ShowChangeInvoiceDate(SalesOrders[index]);
            //    }
            //    else
            //    {
            //        Msg.Show("Cannot generate invoice number" + System.Environment.NewLine + "Please try again later", "Cannot Generate Invoice Number", MsgBoxButtons.OK, MsgBoxImage.Information_Red, MsgBoxResult.Yes);
            //    }
            //}            

            //Prepare Invoice
            int index = SalesOrders.IndexOf(parameter as SalesOrder);
            if (index >= 0)
            {
                Invoice invoice =DBAccess.GetInvoiceBySalesNo(SalesOrders[index].SalesOrderNo);

                BackgroundWorker worker = new BackgroundWorker();
                ChildWindow loadingScreen = new ChildWindow();
                ChildWindow showInvoicingScreen = new ChildWindow();
                loadingScreen.ShowWaitingScreen("Loading..");
                worker.DoWork += (_, __) =>
                {
                    showInvoicingScreen = new ChildWindow();
                };
                worker.RunWorkerCompleted += delegate(object s, RunWorkerCompletedEventArgs args)
                {
                    invoice.CompletedBy = UserData.UserName;
                    invoice.CompletedDate = DateTime.Now;
                    showInvoicingScreen.ShowInvoicingWindow(invoice);
                    showInvoicingScreen.invoicingView_Closed += (r =>
                    {
                        if (r == 0)
                        {
                            SalesOrders.Clear();
                            GetSalesOrders();
                        }
                    });
                };
                worker.RunWorkerAsync();
            }
        }

        private void GetStatus()
        {
            if (IsToBeDispatched)
            {
                loadingStatus = "IsToBeDispatched";
            }
            else if (IsDispatched)
            {
                loadingStatus = "IsDispatched";
            }
            else if (IsCancelled)
            {
                loadingStatus = "IsCancelled";
            }
            else if (IsReturned)
            {
                loadingStatus = "IsReturned";
            }
            else if (IsAll)
            {
                loadingStatus = "IsAll";
            }
            else if (IsSearch)
            {
                loadingStatus = "Search";
            }
            else if (IsHeld)
            {
                loadingStatus = "IsHeld";
            }
        }

        public bool CanClose
        {
            get
            {
                return this.IsDirty == false || MessageBox.Show("Changes were made. Do you want to close this window?", "Close", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
            }
        }

        private bool CanExecute(object parameter)
        {
            return true;
        }

        public ObservableCollection<SalesOrder> SalesOrders
        {
            get
            {
                return _salesOrders;
            }
            set
            {
                _salesOrders = value;
                RaisePropertyChanged("SalesOrders");
            }
        }

        public string Version
        {
            get
            {
                return _version;
            }
            set
            {
                _version = value;
                RaisePropertyChanged("Version");
            }
        }

        public bool IsToBeDispatched
        {
            get
            {
                return _isToBeDispatched;
            }
            set
            {
                _isToBeDispatched = value;
                RaisePropertyChanged("IsToBeDispatched");
                GetStatus();
                GetSalesOrders();
            }
        }

        public bool IsDispatched
        {
            get
            {
                return _isDispatched;
            }
            set
            {
                _isDispatched = value;
                RaisePropertyChanged("IsDispatched");
                GetStatus();
                GetSalesOrders();
            }
        }

        public bool IsCancelled
        {
            get
            {
                return _isCancelled;
            }
            set
            {
                _isCancelled = value;
                RaisePropertyChanged("IsCancelled");
                GetStatus();
                GetSalesOrders();
            }
        }


        public bool IsAll
        {
            get
            {
                return _isAll;
            }
            set
            {
                _isAll = value;
                RaisePropertyChanged("IsAll");
                GetStatus();
                GetSalesOrders();
            }
        }

        public bool IsSearch
        {
            get
            {
                return _isSearch;
            }
            set
            {
                _isSearch = value;
                RaisePropertyChanged("IsSearch");
                GetStatus();
                GetSalesOrders();
            }
        }

        public bool IsReturned
        {
            get
            {
                return _isReturned;
            }
            set
            {
                _isReturned = value;
                RaisePropertyChanged("IsReturned");
                GetStatus();
                GetSalesOrders();
            }
        }

        public string SearchString
        {
            get { return _searchString; }
            set
            {
                _searchString = value;
                RaisePropertyChanged("SearchString");
                ItemsView.Refresh();
                //GetSalesOrders();

            }
        }
        
        public bool IsHeld
        {
            get
            {
                return _isHeld;
            }
            set
            {
                _isHeld = value;
                RaisePropertyChanged("IsHeld");
                GetStatus();
                GetSalesOrders();
            }
        }

        public ICollectionView ItemsView
        {
            get { return _itemsView; }
        }


        #region COMMANDS     
    

        public ICommand RefreshDataCommand
        {
            get
            {
                return _refreshDataCommand ?? (_refreshDataCommand = new CommandHandler(() => GetSalesOrders(), canExecute));
            }
        }

        public ICommand ViewUpdateCancelOrderCommand
        {
            get
            {
                if (_viewUpdateCancelOrderCommand == null)
                {
                    _viewUpdateCancelOrderCommand = new DelegateCommand(CanExecute, ViewUpdateOrder);
                }
                return _viewUpdateCancelOrderCommand;
            }
        }

        public ICommand PrepareInvoiceCommand
        {
            get
            {
                if (_prepareInvoiceCommand == null)
                {
                    _prepareInvoiceCommand = new DelegateCommand(CanExecute, PrepareInvoice);
                }
                return _prepareInvoiceCommand;
            }
        }

        #endregion
    }
}
