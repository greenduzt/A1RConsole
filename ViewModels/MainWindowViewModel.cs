using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.ComponentModel;
using A1RConsole.Bases;
using A1RConsole.Commands;
using A1RConsole.Models;
using A1RConsole.Interfaces;
using A1RConsole.Models.Orders;
using A1RConsole.Views.Orders.SalesOrders;
using System.Windows.Input;
using A1RConsole.ViewModels.Orders.SalesOrders;
using A1RConsole.Core;
using A1RConsole.Models.Meta;
using Microsoft.Practices.Prism.Commands;
using A1RConsole.Views.Orders;
using A1RConsole.ViewModels.Orders;
using A1RConsole.ViewModels.Sales;
using A1RConsole.ViewModels.Products;
using A1RConsole.ViewModels.Stock;
using A1RConsole.ViewModels.Suppliers;
using A1RConsole.ViewModels.Purchasing;
using A1RConsole.Models.Products;
using A1RConsole.ViewModels.Receiving;
using A1RConsole.ViewModels.Shipping;
using A1RConsole.ViewModels.Customers;
using A1RConsole.ViewModels.Invoicing;
using System.Threading;
using System.Windows.Threading;
using A1RConsole.ViewModels.Quoting;
using System.Windows;
using A1RConsole.Models.Users;
using A1RConsole.Models.Customers;
using A1RConsole.Views.Login;
using System.Windows.Data;
using A1RConsole.ViewModels.Orders.PendingOrders;
using A1RConsole.DB;
using System.Data.SqlClient;
using A1RConsole.Models.Stock;
using System.Diagnostics;
using System.IO;
using A1RConsole.Models.Quoting;
using A1RConsole.ViewModels.Orders.NewOrderPDF;
using A1RConsole.ViewModels.Vjs;

namespace A1RConsole.ViewModels
{  

    public class MainWindowViewModel : ViewModelBase
    {
        private IContent _selectedWindow = null;
        private string _selectedTitle, _fullName, _date, _liveTIme, _version;
        private bool _isSupplierEnabled, _isInvoiceEnabled, _isShippingEnabled, _isPurchasingEnabled, _isProdPlanningEnabled, _isInventoryEnabled, _isSalesEnabled,
            _isQuotingEnabled, _isCustomerEnabled, _isCustomerPendingEnabled, _isOnlineOrdersEnabled, _isNewOrderPDFEnabled, _isOrdersNotLeftEnabled, _isSchedulingReportEnabled,
            _isInvoicingEnabled, _isOrderItemsEnabled, _isOrdersEnabled, _isVjsEnabled, _isMailChimp, _isOrderPoolReportEnabled, _isNewQuoteEnabled;
        private string _sendToWarehouseVisibility;
        private ObservableCollection<IContent> _items;
        private ObservableCollection<SalesOrder> _pendingSalesOrders;
        private SalesOrder _selectedSalesOrder;
        private ObservableCollection<Customer> customerList;
        public ObservableCollection<SalesOrder> SalesOrder { get; private set; }
        public StockAvailabilityNotifier stockAvailabilityNotifier { get; set; }
        public CustomerCreditSalesNotifier customerCreditSalesNotifier { get; set; }
        DispatcherTimer Timer = new DispatcherTimer();
        private ICommand _menuButtonPressCommand;
        private ICommand _logOutCommand;
        private ICommand _processOrderCommand;

        public static MainWindowViewModel instance;
        NewQuoteViewModel newQuoteViewModelInstance;
        NewOrderPDFViewModel newOrderPDFViewModelInstance;

