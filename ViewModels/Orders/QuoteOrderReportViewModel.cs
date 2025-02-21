using A1RConsole.Bases;
using A1RConsole.Commands;
using A1RConsole.Core;
using A1RConsole.DB;
using A1RConsole.Interfaces;
using A1RConsole.Models.Orders;
using A1RConsole.Models.Quoting;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace A1RConsole.ViewModels.Orders
{
    public class QuoteOrderReportViewModel : ViewModelBase, IContent
    {
        private DateTime _selectedDate;
        private DateTime _dateStart;
        private string _weeklyDate;
        private string _monthlyDate;      
        private string _dataVisibility;
        private string _monthlyCount;
        private string _weeklyCount;
        private string _weeklyConversionRate;
        private string _monthlyConversionRate;

        private bool IsDirty { get; set; }
        public event EventHandler Closing;

        private ICommand _viewDataCommand;

        public QuoteOrderReportViewModel()
        {         
            SelectedDate = DateTime.Now;
            DateStart = SelectedDate;
            WeeklyDate = string.Empty;
            MonthlyDate = string.Empty;
            DataVisibility = "Collapsed";
        }

        public string Title
        {
            get
            {
                return "Quote/Order Conversion Report";
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

        private void GenerateData()
        {
            WeeklyCount = "";
            MonthlyCount = "";
            WeeklyConversionRate = "";
            MonthlyConversionRate = "";

            BackgroundWorker worker = new BackgroundWorker();
            ChildWindow LoadingScreen = new ChildWindow();
            LoadingScreen.ShowWaitingScreen("Working..");

            worker.DoWork += (_, __) =>
            {

                List<Quote> quoteList = DBAccess.GetNoOfQuotesIssuedPerMonth(SelectedDate);
            if(quoteList.Count > 0)
            {
                DataVisibility = "Visible";

                WeeklyCount = GetNoOfQuotes(quoteList, SelectedDate.Date.AddDays(-7), SelectedDate.Date).ToString();
                MonthlyCount = GetNoOfQuotes(quoteList,SelectedDate.Date.AddMonths(-1), SelectedDate.Date).ToString();

                WeeklyDate = SelectedDate.AddDays(-7).ToString("dd/MM/yyyy") + " - " + SelectedDate.ToString("dd/MM/yyyy");
                MonthlyDate = SelectedDate.AddMonths(-1).ToString("dd/MM/yyyy") + " - " + SelectedDate.ToString("dd/MM/yyyy");

                int weeklyConvertedCount = GetNoOfConvertedQuotes(SelectedDate.AddDays(-7).Date, SelectedDate.Date);
                int monthlyConvertedCount = GetNoOfConvertedQuotes(SelectedDate.AddMonths(-1).Date, SelectedDate.Date);

                WeeklyConversionRate = (weeklyConvertedCount * 100) / Convert.ToInt32(WeeklyCount) + "%";
                MonthlyConversionRate = (monthlyConvertedCount * 100) / Convert.ToInt32(MonthlyCount) + "%";
            }

            };

            worker.RunWorkerCompleted += delegate (object s, RunWorkerCompletedEventArgs args)
            {
                LoadingScreen.CloseWaitingScreen();

            };
            worker.RunWorkerAsync();
        }

        private Int32 GetNoOfQuotes(List<Quote> quoteList,DateTime startDate, DateTime endDate)
        {
            Int32 count = 0;

            count = quoteList.Where(x=>x.QuoteDate.Date >= startDate.Date && x.QuoteDate.Date <= endDate.Date).Count();

            return count;
        }


        private Int32 GetNoOfConvertedQuotes(DateTime startDate, DateTime endDate)
        {
            Int32 count = 0;

            List<NewOrderPDFM> newOrderPDFList =  DBAccess.GetConvertedQuotesByDate(SelectedDate);
            if(newOrderPDFList != null)
            {
                count = newOrderPDFList.Where(x => x.QuoteDate.Date >= startDate.Date && x.QuoteDate.Date <= endDate.Date).Count();
            }

            return count;
        }

        #region PUBLIC_PROPERTIES

        public string MonthlyConversionRate
        {
            get
            {
                return _monthlyConversionRate;
            }
            set
            {
                _monthlyConversionRate = value;
                RaisePropertyChanged("MonthlyConversionRate");
            }
        }


        public string WeeklyConversionRate
        {
            get
            {
                return _weeklyConversionRate;
            }
            set
            {
                _weeklyConversionRate = value;
                RaisePropertyChanged("WeeklyConversionRate");
            }
        }

        public string WeeklyCount
        {
            get
            {
                return _weeklyCount;
            }
            set
            {
                _weeklyCount = value;
                RaisePropertyChanged("WeeklyCount");
            }
        }

        public string MonthlyCount
        {
            get
            {
                return _monthlyCount;
            }
            set
            {
                _monthlyCount = value;
                RaisePropertyChanged("MonthlyCount");
            }
        }

        public string DataVisibility
        {
            get
            {
                return _dataVisibility;
            }
            set
            {
                _dataVisibility = value;
                RaisePropertyChanged("DataVisibility");
            }
        }


        public string WeeklyDate
        {
            get
            {
                return _weeklyDate;
            }
            set
            {
                _weeklyDate = value;
                RaisePropertyChanged("WeeklyDate");
            }
        }

        public string MonthlyDate
        {
            get
            {
                return _monthlyDate;
            }
            set
            {
                _monthlyDate = value;
                RaisePropertyChanged("MonthlyDate");
            }
        }

        public DateTime DateStart
        {
            get
            {
                return _dateStart;
            }
            set
            {
                _dateStart = value;
                RaisePropertyChanged("DateStart");
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

        #endregion
        public ICommand ViewDataCommand
        {
            get
            {
                return _viewDataCommand ?? (_viewDataCommand = new CommandHandler(() => GenerateData(), true));
            }
        }
    }
}
