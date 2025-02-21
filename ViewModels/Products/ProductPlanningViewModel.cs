using A1RConsole.Bases;
using A1RConsole.Commands;
using A1RConsole.Core;
using A1RConsole.DB;
using A1RConsole.Interfaces;
using A1RConsole.Models.Meta;
using A1RConsole.Models.Orders;
using A1RConsole.Models.Products;
using A1RConsole.Models.Purchasing;
using A1RConsole.Models.Users;
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

namespace A1RConsole.ViewModels.Products
{
    public class ProductPlanningViewModel : ViewModelBase, IContent
    {
        private ObservableCollection<SalesOrderLines> _salesOrderLines;
        private ObservableCollection<ProductStock> _productStock;
        private int _mainTabSelectedIndex;
        private string userName;
        private string state;
        private List<UserPrivilages> userPrivilages;
        private List<MetaData> metaData;
        private string _version;
        private bool canExecute;
        private string _searchString;
        private string _searchStringStockedProducts;
        private ChildWindow LoadingScreen;
        private ICollectionView _itemsView;
        private ICollectionView _productPlanningItemsView;
        private bool _isPurchasedProducts;
        private bool _isFabricatedProducts;
        private bool _isBoth;
        private bool _viewProdRequired;
        private decimal _estimatedTotalCost;
        private List<string> _commodityCodes;
        private string _selectedCommodityCode;
        private ObservableCollection<Product> _tempProduct;
        private ObservableCollection<Product> _product;
        //private string _productionButtonVisible;
        //private string _purchaseButtonVisible;

        private bool IsDirty { get; set; }
        public event EventHandler Closing;
        private ICommand _createPurchaseOrderCommand;
        private ICommand _createProductionOrderCommand;
        public RelayCommand CloseCommand { get; private set; }
       
        public ProductPlanningViewModel()
        {
            
            canExecute = true;
            MainTabSelectedIndex = 0;
            //var data = metaData.SingleOrDefault(z => z.KeyName == "version");
            //Version = data.Description;
            SalesOrderLines = new ObservableCollection<SalesOrderLines>();
            ProductStock = new ObservableCollection<ProductStock>();
            TempProduct = new ObservableCollection<Product>();
            LoadData();
            LoadProducts();

            this.CloseCommand = new RelayCommand(CloseWindow);
        }

        private void LoadData()
        {
            if (MainTabSelectedIndex == 0)
            {
                SearchStringStockedProducts = string.Empty;
                BackgroundWorker worker = new BackgroundWorker();
                LoadingScreen = new ChildWindow();
                LoadingScreen.ShowWaitingScreen("Loading");
                worker.DoWork += (_, __) =>
                {
                    ProductStock = null;
                    SalesOrderLines = DBAccess.GetOpenSalesOrders();
                };
                worker.RunWorkerCompleted += delegate(object s, RunWorkerCompletedEventArgs args)
                {
                    LoadingScreen.CloseWaitingScreen();
                    _itemsView = CollectionViewSource.GetDefaultView(SalesOrderLines);
                    _itemsView.Filter = x => Filter(x as SalesOrderLines);
                };
                worker.RunWorkerAsync();
            }
            else if (MainTabSelectedIndex == 1)
            {
                SearchString = string.Empty;
                SalesOrderLines = null;
                IsPurchasedProducts = true;
                IsFabricatedProducts = false;
                IsBoth = false;
            }
        }

        private bool Filter(SalesOrderLines model)
        {
            var searchstring = (SearchString ?? string.Empty).ToLower();
            return model != null &&
                 ((model.SalesOrder.SalesOrderDetails[0].Product.ProductCode ?? string.Empty).ToLower().Contains(searchstring)) ||
                 ((model.SalesOrder.SalesOrderDetails[0].Product.CommodityCode ?? string.Empty).ToLower().Contains(searchstring));
        }

        private bool ProductPlanningFilter(ProductStock model)
        {
            if (!string.IsNullOrWhiteSpace(SearchStringStockedProducts))
            {
                var searchstring = (SearchStringStockedProducts ?? string.Empty).ToLower();

                return model != null &&
                     ((model.Product.ProductCode ?? string.Empty).ToLower().Contains(searchstring));
            }
            else
            {
                var searchstring = (SelectedCommodityCode ?? string.Empty).ToLower();

                return model != null &&
                     ((model.Product.CommodityCode ?? string.Empty).ToLower().Contains(searchstring));
            }
        }

