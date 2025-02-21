using A1RConsole.Bases;
using A1RConsole.Commands;
using A1RConsole.DB;
using A1RConsole.Interfaces;
using A1RConsole.Models.Invoices;
using A1RConsole.Models.Purchasing;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Office.Interop.Excel;
using System.Data;
using System.Windows.Media;
using System.Windows.Documents;
using System.Globalization;
using System.IO;
using A1RConsole.Core;
using System.ComponentModel;

namespace A1RConsole.ViewModels.Invoicing
{
    public class OrderHasNotLeftViewModel : ViewModelBase, IContent
    {
        private bool execute;
        public DateTime _currentDate;
        public DateTime _selectedDate;
        public string _selectedState;
        public decimal _total;
        private ObservableCollection<OrdersNotInvoiced> _ordersNotInvoicedList;
        private Microsoft.Office.Interop.Excel.Application excel;
        private Microsoft.Office.Interop.Excel.Workbook worKbooK;
        private Microsoft.Office.Interop.Excel.Worksheet worksheet;
        private bool IsDirty { get; set; }
        public event EventHandler Closing;
        private Microsoft.Practices.Prism.Commands.DelegateCommand _closeCommand;
        private ICommand _viewReportCommand, _generateDocumentCommand;

        public OrderHasNotLeftViewModel()
        {            
            execute = true;
            SelectedState = "QLD";
            //CurrentDate = DateTime.Now;
            SelectedDate = DateTime.Now;
            Total = 0;
            _closeCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(CloseForm);

            
        }