        public MainWindowViewModel()
        {           

            PendingSalesOrders = new ObservableCollection<SalesOrder>();           

            if (string.IsNullOrWhiteSpace(UserData.FirstName))
            {
                UserData.FirstName = "Test";
            }
            if (string.IsNullOrWhiteSpace(UserData.LastName))
            {
                UserData.FirstName = "User";
            }

            if (string.IsNullOrWhiteSpace(UserData.UserName))
            {
                UserData.FirstName = "Developer";
            }

            FullName = UserData.FirstName.ToUpper() + " " + UserData.LastName.ToUpper();
            Date = DateTime.Now.ToString("dddd, dd MMMM yyyy");

            if (UserData.MetaData == null)
            {
                UserData.MetaData = DBAccess.GetMetaData();
            }

            if (UserData.MetaData != null)
            {
                foreach (var item in UserData.MetaData)
                {
                    if (item.KeyName == "version")
                    {
                        Version = item.Description;
                        break;
                    }
                }
            }


            if (UserData.UserID == 0 && UserData.FirstName == "Developer")
            {
                //Enable privilages
                UserData.UserPrivilages = new List<UserPrivilages>();
                UserData.UserPrivilages.Add(new UserPrivilages() { UserID = 0, Area = "Production_Greyed", Visibility = "Visible" });
                UserData.UserPrivilages.Add(new UserPrivilages() { UserID = 0, Area = "Customer_Credit_History", Visibility = "Visible" });
                UserData.UserPrivilages.Add(new UserPrivilages() { UserID = 0, Area = "Customer_Credit_Details", Visibility = "Visible" });
                UserData.UserPrivilages.Add(new UserPrivilages() { UserID = 0, Area = "Admin", Visibility = "Visible" });
                UserData.UserPrivilages.Add(new UserPrivilages() { UserID = 0, Area = "Maintenance", Visibility = "Visible" });
                UserData.UserPrivilages.Add(new UserPrivilages() { UserID = 0, Area = "Orders", Visibility = "Visible" });
                UserData.UserPrivilages.Add(new UserPrivilages() { UserID = 0, Area = "Stock", Visibility = "Visible" });
                UserData.UserPrivilages.Add(new UserPrivilages() { UserID = 0, Area = "Production", Visibility = "Visible" });
                UserData.UserPrivilages.Add(new UserPrivilages() { UserID = 0, Area = "WorkStations", Visibility = "Visible" });
                UserData.UserPrivilages.Add(new UserPrivilages() { UserID = 0, Area = "Grading", Visibility = "Visible" });
                UserData.UserPrivilages.Add(new UserPrivilages() { UserID = 0, Area = "Mixing", Visibility = "Visible" });
                UserData.UserPrivilages.Add(new UserPrivilages() { UserID = 0, Area = "Slitting", Visibility = "Visible" });
                UserData.UserPrivilages.Add(new UserPrivilages() { UserID = 0, Area = "Peeling", Visibility = "Visible" });
                UserData.UserPrivilages.Add(new UserPrivilages() { UserID = 0, Area = "ReRolling", Visibility = "Visible" });
                UserData.UserPrivilages.Add(new UserPrivilages() { UserID = 0, Area = "Curing", Visibility = "Visible" });
                UserData.UserPrivilages.Add(new UserPrivilages() { UserID = 0, Area = "Warehouse", Visibility = "Visible" });
                UserData.UserPrivilages.Add(new UserPrivilages() { UserID = 0, Area = "Product_Maintenance_Product_Cost", Visibility = "Visible" });
                UserData.UserPrivilages.Add(new UserPrivilages() { UserID = 0, Area = "Credit_Update_Button", Visibility = "Visible" });
                UserData.UserPrivilages.Add(new UserPrivilages() { UserID = 0, Area = "Credit_Add_Button", Visibility = "Visible" });
                UserData.UserPrivilages.Add(new UserPrivilages() { UserID = 0, Area = "Supplier", Visibility = "Visible" });
                UserData.UserPrivilages.Add(new UserPrivilages() { UserID = 0, Area = "Invoice", Visibility = "Visible" });
                UserData.UserPrivilages.Add(new UserPrivilages() { UserID = 0, Area = "Purchasing", Visibility = "Visible" });
                UserData.UserPrivilages.Add(new UserPrivilages() { UserID = 0, Area = "Shipping", Visibility = "Visible" });
                UserData.UserPrivilages.Add(new UserPrivilages() { UserID = 0, Area = "ProductPlanning", Visibility = "Visible" });
                UserData.UserPrivilages.Add(new UserPrivilages() { UserID = 0, Area = "Inventory", Visibility = "Visible" });
                UserData.UserPrivilages.Add(new UserPrivilages() { UserID = 0, Area = "Sales", Visibility = "Visible" });
                UserData.UserPrivilages.Add(new UserPrivilages() { UserID = 0, Area = "Quoting", Visibility = "Visible" });
                UserData.UserPrivilages.Add(new UserPrivilages() { UserID = 0, Area = "Customer", Visibility = "Visible" });
                UserData.UserPrivilages.Add(new UserPrivilages() { UserID = 0, Area = "Customer->customer_type", Visibility = "Visible" });
                UserData.UserPrivilages.Add(new UserPrivilages() { UserID = 0, Area = "AddToCustomerPending", Visibility = "False" });
                UserData.UserPrivilages.Add(new UserPrivilages() { UserID = 0, Area = "OnlineOrdersFrontMenu", Visibility = "Visible" });
                UserData.UserPrivilages.Add(new UserPrivilages() { UserID = 0, Area = "ProductMaintenance->qty_enabled", Visibility = "True" });
                UserData.UserPrivilages.Add(new UserPrivilages() { UserID = 0, Area = "MainWindow->SendToWarehouseVisibility", Visibility = "Visible" });
                UserData.UserPrivilages.Add(new UserPrivilages() { UserID = 0, Area = "CustomerCreditIncDec", Visibility = "True" });
                UserData.UserPrivilages.Add(new UserPrivilages() { UserID = 0, Area = "Customer->admin_notes", Visibility = "True" });
                UserData.UserPrivilages.Add(new UserPrivilages() { UserID = 0, Area = "MainWindow->OrderOnlinePDFVisibility", Visibility = "Visible" });
                UserData.UserPrivilages.Add(new UserPrivilages() { UserID = 0, Area = "MainWindow-Invoicing", Visibility = "Visible" });
                UserData.UserPrivilages.Add(new UserPrivilages() { UserID = 0, Area = "MainWindow->Invoice", Visibility = "Collapsed" });
                UserData.UserPrivilages.Add(new UserPrivilages() { UserID = 0, Area = "MainWindow->Vjs", Visibility = "Visible" });
                UserData.UserPrivilages.Add(new UserPrivilages() { UserID = 0, Area = "MainWindow->OrderNotLeft", Visibility = "Visible" });
                UserData.UserPrivilages.Add(new UserPrivilages() { UserID = 0, Area = "MainWindow->Vjs->Orders", Visibility = "Visible" });
                UserData.UserPrivilages.Add(new UserPrivilages() { UserID = 0, Area = "MainWindow->Vjs->OrderItems", Visibility = "Visible" });
                UserData.UserPrivilages.Add(new UserPrivilages() { UserID = 0, Area = "MainWindow->MailChimp", Visibility = "Visible" });
                UserData.UserPrivilages.Add(new UserPrivilages() { UserID = 0, Area = "MainWindow->OrderPoolReport", Visibility = "Visible" });
                UserData.UserPrivilages.Add(new UserPrivilages() { UserID = 0, Area = "Quoting->NewQuote", Visibility = "Collapsed" });
                UserData.UserPrivilages.Add(new UserPrivilages() { UserID = 0, Area = "MainWindow->SchedulingReport", Visibility = "Visible" });

                AssignPrivilages();

            }
            else
            {
                AssignPrivilages();
            }

            this.Items = new ObservableCollection<IContent>();

            this.SalesOrder = new ObservableCollection<SalesOrder>();


            SalesOrder so = new SalesOrder();
            so.CustomerOrderNo = "111";
            this.SalesOrder.Add(so);

            Timer.Tick += new EventHandler(Timer_Click);
            Timer.Interval = new TimeSpan(0, 0, 1);
            Timer.Start();

            if (SendToWarehouseVisibility == "Visible")
            {
                this.stockAvailabilityNotifier = new StockAvailabilityNotifier();
                this.stockAvailabilityNotifier.NewMessage += new EventHandler<SqlNotificationEventArgs>(notifier_NewMessage);
                ObservableCollection<SalesOrder> ps = this.stockAvailabilityNotifier.RegisterDependency();
                LoadSalesOrders(ps);

                this.customerCreditSalesNotifier = new CustomerCreditSalesNotifier();
                this.customerCreditSalesNotifier.NewMessage += new EventHandler<SqlNotificationEventArgs>(notifier_NewMessage1);
                ObservableCollection<SalesOrder> soAcc = this.customerCreditSalesNotifier.RegisterDependency();
                LoadSalesOrders1(soAcc);
            }

            instance = this;
        }       

        void notifier_NewMessage(object sender, SqlNotificationEventArgs e)
        {
            LoadSalesOrders(this.stockAvailabilityNotifier.RegisterDependency());
        }

        void notifier_NewMessage1(object sender, SqlNotificationEventArgs e)
        {
            LoadSalesOrders1(this.customerCreditSalesNotifier.RegisterDependency());
        }
        