        public string Title
        {
            get
            {
                return "Product Planning";
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


        private void OpenPurchaseOrder(object parameter)
        {
            //Supplier s = new Supplier();
            int index = ProductStock.IndexOf(parameter as ProductStock);
            if (index > -1 && index < ProductStock.Count)
            {
                //Check if supplier exist
                PurchaseOrder po = DBAccess.GetSupplierDetailsByProduct(ProductStock[index].Product);
                if (po.Supplier != null)
                {

                    //BindingList<SalesOrderDetails> salesOrderDetails = new BindingList<SalesOrderDetails>();
                    //salesOrderDetails.Add(new SalesOrderDetails() { Product = ProductStockMaintenance[index].Product });
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
                    childWindow.purchaseOrderView_Closed += (r =>
                    {
                        if (r != null)
                        {
                            //Dispatcher d = new Dispatcher();
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                CheckProductActive();
                            });
                        }
                        ////Un Set the flag for Product Stock
                        //Tuple<List<Tuple<User, Product>>, List<int>> res2 = DBAccess.SetEditModeProductStock(salesOrderDetails, ProductStockMaintenance[index].StockLocation.ID, false, userName, false);
                        //LoadData();

                    });
                    childWindow.ShowPurchaseOrderView(ProductStock[index].Product);
                    //}

                }
                else
                {
                    MessageBox.Show("Cannot find supplier details! Please try again later", "Cannot Find Supplier", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    //MessageBox.Show("Cannot find supplier details! Please try again later", "Cannot Find Supplier", MsgBoxButtons.OK, MsgBoxImage.Alert);
                }
            }
        }

        private string CheckProductActive()
        {
            ObservableCollection<ProductStock> ps = new ObservableCollection<ProductStock>();
            ProductStock = null;
            ProductStock = new ObservableCollection<ProductStock>();
            Product = new ObservableCollection<Product>();
            BackgroundWorker worker = new BackgroundWorker();
            LoadingScreen = new ChildWindow();
            string productActiveStr = string.Empty;
            SearchStringStockedProducts = string.Empty;

            if (IsPurchasedProducts)
            {
                productActiveStr = "Purchased";
            }
            else if (IsFabricatedProducts)
            {
                productActiveStr = "Fabricated";
            }
            else
            {
                productActiveStr = "Both";
            }

            LoadingScreen.ShowWaitingScreen("Loading");
            worker.DoWork += (_, __) =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    ps = DBAccess.GetStockedProducts(productActiveStr);

                    if (ViewProdRequired)
                    {
                        foreach (var item in ps)
                        {
                            if (item.QtyAvailable <= item.Product.OrderPoint || item.QtyAvailable <= item.Product.SafetyStockQty)
                            {
                                ProductStock.Add(item);
                            }
                        }
                    }
                    else
                    {
                        ProductStock = ps;
                    }

                    foreach (var item in TempProduct)
                    {
                        if (productActiveStr == "Fabricated" && item.IsManufactured == true)
                        {
                            Product.Add(item);
                        }
                        else if (productActiveStr == "Purchased" && item.IsPurchased == true)
                        {
                            Product.Add(item);
                        }
                        else if (productActiveStr == "Both" && (item.IsManufactured == true || item.IsPurchased == true))
                        {
                            Product.Add(item);
                        }
                    }

                    CommodityCodes = new List<string>();
                    CommodityCodes = Product.GroupBy(x => x.CommodityCode)
                                .Where(g => g.Count() > 1)
                                .Select(g => g.Key).OrderBy(x => x)
                                .ToList();

                });

                //ProductStock = ProductStock.FirstOrDefault(x=>x.QtyAvailable <= x.Product.OrderPoint || x.QtyAvailable <= x.Product.SafetyStockQty);

            };
            worker.RunWorkerCompleted += delegate(object s, RunWorkerCompletedEventArgs args)
            {
                LoadingScreen.CloseWaitingScreen();
                _productPlanningItemsView = CollectionViewSource.GetDefaultView(ProductStock);
                _productPlanningItemsView.Filter = x => ProductPlanningFilter(x as ProductStock);
            };
            worker.RunWorkerAsync();

