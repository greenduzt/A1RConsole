using A1RConsole.Bases;
using A1RConsole.Commands;
using A1RConsole.Core;
using A1RConsole.DB;
using A1RConsole.Models.Products;
using A1RConsole.Models.Stock;
using A1RConsole.Models.Suppliers;
using A1RConsole.ViewModels.Stock;
using Microsoft.Practices.Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace A1RConsole.ViewModels.Products
{
    public class UpdateProductViewModel : ViewModelBase
    {

        private Product _product;
        private Supplier _supplier;
        private ProductStock _productStock;
        private ProductUnit _selectedUnit;
        private StockLocation _selectedStock;
        private Supplier _selectedSupplier;
        private ProductStockMaintenance stockMaintenanceViewModel;
        private List<ProductUnit> _unit;
        private List<StockLocation> _stockLocation;
        private List<Supplier> _supplierList;
        public string userName;
        public decimal _unitsPerPack;
        private decimal _unitPrice;
        private string _selectedActive;
        private string _lastModifiedBy;
        private string _projAvaBack;
        private string _projAvaFore;
        private string _selectedType;
        private ReasonCode _selectedReasonCode;
        //private string _supplierVisibility;
        //private string _userControlHeight;
        private decimal _projAvailable;
        private decimal _qtyReserved;
        private decimal _unitCost;
        private decimal _qtyAvailable;
        private decimal _totalSupply;
        private decimal _materialCost;
        private string _quantityStr;
        private bool _isManufactured;
        private bool _isPurchased;
        private DateTime _currentDate;
        private ProductStock _selectedProduct;
        private string _chkBoxSendToWarehouseTxt;
        private string _changingQtyVisibility;
        private bool _sendToWarehouseChecked;
        List<ReasonCode> _reasonCodeList;
        public event Action<ProductStock> Closed;
        private Microsoft.Practices.Prism.Commands.DelegateCommand _closeCommand;
        private ICommand _updateCommand;
        private ICommand _updateSupplierCommand;
        private ICommand _addSupplierCommand;
        private ICommand _adjustCommand;
        private bool canExecute;

        public UpdateProductViewModel(ProductStockMaintenance stmvm, string un, bool b, Supplier s)
        {
            Unit = new List<ProductUnit>();
            SupplierList = new List<Supplier>();
            SelectedUnit = new ProductUnit();
            SelectedStock = new StockLocation();
            SelectedSupplier = new Supplier();
            Supplier = new Supplier();
            LoadSuppliers();
            LoadReasonCodes();
            SelectedProduct = new ProductStock();
            CurrentDate = DateTime.Now;
            SendToWarehouseChecked = false;
                       
            var data = UserData.UserPrivilages.SingleOrDefault(x=>x.Area == "ProductMaintenance->qty_enabled");
            if(data != null)
            {
                ChangingQtyVisibility = data.Visibility == "True" ? "Visible" : "Collapsed";
                ChkBoxSendToWarehouseTxt = "Tick to allocate stock to pending Sales Order (This criteria applies if a product has one Sales Order only!)" + System.Environment.NewLine + "This will deduct stock and send the Sales Order to warehouse";
            }


            //Verify Data
            ProductStockMaintenance verifiedProductStockMaintenance = DBAccess.GetProductInventoryInfoByStockIDProdID(stmvm.StockLocation.ID, stmvm.Product.ProductID);

            if (verifiedProductStockMaintenance.ProductStock != null)
            {
                stockMaintenanceViewModel = verifiedProductStockMaintenance;
                Product = verifiedProductStockMaintenance.Product;
                ProductStock = verifiedProductStockMaintenance.ProductStock;
                SelectedUnit.UnitName = verifiedProductStockMaintenance.Product.ProductUnit;
                SelectedStock = verifiedProductStockMaintenance.StockLocation;

                SelectedReasonCode = new ReasonCode();
                SelectedReasonCode.Code = "Select";
                SelectedType = "Select";
                
                SelectedProduct = ProductStock;
                SelectedProduct.Product = new Product();
                SelectedProduct.Product = Product;
                SelectedProduct.StockLocation = new StockLocation();
                SelectedProduct.StockLocation = verifiedProductStockMaintenance.StockLocation;

                if (verifiedProductStockMaintenance.ProductStock.SupplierProduct.Supplier.SupplierName == null)
                {                   
                    LeadTime = 0;
                }
                else
                {
                    SelectedSupplier = verifiedProductStockMaintenance.ProductStock.SupplierProduct.Supplier;
                    LeadTime = verifiedProductStockMaintenance.ProductStock.SupplierProduct.LeadTime;
                }
            }
            else
            {
                //Failed to verify
                MessageBox.Show("Failed to verify stock information", "Data Verification Failed", MessageBoxButton.OK, MessageBoxImage.Warning);
                stockMaintenanceViewModel = stmvm;
                Product = stmvm.Product;
                ProductStock = stmvm.ProductStock;
                SelectedUnit.UnitName = stmvm.Product.ProductUnit;
                SelectedStock = stmvm.StockLocation;
                if (b)
                {
                    Supplier = DBAccess.GetSupplierByProductID(stockMaintenanceViewModel.Product.ProductID);
                    SelectedSupplier = s;
                    LeadTime = Supplier.LeadTime;
                }
            }            
            
            LoadUnits();
            LoadLocation();
            LoadQtyReserved();            
            
            userName = un;
            
            IsManufactured = Product.IsManufactured;
            IsPurchased = Product.IsPurchased;
            SelectedActive = ConvertToStringActive(Product.Active);
            UnitCost = Product.UnitCost;
            UnitPrice = Product.UnitPrice;

            LastModifiedBy = ProductStock.LastUpdatedDate >= Product.LastModifiedDate ? ProductStock.UpdatedBy + " " + ProductStock.LastUpdatedDate : Product.LastModifiedBy + " " + Product.LastModifiedDate;

            QtyAvailable = ProductStock.QtyAvailable;
            TotalSupply = ProductStock.TotalSupply;
            ProjAvailable = ProductStock.ProjectedAvailable;
            UnitsPerPack = Product.UnitsPerPack;
            MaterialCost = Product.MaterialCost;
                        
            CalculateProjectedAvailable();
                                

            canExecute = true;
            _closeCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(CloseForm);
            _updateCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(UpdateProductStock);
        }


        private void LoadUnits()
        {
            Unit = DBAccess.GetAllUnitsActive(true);
        }

        private void LoadReasonCodes()
        {
            ReasonCodeList = new List<ReasonCode>();
            ReasonCodeList = DBAccess.GetReasonCodes();
            ReasonCodeList.Add( new ReasonCode() { ID = 0, Code = "Select", Reason = "" });
        }

        private void LoadLocation()
        {
            StockLocation = DBAccess.GetStockLocations();
        }

        private void LoadQtyReserved()
        {
            QtyReserved = DBAccess.GetQtyReserved(stockMaintenanceViewModel.StockLocation.ID, stockMaintenanceViewModel.Product.ProductID);
        }

        private void LoadSuppliers()
        {

            //Supplier = DBAccess.GetSupplierByProductID(stockMaintenanceViewModel.Product.ProductID);
            SupplierList = DBAccess.GetAllSuppliers();
            SupplierList.Add(new Supplier() { SupplierID = 0, SupplierName = "Select" });
            LeadTime = Supplier.LeadTime;
            SelectedSupplier = Supplier;
            if (Supplier.SupplierID == 0)
            {
                if (SelectedSupplier != null)
                {
                    SelectedSupplier = new Supplier() { SupplierID = 0, SupplierName = "Select" };
                    SelectedSupplier.SupplierName = "Select";
                }
            }
        }

        public void UpdateProductStock()
        {
            ProductStock ps = new ProductStock();
            ps.Product = Product;
            ps.StockLocation = SelectedStock;
            ps.QtyAvailable = QtyAvailable;
            ps.TotalSupply = TotalSupply;
            ps.StockLocation = SelectedStock;
            ps.UpdatedBy = userName;
            ps.LastUpdatedDate = DateTime.Now;
            if (Supplier != null)
            {
                Supplier.LeadTime = LeadTime;
            }

            //Pre check stock if it has been changed
            ObservableCollection<ProductStockMaintenance> temp= DBAccess.GetProductInventoryInfo(SelectedStock.State, "All");
            var data = temp.SingleOrDefault(x => x.Product.ProductID == Product.ProductID && x.StockLocation.ID == SelectedStock.ID);
            if (data.ProductStock.QtyAvailable != ProductStock.QtyAvailable)
            {
                MessageBox.Show("Stock information have been changed" + System.Environment.NewLine + "The new information will be updated", "Stock Information Have Been Changed", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                QtyAvailable = data.ProductStock.QtyAvailable;
                TotalSupply = data.ProductStock.TotalSupply;
                QtyReserved = DBAccess.GetQtyReserved(stockMaintenanceViewModel.StockLocation.ID, stockMaintenanceViewModel.Product.ProductID);
                ProjAvailable = data.ProductStock.ProjectedAvailable;

                ps.QtyAvailable = data.ProductStock.QtyAvailable;
                ps.TotalSupply = data.ProductStock.TotalSupply;
                ProductStock.QtyAvailable = data.ProductStock.QtyAvailable;
            }
            else
            {
                int res = DBAccess.UpdateProductAndStock(this, ps);
                if (res > 0)
                {
                    MessageBox.Show("Stock information updated successfully", "Stock Information Updated", MessageBoxButton.OK, MessageBoxImage.Information);
                    CloseFormNow(ps);
                }
                else
                {
                    MessageBox.Show("You haven't made any changes", "No Changes Were Made", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void CloseForm()
        {
            ProductStock ps = new ProductStock();
            ps.ID = 1;
            CloseFormNow(ps);
        }

        private void PreCheckStock()
        {
            
        }

        private string ConvertToStringActive(bool b)
        {
            string name = string.Empty;
            switch (b)
            {
                case true: name = "Yes";
                    break;
                case false: name = "No";
                    break;
                default:
                    break;
            }

            return name;
        }

        private void CalculateCost()
        {
            //UnitPrice = UnitsPerPack * UnitCost;
        }

        private void CalculateProjectedAvailable()
        {
           // ProjAvailable = QtyAvailable + TotalSupply;

            if (ProjAvailable < Product.OrderPoint && ProjAvailable > Product.SafetyStockQty)
            {
                ProjAvaBack = "#2A00FF";
                ProjAvaFore = "White";
            }
            else if (ProjAvailable <= Product.SafetyStockQty)
            {
                ProjAvaBack = "#E12222";
                ProjAvaFore = "White";
            }
            else
            {
                ProjAvaBack = "#EDEFF7";
                ProjAvaFore = "Black";
            }
        }

        private void CloseFormNow(ProductStock r)
        {
            if (Closed != null)
            {
                Closed(r);
            }
        }

        private void ClearSupplierDetails()
        {
            if (SelectedSupplier != null)
            {
                //SelectedSupplier.SupplierName = "Select";
            }
            Supplier = null;
        }

        private void ShowUpdateSupplierView()
        {
            var childWindow = new ChildWindow();
            //childWindow.showUpdateSupplierView_Closed += (r =>
            //{


            //});
            childWindow.ShowUpdateSupplierView(stockMaintenanceViewModel, userName, Supplier);
        }

        private void ShowAddSupplierView()
        {
            var childWindow = new ChildWindow();
            //childWindow.showUpdateSupplierView_Closed += (r =>
            //{


            //});
            childWindow.ShowAddSupplierView(stockMaintenanceViewModel, userName);
        }

        private void AdjustStock()
        {
            if (SelectedType == "Select" || SelectedType == null)
            {
                MessageBox.Show("Please enter type", "Type Required", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (SelectedProduct.Product == null)
            {
                MessageBox.Show("Please enter product", "Product Required", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (String.IsNullOrWhiteSpace(QuantityStr))
            {
                MessageBox.Show("Please enter quantity", "Quantity Required", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (SelectedReasonCode.Code == "Select" || SelectedReasonCode == null || string.IsNullOrWhiteSpace(SelectedReasonCode.Code))
            {
                MessageBox.Show("Please enter reason", "Reason Required", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (SelectedType == "Adjust Out" && Convert.ToDecimal(QuantityStr) > SelectedProduct.QtyAvailable)
            {
                MessageBox.Show("Cannot adjust out from stock due to insufficient stock" + System.Environment.NewLine + "Only " + SelectedProduct.QtyAvailable + SelectedProduct.Product.ProductUnit + " available", "Insufficient Stock", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                Tuple<int,ProductStock> res = DBAccess.UpdateStock(SelectedProduct.StockLocation.ID,SelectedProduct.Product.ProductID,SelectedProduct.TimeStamp,SelectedType,Convert.ToDecimal(QuantityStr),SelectedReasonCode.Reason);
                if (res.Item1 > 0)
                {
                    if (SendToWarehouseChecked)
                    {
                        BackgroundWorker worker = new BackgroundWorker();
                        ChildWindow LoadingScreen = new ChildWindow();
                        LoadingScreen.ShowWaitingScreen("Working");
                        worker.DoWork += (_, __) =>
                        {
                            //Then check if there are sales orders available
                            ProductStock updatedPS = new ProductStock();
                            updatedPS.QtyAvailable = res.Item2.QtyAvailable;
                            updatedPS.Product = new Product();
                            updatedPS.Product.ProductID = res.Item2.Product.ProductID;

                            StockManager stm = new StockManager();
                            stm.AllocateOrderForStock(updatedPS);

                        };
                        worker.RunWorkerCompleted += delegate(object s, RunWorkerCompletedEventArgs args)
                        {
                            LoadingScreen.CloseWaitingScreen();
                            MessageBox.Show("Stock adjusted successfully!", "", MessageBoxButton.OK, MessageBoxImage.Information);
                        };
                        worker.RunWorkerAsync();
                    }
                    else
                    {
                        MessageBox.Show("Stock adjusted successfully!", "", MessageBoxButton.OK, MessageBoxImage.Information);
                    }                    
                }
                else if (res.Item1 == -1)
                {
                    MessageBox.Show("Stock has been changed since the time you opened this form" + System.Environment.NewLine + "The product details will be reloaded", "Data Changed", MessageBoxButton.OK, MessageBoxImage.Warning);
                   // LoadProductStock();
                }
                else
                {
                    MessageBox.Show("There has been a problem and stock did not adjust", "Error Occured", MessageBoxButton.OK, MessageBoxImage.Error);                    
                }

                CloseForm();
            }
        }

        #region PUBLIC_PROPERTIES
        public Product Product
        {
            get
            {
                return _product;
            }
            set
            {
                _product = value;
                RaisePropertyChanged("Product");
            }
        }

        public Supplier Supplier
        {
            get
            {
                return _supplier;
            }
            set
            {
                _supplier = value;
                RaisePropertyChanged("Supplier");
            }
        }

        private int _leadTime;
        public int LeadTime
        {
            get
            {
                return _leadTime;
            }
            set
            {
                _leadTime = value;
                RaisePropertyChanged("LeadTime");
            }
        }



        public List<ProductUnit> Unit
        {
            get
            {
                return _unit;
            }
            set
            {
                _unit = value;
                RaisePropertyChanged("Unit");
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



        public List<StockLocation> StockLocation
        {
            get
            {
                return _stockLocation;
            }
            set
            {
                _stockLocation = value;
                RaisePropertyChanged("StockLocation");
            }
        }

        public ProductUnit SelectedUnit
        {
            get
            {
                return _selectedUnit;
            }
            set
            {
                _selectedUnit = value;
                RaisePropertyChanged("SelectedUnit");
            }
        }

        public StockLocation SelectedStock
        {
            get
            {
                return _selectedStock;
            }
            set
            {
                _selectedStock = value;
                RaisePropertyChanged("SelectedStock");

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

                //if (SelectedSupplier != null)
                //{
                //    if (SelectedSupplier.SupplierName == "Select")
                //    {
                //        if (Supplier != null)
                //        {
                //            Supplier.LeadTime = 0;
                //        }
                //        LeadTime = 0;
                //    }
                //}
            }
        }


        public bool IsManufactured
        {
            get
            {
                return _isManufactured;
            }
            set
            {
                _isManufactured = value;
                RaisePropertyChanged("IsManufactured");
                if (IsManufactured == false)
                {
                    IsPurchased = true;
                }

            }
        }

        public bool IsPurchased
        {
            get
            {
                return _isPurchased;
            }
            set
            {
                _isPurchased = value;
                RaisePropertyChanged("IsPurchased");
                if (IsPurchased == true)
                {
                    LoadSuppliers();
                    //SupplierVisibility = "Visible";
                    //UserControlHeight = "501";
                }
                else
                {
                    ClearSupplierDetails();
                    //SupplierVisibility = "Collapsed";
                    //UserControlHeight = "412";
                    IsManufactured = true;
                }
            }
        }

        public string SelectedActive
        {
            get
            {
                return _selectedActive;
            }
            set
            {
                _selectedActive = value;
                RaisePropertyChanged("SelectedActive");

            }
        }

        //public string SupplierVisibility
        //{
        //    get
        //    {
        //        return _supplierVisibility;
        //    }
        //    set
        //    {
        //        _supplierVisibility = value;
        //        RaisePropertyChanged("SupplierVisibility");

        //    }
        //}

        //public string UserControlHeight
        //{
        //    get
        //    {
        //        return _userControlHeight;
        //    }
        //    set
        //    {
        //        _userControlHeight = value;
        //        RaisePropertyChanged("UserControlHeight");
        //    }
        //}


        public decimal ProjAvailable
        {
            get
            {
                return _projAvailable;
            }
            set
            {
                _projAvailable = value;
                RaisePropertyChanged("ProjAvailable");
            }
        }

        public ProductStock ProductStock
        {
            get
            {
                return _productStock;
            }
            set
            {
                _productStock = value;
                RaisePropertyChanged("ProductStock");

            }
        }


        public string LastModifiedBy
        {
            get
            {
                return _lastModifiedBy;
            }
            set
            {
                _lastModifiedBy = value;
                RaisePropertyChanged("LastModifiedBy");

            }
        }

        public decimal QtyReserved
        {
            get
            {
                return _qtyReserved;
            }
            set
            {
                _qtyReserved = value;
                RaisePropertyChanged("QtyReserved");
            }
        }

        public string ProjAvaBack
        {
            get
            {
                return _projAvaBack;
            }
            set
            {
                _projAvaBack = value;
                RaisePropertyChanged("ProjAvaBack");
            }
        }

        public string ProjAvaFore
        {
            get
            {
                return _projAvaFore;
            }
            set
            {
                _projAvaFore = value;
                RaisePropertyChanged("ProjAvaFore");
            }
        }

        public decimal UnitsPerPack
        {
            get
            {
                return _unitsPerPack;
            }
            set
            {
                _unitsPerPack = value;
                RaisePropertyChanged("UnitsPerPack");
                CalculateCost();

            }
        }

        public decimal UnitPrice
        {
            get
            {
                return _unitPrice;
            }
            set
            {
                _unitPrice = value;
                RaisePropertyChanged("UnitPrice");
            }
        }

        public decimal UnitCost
        {
            get
            {
                return _unitCost;
            }
            set
            {
                _unitCost = value;
                RaisePropertyChanged("UnitCost");
                CalculateCost();
            }
        }

        public decimal QtyAvailable
        {
            get
            {
                return _qtyAvailable;
            }
            set
            {
                _qtyAvailable = value;
                RaisePropertyChanged("QtyAvailable");
                CalculateProjectedAvailable();
            }
        }

        public decimal TotalSupply
        {
            get
            {
                return _totalSupply;
            }
            set
            {
                _totalSupply = value;
                RaisePropertyChanged("TotalSupply");
                CalculateProjectedAvailable();
            }
        }

        public decimal MaterialCost
        {
            get
            {
                return _materialCost;
            }
            set
            {
                _materialCost = value;
                RaisePropertyChanged("MaterialCost");
            }
        }

        public List<ReasonCode> ReasonCodeList
        {
            get
            {
                return _reasonCodeList;
            }
            set
            {
                _reasonCodeList = value;
                RaisePropertyChanged("ReasonCodeList");
            }
        }

        public ProductStock SelectedProduct
        {
            get
            {
                return _selectedProduct;
            }
            set
            {
                _selectedProduct = value;
                RaisePropertyChanged("SelectedProduct");
            }
        }

        public string SelectedType
        {
            get
            {
                return _selectedType;
            }
            set
            {
                _selectedType = value;
                RaisePropertyChanged("SelectedType");

            }
        }
        
        public ReasonCode SelectedReasonCode
        {
            get
            {
                return _selectedReasonCode;
            }
            set
            {
                _selectedReasonCode = value;
                RaisePropertyChanged("SelectedReasonCode");

            }
        }

        public DateTime CurrentDate
        {
            get
            {
                return _currentDate;
            }
            set
            {
                _currentDate = value;
                RaisePropertyChanged("CurrentDate");

            }
        }

        public string QuantityStr
        {
            get
            {
                return _quantityStr;
            }
            set
            {
                _quantityStr = value;
                RaisePropertyChanged("QuantityStr");

            }
        }

        public string ChkBoxSendToWarehouseTxt
        {
            get
            {
                return _chkBoxSendToWarehouseTxt;
            }
            set
            {
                _chkBoxSendToWarehouseTxt = value;
                RaisePropertyChanged("ChkBoxSendToWarehouseTxt");

            }
        }

        public string ChangingQtyVisibility
        {
            get
            {
                return _changingQtyVisibility;
            }
            set
            {
                _changingQtyVisibility = value;
                RaisePropertyChanged("ChangingQtyVisibility");

            }
        }

        public bool SendToWarehouseChecked
        {
            get
            {
                return _sendToWarehouseChecked;
            }
            set
            {
                _sendToWarehouseChecked = value;
                RaisePropertyChanged("SendToWarehouseChecked");
            }
        }

        

        #endregion
        #region COMMANDS

        //public DelegateCommand UpdateCommand
        //{
        //    get { return _updateCommand; }
        //}

        public Microsoft.Practices.Prism.Commands.DelegateCommand CloseCommand
        {
            get { return _closeCommand; }
        }

        public ICommand UpdateCommand
        {
            get
            {
                return _updateCommand ?? (_updateCommand = new CommandHandler(() => UpdateProductStock(), canExecute));
            }
        }

        public ICommand UpdateSupplierCommand
        {
            get
            {
                return _updateSupplierCommand ?? (_updateSupplierCommand = new CommandHandler(() => ShowUpdateSupplierView(), canExecute));
            }
        }

        public ICommand AddSupplierCommand
        {
            get
            {
                return _addSupplierCommand ?? (_addSupplierCommand = new CommandHandler(() => ShowAddSupplierView(), canExecute));
            }
        }

        public ICommand AdjustCommand
        {
            get
            {
                return _adjustCommand ?? (_adjustCommand = new CommandHandler(() => AdjustStock(), true));
            }
        }

        #endregion


    }
}