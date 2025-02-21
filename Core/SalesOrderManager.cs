using A1RConsole.DB;
using A1RConsole.Models.Invoices;
using A1RConsole.Models.Orders;
using A1RConsole.Models.Stock;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A1RConsole.Core
{
    public static class SalesOrderManager
    {
        public static ObservableCollection<SalesOrder> LoadSalesOrders(string status,int cusId)
        {
            ObservableCollection<SalesOrder> SalesOrders = DBAccess.GetCurrentOrders(GetOrderStatus(status, cusId));
            ObservableCollection<ProductStockReserved> existingProdStockList = DBAccess.CheckProductStockReserved(SalesOrders);

            foreach (var item in SalesOrders)
            {
                item.Invoice = new Invoice();
                List<string> bol = new List<string>();
                foreach (var items in existingProdStockList)
                {
                    if (item.SalesOrderNo == items.SalesNo && items.QtyOrdered == items.QtyReserved)
                    {
                        bol.Add("Yes");
                    }
                    else if (item.SalesOrderNo == items.SalesNo && items.QtyOrdered != items.QtyReserved)
                    {
                        bol.Add("No");
                    }
                }

                bool ex = bol.All(x => x == "Yes");
                item.StockReserved = ex == true ? "Yes" : "No";

                if (item.OrderStatus == OrderStatus.PreparingInvoice.ToString())
                {
                    item.StatusBackgroundCol = "#006622";
                    item.StatusForeGroundCol = "White";
                    item.Invoice.PrintInvoiceActive = true;
                    item.Invoice.PrintInvoiceBackGroundColour = "#666666";
                }
                else if (item.OrderStatus == OrderStatus.Release.ToString() || item.OrderStatus == OrderStatus.InWarehouse.ToString() || item.OrderStatus == OrderStatus.Picking.ToString() || 
                    item.OrderStatus == OrderStatus.ReadyToDispatch.ToString() || item.OrderStatus == OrderStatus.FinalisingShipping.ToString())
                {
                    item.StatusBackgroundCol = "#006622";
                    item.StatusForeGroundCol = "White";
                    item.Invoice.PrintInvoiceActive = false;
                    item.Invoice.PrintInvoiceBackGroundColour = "#a6a6a6";
                }
                else if (item.OrderStatus == OrderStatus.HoldNoStock.ToString() || item.OrderStatus == OrderStatus.AwaitingStock.ToString() || item.OrderStatus == OrderStatus.Hold.ToString() ||
                    item.OrderStatus == OrderStatus.Cancel.ToString() || item.OrderStatus == OrderStatus.HoldStockAllocated.ToString() || item.OrderStatus == OrderStatus.HoldNoCredit.ToString() ||
                    item.OrderStatus == OrderStatus.HoldNoCreditStockAllocated.ToString() || item.OrderStatus == OrderStatus.HoldNoCreditNoStock.ToString() || item.OrderStatus == OrderStatus.Return.ToString())
                {
                    item.StatusBackgroundCol = "#cc0000";
                    item.StatusForeGroundCol = "White";
                    item.Invoice.PrintInvoiceActive = false;
                    item.Invoice.PrintInvoiceBackGroundColour = "#a6a6a6";
                }
                else if (item.OrderStatus == OrderStatus.Dispatched.ToString())
                {
                    item.StatusBackgroundCol = "#084FAA";
                    item.StatusForeGroundCol = "White";
                    item.Invoice.PrintInvoiceActive = true;
                    item.Invoice.PrintInvoiceBackGroundColour = "#666666";
                    item.Invoice.PrintInvoiceActive = false;
                    item.Invoice.PrintInvoiceBackGroundColour = "#a6a6a6";
                }

                string strDay = string.Empty;
                DateTime dumDate = (DateTime)item.DesiredDispatchDate;
                double days = Math.Ceiling((dumDate - DateTime.Now).TotalDays);
                if (item.OrderStatus == OrderStatus.Dispatched.ToString())
                {
                    item.DaysRemaining = "Dispatched-" + ((DateTime)item.DispatchOrder.DispatchedDate).ToString("dd/MM/yyyy");
                    item.DaysRemBackgroundCol = "#084FAA";
                    item.DaysRemForeGroundCol = "White";
                    item.Invoice.PrintInvoiceActive = false;
                    item.Invoice.PrintInvoiceBackGroundColour = "#a6a6a6";
                }
                else if (item.OrderStatus == OrderStatus.Cancel.ToString())
                {
                    item.DaysRemaining = "N/A";
                    item.DaysRemBackgroundCol = "White";
                    item.DaysRemForeGroundCol = "Black";
                    item.Invoice.PrintInvoiceActive = false;
                    item.Invoice.PrintInvoiceBackGroundColour = "#a6a6a6";
                }
                else if (item.OrderStatus == OrderStatus.Return.ToString())
                {
                    item.DaysRemaining = "N/A";
                    item.DaysRemBackgroundCol = "White";
                    item.DaysRemForeGroundCol = "Black";
                    
                    //dr["order_status"].ToString() == OrderStatus.Return.ToString() ? "#a6a6a6" : "#666666"
                }
                else if (item.OrderStatus == OrderStatus.PreparingInvoice.ToString())
                {
                    item.Invoice.PrintInvoiceActive = true;
                    item.Invoice.PrintInvoiceBackGroundColour = "#666666";
                }
                else
                {
                    if (days < 0)
                    {
                        item.DaysRemaining = Math.Abs(days) + " Day" + CheckNumber(days) + " Late";
                        item.DaysRemBackgroundCol = "#E65722";
                        item.DaysRemForeGroundCol = "White";
                    }
                    else if (days > 0)
                    {
                        item.DaysRemaining = days + " Day" + CheckNumber(days) + " To Go";
                        item.DaysRemBackgroundCol = "#009933";
                        item.DaysRemForeGroundCol = "White";
                    }
                    else if (days == 0)
                    {
                        item.DaysRemaining = "Today";
                        item.DaysRemBackgroundCol = "#0424c1";
                        item.DaysRemForeGroundCol = "White";
                    }
                    item.Invoice.PrintInvoiceActive = false;
                    item.Invoice.PrintInvoiceBackGroundColour = "#a6a6a6";
                }
                item.OrderStatus = CoreProcess.ConvertOrderStatusEnum(item.OrderStatus);

                //string st = item.ShipTo.Substring(item.ShipTo.IndexOf("\r\n") + 1).Trim();
                item.ShipTo = !string.IsNullOrWhiteSpace(item.ShipTo) ? item.ShipTo.Replace("\r\n", ",") : string.Empty;
                //item.PaymentFinalisedBackGround = item.PaymentRecieved == true ? "#084FAA" : "#E12222";
                //item.PaymentFinalisedForeGround = item.PaymentRecieved == true ? "White" : "White";
            }

            return SalesOrders;
        }

        private static string GetOrderStatus(string status,int custId)
        {
            string s = string.Empty;

            if (status == "IsToBeDispatched")
            {
                s = "SalesOrder.order_status = 'Hold' OR SalesOrder.order_status = 'Release' OR SalesOrder.order_status = 'NoStock' OR SalesOrder.order_status = 'HoldNoStock' OR SalesOrder.order_status = 'HoldStockAllocated' OR SalesOrder.order_status = 'HoldNoCredit' OR SalesOrder.order_status = 'HoldNoCreditStockAllocated' OR SalesOrder.order_status = 'HoldNoCreditNoStock' OR SalesOrder.order_status = 'AwaitingStock' OR SalesOrder.order_status = 'InProduction' OR SalesOrder.order_status = 'InWarehouse' OR SalesOrder.order_status = 'Picking' OR SalesOrder.order_status = 'ReadyToDispatch' OR SalesOrder.order_status = 'PreparingInvoice' OR SalesOrder.order_status = 'FinalisingShipping' ";
            }
            else if (status == "IsDispatched")
            {
                s = "SalesOrder.order_status = 'Dispatched' ";
            }
            else if (status == "IsCancelled")
            {
                s = "SalesOrder.order_status = 'Cancel' ";
            }
            else if (status == "IsReturned")
            {
                s = "SalesOrder.order_status = 'Return' ";
            }
            else if (status == "IsHeld")
            {
                s = "SalesOrder.order_status = 'Hold' OR SalesOrder.order_status = 'NoStock' OR SalesOrder.order_status = 'HoldNoStock' OR SalesOrder.order_status = 'HoldStockAllocated' OR SalesOrder.order_status = 'HoldNoCredit' OR SalesOrder.order_status = 'HoldNoCreditStockAllocated' OR SalesOrder.order_status = 'HoldNoCreditNoStock' ";
            }
            else if (status == "IsAll" && custId > 0)
            {
                s = "SalesOrder.customer_id = " + custId + " AND (SalesOrder.order_status = 'Hold' OR SalesOrder.order_status = 'Release' OR SalesOrder.order_status = 'NoStock' OR SalesOrder.order_status = 'HoldNoStock' OR SalesOrder.order_status = 'HoldStockAllocated' OR SalesOrder.order_status = 'HoldNoCredit' OR SalesOrder.order_status = 'HoldNoCreditStockAllocated' OR SalesOrder.order_status = 'HoldNoCreditNoStock' OR SalesOrder.order_status = 'AwaitingStock' OR SalesOrder.order_status = 'InProduction' OR SalesOrder.order_status = 'InWarehouse' OR SalesOrder.order_status = 'Picking' OR SalesOrder.order_status = 'ReadyToDispatch' OR SalesOrder.order_status = 'Dispatched' OR SalesOrder.order_status = 'Cancel' OR SalesOrder.order_status = 'Return' OR SalesOrder.order_status = 'PreparingInvoice' OR SalesOrder.order_status = 'FinalisingShipping') ";
            }
            else if (status == "IsAll" && custId == 0)
            {
                s = "SalesOrder.order_status = 'Hold' OR SalesOrder.order_status = 'Release' OR SalesOrder.order_status = 'NoStock' OR SalesOrder.order_status = 'HoldNoStock' OR SalesOrder.order_status = 'HoldStockAllocated' OR SalesOrder.order_status = 'HoldNoCredit' OR SalesOrder.order_status = 'HoldNoCreditStockAllocated' OR SalesOrder.order_status = 'HoldNoCreditNoStock' OR SalesOrder.order_status = 'AwaitingStock' OR SalesOrder.order_status = 'InProduction' OR SalesOrder.order_status = 'InWarehouse' OR SalesOrder.order_status = 'Picking' OR SalesOrder.order_status = 'ReadyToDispatch' OR SalesOrder.order_status = 'Dispatched' OR SalesOrder.order_status = 'Cancel' OR SalesOrder.order_status = 'Return' OR SalesOrder.order_status = 'PreparingInvoice' OR SalesOrder.order_status = 'FinalisingShipping' ";
            }
            else if (status == "Search")
            {
                s = "SalesOrder.order_status = 'Hold' OR SalesOrder.order_status = 'Release' OR SalesOrder.order_status = 'NoStock' OR SalesOrder.order_status = 'HoldNoStock' OR SalesOrder.order_status = 'HoldStockAllocated' OR SalesOrder.order_status = 'HoldNoCredit' OR SalesOrder.order_status = 'HoldNoCreditStockAllocated' OR SalesOrder.order_status = 'HoldNoCreditNoStock' OR SalesOrder.order_status = 'AwaitingStock' OR SalesOrder.order_status = 'InProduction' OR SalesOrder.order_status = 'InWarehouse' OR SalesOrder.order_status = 'Picking' OR SalesOrder.order_status = 'ReadyToDispatch' OR SalesOrder.order_status = 'Dispatched' OR SalesOrder.order_status = 'Cancel' OR SalesOrder.order_status = 'Return' OR SalesOrder.order_status = 'PreparingInvoice' OR SalesOrder.order_status = 'FinalisingShipping' ";
            }
            return s;
        }

        private static string CheckNumber(double num)
        {
            string str = string.Empty;

            if ((num > 1) || (num < -1))
            {
                str = "s";
            }

            return str;
        }
    }
}