        private void LoadSalesOrders(ObservableCollection<SalesOrder> ps)
        {
            if (ps != null )
            {
                for (int i = ps.Count - 1; i >= 0; i--)
                {
                    //For account customers first check credit
                    if (ps[i].TermsID == "30EOM")
                    {
                        CustomerCreditHistory cch = DBAccess.GetCustomerCreditHistoryRecord(ps[i].SalesOrderNo, ps[i].Customer.CustomerId);
                        if(cch != null)
                        {
                            if (cch.SalesOrderNo == ps[i].SalesOrderNo && cch.CreditDeducted < ps[i].TotalAmount)
                            {
                                if(ps[i].Customer.CreditRemaining < ps[i].TotalAmount)
                                {
                                    ps.Remove(ps[i]);
                                    break;
                                }
                            }
                        }
                        else
                        {
                            if (ps[i].Customer.CreditRemaining < ps[i].TotalAmount)
                            {
                                ps.Remove(ps[i]);
                                break;
                            }
                        }
                    }
                    for (int x = 0; x < ps[i].SalesOrderDetails.Count; x++)
                    {
                        var data = ps[i].SalesOrderDetails.Where(a => a.Product.ProductID == ps[i].SalesOrderDetails[x].Product.ProductID).ToList();
                        decimal tot = 0;
                        if (data != null && data.Count > 1)
                        {
                            tot = data.Sum(q => q.Quantity);
                        }

                        //First check if the qty reserved is equal to qty ordered
                        if (ps[i].SalesOrderDetails[x].Quantity != ps[i].SalesOrderDetails[x].QtyToMake)
                        {
                            if (data != null && tot > ps[i].SalesOrderDetails[x].QtyInStock || ps[i].SalesOrderDetails[x].Quantity > ps[i].SalesOrderDetails[x].QtyInStock)
                            {
                                ps.Remove(ps[i]);
                                break;
                            }
                            else
                            {
                                //if (ps[i].TermsID == "Prepaid")
                               // {
                                    CusAndPrePaid<long, string, string> cus = DBAccess.GetCustomerAndPrepaidCusName(ps[i].Customer.CustomerId, ps[i].SalesOrderNo);

                                    if (ps[i].Customer.CustomerId != 0 && cus.CusType == "InDB")
                                    {
                                        //var cusData = customerList.SingleOrDefault(c => c.CustomerId == ps[i].Customer.CustomerId);
                                        //ps[i].Customer = cusData;
                                        ps[i].Customer.CompanyName = cus.Name;
                                    }
                                    else
                                    {
                                        //ps[i].Customer.CompanyName = DBAccess.GetPrePaidCustomer(ps[i].SalesOrderNo);
                                        ps[i].Customer.CompanyName = cus.Name;
                                        ps[i].Customer.CustomerType = "Prepaid";
                                    }
                                //}
                            }
                        }

                    }
                }



                //PendingSalesOrders=ps;
                //PendingSalesOrdersTemp1 = ps;

                for (int i = 0; i < PendingSalesOrders.Count; i++)
                {
                    App.Current.Dispatcher.Invoke((Action)delegate
                    {
                        if (PendingSalesOrders[i].TermsID == "Prepaid")
                        {
                            PendingSalesOrders.RemoveAt(i);
                        }

                    });
                }

                foreach (var item in ps)
                {
                    App.Current.Dispatcher.Invoke((Action)delegate
                    {
                        if (item.TermsID == "Prepaid")
                        {

                            if (PendingSalesOrders.Count == 0)
                            {
                                PendingSalesOrders.Add(item);
                            }
                            else
                            {
                                bool any = PendingSalesOrders.Any(x => x.SalesOrderNo == item.SalesOrderNo);
                                if (any == false)
                                {
                                    PendingSalesOrders.Add(item);
                                }
                            }
                        }
                    });
                }
            }            
        }

        private void LoadSalesOrders1(ObservableCollection<SalesOrder> ps)
        {
            if (ps != null )
            {
                //Reprocess
                for (int i = ps.Count - 1; i >= 0; i--)
                {
                    //For account customers first check credit
                    if (ps[i].TermsID == "30EOM")
                    {
                        CustomerCreditHistory cch = DBAccess.GetCustomerCreditHistoryRecord(ps[i].SalesOrderNo, ps[i].Customer.CustomerId);
                        if (cch != null)
                        {
                            if (cch.CreditDeducted < ps[i].TotalAmount)
                            {
                                if (ps[i].Customer.CreditRemaining < ps[i].TotalAmount)
                                {
                                    ps.Remove(ps[i]);
                                    break;
                                }
                            }
                        }
                        else
                        {
                            if (ps[i].Customer.CreditRemaining < ps[i].TotalAmount)
                            {
                                ps.Remove(ps[i]);
                                break;
                            }
                        }
                    }
                    for (int x = 0; x < ps[i].SalesOrderDetails.Count; x++)
                    {
                        var data = ps[i].SalesOrderDetails.Where(a => a.Product.ProductID == ps[i].SalesOrderDetails[x].Product.ProductID).ToList();
                        decimal tot = 0;
                        if (data != null && data.Count > 1)
                        {
                            tot = data.Sum(q => q.Quantity);
                        }

                        //First check if the qty reserved is equal to qty ordered
                        if (ps[i].SalesOrderDetails[x].Quantity != ps[i].SalesOrderDetails[x].QtyToMake)
                        {
                            if (data != null && tot > ps[i].SalesOrderDetails[x].QtyInStock || ps[i].SalesOrderDetails[x].Quantity > ps[i].SalesOrderDetails[x].QtyInStock)
                            {
                                ps.Remove(ps[i]);
                                break;
                            }
                            else
                            {
                                //if (ps[i].TermsID == "Prepaid")
                                // {
                                CusAndPrePaid<long, string, string> cus = DBAccess.GetCustomerAndPrepaidCusName(ps[i].Customer.CustomerId, ps[i].SalesOrderNo);

                                if (ps[i].Customer.CustomerId != 0 && cus.CusType == "InDB")
                                {
                                    //var cusData = customerList.SingleOrDefault(c => c.CustomerId == ps[i].Customer.CustomerId);
                                    //ps[i].Customer = cusData;
                                    ps[i].Customer.CompanyName = cus.Name;
                                }
                                else
                                {
                                    //ps[i].Customer.CompanyName = DBAccess.GetPrePaidCustomer(ps[i].SalesOrderNo);
                                    ps[i].Customer.CompanyName = cus.Name;
                                    ps[i].Customer.CustomerType = "Prepaid";
                                }
                                //}
                            }
                        }

                    }
                }

                for (int i = 0; i < PendingSalesOrders.Count; i++)
                {
                    App.Current.Dispatcher.Invoke((Action)delegate
                    {
                    if (PendingSalesOrders[i].TermsID == "30EOM")
                    {
                        PendingSalesOrders.RemoveAt(i);
                    }

                        });
                }

                foreach (var item in ps)
                {
                    App.Current.Dispatcher.Invoke((Action)delegate
                    {
                        if (item.TermsID == "30EOM")
                        {

                            if (PendingSalesOrders.Count == 0)
                            {
                                PendingSalesOrders.Add(item);
                            }
                            else
                            {
                                bool any = PendingSalesOrders.Any(x => x.SalesOrderNo == item.SalesOrderNo);
                                if (any == false)
                                {
                                    PendingSalesOrders.Add(item);
                                }
                            }
                        }
                    });
                }    
            }
        }       
        
