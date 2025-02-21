using A1RConsole.Bases;
using A1RConsole.Commands;
using A1RConsole.Core;
using A1RConsole.DB;
using A1RConsole.Interfaces;
using A1RConsole.Models;
using A1RConsole.Models.Dispatch;
using A1RConsole.Models.Meta;
using A1RConsole.Views;
using MsgBox;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;

namespace A1RConsole.ViewModels.Shipping
{
    public class ShippingViewModel : ViewModelBase,IContent
    { 
        private bool execute;
        private string _searchString;
        private string _dispatchRadio;
        //private bool _allChecked;
        //private bool _dispatchedChecked;
        //private bool _dispatchTodayChecked;
        private bool _readyToDispatchChecked;
        //private bool _cancelledChecked;
        //private bool _returnedChecked;
        private string _version;
        //private ICollectionView _itemsView;
        private ObservableCollection<DispatchOrder> _dispatchOrders;
        private ChildWindow showDispatchScreen;
        private ChildWindow loadingScreen;
        private DateTime _currentDate;
        private DateTime _selectedFromDate;
        private DateTime _selectedToDate;
        private bool _lastTwoDaysChecked;
        private bool _dateRangeChecked;
        private string _searchButtonColour;
        public Dispatcher UIDispatcher { get; set; }
        public DispatchOrderNotifier dispatchOrderNotifier { get; set; }

        private bool IsDirty { get; set; }
        public event EventHandler Closing;
        public RelayCommand CloseCommand { get; private set; }
        private ICommand _dispatchCommand;
        private ICommand _clearSearch;
        private ICommand _doubleClickCommand;
        private ICommand _viewDispatchCommand;
        private ICommand _search;

        public ShippingViewModel()
        {
            DispatchOrders = new ObservableCollection<DispatchOrder>();          
            execute = true;
            DispatchRadio = "ReadyToDispatchChecked";
            //ShipFinalisedVisiblity = "Collapsed";
            SearchButtonColour = "#cccccc";
            
            CurrentDate = DateTime.Now;
            SelectedFromDate = CurrentDate;
            SelectedToDate = CurrentDate;

            BackgroundWorker worker = new BackgroundWorker();
            ChildWindow LoadingScreen = new ChildWindow();
            LoadingScreen.ShowWaitingScreen("Loading");

            worker.DoWork += (_, __) =>
            {               
                this.dispatchOrderNotifier = new DispatchOrderNotifier();
                this.dispatchOrderNotifier.NewMessage += new EventHandler<SqlNotificationEventArgs>(notifier_NewMessage);
                DispatchOrders = this.dispatchOrderNotifier.RegisterDependency(DispatchRadio, DateTime.Now, DateTime.Now);
                
                LoadDispatchOrders(DispatchOrders);
                ReadyToDispatchChecked = true;
            };

            worker.RunWorkerCompleted += delegate(object s, RunWorkerCompletedEventArgs args)
            {
                LoadingScreen.CloseWaitingScreen();
            };
            worker.RunWorkerAsync();
            this.CloseCommand = new RelayCommand(CloseWindow);
        }

        private void Notified(ObservableCollection<DispatchOrder> o)
        {
            LoadDispatchOrders(o);
        }

        void notifier_NewMessage(object sender, SqlNotificationEventArgs e)
        {
            Notified(this.dispatchOrderNotifier.RegisterDependency(DispatchRadio, DateTime.Now, DateTime.Now));
        }
       
