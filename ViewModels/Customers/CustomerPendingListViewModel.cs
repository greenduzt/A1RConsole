using A1RConsole.Bases;
using A1RConsole.Interfaces;
using A1RConsole.Models.Customers;
using A1RConsole.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using A1RConsole.DB;
using A1RConsole.Core;
using System.ComponentModel;

namespace A1RConsole.ViewModels.Customers
{
    public class CustomerPendingListViewModel : ViewModelBase, IContent
    {
        private bool canExecute;
        private ObservableCollection<CustomerPending> _customerPendingList;
        private bool IsDirty { get; set; }
        public event EventHandler Closing;
        private ICommand _approveCommand;
        private ICommand _refreshGridCommand;
        private ICommand _deleteCommand;

        public CustomerPendingListViewModel()
        {
            canExecute = true;
            LoadCustomerPeningList();
        }

        private void LoadCustomerPeningList()
        {

            BackgroundWorker worker = new BackgroundWorker();
            ChildWindow loadingScreen = new ChildWindow();
            loadingScreen.ShowWaitingScreen("Loading..");
            worker.DoWork += (_, __) =>
            {
                CustomerPendingList = new ObservableCollection<CustomerPending>();
                CustomerPendingList = DBAccess.GetCustomerPendingList();
            };
            worker.RunWorkerCompleted += delegate(object s, RunWorkerCompletedEventArgs args)
            {
                loadingScreen.CloseWaitingScreen();
            };
            worker.RunWorkerAsync();
        }

        private void ApproveCustomer(Object parameter)
        {
            int index = CustomerPendingList.IndexOf(parameter as CustomerPending);
            if (index >= 0)
            {
                ChildWindow showCustomerView = new ChildWindow();
                showCustomerView.ShowAddCustomerView(CustomerPendingList[index]);
                showCustomerView.showAddCustomerView_Closed += (r=>
                {
                    if(r > 0)
                    {
                        RefreshGrid();
                    }
                });



                //BackgroundWorker worker = new BackgroundWorker();
                //loadingScreen = new ChildWindow();
                //loadingScreen.ShowWaitingScreen("Loading..");
                //worker.DoWork += (_, __) =>
                //{
                //    showInvoicingScreen = new ChildWindow();
                //};
                //worker.RunWorkerCompleted += delegate(object s, RunWorkerCompletedEventArgs args)
                //{
                //    InvoicingList[index].CompletedBy = UserData.UserName;
                //    InvoicingList[index].CompletedDate = DateTime.Now;
                //    showInvoicingScreen.ShowInvoicingWindow(InvoicingList[index]);
                //    showInvoicingScreen.invoicingView_Closed += (r =>
                //    {
                //        if (r == 0)
                //        {
                //            InvoicingList.Clear();
                //            LoadOrders();
                //        }
                //    });
                //};
                //worker.RunWorkerAsync();
            }
        }

        private void DeleteCustomer(Object parameter)
        {
            int index = CustomerPendingList.IndexOf(parameter as CustomerPending);
            if (index >= 0)
            {
                //Delete customer
                if (MessageBox.Show("Are you sure you want to delete this pending customer?", "Deleting Pending Customer", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    int r=DBAccess.DeletePendingCustomer(CustomerPendingList[index].CustomerId);
                    if(r > 0)
                    {
                        RefreshGrid();
                        MessageBox.Show("Pending customer deleted", "Record Deleted", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
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

        private void RefreshGrid()
        {
            LoadCustomerPeningList();
        }

        public string Title
        {
            get
            {
                return "Customer Pending List";
            }
        }

        public bool CanClose
        {
            get
            {
                return this.IsDirty == false || MessageBox.Show("Changes were made. Do you want to close this window?", "Close", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
            }
        }

        private bool CanExecute(object parameter)
        {
            return true;
        }

        public ObservableCollection<CustomerPending> CustomerPendingList
        {
            get
            {
                return _customerPendingList;
            }
            set
            {
                _customerPendingList = value;
                RaisePropertyChanged("CustomerPendingList");
            }
        }


        public ICommand ApproveCommand
        {
            get
            {
                if (_approveCommand == null)
                {
                    _approveCommand = new DelegateCommand(CanExecute, ApproveCustomer);
                }
                return _approveCommand;
            }
        }

        public ICommand DeleteCommand
        {
            get
            {
                if (_deleteCommand == null)
                {
                    _deleteCommand = new DelegateCommand(CanExecute, DeleteCustomer);
                }
                return _deleteCommand;
            }
        }

        

        public ICommand RefreshGridCommand
        {
            get
            {
                return _refreshGridCommand ?? (_refreshGridCommand = new CommandHandler(() => RefreshGrid(), canExecute));
            }
        }
        
    }
}
