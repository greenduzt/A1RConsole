using A1RConsole.Interfaces;
using A1RConsole.Models.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A1RConsole.Models.Quoting
{
    public class QuoteDetails : SalesOrderDetails
    {
        public int ID { get; set; }
        private Int32 _quoteNo;
        private string _timeStamp;

        public Int32 QuoteNo
        {
            get { return _quoteNo; }
            set
            {
                _quoteNo = value;

                RaisePropertyChanged(() => this.QuoteNo);
            }
        }

        public string TimeStamp
        {
            get
            {
                return _timeStamp;
            }
            set
            {
                _timeStamp = value;
                RaisePropertyChanged(() => this.TimeStamp);                
            }
        }

        

        //private int p_SequenceNumber;
        //private int _orderLine;

        //public int SequenceNumber
        //{
        //    get { return p_SequenceNumber; }

        //    set
        //    {
        //        p_SequenceNumber = value;
        //        RaisePropertyChanged(() => this.SequenceNumber);
        //        OrderLine = SequenceNumber;
        //        Console.WriteLine(SequenceNumber);
        //    }
        //}

        //public int OrderLine
        //{
        //    get { return _orderLine; }
        //    set
        //    {
        //        _orderLine = value;

        //        RaisePropertyChanged(() => this.OrderLine);
        //    }
        //}

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
