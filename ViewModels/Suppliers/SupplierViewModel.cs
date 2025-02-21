using A1RConsole.Bases;
using A1RConsole.Commands;
using A1RConsole.Core;
using A1RConsole.DB;
using A1RConsole.Interfaces;
using A1RConsole.Models;
using A1RConsole.Models.Meta;
using A1RConsole.Models.Purchasing;
using A1RConsole.Models.Suppliers;
using A1RConsole.Views;
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

namespace A1RConsole.ViewModels.Suppliers
{
    public class SupplierViewModel  : ViewModelBase, IContent
    {
        private string userName;
        private string state;
        private int _mainTabSelectedIndex;
        private List<Supplier> _supplierList;
        private string _version;
        private string _companyAddress;
        private string _companyCity;
        private string _companyState;
        private string _companyCountry;
        private string _companyPostCode;
        private string _companyEmail;
        private string _companyTelephone;
        private string _companyFax;
        private string _website;
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
        private bool _active;
        private string _lastUpdatedString;
        private string _cancelNewSupplierVisibility;
        private bool canExecute;
        private string _addCommandVisibility;
        private string _newCompanyNameVisibility;
        private string _newSupplierVisibility;
        private string _updateCommandVisibility;
        private Supplier _selectedSupplier;
        private string _newCompanyName;
        private int _bottomTabSelectedIndex;
        private string _selectedCompanyNameVisibility;
        private string _searchString;
        private ICollectionView _itemsView;
        private ObservableCollection<PurchaseOrder> _purchaseOrderLines;
        private bool IsDirty { get; set; }
        public event EventHandler Closing;
        public RelayCommand CloseCommand { get; private set; }
        private ICommand _addNewSupplierDBCommand;
        private ICommand _cancelNewSupplierCommand;
        private ICommand _addNewSupplierCommand;
        private ICommand _updateCommand;
        private ICommand _editSupplierCommand;
        private ICommand _clearSearchCommand;

        public SupplierViewModel()
        {
            MainTabSelectedIndex = 0;          
            canExecute = true;
            //var data = metaData.SingleOrDefault(x => x.KeyName == "version");
            //Version = data.Description;
            AddCommandVisibility = "Collapsed";
            CancelNewSupplierVisibility = "Collapsed";
            NewCompanyNameVisibility = "Collapsed";
            NewSupplierVisibility = "Visible";
            SelectedCompanyNameVisibility = "Visible";
            UpdateCommandVisibility = "Visible";
            BottomTabSelectedIndex = 0;
            SupplierList = new List<Supplier>();
            PurchaseOrderLines = new ObservableCollection<PurchaseOrder>();
            BackgroundWorker worker = new BackgroundWorker();
            ChildWindow LoadingScreen = new ChildWindow();
            LoadingScreen.ShowWaitingScreen("Loading");

            worker.DoWork += (_, __) =>
            {
                LoadSupplierList();
            };

            worker.RunWorkerCompleted += delegate(object s, RunWorkerCompletedEventArgs args)
            {
                LoadingScreen.CloseWaitingScreen();
            };
            worker.RunWorkerAsync();
            this.CloseCommand = new RelayCommand(CloseWindow);
        }


        private void LoadSupplierList()
        {
            SupplierList = DBAccess.GetAllSuppliers();

        }

