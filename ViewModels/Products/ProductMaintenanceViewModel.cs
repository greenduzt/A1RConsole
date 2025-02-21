using A1RConsole.Bases;
using A1RConsole.Commands;
using A1RConsole.Core;
using A1RConsole.DB;
using A1RConsole.Interfaces;
using A1RConsole.Models.Categories;
using A1RConsole.Models.Discounts;
using A1RConsole.Models.Meta;
using A1RConsole.Models.Orders;
using A1RConsole.Models.Products;
using A1RConsole.Models.Purchasing;
using A1RConsole.Models.Stock;
using A1RConsole.Models.Suppliers;
using A1RConsole.Models.Users;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;

namespace A1RConsole.ViewModels.Products
{
    public class ProductMaintenanceViewModel : ViewModelBase, IContent
    {
        //public ObservableCollection<Product> Product { get; set; }

        private ObservableCollection<ProductStock> _productStock;
        private List<string> _productUnit;
        private List<string> _commodityCodes;
        private List<string> _commodityCodesTransactions;
        private List<Category> _categories;
        private List<Supplier> _suppliers;
        private ObservableCollection<SalesOrder> _salesOrderLine;
        private ProductStock _selectedProduct;
        private string _selectedProductUnit;
        private string _selectedCommodityCode;
        private Category _selectedCatgeory;
        private string userName;
        private string state;
        private Supplier _selectedSupplier;
        private int _mainTabSelectedIndex;
        private int _bottomTabSelectedIndex;
        private bool _isInactive;
        private decimal _qtyOnHold;
        private string _totalsString;
        private string _searchString;
        private ICollectionView _itemsView;
        private ChildWindow LoadingScreen;
        private ObservableCollection<ProductStockMaintenance> _productStockMaintenance;
        private ObservableCollection<ProductStockMaintenance> mainProductStockMaintenance;
        private bool _isQldInventoryChecked;
        private bool _isNswInventoryChecked;
        private bool _allInventoriesChecked;
        private bool _isPurchasedProducts;
        private bool _isFabricatedProducts;
        private bool _isBoth;
        private string _inventoryClicked;
        private int _inventoryProdClicked;
        private string _selectedCommodityCodeTransaction;
        private ObservableCollection<Product> _product;
        private Product _prodTransselectedProduct;
        private ObservableCollection<ProductTransactions> _productTransactions;
        private ObservableCollection<ProductTransactions> productTransactions;
        private Category _selectedCatgeoryTransaction;
        private string _version;
        private bool canExecute;
        private bool _lastWeekChecked;
        private bool _lastTwoWeeksChecked;
        private bool _lastThreeWeeksChecked;
        private bool _lastMonthChecked;
        private bool _dateRangeChecked;
        private bool _dateRangeEnabled;
        private bool _allChecked;
        private bool isSearchString;
        private string _prodCostVisibility;
        private string whichDuration;
        private string productSource;
        private string _selectedProductUnitTransaction;
        private List<Category> _categoriesTransaction;
        private DateTime _currentDate;
        private DateTime _fromSelectedDate;
        private DateTime _toSelectedDate;
        private bool IsDirty { get; set; }
        private string _noOfRecords;
        private bool _isQldProdInventoryChecked;
        private bool _isNswProdInventoryChecked;
        private string _listPriceMaskVisibility;
        private string _viewEditButtonVisibility;
        private string _changingQtyVisibility;
        private List<MetaData> metaData;
        public event EventHandler Closing;
        public ProductStockNotifier productStockNotifier { get; set; }
        public ProductStockReservedNotifier productStockReservedNotifier { get; set; }
        public PurchasingOrderQtyNotifier purchasingOrderQtyNotifier { get; set; }
 
        public RelayCommand CloseCommand { get; private set; }
        private ICommand _updateProductCommand;
        private ICommand _clearProductCommand;
        private ICommand _updateCommand;
        private ICommand _checkTransactionsCommand;
        private ICommand _searchTransactionCommand;
        private ICommand _searchProductCommand;
        private ICommand _clearTransactionCommand;
        private ICommand _refreshDataCommand;
        //private ICommand _lostFocusCommand;
       
        public ProductMaintenanceViewModel()
        {
            

            canExecute = true;
            BottomTabSelectedIndex = 0;
            IsQldInventoryChecked = true;
            IsNswInventoryChecked = false;
            AllInventoriesChecked = false;
            IsQldProdInventoryChecked = true;
            IsNswProdInventoryChecked = false;
            IsPurchasedProducts = true;
            IsFabricatedProducts = false;
            IsBoth = false;
            SalesOrderLine = new ObservableCollection<SalesOrder>();
            //var data = metaData.SingleOrDefault(z => z.KeyName == "version");
            //Version = data.Description;
            Suppliers = new List<Supplier>();
            Categories = new List<Category>();
            CurrentDate = DateTime.Now;
            ProductStockMaintenance = new ObservableCollection<ProductStockMaintenance>();
            mainProductStockMaintenance = new ObservableCollection<ProductStockMaintenance>();


            this.productStockNotifier = new ProductStockNotifier();
            this.productStockNotifier.NewMessage += new EventHandler<SqlNotificationEventArgs>(notifier_NewMessage);
            List<ProductStock> ps = this.productStockNotifier.RegisterDependency();
            this.LoadProductStock(ps);

            this.productStockReservedNotifier = new ProductStockReservedNotifier();
            this.productStockReservedNotifier.NewMessage += new EventHandler<SqlNotificationEventArgs>(notifier_NewMessage2);
            List<ProductStock> proStockList = this.productStockReservedNotifier.RegisterDependency();
            this.LoadProductStockReserved(proStockList);

            this.purchasingOrderQtyNotifier = new PurchasingOrderQtyNotifier();
            this.purchasingOrderQtyNotifier.NewMessage += new EventHandler<SqlNotificationEventArgs>(notifier_NewMessage3);
            List<PurchaseOrderDetails> poTotal = this.purchasingOrderQtyNotifier.RegisterDependency();
            this.LoadPurchasingOrderTotal(poTotal);

            BackgroundWorker worker = new BackgroundWorker();
            LoadingScreen = new ChildWindow();
            LoadingScreen.ShowWaitingScreen("Loading");
            worker.DoWork += (_, __) =>
            {
                LoadAllProducts();
                LoadSuppliers();

            };
            worker.RunWorkerCompleted += delegate(object s, RunWorkerCompletedEventArgs args)
            {
                LoadingScreen.CloseWaitingScreen();

            };
            worker.RunWorkerAsync();

            if (UserData.UserPrivilages != null)
            {
                foreach (var item in UserData.UserPrivilages)
                {
                    if (item.Area.Trim() == "Product_Maintenance_Product_Cost")
                    {
                        ProdCostVisibility = item.Visibility;
                        if (item.Visibility == "Visible")
                        {
                            ListPriceMaskVisibility = "Collapsed";
                            ViewEditButtonVisibility = "Collapsed";
                        }
                        else
                        {
                            ListPriceMaskVisibility = "Visible";
                            ViewEditButtonVisibility = "Visible";
                        }                       
                    }
                   
                    if(item.Area.Trim() == "ProductMaintenance->qty_enabled")
                    {
                        ChangingQtyVisibility = item.Visibility == "True" ? "Visible" : "Collapsed";
                    }
                }
            }           

                

            this.CloseCommand = new RelayCommand(CloseWindow);
        }       

