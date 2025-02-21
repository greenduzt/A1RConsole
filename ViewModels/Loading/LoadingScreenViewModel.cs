using A1RConsole.Bases;
using Microsoft.Practices.Prism.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A1RConsole.ViewModels.Loading
{
    public class LoadingScreenViewModel : ViewModelBase
    {
        public event Action Closed;
        private DelegateCommand _closeCommand;
        private string _msg;

        public LoadingScreenViewModel(string m)
        {
            Msg = m;
            _closeCommand = new DelegateCommand(CloseForm);
        }

        private void CloseForm()
        {
            if (Closed != null)
            {
                Closed();
            }
        }

        public string Msg
        {
            get { return _msg; }
            set
            {
                _msg = value;
                RaisePropertyChanged("Msg");
            }
        }
    }
}
