using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A1RConsole.Models.Shifts
{
    public class DateShift
    {
        public DateTime ProdDate { get; set; }
        public List<ShiftDetails> ShiftList { get; set; }
    }
}
