using A1RConsole.DB;
using A1RConsole.Models.Shifts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A1RConsole.Core
{
    public class ShiftManager
    {
        private List<Shift> shiftDetails;

        public ShiftManager()
        {
            shiftDetails = new List<Shift>();
            shiftDetails = DBAccess.GetAllShifts();
        }

        public int GetCurrentShift()
        {
            int curentShift = 0;


            //Check if it is friday
            if (DateTime.Now.DayOfWeek == DayOfWeek.Friday)
            {
                if ((DateTime.Now.TimeOfDay >= new TimeSpan(5, 30, 0)) && (DateTime.Now.TimeOfDay <= new TimeSpan(11, 29, 58)))
                {
                    curentShift = 1;
                }
                else if ((DateTime.Now.TimeOfDay >= new TimeSpan(11, 30, 0)) && (DateTime.Now.TimeOfDay <= new TimeSpan(17, 14, 58)))
                {
                    curentShift = 2;
                }
                else
                {
                    curentShift = 3;
                }
            }
            else
            {
                //Get the current shhift
                foreach (var item in shiftDetails)
                {
                    bool isShift = TimeBetween(DateTime.Now, item.StartTime, item.EndTime);

                    if (isShift == true)
                    {
                        curentShift = item.ShiftID;
                    }
                }
            }

            return curentShift;
        }

        public DateTime GetCurrentDate()
        {
            //BusinessDaysGenerator bdg = new BusinessDaysGenerator();
            DateTime dateTime = DateTime.Now.Date;
            int curShift = GetCurrentShift();

            if (curShift == 3)//checking if still the night shift, if so goes to the previous working day
            {
                TimeSpan midNight = new TimeSpan(00, 00, 00);
                TimeSpan nightEndTime = shiftDetails[2].EndTime;
                TimeSpan curTime = DateTime.Now.TimeOfDay;

                if ((curTime >= midNight) && (curTime <= nightEndTime))
                {
                    //dateTime = bdg.SubstractBusinessDays(DateTime.Now, 1);                   
                    dateTime = DateTime.Now.AddDays(-1);
                }
            }
            return dateTime;
        }

        bool TimeBetween(DateTime datetime, TimeSpan start, TimeSpan end)
        {
            // convert datetime to a TimeSpan
            TimeSpan now = datetime.TimeOfDay;
            // see if start comes before end
            if (start < end)
                return start <= now && now <= end;
            // start is after end, so do the inverse comparison
            return !(end < now && now < start);
        }
    }
}
