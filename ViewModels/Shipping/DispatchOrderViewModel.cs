using A1RConsole.Core;
using A1RConsole.DB;
using A1RConsole.Models.Dispatch;
using A1RConsole.Models.Meta;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using A1RConsole.Models.Stock;
using A1RConsole.Commands;
using A1RConsole.Models;
using System.ComponentModel;
using A1RConsole.Models.Comments;
using System.Collections.ObjectModel;
using A1RConsole.Views;
using A1RConsole.Views.Shipping;
using A1RConsole.Bases;
using A1RConsole.Models.Transactions;
using A1RConsole.Models.Freights;
using System.Transactions;
using A1RConsole.PdfGeneration;
using System.Windows;

namespace A1RConsole.ViewModels.Shipping
{
    public class DispatchOrderViewModel : ViewModelBase
    {
        private ChildWindow dispatchConWindow;
        private DispatchOrder _dispatchOrder;
        private string _printLabelEnDis;
        private string _updateEnDis;
        private string _dispatchEnDis;
        private string _cancelEnDis;
        private string _approveVisibility;
        private string _cancelVisibility;
        private string _enDisPrintDeliveryDocketCommand;
        private string _paymentRecieved;
        private string _invoiceNo;
        private string _invoicedDate;
        private string _status;
        private string _dispatchVisibility;
        private bool _conNoteReadOnly;
        private List<Tuple<string, Int16, string>> timeStamps;
        public event Action<int> Closed;
        private Microsoft.Practices.Prism.Commands.DelegateCommand _closeCommand;
        private bool execute;
        private ICommand _dispatchOrderCommand;
        //private ICommand _printLabelsCommand;
        //private ICommand _updateCommand;
        private ICommand _cancelOrderCommand;
        private ICommand _approveCommand;
        private ICommand _lostFocusCommand;
        private ICommand _printDeliveryDocketCommand;
        private TransactionLog transaction;

        public DispatchOrderViewModel(DispatchOrder dispOrd)
        {
            _closeCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(CloseForm);

            OrderManager om = new OrderManager();
            timeStamps = new List<Tuple<string, Int16, string>>();
            DispatchOrder = dispOrd;

            timeStamps = DBAccess.GetDispatchOrderTimeStamp(DispatchOrder.SalesOrderNo);

            DispatchOrder.DispatchOrderItem = new ObservableCollection<DispatchOrderItem>();
            DispatchOrder.FreightCarrier = new FreightCarrier();
            DispatchOrder.FreightDetails = new BindingList<FreightDetails>();

            DispatchOrder tempDis = new DispatchOrder();
            tempDis = DBAccess.GetDispatchOrderDetails(DispatchOrder.SalesOrderNo);
            
            DispatchOrder.DispatchOrderItem = tempDis.DispatchOrderItem;
            DispatchOrder.FreightCarrier = tempDis.FreightCarrier;
            DispatchOrder.FreightDetails = tempDis.FreightDetails;
            DispatchOrder.Comments = tempDis.Comments;
            DispatchOrder.OrderDate = tempDis.OrderDate;
            DispatchOrder.PaymentDueDate = tempDis.PaymentDueDate;
            DispatchOrder.LastModifiedBy = tempDis.LastModifiedBy;
            DispatchOrder.TermsID = tempDis.TermsID;
            DispatchOrder.SalesOrderNo = tempDis.SalesOrderNo;

            PaymentRecieved = tempDis.PaymentRecieved == true ? "Yes" : "No";
            InvoiceNo = tempDis.Invoice.InvoiceNo == 0 ? "" : tempDis.Invoice.InvoiceNo.ToString();
            InvoicedDate = tempDis.Invoice.InvoiceNo == 0 ? "" : tempDis.Invoice.InvoicedDate.ToString("dd/MM/yyyy");

            execute = true;
            DispatchOrder.DispatchedDate = DateTime.Now;
            Status = CoreProcess.ConvertOrderStatusEnum(DispatchOrder.OrderStatus);
            //Load comments


            if (DispatchOrder.DispatchOrderStatus == DispatchOrderStatus.Preparing.ToString())
            {
                PrintLabelEnDis = "True";
                UpdateEnDis = "True";
                DispatchEnDis = "True";
                CancelEnDis = "True";
                ApproveVisibility = "Collapsed";
                CancelVisibility = "Visible";
                EnDisPrintDeliveryDocketCommand = "Collapsed";
                DispatchVisibility = "Visible";
                ConNoteReadOnly = false;
            }
            else if (DispatchOrder.DispatchOrderStatus == DispatchOrderStatus.Finalised.ToString())
            {
                PrintLabelEnDis = "False";
                UpdateEnDis = "False";
                DispatchEnDis = "False";
                CancelEnDis = "False";
                ApproveVisibility = "Collapsed";
                CancelVisibility = "Visible";
                ApproveVisibility = "Collapsed";
                CancelVisibility = "Visible";
                ConNoteReadOnly = true;
                EnDisPrintDeliveryDocketCommand = "Visible";
                DispatchVisibility = "Collapsed";
            }

            if (tempDis.FreightDetails == null)
            {
                MessageBox.Show("Unable to find Freight Details for the order no : " + DispatchOrder.SalesOrderNo + System.Environment.NewLine, "Freight Details Not Found", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.Yes);
                DispatchVisibility = "Collapsed";
                EnDisPrintDeliveryDocketCommand = "Collapsed";
            }
            else if (tempDis.DispatchOrderItem.Count == 0)
            {
                MessageBox.Show("Unable to find Sales Order Details for the order no : " + DispatchOrder.SalesOrderNo + System.Environment.NewLine, "Sales Order Details Not Found", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.Yes);
                DispatchVisibility = "Collapsed";
                EnDisPrintDeliveryDocketCommand = "Collapsed";
            }    
        }

