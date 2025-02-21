using A1RConsole.Bases;
using A1RConsole.Commands;
using A1RConsole.Core;
using A1RConsole.DB;
using A1RConsole.Interfaces;
using A1RConsole.Models;
using A1RConsole.Models.Meta;
using A1RConsole.Models.Products;
using A1RConsole.Models.Purchasing;
using A1RConsole.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;


namespace A1RConsole.ViewModels.Receiving
{
    public class ReceivingViewModel : ViewModelBase, IContent
    {
        private ObservableCollection<PurchaseOrder> _purchaseOrder;
        public ObservableCollection<Product> Product { get; set; }
        private DateTime _currentDate;
        private PurchaseOrder _purchaseOrderR;
        private int _mainTabSelectedIndex;
        private string _selectedStatus;
        private string userName;
        private string _version;
        private bool canExecute;
        private bool receivedBtnClicked;
        private int _itemCount;
        private List<string> _purchaseOrderNos;
        private string _selectedPurchaseOrderNo;
        private bool _updateBtnEnabled;
        private string _updateBtnBackground;
        private string _btnReceiveBtnBackground;
        private bool _btnReceiveEnabled;
        private bool _statusEnabled;
        private ChildWindow LoadingScreen;
        private bool IsDirty { get; set; }
        public event EventHandler Closing;
        public RelayCommand CloseCommand { get; private set; }
        private ICommand _receiveOrderCommand;
        private ICommand _receiveCommand;
        private ICommand _selectionChangedCommand;
        private ICommand _lostFocusCommand;
        private ICommand _updateCommand;
        private ICommand _removeCommand;
        private ICommand _viewCommand;

        public ReceivingViewModel()
        {        
            canExecute = true;
            receivedBtnClicked = false;
            //var data = metaData.SingleOrDefault(z => z.KeyName == "version");
            //Version = data.Description;
            userName = UserData.UserName;
            MainTabSelectedIndex = 0;
            CurrentDate = DateTime.Now;
            Product = new ObservableCollection<Product>();
            PurchaseOrderR = new PurchaseOrder();
            PurchaseOrderR.PurchaseOrderDetails = new ObservableCollection<PurchaseOrderDetails>();
            PurchaseOrder = new ObservableCollection<PurchaseOrder>();
            BackgroundWorker worker = new BackgroundWorker();
            LoadingScreen = new ChildWindow();
            LoadingScreen.ShowWaitingScreen("Loading");
            worker.DoWork += (_, __) =>
            {
                LoadData();
                LoadProducts();
            };
            worker.RunWorkerCompleted += delegate(object s, RunWorkerCompletedEventArgs args)
            {
                LoadingScreen.CloseWaitingScreen();

            };
            worker.RunWorkerAsync();

            this.CloseCommand = new RelayCommand(CloseWindow);
        }

        void OnPurchaseOrderCollChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            this.ItemCount = this.PurchaseOrderR.PurchaseOrderDetails.Count;
            // Resequence list
            SequencingService.SetCollectionSequence(this.PurchaseOrderR.PurchaseOrderDetails);
            //CalculateFinalTotal();
        }


