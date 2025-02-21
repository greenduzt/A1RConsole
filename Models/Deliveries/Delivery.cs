using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A1RConsole.Models.Deliveries
{
    public class Delivery
    {
        public int ID { get; set; }
        public Int32 SalesID { get; set; }
        public int FreightID { get; set; }
        public string FreightPaidBy { get; set; }
        public string DeliveryOption { get; set; }
        public string DeliveryType { get; set; }
        public DateTime CustomerRequiredDate { get; set; }
        public string DeliveryAddress { get; set; }
        public string Suburb { get; set; }
        public string State { get; set; }
        public string PostCode { get; set; }
        public string ContactNumber { get; set; }
        public string TailGate { get; set; }
        public string ShipVia { get; set; }
        public int DispatchTime { get; set; }
        public string Comments { get; set; }
    }
}