        public string Title
        {
            get
            {
                return "Supplier";
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

        private void AddNewSupplier()
        {
            Active = true;
            SelectedCompanyNameVisibility = "Collapsed";
            SelectedSupplier = null;
            NewSupplierVisibility = "Collapsed";
            LastUpdatedString = string.Empty;
            CancelNewSupplierVisibility = "Visible";
            AddCommandVisibility = "Visible";
            NewCompanyNameVisibility = "Visible";
            UpdateCommandVisibility = "Collapsed";
            Clear();
            PurchaseOrderLines = new ObservableCollection<PurchaseOrder>();
        }


        private void CancelNewSupplier()
        {
            SelectedCompanyNameVisibility = "Visible";
            NewSupplierVisibility = "Visible";
            AddCommandVisibility = "Collapsed";
            CancelNewSupplierVisibility = "Collapsed";
            SelectedSupplier = null;
            NewCompanyNameVisibility = "Collapsed";
            UpdateCommandVisibility = "Visible";
            Clear();
        }

        private void AddNewSupplierDB()
        {
            if (string.IsNullOrWhiteSpace(NewCompanyName))
            {
                Msg.Show("Supplier name required", "Supplier Name Required", MsgBoxButtons.OK, MsgBoxImage.Information_Red);

            }
            else
            {
                Supplier newSupplier = new Supplier();
                newSupplier.SupplierName = NewCompanyName;
                newSupplier.SupplierAddress = CompanyAddress;
                newSupplier.SupplierSuburb = CompanyCity;
                newSupplier.SupplierState = CompanyState;
                newSupplier.SupplierCountry = CompanyCountry;
                newSupplier.SupplierPostCode = CompanyPostCode;
                newSupplier.SupplierEmail = CompanyEmail;
                newSupplier.SupplierTelephone = CompanyTelephone;
                newSupplier.SupplierFax = CompanyFax;
                newSupplier.SupplierWebUrl = Website;
                newSupplier.Designation1 = Designation1;
                newSupplier.FirstName1 = FirstName1;
                newSupplier.LastName1 = LastName1;
                newSupplier.Telephone1 = Telephone1;
                newSupplier.Mobile1 = Mobile1;
                newSupplier.Fax1 = Fax1;
                newSupplier.Email1 = Email1;

                newSupplier.Designation2 = Designation2;
                newSupplier.FirstName2 = FirstName2;
                newSupplier.LastName2 = LastName2;
                newSupplier.Telephone2 = Telephone2;
                newSupplier.Mobile2 = Mobile2;
                newSupplier.Fax2 = Fax2;
                newSupplier.Email2 = Email2;

                newSupplier.Designation3 = Designation3;
                newSupplier.FirstName3 = FirstName3;
                newSupplier.LastName3 = LastName3;
                newSupplier.Telephone3 = Telephone3;
                newSupplier.Mobile3 = Mobile3;
                newSupplier.Fax3 = Fax3;
                newSupplier.Email3 = Email3;

                newSupplier.Active = true;
                newSupplier.LastUpdatedBy = userName;
                newSupplier.LastUpdatedDateTime = DateTime.Now;

                int res = DBAccess.InsertSupplier(newSupplier);

                if (res == -2)
                {
                    Msg.Show("Name with the supplier '" + NewCompanyName + "' exists in the database" + System.Environment.NewLine + "Please choose another name", "Supplier Exists", MsgBoxButtons.OK, MsgBoxImage.Information_Orange);
                }
                else if (res == 0)
                {
                    Msg.Show("There has been a problem and the new supplier did not add to the database", "Problem Occured", MsgBoxButtons.OK, MsgBoxImage.Information_Red);
                }
                else
                {
                    Msg.Show("Supllier added successfully!!!", "Supplier Added", MsgBoxButtons.OK, MsgBoxImage.OK);
                    Clear();
                    LoadSupplierList();
                }
            }
        }

        private void UpdateSupplier()
        {
            if (SelectedSupplier != null)
            {
                SelectedSupplier.SupplierAddress = CompanyAddress;
                SelectedSupplier.SupplierSuburb = CompanyCity;
                SelectedSupplier.SupplierState = CompanyState;
                SelectedSupplier.SupplierCountry = CompanyCountry;
                SelectedSupplier.SupplierPostCode = CompanyPostCode;
                SelectedSupplier.SupplierEmail = CompanyEmail;
                SelectedSupplier.SupplierTelephone = CompanyTelephone;
                SelectedSupplier.SupplierFax = CompanyFax;
                SelectedSupplier.SupplierWebUrl = Website;
                SelectedSupplier.Designation1 = Designation1;
                SelectedSupplier.FirstName1 = FirstName1;
                SelectedSupplier.LastName1 = LastName1;
                SelectedSupplier.Telephone1 = Telephone1;
                SelectedSupplier.Mobile1 = Mobile1;
                SelectedSupplier.Fax1 = Fax1;
                SelectedSupplier.Email1 = Email1;
                SelectedSupplier.Designation2 = Designation2;
                SelectedSupplier.FirstName2 = FirstName2;
                SelectedSupplier.LastName2 = LastName2;
                SelectedSupplier.Telephone2 = Telephone2;
                SelectedSupplier.Mobile2 = Mobile2;
                SelectedSupplier.Fax2 = Fax2;
                SelectedSupplier.Email2 = Email2;
                SelectedSupplier.Designation3 = Designation3;
                SelectedSupplier.FirstName3 = FirstName3;
                SelectedSupplier.LastName3 = LastName3;
                SelectedSupplier.Telephone3 = Telephone3;
                SelectedSupplier.Mobile3 = Mobile3;
                SelectedSupplier.Fax3 = Fax3;
                SelectedSupplier.Email3 = Email3;
                SelectedSupplier.Active = Active;

                int res = DBAccess.UpdateSupplier(SelectedSupplier, userName);
                if (res == 1)
                {
                    Msg.Show("Supplier updated successfully", "Supplier Updated", MsgBoxButtons.OK, MsgBoxImage.OK);
                    Clear();
                    LoadSupplierList();
                }
                else
                {
                    Msg.Show("You haven't made any changes to update", "No Changes Were Made", MsgBoxButtons.OK, MsgBoxImage.Information);
                }
            }
            else
            {
                Msg.Show("Please select Supplier to update", "Invalid Supplier", MsgBoxButtons.OK, MsgBoxImage.Error);
            }
        }

        private bool Filter(Supplier model)
        {
            var searchstring = (SearchString ?? string.Empty).ToLower();
            return model != null &&
                 ((model.SupplierName ?? string.Empty).ToLower().Contains(searchstring)) ||
                 ((model.SupplierAddress ?? string.Empty).ToLower().Contains(searchstring)) ||
                 ((model.SupplierSuburb ?? string.Empty).ToLower().Contains(searchstring)) ||
                 ((model.SupplierState ?? string.Empty).ToLower().Contains(searchstring)) ||
                 ((model.Email1 ?? string.Empty).ToLower().Contains(searchstring)) ||
                 ((model.Email2 ?? string.Empty).ToLower().Contains(searchstring)) ||
                 ((model.Email3 ?? string.Empty).ToLower().Contains(searchstring)) ||
                 ((model.FirstName1 ?? string.Empty).ToLower().Contains(searchstring)) ||
                 ((model.FirstName2 ?? string.Empty).ToLower().Contains(searchstring)) ||
                 ((model.FirstName3 ?? string.Empty).ToLower().Contains(searchstring)) ||
                 ((model.LastName1 ?? string.Empty).ToLower().Contains(searchstring)) ||
                 ((model.LastName2 ?? string.Empty).ToLower().Contains(searchstring)) ||
                 ((model.LastName3 ?? string.Empty).ToLower().Contains(searchstring)) ||
                 ((model.Telephone1 ?? string.Empty).ToLower().Contains(searchstring)) ||
                 ((model.Telephone2 ?? string.Empty).ToLower().Contains(searchstring)) ||
                 ((model.Telephone3 ?? string.Empty).ToLower().Contains(searchstring)) ||
                 ((model.SupplierEmail ?? string.Empty).ToLower().Contains(searchstring)) ||
                 ((model.SupplierTelephone ?? string.Empty).ToLower().Contains(searchstring));
        }

        private void ClearSearchField()
        {
            SearchString = string.Empty;
        }

        #region CLEAR_FIELDS
        private void Clear()
        {
            NewCompanyName = String.Empty;
            CompanyAddress = string.Empty;
            CompanyCity = string.Empty;
            CompanyState = string.Empty;
            CompanyCountry = string.Empty;
            CompanyPostCode = string.Empty;
            CompanyEmail = string.Empty;
            CompanyTelephone = string.Empty;
            CompanyFax = string.Empty;
            Website = string.Empty;

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


        }

        #endregion

        private bool CanExecute(object parameter)
        {
            return true;
        }

        private void GoToSupplier(object parameter)
        {

            int index = SupplierList.IndexOf(parameter as Supplier);
            if (index > -1 && index < SupplierList.Count)
            {
                MainTabSelectedIndex = 0;
                //SelectedCustomer = null;            
                SelectedSupplier = SupplierList[index];
            }
        }

        private void LoadPurchaseOrderLines()
        {
            if (SelectedSupplier != null)
            {
                PurchaseOrderLines = DBAccess.GetPurchasingOrders(SelectedSupplier.SupplierID);
            }
        }

        #region PUBLIC_PROPERTIES

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

        public string CompanyCountry
        {
            get
            {
                return _companyCountry;
            }
            set
            {
                _companyCountry = value;
                RaisePropertyChanged("CompanyCountry");
            }
        }

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

        public string Website
        {
            get
            {
                return _website;
            }
            set
            {
                _website = value;
                RaisePropertyChanged("Website");
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

                LoadSupplierList();

                if (MainTabSelectedIndex == 0)
                {
                    BottomTabSelectedIndex = 0;
                }
                else
                {
                    _itemsView = CollectionViewSource.GetDefaultView(SupplierList);
                    _itemsView.Filter = x => Filter(x as Supplier);
                }
            }
        }

        public List<Supplier> SupplierList
        {
            get
            {
                return _supplierList;
            }
            set
            {
                _supplierList = value;
                RaisePropertyChanged("SupplierList");
            }
        }



        public Supplier SelectedSupplier
        {
            get
            {
                return _selectedSupplier;
            }
            set
            {
                _selectedSupplier = value;
                RaisePropertyChanged("SelectedSupplier");
                if (SelectedSupplier != null)
                {
                    CompanyAddress = SelectedSupplier.SupplierAddress;
                    CompanyCity = SelectedSupplier.SupplierSuburb;
                    CompanyState = SelectedSupplier.SupplierState;
                    CompanyCountry = SelectedSupplier.SupplierCountry;
                    CompanyPostCode = SelectedSupplier.SupplierPostCode;
                    CompanyEmail = SelectedSupplier.SupplierEmail;
                    CompanyTelephone = SelectedSupplier.SupplierTelephone;
                    CompanyFax = SelectedSupplier.SupplierFax;
                    Website = SelectedSupplier.SupplierWebUrl;
                    Designation1 = SelectedSupplier.Designation1;
                    FirstName1 = SelectedSupplier.FirstName1;
                    LastName1 = SelectedSupplier.LastName1;
                    Telephone1 = SelectedSupplier.Telephone1;
                    Mobile1 = SelectedSupplier.Mobile1;
                    Fax1 = SelectedSupplier.Fax1;
                    Email1 = SelectedSupplier.Email1;
                    Designation2 = SelectedSupplier.Designation2;
                    FirstName2 = SelectedSupplier.FirstName2;
                    LastName2 = SelectedSupplier.LastName2;
                    Telephone2 = SelectedSupplier.Telephone2;
                    Mobile2 = SelectedSupplier.Mobile2;
                    Fax2 = SelectedSupplier.Fax2;
                    Email2 = SelectedSupplier.Email2;
                    Designation3 = SelectedSupplier.Designation3;
                    FirstName3 = SelectedSupplier.FirstName3;
                    LastName3 = SelectedSupplier.LastName3;
                    Telephone3 = SelectedSupplier.Telephone3;
                    Mobile3 = SelectedSupplier.Mobile3;
                    Fax3 = SelectedSupplier.Fax3;
                    Email3 = SelectedSupplier.Email3;
                    LastUpdatedString = String.IsNullOrWhiteSpace(SelectedSupplier.LastUpdatedBy) ? "" : SelectedSupplier.LastUpdatedBy + " at " + SelectedSupplier.LastUpdatedDateTime;
                    Active = SelectedSupplier.Active;
                    LoadPurchaseOrderLines();
                }
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

        public string CancelNewSupplierVisibility
        {
            get
            {
                return _cancelNewSupplierVisibility;
            }
            set
            {
                _cancelNewSupplierVisibility = value;
                RaisePropertyChanged("CancelNewSupplierVisibility");
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

        public string NewSupplierVisibility
        {
            get
            {
                return _newSupplierVisibility;
            }
            set
            {
                _newSupplierVisibility = value;
                RaisePropertyChanged("NewSupplierVisibility");
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

                if (BottomTabSelectedIndex == 0)
                {
                }
                else if (BottomTabSelectedIndex == 1)
                {

                }
                else if (BottomTabSelectedIndex == 2)
                {
                    LoadPurchaseOrderLines();
                }

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

        public ObservableCollection<PurchaseOrder> PurchaseOrderLines
        {
            get
            {
                return _purchaseOrderLines;
            }
            set
            {
                _purchaseOrderLines = value;
                RaisePropertyChanged("PurchaseOrderLines");
            }
        }

        #endregion

        #region COMMANDS

        public ICommand UpdateCommand
        {
            get
            {
                return _updateCommand ?? (_updateCommand = new CommandHandler(() => UpdateSupplier(), canExecute));
            }
        }
              

        public ICommand AddNewSupplierDBCommand
        {
            get
            {
                return _addNewSupplierDBCommand ?? (_addNewSupplierDBCommand = new CommandHandler(() => AddNewSupplierDB(), canExecute));
            }
        }

        public ICommand CancelNewSupplierCommand
        {
            get
            {
                return _cancelNewSupplierCommand ?? (_cancelNewSupplierCommand = new CommandHandler(() => CancelNewSupplier(), canExecute));
            }
        }

        public ICommand AddNewSupplierCommand
        {
            get
            {
                return _addNewSupplierCommand ?? (_addNewSupplierCommand = new CommandHandler(() => AddNewSupplier(), canExecute));
            }
        }

        public ICommand EditSupplierCommand
        {
            get
            {
                if (_editSupplierCommand == null)
                {
                    _editSupplierCommand = new A1RConsole.Commands.DelegateCommand(CanExecute, GoToSupplier);
                }
                return _editSupplierCommand;
            }
        }

        public ICommand ClearSearchCommand
        {
            get
            {
                return _clearSearchCommand ?? (_clearSearchCommand = new CommandHandler(() => ClearSearchField(), canExecute));
            }
        }


        #endregion
    }
}
