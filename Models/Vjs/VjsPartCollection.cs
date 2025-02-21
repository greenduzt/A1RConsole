using A1RConsole.Bases;
using A1RConsole.Commands;
using A1RConsole.Models.Orders;
using A1RConsole.Models.Quoting;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace A1RConsole.Models.Vjs
{
    public class VjsPartCollection : ViewModelBase
    {
        public VjsPartCollection(ObservableCollection<VjsPart> vpc)
        {
            VjsProductCollection = vpc;
            SelectedPart = new VjsPart() { ID = "Select" };
        }


        public ObservableCollection<VjsPart> VjsProductCollection { get; set; }
        public VjsPart _selectedPart;      
        
        public VjsPart SelectedPart
        {
            get
            {
                return _selectedPart;
            }
            set
            {
                _selectedPart = value;

                RaisePropertyChanged("SelectedPart");
            }
        }

       
    }
}
