using A1RConsole.Bases;
using A1RConsole.Commands;
using A1RConsole.DB;
using A1RConsole.Models.Stock;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;

namespace A1RConsole.ViewModels.Stock
{
    public class ReasonCodesViewModel : ViewModelBase
    {
        private ICollectionView _itemsView;
        private string _searchString;
        private List<ReasonCode> _reasonCode;
        private StockAdjustmentViewModel stockAdjustmentViewModel;
        private ReasonCode _selectedReasonCode;
        public event Action<ReasonCode> Closed;
        private ICommand _closeCommand;
        private ICommand _addReasonCommand;

        public ReasonCodesViewModel(StockAdjustmentViewModel savm)
        {
            stockAdjustmentViewModel = savm;
            ReasonCode = new List<ReasonCode>();
            ReasonCode = DBAccess.GetReasonCodes();
            if (ReasonCode.Count > 0)
            {
                _itemsView = CollectionViewSource.GetDefaultView(ReasonCode);
                _itemsView.Filter = x => Filter(x as ReasonCode);
            }

            _closeCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(CloseForm);
        }

        private bool Filter(ReasonCode model)
        {
            var searchstring = (SearchString ?? string.Empty).ToLower();
            return model != null &&
                 ((model.Code ?? string.Empty).ToLower().Contains(searchstring)) ||
                 ((model.Reason ?? string.Empty).ToLower().Contains(searchstring));
        }

        private void CloseForm()
        {

            Closed(stockAdjustmentViewModel.SelectedReasonCode);
            //PageSwitcher.Container.Children.Add(new MdiChild
            //{
            //    Name = "StockAdjustment",
            //    Title = "StockA djustment",
            //    Content = new StockAdjustmentView(PageSwitcher, stockAdjustmentViewModel),
            //    Width = 700,
            //    Height = 350,
            //    MaxWidth = 700,
            //    MaxHeight = 350,
            //    Tag = "StockAdjustment"

            //});


        }

        private void AddReason(object parameter)
        {
            int index = ReasonCode.IndexOf(parameter as ReasonCode);
            if (index > -1 && index < ReasonCode.Count)
            {
                stockAdjustmentViewModel.SelectedReasonCode = ReasonCode[index];
                CloseForm();
            }
        }

        private bool CanExecute(object parameter)
        {
            return true;
        }

        public List<ReasonCode> ReasonCode
        {
            get
            {
                return _reasonCode;
            }
            set
            {
                _reasonCode = value;
                RaisePropertyChanged("ReasonCode");

            }
        }


        public string SearchString
        {
            get { return _searchString; }
            set
            {
                _searchString = value;
                RaisePropertyChanged("SearchString");
                ItemsView.Refresh();

            }
        }

        public ICollectionView ItemsView
        {
            get { return _itemsView; }
        }

        public ICommand CloseCommand
        {
            get
            {
                return _closeCommand ?? (_closeCommand = new CommandHandler(() => CloseForm(), true));
            }
        }

        public ICommand AddReasonCommand
        {
            get
            {
                if (_addReasonCommand == null)
                {
                    _addReasonCommand = new DelegateCommand(CanExecute, AddReason);
                }
                return _addReasonCommand;
            }
        }
    }
}