        public string Title
        {
            get
            {
                return "Shipping";
            }
        }

       
        public bool CanClose
        {
            get
            {
                return this.IsDirty == false || MessageBox.Show("Changes were made. Do you want to close this window?", "Close", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
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

        private void LoadDispatchOrders(ObservableCollection<DispatchOrder> dol)
        {
            //if (DispatchOrders != null)
            //{
            //    App.Current.Dispatcher.Invoke((Action)delegate // <--- HERE
            //    {
            //        DispatchOrders.Clear();
            //    });
            //}
            //ObservableCollection<DispatchOrder> dol = new ObservableCollection<DispatchOrder>();
            //dol = DBAccess.GetAllDispatchOrders(DispatchRadio, f, t);
            DispatchOrders = new ObservableCollection<DispatchOrder>();
            if (dol != null)
            {
                

                foreach (var item in dol)
                {
                    if (item.DispatchedDate == Convert.ToDateTime("01/01/0001 12:00:00 AM"))
                    {
                        item.DispatchedDateStr = "------";
                    }
                    else
                    {
                        item.DispatchedDateStr = String.Format("{0:dd/MM/yyyy}", item.DispatchedDate);
                    }

                    if (item.OrderStatus == DispatchOrderStatus.Finalised.ToString())
                    {
                        item.StatusStr = DispatchOrderStatus.Finalised.ToString();
                        item.StatusBackgroundCol = "#006622";
                        item.StatusForeGroundCol = "White";
                    }
                    else if (item.OrderStatus == OrderStatus.Cancel.ToString())
                    {
                        item.StatusStr = OrderStatus.Cancel.ToString();
                        item.StatusBackgroundCol = "Red";
                        item.StatusForeGroundCol = "White";
                    }
                    else if (item.OrderStatus == OrderStatus.Return.ToString())
                    {
                        item.StatusStr = "Returned";
                        item.StatusBackgroundCol = "#FFA500";
                        item.StatusForeGroundCol = "White";
                    }
                    else if (item.OrderStatus == OrderStatus.HoldNoCreditStockAllocated.ToString() || item.OrderStatus == OrderStatus.HoldStockAllocated.ToString())
                    {
                        item.StatusStr = "On Hold";
                        item.StatusBackgroundCol = "#F56329";
                        item.StatusForeGroundCol = "White";
                    }
                    else
                    {
                        string strDay = string.Empty;
                        DateTime dumDate = (DateTime)item.DesiredDispatchDate;
                        double days = Math.Ceiling((dumDate - DateTime.Now).TotalDays);
                        if (days < 0)
                        {
                            item.StatusStr = Math.Abs(days) + " Day" + CheckNumber(days) + " Late";
                            item.StatusBackgroundCol = "#E65722";
                            item.StatusForeGroundCol = "White";
                        }
                        else if (days > 0)
                        {
                            item.StatusStr = days + " Day" + CheckNumber(days) + " To Go";
                            item.StatusBackgroundCol = "#009933";
                            item.StatusForeGroundCol = "White";
                        }
                        else if (days == 0)
                        {
                            item.StatusStr = "Dispatch Today";
                            item.StatusBackgroundCol = "#0424c1";
                            item.StatusForeGroundCol = "White";
                        }
                    }

                    item.PaymentFinalisedBackGround = item.PaymentRecieved == true ? "#084FAA" : "#E12222";
                    item.PaymentFinalisedForeGround = item.PaymentRecieved == true ? "White" : "White";

                    item.DeliveryDocketNo = item.DispatchOrderID;
                    item.DeliveryDocketString = item.DeliveryDocketNo == 0 ? "To be Assigned" : item.DeliveryDocketNo.ToString();
                    item.ConNoteNumberString = string.IsNullOrWhiteSpace(item.ConNoteNumber) ? "To be Assigned" : item.ConNoteNumber;

                    item.DispatchOrderVisibility = (item.DispatchOrderStatus == DispatchOrderStatus.Preparing.ToString() && item.OrderStatus == OrderStatus.FinalisingShipping.ToString()) ? "Visible" : "Collapsed";
                    item.ViewDispatchOrderVisibility = item.DispatchOrderStatus == DispatchOrderStatus.Finalised.ToString() ? "Visible" : "Collapsed";

                    if (item.BillToNoLines != null)
                    {
                        item.BillToNoLines = item.BillTo.Replace(Environment.NewLine, " ");
                    }
                    //string st = item.ShipTo.Substring(item.ShipTo.IndexOf("\r\n") + 1).Trim();                   
                    item.ShipToNoLines = !string.IsNullOrWhiteSpace(item.ShipTo) ? item.ShipTo.Replace("\r\n", ",") : string.Empty;

                    //if (DispatchOrders != null)
                    //{
                    //    App.Current.Dispatcher.Invoke((Action)delegate
                    //    {
                    //        DispatchOrders.Add(item);
                    //    });
                    //}
                }

                DispatchOrders = dol;
            }

        }

        private void SendToDispatch(Object parameter)
        {
            int index = DispatchOrders.IndexOf(parameter as DispatchOrder);
            if (index >= 0)
            {               
                    bool isOrderProcessing = true;
                    BackgroundWorker worker = new BackgroundWorker();
                    loadingScreen = new ChildWindow();
                    loadingScreen.ShowWaitingScreen("Loading..");

                    worker.DoWork += (_, __) =>
                    {
                        isOrderProcessing = DBAccess.CheckIfDispatchBeingProcessed(DispatchOrders[index].SalesOrderNo);

                        if (isOrderProcessing == false)
                        {
                            showDispatchScreen = new ChildWindow();
                            DispatchOrders[index].DispatchedBy = UserData.FirstName + " " + UserData.LastName;
                            DispatchOrders[index].IsProcessing = true;
                            //if (DispatchOrders[index].DispatchOrderStatus == DispatchOrderStatus.Preparing.ToString())
                            //{
                                DispatchOrders[index].DeliveryDocketNo = DispatchOrders[index].DispatchOrderID;
                            //}
                        }
                    };
                    worker.RunWorkerCompleted += delegate(object s, RunWorkerCompletedEventArgs args)
                    {
                        loadingScreen.CloseWaitingScreen();

                        if (isOrderProcessing == true)
                        {
                            MessageBox.Show("This order is being processed by someone at the moment", "Processing Order", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                        }
                        else if (DispatchOrders[index].DeliveryDocketNo == 0 && DispatchOrders[index].OrderStatus != OrderStatus.Cancel.ToString())
                        {
                            MessageBox.Show("There has been a problem when generating the Delivery Docket Number", "Failed To Generate Delivery Docket Number", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        else
                        {
                            if (DispatchOrders[index].DeliveryDocketNo > 0 || DispatchOrders[index].OrderStatus == OrderStatus.Cancel.ToString())
                            {
                                showDispatchScreen.ShowDispatchOrderViewWindow(DispatchOrders[index]);
                                showDispatchScreen.dispatchOrderView_Closed += (r =>
                                {
                                    DispatchOrders.Clear();
                                    if (this.dispatchOrderNotifier != null)
                                    {
                                        DateTime from = DateTime.Now;
                                        DateTime to = DateTime.Now;
                                        
                                        if (DateRangeChecked)
                                        {
                                            from = SelectedFromDate;
                                            to = SelectedToDate;
                                        }

                                        Notified(this.dispatchOrderNotifier.RegisterDependency(DispatchRadio, from, to));
                                    }
                                });
                            }
                        }
                    };
                    worker.RunWorkerAsync();
                
            }
        }

        private void LoadDelivertDocketNo()
        {

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


        private void Clear()
        {
            DispatchRadio = "ReadyToDispatchChecked";
            ReadyToDispatchChecked = true;
            SearchString = string.Empty;
        }

        private void SearchData()
        {
            if (this.dispatchOrderNotifier != null)
            {              

                Notified(this.dispatchOrderNotifier.RegisterDependency(DispatchRadio, SelectedFromDate, SelectedToDate));
            }
        }

        private bool CanExecute(object parameter)
        {
            return true;
        }

        private void GetRadioSelected()
        {            

            if(ReadyToDispatchChecked)
            {
                DispatchRadio = "ReadyToDispatchChecked";
            }
           
            else if (LastTwoDaysChecked)
            {
                DispatchRadio = "LastTwoDays";
            }
            else if (DateRangeChecked)
            {
                DispatchRadio = "DateRange";
            }         


            if (this.dispatchOrderNotifier != null)
            {
                Notified(this.dispatchOrderNotifier.RegisterDependency(DispatchRadio, DateTime.Now, DateTime.Now));
            }
        }     

        public bool ReadyToDispatchChecked
        {
            get { return _readyToDispatchChecked; }
            set
            {
                _readyToDispatchChecked = value;
                RaisePropertyChanged("ReadyToDispatchChecked");
                GetRadioSelected();
            }
        }

        public bool LastTwoDaysChecked
        {
            get
            {
                return _lastTwoDaysChecked;
            }
            set
            {
                _lastTwoDaysChecked = value;
                RaisePropertyChanged("LastTwoDaysChecked");
                GetRadioSelected();
                if (LastTwoDaysChecked)
                {
                    SelectedFromDate = DateTime.Now;
                    SelectedToDate = DateTime.Now;
                    DateRangeEnabled = false;
                    SearchButtonColour = "#cccccc";
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
                GetRadioSelected();
                if (DateRangeChecked)
                {
                    DateRangeEnabled = true;
                    SearchButtonColour = "#666666";
                }
            }
        }

        public string SearchString
        {
            get { return _searchString; }
            set
            {
                _searchString = value;
                RaisePropertyChanged("SearchString");
            }
        }

        public string DispatchRadio
        {
            get { return _dispatchRadio; }
            set
            {
                _dispatchRadio = value;
                RaisePropertyChanged("DispatchRadio");

                //if (DispatchOrders != null && DispatchOrders.Count > 0)
                //{
                //    DispatchOrders.Clear();
                //    LoadDispatchOrders(DispatchOrders);
                //}
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

        public DateTime SelectedFromDate
        {
            get
            {
                return _selectedFromDate;
            }
            set
            {
                _selectedFromDate = value;
                RaisePropertyChanged("SelectedFromDate");
            }
        }

        public DateTime SelectedToDate
        {
            get
            {
                return _selectedToDate;
            }
            set
            {
                _selectedToDate = value;
                RaisePropertyChanged("SelectedToDate");
            }
        }

        

        
        private bool _dateRangeEnabled;
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

        public string SearchButtonColour
        {
            get
            {
                return _searchButtonColour;
            }
            set
            {
                _searchButtonColour = value;
                RaisePropertyChanged("SearchButtonColour");
            }
        }

        


        //public ICollectionView ItemsView
        //{
        //    get { return _itemsView; }
        //}

        public ObservableCollection<DispatchOrder> DispatchOrders
        {
            get
            {
                return _dispatchOrders;
            }
            set
            {
                _dispatchOrders = value;
                RaisePropertyChanged("DispatchOrders");
            }
        }

        //public ObservableCollection<DispatchOrder> DispatchOrders
        //{
        //    get { return _dispatchOrders ?? (_dispatchOrders = new ObservableCollection<DispatchOrder>()); }
        //}

        #region COMMANDS

        public ICommand DispatchCommand
        {
            get
            {
                if (_dispatchCommand == null)
                {
                    _dispatchCommand = new DelegateCommand(CanExecute, SendToDispatch);
                }
                return _dispatchCommand;
            }
        }

        public ICommand DoubleClickCommand
        {
            get
            {
                if (_doubleClickCommand == null)
                {
                    _doubleClickCommand = new DelegateCommand(CanExecute, SendToDispatch);
                }
                return _doubleClickCommand;
            }
        }

        public ICommand ViewDispatchCommand
        {
            get
            {
                if (_viewDispatchCommand == null)
                {
                    _viewDispatchCommand = new DelegateCommand(CanExecute, SendToDispatch);
                }
                return _viewDispatchCommand;
            }
        }


        public ICommand ClearSearch
        {
            get
            {
                return _clearSearch ?? (_clearSearch = new CommandHandler(() => Clear(), execute));
            }
        }

        public ICommand Search
        {
            get
            {
                return _search ?? (_search = new CommandHandler(() => SearchData(), execute));
            }
        }



        

      

        #endregion


    }
}