        private void LoadData()
        {

            if (MainTabSelectedIndex == 0)
            {
                PurchaseOrder = DBAccess.GetPurchaseOrders("Pending");
                receivedBtnClicked = false;

                foreach (var item in PurchaseOrder)
                {
                    //DateTime dumDate = item.RecieveOnDate;
                    double days = Math.Ceiling((item.RecieveOnDate - DateTime.Now).TotalDays);

                    if (days < 0)
                    {
                        item.DaysToReceive = Math.Abs(days) + " Day" + CheckNumber(days) + " Late";
                        item.DaysToReceiveBackground = "#E65722";
                        item.DaysToReceiveForeground = "White";
                    }
                    else if (days > 0)
                    {
                        item.DaysToReceive = days + " Day" + CheckNumber(days) + " To Go";
                        item.DaysToReceiveBackground = "#009933";
                        item.DaysToReceiveForeground = "White";
                    }
                    else if (days == 0)
                    {
                        item.DaysToReceive = "Arrive Today";
                        item.DaysToReceiveBackground = "#0424c1";
                        item.DaysToReceiveForeground = "White";
                    }

                    if (item.Status == "Released")
                    {
                        item.StatusForeground = "White";
                        item.StatusBackground = "#3333ff";
                    }
                    else if (item.Status == "UnApproved")
                    {
                        item.StatusForeground = "White";
                        item.StatusBackground = "#e68a00";
                    }
                }
            }
            else if (MainTabSelectedIndex == 1)
            {
                PurchaseOrder = DBAccess.GetPurchaseOrders("All");
                LoadPurchaseOrderNos();

                if (receivedBtnClicked)
                {
                    if (PurchaseOrderR != null)
                    {
                        SelectedPurchaseOrderNo = PurchaseOrderR.PurchasingOrderNo.ToString();
                        PurchaseOrderR.ReceivedDate = CurrentDate;
                    }
                }
                else
                {
                    //ClearReceiveOrderFields();

                }
            }
            else if (MainTabSelectedIndex == 2)
            {
                PurchaseOrder = DBAccess.GetPurchaseOrders("Other");
                foreach (var item in PurchaseOrder)
                {
                    if (item.Status == ReceivingStatus.Approved.ToString() || item.Status == ReceivingStatus.Cancelled.ToString())
                    {
                        item.StatusBackground = item.Status + " on " + item.LastModifiedDate.ToString("dd/MM/yyyy");
                    }
                    else
                    {
                        item.StatusBackground = "";
                    }
                }
            }
        }

        private void LoadProducts()
        {
            Product = DBAccess.GetAllProds(true);
            //foreach (var item in Product)
            //{
            //    Console.WriteLine(item.ProductCode + " " + item.MaterialCost);
            //}
        }

        private void LoadPurchaseOrderNos()
        {
            PurchaseOrderNos = DBAccess.LoadPurchasingOrderNos();
            PurchaseOrderNos.Add("");
        }

        private bool CanExecute(object parameter)
        {
            return true;
        }

        private void ViewReceiveOrder(object parameter)
        {
            int index = PurchaseOrder.IndexOf(parameter as PurchaseOrder);
            if (index > -1 && index < PurchaseOrder.Count)
            {
                PurchaseOrderR = PurchaseOrder[index];
                SelectedStatus = PurchaseOrder[index].Status;
                // PurchaseOrderR.Status = PurchaseOrder[index].Status == ReceivingStatus.Pending.ToString() ? "Select" : PurchaseOrder[index].Status;
                if (PurchaseOrderR.PurchaseOrderDetails != null)
                {
                    PurchaseOrderR.PurchaseOrderDetails.CollectionChanged += OnPurchaseOrderCollChanged;
                    this.PurchaseOrderR.PurchaseOrderDetails = SequencingService.SetCollectionSequence(this.PurchaseOrderR.PurchaseOrderDetails);
                    receivedBtnClicked = true;

                    if ((PurchaseOrderR.Status == ReceivingStatus.Approved.ToString() && PurchaseOrderR.Completed == true) || PurchaseOrderR.Status == ReceivingStatus.Cancelled.ToString())
                    {
                        StatusEnabled = false;
                    }
                    else
                    {
                        StatusEnabled = true;
                    }

                    MainTabSelectedIndex = 1;
                }
            }
        }


        //private void ViewOrder(object parameter)
        //{

        //    int index = PurchaseOrder.IndexOf(parameter as PurchaseOrder);
        //    if (index > -1 && index < PurchaseOrder.Count)
        //    {
        //        PurchaseOrderR = PurchaseOrder[index];
        //        SelectedStatus = PurchaseOrderR.Status;
        //        if (PurchaseOrderR.PurchaseOrderDetails != null)
        //        {
        //            PurchaseOrderR.PurchaseOrderDetails.CollectionChanged += OnPurchaseOrderCollChanged;
        //            this.PurchaseOrderR.PurchaseOrderDetails = SequencingService.SetCollectionSequence(this.PurchaseOrderR.PurchaseOrderDetails);
        //            receivedBtnClicked = true;
        //            MainTabSelectedIndex = 1;
        //        }                
        //    }
        //}


