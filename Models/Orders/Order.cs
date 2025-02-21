using A1RConsole.Models.Customers;
using A1RConsole.Models.Deliveries;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A1RConsole.Models.Orders
{
    public class Order
    {
        public Int32 OrderNo { get; set; }
        public int OrderPriority { get; set; }
        public int OrderType { get; set; }
        public BindingList<OrderDetails> OrderDetails { get; set; }
        public List<Delivery> DeliveryDetails { get; set; }
        public Customer Customer { get; set; }
        public MasterOrder MasterOrder { get; set; }
        public DateTime RequiredDate { get; set; }
        public DateTime OrderCreatedDate { get; set; }
        public bool IsRequiredDateSelected { get; set; }
        public string Comments { get; set; }
        public string MixingComments { get; set; }
        public string SlittingComments { get; set; }
        public string PeelingComments { get; set; }
        public string ReRollingComments { get; set; }
        public string SalesNo { get; set; }
        public decimal ListPriceTotal { get; set; }
        public decimal GST { get; set; }
        public decimal TotalAmount { get; set; }
        public string SearchString { get; set; }
    }
}
