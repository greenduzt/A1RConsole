using A1RConsole.Bases;
using A1RConsole.Commands;
using A1RConsole.Core;
using A1RConsole.DB;
using A1RConsole.Interfaces;
using A1RConsole.Models.Invoices;
using A1RConsole.Models.Orders;
using A1RConsole.Models.Quoting;
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
using System.Windows.Data;
using System.Windows.Input;

namespace A1RConsole.ViewModels.Orders
{
    public class OrdersReportViewModel : ViewModelBase, IContent
    {
        private ListCollectionView _collDay = null;
        private ObservableCollection<OrdersNotInvoiced> _orders;
        private DateTime _selectedDate;
        private DateTime _endDate;
        private string _totalsStr;
        private string totalsStr1;
        private string totalsStr2;
        private string totalsStr3;
        private string totalsStr4;
        private bool IsDirty { get; set; }
        public event EventHandler Closing;
        private ICommand _viewDataCommand;
        private ICommand _exportToExcelCommand;

        public OrdersReportViewModel()
        {
            Orders = new ObservableCollection<OrdersNotInvoiced>();
            SelectedDate = DateTime.Now;
            EndDate = DateTime.Now;
            TotalsStr = string.Empty;
        }

        public string Title
        {
            get
            {
                return "Daily Order Report";
            }
        }

