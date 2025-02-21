using A1RConsole.Bases;
using A1RConsole.Commands;
using A1RConsole.Core;
using A1RConsole.DB;
using A1RConsole.Interfaces;
using A1RConsole.Models.Categories;
using A1RConsole.Models.Comments;
using A1RConsole.Models.Customers;
using A1RConsole.Models.Discounts;
using A1RConsole.Models.Freights;
using A1RConsole.Models.Meta;
using A1RConsole.Models.Orders;
using A1RConsole.Models.Products;
using A1RConsole.Models.Stock;
using A1RConsole.Models.Users;
using A1RConsole.ViewModels.Discounts;
using A1RConsole.Views.Discounts;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace A1RConsole.ViewModels.Orders.SalesOrders
{
    public class UpdateSalesOrderViewModel :  ViewModelBase
    {
        #region Private_Properties
        public DateTime CurrentDate { get; set; }

        private SalesOrder _salesOrder;
        private SalesOrder oldSalesOrder;
        private ObservableCollection<Customer> _customerList;
        public ObservableCollection<FreightCarrier> FreightList { get; set; }
        public ObservableCollection<Product> Product { get; set; }

        //public List<FreightCode> FreightCodesList { get; set; }
        private ObservableCollection<FreightCode> _freightCodeDetails;
        private ObservableCollection<FreightCode> _orgFreightCodeDetails;
        private List<DiscountStructure> _discountStructure;
        private List<DiscountStructure> _displayDiscountStructure;
        private string userName;
        private string state;
        private List<UserPrivilages> privilages;
        private List<MetaData> metaData;        
        private string _selectedOrderPriority;
        private string _selectedCustomer;
        private string _selectedStockLocation;
        private string _selectedCustomerType;
        //private string _prepaidCustomerVisibility;
        //private string _accountCustomerVisiblity;
        private string _selectedPaymentRecieved;
        private string _creditLimitVisibility;
        private string _orderStatusString;
        private decimal _creditLimit;
        private decimal _creditRemaining;
        private decimal _creditOwing;
        private decimal _totalOwed;
        private string _version;
        private string _statusBackGroundCol;
        private int closeFormVal;
        private List<StockLocation> stockLocation;
        private List<ProductStock> productStockList;
        //private string _paymentRecievedVisibility;
        private string _addUpdateBackground;
        private string _creditOweBackground;
        private string _creditOweForeground;
        private string _holdVisibility;
        private string _releaseVisibility;
        private string _cancelVisibility;
        private string _returnVisibility;
        private string _dispatchedVisibility;
        private bool _productGridEnabled;
        private bool _freightGridEnabled;
        private bool _carrierNameEnabled;
        //private bool _termsIDEnabled;
        private bool _pickUpTimeEnabled;
        private bool _warehouseCommentsEnabled;
        private bool _soldToEnabled;
        private bool _shipToEnabled;
        private bool _orderPriorityEnabled;
        private bool _customerOrderNoEnabled;
        private bool _dispatchDateEnabled;
        private bool _paymentRecievedEnabled;
        private bool _updateEnabled;
        private string _selectedOrderAction;
        private string _updateBackground;
        private string _paymentReceievedString;
        private string _warehouseComment;
        private string _transportComment;
        private int _itemCount;
        private bool canExecute;
        private bool _addUpdateActive;
        private bool _gSTActive;
        private bool _gSTEnabled;
        private string _quoteNo;
        private string _warehouseTxtLengthError;
        private string _transportTxtLengthError;
        private string _discountAppliedTextVisibility;
        private List<Tuple<string, Int16, string>> timeStamps;
        private List<DiscountStructure> discountList;
        public ObservableCollection<IContent> Items { get; private set; }
        public event Action<int> Closed;

        private ICommand _updateCommand;
        private ICommand _clearFieldsCommand;
        private ICommand _selectionChangedCommand;
        private ICommand _removeCommand;
        private ICommand _lostFocusCommand;
        private ICommand _calcFreightTotalCommand;
        private ICommand _removeFreightCodeCommand;
        private ICommand _addUpdateDiscountCommand;
        private ICommand _freightPriceKeyUpCommand;
        private ICommand _searchProductCommand;
        private ICommand _freightCodeChangedCommand;
        private ICommand _priceLostFocusCommand;
        private ICommand _discountLostFocusCommand;
        private ICommand _quantityChangedCommand;
        private Microsoft.Practices.Prism.Commands.DelegateCommand _closeCommand;

        #endregion

        public UpdateSalesOrderViewModel(string un, string s, List<UserPrivilages> p, List<MetaData> md, SalesOrder so, List<DiscountStructure> dsList, string openType, ObservableCollection<Customer> cl)
        {
            timeStamps = new List<Tuple<string, Int16, string>>();
            userName = UserData.FirstName + " " + UserData.LastName;
            state = s;
            privilages = p;
            metaData = md;

            //WarehouseTxtLengthError = "Max 300 characters";
            //TransportTxtLengthError = "Max 300 characters";
            this.Items = new ObservableCollection<IContent>();
            CurrentDate = Convert.ToDateTime(NTPServer.GetNetworkTime().ToString("dd/MM/yyyy"));
            canExecute = true;
            closeFormVal = 0;
            DiscountStructure = new List<DiscountStructure>();
            DisplayDiscountStructure = new List<DiscountStructure>();
            SalesOrder = new SalesOrder();
            SalesOrder.SalesOrderDetails = new ObservableCollection<SalesOrderDetails>();
            SalesOrder.FreightDetails = new BindingList<FreightDetails>();
            SalesOrder.Customer = new Customer();
            SalesOrder.Customer = so.Customer;
            CustomerList = new ObservableCollection<Customer>();
            CustomerList = cl;
            SalesOrder.FreightCarrier = new FreightCarrier();
            SalesOrder.OrderDate = DateTime.Now;
            SalesOrder.PickupTime = so.PickupTime;
            SalesOrder.OrderAction = so.OrderAction;
            SelectedOrderAction = SalesOrder.OrderAction;
            SalesOrder.Comments = new List<Comment>();
            SalesOrder.TermsID = so.TermsID;
            SalesOrder.StockLocation = new StockLocation();

            GSTActive = so.GSTEnabled;
            //PaymentRecievedVisibility = "Collapsed";
            CreditLimitVisibility = "Collapsed";
            AddUpdateBackground = "#bdbdbd";
            DiscountAppliedTextVisibility = "Collapsed";
            discountList = dsList;
            SalesOrder.StockLocation.ID = ConvertStockLocationToInt();

            FreightCodeDetails = new ObservableCollection<FreightCode>();
            OrgFreightCodeDetails = new ObservableCollection<FreightCode>();
            Product = new ObservableCollection<Product>();

            LoadProducts();
            LoadFreights();
            LoadProductStock();
            LoadFreightCodes();

            oldSalesOrder = so;
            if (openType != "SearchProduct")
            {
                SalesOrder = DBAccess.GetSalesOrderDetails(so.SalesOrderNo);
            }
            else
            {
                SalesOrder = oldSalesOrder;
            }
                      

            SalesOrder.StockReserved = so.StockReserved;

            if (SalesOrder.SalesOrderNo > 0)
            {
                if (SalesOrder.Customer.CompanyName != null && SalesOrder.Customer.CustomerType == "Account")
                {
                    SelectedCustomer = SalesOrder.Customer.CompanyName;
                    //Get credit owe                
                    //CreditOwing = DBAccess.GetCustomerCreditOwe(so.SalesOrderNo, so.Customer.CustomerId);
                    CreditOwing = so.Customer.Debt;
                    TotalOwed = so.Customer.CreditOwed;
                    if (so.Customer.Debt > 0)
                    {
                        CreditOweBackground = "#db2503";
                        CreditOweForeground = "White";
                    }
                    else
                    {
                        CreditOweBackground = "White";
                        CreditOweForeground = "black";
                    }                   
                }
                else if ((SalesOrder.Customer.CompanyName != null && SalesOrder.Customer.CustomerType == "Prepaid"))
                {
                    //SalesOrder.PrepaidCustomerName = SalesOrder.Customer.CompanyName;
                    SelectedCustomer = SalesOrder.Customer.CompanyName;
                    CreditOweBackground = "White";
                    CreditOweForeground = "black";
                }
                else if (SalesOrder.Customer.CompanyName == null && SalesOrder.Customer.CustomerType == "Prepaid")
                {
                    SelectedCustomer = SalesOrder.PrepaidCustomerName;
                    //SalesOrder.PrepaidCustomerName = SalesOrder.PrepaidCustomerName;
                    CreditOweBackground = "White";
                    CreditOweForeground = "black";
                }

                //string os = string.Empty;
                //if (SalesOrder.OrderStatus != OrderStatus.Dispatched.ToString() && SalesOrder.OrderStatus != OrderStatus.Return.ToString())
                //{
                //    os = " (STOCK RESERVED - " + so.StockReserved.ToUpper() + ")";
                //}
                //else if (SalesOrder.OrderStatus == OrderStatus.Return.ToString())
                //{
                //    os = " (STOCK RETURNED)";
                //}
                OrderManager om = new OrderManager();
                DiscountStructure = DBAccess.GetDiscount(so.Customer.CustomerId);
                SelectedCustomerType = SalesOrder.Customer.CustomerType;
                SelectedStockLocation = SalesOrder.StockLocation.StockName;
                //SelectedPaymentRecieved = SalesOrder.PaymentRecieved == true ? "Yes" : "No";
                SelectedOrderPriority = ConvertOrderTypeToString(SalesOrder.OrderPriority);
                SalesOrder.OrderAction = om.ConvertToOrderStatus(SalesOrder.OrderStatus);
                SelectedOrderAction = SalesOrder.OrderAction;
                SalesOrder.DesiredDispatchDate = so.DesiredDispatchDate;
                SalesOrder.FreightCarrier.FreightName = so.FreightCarrier.FreightName;
                //OrderStatusString = "CURRENT STATUS - " + CoreProcess.ConvertOrderStatusEnumInformative(SalesOrder.OrderStatus).ToUpper() + os;
                OrderStatusString = "CURRENT STATUS - " + CoreProcess.ConvertOrderStatusEnumInformative(SalesOrder.OrderStatus).ToUpper();
                StatusBackGroundCol = ChangeStatusBackgroundCol(SalesOrder.OrderStatus);
                SalesOrder.SalesOrderDetails.CollectionChanged += productChanged;
                SalesOrder.FreightDetails.ListChanged += freightChanged;
                this.SalesOrder.SalesOrderDetails = SequencingService.SetCollectionSequence(this.SalesOrder.SalesOrderDetails);
                timeStamps = DBAccess.GetUpdateSalesOrderTimeStamp(SalesOrder.SalesOrderNo);

                //Commments
                foreach (var item in SalesOrder.Comments)
                {
                    if (item.LocationID == 7)
                    {
                        WarehouseComment = item.Note;
                    }
                    else if (item.LocationID == 8)
                    {
                        TransportComment = item.Note;
                    }
                }


                for (int i = 0; i < FreightCodeDetails.Count; i++)
                {
                    var d = SalesOrder.FreightDetails.SingleOrDefault(x => x.FreightCodeDetails.FreightCodeID == FreightCodeDetails[i].FreightCodeID);
                    if (d != null)
                    {
                        FreightCodeDetails[i].Price = d.FreightCodeDetails.Price;
                    }
                }

                              
            }
            else
            {
                MessageBox.Show("This sales order no longer exist", "Sales Order Does Not Exist", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            QuoteNo = SalesOrder.QuoteNo == 0 ? "" : SalesOrder.QuoteNo.ToString();
            SalesOrder.SalesCompletedBy = userName;

            _closeCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(CloseForm);

            ActivateDeactivateOrderActionVisibility();
        }

        

        private void LoadCustomers()
        {
            CustomerList = DBAccess.GetCustomerData();
        }

        private void LoadFreights()
        {
            FreightList = DBAccess.GetFreightData();
            FreightList.Add(new FreightCarrier() { FreightName = "Select", FreightDescription = "--" });
        }

        private void LoadProducts()
        {
            //BackgroundWorker worker = new BackgroundWorker();
            //ChildWindow LoadingScreen = new ChildWindow();
            //LoadingScreen.ShowWaitingScreen("Loading");

            //worker.DoWork += (_, __) =>
            //{
                Product = DBAccess.GetAllProds(true);
            //};

            //worker.RunWorkerCompleted += delegate(object s, RunWorkerCompletedEventArgs args)
            //{
            //    LoadingScreen.CloseWaitingScreen();
            //};
            //worker.RunWorkerAsync();
        }

        private void LoadFreightCodes()
        {
            OrgFreightCodeDetails = DBAccess.GetFreightCodes();

            FreightCodeDetails = new ObservableCollection<FreightCode>(OrgFreightCodeDetails);
        }

        private void LoadProductStock()
        {
            productStockList = DBAccess.GetProductStockByStock(1);//Get product stock
        }

        private void LoadStockLocations()
        {
            stockLocation = DBAccess.GetAllStockLocations();
        }

        private int ConvertStockLocationToInt()
        {
            int id = 0;
            switch (SelectedStockLocation)
            {
                case "QLD": id = 1;
                    break;
                case "NSW": id = 2;
                    break;
                default:
                    break;
            }
            return id;
        }

        private void SearchProduct(object parameter)
        {
            int index = SalesOrder.SalesOrderDetails.IndexOf(parameter as SalesOrderDetails);
            if (index > -1 && index < SalesOrder.SalesOrderDetails.Count)
            {
                SalesOrder.SalesOrderDetails[index].IsEditing = true;
                var childWindow = new ChildWindow();
                childWindow.productCodeSearch_Closed += (r =>
                {
                    if (r != null)
                    {
                        //SalesOrder.SalesOrderDetails[index].Product = r;
                        //SalesOrder.SalesOrderDetails[index].Product.ProductCode = r.ProductCode;


                    }
                });
                childWindow.ShowProductSearch(userName, state, privilages, metaData,  SalesOrder, discountList, "Update");
            }
        }

        private void UpdateOrder()
        {
            bool exist = DBAccess.CheckRecordExist(SalesOrder.SalesOrderNo, "SalesOrder");
            if (exist)
            {            

                LoadProductStock();
                bool checkPriceNotZero = false;
                bool dupExist = false;
                RemoveZeroSalesOrderDetails();
                RemoveZeroFreightDetails();

                var prodExists = SalesOrder.SalesOrderDetails.Where(x => x.Product != null && x.Quantity != 0).ToList();
                var freightExists = SalesOrder.FreightDetails.Where(x => x.FreightCodeDetails != null && x.Pallets != 0).ToList();

                if (SalesOrder.SalesOrderDetails != null)
                {
                    //Checking duplicates
                    string[] prods = null;
                    prods = MetaDataManager.GetPriceEditingProducts();

                    if (prods != null)
                    {
                        var duplicates = SalesOrder.SalesOrderDetails.GroupBy(s => s.Product.ProductID)
                                     .Where(g => g.Count() > 1)
                                     .Select(g => g.Key);

                        if (duplicates.Count() > 0)
                        {
                            List<int> d = new List<int>(duplicates);

                            for (int i = 0; i < d.Count; i++)
                            {
                                bool exists = prods.Any(x => Convert.ToInt16(x) == d[i]);
                                if (exists)
                                {
                                    d.RemoveAt(i);
                                }   
                            }
                            dupExist = d.Count > 0;
                        }
                    }
                    //Check prices are zero
                    checkPriceNotZero = SalesOrder.SalesOrderDetails.Any(x => x.QuoteUnitPrice == 0);
                }

                ObservableCollection<SalesOrderDetails> tempSalesOrderDetails = new ObservableCollection<SalesOrderDetails>();
                foreach (var item in SalesOrder.SalesOrderDetails)
                {
                    tempSalesOrderDetails.Add(new SalesOrderDetails() { Product = new Product() { ProductID = item.Product.ProductID } });
                }

                if (prodExists.Count == 0)
                {
                    MessageBox.Show("Please enter qty and product", "Details Required", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.Yes);
                }
                else if (SalesOrder.FreightCarrier.FreightName != "Customer Collect" && freightExists.Count == 0)
                {
                    MessageBox.Show("Please enter no of pallets and freight code", "Details Required", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.Yes);
                }
                else if (dupExist == true)
                {
                    MessageBox.Show("Duplicate products exist. Please remove the duplicate products ", "Duplicate Products Exist", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.Yes);
                }
                else if (checkPriceNotZero)
                {
                    MessageBox.Show("Enter price for product(s)", "Price Required", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else if (SelectedCustomerType == "Account" && SelectedCustomer == "Select")
                {
                    MessageBox.Show("Please select Customer", "Customer Required", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else if (SelectedCustomerType == "Prepaid" && SelectedCustomer == "Select" && String.IsNullOrWhiteSpace(SalesOrder.PrepaidCustomerName))
                {
                    MessageBox.Show("Please enter customer name", "Enter Customer Name", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else if (SalesOrder.DesiredDispatchDate == null)
                {
                    MessageBox.Show("Please select Dispatch Date", "Dispatch Date Required", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else if (SalesOrder.OrderAction == "Select")
                {
                    MessageBox.Show("Please select Order Status", "Order Status Required", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else if (SalesOrder.FreightCarrier.FreightName == "Select")
                {
                    MessageBox.Show("Please select Carrier", "Carrier Name Required", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else if (SelectedOrderAction == OrderStatus.Return.ToString() && CheckReturningItems() == false)
                {
                }
                else
                {
                    Tuple<bool, SalesOrder, List<ProductStockReserved>, List<ProductStock>, bool, Tuple<CustomerCreditHistory, CustomerCreditActivity>> result = null;
                    SalesOrder.Customer.CustomerType = SelectedCustomerType;
                    SalesOrder.OrderPriority = CoreProcess.ConvertOrderTypeToInt(SelectedOrderPriority);
                    OrderManager om = new OrderManager();
                    om.InitiateSalesOrder(CoreProcess.GetStockLocationId(SelectedStockLocation), productStockList);
                    SalesOrder.GSTEnabled = GSTActive;
                    /***If the sales order is cancel, make sure the previous details have been restored before submition**/
                    if (SelectedOrderAction == OrderStatus.Cancel.ToString())
                    {
                        SalesOrder = DBAccess.GetSalesOrderDetails(SalesOrder.SalesOrderNo);
                        SelectedOrderPriority = ConvertOrderTypeToString(SalesOrder.OrderPriority);
                        GSTActive = SalesOrder.GSTEnabled;
                    }
                    /*****************************************************************************************************/
                    /*********************************************COMMENTS************************************************/
                    SalesOrder = ProcessComments(SalesOrder);
                    /*****************************************************************************************************/

                    /******************************************FIX FREIGHT************************************************/
                    if (SalesOrder.FreightCarrier.Id == 6)
                    {
                        SalesOrder.FreightDetails = new BindingList<FreightDetails>();
                        FreightDetails f = new FreightDetails();
                        FreightCode fc = new FreightCode();

                        fc.ID = 51;
                        fc.Description = "";
                        fc.Price = 0;
                        f.Pallets = 0;
                        f.Total = 0;
                        f.FreightCodeDetails = fc;

                        SalesOrder.FreightDetails.Add(f);
                    }
                    /*****************************************************************************************************/
                    /******************CHECK DATA CHANGE SEQUANCE**********************/

                    #region Checking_data_change

                    bool salesOrderChanged = false;
                    bool cusDataChanged = false;
                    bool stockHasChanged = false;
                    bool productHasChanged = false;
                    bool freightHasChanged = false;
                    bool carrierHasChanged = false;
                    bool freightDetailsHasChanged = false;
                    bool commentHasChanged = false;
                    bool discountHasChanged = false;
                    bool newDiscountAdded = false;
                    bool discountRemoved = false;
                    List<Category> removedDiscounts = new List<Category>();
                    string[] prods = null;
                    prods = MetaDataManager.GetPriceEditingProducts();
                    List<Tuple<string, string, Int32>> tup = CheckDataChangeManager.CheckSalesOrderChanged(CustomerList, SalesOrder, SelectedCustomer, CoreProcess.GetStockLocationId(SelectedStockLocation));

                    //Data has changed
                    if (tup.Count > 0)
                    {
                        string msg = string.Empty;
                        Int32 cusId = 0;

                        var dataC = CustomerList.FirstOrDefault(s => s.CompanyName == SelectedCustomer);

                        if (dataC != null)
                        {
                            cusId = dataC.CustomerId;
                        }

                        Tuple<Customer, List<ProductStock>, List<FreightCode>, FreightCarrier, SalesOrder> updatedDetails = CheckDataChangeManager.GetUpdatedSalesOrderDetails(cusId, CoreProcess.GetStockLocationId(SelectedStockLocation), SalesOrder);


                        //If customer is available
                        if (updatedDetails.Item1 != null)
                        {
                            if (!SelectedCustomer.Equals(updatedDetails.Item1.CompanyName))
                            {
                                SelectedCustomer = updatedDetails.Item1.CompanyName;
                                cusDataChanged = true;
                            }

                            if (!SelectedCustomerType.Equals(updatedDetails.Item1.CustomerType))
                            {
                                SelectedCustomerType = updatedDetails.Item1.CustomerType;
                                cusDataChanged = true;
                            }

                            if (updatedDetails.Item1.CustomerType == "Account" && dataC.StopCredit != updatedDetails.Item1.StopCredit)
                            {
                                dataC.StopCredit = updatedDetails.Item1.StopCredit;
                                cusDataChanged = true;
                            }

                            if (updatedDetails.Item1.CustomerType == "Account" && CreditLimit != updatedDetails.Item1.CreditLimit)
                            {
                                CreditLimit = updatedDetails.Item1.CreditLimit;
                                cusDataChanged = true;
                            }

                            if (updatedDetails.Item1.CustomerType == "Account" && CreditRemaining != updatedDetails.Item1.CreditRemaining)
                            {
                                CreditRemaining = updatedDetails.Item1.CreditRemaining;
                                cusDataChanged = true;
                            }

                            if (updatedDetails.Item1.CustomerType == "Account" && TotalOwed != updatedDetails.Item1.CreditOwed)
                            {
                                TotalOwed = updatedDetails.Item1.CreditOwed;
                                cusDataChanged = true;
                            }

                            if (!SalesOrder.BillTo.Equals(updatedDetails.Item1.CompanyAddress + System.Environment.NewLine + updatedDetails.Item1.CompanyCity + System.Environment.NewLine + updatedDetails.Item1.CompanyState + System.Environment.NewLine + updatedDetails.Item1.CompanyPostCode))
                            {
                                SalesOrder.BillTo = updatedDetails.Item1.CompanyAddress + System.Environment.NewLine + updatedDetails.Item1.CompanyCity + System.Environment.NewLine + updatedDetails.Item1.CompanyState + System.Environment.NewLine + updatedDetails.Item1.CompanyPostCode;
                                cusDataChanged = true;
                            }

                            if (!SalesOrder.ShipTo.Equals(updatedDetails.Item1.ShipAddress + System.Environment.NewLine + updatedDetails.Item1.ShipCity + System.Environment.NewLine + updatedDetails.Item1.ShipState + System.Environment.NewLine + updatedDetails.Item1.ShipPostCode))
                            {
                                SalesOrder.ShipTo = updatedDetails.Item1.ShipAddress + System.Environment.NewLine + updatedDetails.Item1.ShipCity + System.Environment.NewLine + updatedDetails.Item1.ShipState + System.Environment.NewLine + updatedDetails.Item1.ShipPostCode;
                                cusDataChanged = true;
                            }

                            //Discount
                            List<DiscountStructure> tempDiscountStructure = new List<DiscountStructure>();
                            foreach (var item in DisplayDiscountStructure)
                            {
                                tempDiscountStructure.Add(item);
                            }
                            foreach (var item in updatedDetails.Item1.DiscountStructure)
                            {
                                var data = tempDiscountStructure.SingleOrDefault(x => x.Category.CategoryID == item.Category.CategoryID);
                                if (data != null)//If the discount is available in the list
                                {
                                    if (data.Category.CategoryID == item.Category.CategoryID)
                                    {
                                        if (!item.TimeStamp.Equals(data.TimeStamp))
                                        {
                                            data.Discount = item.Discount;
                                            data.DiscountStr = item.Category.CategoryName + " " + item.Discount + "%";
                                            data.TimeStamp = item.TimeStamp;
                                            discountHasChanged = true;
                                            newDiscountAdded = true;
                                        }
                                    }
                                }
                                else
                                {
                                    tempDiscountStructure.Add(item);
                                    discountHasChanged = true;
                                    newDiscountAdded = true;
                                }
                            }

                            for (int i = 0; i < tempDiscountStructure.Count; i++)
                            {
                                var data = updatedDetails.Item1.DiscountStructure.SingleOrDefault(x => x.Category.CategoryID == tempDiscountStructure[i].Category.CategoryID);
                                if (data == null)
                                {
                                    removedDiscounts.Add(new Category() { CategoryID = tempDiscountStructure[i].Category.CategoryID, CategoryName = tempDiscountStructure[i].Category.CategoryName });
                                    tempDiscountStructure.RemoveAt(i);
                                    discountHasChanged = true;
                                    discountRemoved = true;
                                }
                            }

                            if (discountHasChanged)
                            {
                                DisplayDiscountStructure = new List<DiscountStructure>();
                                tempDiscountStructure.RemoveAll(x => x.Discount == 0);
                                DisplayDiscountStructure = tempDiscountStructure;
                            }
                        }

                        if (SalesOrder.SalesOrderNo > 0)
                        {
                            //Check Sales Order
                            if (updatedDetails.Item5 != null)
                            {
                                if (!SalesOrder.CustomerOrderNo.Equals(updatedDetails.Item5.CustomerOrderNo))
                                {
                                    SalesOrder.CustomerOrderNo = updatedDetails.Item5.CustomerOrderNo;
                                    SalesOrder.TimeStamp = updatedDetails.Item5.TimeStamp;
                                    salesOrderChanged = true;
                                }

                                if (SalesOrder.DesiredDispatchDate != updatedDetails.Item5.DesiredDispatchDate)
                                {
                                    SalesOrder.DesiredDispatchDate = updatedDetails.Item5.DesiredDispatchDate;
                                    SalesOrder.TimeStamp = updatedDetails.Item5.TimeStamp;
                                    salesOrderChanged = true;
                                }

                                if (SalesOrder.FreightCarrier.FreightName != updatedDetails.Item5.FreightCarrier.FreightName)
                                {
                                    //SalesOrder.FreightCarrier = updatedDetails.Item5.FreightCarrier;
                                    SalesOrder.FreightCarrier.Id = updatedDetails.Item5.FreightCarrier.Id;
                                    SalesOrder.FreightCarrier.FreightName = updatedDetails.Item5.FreightCarrier.FreightName;
                                    SalesOrder.FreightCarrier.FreightDescription = updatedDetails.Item5.FreightCarrier.FreightDescription;
                                    SalesOrder.TimeStamp = updatedDetails.Item5.TimeStamp;
                                    carrierHasChanged = true;
                                }

                                OrderManager om2 = new OrderManager();
                                string orderStatus = om2.ConvertToOrderStatus(updatedDetails.Item5.OrderStatus);
                                if (!SelectedOrderAction.Equals(orderStatus))
                                {
                                    SelectedOrderAction = orderStatus;
                                    SalesOrder.OrderAction = orderStatus;
                                    SalesOrder.TimeStamp = updatedDetails.Item5.TimeStamp;
                                    salesOrderChanged = true;
                                }
                                //Order  details
                                foreach (var item in updatedDetails.Item5.SalesOrderDetails)
                                {
                                    //Check if it is OffSpec or Rotatub custom
                                    //bool exists = false;
                                    //if (prods != null)
                                    //{
                                    //    exists = prods.Any(x => Convert.ToInt16(x) == item.Product.ProductID);
                                    //}

                                    //if (exists == false)
                                    //{
                                        var has = SalesOrder.SalesOrderDetails.SingleOrDefault(x => x.SalesOrderDetailsID == item.SalesOrderDetailsID);
                                        if (has == null)
                                        {
                                            productHasChanged = true;
                                        }
                                        else
                                        {
                                            if (!has.Product.TimeStamp.Equals(item.Product.TimeStamp) || !has.SODTimeStamp.Equals(item.SODTimeStamp) || !has.PSTimeStamp.Equals(item.PSTimeStamp))
                                            {
                                                productHasChanged = true;
                                            }
                                        }
                                    //}
                                }

                                if (productHasChanged)
                                {
                                    //SalesOrder.SalesOrderDetails.Clear();
                                    //SalesOrder.SalesOrderDetails = updatedDetails.Item5.SalesOrderDetails;
                                    foreach (var item in updatedDetails.Item5.SalesOrderDetails)
                                    {
                                        var data = SalesOrder.SalesOrderDetails.SingleOrDefault(x => x.SalesOrderDetailsID == item.SalesOrderDetailsID);
                                        if (data != null)
                                        {
                                            if (data.Quantity != item.Quantity)
                                            {
                                                data.Quantity = item.Quantity;
                                                productHasChanged = true;
                                            }

                                            if (!data.Product.ProductCode.Equals(item.Product.ProductCode))
                                            {
                                                data.Product.ProductCode = item.Product.ProductCode;
                                                productHasChanged = true;
                                            }

                                            if (!data.Product.ProductDescription.Equals(item.Product.ProductDescription))
                                            {
                                                data.Product.ProductDescription = item.Product.ProductDescription;
                                                productHasChanged = true;
                                            }

                                            if (!data.Product.ProductUnit.Equals(item.Product.ProductUnit))
                                            {
                                                data.Product.ProductUnit = item.Product.ProductUnit;
                                                productHasChanged = true;
                                            }

                                            if (data.Product.UnitPrice != item.Product.UnitPrice)
                                            {
                                                data.Product.UnitPrice = item.Product.UnitPrice;
                                                productHasChanged = true;
                                            }

                                            if (data.Discount != item.Discount)
                                            {
                                                data.Discount = item.Discount;
                                                productHasChanged = true;
                                            }
                                            if (data.QtyInStock != item.QtyInStock)
                                            {
                                                data.QtyInStock = item.QtyInStock;
                                                productHasChanged = true;
                                            }

                                            data.Total = item.Total;
                                        }
                                        else
                                        {
                                            SalesOrder.SalesOrderDetails.Add(item);
                                            productHasChanged = true;
                                        }
                                    }

                                    for (int i = 0; i < SalesOrder.SalesOrderDetails.Count; i++)
                                    {
                                        var data = updatedDetails.Item5.SalesOrderDetails.SingleOrDefault(x => x.SalesOrderDetailsID == SalesOrder.SalesOrderDetails[i].SalesOrderDetailsID);
                                        if (data == null)
                                        {
                                            SalesOrder.SalesOrderDetails.RemoveAt(i);
                                            productHasChanged = true;
                                        }
                                    }
                                }

                                //Freight details
                                foreach (var item in updatedDetails.Item5.FreightDetails)
                                {
                                    var has = SalesOrder.FreightDetails.SingleOrDefault(x => x.FreightCodeDetails.FreightCodeID == item.FreightCodeDetails.FreightCodeID);
                                    if (has == null)
                                    {
                                        freightDetailsHasChanged = true;
                                    }
                                    else
                                    {
                                        if (item.FreightCodeDetails.FreightCodeID == has.FreightCodeDetails.FreightCodeID && !item.TimeStamp.Equals(has.TimeStamp))
                                        {

                                            freightDetailsHasChanged = true;
                                        }
                                    }
                                }

                                for (int i = 0; i < SalesOrder.FreightDetails.Count; i++)
                                {
                                    var data = updatedDetails.Item5.FreightDetails.SingleOrDefault(x => x.FreightCodeDetails.FreightCodeID == SalesOrder.FreightDetails[i].FreightCodeDetails.FreightCodeID);
                                    if (data == null)
                                    {
                                        SalesOrder.FreightDetails.RemoveAt(i);
                                        freightDetailsHasChanged = true;
                                    }
                                }

                                if (freightDetailsHasChanged)
                                {
                                    SalesOrder.FreightDetails.Clear();
                                    SalesOrder.FreightDetails = updatedDetails.Item5.FreightDetails;
                                }

                                //Comments

                                for (int i = 0; i < SalesOrder.Comments.Count; i++)
                                {
                                    foreach (var item in updatedDetails.Item5.Comments)
                                    {
                                        if (!SalesOrder.Comments[i].Note.Equals(item.Note) && SalesOrder.Comments[i].LocationID == item.LocationID)
                                        {
                                            SalesOrder.Comments[i].Note = item.Note;
                                            SalesOrder.Comments[i].Note = item.TimeStamp;

                                            if (item.LocationID == 7)
                                            {
                                                WarehouseComment = item.Note;
                                            }
                                            if (item.LocationID == 8)
                                            {
                                                TransportComment = item.Note;
                                            }
                                            commentHasChanged = true;
                                        }
                                    }
                                }

                            }
                        }
                        else
                        {
                            //Check sales order details
                            for (int i = 0; i < SalesOrder.SalesOrderDetails.Count; i++)
                            {
                                foreach (var items in updatedDetails.Item2)
                                {
                                    if (SalesOrder.SalesOrderDetails[i].Product.ProductID == items.Product.ProductID && SalesOrder.SalesOrderDetails[i].QtyInStock != items.QtyAvailable)
                                    {
                                        SalesOrder.SalesOrderDetails[i].QtyInStock = items.QtyAvailable;
                                        stockHasChanged = true;
                                    }

                                    if (SalesOrder.SalesOrderDetails[i].Product.ProductID == items.Product.ProductID && (SalesOrder.SalesOrderDetails[i].Product.Category.CategoryID != items.Product.Category.CategoryID ||
                                                                                              SalesOrder.SalesOrderDetails[i].Product.CommodityCode != items.Product.CommodityCode ||
                                                                                              SalesOrder.SalesOrderDetails[i].Product.UnitPrice != items.Product.UnitPrice ||
                                                                                              !SalesOrder.SalesOrderDetails[i].Product.ProductCode.Equals(items.Product.ProductCode) ||
                                                                                              !SalesOrder.SalesOrderDetails[i].Product.ProductDescription.Equals(items.Product.ProductDescription) ||
                                                                                              SalesOrder.SalesOrderDetails[i].Product.IsRawMaterial != items.Product.IsRawMaterial) ||
                                                                                              SalesOrder.SalesOrderDetails[i].Product.Active != items.Product.Active)
                                    {

                                        SalesOrder.SalesOrderDetails[i].Product.Category.CategoryID = items.Product.Category.CategoryID;
                                        SalesOrder.SalesOrderDetails[i].Product.CommodityCode = items.Product.CommodityCode;
                                        SalesOrder.SalesOrderDetails[i].Product.UnitPrice = items.Product.UnitPrice;
                                        SalesOrder.SalesOrderDetails[i].Product.ProductCode = items.Product.ProductCode;
                                        SalesOrder.SalesOrderDetails[i].Product.ProductDescription = items.Product.ProductDescription;
                                        SalesOrder.SalesOrderDetails[i].Product.IsRawMaterial = items.Product.IsRawMaterial;
                                        SalesOrder.SalesOrderDetails[i].Product.Active = items.Product.Active;
                                        SalesOrder.SalesOrderDetails[i].Product.TimeStamp = items.Product.TimeStamp;

                                        decimal subTotal = SalesOrder.SalesOrderDetails[i].Quantity * SalesOrder.SalesOrderDetails[i].Product.UnitPrice;
                                        decimal disTotal = (subTotal * SalesOrder.SalesOrderDetails[i].Discount) / 100;
                                        SalesOrder.SalesOrderDetails[i].Total = subTotal - disTotal;

                                        productHasChanged = true;
                                    }
                                }
                            }

                            var itemToRemove = SalesOrder.SalesOrderDetails.FirstOrDefault(r => r.Product.Active == false || r.Product.IsRawMaterial);
                            SalesOrder.SalesOrderDetails.Remove(itemToRemove);

                            //Check Freight has changed
                            for (int i = 0; i < SalesOrder.FreightDetails.Count; i++)
                            {
                                foreach (var items in updatedDetails.Item3)
                                {
                                    if (SalesOrder.FreightDetails[i].FreightCodeDetails.ID == items.FreightCodeID)
                                    {
                                        if (!SalesOrder.FreightDetails[i].FreightCodeDetails.Code.Equals(items.Code))
                                        {
                                            SalesOrder.FreightDetails[i].FreightCodeDetails.Code = items.Code;
                                            SalesOrder.FreightDetails[i].FreightCodeDetails.TimeStamp = items.TimeStamp;
                                            freightHasChanged = true;
                                        }

                                        if (!SalesOrder.FreightDetails[i].FreightCodeDetails.Description.Equals(items.Description))
                                        {
                                            SalesOrder.FreightDetails[i].FreightCodeDetails.Description = items.Description;
                                            SalesOrder.FreightDetails[i].FreightCodeDetails.TimeStamp = items.TimeStamp;
                                            freightHasChanged = true;
                                        }

                                        if (!SalesOrder.FreightDetails[i].FreightCodeDetails.Unit.Equals(items.Unit))
                                        {
                                            SalesOrder.FreightDetails[i].FreightCodeDetails.Unit = items.Unit;
                                            SalesOrder.FreightDetails[i].FreightCodeDetails.TimeStamp = items.TimeStamp;
                                            freightHasChanged = true;
                                        }

                                        if (SalesOrder.FreightDetails[i].FreightCodeDetails.Price != items.Price)
                                        {
                                            SalesOrder.FreightDetails[i].FreightCodeDetails.Price = items.Price;
                                            SalesOrder.FreightDetails[i].FreightCodeDetails.TimeStamp = items.TimeStamp;
                                            decimal subTotal = SalesOrder.FreightDetails[i].Pallets * SalesOrder.FreightDetails[i].FreightCodeDetails.Price;
                                            decimal disTotal = (subTotal * SalesOrder.FreightDetails[i].Discount) / 100;
                                            SalesOrder.FreightDetails[i].Total = subTotal - disTotal;
                                            freightHasChanged = true;
                                        }
                                    }
                                }
                            }
                        }
                                        

                        if (salesOrderChanged || cusDataChanged || stockHasChanged || productHasChanged || freightHasChanged || carrierHasChanged || freightDetailsHasChanged || commentHasChanged || discountHasChanged)
                        {
                       
                        

                            foreach (var item in tup)
                            {
                                msg += item.Item2 + System.Environment.NewLine;
                                cusId = item.Item3;
                            }
                            string fMsg = "Data has been changed since you opened this form in following areas"
                                + System.Environment.NewLine + System.Environment.NewLine
                                + msg
                                + System.Environment.NewLine + System.Environment.NewLine
                                + "This form has been updated with new data";

                            MessageBox.Show(fMsg, "Data Has Changed", MessageBoxButton.OK, MessageBoxImage.Warning);

                            #region Discount
                            if (discountHasChanged && (newDiscountAdded || discountRemoved))
                            {
                                string s = string.Empty;

                                if (newDiscountAdded)
                                {
                                    foreach (var item in SalesOrder.SalesOrderDetails)
                                    {
                                        foreach (var items in DisplayDiscountStructure)
                                        {
                                            if (items.Discount != item.Discount && item.Product.Category.CategoryID == items.Category.CategoryID)
                                            {
                                                s += item.Product.ProductDescription + " ( " + items.Discount + "%) " + System.Environment.NewLine;
                                            }
                                        }
                                    
                                    }

                                    if (!String.IsNullOrWhiteSpace(s))
                                    {
                                        if (MessageBox.Show("Would you like to add new discount(s) to selected product(s)?" + System.Environment.NewLine + System.Environment.NewLine + s, "Adding Discount(s) To Products", MessageBoxButton.YesNo, MessageBoxImage.Asterisk) == MessageBoxResult.Yes)
                                        {
                                            foreach (var item in SalesOrder.SalesOrderDetails)
                                            {
                                                foreach (var items in DisplayDiscountStructure)
                                                {
                                                    if (item.Product.Category.CategoryID == items.Category.CategoryID && item.Discount != items.Discount)
                                                    {
                                                        item.Discount = items.Discount;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                else if (discountRemoved && removedDiscounts.Count > 0)
                                {
                                    foreach (var items in SalesOrder.SalesOrderDetails)
                                    {
                                        var data = removedDiscounts.SingleOrDefault(x => x.CategoryID == items.Product.Category.CategoryID);
                                        if (data != null)
                                        {
                                            s += items.Product.ProductDescription + " ( " + items.Discount + "%) " + System.Environment.NewLine;
                                        }
                                    }

                                    if (!String.IsNullOrWhiteSpace(s))
                                    {
                                        if (MessageBox.Show("Would you like to remove discount from selected product(s)?" + System.Environment.NewLine + System.Environment.NewLine + s, "Removing Discount(s) From Products", MessageBoxButton.YesNo, MessageBoxImage.Asterisk) == MessageBoxResult.Yes)
                                        {
                                            foreach (var item in SalesOrder.SalesOrderDetails)
                                            {
                                                foreach (var items in removedDiscounts)
                                                {
                                                    if (item.Product.Category.CategoryID == items.CategoryID)
                                                    {
                                                        item.Discount = 0;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            #endregion                        

                            foreach (var item in SalesOrder.SalesOrderDetails)
                            {
                                CalculateRowTotal(item);
                            }
                        

                            //Update the TimeStamps                    
                            if (dataC != null)
                            {
                                dataC.TimeStamp = updatedDetails.Item1.TimeStamp;
                            }
                        }
                    }

                    #endregion

                    /*****************END OF DATA CHANGE SEQUANCE**********************/

                    SalesOrder.OrderAction = SelectedOrderAction;
                    result = om.ProcessSalesOrder2(SalesOrder, SelectedCustomer, userName);

                    if (result.Item1)
                    {

                        int finalRes = om.UpdateSalesOrderDB(result.Item2, result.Item3, result.Item4, userName, result.Item6, timeStamps);
                        if (finalRes == 1)
                        {
                            MessageBox.Show("Order updated successfully", "Order Updated", MessageBoxButton.OK, MessageBoxImage.Information);
                            ClearFields();
                            closeFormVal = 0;
                            CloseForm();
                        }
                        //else if (finalRes == -1)
                        //{
                        //    MessageBox.Show("Data has been changed since you opened this form!!!" + System.Environment.NewLine + "The form is closing now and please re-open again", "Data Changed", MessageBoxButton.OK, MessageBoxImage.Warning);
                        //    closeFormVal = 0;
                        //    CloseForm();
                        //}
                        else if (finalRes == 0)
                        {
                            MessageBox.Show("You haven't made any changes to update", "No Changes Were Made", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show("There has been a problem when updating this order" + System.Environment.NewLine + "Err No - E12", "Cannot Update Order", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    else
                    {
                        if (result.Item5)
                        {
                            ClearFields();
                            closeFormVal = 0;
                            CloseForm();
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("This order no longer exist", "", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                CloseForm();
            }
        }

        private SalesOrder ProcessComments(SalesOrder so)
        {
            if (!string.IsNullOrWhiteSpace(WarehouseComment))
            {
                bool b = so.Comments.Any(x => x.LocationID == 7);
                if (b)
                {
                    foreach (var item in so.Comments)
                    {
                        if (item.LocationID == 7)
                        {
                            item.CreatedBy = userName;
                            item.Note = WarehouseComment;
                            item.LastUpdatedDate = DateTime.Now;
                            break;
                        }
                    }
                }
                else
                {
                    so.Comments.Add(new Comment() { Prefix = "SO", CreatedBy = userName, CreatedDate = DateTime.Now, LocationID = 7, Note = WarehouseComment, LastUpdatedDate = DateTime.Now });
                }
            }
            else
            {
                bool b = so.Comments.Any(x => x.LocationID == 7);
                if (b)
                {
                    foreach (var item in so.Comments)
                    {
                        if (item.LocationID == 7)
                        {
                            item.Note = string.Empty;
                            break;
                        }
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(TransportComment))
            {
                bool b = so.Comments.Any(x => x.LocationID == 8);
                if (b)
                {
                    foreach (var item in so.Comments)
                    {
                        if (item.LocationID == 8)
                        {
                            item.CreatedBy = userName;
                            item.Note = TransportComment;
                            item.LastUpdatedDate = DateTime.Now;
                            break;
                        }
                    }
                }
                else
                {
                    so.Comments.Add(new Comment() { Prefix = "SO", CreatedBy = userName, CreatedDate = DateTime.Now, LocationID = 8, Note = TransportComment, LastUpdatedDate = DateTime.Now });
                }
            }
            else
            {
                bool b = so.Comments.Any(x => x.LocationID == 8);
                if (b)
                {
                    foreach (var item in so.Comments)
                    {
                        if (item.LocationID == 8)
                        {
                            item.Note = string.Empty;
                            break;
                        }
                    }
                }
            }

            return so;
        }

        private void RemoveZeroSalesOrderDetails()
        {
            var itemToRemove = SalesOrder.SalesOrderDetails.Where(x => x.Product == null).ToList();
            foreach (var item in itemToRemove)
            {
                SalesOrder.SalesOrderDetails.Remove(item);
            }
        }

        private void RemoveZeroFreightDetails()
        {
            var itemToRemove = SalesOrder.FreightDetails.Where(x => x.FreightCodeDetails == null).ToList();
            foreach (var item in itemToRemove)
            {
                SalesOrder.FreightDetails.Remove(item);
            }
        }



        private string ConvertOrderTypeToString(int o)
        {
            string n = string.Empty;
            switch (o)
            {
                case 3: n = "Normal";
                    break;
                case 1: n = "Urgent";
                    break;
                default:
                    break;
            }
            return n;
        }

        private void ClearFields()
        {
            SalesOrder.SalesOrderDetails.Clear();
            // CustomerList.Clear();
            //LoadCustomers();
            SelectedStockLocation = "QLD";
            SelectedCustomer = "Select";
            SalesOrder.PrepaidCustomerName = string.Empty;
            SalesOrder.CustomerOrderNo = string.Empty;
            SalesOrder.PickupTime = "Not Sure";
            SalesOrder.DesiredDispatchDate = null;
            SalesOrder.OrderAction = "Select";
            SelectedOrderPriority = "Normal";
            SalesOrder.BillTo = string.Empty;
            SalesOrder.ShipTo = string.Empty;
            SalesOrder.TermsID = "30EOM";
            DiscountStructure.Clear();
            DiscountStructure = new List<DiscountStructure>();
            DiscountStructure = new List<DiscountStructure>();
            if (DisplayDiscountStructure != null)
            {
                DisplayDiscountStructure.Clear();
                DisplayDiscountStructure = new List<DiscountStructure>();
            }
            //SalesOrder.FreightDetails = new BindingList<FreightDetails>();
            SalesOrder.FreightDetails.Clear();
            SelectedCustomerType = "Account";
            SalesOrder.FreightCarrier.FreightName = "Select";
            SalesOrder.SalesOrderDetails.CollectionChanged += productChanged;
            SalesOrder.FreightTotal = 0;
            SalesOrder.TotalAmount = 0;
            //SalesOrder.Comments.Note = string.Empty;
            WarehouseComment = string.Empty;
            TransportComment = string.Empty;

            //SelectedPaymentRecieved = oldSalesOrder != null && (oldSalesOrder.TotalAmount != SalesOrder.TotalAmount || oldSalesOrder.FreightTotal != SalesOrder.FreightTotal) ? "No" : "Yes";
            CreditLimit = 0;
            CreditRemaining = 0;
            DiscountAppliedTextVisibility = "Collapsed";
        }

        private void CalculateQtyToMake()
        {            
            if (productStockList != null)
            {
                foreach (var item in SalesOrder.SalesOrderDetails)
                {
                    foreach (var itemPS in productStockList)
                    {
                        if (item.Product != null)
                        {
                            if (item.Product.ProductID == itemPS.Product.ProductID)
                            {
                                if (item.Quantity <= itemPS.QtyAvailable)
                                {
                                    item.QtyInStock = Decimal.Parse(itemPS.QtyAvailable.ToString("G29"));
                                    item.QtyToMake = 0;
                                    item.ToMakeCellBack = "White";
                                    item.ToMakeCellFore = "Black";
                                    item.InStockCellBack = "Green";
                                    item.InStockCellFore = "White";
                                }
                                else if (item.Quantity > itemPS.QtyAvailable && itemPS.QtyAvailable != 0)
                                {
                                    item.QtyInStock = Decimal.Parse(itemPS.QtyAvailable.ToString("G29"));
                                    item.QtyToMake = Decimal.Parse(itemPS.QtyAvailable.ToString("G29"));

                                    item.ToMakeCellBack = "#FFC33333";
                                    item.ToMakeCellFore = "White";
                                    item.InStockCellBack = "ffb300";
                                    item.InStockCellFore = "White";
                                }
                                else if (itemPS.QtyAvailable == 0)
                                {
                                    item.QtyToMake = Decimal.Parse(item.Quantity.ToString("G29"));
                                    item.QtyInStock = 0;
                                    item.ToMakeCellBack = "#FFC33333";
                                    item.ToMakeCellFore = "White";
                                    item.InStockCellBack = "#FFC33333";
                                    item.InStockCellFore = "White";
                                }
                                break;
                            }
                        }
                    }
                }
            }

            CalculateFinalTotal();
        }

        void productChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            this.ItemCount = this.SalesOrder.SalesOrderDetails.Count;

            // Resequence list
            SequencingService.SetCollectionSequence(this.SalesOrder.SalesOrderDetails);
            CalculateFinalTotal();
        }

        void freightChanged(object sender, ListChangedEventArgs e)
        {
            CalculateFinalTotal();
        }

        private void CalculateFinalTotal()
        {
            if (SalesOrder != null)
            {
                SalesOrder.ListPriceTotal = SalesOrder.SalesOrderDetails.Sum(x => x.Total);
                SalesOrder.FreightTotal = SalesOrder.FreightDetails.Sum(x => x.Total);
                SalesOrder.GST = GSTActive ? ((SalesOrder.ListPriceTotal + SalesOrder.FreightTotal) * 10) / 100 : 0;
                SalesOrder.TotalAmount = Math.Round(SalesOrder.ListPriceTotal,2) + Math.Round(SalesOrder.FreightTotal,2) + Math.Round(SalesOrder.GST,2);               
            }
        }

        private void Execute(object parameter)
        {
            int index = SalesOrder.SalesOrderDetails.IndexOf(parameter as SalesOrderDetails);
            if (index > -1 && index < SalesOrder.SalesOrderDetails.Count)
            {
                SalesOrder.SalesOrderDetails.RemoveAt(index);
            }
            //if (SalesOrder.SalesOrderDetails.Count == 0)
            //{
            //    SalesOrder.SalesOrderDetails = new ObservableCollection<SalesOrderDetails>();
            //}
        }

        private void DeleteFreightCode(object parameter)
        {
            int index = SalesOrder.FreightDetails.IndexOf(parameter as FreightDetails);
            if (index > -1 && index < SalesOrder.FreightDetails.Count)
            {
                SalesOrder.FreightDetails.RemoveAt(index);
            }
            if (SalesOrder.FreightDetails.Count == 0)
            {
                SalesOrder.FreightDetails = new BindingList<FreightDetails>();
                SalesOrder.FreightDetails.ListChanged += freightChanged;
            }
        }

        private void CalculateFreightTot()
        {
            //if(SalesOrder.FreightDetails != null || SalesOrder.FreightDetails.FreightCode != null)
            //{
            //SalesOrder.FreightDetails.FreightTotal = SalesOrder.FreightDetails.Qty * SalesOrder.FreightDetails.FreightCode.Price;
            //CalculateFinalTotal();
            //}
        }

        private bool CanExecute(object parameter)
        {
            return true;
        }

        private void CloseForm()
        {
            //pageSwitcher.Container.Children.Add(new MdiChild
            //{
            //    Name = "OrderStatus",
            //    Title = "Order Status",
            //    Content = new CurrentOrdersView(userName, state, privilages, metaData, pageSwitcher),
            //    Width = 1800,
            //    Height = 1000,
            //    ToolTip = "Order Status"
            //});

            Closed(closeFormVal);
            ClearFields();
        }

        

        private void ShowAddUpdateDiscountView()
        {
            DiscountNoOverlayView window = new DiscountNoOverlayView(SalesOrder.Customer.CustomerId, SalesOrder);
            window.ShowDialog();
            DiscountStructure.Clear();
            DiscountStructure = DBAccess.GetDiscount(SalesOrder.Customer.CustomerId);
            DisplayDiscountStructure = DiscountStructure;
            DisplayDiscountStructure.RemoveAll(x => x.Discount == 0);
            if (SalesOrder.SalesOrderDetails.Count > 0)
            {
                string str = string.Empty;

                foreach (var item in SalesOrder.SalesOrderDetails)
                {
                    foreach (var items in DiscountStructure)
                    {
                        if (item.Product.Category.CategoryID == items.Category.CategoryID && item.Discount != items.Discount)
                        {
                            str += item.Product.ProductDescription + System.Environment.NewLine;
                        }
                    }
                }
                if (SalesOrder.SalesOrderNo != 0 && !String.IsNullOrWhiteSpace(str))
                {
                    if (MessageBox.Show("Would you like to add this discount to the following products?" + System.Environment.NewLine + str, "Add Discounts To Products", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        foreach (var item in SalesOrder.SalesOrderDetails)
                        {
                            foreach (var items in DiscountStructure)
                            {
                                if (item.Product.Category.CategoryID == items.Category.CategoryID && item.Discount != items.Discount)
                                {
                                    item.Discount = items.Discount;
                                }
                            }
                        }
                    }
                }

                CalculateFinalTotal();
            }           
         
        }

        private bool CheckReturningItems()
        {
            bool res = false;
            string a = string.Empty, b = string.Empty;
            string resString = string.Empty;
            string remString = string.Empty;
            SalesOrder SalesOrderTemp = DBAccess.GetSalesOrderDetails(SalesOrder.SalesOrderNo);
            foreach (var item in SalesOrder.SalesOrderDetails)
            {
                bool itemFound = false;
                foreach (var items in SalesOrderTemp.SalesOrderDetails)
                {
                    if (item.SalesOrderDetailsID == items.SalesOrderDetailsID)
                    {
                        itemFound = true;
                        if (item.Quantity > items.Quantity)
                        {
                            resString += item.Product.ProductDescription + " cannot be greater than " + item.Quantity + System.Environment.NewLine;
                        }
                    }
                }
                remString += itemFound == false ? "Please remove " + item.Product.ProductDescription + System.Environment.NewLine : "";
            }

            if (!string.IsNullOrWhiteSpace(resString))
            {
                a = "Following products contain more quantity than purchase quantity. Please deduct quantity less or equal to purchase quantity " + System.Environment.NewLine + resString + System.Environment.NewLine;
            }
            if (!string.IsNullOrWhiteSpace(remString))
            {
                b = "Following products are not purchased. " + remString;
            }
            if (!string.IsNullOrWhiteSpace(a) || !string.IsNullOrWhiteSpace(b))
            {
                if (MessageBox.Show(a + b + System.Environment.NewLine + "Would you like to load original details", "Invalid Quantity", MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.Yes)
                {
                    SalesOrder = DBAccess.GetSalesOrderDetails(SalesOrder.SalesOrderNo);
                    SelectedOrderPriority = ConvertOrderTypeToString(SalesOrder.OrderPriority);
                    GSTActive = SalesOrder.GSTEnabled;
                    res = true;
                }
                else
                {
                    res = false;
                }
            }
            else
            {
                res = true;
            }
            return res;
        }

        private void CalculateFreightTotal()
        {
            foreach (var item in SalesOrder.FreightDetails)
            {
                //item.Total = item.FreightCodeDetails.Price * item.Pallets;

                decimal subTotal = item.Pallets * item.DummyPrice;
                decimal disTotal = (subTotal * item.Discount) / 100;
                item.Total = subTotal - disTotal;
            
            }
        }

        public static string ChangeStatusBackgroundCol(string os)
        {
            string backCol = string.Empty;
            switch (os)
            {

                case "Hold": backCol = "#db2503";
                    break;
                case "Release": backCol = "#008040";
                    break;
                case "NoStock": backCol = "#db2503";
                    break;
                case "HoldNoStock": backCol = "#db2503";
                    break;
                case "HoldStockAllocated": backCol = "#db2503";
                    break;
                case "HoldNoCredit": backCol = "#db2503";
                    break;
                case "HoldNoCreditStockAllocated": backCol = "#db2503";
                    break;
                case "HoldNoCreditNoStock": backCol = "#db2503";
                    break;
                case "AwaitingStock": backCol = "#db2503";
                    break;
                case "InWarehouse": backCol = "#008040";
                    break;
                case "Picking": backCol = "#008040";
                    break;
                case "FinalisingShipping": backCol = "#008040";
                    break;
                case "ReadyToDispatch": backCol = "#008040";
                    break;
                case "PreparingInvoice": backCol = "#008040";
                    break;
                case "Dispatched": backCol = "#008040";
                    break;
                case "Cancel": backCol = "#db2503";
                    break;
                case "None": backCol = "#db2503";
                    break;
                case "Return": backCol = "#ff9900";
                    break;
                default:
                    break;
            }
            return backCol;
        }

        private void ActivateDeactivateOrderActionVisibility()
        {
            if (SalesOrder.OrderStatus == OrderStatus.AwaitingStock.ToString() || SalesOrder.OrderStatus == OrderStatus.Release.ToString() || SalesOrder.OrderStatus == OrderStatus.Picking.ToString() ||
                SalesOrder.OrderStatus == OrderStatus.Cancel.ToString() || SalesOrder.OrderStatus == OrderStatus.Hold.ToString() || SalesOrder.OrderStatus == OrderStatus.HoldNoCredit.ToString() ||
                SalesOrder.OrderStatus == OrderStatus.HoldNoCreditNoStock.ToString() || SalesOrder.OrderStatus == OrderStatus.HoldNoCreditStockAllocated.ToString() || SalesOrder.OrderStatus == OrderStatus.HoldNoStock.ToString() ||
                SalesOrder.OrderStatus == OrderStatus.HoldStockAllocated.ToString() || SalesOrder.OrderStatus == OrderStatus.InWarehouse.ToString() || SalesOrder.OrderStatus == OrderStatus.NoStock.ToString() ||
                SalesOrder.OrderStatus == OrderStatus.Picking.ToString() || SalesOrder.OrderStatus == OrderStatus.ReadyToDispatch.ToString() || SalesOrder.OrderStatus == OrderStatus.PreparingInvoice.ToString())
            {

                ActivateControles();
            }
            else if (SalesOrder.OrderStatus == OrderStatus.Dispatched.ToString() || SalesOrder.OrderStatus == OrderStatus.Return.ToString())
            {
                DeActivateControles();
            }

            if (SalesOrder.OrderStatus == OrderStatus.Dispatched.ToString())
            {
                HoldVisibility = "Collapsed";
                ReleaseVisibility = "Collapsed";
                CancelVisibility = "Collapsed";
                ReturnVisibility = "Visible";
                DispatchedVisibility = "Visible";
            }
            else if (SalesOrder.OrderStatus == OrderStatus.Return.ToString())
            {
                HoldVisibility = "Collapsed";
                ReleaseVisibility = "Collapsed";
                CancelVisibility = "Collapsed";
                ReturnVisibility = "Visible";
                DispatchedVisibility = "Collapsed";
            }
            else
            {
                HoldVisibility = "Visible";
                ReleaseVisibility = "Visible";
                CancelVisibility = "Visible";
                ReturnVisibility = "Collapsed";
                DispatchedVisibility = "Collapsed";
            }
        }

        private void DeActivateControles()
        {
            //HoldVisibility = "Collapsed";
            //ReleaseVisibility = "Collapsed";
            //CancelVisibility = "Collapsed";
            //DispatchedVisibility = "Visible";
            //ReturnVisibility = "Visible";

            ProductGridEnabled = false;
            FreightGridEnabled = false;
            CarrierNameEnabled = false;
            //TermsIDEnabled = false;
            PickUpTimeEnabled = false;
            WarehouseCommentsEnabled = false;
            SoldToEnabled = false;
            ShipToEnabled = false;
            OrderPriorityEnabled = false;
            CustomerOrderNoEnabled = false;
            DispatchDateEnabled = false;
            PaymentRecievedEnabled = false;
            AddUpdateActive = false;
            GSTEnabled = false;
            UpdateEnabled = false;
            UpdateBackground = "#bdbdbd";
        }

        private void ActivateControles()
        {
            //HoldVisibility = "Visible";
            //ReleaseVisibility = "Visible";
            //CancelVisibility = "Visible";
            //ReturnVisibility = "Collapsed";
            //DispatchedVisibility = "Collapsed";

            ProductGridEnabled = true;
            FreightGridEnabled = true;
            CarrierNameEnabled = true;
            // TermsIDEnabled = true;
            PickUpTimeEnabled = true;
            WarehouseCommentsEnabled = true;
            SoldToEnabled = true;
            ShipToEnabled = true;
            OrderPriorityEnabled = true;
            CustomerOrderNoEnabled = true;
            DispatchDateEnabled = true;
            PaymentRecievedEnabled = true;
            GSTEnabled = true;
            UpdateEnabled = true;
            UpdateBackground = "#666666";
        }

        private void FreightCodeChanged()
        {

            if (SalesOrder.FreightCarrier.FreightName == "Customer Collect")
            {
                SalesOrder.FreightDetails = new BindingList<FreightDetails>();
                SalesOrder.FreightDetails.ListChanged += freightChanged;
                CalculateFinalTotal();
            }
        }

        //private void CalculateRowTotal(object parameter)
        //{
        //    int index = SalesOrder.SalesOrderDetails.IndexOf(parameter as SalesOrderDetails);
        //    if (index > -1 && index < SalesOrder.SalesOrderDetails.Count)
        //    {
        //        if (SalesOrder.SalesOrderDetails[index].Product != null)
        //        {
        //            decimal subTotal = SalesOrder.SalesOrderDetails[index].QuoteUnitPrice * SalesOrder.SalesOrderDetails[index].Quantity;
        //            decimal discountedTotal = (subTotal * SalesOrder.SalesOrderDetails[index].Discount) / 100;
        //            SalesOrder.SalesOrderDetails[index].Total = subTotal - discountedTotal;

        //            SalesOrder.SalesOrderDetails[index].ProductTotalBeforeDis = subTotal;
        //            SalesOrder.SalesOrderDetails[index].DiscountedTotal = discountedTotal;

        //            CalculateFinalTotal();

        //        }
        //    }
        //}

        private void CalculateRowTotal(SalesOrderDetails qd)
        {
            decimal subTotal = qd.QuoteUnitPrice * qd.Quantity;
            decimal discountedTotal = (subTotal * qd.Discount) / 100;
            qd.Total = subTotal - discountedTotal;

            qd.ProductTotalBeforeDis = subTotal;
            qd.DiscountedTotal = discountedTotal;
            CalculateFinalTotal();
        }

        private void FreightChanged(object parameter)
        {
            int index = SalesOrder.FreightDetails.IndexOf(parameter as FreightDetails);
            if (index > -1 && index < SalesOrder.FreightDetails.Count)
            {
                if (SalesOrder.FreightDetails[index].FreightCodeDetails != null)
                {
                    decimal subTotal = 0;
                    decimal disTotal = 0;

                    SalesOrder.FreightDetails[index].DummyDescription = SalesOrder.FreightDetails[index].FreightCodeDetails.Description;
                    SalesOrder.FreightDetails[index].DummyPrice = SalesOrder.FreightDetails[index].FreightCodeDetails.Price;

                    subTotal = SalesOrder.FreightDetails[index].Pallets * SalesOrder.FreightDetails[index].DummyPrice;//FreightCodeDetails.Price;
                    disTotal = (subTotal * SalesOrder.FreightDetails[index].Discount) / 100;
                    SalesOrder.FreightDetails[index].Total = subTotal - disTotal;
                }
            }
        }

        private void ProductChanged(object parameter)
        {
            int index = SalesOrder.SalesOrderDetails.IndexOf(parameter as SalesOrderDetails);
            if (index > -1 && index < SalesOrder.SalesOrderDetails.Count)
            {
                if (SalesOrder.SalesOrderDetails[index].Product != null)
                {

                    var data = productStockList.SingleOrDefault(x => x.Product.ProductID == SalesOrder.SalesOrderDetails[index].Product.ProductID);
                    if (data != null)
                    {
                        if (SalesOrder.SalesOrderDetails[index].Quantity <= data.QtyAvailable)
                        {
                            SalesOrder.SalesOrderDetails[index].QtyInStock = Decimal.Parse(data.QtyAvailable.ToString("G29"));
                            SalesOrder.SalesOrderDetails[index].QtyToMake = 0;
                            SalesOrder.SalesOrderDetails[index].ToMakeCellBack = "White";
                            SalesOrder.SalesOrderDetails[index].ToMakeCellFore = "Black";
                            SalesOrder.SalesOrderDetails[index].InStockCellBack = "Green";
                            SalesOrder.SalesOrderDetails[index].InStockCellFore = "White";
                        }
                        else if (SalesOrder.SalesOrderDetails[index].Quantity > data.QtyAvailable && data.QtyAvailable != 0)
                        {
                            SalesOrder.SalesOrderDetails[index].QtyInStock = Decimal.Parse(data.QtyAvailable.ToString("G29"));
                            SalesOrder.SalesOrderDetails[index].QtyToMake = Decimal.Parse(data.QtyAvailable.ToString("G29"));

                            SalesOrder.SalesOrderDetails[index].ToMakeCellBack = "#FFC33333";
                            SalesOrder.SalesOrderDetails[index].ToMakeCellFore = "White";
                            SalesOrder.SalesOrderDetails[index].InStockCellBack = "Green";
                            SalesOrder.SalesOrderDetails[index].InStockCellFore = "White";
                        }
                        else if (data.QtyAvailable == 0)
                        {
                            SalesOrder.SalesOrderDetails[index].QtyToMake = Decimal.Parse(SalesOrder.SalesOrderDetails[index].Quantity.ToString("G29"));
                            SalesOrder.SalesOrderDetails[index].QtyInStock = 0;
                            SalesOrder.SalesOrderDetails[index].ToMakeCellBack = "#FFC33333";
                            SalesOrder.SalesOrderDetails[index].ToMakeCellFore = "White";
                            SalesOrder.SalesOrderDetails[index].InStockCellBack = "#FFC33333";
                            SalesOrder.SalesOrderDetails[index].InStockCellFore = "White";
                        }
                    }

                    var dataDis = DiscountStructure.SingleOrDefault(x => x.Category.CategoryID == SalesOrder.SalesOrderDetails[index].Product.Category.CategoryID);
                    if (dataDis != null)
                    {
                        SalesOrder.SalesOrderDetails[index].Discount = dataDis.Discount;
                        if (SalesOrder.SalesOrderDetails[index].Discount > 0)
                        {
                            DiscountAppliedTextVisibility = "Visible";
                        }
                        else
                        {
                            DiscountAppliedTextVisibility = "Collapsed";
                        }
                    }
                    else
                    {
                        SalesOrder.SalesOrderDetails[index].Discount = 0;
                    }

                    SalesOrder.SalesOrderDetails[index].QuoteProductDescription = SalesOrder.SalesOrderDetails[index].Product.ProductDescription;
                    SalesOrder.SalesOrderDetails[index].QuoteUnitPrice = SalesOrder.SalesOrderDetails[index].Product.UnitPrice;

                    //CalculateRowTotal(SalesOrder.SalesOrderDetails[index]);
                }
                else
                {
                    SalesOrder.SalesOrderDetails[index].QuoteProductDescription = string.Empty;
                    SalesOrder.SalesOrderDetails[index].QuoteUnitPrice = 0;
                    SalesOrder.SalesOrderDetails[index].Discount = 0;
                }

                CalculateRowTotal(SalesOrder.SalesOrderDetails[index]);
            }
            EnableDisableDiscountText();
            CalculateFinalTotal();
        }

        private void GetRowTotal(object parameter)
        {
            int index = SalesOrder.SalesOrderDetails.IndexOf(parameter as SalesOrderDetails);
            if (index > -1 && index < SalesOrder.SalesOrderDetails.Count)
            {
                if (SalesOrder.SalesOrderDetails[index].Product != null)
                {
                    CalculateRowTotal(SalesOrder.SalesOrderDetails[index]);
                    CalculateFinalTotal();
                }
            }
        }

        private void EnableDisableDiscountText()
        {
            if (DiscountStructure != null || DiscountStructure.Count > 0)
            {
                bool disApplied = false;
                if (SalesOrder.SalesOrderDetails != null)
                {
                    foreach (var item in SalesOrder.SalesOrderDetails)
                    {
                        foreach (var items in DiscountStructure)
                        {
                            if (item.Product != null && items.Category.CategoryID == item.Product.Category.CategoryID && item.Discount == items.Discount)
                            {
                                disApplied = true;
                                break;
                            }
                        }
                    }
                }
                DiscountAppliedTextVisibility = disApplied ? "Visible" : "Collapsed";
            }
        }

        #region PUBLIC_PROPERTIES



        public string QuoteNo
        {
            get
            {
                return _quoteNo;
            }
            set
            {
                _quoteNo = value;
                RaisePropertyChanged("QuoteNo");
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

        public SalesOrder SalesOrder
        {
            get
            {
                return _salesOrder;
            }
            set
            {
                _salesOrder = value;
                RaisePropertyChanged("SalesOrder");

                //Console.WriteLine(SalesOrder.OrderAction + " XXX");
            }
        }

        public List<DiscountStructure> DiscountStructure
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



        //public string PrepaidCustomerName
        //{
        //    get
        //    {
        //        return _prepaidCustomerName;
        //    }
        //    set
        //    {
        //        _prepaidCustomerName = value;
        //        RaisePropertyChanged("PrepaidCustomerName);
        //    }
        //}

        //public string AccountCustomerVisiblity
        //{
        //    get
        //    {
        //        return _accountCustomerVisiblity;
        //    }
        //    set
        //    {
        //        _accountCustomerVisiblity = value;
        //        RaisePropertyChanged("AccountCustomerVisiblity);
        //    }
        //}

        //public string PrepaidCustomerVisibility
        //{
        //    get
        //    {
        //        return _prepaidCustomerVisibility;
        //    }
        //    set
        //    {
        //        _prepaidCustomerVisibility = value;
        //        RaisePropertyChanged("PrepaidCustomerVisibility);
        //        //if(PrepaidCustomerVisibility == "Visible")
        //        //{
        //        //    SelectedCustomer = string.Empty;
        //        //}               
        //    }
        //}

        private void EnableDisableCustomerControls()
        {
            var result = CustomerList.FirstOrDefault(s => s.CompanyName == SalesOrder.Customer.CompanyName);
            if (result != null)
            {
                //PrepaidCustomerVisibility = "Collapsed";
                //AccountCustomerVisiblity = "Visible";
                SelectedCustomer = SalesOrder.Customer.CompanyName;

                if (SelectedCustomerType == "Prepaid")
                {
                    //PaymentRecievedVisibility = "Visible";
                    CreditLimitVisibility = "Collapsed";
                }
                else if (SelectedCustomerType == "Account")
                {
                    //PaymentRecievedVisibility = "Collapsed";
                    CreditLimitVisibility = "Visible";
                }
            }
            else
            {
                SalesOrder.PrepaidCustomerName = SalesOrder.PrepaidCustomerName;
                //PrepaidCustomerVisibility = "Visible";
                //AccountCustomerVisiblity = "Collapsed";
                //PaymentRecievedVisibility = "Visible";
                CreditLimitVisibility = "Collapsed";
            }
        }


        public string SelectedCustomerType
        {
            get
            {
                return _selectedCustomerType;
            }
            set
            {
                _selectedCustomerType = value;
                RaisePropertyChanged("SelectedCustomerType");
                //EnableDisableCustomerControls();
                //var result = CustomerList.FirstOrDefault(s => s.CompanyName == SalesOrder.Customer.CompanyName);
                //if (result != null)
                //{
                //    PrepaidCustomerVisibility = "Collapsed";
                //    AccountCustomerVisiblity = "Visible";
                //    PaymentRecievedVisibility = "Collapsed";
                //    SelectedCustomer = SalesOrder.Customer.CompanyName;

                //    if (SelectedCustomerType == "Prepaid")
                //    {
                //        PrepaidCustomerVisibility = "Collapsed";
                //        AccountCustomerVisiblity = "Visible";
                //        PaymentRecievedVisibility = "Visible";
                //        CreditLimitVisibility = "Visible";            
                //    }
                //    else if (SelectedCustomerType == "Account")
                //    {
                //        PrepaidCustomerVisibility = "Collapsed";
                //        AccountCustomerVisiblity = "Visible";
                //        PaymentRecievedVisibility = "Collapsed";
                //        CreditLimitVisibility = "Visible";
                //    }
                //}
                //else
                //{
                //    SalesOrder.PrepaidCustomerName = SalesOrder.PrepaidCustomerName;
                //    PrepaidCustomerVisibility = "Visible";
                //    AccountCustomerVisiblity = "Collapsed";
                //    PaymentRecievedVisibility = "Visible";
                //    CreditLimitVisibility = "Collapsed";
                //}
            }
        }

        public string SelectedCustomer
        {
            get { return _selectedCustomer; }
            set
            {
                _selectedCustomer = value;
                RaisePropertyChanged("SelectedCustomer");

                DiscountAppliedTextVisibility = "Collapsed";

                if (SelectedCustomer != "Select")
                {
                    var result = CustomerList.FirstOrDefault(s => s.CompanyName == SelectedCustomer);
                    if (result != null)
                    {
                        AddUpdateBackground = "#666666";
                        AddUpdateActive = true;
                        DiscountStructure = DBAccess.GetDiscount(result.CustomerId);
                        DisplayDiscountStructure = DiscountStructure;
                        DisplayDiscountStructure.RemoveAll(x => x.Discount == 0);
                        SelectedCustomerType = result.CustomerType;
                        //PaymentRecievedVisibility = "Collapsed";

                        //if (SalesOrder.SalesOrderDetails != null || SalesOrder.SalesOrderDetails.Count > 0)
                        //{
                        //    foreach (var item in SalesOrder.SalesOrderDetails)
                        //    {
                        //        if (item.Discount == 0)
                        //        {
                        //            var z = DiscountStructure.SingleOrDefault(x => x.Category.CategoryID == item.Product.Category.CategoryID);
                        //            item.Discount = z == null ? 0 : z.Discount;
                        //        }
                        //    }
                        //}

                        if (SelectedCustomerType == "Prepaid")
                        {
                            CreditLimitVisibility = "Collapsed";
                            //PaymentRecievedVisibility = "Visible";
                        }
                        else if (SelectedCustomerType == "Account")
                        {
                            CreditLimitVisibility = "Visible";
                            //PaymentRecievedVisibility = "Collapsed";
                            CreditRemaining = result.CreditRemaining;
                            CreditLimit = result.CreditLimit;
                        }
                    }
                    else
                    {
                        DisplayDiscountStructure = null;
                        SelectedCustomerType = "Prepaid";
                        AddUpdateBackground = " ";
                        AddUpdateActive = false;
                        //PaymentRecievedVisibility = "Visible";
                        CreditLimitVisibility = "Collapsed";
                    }
                }
                else
                {
                    AddUpdateBackground = "#bdbdbd";
                    AddUpdateActive = false;
                }
            }
        }

        public string SelectedOrderPriority
        {
            get
            {
                return _selectedOrderPriority;
            }
            set
            {
                _selectedOrderPriority = value;
                RaisePropertyChanged("SelectedOrderPriority");
            }
        }

        public ObservableCollection<Customer> CustomerList
        {
            get
            {
                return _customerList;
            }
            set
            {
                _customerList = value;
                RaisePropertyChanged("CustomerList");
            }
        }

        public ObservableCollection<FreightCode> FreightCodeDetails
        {
            get
            {
                return _freightCodeDetails;
            }
            set
            {
                _freightCodeDetails = value;
                RaisePropertyChanged("FreightCodeDetails");

                
            }
        }

        public ObservableCollection<FreightCode> OrgFreightCodeDetails
        {
            get
            {
                return _orgFreightCodeDetails;
            }
            set
            {
                _orgFreightCodeDetails = value;
                RaisePropertyChanged("OrgFreightCodeDetails");
            }
        }

        public string SelectedStockLocation
        {
            get { return _selectedStockLocation; }
            set
            {
                _selectedStockLocation = value;
                RaisePropertyChanged("SelectedStockLocation");
            }
        }

        //public string SelectedPaymentRecieved
        //{
        //    get { return _selectedPaymentRecieved; }
        //    set
        //    {
        //        _selectedPaymentRecieved = value;
        //        RaisePropertyChanged("SelectedPaymentRecieved");
        //    }
        //}

        //public string PaymentRecievedVisibility
        //{
        //    get { return _paymentRecievedVisibility; }
        //    set
        //    {
        //        _paymentRecievedVisibility = value;
        //        RaisePropertyChanged("PaymentRecievedVisibility");
        //    }
        //}
        public string CreditLimitVisibility
        {
            get { return _creditLimitVisibility; }
            set
            {
                _creditLimitVisibility = value;
                RaisePropertyChanged("CreditLimitVisibility");
            }
        }

        public string OrderStatusString
        {
            get { return _orderStatusString; }
            set
            {
                _orderStatusString = value;
                RaisePropertyChanged("OrderStatusString");
            }
        }

        public string AddUpdateBackground
        {
            get { return _addUpdateBackground; }
            set
            {
                _addUpdateBackground = value;
                RaisePropertyChanged("AddUpdateBackground");
            }
        }

        public bool AddUpdateActive
        {
            get
            {
                return _addUpdateActive;
            }
            set
            {
                _addUpdateActive = value;
                RaisePropertyChanged("AddUpdateActive");
            }
        }

        public decimal CreditLimit
        {
            get { return _creditLimit; }
            set
            {
                _creditLimit = value;
                RaisePropertyChanged("CreditLimit");
            }
        }

        public decimal CreditRemaining
        {
            get { return _creditRemaining; }
            set
            {
                _creditRemaining = value;
                RaisePropertyChanged("CreditRemaining");
            }
        }

        public decimal CreditOwing
        {
            get { return _creditOwing; }
            set
            {
                _creditOwing = value;
                RaisePropertyChanged("CreditOwing");
            }
        }

        public decimal TotalOwed
        {
            get { return _totalOwed; }
            set
            {
                _totalOwed = value;
                RaisePropertyChanged("TotalOwed");
            }
        }

        public string StatusBackGroundCol
        {
            get { return _statusBackGroundCol; }
            set
            {
                _statusBackGroundCol = value;
                RaisePropertyChanged("StatusBackGroundCol");
            }
        }

        public string CreditOweBackground
        {
            get { return _creditOweBackground; }
            set
            {
                _creditOweBackground = value;
                RaisePropertyChanged("CreditOweBackground");
            }
        }

        public string CreditOweForeground
        {
            get { return _creditOweForeground; }
            set
            {
                _creditOweForeground = value;
                RaisePropertyChanged("CreditOweForeground");
            }
        }
        public List<DiscountStructure> DisplayDiscountStructure
        {
            get
            {
                return _displayDiscountStructure;
            }
            set
            {
                _displayDiscountStructure = value;
                RaisePropertyChanged("DisplayDiscountStructure");
            }
        }

        public string HoldVisibility
        {
            get { return _holdVisibility; }
            set
            {
                _holdVisibility = value;
                RaisePropertyChanged("HoldVisibility");
            }
        }

        public string ReleaseVisibility
        {
            get { return _releaseVisibility; }
            set
            {
                _releaseVisibility = value;
                RaisePropertyChanged("ReleaseVisibility");
            }
        }

        public string CancelVisibility
        {
            get { return _cancelVisibility; }
            set
            {
                _cancelVisibility = value;
                RaisePropertyChanged("CancelVisibility");
            }
        }

        public string ReturnVisibility
        {
            get { return _returnVisibility; }
            set
            {
                _returnVisibility = value;
                RaisePropertyChanged("ReturnVisibility");
            }
        }

        public string DispatchedVisibility
        {
            get { return _dispatchedVisibility; }
            set
            {
                _dispatchedVisibility = value;
                RaisePropertyChanged("DispatchedVisibility");
            }
        }

        public bool ProductGridEnabled
        {
            get { return _productGridEnabled; }
            set
            {
                _productGridEnabled = value;
                RaisePropertyChanged("ProductGridEnabled");
            }
        }

        public bool FreightGridEnabled
        {
            get { return _freightGridEnabled; }
            set
            {
                _freightGridEnabled = value;
                RaisePropertyChanged("FreightGridEnabled");
            }
        }

        public bool CarrierNameEnabled
        {
            get { return _carrierNameEnabled; }
            set
            {
                _carrierNameEnabled = value;
                RaisePropertyChanged("CarrierNameEnabled");
            }
        }

        //public bool TermsIDEnabled
        //{
        //    get { return _termsIDEnabled; }
        //    set
        //    {
        //        _termsIDEnabled = value;
        //        RaisePropertyChanged("TermsIDEnabled");
        //    }
        //}

        public bool PickUpTimeEnabled
        {
            get { return _pickUpTimeEnabled; }
            set
            {
                _pickUpTimeEnabled = value;
                RaisePropertyChanged("PickUpTimeEnabled");
            }
        }

        public bool WarehouseCommentsEnabled
        {
            get { return _warehouseCommentsEnabled; }
            set
            {
                _warehouseCommentsEnabled = value;
                RaisePropertyChanged("WarehouseCommentsEnabled");
            }
        }

        public bool SoldToEnabled
        {
            get { return _soldToEnabled; }
            set
            {
                _soldToEnabled = value;
                RaisePropertyChanged("SoldToEnabled");
            }
        }

        public bool ShipToEnabled
        {
            get { return _shipToEnabled; }
            set
            {
                _shipToEnabled = value;
                RaisePropertyChanged("ShipToEnabled");
            }
        }

        public bool OrderPriorityEnabled
        {
            get { return _orderPriorityEnabled; }
            set
            {
                _orderPriorityEnabled = value;
                RaisePropertyChanged("OrderPriorityEnabled");
            }
        }

        public bool CustomerOrderNoEnabled
        {
            get { return _customerOrderNoEnabled; }
            set
            {
                _customerOrderNoEnabled = value;
                RaisePropertyChanged("CustomerOrderNoEnabled");
            }
        }

        public bool PaymentRecievedEnabled
        {
            get { return _paymentRecievedEnabled; }
            set
            {
                _paymentRecievedEnabled = value;
                RaisePropertyChanged("PaymentRecievedEnabled");
            }
        }

        public bool DispatchDateEnabled
        {
            get { return _dispatchDateEnabled; }
            set
            {
                _dispatchDateEnabled = value;
                RaisePropertyChanged("DispatchDateEnabled");
            }
        }

        public bool UpdateEnabled
        {
            get { return _updateEnabled; }
            set
            {
                _updateEnabled = value;
                RaisePropertyChanged("UpdateEnabled");
            }
        }

        public string SelectedOrderAction
        {
            get { return _selectedOrderAction; }
            set
            {
                _selectedOrderAction = value;
                RaisePropertyChanged("SelectedOrderAction");

                if (SelectedOrderAction == OrderStatus.Dispatched.ToString())
                {
                    DeActivateControles();
                }
                else
                {
                    ActivateControles();
                }

            }
        }

        public string UpdateBackground
        {
            get { return _updateBackground; }
            set
            {
                _updateBackground = value;
                RaisePropertyChanged("UpdateBackground");

            }
        }

        public int ItemCount
        {
            get { return _itemCount; }

            set
            {
                _itemCount = value;
                base.RaisePropertyChanged("ItemCount");
            }
        }


        public bool GSTActive
        {
            get { return _gSTActive; }

            set
            {
                _gSTActive = value;
                base.RaisePropertyChanged("GSTActive");
                CalculateFinalTotal();
            }
        }

        public bool GSTEnabled
        {
            get { return _gSTEnabled; }

            set
            {
                _gSTEnabled = value;
                base.RaisePropertyChanged("GSTActive");
            }
        }

        public string PaymentReceievedString
        {
            get { return _paymentReceievedString; }
            set
            {
                _paymentReceievedString = value;
                RaisePropertyChanged("PaymentReceievedString");
            }
        }

        public string WarehouseComment
        {
            get 
            {
                if (_warehouseComment != null && _warehouseComment.Length > 300)
                {
                    return _warehouseComment.Substring(0, 300);
                }
                else if (_warehouseComment == null)
                {
                    WarehouseTxtLengthError = "300 characters left";
                }                

                return _warehouseComment; 
            }
            set
            {
                _warehouseComment = value;
                RaisePropertyChanged("WarehouseComment");
                string str = (300 - WarehouseComment.Length).ToString();
                WarehouseTxtLengthError = str + " characters left";
            }
        }

        public string TransportComment
        {
            get 
            {
                if (_transportComment != null && _transportComment.Length > 300)
                {
                    return _transportComment.Substring(0, 300);
                }
                else if (_transportComment == null)
                {
                    TransportTxtLengthError = "300 characters left";
                }

                return _transportComment; 
            }
            set
            {
                _transportComment = value;
                RaisePropertyChanged("TransportComment");
                string str = (300 - TransportComment.Length).ToString();
                TransportTxtLengthError = str + " characters left";
            }
        }

        public string WarehouseTxtLengthError
        {
            get { return _warehouseTxtLengthError; }
            set
            {
                _warehouseTxtLengthError = value;
                RaisePropertyChanged("WarehouseTxtLengthError");
            }
        }

        public string TransportTxtLengthError
        {
            get { return _transportTxtLengthError; }
            set
            {
                _transportTxtLengthError = value;
                RaisePropertyChanged("TransportTxtLengthError");
            }
        }

        public string DiscountAppliedTextVisibility
        {
            get { return _discountAppliedTextVisibility; }
            set
            {
                _discountAppliedTextVisibility = value;
                RaisePropertyChanged("DiscountAppliedTextVisibility");
            }
        }

        #endregion

        #region COMMANDS
        public ICommand UpdateCommand
        {
            get
            {
                if (_updateCommand == null)
                    _updateCommand = new RelayCommand(param => this.UpdateOrder(), param => this.canExecute);

                return _updateCommand;
            }
        }

        public ICommand ClearFieldsCommand
        {
            get
            {
                return _clearFieldsCommand ?? (_clearFieldsCommand = new CommandHandler(() => ClearFields(), canExecute));
            }
        }


        //public ICommand SelectionChangedCommand
        //{
        //    get
        //    {
        //        return _selectionChangedCommand ?? (_selectionChangedCommand = new CommandHandler(() => CalculateQtyToMake(), canExecute));
        //    }
        //}

        public ICommand SelectionChangedCommand
        {
            get
            {
                if (_selectionChangedCommand == null)
                {
                    _selectionChangedCommand = new DelegateCommand(CanExecute, ProductChanged);
                }
                return _selectionChangedCommand;
            }
        }

        private ICommand _freightChangedCommand;
        public ICommand FreightChangedCommand
        {
            get
            {
                if (_freightChangedCommand == null)
                {
                    _freightChangedCommand = new DelegateCommand(CanExecute, FreightChanged);
                }
                return _freightChangedCommand;
            }
        }

        public ICommand CalcFreightTotalCommand
        {
            get
            {
                return _calcFreightTotalCommand ?? (_calcFreightTotalCommand = new CommandHandler(() => CalculateFreightTot(), canExecute));
            }
        }

        public ICommand RemoveCommand
        {
            get
            {
                if (_removeCommand == null)
                {
                    _removeCommand = new DelegateCommand(CanExecute, Execute);
                }
                return _removeCommand;
            }
        }

        public ICommand RemoveFreightCodeCommand
        {
            get
            {
                if (_removeFreightCodeCommand == null)
                {
                    _removeFreightCodeCommand = new DelegateCommand(CanExecute, DeleteFreightCode);
                }
                return _removeFreightCodeCommand;
            }
        }

        public ICommand QuantityChangedCommand
        {
            get
            {
                if (_quantityChangedCommand == null)
                {
                    _quantityChangedCommand = new DelegateCommand(CanExecute, GetRowTotal);
                }
                return _quantityChangedCommand;
            }
        }

        public Microsoft.Practices.Prism.Commands.DelegateCommand CloseCommand
        {
            get { return _closeCommand; }
        }

        public ICommand AddUpdateDiscountCommand
        {
            get
            {
                return _addUpdateDiscountCommand ?? (_addUpdateDiscountCommand = new CommandHandler(() => ShowAddUpdateDiscountView(), canExecute));
            }
        }

        public ICommand FreightPriceKeyUpCommand
        {
            get
            {
                return _freightPriceKeyUpCommand ?? (_freightPriceKeyUpCommand = new CommandHandler(() => CalculateFreightTotal(), canExecute));
            }
        }

        public ICommand SearchProductCommand
        {
            get
            {
                if (_searchProductCommand == null)
                {
                    _searchProductCommand = new DelegateCommand(CanExecute, SearchProduct);
                }
                return _searchProductCommand;
            }
        }

        public ICommand FreightCodeChangedCommand
        {
            get
            {
                return _freightCodeChangedCommand ?? (_freightCodeChangedCommand = new CommandHandler(() => FreightCodeChanged(), canExecute));
            }
        }

        public ICommand PriceLostFocusCommand
        {
            get
            {
                if (_priceLostFocusCommand == null)
                {
                    _priceLostFocusCommand = new DelegateCommand(CanExecute, GetRowTotal);
                }
                return _priceLostFocusCommand;
            }
        }

        public ICommand DiscountLostFocusCommand
        {
            get
            {
                if (_discountLostFocusCommand == null)
                {
                    _discountLostFocusCommand = new DelegateCommand(CanExecute, GetRowTotal);
                }
                return _discountLostFocusCommand;
            }
        }

        #endregion
    }
}
