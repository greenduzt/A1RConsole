using A1RConsole.Bases;
using A1RConsole.Commands;
using A1RConsole.Core;
using A1RConsole.DB;
using A1RConsole.Interfaces;
using A1RConsole.Models;
using A1RConsole.Models.Customers;
using A1RConsole.Models.Orders;
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
using System.Windows.Data;
using System.Windows.Input;

namespace A1RConsole.ViewModels.Sales
{
    public class DailySalesReportViewModel : ViewModelBase, IContent
    {
        private string _datagridVisibility;
        private string _totalSales;
        private ICollectionView _ordersView;
        private ObservableCollection<SalesOrder> _salesOrders;
        private List<string> _fromOrderDates;
        private List<string> _toOrderDates;
        private string _fromSelectedOrderDate;
        private string _toSelectedOrderDate;
        private List<Customer> _customerList;
        private Customer _selectedCustomer;
        public Person Person { get; private set; }

        private bool IsDirty { get; set; }
        public event EventHandler Closing;
        public RelayCommand CloseCommand { get; private set; }
        private ICommand _showCommand;
        private ICommand _viewCommand;
        private ICommand _clearCommand;

        public DailySalesReportViewModel()
        {   
            this.CloseCommand = new RelayCommand(CloseWindow);

            BackgroundWorker worker = new BackgroundWorker();
            ChildWindow LoadingScreen = new ChildWindow();
            LoadingScreen.ShowWaitingScreen("Loading");

            worker.DoWork += (_, __) =>
            {
                DatagridVisibility = "Collapsed";
                SalesOrders = new ObservableCollection<SalesOrder>();
                LoadCustomers();
                LoadOrderDates();                
            };

            worker.RunWorkerCompleted += delegate(object s, RunWorkerCompletedEventArgs args)
            {
                LoadingScreen.CloseWaitingScreen();
            };
            worker.RunWorkerAsync();           
        }

        private void LoadCustomers()
        {
            ObservableCollection<Customer> cus = DBAccess.GetCustomerData();
            CustomerList = new List<Customer>(cus);
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
                return true;
            }
        }

        private void LoadOrderDates()
        {
            List<DateTime> dt = new List<DateTime>();
            dt = DBAccess.GetSalesOrderDates();
            FromOrderDates = new List<string>();
            ToOrderDates = new List<string>();


            foreach (var item in dt)
            {
                FromOrderDates.Add(item.ToString("dd/MM/yyyy"));
                ToOrderDates.Add(item.ToString("dd/MM/yyyy"));
            }
        }

        private void ShowSalesOrders()
        {
            if (string.IsNullOrWhiteSpace(FromSelectedOrderDate) && string.IsNullOrWhiteSpace(ToSelectedOrderDate) && SelectedCustomer == null)
            {
                Msg.Show("Please enter Order Date or Customer", "Information Required", MsgBoxButtons.OK, MsgBoxImage.Information_Orange, MsgBoxResult.Yes);
            }
            else
            {
                string fromDate = null;
                string toDate = null;

                if (string.IsNullOrWhiteSpace(FromSelectedOrderDate))
                {
                    fromDate = null;
                }
                else
                {
                    fromDate = string.Format("'{0}'", Convert.ToDateTime(FromSelectedOrderDate).ToString("yyyy-MM-dd"));
                }

                if (string.IsNullOrWhiteSpace(ToSelectedOrderDate))
                {
                    toDate = null;
                }
                else
                {
                    toDate = string.Format("'{0}'", Convert.ToDateTime(ToSelectedOrderDate).ToString("yyyy-MM-dd"));
                }

                DatagridVisibility = "Visible";
                SalesOrders = DBAccess.GetSalesOrdersByDatesByCustomer(SelectedCustomer, fromDate, toDate);
                OrdersView = CollectionViewSource.GetDefaultView(SalesOrders);
                OrdersView.GroupDescriptions.Add(new PropertyGroupDescription("OrderDate"));
                TotalSales = "Total Sales : " + string.Format("{0:C}", SalesOrders.Sum(x => x.ListPriceTotal));

                if (SalesOrders == null || SalesOrders.Count == 0)
                {
                    Msg.Show("No information found", "Information Not Found", MsgBoxButtons.OK, MsgBoxImage.Information_Orange, MsgBoxResult.Yes);
                    SelectedCustomer = null;
                    FromSelectedOrderDate = null;
                    ToSelectedOrderDate = null;
                    TotalSales = string.Empty;
                    DatagridVisibility = "Collapsed";
                }
            }
        }


        private List<SalesOrder> RemoveSearchDuplicates(List<SalesOrder> SearchResults)
        {
            List<SalesOrder> TempList = new List<SalesOrder>();

            foreach (SalesOrder u1 in SearchResults)
            {
                bool duplicatefound = false;
                foreach (SalesOrder u2 in TempList)
                    if (u1.OrderDate == u2.OrderDate)
                        duplicatefound = true;

                if (!duplicatefound)
                    TempList.Add(u1);
            }
            return TempList;
        }

