using A1RConsole.DB;
using A1RConsole.Models.Production;
using A1RConsole.Models.Production.ReRolling;
using A1RConsole.Models.Products;
using A1RConsole.Models.Shifts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A1RConsole.Core
{
    public class ReRollingManager
    {
        private DateTime currentDate;
        private List<ProductionTimeTable> ProdTimeTableDetails;
        private List<Shift> shiftDetails;

        public ReRollingManager()
        {
            currentDate = DateTime.Now;
        }

        public int ProcessReRollingOrder(ReRollingOrder reRollingOrder)
        {
            List<ReRollingOrder> reRollingOrderList = new List<ReRollingOrder>();
            BusinessDaysGenerator bdg = new BusinessDaysGenerator();
            DateTime lDate = currentDate;
            shiftDetails = DBAccess.GetAllShifts();
            int machineId = 0;
            int result = 0;

            if (reRollingOrder != null)
            {
                List<RawProductMachine> rawProductMachine = DBAccess.GetMachineIdByRawProdId(reRollingOrder.Product.RawProduct.RawProductID);
                if (rawProductMachine != null || rawProductMachine.Count > 0)
                {
                    machineId = rawProductMachine[0].ReRollingMachineID;
                    lDate = currentDate;//Start from the current date
                    while (reRollingOrder.Rolls > 0)
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
                                    if (reRollingOrder.Rolls > 0)
                                    {
                                        decimal x = reRollingOrderList.Where(s => s.Shift.ShiftID == itemSL.shift && s.ProdTimetableID == itemSL.ProTimeTableID).Sum(s => s.DollarValue);
                                        decimal y = DBAccess.GetReRollingCurrentShiftDollarValue(itemSL.ProTimeTableID, itemSL.shift);
                                        decimal curDollarVal = x + y;
                                        decimal maxCap = DBAccess.GetReRollingMaxCapacity(itemSL.ProTimeTableID, itemSL.shift, machineId);
                                        decimal avaDollarValue = (maxCap - curDollarVal) < 0 ? 0 : (maxCap - curDollarVal);
                                        decimal avaQty = GetQty(avaDollarValue, reRollingOrder.Product.UnitPrice, reRollingOrder.Product.ProductUnit);
                                        decimal avaRolls = avaQty;
                                        decimal rollsToMake = 0;
                                        decimal dollarValToAdd = 0;
                                        bool addToDb = false;

                                        if (avaRolls >= reRollingOrder.Rolls && avaDollarValue > 0 && avaRolls > 0)
                                        {
                                            rollsToMake = reRollingOrder.Rolls;
                                            dollarValToAdd = rollsToMake * reRollingOrder.Product.UnitPrice;
                                            reRollingOrder.Rolls -= rollsToMake;
                                            addToDb = true;
                                        }
                                        else if (avaRolls < reRollingOrder.Rolls && avaDollarValue > 0 && avaRolls > 0)
                                        {
                                            rollsToMake = avaRolls;
                                            dollarValToAdd = rollsToMake * reRollingOrder.Product.UnitPrice;
                                            reRollingOrder.Rolls -= rollsToMake;
                                            addToDb = true;
                                        }

                                        if (addToDb)
                                        {
                                            ReRollingOrder rro = new ReRollingOrder();
                                            rro.ProdTimetableID = itemSL.ProTimeTableID;
                                            rro.Order = reRollingOrder.Order;
                                            rro.Product = reRollingOrder.Product;
                                            rro.Rolls = rollsToMake;
                                            rro.DollarValue = dollarValToAdd;
                                            rro.Shift = new Shift() { ShiftID = itemSL.shift };
                                            reRollingOrderList.Add(rro);
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


                if (reRollingOrderList.Count > 0)
                {
                    result = DBAccess.AddToReRolling(reRollingOrderList);
                }
            }

            return result;

        }


        private decimal GetQty(decimal dolVal, decimal prodPrice, string prodUnit)
        {
            decimal amount = 0;
            amount = dolVal / prodPrice;


            if (prodUnit == "ROLL")
            {
                amount = Math.Floor(amount);

            }
            return amount;
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
                                Console.WriteLine("ReRolling shift not available - " + lDate);
                                dateShift = null;
                            }

                        }
                        else
                        {
                            Console.WriteLine("ReRolling Machine is switched off - " + lDate);
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
