using A1RConsole.Bases;
using A1RConsole.Commands;
using A1RConsole.Core;
using A1RConsole.DB;
using A1RConsole.Interfaces;
using A1RConsole.Models.Customers;
using A1RConsole.Models.Products;
using A1RConsole.Models.Vjs;
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

namespace A1RConsole.ViewModels.Vjs
{
    public class OrderItemsViewModel : ViewModelBase, IContent
    {
        private DateTime _endDate;
        private DateTime _selectedStartDate;
        private DateTime _selectedEndDate;
        private ObservableCollection<VjsPart> _productList;
        private ObservableCollection<VjsCustomer> _customerList;
        private ObservableCollection<VjsOrder> _vjsOrders;
        private ObservableCollection<VjsPartCollection> _vjsPartCollection;
        private VjsCustomer _selectedCustomer;        
        private ListCollectionView _orderCollection = null;
        private decimal _qldTotal;
        private decimal _nswTotal;
        private bool IsDirty { get; set; }
        public event EventHandler Closing;
        private ICommand _viewDataCommand;
        private ICommand _clearCommand;
        private ICommand _exportToExcelCommand;
        private ICommand _addProductCommand;
        private ICommand _removeCommand;

        public OrderItemsViewModel()
        {
            BackgroundWorker worker = new BackgroundWorker();
            ChildWindow LoadingScreen = new ChildWindow();

            LoadingScreen.ShowWaitingScreen("Working..");

            worker.DoWork += (_, __) =>
            {
                SelectedStartDate = DateTime.Now;
                SelectedEndDate = DateTime.Now;
                EndDate = DateTime.Now;
                LoadProducts();
                LoadCustomers();

                VjsPartCollection = new ObservableCollection<VjsPartCollection>();
            };
            worker.RunWorkerCompleted += delegate (object s, RunWorkerCompletedEventArgs args)
            {
                LoadingScreen.CloseWaitingScreen();

            };
            worker.RunWorkerAsync();


        }

        //public void LoadNotInvoiced()
        //{
        //    List<VjsNotInvoiced> notInvoiced = DBAccess.GetVJSNotInvoiced();


        //    Microsoft.Office.Interop.Excel.Application excel = new Microsoft.Office.Interop.Excel.Application();
        //    excel.Visible = false;
        //    excel.DisplayAlerts = false;

        //    Microsoft.Office.Interop.Excel.Workbook worKbooK = excel.Workbooks.Add(Type.Missing);
        //    Microsoft.Office.Interop.Excel.Worksheet worksheet = (Microsoft.Office.Interop.Excel.Worksheet)worKbooK.ActiveSheet;

        //    worksheet.Name = "OrderItems";
        //    int row = 1;
        //    worksheet.Range[worksheet.Cells[row, 1], worksheet.Cells[row, 8]].Merge();

        //    try
        //    {
        //        excel = new Microsoft.Office.Interop.Excel.Application();
        //        excel.Visible = false;
        //        excel.DisplayAlerts = false;
        //        worKbooK = excel.Workbooks.Add(Type.Missing);

        //        worksheet = (Microsoft.Office.Interop.Excel.Worksheet)worKbooK.ActiveSheet;
        //        worksheet.Name = "OrderItems";



        //        row++;
        //        worksheet.Rows[row].Font.Bold = true;
        //        worksheet.Rows[row].Font.Size = 14;
        //        worksheet.Cells[row, 1] = "Sales No";
        //        worksheet.Cells[row, 2] = "Order Date";
        //        worksheet.Cells[row, 3] = "Desired Shipping Date";
        //        worksheet.Cells[row, 4] = "Customer";
        //        worksheet.Cells[row, 5] = "Line No";
        //        worksheet.Cells[row, 6] = "Part Id";
        //        worksheet.Cells[row, 7] = "Description";
        //        worksheet.Cells[row, 8] = "Qty";
        //        worksheet.Cells[row, 9] = "Unit";
        //        worksheet.Cells[row, 10] = "Unit Price";
        //        worksheet.Cells[row, 11] = "Total";

        //        worksheet.Range["A" + row].RowHeight = 30;
        //        worksheet.Range["A" + row].ColumnWidth = 100;

        //        worksheet.Range["B" + row].RowHeight = 30;
        //        worksheet.Range["B" + row].ColumnWidth = 100;

