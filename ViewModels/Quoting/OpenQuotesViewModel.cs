using A1RConsole.Bases;
using A1RConsole.Commands;
using A1RConsole.Core;
using A1RConsole.DB;
using A1RConsole.Interfaces;
using A1RConsole.Models.Customers;
using A1RConsole.Models.Discounts;
using A1RConsole.Models.Freights;
using A1RConsole.Models.Quoting;
using A1RConsole.PdfGeneration;
using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace A1RConsole.ViewModels.Quoting
{
    public class OpenQuotesViewModel : ViewModelBase, IContent
    {
        private string _noOfQuoteStr;
        private Quote Quote;
        private ObservableCollection<OpenQuotes> _openQuotes;
        MainWindowViewModel mainWindowViewModelRefernce;
        private bool IsDirty { get; set; }
        public event EventHandler Closing;
        private ICommand _viewUpdateCommand;
        private ICommand _viewDataCommand;
        private ICommand _exportToPDFCommand;
        private ICommand _exportToExcelCommand;

        public OpenQuotesViewModel()
        {
            
        }
              

        public void ExportToPDF()
        {
            LoadOpenQuotes();

            if (OpenQuotes != null)
            {
                Tuple<Exception,string> result = null;
                BackgroundWorker worker = new BackgroundWorker();
                ChildWindow LoadingScreen = new ChildWindow();
                LoadingScreen.ShowWaitingScreen("Working..");

                worker.DoWork += (_, __) =>
                {
                    OpenQuotesPDF oq = new OpenQuotesPDF(OpenQuotes);
                    result = oq.CreatePDF();
                };

                worker.RunWorkerCompleted += delegate (object s, RunWorkerCompletedEventArgs args)
                {
                    LoadingScreen.CloseWaitingScreen();

                    if (result.Item1 != null)
                    {
                        MessageBox.Show("A problem has occured while creating the pdf. Please try again later." + System.Environment.NewLine + result.Item1, "Printing Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.Yes);
                    }
                    else
                    {
                        var childWindow = new ChildWindow();
                        childWindow.ShowFormula(result.Item2);
                    }
                };
                worker.RunWorkerAsync();                
            }
            else
            {
                MessageBox.Show("No open quotes found", "", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        public void ExportToExcel()
        {
            if(OpenQuotes != null && OpenQuotes.Count > 0)
            {
                Microsoft.Office.Interop.Excel.Application excel = new Microsoft.Office.Interop.Excel.Application();
                excel.Visible = false;
                excel.DisplayAlerts = false;
                Microsoft.Office.Interop.Excel.Workbook worKbooK = excel.Workbooks.Add(Type.Missing);
                Microsoft.Office.Interop.Excel.Worksheet worksheet = (Microsoft.Office.Interop.Excel.Worksheet)worKbooK.ActiveSheet;
                worksheet.Name = "OpenQuotes";
                worksheet.Range[worksheet.Cells[1, 1], worksheet.Cells[1, 8]].Merge();

                try
                {
                    excel = new Microsoft.Office.Interop.Excel.Application();
                    excel.Visible = false;
                    excel.DisplayAlerts = false;
                    worKbooK = excel.Workbooks.Add(Type.Missing);


                    worksheet = (Microsoft.Office.Interop.Excel.Worksheet)worKbooK.ActiveSheet;
                    worksheet.Name = "Open Quotes";

                    worksheet.Range[worksheet.Cells[1, 1], worksheet.Cells[1, 11]].Merge();
                    worksheet.Cells[1, 1] = "Open Quotes - " + " [" + DateTime.Now.ToString("dd-MM-yyyy hh:mm tt") + "]";
                    worksheet.Cells[1, 1].Font.Bold = true;
                    worksheet.Cells.Font.Size = 12;

                    worksheet.Rows[3].Font.Bold = true;
                    worksheet.Cells[3, 1] = "Quote No";
                    worksheet.Cells[3, 2] = "Quote Date";
                    worksheet.Cells[3, 3] = "Customer";
                    worksheet.Cells[3, 4] = "Project Name";
                    worksheet.Cells[3, 5] = "Product Details";
                    worksheet.Cells[3, 6] = "Total";
                    worksheet.Cells[3, 7] = "Contact";
                    worksheet.Cells[3, 8] = "Quoted By";

                    int row = 4;
                    decimal total = 0;

                    foreach (var item in OpenQuotes)
                    {
                        worksheet.Cells[row, 1] = item.QuoteNo;
                        worksheet.Cells[row, 2] = item.QuoteDate.ToString("dd/MM/yyyy");
                        worksheet.Cells[row, 3] = item.Customer.CompanyName;
                        worksheet.Cells[row, 4] = item.ProjectName;
                        worksheet.Cells[row, 5] = item.ProductDetails;
                        worksheet.Cells[row, 6] = Convert.ToDecimal(item.TotalAmount).ToString("C", CultureInfo.CurrentCulture);
                        worksheet.Cells[row, 7] = item.ContactPerson.ContactPersonName;
                        worksheet.Cells[row, 8] = item.User.FullName;

                        total += item.TotalAmount;
                        row++;
                    }

                    row += 1;

                    worksheet.Cells[row, 5] = "TOTAL";
                    worksheet.Cells[row, 5].Font.Bold = true;
                    worksheet.Cells[row, 6] = total.ToString("C", CultureInfo.CurrentCulture);
                    worksheet.Cells[row, 6].Font.Bold = true;

                    worksheet.Range["A3"].RowHeight = 30;
                    worksheet.Range["A3"].ColumnWidth = 100;
                    worksheet.Range["E3"].ColumnWidth = 100;
                    worksheet.Range["E3"].RowHeight = 30;
                    worksheet.Range["G3"].ColumnWidth = 200;
                    worksheet.Columns.AutoFit();

                    string fileName = "OpenQuotes-"  + DateTime.Now.ToString("dd-MM-yyyy-HH.mm.ss");

                    excel.Visible = true;                    
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);

                }
                finally
                {
                    worksheet = null;
                    worKbooK = null;
                }
            }        
            else
            {
                MessageBox.Show("No information found", "", MessageBoxButton.OK, MessageBoxImage.Information);
            }
}

        public void LoadOpenQuotes()
        {
            BackgroundWorker worker = new BackgroundWorker();
            ChildWindow LoadingScreen = new ChildWindow();
            LoadingScreen.ShowWaitingScreen("Working..");

            worker.DoWork += (_, __) =>
            {
                NoOfQuoteStr = string.Empty;
                OpenQuotes = DBAccess.GetAllOpenQuotes(DateTime.Now);
                if(OpenQuotes != null)
                {
                    NoOfQuoteStr = OpenQuotes.Select(x=>x.QuoteNo).Count().ToString() + " open quotes from " +DateTime.Now.AddMonths(-1).Date.ToString("dd/MM/yyyy") + " to " + DateTime.Now.Date.ToString("dd/MM/yyyy") + System.Environment.NewLine + "Total Value : " + OpenQuotes.Sum(x => x.TotalAmount).ToString("C", CultureInfo.CurrentCulture);
                }

            };

            worker.RunWorkerCompleted += delegate (object s, RunWorkerCompletedEventArgs args)
            {
                LoadingScreen.CloseWaitingScreen();
            };
            worker.RunWorkerAsync();
        }

        private void SearchQuote(int quoteNo)
        {           

            if (Quote.QuoteNo == 0)
            {
                MessageBox.Show("Enter quote no/ Project name/ Customer name and search at the top to retrieve quote", "Quote Required", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                Quote = DBAccess.GetQuote(quoteNo);

                //Copying without reference
                Quote q = new Quote();
                q.QuoteNo = Quote.QuoteNo;
                //q.Customer = Quote.Customer;
                q.QuoteDetails = new ObservableCollection<QuoteDetails>();
                q.ContactPerson = new ContactPerson();
                q.ContactPerson.Active = Quote.ContactPerson.Active;
                q.ContactPerson.ContactPersonID = Quote.ContactPerson.ContactPersonID;
                q.ContactPerson.ContactPersonName = Quote.ContactPerson.ContactPersonName;
                q.ContactPerson.CustomerID = Quote.ContactPerson.CustomerID;
                q.ContactPerson.Email = Quote.ContactPerson.Email;
                q.ContactPerson.PhoneNumber1 = Quote.ContactPerson.PhoneNumber1;
                q.ContactPerson.PhoneNumber2 = Quote.ContactPerson.PhoneNumber2;
                q.ContactPerson.TimeStamp = Quote.ContactPerson.TimeStamp;

                q.FreightDetails = new BindingList<FreightDetails>();
                q.InternalComments = Quote.InternalComments;
                q.Instructions = Quote.Instructions;
                q.GSTActive = Quote.GSTActive;

                q.Customer = new Customer();
                q.Customer.Active = Quote.Customer.Active;
                q.Customer.CompanyAddress = Quote.Customer.CompanyAddress;
                q.Customer.CompanyCity = Quote.Customer.CompanyCity;
                q.Customer.CompanyCountry = Quote.Customer.CompanyCountry;
                q.Customer.CompanyEmail = Quote.Customer.CompanyEmail;
                q.Customer.CompanyFax = Quote.Customer.CompanyFax;
                q.Customer.CompanyName = Quote.Customer.CompanyName;
                q.Customer.CompanyPostCode = Quote.Customer.CompanyPostCode;
                q.Customer.CompanyState = Quote.Customer.CompanyState;
                q.Customer.CompanyTelephone = Quote.Customer.CompanyTelephone;
                q.Customer.ContactPerson = Quote.Customer.ContactPerson;
                q.Customer.CreditLimit = Quote.Customer.CreditLimit;
                q.Customer.CreditOwed = Quote.Customer.CreditOwed;
                q.Customer.CreditRemaining = Quote.Customer.CreditRemaining;
                q.Customer.CustomerId = Quote.Customer.CustomerId;
                q.Customer.CustomerType = Quote.Customer.CustomerType;
                q.Customer.PrimaryBusiness = Quote.Customer.PrimaryBusiness;
                q.Customer.ShipAddress = Quote.Customer.ShipAddress;
                q.Customer.ShipCity = Quote.Customer.ShipCity;
                q.Customer.ShipCountry = Quote.Customer.ShipCountry;
                q.Customer.ShipPostCode = Quote.Customer.ShipPostCode;
                q.Customer.ShipState = Quote.Customer.ShipState;
                q.Customer.TimeStamp = Quote.Customer.TimeStamp;
                q.ProjectName = Quote.ProjectName;
                q.QuoteCourierName = Quote.QuoteCourierName;

                var clonedQuoteDetails = Quote.QuoteDetails.Select(objEntity => (QuoteDetails)objEntity.Clone()).ToList();
                var clonedFreightDetails = Quote.FreightDetails.Select(objEntity => (FreightDetails)objEntity.Clone()).ToList();

                q.QuoteDetails = new ObservableCollection<QuoteDetails>(clonedQuoteDetails);
                q.FreightDetails = new BindingList<FreightDetails>(clonedFreightDetails);

                //TabSelectedIndex = 0;

                mainWindowViewModelRefernce = MainWindowViewModel.instance;
                if (mainWindowViewModelRefernce != null)
                {
                    List<ContactPerson> cpList = DBAccess.GetContactPersonByCustomerID(q.Customer.CustomerId);
                    if (cpList != null)
                    {
                        q.Customer.ContactPerson = cpList;
                    }
                    mainWindowViewModelRefernce.NewQuote(q);

                }
            }

        }

        public string Title
        {
            get
            {
                return "Open Quotes";
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
                var handler = this.Closing;
                if (handler != null)
                {
                    handler(this, EventArgs.Empty);
                }
            }
        }

        private bool CanExecute(object parameter)
        {
            return true;
        }

        private void ViewUpdateQuote(Object parameter)
        {
            int index = OpenQuotes.IndexOf(parameter as OpenQuotes);
            if (index >= 0)
            {
                bool exist = DBAccess.CheckRecordExist(OpenQuotes[index].QuoteNo, "Quote");
                if (exist)
                {
                    Quote = new Quote();
                    Quote.QuoteNo = OpenQuotes[index].QuoteNo;
                    SearchQuote(OpenQuotes[index].QuoteNo);
                }
                else
                {
                    MessageBox.Show("This quote does not exist", "", MessageBoxButton.OK, MessageBoxImage.Information);
                    //RefreshGrid();
                }
            }
        }

        

        public string NoOfQuoteStr
        {
            get
            {
                return _noOfQuoteStr;
            }
            set
            {
                _noOfQuoteStr = value;
                RaisePropertyChanged("NoOfQuoteStr");
            }
        }
        public ObservableCollection<OpenQuotes> OpenQuotes
        {
            get
            {
                return _openQuotes;
            }
            set
            {
                _openQuotes = value;
                RaisePropertyChanged("OpenQuotes");
            }
        }

        public ICommand ViewUpdateCommand
        {
            get
            {
                if (_viewUpdateCommand == null)
                {
                    _viewUpdateCommand = new DelegateCommand(CanExecute, ViewUpdateQuote);
                }
                return _viewUpdateCommand;
            }
        }

        public ICommand ViewDataCommand
        {
            get
            {
                return _viewDataCommand ?? (_viewDataCommand = new CommandHandler(() => LoadOpenQuotes(), true));
            }
        }

        public ICommand ExportToPDFCommand
        {
            get
            {
                return _exportToPDFCommand ?? (_exportToPDFCommand = new CommandHandler(() => ExportToPDF(), true));
            }
        }

        public ICommand ExportToExcelCommand
        {
            get
            {
                return _exportToExcelCommand ?? (_exportToExcelCommand = new CommandHandler(() => ExportToExcel(), true));
            }
        }
        

    }
}
