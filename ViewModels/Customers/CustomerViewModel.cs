using A1RConsole.Commands;
using A1RConsole.Core;
using A1RConsole.DB;
using A1RConsole.Models;
using A1RConsole.Models.Categories;
using A1RConsole.Models.Meta;
using A1RConsole.Models.Products;
using A1RConsole.Views;
using A1RConsole.Views.Customers;
using A1RConsole.ViewModels.Sales;
using A1RConsole.Bases;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using A1RConsole.Models.Customers;
using A1RConsole.Models.Discounts;
using A1RConsole.Models.Orders;
using A1RConsole.Interfaces;
using A1RConsole.ViewModels.Customers;
using System.Text.RegularExpressions;
using A1RConsole.Models.Comments;

namespace A1RConsole.ViewModels.Customers
{
    public class CustomerViewModel : ViewModelBase, IContent
    {
        #region PRIVATE_PROPERTIES
        private ObservableCollection<CustomerCreditActivity> _customerCreditActivity;
        private ObservableCollection<Customer> _customerList;
        private Customer _selectedCustomer;

        private string _selectedCountry;
        private List<Country> _countryList;
        private Int32 customerPendingID;
        private bool _browseTabEnabled;
        private string _version;
        private string _lastUpdatedString;
        private string _lastOrderDate;
        private string _addCreditBackground;
        private string _newCompanyNameVisibility;
        private string _selectedCompanyNameVisibility;
        private string _cancelNewCustomerVisibility;
        private string _newCustomerVisibility;
        private string _newCompanyName;
        private string _updateCommandVisibility;
        private string _addCommandVisibility;
        private string _addNewCustomerCreditVisibility;
        private string _updateCreditVisibility;
        private string _customerType;
        private string _companyAddress;
        private string _companyCity;
        private string _companyState;
        //private string _companyCountry;
        private string _companyPostCode;
        private string _companyEmail;
        private string _companyTelephone;
        private string _companyFax;
        private decimal _creditLimit;
        private decimal _creditRemaining;
        private decimal _debt;
        private decimal _creditOwed;
        private string _shipAddress;
        private string _shipCity;
        private string _shipState;
        private string _shipPostCode;
        private string _shipCountry;
        private string _designation1;
        private string _firstName1;
        private string _lastName1;
        private string _telephone1;
        private string _mobile1;
        private string _fax1;
        private string _email1;
        private string _designation2;
        private string _firstName2;
        private string _lastName2;
        private string _telephone2;
        private string _mobile2;
        private string _fax2;
        private string _email2;
        private string _designation3;
        private string _firstName3;
        private string _lastName3;
        private string _telephone3;
        private string _mobile3;
        private string _fax3;
        private string _email3;
        private string _searchBtnVisibility;
        private bool _isStopCreditEnabled;
        private bool _customerTypeEnabled;
        private Customer newCustomer;
        private ObservableCollection<DiscountStructure> _discountStructure;
        private ObservableCollection<ContactPerson> _contactPersons;
        private bool _active;
        private decimal _totalReleased;
        private int _mainTabSelectedIndex;
        private int _bottomTabSelectedIndex;
        private ObservableCollection<Category> _categories;
        private ObservableCollection<int> _discount;
        private bool _isCreditTabEnabled;
        private bool blockCreditTab;
        private bool blockCreditHistoryTab;
        private ObservableCollection<SalesOrderLines> _salesOrderLines;
        private List<Category> mainCategories;
        private List<Category> _primaryBusinessCategories;
        private List<ProductType> _productsInstalled;
        private ProductType _selectedProductsInstalled;
        private ObservableCollection<ProductType> _productsInstalledView;
        private ProductType _productTypeSelected;
        private bool _isRemoveEnabled;
        private bool _isAddEnabled;
        private bool _isCreditHistoryTabEnabled;
        private ObservableCollection<ProductType> orgProductInstalled;
        private Customer _selectedSearchCustomer;
        private List<string> _contactPersonList;
        private string _selectedFirstName;
        private string _selectedLastName;
        private Category _selectedSearchCategory;
        private List<string> _suburb;
        private string _selectedState;
        private string _selectedSuburb;
        private ObservableCollection<Customer> _searchedCustomers;
        private string _selectedCustomerState;
        private bool _isCreditUpdateBtnEnable;
        private string _editCompanyNameVisibility;
        private string _editCompanyName;
        private string _editNameVisibility;
        private string _cancelEditCompanyNameVisibility;
        private bool _customerBottomTabEnabled;
        private string _customerTypeStarVisibility;
        private bool ignoreCustomerType;
        private string addToCustomerPending;
        private string _closeBtnVisible;
        private bool _isAdminTabEnabled;
        private AdminNote _adminNote;
        private string _closeNormlVisibility;

        private string _addNewContactBtnVisibility;
        private ICollectionView _itemsView;
        //private string _searchString;
        //private bool _stopCreditEnabled;
        private string _customerNote;
        private bool _isStopCredit;
        private Category _selectedCategory;
        private bool IsDirty { get; set; }
        public event EventHandler Closing;
        public event Action<int> Closed;
        public RelayCommand CloseCommand { get; private set; }
        private ICommand _updateCommand;
        private ICommand _updateCustomerCreditCommand;
        private ICommand _addNewCustomerCommand;
        private ICommand _cancelNewCustomerCommand;
        private ICommand _addNewCustomerDBCommand;
        private ICommand _addNewCustomerCreditCommand;
        private ICommand _addNewNoteCommand;
        private ICommand _editCustomerCommand;
        private ICommand _clearSearchCommand;
        private ICommand _viewDocumentCommand;
        //private ICommand _selectionChangedCommand;
        private ICommand _removeProductInstalled;
        private ICommand _addProductInstalled;
        private ICommand _searchCommand;
        private ICommand _editCompanyNameCommand;
        private ICommand _cancelEditCompanyNameCommand;
        private ICommand _viewUpdateCancelOrderCommand;
        private ICommand _searchCustomerCommand;
        private ICommand _customerChangedCommand;
        private ICommand _editContactPersonCommand;
        private ICommand _addNewContactPersonCommand;
        private ICommand _deleteContactPersonCommand;
        private ICommand _countryChangedCommand;
        private ICommand _stateChangedCommand;
        private Microsoft.Practices.Prism.Commands.DelegateCommand _closePopUpViewCommand;
        public RelayCommand CloseCommandNormal { get; private set; }
        //private ICommand _checkDataChangedCommand;

        private bool canExecute;
        private bool _creditEnabled;
             

        #endregion

        public CustomerViewModel()
        {
            LoadCountries();
            AddNewContactBtnVisibility = "Visible";
            CloseNormlVisibility = "Visible";
            IsAdminTabEnabled = false;
            customerPendingID = 0;
            BrowseTabEnabled = true;
            CloseBtnVisible = "Collapsed";
            ignoreCustomerType = false;
            canExecute = true;
            //var data = metaData.SingleOrDefault(x => x.KeyName == "version");
            //Version = data.Description;
            Categories = new ObservableCollection<Category>();
            CustomerList = new ObservableCollection<Customer>();
            DiscountStructure = new ObservableCollection<DiscountStructure>();
            SalesOrderLines = new ObservableCollection<SalesOrderLines>();
            CustomerNotes = new ObservableCollection<CustomerNote>();
            SelectedCategory = new Category();
            newCustomer = new Customer();
            newCustomer.CustomerNotes = new ObservableCollection<CustomerNote>();
            mainCategories = new List<Category>();
            ProductsInstalled = new List<ProductType>();
            ProductsInstalledView = new ObservableCollection<ProductType>();
            ProductTypeSelected = new ProductType();
            orgProductInstalled = new ObservableCollection<ProductType>();
            ContactPersons = new ObservableCollection<ContactPerson>();
            MainTabSelectedIndex = 0;
            AddCreditBackground = "#bdbdbd";
            CreditEnabled = false;
            NewCompanyNameVisibility = "Collapsed";
            SelectedCompanyNameVisibility = "Visible";
            CancelNewCustomerVisibility = "Collapsed";
            NewCustomerVisibility = "Visible";
            UpdateCommandVisibility = "Visible";
            AddCommandVisibility = "Collapsed";
            AddNewCustomerCreditVisibility = "Collapsed";
            UpdateCreditVisibility = "Visible";
            EditCompanyNameVisibility = "Collapsed";
            EditNameVisibility = "Collapsed";
            CancelEditCompanyNameVisibility = "Collapsed";
            CustomerTypeEnabled = true;
            CustomerTypeStarVisibility = "Visible";
            SearchBtnVisibility = "Visible";
            //StopCreditEnabled = false;
            IsCreditHistoryTabEnabled = true;
            IsCreditTabEnabled = true;
            IsRemoveEnabled = false;
            IsAddEnabled = false;
            IsStopCreditEnabled = false;
            IsCreditUpdateBtnEnable = false;
            CustomerBottomTabEnabled = false;
            mainCategories = DBAccess.GetCategories();
            Categories = new ObservableCollection<Category>(mainCategories);
            ContactPersonList = new List<string>();
            Suburb = new List<string>();
            SearchedCustomers = new ObservableCollection<Customer>();
            AdminNote = new AdminNote();
            

            if (UserData.UserPrivilages != null)
            {
                foreach (var item in UserData.UserPrivilages)
                {
                    if (item.Area.Trim() == "Customer_Credit_History")
                    {
                        if (item.Visibility == "Collapsed")
                        {
                            IsCreditHistoryTabEnabled = false;
                        }
                        else
                        {
                            IsCreditHistoryTabEnabled = true;
                        }
                        blockCreditHistoryTab = IsCreditHistoryTabEnabled;
                    }

                    if (item.Area.Trim() == "Customer_Credit_Details")
                    {
                        if (item.Visibility == "Collapsed")
                        {
                            IsCreditTabEnabled = false;
                        }
                        else
                        {
                            IsCreditTabEnabled = true;
                        }
                        blockCreditTab = IsCreditTabEnabled;
                    }

                    if (item.Area.Trim() == "Credit_Update_Button")
                    {
                        if (item.Visibility == "Collapsed")
                        {
                            IsCreditUpdateBtnEnable = false;
                            AddCreditBackground = "#bdbdbd";
                        }
                        else
                        {
                            IsCreditUpdateBtnEnable = true;
                            AddCreditBackground = "#666666";
                        }
                    }

                    if (item.Area.Trim() == "Credit_Add_Button")
                    {
                        if (item.Visibility == "Collapsed")
                        {
                            CreditEnabled = false;
                            AddCreditBackground = "#bdbdbd";
                            ignoreCustomerType = true;
                        }
                        else
                        {
                            CreditEnabled = true;
                            AddCreditBackground = "#666666";
                            ignoreCustomerType = false;
                        }
                    }

                    if (item.Area.Trim() == "AddToCustomerPending")
                    {
                         if (item.Visibility == "True")
                         {
                             addToCustomerPending = "True";
                         }
                         else
                         {
                             addToCustomerPending = "False";
                         }
                    }

                    if(item.Area.Trim() == "Customer->admin_notes")
                    {
                        IsAdminTabEnabled = Convert.ToBoolean(item.Visibility);
                    }                   
                }
            }
            BackgroundWorker worker = new BackgroundWorker();
            ChildWindow LoadingScreen = new ChildWindow();
            LoadingScreen.ShowWaitingScreen("Loading");

            worker.DoWork += (_, __) =>
            {

                LoadCustomerList();            
                LoadProductsInstalled();
                
            };

            worker.RunWorkerCompleted += delegate(object s, RunWorkerCompletedEventArgs args)
            {
                LoadingScreen.CloseWaitingScreen();
            };
            worker.RunWorkerAsync();
            this.CloseCommandNormal = new RelayCommand(CloseWindow);
        }

