using A1RConsole.Bases;
using A1RConsole.Commands;
using A1RConsole.Core;
using A1RConsole.DB;
using A1RConsole.Interfaces;
using A1RConsole.Models;
using A1RConsole.Models.Invoices;
using A1RConsole.Models.Meta;
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


namespace A1RConsole.ViewModels.Invoicing
{
    public class InvoicingActivityViewModel : ViewModelBase, IContent
    {
        private int _tabSelectedIndex;
        private bool execute;
        private bool _tickAll;
        private string _version;
        private string _filePath;
        private ChildWindow loadingScreen;
        private ChildWindow showInvoicingScreen;
        private ObservableCollection<Invoice> _invoicingList;
        private ObservableCollection<Invoice> _invoicedList;
        private List<Tuple<string, Int16, string>> timeStamp;
        //private ObservableCollection<Invoice> _invoiceToMyObList;
        private bool IsDirty { get; set; }
        public event EventHandler Closing;
        public RelayCommand CloseCommand { get; private set; }
        private ICommand _prepareInvoiceCommand;
        private ICommand _viewDetailsCommand;
        private ICommand _printInvoiceCommand;
        private ICommand _sendToMyOBCommand;
        private ICommand _changeFileLocationCommand;
        private ICommand _refreshCommand;

        public InvoicingActivityViewModel()
        {            
            TabSelectedIndex = 0;
            
            execute = true;
            InvoicingList = new ObservableCollection<Invoice>();
            InvoicedList = new ObservableCollection<Invoice>();
            timeStamp = new List<Tuple<string, short, string>>();
            List<MetaData> tempMeta = DBAccess.GetMetaData();
            var d = tempMeta.SingleOrDefault(x => x.KeyName == "invoice_to_myob_path");
            FilePath = d.Description;
           
            this.CloseCommand = new RelayCommand(CloseWindow);
        }

