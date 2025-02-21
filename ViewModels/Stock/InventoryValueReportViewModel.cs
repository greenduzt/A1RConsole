using A1RConsole.Bases;
using A1RConsole.Commands;
using A1RConsole.Core;
using A1RConsole.DB;
using A1RConsole.Interfaces;
using A1RConsole.Models.Products;
using A1RConsole.Models.Stock;
using A1RConsole.PdfGeneration;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace A1RConsole.ViewModels.Stock
{
    public class InventoryValueReportViewModel : ViewModelBase, IContent
    {
        private List<Product> _productList;
        private List<ProductType> _productType;
        private string _selectedSort;
        private string _selectedStockLocation;
        private string _selectedCommodityCode;
        private ProductType _selectedProductType;
        private bool _isActiveProducts;
        private bool _isInActiveProducts;
        private bool _isActInacBoth;
        private bool _isPurchasedProducts;
        private bool _isFabricatedProducts;
        private bool _isPurFabBoth;
        private bool canExecute;
        private List<string> _commodityCodes;
        private List<ProductType> _productTypes;
        private Product _selectedProduct;
        private ICommand _printCommand;
        private bool IsDirty { get; set; }
        public event EventHandler Closing;
        public RelayCommand CloseCommand { get; private set; }

        public InventoryValueReportViewModel()
        {
            this.CloseCommand = new RelayCommand(CloseWindow);

             BackgroundWorker worker = new BackgroundWorker();
             ChildWindow LoadingScreen = new ChildWindow();
            LoadingScreen.ShowWaitingScreen("Loading");

            worker.DoWork += (_, __) =>
            {

            IsActiveProducts = true;
            IsPurchasedProducts = true;
            SelectedStockLocation = "QLD";
            SelectedSort = "Ascending";
            canExecute = true;
            SelectedProduct = new Product();
            SelectedProductType = new ProductType();
            ProductList = DBAccess.GetEveryProducts();
            ProductTypes = DBAccess.GetProductTypes();

            CommodityCodes = ProductList.GroupBy(x => x.CommodityCode)
                             .Where(g => g.Count() > 1)
                             .Select(g => g.Key).OrderBy(x => x)
                             .ToList();

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


        public string Title
        {
            get
            {
                return "Inventory Report";
            }
        }

        public bool CanClose
        {
            get
            {
                return true;
            }
        }

        private void Print()
        {
            Tuple<Exception, string> tuple = null;
            BackgroundWorker worker = new BackgroundWorker();
            ChildWindow LoadingScreen = new ChildWindow();
            LoadingScreen.ShowWaitingScreen("Printing");

            worker.DoWork += (_, __) =>
            {

                List<Product> printableProductList = new List<Product>();
                List<InventoryValue> inventoryValue = DBAccess.GetInventoryValue(GetStockLocation(), SelectedProduct == null ? 0 : SelectedProduct.ProductID, SelectedProductType == null ? 0 : SelectedProductType.ProductTypeID, SelectedCommodityCode == null ? string.Empty : SelectedCommodityCode);
                if (inventoryValue.Count > 0)
                {
                    InventoryValueReportPDF invPdf = new InventoryValueReportPDF(SelectedStockLocation, inventoryValue, SelectedProduct, SelectedProductType, SelectedCommodityCode);
                    tuple = invPdf.InventoryValueReport();
                }
                else
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show("No items found!", "", MessageBoxButton.OK, MessageBoxImage.Asterisk, MessageBoxResult.Yes);

                    });
                }
            };

            worker.RunWorkerCompleted += delegate(object s, RunWorkerCompletedEventArgs args)
            {
                LoadingScreen.CloseWaitingScreen();
                if (tuple.Item1 != null)
                {
                    MessageBox.Show("A problem has occured while prining. Please try again later." + System.Environment.NewLine + tuple.Item1, "Printing Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.Yes);
                }
                else
                {
                    var childWindow = new ChildWindow();
                    childWindow.ShowFormula(tuple.Item2);
                }
            };
            worker.RunWorkerAsync();
        }

       

        private int GetStockLocation()
        {
            int l = 1;

            if (SelectedStockLocation == "QLD")
            {
                l = 1;
            }
            else
            {
                l = 2;
            }
            return l;
        }

        public List<Product> ProductList
        {
            get
            {
                return _productList;
            }
            set
            {
                _productList = value;
                RaisePropertyChanged("ProductList");
            }
        }




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
            }
        }

        public ProductType SelectedProductType
        {
            get
            {
                return _selectedProductType;
            }
            set
            {
                _selectedProductType = value;
                RaisePropertyChanged("SelectedProductType");
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


        public List<ProductType> ProductTypes
        {
            get
            {
                return _productTypes;
            }
            set
            {
                _productTypes = value;
                RaisePropertyChanged("ProductTypes");
            }
        }




        public bool IsPurFabBoth
        {
            get
            {
                return _isPurFabBoth;
            }
            set
            {
                _isPurFabBoth = value;
                RaisePropertyChanged("IsPurFabBoth");
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
            }
        }

        public bool IsActiveProducts
        {
            get
            {
                return _isActiveProducts;
            }
            set
            {
                _isActiveProducts = value;
                RaisePropertyChanged("IsActiveProducts");
            }
        }

        public bool IsInActiveProducts
        {
            get
            {
                return _isInActiveProducts;
            }
            set
            {
                _isInActiveProducts = value;
                RaisePropertyChanged("IsInActiveProducts");
            }
        }

        public bool IsActInacBoth
        {
            get
            {
                return _isActInacBoth;
            }
            set
            {
                _isActInacBoth = value;
                RaisePropertyChanged("IsActInacBoth");
            }
        }



        public string SelectedSort
        {
            get
            {
                return _selectedSort;
            }
            set
            {
                _selectedSort = value;
                RaisePropertyChanged("SelectedSort");
            }
        }

        public string SelectedStockLocation
        {
            get
            {
                return _selectedStockLocation;
            }
            set
            {
                _selectedStockLocation = value;
                RaisePropertyChanged("SelectedStockLocation");
            }
        }




        #region Commands
        public ICommand PrintCommand
        {
            get
            {
                return _printCommand ?? (_printCommand = new CommandHandler(() => Print(), canExecute));
            }
        }

    
        #endregion
    }
}