        private void ViewData(bool exportToExcel)
        {
            BackgroundWorker worker = new BackgroundWorker();
            ChildWindow LoadingScreen = new ChildWindow();

            LoadingScreen.ShowWaitingScreen("Working..");

            worker.DoWork += (_, __) =>
            {
                decimal tot1 = 0, tot2 = 0, tot3 = 0, tot4 = 0, tot5 = 0, tot6 = 0;
                int c1 = 0, c2 = 0, c3 = 0, c4 = 0, c5 = 0, c6 = 0;
                TotalsStr = string.Empty;
                Orders =  DBAccess.GetAllOrdersByDateState(SelectedDate);
                if (Orders != null && Orders.Count > 0)
                {
                    foreach (var item in Orders)
                    {
                        if (item.State.Equals("QLD"))
                        {
                            tot1 += item.Total;
                            c1++;
                        }
                        else if (item.State.Equals("NSW"))
                        {
                            tot2 += item.Total;
                            c2++;
                        }
                        else if (item.State.Equals("Sandleford Holdings Pty Ltd (QLD)"))
                        {
                            tot3 += item.Total;
                            c3++;
                        }
                        else if (item.State.Equals("Sandleford Holdings Pty Ltd (NSW)"))
                        {
                            tot4 += item.Total;
                            c4++;
                        }
                        else if (item.State.Equals("VIC"))
                        {
                            tot5 += item.Total;
                            c5++;
                        }
                        else if (item.State.Equals("Sandleford Holdings Pty Ltd (VIC)"))
                        {
                            tot6 += item.Total;
                            c6++;
                        }
                    }

                    foreach (var item in Orders)
                    {
                        if (item.State.Equals("QLD"))
                        {
                            item.TotalByState += "Total Value : " + Convert.ToDecimal(tot1).ToString("C", CultureInfo.CurrentCulture);
                            item.TotalItems += "Total Items : " + c1;
                        }
                        else if (item.State.Equals("NSW"))
                        {
                            item.TotalByState += "Total Value : " + Convert.ToDecimal(tot2).ToString("C", CultureInfo.CurrentCulture);
                            item.TotalItems += "Total Items : " + c2;
                        }
                        else if (item.State.Equals("Sandleford Holdings Pty Ltd (QLD)"))
                        {
                            item.TotalByState += "Total Value : " + Convert.ToDecimal(tot3).ToString("C", CultureInfo.CurrentCulture);
                            item.TotalItems += "Total Items : " + c3;
                        }
                        else if (item.State.Equals("Sandleford Holdings Pty Ltd (NSW)"))
                        {
                            item.TotalByState += "Total Value : " + Convert.ToDecimal(tot4).ToString("C", CultureInfo.CurrentCulture);
                            item.TotalItems += "Total Items : " + c4;
                        }
                        else if (item.State.Equals("VIC"))
                        {
                            item.TotalByState += "Total Value : " + Convert.ToDecimal(tot5).ToString("C", CultureInfo.CurrentCulture);
                            item.TotalItems += "Total Items : " + c5;
                        }
                        else if (item.State.Equals("Sandleford Holdings Pty Ltd (VIC)"))
                        {
                            item.TotalByState += "Total Value : " + Convert.ToDecimal(tot6).ToString("C", CultureInfo.CurrentCulture);
                            item.TotalItems += "Total Items : " + c6;
                        }
                    }
                }

                totalsStr1 = "QLD & Sandleford Holdings Pty Ltd (QLD) Total : " + Convert.ToDecimal(tot1 + tot3).ToString("C", CultureInfo.CurrentCulture);
                totalsStr2 = "NSW & Sandleford Holdings Pty Ltd (NSW) Total : " + Convert.ToDecimal(tot2 + tot4).ToString("C", CultureInfo.CurrentCulture);
                totalsStr4 = "VIC & Sandleford Holdings Pty Ltd (VIC) Total : " + Convert.ToDecimal(tot5 + tot6).ToString("C", CultureInfo.CurrentCulture);
                totalsStr3 = "Total Value : " + Convert.ToDecimal(tot1 + tot2 + tot3 + tot4 + tot5 + tot6).ToString("C", CultureInfo.CurrentCulture);
                TotalsStr = totalsStr1 + System.Environment.NewLine + totalsStr2 + System.Environment.NewLine + totalsStr4 + System.Environment.NewLine + totalsStr3;

                CollDay = new ListCollectionView(Orders);
                CollDay.GroupDescriptions.Add(new PropertyGroupDescription("State"));

            };
            worker.RunWorkerCompleted += delegate (object s, RunWorkerCompletedEventArgs args)
            {
                LoadingScreen.CloseWaitingScreen();
                if (Orders != null && Orders.Count > 0)
                {
                    if (exportToExcel)
                    {
                        ExportToExcel();
                    }
                }
                else
                {
                    MessageBox.Show("No information found", "", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            };
            worker.RunWorkerAsync();
        }

        public void ExportToExcel()
        {           

            if (Orders != null && Orders.Count > 0)
            {

                BackgroundWorker worker = new BackgroundWorker();
                ChildWindow LoadingScreen = new ChildWindow();

                LoadingScreen.ShowWaitingScreen("Working..");

                worker.DoWork += (_, __) =>
                {

                    Microsoft.Office.Interop.Excel.Application excel = new Microsoft.Office.Interop.Excel.Application();
                    excel.Visible = false;
                    excel.DisplayAlerts = false;

                    Microsoft.Office.Interop.Excel.Workbook worKbooK = excel.Workbooks.Add(Type.Missing);
                    Microsoft.Office.Interop.Excel.Worksheet worksheet = (Microsoft.Office.Interop.Excel.Worksheet)worKbooK.ActiveSheet;
                
                    worksheet.Name = "Orders";
                    worksheet.Range[worksheet.Cells[1, 1], worksheet.Cells[1, 8]].Merge();

                    try
                    {
                        excel = new Microsoft.Office.Interop.Excel.Application();
                        excel.Visible = false;
                        excel.DisplayAlerts = false;
                        worKbooK = excel.Workbooks.Add(Type.Missing);

                        worksheet = (Microsoft.Office.Interop.Excel.Worksheet)worKbooK.ActiveSheet;
                        worksheet.Name = "Orders";

                        worksheet.Range[worksheet.Cells[1, 1], worksheet.Cells[1, 11]].Merge();
                        worksheet.Cells[1, 1] = "Daily Orders - " + " [" + DateTime.Now.ToString("dd-MM-yyyy hh:mm tt") + "]";
                        worksheet.Cells[1, 1].Font.Bold = true;
                        worksheet.Cells.Font.Size = 12;

                        worksheet.Rows[3].Font.Bold = true;
                        worksheet.Cells[3, 1] = "Sales No";
                        worksheet.Cells[3, 2] = "Order Date";
                        worksheet.Cells[3, 3] = "Customer";
                        worksheet.Cells[3, 4] = "Total";

                        worksheet.Range["A3"].Style.HorizontalAlignment = HorizontalAlignment.Center;
                        worksheet.Range["B3"].Style.HorizontalAlignment = HorizontalAlignment.Center;
                        worksheet.Range["C3"].Style.HorizontalAlignment = HorizontalAlignment.Center;
                        worksheet.Range["D3"].Style.HorizontalAlignment = HorizontalAlignment.Center;

                        int row = 4, qldCount = 0, nswCount = 0, vicCount = 0, qldSandCount = 0, nswSandCount = 0, vicSandCount = 0;
                        decimal qldTotal = 0, qldSandTotal = 0, nswTotal = 0, vicTotal = 0, nswSandTotal = 0, vicSandTotal = 0;
                        bool qldDone = false, nswDone = false, vicDone = false, qldSandDone = false, nswSandDone = false, vicSandDone = false;

                        Orders = new ObservableCollection<OrdersNotInvoiced>(Orders.OrderBy(x=>x.OrderFormat));

                        OrdersNotInvoiced qldLastItem = null;
                        OrdersNotInvoiced sfhQldLastItem = null;
                        OrdersNotInvoiced nswLastItem = null;
                        OrdersNotInvoiced sfhNswLastItem = null;
                        OrdersNotInvoiced vicLastItem = null;
                        OrdersNotInvoiced sfhVicLastItem = null;



                        if (Orders.Any(x => x.State.Equals("QLD")))
                        {
                            qldLastItem = Orders.Last(x => x.State.Equals("QLD"));
                        }

                        if (Orders.Any(x => x.State.Equals("Sandleford Holdings Pty Ltd (QLD)")))
                        {
                            sfhQldLastItem = Orders.Last(x => x.State.Equals("Sandleford Holdings Pty Ltd (QLD)"));
                        }
                        
                        if (Orders.Any(x => x.State.Equals("NSW")))
                        {
                            nswLastItem = Orders.Last(x => x.State.Equals("NSW"));
                        }

                        if (Orders.Any(x => x.State.Equals("Sandleford Holdings Pty Ltd (NSW)")))
                        {
                            sfhNswLastItem = Orders.Last(x => x.State.Equals("Sandleford Holdings Pty Ltd (NSW)"));
                        }

                        if (Orders.Any(x => x.State.Equals("VIC")))
                        {
                            vicLastItem = Orders.Last(x => x.State.Equals("VIC"));
                        }

                        if (Orders.Any(x => x.State.Equals("Sandleford Holdings Pty Ltd (VIC)")))
                        {
                            sfhVicLastItem = Orders.Last(x => x.State.Equals("Sandleford Holdings Pty Ltd (VIC)"));
                        }

                        foreach (var item in Orders)
                        {
                            if (item.State.Equals("QLD"))
                            {
                                if (!qldDone)
                                {
                                    row += 2;
                                    worksheet.Cells[row, 1] = "QLD";
                                    worksheet.Cells[row, 1].Font.Bold = true;
                                    worksheet.Cells.Font.Size = 12;
                                    qldDone = true;
                                }
                                qldTotal += item.Total;
                                qldCount++;
                            }                           
                            else if (item.State.Equals("Sandleford Holdings Pty Ltd (QLD)"))
                            {
                                if (!qldSandDone)
                                {
                                    row += 2;
                                    worksheet.Cells[row, 1] = "Sandleford Holdings Pty Ltd (QLD)";
                                    worksheet.Cells[row, 1].Font.Bold = true;
                                    worksheet.Cells.Font.Size = 12;
                                    qldSandDone = true;
                                }
                                qldSandTotal += item.Total;
                                qldSandCount++;
                            }
                            else if (item.State.Equals("NSW"))
                            {
                                if (!nswDone)
                                {
                                    row += 2;
                                    worksheet.Cells[row, 1] = "NSW";
                                    worksheet.Cells[row, 1].Font.Bold = true;
                                    worksheet.Cells.Font.Size = 12;
                                    nswDone = true;
                                }
                                nswTotal += item.Total;
                                nswCount++;
                            }
                            else if (item.State.Equals("Sandleford Holdings Pty Ltd (NSW)"))
                            {
                                if (!nswSandDone)
                                {
                                    row += 2;
                                    worksheet.Cells[row, 1] = "Sandleford Holdings Pty Ltd (NSW)";
                                    worksheet.Cells[row, 1].Font.Bold = true;
                                    worksheet.Cells.Font.Size = 12;
                                    nswSandDone = true;
                                }
                                nswSandTotal += item.Total;
                                nswSandCount++;
                            }
                            else if (item.State.Equals("VIC"))
                            {
                                if (!vicDone)
                                {
                                    row += 2;
                                    worksheet.Cells[row, 1] = "VIC";
                                    worksheet.Cells[row, 1].Font.Bold = true;
                                    worksheet.Cells.Font.Size = 12;
                                    vicDone = true;
                                }
                                vicTotal += item.Total;
                                vicCount++;
                            }
                            else if (item.State.Equals("Sandleford Holdings Pty Ltd (VIC)"))
                            {
                                if (!vicSandDone)
                                {
                                    row += 2;
                                    worksheet.Cells[row, 1] = "Sandleford Holdings Pty Ltd (VIC)";
                                    worksheet.Cells[row, 1].Font.Bold = true;
                                    worksheet.Cells.Font.Size = 12;
                                    vicSandDone = true;
                                }
                                vicSandTotal += item.Total;
                                vicSandCount++;
                            }

                            row++;

                            Range rg = (Microsoft.Office.Interop.Excel.Range)worksheet.Cells[row, 2];
                            rg.EntireColumn.NumberFormat = "MM/DD/YYYY";

                            worksheet.Cells[row, 1] = item.SalesID;
                            worksheet.Cells[row, 2] = item.OrderDate.ToString("dd/MM/yyyy");
                            worksheet.Cells[row, 3] = item.Name;
                            worksheet.Cells[row, 4] = item.Total.ToString("C", CultureInfo.CurrentCulture);

                            //QLD Totals
                            if (qldLastItem != null && item == qldLastItem)
                            {
                                row++;

                                worksheet.Cells[row, 1] = "TOTAL ITEMS : " + qldCount;
                                worksheet.Cells[row, 1].Font.Bold = true;

                                worksheet.Cells[row, 3] = "TOTAL";
                                worksheet.Cells[row, 3].Font.Bold = true;
                                worksheet.Cells[row, 4] = qldTotal.ToString("C", CultureInfo.CurrentCulture);
                                worksheet.Cells[row, 4].Font.Bold = true;
                            }

                            //QLD - Sandleford Totals
                            if (sfhQldLastItem != null && item == sfhQldLastItem)
                            {
                                row++;

                                worksheet.Cells[row, 1] = "TOTAL ITEMS : " + qldSandCount;
                                worksheet.Cells[row, 1].Font.Bold = true;

                                worksheet.Cells[row, 3] = "TOTAL";
                                worksheet.Cells[row, 3].Font.Bold = true;
                                worksheet.Cells[row, 4] = qldSandTotal.ToString("C", CultureInfo.CurrentCulture);
                                worksheet.Cells[row, 4].Font.Bold = true;
                            }

                            //NSW Totals
                            if (nswLastItem != null && item == nswLastItem)
                            {
                                row++;

                                worksheet.Cells[row, 1] = "TOTAL ITEMS : " + nswCount;
                                worksheet.Cells[row, 1].Font.Bold = true;

                                worksheet.Cells[row, 3] = "TOTAL";
                                worksheet.Cells[row, 3].Font.Bold = true;
                                worksheet.Cells[row, 4] = nswTotal.ToString("C", CultureInfo.CurrentCulture);
                                worksheet.Cells[row, 4].Font.Bold = true;
                            }

                            //NSW - Sandleford Totals
                            if (sfhNswLastItem != null && item == sfhNswLastItem)
                            {
                                row++;

                                worksheet.Cells[row, 1] = "TOTAL ITEMS : " + nswSandCount;
                                worksheet.Cells[row, 1].Font.Bold = true;

                                worksheet.Cells[row, 3] = "TOTAL";
                                worksheet.Cells[row, 3].Font.Bold = true;
                                worksheet.Cells[row, 4] = nswSandTotal.ToString("C", CultureInfo.CurrentCulture);
                                worksheet.Cells[row, 4].Font.Bold = true;

                            }

                            //VIC Totals
                            if (vicLastItem != null && item == vicLastItem)
                            {
                                row++;

                                worksheet.Cells[row, 1] = "TOTAL ITEMS : " + vicCount;
                                worksheet.Cells[row, 1].Font.Bold = true;

                                worksheet.Cells[row, 3] = "TOTAL";
                                worksheet.Cells[row, 3].Font.Bold = true;
                                worksheet.Cells[row, 4] = vicTotal.ToString("C", CultureInfo.CurrentCulture);
                                worksheet.Cells[row, 4].Font.Bold = true;
                            }

                            //VIC - Sandleford Totals
                            if (sfhVicLastItem != null && item == sfhVicLastItem)
                            {
                                row++;

                                worksheet.Cells[row, 1] = "TOTAL ITEMS : " + vicSandCount;
                                worksheet.Cells[row, 1].Font.Bold = true;

                                worksheet.Cells[row, 3] = "TOTAL";
                                worksheet.Cells[row, 3].Font.Bold = true;
                                worksheet.Cells[row, 4] = vicSandTotal.ToString("C", CultureInfo.CurrentCulture);
                                worksheet.Cells[row, 4].Font.Bold = true;

                            }
                        }

                        row+=2;

                        //TotalsStr
                        worksheet.Cells[row, 1] = totalsStr1;
                        worksheet.Cells[row, 1].Font.Bold = true;
                        worksheet.Range[worksheet.Cells[row, 1], worksheet.Cells[row, 4]].Merge();

                        row++;

                        worksheet.Cells[row, 1] = totalsStr2;
                        worksheet.Cells[row, 1].Font.Bold = true;
                        worksheet.Range[worksheet.Cells[row, 1], worksheet.Cells[row, 4]].Merge();

                        row++;                        

                        worksheet.Cells[row, 1] = totalsStr4;
                        worksheet.Cells[row, 1].Font.Bold = true;
                        worksheet.Range[worksheet.Cells[row, 1], worksheet.Cells[row, 4]].Merge();

                        row++;

                        worksheet.Cells[row, 1] = totalsStr3;
                        worksheet.Cells[row, 1].Font.Bold = true;
                        worksheet.Range[worksheet.Cells[row, 1], worksheet.Cells[row, 4]].Merge();
                                                
                        row++;

                        worksheet.Range["A3"].RowHeight = 30;
                        worksheet.Range["A3"].ColumnWidth = 100;

                        worksheet.Range["B3"].RowHeight = 30;
                        worksheet.Range["B3"].ColumnWidth = 100;

                        worksheet.Range["C3"].RowHeight = 30;
                        worksheet.Range["C3"].ColumnWidth = 100;

                        worksheet.Range["D3"].RowHeight = 30;
                        worksheet.Range["D3"].ColumnWidth = 100;

                        worksheet.Columns.AutoFit();

                        string fileName = "Orders-" + DateTime.Now.ToString("dd-MM-yyyy-HH.mm.ss");

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

        #region PUBLIC_PROPERTIES

        public string TotalsStr
        {
            get
            {
                return _totalsStr;
            }
            set
            {
                _totalsStr = value;
                RaisePropertyChanged("TotalsStr");
            }
        }
        
        public ObservableCollection<OrdersNotInvoiced> Orders
        {
            get
            {
                return _orders;
            }
            set
            {
                _orders = value;
                RaisePropertyChanged("Orders");
            }
        }
              

        public ListCollectionView CollDay
        {
            get { return _collDay; }
            set
            {
                _collDay = value;
                RaisePropertyChanged("CollDay");

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

        #endregion

        public ICommand ViewDataCommand
        {
            get
            {
                return _viewDataCommand ?? (_viewDataCommand = new CommandHandler(() => ViewData(false), true));
            }
        }

        public ICommand ExportToExcelCommand
        {
            get
            {
                return _exportToExcelCommand ?? (_exportToExcelCommand = new CommandHandler(() => ViewData(true), true));
            }
        }

        
    }
}
