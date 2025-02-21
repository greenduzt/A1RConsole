using A1RConsole.Models.Customers;
using A1RConsole.Models.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A1RConsole.Models.Orders.OnlineOrders
{
    public class OnlineOrder
    {
        public Int32 OnlineOrderNo { get; set; }
        public string OrderRefNo { get; set; }
        public OnlineOrderCustomer OnlineOrderCustomer { get; set; }
        public List<OnlineOrderItem> OnlineOrderItem { get; set; }
        public string JobName { get; set; }
        public string OrderPlacedBy { get; set; }
        public double GST { get; set; }
        public double SubTotal { get; set; }
        public double LessDiscount { get; set; }
        public double TotalAmount { get; set; }
        public bool LengthRequired { get; set; }
        public string Lengths { get; set; }
        public string DeliveryMethod { get; set; }
        public string CollectOption { get; set; }
        public string CollectDate { get; set; }
        public string CollectComments { get; set; }
        public string RequiredDeliveryDate { get; set; }
        public string DeliveryAddress { get; set; }
        public string DeliveryState { get; set; }
        public string DeliveryPostCode { get; set; }
        public string DeliveryContactNo { get; set; }
        public bool TailgateRequired { get; set; }
        public string DeliveryComments { get; set; }
        public string PaymentMethod { get; set; }
        public bool Dispatched { get; set; }
        public bool OrderCancelled { get; set; }
        public DateTime OrderTimeStamp { get; set; }
        public double LessPromoDiscount { get; set; }
        public int PromoDiscount { get; set; }
        public string PromoMsg { get; set; }
        public bool SendToSales { get; set; }
        public DateTime SendToSalesDateTime { get; set; }
        public string TimeStamp { get; set; }
        public bool Active { get; set; }
    }
}
