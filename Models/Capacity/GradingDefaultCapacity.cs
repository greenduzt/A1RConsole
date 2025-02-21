using A1RConsole.Models.Machines;
using A1RConsole.Models.Production.Grading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A1RConsole.Models.Capacity
{
    public class GradingDefaultCapacity
    {
        public int ID { get; set; }
        public Machine Machine { get; set; }
        public RubberGrades RubberGrade { get; set; }
        public decimal Capacity { get; set; }
        public int Shift { get; set; }
        public string Day { get; set; }
    }
}