        //        worksheet.Range["C" + row].RowHeight = 30;
        //        worksheet.Range["C" + row].ColumnWidth = 100;

        //        worksheet.Range["D" + row].RowHeight = 30;
        //        worksheet.Range["D" + row].ColumnWidth = 100;

        //        worksheet.Range["A" + row].Style.HorizontalAlignment = HorizontalAlignment.Center;
        //        worksheet.Range["B" + row].Style.HorizontalAlignment = HorizontalAlignment.Center;
        //        worksheet.Range["C" + row].Style.HorizontalAlignment = HorizontalAlignment.Center;
        //        worksheet.Range["D" + row].Style.HorizontalAlignment = HorizontalAlignment.Center;

        //        row++;
        //        foreach (var item in notInvoiced)
        //        {
        //            Range rg1 = (Microsoft.Office.Interop.Excel.Range)worksheet.Cells[row, 2];
        //            rg1.EntireColumn.NumberFormat = "MM/DD/YYYY";
        //            Range rg2 = (Microsoft.Office.Interop.Excel.Range)worksheet.Cells[row, 3];
        //            rg2.EntireColumn.NumberFormat = "MM/DD/YYYY";

        //            worksheet.Cells[row, 1] = item.ID;
        //            worksheet.Cells[row, 2] = item.OrderDate.ToString("dd/MM/yyyy");
        //            worksheet.Cells[row, 3] = item.DesiredSipDate.ToString("dd/MM/yyyy");
        //            worksheet.Cells[row, 4] = item.Name;
        //            worksheet.Cells[row, 5] = item.LineNo;
        //            worksheet.Cells[row, 6] = item.PartID;
        //            worksheet.Cells[row, 7] = item.Description;
        //            worksheet.Cells[row, 8] = item.OrderQty;
        //            worksheet.Cells[row, 9] = item.Unit;
        //            worksheet.Cells[row, 10] = item.UnitPrice.ToString("C", CultureInfo.CurrentCulture);
        //            worksheet.Cells[row, 11] = item.TotalAmount.ToString("C", CultureInfo.CurrentCulture);
        //            row++;
        //        }


        //        worksheet.Columns.AutoFit();

        //        string fileName = "OrdersItems" + DateTime.Now.ToString("dd-MM-yyyy-HH.mm.ss");

        //        excel.Visible = true;

        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show(ex.Message);

        //    }
        //    finally
        //    {
        //        worksheet = null;
        //        worKbooK = null;
        //    }


        //}

        private void ViewData(bool runExport)
        {
            //LoadNotInvoiced();

            BackgroundWorker worker = new BackgroundWorker();
            ChildWindow LoadingScreen = new ChildWindow();

            LoadingScreen.ShowWaitingScreen("Working..");

            worker.DoWork += (_, __) =>
            {
                VjsOrders = new ObservableCollection<VjsOrder>();
                VjsOrders = DBAccess.GetVJSOrders(SelectedStartDate, SelectedEndDate, SelectedCustomer, VjsPartCollection);

            };
            worker.RunWorkerCompleted += delegate (object s, RunWorkerCompletedEventArgs args)
            {
                LoadingScreen.CloseWaitingScreen();
                if (VjsOrders.Count == 0)
                {
                    MessageBox.Show("No information found", "", MessageBoxButton.OK, MessageBoxImage.Information);
                    VjsOrders = new ObservableCollection<VjsOrder>();
                    OrderCollection = new ListCollectionView(VjsOrders);
                }
                else
                {

                    OrderCollection = new ListCollectionView(VjsOrders);
                    OrderCollection.GroupDescriptions.Add(new PropertyGroupDescription("State"));

                    if (runExport)
                    {
                        ExportToExcel();
                    }
                }
            };
            worker.RunWorkerAsync();
        }
        
