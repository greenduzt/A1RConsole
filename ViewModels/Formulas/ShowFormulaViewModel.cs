using A1RConsole.Bases;
using Microsoft.Practices.Prism.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A1RConsole.ViewModels.Formulas
{
    public class ShowFormulaViewModel : ViewModelBase
    {
        public event Action Closed;
        private DelegateCommand _closeCommand;
        private string _formulaName;

        public ShowFormulaViewModel(string formula)
        {
            FormulaName = formula;
            _closeCommand = new DelegateCommand(CloseForm);
        }

        private void CloseForm()
        {
            if (Closed != null)
            {
                Closed();
            }
        }


        public string FormulaName
        {
            get
            {
                return _formulaName;
            }

            set
            {
                _formulaName = value;
                RaisePropertyChanged("FormulaName");
            }
        }

        public DelegateCommand CloseCommand
        {
            get { return _closeCommand; }
        }
    }
}
