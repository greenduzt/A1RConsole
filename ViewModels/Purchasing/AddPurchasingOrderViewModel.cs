using A1RConsole.Bases;
using A1RConsole.Commands;
using A1RConsole.Core;
using A1RConsole.DB;
using A1RConsole.Interfaces;
using A1RConsole.Models.Products;
using A1RConsole.Models.Purchasing;
using A1RConsole.Models.Suppliers;
using A1RConsole.PdfGeneration;
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
    public class AddPurchasingOrderViewModel : ViewModelBase, IContent
    {
        private string formType;
        private string shipTo;
        private Product prod;
        private List<Supplier> _supplierList;
        public List<Product> Product { get; set; }
        private Supplier _selectedSupplier;
        private string _selectedStatus;
        private DateTime _currentDate;
        private PurchaseOrder _purchaseOrder;
        private bool canExecute;
        private bool _supplierSelectable;
        private int _itemCount;
        public string userName;
        public event Action<PurchaseOrder> Closed;
        private ChildWindow ChildWindowView;
        private bool IsDirty { get; set; }
        public event EventHandler Closing;
        //public RelayCommand CloseCommand { get; private set; }
        private ICommand _addCommand;
        private ICommand _selectionChangedCommand;
        private ICommand _lostFocusCommand;
        private ICommand _removeCommand;
        private ICommand _supplierSelectionChangedCommand;
        private Microsoft.Practices.Prism.Commands.DelegateCommand _closeCommand;

        public AddPurchasingOrderViewModel(Product p)
        {
            formType = "popup";
            userName = UserData.FirstName + " " + UserData.LastName;
            SelectedStatus = "Released";
            CurrentDate = DateTime.Now;
            canExecute = true;          
            PurchaseOrder = new PurchaseOrder();
            PurchaseOrder.PurchaseOrderDetails = new ObservableCollection<PurchaseOrderDetails>();
            PurchaseOrder.RecieveOnDate = CurrentDate;
            PurchaseOrder.Notes = "PLEASE DELIVER TO GATE 1";
            SupplierList = DBAccess.GetAllSuppliers(true);
            prod = p;
           
            shipTo = DBAccess.GetSystemParameterByParaCode("A1Address");

            if (prod == null)
            {
                SupplierSelectable = true;
                PurchaseOrder.OrderDate = CurrentDate;
                PurchaseOrder.ShipTo = shipTo;
            }
            else
            {
                SupplierSelectable = false;
                //PurchaseOrderDetails = new ObservableCollection<PurchaseOrderDetails>();
                BusinessDaysGenerator bdg = new BusinessDaysGenerator();
                PurchaseOrder.PurchaseOrderDetails.Add(new PurchaseOrderDetails() { OrderQty = prod.MinimumOrderQty, Product = new Product() { ProductID = prod.ProductID, ProductCode = prod.ProductCode, ProductDescription = prod.ProductDescription, ProductUnit = prod.ProductUnit, MaterialCost = prod.MaterialCost } });

                if (prod != null)
                {
                    PurchaseOrder po = DBAccess.GetSupplierDetailsByProduct(prod);
                    if (po.Supplier != null)
                    {
                        SelectedSupplier = po.Supplier;
                        PurchaseOrder.OrderDate = DateTime.Now;
                        PurchaseOrder.RecieveOnDate = bdg.AddBusinessDaysWithoutWeekEnds(PurchaseOrder.OrderDate, po.Supplier.LeadTime);
                        PurchaseOrder.Supplier = po.Supplier;
                        PurchaseOrder.ShipTo = po.ShipTo;
                        if (PurchaseOrder.Supplier != null)
                        {
                            string sName = PurchaseOrder.Supplier.SupplierName;
                            string sAddress = PurchaseOrder.Supplier.SupplierAddress;
                            string sSuburb = PurchaseOrder.Supplier.SupplierSuburb;
                            string sState = PurchaseOrder.Supplier.SupplierState;
                            string sPostCode = PurchaseOrder.Supplier.SupplierPostCode;

                            PurchaseOrder.PurchaseFrom = sName += System.Environment.NewLine + sAddress + System.Environment.NewLine;
                            PurchaseOrder.PurchaseFrom += sSuburb + System.Environment.NewLine;
                            PurchaseOrder.PurchaseFrom += sState + ", " + sPostCode;
                        }
                    }
                    else
                    {
                        MessageBox.Show("Cannot find supplier details! Please try again later", "Cannot Find Supplier", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Cannot find product details! Please try again later", "Cannot Find Product", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            if (PurchaseOrder.PurchaseOrderDetails != null)
            {
                PurchaseOrder.PurchaseOrderDetails.CollectionChanged += OnPurchaseOrderCollChanged;
                this.PurchaseOrder.PurchaseOrderDetails = SequencingService.SetCollectionSequence(this.PurchaseOrder.PurchaseOrderDetails);
            }

            //this.CloseCommand = new RelayCommand(CloseWindow);
            _closeCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(CloseForm1);

            CalculateFinalTotal();
        }

        

        public AddPurchasingOrderViewModel()
        {
            formType = "new";
            SelectedStatus = "Released";
            CurrentDate = DateTime.Now;
            canExecute = true;
            PurchaseOrder = new PurchaseOrder();
            PurchaseOrder.PurchaseOrderDetails = new ObservableCollection<PurchaseOrderDetails>();
            PurchaseOrder.RecieveOnDate = CurrentDate;
            PurchaseOrder.Notes = "PLEASE DELIVER TO GATE 1";
            SupplierList = DBAccess.GetAllSuppliers(true);

            shipTo = DBAccess.GetSystemParameterByParaCode("A1Address");

            if (prod == null)
            {
                SupplierSelectable = true;
                PurchaseOrder.OrderDate = CurrentDate;
                PurchaseOrder.ShipTo = shipTo;
            }
            else
            {
                SupplierSelectable = false;
                //PurchaseOrderDetails = new ObservableCollection<PurchaseOrderDetails>();
                BusinessDaysGenerator bdg = new BusinessDaysGenerator();
                PurchaseOrder.PurchaseOrderDetails.Add(new PurchaseOrderDetails() { OrderQty = prod.MinimumOrderQty, Product = new Product() { ProductID = prod.ProductID, ProductCode = prod.ProductCode, ProductDescription = prod.ProductDescription, ProductUnit = prod.ProductUnit, MaterialCost = prod.MaterialCost } });

                if (prod != null)
                {
                    PurchaseOrder po = DBAccess.GetSupplierDetailsByProduct(prod);
                    if (po.Supplier != null)
                    {
                        SelectedSupplier = po.Supplier;
                        PurchaseOrder.OrderDate = DateTime.Now;
                        PurchaseOrder.RecieveOnDate = bdg.AddBusinessDaysWithoutWeekEnds(PurchaseOrder.OrderDate, po.Supplier.LeadTime);
                        PurchaseOrder.Supplier = po.Supplier;
                        PurchaseOrder.ShipTo = po.ShipTo;
                        if (PurchaseOrder.Supplier != null)
                        {
                            string sName = PurchaseOrder.Supplier.SupplierName;
                            string sAddress = PurchaseOrder.Supplier.SupplierAddress;
                            string sSuburb = PurchaseOrder.Supplier.SupplierSuburb;
                            string sState = PurchaseOrder.Supplier.SupplierState;
                            string sPostCode = PurchaseOrder.Supplier.SupplierPostCode;

                            PurchaseOrder.PurchaseFrom = sName += System.Environment.NewLine + sAddress + System.Environment.NewLine;
                            PurchaseOrder.PurchaseFrom += sSuburb + System.Environment.NewLine;
                            PurchaseOrder.PurchaseFrom += sState + ", " + sPostCode;
                        }
                    }
                    else
                    {
                        MessageBox.Show("Cannot find supplier details! Please try again later", "Cannot Find Supplier", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Cannot find product details! Please try again later", "Cannot Find Product", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            if (PurchaseOrder.PurchaseOrderDetails != null)
            {
                PurchaseOrder.PurchaseOrderDetails.CollectionChanged += OnPurchaseOrderCollChanged;
                this.PurchaseOrder.PurchaseOrderDetails = SequencingService.SetCollectionSequence(this.PurchaseOrder.PurchaseOrderDetails);
            }

            //this.CloseCommand = new RelayCommand(CloseWindow);
            _closeCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(CloseForm2);
            CalculateFinalTotal();
        }

        private void CloseForm1()
        {
            Closed(PurchaseOrder);
        }

        private void CloseForm2()
        {
            CloseWindow(this);
        }

        private void AddPurchaseOrder()
        {
            bool dupExist = false;
            RemoveZeroPurchaseDetails();
            var prodExists = PurchaseOrder.PurchaseOrderDetails.Where(x => x.Product != null && x.OrderQty != 0).ToList();

            if (PurchaseOrder.PurchaseOrderDetails != null)
            {
                dupExist = PurchaseOrder.PurchaseOrderDetails.GroupBy(n => n.Product.ProductCode).Any(c => c.Count() > 1);
            }

            int res = 0;
            if (SelectedStatus == "Select")
            {
                MessageBox.Show("Please select purchasing order status", "Order Status Required", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (prodExists.Count == 0)
            {
                MessageBox.Show("Please enter qty and product", "Details Required", MessageBoxButton.OK, MessageBoxImage.Stop, MessageBoxResult.Yes);
            }
            else if (dupExist == true)
            {
                MessageBox.Show("Duplicate products exist. Please remove the duplicate products ", "Duplicate Products Exist", MessageBoxButton.OK, MessageBoxImage.Stop, MessageBoxResult.Yes);
            }
            else
            {
                PurchaseOrder.Supplier = SelectedSupplier;

                List<SupplierProduct> spList = DBAccess.GetAllSupplierProducts();
                if (spList != null || spList.Count > 0)
                {
                    string p = string.Empty;
                    foreach (var item in PurchaseOrder.PurchaseOrderDetails)
                    {
                        bool b = spList.Any(x => x.ProductID == item.Product.ProductID && x.Supplier.SupplierID == PurchaseOrder.Supplier.SupplierID);
                        if (b == false)
                        {
                            p = "[" + item.Product.ProductCode + "] " + item.Product.ProductDescription;
                            break;
                        }
                    }

                    if (String.IsNullOrWhiteSpace(p))
                    {
                        if (PurchaseOrder.Notes == "PLEASE DELIVER TO GATE 1")
                        {
                            PurchaseOrder.Notes = string.Empty;
                        }

                        ObservableCollection<PurchaseOrder> exPurchasingOrders = DBAccess.GetPendingPurchasingOrders(PurchaseOrder.Supplier.SupplierID);
                        if (exPurchasingOrders.Count > 0)
                        {
                            ChildWindow cw = new ChildWindow();
                            cw.ShowConsolidatePurchasingOrderView(exPurchasingOrders, PurchaseOrder, userName);
                            cw.openConsolidatePurchasingOrderView_Closed += (r =>
                            {
                                if (r == 1)
                                {
                                    ClearFields();
                                }
                            });
                        }
                        else
                        {
                            Tuple<Exception, string> tuple = null;
                            BackgroundWorker worker = new BackgroundWorker();
                            ChildWindow LoadingScreen;
                            LoadingScreen = new ChildWindow();
                            LoadingScreen.ShowWaitingScreen("Printing");

                            worker.DoWork += (_, __) =>
                            {
                                PurchaseOrder.Status = ReceivingStatus.Pending.ToString();
                                PurchaseOrder.OrderTIme = DateTime.Now.TimeOfDay;

                                res = DBAccess.CreatePurchaseOrder(PurchaseOrder, userName);
                                if (res > 0)
                                {
                                    PurchaseOrder.PurchasingOrderNo = res;

                                    Application.Current.Dispatcher.Invoke(() =>
                                    {
                                        if (MessageBox.Show("Purchase order created successfully!" + System.Environment.NewLine + "Would you like to print it?", "Purchase Order Created", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                                        {
                                            if (PurchaseOrder.Status == "Pending")
                                            {
                                                if (string.IsNullOrWhiteSpace(PurchaseOrder.Notes))
                                                {
                                                    PurchaseOrder.Notes = "PLEASE DELIVER TO GATE 1";
                                                }
                                                PrintPurchaseOrderPDF poPdf = new PrintPurchaseOrderPDF(PurchaseOrder);
                                                tuple = poPdf.CreatePurcheOrderDoc();
                                            }
                                        }
                                    });
                                }
                                if (formType == "popup")
                                {
                                    Closed(PurchaseOrder);
                                }                                
                                //CloseForm();
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
                                    ClearFields();
                                }

                                if (tuple != null)
                                {
                                    if (tuple.Item1 != null)
                                    {
                                        MessageBox.Show("A problem has occured while prining. Please try again later." + System.Environment.NewLine + tuple.Item1, "Printing Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.Yes);
                                    }
                                }
                                else
                                {
                                    //var childWindow = new ChildWindow();
                                    //childWindow.ShowFormula(tuple.Item2);
                                }

                            };
                            worker.RunWorkerAsync();

                        }
                    }
                    else
                    {
                        MessageBox.Show(p + " does not belong to supplier " + PurchaseOrder.Supplier.SupplierName, "Product Does Not Belong To Supplier", MessageBoxButton.OK, MessageBoxImage.Question);
                    }
                }
            }
        }

        private void RemoveZeroPurchaseDetails()
        {
            var itemToRemove = PurchaseOrder.PurchaseOrderDetails.Where(x => x.Product == null).ToList();
            foreach (var item in itemToRemove)
            {
                PurchaseOrder.PurchaseOrderDetails.Remove(item);
            }
        }

        private void LoadProducts()
        {
            Product = DBAccess.GetProductBySupplier(SelectedSupplier.SupplierID);

        }

        public string Title
        {
            get
            {
                return "New Purchasing Order";
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


        void OnPurchaseOrderCollChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            this.ItemCount = this.PurchaseOrder.PurchaseOrderDetails.Count;

            // Resequence list
            SequencingService.SetCollectionSequence(this.PurchaseOrder.PurchaseOrderDetails);
            CalculateFinalTotal();
        }

        private void CalculateFinalTotal()
        {
            if (PurchaseOrder != null)
            {
                PurchaseOrder.SubTotal = PurchaseOrder.PurchaseOrderDetails.Sum(x => x.Total);
                PurchaseOrder.Tax = (PurchaseOrder.SubTotal * 10) / 100;  //((PurchaseOrder.m + SalesOrder.FreightTotal) * 10) / 100;
                PurchaseOrder.TotalAmount = PurchaseOrder.SubTotal + PurchaseOrder.Tax;
            }
        }

        private bool CanExecute(object parameter)
        {
            return true;
        }

        private void RemoveItem(object parameter)
        {
            int index = PurchaseOrder.PurchaseOrderDetails.IndexOf(parameter as PurchaseOrderDetails);
            if (index > -1 && index < PurchaseOrder.PurchaseOrderDetails.Count)
            {
                PurchaseOrder.PurchaseOrderDetails.RemoveAt(index);
                ObservableCollection<PurchaseOrderDetails> tempColl = new ObservableCollection<PurchaseOrderDetails>();
                tempColl = PurchaseOrder.PurchaseOrderDetails;
                PurchaseOrder.PurchaseOrderDetails = new ObservableCollection<PurchaseOrderDetails>();
                PurchaseOrder.PurchaseOrderDetails = tempColl;
                //SalesOrder.SalesOrderDetails.Add(new SalesOrderDetails() { Quantity=0 });
            }
            if (PurchaseOrder.PurchaseOrderDetails.Count == 0)
            {
                PurchaseOrder.PurchaseOrderDetails = new ObservableCollection<PurchaseOrderDetails>();
            }

            PurchaseOrder.PurchaseOrderDetails.CollectionChanged += OnPurchaseOrderCollChanged;
            this.PurchaseOrder.PurchaseOrderDetails = SequencingService.SetCollectionSequence(this.PurchaseOrder.PurchaseOrderDetails);
        }

        private void SupplierSelectionChanged()
        {
            if (Product.Count == 0)
            {
                if (SelectedSupplier != null)
                {
                    MessageBox.Show("There are no products found for supplier: " + SelectedSupplier.SupplierName, "Products Not Found", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                }
                else
                {   
                    MessageBox.Show("Supplier not found", "", MessageBoxButton.OK, MessageBoxImage.Stop);
                }
            }
        }


        private void ClearFields()
        {
            PurchaseOrder = new PurchaseOrder();
            PurchaseOrder.PurchaseOrderDetails = new ObservableCollection<PurchaseOrderDetails>();
            if (PurchaseOrder.PurchaseOrderDetails != null)
            {
                PurchaseOrder.PurchaseOrderDetails.CollectionChanged += OnPurchaseOrderCollChanged;
                this.PurchaseOrder.PurchaseOrderDetails = SequencingService.SetCollectionSequence(this.PurchaseOrder.PurchaseOrderDetails);
            }

            SelectedSupplier = new Supplier();
            CurrentDate = DateTime.Now;
            PurchaseOrder.RecieveOnDate = DateTime.Now;
            PurchaseOrder.Notes = "PLEASE DELIVER TO GATE 1";
            PurchaseOrder.OrderDate = DateTime.Now;
            PurchaseOrder.ShipTo = shipTo;
        }

        private PurchaseOrderDetails _dataGridSelectedItem;
        public PurchaseOrderDetails DataGridSelectedItem
        {
            get
            {
                return _dataGridSelectedItem;
            }
            set
            {
                _dataGridSelectedItem = value;
                RaisePropertyChanged("DataGridSelectedItem");
                this.SupplierSelectionChangedCommand.Execute(this.DataGridSelectedItem);
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


        public PurchaseOrder PurchaseOrder
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

                if (SelectedSupplier != null)
                {
                    if (prod == null)
                    {
                        PurchaseOrder.PurchaseOrderDetails.Clear();
                    }
                    LoadProducts();
                    string sName = SelectedSupplier.SupplierName;
                    string sAddress = SelectedSupplier.SupplierAddress;
                    string sSuburb = SelectedSupplier.SupplierSuburb;
                    string sState = SelectedSupplier.SupplierState;
                    string sPostCode = SelectedSupplier.SupplierPostCode;

                    PurchaseOrder.PurchaseFrom = sName += System.Environment.NewLine + sAddress + System.Environment.NewLine;
                    PurchaseOrder.PurchaseFrom += sSuburb + System.Environment.NewLine;
                    PurchaseOrder.PurchaseFrom += sState + ", " + sPostCode;

                }
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


        public bool SupplierSelectable
        {
            get
            {
                return _supplierSelectable;
            }
            set
            {
                _supplierSelectable = value;
                RaisePropertyChanged("SupplierSelectable");
            }
        }





        //public ObservableCollection<PurchaseOrderDetails> PurchaseOrderDetails
        //{
        //    get
        //    {
        //        return _purchaseOrderDetails;
        //    }
        //    set
        //    {
        //        _purchaseOrderDetails = value;
        //        RaisePropertyChanged(() => this.PurchaseOrderDetails);
        //    }
        //}


        public int ItemCount
        {
            get { return _itemCount; }

            set
            {
                _itemCount = value;
                base.RaisePropertyChanged("ItemCount");
            }
        }


        public ICommand SupplierSelectionChangedCommand
        {
            get
            {
                return _supplierSelectionChangedCommand ?? (_supplierSelectionChangedCommand = new CommandHandler(() => SupplierSelectionChanged(), canExecute));
            }
        }


      

        public ICommand AddCommand
        {
            get
            {
                return _addCommand ?? (_addCommand = new CommandHandler(() => AddPurchaseOrder(), canExecute));
            }
        }

        public ICommand SelectionChangedCommand
        {
            get
            {
                return _selectionChangedCommand ?? (_selectionChangedCommand = new CommandHandler(() => CalculateFinalTotal(), canExecute));
            }
        }

        public ICommand LostFocusCommand
        {
            get
            {
                return _lostFocusCommand ?? (_lostFocusCommand = new CommandHandler(() => CalculateFinalTotal(), canExecute));
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

        public Microsoft.Practices.Prism.Commands.DelegateCommand CloseCommand
        {
            get { return _closeCommand; }
        }
    }
}

