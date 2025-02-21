using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A1RConsole.Core
{
    public static class MetaDataManager
    {
        public static string[] GetPriceEditingProducts()
        {
            string[] prods = null;

            if (UserData.MetaData != null)
            {
                foreach (var item in UserData.MetaData)
                {
                    if (item.KeyName == "quoting_price_edit")
                    {
                        prods = item.Description.Split('|');
                        break;
                    }
                }
            }

            return prods;
        }
    }
}
