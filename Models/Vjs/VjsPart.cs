using A1RConsole.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A1RConsole.Models.Vjs
{
    public class VjsPart : ViewModelBase
    {
        private string _id;
        private string _description;      

        public string ID
        {
            get
            {
                return _id;
            }
            set
            {
                _id = value;
                RaisePropertyChanged("ID");
            }
        }

        public string Description
        {
            get
            {
                return _description;
            }
            set
            {
                _description = value;
               
               
                RaisePropertyChanged("Description");
            }
        }

        
    }
}
