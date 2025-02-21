using A1RConsole.DB;
using A1RConsole.Models.Production;
using A1RConsole.Models.Production.Peeling;
using A1RConsole.Models.Products;
using A1RConsole.Models.Shifts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A1RConsole.Core
{
    public class PeelingManager
    {
        private DateTime currentDate;
        private List<ProductionTimeTable> ProdTimeTableDetails;
        private List<Shift> shiftDetails;
        private List<ProductMeterage> prodMeterageList;

        public PeelingManager()
        {
            currentDate = DateTime.Now;
            prodMeterageList = new List<ProductMeterage>();
            prodMeterageList = DBAccess.GetProductMeterage();
        }

        public int ProcessPeelingOrder(List<PeelingOrder> peelingOrder)
        {
            List<PeelingOrder> peelingOrderList = new List<PeelingOrder>();
            BusinessDaysGenerator bdg = new BusinessDaysGenerator();
            DateTime lDate = currentDate;
            shiftDetails = DBAccess.GetAllShifts();
            int machineId = 0;
            int result = 0;

            if (peelingOrder != null)
            {
                foreach (var itemPO in peelingOrder)
                {
                    List<RawProductMachine> rawProductMachine = DBAccess.GetMachineIdByRawProdId(itemPO.Product.RawProduct.RawProductID);
                    if (rawProductMachine != null || rawProductMachine.Count > 0)
                    {
                        machineId = rawProductMachine[0].SlitPeelMachineID;
                        lDate = currentDate;//Start from the current date
                        while (itemPO.Logs > 0)
                        {
                            List<DateShift> ShiftDetails = GetShift(lDate, machineId);
                            if (ShiftDetails == null)
                            {
                                do
                                {
                                    lDate = bdg.AddBusinessDays(lDate, 1);
                                    ShiftDetails = GetShift(lDate, machineId);

                                } while (ShiftDetails == null);
                            }

                            if (ShiftDetails != null || ShiftDetails.Count > 0)
                            {
                                foreach (var itemSD in ShiftDetails)
                                {
                                    foreach (var itemSL in itemSD.ShiftList)
                                    {
                                        if (itemPO.Logs > 0)
                                        {
                                            decimal x = peelingOrderList.Where(s => s.Shift.ShiftID == itemSL.shift && s.ProdTimetableID == itemSL.ProTimeTableID).Sum(s => s.DollarValue);
                                            decimal y = DBAccess.GetPeelingCurrentShiftDollarValue(itemSL.ProTimeTableID, itemSL.shift);
                                            decimal curDollarVal = x + y;
                                            decimal maxCap = DBAccess.GetPeelingMaxCapacity(itemSL.ProTimeTableID, itemSL.shift, machineId);
                                            decimal avaDollarValue = (maxCap - curDollarVal) < 0 ? 0 : (maxCap - curDollarVal);
                                            decimal avaBlocks = GetLog(avaDollarValue, itemPO.Product.UnitPrice, itemPO.Product.Type);
                                            decimal logsToMake = 0;
                                            bool addToDb = false;

                                            if (avaBlocks >= itemPO.Logs && avaDollarValue > 0 && avaBlocks > 0)
                                            {
                                                logsToMake = itemPO.Logs;
                                                itemPO.Logs -= logsToMake;
                                                addToDb = true;
                                            }
                                            else if (avaBlocks < itemPO.Logs && avaDollarValue > 0 && avaBlocks > 0)
                                            {
                                                logsToMake = avaBlocks;
                                                itemPO.Logs -= logsToMake;
                                                addToDb = true;
                                            }

                                            if (addToDb)
                                            {
                                                PeelingOrder po = new PeelingOrder();
                                                po.ProdTimetableID = itemSL.ProTimeTableID;
                                                po.Order = itemPO.Order;
                                                po.Product = itemPO.Product;
                                                po.Logs = logsToMake;
                                                po.Qty = GetQty(itemPO.Product, logsToMake);
                                                po.DollarValue = itemPO.Product.UnitPrice * po.Qty;
                                                po.Shift = new Shift() { ShiftID = itemSL.shift };
                                                po.IsReRollingReq = itemPO.IsReRollingReq;
                                                peelingOrderList.Add(po);
                                            }
                                        }
                                        else
                                        {
                                            break;
                                        }

                                        if (itemSL.shift == 3)
                                        {
                                            lDate = bdg.AddBusinessDays(lDate, 1);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (peelingOrderList.Count > 0)
                {
                    result = DBAccess.AddToPeeling(peelingOrderList);
                }
            }

            return result;
        }

        private decimal GetLog(decimal avaDolVal, decimal prodPrice, string type)
        {
            decimal logs = 0;
            logs = avaDolVal / prodPrice;


            if (type == "Bulk" || type == "Roll" || type == "Standard")
            {
                logs = Math.Floor(logs);
            }
            return logs;
        }

        private decimal GetQty(Product product, decimal logs)
        {
            decimal qty = 0;

            if (product.Type == "Roll" || product.Type == "Standard")
            {
                var prodM = prodMeterageList.FirstOrDefault(x => x.Thickness == product.Tile.Thickness && x.MouldSize == product.Width && x.MouldType == product.MouldType);
                qty = Math.Floor((prodM.ExpectedYield / product.Tile.MaxYield) * logs);
            }
            else if (product.Type == "Bulk")
            {
                qty = logs;
            }

            return qty;
        }

        private List<DateShift> GetShift(DateTime lDate, int machineId)
        {
            List<DateShift> dateShift = new List<DateShift>();
            List<ShiftDetails> subShifts = null;
            ProdTimeTableDetails = DBAccess.GetProductionTimeTableDetails(machineId, currentDate);
            int shift = 0;
            int curShift = 0;
            int cId = 0;

            //Get the current shhift
            foreach (var item in shiftDetails)
            {
                bool isShift = TimeBetween(DateTime.Now, item.StartTime, item.EndTime);

                if (isShift == true)
                {
                    curShift = item.ShiftID;
                }
            }

            if (ProdTimeTableDetails.Count == 0 || ProdTimeTableDetails == null)
            {
                do
                {
                    Production production = new Production();
                    production.AddNewDates(lDate, false);
                    ProdTimeTableDetails = DBAccess.GetProductionTimeTableByID(machineId, lDate);

                } while (ProdTimeTableDetails.Count == 0);
            }

            if (ProdTimeTableDetails.Count != 0)
            {
                cId = ProdTimeTableDetails[0].ID;
                foreach (var item in ProdTimeTableDetails)
                {
                    if (item.ProductionDate.Date == lDate.Date)
                    {
                        subShifts = new List<ShiftDetails>();
                        if (item.IsMachineActive == true)
                        {
                            shift = 0;
                            if (item.IsDayShiftActive == true)
                            {

                                if (item.ID == cId && curShift <= 1)
                                {
                                    goto a;
                                }
                                else if (item.ID == cId && curShift > 1)
                                {
                                    goto b;
                                }
                                else
                                {
                                    goto a;
                                }
                            a: ;

                                shift = 1;
                                subShifts.Add(new ShiftDetails() { ProTimeTableID = item.ID, shift = shift });
                            }
                        b: ;
                            if (item.IsEveningShiftActive == true)
                            {
                                if (item.ID == cId && curShift <= 2)
                                {
                                    goto c;
                                }
                                else if (item.ID == cId && curShift > 2)
                                {
                                    goto d;
                                }
                                else
                                {
                                    goto c;
                                }
                            c: ;

                                shift = 2;
                                subShifts.Add(new ShiftDetails() { ProTimeTableID = item.ID, shift = shift });
                            }
                        d: ;
                            if (item.IsNightShiftActive == true)
                            {
                                if (item.ID == cId && curShift <= 3)
                                {
                                    goto e;
                                }
                                else if (item.ID == cId && curShift > 3)
                                {
                                    goto f;
                                }
                                else
                                {
                                    goto e;
                                }
                            e: ;
                                shift = 3;
                                subShifts.Add(new ShiftDetails() { ProTimeTableID = item.ID, shift = shift });
                            }
                        f: ;
                            if (item.IsDayShiftActive == false && item.IsEveningShiftActive == false && item.IsNightShiftActive == false)
                            {
                                Console.WriteLine("Peeling shifts not available - " + lDate);
                                dateShift = null;
                            }

                        }
                        else
                        {
                            Console.WriteLine("Peeling Machine is switched off - " + lDate);
                            dateShift = null;
                        }

                        if (shift != 0)
                        {
                            DateShift ds = new DateShift();
                            ds.ProdDate = lDate;
                            ds.ShiftList = subShifts;
                            dateShift.Add(ds);
                        }
                        else
                        {
                            Console.WriteLine("No shifts found - " + lDate);
                            dateShift = null;
                        }
                    }
                }
            }
            return dateShift;
        }

        bool TimeBetween(DateTime datetime, TimeSpan start, TimeSpan end)
        {
            // convert datetime to a TimeSpan
            TimeSpan now = datetime.TimeOfDay;
            // see if start comes before end
            if (start < end)
                return start <= now && now <= end;
            // start is after end, so do the inverse comparison
            return !(end < now && now < start);
        }
    }
}