        public void ViewReport()
        {
            OrdersNotInvoicedList = DBAccess.GetListOfOrdersCompletedNotInvoiced(SelectedState);
            //Calculate total
            Total = 0;
            if(OrdersNotInvoicedList != null && OrdersNotInvoicedList.Count > 0)
            {
                Total = OrdersNotInvoicedList.Sum(x=>x.Total);
            }
            else
            {
                MessageBox.Show("No information found", "", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        public void GenerateDocument()
        {
            if (OrdersNotInvoicedList != null && OrdersNotInvoicedList.Count > 0)
            {

                BackgroundWorker worker = new BackgroundWorker();
                ChildWindow LoadingScreen = new ChildWindow();

                LoadingScreen.ShowWaitingScreen("Working..");

                worker.DoWork += (_, __) =>
                {
                    excel = new Microsoft.Office.Interop.Excel.Application();
                    excel.Visible = false;
                    excel.DisplayAlerts = false;
                    worKbooK = excel.Workbooks.Add(Type.Missing);
                    worksheet = (Microsoft.Office.Interop.Excel.Worksheet)worKbooK.ActiveSheet;
                    worksheet.Name = "InventoryShippedNotInvoiced";
                    worksheet.Range[worksheet.Cells[1, 1], worksheet.Cells[1, 8]].Merge(); 

                    try
                    {
                        excel = new Microsoft.Office.Interop.Excel.Application();
                        excel.Visible = false;
                        excel.DisplayAlerts = false;
                        worKbooK = excel.Workbooks.Add(Type.Missing);


                        worksheet = (Microsoft.Office.Interop.Excel.Worksheet)worKbooK.ActiveSheet;
                        worksheet.Name = SelectedState;

                        worksheet.Range[worksheet.Cells[1, 1], worksheet.Cells[1, 11]].Merge();
                        worksheet.Cells[1, 1] = "Inventory Shipped Not Invoiced - " + SelectedState + " [" + DateTime.Now.ToString("dd-MM-yyyy hh:mm tt") + "]";
                        worksheet.Cells[1, 1].Font.Bold = true;
                        worksheet.Cells.Font.Size = 12;

                        worksheet.Rows[3].Font.Bold = true;
                        worksheet.Cells[3, 1] = "Shipper ID";                        
                        worksheet.Cells[3, 2] = "Order ID";
                        worksheet.Cells[3, 3] = "Order Date";
                        worksheet.Cells[3, 4] = "Shipped Date";
                        worksheet.Cells[3, 5] = "Name";
                        worksheet.Cells[3, 6] = "Part ID";
                        worksheet.Cells[3, 7] = "Description";
                        worksheet.Cells[3, 8] = "Order QTY";
                        worksheet.Cells[3, 9] = "Unit";
                        worksheet.Cells[3, 10] = "Unit Cost";
                        worksheet.Cells[3, 11] = "Total";

                        int row = 4;

                        foreach (var item in OrdersNotInvoicedList)
                        {
                            worksheet.Cells[row, 1] = item.ShipperID;
                            worksheet.Cells[row, 2] = item.SalesID;
                            worksheet.Cells[row, 3] = item.OrderDate;
                            worksheet.Cells[row, 4] = item.ShippedDate;
                            worksheet.Cells[row, 5] = item.Name;
                            worksheet.Cells[row, 6] = item.PartID;
                            worksheet.Cells[row, 7] = item.Description;
                            worksheet.Cells[row, 8] = item.OrderQty;
                            worksheet.Cells[row, 9] = item.Unit;
                            worksheet.Cells[row, 10] = item.UnitCost;
                            worksheet.Cells[row, 11] = item.Total;
                        
                            row++;
                        }                  

                        row += 2;

                        worksheet.Cells[row, 9] = "TOTAL";
                        worksheet.Cells[row, 9].Font.Bold = true;
                        worksheet.Range[worksheet.Cells[row, 9], worksheet.Cells[row, 10]].Merge();
                        worksheet.Cells[row, 11] = Total.ToString("C", CultureInfo.CurrentCulture); 
                        worksheet.Cells[row, 11].Font.Bold = true;
                    
                        worksheet.Range["A3"].RowHeight = 30;
                        worksheet.Range["A3"].ColumnWidth = 60;
                        worksheet.Columns.AutoFit();

                        string fileName = "NotInvoiced-" + SelectedState + "-" + DateTime.Now.ToString("dd-MM-yyyy-HH.mm.ss");
                    
                        //worKbooK.SaveAs("I:\\Production\\DONOTDELETE\\NotInvoiced\\" + fileName + ".xlsx");
                        //worKbooK.Close();
                        //excel.Quit();
                   
                        excel.Visible = true;

                        //if (Directory.Exists("I:\\Production\\DONOTDELETE\\NotInvoiced"))
                        //{
                        //    FileInfo fi = new FileInfo("I:\\Production\\DONOTDELETE\\NotInvoiced\\" + fileName + ".xlsx");
                        //    if (fi.Exists)
                        //    {
                        //        System.Diagnostics.Process.Start(@"I:\Production\DONOTDELETE\NotInvoiced\\" + fileName + ".xlsx");
                        //    }
                        //    else
                        //    {
                        //        //file doesn't exist
                        //        MessageBox.Show("File doesn't exist", "", MessageBoxButton.OK, MessageBoxImage.Information);
                        //    }
                        //}
                        //else
                        //{
                        //    MessageBox.Show("Please check if the following folder is accessible" + System.Environment.NewLine 
                        //        + "I:\\Production\\DONOTDELETE\\NotInvoiced", "", MessageBoxButton.OK, MessageBoxImage.Information);
                        //}
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

                };
                worker.RunWorkerCompleted += delegate (object s, RunWorkerCompletedEventArgs args)
                {
                    LoadingScreen.CloseWaitingScreen();

                };
                worker.RunWorkerAsync();
            }
            else
            {
                MessageBox.Show("No information found", "", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }       

        public string Title
        {
            get
            {
                return "Inventory Shipped Not Invoiced";
            }
        }

        private void CloseForm()
        {
            CloseWindow(this);
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

        public bool CanClose
        {
            get
            {
                return this.IsDirty == false || MessageBox.Show("Changes were made. Do you want to close this window?", "Close", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
            }
        }

        public decimal Total
        {
            get
            {
                return _total;
            }
            set
            {
                _total = value;
                RaisePropertyChanged("Total");
            }
        }

        public ObservableCollection<OrdersNotInvoiced> OrdersNotInvoicedList
        {
            get
            {
                return _ordersNotInvoicedList;
            }
            set
            {
                _ordersNotInvoicedList = value;
                RaisePropertyChanged("OrdersNotInvoicedList");
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

        public DateTime SelectedDate
        {
            get
            {
                return _selectedDate;
            }
            set
            {
                _selectedDate = value;
                RaisePropertyChanged("SelectedDate");
            }
        }

        public string SelectedState
        {
            get
            {
                return _selectedState;
            }
            set
            {
                _selectedState = value;
                RaisePropertyChanged("SelectedState");
            }
        }

        public Microsoft.Practices.Prism.Commands.DelegateCommand CloseCommand
        {
            get { return _closeCommand; }
        }

        public ICommand ViewReportCommand
        {
            get
            {
                return _viewReportCommand ?? (_viewReportCommand = new CommandHandler(() => ViewReport(), execute));
            }
        }
        public ICommand GenerateDocumentCommand
        {
            get
            {
                return _generateDocumentCommand ?? (_generateDocumentCommand = new CommandHandler(() => GenerateDocument(), execute));
            }
        }
        
    }
}