        private void AssignPrivilages()
        {
            foreach (var item in UserData.UserPrivilages)
            {
                if (item.Area.Trim() == "Supplier")
                {
                    IsSupplierEnabled = item.Visibility == "Visible" ? true : false;
                }
                
                if (item.Area.Trim() == "Shipping")
                {
                    IsShippingEnabled = item.Visibility == "Visible" ? true : false;
                }
                if (item.Area.Trim() == "Purchasing")
                {
                    IsPurchasingEnabled = item.Visibility == "Visible" ? true : false;
                }
                if (item.Area.Trim() == "ProductPlanning")
                {
                    IsProdPlanningEnabled = item.Visibility == "Visible" ? true : false;
                }
                if (item.Area.Trim() == "Inventory")
                {
                    IsInventoryEnabled = item.Visibility == "Visible" ? true : false;
                }
                if (item.Area.Trim() == "Sales")
                {
                    IsSalesEnabled = item.Visibility == "Visible" ? true : false;
                }
                if (item.Area.Trim() == "Quoting")
                {
                    IsQuotingEnabled = item.Visibility == "Visible" ? true : false;
                }
                
                if (item.Area.Trim() == "Customer")
                {
                    IsCustomerEnabled = item.Visibility == "Visible" ? true : false;
                }
                if (item.Area.Trim() == "AddToCustomerPending")
                {
                    IsCustomerPendingEnabled = false;
                }
                if (item.Area.Trim() == "OnlineOrdersFrontMenu")
                {
                    IsOnlineOrdersEnabled = item.Visibility == "Visible" ? true : false;
                }
                if (item.Area.Trim() == "MainWindow->SendToWarehouseVisibility")
                {
                    SendToWarehouseVisibility = item.Visibility;
                }
                if (item.Area.Trim() == "MainWindow->OrderOnlinePDFVisibility")
                {
                    IsNewOrderPDFEnabled = item.Visibility == "Visible" ? true : false;
                }
                if (item.Area.Trim() == "MainWindow-Invoicing")
                {
                    IsInvoicingEnabled = item.Visibility == "Visible" ? true : false;
                }              
                if (item.Area.Trim() == "MainWindow->Invoice")
                {
                    IsInvoiceEnabled = false;// item.Visibility == "Visible" ? true : false;
                }
                if (item.Area.Trim() == "MainWindow->OrderNotLeft")
                {
                    IsOrdersNotLeftEnabled = item.Visibility == "Visible" ? true : false;
                }
                if (item.Area.Trim() == "MainWindow->OrderPoolReport")
                {
                    IsOrderPoolReportEnabled = item.Visibility == "Visible" ? true : false;
                }
                if (item.Area.Trim() == "MainWindow->Vjs->OrderItems")
                {
                    IsOrderItemsEnabled = item.Visibility == "Visible" ? true : false;
                }
                if (item.Area.Trim() == "MainWindow->Vjs->Orders")
                {
                    IsOrdersEnabled = item.Visibility == "Visible" ? true : false;
                }
                if (item.Area.Trim() == "MainWindow->Vjs")
                {
                    IsVjsEnabled = item.Visibility == "Visible" ? true : false;
                }
                if (item.Area.Trim() == "MainWindow->MailChimp")
                {
                    IsMailChimp = item.Visibility == "Visible" ? true : false;
                }
                if (item.Area.Trim() == "Quoting->NewQuote")
                {
                    IsNewQuoteEnabled = item.Visibility == "Visible" ? true : false;
                }
                if (item.Area.Trim() == "MainWindow->SchedulingReport")
                {
                    IsSchedulingReportEnabled = item.Visibility == "Visible" ? true : false;
                }
               
            }
        }

        private void ViewPage(string button)
        {
            switch (button)
            {
                case "NewQuote": Quote q = new Quote(); q = null; NewQuote(q);
                    break;
                case "UpdateQuote": UpdateQuote();
                    break;
                case "NewSalesOrder": ShowSalesOrder();
                    break;
                case "OrderStatus": ShowOrderStatus();
                    break;
                case "SalesReport": ShowSalesReport();
                    break;
                case "Inventory": ShowInventory();
                    break;
                case "StockAdjustment": ShowStockAdjustmentWindow();
                    break;
                case "ProductCount": ShowProductCount();
                    break;
                case "ProductPlanning": ShowProductPlaning();
                    break;
                case "Suppliers": ShowSuppliers();
                    break;
                case "NewPurchaseOrder": ShowNewPurchaseOrder();
                    break;
                case "Recieving": ShowRecieving();
                    break;
                case "PrintPO": ShowPrintPO();
                    break;
                case "Shipping": ShowShipping();
                    break;
                case "Supplier": ShowSuppliers();
                    break;
                case "Customer": ShowCustomers();
                    break;                
                case "CustomerPendingList": ShowCustomerPendingList();
                    break;
                case "PendingOrders": ShowOnlineOrders();
                    break;
                case "NewOrderPDF": ShowNewOrderPDF(0,0);
                    break;
                case "Invoicing":   ShowInvoice();
                    break;
                case "OrdersNotLeft":ShowOrderHasNotLeft();
                    break;
                case "OrderPoolReport": ShowOrderPoolReport();
                    break;
                case "SchedulingReport":ShowSchedulingReport();
                    break;
                case "QuoteOrderReport":ShowQuoteOrderReport();
                    break;
                case "OpenQuotes": ShowOpenQuotes();
                    break;
                case "OrdersReport":  ShowOrdersReport();
                    break;
                case "OrderItemsReport":ShowOrderItemsReport();
                    break;
                case "MailChimp":ShowMailChimp();
                    break;
            }
        }

        private void Timer_Click(object sender, EventArgs e)
        {
            DateTime d;
            d = DateTime.Now;
            LiveTIme = string.Format("{0:hh\\:mm\\:ss\\:tt}", d);
        }

        public void NewQuote(Quote quote)
        {
            var yes = Items.SingleOrDefault(x => x.Title == "New Quote");
            if (yes != null)
            {
                newQuoteViewModelInstance = NewQuoteViewModel.instance;
                if (newQuoteViewModelInstance != null && quote != null)
                {                   
                    for (int i = 0; i < Items.Count; i++)
                    {
                        if (Items[i].Title == "New Quote")
                        {
                            Items.RemoveAt(i);
                        }
                    }

                    var item = new NewQuoteViewModel(quote);
                    item.Closing += (s, e) => this.Items.Remove(item);
                    this.Items.Add(item);
                }
                else
                {
                    int oldIndex = 0;
                    for (int i = 0; i < Items.Count; i++)
                    {
                        if (Items[i].Title == "New Quote")
                        {
                            Items[i] = yes;
                            oldIndex = i;

                        }
                    }
                    Items.Move(oldIndex, Items.Count - 1);
                }
            }
            else
            {
                var item = new NewQuoteViewModel(quote);
                item.Closing += (s, e) => this.Items.Remove(item);

                this.Items.Add(item);
            }
        }

        public void ShowNewOrderPDF(int quoteNo, int orderNo)
        {
            var yes = Items.SingleOrDefault(x => x.Title == "New Order PDF");
            if (yes != null)
            {
                newOrderPDFViewModelInstance = NewOrderPDFViewModel.instance;
                if (newOrderPDFViewModelInstance != null && (quoteNo > 0 || orderNo > 0))
                {
                    for (int i = 0; i < Items.Count; i++)
                    {
                        if (Items[i].Title == "New Order PDF")
                        {
                            Items.RemoveAt(i);
                        }
                    }

                    var item = new NewOrderPDFViewModel(quoteNo, orderNo);
                    item.Closing += (s, e) => this.Items.Remove(item);
                    this.Items.Add(item);
                }
                else
                {
                    int oldIndex = 0;
                    for (int i = 0; i < Items.Count; i++)
                    {
                        if (Items[i].Title == "New Order PDF")
                        {
                            Items[i] = yes;
                            oldIndex = i;
                        }
                    }
                    Items.Move(oldIndex, Items.Count - 1);
                }
            }
            else
            {
                var item = new NewOrderPDFViewModel(quoteNo, orderNo);
                item.Closing += (s, e) => this.Items.Remove(item);
                this.Items.Add(item);
            }

        }



        private void UpdateQuote()
        {
            var yes = Items.SingleOrDefault(x => x.Title == "View/Update Quote");

            if (yes != null)
            {
                int oldIndex = 0;
                for (int i = 0; i < Items.Count; i++)
                {
                    if (Items[i].Title == "View/Update Quote")
                    {
                        Items[i] = yes;
                        oldIndex = i;
                    }
                }
                Items.Move(oldIndex, Items.Count - 1);
            }
            else
            {
                var item = new ViewUpdateQuoteViewModel();
                item.Closing += (s, e) => this.Items.Remove(item);
                this.Items.Add(item);
            }
        }

