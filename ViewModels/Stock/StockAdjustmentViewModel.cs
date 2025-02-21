using A1RConsole.Bases;
using A1RConsole.Commands;
using A1RConsole.Core;
using A1RConsole.DB;
using A1RConsole.Interfaces;
using A1RConsole.Models.Products;
using A1RConsole.Models.Stock;
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
    public class StockAdjustmentViewModel : ViewModelBase,IContent
    {
        private ReasonCode _selectedReasonCode;
        //private ReasonCode _reasonCode;
        private List<ReasonCode> _reasonCodeList;
        private StockAdjustmentViewModel _stockAdjustmentViewModel;
        private decimal _quantity;
        private ProductStock _selectedProduct;
        private List<ProductStock> _productStockList;
        private string _selectedType;
        private DateTime _currentDate;
        private string _changingQtyVisibility;
        private string _chkBoxSendToWarehouseTxt;
        private bool _sendToWarehouseChecked;
        private bool IsDirty { get; set; }
        public event EventHandler Closing;
        public RelayCommand CloseCommand { get; private set; }
       
        private ICommand _adjustCommand;
        private ICommand _searchReasonCodeCommand;

        public StockAdjustmentViewModel(StockAdjustmentViewModel rc)
        {
            this.CloseCommand = new RelayCommand(CloseWindow);
            SAdjustmentViewModel = rc;
            CurrentDate = DateTime.Now;
            SelectedType = "Select";
            
            SelectedProduct = new ProductStock();

            LoadProductStock();
            LoadReasonCodes();
            SelectedType = SAdjustmentViewModel.SelectedType;
            //ReasonCode = SAdjustmentViewModel.ReasonCode;
            SelectedProduct = SAdjustmentViewModel.SelectedProduct;
            SelectedReasonCode = SAdjustmentViewModel.SelectedReasonCode;
            Quantity = SAdjustmentViewModel.Quantity;
            
        }

        public StockAdjustmentViewModel()
        {
            this.CloseCommand = new RelayCommand(CloseWindow);
            BackgroundWorker worker = new BackgroundWorker();
            ChildWindow LoadingScreen = new ChildWindow();
            SendToWarehouseChecked = false;

            var data = UserData.UserPrivilages.SingleOrDefault(x => x.Area == "ProductMaintenance->qty_enabled");
            if (data != null)
            {
                ChangingQtyVisibility = data.Visibility == "True" ? "Visible" : "Collapsed";
                ChkBoxSendToWarehouseTxt = "Tick to allocate stock to pending Sales Order (This criteria applies if a product has one Sales Order only!)" + System.Environment.NewLine + "This will deduct stock and send the Sales Order to warehouse";
            }

            LoadingScreen.ShowWaitingScreen("Loading");

            worker.DoWork += (_, __) =>
            {
                CurrentDate = DateTime.Now;
                SelectedType = "Select";
                SelectedReasonCode = new ReasonCode();
                SelectedProduct = new ProductStock();
                LoadProductStock();
                LoadReasonCodes();
            };

            worker.RunWorkerCompleted += delegate(object s, RunWorkerCompletedEventArgs args)
            {
                LoadingScreen.CloseWaitingScreen();
            };
            worker.RunWorkerAsync();   
        }

        public void CloseWindow(object p)
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

       

        //public static void Raise(this EventHandler handler, object sender, EventArgs args)
        //{
        //    if (handler != null)
        //    {
        //        handler(sender, args);
        //    }
        //}

     

        public bool CanClose
        {
            get
            {
                return true;
            }
        }

        public string Title
        {
            get
            {
                return "Stock Adjustment";
            }
        }

       
        private void LoadProductStock()
        {
            ProductStockList = new List<ProductStock>();
            ProductStockList = DBAccess.GetProductStockByStock(1);//Get product stock            
        }

        private void LoadReasonCodes()
        {
            ReasonCodeList = new List<ReasonCode>();
            ReasonCodeList = DBAccess.GetReasonCodes();

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
            else if (Quantity == 0)
            {
                MessageBox.Show("Please enter quantity", "Quantity Required", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (SelectedReasonCode == null || string.IsNullOrWhiteSpace(SelectedReasonCode.Code))
            {
                MessageBox.Show("Please enter reason", "Reason Required", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (SelectedType == "Adjust Out" && Quantity > SelectedProduct.QtyAvailable)
            {
                MessageBox.Show("Cannot adjust out from stock due to insufficient stock" + System.Environment.NewLine + "Only " + SelectedProduct.QtyAvailable + SelectedProduct.Product.ProductUnit + " available", "Insufficient Stock", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                Tuple<int,ProductStock> res = DBAccess.UpdateStock(SelectedProduct.StockLocation.ID,SelectedProduct.Product.ProductID,SelectedProduct.TimeStamp,SelectedType,Quantity,SelectedReasonCode.Reason);
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
                            ClearFields();
                        };
                        worker.RunWorkerAsync();
                    }
                    else
                    {
                        MessageBox.Show("Stock adjusted successfully!", "", MessageBoxButton.OK, MessageBoxImage.Information);
                        ClearFields();
                    }  
                }
                else if (res.Item1 == -1)
                {
                    MessageBox.Show("Stock has been changed since the time you opened this form" + System.Environment.NewLine + "The product details will be reloaded", "Data Changed", MessageBoxButton.OK, MessageBoxImage.Warning);
                    LoadProductStock();
                }
                else
                {
                    MessageBox.Show("There has been a problem and stock did not adjust", "Error Occured", MessageBoxButton.OK, MessageBoxImage.Error);
                    ClearFields();
                }

            }
        }

        private void ViewReasonCode()
        {

            var childWindow = new ChildWindow();
            childWindow.reasonCodesView_Closed += (r =>
            {
                if (r != null)
                {
                   
                }
            });
            childWindow.ShowReasonCodes(this);
        }

        private void ClearFields()
        {
            CurrentDate = DateTime.Now;
            SelectedType = "Select";
            LoadProductStock();
            Quantity = 0;
            LoadReasonCodes();
            SelectedReasonCode = new ReasonCode();
        }


        //public ReasonCode ReasonCode
        //{
        //    get
        //    {
        //        return _reasonCode;
        //    }
        //    set
        //    {
        //        _reasonCode = value;
        //        RaisePropertyChanged(() => this.ReasonCode);

        //    }
        //}

        public decimal Quantity
        {
            get
            {
                return _quantity;
            }
            set
            {
                _quantity = value;
                RaisePropertyChanged("Quantity");

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

        public List<ProductStock> ProductStockList
        {
            get
            {
                return _productStockList;
            }
            set
            {
                _productStockList = value;
                RaisePropertyChanged("ProductStockList");

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


        public StockAdjustmentViewModel SAdjustmentViewModel
        {
            get
            {
                return _stockAdjustmentViewModel;
            }
            set
            {
                _stockAdjustmentViewModel = value;
                RaisePropertyChanged("AdjustmentViewModel");

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

        public ICommand AdjustCommand
        {
            get
            {
                return _adjustCommand ?? (_adjustCommand = new CommandHandler(() => AdjustStock(), true));
            }
        }

        public ICommand SearchReasonCodeCommand
        {
            get
            {
                return _searchReasonCodeCommand ?? (_searchReasonCodeCommand = new CommandHandler(() => ViewReasonCode(), true));
            }
        }



    }
}