        public CustomerViewModel(CustomerPending cp)
        {
            LoadCountries();
            AddNewContactBtnVisibility = "Collapsed";
            CloseNormlVisibility = "Collapsed";
            IsAdminTabEnabled = false;
            customerPendingID = cp.CustomerId;
            BrowseTabEnabled = false;
            IsCreditHistoryTabEnabled = false;
            CloseBtnVisible = "Visible";
            ignoreCustomerType = false;
            canExecute = true;
            //var data = metaData.SingleOrDefault(x => x.KeyName == "version");
            //Version = data.Description;
            Categories = new ObservableCollection<Category>();
            CustomerList = new ObservableCollection<Customer>();
            DiscountStructure = new ObservableCollection<DiscountStructure>();
            SalesOrderLines = new ObservableCollection<SalesOrderLines>();
            CustomerNotes = new ObservableCollection<CustomerNote>();
            SelectedCategory = new Category();
            newCustomer = new Customer();
            newCustomer.CustomerNotes = new ObservableCollection<CustomerNote>();
            mainCategories = new List<Category>();
            ProductsInstalled = new List<ProductType>();
            ProductsInstalledView = new ObservableCollection<ProductType>();
            ProductTypeSelected = new ProductType();
            orgProductInstalled = new ObservableCollection<ProductType>();
            MainTabSelectedIndex = 0;
            AddCreditBackground = "#bdbdbd";
            CreditEnabled = false;
            NewCompanyNameVisibility = "Collapsed";
            SelectedCompanyNameVisibility = "Visible";
            CancelNewCustomerVisibility = "Collapsed";
            NewCustomerVisibility = "Visible";
            UpdateCommandVisibility = "Visible";
            AddCommandVisibility = "Collapsed";
            AddNewCustomerCreditVisibility = "Collapsed";
            UpdateCreditVisibility = "Visible";
            EditCompanyNameVisibility = "Collapsed";
            EditNameVisibility = "Collapsed";
            CancelEditCompanyNameVisibility = "Collapsed";
            CustomerTypeEnabled = true;
            CustomerTypeStarVisibility = "Visible";
            SearchBtnVisibility = "Visible";
            
            //StopCreditEnabled = false;
            //IsCreditHistoryTabEnabled = true;
            IsCreditTabEnabled = true;
            IsRemoveEnabled = false;
            IsAddEnabled = false;
            IsStopCreditEnabled = false;
            IsCreditUpdateBtnEnable = false;
            CustomerBottomTabEnabled = false;
            mainCategories = DBAccess.GetCategories();
            Categories = new ObservableCollection<Category>(mainCategories);
            ContactPersonList = new List<string>();
            Suburb = new List<string>();
            SearchedCustomers = new ObservableCollection<Customer>();
            AdminNote = new AdminNote();

            if (UserData.UserPrivilages != null)
            {
                foreach (var item in UserData.UserPrivilages)
                {
                    if (!String.IsNullOrWhiteSpace(CustomerType) && (CustomerType == "Prepaid" || CustomerType == "Account"))
                    {

                        if (item.Area.Trim() == "Customer_Credit_History")
                        {
                            if (item.Visibility == "Collapsed")
                            {
                                IsCreditHistoryTabEnabled = false;
                            }
                            else
                            {
                                IsCreditHistoryTabEnabled = true;
                            }
                            blockCreditHistoryTab = IsCreditHistoryTabEnabled;
                        }

                        if (item.Area.Trim() == "Customer_Credit_Details")
                        {
                            if (item.Visibility == "Collapsed")
                            {
                                IsCreditTabEnabled = false;
                            }
                            else
                            {
                                IsCreditTabEnabled = true;
                            }
                            blockCreditTab = IsCreditTabEnabled;
                        }

                        if (item.Area.Trim() == "Credit_Update_Button")
                        {
                            if (item.Visibility == "Collapsed")
                            {
                                IsCreditUpdateBtnEnable = false;
                                AddCreditBackground = "#bdbdbd";
                            }
                            else
                            {
                                IsCreditUpdateBtnEnable = true;
                                AddCreditBackground = "#666666";
                            }
                        }

                        if (item.Area.Trim() == "Credit_Add_Button")
                        {
                            if (item.Visibility == "Collapsed")
                            {
                                CreditEnabled = false;
                                AddCreditBackground = "#bdbdbd";
                                ignoreCustomerType = true;
                            }
                            else
                            {
                                CreditEnabled = true;
                                AddCreditBackground = "#666666";
                                ignoreCustomerType = false;
                            }
                        }
                    }
                    else
                    {
                        IsCreditTabEnabled = false;
                        IsCreditHistoryTabEnabled = false;
                    }

                    if (item.Area.Trim() == "AddToCustomerPending")
                    {
                        if (item.Visibility == "True")
                        {
                            addToCustomerPending = "True";
                        }
                        else
                        {
                            addToCustomerPending = "False";
                        }
                    }

                    if (item.Area.Trim() == "Customer->admin_notes")
                    {
                        IsAdminTabEnabled = Convert.ToBoolean(item.Visibility);
                    }  
                }
            }
            if (cp == null)
            {
                BackgroundWorker worker = new BackgroundWorker();
                ChildWindow LoadingScreen = new ChildWindow();
                LoadingScreen.ShowWaitingScreen("Loading");

                worker.DoWork += (_, __) =>
                {
                   LoadCustomerList();
                    //LoadCountries();
                };

                worker.RunWorkerCompleted += delegate(object s, RunWorkerCompletedEventArgs args)
                {
                    LoadingScreen.CloseWaitingScreen();
                };
                worker.RunWorkerAsync();
            }
            else
            {
                LoadProductsInstalled();
                //LoadCountries();

                AddNewCustomer();
                NewCompanyName = cp.CompanyName;
                CompanyAddress = cp.CompanyAddress;
                CompanyCity = cp.CompanyCity;
                SelectedCustomerState = cp.CompanyState;
                CompanyPostCode = cp.CompanyPostCode;
                //SelectedCountry = new Country();
                SelectedCountry = cp.CompanyCountry;
                CompanyEmail = cp.CompanyEmail;
                CompanyTelephone = cp.CompanyTelephone;
                CompanyFax = cp.CompanyFax;
                SelectedCategory = cp.PrimaryBusiness;
                ShipAddress = cp.ShipAddress;
                ShipCity = cp.ShipCity;
                ShipState = cp.ShipState;
                ShipPostCode = cp.ShipPostCode;
                ShipCountry = cp.ShipCountry;
                FirstName1 = cp.FirstName1;
                LastName1 = cp.LastName1;
                Telephone1 = cp.Telephone1;
                Mobile1 = cp.Mobile1;
                Fax1 = cp.Fax1;
                Email1 = cp.Email1;

                FirstName2 = cp.FirstName2;
                LastName2 = cp.LastName2;
                Telephone2 = cp.Telephone2;
                Mobile2 = cp.Mobile2;
                Fax2 = cp.Fax2;
                Email2 = cp.Email2;

                FirstName3 = cp.FirstName3;
                LastName3 = cp.LastName3;
                Telephone3 = cp.Telephone3;
                Mobile3 = cp.Mobile3;
                Fax3 = cp.Fax3;
                Email3 = cp.Email3;

                /*****************Fix Discounts*****************/
                string[] disSplit = cp.DiscountStr.Split('|');
                foreach (var item in disSplit)
                {
                    string[] str = item.Split(':');
                    if (!String.IsNullOrWhiteSpace(str[0]))
                    {
                        for (int i = 0; i < DiscountStructure.Count; i++)
                        {

                            if (str[0] == DiscountStructure[i].Category.CategoryName && Convert.ToInt16(str[1]) > 0)
                            {
                                DiscountStructure[i].Discount = Convert.ToInt16(str[1]);

                               // DiscountStructure.Add(new DiscountStructure() { Category = new Category() { CategoryName = str[0] }, Discount = Convert.ToInt16(str[1]) });
                            }
                        }
                       
                    }
                }
                

                /***************Fix Product Types**************/
                string[] prodTypesSplit = cp.ProductTypeStr.Split('|');
                foreach (var item in prodTypesSplit)
                {
                    if (!String.IsNullOrWhiteSpace(item))
                    {
                        var xx = ProductsInstalled.SingleOrDefault(x => x.ProductTypeID == Convert.ToInt16(item));
                        if (xx != null)
                        {
                            ProductsInstalledView.Add(xx);
                        }
                    }
                }
                /***************Fix Customer Notes*************/
                string[] cusNotesSplit = cp.CustomerNoteStr.Split('|');
                foreach (var item in cusNotesSplit)
                {
                    if (!String.IsNullOrWhiteSpace(item))
                    {
                        CustomerNotes.Add(new CustomerNote() { DisplayString=item});
                    }
                }
            }
            if (cp != null)
            {
                LastUpdatedString = DBAccess.GetCustomerLatestDateTime(cp.CustomerId);
            }
            _closePopUpViewCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(CloseForm);
           
        }


        private void LoadCountries()
        {
            CountryList = new List<Country>();
            CountryList = DBAccess.GetAllCountries();
            //CountryList.Insert(0,new Country() { ID=0, CountryName = "Select",IsEnabled=true});
            //SelectedCountry = new Country();
            SelectedCountry = "Select";
        }
        
        private void ProcessPrivilages()
        {
            if (UserData.UserPrivilages != null)
            {
                foreach (var item in UserData.UserPrivilages)
                {
                    if (!String.IsNullOrWhiteSpace(CustomerType) && (CustomerType == "Prepaid" || CustomerType == "Account"))
                    {

                        if (item.Area.Trim() == "Customer_Credit_History")
                        {
                            if (item.Visibility == "Collapsed")
                            {
                                IsCreditHistoryTabEnabled = false;
                            }
                            else
                            {
                                IsCreditHistoryTabEnabled = true;
                            }
                            blockCreditHistoryTab = IsCreditHistoryTabEnabled;
                        }

                        if (item.Area.Trim() == "Customer_Credit_Details")
                        {
                            if (item.Visibility == "Collapsed" || CustomerType == "Prepaid")
                            {
                                IsCreditTabEnabled = false;
                            }
                            else
                            {
                                IsCreditTabEnabled = true;
                            }
                            //blockCreditTab = IsCreditTabEnabled;
                        }

                        if (item.Area.Trim() == "Credit_Update_Button")
                        {
                            if (item.Visibility == "Collapsed")
                            {
                                AddCreditBackground = "#bdbdbd";
                                CreditEnabled = false;
                                IsCreditHistoryTabEnabled = false;
                                BottomTabSelectedIndex = 0;
                                IsStopCreditEnabled = false;
                                IsCreditUpdateBtnEnable = false;
                            }
                            else
                            {
                                if (CustomerType == "Account" && CancelNewCustomerVisibility == "Collapsed")
                                {
                                    IsStopCreditEnabled = true;
                                    IsCreditUpdateBtnEnable = true;

                                    //if (blockCreditTab)
                                    //{
                                    AddCreditBackground = "#666666";
                                    CreditEnabled = true;
                                    //}

                                    if (blockCreditHistoryTab)
                                    {
                                        IsCreditHistoryTabEnabled = true;
                                    }

                                }
                                else if (CustomerType == "Account" && CancelNewCustomerVisibility == "Visible")
                                {
                                    IsCreditUpdateBtnEnable = true;
                                    IsStopCreditEnabled = true;
                                    AddCreditBackground = "#666666";
                                    CreditEnabled = true;
                                    
                                }
                                else if (CustomerType == "Prepaid")
                                {
                                    AddCreditBackground = "#bdbdbd";
                                    CreditEnabled = false;
                                    IsCreditHistoryTabEnabled = false;
                                    BottomTabSelectedIndex = 0;
                                    IsStopCreditEnabled = false;
                                    IsCreditUpdateBtnEnable = false;
                                    BottomTabSelectedIndex = 1;
                                }
                            }
                        }

                        if (item.Area.Trim() == "Credit_Add_Button")
                        {
                            if (item.Visibility == "Collapsed")
                            {
                                AddCreditBackground = "#bdbdbd";
                                CreditEnabled = false;
                                IsCreditHistoryTabEnabled = false;
                                BottomTabSelectedIndex = 0;
                                IsStopCreditEnabled = false;
                                IsCreditUpdateBtnEnable = false;
                            }
                            else
                            {
                                if (CustomerType == "Account" && CancelNewCustomerVisibility == "Collapsed")
                                {
                                    IsStopCreditEnabled = true;
                                    IsCreditUpdateBtnEnable = true;
                                    //if (blockCreditTab)
                                    //{
                                    AddCreditBackground = "#666666";
                                    CreditEnabled = true;
                                     

                                    if (blockCreditHistoryTab)
                                    {
                                        IsCreditHistoryTabEnabled = true;
                                    }

                                }
                                else if (CustomerType == "Account" && CancelNewCustomerVisibility == "Visible")
                                {
                                    IsCreditUpdateBtnEnable = true;
                                    IsStopCreditEnabled = true;
                                    AddCreditBackground = "#666666";
                                    CreditEnabled = true;
                                   
                                }
                                else if (CustomerType == "Prepaid")
                                {
                                    AddCreditBackground = "#bdbdbd";
                                    CreditEnabled = false;
                                    IsCreditHistoryTabEnabled = false;
                                    BottomTabSelectedIndex = 0;
                                    IsStopCreditEnabled = false;
                                    IsCreditUpdateBtnEnable = false;
                                    BottomTabSelectedIndex = 1;
                                }
                            }
                        }

                    }
                    else
                    {
                        IsCreditTabEnabled = false;
                    }
                }                
            }
        }



