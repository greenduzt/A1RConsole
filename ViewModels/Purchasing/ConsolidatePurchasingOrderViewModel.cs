using A1RConsole.Bases;
using A1RConsole.Commands;
using A1RConsole.Core;
using A1RConsole.DB;
using A1RConsole.Models.Purchasing;
using A1RConsole.PdfGeneration;
using A1RConsole.Views;
using Microsoft.Practices.Prism.Commands;
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

namespace A1RConsole.ViewModels.Purchasing
{
    public class ConsolidatePurchasingOrderViewModel : ViewModelBase
    {
        private int closeVal;
        private int mergedPO;
        private int mergedIndex;
        private string userName;
        private string _createPOVisibility;
        private string _mergePOVisibility;
        private bool _isNewPurchaseOrder;
        private bool _isMergeToExisting;
        private bool _mergeEnabled;
        private bool _createNewEnabled;
        private ListCollectionView _purchasingOrderListCollView = null;
        private ObservableCollection<PurchaseOrder> oldExPurchasingOrders;
        private ObservableCollection<PurchaseOrder> _exPurchasingOrders;
        private PurchaseOrder _newPurchasingOrder;
        private Microsoft.Practices.Prism.Commands.DelegateCommand _closeCommand;
        private DelegateCommand<string> _createPOCommand;
        //private ICommand _mergePOCommand;
        private ICommand _isMergedCommand;

        public event Action<int> Closed;

        public ConsolidatePurchasingOrderViewModel(ObservableCollection<PurchaseOrder> exPurchasingOrders, PurchaseOrder newPurchasingOrder, string un)
        {
            oldExPurchasingOrders = new ObservableCollection<PurchaseOrder>();
            userName = UserData.UserName ;
            var clonedList = exPurchasingOrders.Select(objEntity => (PurchaseOrder)objEntity.Clone()).ToList();
            oldExPurchasingOrders = new ObservableCollection<PurchaseOrder>(clonedList);

            ExPurchasingOrders = oldExPurchasingOrders;

            NewPurchasingOrder = newPurchasingOrder;
            PurchasingOrderListCollView = new ListCollectionView(ExPurchasingOrders);
            PurchasingOrderListCollView.GroupDescriptions.Add(new PropertyGroupDescription("PurchasingOrderNo"));
            IsNewPurchaseOrder = true;

            _closeCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(CloseForm);
        }

        private void CloseForm()
        {
            closeVal = 2;
            CloseNow();
        }

        private void CloseNow()
        {
            if (Closed != null)
            {
                Closed(closeVal);
            }
        }

        private bool CanExecute(object parameter)
        {
            return true;
        }

        private void MergeClicked(object parameter)
        {
            int index = ExPurchasingOrders.IndexOf(parameter as PurchaseOrder);
            if (index > -1 && index < ExPurchasingOrders.Count)
            {
                mergedPO = ExPurchasingOrders[index].PurchasingOrderNo;
                mergedIndex = index;
                oldExPurchasingOrders = DBAccess.GetPendingPurchasingOrders(ExPurchasingOrders[index].Supplier.SupplierID);
                ExPurchasingOrders = oldExPurchasingOrders;
                foreach (var item in _newPurchasingOrder.PurchaseOrderDetails)
                {
                    var data = oldExPurchasingOrders.FirstOrDefault(x => x.PurchasingOrderNo == ExPurchasingOrders[index].PurchasingOrderNo && x.PurchaseOrderDetails[0].Product.ProductID == item.Product.ProductID);


                    if (data != null)
                    {
                        foreach (var items in ExPurchasingOrders)
                        {
                            if (items.PurchasingOrderNo == data.PurchasingOrderNo && items.PurchaseOrderDetails[0].Product.ProductID == data.PurchaseOrderDetails[0].Product.ProductID)
                            {
                                items.PurchaseOrderDetails[0].OrderQty = data.PurchaseOrderDetails[0].OrderQty + item.OrderQty;
                                items.PurchaseOrderDetails[0].LineDesiredRecieveDate = data.PurchaseOrderDetails[0].LineDesiredRecieveDate;
                                items.PurchaseOrderDetails[0].LastUpdatedBy = userName;
                                items.PurchaseOrderDetails[0].LastUpdatedDate = DateTime.Now;
                            }
                        }
                    }
                    else
                    {
                        var data1 = oldExPurchasingOrders.FirstOrDefault(x => x.PurchasingOrderNo == ExPurchasingOrders[index].PurchasingOrderNo && x.PurchaseOrderDetails[0].Product.ProductID == ExPurchasingOrders[index].PurchaseOrderDetails[0].Product.ProductID);


                        bool e = ExPurchasingOrders.Any(q => q.PurchasingOrderNo == data1.PurchasingOrderNo && q.PurchaseOrderDetails[0].Product.ProductID == item.Product.ProductID);
                        if (e == false)
                        {
                            //item.LineNo = data1.PurchaseOrderDetails[0].LineNo + 1;
                            PurchaseOrder po = new PurchaseOrder();
                            po.PurchasingOrderNo = ExPurchasingOrders[index].PurchasingOrderNo;
                            po.OrderDate = ExPurchasingOrders[index].OrderDate;
                            po.ReceivedDate = ExPurchasingOrders[index].ReceivedDate;
                            po.PurchaseOrderDetails = new ObservableCollection<PurchaseOrderDetails>();
                            po.PurchaseOrderDetails.Add(item);
                            ExPurchasingOrders.Add(po);
                        }
                    }
                }
                PurchasingOrderListCollView = new ListCollectionView(ExPurchasingOrders);
                PurchasingOrderListCollView.GroupDescriptions.Add(new PropertyGroupDescription("PurchasingOrderNo"));
                //PurchasingOrderListCollView.SortDescriptions.Add(new System.ComponentModel.SortDescription("PurchaseOrderDetails.LineNo", System.ComponentModel.ListSortDirection.Ascending));
                ExPurchasingOrders[index].Ticked = true;

                for (int i = 0; i < ExPurchasingOrders.Count; i++)
                {
                    ExPurchasingOrders[i].IsExpanded = i == index ? true : false;
                }
            }
        }

