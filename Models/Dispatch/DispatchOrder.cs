using A1RConsole.Models.Orders;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A1RConsole.Models.Dispatch
{
    public class DispatchOrder : SalesOrder
    {
        public Int32 DispatchOrderID { get; set; }
        public Int32 DeliveryDocketNo { get; set; }
        public ObservableCollection<DispatchOrderItem> DispatchOrderItem { get; set; }
        //public string ConNoteNumber { get; set; }
        public bool OrderDispatched { get; set; }
        public string DispatchedBy { get; set; }
        public DateTime? DispatchedDate { get; set; }
        public string DispatchedDateStr { get; set; }
        public string StatusStr { get; set; }
        //public string StatusForeGroundCol { get; set; }
        //public string StatusBackgroundCol { get; set; }
        public string ShipToNoLines { get; set; }
        public string BillToNoLines { get; set; }
        public bool IsProcessing { get; set; }
        public DateTime? CompletedDateTime { get; set; }
        public string DeliveryDocketString { get; set; }
        public string ConNoteNumberString { get; set; }
        public bool IsActive { get; set; }
        public string DispatchOrderVisibility { get; set; }
        public string ViewDispatchOrderVisibility { get; set; }
        public string DispatchOrderStatus { get; set; }
        public string  DispatchTimeStamp { get; set; }
        //public string PaymentFinalisedBackGround { get; set; }
        //public string PaymentFinalisedForeGround { get; set; }

        private string _conNoteNumber;
        public string ConNoteNumber
        {
            get { return _conNoteNumber; }
            set
            {
                _conNoteNumber = value;
                RaisePropertyChanged("ConNoteNumber");

               
            }
        }
    }
}
