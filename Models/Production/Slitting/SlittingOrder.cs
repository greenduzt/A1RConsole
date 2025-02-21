using A1RConsole.Bases;
using A1RConsole.Commands;
using A1RConsole.Core;
using A1RConsole.Models.Orders;
using A1RConsole.Models.Products;
using A1RConsole.Models.Shifts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace A1RConsole.Models.Production.Slitting
{
    public class SlittingOrder : ViewModelBase
    {
        public Int32 ID { get; set; }
        public Int32 ProdTimetableID { get; set; }
        //public Order Order { get; set; }
        public Product Product { get; set; }
        public Shift Shift { get; set; }
        public decimal Qty { get; set; }
        public decimal Blocks { get; set; }
        public decimal DollarValue { get; set; }
        public string Status { get; set; }
        public DateTime SlittingDate { get; set; }
        public string NotesVisibility { get; set; }
        private string _rowBackgroundColour;
        private ICommand _completeCommand;
        private bool canExecute;
        private Order _order;
        private string _salesOrderNoVisibility;
        //private string _notes;

        public SlittingOrder()
        {
            canExecute = true;
            RowBackgroundColour = "#ffffff";
            NotesVisibility = "Collapsed";
        }

        public Order Order
        {
            get { return _order; }
            set
            {
                _order = value;
                RaisePropertyChanged("Order");


                if (Order.OrderType == 1)
                {
                    RowBackgroundColour = "#cc3700";//Urgent                   
                }
                if (Order.OrderType == 2)
                {
                    RowBackgroundColour = "#cc3700";//Urgent Graded                    
                }
                if (Order.OrderType == 3)
                {
                    RowBackgroundColour = "#ffffff";//Normal                   
                }
                if (Order.OrderType == 4)
                {
                    RowBackgroundColour = "#ffffff";//Normal    
                }


                if (!String.IsNullOrEmpty(Order.Comments))
                {
                    NotesVisibility = "Visible";
                }
                else
                {
                    NotesVisibility = "Collapsed";
                }

                if (String.IsNullOrEmpty(Order.SalesNo))
                {
                    SalesOrderNoVisibility = "Collapsed";
                }
                else
                {
                    SalesOrderNoVisibility = "Visible";
                }
            }
        }

        public string SalesOrderNoVisibility
        {
            get { return _salesOrderNoVisibility; }
            set
            {
                _salesOrderNoVisibility = value;
                RaisePropertyChanged("SalesOrderNoVisibility");


            }
        }
        //public string Notes
        //{
        //    get { return _notes; }
        //    set
        //    {
        //        _notes = value;
        //        RaisePropertyChanged(() => this.Notes);

        //        if (!String.IsNullOrEmpty(Notes))
        //        {
        //            NotesVisibility = "Visible";
        //        }
        //        else
        //        {
        //            NotesVisibility = "Collapsed";
        //        }

        //    }
        //}

        public string RowBackgroundColour
        {
            get { return _rowBackgroundColour; }
            set
            {
                _rowBackgroundColour = value;
                RaisePropertyChanged("RowBackgroundColour");
            }
        }

        private void CompleteSlitting()
        {
            var childWindow = new ChildWindow();
            childWindow.ShowSlittingConfirmationView(this);
        }

        public ICommand CompleteCommand
        {
            get
            {
                return _completeCommand ?? (_completeCommand = new CommandHandler(() => CompleteSlitting(), canExecute));
            }
        }
    }
}