        private void ShowSalesOrder()
        {
            var yes = Items.SingleOrDefault(x => x.Title == "New Sales Order");
            if (yes != null)
            {
                int oldIndex = 0;
                for (int i = 0; i < Items.Count; i++)
                {
                    if (Items[i].Title == "New Sales Order")
                    {
                        Items[i] = yes;
                        oldIndex = i;
                    }
                }
                Items.Move(oldIndex, Items.Count - 1);
            }
            else
            {
                var item = new NewSalesOrderViewModel();
                item.Closing += (s, e) => this.Items.Remove(item);
                this.Items.Add(item);
            }

        }

        private void ShowOrderStatus()
        {
            var yes = Items.SingleOrDefault(x => x.Title == "Order Status");
            if (yes != null)
            {
                int oldIndex = 0;
                for (int i = 0; i < Items.Count; i++)
                {
                    if (Items[i].Title == "Order Status")
                    {
                        Items[i] = yes;
                        oldIndex = i;
                    }
                }
                Items.Move(oldIndex, Items.Count - 1);
            }
            else
            {
                var item = new OrderStatusViewModel();
                item.Closing += (s, e) => this.Items.Remove(item);
                this.Items.Add(item);
            }
        }

        private void ShowSalesReport()
        {

            var yes = Items.SingleOrDefault(x => x.Title == "Sales Report");
            if (yes != null)
            {
                int oldIndex = 0;
                for (int i = 0; i < Items.Count; i++)
                {
                    if (Items[i].Title == "Sales Report")
                    {
                        Items[i] = yes;
                        oldIndex = i;
                    }
                }
                Items.Move(oldIndex, Items.Count - 1);
            }
            else
            {
                var item = new DailySalesReportViewModel();
                item.Closing += (s, e) => this.Items.Remove(item);
                this.Items.Add(item);
            }
        }

        private void ShowInventory()
        {
            var yes = Items.SingleOrDefault(x => x.Title == "Product Maintenance");
            if (yes != null)
            {
                int oldIndex = 0;
                for (int i = 0; i < Items.Count; i++)
                {
                    if (Items[i].Title == "Product Maintenance")
                    {
                        Items[i] = yes;
                        oldIndex = i;
                    }
                }
                Items.Move(oldIndex, Items.Count - 1);
            }
            else
            {
                var item = new ProductMaintenanceViewModel();
                item.Closing += (s, e) => this.Items.Remove(item);
                this.Items.Add(item);
            }
        }

        private void ShowStockAdjustmentWindow()
        {
            var yes = Items.SingleOrDefault(x => x.Title == "Stock Adjustment");
            if (yes != null)
            {
                int oldIndex = 0;
                for (int i = 0; i < Items.Count; i++)
                {
                    if (Items[i].Title == "Stock Adjustment")
                    {
                        Items[i] = yes;
                        oldIndex = i;
                    }
                }
                Items.Move(oldIndex, Items.Count - 1);
            }
            else
            {
                var item = new StockAdjustmentViewModel();
                item.Closing += (s, e) => this.Items.Remove(item);
                this.Items.Add(item);
            }
        }

        private void ShowProductCount()
        {
            var yes = Items.SingleOrDefault(x => x.Title == "Inventory Report");
            if (yes != null)
            {
                int oldIndex = 0;
                for (int i = 0; i < Items.Count; i++)
                {
                    if (Items[i].Title == "Inventory Report")
                    {
                        Items[i] = yes;
                        oldIndex = i;
                    }
                }
                Items.Move(oldIndex, Items.Count - 1);
            }
            else
            {
                var item = new InventoryValueReportViewModel();
                item.Closing += (s, e) => this.Items.Remove(item);
                this.Items.Add(item);
            }
        }

        private void ShowProductPlaning()
        {
            var yes = Items.SingleOrDefault(x => x.Title == "Product Planning");
            if (yes != null)
            {
                int oldIndex = 0;
                for (int i = 0; i < Items.Count; i++)
                {
                    if (Items[i].Title == "Product Planning")
                    {
                        Items[i] = yes;
                        oldIndex = i;
                    }
                }
                Items.Move(oldIndex, Items.Count - 1);
            }
            else
            {
                var item = new ProductPlanningViewModel();
                item.Closing += (s, e) => this.Items.Remove(item);
                this.Items.Add(item);
            }
        }

        private void ShowSuppliers()
        {
            var yes = Items.SingleOrDefault(x => x.Title == "Supplier");
            if (yes != null)
            {
                int oldIndex = 0;
                for (int i = 0; i < Items.Count; i++)
                {
                    if (Items[i].Title == "Supplier")
                    {
                        Items[i] = yes;
                        oldIndex = i;
                    }
                }
                Items.Move(oldIndex, Items.Count - 1);
            }
            else
            {
                var item = new SupplierViewModel();
                item.Closing += (s, e) => this.Items.Remove(item);
                this.Items.Add(item);
            }
        }

        private void ShowNewPurchaseOrder()
        {
            var yes = Items.SingleOrDefault(x => x.Title == "New Purchasing Order");
            if (yes != null)
            {
                int oldIndex = 0;
                for (int i = 0; i < Items.Count; i++)
                {
                    if (Items[i].Title == "New Purchasing Order")
                    {
                        Items[i] = yes;
                        oldIndex = i;
                    }
                }
                Items.Move(oldIndex, Items.Count - 1);
            }
            else
            {
                var item = new AddPurchasingOrderViewModel();
                item.Closing += (s, e) => this.Items.Remove(item);
                this.Items.Add(item);
            }
        }

        private void ShowRecieving()
        {
            var yes = Items.SingleOrDefault(x => x.Title == "Order Receiving");
            if (yes != null)
            {
                int oldIndex = 0;
                for (int i = 0; i < Items.Count; i++)
                {
                    if (Items[i].Title == "Order Receiving")
                    {
                        Items[i] = yes;
                        oldIndex = i;
                    }
                }
                Items.Move(oldIndex, Items.Count - 1);
            }
            else
            {
                var item = new ReceivingViewModel();
                item.Closing += (s, e) => this.Items.Remove(item);
                this.Items.Add(item);
            }
        }

        private void ShowPrintPO()
        {
            var yes = Items.SingleOrDefault(x => x.Title == "Print Purchasing Order");
            if (yes != null)
            {
                int oldIndex = 0;
                for (int i = 0; i < Items.Count; i++)
                {
                    if (Items[i].Title == "Print Purchasing Order")
                    {
                        Items[i] = yes;
                        oldIndex = i;
                    }
                }
                Items.Move(oldIndex, Items.Count - 1);
            }
            else
            {
                var item = new PrintPurchaseOrderViewModel();
                item.Closing += (s, e) => this.Items.Remove(item);
                this.Items.Add(item);
            }
        }

        private void ShowShipping()
        {
            var yes = Items.SingleOrDefault(x => x.Title == "Shipping");
            if (yes != null)
            {
                int oldIndex = 0;
                for (int i = 0; i < Items.Count; i++)
                {
                    if (Items[i].Title == "Shipping")
                    {
                        Items[i] = yes;
                        oldIndex = i;
                    }
                }
                Items.Move(oldIndex, Items.Count - 1);
            }
            else
            {
                var item = new ShippingViewModel();
                item.Closing += (s, e) => this.Items.Remove(item);
                this.Items.Add(item);
            }
        }