        public string Title
        {
            get
            {
                return "Invoicing";
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

        private void LoadOrders()
        {
            InvoicingList = DBAccess.GetReadyToInvoiceOrders(false, true);
            foreach (var item in InvoicingList)
            {
                if (item.OrderStatus == OrderStatus.HoldStockAllocated.ToString() || item.OrderStatus == OrderStatus.HoldNoCreditStockAllocated.ToString())
                {
                    item.OrderStatus = "On Hold";
                    item.OrderStatusVisibility = "Visible";
                    item.PrepareInvoiceActive = false;
                    item.PrepareInvoiceBackGround = "#a6a6a6";
                    item.StatusBackgroundCol = "#F56329";
                }
                else
                {
                    item.OrderStatusVisibility = "Collapsed";
                    item.PrepareInvoiceActive = true;
                    item.PrepareInvoiceBackGround = "#666666";
                }
            }
        }

     

        private void OpenInvoicing(Object parameter)
        {
            int index = InvoicingList.IndexOf(parameter as Invoice);
            if (index >= 0)
            {
                BackgroundWorker worker = new BackgroundWorker();
                loadingScreen = new ChildWindow();
                loadingScreen.ShowWaitingScreen("Loading..");
                worker.DoWork += (_, __) =>
                {
                    showInvoicingScreen = new ChildWindow();
                };
                worker.RunWorkerCompleted += delegate(object s, RunWorkerCompletedEventArgs args)
                {
                    InvoicingList[index].CompletedBy = UserData.UserName;
                    InvoicingList[index].CompletedDate = DateTime.Now;
                    showInvoicingScreen.ShowInvoicingWindow(InvoicingList[index]);
                    showInvoicingScreen.invoicingView_Closed += (r =>
                    {
                        if (r == 0)
                        {
                            InvoicingList.Clear();
                            LoadOrders();
                        }
                    });
                };
                worker.RunWorkerAsync();
            }
        }

        private void OpenInvoiced(Object parameter)
        {
            int index = InvoicedList.IndexOf(parameter as Invoice);
            if (index >= 0)
            {
                BackgroundWorker worker = new BackgroundWorker();
                loadingScreen = new ChildWindow();
                loadingScreen.ShowWaitingScreen("Loading..");
                worker.DoWork += (_, __) =>
                {
                    showInvoicingScreen = new ChildWindow();
                };
                worker.RunWorkerCompleted += delegate(object s, RunWorkerCompletedEventArgs args)
                {
                    InvoicedList[index].CompletedBy = UserData.UserName;
                    InvoicedList[index].CompletedDate = DateTime.Now;
                    showInvoicingScreen.ShowInvoicingWindow(InvoicedList[index]);
                    showInvoicingScreen.invoicingView_Closed += (r =>
                    {
                        if (r == 0)
                        {
                            InvoicedList.Clear();
                            LoadInvoices();
                            //TickAll = false;
                        }
                    });
                };
                worker.RunWorkerAsync();
            }
        }

        private void LoadInvoices()
        {
            BackgroundWorker worker = new BackgroundWorker();
            ChildWindow LoadingScreen = new ChildWindow();
            LoadingScreen.ShowWaitingScreen("Loading");

            worker.DoWork += (_, __) =>
            {
                if (TabSelectedIndex == 1)
                {
                    //Load invoiced orders
                    InvoicedList = DBAccess.GetReadyToInvoiceOrders(true, false);
                    timeStamp = DBAccess.GetInvoiceTimeStamp(InvoicedList, false);
                }
                else if(TabSelectedIndex == 0)
                {
                    LoadOrders();
                }
            };
            worker.RunWorkerCompleted += delegate(object s, RunWorkerCompletedEventArgs args)
            {
                LoadingScreen.CloseWaitingScreen();
            };
            worker.RunWorkerAsync();
        }

        private void PrintInvoice(Object parameter)
        {
            int index = InvoicedList.IndexOf(parameter as Invoice);
            if (index >= 0)
            {                
                if (MessageBox.Show("Print invoice for Sales Order : " + InvoicedList[index].SalesOrderNo + "?", "View Invoice", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    InvoicedList[index].Invoice = new Invoice();
                    InvoicedList[index].Invoice.InvoicedDate = InvoicedList[index].InvoicedDate;
                    InvoicingManager im = new InvoicingManager();
                    im.GenerateInvoice(InvoicedList[index]);
                }                        
            }
        }

        private void SendToMyOB()
        {
            bool s = InvoicedList.Any(x => x.ExportToMyOb == true);
            if (s)
            {
                timeStamp = DBAccess.GetInvoiceTimeStamp(InvoicedList, false);
                if (InvoicedList.Count > 0)
                {
                    List<Invoice> newList = ConsolidateInvoices(InvoicedList);
                    int res = DBAccess.UpdateExportToMyOB(InvoicedList, timeStamp);
                    if (res == 1)
                    {
                        SendToExcel ste = new SendToExcel(newList, FilePath);
                        ste.ConvertToExcel();

                        MessageBox.Show("Invoices have been successfully transfered to MyOB", "MyOB Transfer Successfull", MessageBoxButton.OK, MessageBoxImage.Information);
                        
                    }
                    else if (res == -1)
                    {
                        MessageBox.Show("Data has been changed since you opened this form!!!" + System.Environment.NewLine + "Refreshing Data", "Data Changed", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else if (res == -2)
                    {
                        MessageBox.Show("You haven't made any changes to transfer", "No Changes Detected", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else
                    {
                        MessageBox.Show("There has been a problem while transfering invoices to MyOB", "MyOB Transfer Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                    }

                    InvoicedList.Clear();
                    LoadInvoices();
                    if (TickAll)
                    {
                        TickAll = false;
                    }
                }
            }
            else
            {
                MessageBox.Show("No invoices selected to transfer to excel" + System.Environment.NewLine + "Tick Send To MyOb check box to select", "Select Items", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private List<Invoice> ConsolidateInvoices(ObservableCollection<Invoice> il)
        {
            List<Invoice> invoiceList = DBAccess.GetSalesOrderDetailsForInvoices(il);
            return invoiceList;
        }

        private void ChangeFileLocation()
        {
            System.Windows.Forms.FolderBrowserDialog brwsr = new System.Windows.Forms.FolderBrowserDialog();
            brwsr.SelectedPath = FilePath;
            if (brwsr.ShowDialog() == System.Windows.Forms.DialogResult.Cancel)
                return;
            else
            {
                //string s  = brwSelectedPath.Replace("\","\\");
                int r = DBAccess.UpdateInvoiceToMyObFileLocation(brwsr.SelectedPath);
                FilePath = brwsr.SelectedPath;
                //Do whatever with the new path
            }
        }

        private bool CanExecute(object parameter)
        {
            return true;
        }

        public bool TickAll
        {
            get
            {
                return _tickAll;
            }
            set
            {
                _tickAll = value;
                RaisePropertyChanged("TickAll");

                if (TickAll == true)
                {
                    foreach (var item in InvoicedList)
                    {
                        if (item.ExportedToMyOb == false && item.OrderStatus != "Returned")
                        {
                            item.ExportToMyOb = true;
                        }
                    }
                }
                else
                {
                    //InvoicedList.Clear();
                    //LoadInvoices();
                    foreach (var item in InvoicedList)
                    {
                        if (item.ExportedToMyOb == false)
                        {
                            item.ExportToMyOb = false;
                        }
                    }
                }
            }
        }

        public string FilePath
        {
            get
            {
                return _filePath;
            }
            set
            {
                _filePath = value;
                RaisePropertyChanged("FilePath");
            }
        }

        public ObservableCollection<Invoice> InvoicingList
        {
            get
            {
                return _invoicingList;
            }
            set
            {
                _invoicingList = value;
                RaisePropertyChanged("InvoicingList");
            }
        }

        public ObservableCollection<Invoice> InvoicedList
        {
            get
            {
                return _invoicedList;
            }
            set
            {
                _invoicedList = value;
                RaisePropertyChanged("InvoicedList");
            }
        }

        //public ObservableCollection<Invoice> InvoiceToMyObList
        //{
        //    get
        //    {
        //        return _invoiceToMyObList;
        //    }
        //    set
        //    {
        //        _invoiceToMyObList = value;
        //        RaisePropertyChanged(() => this.InvoiceToMyObList);
        //    }
        //}



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

        public int TabSelectedIndex
        {
            get
            {
                return _tabSelectedIndex;
            }
            set
            {
                _tabSelectedIndex = value;
                RaisePropertyChanged("TabSelectedIndex");
                LoadInvoices();
            }
        }


        #region COMMANDS



        public ICommand RefreshCommand
        {
            get
            {
                return _refreshCommand ?? (_refreshCommand = new CommandHandler(() => LoadInvoices(), execute));
            }
        }

        public ICommand SendToMyOBCommand
        {
            get
            {
                return _sendToMyOBCommand ?? (_sendToMyOBCommand = new CommandHandler(() => SendToMyOB(), execute));
            }
        }

        public ICommand ChangeFileLocationCommand
        {
            get
            {
                return _changeFileLocationCommand ?? (_changeFileLocationCommand = new CommandHandler(() => ChangeFileLocation(), execute));
            }
        }



        public ICommand PrepareInvoiceCommand
        {
            get
            {
                if (_prepareInvoiceCommand == null)
                {
                    _prepareInvoiceCommand = new DelegateCommand(CanExecute, OpenInvoicing);
                }
                return _prepareInvoiceCommand;
            }
        }

        public ICommand ViewDetailsCommand
        {
            get
            {
                if (_viewDetailsCommand == null)
                {
                    _viewDetailsCommand = new DelegateCommand(CanExecute, OpenInvoiced);
                }
                return _viewDetailsCommand;
            }
        }

        public ICommand PrintInvoiceCommand
        {
            get
            {
                if (_printInvoiceCommand == null)
                {
                    _printInvoiceCommand = new DelegateCommand(CanExecute, PrintInvoice);
                }
                return _printInvoiceCommand;
            }
        }



        #endregion
    }
}