        public void ExportToExcel()
        {            

            //if (VjsOrders != null && VjsOrders.Count > 0)
            //{
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

                        worksheet.Name = "OrderItems";
                        int row = 1;
                        worksheet.Range[worksheet.Cells[row, 1], worksheet.Cells[row, 8]].Merge();

                        try
                        {
                            excel = new Microsoft.Office.Interop.Excel.Application();
                            excel.Visible = false;
                            excel.DisplayAlerts = false;
                            worKbooK = excel.Workbooks.Add(Type.Missing);

                            worksheet = (Microsoft.Office.Interop.Excel.Worksheet)worKbooK.ActiveSheet;
                            worksheet.Name = "OrderItems";

                            row++;
                            worksheet.Range[worksheet.Cells[row, 1], worksheet.Cells[row, 11]].Merge();
                            worksheet.Cells[row, 1] = "Printed Date Time : " + DateTime.Now;
                            worksheet.Cells[row, 1].Font.Bold = true;
                            worksheet.Cells.Font.Size = 12;
                            row += 2;
                            worksheet.Range[worksheet.Cells[row, 1], worksheet.Cells[row, 11]].Merge();
                            worksheet.Cells[row, 1] = "Periodic Orders - From " + SelectedStartDate.ToString("dd/MM/yyyy") + " To " + SelectedEndDate.ToString("dd/MM/yyyy");
                            worksheet.Cells[row, 1].Font.Bold = true;
                            worksheet.Cells.Font.Size = 12;

                            if (SelectedCustomer != null && !SelectedCustomer.Name.Equals("No Customer"))
                            {
                                row++;
                                worksheet.Cells[row, 1] = "Filtered by customer : " + SelectedCustomer.Name;
                                worksheet.Cells[row, 1].Font.Bold = true;
                                worksheet.Cells.Font.Size = 12;
                                worksheet.Range[worksheet.Cells[row, 1], worksheet.Cells[row, 11]].Merge();
                            }

                            string partName = string.Empty;


                            if (VjsPartCollection != null && VjsPartCollection.Count > 0)
                            {
                                bool run = false;
                                foreach (var item in VjsPartCollection)
                                {
                                    if (item.SelectedPart != null && !item.SelectedPart.ID.Equals("Select"))
                                    {
                                        if (!run)
                                        {
                                            partName += item.SelectedPart.ID;
                                            run = true;
                                        }
                                        else
                                        {
                                            partName += " | " + item.SelectedPart.ID;
                                        }
                                    }
                                }
                            }

                            if (!string.IsNullOrEmpty(partName))
                            {
                                row++;
                                worksheet.Cells[row, 1] = "Filtered by part : " + partName;
                                worksheet.Cells[row, 1].Font.Bold = true;
                                worksheet.Cells[row, 1].Font.Size = 12;
                                worksheet.Range[worksheet.Cells[row, 1], worksheet.Cells[row, 11]].Merge();
                            }

                            row++;
                            worksheet.Rows[row].Font.Bold = true;
                            worksheet.Rows[row].Font.Size = 14;
                            worksheet.Cells[row, 1] = "Sales No";
                            worksheet.Cells[row, 2] = "Order Date";
                            worksheet.Cells[row, 3] = "Customer";
                            worksheet.Cells[row, 4] = "Line No";
                            worksheet.Cells[row, 5] = "Part Id";
                            worksheet.Cells[row, 6] = "Description";
                            worksheet.Cells[row, 7] = "Qty";
                            worksheet.Cells[row, 8] = "Unit";
                            worksheet.Cells[row, 9] = "Unit Price";
                            worksheet.Cells[row, 10] = "Total";

                            worksheet.Range["A" + row].RowHeight = 30;
                            worksheet.Range["A" + row].ColumnWidth = 100;

                            worksheet.Range["B" + row].RowHeight = 30;
                            worksheet.Range["B" + row].ColumnWidth = 100;

                            worksheet.Range["C" + row].RowHeight = 30;
                            worksheet.Range["C" + row].ColumnWidth = 100;

                            worksheet.Range["D" + row].RowHeight = 30;
                            worksheet.Range["D" + row].ColumnWidth = 100;

                            worksheet.Range["A" + row].Style.HorizontalAlignment = HorizontalAlignment.Center;
                            worksheet.Range["B" + row].Style.HorizontalAlignment = HorizontalAlignment.Center;
                            worksheet.Range["C" + row].Style.HorizontalAlignment = HorizontalAlignment.Center;
                            worksheet.Range["D" + row].Style.HorizontalAlignment = HorizontalAlignment.Center;

                            decimal total = 0;
                            bool qldDone = false, nswDone = false, vicDone = false;

                            foreach (var item in VjsOrders)
                            {
                                if (item.State.Equals("QLD"))
                                {
                                    if (!qldDone)
                                    {
                                        row += 2;
                                        worksheet.Cells[row, 1] = "QLD Orders";
                                        worksheet.Cells[row, 1].Font.Bold = true;
                                        worksheet.Cells.Font.Size = 12;
                                        qldDone = true;
                                        row++;
                                    }
                                }
                                else if (item.State.Equals("NSW"))
                                {
                                    if (!nswDone)
                                    {
                                        if (qldDone)
                                        {
                                            row++;
                                            worksheet.Cells[row, 9] = "TOTAL";
                                            worksheet.Cells[row, 9].Font.Bold = true;
                                            worksheet.Cells[row, 10] = total.ToString("C", CultureInfo.CurrentCulture);
                                            worksheet.Cells[row, 10].Font.Bold = true;

                                            total = 0;
                                            row += 2;
                                        }
                                        worksheet.Cells[row, 1] = "NSW Orders";
                                        worksheet.Cells[row, 1].Font.Bold = true;
                                        worksheet.Cells.Font.Size = 12;
                                        nswDone = true;
                                        row++;
                                    }
                                }
                                else if (item.State.Equals("VIC"))
                                {
                                    if (!vicDone)
                                    {
                                        if (qldDone)
                                        {
                                            row++;
                                            worksheet.Cells[row, 9] = "TOTAL";
                                            worksheet.Cells[row, 9].Font.Bold = true;
                                            worksheet.Cells[row, 10] = total.ToString("C", CultureInfo.CurrentCulture);
                                            worksheet.Cells[row, 10].Font.Bold = true;

                                            total = 0;
                                            row += 2;
                                        }
                                        worksheet.Cells[row, 1] = "VIC Orders";
                                        worksheet.Cells[row, 1].Font.Bold = true;
                                        worksheet.Cells.Font.Size = 12;
                                        vicDone = true;
                                        row++;
                                    }
                                }

                            //row++;

                            Range rg = (Microsoft.Office.Interop.Excel.Range)worksheet.Cells[row, 2];
                                rg.EntireColumn.NumberFormat = "MM/DD/YYYY";

                                worksheet.Cells[row, 1] = item.OrderID;
                                worksheet.Cells[row, 2] = item.OrderDate.ToString("dd/MM/yyyy");
                                worksheet.Cells[row, 3] = item.CustomerName;

                                foreach (var items in item.VjsOrderDetails)
                                {
                                    worksheet.Cells[row, 1] = item.OrderID;
                                    worksheet.Cells[row, 2] = item.OrderDate.ToString("dd/MM/yyyy");
                                    worksheet.Cells[row, 3] = item.CustomerName;
                                    worksheet.Cells[row, 4] = items.LineNo;
                                    worksheet.Cells[row, 5] = items.PartID;
                                    worksheet.Cells[row, 6] = items.Description;
                                    worksheet.Cells[row, 7] = items.OrderQty;
                                    worksheet.Cells[row, 8] = items.Unit;
                                    worksheet.Cells[row, 9] = items.UnitPrice;
                                    worksheet.Cells[row, 10] = items.OrderAmount.ToString("C", CultureInfo.CurrentCulture);
                                    total += items.OrderAmount;
                                    row++;
                                }
                            }

                            if (nswDone)
                            {
                                row++;
                                worksheet.Cells[row, 9] = "TOTAL";
                                worksheet.Cells[row, 9].Font.Bold = true;
                                worksheet.Cells[row, 10] = total.ToString("C", CultureInfo.CurrentCulture);
                                worksheet.Cells[row, 10].Font.Bold = true;
                            }
                            else
                            {
                                row++;
                                worksheet.Cells[row, 9] = "TOTAL";
                                worksheet.Cells[row, 9].Font.Bold = true;
                                worksheet.Cells[row, 10] = total.ToString("C", CultureInfo.CurrentCulture);
                                worksheet.Cells[row, 10].Font.Bold = true;
                            }

                            worksheet.Columns.AutoFit();

                            string fileName = "OrdersItems" + DateTime.Now.ToString("dd-MM-yyyy-HH.mm.ss");

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


        private void RemoveProduct(object parameter)
        {
            int index = VjsPartCollection.IndexOf(parameter as VjsPartCollection);
            if (index > -1 && index < VjsPartCollection.Count)
            {
                VjsPartCollection.RemoveAt(index);

            }
        }

        private void AddProduct()
        {
            if (VjsPartCollection == null)
            {
                VjsPartCollection = new ObservableCollection<VjsPartCollection>();
            }
            VjsPartCollection.Add(new VjsPartCollection(ProductList));
        }


        private void ClearData()
        {
            BackgroundWorker worker = new BackgroundWorker();
            ChildWindow LoadingScreen = new ChildWindow();

            LoadingScreen.ShowWaitingScreen("Working..");

            worker.DoWork += (_, __) =>
            {
                LoadProducts();
                LoadCustomers();

                SelectedCustomer.Name = "No Customer";

                SelectedStartDate = DateTime.Now;
                SelectedEndDate = DateTime.Now;

                VjsOrders = new ObservableCollection<VjsOrder>();
                OrderCollection = new ListCollectionView(VjsOrders);

                VjsPartCollection = new ObservableCollection<VjsPartCollection>();

            };
            worker.RunWorkerCompleted += delegate (object s, RunWorkerCompletedEventArgs args)
            {
                LoadingScreen.CloseWaitingScreen();

            };
            worker.RunWorkerAsync();
        }

        private void LoadProducts()
        {
            ProductList = new ObservableCollection<VjsPart>();
            ProductList = DBAccess.GetVJSProducts();
            ProductList.Insert(0, new VjsPart() { ID = "Select", Description = "" });

        }

        private void LoadCustomers()
        {
            CustomerList = new ObservableCollection<VjsCustomer>();
            CustomerList = DBAccess.GetVJSOrderCustomers();
            CustomerList.Insert(0, new VjsCustomer() { Name = "No Customer" });

            SelectedCustomer = new VjsCustomer();
            SelectedCustomer.Name = "No Customer";
        }

        public string Title
        {
            get
            {
                return "Periodic Order Report";
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

        public bool CanClose
        {
            get
            {
                return this.IsDirty == false || MessageBox.Show("Changes were made. Do you want to close this window?", "Close", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
            }
        }

        
        public decimal QldTotal
        {
            get
            {
                return _qldTotal;
            }
            set
            {
                _qldTotal = value;
                RaisePropertyChanged("QldTotal");
            }
        }

        public decimal NswTotal
        {
            get
            {
                return _nswTotal;
            }
            set
            {
                _nswTotal = value;
                RaisePropertyChanged("NswTotal");
            }
        }

        public VjsCustomer SelectedCustomer
        {
            get
            {
                return _selectedCustomer;
            }
            set
            {
                _selectedCustomer = value;
                RaisePropertyChanged("SelectedCustomer");
            }
        }
        public ObservableCollection<VjsPart> ProductList
        {
            get
            {
                return _productList;
            }
            set
            {
                _productList = value;
                RaisePropertyChanged("ProductList");
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

        public DateTime SelectedStartDate
        {
            get
            {
                return _selectedStartDate;
            }
            set
            {
                _selectedStartDate = value;
                RaisePropertyChanged("SelectedStartDate");
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

        

        public ObservableCollection<VjsCustomer> CustomerList
        {
            get
            {
                return _customerList;
            }
            set
            {
                _customerList = value;
                RaisePropertyChanged("CustomerList");
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

        public ListCollectionView OrderCollection
        {
            get { return _orderCollection; }
            set
            {
                _orderCollection = value;
                RaisePropertyChanged("OrderCollection");

            }
        }

        public ObservableCollection<VjsPartCollection> VjsPartCollection
        {
            get { return _vjsPartCollection; }
            set
            {
                _vjsPartCollection = value;
                RaisePropertyChanged("VjsPartCollection");               
            }
        }

        
        public ICommand AddProductCommand
        {
            get
            {
                return _addProductCommand ?? (_addProductCommand = new CommandHandler(() => AddProduct(), true));
            }
        }

        public ICommand ViewDataCommand
        {
            get
            {
                return _viewDataCommand ?? (_viewDataCommand = new CommandHandler(() => ViewData(false), true));
            }
        }

        public ICommand ClearCommand
        {
            get
            {
                return _clearCommand ?? (_clearCommand = new CommandHandler(() => ClearData(), true));
            }
        }

        public ICommand ExportToExcelCommand
        {
            get
            {
                return _exportToExcelCommand ?? (_exportToExcelCommand = new CommandHandler(() => ViewData(true), true));
            }
        }

        public ICommand RemoveCommand
        {
            get
            {
                if (_removeCommand == null)
                {
                    _removeCommand = new DelegateCommand(CanExecute, RemoveProduct);
                }
                return _removeCommand;
            }
        }

    }
}
