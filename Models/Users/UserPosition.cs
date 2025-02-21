using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A1RConsole.Models.Users
{
    public class UserPosition
    {
        public int ID { get; set; }
        public User User { get; set; }
        public string FullName { get; set; }
        public string Position { get; set; }
    }
}
