using A1RConsole.DB;
using A1RConsole.Models.Capacity;
using A1RConsole.Models.Formulas;
using A1RConsole.Models.Production;
using A1RConsole.Models.Production.Grading;
using A1RConsole.Models.Products;
using A1RConsole.Models.Shifts;
using A1RConsole.Models.Stock;
using MsgBox;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A1RConsole.Core
{
    public class GradingManager
    {
        public List<ProductionTimeTable> shiftList { get; set; }
        public ObservableCollection<ProductCapacity> CapacityLimitations { get; set; }

        private List<ProductionTimeTable> ProductionTimeTable;
        private ObservableCollection<Formula> formulaList = null;
        private List<CurrentCapacity> currentCapacities = null;
        private List<GradingScheduling> gradingScheduling = null;
        private DateTime CurrentDate;
        private List<Shift> shiftDetails;
        private DateTime ProductionDate;
        private ObservableCollection<Formula> FormulaColl;
        private ChildWindow LoadingScreen;
        private List<Tuple<Int32, DateTime, int, int>> expectedDeliveryInfo;
        private List<GradedStock> GradedStock;
        private List<ShredStock> ShredStock;
        private decimal blCounter = 0;
        private List<ProductionTimeTable> MixingTimeTableDetails;
        private ObservableCollection<CurrentCapacity> CurrentCapacities;

        public GradingManager()
        {
            CurrentCapacities = new ObservableCollection<CurrentCapacity>();
            expectedDeliveryInfo = new List<Tuple<Int32, DateTime, int, int>>();
            CurrentDate = Convert.ToDateTime(NTPServer.GetNetworkTime().ToString("dd/MM/yyyy"));
            FormulaColl = DBAccess.GetFormulas();
            shiftDetails = DBAccess.GetAllShifts();
        }

        public int AddToGrading(List<GradingOrder> gradingOrder)
        {
            return CreateProductionOrder(gradingOrder);
        }

        private int CreateProductionOrder(List<GradingOrder> gradingOrder)
        {
            decimal amount1 = 0;
            decimal amount2 = 0;
            decimal gradingWeight1 = 0;
            decimal gradingWeight2 = 0;
            int result1 = 0;
            int rawProdId1 = 0;
            int rawProdId2 = 0;
            int prodId1 = 0;
            int prodId2 = 0;
            int res = 0;
            //int curShift = 0;
            bool dateHas = false;

            expectedDeliveryInfo = new List<Tuple<Int32, DateTime, int, int>>();
            ProductionDate = gradingOrder[0].Order.RequiredDate;

            if (gradingOrder != null)
            {
                if (shiftDetails != null)
                {
                    if (FormulaColl != null)
                    {
                        dateHas = CheckDateAvailable(ProductionDate);
                        if (dateHas == true)
                        {
                            /*************SENDING TO GRADING PRODUCTION*****************/

                            foreach (var item in gradingOrder)
                            {
                                foreach (var items in FormulaColl)
                                {
                                    if (item.Product.RawProduct.RawProductID == items.RawProductID)
                                    {
                                        List<RawProductMachine> rawProductMachineList = DBAccess.GetMachineIdByRawProdId(item.Product.RawProduct.RawProductID);
                                        int mixingMachineID = 0;
                                        foreach (var itemRPML in rawProductMachineList)
                                        {
                                            mixingMachineID = itemRPML.MixingMachineID;
                                        }

                                        if (items.ProductCapacity1 != 0)
                                        {
                                            decimal tempAmount = 0;
                                            if (item.Product.Type == "Tile")
                                            {
                                                tempAmount = CalcBlkLog(item.Qty, item.Product.Tile.MaxYield) * items.GradingWeight1;
                                            }
                                            else if (item.Product.Type == "Bulk")
                                            {
                                                tempAmount = item.Qty * items.GradingWeight1;
                                            }
                                            else if (item.Product.Type == "Standard" || item.Product.Type == "Roll" || item.Product.Type == "Block" || item.Product.Type == "Log" || item.Product.Type == "Curvedge" || item.Product.Type == "Box" || item.Product.Type == "BoxPallet" || item.Product.Type == "Pallet")
                                            {
                                                tempAmount = item.BlocksLogs * items.GradingWeight1;
                                            }

                                            amount1 = tempAmount;
                                            rawProdId1 = item.Product.RawProduct.RawProductID;
                                            prodId1 = 0;
                                            gradingWeight1 = items.GradingWeight1;
                                        }
                                        else
                                        {
                                            amount1 = 0;
                                            rawProdId1 = 0;
                                            prodId1 = 0;
                                            gradingWeight1 = 0;
                                        }
                                        if (items.ProductCapacity2 != 0)
                                        {
                                            decimal tempAmount = 0;
                                            if (item.Product.Type == "Tile")
                                            {
                                                tempAmount = CalcBlkLog(item.Qty, item.Product.Tile.MaxYield) * items.GradingWeight2;
                                            }
                                            else if (item.Product.Type == "Bulk")
                                            {
                                                tempAmount = item.Qty * items.GradingWeight2;
                                            }
                                            else if (item.Product.Type == "Standard" || item.Product.Type == "Roll" || item.Product.Type == "Block" || item.Product.Type == "Log" || item.Product.Type == "Curvedge" || item.Product.Type == "Box" || item.Product.Type == "BoxPallet" || item.Product.Type == "Pallet")
                                            {
                                                tempAmount = item.BlocksLogs * items.GradingWeight2;
                                            }

                                            amount2 = tempAmount;
                                            rawProdId2 = item.Product.RawProduct.RawProductID;
                                            prodId2 = 0;
                                            gradingWeight2 = items.GradingWeight2;
                                        }
                                        else
                                        {
                                            amount2 = 0;
                                            rawProdId2 = 0;
                                            prodId2 = 0;
                                            gradingWeight2 = 0;
                                        }

                                        CalculateGradingCapacity(rawProdId1, prodId1, items.ProductCapacity1, gradingWeight1, amount1, rawProdId2, prodId2, items.ProductCapacity2, gradingWeight2, amount2, item.Order.OrderType, mixingMachineID);

                                        break;
                                    }
                                }

                                if (CurrentCapacities.Count != 0)
                                {
                                    //JOIN GRADES AND CREATE BLOCKS TO BE MADE
                                    KGToBlocksCreator kGToBlocksCreator = new KGToBlocksCreator(CurrentCapacities, FormulaColl, item.Order.OrderNo, item.BlocksLogs);
                                    List<CurrentCapacity> GradingList = kGToBlocksCreator.CreateBlocks();
                                    //ADD CAPACITIES TO THE DB
                                    result1 = DBAccess.AllocateCapacity(CurrentCapacities, GradingList, item.Order.OrderNo, item.Order.SalesNo);
                                    foreach (var itemGL in GradingList)
                                    {
                                        expectedDeliveryInfo.Add(Tuple.Create(itemGL.ProdTimeTableID, itemGL.GradingDate, itemGL.Shift, itemGL.RawProductID));
                                    }
                                    CurrentCapacities.Clear();
                                    GradingList.Clear();
                                }
                            }

                            if (result1 != 0)
                            {
                                res = 1;
                            }
                            else
                            {
                                res = 0;
                            }
                        }
                        else
                        {
                            Console.WriteLine("Date not available......");
                        }
                    }
                }
            }
            return res;
        }

        private string CheckGradedOrShred(decimal prodCap1, decimal prodCap2)
        {
            string stock = string.Empty;
            bool ss = ShredStock.Any(x => x.Shred.ID == prodCap1 || x.Shred.ID == prodCap2);
            if (ss == true)
            {
                stock = "shred";
            }
            else if (ss == false)
            {
                bool gs = GradedStock.Any(x => x.ID == prodCap1 || x.ID == prodCap2);
                if (gs == true)
                {
                    stock = "graded";
                }
            }
            return stock;
        }


        private void CalculateGradingCapacity(int rawProdId1, int prodId1, int prodCap1, decimal gradingWeight1, decimal amount1, int rawProdId2, int prodId2, int prodCap2, decimal gradingWeight2, decimal amount2, int orderTypeId, int mixingMachineId)
        {
            List<DateShift> dateShift = new List<DateShift>();
            BusinessDaysGenerator bdg = new BusinessDaysGenerator();
            List<GradedStock> gsList = new List<GradedStock>();
            List<ShredStock> ssList = new List<ShredStock>();
            GradingManager gradingManager = new GradingManager();
            DateTime lDate = ProductionDate;
            DateTime mDate = CurrentDate;
            decimal maxBL1 = 0;
            decimal maxBL2 = 0;
            decimal maxCap1 = 0;
            decimal maxCap2 = 0;
            decimal minBlockLog = 0;
            decimal graded1 = 0;
            decimal graded2 = 0;
            int curShift = 0;
            bool isGradedEx = false;

            DateTime dCheck = bdg.AddBusinessDays(CurrentDate, 5);
            //Check date available if not create date
            bool b = CheckDateAvailable(dCheck);

            LoadGradedStock();
            LoadShredStock();

            //Check if it is gradedstock or regrinding
            //ShredStock
            foreach (var itemSS in ShredStock)
            {
                if (itemSS.Shred.ID == prodCap1)
                {
                    graded1 = itemSS.Qty;
                }

                if (itemSS.Shred.ID == prodCap2)
                {
                    graded2 = itemSS.Qty;
                }
            }

            if (graded1 == 0 && graded2 == 0)
            {
                //GradedStock
                foreach (var itemGS in GradedStock)
                {
                    if (itemGS.ID == prodCap1)
                    {
                        graded1 = itemGS.Qty;
                    }

                    if (itemGS.ID == prodCap2)
                    {
                        graded2 = itemGS.Qty;
                    }
                }
            }

            //Get the current shhift
            foreach (var item in shiftDetails)
            {
                bool isShift = TimeBetween(DateTime.Now, item.StartTime, item.EndTime);

                if (isShift == true)
                {
                    curShift = item.ShiftID;
                }
            }

            if (amount1 != 0 && amount2 != 0)
            {
                while (amount1 > 0)
                {
                    while (amount2 > 0)
                    {
                        dateShift = gradingManager.GetShift(lDate, prodCap1, gradingWeight1, orderTypeId);
                        if (dateShift == null)
                        {
                            do
                            {
                                lDate = bdg.AddBusinessDays(lDate, 1);
                                dateShift = gradingManager.GetShift(lDate, prodCap1, gradingWeight1, orderTypeId);

                            } while (dateShift == null);
                        }

                        foreach (var item in dateShift)
                        {
                            lDate = item.ProdDate;
                            dateShift = gradingManager.GetShift(lDate, prodCap2, gradingWeight2, orderTypeId);
                            if (dateShift == null)
                            {
                                do
                                {
                                    lDate = bdg.AddBusinessDays(lDate, 1);
                                    dateShift = gradingManager.GetShift(lDate, prodCap2, gradingWeight2, orderTypeId);

                                } while (dateShift == null);
                            }
                            foreach (var item2 in dateShift)
                            {
                                foreach (var items in item.ShiftList)
                                {
                                    foreach (var items2 in item2.ShiftList)
                                    {

                                        if (items.ProTimeTableID == items2.ProTimeTableID && items.shift == items2.shift && amount1 > 0 && amount2 > 0)
                                        {
                                            if (graded1 == 0 || graded2 == 0)//If no graded stock
                                            {
                                                isGradedEx = true;
                                            }

                                            /************* NORMAL GRADED/URGENT GRADED ORDER **************/

                                            if ((orderTypeId == 1 && isGradedEx == false))
                                            {
                                                int oType = 0;
                                                decimal totMixesLeft = CalTotMixesLeft(mixingMachineId, lDate, blCounter);
                                                if (orderTypeId == 1)
                                                {
                                                    oType = 2;
                                                }
                                                else if (orderTypeId == 3)
                                                {
                                                    oType = 4;
                                                }
                                                if (totMixesLeft < 0)
                                                {
                                                    totMixesLeft = 0;
                                                }
                                                if (totMixesLeft > 0)
                                                {
                                                    MixingTimeTableDetails = DBAccess.GetProductionTimeTableDetails(mixingMachineId, lDate);
                                                    if (MixingTimeTableDetails.Count != 0)
                                                    {
                                                        List<DateShift> MShiftDetails = GetMixingShift(lDate);
                                                        foreach (var itemSD in MShiftDetails)
                                                        {
                                                            foreach (var itemSL in itemSD.ShiftList)
                                                            {
                                                                if (graded1 > 0 && graded2 > 0 && amount1 > 0 && amount2 > 0 && totMixesLeft > 0)
                                                                {
                                                                    if ((amount1 > graded1) && (amount2 > graded2))
                                                                    {
                                                                        maxBL1 = decimal.Floor(graded1 / gradingWeight1);
                                                                        maxBL2 = decimal.Floor(graded2 / gradingWeight2);

                                                                        if (maxBL1 > 0 && maxBL2 > 0)
                                                                        {
                                                                            if (maxBL1 <= maxBL2)
                                                                            {
                                                                                minBlockLog = maxBL1;
                                                                            }
                                                                            else
                                                                            {
                                                                                minBlockLog = maxBL2;
                                                                            }

                                                                            if (minBlockLog > totMixesLeft)//Checking mixing capacity
                                                                            {
                                                                                minBlockLog = totMixesLeft;
                                                                            }
                                                                            maxCap1 = gradingWeight1 * minBlockLog;
                                                                            maxCap2 = gradingWeight2 * minBlockLog;

                                                                            if ((graded1 >= maxCap1) && (graded2 >= maxCap2))
                                                                            {
                                                                                amount1 = amount1 - maxCap1;
                                                                                amount2 = amount2 - maxCap2;
                                                                                graded1 = graded1 - maxCap1;
                                                                                graded2 = graded2 - maxCap2;

                                                                                string stock = CheckGradedOrShred(prodCap1, prodCap2);
                                                                                if (stock == "graded")
                                                                                {
                                                                                    gsList.Add(new GradedStock() { ID = prodCap1, Qty = maxCap1 });
                                                                                    gsList.Add(new GradedStock() { ID = prodCap2, Qty = maxCap2 });
                                                                                }
                                                                                else if (stock == "shred")
                                                                                {
                                                                                    ssList.Add(new ShredStock() { Shred = new Shred() { ID = prodCap1 }, Qty = maxCap1 });
                                                                                    ssList.Add(new ShredStock() { Shred = new Shred() { ID = prodCap2 }, Qty = maxCap2 });
                                                                                }
                                                                                CurrentCapacities.Add(new CurrentCapacity() { ProdTimeTableID = itemSL.ProTimeTableID, GradingDate = itemSL.ProdDate, ProductCapacityID = items.GradingID, ProductID = prodId1, RawProductID = rawProdId1, CapacityKG = maxCap1, Shift = curShift, BlocksLogs = minBlockLog, Paired = false, OrderType = oType });
                                                                                CurrentCapacities.Add(new CurrentCapacity() { ProdTimeTableID = itemSL.ProTimeTableID, GradingDate = itemSL.ProdDate, ProductCapacityID = items2.GradingID, ProductID = prodId2, RawProductID = rawProdId2, CapacityKG = maxCap2, Shift = curShift, BlocksLogs = minBlockLog, Paired = false, OrderType = oType });
                                                                                //blCounter += minBlockLog;
                                                                            }
                                                                        }
                                                                    }
                                                                    else if ((amount1 <= graded1) && (amount2 <= graded2))
                                                                    {
                                                                        maxBL1 = decimal.Floor(amount1 / gradingWeight1);
                                                                        maxBL2 = decimal.Floor(amount2 / gradingWeight2);
                                                                        if (maxBL1 > 0 && maxBL2 > 0)
                                                                        {
                                                                            if (maxBL1 <= maxBL2)
                                                                            {
                                                                                minBlockLog = maxBL1;
                                                                            }
                                                                            else
                                                                            {
                                                                                minBlockLog = maxBL2;
                                                                            }

                                                                            if (minBlockLog > totMixesLeft)//Checking mixing capacity
                                                                            {
                                                                                minBlockLog = totMixesLeft;
                                                                            }

                                                                            maxCap1 = gradingWeight1 * minBlockLog;
                                                                            maxCap2 = gradingWeight2 * minBlockLog;

                                                                            if ((graded1 >= maxCap1) && (graded2 >= maxCap2))
                                                                            {
                                                                                amount1 = amount1 - maxCap1;
                                                                                amount2 = amount2 - maxCap2;
                                                                                graded1 = graded1 - maxCap1;
                                                                                graded2 = graded2 - maxCap2;

                                                                                string stock = CheckGradedOrShred(prodCap1, prodCap2);
                                                                                if (stock == "graded")
                                                                                {
                                                                                    gsList.Add(new GradedStock() { ID = prodCap1, Qty = maxCap1 });
                                                                                    gsList.Add(new GradedStock() { ID = prodCap2, Qty = maxCap2 });
                                                                                }
                                                                                else if (stock == "shred")
                                                                                {
                                                                                    ssList.Add(new ShredStock() { Shred = new Shred() { ID = prodCap1 }, Qty = maxCap1 });
                                                                                    ssList.Add(new ShredStock() { Shred = new Shred() { ID = prodCap2 }, Qty = maxCap2 });
                                                                                }
                                                                                CurrentCapacities.Add(new CurrentCapacity() { ProdTimeTableID = itemSL.ProTimeTableID, GradingDate = itemSL.ProdDate, ProductCapacityID = items.GradingID, ProductID = prodId1, RawProductID = rawProdId1, CapacityKG = maxCap1, Shift = curShift, BlocksLogs = minBlockLog, Paired = false, OrderType = oType });
                                                                                CurrentCapacities.Add(new CurrentCapacity() { ProdTimeTableID = itemSL.ProTimeTableID, GradingDate = itemSL.ProdDate, ProductCapacityID = items2.GradingID, ProductID = prodId2, RawProductID = rawProdId2, CapacityKG = maxCap2, Shift = curShift, BlocksLogs = minBlockLog, Paired = false, OrderType = oType });
                                                                                //blCounter += minBlockLog;

                                                                            }
                                                                        }
                                                                    }

                                                                    if (gsList.Count != 0)
                                                                    {

                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                                isGradedEx = true;
                                            }


                                            if (orderTypeId == 3 || isGradedEx == true)
                                            {

                                                decimal curCap1 = 0;
                                                decimal curCap2 = 0;

                                                curCap1 = items.TotCapacityKg - items.CurrentCapacity;
                                                curCap2 = items2.TotCapacityKg - items2.CurrentCapacity;

                                                if (amount1 > curCap1 || amount2 > curCap2 && items.CurrentCapacity < items.TotCapacityKg && items2.CurrentCapacity < items2.TotCapacityKg)
                                                {
                                                    maxBL1 = decimal.Floor(curCap1 / gradingWeight1);
                                                    maxBL2 = decimal.Floor(curCap2 / gradingWeight2);

                                                    if (maxBL1 > 0 && maxBL2 > 0)
                                                    {
                                                        if (maxBL1 <= maxBL2)
                                                        {
                                                            minBlockLog = maxBL1;
                                                        }
                                                        else
                                                        {
                                                            minBlockLog = maxBL2;
                                                        }

                                                        maxCap1 = gradingWeight1 * minBlockLog;
                                                        maxCap2 = gradingWeight2 * minBlockLog;

                                                        if ((curCap1 >= maxCap1) && (curCap2 >= maxCap2))
                                                        {
                                                            amount1 = amount1 - maxCap1;
                                                            amount2 = amount2 - maxCap2;

                                                            CurrentCapacities.Add(new CurrentCapacity() { ProdTimeTableID = items.ProTimeTableID, GradingDate = items.ProdDate, ProductCapacityID = items.GradingID, ProductID = prodId1, RawProductID = rawProdId1, CapacityKG = maxCap1, Shift = items.shift, BlocksLogs = minBlockLog, Paired = false, OrderType = orderTypeId });
                                                            CurrentCapacities.Add(new CurrentCapacity() { ProdTimeTableID = items2.ProTimeTableID, GradingDate = items.ProdDate, ProductCapacityID = items2.GradingID, ProductID = prodId2, RawProductID = rawProdId2, CapacityKG = maxCap2, Shift = items2.shift, BlocksLogs = minBlockLog, Paired = false, OrderType = orderTypeId });
                                                            if (lDate == CurrentDate)
                                                            {
                                                                blCounter += minBlockLog;
                                                            }
                                                        }
                                                    }
                                                }
                                                else if ((amount1 <= curCap1 && amount1 > 0) && (amount2 <= curCap2 && amount2 > 0))
                                                {
                                                    maxBL1 = decimal.Floor(amount1 / gradingWeight1);
                                                    maxBL2 = decimal.Floor(amount2 / gradingWeight2);
                                                    if (maxBL1 > 0 && maxBL2 > 0)
                                                    {
                                                        if (maxBL1 <= maxBL2)
                                                        {
                                                            minBlockLog = maxBL1;
                                                        }
                                                        else
                                                        {
                                                            minBlockLog = maxBL2;
                                                        }

                                                        maxCap1 = gradingWeight1 * maxBL1;
                                                        maxCap2 = gradingWeight2 * maxBL2;

                                                        if ((curCap1 >= maxCap1) && (curCap2 >= maxCap2))
                                                        {
                                                            amount1 = amount1 - maxCap1;
                                                            amount2 = amount2 - maxCap2;

                                                            CurrentCapacities.Add(new CurrentCapacity() { ProdTimeTableID = items.ProTimeTableID, GradingDate = items.ProdDate, ProductCapacityID = items.GradingID, ProductID = prodId1, RawProductID = rawProdId1, CapacityKG = maxCap1, Shift = items.shift, BlocksLogs = minBlockLog, Paired = false, OrderType = orderTypeId });
                                                            CurrentCapacities.Add(new CurrentCapacity() { ProdTimeTableID = items2.ProTimeTableID, GradingDate = items.ProdDate, ProductCapacityID = items2.GradingID, ProductID = prodId2, RawProductID = rawProdId2, CapacityKG = maxCap2, Shift = items2.shift, BlocksLogs = minBlockLog, Paired = false, OrderType = orderTypeId });
                                                            if (lDate == CurrentDate)
                                                            {
                                                                blCounter += minBlockLog;
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            break;
                                        }
                                    }
                                }
                                lDate = item.ProdDate;
                            }
                        }
                        lDate = bdg.AddBusinessDays(lDate, 1);//Going forward only
                    }
                }
            }
            else
            {
                while (amount1 > 0)
                {
                    dateShift = gradingManager.GetShift(lDate, prodCap1, gradingWeight1, orderTypeId);

                    if (dateShift == null)
                    {
                        do
                        {
                            lDate = bdg.AddBusinessDays(lDate, 1);
                            dateShift = gradingManager.GetShift(lDate, prodCap1, gradingWeight1, orderTypeId);

                        } while (dateShift == null);
                    }

                    foreach (var item in dateShift)
                    {
                        lDate = item.ProdDate;
                        foreach (var items in item.ShiftList)
                        {
                            if (graded1 == 0)
                            {
                                isGradedEx = true;
                            }

                            /************* NORMAL GRADED ORDER **************/
                            if ((items.GradingID == 6) || (orderTypeId == 1 && isGradedEx == false))
                            {
                                int oType = 0;
                                decimal totMixesLeft = CalTotMixesLeft(mixingMachineId, lDate, blCounter);
                                if (items.GradingID == 6)
                                {
                                    totMixesLeft = 100;
                                }

                                if (orderTypeId == 1)
                                {
                                    oType = 2;
                                }
                                else if (orderTypeId == 3)
                                {
                                    oType = 4;
                                }

                                if (totMixesLeft < 0)
                                {
                                    totMixesLeft = 0;
                                }
                                if (totMixesLeft > 0)
                                {
                                    MixingTimeTableDetails = DBAccess.GetProductionTimeTableDetails(mixingMachineId, lDate);
                                    if (MixingTimeTableDetails.Count != 0)
                                    {
                                        List<DateShift> MShiftDetails = GetMixingShift(lDate);
                                        foreach (var itemSD in MShiftDetails)
                                        {
                                            foreach (var itemSL in itemSD.ShiftList)
                                            {
                                                if (graded1 > 0 && amount1 > 0 && totMixesLeft > 0)
                                                {
                                                    if (amount1 > graded1)
                                                    {
                                                        maxBL1 = decimal.Floor(graded1 / gradingWeight1);


                                                        if (maxBL1 > 0)
                                                        {
                                                            minBlockLog = maxBL1;

                                                            if (minBlockLog > totMixesLeft)//Checking mixing capacity
                                                            {
                                                                minBlockLog = totMixesLeft;
                                                            }
                                                            maxCap1 = gradingWeight1 * minBlockLog;

                                                            if (graded1 >= maxCap1)
                                                            {
                                                                amount1 = amount1 - maxCap1;
                                                                graded1 = graded1 - maxCap1;

                                                                string stock = CheckGradedOrShred(prodCap1, prodCap2);
                                                                if (stock == "graded")
                                                                {
                                                                    gsList.Add(new GradedStock() { ID = prodCap1, Qty = maxCap1 });
                                                                }
                                                                else if (stock == "shred")
                                                                {
                                                                    ssList.Add(new ShredStock() { Shred = new Shred() { ID = prodCap1 }, Qty = maxCap1 });
                                                                }

                                                                CurrentCapacities.Add(new CurrentCapacity() { ProdTimeTableID = itemSL.ProTimeTableID, GradingDate = items.ProdDate, ProductCapacityID = items.GradingID, ProductID = prodId1, RawProductID = rawProdId1, CapacityKG = maxCap1, Shift = curShift, BlocksLogs = minBlockLog, Paired = false, OrderType = oType });
                                                                //blCounter += minBlockLog;
                                                            }
                                                        }
                                                    }
                                                    else if (amount1 <= graded1)
                                                    {
                                                        maxBL1 = decimal.Floor(amount1 / gradingWeight1);

                                                        if (maxBL1 > 0)
                                                        {
                                                            minBlockLog = maxBL1;

                                                            if (minBlockLog > totMixesLeft)//Checking mixing capacity
                                                            {
                                                                minBlockLog = totMixesLeft;
                                                            }

                                                            maxCap1 = gradingWeight1 * minBlockLog;

                                                            if (graded1 >= maxCap1)
                                                            {
                                                                amount1 = amount1 - maxCap1;
                                                                graded1 = graded1 - maxCap1;

                                                                string stock = CheckGradedOrShred(prodCap1, prodCap2);
                                                                if (stock == "graded")
                                                                {
                                                                    gsList.Add(new GradedStock() { ID = prodCap1, Qty = maxCap1 });
                                                                }
                                                                else if (stock == "shred")
                                                                {
                                                                    ssList.Add(new ShredStock() { Shred = new Shred() { ID = prodCap1 }, Qty = maxCap1 });
                                                                }

                                                                CurrentCapacities.Add(new CurrentCapacity() { ProdTimeTableID = itemSL.ProTimeTableID, GradingDate = items.ProdDate, ProductCapacityID = items.GradingID, ProductID = prodId1, RawProductID = rawProdId1, CapacityKG = maxCap1, Shift = curShift, BlocksLogs = minBlockLog, Paired = false, OrderType = oType });
                                                                //blCounter += minBlockLog;
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }

                                }
                                isGradedEx = true;
                            }

                            if (items.GradingID != 6)
                            {
                                decimal curCap1 = items.TotCapacityKg - items.CurrentCapacity;
                                if (amount1 != 0 && amount1 <= curCap1)
                                {
                                    minBlockLog = decimal.Floor(amount1 / gradingWeight1);
                                    maxCap1 = gradingWeight1 * minBlockLog;
                                    amount1 = amount1 - maxCap1;

                                    CurrentCapacities.Add(new CurrentCapacity()
                                    {
                                        ProdTimeTableID = items.ProTimeTableID,
                                        GradingDate = items.ProdDate,
                                        ProductCapacityID = items.GradingID,
                                        ProductID = prodId1,
                                        RawProductID = rawProdId1,
                                        CapacityKG = maxCap1,
                                        Shift = items.shift,
                                        BlocksLogs = minBlockLog,
                                        Paired = false,
                                        OrderType = orderTypeId
                                    });
                                    if (lDate == CurrentDate)
                                    {
                                        blCounter += minBlockLog;
                                    }
                                }
                                else if (amount1 != 0 && amount1 > curCap1)
                                {
                                    minBlockLog = decimal.Floor(curCap1 / gradingWeight1);
                                    maxCap1 = gradingWeight1 * minBlockLog;
                                    amount1 = amount1 - maxCap1;
                                    CurrentCapacities.Add(new CurrentCapacity()
                                    {
                                        ProdTimeTableID = items.ProTimeTableID,
                                        GradingDate = items.ProdDate,
                                        ProductCapacityID = items.GradingID,
                                        ProductID = prodId1,
                                        RawProductID = rawProdId1,
                                        CapacityKG = maxCap1,
                                        Shift = items.shift,
                                        BlocksLogs = minBlockLog,
                                        Paired = false,
                                        OrderType = orderTypeId
                                    });
                                    if (lDate == CurrentDate)
                                    {
                                        blCounter += minBlockLog;
                                    }
                                }
                            }
                        }
                        lDate = bdg.AddBusinessDays(lDate, 1);//Going forward only
                    }
                }
            }

            //Deduct from Graded Stock            
            if (gsList.Count > 0)
            {
                int res = DBAccess.UpdateGradedStock(gsList);
                if (res == 0)
                {
                    Console.WriteLine("Adding to Graded Stock from Order has an error! ");
                }
            }
            //Deduct from Shred Stock            
            if (ssList.Count > 0)
            {
                int res = DBAccess.DeductShredStock(ssList);
                if (res == 0)
                {
                    Console.WriteLine("Adding to Shred Stock from Order has an error! ");
                }
            }


        }


        public bool CheckDateAvailable(DateTime cDate)
        {
            bool res = false;
            Production prod = new Production();
            ProductionTimeTable = DBAccess.GetProductionTimeTableByID(1, cDate);//Get Production TimeTable
            if (ProductionTimeTable.Count == 0)
            {
                res = prod.AddNewDates(cDate, false);
            }
            else
            {
                res = true;
                Console.WriteLine("date available in the current list");
            }
            return res;
        }

        private decimal CalcBlkLog(decimal Quantity, decimal MaxItemsPer)
        {
            decimal BlockLogQty = 0;
            if (MaxItemsPer == 0)
            {
                MaxItemsPer = 1;
            }

            return BlockLogQty = Math.Ceiling(Quantity / MaxItemsPer);
        }


        public List<DateShift> GetShift(DateTime localDate, int capId, decimal gradingWeight, int orderType)
        {
            int shift = 0;
            Production production = new Production();
            BusinessDaysGenerator bdg = new BusinessDaysGenerator();
            //DateTime localDate = lDate;
            shiftList = new List<ProductionTimeTable>();
            shiftList = DBAccess.GetProductionTimeTableByID(1, localDate);
            List<DateShift> dateShift = new List<DateShift>();
            List<ShiftDetails> subShifts = null;
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

            if (shiftList.Count == 0 || shiftList == null)
            {
                do
                {
                    production.AddNewDates(localDate, false);
                    shiftList = DBAccess.GetProductionTimeTableByID(1, localDate);

                } while (shiftList.Count == 0);
            }

            if (shiftList.Count != 0)
            {
                CapacityLimitations = DBAccess.GetCapacityLimitations(shiftList[0].ID);//Get capacity limits                

                List<ProductionTimeTable> pList = DBAccess.GetProductionTimeTableByID(1, CurrentDate);//Get current date id
                do
                {
                    if (pList.Count == 0 || pList == null)
                    {
                        Production prod = new Production();
                        prod.AddNewDates(CurrentDate, true);
                        pList = DBAccess.GetProductionTimeTableByID(1, CurrentDate);//Get current date id
                    }

                }
                while (pList.Count == 0 || pList == null);

                if (pList.Count > 0)
                {
                    cId = pList[0].ID;
                    foreach (var item in shiftList)
                    {
                        subShifts = new List<ShiftDetails>();

                        if (item.IsMachineActive == true)
                        {
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

                                var data = new ProductCapacity();
                                //shift = 0;
                                if (capId == 7 || capId == 8 || capId == 9)
                                {
                                    data.CapacityKG = 10000;
                                    data.ProductionTimeTableID = item.ID;
                                    data.RubberGradingID = capId;
                                }
                                else
                                {

                                    data = CapacityLimitations.Single(c => c.ProductionTimeTableID == item.ID && c.RubberGradingID == capId && c.Shift == 1);//ProductCapacity
                                }
                                decimal cWeight = DBAccess.CheckCapacityByDateShift(item.ID, capId, 1, orderType);//CurrentCapacity
                                decimal avaWeight = data.CapacityKG - cWeight;
                                if (avaWeight > 0)
                                {
                                    decimal existingBlkLogs = CalNoOfBlocks(avaWeight, gradingWeight);

                                    decimal maxBlkLogs = CalNoOfBlocks(data.CapacityKG, gradingWeight);

                                    if (existingBlkLogs > 0 && existingBlkLogs <= maxBlkLogs)
                                    {
                                        shift = 1;
                                        subShifts.Add(new ShiftDetails() { ProTimeTableID = data.ProductionTimeTableID, ProdDate = localDate, GradingID = data.RubberGradingID, shift = 1, CurrentCapacity = cWeight, TotCapacityKg = data.CapacityKG, GradedKg = data.GradedKG });
                                    }
                                }
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
                                var data = new ProductCapacity();
                                //shift = 0;
                                if (capId == 7 || capId == 8 || capId == 9)
                                {
                                    data.CapacityKG = 10000;
                                    data.ProductionTimeTableID = item.ID;
                                    data.RubberGradingID = capId;
                                }
                                else
                                {
                                    //shift = 0;
                                    data = CapacityLimitations.Single(c => c.ProductionTimeTableID == item.ID && c.RubberGradingID == capId && c.Shift == 2);
                                }
                                decimal cWeight = DBAccess.CheckCapacityByDateShift(item.ID, capId, 2, orderType);

                                decimal avaWeight = data.CapacityKG - cWeight;
                                if (avaWeight > 0)
                                {
                                    decimal existingBlkLogs = CalNoOfBlocks(avaWeight, gradingWeight);

                                    decimal maxBlkLogs = CalNoOfBlocks(data.CapacityKG, gradingWeight);

                                    if (existingBlkLogs > 0 && existingBlkLogs <= maxBlkLogs)
                                    {
                                        shift = 2;
                                        subShifts.Add(new ShiftDetails() { ProTimeTableID = data.ProductionTimeTableID, ProdDate = localDate, GradingID = data.RubberGradingID, shift = 2, CurrentCapacity = cWeight, TotCapacityKg = data.CapacityKG, GradedKg = data.GradedKG });
                                    }
                                }
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
                                //shift = 0;
                                decimal cWeight = DBAccess.CheckCapacityByDateShift(item.ID, capId, 3, orderType);
                                var data = new ProductCapacity();
                                //shift = 0;
                                if (capId == 7 || capId == 8 || capId == 9)
                                {
                                    data.CapacityKG = 10000;
                                    data.ProductionTimeTableID = item.ID;
                                    data.RubberGradingID = capId;
                                }
                                else
                                {
                                    data = CapacityLimitations.Single(c => c.ProductionTimeTableID == item.ID && c.RubberGradingID == capId && c.Shift == 3);
                                }
                                decimal avaWeight = data.CapacityKG - cWeight;
                                if (avaWeight > 0)
                                {
                                    decimal existingBlkLogs = CalNoOfBlocks(avaWeight, gradingWeight);

                                    decimal maxBlkLogs = CalNoOfBlocks(data.CapacityKG, gradingWeight);

                                    if (existingBlkLogs > 0 && existingBlkLogs <= maxBlkLogs)
                                    {
                                        shift = 3;
                                        subShifts.Add(new ShiftDetails() { ProTimeTableID = data.ProductionTimeTableID, ProdDate = localDate, GradingID = data.RubberGradingID, shift = 3, CurrentCapacity = cWeight, TotCapacityKg = data.CapacityKG, GradedKg = data.GradedKG });
                                    }
                                }
                            }
                        f: ;
                            if (item.IsDayShiftActive == false && item.IsEveningShiftActive == false && item.IsNightShiftActive == false)
                            {
                                dateShift = null;
                            }
                        }
                        else
                        {
                            Console.WriteLine("Machine is switched off on - " + localDate);
                            dateShift = null;
                        }
                    }
                }

            }

            if (shift != 0)
            {
                DateShift ds = new DateShift();
                ds.ProdDate = localDate;
                ds.ShiftList = subShifts;
                dateShift.Add(ds);
            }
            else
            {
                Console.WriteLine("No shifts found " + localDate);
                dateShift = null;
            }



            return dateShift;
        }

        public CurrentCapacity CalcuateCapacityBlockLogs(decimal cc, decimal totc, decimal gw, decimal bl)
        {
            decimal remKg = 0;
            decimal remBlkLog = 0;
            decimal totBlks = 0;
            decimal totKg = 0;
            CurrentCapacity currentCapacity = new CurrentCapacity();
            //Remaining Space
            if (cc <= totc)
            {
                remKg = totc - cc;//Remaining space KG
            }
            else
            {
                remKg = cc - totc;//Remaining space KG
            }

            remBlkLog = decimal.Floor(remKg / gw);//Remaining block/log space

            if (bl <= remBlkLog)
            {
                totBlks = bl;
            }
            else
            {
                totBlks = remBlkLog;
            }

            totKg = totBlks * gw;

            currentCapacity.BlocksLogs = totBlks;
            currentCapacity.CapacityKG = totKg;

            return currentCapacity;
        }

        private Tuple<List<GradingScheduling>, List<CurrentCapacity>> MergeSimilarObjects(List<GradingScheduling> gsl, List<CurrentCapacity> cc)
        {
            bool exeGS = false;
            bool exeCC = false;
            List<GradingScheduling> gsList = new List<GradingScheduling>();
            List<CurrentCapacity> ccList = new List<CurrentCapacity>();

            foreach (var item in gsl)
            {
                exeGS = false;

                if (gsList.Count == 0)
                {
                    exeGS = true;
                }
                else
                {
                    var dat = gsList.FirstOrDefault(c => c.RawProductID == item.RawProductID && c.SalesID == item.SalesID && c.Status == item.Status && c.OrderType == item.OrderType);
                    if (dat != null)
                    {
                        foreach (var itemGSL in gsList)
                        {
                            if (item.RawProductID == itemGSL.RawProductID && item.SalesID == itemGSL.SalesID && item.Status == itemGSL.Status && item.OrderType == itemGSL.OrderType)
                            {
                                itemGSL.BlocklogQty += item.BlocklogQty;
                            }
                        }
                        exeGS = false;
                    }
                    else
                    {
                        exeGS = true;
                    }
                }

                if (exeGS)
                {
                    GradingScheduling gs = new GradingScheduling();
                    //gs.ProductionTimeTableID = item.ProductionTimeTableID;
                    gs.RawProductID = item.RawProductID;
                    gs.SalesID = item.SalesID;
                    gs.BlocklogQty = item.BlocklogQty;
                    //gs.Shift = item.Shift;
                    gs.Status = item.Status;
                    gs.OrderType = item.OrderType;
                    gs.ActiveOrder = item.ActiveOrder;
                    gs.PrintCounter = item.PrintCounter;
                    gsList.Add(gs);
                }
            }

            foreach (var item in cc)
            {
                if (ccList.Count == 0)
                {
                    exeCC = true;
                }
                else
                {
                    var dat = ccList.FirstOrDefault(c => c.RawProductID == item.RawProductID && c.ProductCapacityID == item.ProductCapacityID && c.SalesID == item.SalesID && c.OrderType == item.OrderType);
                    if (dat != null)
                    {
                        foreach (var itemCCL in ccList)
                        {
                            if (item.RawProductID == itemCCL.RawProductID && item.ProductCapacityID == itemCCL.ProductCapacityID && item.SalesID == itemCCL.SalesID && item.OrderType == itemCCL.OrderType)
                            {
                                itemCCL.BlocksLogs += item.BlocksLogs;
                                itemCCL.CapacityKG += item.CapacityKG;
                            }
                        }
                        exeCC = false;
                    }
                    else
                    {
                        exeCC = true;
                    }
                }
                if (exeCC)
                {
                    CurrentCapacity c = new CurrentCapacity();
                    //c.ProdTimeTableID = item.ProdTimeTableID;
                    c.ProductCapacityID = item.ProductCapacityID;
                    c.SalesID = item.SalesID;
                    c.RawProductID = item.RawProductID;
                    c.ProductID = item.ProductID;
                    //c.Shift = item.Shift;
                    c.CapacityKG = item.CapacityKG;
                    c.BlocksLogs = item.BlocksLogs;
                    c.OrderType = item.OrderType;
                    ccList.Add(c);
                }
            }

            return Tuple.Create(gsList, ccList);
        }

        public void ShiftOrders(string buttonType, int shift, DateTime skipDate, string btnType)
        {
            currentCapacities = new List<CurrentCapacity>();
            gradingScheduling = new List<GradingScheduling>();
            BusinessDaysGenerator bdg = new BusinessDaysGenerator();
            GradingManager gm = new GradingManager();

            formulaList = DBAccess.GetFormulas();

            if (formulaList.Count > 0 || formulaList != null)
            {
                Tuple<List<GradingScheduling>, List<CurrentCapacity>> element = DBAccess.GetAllGradingCapacitesToMove(1, skipDate, buttonType, shift, btnType);//Get GradingScheduling and CurrentCapacity details

                if ((element.Item1.Count > 0 && element.Item2.Count > 0) && (element.Item1 != null && element.Item2 != null))
                {
                    foreach (var item in element.Item2)
                    {
                        Console.WriteLine(item.Shift + " " + item.RawProductID + " " + item.ProdTimeTableID + " " + item.SalesID + " " + item.BlocksLogs);
                    }

                    //Backup data
                    string type = buttonType;
                    int backRes = DBAccess.InsertCapacityAndGradingSchedulingBackUp(DateTime.Now, element.Item2, element.Item1, type);
                    if (backRes > 0)
                    {
                        //Delete data
                        int delRes = DBAccess.DeleteCapacity(element.Item2, element.Item1);
                        if (delRes > 0)
                        {
                            BackgroundWorker worker = new BackgroundWorker();
                            LoadingScreen = new ChildWindow();
                            LoadingScreen.ShowWaitingScreen("Processing");

                            worker.DoWork += (_, __) =>
                            {
                                Tuple<List<GradingScheduling>, List<CurrentCapacity>> data = MergeSimilarObjects(element.Item1, element.Item2);
                                DateTime startdate = DateTime.MinValue;
                                DateTime movingDate = DateTime.MinValue;

                                if (buttonType == "Enable")//Shift backwords
                                {
                                    startdate = skipDate;
                                    movingDate = skipDate;
                                }
                                else if (buttonType == "Disable")//Shift forward
                                {
                                    if (shift == 0 || shift == 3)
                                    {
                                        startdate = bdg.AddBusinessDays(Convert.ToDateTime(skipDate), 1);//Shifting
                                        movingDate = startdate;
                                        shift = 1;
                                    }
                                    else if (shift == 1)
                                    {
                                        startdate = skipDate;
                                        movingDate = skipDate;
                                        shift = 2;
                                    }
                                    else if (shift == 2)
                                    {
                                        startdate = skipDate;
                                        movingDate = skipDate;
                                        shift = 3;
                                    }

                                }

                                foreach (var item in data.Item1)
                                {
                                    movingDate = startdate;//Starting date
                                    var fData = formulaList.Single(c => c.RawProductID == item.RawProductID);//Formula

                                    while (item.BlocklogQty > 0)
                                    {
                                        decimal grade1Kg = 0;
                                        List<DateShift> shiftDetails1 = null;

                                        if (fData.GradingWeight1 != 0) //Grading1
                                        {
                                            grade1Kg = fData.GradingWeight1 * item.BlocklogQty;

                                            shiftDetails1 = GetNextShift(movingDate, shift, fData.ProductCapacity1, fData.GradingWeight1, item.OrderType);

                                            foreach (var itemSD1 in shiftDetails1)
                                            {
                                                foreach (var itemSL1 in itemSD1.ShiftList)
                                                {
                                                    if (item.BlocklogQty > 0)
                                                    {
                                                        CurrentCapacity currentCap = gm.CalcuateCapacityBlockLogs(itemSL1.CurrentCapacity, itemSL1.TotCapacityKg, fData.GradingWeight1, item.BlocklogQty);
                                                        if (fData.GradingWeight2 == 0)//Grading1
                                                        {
                                                            item.BlocklogQty = item.BlocklogQty - currentCap.BlocksLogs;
                                                            //Grading1
                                                            currentCapacities.Add(new CurrentCapacity() { ProdTimeTableID = itemSL1.ProTimeTableID, SalesID = item.SalesID, ProductCapacityID = itemSL1.GradingID, ProductID = 0, RawProductID = item.RawProductID, CapacityKG = currentCap.CapacityKG, Shift = itemSL1.shift, BlocksLogs = currentCap.BlocksLogs, Paired = false, OrderType = item.OrderType });
                                                            gradingScheduling.Add(new GradingScheduling() { ProductionTimeTableID = itemSL1.ProTimeTableID, RawProductID = item.RawProductID, SalesID = item.SalesID, BlocklogQty = currentCap.BlocksLogs, Shift = itemSL1.shift, Status = item.Status, OrderType = item.OrderType, ActiveOrder = item.ActiveOrder, PrintCounter = item.PrintCounter });

                                                            InsertGrdaings(skipDate, buttonType, element.Item1, element.Item2);
                                                        }

                                                        if (fData.GradingWeight2 != 0) //Grading2
                                                        {
                                                            decimal grade2Kg = 0;
                                                            List<DateShift> shiftDetails2 = null;

                                                            grade2Kg = fData.GradingWeight2 * item.BlocklogQty;

                                                            shiftDetails2 = GetNextShift(movingDate, shift, fData.ProductCapacity2, fData.GradingWeight2, item.OrderType);

                                                            foreach (var itemSD2 in shiftDetails2)
                                                            {
                                                                foreach (var itemSL2 in itemSD2.ShiftList)
                                                                {
                                                                    if (itemSL1.ProTimeTableID == itemSL2.ProTimeTableID && itemSL1.shift == itemSL2.shift && item.BlocklogQty > 0)
                                                                    {
                                                                        decimal bl = 0;
                                                                        CurrentCapacity currentCap2 = gm.CalcuateCapacityBlockLogs(itemSL2.CurrentCapacity, itemSL2.TotCapacityKg, fData.GradingWeight2, item.BlocklogQty);

                                                                        if (currentCap.BlocksLogs <= currentCap2.BlocksLogs)
                                                                        {
                                                                            bl = currentCap.BlocksLogs;
                                                                        }
                                                                        else
                                                                        {
                                                                            bl = currentCap2.BlocksLogs;
                                                                        }

                                                                        if (bl > 0)
                                                                        {
                                                                            decimal g1Kg = 0;
                                                                            decimal g2Kg = 0;
                                                                            g1Kg = bl * fData.GradingWeight1;
                                                                            g2Kg = bl * fData.GradingWeight2;

                                                                            item.BlocklogQty = item.BlocklogQty - bl;
                                                                            //Grading1
                                                                            currentCapacities.Add(new CurrentCapacity() { ProdTimeTableID = itemSL1.ProTimeTableID, SalesID = item.SalesID, ProductCapacityID = itemSL1.GradingID, ProductID = 0, RawProductID = item.RawProductID, CapacityKG = g1Kg, Shift = itemSL1.shift, BlocksLogs = bl, Paired = false, OrderType = item.OrderType });
                                                                            //Grading2
                                                                            currentCapacities.Add(new CurrentCapacity() { ProdTimeTableID = itemSL1.ProTimeTableID, SalesID = item.SalesID, ProductCapacityID = itemSL2.GradingID, ProductID = 0, RawProductID = item.RawProductID, CapacityKG = g2Kg, Shift = itemSL1.shift, BlocksLogs = bl, Paired = false, OrderType = item.OrderType });
                                                                            gradingScheduling.Add(new GradingScheduling() { ProductionTimeTableID = itemSL1.ProTimeTableID, RawProductID = item.RawProductID, SalesID = item.SalesID, BlocklogQty = bl, Shift = itemSL1.shift, Status = item.Status, OrderType = item.OrderType, ActiveOrder = item.ActiveOrder, PrintCounter = item.PrintCounter });

                                                                            InsertGrdaings(skipDate, buttonType, element.Item1, element.Item2);

                                                                        }

                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }

                                                movingDate = bdg.AddBusinessDays(movingDate, 1);//Going forward only Next date
                                            }
                                        }
                                    }
                                }

                            };

                            worker.RunWorkerCompleted += delegate(object s, RunWorkerCompletedEventArgs args)
                            {
                                LoadingScreen.CloseWaitingScreen();
                            };
                            worker.RunWorkerAsync();
                        }
                        else
                        {
                            DBAccess.InsertErrorLog("Error E03 | Failed to delete GradingScheduling and CurrentCapacity " + DateTime.Now);
                            Msg.Show("There has been a problem while backing up data! Error Code - E03" + System.Environment.NewLine + "Please try again later.", "Data Backing Up Failed Error Code - E03", MsgBoxButtons.OK, MsgBoxImage.Information_Red, MsgBoxResult.Yes);
                        }
                    }
                    else
                    {
                        DBAccess.InsertErrorLog("Error E02 | Failed to backup GradingScheduling and CurrentCapacity " + DateTime.Now);
                        Msg.Show("There has been a problem while backing up data! Error Code - E02" + System.Environment.NewLine + "Please try again later.", "Data Backing Up Failed Error Code - E02", MsgBoxButtons.OK, MsgBoxImage.Information_Red, MsgBoxResult.Yes);
                    }
                }
                else
                {
                    //DBAccess.InsertErrorLog("Error E01 | Tuple returns Element1 - " + element.Item1.Count + " and Element2 - " + element.Item2.Count + " " + DateTime.Now);
                    //Msg.Show("There has been a problem while fetching data from Grading! Error Code - E01" + System.Environment.NewLine + "Please try again later.", "Data Unavailable Error Code - E01", MsgBoxButtons.OK, MsgBoxImage.Information_Red, MsgBoxResult.Yes);

                }
            }
            else
            {
                DBAccess.InsertErrorLog("Error E08 | Formula list loading failed - " + formulaList.Count + " " + DateTime.Now);
                Msg.Show("There has been a problem while completing your request! Error Code - E08" + System.Environment.NewLine + "Please try again later.", "Data Unavailable Error Code - E08", MsgBoxButtons.OK, MsgBoxImage.Information_Red, MsgBoxResult.Yes);

            }
            foreach (var items in currentCapacities)
            {
                Console.WriteLine(items.ProdTimeTableID + " " + items.ProductCapacityID + " " + items.ProductID + " " + items.RawProductID + " " + items.CapacityKG + " " + items.Shift + " " + items.BlocksLogs);
            }
        }

        private void InsertGrdaings(DateTime skipDate, string buttonType, List<GradingScheduling> rbg, List<CurrentCapacity> rbcc)
        {
            //Insert to database
            if ((gradingScheduling != null && currentCapacities != null) || (gradingScheduling.Count > 0 && currentCapacities.Count > 0))
            {
                int res1 = DBAccess.AllocateGradingCapacity(currentCapacities, gradingScheduling);
                if (res1 > 0)
                {
                    gradingScheduling.Clear();
                    currentCapacities.Clear();
                    //Msg.Show("You have successfully switched off " + skipDate.ToString("dd/MM/yyyy") + "(" + skipDate.DayOfWeek + ")" + System.Environment.NewLine + "All the Orders have been shifted to the next available shifts.", "Day Switched Off Successfull", MsgBoxButtons.OK, MsgBoxImage.OK, MsgBoxResult.Yes);
                }
                else
                {
                    /***************ROLL BACK***************/
                    DBAccess.InsertErrorLog("Error E04 | Problem when " + buttonType + ". Insert failed for CurrentCapacity or GradingScheduling! Starting RollBack " + DateTime.Now);
                    int res2 = DBAccess.AllocateGradingCapacity(rbcc, rbg);
                    if (res2 > 0)
                    {
                        DBAccess.InsertErrorLog("Error E05 | " + DateTime.Now + " RollBack Successfull");
                        Msg.Show("There was a problem and the day connot be switched off at this moment! Error Code - E05" + System.Environment.NewLine + "Please try again later.", "Day Switched Off Failed Error Code - E05", MsgBoxButtons.OK, MsgBoxImage.Information_Red, MsgBoxResult.Yes);
                    }
                    else
                    {
                        DBAccess.InsertErrorLog("Error E06 | " + DateTime.Now + " RollBack Failed");
                        Msg.Show("There was a problem and the day connot be switched off at this moment! Error Code - E06" + System.Environment.NewLine + "Please try again later.", "Day Switched Off Failed Error Code - E06", MsgBoxButtons.OK, MsgBoxImage.Information_Red, MsgBoxResult.Yes);
                    }
                }
            }
            else
            {
                /***************ROLL BACK***************/
                DBAccess.InsertErrorLog("Error E07 | Problem when " + buttonType + ". GradingScheduling,Count - " + gradingScheduling.Count + " CurrentCapacity - " + currentCapacities.Count + " lists are empty! Starting RollBack " + DateTime.Now);
                Msg.Show("There was a problem and the day connot be switched off at this moment! Error Code - E07" + System.Environment.NewLine + "Please try again later.", "Day Switched Off Failed Error Code - E07", MsgBoxButtons.OK, MsgBoxImage.Information_Red, MsgBoxResult.Yes);
            }
        }

        private List<DateShift> GetNextShift(DateTime date, int shift, int capId, decimal gw, int ot)
        {
            List<DateShift> dShift = new List<DateShift>();
            GradingManager gm = new GradingManager();
            BusinessDaysGenerator bdg = new BusinessDaysGenerator();

            dShift = gm.GetShift(date, capId, gw, ot);
            if (dShift == null)
            {
                do
                {
                    date = bdg.AddBusinessDays(date, 1);
                    dShift = gm.GetShift(date, capId, gw, ot);

                } while (dShift == null);
            }

            return dShift;
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

        private decimal CalNoOfBlocks(decimal amount, decimal gradingWeight)
        {
            decimal blk = 0;

            blk = decimal.Floor(amount / gradingWeight);

            return blk;
        }

        private decimal CalTotMixesLeft(int mixingMachineId, DateTime mDate, decimal bc)
        {
            Int32 mixPrdTimeTableId = 0;
            decimal curMixes = 0;
            decimal totBL = 0;
            //GET CURRENT MIXES FROM MixingCurrentCapacity Table
            if (mixingMachineId > 0)
            {
                List<ProductionTimeTable> pTL = DBAccess.GetProductionTimeTableByID(mixingMachineId, mDate);//Gte the Mixing Time Table ID
                foreach (var itempTL in pTL)
                {
                    mixPrdTimeTableId = itempTL.ID;
                }
                curMixes = DBAccess.GetCurrentMixingCapacity(mixPrdTimeTableId);
            }
            ///GET CURRENT BLOCKS LOGS FROM the list
            //decimal total = CurrentCapacities.Sum(x => x.BlocksLogs);

            //Get Current date id's
            List<ProductionTimeTable> pList = DBAccess.GetProductionTimeTableByID(1, mDate);
            int cId = 0;
            foreach (var itemPL in pList)
            {
                cId = itemPL.ID;
            }

            //GET TOTAL BLOCKSLOGS FROM GradingScheduling Table
            totBL = DBAccess.GetCurrentBlockLogTotal(cId);
            decimal totMixingOccupied = curMixes + totBL + bc;
            decimal maxMixesPerDay = 0;
            if (mixPrdTimeTableId > 0)
            {
                maxMixesPerDay = DBAccess.GetMaxMixes(mixPrdTimeTableId);

            }
            decimal totMixesLeft = maxMixesPerDay - totMixingOccupied;

            return totMixesLeft;
        }

        private void LoadGradedStock()
        {
            GradedStock = DBAccess.GetGradedStock();
        }

        private void LoadShredStock()
        {
            ShredStock = DBAccess.GetShredStock();
        }

        public List<DateShift> GetMixingShift(DateTime lDate)
        {
            BusinessDaysGenerator bdg = new BusinessDaysGenerator();
            List<DateShift> dateShift = new List<DateShift>();
            List<ShiftDetails> subShifts = null;

            if (MixingTimeTableDetails.Count != 0)
            {
                foreach (var item in MixingTimeTableDetails)
                {
                    if (dateShift.Count == 0)
                    {
                        if (item.ProductionDate == lDate)
                        {
                            subShifts = new List<ShiftDetails>();
                            if (item.IsMachineActive == true)
                            {
                                //Mixing machine is working, get the grading machine id
                                List<ProductionTimeTable> ptl = DBAccess.GetProductionTimeTableByID(1, lDate);
                                int id = item.ID;

                                foreach (var itemPTL in ptl)
                                {
                                    id = itemPTL.ID;
                                }
                                subShifts.Add(new ShiftDetails() { ProTimeTableID = id });

                                DateShift ds = new DateShift();
                                ds.ProdDate = lDate;
                                ds.ShiftList = subShifts;
                                dateShift.Add(ds);
                            }
                            else
                            {
                                Console.WriteLine("Mixing Machine is switched off - " + lDate);
                                lDate = bdg.AddBusinessDays(lDate, 1);
                                dateShift = GetMixingShift(lDate);
                            }
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                if (dateShift.Count == 0)
                {
                    //Create a new date
                    lDate = bdg.AddBusinessDays(lDate, 1);
                    dateShift = GetMixingShift(lDate);
                }
            }
            return dateShift;
        }

    }
}
