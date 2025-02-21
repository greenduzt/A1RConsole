using A1RConsole.DB;
using A1RConsole.Models.Orders;
using A1RConsole.PdfGeneration;
using MsgBox;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace A1RConsole.Core
{
    public class InvoicingManager
    {
        private Exception ex;
        private SalesOrder salesOrder;
        public InvoicingManager() { }

        public void GenerateInvoice(SalesOrder so)
        {
            Tuple<Exception, string> tuple = null;
            BackgroundWorker worker = new BackgroundWorker();
            ChildWindow LoadingScreen = new ChildWindow();
            LoadingScreen.ShowWaitingScreen("Loading");

            worker.DoWork += (_, __) =>
            {
                salesOrder = new SalesOrder();
                salesOrder = DBAccess.GetSalesOrderDetails(so.SalesOrderNo);
                salesOrder.Invoice.InvoicedDate = so.Invoice.InvoicedDate;
                tuple = PrintInvoicePDF();
            };

            worker.RunWorkerCompleted += delegate(object s, RunWorkerCompletedEventArgs args)
            {
                LoadingScreen.CloseWaitingScreen();
                if (tuple.Item1 != null)
                {
                    Msg.Show("A problem has occured while prining. Please try again later." + System.Environment.NewLine + tuple.Item1, "Printing Error", MsgBoxButtons.OK, MsgBoxImage.Information_Red, MsgBoxResult.Yes);
                }
                else
                {
                    var childWindow = new ChildWindow();
                    childWindow.ShowFormula(tuple.Item2);
                }
            };
            worker.RunWorkerAsync();
        }

        private Tuple<Exception, string> PrintInvoicePDF()
        {
            Tuple<Exception, string> tuple = null;
            PrintInvoicePDF p = new PrintInvoicePDF(salesOrder);
            tuple = p.CreateInvoice();

            return tuple;
        }

        public string Get8Digits()
        {
            var bytes = new byte[4];
            var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            uint random = BitConverter.ToUInt32(bytes, 0) % 100000000;
            return String.Format("{0:D8}", random);
        }
    }
}

