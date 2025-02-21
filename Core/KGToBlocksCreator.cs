using A1RConsole.Models.Capacity;
using A1RConsole.Models.Formulas;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A1RConsole.Core
{
    public class KGToBlocksCreator
    {
        private ObservableCollection<CurrentCapacity> currentCapacities;
        private ObservableCollection<Formula> formulaColl;
        private int salesId;
        private decimal blockLogQty;

        public KGToBlocksCreator(ObservableCollection<CurrentCapacity> cc, ObservableCollection<Formula> fc, int si, decimal blq)
        {
            currentCapacities = cc;
            formulaColl = fc;
            salesId = si;
            blockLogQty = blq;
        }

        public List<CurrentCapacity> CreateBlocks()
        {
            int cap = 2;

            List<CurrentCapacity> lCC = new List<CurrentCapacity>();

            foreach (var itemC1 in currentCapacities)
            {
                var results = formulaColl.First(s => s.RawProductID == itemC1.RawProductID);

                if (itemC1.ProductCapacityID == results.ProductCapacity1)
                {
                    cap = results.ProductCapacity2;
                }
                else if (itemC1.ProductCapacityID == results.ProductCapacity2)
                {
                    cap = results.ProductCapacity1;
                }

                foreach (var item in currentCapacities)
                {
                    if (itemC1.ProdTimeTableID == item.ProdTimeTableID && itemC1.RawProductID == item.RawProductID && item.ProductCapacityID == cap && itemC1.Shift == item.Shift && itemC1.Paired == false && item.Paired == false)
                    {
                        itemC1.Paired = true;
                        item.Paired = true;
                        decimal minVal = Math.Min(itemC1.BlocksLogs, item.BlocksLogs);

                        if (itemC1.BlocksLogs == minVal)
                        {
                            lCC.Add(new CurrentCapacity() { ProdTimeTableID = itemC1.ProdTimeTableID, GradingDate = item.GradingDate, RawProductID = itemC1.RawProductID, ProductID = itemC1.ProductID, SalesID = salesId, BlocksLogs = minVal, Shift = itemC1.Shift, OrderType = item.OrderType });
                        }
                        else
                        {
                            lCC.Add(new CurrentCapacity() { ProdTimeTableID = item.ProdTimeTableID, GradingDate = item.GradingDate, RawProductID = item.RawProductID, ProductID = item.ProductID, SalesID = salesId, BlocksLogs = minVal, Shift = item.Shift, OrderType = item.OrderType });
                        }
                    }
                }
                if (itemC1.Paired == false)
                {
                    lCC.Add(new CurrentCapacity() { ProdTimeTableID = itemC1.ProdTimeTableID, GradingDate = itemC1.GradingDate, RawProductID = itemC1.RawProductID, ProductID = itemC1.ProductID, SalesID = salesId, BlocksLogs = itemC1.BlocksLogs, Shift = itemC1.Shift, OrderType = itemC1.OrderType });
                }
            }


            foreach (var item in lCC)
            {
                Console.WriteLine(item.ProdTimeTableID + " " + item.RawProductID + " " + item.SalesID + " " + item.Shift + " " + item.BlocksLogs);
            }

            return lCC;
        }
    }
}