            return productActiveStr;
        }

        private void CreateProductionOrder(object parameter)
        {
            //bool b = false;
            //Supplier s = new Supplier();
            int index = ProductStock.IndexOf(parameter as ProductStock);
            if (index > -1 && index < ProductStock.Count)
            {
                MessageBox.Show("This section is under construction", "Under Construction", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void LoadProducts()
        {
            BackgroundWorker worker = new BackgroundWorker();
            LoadingScreen = new ChildWindow();
            LoadingScreen.ShowWaitingScreen("Loading");
            worker.DoWork += (_, __) =>
            {
                TempProduct = DBAccess.GetAllProds(true);
            };
            worker.RunWorkerCompleted += delegate(object s, RunWorkerCompletedEventArgs args)
            {
                LoadingScreen.CloseWaitingScreen();
            };
            worker.RunWorkerAsync();
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

        #region PUBLIC PROPERTIES


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


        public ObservableCollection<Product> TempProduct
        {
            get
            {
                return _tempProduct;
            }
            set
            {
                _tempProduct = value;
                RaisePropertyChanged("TempProduct");
            }
        }

        private Product _selectedProduct;
        public Product SelectedProduct
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
                LoadData();
            }
        }

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

        public string SearchStringStockedProducts
        {
            get
            {
                return _searchStringStockedProducts;
            }
            set
            {
                _searchStringStockedProducts = value;
                RaisePropertyChanged("SearchStringStockedProducts");
                if (ProductPlanningItemsView != null)
                {
                    ProductPlanningItemsView.Refresh();
                }

                if (!String.IsNullOrWhiteSpace(SearchStringStockedProducts))
                {
                    SelectedCommodityCode = string.Empty;
                }
            }
        }


        public ICollectionView ItemsView
        {
            get { return _itemsView; }
        }

        public ICollectionView ProductPlanningItemsView
        {
            get { return _productPlanningItemsView; }
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
                CheckProductActive();
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
                CheckProductActive();
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
                CheckProductActive();
            }
        }



        public decimal EstimatedTotalCost
        {
            get
            {
                return _estimatedTotalCost;
            }
            set
            {
                _estimatedTotalCost = value;
                RaisePropertyChanged("EstimatedTotalCost");
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

        public bool ViewProdRequired
        {
            get
            {
                return _viewProdRequired;
            }
            set
            {
                _viewProdRequired = value;
                RaisePropertyChanged("ViewProdRequired");
                CheckProductActive();
            }
        }

        //public string PurchaseButtonVisible
        //{
        //    get
        //    {
        //        return _purchaseButtonVisible;
        //    }
        //    set
        //    {
        //        _purchaseButtonVisible = value;
        //        RaisePropertyChanged(() => this.PurchaseButtonVisible);
        //    }
        //}

        //public string ProductionButtonVisible
        //{
        //    get
        //    {
        //        return _productionButtonVisible;
        //    }
        //    set
        //    {
        //        _productionButtonVisible = value;
        //        RaisePropertyChanged(() => this.ProductionButtonVisible);
        //    }
        //}

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
                if (ProductPlanningItemsView != null)
                {
                    ProductPlanningItemsView.Refresh();
                }
                if (!String.IsNullOrWhiteSpace(SelectedCommodityCode))
                {
                    SearchStringStockedProducts = string.Empty;
                }
            }
        }

        #endregion

       

       

        public ICommand CreatePurchaseOrderCommand
        {
            get
            {
                if (_createPurchaseOrderCommand == null)
                {
                    _createPurchaseOrderCommand = new A1RConsole.Commands.DelegateCommand(CanExecute, OpenPurchaseOrder);
                }
                return _createPurchaseOrderCommand;
            }
        }

        public ICommand CreateProductionOrderCommand
        {
            get
            {
                if (_createProductionOrderCommand == null)
                {
                    _createProductionOrderCommand = new A1RConsole.Commands.DelegateCommand(CanExecute, CreateProductionOrder);
                }
                return _createProductionOrderCommand;
            }
        }

     
        

    }
}
