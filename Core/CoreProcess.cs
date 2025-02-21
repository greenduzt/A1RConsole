using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A1RConsole.Core
{
    public static class CoreProcess
    {
        public static string ConvertOrderStatusEnum(string os)
        {
            string result = string.Empty;

            switch (os)
            {
                case "Hold": result = "Held";
                    break;
                case "Release": result = "Released";
                    break;
                case "NoStock": result = "No Stock";
                    break;
                case "HoldNoStock": result = "Held No Stock";
                    break;
                case "PreparingInvoice": result = "Preparing Invoice";
                    break;
                case "HoldStockAllocated": result = "Held Stock Allocated";
                    break;
                case "HoldNoCredit": result = "Held No Credit";
                    break;
                case "HoldNoCreditStockAllocated": result = "Held No Credit Stock Allocated";
                    break;
                case "HoldNoCreditNoStock": result = "Held No Credit No Stock";
                    break;
                case "AwaitingStock": result = "Awaiting Stock";
                    break;
                case "InProduction": result = "In Production";
                    break;
                case "InWarehouse": result = "In Warehouse";
                    break;
                case "Picking": result = "Picking Order";
                    break;
                case "FinalisingShipping": result = "Finalising Shipping";
                    break;
                case "ReadyToDispatch": result = "Ready To Dispatch";
                    break;
                case "Completed": result = "Order Dispatched";
                    break;
                case "Dispatched": result = "Order Dispatched";
                    break;
                case "Grading": result = "Grading";
                    break;
                case "Mixing": result = "Mixing";
                    break;
                case "Curing": result = "Curing";
                    break;
                case "Slitting": result = "Slitting";
                    break;
                case "Peeling": result = "Peeling";
                    break;
                case "ReRolling": result = "ReRolling";
                    break;
                case "Bagging": result = "Bagging";
                    break;
                case "Cancel": result = "Cancelled";
                    break;
                case "Return": result = "Return";
                    break;
                case "None": result = "None";
                    break;
                default:
                    break;
            }

            return result;
        }

        public static string ConvertOrderStatusEnumInformative(string os)
        {
            string result = string.Empty;

            switch (os)
            {
                case "Hold": result = "This Order Is On Hold By User";
                    break;
                case "Release": result = "Released";
                    break;
                case "NoStock": result = "No Stock";
                    break;
                case "HoldNoStock": result = "This Order Is On Hold For No Stock";
                    break;
                case "HoldStockAllocated": result = "This Order Is On Hold But Stock Allocated";
                    break;
                case "HoldNoCredit": result = "This Order Is On Hold For Insufficient Credit";
                    break;
                case "HoldNoCreditStockAllocated": result = "This Order Is On Hold For Insufficient Credit But Stock Allocated";
                    break;
                case "HoldNoCreditNoStock": result = "This Order Is On Hold For Insufficient Credit & No Stock";
                    break;
                case "AwaitingStock": result = "This Order Is Awaiting Stock";
                    break;
                case "InWarehouse": result = "This Order Is In Warehouse";
                    break;
                case "Picking": result = "Picking Items For This Order";
                    break;
                case "FinalisingShipping": result = "Finalising Shipping";
                    break;
                case "ReadyToDispatch": result = "This Order Is Ready To Dispatch";
                    break;
                case "Completed": result = "This Order Is Dispatched";
                    break;
                case "PreparingInvoice": result = "Preparing Invoice";
                    break;
                case "Dispatched": result = "This Order Is Dispatched";
                    break;
                case "Cancel": result = "This Order Is Cancelled";
                    break;
                case "Return": result = "This Order Is Returned";
                    break;
                case "None": result = "None";
                    break;
                default:
                    break;
            }

            return result;
        }

        public static string MakePlural(decimal num)
        {
            string name = string.Empty;

            if (num > 1)
            {
                name = "s";
            }

            return name;
        }

        public static int GetStockLocationId(string sName)
        {
            int sl = 0;

            switch (sName)
            {
                case "NSW": sl = 2;
                    break;
                case "QLD": sl = 1;
                    break;
                default:
                    break;
            }
            return sl;
        }

        public static int ConvertOrderTypeToInt(string op)
        {
            int n = 0;
            switch (op)
            {
                case "Normal": n = 3;
                    break;
                case "Urgent": n = 1;
                    break;
                default:
                    break;
            }
            return n;
        }
    }
}
