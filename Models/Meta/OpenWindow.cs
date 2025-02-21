using A1RConsole.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A1RConsole.Models.Meta
{
    public class OpenWindow : ViewModelBase
    {
        //public string WindowName { get; set; }

        private string _windowName;
        public string WindowName
        {
            get
            {
                return _windowName;
            }
            set
            {
                _windowName = value;
                RaisePropertyChanged("WindowName");
            }
        }
    }
}
