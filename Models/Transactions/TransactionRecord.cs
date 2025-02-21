using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A1RConsole.Models.Transactions
{
    public class TransactionRecord
    {
        public string Area { get; set; }
        public string Description { get; set; }
        public string ScriptName { get; set; }
        public DateTime DateTime { get; set; }
        public string Values { get; set; }
        public string Result { get; set; }
        public string UserName { get; set; }
    }
}
