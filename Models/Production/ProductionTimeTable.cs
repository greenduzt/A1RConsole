using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A1RConsole.Models.Production
{
    public class ProductionTimeTable
    {
        public int ID { get; set; }
        public int MachineID { get; set; }
        public DateTime ProductionDate { get; set; }
        public bool IsMachineActive { get; set; }
        public bool IsDayShiftActive { get; set; }
        public bool IsEveningShiftActive { get; set; }
        public bool IsNightShiftActive { get; set; }
        //public CurrentCapacityList CurrentCapacityList { get; set; }
        //public MixingCapacityList MixingCapacityList1 { get; set; }
        //public MixingCapacityList MixingCapacityList2 { get; set; }
        //public SlittingDefaultCapacity SlittingCapacityList { get; set; }
        //public PeelingDefaultCapacity PeelingCapacityList { get; set; }
    }
}
