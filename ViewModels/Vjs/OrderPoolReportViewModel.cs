using A1RConsole.Bases;
using A1RConsole.Commands;
using A1RConsole.Core;
using A1RConsole.DB;
using A1RConsole.Interfaces;
using A1RConsole.Models.Invoices;
using A1RConsole.Models.Vjs;
using Microsoft.Office.Interop.Excel;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace A1RConsole.ViewModels.Vjs
{

    public class OrderPoolReportViewModel : ViewModelBase, IContent
    {
        private DateTime _endDate;
        private DateTime _selectedEndDate;
        private Microsoft.Office.Interop.Excel.Application excel;
        private Microsoft.Office.Interop.Excel.Workbook worKbooK;
        private Microsoft.Office.Interop.Excel.Worksheet worksheet;
        private ObservableCollection<VjsOrder> _vjsOrders;
        private decimal _total;
        private string _selectedState;
        private bool IsDirty { get; set; }
        public event EventHandler Closing;

        private ICommand _viewReportCommand;
        private ICommand _generateDocumentCommand;
        public OrderPoolReportViewModel()
        {
            SelectedState = "All";
            Total = 0;
            EndDate = DateTime.Now;
            SelectedEndDate = DateTime.Now;
        }

        private void LoadDataAndGenerateDoc(bool generateDoc)
        {
            BackgroundWorker worker = new BackgroundWorker();
            ChildWindow LoadingScreen = new ChildWindow();

            LoadingScreen.ShowWaitingScreen("Working..");

            worker.DoWork += (_, __) =>
            {
                VjsOrders = new ObservableCollection<VjsOrder>();
                VjsOrders = DBAccess.GetCurrentVjsOrders(SelectedState, SelectedEndDate);
                Total = VjsOrders.Sum(x => x.VjsOrderDetails[0].OrderAmount);
                if (generateDoc && VjsOrders != null && VjsOrders.Count > 0)
                {
                    excel = new Microsoft.Office.Interop.Excel.Application();
                    excel.Visible = false;
                    excel.DisplayAlerts = false;
                    worKbooK = excel.Workbooks.Add(Type.Missing);
                    worksheet = (Microsoft.Office.Interop.Excel.Worksheet)worKbooK.ActiveSheet;
                    worksheet.Name = "CurrentOrders";
                    worksheet.Range[worksheet.Cells[1, 1], worksheet.Cells[1, 8]].Merge();

                    try
                    {
                        excel = new Microsoft.Office.Interop.Excel.Application();
                        excel.Visible = false;
                        excel.DisplayAlerts = false;
                        worKbooK = excel.Workbooks.Add(Type.Missing);

                        worksheet = (Microsoft.Office.Interop.Excel.Worksheet)worKbooK.ActiveSheet;
                        worksheet.Name = SelectedState;

                        //worksheet.Range[worksheet.Cells[1, 1], worksheet.Cells[1, 11]].Merge();
                        worksheet.Cells[1, 1] = "Total ";
                        worksheet.Cells[1, 2] = Total.ToString();
                        worksheet.Cells.Font.Size = 12;

                        worksheet.Rows[3].Font.Bold = true;
                        worksheet.Cells[3, 1] = "Order ID";
                        worksheet.Cells[3, 2] = "Order Date";
                        worksheet.Cells[3, 3] = "Desired Shipping Date";
                        worksheet.Cells[3, 4] = "Order Status";
                        worksheet.Cells[3, 5] = "Name";
                        worksheet.Cells[3, 6] = "Line No";
                        worksheet.Cells[3, 7] = "Part ID";
                        worksheet.Cells[3, 8] = "Description";
                        worksheet.Cells[3, 9] = "Order QTY";
                        worksheet.Cells[3, 10] = "Unit";
                        worksheet.Cells[3, 11] = "Unit Price";
                        worksheet.Cells[3, 12] = "Discount";
                        worksheet.Cells[3, 13] = "Order Amount";
                        worksheet.Cells[3, 14] = "Notes";

                        int row = 4;

                        foreach (var item in VjsOrders)
                        {
                            worksheet.Cells[row, 1] = item.OrderID;
                            worksheet.Cells[row, 2] = item.OrderDate.Date;
                            worksheet.Cells[row, 3] = item.DesiredShippingDate.Date;
                            worksheet.Cells[row, 4] = item.OrderStatus;
                            worksheet.Cells[row, 5] = item.CustomerName;
                            worksheet.Cells[row, 6] = item.VjsOrderDetails[0].LineNo;
                            worksheet.Cells[row, 7] = item.VjsOrderDetails[0].PartID;
                            worksheet.Cells[row, 8] = item.VjsOrderDetails[0].Description;
                            worksheet.Cells[row, 9] = item.VjsOrderDetails[0].OrderQty;
                            worksheet.Cells[row, 10] = item.VjsOrderDetails[0].Unit;
                            worksheet.Cells[row, 11] = item.VjsOrderDetails[0].UnitPrice;
                            worksheet.Cells[row, 12] = item.VjsOrderDetails[0].Discount;
                            worksheet.Cells[row, 13] = item.VjsOrderDetails[0].OrderAmount;
                            worksheet.Cells[row, 14] = item.Notes;

                            row++;
                        }

                        row += 2;

                        //worksheet.Cells[row, 9] = "TOTAL";
                        //worksheet.Cells[row, 9].Font.Bold = true;
                        //worksheet.Range[worksheet.Cells[row, 9], worksheet.Cells[row, 10]].Merge();
                        //worksheet.Cells[row, 11] = Total.ToString("C", CultureInfo.CurrentCulture);
                        //worksheet.Cells[row, 11].Font.Bold = true;

                        worksheet.Range["A3"].RowHeight = 30;
                        worksheet.Range["A3"].ColumnWidth = 60;

                        
                        worksheet.Columns.AutoFit();
                        worksheet.Columns[14].ColumnWidth = 70;
            
                       
                        string fileName = "CurrentOrders-" + SelectedState + "-" + DateTime.Now.ToString("dd-MM-yyyy-HH.mm.ss");

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
            };
            worker.RunWorkerCompleted += delegate (object s, RunWorkerCompletedEventArgs args)
            {
                LoadingScreen.CloseWaitingScreen();
                if (VjsOrders == null || VjsOrders.Count == 0)
                {
                    MessageBox.Show("No information found", "", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            };
            worker.RunWorkerAsync();

            Total = 0;

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

        public bool CanClose
        {
            get
            {
                return this.IsDirty == false || MessageBox.Show("Changes were made. Do you want to close this window?", "Close", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
            }
        }

        public string Title
        {
            get
            {
                return "Order Pool Report";
            }
        }


        public DateTime SelectedEndDate
        {
            get
            {
                return _selectedEndDate;
            }
            set
            {
                _selectedEndDate = value;
                RaisePropertyChanged("SelectedEndDate");
            }
        }

        public DateTime EndDate
        {
            get
            {
                return _endDate;
            }
            set
            {
                _endDate = value;
                RaisePropertyChanged("EndDate");
            }
        }
        public ObservableCollection<VjsOrder> VjsOrders
        {
            get
            {
                return _vjsOrders;
            }
            set
            {
                _vjsOrders = value;
                RaisePropertyChanged("VjsOrders");
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

        public ICommand ViewReportCommand
        {
            get
            {
                return _viewReportCommand ?? (_viewReportCommand = new CommandHandler(() => LoadDataAndGenerateDoc(false), true));
            }
        }

        public ICommand GenerateDocumentCommand
        {
            get
            {
                return _generateDocumentCommand ?? (_generateDocumentCommand = new CommandHandler(() => LoadDataAndGenerateDoc(true), true));
            }
        }
    }
}
