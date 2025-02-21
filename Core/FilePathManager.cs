using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A1RConsole.Core
{
    public static class FilePathManager
    {
        //Quote saving path
        public static string GetQuoteSavingPath()
        {
            //default path
            string filePath = "S:/SALES SUPPORT/CUSTOMERS/Customer Quotes/Customer_Quotes_" + DateTime.Now.Year;

            if (UserData.MetaData != null)
            {
                var data = UserData.MetaData.SingleOrDefault(x => x.KeyName == "customer_quote_location");
                filePath = data.Description + DateTime.Now.Year.ToString();
            }

            return filePath;
        }


        //Quote saving path
        public static string GetNewOrderSavingPath()
        {
            //default path
            string filePath = "S:/SALES SUPPORT/SALES ORDERS/Sales_Orders_" + DateTime.Now.Year;

            if (UserData.MetaData != null)
            {
                var data = UserData.MetaData.SingleOrDefault(x => x.KeyName == "sales_order_location_pdf");
                filePath = data.Description + DateTime.Now.Year.ToString();
            }

            return filePath;
        }
    }
}
