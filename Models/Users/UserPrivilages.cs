using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A1RConsole.Models.Users
{
    public class UserPrivilages
    {
        public int ID { get; set; }
        public int UserID { get; set; }
        public string Area { get; set; }
        public string Visibility { get; set; }
    }
}