        public void RollBackData(object sender, EventArgs e)
        {
            CloseForm();
        }

        private void CloseForm()
        {
            if (Closed != null)
            {
                int res = 0;
                Closed(res);
                               
               // ClearFields();
            }
        }

        private void DispatchSalesOrder()
        {
            if (MessageBox.Show("Would you like to finalize shipping for this order", "Shipping Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes) == MessageBoxResult.Yes)
            {
                CloseForm();

                var childWindow = new ChildWindow();
                childWindow.ShowDispatchConfirmation(DispatchOrder, timeStamps);
            }
        }

        private void CancelOrder()
        {
            if (MessageBox.Show("Are you sure you want to cancel this order?" + System.Environment.NewLine + "The products will be released to the stock", "Order Cancelling Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes) == MessageBoxResult.Yes)
            {
                try
                {
                    using (TransactionScope tran = new TransactionScope())
                    {
                        //ReleaseDeliveryDocketNo();
                        DBAccess.ReAssignProductStock(DispatchOrder.SalesOrderNo, DispatchOrder.DispatchedBy);
                        int r = 0;
                        Closed(r);
                        tran.Complete();
                    }
                }
                catch (TransactionAbortedException ex)
                {
                    MessageBox.Show("TransactionAbortedException Message: {0}" + ex.Message, "Transaction Aborted Exception", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (ApplicationException ex)
                {
                    MessageBox.Show("TransactionAbortedException Message: {0}"+ ex.Message, "Transaction Aborted Exception", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ApproveOrder()
        {
            if (MessageBox.Show("Are you sure you want to approve this order?", "Order Approving Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes) == MessageBoxResult.Yes)
            {
                ApproveVisibility = "Collapsed";
                CancelVisibility = "Visible";
                CancelEnDis = "True";
            }
        }


        //private void ReleaseDeliveryDocketNo()
        //{
        //    try
        //    {
        //        using (TransactionScope tran = new TransactionScope())
        //        {
        //            int r1 = DBAccess.UpdateDispatchOrderIsProcessing(DispatchOrder.SalesOrderNo, false);
        //            int r2 = DBAccess.ReleaseDeliveryDocketNo(DispatchOrder.DeliveryDocketNo);
        //            if (r1 > 0 && r2 > 0)
        //            {
        //                transaction = new A1QSystem.Model.Transaction.TransactionLog()
        //                {
        //                    TransDateTime = DateTime.Now,
        //                    Transtype = "Released DDNo & DO available" + DispatchOrder.DeliveryDocketNo,
        //                    SalesOrderID = DispatchOrder.SalesOrderNo,
        //                    Products = new List<RawStock>() { new RawStock() { RawProductID = 0, Qty = 0 }, },
        //                    CreatedBy = DispatchOrder.DispatchedBy
        //                };
        //                DBAccess.InsertTransaction(transaction);
        //            }
        //            tran.Complete();

        //        }
        //    }
        //    catch (TransactionAbortedException ex)
        //    {
        //        Msg.Show("TransactionAbortedException Message: {0}", ex.Message, "Transaction Aborted Exception", MsgBoxButtons.OK, MsgBoxImage.Error);
        //    }
        //    catch (ApplicationException ex)
        //    {
        //        Msg.Show("TransactionAbortedException Message: {0}", ex.Message, "Transaction Aborted Exception", MsgBoxButtons.OK, MsgBoxImage.Error);
        //    }
        //}



        //private void UpdateOrder()
        //{

        //    bool dupExist = false;
        //    RemoveZeroSalesOrderDetails();
        //    RemoveZeroFreightDetails();

        //    var prodExists = DispatchOrder.DispatchOrderItem.Where(x => x.Product != null && x.OrderQty != 0).ToList();
        //    var freightExists = DispatchOrder.FreightDetails.Where(x => x.FreightCodeDetails != null && x.Pallets != 0).ToList();

        //    if (DispatchOrder.DispatchOrderItem != null)
        //    {
        //        dupExist = DispatchOrder.DispatchOrderItem.GroupBy(n => n.Product.ProductCode).Any(c => c.Count() > 1);
        //    }

        //    BindingList<SalesOrderDetails> tempSalesOrderDetails = new BindingList<SalesOrderDetails>();
        //    foreach (var item in DispatchOrder.DispatchOrderItem)
        //    {
        //        tempSalesOrderDetails.Add(new SalesOrderDetails() { Product = new Product() { ProductID = item.Product.ProductID } });
        //    }


        //    if (prodExists.Count == 0)
        //    {
        //        Msg.Show("Please enter qty and product", "Details Required", MsgBoxButtons.OK, MsgBoxImage.Information_Red, MsgBoxResult.Yes);
        //    }
        //    else if (freightExists.Count == 0)
        //    {
        //        Msg.Show("Please enter no of pallets and freight code", "Details Required", MsgBoxButtons.OK, MsgBoxImage.Information_Red, MsgBoxResult.Yes);
        //    }
        //    else if (dupExist == true)
        //    {
        //        Msg.Show("Duplicate products exist. Please remove the duplicate products ", "Duplicate Products Exist", MsgBoxButtons.OK, MsgBoxImage.Information_Red, MsgBoxResult.Yes);
        //    }
        //    else if (DispatchOrder.Customer.CustomerType == "Account" && DispatchOrder.Customer.CompanyName == "Select")
        //    {
        //        Msg.Show("Please select Customer", "Customer Required", MsgBoxButtons.OK, MsgBoxImage.Error);
        //    }
        //    else if (DispatchOrder.Customer.CustomerType == "Prepaid" && DispatchOrder.Customer.CompanyName == "Select" && String.IsNullOrWhiteSpace(DispatchOrder.PrepaidCustomerName))
        //    {
        //        Msg.Show("Please enter customer name", "Enter Customer Name", MsgBoxButtons.OK, MsgBoxImage.Error);
        //    }
        //    else if (DispatchOrder.DesiredDispatchDate == null)
        //    {
        //        Msg.Show("Please select Dispatch Date", "Dispatch Date Required", MsgBoxButtons.OK, MsgBoxImage.Error);
        //    }
        //    else if (DispatchOrder.OrderAction == "Select")
        //    {
        //        Msg.Show("Please select Order Status", "Order Status Required", MsgBoxButtons.OK, MsgBoxImage.Error);
        //    }
        //    else if (DispatchOrder.FreightCarrier.FreightName == "Select")
        //    {
        //        Msg.Show("Please select Carrier", "Carrier Name Required", MsgBoxButtons.OK, MsgBoxImage.Error);
        //    }
        //    //else if (DispatchOrder.Customer.CustomerType == "Prepaid" && SelectedPaymentRecieved == "No")
        //    //{
        //    //    Msg.Show("Customer must make the payment to proceed", "Payment Required", MsgBoxButtons.OK, MsgBoxImage.Error);
        //    //}
        //    else
        //    {
        //        ObservableCollection<SalesOrderDetails> sodList = new ObservableCollection<SalesOrderDetails>();
        //        foreach (var item in DispatchOrder.DispatchOrderItem)
        //        {
        //            sodList.Add(new SalesOrderDetails() { Product = item.Product, Quantity = item.DispatchQty });
        //        }

        //        //Set the flag for Product Stock
        //        Tuple<List<Tuple<User, Product>>, List<int>> res1 = DBAccess.SetEditModeProductStock(sodList, DispatchOrder.StockLocation.ID, true, DispatchOrder.DispatchedBy, true);
        //        if (res1.Item1.Count > 0)
        //        {
        //            string str = string.Empty;
        //            foreach (var item in res1.Item1)
        //            {
        //                str += item.Item2.ProductDescription + " is updating by " + item.Item1.FullName + System.Environment.NewLine;
        //            }
        //            Msg.Show("Cannot complete this order at the moment" + System.Environment.NewLine + "Following products are being modified at the moment" + System.Environment.NewLine + System.Environment.NewLine + str + System.Environment.NewLine + "Try again later", "Cannot Complete Order", MsgBoxButtons.OK, MsgBoxImage.Information_Orange);
        //        }
        //        else
        //        {
        //            if (res1.Item2.Count > 0)
        //            {
        //                Tuple<bool, SalesOrder, List<ProductStockReserved>, List<ProductStock>, bool, CustomerCreditHistory> result = null;
        //                //DispatchOrder.Customer.CustomerType = SelectedCustomerType;
        //                DispatchOrder.OrderPriority = DispatchOrder.OrderPriority;
        //                //DispatchOrder.Comments = new Comment();
        //                DispatchOrder.OrderAction = OrderStatus.Release.ToString();
        //                DispatchOrder.SalesOrderDetails = sodList;
        //                //DispatchOrder.FreightCarrier
        //                OrderManager om = new OrderManager();
        //                om.InitiateSalesOrder(DispatchOrder.StockLocation.ID);
        //                result = om.ProcessSalesOrder2(DispatchOrder, DispatchOrder.Customer.CompanyName, DispatchOrder.DispatchedBy, "Yes");

        //                if (result.Item1)
        //                {
        //                    int finalRes = om.UpdateSalesOrderDB(result.Item2, result.Item3, result.Item4, DispatchOrder.DispatchedBy, result.Item6);
        //                    if (finalRes == 1)
        //                    {
        //                        Msg.Show("Order updated successfully", "Order Updated", MsgBoxButtons.OK, MsgBoxImage.OK);
        //                        ClearFields();
        //                        //closeFormVal = 0;
        //                        CloseForm();
        //                    }
        //                    else if (finalRes == 0)
        //                    {
        //                        Msg.Show("You haven't made any changes to update", "No Changes Were Made", MsgBoxButtons.OK, MsgBoxImage.Information);
        //                    }
        //                    else
        //                    {
        //                        Msg.Show("There has been a problem when updating this order" + System.Environment.NewLine + "Err No - E12", "Cannot Update Order", MsgBoxButtons.OK, MsgBoxImage.Error);
        //                    }
        //                }
        //                else
        //                {
        //                    if (result.Item5)
        //                    {
        //                        ClearFields();
        //                        //closeFormVal = 0;
        //                        CloseForm();
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                Msg.Show("Cannot flag product", "Incorrect Flag", MsgBoxButtons.OK, MsgBoxImage.Error);
        //            }
        //            //Un Set the flag for Product Stock

        //            Tuple<List<Tuple<User, Product>>, List<int>> res2 = DBAccess.SetEditModeProductStock(tempSalesOrderDetails, DispatchOrder.StockLocation.ID, false, DispatchOrder.DispatchedBy, false);
        //        }
        //    }
        //}

        private void PrintDeliveryDocket()
        {
            Exception exception = null;
            BackgroundWorker worker = new BackgroundWorker();
            ChildWindow LoadingScreen = new ChildWindow();
            LoadingScreen.ShowWaitingScreen("Printing");

            worker.DoWork += (_, __) =>
            {
                PrintDeliveryDocketPDF pdd = new PrintDeliveryDocketPDF(DispatchOrder);
                exception = pdd.CreateDeliveryDocket();
            };

            worker.RunWorkerCompleted += delegate(object s, RunWorkerCompletedEventArgs args)
            {
                LoadingScreen.CloseWaitingScreen();
                if (exception != null)
                {
                    MessageBox.Show("A problem has occured while the work order is prining. Please try again later." + System.Environment.NewLine + exception, "Printing Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.Yes);
                }
            };
            worker.RunWorkerAsync();
        }

        private void CalculateQtyToMake()
        {
            //productStockList = DBAccess.GetProductStockByStock(1);//Get product stock
            //if (productStockList != null)
            //{

            //    foreach (var item in DispatchOrder.DispatchOrderItem)
            //    {
            //        foreach (var itemPS in productStockList)
            //        {
            //            if (item.Product != null)
            //            {
            //                if (item.Product.ProductID == itemPS.Product.ProductID)
            //                {
            //                    var x = DiscountStructure.FirstOrDefault(z => z.CustomerID == SalesOrder.Customer.CustomerId && z.Category.CategoryID == item.Product.Category.CategoryID);
            //                    if (x != null)
            //                    {
            //                        item.Discount = (x == null ? 0 : x.Discount);
            //                    }
            //                    else
            //                    {
            //                        item.Discount = 0;
            //                    }

            //                    if (item.Quantity <= itemPS.QtyAvailable)
            //                    {
            //                        item.QtyInStock = Decimal.Parse(itemPS.QtyAvailable.ToString("G29"));
            //                        itemPS.QtyAvailable = itemPS.QtyAvailable - item.Quantity;
            //                        item.QtyToMake = 0;
            //                        item.ToMakeCellBack = "White";
            //                        item.ToMakeCellFore = "Black";
            //                        item.InStockCellBack = "Green";
            //                        item.InStockCellFore = "White";
            //                    }
            //                    else if (item.Quantity > itemPS.QtyAvailable && itemPS.QtyAvailable != 0)
            //                    {
            //                        item.QtyInStock = Decimal.Parse(itemPS.QtyAvailable.ToString("G29"));
            //                        itemPS.QtyAvailable = item.Quantity - itemPS.QtyAvailable;
            //                        item.QtyToMake = Decimal.Parse(itemPS.QtyAvailable.ToString("G29"));

            //                        item.ToMakeCellBack = "#FFC33333";
            //                        item.ToMakeCellFore = "White";
            //                        item.InStockCellBack = "#ffb300";
            //                        item.InStockCellFore = "White";
            //                    }
            //                    else if (itemPS.QtyAvailable == 0)
            //                    {
            //                        item.QtyToMake = Decimal.Parse(item.Quantity.ToString("G29"));
            //                        item.QtyInStock = 0;
            //                        item.ToMakeCellBack = "#FFC33333";
            //                        item.ToMakeCellFore = "White";
            //                        item.InStockCellBack = "#FFC33333";
            //                        item.InStockCellFore = "White";
            //                    }
            //                    break;
            //                }
            //            }
            //        }
            //    }
            //}
        }

        private void ClearFields()
        {
            DispatchOrder.ConNoteNumber = string.Empty;
            DispatchOrder.DeliveryDocketNo = 0;
        }

        private void RemoveZeroSalesOrderDetails()
        {
            var itemToRemove = DispatchOrder.DispatchOrderItem.Where(x => x.Product == null).ToList();
            foreach (var item in itemToRemove)
            {
                DispatchOrder.SalesOrderDetails.Remove(item);
            }
        }

        private void RemoveZeroFreightDetails()
        {
            var itemToRemove = DispatchOrder.FreightDetails.Where(x => x.FreightCodeDetails == null).ToList();
            foreach (var item in itemToRemove)
            {
                DispatchOrder.FreightDetails.Remove(item);
            }
        }

        public DispatchOrder DispatchOrder
        {
            get { return _dispatchOrder; }
            set
            {
                _dispatchOrder = value;
                RaisePropertyChanged("DispatchOrder");
            }
        }

        public string PrintLabelEnDis
        {
            get { return _printLabelEnDis; }
            set
            {
                _printLabelEnDis = value;
                RaisePropertyChanged("PrintLabelEnDis");
            }
        }

        public string UpdateEnDis
        {
            get { return _updateEnDis; }
            set
            {
                _updateEnDis = value;
                RaisePropertyChanged("UpdateEnDis");
            }
        }

        public string DispatchEnDis
        {
            get { return _dispatchEnDis; }
            set
            {
                _dispatchEnDis = value;
                RaisePropertyChanged("DispatchEnDis");
            }
        }

        public string CancelEnDis
        {
            get { return _cancelEnDis; }
            set
            {
                _cancelEnDis = value;
                RaisePropertyChanged("CancelEnDis");
            }
        }

        public string ApproveVisibility
        {
            get { return _approveVisibility; }
            set
            {
                _approveVisibility = value;
                RaisePropertyChanged("ApproveVisibility");
            }
        }

        public string CancelVisibility
        {
            get { return _cancelVisibility; }
            set
            {
                _cancelVisibility = value;
                RaisePropertyChanged("CancelVisibility");
            }
        }

        public string EnDisPrintDeliveryDocketCommand
        {
            get { return _enDisPrintDeliveryDocketCommand; }
            set
            {
                _enDisPrintDeliveryDocketCommand = value;
                RaisePropertyChanged("EnDisPrintDeliveryDocketCommand");
            }
        }



        public bool ConNoteReadOnly
        {
            get { return _conNoteReadOnly; }
            set
            {
                _conNoteReadOnly = value;
                RaisePropertyChanged("ConNoteReadOnly");
            }
        }

        public string PaymentRecieved
        {
            get { return _paymentRecieved; }
            set
            {
                _paymentRecieved = value;
                RaisePropertyChanged("PaymentRecieved");
            }
        }

        public string InvoiceNo
        {
            get { return _invoiceNo; }
            set
            {
                _invoiceNo = value;
                RaisePropertyChanged("InvoiceNo");
            }
        }

        public string InvoicedDate
        {
            get { return _invoicedDate; }
            set
            {
                _invoicedDate = value;
                RaisePropertyChanged("InvoicedDate");
            }
        }

        public string Status
        {
            get { return _status; }
            set
            {
                _status = value;
                RaisePropertyChanged("Status");
            }
        }

        public string DispatchVisibility
        {
            get { return _dispatchVisibility; }
            set
            {
                _dispatchVisibility = value;
                RaisePropertyChanged("DispatchVisibility");
            }
        }
        
        
        
        
        #region COMMANDS

        public ICommand DispatchOrderCommand
        {
            get
            {
                return _dispatchOrderCommand ?? (_dispatchOrderCommand = new CommandHandler(() => DispatchSalesOrder(), execute));
            }
        }

        //public ICommand PrintLabelsCommand
        //{
        //    get
        //    {
        //        return _printLabelsCommand ?? (_printLabelsCommand = new LogOutCommandHandler(() => PrintLabels(), execute));
        //    }
        //}

        //public ICommand UpdateCommand
        //{
        //    get
        //    {
        //        return _updateCommand ?? (_updateCommand = new LogOutCommandHandler(() => UpdateOrder(), execute));
        //    }
        //}

        public ICommand CancelOrderCommand
        {
            get
            {
                return _cancelOrderCommand ?? (_cancelOrderCommand = new CommandHandler(() => CancelOrder(), execute));
            }
        }

        public ICommand ApproveCommand
        {
            get
            {
                return _approveCommand ?? (_approveCommand = new CommandHandler(() => ApproveOrder(), execute));
            }
        }

        public ICommand LostFocusCommand
        {
            get
            {
                return _lostFocusCommand ?? (_lostFocusCommand = new CommandHandler(() => CalculateQtyToMake(), execute));
            }
        }

        public ICommand PrintDeliveryDocketCommand
        {
            get
            {
                return _printDeliveryDocketCommand ?? (_printDeliveryDocketCommand = new CommandHandler(() => PrintDeliveryDocket(), execute));
            }
        }

        public Microsoft.Practices.Prism.Commands.DelegateCommand CloseCommand
        {
            get { return _closeCommand; }
        }

        #endregion
    }
}

