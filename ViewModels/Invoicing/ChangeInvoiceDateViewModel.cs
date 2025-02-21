using A1RConsole.Bases;
using A1RConsole.Commands;
using A1RConsole.Core;
using A1RConsole.DB;
using A1RConsole.Models.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace A1RConsole.ViewModels.Invoicing
{
    public class ChangeInvoiceDateViewModel : ViewModelBase
    {
        public event Action Closed;
        private SalesOrder salesOrder;
        private DateTime _selectedInvoiceDate;
        private DateTime _currentDate;
        private ICommand _closeCommand;
        private ICommand _updatePrintCommand;

        public ChangeInvoiceDateViewModel(SalesOrder so)
        {
            CurrentDate = DateTime.Now;
            salesOrder = so;
            SelectedInvoiceDate = so.Invoice.InvoicedDate;
        }

        private void CloseForm()
        {
            Closed();
        }

        private void UpdatePrint()
        {
            salesOrder.Invoice.InvoicedDate = SelectedInvoiceDate;
            int r = DBAccess.UpdateInvoiceDate(salesOrder.SalesOrderNo, SelectedInvoiceDate);
            InvoicingManager im = new InvoicingManager();
            im.GenerateInvoice(salesOrder);
            //CloseForm();
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



        public ICommand CloseCommand
        {
            get
            {
                return _closeCommand ?? (_closeCommand = new CommandHandler(() => CloseForm(), true));
            }
        }

        public ICommand UpdatePrintCommand
        {
            get
            {
                return _updatePrintCommand ?? (_updatePrintCommand = new CommandHandler(() => UpdatePrint(), true));
            }
        }


    }
}
