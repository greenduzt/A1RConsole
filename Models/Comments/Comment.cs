using A1RConsole.Models.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A1RConsole.Models.Comments
{
    public class Comment : User
    {
        public Int32 CommentID { get; set; }
        public string Prefix { get; set; }//SO or AO or PO
        public Int32 No { get; set; }
        public int LocationID { get; set; }
        public string Note { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime LastUpdatedDate { get; set; }
        public string TimeStamp { get; set; }
    }
}