        private void CreatePO(string str)
        {
            int res = 0;
            Tuple<Exception, string> tuple = null;
            BackgroundWorker worker = new BackgroundWorker();
            ChildWindow LoadingScreen;
            LoadingScreen = new ChildWindow();
            LoadingScreen.ShowWaitingScreen("Loading");

            worker.DoWork += (_, __) =>
            {
                PurchaseOrder po = new PurchaseOrder();

                if (str == "merge")
                {
                    int exNoteSize = ExPurchasingOrders[mergedIndex].Notes.ToCharArray().Count();
                    int newNoteSize = NewPurchasingOrder.Notes.ToCharArray().Count();

                    po.PurchasingOrderNo = ExPurchasingOrders[mergedIndex].PurchasingOrderNo;
                    po.Completed = false;
                    po.OrderDate = DateTime.Now;
                    po.OrderTIme = DateTime.Now.TimeOfDay;
                    po.Supplier = ExPurchasingOrders[mergedIndex].Supplier;
                    po.RecieveOnDate = NewPurchasingOrder.RecieveOnDate;
                    po.LastModifiedBy = userName;
                    po.LastModifiedDate = DateTime.Now;
                    po.SupplierQuoteReference = NewPurchasingOrder.SupplierQuoteReference == null ? "" : NewPurchasingOrder.SupplierQuoteReference;
                    po.PurchaseFrom = NewPurchasingOrder.PurchaseFrom;
                    po.ShipTo = NewPurchasingOrder.ShipTo;
                    po.Status = ReceivingStatus.Pending.ToString();
                    po.PurchaseOrderDetails = new ObservableCollection<PurchaseOrderDetails>();

                    foreach (var item in ExPurchasingOrders)
                    {
                        if (item.PurchasingOrderNo == mergedPO)
                        {
                            foreach (var items in item.PurchaseOrderDetails)
                            {
                                items.LineDesiredRecieveDate = po.RecieveOnDate;
                                items.LastUpdatedBy = userName;
                                items.LastUpdatedDate = DateTime.Now;
                                po.PurchaseOrderDetails.Add(items);
                            }
                        }
                    }
                    po.SubTotal = po.PurchaseOrderDetails.Sum(x => x.Total);
                    po.Tax = (po.SubTotal * 10) / 100;
                    po.TotalAmount = po.SubTotal + po.Tax;
                    po.Notes = exNoteSize > newNoteSize ? ExPurchasingOrders[mergedIndex].Notes : NewPurchasingOrder.Notes;
                    res = DBAccess.ConsolidatePurchaseOrder(po);
                }
                else if (str == "create")
                {
                    NewPurchasingOrder.Status = ReceivingStatus.Pending.ToString();
                    NewPurchasingOrder.OrderTIme = DateTime.Now.TimeOfDay;
                    NewPurchasingOrder.Notes = NewPurchasingOrder.Notes;
                    res = DBAccess.CreatePurchaseOrder(NewPurchasingOrder, userName);
                    po = NewPurchasingOrder;
                    po.PurchasingOrderNo = res;
                }

                if (res > 0)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        if (MessageBox.Show("Purchase order created successfully!" + System.Environment.NewLine + "Would you like to print it?", "Purchase Order Created", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                        {
                            if (po.Status == "Pending")
                            {
                                if (string.IsNullOrWhiteSpace(po.Notes))
                                {
                                    po.Notes = "PLEASE DELIVER TO GATE 1";
                                }
                                PrintPurchaseOrderPDF poPdf = new PrintPurchaseOrderPDF(po);
                                tuple = poPdf.CreatePurcheOrderDoc();
                            }
                        }
                    });
                }
            };
            worker.RunWorkerCompleted += delegate(object s, RunWorkerCompletedEventArgs args)
            {
                LoadingScreen.CloseWaitingScreen();

                if (res == 0)
                {
                    MessageBox.Show("Error has occured while creating the purchase order!" + System.Environment.NewLine + "Please try again later", "Did Not Create Purchase Order", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    closeVal = 1;
                    CloseNow();
                }

                if (tuple != null)
                {
                    if (tuple.Item1 != null)
                    {
                        MessageBox.Show("A problem has occured while prining. Please try again later." + System.Environment.NewLine + tuple.Item1, "Printing Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.Yes);
                    }
                    else
                    {
                        var childWindow = new ChildWindow();
                        childWindow.ShowFormula(tuple.Item2);
                    }
                }

            };
            worker.RunWorkerAsync();

        }



        #region PUBLIC_PROPERTIES

        public ListCollectionView PurchasingOrderListCollView
        {
            get { return _purchasingOrderListCollView; }
            set
            {
                _purchasingOrderListCollView = value;
                RaisePropertyChanged("PurchasingOrderListCollView");

            }
        }

        public ObservableCollection<PurchaseOrder> ExPurchasingOrders
        {
            get
            {
                return _exPurchasingOrders;
            }
            set
            {
                _exPurchasingOrders = value;
                RaisePropertyChanged("ExPurchasingOrders");
            }
        }

        public PurchaseOrder NewPurchasingOrder
        {
            get
            {
                return _newPurchasingOrder;
            }
            set
            {
                _newPurchasingOrder = value;
                RaisePropertyChanged("NewPurchasingOrder");
            }
        }

        public bool IsNewPurchaseOrder
        {
            get
            {
                return _isNewPurchaseOrder;
            }
            set
            {
                _isNewPurchaseOrder = value;
                RaisePropertyChanged("IsNewPurchaseOrder");
                if (IsNewPurchaseOrder)
                {
                    CreateNewEnabled = true;
                    MergeEnabled = false;
                    CreatePOVisibility = "Visible";
                    MergePOVisibility = "Collapsed";
                    ObservableCollection<PurchaseOrder> po = DBAccess.GetPendingPurchasingOrders(ExPurchasingOrders[0].Supplier.SupplierID);
                    foreach (var item in ExPurchasingOrders)
                    {
                        var data = po.FirstOrDefault(x => x.PurchasingOrderNo == item.PurchasingOrderNo && x.PurchaseOrderDetails[0].Product.ProductID == item.PurchaseOrderDetails[0].Product.ProductID);
                        if (data != null)
                        {
                            item.IsExpanded = false;
                            item.Ticked = false;
                            item.PurchaseOrderDetails[0].OrderQty = data.PurchaseOrderDetails[0].OrderQty;
                        }
                        else
                        {
                            ExPurchasingOrders.Remove(item);
                            break;
                        }
                    }
                }
            }
        }

        public bool IsMergeToExisting
        {
            get
            {
                return _isMergeToExisting;
            }
            set
            {
                _isMergeToExisting = value;
                RaisePropertyChanged("IsMergeToExisting");
                if (IsMergeToExisting)
                {
                    CreateNewEnabled = false;
                    MergeEnabled = true;
                    CreatePOVisibility = "Collapsed";
                    MergePOVisibility = "Visible";
                }
            }
        }

        public bool MergeEnabled
        {
            get
            {
                return _mergeEnabled;
            }
            set
            {
                _mergeEnabled = value;
                RaisePropertyChanged("MergeEnabled");
            }
        }

        public bool CreateNewEnabled
        {
            get
            {
                return _createNewEnabled;
            }
            set
            {
                _createNewEnabled = value;
                RaisePropertyChanged("CreateNewEnabled");
            }
        }

        public string CreatePOVisibility
        {
            get
            {
                return _createPOVisibility;
            }
            set
            {
                _createPOVisibility = value;
                RaisePropertyChanged("CreatePOVisibility");
            }
        }

        public string MergePOVisibility
        {
            get
            {
                return _mergePOVisibility;
            }
            set
            {
                _mergePOVisibility = value;
                RaisePropertyChanged("MergePOVisibility");
            }
        }

        private static bool CanButtonPress(string button)
        {
            return true;
        }

        #endregion

        #region COMMANDS

        public Microsoft.Practices.Prism.Commands.DelegateCommand CloseCommand
        {
            get { return _closeCommand; }
        }

        public ICommand IsMergedCommand
        {
            get
            {
                if (_isMergedCommand == null)
                {
                    _isMergedCommand = new A1RConsole.Commands.DelegateCommand(CanExecute, MergeClicked);
                }
                return _isMergedCommand;
            }
        }


        public ICommand CeratePOCommand
        {
            get
            {
                if (_createPOCommand == null)
                {
                    _createPOCommand = new DelegateCommand<string>(
                        CreatePO, CanButtonPress);
                }
                return _createPOCommand;
            }
        }

        #endregion
    }
}

