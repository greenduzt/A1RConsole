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
using A1RConsole.ViewModels.Quoting;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace A1RConsole.ViewModels.Orders.NewOrderPDF
{
    public interface ICloseable
    {
        void Close();
    }
    public class NewOrderPDFViewModel : ViewModelBase, IContent
    {
        public ObservableCollection<Product> Product { get; set; }
        private List<DiscountStructure> _discountStructure;
        private NewOrderPDFM _newOrderPDFM;
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
        private string _phoneCopyVisibility;
        private Quote quote;
        //public LoadAllCustomersNotifier loadAllCustomersNotifier { get; set; }
        private string originalCustomer;
        private bool _gstEnabled;
        private bool _contactPersonEnabled;
        private bool _addShippingAddToProfileEnabled;        
        private bool _general;
        private bool _express;
        private bool _a1RubberOrderTruck;
        private bool _customerOrderTruck;
        private bool _customerToChargeFreight;
        private bool _customerNotToChargeFreight;
        private bool _freightAndContactDetailsEnabled;
        private bool _addCustomerToDatabase;
        private bool _isPrimaryBusinessEnabled;
        private bool _isForkliftAvailable;
        private bool _isTailgate;
        private string _popUpCloseVisibility;
        private string _mainCloseVisibility;
        private string _addCustomerVisibility;
        private List<Category> _categories;
        private Category _selectedCategory;
        private string _addUpdateBackground;
        private bool _addUpdateActive;
        private bool addDiscountToPrice;
        private bool _flexibleChecked;
        private bool _specificChecked;
        private bool _isOther;
        private bool _isCopyPhone;
        private bool _isCopyEmail;
        private string _tickAddThisCusLabel;
        private string _unloadTypeOther;
        private string _otherCommandPara;
        private static ContactPerson prevContactPerson;
        private bool _otherTextBoxEnabled;
        private bool _addCompanyAddressToShipping;
        private bool _addCcompanyAddToShippingEnabled;
        private bool _tickCopyPhoneEnabled;
        private bool _tickCopyEmailEnabled;
        private bool _courierNameEnabled;
        private List<Tuple<int, int>> reducedDiscount;
        ViewUpdateQuoteViewModel viewUpdateQuoteViewModelReference;
        private string[] freightCodesArr;
        public event Action<int> Closed;
        private DateTime _startDate;

        private ICommand _removeCommand;
        private ICommand _selectionChangedCommand;
        private ICommand _searchProductCommand;
        private ICommand _discountLostFocusCommand;
        private ICommand _clearCommand;
        private ICommand _submitCommand;
        private ICommand _customerChangedCommand;
        private ICommand _priceLostFocusCommand;
        private ICommand _addUpdateDiscountCommand;
        private ICommand _gSTCheckedCommand;
        private ICommand _contactPersonChangedCommand;
    
        public RelayCommand CloseCommand { get; private set; }

        public static NewOrderPDFViewModel instance;

        int qNo;

        public NewOrderPDFViewModel(int quoteNo,int orderNo)
        {

            LoaddAllData(quoteNo, orderNo);

            this.CloseCommand = new RelayCommand(CloseWindow);
        }

        private void LoaddAllData(int quoteNo, int orderNo)
        {
            //this.loadAllCustomersNotifier = new LoadAllCustomersNotifier();
            //this.loadAllCustomersNotifier.NewMessage += new EventHandler<SqlNotificationEventArgs>(notifier_NewMessage);
            //LoadCustomerList(loadAllCustomersNotifier.RegisterDependency());

            LoadCustomerList();
            var freightCodes = UserData.MetaData.SingleOrDefault(x => x.KeyName == "courier_name_CNA");
            freightCodesArr = freightCodes.Description.Split('|');

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

            Product = new ObservableCollection<Product>();

            NewOrderPDFM = new NewOrderPDFM();
            NewOrderPDFM.QuoteDetails = new ObservableCollection<QuoteDetails>();
            NewOrderPDFM.Customer = new Customer();
            NewOrderPDFM.Customer.DiscountStructure = new ObservableCollection<DiscountStructure>();
            NewOrderPDFM.FreightDetails = new BindingList<FreightDetails>();
            NewOrderPDFM.FreightCarrier = new FreightCarrier();
            NewOrderPDFM.QuoteDate = DateTime.Now;
            NewOrderPDFM.User = new User() { ID = UserData.UserID, FullName = UserData.FirstName + " " + UserData.LastName };
            NewOrderPDFM.LastUpdatedBy = new User() { ID = UserData.UserID, FullName = UserData.FirstName + " " + UserData.LastName };
            NewOrderPDFM.LastUpdatedDate = DateTime.Now;
            NewOrderPDFM.GSTActive = true;
            NewOrderPDFM.QuoteNoStr = string.Empty;
            NewOrderPDFM.RequiredDate = null;
            NewOrderPDFM.CourierName = string.Empty;
            prevContactPerson = new ContactPerson();

            DiscountStructure = new List<DiscountStructure>();

            FreightCodeDetails = new ObservableCollection<FreightCode>();
            SelectedFreightDetails = new FreightDetails();
            SelectedFreightDetails.FreightCodeDetails = new FreightCode();

            Categories = new List<Category>();
            Categories = DBAccess.GetCategories();
            Categories.Add(new Category() { CategoryID = 0, CategoryName = "Select" });
            SelectedCategory = new Category() { CategoryID = 0, CategoryName = "Select" };
            SelectedShipTo = "Customer Address";
            PrepaidCustomerName = "Select";
            AddUpdateBackground = "#FFDEDEDE";
            AddUpdateActive = false;
            TickAddThisCusLabel = "Collapsed";
            DiscountAppliedTextVisibility = "Collapsed";
            OtherContactPersonVisibility = "Collapsed";
            ContactPersonPhoneVisibility = "Collapsed";
            AddCustomerVisibility = "Collapsed";
            PhoneCopyVisibility = "Collapsed";
            TickCopyPhoneEnabled = false;
            TickCopyEmailEnabled = false;
            AddCustomerToDatabase = false;

            ShipToEnabled = false;
            AddShippingAddToProfileEnabled = false;
            AddShippingAddress = false;
            AddCompanyAddressToShipping = false;
            AddCcompanyAddToShippingEnabled = false;
            CustomerTypeEnabled = false;
            IsPrimaryBusinessEnabled = false;
            addDiscountToPrice = true;
            GstEnabled = false;
            IsForkliftAvailable = false;
            IsTailgate = false;
            IsOther = false;
            OtherTextBoxEnabled = false;
            LoadFreights();
            LoadProducts();
            LoadFreightCodes();
            qNo = 0;
            StartDate = DateTime.Now;
            originalCustomer = string.Empty;
            ContactPersonEnabled = false;
            General = false;
            Express = false;
            //IsCopyPhone = false;
            //IsCopyEmail = false;

            A1RubberOrderTruck = false;
            CustomerOrderTruck = false;

            CustomerToChargeFreight = false;
            CustomerNotToChargeFreight = false;

            if (quoteNo > 0 || orderNo > 0)
            {
                if (quoteNo > 0 && orderNo == 0)
                {
                    quote = DBAccess.GetQuote(quoteNo);
                    qNo = quote.QuoteNo;
                    NewOrderPDFM.CourierName = quote.QuoteCourierName;
                }
                else
                {
                    NewOrderPDFM temp = new NewOrderPDFM();
                    temp = DBAccess.GetNewOrderPDF(orderNo);
                    quote = temp;

                    NewOrderPDFM.ID = temp.ID;
                    NewOrderPDFM.SiteContactName = temp.SiteContactName;
                    NewOrderPDFM.SiteContactPhone = temp.SiteContactPhone;
                    NewOrderPDFM.RequiredDate = temp.RequiredDate;
                    NewOrderPDFM.CourierName = temp.CourierName;

                    if (temp.UnloadType == "ForkliftAvailable")
                    {
                        IsForkliftAvailable = true;
                        IsTailgate = false;
                        IsOther = false;
                    }
                    else if (temp.UnloadType == "Tailgate")
                    {
                        IsForkliftAvailable = false;
                        IsTailgate = true;
                        IsOther = false;
                    }
                    else
                    {
                        IsForkliftAvailable = false;
                        IsTailgate = false;

                        if (string.IsNullOrWhiteSpace(temp.UnloadType))
                        {
                            IsOther = false;
                            OtherTextBoxEnabled = false;
                        }
                        else
                        {
                            IsOther = true;
                            UnloadTypeOther = temp.UnloadType;
                            OtherTextBoxEnabled = true;
                        }
                    }

                    if (temp.DateTypeRequired == "Flexible")
                    {
                        FlexibleChecked = true;
                        SpecificChecked = false;
                    }
                    else
                    {
                        SpecificChecked = true;
                        FlexibleChecked = false;
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

                    if (temp.OrderTruck == "Customer")
                    {
                        A1RubberOrderTruck = false;
                        CustomerOrderTruck = true;
                    }
                    else if (temp.OrderTruck == "A1 Rubber")
                    {
                        A1RubberOrderTruck = true;
                        CustomerOrderTruck = false;
                    }
                    else
                    {
                        A1RubberOrderTruck = false;
                        CustomerOrderTruck = false;
                    }

                    if (temp.CustomerToChargedFreight == "True")
                    {
                        CustomerToChargeFreight = true;
                        CustomerNotToChargeFreight = false;
                    }
                    else if (temp.CustomerToChargedFreight == "False")
                    {
                        CustomerToChargeFreight = false;
                        CustomerNotToChargeFreight = true;
                    }
                    else
                    {
                        CustomerToChargeFreight = false;
                        CustomerNotToChargeFreight = false;
                    }

                    if (temp.FreightType == "General")
                    {
                        General = true;
                        Express = false;
                    }
                    else if (temp.FreightType == "Express")
                    {
                        General = false;
                        Express = true;
                    }
                    else
                    {
                        General = false;
                        Express = false;
                    }
                }

                SelectedCustomer = quote.Customer;
                if (SelectedCustomer != null && SelectedCustomer.CompanyName != "Select")
                {
                    AddUpdateActive = true;
                    AddUpdateBackground = "#666666";
                }

                PrepaidCustomerName = quote.Customer.CompanyName;
                originalCustomer = quote.Customer.CompanyName;
                CustomerType = quote.Customer.CustomerType;

                NewOrderPDFM.QuoteNoStr = quote.QuoteNo == 0 ? "" : quote.QuoteNo.ToString();
                NewOrderPDFM.User.FullName = UserData.FirstName + " " + UserData.LastName;
                NewOrderPDFM.ProjectName = quote.ProjectName;

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
                        //System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        //{
                        int maxDiscount = 0;
                        if (item.Product.Category.CategoryID == 11)
                        {
                            maxDiscount = item.Discount;
                        }

                        NewOrderPDFM.QuoteDetails.Add(new QuoteDetails()
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
                            IsDiscountEnabled = isNoDisExists ? false : true
                        });
                        //});
                    }
                }

                for (int i = 0; i < NewOrderPDFM.QuoteDetails.Count; i++)
                {
                    var data = quote.QuoteDetails.SingleOrDefault(x => x.ID == NewOrderPDFM.QuoteDetails[i].ID);

                    NewOrderPDFM.QuoteDetails[i].QuoteProductDescription = data != null ? data.QuoteProductDescription : NewOrderPDFM.QuoteDetails[i].QuoteProductDescription;

                    CalculateRowTotal(NewOrderPDFM.QuoteDetails[i]);
                }

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

                    if (quote.FreightDetails[0].FreightCodeDetails.ID != 50)
                    {
                        SelectedFreightDetails.FreightCodeDetails.Price = quote.FreightDetails[0].FreightCodeDetails.Price;
                    }
                    else
                    {
                        fData.Price = quote.FreightDetails[0].FreightCodeDetails.Price;
                    }
                }

                NewOrderPDFM.Instructions = quote.Instructions;
                NewOrderPDFM.InternalComments = quote.Instructions;
                NewOrderPDFM.GSTActive = quote.GSTActive;

                if (quote.Customer.CustomerId > 0)
                {
                    DiscountStructure = DBAccess.GetDiscount(quote.Customer.CustomerId);
                    DisplayDiscountStructure = new ObservableCollection<DiscountStructure>(DiscountStructure);
                    for (int i = DisplayDiscountStructure.Count - 1; i >= 0; i--)
                    {
                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            if (DisplayDiscountStructure[i].Discount == 0)
                                DisplayDiscountStructure.RemoveAt(i);
                        });
                    }
                }

                if (quote.Customer.ShipAddress == "Collect from A1 Rubber QLD" || quote.Customer.ShipAddress == "Collect from A1 Rubber NSW")
                {
                    FreightAndContactDetailsEnabled = false;
                    CourierNameEnabled = false;
                    OtherTextBoxEnabled = false;
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
                    SelectedShipTo = "Customer Address";
                    ShippingAddressVisibility = "Visible";
                    ShipToText = string.Empty;
                    FreightAndContactDetailsEnabled = true;
                    CourierNameEnabled = true;
                }

                CustomerTypeEnabled = true;
                ProjectNameEnabled = true;
                SoldToEnabled = true;
                ShipToEnabled = true;
                QuoteDetailsEnabled = true;
                FreightDetailsEnabled = true;
                InstructionsEnabled = true;
                InternalCommentsEnabled = true;
                GstEnabled = true;
                CalculateFinalTotal();
                PopUpCloseVisibility = "Visible";
                MainCloseVisibility = "Collapsed";
                addDiscountToPrice = false;

                NewOrderPDFM.ContactPerson = quote.ContactPerson;
                var conCloned = quote.ContactPerson.Clone();
                prevContactPerson = (ContactPerson)conCloned;

                if (quote.ContactPerson.CustomerID == 0)
                {
                    if (quote.ContactPerson.ContactPersonName != null && quote.ContactPerson.ContactPersonName != "Other")
                    {
                        SelectedCustomer.ContactPerson.Add(quote.ContactPerson);
                        NewOrderPDFM.ContactPerson.ContactPersonName = quote.ContactPerson.ContactPersonName;
                    }
                    else
                    {
                        NewOrderPDFM.ContactPerson = new ContactPerson() { ContactPersonID = -1, ContactPersonName = "Other", CustomerID = SelectedCustomer.CustomerId };
                    }
                }

                FreightCodeChanged();
            }
            else
            {
                quote = null;
                //CustomerTypeEnabled = false;
                ProjectNameEnabled = false;
                FreightAndContactDetailsEnabled = false;
                CourierNameEnabled = false;
                SoldToEnabled = false;
                ShipToEnabled = false;
                AddShippingAddToProfileEnabled = false;
                QuoteDetailsEnabled = false;
                FreightDetailsEnabled = false;
                InstructionsEnabled = false;
                InternalCommentsEnabled = false;
                GstEnabled = false;
                PopUpCloseVisibility = "Collapsed";
                MainCloseVisibility = "Visible";
                //this.MainCloseCommand = new RelayCommand(CloseWindow);
            }

            NewOrderPDFM.QuoteDetails.CollectionChanged += productChanged;
            this.NewOrderPDFM.QuoteDetails = SequencingService.SetCollectionSequence(this.NewOrderPDFM.QuoteDetails);


            instance = this;
        }


        private void FixDiscount()
        {
            foreach (var item in NewOrderPDFM.QuoteDetails)
            {
                ProductChanged(item);
            }
        }

        private void ProcessFreight()
        {
            if (NewOrderPDFM != null && NewOrderPDFM.QuoteDetails != null)
            {
                bool has = false;

                if (NewOrderPDFM != null && NewOrderPDFM.QuoteDetails != null && SelectedFreightDetails != null)
                {
                    if (quote != null && quote.FreightDetails != null)
                    {
                        //SelectedFreightDetails = new FreightDetails();
                        //SelectedFreightDetails.FreightCodeDetails = new FreightCode();
                        //SelectedFreightDetails.FreightCodeDetails.ID = quote.FreightDetails[0].FreightCodeDetails.ID;
                        //SelectedFreightDetails.FreightCodeDetails.Code = quote.FreightDetails[0].FreightCodeDetails.Code;
                        //SelectedFreightDetails.FreightCodeDetails.Description = quote.FreightDetails[0].FreightCodeDetails.Description;
                        //SelectedFreightDetails.FreightCodeDetails.Unit = quote.FreightDetails[0].FreightCodeDetails.Unit;
                        //SelectedFreightDetails.FreightCodeDetails.Price = quote.FreightDetails[0].FreightCodeDetails.Price;
                        //SelectedFreightDetails.PalletsStr = quote.FreightDetails[0].PalletsStr;
                        //SelectedFreightDetails.Discount = quote.FreightDetails[0].Discount;


                        //Console.WriteLine(quote.FreightDetails[0].FreightCodeDetails.ID);
                        //Console.WriteLine(quote.FreightDetails[0].FreightCodeDetails.Code);
                        //Console.WriteLine(quote.FreightDetails[0].FreightCodeDetails.Description);
                        //Console.WriteLine(quote.FreightDetails[0].FreightCodeDetails.Unit);
                        //Console.WriteLine(quote.FreightDetails[0].FreightCodeDetails.Price);
                        //Console.WriteLine(quote.FreightDetails[0].PalletsStr);
                        //Console.WriteLine(quote.FreightDetails[0].Discount);

                        if (SelectedShipTo == "A1 Rubber NSW")
                        {
                            has = NewOrderPDFM.QuoteDetails.Any(x => x.Product.LocationType == "QLD");
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
                            NewOrderPDFM.QuoteCourierName = string.Empty;
                            CourierNameEnabled = false;
                        }

                    }
                    else if (SelectedShipTo == "Customer Address")
                    {
                        SelectedFreightDetails.FreightCodeDetails = new FreightCode() { Code = "Select", Unit = "", Price = 0 };
                        NewOrderPDFM.QuoteCourierName = string.Empty;
                        //CourierNameEnabled = true;                        
                    }
                    else
                    {
                        if (SelectedShipTo == "A1 Rubber NSW")
                        {
                            has = NewOrderPDFM.QuoteDetails.Any(x => x.Product.LocationType == "QLD");
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
                            NewOrderPDFM.QuoteCourierName = string.Empty;
                            CourierNameEnabled = false;
                        }
                    }
                }
            }    
        }      

        //void notifier_NewMessage(object sender, SqlNotificationEventArgs e)
        //{
        //    LoadCustomerList(this.loadAllCustomersNotifier.RegisterDependency());
        //}

        private void CloseForm()
        {
            if (Closed != null)
            {
                Closed(1);
            }
            ClearFields();
        }


        private void LoadCustomerList()
        {
            CustomerList = DBAccess.GetAllActiveCustomers();

            //if (pSList != null)
            //{
            //    if (SelectedCustomer != null)
            //    {
            //        //Get previous details
            //        var previouslySelectedItem = SelectedCustomer;
            //        List<QuoteDetails> tempQuoteDetails = new List<QuoteDetails>();
            //        string tempProjname = NewOrderPDFM.ProjectName;
            //        ContactPerson tempContactPerson = new ContactPerson();
            //        string tempSelectedShipTo = SelectedShipTo;
            //        string tempShippingAddress = ShippingAddress;
            //        string tempShippingCity = ShippingCity;
            //        string tempShippingState = ShippingState;
            //        string tempShippingPostCode = ShippingPostCode;
            //        string tempShippingCountry = ShippingCountry;
            //        FreightDetails tempFreightDetails = new FreightDetails();
            //        string tempInternalComments = NewOrderPDFM.InternalComments;

            //        if (NewOrderPDFM.QuoteDetails != null)
            //        {
            //            tempQuoteDetails = new List<QuoteDetails>(NewOrderPDFM.QuoteDetails);
            //            NewOrderPDFM.QuoteDetails = new ObservableCollection<QuoteDetails>();
            //        }

            //        if (NewOrderPDFM.ContactPerson != null)
            //        {
            //            tempContactPerson = NewOrderPDFM.ContactPerson;
            //        }

            //        if (SelectedFreightDetails != null)
            //        {
            //            tempFreightDetails = SelectedFreightDetails;
            //            tempFreightDetails.PalletsStr = SelectedFreightDetails.PalletsStr;
            //            tempFreightDetails.Discount = SelectedFreightDetails.Discount;
            //        }

            //        if (CustomerList.Count > 0)
            //        {
            //            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            //            {
            //                CustomerList.Clear();
            //            });
            //        }

            //        CustomerList = pSList;

            //        var data = CustomerList.SingleOrDefault(x => x.CompanyName.Equals(previouslySelectedItem.CompanyName));
            //        if (data != null)
            //        {
            //            SelectedCustomer = data;
            //            SelectedCustomer.ContactPerson = data.ContactPerson;
            //            NewOrderPDFM.ProjectName = tempProjname;
            //            NewOrderPDFM.ContactPerson = tempContactPerson;
            //            NewOrderPDFM.ContactPerson.ContactPersonName = tempContactPerson.ContactPersonName;
            //            SelectedShipTo = tempSelectedShipTo;
            //            ShippingAddress = tempShippingAddress;
            //            ShippingCity = tempShippingCity;
            //            ShippingState = tempShippingState;
            //            ShippingPostCode = tempShippingPostCode;
            //            ShippingCountry = tempShippingCountry;
            //            SelectedFreightDetails.FreightCodeDetails = tempFreightDetails.FreightCodeDetails;
            //            SelectedFreightDetails.PalletsStr = tempFreightDetails.PalletsStr;
            //            SelectedFreightDetails.Discount = tempFreightDetails.Discount;
            //            //NewOrderPDFM.Instructions = tempInstructions;
            //            NewOrderPDFM.InternalComments = tempInternalComments;


            //            foreach (var item in tempQuoteDetails)
            //            {
            //                System.Windows.Application.Current.Dispatcher.Invoke(() =>
            //                {
            //                    NewOrderPDFM.QuoteDetails.Add(new QuoteDetails()
            //                    {
            //                        BlocksLogsToMake = item.BlocksLogsToMake,
            //                        CustomProductVisible = item.CustomProductVisible,
            //                        InStockCellBack = item.InStockCellBack,
            //                        InStockCellFore = item.InStockCellFore,
            //                        IsEditing = item.IsEditing,
            //                        LineStatus = item.LineStatus,
            //                        OrderLine = item.OrderLine,
            //                        Product = new Product()
            //                        {
            //                            ProductID = item.Product.ProductID,
            //                            Category = new Models.Categories.Category() { CategoryID = item.Product.Category.CategoryID },
            //                            ProductCode = item.Product.ProductCode,
            //                            ProductDescription = item.Product.ProductDescription,
            //                            ProductUnit = item.Product.ProductUnit,
            //                            UnitPrice = item.QuoteUnitPrice,
            //                            Type = item.Product.Type,
            //                            Size = item.Product.Size,
            //                            LocationType = item.Product.LocationType
            //                        },
            //                        Quantity = item.Quantity,
            //                        QuantityStr = item.QuantityStr,
            //                        TimeStamp = data.TimeStamp,
            //                        QuoteUnitPrice = item.QuoteUnitPrice,
            //                        QuoteProductDescription = item.QuoteProductDescription,
            //                        Discount = item.Discount,
            //                        DiscountedTotal = item.DiscountedTotal,
            //                        Total = item.Total
            //                    });
            //                });
            //            }
            //        }
            //    }
            //    else
            //    {
            //        CustomerList = pSList;
            //    }

            //}
        }

        private void LoadProducts()
        {
            Product = DBAccess.GetAllProds(false);

        }

        private void ProductChanged(object parameter)
        {
            int index = NewOrderPDFM.QuoteDetails.IndexOf(parameter as QuoteDetails);
            if (index > -1 && index < NewOrderPDFM.QuoteDetails.Count)
            {
                if (NewOrderPDFM.QuoteDetails[index].Product != null)
                {
                    string totalKg = string.Empty;
                    if (NewOrderPDFM.QuoteDetails[index].Product.ProductID != 56 && NewOrderPDFM.QuoteDetails[index].Product.Type == "Bag")
                    {
                        int tot = Convert.ToInt16(NewOrderPDFM.QuoteDetails[index].QuantityStr) * Convert.ToInt16(NewOrderPDFM.QuoteDetails[index].Product.Size);
                        totalKg = tot > 0 ? " [Total " + tot + "Kg]" : string.Empty;
                    }

                    NewOrderPDFM.QuoteDetails[index].IsDiscountEnabled = true;
                    NewOrderPDFM.QuoteDetails[index].QuoteProductDescription = NewOrderPDFM.QuoteDetails[index].Product.ProductDescription + totalKg;
                    NewOrderPDFM.QuoteDetails[index].QuoteUnitPrice = NewOrderPDFM.QuoteDetails[index].Product.UnitPrice;

                    //Apply non discount products
                    var noDiscountProducts = UserData.MetaData.SingleOrDefault(x => x.KeyName == "no_discount_products");
                    string[] noDiscountProductsArr = noDiscountProducts.Description.Split('|');

                    bool isNoDisExists = noDiscountProductsArr.Any(x => Convert.ToInt16(x) == NewOrderPDFM.QuoteDetails[index].Product.ProductID);

                    if (addDiscountToPrice || (isNoDisExists == false && (quote == null || quote.QuoteNo == 0 && NewOrderPDFM.ID == 0)) || (isNoDisExists == false && quote != null && AddCustomerToDatabase))
                    {
                        if (DiscountStructure != null && DiscountStructure.Count > 0)
                        {
                            var data = DiscountStructure.SingleOrDefault(x => x.Category.CategoryID == NewOrderPDFM.QuoteDetails[index].Product.Category.CategoryID);
                            if (data != null)
                            {
                                if (data.Category.CategoryID == 11)
                                {
                                    if (data.Discount > 0)
                                    {
                                        NewOrderPDFM.QuoteDetails[index].MaxDiscount = data.Discount;
                                    }
                                    else
                                    {
                                        var data2 = Categories.SingleOrDefault(x => x.CategoryID == 11);
                                        if (data2 != null)
                                        {
                                            NewOrderPDFM.QuoteDetails[index].MaxDiscount = data2.Discounts.Max();
                                        }
                                    }
                                }

                                var prodExistsData = reducedDiscount.SingleOrDefault(x => x.Item1 == NewOrderPDFM.QuoteDetails[index].Product.ProductID);
                                if (prodExistsData != null)
                                {
                                    NewOrderPDFM.QuoteDetails[index].Discount = data.Discount > prodExistsData.Item2 ? prodExistsData.Item2 : data.Discount;
                                }
                                else
                                {
                                    NewOrderPDFM.QuoteDetails[index].Discount =  data.Discount;
                                }                              
                            }
                            else
                            {
                                if (NewOrderPDFM.QuoteDetails[index].Product.Category.CategoryID == 11)
                                {
                                    var data2 = Categories.SingleOrDefault(x => x.CategoryID == 11);
                                    if (data2 != null)
                                    {
                                        NewOrderPDFM.QuoteDetails[index].MaxDiscount = data2.Discounts.Max();
                                    }
                                }
                                else
                                {
                                    NewOrderPDFM.QuoteDetails[index].Discount = 0;
                                }
                            }
                        }
                        else
                        {
                            if (Categories != null && Categories.Count > 0)
                            {
                                var data = Categories.SingleOrDefault(x => x.CategoryID == 11);
                                if (data != null)
                                {
                                    NewOrderPDFM.QuoteDetails[index].MaxDiscount = data.Discounts.Max();
                                }
                            }

                            NewOrderPDFM.QuoteDetails[index].Discount = 0;
                        }
                    }
                    else if (addDiscountToPrice == false && isNoDisExists == false && quote != null && quote.QuoteDetails != null)
                    {
                        var data = DiscountStructure.SingleOrDefault(x => x.Category.CategoryID == NewOrderPDFM.QuoteDetails[index].Product.Category.CategoryID);
                        if (data != null)
                        {
                            if (data.Category.CategoryID == 11)
                            {
                                if (data.Discount > 0)
                                {
                                    NewOrderPDFM.QuoteDetails[index].MaxDiscount = data.Discount;
                                }
                                else
                                {
                                    var data1 = Categories.SingleOrDefault(x => x.CategoryID == 11);
                                    if (data != null)
                                    {
                                        NewOrderPDFM.QuoteDetails[index].MaxDiscount = data1.Discounts.Max();
                                    }
                                }
                            }

                            NewOrderPDFM.QuoteDetails[index].Discount = data.Discount;
                        }
                        else
                        {
                            var data1 = Categories.SingleOrDefault(x => x.CategoryID == 11);
                            if (data1 != null)
                            {
                                NewOrderPDFM.QuoteDetails[index].MaxDiscount = data1.Discounts.Max();
                            }
                        }
                    }
                    else if (isNoDisExists)
                    {
                        NewOrderPDFM.QuoteDetails[index].IsDiscountEnabled = false;
                        NewOrderPDFM.QuoteDetails[index].Discount = 0;                        
                    }
                    else
                    {
                        NewOrderPDFM.QuoteDetails[index].Discount = 0;
                    }                   
                }
                else
                {
                    NewOrderPDFM.QuoteDetails[index].QuoteProductDescription = string.Empty;
                    NewOrderPDFM.QuoteDetails[index].QuoteUnitPrice = 0;
                    NewOrderPDFM.QuoteDetails[index].Discount = 0;
                }

                ProcessFreight();
                CalculateFinalTotal();
            }            
        }

        private void CalculateQtyToMake()
        {
            CalculateFinalTotal();
        }

        private void RemoveProduct(object parameter)
        {
            int index = NewOrderPDFM.QuoteDetails.IndexOf(parameter as QuoteDetails);
            if (index > -1 && index < NewOrderPDFM.QuoteDetails.Count)
            {
                NewOrderPDFM.QuoteDetails.RemoveAt(index);
                ObservableCollection<QuoteDetails> tempColl = new ObservableCollection<QuoteDetails>();
                tempColl = NewOrderPDFM.QuoteDetails;
                NewOrderPDFM.QuoteDetails = new ObservableCollection<QuoteDetails>();
                NewOrderPDFM.QuoteDetails = tempColl;
            }
        }

        private void GetRowTotal(object parameter)
        {
            int index = NewOrderPDFM.QuoteDetails.IndexOf(parameter as QuoteDetails);
            if (index > -1 && index < NewOrderPDFM.QuoteDetails.Count)
            {
                if (NewOrderPDFM.QuoteDetails[index].Product != null)
                {
                    CalculateRowTotal(NewOrderPDFM.QuoteDetails[index]);
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
            if (NewOrderPDFM != null)
            {
                for (int i = 0; i < NewOrderPDFM.QuoteDetails.Count; i++)
                {
                    CalculateRowTotal(NewOrderPDFM.QuoteDetails[i]);
                }

                NewOrderPDFM.ListPriceTotal = NewOrderPDFM.QuoteDetails.Sum(x => x.ProductTotalBeforeDis);
                NewOrderPDFM.DiscountedTotal = NewOrderPDFM.QuoteDetails.Sum(x => x.DiscountedTotal);

                decimal noOfPallets = Convert.ToDecimal(string.IsNullOrWhiteSpace(SelectedFreightDetails.PalletsStr) == true ? 0 : Convert.ToDecimal(SelectedFreightDetails.PalletsStr));
                decimal freightPrice = SelectedFreightDetails.FreightCodeDetails == null ? 0 : SelectedFreightDetails.FreightCodeDetails.Price;

                NewOrderPDFM.FreightTotal = freightPrice * noOfPallets - (((freightPrice * noOfPallets) * SelectedFreightDetails.Discount) / 100);
                if (NewOrderPDFM.GSTActive)
                {
                    NewOrderPDFM.Gst = NewOrderPDFM.GSTActive ? Math.Round((((NewOrderPDFM.ListPriceTotal - NewOrderPDFM.DiscountedTotal) + NewOrderPDFM.FreightTotal) * 10) / 100, 2) : 0;
                }
                else
                {
                    NewOrderPDFM.Gst = 0;
                }

                NewOrderPDFM.TotalBeforeTax = NewOrderPDFM.ListPriceTotal - NewOrderPDFM.DiscountedTotal;
                NewOrderPDFM.TotalAmount = Math.Round(NewOrderPDFM.TotalBeforeTax, 2) + Math.Round(NewOrderPDFM.FreightTotal, 2) + Math.Round(NewOrderPDFM.Gst, 2);

                EnableDisableDiscountText();
            }
        }

        private void EnableDisableDiscountText()
        {
            if (DiscountStructure != null || DiscountStructure.Count > 0)
            {
                bool disApplied = false;
                if (NewOrderPDFM.QuoteDetails != null)
                {
                    //Apply non discount products
                    var noDiscountProducts = UserData.MetaData.SingleOrDefault(x => x.KeyName == "no_discount_products");
                    string[] noDiscountProductsArr = noDiscountProducts.Description.Split('|');   

                    foreach (var item in NewOrderPDFM.QuoteDetails)
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

            int index = NewOrderPDFM.QuoteDetails.IndexOf(parameter as QuoteDetails);
            if (index > -1 && index < NewOrderPDFM.QuoteDetails.Count)
            {
                SalesOrder salesOrder = new SalesOrder();
                salesOrder.SalesOrderDetails = new ObservableCollection<SalesOrderDetails>();

                foreach (var item in NewOrderPDFM.QuoteDetails)
                {
                    salesOrder.SalesOrderDetails.Add(item);
                }

                var childWindow = new ChildWindow();
                childWindow.productCodeSearch_Closed += (r =>
                {
                    if (r != null && r.ProductCode != null)
                    {
                        NewOrderPDFM.QuoteDetails[index].Product = r;
                        NewOrderPDFM.QuoteDetails[index].Product.ProductCode = r.ProductCode;
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
            this.ItemCount = this.NewOrderPDFM.QuoteDetails.Count;

            // Resequence list
            SequencingService.SetCollectionSequence(this.NewOrderPDFM.QuoteDetails);
            CalculateFinalTotal();
        }


        void freightChanged(object sender, ListChangedEventArgs e)
        {
            CalculateFinalTotal();
        }

        private void RemoveZeroQuoteDetails()
        {
            var itemToRemove = NewOrderPDFM.QuoteDetails.Where(x => x.Product == null).ToList();
            foreach (var item in itemToRemove)
            {
                NewOrderPDFM.QuoteDetails.Remove(item);
            }
        }

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

            PrepaidCustomerName = "Select";
            SelectedShipTo = "Customer Address";
            SelectedCategory = new Category() { CategoryID = 0, CategoryName = "Select" };
            IsPrimaryBusinessEnabled = false;

            CustomerType = string.Empty;
            ContactPersonEnabled = false;
            AddCustomerVisibility = "Collapsed";
            PhoneCopyVisibility = "Collapsed";
            TickCopyPhoneEnabled = false;
            TickCopyEmailEnabled = false;
         
            quote = new Quote();
            quote.QuoteDetails = new ObservableCollection<QuoteDetails>();
            NewOrderPDFM.QuoteDetails = new ObservableCollection<QuoteDetails>();
            NewOrderPDFM.QuoteDetails.CollectionChanged += productChanged;
            NewOrderPDFM.FreightDetails = new BindingList<FreightDetails>();
            NewOrderPDFM.FreightDetails.ListChanged += freightChanged;
            NewOrderPDFM.Customer = new Customer();
            NewOrderPDFM.FreightCarrier = new FreightCarrier();
            NewOrderPDFM.ContactPerson = new ContactPerson();
            NewOrderPDFM.ContactPerson.ContactPersonName = "Other";
            NewOrderPDFM.QuoteDate = DateTime.Now;
            NewOrderPDFM.SiteContactName = string.Empty;
            NewOrderPDFM.SiteContactPhone = string.Empty;
            NewOrderPDFM.QuoteNoStr = string.Empty;
            NewOrderPDFM.RequiredDate = null;
            NewOrderPDFM.ProjectName = string.Empty;
            NewOrderPDFM.RequiredDate = null;
            NewOrderPDFM.ID = 0;
            NewOrderPDFM.CourierName = string.Empty;
            NewOrderPDFM.UnloadType = string.Empty;
            NewOrderPDFM.FreightType = string.Empty;
            NewOrderPDFM.OrderTruck = string.Empty;
            NewOrderPDFM.CustomerToChargedFreight = string.Empty;
            NewOrderPDFM.Instructions = string.Empty;
            FlexibleChecked = false;
            SpecificChecked = false;
            OtherContactNameEmail = string.Empty;
            OtherContactNamePhone1 = string.Empty;
            OtherContactNamePhone2 = string.Empty;
            OtherContactName = string.Empty;
            AddCustomerToDatabase = false;           
            General = false;
            Express = false;
            A1RubberOrderTruck = false;
            CustomerOrderTruck = false;
            CustomerToChargeFreight = false;
            CustomerNotToChargeFreight = false;
            AddShippingAddress = false;
            AddCompanyAddressToShipping = false;
            AddCcompanyAddToShippingEnabled = false;
            AddCompanyAddressToShipping = false;
            GstEnabled = false;

            UnloadTypeOther = string.Empty;

            IsForkliftAvailable = false;
            IsTailgate = false;
            IsOther = false;
            OtherTextBoxEnabled = false;
            IsCopyPhone = false;
            IsCopyEmail = false;
            

            AddUpdateBackground = "#FFDEDEDE";
            AddUpdateActive = false;
            DiscountStructure.Clear();
            DiscountStructure = new List<DiscountStructure>();
            if (DisplayDiscountStructure != null)
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                          {
                              DisplayDiscountStructure.Clear();
                              DisplayDiscountStructure = new ObservableCollection<DiscountStructure>();
                          });
            }

            CustomerTypeEnabled = false;
            ProjectNameEnabled = false;
            FreightAndContactDetailsEnabled = false;
            CourierNameEnabled = false;
            SoldToEnabled = false;
            ShipToEnabled = false;
            AddShippingAddToProfileEnabled = false;
            QuoteDetailsEnabled = false;
            FreightDetailsEnabled = false;
            InstructionsEnabled = false;
            InternalCommentsEnabled = false;
            DiscountAppliedTextVisibility = "Collapsed";

            NewOrderPDFM.User = new User() { ID = UserData.UserID, FullName = UserData.FirstName + " " + UserData.LastName };
            NewOrderPDFM.LastUpdatedBy = new User() { ID = UserData.UserID, FullName = UserData.FirstName + " " + UserData.LastName };
            NewOrderPDFM.LastUpdatedDate = DateTime.Now;
            NewOrderPDFM.GSTActive = true;

            LoadFreightCodes();

            SelectedFreightDetails = new FreightDetails();
            SelectedFreightDetails.FreightCodeDetails = new FreightCode() { ID = 0, Code = "Select", Description = "", Price = 0, Unit = "" };            
        }

        public string Title
        {
            get
            {
                return "New Order PDF";
            }
        }


        public void SubmitQuote()
        {
            
            bool dupExist = false;
            bool checkPriceNotZero = false;
            RemoveZeroQuoteDetails();

            var prodExists = NewOrderPDFM.QuoteDetails.Where(x => x.Product != null && x.Quantity != 0).ToList();

            if (NewOrderPDFM.QuoteDetails != null)
            {
                //Checking duplicates
                string[] prods = null;
                prods = MetaDataManager.GetPriceEditingProducts();

                if (prods != null)
                {
                    var duplicates = NewOrderPDFM.QuoteDetails.GroupBy(s => s.Product.ProductID)
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
                checkPriceNotZero = NewOrderPDFM.QuoteDetails.Any(x => x.QuoteUnitPrice == 0);
            }

            ObservableCollection<QuoteDetails> tempQuoteDetails = new ObservableCollection<QuoteDetails>();
            foreach (var item in NewOrderPDFM.QuoteDetails)
            {
                tempQuoteDetails.Add(new QuoteDetails() { Product = new Product() { ProductID = item.Product.ProductID } });
            }


            if (string.IsNullOrWhiteSpace(PrepaidCustomerName) && SelectedCustomer.CompanyName == "Select")
            {
                MessageBox.Show("Please select customer name", "Select Customer Name", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (string.IsNullOrWhiteSpace(NewOrderPDFM.ProjectName))
            {
                MessageBox.Show("Project name required", "Enter Project Name", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (NewOrderPDFM.ContactPerson.ContactPersonName == "Other" && string.IsNullOrWhiteSpace(OtherContactName))
            {
                MessageBox.Show("Enter contact person name", "Contact Person Name Required", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (NewOrderPDFM.ContactPerson.ContactPersonName == "No Contact")
            {
                MessageBox.Show("Contact person required", "", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (NewOrderPDFM.ContactPerson.ContactPersonName == "Other" && (string.IsNullOrWhiteSpace(OtherContactNamePhone1) && string.IsNullOrWhiteSpace(OtherContactNamePhone2)))
            {
                MessageBox.Show("Phone number required", "", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (NewOrderPDFM.ContactPerson.ContactPersonName == "Other" && string.IsNullOrWhiteSpace(OtherContactNameEmail))
            {
                MessageBox.Show("E-mail address required", "", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (NewOrderPDFM.ContactPerson.ContactPersonName == "Other" && !string.IsNullOrWhiteSpace(OtherContactNameEmail) && !Regex.IsMatch(OtherContactNameEmail, @"^[a-zA-Z][\w\.-]*[a-zA-Z0-9]@[a-zA-Z0-9][\w\.-]*[a-zA-Z0-9]\.[a-zA-Z][a-zA-Z\.]*[a-zA-Z]$"))
            {
                MessageBox.Show("Valid E-mail address required", "", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (!string.IsNullOrWhiteSpace(NewOrderPDFM.ContactPerson.ContactPersonName) && NewOrderPDFM.ContactPerson.ContactPersonName != "Other" && (string.IsNullOrWhiteSpace(NewOrderPDFM.ContactPerson.PhoneNumber1) && string.IsNullOrWhiteSpace(NewOrderPDFM.ContactPerson.PhoneNumber2)))
            {
                MessageBox.Show("Phone number required", "", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (!string.IsNullOrWhiteSpace(NewOrderPDFM.ContactPerson.ContactPersonName) && NewOrderPDFM.ContactPerson.ContactPersonName != "Other" && string.IsNullOrWhiteSpace(NewOrderPDFM.ContactPerson.Email))
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
            else if (string.IsNullOrWhiteSpace(NewOrderPDFM.DateTypeRequired))
            {
                MessageBox.Show("Please select Flexible or Specific for required date", "", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (NewOrderPDFM.RequiredDate == null)
            {
                MessageBox.Show("Please select order required date", "", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (NewOrderPDFM.QuoteDetails.Count == 0)
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
            else if (SelectedShipTo == "Customer Address" && string.IsNullOrWhiteSpace(NewOrderPDFM.CourierName))
            {
                MessageBox.Show("Courier name required", "Courier Name Required", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (SelectedShipTo == "Customer Address" && string.IsNullOrWhiteSpace(NewOrderPDFM.UnloadType) && OtherTextBoxEnabled == false)
            {
                MessageBox.Show("Please tick unload type", "Unload Type Required", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (SelectedShipTo == "Customer Address" && string.IsNullOrWhiteSpace(NewOrderPDFM.UnloadType) && OtherTextBoxEnabled == true && string.IsNullOrWhiteSpace(UnloadTypeOther))
            {
                MessageBox.Show("Please enter unload type", "Unload Type Required", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (SelectedShipTo == "Customer Address" && string.IsNullOrWhiteSpace(NewOrderPDFM.FreightType))
            {
                MessageBox.Show("Please tick freight type", "Freight Type Required", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (SelectedShipTo == "Customer Address" && string.IsNullOrWhiteSpace(NewOrderPDFM.OrderTruck))
            {
                MessageBox.Show("Please tick who is ordering truck", "Ordering Truck Required", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (SelectedShipTo == "Customer Address" && string.IsNullOrWhiteSpace(NewOrderPDFM.CustomerToChargedFreight))
            {
                MessageBox.Show("Please tick if customer to be charged freight by A1 Rubber", "Customer Charge Freight Required", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                string wholeFilePath = string.Empty;
                string filePath = string.Empty;
                string fileName = string.Empty;
                Tuple<int, int, string> insQteTuple = null;
                Exception exceptionPdf = null;
                Exception exceptionEmail = null;
                bool canDirectoryAccessed = true;

                BackgroundWorker worker = new BackgroundWorker();
                ChildWindow LoadingScreen = new ChildWindow();

                LoadingScreen.ShowWaitingScreen("Working..");

                worker.DoWork += (_, __) =>
                {
                    NewOrderPDFM.QuoteNo = quote != null ? quote.QuoteNo : 0;
                    NewOrderPDFM.Customer = SelectedCustomer;
                    NewOrderPDFM.User = new User();
                    NewOrderPDFM.User.FullName = UserData.FirstName + " " + UserData.LastName;
                    NewOrderPDFM.User.ID = UserData.UserID;
                    NewOrderPDFM.LastUpdatedBy = new User() { ID = UserData.UserID, FullName = UserData.FirstName + " " + UserData.LastName };
                    NewOrderPDFM.LastUpdatedDate = DateTime.Now;
                    NewOrderPDFM.QuoteDate = DateTime.Now;

                    if (string.IsNullOrWhiteSpace(OtherContactNamePhone1) && !string.IsNullOrWhiteSpace(OtherContactNamePhone2))
                    {
                        OtherContactNamePhone1 = OtherContactNamePhone2;
                        OtherContactNamePhone2 = string.Empty;
                    }

                    NewOrderPDFM.FreightDetails = new BindingList<FreightDetails>();
                    NewOrderPDFM.FreightDetails.Add(SelectedFreightDetails);
                    if (!SelectedShipTo.Equals("Customer Address"))
                    {
                        ShippingAddress = ShipToText;
                        ShippingCity = string.Empty;
                        ShippingState = string.Empty;
                        ShippingPostCode = string.Empty;
                        ShippingCountry = string.Empty;
                    }

                    insQteTuple = DBAccess.InsertNewOrderPDF(NewOrderPDFM, OtherContactName, OtherContactNamePhone1 == null ? "" : OtherContactNamePhone1, OtherContactNamePhone2 == null ? "" : OtherContactNamePhone2,
                                                             AddShippingAddress, ShippingAddress, ShippingCity, ShippingState, ShippingPostCode, ShippingCountry, PrepaidCustomerName, OtherContactNameEmail,
                                                             CustomerType, SelectedCategory, AddCustomerToDatabase, DisplayDiscountStructure, quote);

                    filePath = FilePathManager.GetNewOrderSavingPath();

                    if (insQteTuple.Item2 == 1)
                    {
                        NewOrderPDFM.FileName = insQteTuple.Item3;
                        fileName = insQteTuple.Item3;

                        //Create directory if it does not exist
                        try
                        {
                            Directory.CreateDirectory(filePath);
                        }
                        catch (DirectoryNotFoundException dirEx)
                        {
                            canDirectoryAccessed = false;
                        }

                        if (insQteTuple.Item2 == 1 && canDirectoryAccessed)
                        {
                            NewSalesOrderPDF uqpdf = new NewSalesOrderPDF(insQteTuple.Item1, insQteTuple.Item3);
                            exceptionPdf = uqpdf.CreateQuote();

                            if (exceptionPdf == null)
                            {
                                try
                                {
                                    var emailsVar = UserData.MetaData.SingleOrDefault(x => x.KeyName == "sales_order_sending_email");
                                    string[] emails = emailsVar.Description.Split('|');

                                    //Send email with attachment
                                    MailAddress mailfrom = new MailAddress("a1rubber.orders@gmail.com");
                                    //MailAddress mailto = new MailAddress("chamara@a1rubber.com");
                                    MailMessage msg = new MailMessage();
                                    //MailMessage newmsg = new MailMessage(mailfrom, mailto);
                                    foreach (var item in emails)
                                    {
                                        msg.To.Add(item);
                                    }
                                    
                                    msg.From = mailfrom;
                                    msg.Subject = string.Format("New order from A1R Console sent {0}", DateTime.Now.ToLongDateString());
                                    msg.Body = "Hi" + System.Environment.NewLine + System.Environment.NewLine + "Please find the new order attached." + System.Environment.NewLine + System.Environment.NewLine + System.Environment.NewLine + System.Environment.NewLine + "A1R Console";

                                    string strAttPath = filePath.Replace("/", @"\\");
                                    Attachment att = new Attachment(strAttPath + "\\" + fileName);
                                    msg.Attachments.Add(att);

                                    SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587);
                                    smtp.UseDefaultCredentials = false;
                                    //smtp.Credentials = new NetworkCredential("a1rubber.orders@gmail.com", "Shad0wA1$");
                                    smtp.Credentials = new NetworkCredential("a1rubber.orders@gmail.com", "bgdobybwyownoeou");
                                    smtp.EnableSsl = true;
                                    smtp.Send(msg);
                                }
                                catch (Exception ex)
                                {
                                    exceptionEmail = ex;
                                }
                            }
                        }
                    }
                };
                worker.RunWorkerCompleted += delegate(object s, RunWorkerCompletedEventArgs args)
                {
                    LoadingScreen.CloseWaitingScreen();
                    //Refresh both grids of UpdateQuoteViewModel
                    if (ViewUpdateQuoteViewModel.instance != null)
                    {
                        viewUpdateQuoteViewModelReference = ViewUpdateQuoteViewModel.instance;
                        viewUpdateQuoteViewModelReference.RefreshGrid();
                        viewUpdateQuoteViewModelReference.RefreshConvQuotesGrid();
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
                            else
                            {
                                //LoadCustomers();
                            }

                            var emailsVar = UserData.MetaData.SingleOrDefault(x => x.KeyName == "sales_order_sending_email");
                            string[] emails = emailsVar.Description.Split('|');
                            string emailList = string.Empty;
                            if (emails != null)
                            {
                                int eLength = emails.Length;
                                string line = " | ";
                                int z = 1;

                                for (int i = 0; i < emails.Length; i++)
                                {
                                    if (z == eLength)
                                    {
                                        line = "";
                                    }
                                    else
                                    {
                                        line = " | ";
                                    }
                                    if (!string.IsNullOrWhiteSpace(emails[i]))
                                    {
                                        emailList += emails[i] + line;
                                    }
                                    z++;
                                }
                            }
                            string strLine2 = string.Empty;
                            if (!string.IsNullOrWhiteSpace(emailList))
                            {
                                strLine2 = exceptionEmail == null ? "Order was emailed to " + emailList : "";
                            }
                            MessageBox.Show("Order created successfully!" + System.Environment.NewLine + strLine2, "Order No - " + insQteTuple.Item1 + " Created", MessageBoxButton.OK, MessageBoxImage.Information);
                        }

                        if (insQteTuple.Item2 == 1)
                        {
                            ClearFields();
                            LoaddAllData(0, 0);
                        }

                        if (canDirectoryAccessed)
                        {
                            if (exceptionPdf != null)
                            {
                                MessageBox.Show("Cannot open this order. Please try again later." + System.Environment.NewLine + exceptionPdf, "Cannot Open Order", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.Yes);
                            }
                            else
                            {
                                if (exceptionEmail != null)
                                {
                                    MessageBox.Show("Could not send the email with the attachment" + System.Environment.NewLine + exceptionEmail.Message, "Email Sending Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                                }

                                var childWindow = new ChildWindow();
                                childWindow.ShowFormula(filePath + "/" + fileName);                              
                            }
                        }
                        else
                        {
                            MessageBox.Show("System was unable to access path (" + filePath + ")" + System.Environment.NewLine + "The Order created successfully! but the PDF was unable to create." + System.Environment.NewLine + "Please check the file path or try enabling VPN", "Cannot Access Path", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.Yes);
                        }                        
                    }
                    else if (insQteTuple.Item2 == 0)
                    {
                        MessageBox.Show("There has been an error when creating this order " + System.Environment.NewLine + "Please try again", "Error Occured", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
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

                };
                worker.RunWorkerAsync();
            }
        }

        private bool CheckProjectNameAvailable()
        {
            bool isAvailable = true;

            isAvailable = DBAccess.CheckNewOrderPDFProjectName(NewOrderPDFM.ProjectName);

            return isAvailable;
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

        private void ContactPersonChanged()
        {
            if (prevContactPerson != null && NewOrderPDFM.ContactPerson != null)
            {
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

        private void CustomerChanged()
        {
            DiscountAppliedTextVisibility = "Collapsed";
            ContactPersonPhoneVisibility = "Collapsed";
            NewOrderPDFM.GSTActive = true;
            NewOrderPDFM.InternalComments = string.Empty;
            SelectedShipTo = "Customer Address";

            if (SelectedCustomer != null && SelectedCustomer.CustomerId > 0 && SelectedCustomer.CompanyName != null && SelectedCustomer.CompanyName != "Select")
            {
                if (NewOrderPDFM.QuoteDetails != null && NewOrderPDFM.QuoteDetails.Count > 0)
                {
                    NewOrderPDFM.QuoteDetails.Clear();
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
                    FreightAndContactDetailsEnabled = true;
                    SoldToEnabled = true;
                    QuoteDetailsEnabled = true;
                    //CourierNameEnabled = true;
                    OtherTextBoxEnabled = false;
                    FreightDetailsEnabled = true;
                    InstructionsEnabled = true;
                    InternalCommentsEnabled = true;
                    ShipToEnabled = true;
                    AddShippingAddToProfileEnabled = false;
                    GstEnabled = true;
                    ContactPersonEnabled = false;
                    NewOrderPDFM = new NewOrderPDFM();
                    NewOrderPDFM.GSTActive = true;                   
                    if (NewOrderPDFM.QuoteDetails != null)
                    {
                        NewOrderPDFM.QuoteDetails.Clear();
                    }

                    NewOrderPDFM.User = new User();
                    CustomerType = "Prepaid";
                }
                else
                {                 
                    ProjectNameEnabled = false;
                    FreightAndContactDetailsEnabled = false;
                    CourierNameEnabled = false;
                    SoldToEnabled = false;
                    ShipToEnabled = false;
                    AddShippingAddToProfileEnabled = false;
                    FreightDetailsEnabled = false;
                    InstructionsEnabled = false;
                    InternalCommentsEnabled = false;
                    GstEnabled = false;
                    ContactPersonEnabled = false;

                    NewOrderPDFM.ProjectName = string.Empty;
                    NewOrderPDFM.ShipTo = string.Empty;
                
                    if (NewOrderPDFM.QuoteDetails != null && NewOrderPDFM.QuoteDetails.Count > 0)
                    {
                        NewOrderPDFM.QuoteDetails.Clear();
                    }
                    NewOrderPDFM.InternalComments = string.Empty;

                    CustomerType = string.Empty;

                    DiscountStructure.Clear();
                    DiscountStructure = new List<DiscountStructure>();
                    if (DisplayDiscountStructure != null)
                    {
                        DisplayDiscountStructure.Clear();
                        DisplayDiscountStructure = new ObservableCollection<DiscountStructure>();
                    }
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

        private void GSTChecked()
        {
            CalculateFinalTotal();
        }

        private void CheckUnloadType(string g)
        {
            if (g == "ForkliftAvailable")
            {
                NewOrderPDFM.UnloadType = "ForkliftAvailable";
            }
            else if (g == "Tailgate")
            {
                NewOrderPDFM.UnloadType = "Tailgate";
            }
            else
            {
                NewOrderPDFM.UnloadType = g;
            }
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
                        if (data != null && NewOrderPDFM != null)
                        {
                            NewOrderPDFM.CourierName = "Willmax";
                            CourierNameEnabled = false;
                        }
                        else
                        {
                            if (!quote.QuoteCourierName.Equals("Willmax"))
                            {
                                NewOrderPDFM.CourierName = quote.QuoteCourierName;
                                CourierNameEnabled = true;
                            }
                            else
                            {
                                NewOrderPDFM.CourierName = string.Empty;
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
                        if (data != null && NewOrderPDFM != null)
                        {
                            NewOrderPDFM.CourierName = "Willmax";
                            CourierNameEnabled = false;
                        }
                        else
                        {
                        //    NewOrderPDFM.CourierName = string.Empty;
                            if(NewOrderPDFM != null && NewOrderPDFM.CourierName.Equals("Willmax"))
                            {
                                NewOrderPDFM.CourierName = string.Empty;
                            }                        
                        }
                    }
                }

                //if (freightCodesArr != null)
                //{
                //    var data = freightCodesArr.SingleOrDefault(x => Convert.ToInt16(x) == SelectedFreightDetails.FreightCodeDetails.ID);
                //    if (data != null && NewOrderPDFM != null)
                //    {
                //        NewOrderPDFM.CourierName = "Willmax";
                //        CourierNameEnabled = false;
                //    }
                //}
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
                //SelectedShipTo = "Customer Address";                

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
                    AddCustomerVisibility = "Collapsed";
                    TickAddThisCusLabel = "Collapsed";
                    PhoneCopyVisibility = "Visible";
                    TickCopyPhoneEnabled = string.IsNullOrWhiteSpace(SelectedCustomer.CompanyTelephone) ? false : true;
                    TickCopyEmailEnabled = string.IsNullOrWhiteSpace(SelectedCustomer.CompanyEmail) ? false : true;
                    CustomerTypeEnabled = false;

                    if (originalCustomer.Equals(SelectedCustomer.CompanyName))
                    {                  
                        if (quote != null && quote.Customer != null)
                        {
                            if (quote.Customer.ShipAddress.Equals("Collect from A1 Rubber QLD")) 
                            {
                                SelectedShipTo = "A1 Rubber QLD";
                            }
                            else if(quote.Customer.ShipAddress.Equals("Collect from A1 Rubber NSW"))
                            {
                                SelectedShipTo = "A1 Rubber NSW";
                            }
                            else
                            {
                                SelectedShipTo = "Customer Address";
                            }
                        }
                        else
                        {
                            SelectedShipTo = "Customer Address";
                        }
                    }
                    else
                    {
                        SelectedShipTo = "Customer Address";
                    }

                    GstEnabled = true;
                    QuoteDetailsEnabled = true;
                    InstructionsEnabled = true;
                    InternalCommentsEnabled = true;
                    DiscountAppliedTextVisibility = "Collapsed";

                    if (SelectedCustomer.CustomerId > 0)
                    {
                        DiscountStructure.Clear();
                        DiscountStructure = DBAccess.GetDiscount(SelectedCustomer.CustomerId);
                        DisplayDiscountStructure = new ObservableCollection<Models.Discounts.DiscountStructure>(DiscountStructure);
                        for (int i = DisplayDiscountStructure.Count - 1; i >= 0; i--)
                        {
                            if (DisplayDiscountStructure[i].Discount == 0)
                                DisplayDiscountStructure.RemoveAt(i);
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(originalCustomer) && !originalCustomer.Equals(SelectedCustomer.CompanyName))
                    {
                        NewOrderPDFM.QuoteDetails = new ObservableCollection<QuoteDetails>();
                        NewOrderPDFM.QuoteDetails.CollectionChanged += productChanged;

                        SelectedFreightDetails = new FreightDetails();
                        SelectedFreightDetails.FreightCodeDetails = new FreightCode();
                        SelectedFreightDetails.FreightCodeDetails = new FreightCode() { ID = 0, Code = "Select", Description = "", Price = 0, Unit = "" };
                        SelectedFreightDetails.FreightCodeDetails.PriceEnabled = false;

                        NewOrderPDFM.ContactPerson = new ContactPerson();
                        NewOrderPDFM.InternalComments = string.Empty;

                        NewOrderPDFM.CourierName = string.Empty;
                    }

                    //Get contact Person details
                    if (SelectedCustomer != null)
                    {
                        List<ContactPerson> cp = DBAccess.GetContactPersonByCustomerID(SelectedCustomer.CustomerId);
                        cp.Add(new ContactPerson() { ContactPersonID = -1, ContactPersonName = "Other", CustomerID = SelectedCustomer.CustomerId });
                        SelectedCustomer.ContactPerson = cp;

                        if (quote != null && quote.ContactPerson != null && SelectedCustomer.CompanyName != null && SelectedCustomer.CompanyName.Equals(quote.Customer.CompanyName))
                        {
                            if (NewOrderPDFM.ContactPerson != null)
                            {
                                NewOrderPDFM.ContactPerson.ContactPersonID = quote.ContactPerson.ContactPersonID;
                                NewOrderPDFM.ContactPerson = quote.ContactPerson;
                                NewOrderPDFM.ContactPerson.ContactPersonName = quote.ContactPerson.ContactPersonName;
                                NewOrderPDFM.ContactPerson.PhoneNumber1 = quote.ContactPerson.PhoneNumber1;
                                NewOrderPDFM.ContactPerson.PhoneNumber2 = quote.ContactPerson.PhoneNumber2;
                                NewOrderPDFM.ContactPerson.Email = quote.ContactPerson.Email;
                            }
                            else
                            {
                                NewOrderPDFM.ContactPerson = new ContactPerson();
                                NewOrderPDFM.ContactPerson.ContactPersonName = "Other";
                            }
                        }
                        else
                        {
                            if (NewOrderPDFM.ContactPerson == null)
                            {
                                NewOrderPDFM.ContactPerson = new ContactPerson();
                            }
                            NewOrderPDFM.ContactPerson.ContactPersonName = "Other";
                        }
                    }

                    if(!string.IsNullOrWhiteSpace(SelectedCustomer.CompanyAddress) || !string.IsNullOrWhiteSpace(SelectedCustomer.CompanyCity) || !string.IsNullOrWhiteSpace(SelectedCustomer.CompanyState) ||
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

                    NewOrderPDFM.QuoteDetails = new ObservableCollection<QuoteDetails>();
                    NewOrderPDFM.QuoteDetails.CollectionChanged += productChanged;

                    SelectedFreightDetails = new FreightDetails();
                    SelectedFreightDetails.FreightCodeDetails = new FreightCode();
                    //SelectedFreightDetails.FreightCodeDetails.Code = "Select";
                    SelectedFreightDetails.FreightCodeDetails = new FreightCode() { ID = 0, Code = "Select", Description = "", Price = 0, Unit = "" };
                    SelectedFreightDetails.FreightCodeDetails.PriceEnabled = false;
                    NewOrderPDFM.CourierName = string.Empty;
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

                    NewOrderPDFM.InternalComments = string.Empty;

                    DiscountStructure.Clear();
                    if (DisplayDiscountStructure != null)
                    {
                        if(DisplayDiscountStructure.Count > 0)
                        {
                            System.Windows.Application.Current.Dispatcher.Invoke(() =>
                          {
                              DisplayDiscountStructure.Clear();
                              DisplayDiscountStructure = new ObservableCollection<DiscountStructure>();
                          });
                        }
                    }


                    QuoteDetailsEnabled = true;
                    //CourierNameEnabled = true;
                    SoldToEnabled = false;
                    AddShippingAddToProfileEnabled = false;
                    AddCcompanyAddToShippingEnabled = false;
                    AddShippingAddress = false;
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



        public NewOrderPDFM NewOrderPDFM
        {
            get
            {
                if (_newOrderPDFM != null)
                {
                    if (_newOrderPDFM.ContactPerson != null)
                    {
                        if (_newOrderPDFM.ContactPerson.ContactPersonID == -1)
                        {
                            OtherContactPersonVisibility = "Visible";
                            ContactPersonPhoneVisibility = "Collapsed";
                        }
                        else if (_newOrderPDFM.ContactPerson.ContactPersonID == 0)
                        {
                            OtherContactPersonVisibility = "Collapsed";
                            ContactPersonPhoneVisibility = "Collapsed";
                            if (_newOrderPDFM.ContactPerson != null && _newOrderPDFM.ContactPerson.ContactPersonID == 0)
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
                        if (_newOrderPDFM.ContactPerson != null && _newOrderPDFM.ContactPerson.ContactPersonID == 0)
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
                }
                return _newOrderPDFM;
            }
            set
            {
                _newOrderPDFM = value;
                RaisePropertyChanged("NewOrderPDFM");

                if (NewOrderPDFM.ContactPerson == null)
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

        public bool FreightAndContactDetailsEnabled
        {
            get { return _freightAndContactDetailsEnabled; }
            set
            {
                _freightAndContactDetailsEnabled = value;
                RaisePropertyChanged("FreightAndContactDetailsEnabled");
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

        public bool AddCcompanyAddToShippingEnabled
        {
            get { return _addCcompanyAddToShippingEnabled; }
            set
            {
                _addCcompanyAddToShippingEnabled = value;
                RaisePropertyChanged("AddCcompanyAddToShippingEnabled");
                
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
                    if (SelectedCustomer != null && (!string.IsNullOrWhiteSpace(SelectedCustomer.CompanyAddress) || !string.IsNullOrWhiteSpace(SelectedCustomer.CompanyCity) || !string.IsNullOrWhiteSpace(SelectedCustomer.CompanyState) ||
                        !string.IsNullOrWhiteSpace(SelectedCustomer.CompanyPostCode)))
                    {
                        AddCcompanyAddToShippingEnabled = true;
                    }
                    else
                    {
                        AddCcompanyAddToShippingEnabled = false;
                    }

                    FreightAndContactDetailsEnabled = true;
                    //CourierNameEnabled = true;
                    ShippingAddressVisibility = "Visible";
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
                            else if (string.IsNullOrWhiteSpace(SelectedCustomer.ShipAddress) && quote != null)
                            {
                                ShippingAddress = quote.Customer.ShipAddress == "Collect from A1 Rubber QLD" || quote.Customer.ShipAddress == "Collect from A1 Rubber NSW" ? "" : quote.Customer.ShipAddress;
                                ShippingCity = quote.Customer.ShipCity;
                                ShippingState = quote.Customer.ShipState;
                                ShippingPostCode = quote.Customer.ShipPostCode;
                                ShippingCountry = quote.Customer.ShipCountry;
                            }
                            else if (quote != null)
                            {
                                ShippingAddress = quote.Customer.ShipAddress == "Collect from A1 Rubber QLD" || quote.Customer.ShipAddress == "Collect from A1 Rubber NSW" ? "" : quote.Customer.ShipAddress;
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
                        ShipToText = "Collect from A1 Rubber QLD";
                    }
                    else if (SelectedShipTo == "A1 Rubber NSW")
                    {
                        ShipToText = "Collect from A1 Rubber NSW";
                    }

                    AddCcompanyAddToShippingEnabled = false;
                    ShippingAddressVisibility = "Collapsed";
                    NewOrderPDFM.CourierName = string.Empty;
                    NewOrderPDFM.UnloadType = string.Empty;
                    NewOrderPDFM.FreightType = string.Empty;
                    NewOrderPDFM.OrderTruck = string.Empty;
                    NewOrderPDFM.CustomerToChargedFreight = string.Empty;
                    OtherTextBoxEnabled = false;
                    General = false;
                    Express = false;
                    A1RubberOrderTruck = false;
                    CustomerOrderTruck = false;
                    CustomerToChargeFreight = false;
                    CustomerNotToChargeFreight = false;
                    NewOrderPDFM.SiteContactName = string.Empty;
                    NewOrderPDFM.SiteContactPhone = string.Empty;
                    FreightAndContactDetailsEnabled = false;
                    CourierNameEnabled = false;
                    AddShippingAddress = false;
                    AddCompanyAddressToShipping = false;
                    ShippingAddress = ShipToText;
                    ShippingCity = string.Empty;
                    ShippingState = string.Empty;
                    ShippingPostCode = string.Empty;
                    ShippingCountry = string.Empty;
                    NewOrderPDFM.CourierName = string.Empty;
                    IsForkliftAvailable = false;
                    IsTailgate = false;
                    IsOther = false;
                    UnloadTypeOther = string.Empty;
                }

                ProcessFreight();
            }
        }

        public string PrepaidCustomerName
        {
            get { return _prepaidCustomerName; }
            set
            {
                _prepaidCustomerName = value;
                RaisePropertyChanged("PrepaidCustomerName");

                if (!string.IsNullOrWhiteSpace(PrepaidCustomerName))
                {
                    ProjectNameEnabled = true;
                    FreightAndContactDetailsEnabled = true;
                    ShipToEnabled = true;
                    AddCustomerVisibility = "Visible";
                    TickAddThisCusLabel = "Visible";
                    PhoneCopyVisibility = "Collapsed";
                    TickCopyPhoneEnabled = false;
                    TickCopyEmailEnabled = false;
                    AddCustomerToDatabase = false;
                    CustomerTypeEnabled = false;

                    if(!string.IsNullOrWhiteSpace(originalCustomer) && !originalCustomer.Equals(PrepaidCustomerName))
                    {
                        //Clear all the details if customer is changed
                        originalCustomer = string.Empty;
                        NewOrderPDFM.QuoteNoStr = string.Empty;
                        NewOrderPDFM.SiteContactName = string.Empty;
                        NewOrderPDFM.SiteContactPhone = string.Empty;
                        FlexibleChecked = false;
                        SpecificChecked = false;
                        NewOrderPDFM.RequiredDate = null;
                        NewOrderPDFM.CourierName = string.Empty;
                        IsForkliftAvailable = false;
                        IsTailgate = false;
                        IsOther = false;
                        UnloadTypeOther = string.Empty;
                        General = false;
                        Express = false;
                        A1RubberOrderTruck = false;
                        CustomerOrderTruck = false;
                        CustomerToChargeFreight = false;
                        CustomerNotToChargeFreight = false;
                        AddCompanyAddressToShipping = false;

                        NewOrderPDFM.UnloadType = string.Empty;
                        NewOrderPDFM.FreightType = string.Empty;
                        NewOrderPDFM.OrderTruck = string.Empty;
                        NewOrderPDFM.CustomerToChargedFreight = string.Empty;
                        NewOrderPDFM.Instructions = string.Empty;
                        NewOrderPDFM.DateTypeRequired = string.Empty;

                        ShippingAddress = string.Empty;
                        ShippingCity = string.Empty;
                        ShippingState = string.Empty;
                        ShippingPostCode = string.Empty;
                        ShippingCountry = string.Empty;

                        if(quote != null)
                        {
                            quote = null;
                        }
                    }
                                       
                    //This customer is not in DB
                    if (SelectedCustomer == null)
                    {
                        CustomerType = "Prepaid";
                        QuoteDetailsEnabled = true;   
                    
                        NewOrderPDFM.QuoteDetails = new ObservableCollection<QuoteDetails>();
                        NewOrderPDFM.QuoteDetails.CollectionChanged += productChanged;

                        SelectedFreightDetails = new FreightDetails();
                        SelectedFreightDetails.FreightCodeDetails = new FreightCode() { ID = 0, Code = "Select", Description = "", Price = 0, Unit = "" };
                        SelectedFreightDetails.FreightCodeDetails.PriceEnabled = false;

                        NewOrderPDFM.ContactPerson = new ContactPerson();
                        ContactPersonEnabled = true;
                        AddShippingAddToProfileEnabled = false;

                        SelectedCustomer = new Customer();
                        SelectedCustomer.ContactPerson = new List<ContactPerson>();
                        SelectedCustomer.ContactPerson.Add(new ContactPerson() { ContactPersonID = -1, ContactPersonName = "Other", CustomerID = SelectedCustomer.CustomerId });
                        NewOrderPDFM.ContactPerson.ContactPersonName = "Other";

                        NewOrderPDFM.CourierName = string.Empty;
                        CourierNameEnabled = false;
                    }
                    else
                    {
                        AddCcompanyAddToShippingEnabled = false;
                        if (SelectedCustomer.CustomerId == 0)
                        {
                            QuoteDetailsEnabled = true;
                            //CourierNameEnabled = true;
                            AddCcompanyAddToShippingEnabled = false;
                        }
                        else
                        {
                            AddCcompanyAddToShippingEnabled = true;
                            AddCustomerVisibility = "Collapsed";
                            TickAddThisCusLabel = "Collapsed";
                            PhoneCopyVisibility = "Visible";
                            TickCopyPhoneEnabled = string.IsNullOrWhiteSpace(SelectedCustomer.CompanyTelephone) ? false : true ;
                            TickCopyEmailEnabled = string.IsNullOrWhiteSpace(SelectedCustomer.CompanyEmail) ? false : true;
                            CustomerTypeEnabled = false;

                            AddUpdateActive = true;
                            AddUpdateBackground = "#666666";                            
                        }  

                        //This customer is in DB
                        AddShippingAddToProfileEnabled = true;
                        ContactPersonEnabled = true;
                        AddShippingAddToProfileEnabled = SelectedCustomer.CustomerId == 0 ? false : true; 
                    }
                }
                else
                {
                    if (quote != null)
                    {
                        quote = null;
                    }
                    originalCustomer = string.Empty;
                    NewOrderPDFM.QuoteNoStr = string.Empty;
                    NewOrderPDFM.SiteContactName = string.Empty;
                    NewOrderPDFM.SiteContactPhone = string.Empty;
                    FlexibleChecked = false;
                    SpecificChecked = false;
                    NewOrderPDFM.RequiredDate = null;
                    NewOrderPDFM.CourierName = string.Empty;
                    IsForkliftAvailable = false;
                    IsTailgate = false;
                    IsOther = false;
                    UnloadTypeOther = string.Empty;
                    General = false;
                    Express = false;
                    A1RubberOrderTruck = false;
                    CustomerOrderTruck = false;
                    CustomerToChargeFreight = false;
                    CustomerNotToChargeFreight = false;

                    NewOrderPDFM.UnloadType = string.Empty;
                    NewOrderPDFM.FreightType = string.Empty;
                    NewOrderPDFM.OrderTruck = string.Empty;
                    NewOrderPDFM.CustomerToChargedFreight = string.Empty;
                    NewOrderPDFM.Instructions = string.Empty;
                    NewOrderPDFM.DateTypeRequired = string.Empty;

                    NewOrderPDFM.QuoteDetails = new ObservableCollection<QuoteDetails>();
                    NewOrderPDFM.QuoteDetails.CollectionChanged += productChanged;

                    SelectedFreightDetails = new FreightDetails();
                    SelectedFreightDetails.FreightCodeDetails = new FreightCode() { ID = 0, Code = "Select", Description = "", Price = 0, Unit = "" };
                    SelectedFreightDetails.FreightCodeDetails.PriceEnabled = false;

                    SelectedShipTo = "Customer Address";
                    ShippingAddress = string.Empty;
                    ShippingCity = string.Empty;
                    ShippingCountry = string.Empty;
                    ShippingPostCode = string.Empty;
                    ShippingState = string.Empty;
                    AddCustomerVisibility = "Collapsed";
                    PhoneCopyVisibility = "Collapsed";
                    TickAddThisCusLabel = "Collapsed";
                    TickCopyPhoneEnabled =false;
                    TickCopyEmailEnabled = false;
                    NewOrderPDFM.InternalComments = string.Empty;
                    CustomerTypeEnabled = false;                                     

                    DiscountStructure.Clear();
                    if (DisplayDiscountStructure != null)
                    {
                        DisplayDiscountStructure.Clear();
                        DisplayDiscountStructure = new ObservableCollection<DiscountStructure>();
                    }

                    QuoteDetailsEnabled = false;                   
                    CustomerTypeEnabled = false;
                    ProjectNameEnabled = false;
                    FreightAndContactDetailsEnabled = false;
                    SoldToEnabled = false;
                    ShipToEnabled = false;
                    AddShippingAddToProfileEnabled = false;
                    FreightDetailsEnabled = false;
                    InstructionsEnabled = false;
                    InternalCommentsEnabled = false;
                    GstEnabled = false;
                    ContactPersonEnabled = false;
                    CustomerType = string.Empty;

                    NewOrderPDFM.QuoteCourierName = string.Empty;
                    CourierNameEnabled = false;
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

        public DateTime StartDate
        {
            get { return _startDate; }
            set
            {
                _startDate = value;
                RaisePropertyChanged("StartDate");
            }
        }       

        public bool General
        {
            get { return _general; }
            set
            {
                _general = value;
                RaisePropertyChanged("General");
                if (General)
                {
                    CheckFreightType("General");
                }
            }
        }

       

        public bool Express
        {
            get { return _express; }
            set
            {
                _express = value;
                RaisePropertyChanged("Express");
                if (Express)
                {
                    CheckFreightType("Express");
                }
            }
        }

        private void CheckFreightType(string g)
        {
            if (g == "General")
            {
                NewOrderPDFM.FreightType = "General";
            }
            else
            {
                NewOrderPDFM.FreightType = "Express";
            }

        }


        public bool A1RubberOrderTruck
        {
            get { return _a1RubberOrderTruck; }
            set
            {
                _a1RubberOrderTruck = value;
                RaisePropertyChanged("A1RubberOrderTruck");
                if (A1RubberOrderTruck)
                {
                    OrderTruckCheck("A1 Rubber");
                }
            }
        }

        public bool CustomerOrderTruck
        {
            get { return _customerOrderTruck; }
            set
            {
                _customerOrderTruck = value;
                RaisePropertyChanged("CustomerOrderTruck");
                if (CustomerOrderTruck)
                {
                    OrderTruckCheck("Customer");
                }
            }
        }

        private void OrderTruckCheck(string s)
        {
            if (s == "A1 Rubber")
            {
                NewOrderPDFM.OrderTruck = "A1 Rubber";
            }
            else
            {
                NewOrderPDFM.OrderTruck = "Customer";
            }
        }

        public bool CustomerToChargeFreight
        {
            get { return _customerToChargeFreight; }
            set
            {
                _customerToChargeFreight = value;
                RaisePropertyChanged("CustomerToChargeFreight");
                if(CustomerToChargeFreight)
                {
                    CustomerChargeFreight(true);
                }
            }
        }

        public bool CustomerNotToChargeFreight
        {
            get { return _customerNotToChargeFreight; }
            set
            {
                _customerNotToChargeFreight = value;
                RaisePropertyChanged("CustomerNotToChargeFreight");
                if (CustomerNotToChargeFreight)
                {
                    CustomerChargeFreight(false);
                }
            }
        }

        private void CustomerChargeFreight(bool s)
        {
            if (s)
            {
                NewOrderPDFM.CustomerToChargedFreight = "True";
            }
            else
            {
                NewOrderPDFM.CustomerToChargedFreight = "False";
            }
        }


        public string PopUpCloseVisibility
        {
            get { return _popUpCloseVisibility; }
            set
            {
                _popUpCloseVisibility = value;
                RaisePropertyChanged("PopUpCloseVisibility");
            }
        }

        public string MainCloseVisibility
        {
            get { return _mainCloseVisibility; }
            set
            {
                _mainCloseVisibility = value;
                RaisePropertyChanged("MainCloseVisibility");
            }
        }

        public bool IsForkliftAvailable
        {
            get { return _isForkliftAvailable; }
            set
            {
                _isForkliftAvailable = value;
                RaisePropertyChanged("IsForkliftAvailable");
                if (IsForkliftAvailable)
                {
                    CheckUnloadType("ForkliftAvailable");
                }
            }
        }

        public bool IsTailgate
        {
            get { return _isTailgate; }
            set
            {
                _isTailgate = value;
                RaisePropertyChanged("IsTailgate");
                if (IsTailgate)
                {
                    CheckUnloadType("Tailgate");
                }
            }
        }

        public bool IsOther
        {
            get { return _isOther; }
            set
            {
                _isOther = value;
                RaisePropertyChanged("IsOther");
                if (IsOther)
                {
                    CheckUnloadType(UnloadTypeOther);
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

                    if (_selectedFreightDetails.FreightCodeDetails.FreightCodeID == 50)
                    {
                        _selectedFreightDetails.FreightCodeDetails.PriceEnabled = true;
                    }
                    else
                    {
                        _selectedFreightDetails.FreightCodeDetails.PriceEnabled = false;
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

                NewOrderPDFM.FreightTotal = fPrice * pallets;

                decimal freightDis = (NewOrderPDFM.FreightTotal * _selectedFreightDetails.Discount) / 100;
                NewOrderPDFM.FreightTotal = NewOrderPDFM.FreightTotal - freightDis;

                if (NewOrderPDFM.GSTActive)
                {
                    NewOrderPDFM.Gst = NewOrderPDFM.GSTActive ? Math.Round((((NewOrderPDFM.ListPriceTotal - NewOrderPDFM.DiscountedTotal) + NewOrderPDFM.FreightTotal) * 10) / 100, 2) : 0;
                }
                else
                {
                    NewOrderPDFM.Gst = 0;
                }


                NewOrderPDFM.TotalBeforeTax = NewOrderPDFM.ListPriceTotal - NewOrderPDFM.DiscountedTotal;

                NewOrderPDFM.TotalAmount = Math.Round(NewOrderPDFM.TotalBeforeTax, 2) + Math.Round(NewOrderPDFM.FreightTotal, 2) + Math.Round(NewOrderPDFM.Gst, 2);

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

        public bool AddCustomerToDatabase
        {
            get { return _addCustomerToDatabase; }
            set
            {
                _addCustomerToDatabase = value;
                RaisePropertyChanged("AddCustomerToDatabase");

                IsPrimaryBusinessEnabled = false;
               
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
                    TickAddThisCusLabel = "Collapsed";
                    AddUpdateActive = true;
                    AddUpdateBackground = "#666666";
                    CustomerTypeEnabled = true;
                }
                else
                {
                    AddShippingAddToProfileEnabled = false;
                    AddShippingAddress = false;
                    TickAddThisCusLabel = "Visible";
                    AddUpdateBackground = "#FFDEDEDE";
                    AddUpdateActive = false;
                    CustomerTypeEnabled = false;

                    if ((SelectedCustomer == null || SelectedCustomer.CustomerId == 0) && DisplayDiscountStructure != null)
                    {
                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                          {
                              DisplayDiscountStructure.Clear();
                          });
                    }
                }
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
       
        public string AddUpdateBackground
        {
            get { return _addUpdateBackground; }
            set
            {
                _addUpdateBackground = value;
                RaisePropertyChanged("AddUpdateBackground");
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

        public string AddCustomerVisibility
        {
            get { return _addCustomerVisibility; }
            set
            {
                _addCustomerVisibility = value;
                RaisePropertyChanged("AddCustomerVisibility");
            }
        }

        public string TickAddThisCusLabel
        {
            get { return _tickAddThisCusLabel; }
            set
            {
                _tickAddThisCusLabel = value;
                RaisePropertyChanged("TickAddThisCusLabel");
            }
        }

        

        private bool CanExecute(object parameter)
        {
            return true;
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

        public string UnloadTypeOther
        {
            get
            {
                return _unloadTypeOther;
            }
            set
            {
                _unloadTypeOther = value;
                RaisePropertyChanged("UnloadTypeOther");
                if (IsOther == true)
                {
                    if (!string.IsNullOrWhiteSpace(UnloadTypeOther))
                    {
                        NewOrderPDFM.UnloadType = UnloadTypeOther;
                    }
                    else
                    {
                        NewOrderPDFM.UnloadType = string.Empty;
                    }
                }
            }
        }

        public string OtherCommandPara
        {
            get
            {
                return _otherCommandPara;
            }
            set
            {
                _otherCommandPara = value;
                RaisePropertyChanged("OtherCommandPara");
            }
        }

        public bool OtherTextBoxEnabled
        {
            get
            {
                return _otherTextBoxEnabled;
            }
            set
            {
                _otherTextBoxEnabled = value;
                RaisePropertyChanged("OtherTextBoxEnabled");
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


        public bool FlexibleChecked
        {
            get
            {
                return _flexibleChecked;
            }
            set
            {
                _flexibleChecked = value;
                RaisePropertyChanged("FlexibleChecked");

                if (FlexibleChecked && NewOrderPDFM != null)
                {
                    NewOrderPDFM.DateTypeRequired = "Flexible";
                }
                
            }
        }

        public bool SpecificChecked
        {
            get
            {
                return _specificChecked;
            }
            set
            {
                _specificChecked = value;
                RaisePropertyChanged("SpecificChecked");

                if (SpecificChecked && NewOrderPDFM != null)
                {
                    NewOrderPDFM.DateTypeRequired = "Specific";
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
                    if (NewOrderPDFM != null && NewOrderPDFM.ContactPerson != null && NewOrderPDFM.ContactPerson.ContactPersonName.Equals("Other"))
                    {
                        OtherContactNamePhone1 = SelectedCustomer.CompanyTelephone;
                    }
                    else if (!string.IsNullOrWhiteSpace(NewOrderPDFM.ContactPerson.ContactPersonName))
                    {
                        if (NewOrderPDFM != null && NewOrderPDFM.ContactPerson != null)
                        {
                            prevContactPerson.ContactPersonID = NewOrderPDFM.ContactPerson.ContactPersonID;
                            prevContactPerson.PhoneNumber1 = NewOrderPDFM.ContactPerson.PhoneNumber1;
                        }
                        NewOrderPDFM.ContactPerson.PhoneNumber1 = SelectedCustomer.CompanyTelephone;
                    }
                }
                else
                {
                    OtherContactNamePhone1 = string.Empty;

                    if (((quote != null && quote.Customer != null) || (NewOrderPDFM != null)) && prevContactPerson != null && !string.IsNullOrWhiteSpace(prevContactPerson.PhoneNumber1) && NewOrderPDFM.ContactPerson != null)
                    {
                        var data = SelectedCustomer.ContactPerson.SingleOrDefault(x => x.ContactPersonID == prevContactPerson.ContactPersonID);
                        if (data != null)
                        {
                            data.PhoneNumber1 = prevContactPerson.PhoneNumber1;

                            if (NewOrderPDFM.ContactPerson.ContactPersonID == prevContactPerson.ContactPersonID && !string.IsNullOrWhiteSpace(prevContactPerson.PhoneNumber1))
                            {
                                NewOrderPDFM.ContactPerson.PhoneNumber1 = prevContactPerson.PhoneNumber1;
                            }
                        }
                        prevContactPerson.PhoneNumber1 = null;
                    }                  
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
                    if (NewOrderPDFM != null && NewOrderPDFM.ContactPerson != null && NewOrderPDFM.ContactPerson.ContactPersonName.Equals("Other"))
                    {
                        OtherContactNameEmail = SelectedCustomer.CompanyEmail;
                    }
                    else if (!string.IsNullOrWhiteSpace(NewOrderPDFM.ContactPerson.ContactPersonName))
                    {
                        if (NewOrderPDFM != null && NewOrderPDFM.ContactPerson != null)
                        {
                            prevContactPerson.ContactPersonID = NewOrderPDFM.ContactPerson.ContactPersonID;
                            prevContactPerson.Email = NewOrderPDFM.ContactPerson.Email;
                        }
                        NewOrderPDFM.ContactPerson.Email = SelectedCustomer.CompanyEmail;
                    }
                }
                else
                {
                    OtherContactNameEmail = string.Empty;

                    if (((quote != null && quote.Customer != null) || (NewOrderPDFM != null)) && prevContactPerson != null && !string.IsNullOrWhiteSpace(prevContactPerson.Email) && NewOrderPDFM.ContactPerson != null)
                    {
                        var data = SelectedCustomer.ContactPerson.SingleOrDefault(x => x.ContactPersonID == prevContactPerson.ContactPersonID);
                        if (data != null)
                        {
                            data.Email = prevContactPerson.Email;

                            if (NewOrderPDFM.ContactPerson.ContactPersonID == prevContactPerson.ContactPersonID && !string.IsNullOrWhiteSpace(prevContactPerson.Email))
                            {
                                NewOrderPDFM.ContactPerson.Email = prevContactPerson.Email;
                            }
                        }
                        prevContactPerson.Email = null;
                    }                  
                }
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

        public ICommand CustomerChangedCommand
        {
            get
            {
                return _customerChangedCommand ?? (_customerChangedCommand = new CommandHandler(() => CustomerChanged(), true));
            }
        }

        public ICommand ContactPersonChangedCommand
        {
            get
            {
                return _contactPersonChangedCommand ?? (_contactPersonChangedCommand = new CommandHandler(() => ContactPersonChanged(), true));
            }
        }
        
        public RelayCommand FreightTypeCommand
        {
            get
            {
                return new RelayCommand(parameter =>
                {
                    string str = (string)parameter;
                    NewOrderPDFM.FreightType = str;
                });
            }
        }

        public RelayCommand UnloadTypeCommand
        {
            get
            {
                return new RelayCommand(parameter =>
                {
                    string str = (string)parameter;
                    NewOrderPDFM.UnloadType = str;
                    if (str == "ForkliftAvailable" || str == "Tailgate")
                    {
                        UnloadTypeOther = string.Empty;
                        OtherTextBoxEnabled = false;
                    }
                    else 
                    {
                        OtherTextBoxEnabled = true;
                    }
                });
            }
        }

        public RelayCommand OrderTruckCommand
        {
            get
            {
                return new RelayCommand(parameter =>
                {
                    string str = (string)parameter;
                    NewOrderPDFM.OrderTruck = str;
                });
            }
        }

        public RelayCommand FreightChargeCommand
        {
            get
            {
                return new RelayCommand(parameter =>
                {
                    string str = (string)parameter;
                    NewOrderPDFM.CustomerToChargedFreight = str;
                });
            }
        }

        public RelayCommand DateTypeRequiredCommand
        {
            get
            {
                return new RelayCommand(parameter =>
                {
                    string str = (string)parameter;
                    NewOrderPDFM.DateTypeRequired = str;
                });
            }
        }    

        public ICommand AddUpdateDiscountCommand
        {
            get
            {
                return _addUpdateDiscountCommand ?? (_addUpdateDiscountCommand = new CommandHandler(() => AddUpdateDiscount(), true));
            }
        }

        //public ICommand StockedValueLostFocusCommand
        //{
        //    get
        //    {
        //        return _stockedValueLostFocusCommand ?? (_stockedValueLostFocusCommand = new CommandHandler(() => ProcessFreight(), true));
        //    }
        //}

        public ICommand GSTCheckedCommand
        {
            get
            {
                return _gSTCheckedCommand ?? (_gSTCheckedCommand = new CommandHandler(() => GSTChecked(), true));
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
