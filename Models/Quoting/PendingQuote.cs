using A1RConsole.Models.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A1RConsole.Models.Quoting
{
    public class PendingQuote : Quote
    {        
        public bool SentToPendingSale { get; set; }
        public User SentToPendingSaleBy { get; set; }
        public DateTime SentToPendingSaleDate { get; set; }

        public bool PendingSaleToSale { get; set; }
        public User PendingSaleToSaleBy { get; set; }
        public DateTime PendingSaleToSaleDate { get; set; }
        
    }
}