        public string Title
        {
            get
            {
                return "Customer";
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

        public bool CanClose
        {
            get
            {
                return this.IsDirty == false || MessageBox.Show("Changes were made. Do you want to close this window?", "Close", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
            }
        }

        private void CloseForm()
        {
            if (Closed != null)
            {
                Closed(0);
            }
        }

        

        private void DiscountChanged()
        {
            //foreach (var item in DiscountStructure)
            //{
            //    if (item.Category.CategoryID == 3)
            //    {
            //        if (item.Discount == 55)
            //        {
            //            item.DiscountLabelVisibility = "Visible";
            //        }
            //        else if (item.Discount == 53)
            //        {
            //            item.DiscountLabelVisibility = "Visible";
            //        }
            //        else
            //        {
            //            item.DiscountLabelVisibility = "Collapsed";
            //        }
            //    }
            //    else
            //    {
            //        item.DiscountLabelVisibility = "Collapsed";
            //    }
            //}
        }

        private void LoadProductsInstalled()
        {
            ProductsInstalled = DBAccess.GetProductTypes();
            ProductsInstalled.Add(new ProductType() { ProductTypeID = 0, Type = "Select" });
            SelectedProductsInstalled = new ProductType() { ProductTypeID = 0, Type = "Select" };
        }

        private void GetProductsInstalled()
        {
            List<string> tempSuburb = new List<string>();
            //List<string> tempCustomerName = new List<string>();
            List<ProductInstalled> prodInsTemp = new List<ProductInstalled>();
            prodInsTemp = DBAccess.GetProductsInstalled();

            foreach (var item in CustomerList)
            {
                //if (!String.IsNullOrWhiteSpace(item.FirstName1))
                //{
                //    bool x = tempCustomerName.Any(z => z == item.FirstName1);
                //    if (x == false)
                //    {
                //        tempCustomerName.Add(item.FirstName1);
                //    }
                //}

                if (!String.IsNullOrWhiteSpace(item.CompanyCity))
                {
                    bool y = tempSuburb.Any(x => x == item.CompanyCity);
                    if (y == false)
                    {
                        tempSuburb.Add(item.CompanyCity);
                    }
                }

                item.ProductsInstalled = new ObservableCollection<ProductType>();
                foreach (var items in prodInsTemp)
                {
                    if (item.CustomerId == items.CustomerID)
                    {
                        item.ProductsInstalled.Add(new ProductType() { Type = items.ProductType[0].Type, ProductTypeID = items.ProductType[0].ProductTypeID });
                    }
                }
            }

            //CustomerName = tempCustomerName.OrderBy(z => z).ToList();
            Suburb = tempSuburb.OrderBy(x => x).ToList();

        }
        private void LoadCustomerList()
        {
            //BackgroundWorker worker = new BackgroundWorker();
            //ChildWindow LoadingScreen = new ChildWindow();
            //LoadingScreen.ShowWaitingScreen("Loading");

            //worker.DoWork += (_, __) =>
            //{
                ObservableCollection<Customer> tempList = DBAccess.GetAllCustomers();
                CustomerList = new ObservableCollection<Customer>(tempList);
                GetProductsInstalled();

            //};

            //worker.RunWorkerCompleted += delegate(object s, RunWorkerCompletedEventArgs args)
            //{
            //    LoadingScreen.CloseWaitingScreen();
            //};
            //worker.RunWorkerAsync();
        }

        private void ConsolidateDiscount()
        {
            if (SelectedCustomer != null)
            {
                if (SelectedCustomer.DiscountStructure != null && SelectedCustomer.DiscountStructure.Count == 0)
                {
                    DiscountStructure = SelectedCustomer.DiscountStructure;
                }

                DiscountStructure = new ObservableCollection<DiscountStructure>();
                int c = Categories.Count - 2;

                foreach (var item in Categories)
                {
                    if (item.CategoryID != 8 && item.CategoryID != 9)
                    {
                        var z = SelectedCustomer.DiscountStructure.FirstOrDefault(x => x.Category.CategoryID == item.CategoryID);

                        if (z != null)
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                DiscountStructure.Add(new DiscountStructure()
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
                                    DiscountLabelVisibility = z.DiscountLabelVisibility,
                                    TimeStamp = z.TimeStamp
                                });
                            });
                        }
                        else
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                DiscountStructure.Add(new DiscountStructure()
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
                                    Discount = 0,
                                    DiscountLabelVisibility = "Collapsed"
                                });
                            });
                        }

                    }
                }

                if (CustomerType == "Account")
                {
                    LoadCustomerCreditHistory();
                }
                else
                {
                    IsStopCredit = false;
                    if (CustomerCreditActivity != null)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            CustomerCreditActivity.Clear();
                        });
                    }
                }
            }
        }

        #region UPDATE_CUSTOMER
        private void UpdateCustomer()
        {
            if (SelectedCustomer == null)
            {
                MessageBox.Show("Please select customer to update", "Invalid Customer", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
            else if (string.IsNullOrWhiteSpace(CustomerType) || CustomerType == "Select")
            {
                MessageBox.Show("Please select customer type", "Customer Type Required", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
            else if (string.IsNullOrWhiteSpace(SelectedCustomerState))
            {
                MessageBox.Show("Please select state", "State Required", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
            else if (string.IsNullOrWhiteSpace(SelectedCategory.CategoryName))
            {
                MessageBox.Show("Please select primary business", "Primary Business Required", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
            else if(string.IsNullOrWhiteSpace(SelectedCountry) || SelectedCountry.Equals("Select") )
            {
                MessageBox.Show("Please select country", "Country Required", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
            else
            {
                //Check if customer needs to be modified
                if (!string.IsNullOrWhiteSpace(EditCompanyName) && !EditCompanyName.Equals(SelectedCustomer.CompanyName))
                {
                    SelectedCustomer.CompanyName = EditCompanyName;
                }
                SelectedCustomer.CustomerType = CustomerType;
                SelectedCustomer.CompanyAddress = CompanyAddress;
                SelectedCustomer.CompanyCity = CompanyCity;
                SelectedCustomer.CompanyState = SelectedCustomerState;
                SelectedCustomer.CompanyCountry = SelectedCountry ;
                SelectedCustomer.CompanyPostCode = CompanyPostCode;
                SelectedCustomer.CompanyEmail = CompanyEmail;
                SelectedCustomer.CompanyTelephone = CompanyTelephone;
                SelectedCustomer.CompanyFax = CompanyFax;
                SelectedCustomer.ShipAddress = ShipAddress;
                SelectedCustomer.ShipCity = ShipCity;
                SelectedCustomer.ShipState = ShipState;
                SelectedCustomer.ShipPostCode = ShipPostCode;
                SelectedCustomer.ShipCountry = ShipCountry;
                SelectedCustomer.Designation1 = Designation1;
                SelectedCustomer.FirstName1 = FirstName1;
                SelectedCustomer.LastName1 = LastName1;
                SelectedCustomer.Telephone1 = Telephone1;
                SelectedCustomer.Mobile1 = Mobile1;
                SelectedCustomer.Fax1 = Fax1;
                SelectedCustomer.Email1 = Email1;
                SelectedCustomer.Designation2 = Designation2;
                SelectedCustomer.FirstName2 = FirstName2;
                SelectedCustomer.LastName2 = LastName2;
                SelectedCustomer.Telephone2 = Telephone2;
                SelectedCustomer.Mobile2 = Mobile2;
                SelectedCustomer.Fax2 = Fax2;
                SelectedCustomer.Email2 = Email2;
                SelectedCustomer.Designation3 = Designation3;
                SelectedCustomer.FirstName3 = FirstName3;
                SelectedCustomer.LastName3 = LastName3;
                SelectedCustomer.Telephone3 = Telephone3;
                SelectedCustomer.Mobile3 = Mobile3;
                SelectedCustomer.Fax3 = Fax3;
                SelectedCustomer.Email3 = Email3;
                SelectedCustomer.Active = Active;
                SelectedCustomer.DiscountStructure = DiscountStructure;
                SelectedCustomer.PrimaryBusiness = SelectedCategory;
                SelectedCustomer.ProductsInstalled = ProductsInstalledView;
                SelectedCustomer.StopCredit = IsStopCredit == true ? "True" : "False";

                if (AdminNote != null)
                {
                    AdminNote.CustomerID = SelectedCustomer.CustomerId;
                    AdminNote.Area = "Customer";
                    AdminNote.CreatedBy = UserData.FirstName + " " + UserData.LastName;
                    AdminNote.CreatedDate = DateTime.Now;
                    AdminNote.Active = true;
                }
                Tuple<int, string, string, Customer> res = DBAccess.UpdateCustomerDB(SelectedCustomer, UserData.FirstName + " " + UserData.LastName, AdminNote);

                if (res.Item1 == 1)
                {
                    MessageBox.Show("Customer updated successfully", "Customer Updated", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else if (res.Item1 == -1)
                {
                    MessageBox.Show("Data has been changed since you opened this form!!!" + System.Environment.NewLine + "Data will be refreshed", "Data Changed", MessageBoxButton.OK, MessageBoxImage.Stop);
                }
                else
                {
                    MessageBox.Show("You haven't made any changes to update", "No Changes Were Made", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                if (res.Item1 == 1 || res.Item1 == -1)
                {
                    SelectedCompanyNameVisibility = "Visible";
                    EditNameVisibility = "Collapsed";
                    CancelEditCompanyNameVisibility = "Collapsed";
                    SearchBtnVisibility = "Visible";

                    LoadCustomerList();
                    SelectedCustomer = res.Item4;
                    SelectedCustomer.CompanyName = res.Item4.CompanyName;

                    BackgroundWorker worker = new BackgroundWorker();
                    ChildWindow LoadingScreen = new ChildWindow();
                    LoadingScreen.ShowWaitingScreen("Loading");

                    worker.DoWork += (_, __) =>
                    {
                        
                        SearchCustomerInfo();
                    };

                    worker.RunWorkerCompleted += delegate(object s, RunWorkerCompletedEventArgs args)
                    {
                        LoadingScreen.CloseWaitingScreen();
                    };
                    worker.RunWorkerAsync();
                }
            }
        }

        #endregion

        private void ViewCustomerCreditForm()
        {
            if (SelectedCustomer != null)
            {
                var childWindow = new ChildWindow();
                childWindow.customerCreditForm_Closed += (r =>
                {
                    if (r.CustomerId > 0)
                    {
                        //Msg.Show("Customer credit updated!!!", "Credit Updated", MsgBoxButtons.OK, MsgBoxImage.OK);
                        //LoadCustomerInfo();
                        CreditLimit = r.CreditLimit;
                        CreditRemaining = r.CreditRemaining;
                        CreditOwed = r.CreditOwed;
                        Debt = r.Debt;
                        LastUpdatedString = DBAccess.GetCustomerLatestDateTime(r.CustomerId);


                        //var yes = Global.Items.SingleOrDefault(x => x.Title == "Customer");
                        //if (yes != null)
                        //{
                        //    for (int i = 0; i < Global.Items.Count; i++)
                        //    {
                        //        if (Global.Items[i].Title == "Customer")
                        //        {
                        //            Global.Items[i] = yes;                                   
                        //        }
                        //    }
                        //}
                        //else
                        //{
                        //    var item = new CustomerViewModel();
                        //    item.Closing += (s, e) => Global.Items.Remove(item);
                        //    Global.Items.Add(item);
                        //}  
                        
            
                    }
                });

                childWindow.ShowAddCustomerCredit(SelectedCustomer);

                
            }
            else
            {
                MessageBox.Show("Please select customer to add credit", "Select Customer", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void LoadCreditInfo()
        {
            LastUpdatedString = DBAccess.GetCustomerLatestDateTime(SelectedCustomer.CustomerId);
            Tuple<decimal, string> ele = DBAccess.GetCustomerOrderInfoByCustomerID(SelectedCustomer.CustomerId);
            LastOrderDate = ele.Item2;
            TotalReleased = ele.Item1;
        }

        private void LoadCustomerInfo()
        {
            OldCustomerInfo c = new OldCustomerInfo();
            c.OldCustomer = SelectedCustomer;
            SelectedCustomer = null;
            LoadCustomerList();
            SelectedCustomer = CustomerList.SingleOrDefault(x => x.CustomerId == c.OldCustomer.CustomerId);
            ConsolidateDiscount();
        }

        #region CLEAR_FIELDS
        private void Clear()
        {
            
            NewCompanyName = String.Empty;
            CustomerType = "";
            CompanyAddress = string.Empty;
            CompanyCity = string.Empty;
            CompanyState = string.Empty;
            CompanyPostCode = string.Empty;
            CompanyEmail = string.Empty;
            CompanyTelephone = string.Empty;
            CompanyFax = string.Empty;
            CreditLimit = 0;
            CreditRemaining = 0;
            ShipAddress = string.Empty;
            ShipCity = string.Empty;
            ShipState = string.Empty;
            ShipPostCode = string.Empty;
            ShipCountry = string.Empty;
            Designation1 = string.Empty;
            FirstName1 = string.Empty;
            LastName1 = string.Empty;
            Telephone1 = string.Empty;
            Mobile1 = string.Empty;
            Fax1 = string.Empty;
            Email1 = string.Empty;
            Designation2 = string.Empty;
            FirstName2 = string.Empty;
            LastName2 = string.Empty;
            Telephone2 = string.Empty;
            Mobile2 = string.Empty;
            Fax2 = string.Empty;
            Email2 = string.Empty;
            Designation3 = string.Empty;
            FirstName3 = string.Empty;
            LastName3 = string.Empty;
            Telephone3 = string.Empty;
            Mobile3 = string.Empty;
            Fax3 = string.Empty;
            Email3 = string.Empty;
            CreditRemaining = 0;
            CreditLimit = 0;
            Debt = 0;
            TotalReleased = 0;
            LastUpdatedString = string.Empty;
            IsStopCredit = false;
            CustomerNote = string.Empty;
            SelectedProductsInstalled = new ProductType() { ProductTypeID = 0, Type = "Select" };
            ProductsInstalledView = new ObservableCollection<ProductType>();
            CustomerNotes = new ObservableCollection<CustomerNote>();
            SelectedCustomerState = "";
            AdminNote = new AdminNote();
            //AddNewContactBtnVisibility = "Collapsed";
            if (ContactPersons != null)
            {
                ContactPersons.Clear();
            }
            SalesOrderLines.Clear();

            Categories = new ObservableCollection<Category>();
            SelectedCategory = new Category();
            Categories = new ObservableCollection<Category>( mainCategories);

            if (NewCompanyNameVisibility == "Visible")
            {
                DiscountStructure = new ObservableCollection<DiscountStructure>();

                foreach (var item in Categories)
                {
                    if (item.CategoryID != 8 && item.CategoryID != 9)
                    {
                        DiscountStructure.Add(new DiscountStructure() { CustomerID = 0, Category = new Category() { CategoryID = item.CategoryID, CategoryName = item.CategoryName, CategoryDescription = item.CategoryDescription, Discounts = item.Discounts }, UpdatedBy = UserData.UserName, UpdatedDate = DateTime.Now, Discount = 0, DiscountLabelVisibility = "Collapsed" });
                    }
                }
                //DiscountStructure = DiscountStructure.OrderBy(x => x.Category.CategoryName).ToList();                
            }
            else
            {
                DiscountStructure.Clear();
            }

            //SelectedCountry = new Country();
            //SelectedCountry = "Select";
        }

        #endregion

        private void AppendExclusiveDiscount()
        {
            if (SelectedCustomer != null && UserData.MetaData != null && Categories != null)
            {
                var data = UserData.MetaData.FirstOrDefault(x => x.KeyName.Equals("customer_exclusive_discount"));
                if (data != null)
                {
                    string[] str1 = data.Description.Split('|');//"1840[2:55]|4871[3:65]|563[3:65]|539[2:55,11:45]".Split('|');
                    if (str1 != null)
                    {
                        foreach (var item in str1)
                        {
                            string[] str2 = item.Split('[');
                            if (str2 != null && int.Parse(str2[0]) == SelectedCustomer.CustomerId)
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
            }
        }

        private void RemoveExclusiveDiscount()
        {
            mainCategories = DBAccess.GetCategories();
            Categories = new ObservableCollection<Category>(mainCategories);
        }

        private void AddNewCustomer()
        {

            if (UserData.UserPrivilages != null)
            {
                foreach (var item in UserData.UserPrivilages)
                {
                    if (item.Area.Trim() == "Customer->customer_type")
                    {
                        if (item.Visibility == "Collapsed")
                        {
                            CustomerTypeEnabled = false;
                            CustomerTypeStarVisibility = item.Visibility;
                        }
                        else
                        {
                            CustomerTypeEnabled = true;
                            CustomerTypeStarVisibility = item.Visibility;
                        }
                    }

                }
            }
            RemoveExclusiveDiscount();
            SearchBtnVisibility = "Collapsed";
            CustomerBottomTabEnabled = true;
            SelectedCustomer = null;
            NewCompanyNameVisibility = "Collapsed";
            SelectedCompanyNameVisibility = "Collapsed";
            LastUpdatedString = string.Empty;
            CancelNewCustomerVisibility = "Visible";
            if (BrowseTabEnabled==false)
            {
                CancelNewCustomerVisibility = "Collapsed";
            }
            CancelEditCompanyNameVisibility = "Collapsed";
            NewCustomerVisibility = "Collapsed";
            NewCompanyNameVisibility = "Visible";
            UpdateCommandVisibility = "Collapsed";
            AddCommandVisibility = "Visible";
            AddNewCustomerCreditVisibility = "Visible";
            UpdateCreditVisibility = "Collapsed";
            BottomTabSelectedIndex = 0;
            EditCompanyNameVisibility = "Collapsed";
            //AddNewContactBtnVisibility = "Collapsed";
            EditCompanyName = string.Empty;
            if (ContactPersons != null)
            {
                ContactPersons.Clear();
            }
            Active = true;
            Clear();
            CustomerNotes = new ObservableCollection<CustomerNote>();
            SalesOrderLines = new ObservableCollection<SalesOrderLines>();
            ProductsInstalledView = new ObservableCollection<ProductType>();
        }

        private void CountryChanged()
        {
            if (!SelectedCountry.Equals("Australia") && !SelectedCountry.Equals("Select") && !SelectedCountry.Equals("-----------------------"))
            {
                SelectedCustomerState = "OTHER";
                //if (string.IsNullOrWhiteSpace(SelectedCountry) && (!SelectedCustomerState.Equals("OTHER") || !SelectedCustomerState.Equals("Select")))
                //{
                //    SelectedCountry = "Australia";
                //}                
            }
            else
            {
                if (SelectedCustomerState.Equals("OTHER") || SelectedCustomerState.Equals("Select"))
                {
                    SelectedCustomerState = "Select";
                }
            }
            //else if (SelectedCountry.Equals("Australia"))
            //{
            //    if (SelectedCustomerState.Equals("Select") || SelectedCustomerState.Equals("OTHER"))
            //    {
            //        SelectedCustomerState = "Select";
            //    }
            //}         

        }

        private void StateChanged()
        {
            if (SelectedCustomerState.Equals("OTHER") || SelectedCustomerState.Equals("Select"))
            {
                if (SelectedCountry.Equals("Australia"))
                {
                    SelectedCountry = "Select";
                }
            }
            else
            {
                if (!SelectedCountry.Equals("Australia"))
                {
                    SelectedCountry = "Australia";
                }
            }
        }

        private void CancelNewCustomer()
        {
            SearchBtnVisibility = "Visible";
            CustomerBottomTabEnabled = false ;
            CustomerTypeEnabled = true;
            CustomerTypeStarVisibility = "Visible";
            SelectedCompanyNameVisibility = "Visible";
            CancelNewCustomerVisibility = "Collapsed";
            CancelEditCompanyNameVisibility = "Collapsed";
            NewCustomerVisibility = "Visible";
            NewCompanyNameVisibility = "Collapsed";
            UpdateCommandVisibility = "Visible";
            AddCommandVisibility = "Collapsed";
            AddNewCustomerCreditVisibility = "Collapsed";
            UpdateCreditVisibility = "Visible";
            SelectedCustomer = null;
            EditCompanyNameVisibility = "Collapsed";
            EditCompanyName = string.Empty;
            //AddNewContactBtnVisibility = "Visible";
            
            Active = false;
            Clear();
            ProductsInstalledView = new ObservableCollection<ProductType>();
        }

        #region ADD_NEW_CUSTOMER_DB
        private void AddNewCustomerDB()
        {
            if (string.IsNullOrWhiteSpace(NewCompanyName))
            {
                MessageBox.Show("Company name required", "Company Name Required", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (ignoreCustomerType ==false && (string.IsNullOrWhiteSpace(CustomerType) || CustomerType == "Select"))
            {
                MessageBox.Show("Please select customer type", "Customer Type Required", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (string.IsNullOrWhiteSpace(SelectedCustomerState))
            {
                MessageBox.Show("Please select state", "State Required", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (SelectedCategory == null || string.IsNullOrWhiteSpace(SelectedCategory.CategoryName))
            {
                MessageBox.Show("Please select primary business", "Primary Business Required", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                newCustomer.CompanyName = NewCompanyName;
                newCustomer.CustomerType = CustomerType;
                newCustomer.CompanyAddress = CompanyAddress;
                newCustomer.CompanyCity = CompanyCity;
                newCustomer.CompanyState = SelectedCustomerState;
                newCustomer.CompanyCountry = SelectedCountry;
                newCustomer.CompanyPostCode = CompanyPostCode;
                newCustomer.CompanyEmail = CompanyEmail;
                newCustomer.CompanyTelephone = CompanyTelephone;
                newCustomer.CompanyFax = CompanyFax;
                newCustomer.ShipAddress = ShipAddress;
                newCustomer.ShipCity = ShipCity;
                newCustomer.ShipState = ShipState;
                newCustomer.ShipPostCode = ShipPostCode;
                newCustomer.ShipCountry = ShipCountry;
                newCustomer.Designation1 = Designation1;
                newCustomer.FirstName1 = FirstName1;
                newCustomer.LastName1 = LastName1;
                newCustomer.Telephone1 = Telephone1;
                newCustomer.Mobile1 = Mobile1;
                newCustomer.Fax1 = Fax1;
                newCustomer.Email1 = Email1;

                newCustomer.Designation2 = Designation2;
                newCustomer.FirstName2 = FirstName2;
                newCustomer.LastName2 = LastName2;
                newCustomer.Telephone2 = Telephone2;
                newCustomer.Mobile2 = Mobile2;
                newCustomer.Fax2 = Fax2;
                newCustomer.Email2 = Email2;

                newCustomer.Designation3 = Designation3;
                newCustomer.FirstName3 = FirstName3;
                newCustomer.LastName3 = LastName3;
                newCustomer.Telephone3 = Telephone3;
                newCustomer.Mobile3 = Mobile3;
                newCustomer.Fax3 = Fax3;
                newCustomer.Email3 = Email3;

                newCustomer.CreditLimit = CreditLimit;
                newCustomer.CreditRemaining = CreditRemaining;
                newCustomer.Debt = Debt;

                newCustomer.Active = true;
                newCustomer.LastUpdatedBy = UserData.FirstName + " " + UserData.LastName ;
                newCustomer.LastUpdatedDateTime = DateTime.Now;
                newCustomer.DiscountStructure = DiscountStructure;
                newCustomer.StopCredit = IsStopCredit == true ? "true" : "false";
                newCustomer.PrimaryBusiness = SelectedCategory;
                newCustomer.ProductsInstalled = ProductsInstalledView;

                newCustomer.ContactPerson = new List<ContactPerson>();
                if (ContactPersons != null && ContactPersons.Count > 0)
                {
                    newCustomer.ContactPerson = new List<ContactPerson>(ContactPersons);
                }

                string disString = string.Empty;
                string prodTypesStr = string.Empty;
                string customerNoteStr = string.Empty;

                foreach (var item in DiscountStructure)
                {
                    disString += "|" + item.Category.CategoryName + ":" + item.Discount;
                }

                foreach (var item in ProductsInstalledView)
                {
                    prodTypesStr += "|" + item.ProductTypeID;
                }

                foreach (var item in newCustomer.CustomerNotes)
                {
                    //customerNoteStr += "|<CB:"+item.CreatedBy + "><DT" + item.DisplayString + item.Comment;
                    customerNoteStr += "|" + item.DisplayString;
                }

                if (AdminNote != null)
                {
                    AdminNote.CustomerID = 0;
                    AdminNote.Area = "Customer";
                    AdminNote.CreatedBy = UserData.FirstName + " " + UserData.LastName;
                    AdminNote.CreatedDate = DateTime.Now;
                    AdminNote.Active = true;
                }


                int res = DBAccess.AddNewCustomer(newCustomer, addToCustomerPending, disString, prodTypesStr, customerNoteStr, customerPendingID, AdminNote);

                if (res == -2)
                {
                    MessageBox.Show("Name with the customer '" + NewCompanyName + "' exists in the database" + System.Environment.NewLine + "Please choose another name", "Customer Exists", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else if (res == 0)
                {
                    MessageBox.Show("There has been a problem and the new customer did not add to the database", "Problem Occured", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else if(res > 0)
                {
                    MessageBox.Show("Customer added successfully to the database!!!", "Customer Added", MessageBoxButton.OK, MessageBoxImage.Information);
                    CancelNewCustomer();
                    Clear();                    
                }
                else
                {
                    MessageBox.Show("Customer added successfully!!!", "Customer Added", MessageBoxButton.OK, MessageBoxImage.Information);

                    if (customerPendingID == 0)
                    {
                        CancelNewCustomer();
                        Clear();
                        LoadCustomerList();
                    }
                    else
                    {
                        if (Closed != null)
                        {
                            Closed(1);
                        }
                    }
                }
            }

        }
        #endregion

        private void ViewAddNewCustomer()
        {
            if (!string.IsNullOrWhiteSpace(NewCompanyName) || SelectedCustomer != null)
            {
                Customer c = new Customer();
                c.CustomerId = 0;
                c.CompanyName = NewCompanyName;
                

                if (SelectedCustomer != null && string.IsNullOrWhiteSpace(NewCompanyName))
                {
                    c = SelectedCustomer;
                }                
                
                if(BrowseTabEnabled == false)
                {   

                    AddCustomerCreditNoOverlayView window = new AddCustomerCreditNoOverlayView(c);
                    window.ShowDialog();

                    CreditLimit = Global.creditLimit;
                    Debt = 0;
                    CreditRemaining = Global.creditRemaining;
                }
                else
                {
                    var childWindow = new ChildWindow();
                    childWindow.customerCreditForm_Closed += (r =>
                    {
                        if (r.CustomerId != 0)
                        {
                            if (CreditLimit != r.CreditLimit || Debt != r.Debt || CreditOwed != r.CreditOwed || CreditRemaining != r.CreditRemaining)
                            {
                                MessageBox.Show("Customer credit limit updated!!!", "Credit Limit Updated", MessageBoxButton.OK, MessageBoxImage.Information);

                                AddNewCustomerCreditVisibility="Collapsed";
                                UpdateCreditVisibility = "Visible";
                            }
                            else
                            {
                                AddNewCustomerCreditVisibility = "Visible";
                                UpdateCreditVisibility = "Collapsed";
                            }

                            
                            //LoadCustomerInfo();
                            CreditLimit = r.CreditLimit;
                            Debt = r.Debt;
                            CreditOwed = r.CreditOwed;
                            CreditRemaining = r.CreditRemaining;
                        }
                        else
                        {
                            CreditLimit = r.CreditLimit;
                            Debt = r.Debt;
                            CreditRemaining = r.CreditRemaining;
                        }
                    });

                    childWindow.ShowAddCustomerCredit(c);
                }
            }
            else
            {
                MessageBox.Show("Please add new customer name to add credit", "Enter Customer", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        //private void EnDisSalesOrderTab()
        //{
        //    if (SelectedCompanyNameVisibility == "Visible" && SelectedCustomer != null)
        //    {
        //        TabSalesOrderLinesEnabled = true;
        //    }
        //    else if (NewCompanyNameVisibility == "Visible")
        //    {
        //        TabSalesOrderLinesEnabled = false;
        //    }
        //    else if (SelectedCustomer != null)
        //    {
        //        TabSalesOrderLinesEnabled = true;
        //    }
        //    else if (SelectedCustomer == null)
        //    {
        //        TabSalesOrderLinesEnabled = false;
        //    }
        //}

        private void LoadSalesOrderLines()
        {
            if (SelectedCustomer != null)
            {
                //BackgroundWorker worker = new BackgroundWorker();
                //ChildWindow LoadingScreen = new ChildWindow();
                //LoadingScreen.ShowWaitingScreen("Loading");

                //worker.DoWork += (_, __) =>
                //{
                    ObservableCollection<SalesOrder> salesOrders = new ObservableCollection<SalesOrder>();
                    salesOrders = SalesOrderManager.LoadSalesOrders("IsAll", SelectedCustomer.CustomerId);

                    SalesOrderLines = DBAccess.GetOpenSalesOrdersByCustomerID(SelectedCustomer.CustomerId);

                    foreach (var item in SalesOrderLines)
                    {
                        foreach (var items in salesOrders)
                        {
                            if(item.SalesOrder.SalesOrderNo==items.SalesOrderNo)
                            {
                                SalesOrderDetails temp = new SalesOrderDetails();
                                temp = item.SalesOrder.SalesOrderDetails[0];
                                item.SalesOrder = items;
                                item.SalesOrder.SalesOrderDetails = new ObservableCollection<SalesOrderDetails>();
                                item.SalesOrder.SalesOrderDetails.Add(temp);
                            }
                        }
                    }
                //};
                //worker.RunWorkerCompleted += delegate(object s, RunWorkerCompletedEventArgs args)
                //{
                //    LoadingScreen.CloseWaitingScreen();
                //};
                //worker.RunWorkerAsync();
            }
        }

        private void AddNewNoteDB()
        {  
            if (CancelNewCustomerVisibility == "Visible" || customerPendingID!=0)
            {
                if (string.IsNullOrWhiteSpace(CustomerNote) )
                {
                    MessageBox.Show("Please enter customer note", "Customer Note Required", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    CustomerNote cn = new CustomerNote();
                    cn.CustomerID = 0;
                    cn.Comment = CustomerNote;
                    cn.CreatedBy = UserData.FirstName + " " + UserData.LastName;
                    cn.DateTime = DateTime.Now;
                    cn.DisplayString = "------------------------------------------------------------------------------------" + System.Environment.NewLine + cn.CreatedBy + " at " + cn.DateTime + System.Environment.NewLine + System.Environment.NewLine + cn.Comment;
                    
                    newCustomer.CustomerNotes.Add(cn);
                    
                    //Add previous notes
                    if (customerPendingID > 0)
                    {
                        foreach (var item in CustomerNotes)
                        {
                            String[] elements = System.Text.RegularExpressions.Regex.Split(item.DisplayString, "PM\r\n\r\n");
                            String[] elements1 = System.Text.RegularExpressions.Regex.Split(elements[0], "at ");
                            String[] elements2 = System.Text.RegularExpressions.Regex.Split(elements1[0], "\r\n");
                            newCustomer.CustomerNotes.Add(new CustomerNote() { Comment = elements[1], DateTime = Convert.ToDateTime(elements1[1]), DisplayString = item.DisplayString, CreatedBy = elements2[1] });
                        }
                    }

                    if (cn != null)
                    {
                        CustomerNotes.Add(cn);
                        CustomerNote = string.Empty;
                        MessageBox.Show("This note will be added when the customer is saved", "Note Saved", MessageBoxButton.OK, MessageBoxImage.None);
                    }
                }
            }
            else
            {
                if (SelectedCustomer == null)
                {
                    MessageBox.Show("Please select customer", "Customer Required", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else if (string.IsNullOrWhiteSpace(CustomerNote))
                {
                    MessageBox.Show("Please enter customer note", "Customer Note Required", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    CustomerNote cn = new CustomerNote();
                    cn.CustomerID = SelectedCustomer.CustomerId;
                    cn.Comment = CustomerNote;
                    cn.CreatedBy = UserData.FirstName + " " + UserData.LastName;
                    cn.DateTime = DateTime.Now;

                    int r = DBAccess.AddNewCustomerNote(cn);
                    if (r > 0)
                    {
                        MessageBox.Show("Customer note added!!!", "Note Added", MessageBoxButton.OK, MessageBoxImage.Information);
                        CustomerNote = string.Empty;
                        LastUpdatedString = DBAccess.GetCustomerLatestDateTime(SelectedCustomer.CustomerId);
                        LoadCustomerNotes();
                    }
                    else if (r == -3)
                    {
                        MessageBox.Show("There has been a problem and the customer note did not add to the database" + System.Environment.NewLine + "Cannot generate customer id", "Problem Occured", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else
                    {
                        MessageBox.Show("There has been a problem and the customer note did not add to the database", "Problem Occured", MessageBoxButton.OK, MessageBoxImage.Error);
                    }

                }
            }
        }

        private void LoadCustomerNotes()
        {
            if (SelectedCustomer != null)
            {
                CustomerNotes = DBAccess.GetCustomerNotes(SelectedCustomer.CustomerId);
            }
        }

        private void GoToCustomer(object parameter)
        {

            int index = SearchedCustomers.IndexOf(parameter as Customer);
            if (index > -1 && index < SearchedCustomers.Count)
            {
                MainTabSelectedIndex = 0;
                CustomerBottomTabEnabled = true;
                

                var c = CustomerList.SingleOrDefault(x => x.CustomerId == SearchedCustomers[index].CustomerId);

                SelectedCustomer = c;
                SearchCustomerInfo();
                //ProcessPrivilages();
               // LoadCustomerNotes();
                //SelectedCustomer.CompanyName = SearchedCustomers[index].CompanyName;
                //ClearSearchField();     
                //SelectedCustomer = null;


            }
        }

        private void ViewDocument(object parameter)
        {

            int index = DiscountStructure.IndexOf(parameter as DiscountStructure);
            if (index > -1 && index < DiscountStructure.Count)
            {
                var childWindow = new ChildWindow();
                childWindow.ShowFormula(DiscountStructure[index].Category.DocumentPath);
            }
        }

        private void LoadCustomerCreditHistory()
        {
            if (SelectedCustomer != null)
            {
                CustomerCreditActivity = DBAccess.GetCustomerCreditHistory(SelectedCustomer.CustomerId);
            }
        }

        private void ClearSearchField()
        {
            ContactPersonList = new List<string>();
            Categories = new ObservableCollection<Category>();
            Suburb = new List<string>();

            BackgroundWorker worker = new BackgroundWorker();
            ChildWindow LoadingScreen = new ChildWindow();
            LoadingScreen.ShowWaitingScreen("Loading");

            worker.DoWork += (_, __) =>
            {                
                SelectedState = "";
                SearchedCustomers = new ObservableCollection<Customer>();
                LoadCustomerList();
                Categories = new ObservableCollection<Category>(mainCategories);
                //Load contact person
                ContactPersonList = DBAccess.GetListOfContactPersonNames();               

            };
            worker.RunWorkerCompleted += delegate(object s, RunWorkerCompletedEventArgs args)
            {
               LoadingScreen.CloseWaitingScreen();
               if (ContactPersonList.Count == 0)
               {
                   MessageBox.Show("Failed to load contact list", "", MessageBoxButton.OK, MessageBoxImage.Error);
               }
            };
            worker.RunWorkerAsync();
        }

        private void ClearFields()
        {
            NewCompanyName = string.Empty;
            CustomerType = string.Empty;
            CompanyAddress = string.Empty;
            CompanyCity = string.Empty;
            CompanyState = string.Empty;
            CompanyPostCode = string.Empty;
            CompanyEmail = string.Empty;
            CompanyTelephone = string.Empty;
            CompanyFax = string.Empty;
            ShipAddress = string.Empty;
            ShipCity = string.Empty;
            ShipState = string.Empty;
            ShipPostCode = string.Empty;
            ShipCountry = string.Empty;
            Designation1 = string.Empty;
            FirstName1 = string.Empty;
            LastName1 = string.Empty;
            Telephone1 = string.Empty;
            Mobile1 = string.Empty;
            Fax1 = string.Empty;
            Email1 = string.Empty;

            Designation2 = string.Empty;
            FirstName2 = string.Empty;
            LastName2 = string.Empty;
            Telephone2 = string.Empty;
            Mobile2 = string.Empty;
            Fax2 = string.Empty;
            Email2 = string.Empty;

            Designation3 = string.Empty;
            FirstName3 = string.Empty;
            LastName3 = string.Empty;
            Telephone3 = string.Empty;
            Mobile3 = string.Empty;
            Fax3 = string.Empty;
            Email3 = string.Empty;

            CreditLimit = 0;
            CreditRemaining = 0;
            Debt = 0;

            Active = false;
            LastUpdatedString = string.Empty;
            DiscountStructure.Clear();
            IsStopCredit = false;
            //SelectedCountry = new Country();
            SelectedCountry = "Select";
        }

        private void RemoveProductInstalledNow()
        {
            if (ProductTypeSelected == null)
            {

            }
            else if (SelectedCustomer == null && CancelNewCustomerVisibility == "Collapsed")
            {

            }
            else
            {
                if (ProductsInstalledView.Count > 0)
                {
                    for (int i = 0; i < ProductsInstalledView.Count; i++)
                    {
                        if (ProductTypeSelected != null)
                        {
                            if (ProductsInstalledView[i].Type == ProductTypeSelected.Type)
                            {
                                ProductsInstalledView.RemoveAt(i);
                            }
                        }
                    }
                }
            }
        }
        private void AddProductInstalledNow()
        {
            //if (CancelNewCustomerVisibility == "Collapsed")
            //{
            if (SelectedProductsInstalled.ProductTypeID != 0)
            {
                if (SelectedProductsInstalled != null)
                {
                    bool yes = ProductsInstalledView.Any(x => x.ProductTypeID == SelectedProductsInstalled.ProductTypeID);
                    if (yes == false)
                    {
                        ProductsInstalledView.Add(SelectedProductsInstalled);
                    }
                    else
                    {
                        MessageBox.Show("Product " + SelectedProductsInstalled.Type + " already in the list", "", MessageBoxButton.OK, MessageBoxImage.Stop);
                    }

                }
            }
            else
            {
                MessageBox.Show("Please select a product to add", "", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            //}
            //else
            //{
            //    if (SelectedProductsInstalled != null)
            //    {
            //        ProductsInstalledView.Add(SelectedProductsInstalled);
            //    }

            //}
        }

        private void SearchCustomer()
        {
            if (SelectedSearchCategory == null && SelectedSearchCustomer == null && string.IsNullOrWhiteSpace(SelectedSuburb) && string.IsNullOrWhiteSpace(SelectedState) && string.IsNullOrWhiteSpace(SelectedFirstName))
            {
                MessageBox.Show("Please enter search criteria", "", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                SearchedCustomers = new ObservableCollection<Customer>();

                string catName = SelectedSearchCategory == null ? "" : SelectedSearchCategory.CategoryName;
                string compName = SelectedSearchCustomer == null ? "" : SelectedSearchCustomer.CompanyName;
                string suburb = SelectedSuburb == null ? "" : SelectedSuburb;
                string state = SelectedState == null ? "" : SelectedState;
                string firstName = SelectedFirstName == null ? "" : SelectedFirstName;

                BackgroundWorker worker = new BackgroundWorker();
                ChildWindow LoadingScreen = new ChildWindow();
                LoadingScreen.ShowWaitingScreen("Loading");

                worker.DoWork += (_, __) =>
                {
                    SearchedCustomers = DBAccess.SearchCustomerDB(catName, compName, suburb, state, firstName);
                };

                worker.RunWorkerCompleted += delegate(object s, RunWorkerCompletedEventArgs args)
                {
                    LoadingScreen.CloseWaitingScreen();
                };
                worker.RunWorkerAsync();
            }
        }

        private void EditCompanyNameFunc()
        {
            EditNameVisibility = "Visible";
            SelectedCompanyNameVisibility = "Collapsed";
            EditCompanyName = string.Empty;
            CancelEditCompanyNameVisibility = "Visible";
            SearchBtnVisibility = "Collapsed";
            if (SelectedCustomer != null)
            {
                EditCompanyName = SelectedCustomer.CompanyName;
            }
            else 
            {
                MessageBox.Show("Please select customer to edit", "", MessageBoxButton.OK, MessageBoxImage.Asterisk);
            }
        }

        private void CancelEditCompanyNameFunc()
        {
            EditNameVisibility = "Collapsed";
            SelectedCompanyNameVisibility = "Visible";
            CancelEditCompanyNameVisibility = "Collapsed";
            EditCompanyName = string.Empty;
            SearchBtnVisibility = "Visible";
        }

        private bool CanExecute(object parameter)
        {
            return true;
        }


        private void ViewUpdateOrder(Object parameter)
        {
            int index = SalesOrderLines.IndexOf(parameter as SalesOrderLines);
            if (index >= 0)
            {
                List<DiscountStructure> discountList = new List<DiscountStructure>();
                ChildWindow showUpdateSalesScreen = new ChildWindow();
                showUpdateSalesScreen.ShowUpdateSalesOrderView(UserData.UserName, UserData.State, UserData.UserPrivilages, UserData.MetaData, SalesOrderLines[index].SalesOrder, discountList, "", CustomerList);
                showUpdateSalesScreen.updateSalesOrder_Closed += (r =>
                {                   
                    BottomTabSelectedIndex = 2;
                    BottomTabSelectedIndex=3;                        
                });            
            }
        }

        private void EditContactPerson(Object parameter)
        {
            int index = ContactPersons.IndexOf(parameter as ContactPerson);
            if (index >= 0)
            {
                var childWindow = new ChildWindow();
                childWindow.addEditContactPerson_Closed += (r =>
                {
                   if(r != null)
                   {
                       if (r.CustomerID > 0)
                       {
                           //Get contact person details
                           List<ContactPerson> cpList = new List<ContactPerson>();
                           cpList = DBAccess.GetContactPersonByCustomerID(SelectedCustomer.CustomerId);
                           ContactPersons = new ObservableCollection<ContactPerson>(cpList as List<ContactPerson>);
                           LastUpdatedString = DBAccess.GetCustomerLatestDateTime(r.CustomerID);
                       }
                       else
                       {
                           if(ContactPersons.Count > 0)
                           {
                               var data = ContactPersons.SingleOrDefault(x=>x.ContactPersonName.Equals(r.ContactPersonName));
                               if(data != null)
                               {
                                   data = r;
                               }
                           }
                       }
                   }
                });
                childWindow.ShowAddUpdateContactPerson(ContactPersons[index]);                               

            }
        }

        private void DeleteContactPerson(Object parameter)
        {
            int index = ContactPersons.IndexOf(parameter as ContactPerson);
            if (index >= 0)
            {
                if (MessageBox.Show("Are you sure you want to delete this contact", "Deleting Contact", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    int res = DBAccess.DeleteContactPersonById(ContactPersons[index]);
                    if (res == 1)
                    {
                        MessageBox.Show("Contact deleted", "Quote Deleted", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Could not delete contact" + System.Environment.NewLine + "Please try again later", "Contact Deletion Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                    }

                    //Get contact person details
                    ContactPersons.Clear();
                    List<ContactPerson> cpList = new List<ContactPerson>();
                    cpList = DBAccess.GetContactPersonByCustomerID(SelectedCustomer.CustomerId);

                    ContactPersons = new ObservableCollection<ContactPerson>(cpList as List<ContactPerson>);

                    LastUpdatedString=DBAccess.GetCustomerLatestDateTime(SelectedCustomer.CustomerId);
                }
            }
        }

        

        private void CopyContactPersonRow(Object parameter)
        {
            int index = ContactPersons.IndexOf(parameter as ContactPerson);
            if (index >= 0)
            {
                //Clipboard.SetText(parameter != null ? parameter.ToString() : "<null>");
                string phone1 = ContactPersons[index].PhoneNumber1 == "Not Available" ? "" : ContactPersons[index].PhoneNumber1;
                string phone2 = ContactPersons[index].PhoneNumber2 == "Not Available" ? "" : ContactPersons[index].PhoneNumber2;
                Clipboard.SetText(ContactPersons[index].ContactPersonName + "       " + phone1 + "      " + phone2 + "      " + ContactPersons[index].Email);
            }
        }

        

        private void AddNewContactPerson()
        {
            //if (SelectedCustomer != null)
            //{
                ContactPerson cp = new ContactPerson();
                cp.CustomerID = SelectedCustomer == null ? 0 : SelectedCustomer.CustomerId;
                var childWindow = new ChildWindow();
                childWindow.addEditContactPerson_Closed += (r =>
                {
                    if (r != null)
                    {
                        if (r.CustomerID > 0)
                        {
                            //Get contact person details
                            List<ContactPerson> cpList = new List<ContactPerson>();
                            cpList = DBAccess.GetContactPersonByCustomerID(SelectedCustomer.CustomerId);
                            ContactPersons = new ObservableCollection<ContactPerson>(cpList as List<ContactPerson>);
                            LastUpdatedString = DBAccess.GetCustomerLatestDateTime(r.CustomerID);
                        }
                        else
                        {
                            ContactPersons.Add(r);
                        }
                        
                    }
                });
                childWindow.ShowAddUpdateContactPerson(cp);
            ///}
        }

        private void SearchCustomerWithBackgroundWorker()
        {
            if (SelectedCustomer != null)
            {
                BackgroundWorker worker = new BackgroundWorker();
                ChildWindow LoadingScreen = new ChildWindow();
                LoadingScreen.ShowWaitingScreen("Loading");

                worker.DoWork += (_, __) =>
                {                
                    SearchCustomerInfo();
                };

                worker.RunWorkerCompleted += delegate(object s, RunWorkerCompletedEventArgs args)
                {
                    LoadingScreen.CloseWaitingScreen();
                };
                worker.RunWorkerAsync();
            }
            else
            {
                MessageBox.Show("Please select customer", "", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void SearchCustomerInfo()
        {
            if(SelectedCustomer != null)
            {
                RemoveExclusiveDiscount();
                AppendExclusiveDiscount();

                BottomTabSelectedIndex = 1;
                //Load discount data                
                List<DiscountStructure> d = DBAccess.GetDiscount(SelectedCustomer.CustomerId);
                DiscountStructure = new ObservableCollection<DiscountStructure>(d);
                SelectedCustomer.DiscountStructure = new ObservableCollection<DiscountStructure>();
                SelectedCustomer.DiscountStructure = DiscountStructure;

                CustomerBottomTabEnabled = true ;  
                EditCompanyNameVisibility = "Visible";
                CancelEditCompanyNameVisibility = "Collapsed";
                CustomerType = SelectedCustomer.CustomerType;
                CompanyAddress = SelectedCustomer.CompanyAddress;
                CompanyCity = SelectedCustomer.CompanyCity;
                CompanyState = SelectedCustomer.CompanyState;                
                CompanyPostCode = SelectedCustomer.CompanyPostCode;
                CompanyEmail = SelectedCustomer.CompanyEmail;
                CompanyTelephone = SelectedCustomer.CompanyTelephone;
                CompanyFax = SelectedCustomer.CompanyFax;
                CreditLimit = SelectedCustomer.CreditLimit;
                CreditRemaining = SelectedCustomer.CreditRemaining;
                Debt = SelectedCustomer.Debt;
                ShipAddress = SelectedCustomer.ShipAddress;
                ShipCity = SelectedCustomer.ShipCity;
                ShipState = SelectedCustomer.ShipState;
                ShipPostCode = SelectedCustomer.ShipPostCode;
                ShipCountry = SelectedCustomer.ShipCountry;
                Designation1 = SelectedCustomer.Designation1;
                FirstName1 = SelectedCustomer.FirstName1;
                LastName1 = SelectedCustomer.LastName1;
                Telephone1 = SelectedCustomer.Telephone1;
                Mobile1 = SelectedCustomer.Mobile1;
                Fax1 = SelectedCustomer.Fax1;
                Email1 = SelectedCustomer.Email1;
                Designation2 = SelectedCustomer.Designation2;
                FirstName2 = SelectedCustomer.FirstName2;
                LastName2 = SelectedCustomer.LastName2;
                Telephone2 = SelectedCustomer.Telephone2;
                Mobile2 = SelectedCustomer.Mobile2;
                Fax2 = SelectedCustomer.Fax2;
                Email2 = SelectedCustomer.Email2;
                Designation3 = SelectedCustomer.Designation3;
                FirstName3 = SelectedCustomer.FirstName3;
                LastName3 = SelectedCustomer.LastName3;
                Telephone3 = SelectedCustomer.Telephone3;
                Mobile3 = SelectedCustomer.Mobile3;
                Fax3 = SelectedCustomer.Fax3;
                Email3 = SelectedCustomer.Email3;
                Active = SelectedCustomer.Active;
                DiscountStructure = SelectedCustomer.DiscountStructure;
                CreditOwed = SelectedCustomer.CreditOwed;
                SelectedCustomerState = SelectedCustomer.CompanyState;
                ProductsInstalledView = SelectedCustomer.ProductsInstalled;
                IsStopCredit = SelectedCustomer.StopCredit == "False" ? false : true;
                SelectedCountry = SelectedCustomer.CompanyCountry;
                LastUpdatedString = DBAccess.GetCustomerLatestDateTime(SelectedCustomer.CustomerId);

                SelectedCategory = SelectedCustomer.PrimaryBusiness;
                LoadCreditInfo();
                ConsolidateDiscount();
                LoadSalesOrderLines();
                LoadCustomerNotes();

                if (SelectedCustomer.CreditLimit == 0 && SelectedCustomer.CreditRemaining == 0 && SelectedCustomer.CreditOwed == 0 && SelectedCustomer.Debt == 0)
                {
                    AddNewCustomerCreditVisibility = "Visible";
                    UpdateCreditVisibility = "Collapsed";
                }
                else
                {
                    AddNewCustomerCreditVisibility = "Collapsed";
                    UpdateCreditVisibility = "Visible";
                }                

                if(BottomTabSelectedIndex == 5)
                {
                    AdminNote = DBAccess.GetAdminNoteByID(SelectedCustomer.CustomerId);
                }

                //Get contact person details
                List<ContactPerson> cpList = new List<ContactPerson>();
                cpList=  DBAccess.GetContactPersonByCustomerID(SelectedCustomer.CustomerId);

                ContactPersons = new ObservableCollection<ContactPerson>(cpList as List<ContactPerson>);

                LastUpdatedString=DBAccess.GetCustomerLatestDateTime(SelectedCustomer.CustomerId);
                
            }
            else
            {
                CustomerBottomTabEnabled = false;           
                MessageBox.Show("Please enter a valid Company Name", "Select Company Name", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }       

        private void CustomerChanged()
        {
            Clear();
        }

        private string GetLastUpdatedDateTimeString()
        {
            string lastUpdatedStr = string.Empty;
            return lastUpdatedStr;        
        }

        #region PUBLIC_PROPERTIES

        private ObservableCollection<CustomerNote> _customerNotes;
        public ObservableCollection<CustomerNote> CustomerNotes
        {
            get
            {
                return _customerNotes;
            }
            set
            {
                _customerNotes = value;
                RaisePropertyChanged("CustomerNotes");
            }
        }

        public string CustomerNote
        {
            get
            {
                return _customerNote;
            }
            set
            {
                _customerNote = value;
                RaisePropertyChanged("CustomerNote");
            }
        }

        //public bool TabSalesOrderLinesEnabled
        //{
        //    get
        //    {
        //        return _tabSalesOrderLinesEnabled;
        //    }
        //    set
        //    {
        //        _tabSalesOrderLinesEnabled = value;
        //        RaisePropertyChanged("TabSalesOrderLinesEnabled");
        //    }
        //}

        public ObservableCollection<SalesOrderLines> SalesOrderLines
        {
            get
            {
                return _salesOrderLines;
            }
            set
            {
                _salesOrderLines = value;
                RaisePropertyChanged("SalesOrderLines");
            }
        }

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

        public string Designation1
        {
            get
            {
                return _designation1;
            }
            set
            {
                _designation1 = value;
                RaisePropertyChanged("Designation1");
            }
        }

        public string FirstName1
        {
            get
            {
                return _firstName1;
            }
            set
            {
                _firstName1 = value;
                RaisePropertyChanged("FirstName1");
            }
        }

        public string LastName1
        {
            get
            {
                return _lastName1;
            }
            set
            {
                _lastName1 = value;
                RaisePropertyChanged("LastName1");
            }
        }

        public string Telephone1
        {
            get
            {
                return _telephone1;
            }
            set
            {
                _telephone1 = value;
                RaisePropertyChanged("Telephone1");
            }
        }

        public string Mobile1
        {
            get
            {
                return _mobile1;
            }
            set
            {
                _mobile1 = value;
                RaisePropertyChanged("Mobile1");
            }
        }


        public string Fax1
        {
            get
            {
                return _fax1;
            }
            set
            {
                _fax1 = value;
                RaisePropertyChanged("Fax1");
            }
        }

        public string Email1
        {
            get
            {
                return _email1;
            }
            set
            {
                _email1 = value;
                RaisePropertyChanged("Email1");
            }
        }

        public string Designation2
        {
            get
            {
                return _designation2;
            }
            set
            {
                _designation2 = value;
                RaisePropertyChanged("Designation2");
            }
        }

        public string FirstName2
        {
            get
            {
                return _firstName2;
            }
            set
            {
                _firstName2 = value;
                RaisePropertyChanged("FirstName2");
            }
        }

        public string LastName2
        {
            get
            {
                return _lastName2;
            }
            set
            {
                _lastName2 = value;
                RaisePropertyChanged("LastName2");
            }
        }

        public string Telephone2
        {
            get
            {
                return _telephone2;
            }
            set
            {
                _telephone2 = value;
                RaisePropertyChanged("Telephone2");
            }
        }

        public string Mobile2
        {
            get
            {
                return _mobile2;
            }
            set
            {
                _mobile2 = value;
                RaisePropertyChanged("Mobile2");
            }
        }


        public string Fax2
        {
            get
            {
                return _fax2;
            }
            set
            {
                _fax2 = value;
                RaisePropertyChanged("Fax2");
            }
        }

        public string Email2
        {
            get
            {
                return _email2;
            }
            set
            {
                _email2 = value;
                RaisePropertyChanged("Email2");
            }
        }

        public string Designation3
        {
            get
            {
                return _designation3;
            }
            set
            {
                _designation3 = value;
                RaisePropertyChanged("Designation3");
            }
        }

        public string FirstName3
        {
            get
            {
                return _firstName3;
            }
            set
            {
                _firstName3 = value;
                RaisePropertyChanged("FirstName3");
            }
        }

        public string LastName3
        {
            get
            {
                return _lastName3;
            }
            set
            {
                _lastName3 = value;
                RaisePropertyChanged("LastName3");
            }
        }

        public string Telephone3
        {
            get
            {
                return _telephone3;
            }
            set
            {
                _telephone3 = value;
                RaisePropertyChanged("Telephone3");
            }
        }

        public string Mobile3
        {
            get
            {
                return _mobile3;
            }
            set
            {
                _mobile3 = value;
                RaisePropertyChanged("Mobile3");
            }
        }


        public string Fax3
        {
            get
            {
                return _fax3;
            }
            set
            {
                _fax3 = value;
                RaisePropertyChanged("Fax3");
            }
        }

        public string Email3
        {
            get
            {
                return _email3;
            }
            set
            {
                _email3 = value;
                RaisePropertyChanged("Email3");
            }
        }

        public string ShipAddress
        {
            get
            {
                return _shipAddress;
            }
            set
            {
                _shipAddress = value;
                RaisePropertyChanged("ShipAddress");
            }
        }

        public string ShipCity
        {
            get
            {
                return _shipCity;
            }
            set
            {
                _shipCity = value;
                RaisePropertyChanged("ShipCity");
            }
        }

        public string ShipState
        {
            get
            {
                return _shipState;
            }
            set
            {
                _shipState = value;
                RaisePropertyChanged("ShipState");
            }
        }


        public string ShipPostCode
        {
            get
            {
                return _shipPostCode;
            }
            set
            {
                _shipPostCode = value;
                RaisePropertyChanged("ShipPostCode");
            }
        }

        public string ShipCountry
        {
            get
            {
                return _shipCountry;
            }
            set
            {
                _shipCountry = value;
                RaisePropertyChanged("ShipCountry");
            }
        }


        public decimal Debt
        {
            get
            {
                return _debt;
            }
            set
            {
                _debt = value;
                RaisePropertyChanged("Debt");
            }
        }

        public decimal CreditOwed
        {
            get
            {
                return _creditOwed;
            }
            set
            {
                _creditOwed = value;
                RaisePropertyChanged("CreditOwed");
            }
        }

        public decimal CreditRemaining
        {
            get
            {
                return _creditRemaining;
            }
            set
            {
                _creditRemaining = value;
                RaisePropertyChanged("CreditRemaining");
            }
        }

        public decimal CreditLimit
        {
            get
            {
                return _creditLimit;
            }
            set
            {
                _creditLimit = value;
                RaisePropertyChanged("CreditLimit");
            }
        }

        public bool Active
        {
            get
            {
                return _active;
            }
            set
            {
                _active = value;
                RaisePropertyChanged("Active");
            }
        }

        public string CustomerType
        {
            get
            {
                return _customerType;
            }
            set
            {
                _customerType = value;
                RaisePropertyChanged("CustomerType");                

                CreditLimit = 0;
                CreditRemaining = 0;
                CreditOwed = 0;
                Debt = 0;
                TotalReleased = 0;
                LastOrderDate = string.Empty;
                IsStopCredit = false;
                if (CustomerCreditActivity != null)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        CustomerCreditActivity.Clear();
                    });
                }

                ProcessPrivilages();
            }
        }

        public string CompanyAddress
        {
            get
            {
                return _companyAddress;
            }
            set
            {
                _companyAddress = value;
                RaisePropertyChanged("CompanyAddress");
            }
        }

        public string CompanyCity
        {
            get
            {
                return _companyCity;
            }
            set
            {
                _companyCity = value;
                RaisePropertyChanged("CompanyCity");
            }
        }

        public string CompanyState
        {
            get
            {
                return _companyState;
            }
            set
            {
                _companyState = value;
                RaisePropertyChanged("CompanyState");
            }
        }

        //public string CompanyCountry
        //{
        //    get
        //    {
        //        return _companyCountry;
        //    }
        //    set
        //    {
        //        _companyCountry = value;
        //        RaisePropertyChanged("CompanyCountry");
        //    }
        //}

        public string CompanyPostCode
        {
            get
            {
                return _companyPostCode;
            }
            set
            {
                _companyPostCode = value;
                RaisePropertyChanged("CompanyPostCode");
            }
        }

        public string CompanyEmail
        {
            get
            {
                return _companyEmail;
            }
            set
            {
                _companyEmail = value;
                RaisePropertyChanged("CompanyEmail");
            }
        }

        public string CompanyTelephone
        {
            get
            {
                return _companyTelephone;
            }
            set
            {
                _companyTelephone = value;
                RaisePropertyChanged("CompanyTelephone");
            }
        }

        public string CompanyFax
        {
            get
            {
                return _companyFax;
            }
            set
            {
                _companyFax = value;
                RaisePropertyChanged("CompanyFax");
            }
        }

        public string UpdateCommandVisibility
        {
            get
            {
                return _updateCommandVisibility;
            }
            set
            {
                _updateCommandVisibility = value;
                RaisePropertyChanged("UpdateCommandVisibility");
            }
        }
        public string AddCommandVisibility
        {
            get
            {
                return _addCommandVisibility;
            }
            set
            {
                _addCommandVisibility = value;
                RaisePropertyChanged("AddCommandVisibility");
            }
        }

        public string NewCompanyName
        {
            get
            {
                return _newCompanyName;
            }
            set
            {
                _newCompanyName = value;
                RaisePropertyChanged("NewCompanyName");
            }
        }

        public bool CreditEnabled
        {
            get
            {
                return _creditEnabled;
            }
            set
            {
                _creditEnabled = value;
                RaisePropertyChanged("CreditEnabled");
            }
        }

        public ObservableCollection<int> Discount
        {
            get
            {
                return _discount;
            }
            set
            {
                _discount = value;
                RaisePropertyChanged("Discount");
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



        public Customer SelectedSearchCustomer
        {
            get
            {
                return _selectedSearchCustomer;
            }
            set
            {
                _selectedSearchCustomer = value;
                RaisePropertyChanged("SelectedSearchCustomer");
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

                //AddNewContactBtnVisibility = "Collapsed";

                if (SelectedCustomer != null)
                {
                    //AddNewContactBtnVisibility = "Visible";
                    //AdminNote = DBAccess.GetAdminNoteByID(SelectedCustomer.CustomerId);
                }
                else
                {
                    //AdminNote = null;
                   
                    EditCompanyNameVisibility = "Collapsed";
                    CancelEditCompanyNameVisibility = "Collapsed";
                    Clear();
                   
                }
            }
        }


        public decimal TotalReleased
        {
            get
            {
                return _totalReleased;
            }
            set
            {
                _totalReleased = value;
                RaisePropertyChanged("TotalReleased");
            }
        }

        public string LastUpdatedString
        {
            get
            {
                return _lastUpdatedString;
            }
            set
            {
                _lastUpdatedString = value;
                RaisePropertyChanged("LastUpdatedString"); 
            }
        }

        public string LastOrderDate
        {
            get
            {
                return _lastOrderDate;
            }
            set
            {
                _lastOrderDate = value;
                RaisePropertyChanged("LastOrderDate");
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


        public int MainTabSelectedIndex
        {
            get
            {
                return _mainTabSelectedIndex;
            }
            set
            {
                _mainTabSelectedIndex = value;
                RaisePropertyChanged("MainTabSelectedIndex");
                
                if (MainTabSelectedIndex == 0)
                {
                    CancelNewCustomer();
                    UpdateCommandVisibility = "Visible";
                    BottomTabSelectedIndex = 0;
                    Categories = new ObservableCollection<Category>(mainCategories);
                    
                    if(SelectedCustomer!=null)
                    {
                        if(SelectedCustomer.CreditLimit == 0 && SelectedCustomer.CreditRemaining==0 && SelectedCustomer.CreditOwed==0 && SelectedCustomer.Debt ==0)
                        {
                            AddNewCustomerCreditVisibility = "Visible";
                            UpdateCreditVisibility = "Collapsed";
                        }
                        else
                        {
                            AddNewCustomerCreditVisibility = "Collapsed";
                            UpdateCreditVisibility = "Visible";
                        }
                    }
                }
                else if (MainTabSelectedIndex == 1)
                {
                    UpdateCommandVisibility = "Collapsed";
                    AddCommandVisibility = "Collapsed";
                    ClearSearchField();
                   
                }              
            }
        }

        public int BottomTabSelectedIndex
        {
            get
            {
                return _bottomTabSelectedIndex;
            }
            set
            {
                _bottomTabSelectedIndex = value;
                RaisePropertyChanged("BottomTabSelectedIndex");

                if (CustomerCreditActivity != null)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        CustomerCreditActivity.Clear();
                    });
                }

                if (BottomTabSelectedIndex == 0)
                {
                    //DiscountChanged();
                }
                else if (BottomTabSelectedIndex == 1)
                {

                }
                else if (BottomTabSelectedIndex == 2)
                {

                }
                else if (BottomTabSelectedIndex == 3)
                {
                    LoadSalesOrderLines();
                }
                else if (BottomTabSelectedIndex == 4)
                {
                    LoadCustomerNotes();
                }
                else if (BottomTabSelectedIndex == 5)
                {
                    if (SelectedCustomer != null)
                    {
                        AdminNote = DBAccess.GetAdminNoteByID(SelectedCustomer.CustomerId);
                    }
                    else
                    {
                        AdminNote = new AdminNote();
                    }
                }
                else if (BottomTabSelectedIndex == 6)
                {
                    if (SelectedCustomer != null)
                    {                        

                        Tuple<Customer, List<ProductStock>> tupe = DBAccess.GetMultipleData(SelectedCustomer.CustomerId, new ObservableCollection<SalesOrderDetails>(), 0);
                        CreditLimit = tupe.Item1.CreditLimit;
                        CreditRemaining = tupe.Item1.CreditRemaining;
                        CreditOwed = tupe.Item1.CreditOwed;
                        Debt = tupe.Item1.Debt;

                        SelectedCustomer.CreditLimit = CreditLimit;
                        SelectedCustomer.CreditRemaining = CreditRemaining;
                        SelectedCustomer.CreditOwed = CreditOwed;
                        SelectedCustomer.Debt = Debt;

                        var item = CustomerList.FirstOrDefault(i => i.CustomerId == SelectedCustomer.CustomerId);
                        if (item != null)
                        {
                            item = SelectedCustomer;
                        }

                        if (CreditLimit == 0 && CreditRemaining == 0 && CreditOwed == 0 && Debt == 0)
                        {
                            AddNewCustomerCreditVisibility = "Visible";
                            UpdateCreditVisibility = "Collapsed";
                        }
                        else
                        {
                            AddNewCustomerCreditVisibility = "Collapsed";
                            UpdateCreditVisibility = "Visible";
                        }
                    }
                }
                else if (BottomTabSelectedIndex == 7)
                {
                    CustomerCreditActivity = new ObservableCollection<CustomerCreditActivity>();
                    LoadCustomerCreditHistory();
                }
               
            }
        }


        public ObservableCollection<Category> Categories
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


        public string AddCreditBackground
        {
            get
            {
                return _addCreditBackground;
            }
            set
            {
                _addCreditBackground = value;
                RaisePropertyChanged("AddCreditBackground");
            }
        }

        public string NewCompanyNameVisibility
        {
            get
            {
                return _newCompanyNameVisibility;
            }
            set
            {
                _newCompanyNameVisibility = value;
                RaisePropertyChanged("NewCompanyNameVisibility");
                //EnDisSalesOrderTab();
            }
        }

        private Customer _newCustomer;
        public Customer NewCustomer
        {
            get
            {
                return _newCustomer;
            }
            set
            {
                _newCustomer = value;
                RaisePropertyChanged("NewCustomer");
            }
        }

        public string SelectedCompanyNameVisibility
        {
            get
            {
                return _selectedCompanyNameVisibility;
            }
            set
            {
                _selectedCompanyNameVisibility = value;
                RaisePropertyChanged("SelectedCompanyNameVisibility");
                //EnDisSalesOrderTab();
            }
        }



        public string CancelNewCustomerVisibility
        {
            get
            {
                return _cancelNewCustomerVisibility;
            }
            set
            {
                _cancelNewCustomerVisibility = value;
                RaisePropertyChanged("CancelNewCustomerVisibility");
            }
        }

        public string NewCustomerVisibility
        {
            get
            {
                return _newCustomerVisibility;
            }
            set
            {
                _newCustomerVisibility = value;
                RaisePropertyChanged("NewCustomerVisibility");

                if (NewCustomerVisibility == "Visible")
                {
                    ProcessPrivilages();
                }
            }
        }

        public string AddNewCustomerCreditVisibility
        {
            get
            {
                return _addNewCustomerCreditVisibility;
            }
            set
            {
                _addNewCustomerCreditVisibility = value;
                RaisePropertyChanged("AddNewCustomerCreditVisibility");
            }
        }

        public string UpdateCreditVisibility
        {
            get
            {
                return _updateCreditVisibility;
            }
            set
            {
                _updateCreditVisibility = value;
                RaisePropertyChanged("UpdateCreditVisibility");
            }
        }

        //public string SearchString
        //{
        //    get { return _searchString; }
        //    set
        //    {
        //        _searchString = value;
        //        RaisePropertyChanged("SearchString);
        //        if (ItemsView != null)
        //        {
        //           Application.Current.Dispatcher.Invoke(() =>
        //           {
        //               ItemsView.Refresh();
        //           });
        //        }
        //    }
        //}

        public bool IsCreditTabEnabled
        {
            get { return _isCreditTabEnabled; }
            set
            {
                _isCreditTabEnabled = value;
                RaisePropertyChanged("IsCreditTabEnabled");
            }
        }

        public ObservableCollection<CustomerCreditActivity> CustomerCreditActivity
        {
            get { return _customerCreditActivity; }
            set
            {
                _customerCreditActivity = value;
                RaisePropertyChanged("CustomerCreditActivity");
            }
        }

        public string SelectedCountry
        {
            get { return _selectedCountry; }
            set
            {
                _selectedCountry = value;
                RaisePropertyChanged("SelectedCountry");               
            }
        }

        public bool IsCreditHistoryTabEnabled
        {
            get { return _isCreditHistoryTabEnabled; }
            set
            {
                _isCreditHistoryTabEnabled = value;
                RaisePropertyChanged("IsCreditHistoryTabEnabled");
            }
        }



        public bool IsStopCredit
        {
            get { return _isStopCredit; }
            set
            {
                _isStopCredit = value;
                RaisePropertyChanged("IsStopCredit");
                if (SelectedCustomer != null)
                {
                    //if (IsStopCredit)
                    //{
                    //    SelectedCustomer.StopCredit = "True";
                    //}
                    //else
                    //{
                    //    SelectedCustomer.StopCredit = "False";
                    //}
                }
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


        public List<ProductType> ProductsInstalled
        {
            get
            {
                return _productsInstalled;
            }
            set
            {
                _productsInstalled = value;
                RaisePropertyChanged("ProductsInstalled");
            }
        }


        public ProductType SelectedProductsInstalled
        {
            get
            {
                return _selectedProductsInstalled;
            }
            set
            {
                _selectedProductsInstalled = value;
                RaisePropertyChanged("SelectedProductsInstalled");
                if (SelectedProductsInstalled != null)
                {
                    IsAddEnabled = true;
                }
                else
                {
                    IsAddEnabled = false;
                }
            }
        }


        public ObservableCollection<ProductType> ProductsInstalledView
        {
            get
            {
                return _productsInstalledView;
            }
            set
            {
                _productsInstalledView = value;
                RaisePropertyChanged("ProductsInstalledView");
            }
        }

        public ProductType ProductTypeSelected
        {
            get
            {
                return _productTypeSelected;
            }
            set
            {
                _productTypeSelected = value;
                RaisePropertyChanged("ProductTypeSelected");
                if (ProductTypeSelected != null)
                {
                    IsRemoveEnabled = true;
                }
                else
                {
                    IsRemoveEnabled = false;
                }
            }
        }
        public bool IsAddEnabled
        {
            get
            {
                return _isAddEnabled;
            }
            set
            {
                _isAddEnabled = value;
                RaisePropertyChanged("IsAddEnabled");
            }
        }

        public bool IsRemoveEnabled
        {
            get
            {
                return _isRemoveEnabled;
            }
            set
            {
                _isRemoveEnabled = value;
                RaisePropertyChanged("IsRemoveEnabled");
            }
        }


        public List<string> ContactPersonList
        {
            get
            {
                return _contactPersonList;
            }
            set
            {
                _contactPersonList = value;
                RaisePropertyChanged("ContactPersonList");
            }
        }


        public string SelectedFirstName
        {
            get
            {
                return _selectedFirstName;
            }
            set
            {
                _selectedFirstName = value;
                RaisePropertyChanged("SelectedFirstName");
            }
        }

        public string SelectedLastName
        {
            get
            {
                return _selectedLastName;
            }
            set
            {
                _selectedLastName = value;
                RaisePropertyChanged("SelectedLastName");
            }
        }


        public Category SelectedSearchCategory
        {
            get
            {
                return _selectedSearchCategory;
            }
            set
            {
                _selectedSearchCategory = value;
                RaisePropertyChanged("SelectedSearchCategory");
            }
        }


        public List<string> Suburb
        {
            get
            {
                return _suburb;
            }
            set
            {
                _suburb = value;
                RaisePropertyChanged("Suburb");
            }
        }

        public string SelectedState
        {
            get
            {
                return _selectedState;
            }
            set
            {
                _selectedState = value;
                RaisePropertyChanged("SelectedState");
            }
        }

        public string SelectedSuburb
        {
            get
            {
                return _selectedSuburb;
            }
            set
            {
                _selectedSuburb = value;
                RaisePropertyChanged("SelectedSuburb");
            }
        }

        public ObservableCollection<Customer> SearchedCustomers
        {
            get
            {
                return _searchedCustomers;
            }
            set
            {
                _searchedCustomers = value;
                RaisePropertyChanged("SearchedCustomers");
            }
        }


        public string SelectedCustomerState
        {
            get
            {
                return _selectedCustomerState;
            }
            set
            {
                _selectedCustomerState = value;
                RaisePropertyChanged("SelectedCustomerState");

                
            }
        }

        public bool IsCreditUpdateBtnEnable
        {
            get
            {
                return _isCreditUpdateBtnEnable;
            }
            set
            {
                _isCreditUpdateBtnEnable = value;
                RaisePropertyChanged("IsCreditUpdateBtnEnable");
            }
        }

        public bool IsStopCreditEnabled
        {
            get
            {
                return _isStopCreditEnabled;
            }
            set
            {
                _isStopCreditEnabled = value;
                RaisePropertyChanged("IsStopCreditEnabled");
            }
        }

        public string EditCompanyNameVisibility
        {
            get
            {
                return _editCompanyNameVisibility;
            }
            set
            {
                _editCompanyNameVisibility = value;
                RaisePropertyChanged("EditCompanyNameVisibility");
            }
        }

        public string EditCompanyName
        {
            get
            {
                return _editCompanyName;
            }
            set
            {
                _editCompanyName = value;
                RaisePropertyChanged("EditCompanyName");
            }
        }

        public string EditNameVisibility
        {
            get
            {
                return _editNameVisibility;
            }
            set
            {
                _editNameVisibility = value;
                RaisePropertyChanged("EditNameVisibility");
            }
        }

        public string CancelEditCompanyNameVisibility
        {
            get
            {
                return _cancelEditCompanyNameVisibility;
            }
            set
            {
                _cancelEditCompanyNameVisibility = value;
                RaisePropertyChanged("CancelEditCompanyNameVisibility");
            }
        }

        public bool CustomerBottomTabEnabled
        {
            get
            {
                return _customerBottomTabEnabled;
            }
            set
            {
                _customerBottomTabEnabled = value;
                RaisePropertyChanged("CustomerBottomTabEnabled");
            }
        }

        public bool CustomerTypeEnabled
        {
            get
            {
                return _customerTypeEnabled;
            }
            set
            {
                _customerTypeEnabled = value;
                RaisePropertyChanged("CustomerTypeEnabled");
            }
        }

        public string CustomerTypeStarVisibility
        {
            get
            {
                return _customerTypeStarVisibility;
            }
            set
            {
                _customerTypeStarVisibility = value;
                RaisePropertyChanged("CustomerTypeStarVisibility");
            }
        }

        public string SearchBtnVisibility
        {
            get
            {
                return _searchBtnVisibility;
            }
            set
            {
                _searchBtnVisibility = value;
                RaisePropertyChanged("SearchBtnVisibility");
            }
        }

        public bool BrowseTabEnabled
        {
            get
            {
                return _browseTabEnabled;
            }
            set
            {
                _browseTabEnabled = value;
                RaisePropertyChanged("BrowseTabEnabled");
            }
        }

        public string CloseBtnVisible
        {
            get
            {
                return _closeBtnVisible;
            }
            set
            {
                _closeBtnVisible = value;
                RaisePropertyChanged("CloseBtnVisible");
            }
        }

        public bool IsAdminTabEnabled
        {
            get
            {
                return _isAdminTabEnabled;
            }
            set
            {
                _isAdminTabEnabled = value;
                RaisePropertyChanged("IsAdminTabEnabled");
            }
        }

        public AdminNote AdminNote
        {
            get
            {
                return _adminNote;
            }
            set
            {
                _adminNote = value;
                RaisePropertyChanged("AdminNote");
            }
        }

        public List<Country> CountryList
        {
            get
            {
                return _countryList;
            }
            set
            {
                _countryList = value;
                RaisePropertyChanged("CountryList");
            }
        }
        

        public ObservableCollection<ContactPerson> ContactPersons
        {
            get
            {
                return _contactPersons;
            }
            set
            {
                _contactPersons = value;
                RaisePropertyChanged("ContactPersons");
            }
        }

        public string AddNewContactBtnVisibility
        {
            get
            {
                return _addNewContactBtnVisibility;
            }
            set
            {
                _addNewContactBtnVisibility = value;
                RaisePropertyChanged("AddNewContactBtnVisibility");
            }
        }

        public string CloseNormlVisibility
        {
            get
            {
                return _closeNormlVisibility;
            }
            set
            {
                _closeNormlVisibility = value;
                RaisePropertyChanged("CloseNormlVisibility");
            }
        }
        
        

        public ICollectionView ItemsView
        {
            get { return _itemsView; }
        }

        #endregion

        #region COMMANDS



        public ICommand UpdateCommand
        {
            get
            {
                return _updateCommand ?? (_updateCommand = new CommandHandler(() => UpdateCustomer(), canExecute));
            }
        }

        public ICommand UpdateCustomerCreditCommand
        {
            get
            {
                return _updateCustomerCreditCommand ?? (_updateCustomerCreditCommand = new CommandHandler(() => ViewCustomerCreditForm(), canExecute));
            }
        }

        public ICommand AddNewCustomerCommand
        {
            get
            {
                return _addNewCustomerCommand ?? (_addNewCustomerCommand = new CommandHandler(() => AddNewCustomer(), canExecute));
            }
        }

        public ICommand CancelNewCustomerCommand
        {
            get
            {
                return _cancelNewCustomerCommand ?? (_cancelNewCustomerCommand = new CommandHandler(() => CancelNewCustomer(), canExecute));
            }
        }

        public ICommand AddNewCustomerDBCommand
        {
            get
            {
                return _addNewCustomerDBCommand ?? (_addNewCustomerDBCommand = new CommandHandler(() => AddNewCustomerDB(), canExecute));
            }
        }

        public ICommand AddNewCustomerCreditCommand
        {
            get
            {
                return _addNewCustomerCreditCommand ?? (_addNewCustomerCreditCommand = new CommandHandler(() => ViewAddNewCustomer(), canExecute));
            }
        }

        public ICommand AddNewNoteCommand
        {
            get
            {
                return _addNewNoteCommand ?? (_addNewNoteCommand = new CommandHandler(() => AddNewNoteDB(), canExecute));
            }
        }

        public ICommand EditCustomerCommand
        {
            get
            {
                if (_editCustomerCommand == null)
                {
                    _editCustomerCommand = new DelegateCommand(CanExecute, GoToCustomer);
                }
                return _editCustomerCommand;
            }
        }

        public ICommand ViewDocumentCommand
        {
            get
            {
                if (_viewDocumentCommand == null)
                {
                    _viewDocumentCommand = new DelegateCommand(CanExecute, ViewDocument);
                }
                return _viewDocumentCommand;
            }
        }

        public ICommand ClearSearchCommand
        {
            get
            {
                return _clearSearchCommand ?? (_clearSearchCommand = new CommandHandler(() => ClearSearchField(), canExecute));
            }
        }


        //public ICommand SelectionChangedCommand
        //{
        //    get
        //    {
        //        return _selectionChangedCommand ?? (_selectionChangedCommand = new CommandHandler(() => DiscountChanged(), canExecute));
        //    }
        //}

        public ICommand RemoveProductInstalled
        {
            get
            {
                return _removeProductInstalled ?? (_removeProductInstalled = new CommandHandler(() => RemoveProductInstalledNow(), canExecute));
            }
        }

        public ICommand AddProductInstalled
        {
            get
            {
                return _addProductInstalled ?? (_addProductInstalled = new CommandHandler(() => AddProductInstalledNow(), canExecute));
            }
        }

        public ICommand SearchCommand
        {
            get
            {
                return _searchCommand ?? (_searchCommand = new CommandHandler(() => SearchCustomer(), canExecute));
            }
        }

        public ICommand EditCompanyNameCommand
        {
            get
            {
                return _editCompanyNameCommand ?? (_editCompanyNameCommand = new CommandHandler(() => EditCompanyNameFunc(), canExecute));
            }
        }

        public ICommand CancelEditCompanyNameCommand
        {
            get
            {
                return _cancelEditCompanyNameCommand ?? (_cancelEditCompanyNameCommand = new CommandHandler(() => CancelEditCompanyNameFunc(), canExecute));
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

        public ICommand SearchCustomerCommand
        {
            get
            {
                return _searchCustomerCommand ?? (_searchCustomerCommand = new CommandHandler(() => SearchCustomerWithBackgroundWorker(), canExecute));
            }
        }

        public ICommand CustomerChangedCommand
        {
            get
            {
                return _customerChangedCommand ?? (_customerChangedCommand = new CommandHandler(() => CustomerChanged(), canExecute));
            }
        }

        public Microsoft.Practices.Prism.Commands.DelegateCommand ClosePopUpViewCommand
        {
            get { return _closePopUpViewCommand; }
        }

        public ICommand EditContactPersonCommand
        {
            get
            {
                if (_editContactPersonCommand == null)
                {
                    _editContactPersonCommand = new DelegateCommand(CanExecute, EditContactPerson);
                }
                return _editContactPersonCommand;
            }
        }

        public ICommand DeleteContactPersonCommand
        {
            get
            {
                if (_deleteContactPersonCommand == null)
                {
                    _deleteContactPersonCommand = new DelegateCommand(CanExecute, DeleteContactPerson);
                }
                return _deleteContactPersonCommand;
            }
        }

        private ICommand _rowCopyCommand;
        public ICommand RowCopyCommand
        {
            get
            {
                if (_rowCopyCommand == null)
                {
                    _rowCopyCommand = new DelegateCommand(CanExecute, CopyContactPersonRow);
                }
                return _rowCopyCommand;
            }
        }

        public ICommand AddNewContactPersonCommand
        {
            get
            {
                return _addNewContactPersonCommand ?? (_addNewContactPersonCommand = new CommandHandler(() => AddNewContactPerson(), canExecute));
            }
        }

        public ICommand CountryChangedCommand
        {
            get
            {
                return _countryChangedCommand ?? (_countryChangedCommand = new CommandHandler(() => CountryChanged(), canExecute));
            }
        }

        public ICommand StateChangedCommand
        {
            get
            {
                return _stateChangedCommand ?? (_stateChangedCommand = new CommandHandler(() => StateChanged(), canExecute));
            }
        }

        

        //public ICommand CheckDataChangedCommand
        //{
        //    get
        //    {
        //        return _checkDataChangedCommand ?? (_checkDataChangedCommand = new RelayCommand(
        //                  x => { CheckDataChange(); }));
        //    }
        //}

        #endregion
    }
}