        private void ViewReport()
        {
            if (SalesOrders.Count > 0)
            {
                Tuple<Exception, string> tuple = null;

                BackgroundWorker worker = new BackgroundWorker();
                ChildWindow LoadingScreen = new ChildWindow();
                LoadingScreen.ShowWaitingScreen("Loading");

                worker.DoWork += (_, __) =>
                {
                    var soWithDuplicates = SalesOrders.OrderBy(x => x.OrderDate);

                    List<SalesOrder> so = new List<SalesOrder>(SalesOrders);
                    List<SalesOrder> soNoDuplicates = RemoveSearchDuplicates(so);
                    string compName = SelectedCustomer != null ? SelectedCustomer.CompanyName : "";
                    PrintSalesReportPDF psr = new PrintSalesReportPDF(soWithDuplicates, soNoDuplicates, FromSelectedOrderDate, ToSelectedOrderDate, compName);
                    tuple = psr.CreatePDF();

                };

                worker.RunWorkerCompleted += delegate(object s, RunWorkerCompletedEventArgs args)
                {
                    LoadingScreen.CloseWaitingScreen();
                    if (tuple.Item1 != null)
                    {
                        Msg.Show("A problem has occured while creating report. Please try again later." + System.Environment.NewLine + tuple.Item1, "Report Error", MsgBoxButtons.OK, MsgBoxImage.Information_Red, MsgBoxResult.Yes);
                    }
                    else
                    {
                        var childWindow = new ChildWindow();
                        childWindow.ShowFormula(tuple.Item2);
                    }
                };
                worker.RunWorkerAsync();
            }
            else
            {
             //   Msg.Show("No items found!", "", MsgBoxButtons.OK, MsgBoxImage.Information_Orange, MsgBoxResult.Yes);

                MessageBox.Show("No items found!", "", MessageBoxButton.OK, MessageBoxImage.Exclamation);

            }

        }      

        private void ClearFields()
        {
            SelectedCustomer = null;
            FromSelectedOrderDate = null;
            ToSelectedOrderDate = null;
            SalesOrders.Clear();
            OrdersView = CollectionViewSource.GetDefaultView(SalesOrders);
            OrdersView.GroupDescriptions.Add(new PropertyGroupDescription("OrderDate"));
            TotalSales = "Total Sales : " + string.Format("{0:C}", SalesOrders.Sum(x => x.ListPriceTotal));
        }


       

        public string Title
        {
            get
            {
                return "Sales Report";
            }
        }

        public ICollectionView OrdersView
        {
            get
            {
                return _ordersView;
            }
            set
            {
                _ordersView = value;
                RaisePropertyChanged("OrdersView");
            }
        }

        public ObservableCollection<SalesOrder> SalesOrders
        {
            get
            {
                return _salesOrders;
            }
            set
            {
                _salesOrders = value;
                RaisePropertyChanged("SalesOrders");
            }
        }

        public List<string> FromOrderDates
        {
            get
            {
                return _fromOrderDates;
            }
            set
            {
                _fromOrderDates = value;
                RaisePropertyChanged("FromOrderDates");
            }
        }

        public List<string> ToOrderDates
        {
            get
            {
                return _toOrderDates;
            }
            set
            {
                _toOrderDates = value;
                RaisePropertyChanged("ToOrderDates");
            }
        }

        public List<Customer> CustomerList
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

        public string FromSelectedOrderDate
        {
            get
            {
                return _fromSelectedOrderDate;
            }
            set
            {
                _fromSelectedOrderDate = value;
                RaisePropertyChanged("FromSelectedOrderDate");
            }
        }

        public string ToSelectedOrderDate
        {
            get
            {
                return _toSelectedOrderDate;
            }
            set
            {
                _toSelectedOrderDate = value;
                RaisePropertyChanged("ToSelectedOrderDate");
            }
        }


        public Customer SelectedCustomer
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

        public string TotalSales
        {
            get
            {
                return _totalSales;
            }
            set
            {
                _totalSales = value;
                RaisePropertyChanged("TotalSales");
            }
        }

        public string DatagridVisibility
        {
            get
            {
                return _datagridVisibility;
            }
            set
            {
                _datagridVisibility = value;
                RaisePropertyChanged("DatagridVisibility");
            }
        }



        public ICommand ShowCommand
        {
            get
            {
                return _showCommand ?? (_showCommand = new CommandHandler(() => ShowSalesOrders(), true));
            }
        }

    

        public ICommand ViewCommand
        {
            get
            {
                return _viewCommand ?? (_viewCommand = new CommandHandler(() => ViewReport(), true));
            }
        }

        public ICommand ClearCommand
        {
            get
            {
                return _clearCommand ?? (_clearCommand = new CommandHandler(() => ClearFields(), true));
            }
        }



    }
}
