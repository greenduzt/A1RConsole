using A1RConsole.Models.Meta;
using A1RConsole.Models.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A1RConsole.Core
{
    public static class UserData
    {
        public static int UserID = 0;
        public static string UserName = string.Empty;
        public static string State = string.Empty;
        public static string FirstName = string.Empty;
        public static string LastName = string.Empty;
        public static List<MetaData> MetaData = null;
        public static List<UserPrivilages> UserPrivilages = null;
      
    }
}
