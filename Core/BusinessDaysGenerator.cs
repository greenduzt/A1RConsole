using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A1RConsole.Core
{
    public class BusinessDaysGenerator
    {
        //private List<PublicHoliday> publicHolidayList; 
        //public BusinessDaysGenerator()
        //{
        //    publicHolidayList = DBAccess.GetAllPublicHolidays(DateTime.Now);
        //}

        public DateTime AddBusinessDays(DateTime current, int days)
        {
            var sign = Math.Sign(days);
            var unsignedDays = Math.Abs(days);
            for (var i = 0; i < unsignedDays; i++)
            {
                //do
                //{
                current = current.AddDays(sign);
                //}
                //while (current.DayOfWeek == DayOfWeek.Saturday || current.DayOfWeek == DayOfWeek.Sunday);

            }
            return current;
        }

        public DateTime AddBusinessDaysWithoutWeekEnds(DateTime current, int days)
        {
            var sign = Math.Sign(days);
            var unsignedDays = Math.Abs(days);
            for (var i = 0; i < unsignedDays; i++)
            {
                do
                {
                    current = current.AddDays(sign);
                }
                while (current.DayOfWeek == DayOfWeek.Saturday || current.DayOfWeek == DayOfWeek.Sunday);

            }
            return current;
        }

        public bool CheckWeekEnd(DateTime date)
        {
            bool isNotWeekEnd = true;

            if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
            {
                isNotWeekEnd = false;
            }

            return isNotWeekEnd;
        }





        public DateTime SubstractBusinessDays(DateTime current, int days)
        {
            var sign = Math.Sign(days);
            var unsignedDays = Math.Abs(days);
            for (var i = days; i > 0; i--)
            {
                //do
                //{
                current = current.AddDays(-sign);
                //}
                //while (current.DayOfWeek == DayOfWeek.Saturday || current.DayOfWeek == DayOfWeek.Sunday);
            }
            return current;
        }


        public DateTime SkipWeekends(DateTime date)
        {
            if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
            {
                if (date.DayOfWeek == DayOfWeek.Saturday)
                {
                    date = date.AddDays(2);
                }
                else if (date.DayOfWeek == DayOfWeek.Sunday)
                {
                    date = date.AddDays(1);
                }
            }
            return date;
        }

        public DateTime BeforeWeekends(DateTime date)
        {
            if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
            {
                if (date.DayOfWeek == DayOfWeek.Saturday)
                {
                    date = date.AddDays(-1);
                }
                else if (date.DayOfWeek == DayOfWeek.Sunday)
                {
                    date = date.AddDays(-2);
                }
            }
            return date;
        }


        public DateTime GetNextDateByDay(DayOfWeek dw)
        {
            DateTime today = DateTime.Today;
            DateTime nextMonday = today;

            if (today.DayOfWeek == DayOfWeek.Monday)
            {
                today = today.AddDays(1);
            }

            int daysUntilMonday = ((int)dw - (int)today.DayOfWeek + 7) % 7;
            return nextMonday = today.AddDays(daysUntilMonday);
        }

        public string ConvertDayToColour(DateTime date)
        {
            string col = string.Empty;

            string name = date.DayOfWeek.ToString();

            switch (name)
            {
                case "Monday": col = "#FA5B5B";
                    break;
                case "Tuesday": col = "#FA9E5B";
                    break;
                case "Wednesday": col = "#FAD85B";
                    break;
                case "Thursday": col = "#62e53e";
                    break;
                case "Friday": col = "#b1c2fd";
                    break;
                default:
                    col = "#FFFFFF";
                    break;
            }


            return col;
        }
    }
}