        private void ShowCustomers()
        {

            var yes = Items.SingleOrDefault(x => x.Title == "Customer");
            if (yes != null)
            {
                int oldIndex = 0;

                for (int i = 0; i < Items.Count; i++)
                {
                    if (Items[i].Title == "Customer")
                    {
                        Items[i] = yes;
                        oldIndex = i;
                    }
                }

                Items.Move(oldIndex, Items.Count - 1);
            }
            else
            {
                var item = new CustomerViewModel();
                item.Closing += (s, e) => this.Items.Remove(item);
                this.Items.Add(item);
            }

        }

        private void ShowInvoice()
        {
            var yes = Items.SingleOrDefault(x => x.Title == "Invoicing");
            if (yes != null)
            {
                int oldIndex = 0;
                for (int i = 0; i < Items.Count; i++)
                {
                    if (Items[i].Title == "Invoicing")
                    {
                        Items[i] = yes;
                        oldIndex = i;
                    }
                }
                Items.Move(oldIndex, Items.Count - 1);
            }
            else
            {
                var item = new InvoicingActivityViewModel();
                item.Closing += (s, e) => this.Items.Remove(item);
                this.Items.Add(item);
            }
        }

        private void ShowCustomerPendingList()
        {
            var yes = Items.SingleOrDefault(x => x.Title == "Customer Pending List");
            if (yes != null)
            {
                int oldIndex = 0;
                for (int i = 0; i < Items.Count; i++)
                {
                    if (Items[i].Title == "Customer Pending List")
                    {
                        Items[i] = yes;
                        oldIndex = i;
                    }
                }
                Items.Move(oldIndex, Items.Count - 1);
            }
            else
            {
                var item = new CustomerPendingListViewModel();
                item.Closing += (s, e) => this.Items.Remove(item);
                this.Items.Add(item);
            }
        }

        private void ShowOnlineOrders()
        {
            var yes = Items.SingleOrDefault(x => x.Title == "Pending Orders");
            if (yes != null)
            {
                int oldIndex = 0;
                for (int i = 0; i < Items.Count; i++)
                {
                    if (Items[i].Title == "Pending Orders")
                    {
                        Items[i] = yes;
                        oldIndex = i;
                    }
                }
                Items.Move(oldIndex, Items.Count - 1);
            }
            else
            {
                var item = new PendingOrdersListViewModel();
                item.Closing += (s, e) => this.Items.Remove(item);
                this.Items.Add(item);
            }
        }

        private void ShowOrderHasNotLeft()
        {
            var yes = Items.SingleOrDefault(x => x.Title == "Inventory Shipped Not Invoiced");
            if (yes != null)
            {
                int oldIndex = 0;
                for (int i = 0; i < Items.Count; i++)
                {
                    if (Items[i].Title == "Inventory Shipped Not Invoiced")
                    {
                        Items[i] = yes;
                        oldIndex = i;
                    }
                }
                Items.Move(oldIndex, Items.Count - 1);
            }
            else
            {
                var item = new OrderHasNotLeftViewModel();
                item.Closing += (s, e) => this.Items.Remove(item);
                this.Items.Add(item);
            }

        }

        private void ShowOrderPoolReport()
        {
            var yes = Items.SingleOrDefault(x => x.Title == "Order Pool Report");
            if (yes != null)
            {
                int oldIndex = 0;
                for (int i = 0; i < Items.Count; i++)
                {
                    if (Items[i].Title == "Order Pool Report")
                    {
                        Items[i] = yes;
                        oldIndex = i;
                    }
                }
                Items.Move(oldIndex, Items.Count - 1);
            }
            else
            {
                var item = new OrderPoolReportViewModel();
                item.Closing += (s, e) => this.Items.Remove(item);
                this.Items.Add(item);
            }

        }

        private void ShowSchedulingReport()
        {
            var yes = Items.SingleOrDefault(x => x.Title == "Scheduling Report");
            if (yes != null)
            {
                int oldIndex = 0;
                for (int i = 0; i < Items.Count; i++)
                {
                    if (Items[i].Title == "Scheduling Report")
                    {
                        Items[i] = yes;
                        oldIndex = i;
                    }
                }
                Items.Move(oldIndex, Items.Count - 1);
            }
            else
            {
                var item = new SchedulingReportViewModel();
                item.Closing += (s, e) => this.Items.Remove(item);
                this.Items.Add(item);
            }
        }        


        private void ShowQuoteOrderReport()
        {
            var yes = Items.SingleOrDefault(x => x.Title == "Quote/Order Conversion Report");
            if (yes != null)
            {
                int oldIndex = 0;
                for (int i = 0; i < Items.Count; i++)
                {
                    if (Items[i].Title == "Quote/Order Conversion Report")
                    {
                        Items[i] = yes;
                        oldIndex = i;
                    }
                }
                Items.Move(oldIndex, Items.Count - 1);
            }
            else
            {
                var item = new QuoteOrderReportViewModel();
                item.Closing += (s, e) => this.Items.Remove(item);
                this.Items.Add(item);
            }
        }

        private void ShowMailChimp()
        {
            var yes = Items.SingleOrDefault(x => x.Title == "MailChimp Report");
            if (yes != null)
            {
                int oldIndex = 0;
                for (int i = 0; i < Items.Count; i++)
                {
                    if (Items[i].Title == "MailChimp Report")
                    {
                        Items[i] = yes;
                        oldIndex = i;
                    }
                }
                Items.Move(oldIndex, Items.Count - 1);
            }
            else
            {
                var item = new MailChimpViewModel();
                item.Closing += (s, e) => this.Items.Remove(item);
                this.Items.Add(item);
            }
        }

        private void ShowOpenQuotes()
        {
            var yes = Items.SingleOrDefault(x => x.Title == "Open Quotes");
            if (yes != null)
            {
                int oldIndex = 0;
                for (int i = 0; i < Items.Count; i++)
                {
                    if (Items[i].Title == "Open Quotes")
                    {
                        Items[i] = yes;
                        oldIndex = i;
                    }
                }
                Items.Move(oldIndex, Items.Count - 1);
            }
            else
            {
                var item = new OpenQuotesViewModel();
                item.Closing += (s, e) => this.Items.Remove(item);
                this.Items.Add(item);
            }
        }

        private void ShowOrdersReport()
        {
            var yes = Items.SingleOrDefault(x => x.Title == "Daily Order Report");
            if (yes != null)
            {
                int oldIndex = 0;
                for (int i = 0; i < Items.Count; i++)
                {
                    if (Items[i].Title == "Daily Order Report")
                    {
                        Items[i] = yes;
                        oldIndex = i;
                    }
                }
                Items.Move(oldIndex, Items.Count - 1);
            }
            else
            {
                var item = new OrdersReportViewModel();
                item.Closing += (s, e) => this.Items.Remove(item);
                this.Items.Add(item);
            }
        }

        private void ShowOrderItemsReport()
        {
            var yes = Items.SingleOrDefault(x => x.Title == "Periodic Order Report");
            if (yes != null)
            {
                int oldIndex = 0;
                for (int i = 0; i < Items.Count; i++)
                {
                    if (Items[i].Title == "Periodic Order Report")
                    {
                        Items[i] = yes;
                        oldIndex = i;
                    }
                }
                Items.Move(oldIndex, Items.Count - 1);
            }
            else
            {
                var item = new OrderItemsViewModel();
                item.Closing += (s, e) => this.Items.Remove(item);
                this.Items.Add(item);
            }
        }

