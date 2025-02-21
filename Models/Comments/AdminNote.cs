using A1RConsole.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A1RConsole.Models.Comments
{
    public class AdminNote : ViewModelBase
    {
        public Int32 ID { get; set; }
        public int CustomerID { get; set; }
        public string Area { get; set; }
        //public string Note { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool Active { get; set; }

        private string _note;
        public string Note
        {
            get
            {
                return _note;
            }
            set
            {
                _note = value;
                RaisePropertyChanged("Note");
                
            }
        }
    }
}
