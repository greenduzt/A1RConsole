using A1RConsole.DB;
using A1RConsole.Models.Transactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A1RConsole.Core
{
    public static class TransactionManager
    {
        public static void CreateTransaction(List<TransactionRecord> tr)
        {
            DBAccess.InsertTransactionRecord(tr);
        }
    }
}