        private void NavigateToView(string title)
        {
            if (title != null)
            {
                //Console.WriteLine(title.ToString());

                switch (title)
                {
                    case "A1RConsole.ViewModels.Quoting.ViewUpdateQuoteViewModel":
                        UpdateQuote();
                        break;
                    case "A1RConsole.ViewModels.Quoting.NewQuoteViewModel": Quote q = new Quote(); q = null;
                        NewQuote(q);
                        break;
                    case "A1RConsole.ViewModels.Orders.SalesOrders.NewSalesOrderViewModel":
                        ShowSalesOrder();
                        break;
                    case "A1RConsole.ViewModels.Orders.OrderStatusViewModel":
                        ShowOrderStatus();
                        break;
                    case "A1RConsole.ViewModels.Sales.DailySalesReportViewModel":
                        ShowSalesReport();
                        break;
                    case "A1RConsole.ViewModels.Products.ProductMaintenanceViewModel":
                        ShowInventory();
                        break;
                    case "A1RConsole.ViewModels.Stock.StockAdjustmentViewModel":
                        ShowStockAdjustmentWindow();
                        break;
                    case "A1RConsole.ViewModels.Stock.InventoryValueReportViewModel":
                        ShowProductCount();
                        break;
                    case "A1RConsole.ViewModels.Products.ProductPlanningViewModel":
                        ShowProductPlaning();
                        break;
                    case "A1RConsole.ViewModels.Suppliers.SupplierViewModel":
                        ShowSuppliers();
                        break;
                    case "A1RConsole.ViewModels.Purchasing.AddPurchasingOrderViewModel":
                        ShowNewPurchaseOrder();
                        break;
                    case "A1RConsole.ViewModels.Receiving.ReceivingViewModel":
                        ShowRecieving();
                        break;
                    case "A1RConsole.ViewModels.Purchasing.PrintPurchaseOrderViewModel":
                        ShowPrintPO();
                        break;
                    case "A1RConsole.ViewModels.Shipping.ShippingViewModel":
                        ShowShipping();
                        break;
                    case "A1RConsole.ViewModels.Customers.CustomerViewModel":
                        ShowCustomers();
                        break;
                    case "A1RConsole.ViewModels.Customers.CustomerPendingListViewModel":
                        ShowCustomerPendingList();
                        break;                    
                    case "A1RConsole.ViewModels.Orders.PendingOrders.PendingOrdersListViewModel":
                        ShowOnlineOrders();
                        break;
                    case "A1RConsole.ViewModels.Orders.NewOrderPDF.NewOrderPDFViewModel":
                        ShowNewOrderPDF(0,0);
                        break;
                    case "A1RConsole.ViewModels.Invoicing.InvoicingActivityViewModel":
                        ShowInvoice();
                        break;
                    case "A1RConsole.ViewModels.Invoicing.OrderHasNotLeftViewModel":
                        ShowOrderHasNotLeft();
                        break;
                    case "A1RConsole.ViewModels.Orders.QuoteOrderReportViewModel":
                        ShowQuoteOrderReport();
                        break;
                    case "A1RConsole.ViewModels.Quoting.OpenQuotesViewModel":
                        ShowOpenQuotes();
                        break;
                    case "A1RConsole.ViewModels.Orders.OrdersReportViewModel":
                        ShowOrdersReport();
                        break;
                    case "A1RConsole.ViewModels.Vjs.OrderItemsViewModel":
                        ShowOrderItemsReport();
                        break;
                    case "A1RConsole.ViewModels.Customers.MailChimpViewModel":
                        ShowMailChimp();
                        break;
                    case "A1RConsole.ViewModels.Vjs.OrderPoolReportViewModel":
                        ShowOrderPoolReport();
                        break;
                    case "A1RConsole.ViewModels.Vjs.SchedulingReportViewModel":
                        ShowSchedulingReport();
                        break;

                    default:
                        break;
                }
            }
        }

