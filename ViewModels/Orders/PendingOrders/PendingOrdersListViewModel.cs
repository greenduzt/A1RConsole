using A1RConsole.Bases;
using A1RConsole.Commands;
using A1RConsole.Core;
using A1RConsole.DB;
using A1RConsole.Interfaces;
using A1RConsole.Models.Comments;
using A1RConsole.Models.Customers;
using A1RConsole.Models.Freights;
using A1RConsole.Models.Orders;
using A1RConsole.Models.Orders.OnlineOrders;
using A1RConsole.Models.Products;
using A1RConsole.Models.Quoting;
using A1RConsole.Models.Stock;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace A1RConsole.ViewModels.Orders.PendingOrders
{
    public class PendingOrdersListViewModel : ViewModelBase, IContent
    {
        private int _selectedTab;
        private List<ProductStock> productStockList;
        private ObservableCollection<Customer> customerList;
        private ObservableCollection<OnlineOrder> _onlineOrderList;
        private ObservableCollection<PendingQuote> _pendingQuoteList;
        private bool IsDirty { get; set; }
        public event EventHandler Closing;
        //public RelayCommand CloseCommand { get; private set; }
        //public OnlineOrdersNotifier onlineOrdersNotifier { get; set; }
        private Microsoft.Practices.Prism.Commands.DelegateCommand _closeCommand;
        private ICommand _processCommand;
        private ICommand _deleteQuoteCommand;
        private ICommand _createSalesOrderCommand;
        private ICommand _refreshPendingQuotesCommand;
        private ICommand _refreshOnlineOrdersCommand;
        private ICommand _deleteOnlineOrderCommand;

        public PendingOrdersListViewModel()
        {
            OnlineOrderList = new ObservableCollection<OnlineOrder>();
            PendingQuoteList = new ObservableCollection<PendingQuote>();
            SelectedTab = 0;
            productStockList = new List<ProductStock>();
            customerList = new ObservableCollection<Customer>();

            //this.onlineOrdersNotifier = new OnlineOrdersNotifier();
            //this.onlineOrdersNotifier.NewMessage += new EventHandler<SqlNotificationEventArgs>(notifier_NewMessage);
            //ObservableCollection<OnlineOrder> opd = this.onlineOrdersNotifier.RegisterDependency();
            //LoadOnlineOrders(opd);
            _closeCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(CloseWindow);          
           
        }

        public string Title
        {
            get
            {
                return "Pending Orders";
            }
        }

        private void CloseWindow()
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

        //void notifier_NewMessage(object sender, SqlNotificationEventArgs e)
        //{
        //    LoadOnlineOrders(this.onlineOrdersNotifier.RegisterDependency());
        //}

        private void LoadOnlineOrders()
        {
            OnlineOrderList = DBAccess.GetAllOnlineOrders();
        }

        private void LoadPendingQuotes()
        {
            PendingQuoteList =DBAccess.GetPendingQuotesToSale();
        }


        private void DeleteOnlineOrder(Object parameter)
        {
            int index = OnlineOrderList.IndexOf(parameter as OnlineOrder);
            if (index >= 0)
            {
                bool exist = DBAccess.CheckRecordExist(OnlineOrderList[index].OnlineOrderNo, "Online");
                if (exist)
                {
                    if (MessageBox.Show("Are you sure you want to delete this order?" + System.Environment.NewLine + "Once deleted cannot revert back", "Deleting Order Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        int res = DBAccess.DeleteOnlineOrder(OnlineOrderList[index].OnlineOrderNo);
                        if(res ==1)
                        {
                            MessageBox.Show("Order deleted successfully!", "", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show("There has been a problem when deleting this order" + System.Environment.NewLine + "Please try again later", "Cannot Delete Order", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    OnlineOrderList.Clear();
                    LoadOnlineOrders();
                }
                else
                {
                    MessageBox.Show("This online order no longer exist", "Online Order Does Not Exist", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadOnlineOrders();
                }
            }
        }

        private void DeleteQuote(Object parameter)
        {
            int index = PendingQuoteList.IndexOf(parameter as PendingQuote);
            if (index >= 0)
            {
                bool exist = DBAccess.CheckRecordExist(PendingQuoteList[index].QuoteNo, "Quote");
                if (exist)
                {
                    if (MessageBox.Show("Are you sure you want to delete this quote?" + System.Environment.NewLine + "Once deleted cannot revert back", "Deleting Order Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        int res = DBAccess.DeletePendingQuote(PendingQuoteList[index].QuoteNo);
                        if(res ==1)
                        {
                            MessageBox.Show("Quote deleted successfully!", "", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show("There has been a problem when deleting this quote" + System.Environment.NewLine + "Please try again later", "Cannot Delete Quote", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    PendingQuoteList.Clear();
                    LoadPendingQuotes();
                }
                else
                {
                    MessageBox.Show("This quote no longer exist", "Quote Does Not Exist", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadPendingQuotes();
                }
            }
        }
             

        private void LoadProductStock()
        {
            productStockList = DBAccess.GetProductStockByStock(1);//Get product stock
        }

        private void LoadCustomerList()
        {
            customerList = DBAccess.GetCustomerData();
        }

        private Tuple<bool,Customer> SearchCustomer(string companyName)
        {
            LoadCustomerList();
            Customer customer = new Customer();
            bool customerFound = false;            
               
            //BackgroundWorker worker = new BackgroundWorker();
            //ChildWindow LoadingScreen = new ChildWindow();
            //LoadingScreen.ShowWaitingScreen("Loading");

            //worker.DoWork += (_, __) =>
            //{

                foreach (var item in customerList)
                {
                    if (String.Equals(item.CompanyName, companyName))
                    {
                        customerFound = true;
                        customer = item;
                        break;
                    }
                }
            //};

            //worker.RunWorkerCompleted += delegate(object s, RunWorkerCompletedEventArgs args)
            //{
            //    LoadingScreen.CloseWaitingScreen();
            //};
            //worker.RunWorkerAsync();

            return Tuple.Create(customerFound, customer);
        }

        //Pending Quotes
        private void SendToSale(Object parameter)
        {
            int index = PendingQuoteList.IndexOf(parameter as PendingQuote);
            if (index >= 0)
            {
                bool exist = DBAccess.CheckRecordExist(PendingQuoteList[index].QuoteNo, "Quote");
                if (exist)
                {
                    if (MessageBox.Show("Are you sure you want to create a sales order for quote no - " + PendingQuoteList[index].QuoteNo + "?", "Create Sales Order", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        SalesOrder SalesOrder = new SalesOrder();
                        SalesOrder.SalesOrderDetails = new ObservableCollection<SalesOrderDetails>();
                        SalesOrder.FreightCarrier = new FreightCarrier();
                        SalesOrder.Customer = new Customer();
                        SalesOrder.Comments = new List<Comment>();
                        SalesOrder.StockLocation = new StockLocation() { ID = 1 };

                        //Check customer                    
                        Tuple<bool, Customer> customerElement = SearchCustomer(PendingQuoteList[index].Customer.CompanyName);
                        if (customerElement.Item1 == false)
                        {
                            //This customer is a PrepaidCustomer
                            SalesOrder.PrepaidCustomerName = DBAccess.GetPrePaidCustomer(PendingQuoteList[index].QuoteNo, 0);
                            //customerElement.Item1 = true;

                            customerElement = Tuple.Create(true, customerElement.Item2);
                        }

                        if (customerElement.Item1)
                        {
                            //Comments     
                            SalesOrder.Comments.Add(new Comment() { Prefix = "SO", CreatedBy = UserData.FirstName + " " + UserData.LastName, CreatedDate = DateTime.Now, LocationID = 7, Note = string.Empty, LastUpdatedDate = DateTime.Now });
                            SalesOrder.Comments.Add(new Comment() { Prefix = "SO", CreatedBy = UserData.FirstName + " " + UserData.LastName, CreatedDate = DateTime.Now, LocationID = 8, Note = string.Empty, LastUpdatedDate = DateTime.Now });
                            SalesOrder.SalesMadeBy = UserData.FirstName + " " + UserData.LastName;
                            SalesOrder.SalesCompletedBy = UserData.FirstName + " " + UserData.LastName;
                            SalesOrder.OrderAction = OrderStatus.Release.ToString();
                            SalesOrder.OrderDate = DateTime.Now;

                            //Fix Freight
                            if (SalesOrder.FreightCarrier.Id == 6)
                            {
                                SalesOrder.FreightDetails = new BindingList<FreightDetails>();
                                FreightDetails f = new FreightDetails();
                                FreightCode fc = new FreightCode();
                                fc.ID = PendingQuoteList[index].FreightDetails[0].FreightCodeDetails.ID;
                                fc.Description = "Pickup";
                                fc.Price = 0;
                                f.Pallets = 0;
                                f.Total = 0;
                                f.FreightCodeDetails = fc;
                                SalesOrder.FreightDetails.Add(f);
                            }

                            SalesOrder.Customer = customerElement.Item2;
                            SalesOrder.Customer.CustomerType = customerElement.Item2.CustomerType;
                            SalesOrder.OrderPriority = CoreProcess.ConvertOrderTypeToInt("Normal");
                            SalesOrder.GSTEnabled = true;
                            OrderManager om = new OrderManager();

                            LoadProductStock();
                            if (productStockList != null || productStockList.Count > 0)
                            {
                                //Add to sales order details
                                Quote quote = DBAccess.GetQuote(PendingQuoteList[index].QuoteNo);
                                if (quote != null)
                                {
                                    foreach (var item in quote.QuoteDetails)
                                    {
                                        SalesOrderDetails sod = new SalesOrderDetails();
                                        sod.Product = new Product();
                                        sod.Product = item.Product;
                                        sod.QuantityStr = item.QuantityStr.ToString();
                                        sod.Quantity = item.Quantity;
                                        sod.Discount = item.Discount;
                                        SalesOrder.SalesOrderDetails.Add(sod);
                                    }

                                    SalesOrder.FreightDetails = new BindingList<FreightDetails>();
                                    SalesOrder.FreightDetails.Add(quote.FreightDetails[0]);
                                    SalesOrder.ShipTo = quote.Customer.ShipAddress + System.Environment.NewLine + quote.Customer.ShipCity + System.Environment.NewLine + quote.Customer.ShipState + System.Environment.NewLine + quote.Customer.CompanyPostCode;
                                    PendingQuoteList[index].Instructions = quote.Instructions;

                                    ChildWindow showNewSalesScreen = new ChildWindow();

                                    object obj = PendingQuoteList[index];
                                    OrderOrigin<object, string> oo = new OrderOrigin<object, string>();
                                    oo.OrderType = obj;
                                    oo.Origin = OrderOriginTypes.Sales.ToString();

                                    showNewSalesScreen.ShowNewSalesOrderView(SalesOrder, oo);
                                    showNewSalesScreen.newSalesOrder_Closed += (r =>
                                    {
                                        PendingQuoteList.Clear();
                                        LoadPendingQuotes();
                                    });
                                }
                                else
                                {
                                    MessageBox.Show("No online order items found for this order", "", MessageBoxButton.OK, MessageBoxImage.Error);
                                }
                            }
                            else
                            {
                                MessageBox.Show("Cannot load Stock details. Please try again later", "Cannot Load Stock Details", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                        else
                        {
                            MessageBox.Show("Customer - " + PendingQuoteList[index].Customer.CompanyName + " does not exist in the customer database" + System.Environment.NewLine + "Add this customer to customer database", "Cannot Find Customer", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                        }
                    }
                }
                else
                {
                    MessageBox.Show("This quote no longer exist", "", MessageBoxButton.OK, MessageBoxImage.Error);
                    LoadPendingQuotes();
                }
            }
           
        }

        
        //Online orders
        private void ProcessOrder(Object parameter)
        {
            int index = OnlineOrderList.IndexOf(parameter as OnlineOrder);
            if (index >= 0)
            {
                bool exist = DBAccess.CheckRecordExist(OnlineOrderList[index].OnlineOrderNo, "Online");
                if (exist)
                {
                    //Check customer
                    Tuple<bool,Customer> customerElement = SearchCustomer(OnlineOrderList[index].OnlineOrderCustomer.CompanyName);
                                
                    if (customerElement.Item1 == false)
                    {
                        MessageBox.Show("The customer - " + OnlineOrderList[index].OnlineOrderCustomer.CompanyName + " does not exist in the database" + System.Environment.NewLine + "First add customer and create sales order", "", MessageBoxButton.OK, MessageBoxImage.Warning);

                        ChildWindow showCustomerView = new ChildWindow();
                        showCustomerView.ShowAddCustomerView(new CustomerPending()
                        {
                            CustomerId = 999999999,
                            CompanyName = OnlineOrderList[index].OnlineOrderCustomer.CompanyName,
                            DiscountStr = string.Empty,
                            ProductTypeStr = string.Empty,
                            CustomerNoteStr = string.Empty,
                            CompanyAddress = string.Empty,
                            CompanyCity = string.Empty,
                            CompanyState = string.Empty,
                            CompanyPostCode = string.Empty,
                            CompanyCountry = string.Empty,
                            CompanyEmail = OnlineOrderList[index].OnlineOrderCustomer.UserEmail,
                            CompanyTelephone = string.Empty,
                            CompanyFax = string.Empty,
                            Designation1 = string.Empty,
                            FirstName1 = string.Empty,
                            LastName1 = string.Empty,
                            Telephone1 = string.Empty,
                            Mobile1 = string.Empty,
                            Fax1 = string.Empty,
                            Email1 = string.Empty,
                            Designation2 = string.Empty,
                            FirstName2 = string.Empty,
                            LastName2 = string.Empty,
                            Telephone2 = string.Empty,
                            Mobile2 = string.Empty,
                            Fax2 = string.Empty,
                            Email2 = string.Empty,
                            Designation3 = string.Empty,
                            FirstName3 = string.Empty,
                            LastName3 = string.Empty,
                            Telephone3 = string.Empty,
                            Mobile3 = string.Empty,
                            Fax3 = string.Empty,
                            Email3 = string.Empty,
                            ShipAddress = OnlineOrderList[index].DeliveryAddress,
                            ShipCity = string.Empty,
                            ShipState = OnlineOrderList[index].DeliveryState,
                            ShipPostCode = OnlineOrderList[index].DeliveryPostCode,
                            ShipCountry = string.Empty,
                            CreditLimit = 0,
                            CreditRemaining = 0,
                            Debt = 0,
                            LastUpdatedBy = UserData.FirstName + " " + UserData.LastName,
                            LastUpdatedDateTime = DateTime.Now,
                            StopCredit = "False",
                            Active = true

                        });
                        showCustomerView.showAddCustomerView_Closed += (r =>
                        {
                            if (r > 0)
                            {
                                customerElement = SearchCustomer(OnlineOrderList[index].OnlineOrderCustomer.CompanyName);
                                MessageBox.Show("Click MAKE SALE button to process this order", "", MessageBoxButton.OK, MessageBoxImage.Warning);
                            }
                        });   
                    }

                    if (customerElement.Item1)
                    {
                        //Comments     
                        SalesOrder SalesOrder = new SalesOrder();
                        SalesOrder.SalesOrderDetails = new ObservableCollection<SalesOrderDetails>();
                        SalesOrder.FreightCarrier = new FreightCarrier();
                        SalesOrder.Customer = new Customer();
                        SalesOrder.Comments = new List<Comment>();
                        SalesOrder.StockLocation = new StockLocation() { ID = 1 };
                        SalesOrder.Comments.Add(new Comment() { Prefix = "SO", CreatedBy = UserData.FirstName + " " + UserData.LastName, CreatedDate = DateTime.Now, LocationID = 7, Note = string.Empty, LastUpdatedDate = DateTime.Now });
                        SalesOrder.Comments.Add(new Comment() { Prefix = "SO", CreatedBy = UserData.FirstName + " " + UserData.LastName, CreatedDate = DateTime.Now, LocationID = 8, Note = string.Empty, LastUpdatedDate = DateTime.Now });
                        SalesOrder.SalesMadeBy = UserData.FirstName + " " + UserData.LastName;
                        SalesOrder.SalesCompletedBy = UserData.FirstName + " " + UserData.LastName;
                        SalesOrder.OrderAction = OrderStatus.Release.ToString();
                        SalesOrder.OrderDate = DateTime.Now;

                        //Fix Freight
                        if (SalesOrder.FreightCarrier.Id == 6)
                        {
                            SalesOrder.FreightDetails = new BindingList<FreightDetails>();
                            FreightDetails f = new FreightDetails();
                            FreightCode fc = new FreightCode();
                            fc.ID = 51;
                            fc.Description = "Pickup";
                            fc.Price = 0;
                            f.Pallets = 0;
                            f.Total = 0;
                            f.FreightCodeDetails = fc;
                            SalesOrder.FreightDetails.Add(f);
                        }

                        DateTime? collDate = null;
                        if (!string.IsNullOrWhiteSpace(OnlineOrderList[index].CollectDate))
                        {
                            collDate = Convert.ToDateTime(OnlineOrderList[index].CollectDate);
                        }
                        else if (!string.IsNullOrWhiteSpace(OnlineOrderList[index].RequiredDeliveryDate))
                        {
                            collDate = Convert.ToDateTime(OnlineOrderList[index].RequiredDeliveryDate);
                        }

                        //SalesOrder.ShipTo = OnlineOrderList[index].DeliveryAddress + System.Environment.NewLine + " "
                        SalesOrder.DesiredDispatchDate = collDate;
                        SalesOrder.Customer = customerElement.Item2;
                        SalesOrder.Customer.CustomerType = customerElement.Item2.CustomerType;
                        SalesOrder.OrderPriority = CoreProcess.ConvertOrderTypeToInt("Normal");
                        SalesOrder.GSTEnabled = true;
                        OrderManager om = new OrderManager();

                        LoadProductStock();
                        if (productStockList != null || productStockList.Count > 0)
                        {
                            //Add to sales order details
                            ObservableCollection<OnlineOrderItem> oI = DBAccess.GetOnlineOrderItemsByOrderNo(OnlineOrderList[index].OnlineOrderNo);
                            if (oI != null)
                            {
                                //Match onlineorder items with system products
                                ObservableCollection<Product> productsList = DBAccess.GetAllProds(true);
                                if (productsList != null)
                                {
                                    foreach (var item in oI)
                                    {
                                        var data = productsList.SingleOrDefault(x => x.ProductCode == item.ProductCode);
                                        if (data != null)
                                        {
                                            SalesOrderDetails sod = new SalesOrderDetails();
                                            sod.Product = new Product();
                                            sod.Product = data;
                                            sod.QuantityStr = item.ProductQty.ToString();
                                            sod.Quantity = item.ProductQty;
                                            sod.Discount = item.Discount;
                                            SalesOrder.SalesOrderDetails.Add(sod);
                                        }
                                        else
                                        {
                                            MessageBox.Show("Item - " + item.ProductCode + " not found in the stock", "", MessageBoxButton.OK, MessageBoxImage.Error);
                                            break;
                                        }
                                    }

                                    ChildWindow showNewSalesScreen = new ChildWindow();

                                    object obj = OnlineOrderList[index];
                                    OrderOrigin<object, string> oo = new OrderOrigin<object, string>();
                                    oo.OrderType = obj;
                                    oo.Origin = OrderOriginTypes.A1RubberOnline.ToString();

                                    showNewSalesScreen.ShowNewSalesOrderView(SalesOrder, oo);
                                    showNewSalesScreen.newSalesOrder_Closed += (r =>
                                    {
                                        OnlineOrderList.Clear();
                                        LoadOnlineOrders();
                                    });
                                }
                                else
                                {
                                    MessageBox.Show("Cannot load product list", "", MessageBoxButton.OK, MessageBoxImage.Error);
                                }
                            }
                            else
                            {
                                MessageBox.Show("No online order items found for this order", "", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                        else
                        {
                            MessageBox.Show("Cannot load Stock details. Please try again later", "Cannot Load Stock Details", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }

                }
                else
                {
                    MessageBox.Show("This online order no longer exist", "Online Order Does Not Exist", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadOnlineOrders();
                }
            }
        }

        public ObservableCollection<OnlineOrder> OnlineOrderList
        {
            get
            {
                return this._onlineOrderList;
            }
            set
            {
                this._onlineOrderList = value;
                RaisePropertyChanged("OnlineOrderList");
            }
        }

        public ObservableCollection<PendingQuote> PendingQuoteList
        {
            get
            {
                return this._pendingQuoteList;
            }
            set
            {
                this._pendingQuoteList = value;
                RaisePropertyChanged("PendingQuoteList");
            }
        }

        

        public int SelectedTab
        {
            get
            {
                return this._selectedTab;
            }
            set
            {
                this._selectedTab = value;
                RaisePropertyChanged("SelectedTab");
                if(SelectedTab ==0)
                {
                    LoadPendingQuotes();
                }
                else
                {
                    LoadOnlineOrders();
                }
               
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

        public Microsoft.Practices.Prism.Commands.DelegateCommand CloseCommand
        {
            get { return _closeCommand; }
        }

        public ICommand ProcessCommand
        {
            get
            {
                if (_processCommand == null)
                {
                    _processCommand = new DelegateCommand(CanExecute, ProcessOrder);
                }
                return _processCommand;
            }
        }

        public ICommand DeleteQuoteCommand
        {
            get
            {
                if (_deleteQuoteCommand == null)
                {
                    _deleteQuoteCommand = new DelegateCommand(CanExecute, DeleteQuote);
                }
                return _deleteQuoteCommand;
            }
        }

        public ICommand DeleteOnlineOrderCommand
        {
            get
            {
                if (_deleteOnlineOrderCommand == null)
                {
                    _deleteOnlineOrderCommand = new DelegateCommand(CanExecute, DeleteOnlineOrder);
                }
                return _deleteOnlineOrderCommand;
            }
        }
               


        public ICommand CreateSalesOrderCommand
        {
            get
            {
                if (_createSalesOrderCommand == null)
                {
                    _createSalesOrderCommand = new DelegateCommand(CanExecute, SendToSale);
                }
                return _createSalesOrderCommand;
            }
        }

        public ICommand RefreshPendingQuotesCommand
        {
            get
            {
                return _refreshPendingQuotesCommand ?? (_refreshPendingQuotesCommand = new CommandHandler(() => LoadPendingQuotes(), true));
            }
        }

        public ICommand RefreshOnlineOrdersCommand
        {
            get
            {
                return _refreshOnlineOrdersCommand ?? (_refreshOnlineOrdersCommand = new CommandHandler(() => LoadOnlineOrders(), true));
            }
        }

        
    }
}