        void notifier_NewMessage(object sender, SqlNotificationEventArgs e)
        {
            LoadProductStock(this.productStockNotifier.RegisterDependency());
        }

        void notifier_NewMessage2(object sender, SqlNotificationEventArgs e)
        {
            LoadProductStockReserved(this.productStockReservedNotifier.RegisterDependency());
        }

        void notifier_NewMessage3(object sender, SqlNotificationEventArgs e)
        {
            LoadPurchasingOrderTotal(this.purchasingOrderQtyNotifier.RegisterDependency());
        }


        private void LoadProductStock(List<ProductStock> ps)
        {           
            if (ps != null)
            {
                if(MainTabSelectedIndex==0)
                {
                    if(SelectedProduct != null)
                    {
                        var d = ps.SingleOrDefault(x=>x.Product.ProductID == SelectedProduct.Product.ProductID && x.StockLocation.ID == SelectedProduct.StockLocation.ID);
                        if (d != null && (SelectedProduct.TimeStamp != d.TimeStamp || SelectedProduct.Product.TimeStamp != d.Product.TimeStamp))
                        {
                            MessageBox.Show("Data has changed since you opened this form" + System.Environment.NewLine + "Form will be updated with new data", "Data Changed", MessageBoxButton.OK, MessageBoxImage.Warning);
                            ProductStock productStock = DBAccess.GetSelectedProductStock(SelectedProduct.StockLocation.ID, SelectedProduct.Product.ProductID);
                            SelectedProduct = productStock;
                            CalculateProjectedAvailable();
                        }
                    }
                }
                else if(MainTabSelectedIndex==1)
                {

                }
            }           
        }

        private void LoadProductStockReserved(List<ProductStock> pSList)
        {
            if (pSList != null)
            {
                if (MainTabSelectedIndex == 0)
                {                    
                    if (SelectedProduct != null)
                    {
                        var d = pSList.SingleOrDefault(x => x.Product.ProductID == SelectedProduct.Product.ProductID && x.StockLocation.ID == SelectedProduct.StockLocation.ID);
                        if (d != null && (SelectedProduct.StockLocation.ID == d.StockLocation.ID && SelectedProduct.Product.ProductID == d.Product.ProductID))
                        {
                            MessageBox.Show("Data has changed since you opened this form" + System.Environment.NewLine + "Form will be updated with new data", "Data Changed", MessageBoxButton.OK, MessageBoxImage.Warning);

                            SelectedProduct.QtyOnHold = d.QtyOnHold;
                            CalculateProjectedAvailable();
                        }
                    }
                }
            }
        }
        //ON ORDER FROM SUPPLY
        private void LoadPurchasingOrderTotal(List<PurchaseOrderDetails> pSList)
        {
            if (pSList != null)
            {
                if (MainTabSelectedIndex == 0)
                {
                    if (SelectedProduct != null)
                    {
                        var d = pSList.SingleOrDefault(x => x.Product.ProductID == SelectedProduct.Product.ProductID);
                        if (d != null)
                        {
                            decimal totalSupply = d.RecievedQty < d.OrderQty ? d.OrderQty - d.RecievedQty : d.OrderQty;

                            if (SelectedProduct.Product.ProductID == d.Product.ProductID && SelectedProduct.TotalSupply != totalSupply)
                            {
                                MessageBox.Show("On Order From Supply has changed since you opened this form" + System.Environment.NewLine + "Form will be updated with new Total Supply", "On Order From Supply Chanaged", MessageBoxButton.OK, MessageBoxImage.Warning);

                                SelectedProduct.TotalSupply = totalSupply;
                                CalculateProjectedAvailable();
                            }
                        }
                    }
                }
            }
        }

        private void CalculateProjectedAvailable()
        {
            SelectedProduct.ProjectedAvailable = SelectedProduct.QtyAvailable + SelectedProduct.TotalSupply;
        }