        private void UpdateOrder(bool addToStock)
        {
            bool exec = false;
            if (PurchaseOrderR != null)
            {
                var hasSelect = PurchaseOrderR.PurchaseOrderDetails.FirstOrDefault(x => x.Result == "Select" && x.LineStatus != LineStatus.Closed.ToString());
                bool has = PurchaseOrderR.PurchaseOrderDetails.All(x => x.EnteredQty == 0 && (x.Result == "Select" || x.Result == "Full Order Received"));

                if (String.IsNullOrWhiteSpace(SelectedPurchaseOrderNo))
                {
                    MessageBox.Show("Please enter Purchase Order No", "Purchase Order No", MessageBoxButton.OK, MessageBoxImage.Stop);
                }
                else if (PurchaseOrderR.PurchaseOrderDetails.Count == 0)
                {
                    MessageBox.Show("Please enter product to purchase", "Product Required", MessageBoxButton.OK, MessageBoxImage.Stop);
                }
                else if (has)
                {
                    MessageBox.Show("Please enter arrived qty to receive this order", "Arrived Qty Required", MessageBoxButton.OK, MessageBoxImage.Stop, MessageBoxResult.Yes);
                }
                else if (hasSelect != null)
                {
                    MessageBox.Show("Please select a result for Line No : " + hasSelect.LineNo, "Result Required", MessageBoxButton.OK, MessageBoxImage.Stop, MessageBoxResult.Yes);
                }
                else
                {
                    if (PurchaseOrderR.Status == "Cancelled")
                    {
                        if (MessageBox.Show("Are you sure you want to cancel this order?" + System.Environment.NewLine + "Cancelation will permanently remove this purchase order", "Canceling Order", MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
                        {
                            exec = true;
                        }
                    }
                    else
                    {
                        exec = true;
                    }

                    if (exec)
                    {
                        //Check if the product belongs to the correct supplier
                        List<SupplierProduct> spList = DBAccess.GetAllSupplierProducts();
                        if (spList != null || spList.Count > 0)
                        {
                            string p = string.Empty;
                            foreach (var item in PurchaseOrderR.PurchaseOrderDetails)
                            {
                                //If Order Qty == Recieved Qty, close the line otherwise open
                                if (item.OrderQty == (item.EnteredQty + item.RecievedQty))
                                {
                                    item.LineStatus = LineStatus.Closed.ToString();
                                }
                                else
                                {
                                    item.LineStatus = LineStatus.Open.ToString();
                                }
                                //End

                                bool b = spList.Any(x => x.ProductID == item.Product.ProductID && x.Supplier.SupplierID == PurchaseOrderR.Supplier.SupplierID);
                                if (b == false)
                                {
                                    p = "[" + item.Product.ProductCode + "] " + item.Product.ProductDescription;
                                    break;
                                }
                            }
                            if (String.IsNullOrWhiteSpace(p))
                            {
                                if (addToStock)
                                {
                                    if (PurchaseOrderR.Status == ReceivingStatus.Approved.ToString() || PurchaseOrderR.Status == ReceivingStatus.Cancelled.ToString())
                                    {
                                        bool x = PurchaseOrderR.PurchaseOrderDetails.Any(s => s.TotalRecieved < s.OrderQty && s.LineStatus == LineStatus.Open.ToString());
                                        if (x)
                                        {
                                            PurchaseOrderR.Completed = false;
                                            PurchaseOrderR.Status = ReceivingStatus.Pending.ToString();
                                        }
                                        else
                                        {
                                            PurchaseOrderR.Completed = true;
                                        }
                                    }
                                    else
                                    {
                                        PurchaseOrderR.Completed = false;
                                    }
                                }
                                else
                                {
                                    if (PurchaseOrderR.Status == ReceivingStatus.Cancelled.ToString())
                                    {
                                        PurchaseOrderR.Completed = true;
                                    }
                                    else
                                    {
                                        PurchaseOrderR.Completed = false;
                                    }
                                }


                                int res = DBAccess.UpdateReceivingOrder(PurchaseOrderR, userName, addToStock);
                                if (res == 1)
                                {
                                    MessageBox.Show("Purchase order updated successfully!", "Purchase Order Updated", MessageBoxButton.OK, MessageBoxImage.Information);
                                    MainTabSelectedIndex = 2;
                                }
                                else if (res == 2)
                                {
                                    MessageBox.Show("Order received successfully!" + System.Environment.NewLine + "Stock updated", "Purchase Order Received", MessageBoxButton.OK, MessageBoxImage.Information);
                                    MainTabSelectedIndex = 2;
                                }
                                else if (res == -1)
                                {
                                    MessageBox.Show("There has been a problem and the Purchase Order did not receive successfully!!!", "Order Receive Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                                }
                                else
                                {
                                    MessageBox.Show("You haven't made any changes", "No Changes Were Made", MessageBoxButton.OK, MessageBoxImage.Information);
                                }
                            }
                            else
                            {
                                MessageBox.Show(p + " does not belong to supplier " + PurchaseOrderR.Supplier.SupplierName, "Product Does Not Belong To Supplier", MessageBoxButton.OK, MessageBoxImage.Stop);
                            }
                        }
                        else
                        {
                            MessageBox.Show("Cannot load supplier products", "Cannot Load Data", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Please enter product", "Product Required", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }


        private string CheckNumber(double num)
        {
            string str = string.Empty;

            if ((num > 1) || (num < -1))
            {
                str = "s";
            }

            return str;
        }

        private void CalculateFinalTotal(object parameter)
        {
            int index = PurchaseOrderR.PurchaseOrderDetails.IndexOf(parameter as PurchaseOrderDetails);
            if (index > -1 && index < PurchaseOrderR.PurchaseOrderDetails.Count)
            {
                PurchaseOrderR.PurchaseOrderDetails[index].TotalRecieved = PurchaseOrderR.PurchaseOrderDetails[index].RecievedQty;
                PurchaseOrderR.PurchaseOrderDetails[index].TotalRecieved += PurchaseOrderR.PurchaseOrderDetails[index].EnteredQty;
            }
            //PurchaseOrderR.SubTotal = PurchaseOrderR.PurchaseOrderDetails.Sum(x => x.Total);
            //PurchaseOrderR.Tax = (PurchaseOrderR.SubTotal * 10) / 100;  //((PurchaseOrder.m + SalesOrder.FreightTotal) * 10) / 100;
            //PurchaseOrderR.TotalAmount = PurchaseOrderR.SubTotal + PurchaseOrderR.Tax;
        }

        private void RemoveItem(object parameter)
        {
            int index = PurchaseOrderR.PurchaseOrderDetails.IndexOf(parameter as PurchaseOrderDetails);
            if (index > -1 && index < PurchaseOrderR.PurchaseOrderDetails.Count)
            {
                PurchaseOrderR.PurchaseOrderDetails.RemoveAt(index);
                ObservableCollection<PurchaseOrderDetails> tempColl = new ObservableCollection<PurchaseOrderDetails>();
                tempColl = PurchaseOrderR.PurchaseOrderDetails;
                PurchaseOrderR.PurchaseOrderDetails = new ObservableCollection<PurchaseOrderDetails>();
                PurchaseOrderR.PurchaseOrderDetails = tempColl;
                //SalesOrder.SalesOrderDetails.Add(new SalesOrderDetails() { Quantity=0 });
            }
            if (PurchaseOrderR.PurchaseOrderDetails.Count == 0)
            {
                PurchaseOrderR.PurchaseOrderDetails = new ObservableCollection<PurchaseOrderDetails>();
            }


            if (PurchaseOrderR.PurchaseOrderDetails != null)
            {
                PurchaseOrderR.PurchaseOrderDetails.CollectionChanged += OnPurchaseOrderCollChanged;
                this.PurchaseOrderR.PurchaseOrderDetails = SequencingService.SetCollectionSequence(this.PurchaseOrderR.PurchaseOrderDetails);
            }
        }

        private void ClearReceiveOrderFields()
        {
            PurchaseOrderR = new PurchaseOrder();
            SelectedPurchaseOrderNo = string.Empty;
        }

        private void ReceiveOrder()
        {
            if (PurchaseOrderR != null)
            {
                if (PurchaseOrderR.Status != "Select")
                {
                    string str = string.Empty;
                    switch (SelectedStatus)
                    {
                        case "Approved": str = "approve";
                            break;
                        case "UnApproved": str = "unapprove";
                            break;
                        case "Cancelled": str = "cancel";
                            break;
                        default:
                            break;
                    }
                    if (MessageBox.Show("Are you sure you want to " + str + " this order?", "Order Receiving Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
                    {
                        UpdateOrder(true);
                    }
                }
            }
            else
            {
                MessageBox.Show("Please enter a purchasing order", "Purchasing Order Required", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }


        public string Title
        {
            get
            {
                return "Order Receiving";
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

        #region PUBLIC_PROPERTIES


        public string SelectedStatus
        {
            get
            {
                return _selectedStatus;
            }
            set
            {
                _selectedStatus = value;
                RaisePropertyChanged("SelectedStatus");
                if (!string.IsNullOrWhiteSpace(SelectedStatus) && PurchaseOrderR != null)
                {

                    if (SelectedStatus == ReceivingStatus.Approved.ToString() && PurchaseOrderR.Completed == false)
                    {
                        BtnReceiveBtnBackground = "#666666";
                        BtnReceiveEnabled = true;
                        UpdateBtnBackground = "#bfbfbf";
                        UpdateBtnEnabled = false;
                    }
                    else if (SelectedStatus == ReceivingStatus.Approved.ToString() && PurchaseOrderR.Completed == true)
                    {
                        BtnReceiveBtnBackground = "#bfbfbf";
                        BtnReceiveEnabled = false;
                        UpdateBtnBackground = "#bfbfbf";
                        UpdateBtnEnabled = false;
                    }
                    else if (SelectedStatus == "Select" || (SelectedStatus == ReceivingStatus.Cancelled.ToString() && PurchaseOrderR.Completed == true))
                    {
                        BtnReceiveBtnBackground = "#bfbfbf";
                        BtnReceiveEnabled = false;
                        UpdateBtnBackground = "#bfbfbf";
                        UpdateBtnEnabled = false;
                    }
                    else
                    {
                        BtnReceiveBtnBackground = "#bfbfbf";
                        BtnReceiveEnabled = false;
                        UpdateBtnBackground = "#666666";
                        UpdateBtnEnabled = true;
                    }
                    PurchaseOrderR.Status = SelectedStatus;
                }
                else
                {
                    UpdateBtnBackground = "#bfbfbf";
                    BtnReceiveBtnBackground = "#bfbfbf";
                    BtnReceiveEnabled = false;
                    UpdateBtnEnabled = false;
                    PurchaseOrderR.Status = "Pending";
                }
            }
        }

        public PurchaseOrder PurchaseOrderR
        {
            get
            {
                return _purchaseOrderR;
            }
            set
            {
                _purchaseOrderR = value;
                RaisePropertyChanged("PurchaseOrderR");

            }
        }





        public ObservableCollection<PurchaseOrder> PurchaseOrder
        {
            get
            {
                return _purchaseOrder;
            }
            set
            {
                _purchaseOrder = value;
                RaisePropertyChanged("PurchaseOrder");
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


        public bool UpdateBtnEnabled
        {
            get
            {
                return _updateBtnEnabled;
            }
            set
            {
                _updateBtnEnabled = value;
                RaisePropertyChanged("UpdateBtnEnabled");
            }
        }

        public string UpdateBtnBackground
        {
            get
            {
                return _updateBtnBackground;
            }
            set
            {
                _updateBtnBackground = value;
                RaisePropertyChanged("UpdateBtnBackground");
            }
        }

        public string BtnReceiveBtnBackground
        {
            get
            {
                return _btnReceiveBtnBackground;
            }
            set
            {
                _btnReceiveBtnBackground = value;
                RaisePropertyChanged("BtnReceiveBtnBackground");
            }
        }

        public bool BtnReceiveEnabled
        {
            get
            {
                return _btnReceiveEnabled;
            }
            set
            {
                _btnReceiveEnabled = value;
                RaisePropertyChanged("BtnReceiveEnabled");
            }
        }

        public bool StatusEnabled
        {
            get
            {
                return _statusEnabled;
            }
            set
            {
                _statusEnabled = value;
                RaisePropertyChanged("StatusEnabled");
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

        public List<string> PurchaseOrderNos
        {
            get { return _purchaseOrderNos; }

            set
            {
                _purchaseOrderNos = value;
                base.RaisePropertyChanged("PurchaseOrderNos");
            }
        }


        public string SelectedPurchaseOrderNo
        {
            get { return _selectedPurchaseOrderNo; }

            set
            {
                _selectedPurchaseOrderNo = value;
                RaisePropertyChanged("SelectedPurchaseOrderNo");
                if (!String.IsNullOrWhiteSpace(SelectedPurchaseOrderNo))
                {
                    PurchaseOrderR = PurchaseOrder.FirstOrDefault(x => x.PurchasingOrderNo == Convert.ToInt32(SelectedPurchaseOrderNo));
                    PurchaseOrderR.ReceivedDate = CurrentDate;
                    SelectedStatus = PurchaseOrderR.Status;

                    if (PurchaseOrderR.Completed == true)
                    {
                        StatusEnabled = false;
                    }
                    else
                    {
                        StatusEnabled = true;
                    }

                    PurchaseOrderR.PurchaseOrderDetails.CollectionChanged += OnPurchaseOrderCollChanged;
                    this.PurchaseOrderR.PurchaseOrderDetails = SequencingService.SetCollectionSequence(this.PurchaseOrderR.PurchaseOrderDetails);
                }
                else
                {
                    PurchaseOrderR = null;
                }
            }
        }


        #endregion

        /***Receive Orders***/
      

      
        public ICommand ReceiveOrderCommand
        {
            get
            {
                if (_receiveOrderCommand == null)
                {
                    _receiveOrderCommand = new DelegateCommand(CanExecute, ViewReceiveOrder);
                }
                return _receiveOrderCommand;
            }
        }

        public ICommand ViewCommand
        {
            get
            {
                if (_viewCommand == null)
                {
                    _viewCommand = new DelegateCommand(CanExecute, ViewReceiveOrder);
                }
                return _viewCommand;
            }
        }

        public ICommand ReceiveCommand
        {
            get
            {
                return _receiveCommand ?? (_receiveCommand = new CommandHandler(() => ReceiveOrder(), canExecute));
            }
        }

        public ICommand UpdateCommand
        {
            get
            {
                return _updateCommand ?? (_updateCommand = new CommandHandler(() => UpdateOrder(false), canExecute));
            }
        }

        public ICommand SelectionChangedCommand
        {
            get
            {
                return null;// _selectionChangedCommand ?? (_selectionChangedCommand = new LogOutCommandHandler(() => CalculateFinalTotal(), canExecute));
            }
        }

        //public ICommand LostFocusCommand
        //{
        //    get
        //    {
        //        return _lostFocusCommand ?? (_lostFocusCommand = new LogOutCommandHandler(() => CalculateFinalTotal(), canExecute));
        //    }
        //}

        public ICommand LostFocusCommand
        {
            get
            {
                if (_lostFocusCommand == null)
                {
                    _lostFocusCommand = new DelegateCommand(CanExecute, CalculateFinalTotal);
                }
                return _lostFocusCommand;
            }
        }

        public ICommand RemoveCommand
        {
            get
            {
                if (_removeCommand == null)
                {
                    _removeCommand = new DelegateCommand(CanExecute, RemoveItem);
                }
                return _removeCommand;
            }
        }


    }
}

