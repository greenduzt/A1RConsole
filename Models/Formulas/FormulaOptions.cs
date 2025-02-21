using A1RConsole.Bases;
using A1RConsole.Models.Products;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A1RConsole.Models.Formulas
{
    public class FormulaOptions : ViewModelBase
    {
        public int ID { get; set; }
        public int GradingSchedulingID { get; set; }
        public int GroupID { get; set; }
        public RawProduct RawProduct { get; set; }
        //public Formulas Formula { get; set; }

        private Formula _formula;
        public Formula Formula
        {
            get
            {
                return _formula;
            }
            set
            {
                _formula = value;
                RaisePropertyChanged("Formula");
            }
        }

        private bool _checked;
        public bool Checked
        {
            get
            {
                return _checked;
            }
            set
            {
                _checked = value;
                RaisePropertyChanged("Checked");
            }
        }
    }
}