        public string Title
        {
            get
            {
                return "Product Maintenance";
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

        private void LoadAllProducts()
        {
            ProductStock = DBAccess.GetAllProductStock(InventoryProdClicked);

            ProductUnit = ProductStock.GroupBy(x => x.Product.ProductUnit)
                             .Where(g => g.Count() > 1)
                             .Select(g => g.Key).OrderBy(x => x)
                             .ToList();           

            CommodityCodes = ProductStock.GroupBy(x => x.Product.CommodityCode)
                             .Where(g => g.Count() > 1)
                             .Select(g => g.Key).OrderBy(x => x)
                             .ToList();

            CommodityCodesTransactions = CommodityCodes;

            /*Categories = ProductStock.GroupBy(x => x.Product.Category.CategoryName)
                             .Where(g => g.Count() > 1)
                             .Select(g => g.Key).OrderBy(x => x)
                             .ToList();
              */
            //List<Category> c = new List<Category>();
            Categories = ProductStock
                                     .GroupBy(l => l.Product.Category.CategoryName)
                                     .Select(cl => new Category
                                     {
                                         CategoryID = cl.First().Product.Category.CategoryID,
                                         CategoryName = cl.First().Product.Category.CategoryName,
                                     })
                                     .OrderBy(x => x.CategoryName)
                                     .ToList();

            CategoriesTransaction = Categories;
        }

        private void LoadSuppliers()
        {
            Suppliers = DBAccess.GetAllSuppliers(true);
            Suppliers.Add(new Supplier() { SupplierID = 0, SupplierName = "No Supplier" });
        }

        private void UpdateProduct()
        {
            if (SelectedProduct != null)
            {
                Tuple<int, string, SupplierProduct> res = null;
                BackgroundWorker worker = new BackgroundWorker();
                LoadingScreen = new ChildWindow();
                LoadingScreen.ShowWaitingScreen("Loading");
                worker.DoWork += (_, __) =>
                {
                    SelectedProduct.Product.Category = SelectedCatgeory;
                    SelectedProduct.Product.CommodityCode = SelectedCommodityCode;
                    SelectedProduct.UpdatedBy = UserData.UserName;
                    res = DBAccess.UpdateProductStock(SelectedProduct);
                };
                worker.RunWorkerCompleted += delegate(object s, RunWorkerCompletedEventArgs args)
                {
                    LoadingScreen.CloseWaitingScreen();
                    if (res.Item1 == 0)
                    {
                        MessageBox.Show("No changes were detected to update this product", "No Changes Detected", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                    }
                    else if (res.Item1 == 1)
                    {
                        MessageBox.Show("Product updated successfully", "Update Successfull", MessageBoxButton.OK, MessageBoxImage.Information);
                        SelectedProduct.Product.LastModifiedBy = res.Item2;
                    }
                    else if (res.Item1 == 2)
                    {
                        MessageBox.Show("Someone has changed the supplier for this product to " + res.Item3.Supplier.SupplierName + System.Environment.NewLine + "The new supplier will be added to this product", "Update Successfull", MessageBoxButton.OK, MessageBoxImage.Warning);
                        SelectedProduct.SupplierProduct = res.Item3;
                    }    
                };
                worker.RunWorkerAsync();
                           
            }
            else
            {
                MessageBox.Show("Please select product to update", "Product Required", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearProduct()
        {
            BackgroundWorker worker = new BackgroundWorker();
            LoadingScreen = new ChildWindow();
            LoadingScreen.ShowWaitingScreen("Loading");
            worker.DoWork += (_, __) =>
            {
                SelectedProduct = null;
                IsInactive = false;
                LoadAllProducts();

            };
            worker.RunWorkerCompleted += delegate(object s, RunWorkerCompletedEventArgs args)
            {
                LoadingScreen.CloseWaitingScreen();

            };
            worker.RunWorkerAsync();
        }

        private bool Filter(ProductStockMaintenance model)
        {
            var searchstring = (SearchString ?? string.Empty).ToLower();

            return model != null &&
                 //((model.Product.CommodityCode ?? string.Empty).ToLower().Contains(searchstring)) ||
                 ((model.Product.ProductCode ?? string.Empty).ToLower().Contains(searchstring) ||
                 ((model.Product.ProductDescription ?? string.Empty).ToLower().Contains(searchstring)));
        }

        private void LoadData()
        {
            BackgroundWorker worker = new BackgroundWorker();
            LoadingScreen = new ChildWindow();
            LoadingScreen.ShowWaitingScreen("Loading");
            worker.DoWork += (_, __) =>
            {                
                ProductStockMaintenance = DBAccess.GetProductInventoryInfo(InventoryClicked, productSource);
                UserPrivilages up = new UserPrivilages();

                if (UserData.UserPrivilages != null)
                {
                    up = UserData.UserPrivilages.SingleOrDefault(x => x.Area.Trim() == "ProductMaintenance->qty_enabled");
                }

                
                
                foreach (var item in ProductStockMaintenance)
                {                    
                    item.QtyAvailbleEnabled = up != null ? Convert.ToBoolean(up.Visibility) : false;                    

                    if (item.ProductStock.ProjectedAvailable < item.Product.OrderPoint && item.ProductStock.ProjectedAvailable > item.Product.SafetyStockQty)
                    {
                        item.ProjAvaCellBack = "#2A00FF";
                        item.ProjAvaCellFore = "White";
                    }
                    else if (item.ProductStock.ProjectedAvailable <= item.Product.SafetyStockQty)
                    {
                        item.ProjAvaCellBack = "#E12222";
                        item.ProjAvaCellFore = "White";
                    }
                    else
                    {
                        item.ProjAvaCellBack = "White";
                        item.ProjAvaCellFore = "Black";
                    }

                    item.Product.LastModifiedBy = item.ProductStock.LastUpdatedDate >= item.Product.LastModifiedDate ? item.ProductStock.UpdatedBy + " " + item.ProductStock.LastUpdatedDate : item.Product.LastModifiedBy + " " + item.Product.LastModifiedDate;
                    App.Current.Dispatcher.Invoke((Action)delegate
                    {
                        mainProductStockMaintenance.Add(item);
                    });                    
                }                
            };
            worker.RunWorkerCompleted += delegate(object s, RunWorkerCompletedEventArgs args)
            {
                LoadingScreen.CloseWaitingScreen();
                _itemsView = CollectionViewSource.GetDefaultView(ProductStockMaintenance);
                _itemsView.Filter = x => Filter(x as ProductStockMaintenance);
            };
            worker.RunWorkerAsync();
                      
        }

        private void CheckWhichInventoryClicked()
        {
            if (IsQldInventoryChecked)
            {
                InventoryClicked = "QLD";
            }
            else if (IsNswInventoryChecked)
            {
                InventoryClicked = "NSW";
            }
            else if (AllInventoriesChecked)
            {
                InventoryClicked = "All";
            }
            SearchString = string.Empty;
            SelectedCommodityCode = string.Empty;
            LoadData();
            if (_itemsView != null)
            {
                _itemsView = CollectionViewSource.GetDefaultView(ProductStockMaintenance);
                _itemsView.Filter = x => Filter(x as ProductStockMaintenance);
            }
        }

        private void CheckWhichProdInventoryClicked()
        {
            if (IsQldProdInventoryChecked)
            {
                InventoryProdClicked = 1;
            }
            else if (IsNswProdInventoryChecked)
            {
                InventoryProdClicked = 2;
            }
            ClearProduct();
        }

        private void CheckProductSource()
        {
            if(IsPurchasedProducts)
            {
                productSource = "Purchased";
            }
            else if(IsFabricatedProducts)
            {
                productSource = "Manufactured";
            }
            else if(IsBoth)
            {
                productSource = "All";
            }

            SearchString = string.Empty;
            SelectedCommodityCode = string.Empty;
            LoadData();
            if (_itemsView != null)
            {
                _itemsView = CollectionViewSource.GetDefaultView(ProductStockMaintenance);
                _itemsView.Filter = x => Filter(x as ProductStockMaintenance);
            }
        }

        private void EditStock(object parameter)
        {
            bool b = false;
            Supplier s = new Supplier();
            int index = ProductStockMaintenance.IndexOf(parameter as ProductStockMaintenance);
            if (index > -1 && index < ProductStockMaintenance.Count)
            {
                ObservableCollection<SalesOrderDetails> salesOrderDetails = new ObservableCollection<SalesOrderDetails>();
                salesOrderDetails.Add(new SalesOrderDetails() { Product = ProductStockMaintenance[index].Product });
                ////Set the flag for Product Stock
                //Tuple<List<Tuple<User, Product>>, List<int>> res1 = DBAccess.SetEditModeProductStock(salesOrderDetails, ProductStockMaintenance[index].StockLocation.ID, true, userName, true);

                //if (res1.Item1.Count > 0)
                //{
                //    string str = string.Empty;
                //    foreach (var item in res1.Item1)
                //    {
                //        str += item.Item2.ProductDescription + " is updating by " + item.Item1.FullName + System.Environment.NewLine;
                //    }
                //    Msg.Show("Cannot update stock at the moment" + System.Environment.NewLine + "Following products are being modified at the moment" + System.Environment.NewLine + System.Environment.NewLine + str + System.Environment.NewLine + "Try again later", "Cannot Update Stock", MsgBoxButtons.OK, MsgBoxImage.Information_Orange);
                //}
                //else
                //{
                var childWindow = new ChildWindow();
                childWindow.updateProductInventory_Closed += (r =>
                {
                    ////Un Set the flag for Product Stock
                    //Tuple<List<Tuple<User, Product>>, List<int>> res2 = DBAccess.SetEditModeProductStock(salesOrderDetails, ProductStockMaintenance[index].StockLocation.ID, false, userName, false);
                    if (r.ID == 1)
                    {
                        LoadData();
                    }

                });
                childWindow.ShowUpdateProductInventory(ProductStockMaintenance[index], userName, b, s);
                //}
            }
        }

        private void LoadProducts()
        {
            Product = DBAccess.GetAllProds(true);
        }

        private bool CanExecute(object parameter)
        {
            return true;
        }

        private void GoToTransactions()
        {
            if (SelectedProduct != null)
            {
                MainTabSelectedIndex = 2;
                ProdTransSelectedProduct = SelectedProduct.Product;
                LastMonthChecked = true;
                SearchTransactions();
            }
            else
            {
                MessageBox.Show("Please select product to see transactions", "Product Required", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        public void LoadProdTransactions()
        {
            //Duration
            DateTime from = DateTime.Now.Date;
            DateTime to = DateTime.Now.Date;


            if (whichDuration == "LastWeekChecked")
            {
                DateRangeEnabled = false;
                from = from.AddDays(-7);
            }
            else if (whichDuration == "LastTwoWeeksChecked")
            {
                DateRangeEnabled = false;
                from = from.AddDays(-14);
            }
            else if (whichDuration == "LastThreeWeeksChecked")
            {
                DateRangeEnabled = false;
                from = from.AddDays(-21);
            }
            else if (whichDuration == "LastMonthChecked")
            {
                DateRangeEnabled = false;
                from = from.AddMonths(-1);
            }
            else if (whichDuration == "DateRangeChecked")
            {
                DateRangeEnabled = true;
                from = FromSelectedDate;
                to = ToSelectedDate;
            }
            else if (whichDuration == "AllChecked")
            {
                DateRangeEnabled = false;
            }

            if (ProdTransSelectedProduct != null || SelectedCatgeoryTransaction != null || !string.IsNullOrWhiteSpace(SelectedCommodityCodeTransaction))
            {
                string where = string.Empty;
                ProductTransactions = new ObservableCollection<ProductTransactions>();
                productTransactions = new ObservableCollection<ProductTransactions>();

                //Construct where field
                if (ProdTransSelectedProduct != null && SelectedCatgeoryTransaction == null && string.IsNullOrWhiteSpace(SelectedCommodityCodeTransaction))
                {
                    where = "WHERE ProductTransactions.product_id = "+ ProdTransSelectedProduct.ProductID + " ";
                }
                else if (ProdTransSelectedProduct == null && SelectedCatgeoryTransaction != null && string.IsNullOrWhiteSpace(SelectedCommodityCodeTransaction))
                {
                    where = "WHERE Products.category_id = " + SelectedCatgeoryTransaction.CategoryID + " ";
                }
                else if (ProdTransSelectedProduct == null && SelectedCatgeoryTransaction == null && !string.IsNullOrWhiteSpace(SelectedCommodityCodeTransaction))
                {
                    where = "WHERE Products.commodity_code = '" + SelectedCommodityCodeTransaction + "' ";
                }
                else if (ProdTransSelectedProduct == null && SelectedCatgeoryTransaction != null && !string.IsNullOrWhiteSpace(SelectedCommodityCodeTransaction))
                {
                    where = "WHERE Products.category_id = " + SelectedCatgeoryTransaction.CategoryID + " AND Products.commodity_code = '" + SelectedCommodityCodeTransaction + "' ";
                }
                else if (ProdTransSelectedProduct != null && SelectedCatgeoryTransaction != null && !string.IsNullOrWhiteSpace(SelectedCommodityCodeTransaction))
                {
                    where = "WHERE ProductTransactions.product_id = " + ProdTransSelectedProduct.ProductID + " AND Products.category_id = " + SelectedCatgeoryTransaction.CategoryID + " AND Products.commodity_code = '" + SelectedCommodityCodeTransaction + "' ";
                }

                productTransactions = DBAccess.GetProductTransactions(where);

                foreach (var item in productTransactions)
                {
                    
                        if (whichDuration != "AllChecked")
                        {
                            DateTime transDate = DateTime.Parse(item.TransactionDate.ToString("dd/MM/yyyy"));
                            DateTime startDate = DateTime.Parse(from.Date.ToString("dd/MM/yyyy"));
                            DateTime endDate = DateTime.Parse(to.Date.ToString("dd/MM/yyyy"));

                            if ((startDate <= transDate) && (endDate >= transDate))
                            {
                                ProductTransactions.Add(item);
                            }
                        }
                        else
                        {
                            ProductTransactions.Add(item);
                        }                    
                }

                if (ProductTransactions.Count > 1 || ProductTransactions.Count==0)
                {
                    NoOfRecords = ProductTransactions.Count + " records found";
                }
                else
                {
                    NoOfRecords = ProductTransactions.Count + " record found";
                }                
            }
        }

        private void SearchTransactions()
        {
            if (ProdTransSelectedProduct == null && SelectedCatgeoryTransaction == null && string.IsNullOrWhiteSpace(SelectedCommodityCodeTransaction))
            {
                MessageBox.Show("Please select either product, category or commodity code to retrieve transactions", "Product,Category or Commodity Code Required", MessageBoxButton.OK, MessageBoxImage.Asterisk);
            }
            else
            {
                LoadProdTransactions();
            }
        }


        private void SearchProduct()
        {
           
            SalesOrder so = new SalesOrder();
            List<DiscountStructure> ds = new List<DiscountStructure>();
            List<UserPrivilages> up = new List<UserPrivilages>();
            var childWindow = new ChildWindow();
            childWindow.productCodeSearch_Closed += (r =>
            {
                if (r.ProductCode != null)
                {
                    if (MainTabSelectedIndex == 0)
                    {
                        if (SelectedProduct == null)
                        {
                            SelectedProduct = new ProductStock();
                            SelectedProduct.Product = new Product();
                            SelectedProduct.Product.Category = new Category();
                        }
                        SelectedProduct.Product = r;
                    }
                    else if (MainTabSelectedIndex ==2)
                    {
                        ProdTransSelectedProduct = r;
                    }

                }
            });
            childWindow.ShowProductSearch(UserData.UserName, state, up, metaData, so, ds, "ProductMaintenance");
  
        }

        private void ClearFieldsTransactions()
        {
            ProdTransSelectedProduct = null;
            SelectedCatgeoryTransaction = null;
            SelectedCommodityCodeTransaction = string.Empty;
            if (ProductTransactions != null)
            {
                ProductTransactions.Clear();
                NoOfRecords = ProductTransactions.Count + " records found";
            }
            else
            {
                NoOfRecords = "0 records found";
            }
        }

        private void UpdateQtyAvailable(object parameter)
        {
            int index = ProductStockMaintenance.IndexOf(parameter as ProductStockMaintenance);
            if (index > -1 && index < ProductStockMaintenance.Count)
            {
                Tuple<int, ProductStock> res = null;
                ProductStock ps = DBAccess.GetProductStockByID(ProductStockMaintenance[index].Product.ProductID, 1);

                if (ps.QtyAvailable != ProductStockMaintenance[index].ProductStock.QtyAvailable)
                {

                    if (MessageBox.Show("Update Qty?", "Update Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes) == MessageBoxResult.Yes)
                    {
                        res = DBAccess.UpdateProductStock(1, ProductStockMaintenance[index].Product.ProductID, ProductStockMaintenance[index].ProductStock.TimeStamp, ProductStockMaintenance[index].ProductStock.QtyAvailable);
                    }
                    else
                    {
                        if (ps != null)
                        {
                            ProductStockMaintenance[index].ProductStock.QtyAvailable = ps.QtyAvailable;
                        }
                    }

                    if (res != null)
                    {
                        if (res.Item1 == -1)
                        {
                            MessageBox.Show("Data has been changed and the form will reload", "Data Has Been Changed", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                            //ProductStockMaintenance[index].ProductStock.QtyAvailable = ss.ProductStock.QtyAvailable;
                            LoadData();
                        }
                        else if (res.Item1 == 0)
                        {
                            MessageBox.Show("Cannot updated quantity. Please try again", "Cannot Update", MessageBoxButton.OK, MessageBoxImage.Error);
                            //ProductStockMaintenance[index].ProductStock.QtyAvailable = ss.ProductStock.QtyAvailable;
                        }
                        else
                        {

                            BackgroundWorker worker = new BackgroundWorker();
                            LoadingScreen = new ChildWindow();
                            LoadingScreen.ShowWaitingScreen("Loading");
                            worker.DoWork += (_, __) =>
                            {
                                //Then check if there are sales orders available
                                ProductStock updatedPS = new ProductStock();
                                updatedPS.QtyAvailable = res.Item2.QtyAvailable;
                                updatedPS.Product = new Product();
                                updatedPS.Product.ProductID = res.Item2.Product.ProductID;

                                StockManager stm = new StockManager();
                                stm.AllocateOrderForStock(updatedPS);

                                //Success
                                //Re-check product stock
                                ProductStock ps2 = DBAccess.GetProductStockByID(res.Item2.Product.ProductID, 1);

                                ProductStockMaintenance[index].ProductStock.QtyAvailable = ps2.QtyAvailable;
                                ProductStockMaintenance[index].ProductStock.TotalSupply = ps2.TotalSupply;
                                ProductStockMaintenance[index].ProductStock.ProjectedAvailable = ps2.ProjectedAvailable;
                                ProductStockMaintenance[index].ProductStock.QtyOnHold = ps2.QtyOnHold;
                                ProductStockMaintenance[index].Product.LastModifiedBy = UserData.FirstName + " " + UserData.LastName + " " + DateTime.Now;
                                ProductStockMaintenance[index].ProductStock.TimeStamp = ps2.TimeStamp;
                            };
                            worker.RunWorkerCompleted += delegate(object s, RunWorkerCompletedEventArgs args)
                            {
                                LoadingScreen.CloseWaitingScreen();
                            };
                            worker.RunWorkerAsync();                            
                        }
                    }
                }
            }
        }


        #region Public_Properties



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


        public string ListPriceMaskVisibility
        {
            get
            {
                return _listPriceMaskVisibility;
            }
            set
            {
                _listPriceMaskVisibility = value;
                RaisePropertyChanged("ListPriceMaskVisibility");

                CheckProductSource();
            }
        }

        public bool IsPurchasedProducts
        {
            get
            {
                return _isPurchasedProducts;
            }
            set
            {
                _isPurchasedProducts = value;
                RaisePropertyChanged("IsPurchasedProducts");

                CheckProductSource();
            }
        }

        public bool IsFabricatedProducts
        {
            get
            {
                return _isFabricatedProducts;
            }
            set
            {
                _isFabricatedProducts = value;
                RaisePropertyChanged("IsFabricatedProducts");
                CheckProductSource();
            }
        }

        public bool IsBoth
        {
            get
            {
                return _isBoth;
            }
            set
            {
                _isBoth = value;
                RaisePropertyChanged("IsBoth");
                CheckProductSource();
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

                ProdTransSelectedProduct = null;
                if (ProductTransactions != null)
                {
                    ProductTransactions.Clear();
                }

                if (MainTabSelectedIndex == 0)
                {

                    IsQldProdInventoryChecked = true;
                    ProductStock temp = new Models.Products.ProductStock();
                    temp = SelectedProduct;

                    LoadAllProducts();
                    SelectedProduct = temp;
                }
                else if (MainTabSelectedIndex == 1)
                {
                    CheckWhichInventoryClicked();
                    
                    
                }
                else if (MainTabSelectedIndex == 2)
                {
                    ProductStockMaintenance.Clear();

                    BackgroundWorker worker = new BackgroundWorker();
                    LoadingScreen = new ChildWindow();
                    LoadingScreen.ShowWaitingScreen("Loading");
                    worker.DoWork += (_, __) =>
                    {

                            DateRangeEnabled = false;
                            LastWeekChecked = true;
                            
                            LoadProducts();
                            FromSelectedDate = DateTime.Now;
                            ToSelectedDate = DateTime.Now;

                    };
                    worker.RunWorkerCompleted += delegate(object s, RunWorkerCompletedEventArgs args)
                    {
                        LoadingScreen.CloseWaitingScreen();

                    };
                    worker.RunWorkerAsync();
                }
                else
                {
                    ProductStockMaintenance.Clear();
                }
            }
        }

        public ObservableCollection<ProductTransactions> ProductTransactions
        {
            get
            {
                return _productTransactions;
            }
            set
            {
                _productTransactions = value;
                RaisePropertyChanged("ProductTransactions");

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

                if (BottomTabSelectedIndex == 1)
                {
                    if (SelectedProduct != null)
                    {
                        SalesOrderLine = DBAccess.GetSalesOrderLinesByProductID(SelectedProduct.Product.ProductID);
                        decimal TotalAmount = SalesOrderLine.Sum(x => x.SalesOrderDetails.Sum(y => y.Total));
                        decimal UnitPrice = SalesOrderLine.Sum(x => x.SalesOrderDetails.Sum(y => y.Product.UnitPrice));
                        decimal OrderQty = SalesOrderLine.Sum(x => x.SalesOrderDetails.Sum(y => y.Quantity));
                        TotalsString = "Total Order Qty : " + OrderQty + " | Total Unit Price: " + UnitPrice.ToString("C", CultureInfo.CurrentCulture) + " | Total Amount: " + TotalAmount.ToString("C", CultureInfo.CurrentCulture);
                    }
                }
                else
                {
                    if (SalesOrderLine != null)
                    {
                        SalesOrderLine.Clear();
                    }
                }
            }
        }

        public List<string> ProductUnit
        {
            get
            {
                return _productUnit;
            }
            set
            {
                _productUnit = value;
                RaisePropertyChanged("ProductUnit");
            }
        }

        
        public ObservableCollection<ProductStock> ProductStock
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
                if (SelectedProduct != null)
                {                   
                    if (SelectedProduct.Product != null)
                    {
                        SelectedProductUnit = SelectedProduct.Product.ProductUnit;
                        //SelectedProduct.Product.Category = SelectedProduct.Product.Category;
                        SelectedCommodityCode = SelectedProduct.Product.CommodityCode;
                        IsInactive = SelectedProduct.Product.Active == true ? false : true;
                        //IsAutoOrder = SelectedProduct.Product.IsAutoOrder == true ? false : true;
                        if (SelectedProduct.SupplierProduct != null)
                        {
                            SelectedSupplier = SelectedProduct.SupplierProduct.Supplier;
                        }
                        //QtyOnHold = SelectedProduct.QtyOnHold;
                        SelectedCatgeory = SelectedProduct.Product.Category;

                        if (SelectedProduct.SupplierProduct.Supplier.SupplierID == 0)
                        {
                            SelectedProduct.SupplierProduct.Supplier.SupplierName = "No Supplier";
                        }

                        if (BottomTabSelectedIndex == 1)
                        {
                            if (SelectedProduct != null)
                            {
                                SalesOrderLine = DBAccess.GetSalesOrderLinesByProductID(SelectedProduct.Product.ProductID);
                                decimal TotalAmount = SalesOrderLine.Sum(x => x.SalesOrderDetails.Sum(y => y.Total));
                                decimal UnitPrice = SalesOrderLine.Sum(x => x.SalesOrderDetails.Sum(y => y.Product.UnitPrice));
                                decimal OrderQty = SalesOrderLine.Sum(x => x.SalesOrderDetails.Sum(y => y.Quantity));
                                TotalsString = "Total Order Qty : " + OrderQty.ToString("C", CultureInfo.CurrentCulture) + " | Total Unit Price: " + UnitPrice.ToString("C", CultureInfo.CurrentCulture) + " | Total Amount: " + TotalAmount.ToString("C", CultureInfo.CurrentCulture);
                            }
                        }
                        else
                        {
                            if (SalesOrderLine != null)
                            {
                               App.Current.Dispatcher.Invoke((Action)delegate
                               {
                                   SalesOrderLine.Clear();
                               });
                            }
                        }
                    }
                        
                    
                    //if(SelectedProduct.Product.IsManufactured)
                    //{
                    //IsManufactured = true;
                    //IsPurchased = false;
                    //}
                    //else
                    //{
                    //IsPurchased = true;
                    //IsManufactured = false;
                    //}
                }
                else
                {
                    SelectedProductUnit = string.Empty;
                    SelectedCatgeory = null;
                    SelectedCommodityCode = string.Empty;
                    IsInactive = false;
                    //IsAutoOrder = false;
                    //IsManufactured = false;
                    //IsPurchased = false;
                    //SelectedSupplier = string.Empty;
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

        public ObservableCollection<SalesOrder> SalesOrderLine
        {
            get
            {
                return _salesOrderLine;
            }
            set
            {
                _salesOrderLine = value;
                RaisePropertyChanged("SalesOrderLine");
            }
        }

        public string SelectedProductUnit
        {
            get
            {
                return _selectedProductUnit;
            }
            set
            {
                _selectedProductUnit = value;
                RaisePropertyChanged("SelectedProductUnit");
            }
        }

        public string SelectedProductUnitTransaction
        {
            get
            {
                return _selectedProductUnitTransaction;
            }
            set
            {
                _selectedProductUnitTransaction = value;
                RaisePropertyChanged("SelectedProductUnitTransaction");
            }
        }

        public List<string> CommodityCodes
        {
            get
            {
                return _commodityCodes;
            }
            set
            {
                _commodityCodes = value;
                RaisePropertyChanged("CommodityCodes");
            }
        }

        public List<string> CommodityCodesTransactions
        {
            get
            {
                return _commodityCodesTransactions;
            }
            set
            {
                _commodityCodesTransactions = value;
                RaisePropertyChanged("CommodityCodesTransactions");
            }
        }


        public string SelectedCommodityCode
        {
            get
            {
                return _selectedCommodityCode;
            }
            set
            {
                _selectedCommodityCode = value;
                RaisePropertyChanged("SelectedCommodityCode");               
                if (!String.IsNullOrWhiteSpace(SelectedCommodityCode))
                {
                    SearchString = string.Empty;
                    isSearchString = false;
                   
                    ProductStockMaintenance=mainProductStockMaintenance;                   
                    
                    if (ProductStockMaintenance != null || ProductStockMaintenance.Count > 0)
                    {
                        ObservableCollection<ProductStockMaintenance> temp = new ObservableCollection<ProductStockMaintenance>();
                       
                        foreach (var item in ProductStockMaintenance)
                        {
                            if(item.Product.CommodityCode == SelectedCommodityCode)
                            {
                                temp.Add(item);
                            }
                        }
                            
                        if(temp.Count > 0)
                        {                            
                            ProductStockMaintenance = temp;
                        }
                        else
                        {
                            App.Current.Dispatcher.Invoke((Action)delegate
                            {
                                ProductStockMaintenance.Clear();
                            });
                        }
                    }
                }
            }
        }

        public string SelectedCommodityCodeTransaction
        {
            get
            {
                return _selectedCommodityCodeTransaction;
            }
            set
            {
                _selectedCommodityCodeTransaction = value;
                RaisePropertyChanged("SelectedCommodityCodeTransaction");
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
       
        public List<Category> CategoriesTransaction
        {
            get
            {
                return _categoriesTransaction;
            }
            set
            {
                _categoriesTransaction = value;
                RaisePropertyChanged("CategoriesTransaction");
            }
        }

        public Category SelectedCatgeory
        {
            get
            {
                return _selectedCatgeory;
            }
            set
            {
                _selectedCatgeory = value;
                RaisePropertyChanged("SelectedCatgeory");
            }
        }
        
        public Category SelectedCatgeoryTransaction
        {
            get
            {
                return _selectedCatgeoryTransaction;
            }
            set
            {
                _selectedCatgeoryTransaction = value;
                RaisePropertyChanged("SelectedCatgeoryTransaction");
            }
        }

        

        public bool IsInactive
        {
            get
            {
                return _isInactive;
            }
            set
            {
                _isInactive = value;
                RaisePropertyChanged("IsInactive");
            }
        }

        //public bool IsManufactured
        //{
        //    get
        //    {
        //        return _isManufactured;
        //    }
        //    set
        //    {
        //        _isManufactured = value;
        //        RaisePropertyChanged(() => this.IsManufactured);
        //    }
        //}

        //public bool IsPurchased
        //{
        //    get
        //    {
        //        return _isPurchased;
        //    }
        //    set
        //    {
        //        _isPurchased = value;
        //        RaisePropertyChanged(() => this.IsPurchased);
        //    }
        //}

        //public bool IsAutoOrder
        //{
        //    get
        //    {
        //        return _isAutoOrder;
        //    }
        //    set
        //    {
        //        _isAutoOrder = value;
        //        RaisePropertyChanged(() => this.IsAutoOrder);
        //    }
        //}

        public List<Supplier> Suppliers
        {
            get
            {
                return _suppliers;
            }
            set
            {
                _suppliers = value;
                RaisePropertyChanged("Suppliers");
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


        public decimal QtyOnHold
        {
            get
            {
                return _qtyOnHold;
            }
            set
            {
                _qtyOnHold = value;
                RaisePropertyChanged("QtyOnHold");
            }
        }

        public string TotalsString
        {
            get
            {
                return _totalsString;
            }
            set
            {
                _totalsString = value;
                RaisePropertyChanged("TotalsString");
            }
        }


        public string SearchString
        {
            get { return _searchString; }
            set
            {
                _searchString = value;
                RaisePropertyChanged("SearchString");
                if (ItemsView != null)
                {
                   
                    ItemsView.Refresh();

                }

                if(!string.IsNullOrWhiteSpace(SearchString))
                {
                    SelectedCommodityCode = string.Empty;
                    
                    if(isSearchString==false)
                    {
                        isSearchString = true;
                        LoadData();
                    }
                }
            }
        }

        public ObservableCollection<ProductStockMaintenance> ProductStockMaintenance
        {
            get
            {
                return _productStockMaintenance;
            }
            set
            {
                _productStockMaintenance = value;
                RaisePropertyChanged("ProductStockMaintenance");
            }
        }

        public ICollectionView ItemsView
        {
            get { return _itemsView; }
        }

        public bool IsQldInventoryChecked
        {
            get
            {
                return _isQldInventoryChecked;
            }
            set
            {
                _isQldInventoryChecked = value;
                RaisePropertyChanged("IsQldInventoryChecked");
                CheckWhichInventoryClicked();
            }
        }

        
        public bool IsQldProdInventoryChecked
        {
            get
            {
                return _isQldProdInventoryChecked;
            }
            set
            {
                _isQldProdInventoryChecked = value;
                RaisePropertyChanged("IsQldProdInventoryChecked");
                CheckWhichProdInventoryClicked();
            }
        }

        public bool IsNswInventoryChecked
        {
            get
            {
                return _isNswInventoryChecked;
            }
            set
            {
                _isNswInventoryChecked = value;
                RaisePropertyChanged("IsNswInventoryChecked");
                CheckWhichInventoryClicked();
            }
        }

        public bool IsNswProdInventoryChecked
        {
            get
            {
                return _isNswProdInventoryChecked;
            }
            set
            {
                _isNswProdInventoryChecked = value;
                RaisePropertyChanged("IsNswProdInventoryChecked");
                CheckWhichProdInventoryClicked();
            }
        }

        public bool AllInventoriesChecked
        {
            get
            {
                return _allInventoriesChecked;
            }
            set
            {
                _allInventoriesChecked = value;
                RaisePropertyChanged("AllInventoriesChecked");
                CheckWhichInventoryClicked();
            }
        }

        public string InventoryClicked
        {
            get
            {
                return _inventoryClicked;
            }
            set
            {
                _inventoryClicked = value;
                RaisePropertyChanged("InventoryClicked");
            }
        }
        public int InventoryProdClicked
        {
            get
            {
                return _inventoryProdClicked;
            }
            set
            {
                _inventoryProdClicked = value;
                RaisePropertyChanged("InventoryProdClicked");
            }
        }

        public ObservableCollection<Product> Product
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

        public Product ProdTransSelectedProduct
        {
            get
            {
                return _prodTransselectedProduct;
            }
            set
            {
                _prodTransselectedProduct = value;
                RaisePropertyChanged("ProdTransSelectedProduct");
                //LoadProdTransactions();
                if (ProdTransSelectedProduct != null)
                {
                    SelectedCatgeoryTransaction=ProdTransSelectedProduct.Category;
                    SelectedCommodityCodeTransaction = ProdTransSelectedProduct.CommodityCode;
                }
                else
                {
                    SelectedCatgeoryTransaction = null;
                    SelectedCommodityCodeTransaction = string.Empty;
                }
            }
        }

        public bool LastWeekChecked
        {
            get
            {
                return _lastWeekChecked;
            }
            set
            {
                _lastWeekChecked = value;
                RaisePropertyChanged("LastWeekChecked");
                if (LastWeekChecked == true)
                {
                    whichDuration = "LastWeekChecked";
                    //LoadProdTransactions();
                }
            }
        }

        public bool LastTwoWeeksChecked
        {
            get
            {
                return _lastTwoWeeksChecked;
            }
            set
            {
                _lastTwoWeeksChecked = value;
                RaisePropertyChanged("LastTwoWeeksChecked");
                if (LastTwoWeeksChecked == true)
                {
                    whichDuration = "LastTwoWeeksChecked";
                    //LoadProdTransactions();
                }
            }
        }

        public bool LastThreeWeeksChecked
        {
            get
            {
                return _lastThreeWeeksChecked;
            }
            set
            {
                _lastThreeWeeksChecked = value;
                RaisePropertyChanged("LastThreeWeeksChecked");
                if (LastThreeWeeksChecked == true)
                {
                    whichDuration = "LastThreeWeeksChecked";
                    //LoadProdTransactions();
                }
            }
        }

        public bool LastMonthChecked
        {
            get
            {
                return _lastMonthChecked;
            }
            set
            {
                _lastMonthChecked = value;
                RaisePropertyChanged("LastMonthChecked");
                if (LastMonthChecked == true)
                {
                    whichDuration = "LastMonthChecked";
                    //LoadProdTransactions();
                }
            }
        }

        public bool DateRangeChecked
        {
            get
            {
                return _dateRangeChecked;
            }
            set
            {
                _dateRangeChecked = value;
                RaisePropertyChanged("DateRangeChecked");
                if (DateRangeChecked == true)
                {
                    DateRangeEnabled = true;
                    whichDuration = "DateRangeChecked";
                    //LoadProdTransactions();
                }
            }
        }

        public bool DateRangeEnabled
        {
            get
            {
                return _dateRangeEnabled;
            }
            set
            {
                _dateRangeEnabled = value;
                RaisePropertyChanged("DateRangeEnabled");
            }
        }

        public bool AllChecked
        {
            get
            {
                return _allChecked;
            }
            set
            {
                _allChecked = value;
                RaisePropertyChanged("AllChecked");
                whichDuration = "AllChecked";
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

        public DateTime FromSelectedDate
        {
            get
            {
                return _fromSelectedDate;
            }
            set
            {
                _fromSelectedDate = value;
                RaisePropertyChanged("FromSelectedDate");
                //LoadProdTransactions(); 
            }
        }



        public DateTime ToSelectedDate
        {
            get
            {
                return _toSelectedDate;
            }
            set
            {
                _toSelectedDate = value;
                RaisePropertyChanged("ToSelectedDate");
                //LoadProdTransactions(); 
            }
        }

        public string ProdCostVisibility
        {
            get
            {
                return _prodCostVisibility;
            }
            set
            {
                _prodCostVisibility = value;
                RaisePropertyChanged("ProdCostVisibility");
            }
        }

        public string NoOfRecords
        {
            get
            {
                return _noOfRecords;
            }
            set
            {
                _noOfRecords = value;
                RaisePropertyChanged("NoOfRecords");
            }
        }

        public string ViewEditButtonVisibility
        {
            get
            {
                return _viewEditButtonVisibility;
            }
            set
            {
                _viewEditButtonVisibility = value;
                RaisePropertyChanged("ViewEditButtonVisibility");
            }
        }
        
        

        #endregion

        #region Commands

        //public ICommand QtyAvailableKeyUpCommand
        //{
        //    get
        //    {
        //        return _lostFocusCommand ?? (_lostFocusCommand = new CommandHandler(() => CalculateQtyToMake(), canExecute));
        //    }
        //}

        public ICommand UpdateProductCommand
        {
            get
            {
                return _updateProductCommand ?? (_updateProductCommand = new CommandHandler(() => UpdateProduct(), canExecute));
            }
        }

        public ICommand ClearProductCommand
        {
            get
            {
                return _clearProductCommand ?? (_clearProductCommand = new CommandHandler(() => ClearProduct(), canExecute));
            }
        }

        public ICommand UpdateCommand
        {
            get
            {
                if (_updateCommand == null)
                {
                    _updateCommand = new A1RConsole.Commands.DelegateCommand(CanExecute, EditStock);
                }
                return _updateCommand;
            }
        }

        public ICommand CheckTransactionsCommand
        {
            get
            {
                return _checkTransactionsCommand ?? (_checkTransactionsCommand = new CommandHandler(() => GoToTransactions(), canExecute));
            }
        }

        public ICommand SearchTransactionCommand
        {
            get
            {
                return _searchTransactionCommand ?? (_searchTransactionCommand = new CommandHandler(() => SearchTransactions(), canExecute));
            }
        }

        public ICommand SearchProductCommand
        {
            get
            {
                return _searchProductCommand ?? (_searchProductCommand = new CommandHandler(() => SearchProduct(), canExecute));
               
            }
        }

        public ICommand ClearTransactionCommand
        {
            get
            {
                return _clearTransactionCommand ?? (_clearTransactionCommand = new CommandHandler(() => ClearFieldsTransactions(), canExecute));
               
            }
        }

        private void SearchProduct(object parameter)
        {
            int index = ProductStockMaintenance.IndexOf(parameter as ProductStockMaintenance);
            if (index > -1 && index < ProductStockMaintenance.Count)
            {

            }

        }

        //private ICommand _lostFocusCommand;
        //public ICommand LostFocusCommand
        //{
        //    get
        //    {
        //        return _lostFocusCommand ?? (_lostFocusCommand = new CommandHandler(() => CalculateQtyToMake(), canExecute));
        //    }
        //}


        //private void CalculateQtyToMake()
        //{
        //    //ProductStockMaintenance ss = (ProductStockMaintenance)_currentcell.Item;

        //    //Console.WriteLine(ss.Product.ProductCode + " " + ss.ProductStock.QtyAvailable);
        //}


        //private DataGridCellInfo _currentcell;

        //public DataGridCellInfo CurrentCell
        //{
        //    get { return _currentcell; }
        //    set
        //    {
        //        if (value.Column != null && value != _currentcell)
        //        {
        //            _currentcell = value;

        //            //ProductStockMaintenance ss = (ProductStockMaintenance)CurrentCell.Item;

        //            //Console.WriteLine(ss.Product.ProductCode + " " + ss.ProductStock.QtyAvailable);

        //            //_selectedproduct = (Product)_currentcell.ProductStock.QtyAvailable;
        //            // optional, in my case, i didn't need to because I am not binding SelectedProduct to the view
        //            // OnPropertyChanged("SelectedProduct");
        //        }
        //    }
        //}


        private RelayCommand _editQtyAvailableCommand;
        public ICommand EditQtyAvailableCommand
        {
            get
            {
                if (_editQtyAvailableCommand == null)
                {
                    _editQtyAvailableCommand = new RelayCommand(UpdateQtyAvailable, CanCellEdit);
                }
                return _editQtyAvailableCommand;
            }
        }
        private bool CanCellEdit(object parameter)
        {
            return true;
        }

        public ICommand RefreshDataCommand
        {
            get
            {
                return _refreshDataCommand ?? (_refreshDataCommand = new CommandHandler(() => CheckWhichInventoryClicked(), canExecute));
            }
        }

        #endregion
    }
}
