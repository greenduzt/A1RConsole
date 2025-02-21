using A1RConsole.DB;
using A1RConsole.Models.Machines;
using A1RConsole.Models.Production;
using A1RConsole.Models.Production.Slitting;
using A1RConsole.Models.Products;
using A1RConsole.Models.Shifts;
using MsgBox;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A1RConsole.Core
{
    public class SlittingManager
    {
        private DateTime currentDate;
        private List<ProductionTimeTable> ProdTimeTableDetails;
        private List<Shift> shiftDetails;

        public SlittingManager()
        {
            currentDate = DateTime.Now;
            shiftDetails = DBAccess.GetAllShifts();
        }

        //public int ProcessSlittingOrder(List<SlittingOrder> slittingOrder)
        //{
        //    List<SlittingOrder> slittingOrderList = new List<SlittingOrder>();
        //    BusinessDaysGenerator bdg = new BusinessDaysGenerator();
        //    DateTime lDate = currentDate;
        //    shiftDetails = DBAccess.GetAllShifts();
        //    int machineId = 0;
        //    int result = 0;

        //    //Sort the order by type
        //    slittingOrder = slittingOrder.OrderBy(x => x.Order.OrderType).ToList();

        //    if (slittingOrder != null)
        //    {
        //        foreach (var itemSO in slittingOrder)
        //        {
        //            List<RawProductMachine> rawProductMachine = DBAccess.GetMachineIdByRawProdId(itemSO.Product.RawProduct.RawProductID);
        //            if (rawProductMachine != null || rawProductMachine.Count > 0)
        //            {
        //                machineId = rawProductMachine[0].SlitPeelMachineID;
        //                lDate = currentDate;//Start from the current date
        //                while (itemSO.Qty > 0)
        //                {
        //                    List<DateShift> ShiftDetails = GetShift(lDate, machineId);
        //                    if (ShiftDetails == null)
        //                    {
        //                        do
        //                        {
        //                            lDate = bdg.AddBusinessDays(lDate, 1);
        //                            ShiftDetails = GetShift(lDate, machineId);

        //                        } while (ShiftDetails == null);
        //                    }

        //                    if (ShiftDetails != null || ShiftDetails.Count > 0)
        //                    {

        //                        foreach (var itemSD in ShiftDetails)
        //                        {
        //                            foreach (var itemSL in itemSD.ShiftList)
        //                            {
        //                                if (itemSO.Qty > 0)
        //                                {
        //                                    decimal maxCap = 0;
        //                                    decimal qtyToMake = 0;
        //                                    decimal avaQty = 0;
        //                                    decimal blocksToMake = 0;
        //                                    decimal dollarValToAdd = 0;
        //                                    decimal avaDollarValue = 0;
        //                                    bool addToDb = false;

        //                                    if (itemSO.Order.OrderType == 1)
        //                                    {
        //                                        decimal x = slittingOrderList.Where(s => s.Shift.ShiftID == itemSL.shift && s.ProdTimetableID == itemSL.ProTimeTableID && s.Order.OrderType == itemSO.Order.OrderType).Sum(s => s.DollarValue);
        //                                        decimal y = DBAccess.GetCurrentShiftDollarValueByOrderType(itemSL.ProTimeTableID, itemSL.shift, itemSO.Order.OrderType);
        //                                        decimal curDollarVal = x + y;
        //                                        //Get the max capacity for the shift
        //                                        maxCap = DBAccess.GetSlitPeelMaxCapacity(itemSL.ProTimeTableID, itemSL.shift, machineId);
        //                                        avaDollarValue = (maxCap - curDollarVal) < 0 ? 0 : (maxCap - curDollarVal);
        //                                        avaQty = GetQty(avaDollarValue, itemSO.Product.UnitPrice, itemSO.Product.ProductUnit);
        //                                        decimal avaBlocks = GetBlockLog(avaQty, itemSO.Product.Tile.MaxYield, itemSO.Product.ProductUnit);

        //                                        if (avaQty >= itemSO.Qty && avaDollarValue > 0 && avaBlocks > 0 && itemSO.Qty > 0)
        //                                        {
        //                                            qtyToMake = itemSO.Qty;
        //                                            blocksToMake = GetBlockLog(qtyToMake, itemSO.Product.Tile.MaxYield, itemSO.Product.ProductUnit);
        //                                            dollarValToAdd = qtyToMake * itemSO.Product.UnitPrice;
        //                                            itemSO.Qty -= qtyToMake;
        //                                            addToDb = true;
        //                                        }
        //                                        else if (avaQty < itemSO.Qty && avaDollarValue > 0 && avaBlocks > 0 && itemSO.Qty > 0)
        //                                        {
        //                                            qtyToMake = avaQty;
        //                                            blocksToMake = GetBlockLog(qtyToMake, itemSO.Product.Tile.MaxYield, itemSO.Product.ProductUnit);
        //                                            dollarValToAdd = qtyToMake * itemSO.Product.UnitPrice;
        //                                            itemSO.Qty -= qtyToMake;
        //                                            addToDb = true;
        //                                        }
        //                                    }
        //                                    else
        //                                    {
        //                                        decimal x = slittingOrderList.Where(s => s.Shift.ShiftID == itemSL.shift && s.ProdTimetableID == itemSL.ProTimeTableID).Sum(s => s.DollarValue);
        //                                        decimal y = DBAccess.GetCurrentShiftDollarValue(itemSL.ProTimeTableID, itemSL.shift);
        //                                        decimal curDollarVal = x + y;
        //                                        maxCap = DBAccess.GetSlitPeelMaxCapacity(itemSL.ProTimeTableID, itemSL.shift, machineId);
        //                                        avaDollarValue = (maxCap - curDollarVal) < 0 ? 0 : (maxCap - curDollarVal);
        //                                        avaQty = GetQty(avaDollarValue, itemSO.Product.UnitPrice, itemSO.Product.ProductUnit);
        //                                        decimal avaBlocks = GetBlockLog(avaQty, itemSO.Product.Tile.MaxYield, itemSO.Product.ProductUnit);

        //                                        if (avaQty >= itemSO.Qty && avaDollarValue > 0 && avaBlocks > 0 && itemSO.Qty > 0)
        //                                        {
        //                                            qtyToMake = itemSO.Qty;
        //                                            blocksToMake = GetBlockLog(qtyToMake, itemSO.Product.Tile.MaxYield, itemSO.Product.ProductUnit);
        //                                            dollarValToAdd = qtyToMake * itemSO.Product.UnitPrice;

        //                                            itemSO.Qty -= qtyToMake;
        //                                            addToDb = true;

        //                                        }
        //                                        else if (avaQty < itemSO.Qty && avaDollarValue > 0 && avaBlocks > 0 && itemSO.Qty > 0)
        //                                        {
        //                                            qtyToMake = avaQty;
        //                                            blocksToMake = GetBlockLog(qtyToMake, itemSO.Product.Tile.MaxYield, itemSO.Product.ProductUnit);
        //                                            dollarValToAdd = qtyToMake * itemSO.Product.UnitPrice;

        //                                            itemSO.Qty -= qtyToMake;
        //                                            addToDb = true;
        //                                        }
        //                                    }

        //                                    if (addToDb)
        //                                    {
        //                                        SlittingOrder so = new SlittingOrder();
        //                                        so.ProdTimetableID = itemSL.ProTimeTableID;
        //                                        so.Order = itemSO.Order;
        //                                        so.Product = itemSO.Product;
        //                                        so.Qty = qtyToMake;
        //                                        so.Blocks = blocksToMake;
        //                                        so.DollarValue = dollarValToAdd;
        //                                        so.Shift = new Shift() { ShiftID = itemSL.shift };
        //                                        so.Machine = new Machine() { MachineID = machineId };
        //                                        slittingOrderList.Add(so);
        //                                    }
        //                                }
        //                                else
        //                                {
        //                                    break;
        //                                }

        //                                if (itemSL.shift == 3)
        //                                {
        //                                    lDate = bdg.AddBusinessDays(lDate, 1);
        //                                }
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }

        //        if (slittingOrderList.Count > 0)
        //        {
        //            result = DBAccess.AddToSlitting(slittingOrderList);
        //        }
        //    }

        //    return result;
        //}

        //public List<DateShift> GetShift(DateTime lDate, int machineId)
        //{
        //    List<DateShift> dateShift = new List<DateShift>();
        //    List<ShiftDetails> subShifts = null;

        //    ProdTimeTableDetails = DBAccess.GetProductionTimeTableDetails(machineId, currentDate);
        //    int shift = 0;
        //    int curShift = 0;
        //    int cId = 0;

        //    //Get the current shhift
        //    foreach (var item in shiftDetails)
        //    {
        //        bool isShift = TimeBetween(DateTime.Now, item.StartTime, item.EndTime);

        //        if (isShift == true)
        //        {
        //            curShift = item.ShiftID;
        //        }
        //    }

        //    if (ProdTimeTableDetails.Count == 0 || ProdTimeTableDetails == null)
        //    {
        //        do
        //        {
        //            Production production = new Production();
        //            production.AddNewDates(lDate, false);
        //            ProdTimeTableDetails = DBAccess.GetProductionTimeTableByID(machineId, lDate);

        //        } while (ProdTimeTableDetails.Count == 0);
        //    }

        //    if (ProdTimeTableDetails.Count != 0)
        //    {
        //        cId = ProdTimeTableDetails[0].ID;
        //        foreach (var item in ProdTimeTableDetails)
        //        {
        //            if (item.ProductionDate.Date == lDate.Date)
        //            {
        //                subShifts = new List<ShiftDetails>();
        //                if (item.IsMachineActive == true)
        //                {
        //                    shift = 0;
        //                    if (item.IsDayShiftActive == true)
        //                    {

        //                        if (item.ID == cId && curShift <= 1)
        //                        {
        //                            goto a;
        //                        }
        //                        else if (item.ID == cId && curShift > 1)
        //                        {
        //                            goto b;
        //                        }
        //                        else
        //                        {
        //                            goto a;
        //                        }
        //                    a: ;

        //                        shift = 1;
        //                        subShifts.Add(new ShiftDetails() { ProTimeTableID = item.ID, shift = shift });
        //                    }
        //                b: ;
        //                    if (item.IsEveningShiftActive == true)
        //                    {
        //                        if (item.ID == cId && curShift <= 2)
        //                        {
        //                            goto c;
        //                        }
        //                        else if (item.ID == cId && curShift > 2)
        //                        {
        //                            goto d;
        //                        }
        //                        else
        //                        {
        //                            goto c;
        //                        }
        //                    c: ;

        //                        shift = 2;
        //                        subShifts.Add(new ShiftDetails() { ProTimeTableID = item.ID, shift = shift });
        //                    }
        //                d: ;
        //                    if (item.IsNightShiftActive == true)
        //                    {
        //                        if (item.ID == cId && curShift <= 3)
        //                        {
        //                            goto e;
        //                        }
        //                        else if (item.ID == cId && curShift > 3)
        //                        {
        //                            goto f;
        //                        }
        //                        else
        //                        {
        //                            goto e;
        //                        }
        //                    e: ;
        //                        shift = 3;
        //                        subShifts.Add(new ShiftDetails() { ProTimeTableID = item.ID, shift = shift });
        //                    }
        //                f: ;
        //                    if (item.IsDayShiftActive == false && item.IsEveningShiftActive == false && item.IsNightShiftActive == false)
        //                    {
        //                        Console.WriteLine("Slitting shifts not available - " + lDate);
        //                        dateShift = null;
        //                    }
        //                }
        //                else
        //                {
        //                    Console.WriteLine("Slitting Machine is switched off - " + lDate);
        //                    dateShift = null;
        //                }

        //                if (shift != 0)
        //                {
        //                    DateShift ds = new DateShift();
        //                    ds.ProdDate = lDate;
        //                    ds.ShiftList = subShifts;
        //                    dateShift.Add(ds);
        //                }
        //                else
        //                {
        //                    Console.WriteLine("No shifts found - " + lDate);
        //                    dateShift = null;
        //                }
        //            }
        //        }
        //    }

        //    return dateShift;
        //}


        //bool TimeBetween(DateTime datetime, TimeSpan start, TimeSpan end)
        //{
        //    // convert datetime to a TimeSpan
        //    TimeSpan now = datetime.TimeOfDay;
        //    // see if start comes before end
        //    if (start < end)
        //        return start <= now && now <= end;
        //    // start is after end, so do the inverse comparison
        //    return !(end < now && now < start);
        //}

        //private decimal GetQty(decimal dolVal, decimal prodPrice, string prodUnit)
        //{
        //    decimal amount = 0;
        //    amount = dolVal / prodPrice;


        //    if (prodUnit == "EA" || prodUnit == "TILE")
        //    {
        //        amount = Math.Floor(amount);

        //    }

        //    return amount;
        //}


        //private decimal GetBlockLog(decimal qty, decimal maxYield, string prodUnit)
        //{
        //    decimal amount = 0;
        //    amount = qty / maxYield;


        //    if (prodUnit == "EA" || prodUnit == "TILE")
        //    {
        //        amount = Math.Ceiling(amount);
        //        //if (amount == 0)
        //        //{
        //        //    amount = 1;
        //        //}
        //    }

        //    return amount;
        //}


        //public Tuple<decimal, decimal, ProductionTimeTable, decimal, decimal> CalculateDestinationRemainingDollarValue(SlittingOrder so, DateTime selectedDate, int selectedShift, decimal qty)
        //{
        //    decimal mBlocks = 0;
        //    decimal mDollars = 0;
        //    decimal desDollarValue = 0;
        //    decimal desMaxDollarValue = 0;
        //    decimal desRemainingDollarValue = 0;
        //    decimal mTiles = 0;
        //    string updateType = string.Empty;
        //    List<ProductionTimeTable> destProdTimeTable = new List<ProductionTimeTable>();

        //    mBlocks = qty;
        //    mTiles = mBlocks * so.Product.Tile.MaxYield;
        //    mDollars = mTiles * so.Product.UnitPrice;

        //    //Calculate destination capacity
        //    destProdTimeTable = DBAccess.GetProductionTimeTableByID(so.Machine.MachineID, selectedDate);
        //    desMaxDollarValue = DBAccess.GetSlittingDollarValue(destProdTimeTable[0], so.Machine.MachineID, selectedShift);
        //    desDollarValue = DBAccess.GetSlittingDollarValueForShift(destProdTimeTable[0], selectedShift);
        //    desRemainingDollarValue = (desMaxDollarValue - desDollarValue) < 0 ? 0 : desMaxDollarValue - desDollarValue;

        //    return Tuple.Create(mDollars, desRemainingDollarValue, destProdTimeTable[0], mTiles, mBlocks);
        //}


        //public Tuple<decimal, decimal, decimal> CalculateOriginQuantities(SlittingOrder so, decimal qty)
        //{
        //    decimal originQty = 0;
        //    decimal originBlocks = 0;
        //    decimal originDollarValue = 0;

        //    originBlocks = so.Blocks - qty;
        //    originQty = originBlocks * so.Product.Tile.MaxYield;
        //    originDollarValue = originQty * so.Product.UnitPrice;

        //    return Tuple.Create(originBlocks, originQty, originDollarValue);
        //}

        //public List<SlittingOrder> SeparateReleventSlittingOrders(ObservableCollection<SlittingOrder> soList, int shift, int machineID)
        //{
        //    List<SlittingOrder> slittingOrder = new List<SlittingOrder>();

        //    foreach (var item in soList)
        //    {
        //        if (shift == 0 && item.Machine.MachineID == machineID)
        //        {
        //            slittingOrder.Add(item);
        //        }
        //        else if (item.Shift.ShiftID == shift && item.Machine.MachineID == machineID)
        //        {
        //            slittingOrder.Add(item);
        //        }
        //    }

        //    return slittingOrder;
        //}

        //public void ShiftMultipleOrders(List<SlittingOrder> soList, int machineId)
        //{
        //    BusinessDaysGenerator bdg = new BusinessDaysGenerator();
        //    SlittingManager sm = new SlittingManager();
        //    DateTime lDate = DateTime.Now;
        //    List<DateShift> ShiftDetails = sm.GetShift(DateTime.Now, machineId);

        //    do
        //    {
        //        lDate = bdg.AddBusinessDays(lDate, 1);
        //        ShiftDetails = sm.GetShift(lDate, machineId);


        //    } while (ShiftDetails == null);

        //    //Get the first available shift
        //    var firstElement = ShiftDetails[0].ShiftList.First();
        //    DBAccess.MoveMultipleSlittingOrders(soList, firstElement.shift, firstElement.ProTimeTableID);
        //}

        //public void BackupDeleteAdd(List<SlittingOrder> slittingOrderList)
        //{
        //    //Backup and delete slitting orders
        //    int insResult = 0;
        //    int res = DBAccess.BackupDeleteSlittingOrders(slittingOrderList, "Disabling Slitting Shift");
        //    if (res == 1)
        //    {
        //        //Transfer orders in to next shift
        //        insResult = ProductionManager.AddToSlitting(slittingOrderList);
        //    }
        //    else if (res == -1)
        //    {
        //        //Msg.Show("Backup was unsuccessful." + System.Environment.NewLine + "Please try again in few minutes ", "Backup Unsuccessful", MsgBoxButtons.OK, MsgBoxImage.Information_Red, MsgBoxResult.Yes);
        //    }

        //    else if (res == -2)
        //    {
        //        Msg.Show("Backup was successful but could not delete the orders." + System.Environment.NewLine + "Please try again in few minutes ", "System Error Occured", MsgBoxButtons.OK, MsgBoxImage.Information_Red, MsgBoxResult.Yes);
        //    }

        //    else
        //    {
        //        Msg.Show("Unknown error occured while disabling shift." + System.Environment.NewLine + "Please try again in few minutes ", "System Error Occured", MsgBoxButtons.OK, MsgBoxImage.Information_Red, MsgBoxResult.Yes);
        //    }

        //    //Rollback data
        //    if (res != 1 || insResult == 0)
        //    {

        //    }

        //}
    }
}