        private void LogOut()
        {

            if (MessageBox.Show("Hi " + UserData.FirstName + " " + UserData.LastName + System.Environment.NewLine + "Are you sure you want to Log out from A1 Rubber Console?", "Log Out Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes) == MessageBoxResult.Yes)
            {

                LoginView window = new LoginView();
                window.Show();

                Application.Current.Windows[0].Close();
            }
        }

        private void ProcessSalesOrder(object parameter)
        {
            int index = PendingSalesOrders.IndexOf(parameter as SalesOrder);
            if (index > -1 && index < PendingSalesOrders.Count)
            {
                if (MessageBox.Show("There is stock available to process sales order no : " + PendingSalesOrders[index].SalesOrderNo + System.Environment.NewLine + "Do you want to send this order to Warehouse?", "Sending To Warehouse", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes) == MessageBoxResult.Yes)
                {
                    //Process Sales Order
                    int res = 0;
                    BackgroundWorker worker = new BackgroundWorker();
                    ChildWindow LoadingScreen = new ChildWindow();
                    LoadingScreen.ShowWaitingScreen("Working");
                    worker.DoWork += (_, __) =>
                    {
                        StockManager stm = new StockManager();
                        List<SalesOrder> soList = new List<SalesOrder>();
                        soList.Add(PendingSalesOrders[index]);
                        res = stm.AllocateStockForOrder(soList);
                    };
                    worker.RunWorkerCompleted += delegate(object s, RunWorkerCompletedEventArgs args)
                    {
                        LoadingScreen.CloseWaitingScreen();
                        if (res == 1)
                        {
                            MessageBox.Show("Order sent to warehouse", "Order Sent", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else if (res == -1)
                        {
                            MessageBox.Show("Data has been changed since you opened this form!!!" + System.Environment.NewLine + "The form is closing now and please re-open again", "Data Changed", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                        else if (res == 0)
                        {
                            MessageBox.Show("You haven't made any changes to update", "No Changes Were Made", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show("There has been a problem when updating this order" + System.Environment.NewLine + "Err No - E12", "Cannot Update Order", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    };
                    worker.RunWorkerAsync();

                }
            }
        }

        private static bool CanMenuButtonPress(string button)
        {
            return true;
        }


        public IContent SelectedWindow
        {
            get
            {
                return this._selectedWindow;
            }
            set
            {
                this._selectedWindow = value;
                RaisePropertyChanged("SelectedWindow");
            }
        }

        public string SelectedTitle
        {
            get
            {
                return _selectedTitle;
            }
            set
            {
                _selectedTitle = value;
                RaisePropertyChanged("SelectedTitle");
                NavigateToView(SelectedTitle);
            }
        }

        public string Date
        {
            get
            {
                return _date;
            }
            set
            {
                _date = value;
                RaisePropertyChanged("Date");
            }
        }

        public ObservableCollection<IContent> Items
        {
            get
            {
                return _items;
            }
            set
            {
                _items = value;
                RaisePropertyChanged("Items");
            }
        }


        public string FullName
        {
            get
            {
                return _fullName;
            }
            set
            {
                _fullName = value;
                RaisePropertyChanged("FullName");
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



        public string LiveTIme
        {
            get
            {
                return _liveTIme;
            }
            set
            {
                _liveTIme = value;
                RaisePropertyChanged("LiveTIme");
            }
        }

        public bool IsSupplierEnabled
        {
            get
            {
                return _isSupplierEnabled;
            }
            set
            {
                _isSupplierEnabled = value;
                RaisePropertyChanged("IsSupplierEnabled");
            }
        }

        public bool IsInvoiceEnabled
        {
            get
            {
                return _isInvoiceEnabled;
            }
            set
            {
                _isInvoiceEnabled = value;
                RaisePropertyChanged("IsInvoiceEnabled");

            }
        }

        public bool IsPurchasingEnabled
        {
            get
            {
                return _isPurchasingEnabled;
            }
            set
            {
                _isPurchasingEnabled = value;
                RaisePropertyChanged("IsPurchasingEnabled");

            }
        }

        public bool IsNewQuoteEnabled
        {
            get
            {
                return _isNewQuoteEnabled;
            }
            set
            {
                _isNewQuoteEnabled = value;
                RaisePropertyChanged("IsNewQuoteEnabled");

            }
        }
        

        public bool IsShippingEnabled
        {
            get
            {
                return _isShippingEnabled;
            }
            set
            {
                _isShippingEnabled = value;
                RaisePropertyChanged("IsShippingEnabled");
            }
        }

        public bool IsProdPlanningEnabled
        {
            get
            {
                return _isProdPlanningEnabled;
            }
            set
            {
                _isProdPlanningEnabled = value;
                RaisePropertyChanged("IsProdPlanningEnabled");

            }
        }

        public bool IsInventoryEnabled
        {
            get
            {
                return _isInventoryEnabled;
            }
            set
            {
                _isInventoryEnabled = value;
                RaisePropertyChanged("IsInventoryEnabled");
            }
        }

        public bool IsSalesEnabled
        {
            get
            {
                return _isSalesEnabled;
            }
            set
            {
                _isSalesEnabled = value;
                RaisePropertyChanged("IsSalesEnabled");
            }
        }

        public bool IsQuotingEnabled
        {
            get
            {
                return _isQuotingEnabled;
            }
            set
            {
                _isQuotingEnabled = value;
                RaisePropertyChanged("IsQuotingEnabled");
            }
        }

        public bool IsOnlineOrdersEnabled
        {
            get
            {
                return _isOnlineOrdersEnabled;
            }
            set
            {
                _isOnlineOrdersEnabled = value;
                RaisePropertyChanged("IsOnlineOrdersEnabled");
            }
        }

        public bool IsNewOrderPDFEnabled
        {
            get
            {
                return _isNewOrderPDFEnabled;
            }
            set
            {
                _isNewOrderPDFEnabled = value;
                RaisePropertyChanged("IsNewOrderPDFEnabled");
            }
        }
               

        public bool IsCustomerEnabled
        {
            get
            {
                return _isCustomerEnabled;
            }
            set
            {
                _isCustomerEnabled = value;
                RaisePropertyChanged("IsCustomerEnabled");
            }
        }
        public bool IsSchedulingReportEnabled
        {
            get
            {
                return _isSchedulingReportEnabled;
            }
            set
            {
                _isSchedulingReportEnabled = value;
                RaisePropertyChanged("IsSchedulingReportEnabled");
            }
        }
        
        public bool IsCustomerPendingEnabled
        {
            get
            {
                return _isCustomerPendingEnabled;
            }
            set
            {
                _isCustomerPendingEnabled = value;
                RaisePropertyChanged("IsCustomerPendingEnabled");
            }
        }

        public ObservableCollection<SalesOrder> PendingSalesOrders
        {
            get
            {
                return _pendingSalesOrders;
            }
            set
            {
                _pendingSalesOrders = value;
                RaisePropertyChanged("PendingSalesOrders");
            }
        }

        public SalesOrder SelectedSalesOrder
        {
            get
            {
                return _selectedSalesOrder;
            }
            set
            {
                _selectedSalesOrder = value;
                RaisePropertyChanged("SelectedSalesOrder");
            }
        }

        public string SendToWarehouseVisibility
        {
            get
            {
                return _sendToWarehouseVisibility;
            }
            set
            {
                _sendToWarehouseVisibility = value;
                RaisePropertyChanged("SendToWarehouseVisibility");
            }
        }

        public bool IsOrdersNotLeftEnabled
        {
            get
            {
                return _isOrdersNotLeftEnabled;
            }
            set
            {
                _isOrdersNotLeftEnabled = value;
                RaisePropertyChanged("IsOrdersNotLeftEnabled");
            }
        }

        public bool IsOrderPoolReportEnabled
        {
            get
            {
                return _isOrderPoolReportEnabled;
            }
            set
            {
                _isOrderPoolReportEnabled = value;
                RaisePropertyChanged("IsOrderPoolReportEnabled");
            }
        }
        public bool IsInvoicingEnabled
        {
            get
            {
                return _isInvoicingEnabled;
            }
            set
            {
                _isInvoicingEnabled = value;
                RaisePropertyChanged("IsInvoicingEnabled");
            }
        }
        
        public bool IsOrderItemsEnabled
        {
            get
            {
                return _isOrderItemsEnabled;
            }
            set
            {
                _isOrderItemsEnabled = value;
                RaisePropertyChanged("IsOrderItemsEnabled");
            }
        }

        public bool IsOrdersEnabled
        {
            get
            {
                return _isOrdersEnabled;
            }
            set
            {
                _isOrdersEnabled = value;
                RaisePropertyChanged("IsOrdersEnabled");
            }
        }

        

        public bool IsVjsEnabled
        {
            get
            {
                return _isVjsEnabled;
            }
            set
            {
                _isVjsEnabled = value;
                RaisePropertyChanged("IsVjsEnabled");
            }
        }

        public bool IsMailChimp
        {
            get
            {
                return _isMailChimp;
            }
            set
            {
                _isMailChimp = value;
                RaisePropertyChanged("IsMailChimp");
            }
        }


        public ICommand MenuButtonPressCommand
        {
            get
            {
                if (_menuButtonPressCommand == null)
                {
                    _menuButtonPressCommand = new DelegateCommand<string>(ViewPage, CanMenuButtonPress);
                }
                return _menuButtonPressCommand;
            }
        }

        public ICommand LogOutCommand
        {
            get
            {
                return _logOutCommand ?? (_logOutCommand = new CommandHandler(() => LogOut(), true));
            }
        }

        private ICommand _removeItemCommand;
        public ICommand RemoveItemCommand
        {
            get
            {
                if (_removeItemCommand == null)
                {
                    _removeItemCommand = new A1RConsole.Commands.DelegateCommand(CanExecute, ViewUpdateOrder);
                }
                return _removeItemCommand;
            }
        }


        private bool CanExecute(object parameter)
        {
            return true;
        }
        private void ViewUpdateOrder(Object parameter)
        {
            if (parameter != null)
            {
                for (int i = 0; i < Items.Count; i++)
                {
                    if (Items[i].ToString() == parameter.ToString())
                    {
                        Items.RemoveAt(i);
                    }
                }
            }
        }


        public ICommand ProcessOrderCommand
        {
            get
            {
                if (_processOrderCommand == null)
                {
                    _processOrderCommand = new A1RConsole.Commands.DelegateCommand(CanExecute, ProcessSalesOrder);
                }
                return _processOrderCommand;
            }
        }

    }
}