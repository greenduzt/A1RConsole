﻿using A1RConsole.Models.Orders;
using A1RConsole.Models.Products;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A1RConsole.Models.Production.Grading
{
    public class GradingOrder
    {
        public Int32 GradingTimeTableID { get; set; }
        public Product Product { get; set; }
        public Order Order { get; set; }
        public decimal Qty { get; set; }
        public decimal BlocksLogs { get; set; }
        public int Shift { get; set; }
    }
}
