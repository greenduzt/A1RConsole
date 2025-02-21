using A1RConsole.Bases;
using A1RConsole.Commands;
using A1RConsole.Core;
using A1RConsole.DB;
using A1RConsole.Models.Customers;
using A1RConsole.Models.Dispatch;
using A1RConsole.Models.Freights;
using A1RConsole.Models.Invoices;
using A1RConsole.Models.Orders;
using A1RConsole.Views;
using A1RConsole.Views.Invoicing;
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
    public class InvoicingViewModel : ViewModelBase
    {
        //private ObservableCollection<FreightCode> _freightCodeDetails;
        private DateTime _currentDate;
        private DateTime _selectedInvoiceDate;
        private string _exportUpdateVisibility;
        private string _invoiceBtnVisiblity;
        private Invoice _invoice;
        private bool _exportToMyOBEnabled;
        private bool execute;
        private bool _gSTActive;
        private bool _invoiceDateEnabled;
        private bool _freightGridEnbaled;
        private List<Tuple<string, Int16, string>> timeStamp;
        public event Action<int> Closed;
        private ICommand _createInvoiceCommand;
        private ICommand _exportToMyObCommand;
        private ICommand _freightPriceKeyUpCommand;
        private ICommand _lostFocusCommand;
        private Microsoft.Practices.Prism.Commands.DelegateCommand _closeCommand;

        public InvoicingViewModel(Invoice inv)
        {
            //FreightCodeDetails = new ObservableCollection<FreightCode>();
            Invoice = inv;
            execute = true;
            CurrentDate = DateTime.Now;
            SelectedInvoiceDate = inv.InvoicedDate;
            Invoice.CompletedBy = UserData.FirstName + " " + UserData.LastName;
            _closeCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(CloseForm);
            //Update SendToMyOB in InvoiceToMyOB table
            DBAccess.InsertInToSendToMyOB(Invoice);
            timeStamp = new List<Tuple<string, short, string>>();
            ObservableCollection<Invoice> tempInvList = new ObservableCollection<Invoice>();
            tempInvList.Add(new Invoice() { SalesOrderNo = Invoice.SalesOrderNo });
            timeStamp = DBAccess.GetInvoiceTimeStamp(tempInvList, true);
            Invoice.FreightDetails = new BindingList<FreightDetails>();
            //LoadFreightCodes();
            LoadData();            

            if (Invoice.IsCompleted)
            {
                FreightGridEnbaled = false;
            }
            else
            {
                FreightGridEnbaled = true;
            }

            Invoice.FreightDetails.ListChanged += freightChanged;
        }

        void freightChanged(object sender, ListChangedEventArgs e)
        {
            CalculateFinalTotal();
        }

        //private void LoadFreightCodes()
        //{
        //    FreightCodeDetails = DBAccess.GetFreightCodes();
        //}

        private void LoadData()
        {
            Invoice tempInvoice = new Invoice();
            tempInvoice = DBAccess.GetInvoice(Invoice);
            if (tempInvoice != null)
            {
                if (tempInvoice.OrderStatus == OrderStatus.Return.ToString())
                {
                    ExportToMyOBEnabled = false;
                    ExportUpdateVisibility = "Collapsed";
                    InvoiceBtnVisiblity = "Collapsed";
                    tempInvoice.OrderAction = "Returned";
                    InvoiceDateEnabled = false;
                }
                else
                {
                    if (tempInvoice.IsCompleted && tempInvoice.IsActive == false)
                    {
                        ExportUpdateVisibility = "Visible";
                        InvoiceBtnVisiblity = "Collapsed";
                        tempInvoice.OrderAction = OrderStatus.Dispatched.ToString();
                        InvoiceDateEnabled = false;
                    }
                    else if (tempInvoice.IsCompleted == false && tempInvoice.IsActive)
                    {
                        ExportUpdateVisibility = "Collapsed";
                        InvoiceBtnVisiblity = "Visible";
                        tempInvoice.OrderAction = OrderStatus.PreparingInvoice.ToString();
                        InvoiceDateEnabled = true;
                    }
                    ExportToMyOBEnabled = tempInvoice.ExportedToMyOb == true ? false : true;
                }
                
                tempInvoice.CompletedDate = DateTime.Now;
                Invoice = tempInvoice;
                Invoice.CompletedBy = UserData.FirstName + " " + UserData.LastName;
                GSTActive = tempInvoice.GSTEnabled;                              
            }
            else
            {
                MessageBox.Show("Cannot load invoice details. Please try again later", "Invoice Details Not Found", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        private void CreateInvoice()
        {
            if (MessageBox.Show("Are you sure you want to create an invoice for this order?", "Creating Invoice Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes) == MessageBoxResult.Yes)
            {
                RemoveZeroFreightDetails();
                CustomerCreditHistory cchnew = new CustomerCreditHistory();
                CustomerCreditActivity cca = new CustomerCreditActivity();                
                //Check if it has enough credit
               if(Invoice.Customer.CustomerType == "Account")
               {
                   cchnew.SalesOrderNo = Invoice.SalesOrderNo;
                   cchnew.Customer = new Customer() { CustomerId = Invoice .Customer.CustomerId};
                   CustomerCreditHistory cch = DBAccess.GetCustomerCreditHistoryRecord(Invoice.SalesOrderNo, Invoice.Customer.CustomerId);
                   cca.Customer = new Customer() { CustomerId = Invoice.Customer.CustomerId };
                   decimal difference = 0;

                   if (cch.CreditDeducted != Invoice.TotalAmount)
                   {
                       if (Invoice.TotalAmount > cch.CreditDeducted)
                       {
                           difference = Invoice.TotalAmount - cch.CreditDeducted;

                           if (Invoice.Customer.CreditRemaining >= difference)
                           {

                               if (difference <= Invoice.Customer.CreditRemaining)
                               {
                                   cchnew.TotalDebt = 0;
                                   cchnew.TotalCreditRemaining = Invoice.Customer.CreditRemaining - difference;
                                   cchnew.CreditDeducted = Invoice.TotalAmount;
                               }
                               else
                               {
                                   decimal x = difference - Invoice.Customer.CreditRemaining;
                                   cchnew.TotalCreditRemaining = 0;
                                   cchnew.TotalDebt = Invoice.Customer.Debt + Math.Abs(x);
                                   cchnew.CreditDeducted = Invoice.TotalAmount;
                               }
                               cchnew.TotalCreditOwed = Invoice.Customer.CreditOwed + difference;                               
                               cca.SalesOrderNo = Invoice.SalesOrderNo;
                               cca.Amount = Invoice.TotalAmount;
                               cca.Type = "Updated";
                               cca.Activity = "Order updated credit deducted : " + String.Format("{0:C}", difference);
                               cca.UpdatedDate = DateTime.Now;
                               cca.UpdatedBy = UserData.FirstName + " " + UserData.LastName;
                           }
                           else
                           {
                               //Not enough credit
                               MessageBox.Show("Insufficient Credit!" + System.Environment.NewLine + "Credit remaining : " + String.Format("{0:C}", Invoice.Customer.CreditRemaining), "Insufficient Credit", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                               goto Exit;
                           }
                       }
                       else
                       {
                           difference = cch.CreditDeducted - Invoice.TotalAmount;
                           if (Invoice.Customer.CreditRemaining >= difference)
                           {
                               cchnew.TotalDebt = (Invoice.Customer.Debt - difference) < 0 ? 0 : Invoice.Customer.Debt - difference;

                               if (cchnew.TotalDebt == 0)
                               {
                                   cchnew.TotalCreditRemaining = Invoice.Customer.CreditRemaining + Math.Abs(Invoice.Customer.Debt - difference);
                                   cchnew.CreditDeducted = Invoice.TotalAmount;
                                   cchnew.TotalCreditOwed = Invoice.Customer.CreditOwed - difference;
                               }
                               else
                               {
                                   cchnew.TotalCreditRemaining = (Invoice.Customer.CreditRemaining - cchnew.TotalDebt) <= 0 ? 0 : Invoice.Customer.CreditRemaining - cchnew.TotalDebt;
                                   cchnew.CreditDeducted = Invoice.TotalAmount;
                                   cchnew.TotalCreditOwed = Invoice.Customer.CreditOwed - cchnew.TotalDebt < 0 ? 0 : Invoice.Customer.CreditOwed - cchnew.TotalDebt;
                               }
                               
                               cca.SalesOrderNo = Invoice.SalesOrderNo;
                               cca.Amount = Invoice.TotalAmount;
                               cca.Type = "Updated";
                               cca.Activity = "Order updated credit reallocated : " + String.Format("{0:C}", difference);
                               cca.UpdatedDate = DateTime.Now;
                               cca.UpdatedBy = UserData.FirstName + " " + UserData.LastName;
                           }
                           else
                           {
                               //Not enough credit
                               MessageBox.Show("Insufficient Credit!" + System.Environment.NewLine + "Credit remaining : " + String.Format("{0:C}", Invoice.Customer.CreditRemaining), "Insufficient Credit", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                               goto Exit; 
                           }
                       }
                   }
               }           

                ObservableCollection<SalesOrderDetails> sod = DBAccess.GetSalesOrderDetailsBySalesNo(Invoice);
                int res = DBAccess.FinalizeOrderDB(Invoice, Invoice.CompletedBy, sod, Invoice.ExportToMyOb, timeStamp, cchnew, cca);
                if (res == 1)
                {
                    if (MessageBox.Show("Sales Order : " + Invoice.SalesOrderNo + " (Invoice : " + Invoice.InvoiceNo + ") was successfully completed" + System.Environment.NewLine + "Do you want to print this Invoice?", "Invoice Printing Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes) == MessageBoxResult.Yes)
                    {
                        SalesOrder so = new SalesOrder();
                        so.SalesOrderNo = Invoice.SalesOrderNo;
                        so.Invoice = new Invoice();
                        so.Invoice.InvoicedDate = SelectedInvoiceDate;
                        //so.Invoice = new Invoice();
                        //so.Invoice.InvoicedDate = Invoice.InvoicedDate;
                        //pageSwitcher.Container.Children.Add(new MdiChild
                        //{
                        //    Name = "ChangeInvoiceDate",
                        //    Title = "Change Invoice Date",
                        //    Content = new ChangeInvoiceDateView(pageSwitcher, so),
                        //    Width = 400,
                        //    MaxWidth = 400,
                        //    MaxHeight = 400,
                        //    Height = 400,
                        //    ToolTip = "Change Invoice Date",
                        //    Tag = "ChangeInvoiceDate"
                        //});

                        InvoicingManager im = new InvoicingManager();
                        im.GenerateInvoice(so);
                    }
                }
                else if (res == -1)
                {
                    MessageBox.Show("Data has been changed since you opened this form!!!" + System.Environment.NewLine + "Please close the form and re-open again", "Data Changed", MessageBoxButton.OK, MessageBoxImage.Stop);
                }
                else
                {
                    MessageBox.Show("There has been a problem while finalizing this order. Please try again later", "Cannot Finalize Order", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }

            Exit:

                CloseForm();
            }
        }

        private void UpdateExportMyOb()
        {
            if (Invoice.ExportedToMyOb == false)
            {
                int res = DBAccess.UpdateSendToMyOB(Invoice.SalesOrderNo, Invoice.ExportToMyOb);
                if (res > 0)
                {
                    MessageBox.Show("Data Updated Successfully.", "Data Updated", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("You haven't made any changes to update.", "No Changes Detected", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("This invoice has been already sent to MyOB", "Already Exported To MyOB", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void RemoveZeroFreightDetails()
        {
            var itemToRemove = Invoice.FreightDetails.Where(x => x.FreightCodeDetails == null).ToList();
            foreach (var item in itemToRemove)
            {
                Invoice.FreightDetails.Remove(item);
            }
        }

        private void CalculateFreightTotal()
        {
            foreach (var item in Invoice.FreightDetails)
            {
                item.Total = Math.Round(item.FreightCodeDetails.Price * item.Pallets,2);
            }
            CalculateFinalTotal();
        }

        private void CloseForm()
        {
            if (Closed != null)
            {
                int res = 0;
                Closed(res);
            }
        }

     

        private void CalculateFinalTotal()
        {
            if (Invoice != null)
            {
                Invoice.ListPriceTotal = Invoice.SalesOrderDetails.Sum(x => x.Total);
                Invoice.FreightTotal = Invoice.FreightDetails.Sum(x => x.Total);
                Invoice.GST = GSTActive ? ((Invoice.ListPriceTotal + Invoice.FreightTotal) * 10) / 100 : 0;
                Invoice.TotalAmount = Invoice.ListPriceTotal + Invoice.FreightTotal + Invoice.GST;
            }
        }

        

        public Invoice Invoice
        {
            get { return _invoice; }
            set
            {
                _invoice = value;
                RaisePropertyChanged("Invoice");

            }
        }

        public string InvoiceBtnVisiblity
        {
            get { return _invoiceBtnVisiblity; }
            set
            {
                _invoiceBtnVisiblity = value;
                RaisePropertyChanged("InvoiceBtnVisiblity");
            }
        }


        public string ExportUpdateVisibility
        {
            get { return _exportUpdateVisibility; }
            set
            {
                _exportUpdateVisibility = value;
                RaisePropertyChanged("ExportUpdateVisibility");
            }
        }


        public bool GSTActive
        {
            get { return _gSTActive; }

            set
            {
                _gSTActive = value;
                base.RaisePropertyChanged("GSTActive");
            }
        }

        public bool ExportToMyOBEnabled
        {
            get { return _exportToMyOBEnabled; }

            set
            {
                _exportToMyOBEnabled = value;
                base.RaisePropertyChanged("ExportToMyOBEnabled");
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

        public DateTime SelectedInvoiceDate
        {
            get
            {
                return _selectedInvoiceDate;
            }
            set
            {
                _selectedInvoiceDate = value;
                RaisePropertyChanged("SelectedInvoiceDate");
            }
        }

        public bool InvoiceDateEnabled
        {
            get
            {
                return _invoiceDateEnabled;
            }
            set
            {
                _invoiceDateEnabled = value;
                RaisePropertyChanged("InvoiceDateEnabled");
            }
        }

        //public ObservableCollection<FreightCode> FreightCodeDetails
        //{
        //    get
        //    {
        //        return _freightCodeDetails;
        //    }
        //    set
        //    {
        //        _freightCodeDetails = value;
        //        RaisePropertyChanged("FreightCodeDetails");
        //    }
        //}


        public bool FreightGridEnbaled
        {
            get
            {
                return _freightGridEnbaled;
            }
            set
            {
                _freightGridEnbaled = value;
                RaisePropertyChanged("FreightGridEnbaled");
            }
        }
        

        //public bool ExportedToMyOb
        //{
        //    get { return _exportedToMyOb; }
        //    set
        //    {
        //        _exportedToMyOb = value;
        //        RaisePropertyChanged(() => this.ExportedToMyOb);
        //    }
        //}

        #region COMMANDS

        public ICommand CreateInvoiceCommand
        {
            get
            {
                return _createInvoiceCommand ?? (_createInvoiceCommand = new CommandHandler(() => CreateInvoice(), execute));
            }
        }

        public ICommand ExportToMyObCommand
        {
            get
            {
                return _exportToMyObCommand ?? (_exportToMyObCommand = new CommandHandler(() => UpdateExportMyOb(), execute));
            }
        }


        public Microsoft.Practices.Prism.Commands.DelegateCommand CloseCommand
        {
            get { return _closeCommand; }
        }

        public ICommand FreightPriceKeyUpCommand
        {
            get
            {
                return _freightPriceKeyUpCommand ?? (_freightPriceKeyUpCommand = new CommandHandler(() => CalculateFreightTotal(), true));
            }
        }

        public ICommand LostFocusCommand
        {
            get
            {
                return _lostFocusCommand ?? (_lostFocusCommand = new CommandHandler(() => CalculateFinalTotal(), true));
            }
        }
        

        #endregion
    }



}
