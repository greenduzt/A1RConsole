using A1RConsole.Bases;
using A1RConsole.Commands;
using A1RConsole.Core;
using A1RConsole.DB;
using A1RConsole.Interfaces;
using A1RConsole.Models.Purchasing;
using A1RConsole.Models.Suppliers;
using A1RConsole.PdfGeneration;
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
using System.Windows.Input;

namespace A1RConsole.ViewModels.Purchasing
{
    public class PrintPurchaseOrderViewModel : ViewModelBase, IContent
    {
        private ObservableCollection<PurchaseOrder> _purchaseOrder;
        private Supplier _selectedSupplier;
        private List<Supplier> _supplierList;
        private List<string> _fromPurchaseOrderNos;
        private List<string> _toPurchaseOrderNos;
        private string _fromSelectedPurchaseOrderNo;
        private string _toSelectedPurchaseOrderNo;
        private bool IsDirty { get; set; }
        public event EventHandler Closing;
        public RelayCommand CloseCommand { get; private set; }
        private ICommand _showCommand;       
        private ICommand _printPurchaseOrderCommand;

        public PrintPurchaseOrderViewModel()
        {           
            PurchaseOrder = new ObservableCollection<PurchaseOrder>();

            LoadPurchaseOrderNos();
            Loadsuppliers();
            this.CloseCommand = new RelayCommand(CloseWindow);
        }

        public string Title
        {
            get
            {
                return "Print Purchasing Order";
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

        private void LoadPurchaseOrderNos()
        {
            List<string> purchaseOrderNos = DBAccess.LoadPurchasingOrderNos();
            FromPurchaseOrderNos = new List<string>();
            ToPurchaseOrderNos = new List<string>();

            //foreach (var item in purchaseOrderNos)
            //{
            //    FromPurchaseOrderNos.Add(item);
            //}

            //foreach (var item in purchaseOrderNos)
            //{
            //    ToPurchaseOrderNos.Add(item);
            //}
            FromPurchaseOrderNos = purchaseOrderNos;
            ToPurchaseOrderNos = purchaseOrderNos;
            //PurchaseOrderNos.Add("");
        }

        private void Loadsuppliers()
        {
            SupplierList = DBAccess.GetAllSuppliers(true);
        }

        private void ShowPOs()
        {
            if (string.IsNullOrWhiteSpace(FromSelectedPurchaseOrderNo) && string.IsNullOrWhiteSpace(ToSelectedPurchaseOrderNo) && SelectedSupplier == null)
            {
                Msg.Show("Please enter Purchase Order No or Supplier", "Information Required", MsgBoxButtons.OK, MsgBoxImage.Information_Orange, MsgBoxResult.Yes);
            }
            else
            {
                PurchaseOrder = DBAccess.GetPurchaseOrdersBySearch(SelectedSupplier, FromSelectedPurchaseOrderNo, ToSelectedPurchaseOrderNo);
            }
        }

      
        private bool CanExecute(object parameter)
        {
            return true;
        }

        private void PrintPO(Object parameter)
        {

            int index = PurchaseOrder.IndexOf(parameter as PurchaseOrder);
            if (index >= 0)
            {
                Tuple<Exception, string> tuple = null;
                BackgroundWorker worker = new BackgroundWorker();
                ChildWindow LoadingScreen = new ChildWindow();
                LoadingScreen.ShowWaitingScreen("Loading");
                worker.DoWork += (_, __) =>
                {
                    PrintPurchaseOrderPDF poPdf = new PrintPurchaseOrderPDF(PurchaseOrder[index]);
                    tuple = poPdf.CreatePurcheOrderDoc();
                };

                worker.RunWorkerCompleted += delegate(object s, RunWorkerCompletedEventArgs args)
                {
                    if (tuple.Item1 != null)
                    {
                        Msg.Show("A problem has occured while prining. Please try again later." + System.Environment.NewLine + tuple.Item1, "Printing Error", MsgBoxButtons.OK, MsgBoxImage.Information_Red, MsgBoxResult.Yes);
                    }
                    else
                    {
                        var childWindow = new ChildWindow();
                        childWindow.ShowFormula(tuple.Item2);
                    }
                };
                worker.RunWorkerAsync();
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

        public List<string> FromPurchaseOrderNos
        {
            get { return _fromPurchaseOrderNos; }

            set
            {
                _fromPurchaseOrderNos = value;
                base.RaisePropertyChanged("FromPurchaseOrderNos");
            }
        }

        public List<string> ToPurchaseOrderNos
        {
            get { return _toPurchaseOrderNos; }

            set
            {
                _toPurchaseOrderNos = value;
                base.RaisePropertyChanged("ToPurchaseOrderNos");
            }
        }

        public Supplier SelectedSupplier
        {
            get { return _selectedSupplier; }

            set
            {
                _selectedSupplier = value;
                base.RaisePropertyChanged("SelectedSupplier");
            }
        }

        public string FromSelectedPurchaseOrderNo
        {
            get { return _fromSelectedPurchaseOrderNo; }

            set
            {
                _fromSelectedPurchaseOrderNo = value;
                base.RaisePropertyChanged("FromSelectedPurchaseOrderNo");
            }
        }

        public string ToSelectedPurchaseOrderNo
        {
            get { return _toSelectedPurchaseOrderNo; }

            set
            {
                _toSelectedPurchaseOrderNo = value;
                base.RaisePropertyChanged("ToSelectedPurchaseOrderNo");
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

        public ICommand ShowCommand
        {
            get
            {
                return _showCommand ?? (_showCommand = new CommandHandler(() => ShowPOs(), true));
            }
        }

       

        public ICommand PrintPurchaseOrderCommand
        {
            get
            {
                if (_printPurchaseOrderCommand == null)
                {
                    _printPurchaseOrderCommand = new DelegateCommand(CanExecute, PrintPO);
                }
                return _printPurchaseOrderCommand;
            }
        }
    }
}
