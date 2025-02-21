using A1RConsole.Bases;
using A1RConsole.Commands;
using A1RConsole.Core;
using A1RConsole.DB;
using A1RConsole.Interfaces;
using A1RConsole.Models.Categories;
using A1RConsole.Models.Customers;
using A1RConsole.Models.Discounts;
using A1RConsole.Models.Freights;
using A1RConsole.Models.Orders;
using A1RConsole.Models.Products;
using A1RConsole.Models.Quoting;
using A1RConsole.Models.Users;
using A1RConsole.PdfGeneration;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace A1RConsole.ViewModels.Quoting
{
    public class NewQuoteViewModel : ViewModelBase, IContent
    {
        public ObservableCollection<Product> Product { get; set; }
        private List<DiscountStructure> _discountStructure;
        private Quote _quote;
        private ObservableCollection<DiscountStructure> _displayDiscountStructure;
        private Customer _selectedCustomer;
        private bool IsDirty { get; set; }
        private int _itemCount;
        public event EventHandler Closing;
        private ObservableCollection<Customer> _customerList;
        private List<ProductStock> productStockList;
        private ObservableCollection<FreightCode> _freightCodeDetails;
        private ObservableCollection<FreightCarrier> _freightList;
        private FreightDetails _selectedFreightDetails;
        private bool _customerTypeEnabled;
        private bool _projectNameEnabled;
        private bool _soldToEnabled;
        private bool _shipToEnabled;
        private bool _quoteDetailsEnabled;
        private bool _freightDetailsEnabled;
        private bool _instructionsEnabled;
        private bool _internalCommentsEnabled;
        private string _discountAppliedTextVisibility;
        private bool _addShippingAddress;
        private string _otherContactName;
        private string _otherContactNamePhone1;
        private string _otherContactNamePhone2;
        private string _otherContactNameEmail;
        private string _otherContactPersonVisibility;
        private string _contactPersonPhoneVisibility;
        private string _selectedShipTo;
        private string _shippingAddressVisibility;
        private string _shipToText;
        private string _shippingAddress;
        private string _shippingCity;
        private string _shippingState;
        private string _shippingPostCode;
        private string _shippingCountry;
        private string _prepaidCustomerName;
        private string _customerType;
        private string _addCustomerVisibility;
        private string _phoneCopyVisibility;
        private static ContactPerson prevContactPerson;
        private bool _addCustomerToDatabase;
        private bool _isPrimaryBusinessEnabled;
        private bool _addCompanyAddressToShipping;
        private bool _isCopyPhone;
        private bool _tickCopyEmailEnabled;
        private bool _courierNameEnabled;
        private Quote quote;
        private Category _selectedCategory;
        private List<Category> _categories;
        private List<Tuple<int, int>> reducedDiscount;
        public LoadAllCustomersNotifier loadAllCustomersNotifier { get; set; }
        ViewUpdateQuoteViewModel viewUpdateQuoteViewModelInstance;

        private string[] freightCodesArr;
        private bool _addCcompanyAddToShippingEnabled;
        private string originalCustomer;
        private bool _gstEnabled;
        private bool _contactPersonEnabled;
        private bool _addShippingAddToProfileEnabled;
        private string _addUpdateBackground, _quoteNoVisibility;
        private bool _addUpdateActive;
        private bool addDiscountToPrice;
        private bool _isCopyEmail;
        private bool _tickCopyPhoneEnabled;
        private string _tickAddThisCusLabel;
        public RelayCommand CloseCommand { get; private set; }
        private ICommand _removeCommand;
        private ICommand _selectionChangedCommand;
        private ICommand _searchProductCommand;
        private ICommand _discountLostFocusCommand;
        private ICommand _clearCommand;
        private ICommand _submitCommand;
        private ICommand _freightPriceKeyUpCommand;
        private ICommand _priceLostFocusCommand;
        private ICommand _addUpdateDiscountCommand;
        private ICommand _gSTCheckedCommand;
        private ICommand _contactPersonChangedCommand;
        public static NewQuoteViewModel instance;

        int qNo;

        public NewQuoteViewModel(Quote q)
        {     
            this.loadAllCustomersNotifier = new LoadAllCustomersNotifier();
            this.loadAllCustomersNotifier.NewMessage += new EventHandler<SqlNotificationEventArgs>(notifier_NewMessage);
            LoadCustomerList(loadAllCustomersNotifier.RegisterDependency());

            QuoteNoVisibility = "Collapsed";
            ShippingAddressVisibility = "Visible";
            BackgroundWorker worker = new BackgroundWorker();
            ChildWindow LoadingScreen = new ChildWindow();
            LoadingScreen.ShowWaitingScreen("Loading");

            worker.DoWork += (_, __) =>
            {
                var freightCodes = UserData.MetaData.SingleOrDefault(x => x.KeyName == "courier_name_CNA");
                freightCodesArr = freightCodes.Description.Split('|');

                Product = new ObservableCollection<Product>();
                Quote = new Quote();
                Quote.QuoteDetails = new ObservableCollection<QuoteDetails>();
                Quote.Customer = new Customer();
                Quote.Customer.DiscountStructure = new ObservableCollection<DiscountStructure>();
                DiscountStructure = new List<DiscountStructure>();
                Quote.FreightDetails = new BindingList<FreightDetails>();
                FreightCodeDetails = new ObservableCollection<FreightCode>();
                Quote.FreightCarrier = new FreightCarrier();
                Quote.QuoteDate = DateTime.Now;
                Quote.User = new User() { ID = UserData.UserID, FullName = UserData.FirstName + " " + UserData.LastName };
                Quote.LastUpdatedBy = new User() { ID = UserData.UserID, FullName = UserData.FirstName + " " + UserData.LastName };
                Quote.LastUpdatedDate = DateTime.Now;
                prevContactPerson = new ContactPerson();

                //Get reduced discount products to a list
                reducedDiscount = new List<Tuple<int, int>>();
                var redDiscountProducts = UserData.MetaData.SingleOrDefault(x => x.KeyName == "reduce_discount");
                if (redDiscountProducts != null)
                {
                    var dict = redDiscountProducts.Description.Split('|').Select(x => x.Split(':')).ToDictionary(x => x[0], x => x[1]);

                    foreach (var item in dict)
                    {
                        reducedDiscount.Add(Tuple.Create(Convert.ToInt32(item.Key), Convert.ToInt32(item.Value)));
                    }
                }

                SelectedFreightDetails = new FreightDetails();
                SelectedFreightDetails.FreightCodeDetails = new FreightCode();
                SelectedFreightDetails.FreightCodeDetails = new FreightCode() { ID = 0, Code = "Select", Description = "", Price = 0, Unit = "" };

                Categories = new List<Category>();
                Categories = DBAccess.GetCategories();
                Categories.Add(new Category() { CategoryID = 0, CategoryName = "Select" });
                SelectedCategory = new Category();
                SelectedCategory = new Category() { CategoryID = 0, CategoryName = "Select" };

                PhoneCopyVisibility = "Collapsed";
                SelectedShipTo = "Customer Address";
                PrepaidCustomerName = "Select";
                AddUpdateBackground = "#FFDEDEDE";
                AddUpdateActive = false;
                DiscountAppliedTextVisibility = "Collapsed";
                OtherContactPersonVisibility = "Collapsed";
                ContactPersonPhoneVisibility = "Collapsed";
                TickAddThisCusLabel = "Collapsed";
                AddCcompanyAddToShippingEnabled = false;
                ShipToEnabled = false;
                AddShippingAddToProfileEnabled = false;
                AddShippingAddress = false;
                Quote.GSTActive = true;
                GstEnabled = false;
                CustomerTypeEnabled = false;
                IsPrimaryBusinessEnabled = false;
                addDiscountToPrice = true;
                ContactPersonEnabled = false;
                AddCompanyAddressToShipping = false;
                CourierNameEnabled = false;
               
                LoadCustomers();
                LoadFreights();
                LoadProducts();
                LoadFreightCodes();
                qNo = 0;
                originalCustomer = string.Empty;
                AddCustomerVisibility = "Collapsed";
                AddCustomerToDatabase = false;
                TickCopyPhoneEnabled = false;
                TickCopyEmailEnabled = false;

                if (q != null)
                {
                    QuoteNoVisibility = "Visible";
                    quote = q;
                    qNo = quote.QuoteNo;
                    SelectedCustomer = quote.Customer;
                    if (SelectedCustomer != null && SelectedCustomer.CompanyName != "Select")
                    {
                        AddUpdateActive = true;
                        AddUpdateBackground = "#666666";
                    }

                    PrepaidCustomerName = quote.Customer.CompanyName;
                    originalCustomer = quote.Customer.CompanyName;
                    CustomerType = quote.Customer.CustomerType;

                    Quote.QuoteNo = quote.QuoteNo;
                    Quote.User.FullName = UserData.FirstName + " " + UserData.LastName;
                    Quote.ProjectName = quote.ProjectName;
                    
                    string[] prods = null;
                    prods = MetaDataManager.GetPriceEditingProducts();

                    //Apply non discount products
                    var noDiscountProducts = UserData.MetaData.SingleOrDefault(x => x.KeyName == "no_discount_products");
                    string[] noDiscountProductsArr = noDiscountProducts.Description.Split('|');

                    //Update the quote with new price list
                    foreach (var item in quote.QuoteDetails)
                    {
                        var data = Product.SingleOrDefault(x => x.ProductID == item.Product.ProductID);
                        if (data != null)
                        {
                            decimal unitPrice = data.UnitPrice;

                            var p = prods.SingleOrDefault(x => Convert.ToInt16(x) == data.ProductID);
                            if (p != null)
                            {
                                unitPrice = item.Product.UnitPrice;
                            }

                            bool isNoDisExists = noDiscountProductsArr.Any(x => Convert.ToInt16(x) == item.Product.ProductID);

                            System.Windows.Application.Current.Dispatcher.Invoke(() =>
                            {
                                int maxDiscount = 0;
                                if (item.Product.Category.CategoryID == 11)
                                {
                                    maxDiscount = item.Discount;
                                }

                                Quote.QuoteDetails.Add(new QuoteDetails()
                                {
                                    ID = item.ID,
                                    BlocksLogsToMake = item.BlocksLogsToMake,
                                    CustomProductVisible = item.CustomProductVisible,
                                    InStockCellBack = item.InStockCellBack,
                                    InStockCellFore = item.InStockCellFore,
                                    IsEditing = item.IsEditing,
                                    LineStatus = item.LineStatus,
                                    OrderLine = item.OrderLine,
                                    Product = new Product()
                                    {
                                        ProductID = item.Product.ProductID,
                                        Category = new Models.Categories.Category() { CategoryID = item.Product.Category.CategoryID },
                                        ProductCode = item.Product.ProductCode,
                                        ProductDescription = item.Product.ProductDescription,
                                        ProductUnit = item.Product.ProductUnit,
                                        UnitPrice = unitPrice,
                                        Type = item.Product.Type,
                                        Size = item.Product.Size,
                                        LocationType = item.Product.LocationType                                        
                                    },

                                    Quantity = item.Quantity,
                                    QuantityStr = item.QuantityStr,
                                    TimeStamp = data.TimeStamp,
                                    QuoteUnitPrice = unitPrice,
                                    QuoteProductDescription = item.QuoteProductDescription,
                                    MaxDiscount = maxDiscount,
                                    Discount = item.Discount,
                                    DiscountedTotal = item.DiscountedTotal,
                                    Total = item.Total,
                                    IsDiscountEnabled = isNoDisExists ? false : true,
                                    Prefix = item.Prefix
                                });
                            });
                        }
                    }

                    for (int i = 0; i < Quote.QuoteDetails.Count; i++)
                    {
                        CalculateRowTotal(Quote.QuoteDetails[i]);
                    }

                    // AddUpdateActive = false;

                    if (quote.FreightDetails.Count > 0)
                    {
                        SelectedFreightDetails.FreightCodeDetails = new FreightCode();
                        SelectedFreightDetails.FreightCodeDetails.ID = quote.FreightDetails[0].FreightCodeDetails.ID;
                        SelectedFreightDetails.FreightCodeDetails.Code = quote.FreightDetails[0].FreightCodeDetails.Code;
                        SelectedFreightDetails.FreightCodeDetails.Description = quote.FreightDetails[0].FreightCodeDetails.Description;
                        SelectedFreightDetails.FreightCodeDetails.Unit = quote.FreightDetails[0].FreightCodeDetails.Unit;
                        SelectedFreightDetails.FreightCodeDetails.Price = quote.FreightDetails[0].FreightCodeDetails.Price;
                        SelectedFreightDetails.PalletsStr = quote.FreightDetails[0].PalletsStr;
                        SelectedFreightDetails.Discount = quote.FreightDetails[0].Discount;

                        var fData = FreightCodeDetails.SingleOrDefault(x => x.ID == quote.FreightDetails[0].FreightCodeDetails.ID);
                        if (fData != null)
                        {
                            fData.Description = quote.FreightDetails[0].FreightCodeDetails.Description;

                            if (q.FreightDetails[0].FreightCodeDetails.ID != 50)
                            {
                                SelectedFreightDetails.FreightCodeDetails.Price = quote.FreightDetails[0].FreightCodeDetails.Price;
                                SelectedFreightDetails.FreightCodeDetails.PriceEnabled = false;
                            }
                            else
                            {
                                fData.Price = quote.FreightDetails[0].FreightCodeDetails.Price;
                                SelectedFreightDetails.FreightCodeDetails.PriceEnabled = true;
                            }
                        }
                    }

                    SelectedCategory = new Category();
                    if (!string.IsNullOrWhiteSpace(quote.Customer.PrimaryBusiness.CategoryName))
                    {
                        SelectedCategory.CategoryID = quote.Customer.PrimaryBusiness.CategoryID;
                        SelectedCategory.CategoryName = quote.Customer.PrimaryBusiness.CategoryName;
                    }
                    else
                    {
                        SelectedCategory = new Category() { CategoryID = 0, CategoryName = "Select" };
                    }                                       

                    Quote.InternalComments = quote.InternalComments;
                    Quote.Instructions = quote.Instructions;
                    Quote.GSTActive = quote.GSTActive;

                    if (quote.Customer.CustomerId > 0)
                    {
                        DiscountStructure = DBAccess.GetDiscount(quote.Customer.CustomerId);
                        DisplayDiscountStructure = new ObservableCollection<Models.Discounts.DiscountStructure>(DiscountStructure);
                        for (int i = DisplayDiscountStructure.Count - 1; i >= 0; i--)
                        {
                            if (DisplayDiscountStructure[i].Discount == 0)
                                DisplayDiscountStructure.RemoveAt(i);
                        }
                        //DisplayDiscountStructure.RemoveAll(x => x.Discount == 0);
                    }

                    if (quote.Customer.ShipAddress == "Collect from A1 Rubber QLD" || quote.Customer.ShipAddress == "Collect from A1 Rubber NSW")
                    {
                        ShippingAddressVisibility = "Collapsed";
                        AddCcompanyAddToShippingEnabled = false;
                        ShipToText = quote.Customer.ShipAddress;
                        if (quote.Customer.ShipAddress == "Collect from A1 Rubber QLD")
                        {
                            SelectedShipTo = "A1 Rubber QLD";
                        }
                        else if (quote.Customer.ShipAddress == "Collect from A1 Rubber NSW")
                        {
                            SelectedShipTo = "A1 Rubber NSW";
                        }
                    }
                    else
                    {
                        AddCcompanyAddToShippingEnabled = true;
                        SelectedShipTo = "Customer Address";
                        ShippingAddressVisibility = "Visible";
                        ShipToText = string.Empty;
                    }

                    ProjectNameEnabled = true;
                    SoldToEnabled = true;
                    ShipToEnabled = true;
                    QuoteDetailsEnabled = true;
                    CourierNameEnabled = true;
                    FreightDetailsEnabled = true;
                    InstructionsEnabled = true;
                    InternalCommentsEnabled = true;
                    GstEnabled = true;
                    addDiscountToPrice = false;

                    
                    Quote.ContactPerson = quote.ContactPerson;
                    var conCloned = quote.ContactPerson.Clone();
                    prevContactPerson = (ContactPerson)conCloned;
                                        
                    if (quote.ContactPerson.CustomerID == 0)
                    {
                        if (quote.ContactPerson.ContactPersonName != null && quote.ContactPerson.ContactPersonName != "Other")
                        {
                            SelectedCustomer.ContactPerson.Add(quote.ContactPerson);
                            Quote.ContactPerson = quote.ContactPerson;
                            Quote.ContactPerson.ContactPersonName = quote.ContactPerson.ContactPersonName;
                        }
                        else
                        {
                            Quote.ContactPerson = new ContactPerson() { ContactPersonID = -1, ContactPersonName = "Other", CustomerID = SelectedCustomer.CustomerId };
                        }
                    }

                    Quote.QuoteCourierName = quote.QuoteCourierName;

                }
                else
                {
                    ProjectNameEnabled = false;
                    SoldToEnabled = false;
                    ShipToEnabled = false;
                    AddShippingAddToProfileEnabled = false;
                    QuoteDetailsEnabled = false;
                    CourierNameEnabled = false;
                    FreightDetailsEnabled = false;
                    InstructionsEnabled = false;
                    InternalCommentsEnabled = false;
                    GstEnabled = false;
                }

                Quote.QuoteDetails.CollectionChanged += productChanged;
                this.Quote.QuoteDetails = SequencingService.SetCollectionSequence(this.Quote.QuoteDetails);
            };
            worker.RunWorkerCompleted += delegate(object s, RunWorkerCompletedEventArgs args)
            {
                LoadingScreen.CloseWaitingScreen();
            };
            worker.RunWorkerAsync();
            this.CloseCommand = new RelayCommand(CloseWindow);

            instance = this;
            quote = null;
        }

        private void FixDiscount()
        {
            foreach (var item in Quote.QuoteDetails)
            {
                ProductChanged(item);
            }
        }

        void notifier_NewMessage(object sender, SqlNotificationEventArgs e)
        {
            LoadCustomerList(this.loadAllCustomersNotifier.RegisterDependency());
        }

        private void LoadCustomerList(ObservableCollection<Customer> pSList)
        {
            if (pSList != null)
            {
                if (SelectedCustomer != null)
                {
                    //Get previous details
                    var previouslySelectedItem = SelectedCustomer;
                    List<QuoteDetails> tempQuoteDetails = new List<QuoteDetails>();
                    string tempProjname = Quote.ProjectName;
                    ContactPerson tempContactPerson = new ContactPerson();
                    string tempSelectedShipTo = SelectedShipTo;
                    string tempShippingAddress = ShippingAddress;
                    string tempShippingCity = ShippingCity;
                    string tempShippingState = ShippingState;
                    string tempShippingPostCode = ShippingPostCode;
                    string tempShippingCountry = ShippingCountry;
                    FreightDetails tempFreightDetails = new FreightDetails();
                    string tempInstructions = Quote.Instructions;
                    string tempInternalComments = Quote.InternalComments;
                    string tempCourierName = Quote.QuoteCourierName;

                    if (Quote.QuoteDetails != null)
                    {
                        tempQuoteDetails = new List<QuoteDetails>(Quote.QuoteDetails);
                    }

                    if (Quote.ContactPerson != null)
                    {
                        tempContactPerson = Quote.ContactPerson;
                    }

                    if (SelectedFreightDetails != null)
                    {
                        tempFreightDetails = SelectedFreightDetails;
                        //tempFreightDetails.FreightCodeDetails = new FreightCode();
                        tempFreightDetails.FreightCodeDetails.ID = SelectedFreightDetails.FreightCodeDetails.ID;
                        tempFreightDetails.FreightCodeDetails.Code = SelectedFreightDetails.FreightCodeDetails.Code;
                        tempFreightDetails.FreightCodeDetails.Description = SelectedFreightDetails.FreightCodeDetails.Description;
                        tempFreightDetails.FreightCodeDetails.Unit = SelectedFreightDetails.FreightCodeDetails.Unit;
                        tempFreightDetails.FreightCodeDetails.Price = SelectedFreightDetails.FreightCodeDetails.Price;
                        tempFreightDetails.PalletsStr = SelectedFreightDetails.PalletsStr;
                        tempFreightDetails.Discount = SelectedFreightDetails.Discount;
                    }

                    if (CustomerList.Count > 0)
                    {
                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            CustomerList.Clear();
                        });
                    }

                    CustomerList = pSList;

                    var data = CustomerList.SingleOrDefault(x => x.CompanyName.Equals(previouslySelectedItem.CompanyName));
                    if (data != null)
                    {

                        //if (quote.FreightDetails.Count > 0)
                        //{
                        //    SelectedFreightDetails.FreightCodeDetails = new FreightCode();
                        //    SelectedFreightDetails.FreightCodeDetails.ID = quote.FreightDetails[0].FreightCodeDetails.ID;
                        //    SelectedFreightDetails.FreightCodeDetails.Code = quote.FreightDetails[0].FreightCodeDetails.Code;
                        //    SelectedFreightDetails.FreightCodeDetails.Description = quote.FreightDetails[0].FreightCodeDetails.Description;
                        //    SelectedFreightDetails.FreightCodeDetails.Unit = quote.FreightDetails[0].FreightCodeDetails.Unit;
                        //    SelectedFreightDetails.FreightCodeDetails.Price = quote.FreightDetails[0].FreightCodeDetails.Price;
                        //    SelectedFreightDetails.PalletsStr = quote.FreightDetails[0].PalletsStr;
                        //    SelectedFreightDetails.Discount = quote.FreightDetails[0].Discount;

                        //    var fData = FreightCodeDetails.SingleOrDefault(x => x.ID == quote.FreightDetails[0].FreightCodeDetails.ID);
                        //    if (fData != null)
                        //    {
                        //        fData.Description = quote.FreightDetails[0].FreightCodeDetails.Description;

                        //        if (q.FreightDetails[0].FreightCodeDetails.ID != 50)
                        //        {
                        //            SelectedFreightDetails.FreightCodeDetails.Price = quote.FreightDetails[0].FreightCodeDetails.Price;
                        //            SelectedFreightDetails.FreightCodeDetails.PriceEnabled = false;
                        //        }
                        //        else
                        //        {
                        //            fData.Price = quote.FreightDetails[0].FreightCodeDetails.Price;
                        //            SelectedFreightDetails.FreightCodeDetails.PriceEnabled = true;
                        //        }
                        //    }
                        //}

                        SelectedCustomer = data;
                        SelectedCustomer.ContactPerson = data.ContactPerson;
                        Quote.ProjectName = tempProjname;
                        Quote.ContactPerson = tempContactPerson;
                        Quote.ContactPerson.ContactPersonName = tempContactPerson.ContactPersonName;
                        SelectedShipTo = tempSelectedShipTo;
                        ShippingAddress = tempShippingAddress;
                        ShippingCity = tempShippingCity;
                        ShippingState = tempShippingState;
                        ShippingPostCode = tempShippingPostCode;
                        ShippingCountry = tempShippingCountry;
                        //SelectedFreightDetails.FreightCodeDetails = tempFreightDetails.FreightCodeDetails;
                        //SelectedFreightDetails.PalletsStr = tempFreightDetails.PalletsStr;
                        //SelectedFreightDetails.Discount = tempFreightDetails.Discount;


                        //SelectedFreightDetails.FreightCodeDetails = new FreightCode();
                        SelectedFreightDetails.FreightCodeDetails.ID = tempFreightDetails.FreightCodeDetails.ID;
                        SelectedFreightDetails.FreightCodeDetails.Code = tempFreightDetails.FreightCodeDetails.Code;
                        SelectedFreightDetails.FreightCodeDetails.Description = tempFreightDetails.FreightCodeDetails.Description;
                        SelectedFreightDetails.FreightCodeDetails.Unit = tempFreightDetails.FreightCodeDetails.Unit;
                        SelectedFreightDetails.FreightCodeDetails.Price = tempFreightDetails.FreightCodeDetails.Price;
                        SelectedFreightDetails.PalletsStr = tempFreightDetails.PalletsStr;
                        SelectedFreightDetails.Discount = tempFreightDetails.Discount;


                        Quote.Instructions = tempInstructions;
                        Quote.InternalComments = tempInternalComments;
                        Quote.QuoteCourierName = tempCourierName;
                        if (Quote.QuoteDetails != null)
                        {
                            System.Windows.Application.Current.Dispatcher.Invoke(() =>
                            {
                                Quote.QuoteDetails.Clear();
                            });
                        }

                        foreach (var item in tempQuoteDetails)
                        {
                            System.Windows.Application.Current.Dispatcher.Invoke(() =>
                            {
                                Quote.QuoteDetails.Add(new QuoteDetails()
                                {
                                    BlocksLogsToMake = item.BlocksLogsToMake,
                                    CustomProductVisible = item.CustomProductVisible,
                                    InStockCellBack = item.InStockCellBack,
                                    InStockCellFore = item.InStockCellFore,
                                    IsEditing = item.IsEditing,
                                    LineStatus = item.LineStatus,
                                    OrderLine = item.OrderLine,
                                    Product = new Product()
                                    {
                                        ProductID = item.Product.ProductID,
                                        Category = new Models.Categories.Category() { CategoryID = item.Product.Category.CategoryID },
                                        ProductCode = item.Product.ProductCode,
                                        ProductDescription = item.Product.ProductDescription,
                                        ProductUnit = item.Product.ProductUnit,
                                        UnitPrice = item.QuoteUnitPrice,
                                        Type = item.Product.Type,
                                        Size = item.Product.Size
                                    },
                                    Quantity = item.Quantity,
                                    QuantityStr = item.QuantityStr,
                                    TimeStamp = data.TimeStamp,
                                    QuoteUnitPrice = item.QuoteUnitPrice,
                                    QuoteProductDescription = item.QuoteProductDescription,
                                    Discount = item.Discount,
                                    DiscountedTotal = item.DiscountedTotal,
                                    Total = item.Total
                                });

                            });
                        }
                    }
                }
                else
                {

                    CustomerList = pSList;
                }

            }
        }

        private void LoadCustomers()
        {
            //CustomerList = DBAccess.GetCustomerData();
        }

        private void LoadProducts()
        {
            Product = DBAccess.GetAllProds(false);

        }

        public string Title
        {
            get
            {
                return "New Quote";
            }
        }




        private void ProductChanged(object parameter)
        {
            int index = Quote.QuoteDetails.IndexOf(parameter as QuoteDetails);
            if (index > -1 && index < Quote.QuoteDetails.Count)
            {
                if (Quote.QuoteDetails[index].Product != null)
                {
                    string totalKg = string.Empty;
                    if (Quote.QuoteDetails[index].Product.ProductID != 56 && Quote.QuoteDetails[index].Product.Type == "Bag")
                    {
                        int tot = Convert.ToInt16(Quote.QuoteDetails[index].QuantityStr) * Convert.ToInt16(Quote.QuoteDetails[index].Product.Size);
                        totalKg = tot > 0 ? " [Total " + tot + "Kg]" : string.Empty;
                    }

                    Quote.QuoteDetails[index].IsDiscountEnabled = true;
                    Quote.QuoteDetails[index].QuoteProductDescription = Quote.QuoteDetails[index].Product.ProductDescription + totalKg;
                    Quote.QuoteDetails[index].QuoteUnitPrice = Quote.QuoteDetails[index].Product.UnitPrice;
                    
                    //Apply non discount products
                    var noDiscountProducts = UserData.MetaData.SingleOrDefault(x => x.KeyName == "no_discount_products");
                    string[] noDiscountProductsArr = noDiscountProducts.Description.Split('|');

                    bool isNoDisExists = noDiscountProductsArr.Any(x => Convert.ToInt16(x) == Quote.QuoteDetails[index].Product.ProductID);

                    if (addDiscountToPrice || (isNoDisExists == false && (quote == null || quote.QuoteNo == 0)) || (isNoDisExists == false && quote != null && AddCustomerToDatabase))
                    {
                        if (DiscountStructure != null && DiscountStructure.Count > 0)
                        {
                            var data = DiscountStructure.SingleOrDefault(x => x.Category.CategoryID == Quote.QuoteDetails[index].Product.Category.CategoryID);
                            if (data != null)
                            {
                                if (data.Category.CategoryID == 11)
                                {
                                    if (data.Discount > 0)
                                    {
                                        Quote.QuoteDetails[index].MaxDiscount = data.Discount;
                                    }
                                    else
                                    {
                                        var data2 = Categories.SingleOrDefault(x => x.CategoryID == 11);
                                        if (data2 != null)
                                        {
                                            Quote.QuoteDetails[index].MaxDiscount = data2.Discounts.Max();
                                        }
                                    }
                                }

                                var prodExistsData = reducedDiscount.SingleOrDefault(x => x.Item1 == Quote.QuoteDetails[index].Product.ProductID);
                                if (prodExistsData != null)
                                {
                                    Quote.QuoteDetails[index].Discount = data.Discount > prodExistsData.Item2 ? prodExistsData.Item2 : data.Discount;
                                }
                                else
                                {
                                    Quote.QuoteDetails[index].Discount = data.Discount;
                                }
                            }
                            else
                            {
                                if (Quote.QuoteDetails[index].Product.Category.CategoryID == 11)
                                {
                                    var data2 = Categories.SingleOrDefault(x => x.CategoryID == 11);
                                    if (data2 != null)
                                    {
                                        Quote.QuoteDetails[index].MaxDiscount = data2.Discounts.Max();
                                    }
                                }
                                else
                                {                                
                                    Quote.QuoteDetails[index].Discount = 0;
                                }
                            }
                        }
                        else
                        {

                            if (Categories != null && Categories.Count > 0)
                            {
                                var data = Categories.SingleOrDefault(x=>x.CategoryID == 11);
                                if(data != null)
                                {
                                    Quote.QuoteDetails[index].MaxDiscount = data.Discounts.Max();
                                }                                
                            }

                            Quote.QuoteDetails[index].Discount = 0;
                        }
                    }
                    else if (addDiscountToPrice == false && isNoDisExists == false && quote != null && quote.QuoteDetails != null)
                    {
                        var data = DiscountStructure.SingleOrDefault(x => x.Category.CategoryID == Quote.QuoteDetails[index].Product.Category.CategoryID);
                        if (data != null)
                        {
                            if (data.Category.CategoryID == 11)
                            {
                                if (data.Discount > 0)
                                {
                                    Quote.QuoteDetails[index].MaxDiscount = data.Discount;
                                }
                                else
                                {
                                    var data1 = Categories.SingleOrDefault(x => x.CategoryID == 11);
                                    if (data != null)
                                    {
                                        Quote.QuoteDetails[index].MaxDiscount = data1.Discounts.Max();
                                    }
                                }
                            }

                            Quote.QuoteDetails[index].Discount = data.Discount;
                        }
                        else
                        {
                            var data1 = Categories.SingleOrDefault(x => x.CategoryID == 11);
                            if (data1 != null)
                            {
                                Quote.QuoteDetails[index].MaxDiscount = data1.Discounts.Max();
                            }
                        }
                    }
                    else if (isNoDisExists)
                    {                        
                        Quote.QuoteDetails[index].IsDiscountEnabled = false;
                        Quote.QuoteDetails[index].Discount = 0;                        
                    }
                    else
                    {
                        Quote.QuoteDetails[index].Discount = 0;
                    }                   
                }
                else
                {
                    Quote.QuoteDetails[index].QuoteProductDescription = string.Empty;
                    Quote.QuoteDetails[index].QuoteUnitPrice = 0;
                    Quote.QuoteDetails[index].Discount = 0;
                }
                ProcessFreight();
                CalculateFinalTotal();
            }
            
           
        }

        //private void ProcessFreight()
        //{
        //    if (Quote != null && Quote.QuoteDetails != null)
        //    {
        //        bool has = false;

        //        if (Quote != null && Quote.QuoteDetails != null && SelectedFreightDetails != null)
        //        {
        //            if (quote != null && quote.FreightDetails != null)
        //            {                       

        //                if (SelectedShipTo != "Customer Address")
        //                {
        //                    Quote.QuoteCourierName = string.Empty;
        //                    CourierNameEnabled = true;
        //                }
        //            }
        //            else if (SelectedShipTo == "Customer Address")
        //            {
        //                //SelectedFreightDetails.FreightCodeDetails = new FreightCode() { Code = "Select", Unit = "", Price = 0 };
        //                CourierNameEnabled = true;
        //            }
        //            else
        //            {
        //                if (SelectedShipTo == "A1 Rubber NSW")
        //                {
        //                    has = Quote.QuoteDetails.Any(x => x.Product != null && x.Product.LocationType == "QLD");
        //                }

        //                if (has)
        //                {
        //                    var data = FreightCodeDetails.SingleOrDefault(x => x.ID == 35);
        //                    if (data != null)
        //                    {
        //                        SelectedFreightDetails.FreightCodeDetails = data;
        //                    }
        //                }
        //                else if (has == false && (SelectedShipTo == "A1 Rubber QLD" || SelectedShipTo == "A1 Rubber NSW"))
        //                {
        //                    SelectedFreightDetails.FreightCodeDetails = new FreightCode() { ID = 51, Code = "Pickup", Unit = "PLT", Price = 0, Description = "Pickup" };
        //                    Quote.QuoteCourierName = string.Empty;
        //                    CourierNameEnabled = false;
        //                }                        
        //            }
        //        }
        //    }            
        //}

        private void ProcessFreight()
        {
            if (Quote != null && Quote.QuoteDetails != null)
            {
                bool has = false;

                if (Quote != null && Quote.QuoteDetails != null && SelectedFreightDetails != null)
                {
                    if (quote != null && quote.FreightDetails != null)
                    {
                        if (SelectedShipTo == "A1 Rubber NSW")
                        {
                            has = Quote.QuoteDetails.Any(x => x.Product.LocationType == "QLD");
                        }

                        if (has)
                        {
                            var data = FreightCodeDetails.SingleOrDefault(x => x.ID == 35);
                            if (data != null)
                            {
                                SelectedFreightDetails.FreightCodeDetails = data;
                            }
                        }
                        else if (has == false && (SelectedShipTo == "A1 Rubber QLD" || SelectedShipTo == "A1 Rubber NSW"))
                        {
                            SelectedFreightDetails.FreightCodeDetails = new FreightCode() { ID = 51, Code = "Pickup", Unit = "PLT", Price = 0, Description = "Pickup" };
                            Quote.QuoteCourierName = string.Empty;
                            CourierNameEnabled = false;
                        }

                    }
                    else if (SelectedShipTo == "Customer Address")
                    {
                        //SelectedFreightDetails.FreightCodeDetails = new FreightCode() { Code = "Select", Unit = "", Price = 0 };
                        //Quote.QuoteCourierName = string.Empty;
                    }
                    else
                    {
                        if (SelectedShipTo == "A1 Rubber NSW")
                        {
                            has = Quote.QuoteDetails.Any(x => x.Product.LocationType == "QLD");
                        }

                        if (has)
                        {
                            var data = FreightCodeDetails.SingleOrDefault(x => x.ID == 35);
                            if (data != null)
                            {
                                SelectedFreightDetails.FreightCodeDetails = data;
                            }
                        }
                        else if (has == false && (SelectedShipTo == "A1 Rubber QLD" || SelectedShipTo == "A1 Rubber NSW"))
                        {
                            SelectedFreightDetails.FreightCodeDetails = new FreightCode() { ID = 51, Code = "Pickup", Unit = "PLT", Price = 0, Description = "Pickup" };
                            Quote.QuoteCourierName = string.Empty;
                            CourierNameEnabled = false;
                        }
                    }
                }
            }
        }

        private void CalculateQtyToMake()
        {
            //bool disApplied = false;
            //if (DiscountStructure != null || DiscountStructure.Count > 0)
            //{

            //    for (int i = 0; i < Quote.QuoteDetails.Count; i++)
            //    {
            //        if (Quote.QuoteDetails[i].Product != null)
            //        {

            //            foreach (var item in DiscountStructure)
            //            {
            //                if(item.Category.CategoryID == Quote.QuoteDetails[i].Product.Category.CategoryID)
            //                {
            //                    Quote.QuoteDetails[i].Discount = (item == null ? 0 : item.Discount);
            //                    if (Quote.QuoteDetails[i].Discount > 0)
            //                    {
            //                        disApplied = true;
            //                        break;
            //                    }
            //                }
            //                else
            //                {
            //                    Quote.QuoteDetails[i].Discount = 0;
            //                } 
            //            }
            //        }
            //    }
            //}
            //DiscountAppliedTextVisibility = disApplied ? "Visible" : "Collapsed";

            CalculateFinalTotal();
        }


        private void RemoveProduct(object parameter)
        {
            int index = Quote.QuoteDetails.IndexOf(parameter as QuoteDetails);
            if (index > -1 && index < Quote.QuoteDetails.Count)
            {
                Quote.QuoteDetails.RemoveAt(index);
                ObservableCollection<QuoteDetails> tempColl = new ObservableCollection<QuoteDetails>();
                tempColl = Quote.QuoteDetails;
                Quote.QuoteDetails = new ObservableCollection<QuoteDetails>();
                Quote.QuoteDetails = tempColl;
            }
        }
    
        private void GetRowTotal(object parameter)
        {
            int index = Quote.QuoteDetails.IndexOf(parameter as QuoteDetails);
            if (index > -1 && index < Quote.QuoteDetails.Count)
            {

                if (Quote.QuoteDetails[index].Product != null)
                {
                    CalculateRowTotal(Quote.QuoteDetails[index]);
                    CalculateFinalTotal();                    
                }
            }
        }

        private void CalculateRowTotal(QuoteDetails qd)
        {          

            decimal subTotal = qd.QuoteUnitPrice * qd.Quantity;
            decimal discountedTotal = (subTotal * qd.Discount) / 100;
            qd.Total = subTotal - discountedTotal;

            qd.ProductTotalBeforeDis = subTotal;
            qd.DiscountedTotal = discountedTotal;

            if (qd.Product != null)
            {
                string description = string.Empty;
                string totalKg = string.Empty;
                if (qd.Product.ProductID != 56 && qd.Product.Type == "Bag")
                {
                    Int64 tot = Convert.ToInt64(qd.QuantityStr) * Convert.ToInt64(qd.Product.Size);
                    totalKg = tot > 0 ? " [Total " + tot + "Kg]" : string.Empty;

                    //Check if there is an existing quote
                    if (quote != null && quote.QuoteDetails != null)
                    {
                        var data = quote.QuoteDetails.SingleOrDefault(x => x.Product.ProductID == qd.Product.ProductID && x.ID == qd.ID);
                        if (data != null)
                        {
                            Regex xx = new Regex("(\\[)(.*?)(\\])");
                            string ss = data.QuoteProductDescription;
                            description = xx.Replace(ss, totalKg);
                        }
                        qd.QuoteProductDescription = data != null ? description : qd.Product.ProductDescription + totalKg;
                    }
                    else
                    {
                        Regex xx = new Regex("(\\[)(.*?)(\\])");
                        string ss = qd.QuoteProductDescription;
                        description = xx.Replace(ss, totalKg);
                        qd.QuoteProductDescription = description;
                    }
                }
            }
        }

        private void CalculateFinalTotal()
        {
            if (Quote != null)
            {
                for (int i = 0; i < Quote.QuoteDetails.Count; i++)
                {
                    CalculateRowTotal(Quote.QuoteDetails[i]);
                }

                Quote.ListPriceTotal = Quote.QuoteDetails.Sum(x => x.ProductTotalBeforeDis);
                Quote.DiscountedTotal = Quote.QuoteDetails.Sum(x => x.DiscountedTotal);

                decimal noOfPallets = Convert.ToDecimal(string.IsNullOrWhiteSpace(SelectedFreightDetails.PalletsStr) == true ? 0 : Convert.ToDecimal(SelectedFreightDetails.PalletsStr));
                decimal freightPrice = SelectedFreightDetails.FreightCodeDetails == null ? 0 : SelectedFreightDetails.FreightCodeDetails.Price;

                Quote.FreightTotal = freightPrice * noOfPallets - (((freightPrice * noOfPallets) * SelectedFreightDetails.Discount) / 100);
                if (Quote.GSTActive)
                {
                    Quote.Gst = Quote.GSTActive ? Math.Round((((Quote.ListPriceTotal - Quote.DiscountedTotal) + Quote.FreightTotal) * 10) / 100, 2) : 0;
                }
                else
                {
                    Quote.Gst = 0;
                }

                Quote.TotalBeforeTax = Quote.ListPriceTotal - Quote.DiscountedTotal;
                Quote.TotalAmount = Math.Round(Quote.TotalBeforeTax, 2) + Math.Round(Quote.FreightTotal, 2) + Math.Round(Quote.Gst, 2);

                EnableDisableDiscountText();
            }
        }

        private void EnableDisableDiscountText()
        {
            if (DiscountStructure != null || DiscountStructure.Count > 0)
            {
                bool disApplied = false;
                if (Quote.QuoteDetails != null)
                {
                    //Apply non discount products
                    var noDiscountProducts = UserData.MetaData.SingleOrDefault(x => x.KeyName == "no_discount_products");
                    string[] noDiscountProductsArr = noDiscountProducts.Description.Split('|');

                    foreach (var item in Quote.QuoteDetails)
                    {
                        if (item.Product != null)
                        {
                            bool isNoDisExists = noDiscountProductsArr.Any(x => Convert.ToInt16(x) == item.Product.ProductID);
                            if (isNoDisExists == false)
                            {
                                foreach (var items in DiscountStructure)
                                {
                                    if (item.Product != null && items.Discount > 0 && items.Category.CategoryID == item.Product.Category.CategoryID && item.Discount == items.Discount)
                                    {
                                        disApplied = true;
                                        break;
                                    }
                                }
                            }                            
                        }
                    }
                }
                DiscountAppliedTextVisibility = disApplied ? "Visible" : "Collapsed";
            }
        }

        private void SearchProduct(object parameter)
        {
            int index = Quote.QuoteDetails.IndexOf(parameter as QuoteDetails);
            if (index > -1 && index < Quote.QuoteDetails.Count)
            {
                SalesOrder salesOrder = new SalesOrder();
                salesOrder.SalesOrderDetails = new ObservableCollection<SalesOrderDetails>();

                foreach (var item in Quote.QuoteDetails)
                {
                    salesOrder.SalesOrderDetails.Add(item);
                }

                var childWindow = new ChildWindow();
                childWindow.productCodeSearch_Closed += (r =>
                {
                    if (r != null && r.ProductCode != null)
                    {
                        Quote.QuoteDetails[index].Product = r;
                        Quote.QuoteDetails[index].Product.ProductCode = r.ProductCode;
                    }
                });
                childWindow.ShowProductSearch(UserData.UserName, UserData.State, UserData.UserPrivilages, UserData.MetaData, salesOrder, DiscountStructure, "New");

            }
        }

        private void LoadProductStock()
        {
            productStockList = DBAccess.GetProductStockByStock(1);//Get product stock
        }

        void productChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            this.ItemCount = this.Quote.QuoteDetails.Count;

            // Resequence list
            SequencingService.SetCollectionSequence(this.Quote.QuoteDetails);
            CalculateFinalTotal();
        }


        void freightChanged(object sender, ListChangedEventArgs e)
        {
            if (Quote.FreightCarrier != null && Quote.FreightCarrier.FreightName == "Customer Collect")
            {
                //SalesOrder.FreightCarrier.FreightName = "Select";
                //FreightList = new ObservableCollection<FreightCarrier>();
            }
            CalculateFinalTotal();
        }

        private void CalculateFreightTotal()
        {


        }

        private void RemoveZeroQuoteDetails()
        {
            var itemToRemove = Quote.QuoteDetails.Where(x => x.Product == null).ToList();
            foreach (var item in itemToRemove)
            {
                Quote.QuoteDetails.Remove(item);
            }
        }

        //private void RemoveZeroFreightDetails()
        //{
        //    var itemToRemove = Quote.FreightDetails.Where(x => x.FreightCodeDetails == null).ToList();
        //    foreach (var item in itemToRemove)
        //    {
        //        Quote.FreightDetails.Remove(item);
        //    }
        //}

        private void LoadFreightCodes()
        {
            FreightCodeDetails = DBAccess.GetFreightCodes();
            FreightCodeDetails.Add(new FreightCode() { ID = 0, Code = "Select", Description = "", Price = 0, Unit = "" });
        }

        private void LoadFreights()
        {
            FreightList = DBAccess.GetFreightData();
            FreightList.Add(new FreightCarrier() { FreightName = "Select", FreightDescription = "--" });
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

        public bool CanClose
        {
            get
            {
                return this.IsDirty == false || MessageBox.Show("Changes were made. Do you want to close this window?", "Close", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
            }
        }

        private void ClearFields()
        {
            SelectedCustomer = null;
            SelectedCustomer = new Customer();
            SelectedCustomer.ContactPerson = new List<ContactPerson>();
            SelectedCustomer.ContactPerson.Add(new ContactPerson() { ContactPersonID = -1, ContactPersonName = "Other", CustomerID = SelectedCustomer.CustomerId });

            PhoneCopyVisibility = "Collapsed";
            PrepaidCustomerName = "Select";
            SelectedShipTo = "Customer Address";
            SelectedCategory = new Category() { CategoryID = 0, CategoryName = "Select" };
            IsPrimaryBusinessEnabled = false;

            quote = new Models.Quoting.Quote();
            quote.QuoteDetails = new ObservableCollection<QuoteDetails>();
            Quote = new Quote();
            Quote.QuoteDetails = new ObservableCollection<QuoteDetails>();
            Quote.QuoteDetails.CollectionChanged += productChanged;
            Quote.FreightDetails = new BindingList<FreightDetails>();
            Quote.FreightDetails.ListChanged += freightChanged;
            Quote.Customer = new Customer();
            Quote.FreightCarrier = new FreightCarrier();
            Quote.ContactPerson = new ContactPerson();
            Quote.ContactPerson.ContactPersonName = "Other";
            Quote.QuoteDate = DateTime.Now;
            AddShippingAddress = false;
            GstEnabled = false;
            AddCustomerToDatabase = false;
            AddUpdateBackground = "#FFDEDEDE";
            AddUpdateActive = false;

            DiscountStructure.Clear();
            DiscountStructure = new List<DiscountStructure>();
            if (DisplayDiscountStructure != null)
            {
                DisplayDiscountStructure.Clear();
                DisplayDiscountStructure = new ObservableCollection<DiscountStructure>();
            }

            CustomerTypeEnabled = false;
            ProjectNameEnabled = false;
            SoldToEnabled = false;
            ShipToEnabled = false;
            AddShippingAddToProfileEnabled = false;
            QuoteDetailsEnabled = false;
            CourierNameEnabled = false;
            FreightDetailsEnabled = false;
            InstructionsEnabled = false;
            InternalCommentsEnabled = false;
            DiscountAppliedTextVisibility = "Collapsed";
            QuoteNoVisibility = "Collapsed";
            OtherContactNameEmail = string.Empty;
            OtherContactNamePhone1 = string.Empty;
            OtherContactNamePhone2 = string.Empty;
            OtherContactName = string.Empty;
            ContactPersonEnabled = false;
            AddCustomerVisibility = "Collapsed";
            AddCompanyAddressToShipping = false;
            IsCopyPhone = false;
            IsCopyEmail = false;
            TickCopyPhoneEnabled = false;
            TickCopyEmailEnabled = false;

            Quote.User = new User() { ID = UserData.UserID, FullName = UserData.FirstName + " " + UserData.LastName };
            Quote.LastUpdatedBy = new User() { ID = UserData.UserID, FullName = UserData.FirstName + " " + UserData.LastName };
            Quote.LastUpdatedDate = DateTime.Now;
            Quote.GSTActive = true;


            LoadFreightCodes();

            SelectedFreightDetails = new FreightDetails();
            SelectedFreightDetails.FreightCodeDetails = new FreightCode() { ID = 0, Code = "Select", Description = "", Price = 0, Unit = "" };

            //CustomerChanged();

        }

        private bool CheckProjectNameAvailable()
        {
            bool isAvailable = true;

            isAvailable = DBAccess.CheckQuoteProjectName(Quote.ProjectName);

            return isAvailable;
        }

        private void SubmitQuote()
        {
            bool dupExist = false;
            bool checkPriceNotZero = false;
            RemoveZeroQuoteDetails();
            //RemoveZeroFreightDetails();

            var prodExists = Quote.QuoteDetails.Where(x => x.Product != null && x.Quantity != 0).ToList();

            if (Quote.QuoteDetails != null)
            {
                //Checking duplicates
                string[] prods = null;
                prods = MetaDataManager.GetPriceEditingProducts();

                if (prods != null)
                {
                    var duplicates = Quote.QuoteDetails.GroupBy(s => s.Product.ProductID)
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
                checkPriceNotZero = Quote.QuoteDetails.Any(x => x.QuoteUnitPrice == 0);
            }

            ObservableCollection<QuoteDetails> tempQuoteDetails = new ObservableCollection<QuoteDetails>();
            foreach (var item in Quote.QuoteDetails)
            {
                tempQuoteDetails.Add(new QuoteDetails() { Product = new Product() { ProductID = item.Product.ProductID } });
            }

            if (string.IsNullOrWhiteSpace(PrepaidCustomerName) && SelectedCustomer.CompanyName == "Select")
            {
                MessageBox.Show("Please select customer name", "Select Customer Name", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (string.IsNullOrWhiteSpace(Quote.ProjectName))
            {
                MessageBox.Show("Project name required", "Enter Project Name", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (Quote.ContactPerson.ContactPersonName == "Other" && string.IsNullOrWhiteSpace(OtherContactName))
            {
                MessageBox.Show("Enter contact person name", "Contact Person Name Required", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (Quote.ContactPerson.ContactPersonName == "No Contact")
            {
                MessageBox.Show("Contact person required", "", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (Quote.ContactPerson.ContactPersonName == "Other" && (string.IsNullOrWhiteSpace(OtherContactNamePhone1) && string.IsNullOrWhiteSpace(OtherContactNamePhone2)))
            {
                MessageBox.Show("Phone number required", "", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (Quote.ContactPerson.ContactPersonName == "Other" && string.IsNullOrWhiteSpace(OtherContactNameEmail))
            {
                MessageBox.Show("E-mail address required", "", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (Quote.ContactPerson.ContactPersonName == "Other" && !string.IsNullOrWhiteSpace(OtherContactNameEmail) && !Regex.IsMatch(OtherContactNameEmail, @"^[a-zA-Z][\w\.-]*[a-zA-Z0-9]@[a-zA-Z0-9][\w\.-]*[a-zA-Z0-9]\.[a-zA-Z][a-zA-Z\.]*[a-zA-Z]$"))
            {
                MessageBox.Show("Valid E-mail address required", "", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (!string.IsNullOrWhiteSpace(Quote.ContactPerson.ContactPersonName) && Quote.ContactPerson.ContactPersonName != "Other" && (string.IsNullOrWhiteSpace(Quote.ContactPerson.PhoneNumber1) && string.IsNullOrWhiteSpace(Quote.ContactPerson.PhoneNumber2)))
            {
                MessageBox.Show("Phone number required", "", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (!string.IsNullOrWhiteSpace(Quote.ContactPerson.ContactPersonName) && Quote.ContactPerson.ContactPersonName != "Other" && string.IsNullOrWhiteSpace(Quote.ContactPerson.Email))
            {
                MessageBox.Show("E-mail address required", "", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if ((AddCustomerToDatabase && SelectedCategory == null) || (AddCustomerToDatabase && SelectedCategory.CategoryName == "Select"))
            {
                MessageBox.Show("Please select primary business", "", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (SelectedShipTo == "Customer Address" && string.IsNullOrWhiteSpace(ShippingAddress))
            {
                MessageBox.Show("Ship to address required", "Sold To Address Required", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (CheckProjectNameAvailable())
            {
                MessageBox.Show("Project name exists. Please select a different name", "Project Name Exists", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (Quote.QuoteDetails.Count == 0)
            {
                MessageBox.Show("Enter product details", "Product Details Required", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (dupExist == true)
            {
                MessageBox.Show("Duplicate products exist. Please remove the duplicate products ", "Duplicate Products Exist", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (checkPriceNotZero)
            {
                MessageBox.Show("Enter price for product(s)", "Price Required", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (SelectedFreightDetails == null || SelectedFreightDetails.FreightCodeDetails == null || string.IsNullOrWhiteSpace(SelectedFreightDetails.FreightCodeDetails.Code) || SelectedFreightDetails.FreightCodeDetails.Code == "Select")
            {
                MessageBox.Show("Enter freight details", "Freight Details Required", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (string.IsNullOrWhiteSpace(SelectedFreightDetails.PalletsStr))
            {
                MessageBox.Show("Enter no of pallets", "Pallets Required", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (string.IsNullOrWhiteSpace(SelectedFreightDetails.FreightCodeDetails.Description))
            {
                MessageBox.Show("Freight description required", "Freight Description Required", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            //else if (string.IsNullOrWhiteSpace(Quote.QuoteCourierName) && SelectedShipTo.Equals("Customer Address"))
            //{
            //    MessageBox.Show("Enter Courier name", "Courier Name Required", MessageBoxButton.OK, MessageBoxImage.Error);
            //}
            else
            {
                string wholeFilePath = string.Empty;
                string filePath = string.Empty;
                string fileName = string.Empty;
                Tuple<int, int, string> insQteTuple = null;
                Exception exception = null;
                bool canDirectoryAccessed = true;
                BackgroundWorker worker = new BackgroundWorker();
                ChildWindow LoadingScreen = new ChildWindow();

                LoadingScreen.ShowWaitingScreen("Working");

                worker.DoWork += (_, __) =>
                {

                    Quote.QuoteNo = 0;
                    Quote.Customer = SelectedCustomer;
                    Quote.User = new User();
                    Quote.User.FullName = UserData.FirstName + " " + UserData.LastName;
                    Quote.User.ID = UserData.UserID;
                    Quote.LastUpdatedBy = new User() { ID = UserData.UserID, FullName = UserData.FirstName + " " + UserData.LastName };
                    Quote.LastUpdatedDate = DateTime.Now;
                    Quote.QuoteDate = DateTime.Now;

                    if (string.IsNullOrWhiteSpace(OtherContactNamePhone1) && !string.IsNullOrWhiteSpace(OtherContactNamePhone2))
                    {
                        OtherContactNamePhone1 = OtherContactNamePhone2;
                        OtherContactNamePhone2 = string.Empty;
                    }

                    filePath = FilePathManager.GetQuoteSavingPath();

                    Quote.FreightDetails = new BindingList<FreightDetails>();
                    Quote.FreightDetails.Add(SelectedFreightDetails);


                    insQteTuple = DBAccess.InsertQuote(Quote, OtherContactName, OtherContactNamePhone1 == null ? "" : OtherContactNamePhone1, OtherContactNamePhone2 == null ? "" : OtherContactNamePhone2,
                                                       AddShippingAddress, ShippingAddress, ShippingCity, ShippingState, ShippingPostCode, ShippingCountry, PrepaidCustomerName, OtherContactNameEmail,
                                                       CustomerType, SelectedCategory, AddCustomerToDatabase, DisplayDiscountStructure, quote);

                    if (insQteTuple.Item2 == 1)
                    {
                        Quote.QuoteNo = insQteTuple.Item1;

                        //Create directory if it does not exist
                        try
                        {
                            Directory.CreateDirectory(filePath);
                        }
                        catch (DirectoryNotFoundException dirEx)
                        {
                            canDirectoryAccessed = false;
                        }
                        Quote.FileName = insQteTuple.Item3;
                        fileName = insQteTuple.Item3;

                        if (insQteTuple.Item2 == 1 && canDirectoryAccessed)
                        {
                            //ContactPerson cp = new ContactPerson();
                            //if (Quote.ContactPerson.ContactPersonID == -1)
                            //{
                            //    cp.ContactPersonName = OtherContactName;
                            //    cp.PhoneNumber1 = OtherContactNamePhone1;
                            //    cp.PhoneNumber2 = OtherContactNamePhone2;
                            //    cp.Email = OtherContactNameEmail;
                            //}
                            //else
                            //{
                            //    cp.ContactPersonName = Quote.ContactPerson.ContactPersonName;
                            //    cp.PhoneNumber1 = Quote.ContactPerson.PhoneNumber1;
                            //    cp.PhoneNumber2 = Quote.ContactPerson.PhoneNumber2;
                            //    cp.Email = Quote.ContactPerson.Email;
                            //}
                            UpdateQuotePDF uqpdf = new UpdateQuotePDF(Quote);
                            exception = uqpdf.CreateQuote();
                        }
                    }
                };

                worker.RunWorkerCompleted += delegate (object s, RunWorkerCompletedEventArgs args)
                {
                    LoadingScreen.CloseWaitingScreen();

                    //Refresh UpdateQuoteViewModel QuoteList grid
                    if (ViewUpdateQuoteViewModel.instance != null)
                    {
                        viewUpdateQuoteViewModelInstance = ViewUpdateQuoteViewModel.instance;
                        viewUpdateQuoteViewModelInstance.RefreshGrid();
                    }

                    if (insQteTuple.Item2 == 1)
                    {
                        if (insQteTuple.Item2 == 1)
                        {
                            //Update Customer shipping address
                            if (AddShippingAddress)
                            {
                                UpdateNewCustomerShippingAddress();
                            }

                            MessageBox.Show("Quote created successfully!", "Quote No - " + insQteTuple.Item1 + " Created", MessageBoxButton.OK, MessageBoxImage.Information);
                        }

                        if (canDirectoryAccessed)
                        {
                            if (exception != null)
                            {
                                MessageBox.Show("Cannot open this quote. Please try again later." + System.Environment.NewLine + exception, "Cannot Open Quote", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.Yes);
                            }
                            else
                            {
                                var childWindow = new ChildWindow();
                                childWindow.ShowFormula(filePath + "/" + fileName);
                            }
                        }
                        else
                        {
                            MessageBox.Show("System was unable to access path (" + filePath + ")" + System.Environment.NewLine + "The Quote created successfully! but the PDF was unable to create." + System.Environment.NewLine + "Please check the file path or try enabling VPN", "Cannot Access Path", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.Yes);
                        }

                        if (insQteTuple.Item2 == 1)
                        {
                            ClearFields();

                        }
                    }
                    else if (insQteTuple.Item2 == 0)
                    {
                        MessageBox.Show("There has been an error when creating this quote " + System.Environment.NewLine + "Please try again", "Error Occured", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                    }
                    else if (insQteTuple.Item2 == -2)
                    {
                        MessageBox.Show("Duplicate contact name exists for the customer " + SelectedCustomer.CompanyName + System.Environment.NewLine + "Enter a different name", "Duplicate Name Exists", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                    }
                    else if (insQteTuple.Item2 == -3)
                    {
                        MessageBox.Show("Cannot add the new customer to the database" + System.Environment.NewLine + "Please try again later", "", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                    }
                    else if (insQteTuple.Item2 == -4)
                    {
                        MessageBox.Show("This customer exists in the database" + System.Environment.NewLine + "Please choose another name", "Customer Exists", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    //CloseWindow(new Object());
                };
                worker.RunWorkerAsync();
            }
        }

        private void UpdateNewCustomerShippingAddress()
        {
            for (int i = 0; i < CustomerList.Count; i++)
            {
                if (CustomerList[i].CustomerId == SelectedCustomer.CustomerId)
                {
                    CustomerList[i].ShipAddress = SelectedCustomer.ShipAddress;
                    CustomerList[i].ShipCity = SelectedCustomer.ShipCity;
                    CustomerList[i].ShipState = SelectedCustomer.ShipState;
                    CustomerList[i].ShipPostCode = SelectedCustomer.ShipPostCode;
                    CustomerList[i].ShipCountry = SelectedCustomer.ShipCountry;
                }
            }
        }

        private bool CanExecute(object parameter)
        {
            return true;
        }

        
                

        private void CustomerChanged()
        {

            DiscountAppliedTextVisibility = "Collapsed";
            ContactPersonPhoneVisibility = "Collapsed";

            Quote.GSTActive = true;
            Quote.Instructions = string.Empty;
            Quote.InternalComments = string.Empty;

            OtherContactName = string.Empty;
            OtherContactNamePhone1 = string.Empty;
            OtherContactNamePhone2 = string.Empty;
            OtherContactNameEmail = string.Empty;

            SelectedShipTo = "Customer Address";

            if (SelectedCustomer != null && SelectedCustomer.CustomerId > 0 && SelectedCustomer.CompanyName != null && SelectedCustomer.CompanyName != "Select")
            {
                if (Quote.QuoteDetails != null && Quote.QuoteDetails.Count > 0)
                {
                    Quote.QuoteDetails.Clear();
                }
            }
            else
            {
                DiscountStructure.Clear();
                DiscountStructure = new List<DiscountStructure>();
                if (DisplayDiscountStructure != null)
                {
                    DisplayDiscountStructure.Clear();
                    DisplayDiscountStructure = new ObservableCollection<DiscountStructure>();
                }

                ClearShippingDetails();

                if (!string.IsNullOrWhiteSpace(PrepaidCustomerName))
                {
                    //CustomerTypeEnabled = true;
                    ProjectNameEnabled = true;
                    SoldToEnabled = true;
                    QuoteDetailsEnabled = true;
                    CourierNameEnabled = true;
                    FreightDetailsEnabled = true;
                    InstructionsEnabled = true;
                    InternalCommentsEnabled = true;
                    ShipToEnabled = true;
                    AddShippingAddToProfileEnabled = false;
                    GstEnabled = true;
                    ContactPersonEnabled = false;
                    Quote = new Quote();
                    Quote.GSTActive = true;
                    //Quote.QuoteDetails = new ObservableCollection<QuoteDetails>();
                    //Quote.QuoteDetails.CollectionChanged += productChanged;
                    if (Quote.QuoteDetails != null)
                    {
                        Quote.QuoteDetails.Clear();
                    }

                    Quote.User = new User();
                    CustomerType = "Prepaid";
                }
                else
                {
                    //System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    //{
                    //CustomerTypeEnabled = false;
                    ProjectNameEnabled = false;
                    SoldToEnabled = false;
                    ShipToEnabled = false;
                    AddShippingAddToProfileEnabled = false;
                    FreightDetailsEnabled = false;
                    InstructionsEnabled = false;
                    InternalCommentsEnabled = false;
                    GstEnabled = false;
                    ContactPersonEnabled = false;

                    Quote.ProjectName = string.Empty;
                    Quote.ShipTo = string.Empty;
                    //Quote.FreightDetails.Clear();
                    if (Quote.QuoteDetails != null && Quote.QuoteDetails.Count > 0)
                    {
                        Quote.QuoteDetails.Clear();
                    }
                    Quote.Instructions = string.Empty;
                    Quote.InternalComments = string.Empty;

                    CustomerType = string.Empty;

                    DiscountStructure.Clear();
                    DiscountStructure = new List<DiscountStructure>();
                    if (DisplayDiscountStructure != null)
                    {
                        DisplayDiscountStructure.Clear();
                        DisplayDiscountStructure = new ObservableCollection<DiscountStructure>();
                    }
                    //});
                }
            }
        }

        private void ClearShippingDetails()
        {
            ShippingAddress = string.Empty;
            ShippingCity = string.Empty;
            ShippingState = string.Empty;
            ShippingPostCode = string.Empty;
            ShippingCountry = string.Empty;
        }

        private void AddUpdateDiscount()
        {
            SalesOrder so = new SalesOrder();
            so.SalesOrderNo = 0;

            var childWindow = new ChildWindow();
            childWindow.showAddUpdateDiscountView_Closed += (r =>
            {
                if (r != null && r.Count > 0)
                {
                    MessageBox.Show("New discounts will be updated in the customer profile once this quote is made", "", MessageBoxButton.OK, MessageBoxImage.Information);

                    string catName = string.Empty;
                    if (SelectedCategory != null)
                    {
                        catName = SelectedCategory.CategoryName;
                    }

                    DiscountStructure.Clear();
                    DisplayDiscountStructure = new ObservableCollection<DiscountStructure>();
                    Categories = DBAccess.GetCategories();
                    if (SelectedCategory == null && !string.IsNullOrWhiteSpace(catName))
                    {
                        SelectedCategory = new Category();
                        SelectedCategory.CategoryName = catName;
                    }

                    foreach (var item in Categories)
                    {
                        if (item.CategoryID != 8 || item.CategoryID != 9)
                        {
                            var z = r.FirstOrDefault(x => x.Category.CategoryID == item.CategoryID);

                            if (z != null && z.Discount > 0)
                            {
                                string vis = "Collapsed";
                                if (item.CategoryID == 3)
                                {
                                    if (z.Discount == 60 || z.Discount == 58)
                                    {
                                        vis = "Visible";
                                    }
                                }

                                DisplayDiscountStructure.Add(new DiscountStructure()
                                {
                                    CustomerID = SelectedCustomer.CustomerId,
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
                                    DiscountStr = item.CategoryName + " " + z.Discount + "%",
                                    DiscountLabelVisibility = vis,
                                    TimeStamp = z.TimeStamp
                                });
                            }
                        }
                    }
                }
                if (DisplayDiscountStructure != null)
                {
                    DiscountStructure = new List<DiscountStructure>(DisplayDiscountStructure);
                }
                addDiscountToPrice = true;
                FixDiscount();
                EnableDisableDiscountText();
            });
            childWindow.ShowAddUpdateDiscountView(SelectedCustomer.CustomerId, DisplayDiscountStructure);
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

        public Customer SelectedCustomer
        {
            get
            {

                return _selectedCustomer;
            }
            set
            {
                _selectedCustomer = value;
                RaisePropertyChanged("SelectedCustomer");

                OtherContactName = string.Empty;
                OtherContactNameEmail = string.Empty;
                OtherContactNamePhone1 = string.Empty;
                OtherContactNamePhone2 = string.Empty;
                SelectedShipTo = "Customer Address";
                //IsCopyPhone = false;
                //IsCopyEmail = false;

                if (SelectedCustomer != null && !string.IsNullOrWhiteSpace(SelectedCustomer.CompanyName) && SelectedCustomer.CompanyName != "Select")
                {
                    CustomerType = SelectedCustomer.CustomerId == 0 ? "Prepaid" : SelectedCustomer.CustomerType;
                    AddUpdateBackground = "#666666";
                    AddUpdateActive = true;

                    if (SelectedCustomer.PrimaryBusiness != null)
                    {
                        if (SelectedCategory == null)
                        {
                            SelectedCategory = new Category();
                        }

                        SelectedCategory = SelectedCustomer.PrimaryBusiness;
                        IsPrimaryBusinessEnabled = false;
                    }
                    else
                    {
                        if (SelectedCategory == null)
                        {
                            SelectedCategory = new Category();
                        }
                        SelectedCategory = new Category() { CategoryID = 0, CategoryName = "Select" };
                        IsPrimaryBusinessEnabled = true;
                    }
                    PhoneCopyVisibility = "Visible";
                    AddCustomerVisibility = "Collapsed";
                    TickAddThisCusLabel = "Collapsed";
                    TickCopyPhoneEnabled = string.IsNullOrWhiteSpace(SelectedCustomer.CompanyTelephone) ? false : true;
                    TickCopyEmailEnabled = string.IsNullOrWhiteSpace(SelectedCustomer.CompanyEmail) ? false : true;
                    CustomerTypeEnabled = false;

                    if (originalCustomer.Equals(SelectedCustomer.CompanyName))
                    {
                        if (quote != null && quote.QuoteNo > 0)
                        {
                            if (!string.IsNullOrWhiteSpace(quote.Customer.ShipAddress) && !string.IsNullOrWhiteSpace(SelectedCustomer.ShipAddress))
                            {
                                ShippingAddress = SelectedCustomer.ShipAddress;
                                ShippingCity = SelectedCustomer.ShipCity;
                                ShippingState = SelectedCustomer.ShipState;
                                ShippingPostCode = SelectedCustomer.ShipPostCode;
                                ShippingCountry = SelectedCustomer.ShipCountry;
                            }
                            else if (!string.IsNullOrWhiteSpace(quote.Customer.ShipAddress) && string.IsNullOrWhiteSpace(SelectedCustomer.ShipAddress))
                            {
                                ShippingAddress = quote.Customer.ShipAddress == "Collect from A1 Rubber QLD" || quote.Customer.ShipAddress == "Collect from A1 Rubber NSW" ? "" : quote.Customer.ShipAddress;
                                ShippingCity = quote.Customer.ShipCity;
                                ShippingState = quote.Customer.ShipState;
                                ShippingPostCode = quote.Customer.ShipPostCode;
                                ShippingCountry = quote.Customer.ShipCountry;
                            }
                            else if (string.IsNullOrWhiteSpace(quote.Customer.ShipAddress) && !string.IsNullOrWhiteSpace(SelectedCustomer.ShipAddress))
                            {
                                ShippingAddress = SelectedCustomer.ShipAddress;
                                ShippingCity = SelectedCustomer.ShipCity;
                                ShippingState = SelectedCustomer.ShipState;
                                ShippingPostCode = SelectedCustomer.ShipPostCode;
                                ShippingCountry = SelectedCustomer.ShipCountry;
                            }
                        }
                    }
                    else
                    {
                        ShippingAddress = SelectedCustomer.ShipAddress;
                        ShippingCity = SelectedCustomer.ShipCity;
                        ShippingState = SelectedCustomer.ShipState;
                        ShippingPostCode = SelectedCustomer.ShipPostCode;
                        ShippingCountry = SelectedCustomer.ShipCountry;
                    }


                    GstEnabled = true;
                    QuoteDetailsEnabled = true;
                    CourierNameEnabled = true;
                    InstructionsEnabled = true;
                    InternalCommentsEnabled = true;
                    DiscountAppliedTextVisibility = "Collapsed";

                    if (SelectedCustomer.CustomerId > 0)
                    {
                        DiscountStructure.Clear();
                        DiscountStructure = DBAccess.GetDiscount(SelectedCustomer.CustomerId);
                        DisplayDiscountStructure = new ObservableCollection<DiscountStructure>(DiscountStructure);
                        if (DisplayDiscountStructure.Count > 0)
                        {
                            for (int i = DisplayDiscountStructure.Count - 1; i >= 0; i--)
                            {
                                if (DisplayDiscountStructure[i].Discount == 0)
                                    DisplayDiscountStructure.RemoveAt(i);
                            }
                        }
                    }

                    if (!originalCustomer.Equals(SelectedCustomer.CompanyName))
                    {
                        Quote.QuoteDetails = new ObservableCollection<QuoteDetails>();
                        Quote.QuoteDetails.CollectionChanged += productChanged;

                        SelectedFreightDetails = new FreightDetails();
                        SelectedFreightDetails.FreightCodeDetails = new FreightCode();
                        SelectedFreightDetails.FreightCodeDetails = new FreightCode() { ID = 0, Code = "Select", Description = "", Price = 0, Unit = "" };
                        SelectedFreightDetails.FreightCodeDetails.PriceEnabled = false;

                        Quote.ContactPerson = new ContactPerson();

                        Quote.Instructions = string.Empty;
                        Quote.InternalComments = string.Empty;
                        Quote.QuoteCourierName = string.Empty;
                        //CourierNameEnabled = false;
                    }

                    //Get contact Person details
                    if (SelectedCustomer != null)
                    {
                        List<ContactPerson> cp = DBAccess.GetContactPersonByCustomerID(SelectedCustomer.CustomerId);
                        cp.Add(new ContactPerson() { ContactPersonID = -1, ContactPersonName = "Other", CustomerID = SelectedCustomer.CustomerId });
                        SelectedCustomer.ContactPerson = cp;

                        if (quote != null && quote.ContactPerson != null && SelectedCustomer.CompanyName != null && SelectedCustomer.CompanyName.Equals(quote.Customer.CompanyName))
                        {
                            if (Quote.ContactPerson != null)
                            {
                                Quote.ContactPerson.ContactPersonID = quote.ContactPerson.ContactPersonID;
                                Quote.ContactPerson = quote.ContactPerson;
                                Quote.ContactPerson.ContactPersonName = quote.ContactPerson.ContactPersonName;
                                Quote.ContactPerson.PhoneNumber1 = quote.ContactPerson.PhoneNumber1;
                                Quote.ContactPerson.Email = quote.ContactPerson.Email;
                            }
                            else
                            {
                                Quote.ContactPerson = new ContactPerson();
                                Quote.ContactPerson.ContactPersonName = "Other";
                            }                           
                        }
                        else
                        {
                            if (Quote.ContactPerson == null)
                            {
                                Quote.ContactPerson = new ContactPerson();
                            }
                            Quote.ContactPerson.ContactPersonName = "Other";
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(SelectedCustomer.CompanyAddress) || !string.IsNullOrWhiteSpace(SelectedCustomer.CompanyCity) || !string.IsNullOrWhiteSpace(SelectedCustomer.CompanyState) ||
                        !string.IsNullOrWhiteSpace(SelectedCustomer.CompanyPostCode))
                    {
                        AddCcompanyAddToShippingEnabled = true;
                    }
                    else
                    {
                        AddCcompanyAddToShippingEnabled = false;
                        AddCompanyAddressToShipping = false;
                    }
                }
                else
                {
                    //SelectedCustomer = new Customer();
                    //SelectedCustomer.CustomerId = 0;

                    quote = new Quote();

                    Quote.QuoteDetails = new ObservableCollection<QuoteDetails>();
                    Quote.QuoteDetails.CollectionChanged += productChanged;

                    SelectedFreightDetails = new FreightDetails();
                    SelectedFreightDetails.FreightCodeDetails = new FreightCode();
                    //SelectedFreightDetails.FreightCodeDetails.Code = "Select";
                    SelectedFreightDetails.FreightCodeDetails = new FreightCode() { ID = 0, Code = "Select", Description = "", Price = 0, Unit = "" };
                    SelectedFreightDetails.FreightCodeDetails.PriceEnabled = false;
                    Quote.QuoteCourierName = string.Empty;
                    CourierNameEnabled = false;

                    if (FreightCodeDetails != null)
                    {
                        var data = FreightCodeDetails.SingleOrDefault(x => x.Code == "Freight Out");
                        if (data != null)
                        {
                            data.Price = 0;
                        }
                    }

                    SelectedShipTo = "Customer Address";
                    ShippingAddress = string.Empty;
                    ShippingCity = string.Empty;
                    ShippingCountry = string.Empty;
                    ShippingPostCode = string.Empty;
                    ShippingState = string.Empty;

                    Quote.Instructions = string.Empty;
                    Quote.InternalComments = string.Empty;

                    DiscountStructure.Clear();
                    if (DisplayDiscountStructure != null)
                    {
                        DisplayDiscountStructure.Clear();
                        DisplayDiscountStructure = new ObservableCollection<DiscountStructure>();
                    }


                    QuoteDetailsEnabled = true;
                    CourierNameEnabled = true;
                    SoldToEnabled = false;
                    AddShippingAddToProfileEnabled = false;
                    AddCcompanyAddToShippingEnabled = false;
                    AddCompanyAddressToShipping = false;
                    FreightDetailsEnabled = false;

                    AddUpdateBackground = "#FFDEDEDE";
                    AddUpdateActive = false;
                }

                CalculateFinalTotal();
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

        public string OtherContactName
        {
            get
            {
                return _otherContactName;
            }
            set
            {
                _otherContactName = value;
                RaisePropertyChanged("OtherContactName");
            }
        }

        public string OtherContactNamePhone1
        {
            get
            {
                return _otherContactNamePhone1;
            }
            set
            {
                _otherContactNamePhone1 = value;
                RaisePropertyChanged("OtherContactNamePhone1");
            }
        }

        public string OtherContactNameEmail
        {
            get
            {
                return _otherContactNameEmail;
            }
            set
            {
                _otherContactNameEmail = value;
                RaisePropertyChanged("OtherContactNameEmail");
            }
        }

        public string OtherContactNamePhone2
        {
            get
            {
                return _otherContactNamePhone2;
            }
            set
            {
                _otherContactNamePhone2 = value;
                RaisePropertyChanged("OtherContactNamePhone2");
            }
        }

        public string OtherContactPersonVisibility
        {
            get
            {
                return _otherContactPersonVisibility;
            }
            set
            {
                _otherContactPersonVisibility = value;
                RaisePropertyChanged("OtherContactPersonVisibility");
            }
        }

        public string ContactPersonPhoneVisibility
        {
            get
            {
                return _contactPersonPhoneVisibility;
            }
            set
            {
                _contactPersonPhoneVisibility = value;
                RaisePropertyChanged("ContactPersonPhoneVisibility");
            }
        }



        public Quote Quote
        {
            get
            {
                if (_quote != null)
                {
                    if (_quote.ContactPerson != null)
                    {

                        if (_quote.ContactPerson.ContactPersonID == -1)
                        {
                            OtherContactPersonVisibility = "Visible";
                            ContactPersonPhoneVisibility = "Collapsed";
                        }
                        else if (_quote.ContactPerson.ContactPersonID == 0)
                        {
                            OtherContactPersonVisibility = "Collapsed";
                            ContactPersonPhoneVisibility = "Collapsed";
                            if (_quote.ContactPerson != null && _quote.ContactPerson.ContactPersonID == 0)
                            {
                                OtherContactPersonVisibility = "Visible";
                                ContactPersonPhoneVisibility = "Collapsed";
                            }
                        }
                        else
                        {
                            OtherContactPersonVisibility = "Collapsed";
                            ContactPersonPhoneVisibility = "Visible";
                        }
                    }
                    else
                    {
                        OtherContactPersonVisibility = "Collapsed";
                        ContactPersonPhoneVisibility = "Collapsed";
                        if (_quote.ContactPerson != null && _quote.ContactPerson.ContactPersonID == 0)
                        {
                            OtherContactPersonVisibility = "Visible";
                            ContactPersonPhoneVisibility = "Collapsed";
                        }
                    }

                    if (SelectedCustomer == null)
                    {
                        OtherContactPersonVisibility = "Collapsed";
                        ContactPersonPhoneVisibility = "Collapsed";
                    }

                    //if (_quote != null)
                    //{
                    //    Console.WriteLine(_quote.GSTActive);
                    //}
                }
                return _quote;
            }
            set
            {
                _quote = value;
                RaisePropertyChanged("Quote");
                if (Quote.ContactPerson == null)
                {
                    OtherContactPersonVisibility = "Visible";
                    ContactPersonPhoneVisibility = "Collapsed";
                }


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

        public ObservableCollection<DiscountStructure> DisplayDiscountStructure
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
        public ObservableCollection<FreightCarrier> FreightList
        {
            get { return _freightList; }
            set
            {
                _freightList = value;
                RaisePropertyChanged("FreightList");
            }
        }


        public bool CustomerTypeEnabled
        {
            get { return _customerTypeEnabled; }
            set
            {
                _customerTypeEnabled = value;
                RaisePropertyChanged("CustomerTypeEnabled");
            }
        }

        public bool ProjectNameEnabled
        {
            get { return _projectNameEnabled; }
            set
            {
                _projectNameEnabled = value;
                RaisePropertyChanged("ProjectNameEnabled");
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

        public bool QuoteDetailsEnabled
        {
            get { return _quoteDetailsEnabled; }
            set
            {
                _quoteDetailsEnabled = value;
                RaisePropertyChanged("QuoteDetailsEnabled");
            }
        }

        public bool FreightDetailsEnabled
        {
            get { return _freightDetailsEnabled; }
            set
            {
                _freightDetailsEnabled = value;
                RaisePropertyChanged("FreightDetailsEnabled");
            }
        }

        public bool InstructionsEnabled
        {
            get { return _instructionsEnabled; }
            set
            {
                _instructionsEnabled = value;
                RaisePropertyChanged("InstructionsEnabled");
            }
        }

        public bool InternalCommentsEnabled
        {
            get { return _internalCommentsEnabled; }
            set
            {
                _internalCommentsEnabled = value;
                RaisePropertyChanged("InternalCommentsEnabled");
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

        public bool AddShippingAddress
        {
            get { return _addShippingAddress; }
            set
            {
                _addShippingAddress = value;
                RaisePropertyChanged("AddShippingAddress");
            }
        }

        public string ShippingAddressVisibility
        {
            get { return _shippingAddressVisibility; }
            set
            {
                _shippingAddressVisibility = value;
                RaisePropertyChanged("ShippingAddressVisibility");
            }
        }

        public string ShipToText
        {
            get { return _shipToText; }
            set
            {
                _shipToText = value;
                RaisePropertyChanged("ShipToText");
            }
        }


        public string SelectedShipTo
        {
            get { return _selectedShipTo; }
            set
            {
                _selectedShipTo = value;
                RaisePropertyChanged("SelectedShipTo");

                if (SelectedShipTo == "Customer Address")
                {
                    ShippingAddressVisibility = "Visible";

                    if (SelectedCustomer != null && ( !string.IsNullOrWhiteSpace(SelectedCustomer.CompanyAddress) || !string.IsNullOrWhiteSpace(SelectedCustomer.CompanyCity) || !string.IsNullOrWhiteSpace(SelectedCustomer.CompanyState) ||
                        !string.IsNullOrWhiteSpace(SelectedCustomer.CompanyPostCode)))
                    {
                        AddCcompanyAddToShippingEnabled = true;
                    }
                    else
                    {
                        AddCcompanyAddToShippingEnabled = false;
                    }
                    
                    ShipToText = string.Empty;
                    ShippingAddress = string.Empty;
                    ShippingCity = string.Empty;
                    ShippingState = string.Empty;
                    ShippingPostCode = string.Empty;
                    ShippingCountry = string.Empty;

                    if (SelectedCustomer != null && !string.IsNullOrWhiteSpace(SelectedCustomer.CompanyName))
                    {
                        if (!string.IsNullOrWhiteSpace(originalCustomer) && originalCustomer.Equals(SelectedCustomer.CompanyName))
                        {

                            if (!string.IsNullOrWhiteSpace(SelectedCustomer.ShipAddress) && quote == null)
                            {
                                ShippingAddress = SelectedCustomer.ShipAddress;
                                ShippingCity = SelectedCustomer.ShipCity;
                                ShippingState = SelectedCustomer.ShipState;
                                ShippingPostCode = SelectedCustomer.ShipPostCode;
                                ShippingCountry = SelectedCustomer.ShipCountry;
                            }
                            else if (string.IsNullOrWhiteSpace(SelectedCustomer.ShipAddress) && quote != null && quote.QuoteNo > 0)
                            {
                                ShippingAddress = quote.Customer.ShipAddress == "Collect from A1 Rubber QLD" || quote.Customer.ShipAddress == "Collect from A1 Rubber NSW" ? "" : quote.Customer.ShipAddress;
                                ShippingCity = quote.Customer.ShipCity;
                                ShippingState = quote.Customer.ShipState;
                                ShippingPostCode = quote.Customer.ShipPostCode;
                                ShippingCountry = quote.Customer.ShipCountry;
                            }
                            else if (quote != null)
                            {
                                ShippingAddress = quote.Customer.ShipAddress;
                                ShippingCity = quote.Customer.ShipCity;
                                ShippingState = quote.Customer.ShipState;
                                ShippingPostCode = quote.Customer.ShipPostCode;
                                ShippingCountry = quote.Customer.ShipCountry;
                            }
                        }
                        else
                        {
                            ShippingAddress = SelectedCustomer.ShipAddress;
                            ShippingCity = SelectedCustomer.ShipCity;
                            ShippingState = SelectedCustomer.ShipState;
                            ShippingPostCode = SelectedCustomer.ShipPostCode;
                            ShippingCountry = SelectedCustomer.ShipCountry;
                        }
                    }

                }
                else if (SelectedShipTo == "A1 Rubber QLD" || SelectedShipTo == "A1 Rubber NSW")
                {
                    if (SelectedShipTo == "A1 Rubber QLD")
                    {
                        ShippingAddressVisibility = "Collapsed";
                        ShipToText = "Collect from A1 Rubber QLD";
                    }
                    else if (SelectedShipTo == "A1 Rubber NSW")
                    {
                        ShippingAddressVisibility = "Collapsed";
                        ShipToText = "Collect from A1 Rubber NSW";
                    }

                    AddCcompanyAddToShippingEnabled = false;
                    AddShippingAddress = false;
                    ShippingAddress = ShipToText;
                    ShippingCity = string.Empty;
                    ShippingState = string.Empty;
                    ShippingPostCode = string.Empty;
                    ShippingCountry = string.Empty;
                }

                ProcessFreight();
            }
        }

        public bool AddCcompanyAddToShippingEnabled
        {
            get { return _addCcompanyAddToShippingEnabled; }
            set
            {
                _addCcompanyAddToShippingEnabled = value;
                RaisePropertyChanged("AddCcompanyAddToShippingEnabled");

            }
        }

        public string PrepaidCustomerName
        {
            get { return _prepaidCustomerName; }
            set
            {
                _prepaidCustomerName = value;
                RaisePropertyChanged("PrepaidCustomerName");

                if (!string.IsNullOrWhiteSpace(PrepaidCustomerName) && !PrepaidCustomerName.Equals("Select"))
                {
                    ProjectNameEnabled = true;
                    ShipToEnabled = true;
                    AddCustomerVisibility = "Visible";
                    TickAddThisCusLabel = "Visible";
                    PhoneCopyVisibility = "Collapsed";
                    AddCustomerToDatabase = false;
                    CustomerTypeEnabled = true;
                    TickCopyPhoneEnabled = false;
                    TickCopyEmailEnabled = false;

                    if (!string.IsNullOrWhiteSpace(originalCustomer) && !originalCustomer.Equals(PrepaidCustomerName))
                    {
                        //Clear all the details if customer is changed
                        originalCustomer = string.Empty;     
                        ShippingAddress = string.Empty;
                        ShippingCity = string.Empty;
                        ShippingState = string.Empty;
                        ShippingPostCode = string.Empty;
                        ShippingCountry = string.Empty;
                        Quote.InternalComments = string.Empty;
                        Quote.Instructions = string.Empty;

                        if (quote != null)
                        {
                            quote = null;
                        }
                    }

                    //This customer is not in DB
                    if (SelectedCustomer == null)
                    {
                        CustomerType = "Prepaid";
                        QuoteDetailsEnabled = true;
                        CourierNameEnabled = true;
                        Quote.QuoteDetails = new ObservableCollection<QuoteDetails>();
                        Quote.QuoteDetails.CollectionChanged += productChanged;

                        SelectedFreightDetails = new FreightDetails();
                        SelectedFreightDetails.FreightCodeDetails = new FreightCode();
                        SelectedFreightDetails.FreightCodeDetails = new FreightCode() { ID = 0, Code = "Select", Description = "", Price = 0, Unit = "" };
                        SelectedFreightDetails.FreightCodeDetails.PriceEnabled = false;

                        Quote.QuoteCourierName = string.Empty;
                        CourierNameEnabled = false;
                                                
                        ContactPersonEnabled = true;
                        AddShippingAddToProfileEnabled = false;
                                                
                        SelectedCustomer = new Customer();
                        SelectedCustomer.ContactPerson = new List<ContactPerson>();
                        SelectedCustomer.ContactPerson.Add(new ContactPerson() { ContactPersonID = -1, ContactPersonName = "Other", CustomerID = SelectedCustomer.CustomerId });
                        Quote.ContactPerson = new ContactPerson();
                        Quote.ContactPerson.ContactPersonName = "Other";
                        
                    }
                    else
                    {
                        AddCcompanyAddToShippingEnabled = false;
                        if (SelectedCustomer.CustomerId == 0)
                        {
                            QuoteDetailsEnabled = true;
                            CourierNameEnabled = true;
                            AddCcompanyAddToShippingEnabled = false;
                            //if (Quote.ContactPerson == null)
                            //{
                            //    Quote.ContactPerson = new ContactPerson();
                            //}

                            //Quote.ContactPerson.ContactPersonName = "Other";
                        }
                        else
                        {                           

                            AddCcompanyAddToShippingEnabled = true;
                            AddCustomerVisibility = "Collapsed";
                            TickAddThisCusLabel = "Collapsed";
                            CustomerTypeEnabled = false;
                            PhoneCopyVisibility = "Visible";
                            AddUpdateActive = true;
                            AddUpdateBackground = "#666666";
                            TickCopyPhoneEnabled = string.IsNullOrWhiteSpace(SelectedCustomer.CompanyTelephone) ? false : true;
                            TickCopyEmailEnabled = string.IsNullOrWhiteSpace(SelectedCustomer.CompanyEmail) ? false : true;
                        }
                        //This customer is in DB
                        ContactPersonEnabled = true;
                        AddShippingAddToProfileEnabled = SelectedCustomer.CustomerId == 0 ? false : true;
                    }
                }
                else
                {
                    originalCustomer = string.Empty;
                    ShippingAddress = string.Empty;
                    ShippingCity = string.Empty;
                    ShippingState = string.Empty;
                    ShippingPostCode = string.Empty;
                    ShippingCountry = string.Empty;
                    Quote.InternalComments = string.Empty;
                    Quote.Instructions = string.Empty;

                    Quote.QuoteDetails = new ObservableCollection<QuoteDetails>();
                    Quote.QuoteDetails.CollectionChanged += productChanged;

                    SelectedFreightDetails = new FreightDetails();
                    SelectedFreightDetails.FreightCodeDetails = new FreightCode() { ID = 0, Code = "Select", Description = "", Price = 0, Unit = "" };
                    SelectedFreightDetails.FreightCodeDetails.PriceEnabled = false;

                    Quote.QuoteCourierName = string.Empty;
                    CourierNameEnabled = false;

                    SelectedShipTo = "Customer Address";
                    ShippingAddress = string.Empty;
                    ShippingCity = string.Empty;
                    ShippingCountry = string.Empty;
                    ShippingPostCode = string.Empty;
                    ShippingState = string.Empty;
                    AddCustomerVisibility = "Collapsed";
                    TickAddThisCusLabel = "Collapsed";
                    CustomerTypeEnabled = false;
                    PhoneCopyVisibility = "Collapsed";
                    TickCopyPhoneEnabled = false;
                    TickCopyEmailEnabled = false;

                    Quote.Instructions = string.Empty;
                    Quote.InternalComments = string.Empty;

                    DiscountStructure.Clear();
                    if (DisplayDiscountStructure != null)
                    {
                        DisplayDiscountStructure.Clear();
                        DisplayDiscountStructure = new ObservableCollection<DiscountStructure>();
                    }

                    QuoteDetailsEnabled = false;                    
                    ProjectNameEnabled = false;
                    SoldToEnabled = false;
                    ShipToEnabled = false;
                    AddShippingAddToProfileEnabled = false;
                    FreightDetailsEnabled = false;
                    InstructionsEnabled = false;
                    InternalCommentsEnabled = false;
                    GstEnabled = false;
                    ContactPersonEnabled = false;
                    CustomerType = string.Empty;
                }
            }
        }

        public string ShippingAddress
        {
            get { return _shippingAddress; }
            set
            {
                _shippingAddress = value;
                RaisePropertyChanged("ShippingAddress");
            }
        }

        public string ShippingCity
        {
            get { return _shippingCity; }
            set
            {
                _shippingCity = value;
                RaisePropertyChanged("ShippingCity");
            }
        }

        public string ShippingState
        {
            get { return _shippingState; }
            set
            {
                _shippingState = value;
                RaisePropertyChanged("ShippingState");
            }
        }

        public string ShippingPostCode
        {
            get { return _shippingPostCode; }
            set
            {
                _shippingPostCode = value;
                RaisePropertyChanged("ShippingPostCode");
            }
        }

        public string ShippingCountry
        {
            get { return _shippingCountry; }
            set
            {
                _shippingCountry = value;
                RaisePropertyChanged("ShippingCountry");
            }
        }

        public bool GstEnabled
        {
            get { return _gstEnabled; }
            set
            {
                _gstEnabled = value;
                RaisePropertyChanged("GstEnabled");
            }
        }

        public bool ContactPersonEnabled
        {
            get { return _contactPersonEnabled; }
            set
            {
                _contactPersonEnabled = value;
                RaisePropertyChanged("ContactPersonEnabled");
            }
        }

        public bool AddShippingAddToProfileEnabled
        {
            get { return _addShippingAddToProfileEnabled; }
            set
            {
                _addShippingAddToProfileEnabled = value;
                RaisePropertyChanged("AddShippingAddToProfileEnabled");


            }
        }

        public string CustomerType
        {
            get { return _customerType; }
            set
            {
                _customerType = value;
                RaisePropertyChanged("CustomerType");
            }
        }



        public string AddCustomerVisibility
        {
            get { return _addCustomerVisibility; }
            set
            {
                _addCustomerVisibility = value;
                RaisePropertyChanged("AddCustomerVisibility");
            }
        }

        public bool AddCustomerToDatabase
        {
            get { return _addCustomerToDatabase; }
            set
            {
                _addCustomerToDatabase = value;
                RaisePropertyChanged("AddCustomerToDatabase");

                IsPrimaryBusinessEnabled = false;
                //if (SelectedCategory == null)
                //{
                //    SelectedCategory = new Category();
                //    SelectedCategory = new Category() { CategoryID = 0, CategoryName = "Select" };
                //}

                if (SelectedCustomer != null && SelectedCustomer.PrimaryBusiness != null && !string.IsNullOrWhiteSpace(SelectedCustomer.PrimaryBusiness.CategoryName))
                {
                    //SelectedCategory.CategoryName = SelectedCustomer.PrimaryBusiness.CategoryName;
                }
                else
                {
                    SelectedCategory = new Category() { CategoryID = 0, CategoryName = "Select" };
                }

                if (AddCustomerToDatabase)
                {
                    IsPrimaryBusinessEnabled = true;
                    AddShippingAddToProfileEnabled = true;
                    AddUpdateActive = true;
                    AddUpdateBackground = "#666666";
                    TickAddThisCusLabel = "Collapsed";
                }
                else
                {

                    AddShippingAddToProfileEnabled = false;
                    AddShippingAddress = false;
                    TickAddThisCusLabel = "Visible";
                    AddUpdateBackground = "#FFDEDEDE";
                    AddUpdateActive = false;

                    if ((SelectedCustomer == null || SelectedCustomer.CustomerId == 0) && DisplayDiscountStructure != null)
                    {
                        DisplayDiscountStructure.Clear();
                    }
                }
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

        public bool IsPrimaryBusinessEnabled
        {
            get
            {
                return _isPrimaryBusinessEnabled;
            }
            set
            {
                _isPrimaryBusinessEnabled = value;
                RaisePropertyChanged("IsPrimaryBusinessEnabled");
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

        public string TickAddThisCusLabel
        {
            get
            {
                return _tickAddThisCusLabel;
            }
            set
            {
                _tickAddThisCusLabel = value;
                RaisePropertyChanged("TickAddThisCusLabel");
            }
        }




        public FreightDetails SelectedFreightDetails
        {
            get
            {
                if (_selectedFreightDetails.FreightCodeDetails == null)
                {
                    _selectedFreightDetails.PalletsStr = string.Empty;
                    _selectedFreightDetails.Discount = 0;
                }
                else
                {
                    if (_selectedFreightDetails.FreightCodeDetails.FreightCodeID != 0)
                    {
                        if (_selectedFreightDetails.FreightCodeDetails.FreightCodeID == 50)
                        {
                            _selectedFreightDetails.FreightCodeDetails.PriceEnabled = true;
                        }
                        else
                        {
                            _selectedFreightDetails.FreightCodeDetails.PriceEnabled = false;
                        }
                    }
                }

                if (_selectedFreightDetails != null && _selectedFreightDetails.FreightCodeDetails != null)
                {
                    if (_selectedFreightDetails.FreightCodeDetails.ID != 37 && _selectedFreightDetails.FreightCodeDetails.ID != 70)
                    {
                        _selectedFreightDetails.Discount = 0;
                    }
                }


                decimal fPrice = _selectedFreightDetails.FreightCodeDetails == null ? 0 : _selectedFreightDetails.FreightCodeDetails.Price;
                decimal pallets = string.IsNullOrWhiteSpace(_selectedFreightDetails.PalletsStr) == true ? 0 : Convert.ToDecimal(_selectedFreightDetails.PalletsStr);
                decimal rowAmount = pallets * fPrice;
                decimal rowDis = (rowAmount * _selectedFreightDetails.Discount) / 100;
                _selectedFreightDetails.Total = rowAmount - rowDis;

                Quote.FreightTotal = fPrice * pallets;

                decimal freightDis = (Quote.FreightTotal * _selectedFreightDetails.Discount) / 100;
                Quote.FreightTotal = Quote.FreightTotal - freightDis;

                if (Quote.GSTActive)
                {
                    Quote.Gst = Quote.GSTActive ? Math.Round((((Quote.ListPriceTotal - Quote.DiscountedTotal) + Quote.FreightTotal) * 10) / 100, 2) : 0;
                }
                else
                {
                    Quote.Gst = 0;
                }


                Quote.TotalBeforeTax = Quote.ListPriceTotal - Quote.DiscountedTotal;

                Quote.TotalAmount = Math.Round(Quote.TotalBeforeTax, 2) + Math.Round(Quote.FreightTotal, 2) + Math.Round(Quote.Gst, 2);

                return _selectedFreightDetails;
            }
            set
            {
                _selectedFreightDetails = value;
                RaisePropertyChanged("SelectedFreightDetails");

                if (SelectedFreightDetails != null)
                {
                    SelectedFreightDetails.PalletsStr = string.Empty;
                                       
                }

                // CalculateFreightTotal();
            }
        }


        public bool AddCompanyAddressToShipping
        {
            get { return _addCompanyAddressToShipping; }
            set
            {
                _addCompanyAddressToShipping = value;
                RaisePropertyChanged("AddCompanyAddressToShipping");

                if (SelectedCustomer != null)
                {
                    if (AddCompanyAddressToShipping)
                    {
                        if (!string.IsNullOrWhiteSpace(SelectedCustomer.CompanyAddress))
                        {
                            ShippingAddress = SelectedCustomer.CompanyAddress;
                        }

                        if (!string.IsNullOrWhiteSpace(SelectedCustomer.CompanyCity))
                        {
                            ShippingCity = SelectedCustomer.CompanyCity;
                        }

                        if (!string.IsNullOrWhiteSpace(SelectedCustomer.CompanyState))
                        {
                            ShippingState = SelectedCustomer.CompanyState;
                        }

                        if (!string.IsNullOrWhiteSpace(SelectedCustomer.CompanyPostCode))
                        {
                            ShippingPostCode = SelectedCustomer.CompanyPostCode;
                        }

                        if (!string.IsNullOrWhiteSpace(SelectedCustomer.CompanyCountry))
                        {
                            ShippingCountry = SelectedCustomer.CompanyCountry;
                        }
                    }
                    else
                    {
                        if (quote != null && quote.Customer != null)
                        {
                            if (!string.IsNullOrWhiteSpace(SelectedCustomer.CompanyAddress))
                            {
                                ShippingAddress = quote.Customer.ShipAddress;
                            }

                            if (!string.IsNullOrWhiteSpace(SelectedCustomer.CompanyCity))
                            {
                                ShippingCity = quote.Customer.ShipCity;
                            }

                            if (!string.IsNullOrWhiteSpace(SelectedCustomer.CompanyState))
                            {
                                ShippingState = quote.Customer.ShipState;
                            }

                            if (!string.IsNullOrWhiteSpace(SelectedCustomer.CompanyPostCode))
                            {
                                ShippingPostCode = quote.Customer.ShipPostCode;
                            }

                            if (!string.IsNullOrWhiteSpace(SelectedCustomer.CompanyCountry))
                            {
                                ShippingCountry = quote.Customer.ShipCountry;
                            }
                        }
                        else
                        {
                            ShippingAddress = SelectedCustomer.ShipAddress;
                            ShippingCity = SelectedCustomer.ShipCity;
                            ShippingState = SelectedCustomer.ShipState;
                            ShippingPostCode = SelectedCustomer.ShipPostCode;
                            ShippingCountry = SelectedCustomer.ShipCountry;
                        }
                    }
                }
            }
        }

        public string PhoneCopyVisibility
        {
            get { return _phoneCopyVisibility; }
            set
            {
                _phoneCopyVisibility = value;
                RaisePropertyChanged("PhoneCopyVisibility");
            }
        }

        private void ContactPersonChanged()
        {           
            if (prevContactPerson != null && Quote.ContactPerson != null)
            {
                //Quote.ContactPerson.PhoneNumber1 = prevContactPerson.PhoneNumber1;
                var data = SelectedCustomer.ContactPerson.SingleOrDefault(x => x.ContactPersonID == prevContactPerson.ContactPersonID);
                if (data != null)
                {
                    if (!string.IsNullOrWhiteSpace(prevContactPerson.PhoneNumber1))
                    {
                        data.PhoneNumber1 = prevContactPerson.PhoneNumber1;
                    }

                    if (!string.IsNullOrWhiteSpace(prevContactPerson.Email))
                    {
                        data.Email = prevContactPerson.Email;
                    }
                }
            }
           
                IsCopyPhone = false;
                IsCopyEmail = false;
           
        }

        private void FreightCodeChanged()
        {
            if (SelectedFreightDetails != null && SelectedFreightDetails.FreightCodeDetails != null && !string.IsNullOrWhiteSpace(SelectedFreightDetails.FreightCodeDetails.Code) && SelectedFreightDetails.FreightCodeDetails.Code != "Select")
            {
                if (quote != null && !string.IsNullOrWhiteSpace(quote.QuoteCourierName))
                {
                    CourierNameEnabled = true;
                    if (freightCodesArr != null)
                    {
                        var data = freightCodesArr.SingleOrDefault(x => Convert.ToInt16(x) == SelectedFreightDetails.FreightCodeDetails.ID);
                        if (data != null && Quote != null)
                        {
                            Quote.QuoteCourierName = "Willmax";
                            CourierNameEnabled = false;
                        }
                        else
                        {
                            if (!quote.QuoteCourierName.Equals("Willmax"))
                            {
                                Quote.QuoteCourierName = quote.QuoteCourierName;
                                CourierNameEnabled = true;
                            }
                            else
                            {
                                Quote.QuoteCourierName = string.Empty;
                                CourierNameEnabled = true;
                            }
                        }
                    }
                }
                else
                {
                    CourierNameEnabled = true;
                    if (freightCodesArr != null)
                    {
                        var data = freightCodesArr.SingleOrDefault(x => Convert.ToInt16(x) == SelectedFreightDetails.FreightCodeDetails.ID);
                        if (data != null && Quote != null)
                        {
                            Quote.QuoteCourierName = "Willmax";
                            CourierNameEnabled = false;
                        }
                        else
                        {                       
                            if(Quote != null && Quote.QuoteCourierName.Equals("Willmax"))
                            {
                                Quote.QuoteCourierName = string.Empty;
                            }                        
                        }
                    }
                }              
            }
        }

        public bool IsCopyPhone
        {
            get
            {
                return _isCopyPhone;
            }
            set
            {
                _isCopyPhone = value;
                RaisePropertyChanged("IsCopyPhone");
                if (IsCopyPhone)
                {    
                    if (Quote != null && Quote.ContactPerson != null && Quote.ContactPerson.ContactPersonName.Equals("Other"))
                    {
                        OtherContactNamePhone1 = SelectedCustomer.CompanyTelephone;
                    }
                    else if (!string.IsNullOrWhiteSpace(Quote.ContactPerson.ContactPersonName))
                    {
                        if (Quote != null && Quote.ContactPerson != null)
                        {                            
                            prevContactPerson.ContactPersonID = Quote.ContactPerson.ContactPersonID;
                            prevContactPerson.PhoneNumber1 = Quote.ContactPerson.PhoneNumber1;
                        }
                        Quote.ContactPerson.PhoneNumber1 = SelectedCustomer.CompanyTelephone;
                    }
                }
                else
                {
                    OtherContactNamePhone1 = string.Empty;

                    if (((quote != null && quote.Customer != null) || (Quote != null)) && prevContactPerson != null && !string.IsNullOrWhiteSpace(prevContactPerson.PhoneNumber1) && Quote.ContactPerson != null)
                    {
                        var data = SelectedCustomer.ContactPerson.SingleOrDefault(x => x.ContactPersonID == prevContactPerson.ContactPersonID);
                        if (data != null)
                        {
                            data.PhoneNumber1 = prevContactPerson.PhoneNumber1;

                            if (Quote.ContactPerson.ContactPersonID == prevContactPerson.ContactPersonID && !string.IsNullOrWhiteSpace(prevContactPerson.PhoneNumber1))
                            {
                                Quote.ContactPerson.PhoneNumber1 = prevContactPerson.PhoneNumber1;
                            }                                                        
                        }
                        prevContactPerson.PhoneNumber1 = null;
                    }
                    //else if (Quote != null && Quote.ContactPerson != null && prevContactPerson != null && !string.IsNullOrWhiteSpace(prevContactPerson.PhoneNumber1))
                    //{
                    //    var data = SelectedCustomer.ContactPerson.SingleOrDefault(x => x.ContactPersonID == prevContactPerson.ContactPersonID);
                    //    if (data != null)
                    //    {
                    //        data.PhoneNumber1 = prevContactPerson.PhoneNumber1;
                    //    }
                    //    prevContactPerson.PhoneNumber1 = null;
                    //}        
                }
            }
        }

        public bool IsCopyEmail
        {
            get
            {
                return _isCopyEmail;
            }
            set
            {
                _isCopyEmail = value;
                RaisePropertyChanged("IsCopyEmail");
                if (IsCopyEmail)
                {
                    if (Quote != null && Quote.ContactPerson != null && Quote.ContactPerson.ContactPersonName.Equals("Other"))
                    {
                        OtherContactNameEmail = SelectedCustomer.CompanyEmail;
                    }
                    else if (!string.IsNullOrWhiteSpace(Quote.ContactPerson.ContactPersonName))
                    {
                        if (Quote != null && Quote.ContactPerson != null)
                        {                           
                            prevContactPerson.ContactPersonID = Quote.ContactPerson.ContactPersonID;
                            prevContactPerson.Email = Quote.ContactPerson.Email;
                        }
                        Quote.ContactPerson.Email = SelectedCustomer.CompanyEmail;
                    }
                }
                else
                {
                    OtherContactNameEmail = string.Empty;

                    if (((quote != null && quote.Customer != null) || (Quote != null)) && prevContactPerson != null && !string.IsNullOrWhiteSpace(prevContactPerson.Email) && Quote.ContactPerson != null)
                    {
                        var data = SelectedCustomer.ContactPerson.SingleOrDefault(x => x.ContactPersonID == prevContactPerson.ContactPersonID);
                        if (data != null)
                        {
                            data.Email = prevContactPerson.Email;

                            if (Quote.ContactPerson.ContactPersonID == prevContactPerson.ContactPersonID && !string.IsNullOrWhiteSpace(prevContactPerson.Email))
                            {
                                Quote.ContactPerson.Email = prevContactPerson.Email;
                            }
                        }
                        prevContactPerson.Email = null;
                    }                   
                }
            }
        }

        public bool TickCopyEmailEnabled
        {
            get
            {
                return _tickCopyEmailEnabled;
            }
            set
            {
                _tickCopyEmailEnabled = value;
                RaisePropertyChanged("TickCopyEmailEnabled");
            }
        }

        public bool CourierNameEnabled
        {
            get
            {
                return _courierNameEnabled;
            }
            set
            {
                _courierNameEnabled = value;
                RaisePropertyChanged("CourierNameEnabled");
            }
        }

        public string QuoteNoVisibility
        {
            get
            {
                return _quoteNoVisibility;
            }
            set
            {
                _quoteNoVisibility = value;
                RaisePropertyChanged("QuoteNoVisibility");
            }
        }
        

        public bool TickCopyPhoneEnabled
        {
            get
            {
                return _tickCopyPhoneEnabled;
            }
            set
            {
                _tickCopyPhoneEnabled = value;
                RaisePropertyChanged("TickCopyPhoneEnabled");
            }
        }

        private ICommand _qtyChangedCommand;
        public ICommand QtyChangedCommand
        {
            get
            {
                return _qtyChangedCommand ?? (_qtyChangedCommand = new CommandHandler(() => CalculateFinalTotal(), true));
            }
        }

        public ICommand SelectionChangedCommand
        {
            get
            {
                if (_selectionChangedCommand == null)
                {
                    addDiscountToPrice = false;
                    _selectionChangedCommand = new DelegateCommand(CanExecute, ProductChanged);
                }
                return _selectionChangedCommand;
            }
        }


        public ICommand RemoveCommand
        {
            get
            {
                if (_removeCommand == null)
                {
                    _removeCommand = new DelegateCommand(CanExecute, RemoveProduct);
                }
                return _removeCommand;
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
        //public ICommand LostFocusCommand
        //{
        //    get
        //    {
        //        return _lostFocusCommand ?? (_lostFocusCommand = new CommandHandler(() => CalculateQtyToMake(), true));
        //    }
        //}


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

        //public ICommand StockedValueLostFocusCommand
        //{
        //    get
        //    {
        //        if (_stockedValueLostFocusCommand == null)
        //        {
        //            _stockedValueLostFocusCommand = new DelegateCommand(CanExecute, ProcessFreight);
        //        }
        //        return _stockedValueLostFocusCommand;
        //    }
        //}

        //public ICommand StockedValueLostFocusCommand
        //{
        //    get
        //    {
        //        return _stockedValueLostFocusCommand ?? (_stockedValueLostFocusCommand = new CommandHandler(() => ProcessFreight(), true));
        //    }
        //}


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




        //public ICommand DiscountLostFocusCommand
        //{
        //    get
        //    {
        //        return _discountLostFocusCommand ?? (_discountLostFocusCommand = new CommandHandler(() => CalculateFinalTotal(), true));
        //    }
        //}



        public ICommand ClearCommand
        {
            get
            {
                return _clearCommand ?? (_clearCommand = new CommandHandler(() => ClearFields(), true));
            }
        }

        public ICommand SubmitCommand
        {
            get
            {
                return _submitCommand ?? (_submitCommand = new CommandHandler(() => SubmitQuote(), true));
            }
        }

        //public ICommand RemoveFreightCodeCommand
        //{
        //    get
        //    {
        //        if (_removeFreightCodeCommand == null)
        //        {
        //            _removeFreightCodeCommand = new DelegateCommand(CanExecute, DeleteFreightCode);
        //        }
        //        return _removeFreightCodeCommand;
        //    }
        //}

        public ICommand FreightPriceKeyUpCommand
        {
            get
            {
                return _freightPriceKeyUpCommand ?? (_freightPriceKeyUpCommand = new CommandHandler(() => CalculateFreightTotal(), true));
            }
        }

        //public ICommand CustomerChangedCommand
        //{
        //    get
        //    {
        //        return _customerChangedCommand ?? (_customerChangedCommand = new CommandHandler(() => CustomerChanged(), true));
        //    }
        //}

        public ICommand AddUpdateDiscountCommand
        {
            get
            {
                return _addUpdateDiscountCommand ?? (_addUpdateDiscountCommand = new CommandHandler(() => AddUpdateDiscount(), true));
            }
        }

        public ICommand GSTCheckedCommand
        {
            get
            {
                return _gSTCheckedCommand ?? (_gSTCheckedCommand = new CommandHandler(() => GSTChecked(), true));
            }
        }

        private void GSTChecked()
        {
            CalculateFinalTotal();
        }


        public ICommand ContactPersonChangedCommand
        {
            get
            {
                return _contactPersonChangedCommand ?? (_contactPersonChangedCommand = new CommandHandler(() => ContactPersonChanged(), true));
            }
        }

        private ICommand _freightCodeChangedCommand;
        public ICommand FreightCodeChangedCommand
        {
            get
            {
                return _freightCodeChangedCommand ?? (_freightCodeChangedCommand = new CommandHandler(() => FreightCodeChanged(), true));
            }
        }



    }
}
