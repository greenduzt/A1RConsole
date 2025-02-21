
using A1RConsole.Bases;
using A1RConsole.Commands;
using A1RConsole.Core;
using A1RConsole.DB;
using A1RConsole.Models.Meta;
using A1RConsole.Models.Orders;
using A1RConsole.Models.Production;
using A1RConsole.Models.Production.Peeling;
using A1RConsole.Models.Production.ReRolling;
using A1RConsole.Models.Products;
using A1RConsole.PdfGeneration;
using MsgBox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace A1RConsole.ViewModel.Productions.Peeling
{
    public class PeelingConfirmationViewModel : ViewModelBase
    {
        public decimal maxYield;
        public decimal minYield;
        public int currentShift;
        public event Action Closed;
        private ICommand _closeCommand;
        private ICommand _submitCommand;
        private ICommand _printYieldCommand;
        private decimal _totalYieldCut;
        private decimal updatedMaxYield;
        private bool _totYieldCutPrint;
        private string _headerStr;
        private string _subHeaderStr;
        private string _hdrTotalYieldCut;
        private bool _enDisSubmit;
        private decimal peelingLogs;
        private decimal peelingDollarVal;
        private List<ProductMeterage> prodMeterageList;
        public PeelingOrder peelingProductionDetails { get; set; }
        public decimal TilesToDo { get; set; }


        public PeelingConfirmationViewModel(PeelingOrder ppd)
        {

            peelingProductionDetails = ppd;
            if (peelingProductionDetails != null)
            {
                //List<string> elements = peelingProductionDetails.Peeling.Product.Size.Split('x').ToList<string>();
                //List<ProductYield> productYield = DBAccess.GetProductYieldByID(peelingProductionDetails.Peeling.Product.ProductID);

                HeaderStr = "1 Log of " + peelingProductionDetails.Product.ProductName + " " + peelingProductionDetails.Product.Density + " " + peelingProductionDetails.Product.Tile.Thickness + "mm";

                ProductMeterage pm = new ProductMeterage();
                pm.Thickness = peelingProductionDetails.Product.Tile.Thickness;
                pm.MouldType = peelingProductionDetails.Product.MouldType;
                pm.MouldSize = peelingProductionDetails.Product.Width;
                prodMeterageList = DBAccess.GetProductMeterageByValues(pm);
                maxYield = Math.Ceiling(prodMeterageList[0].ExpectedYield);
                updatedMaxYield = Math.Floor(((maxYield * 5) / 100) + maxYield);

                minYield = Math.Floor(peelingProductionDetails.Product.Tile.MinYield);
                string minY = minYield == 0 ? "" : "Min yield - " + minYield + "L/m      ";
                SubHeaderStr = minY + "MaxYield - " + updatedMaxYield + "L/m";
                HdrTotalYieldCut = "Total yield cut " + peelingProductionDetails.Product.Width.ToString("G29") + " x ";
                TotYieldCutPrint = false;
                EnDisSubmit = false;


                ShiftManager sm = new ShiftManager();
                currentShift = sm.GetCurrentShift();
            }
        }


        public void SubmitOrder()
        {
            if (TotalYieldCut == 0)
            {
                Msg.Show("Enter the total yield cut in L/m", "Input Required", MsgBoxButtons.OK, MsgBoxImage.Error, MsgBoxResult.Yes);
            }
            else if (TotalYieldCut > updatedMaxYield)
            {
                Msg.Show("The total yield cut cannot be greater than " + updatedMaxYield.ToString("G29") + "L/m", "Invalid Input", MsgBoxButtons.OK, MsgBoxImage.Error, MsgBoxResult.Yes);
            }
            else
            {
                //Check if it safe to complete
                List<SystemParameters> systemParameters = DBAccess.GetAllSystemParametersByValue(true);
                bool has = systemParameters.Any(x => x.Value == true && (x.ParameterCode == "ChangingShiftForPeeling" || x.ParameterCode == "PeelingCompletion"));
                if (has == true)
                {
                    Msg.Show("System is performing some updates. Please try again in few minutes ", "System Busy", MsgBoxButtons.OK, MsgBoxImage.Information, MsgBoxResult.Yes);
                }
                else
                {
                    if (TotalYieldCut < minYield)
                    {
                        Msg.Show("The total yield cut is less than the minimum yield of " + minYield.ToString("G29") + "L/m" + System.Environment.NewLine + "Please report to supervisor", "Report To Supervisor", MsgBoxButtons.OK, MsgBoxImage.Information_Orange, MsgBoxResult.Yes);

                    }

                    List<PeelingOrder> peelingOrderList = DBAccess.GetPeelingOrderByID(peelingProductionDetails.ID);

                    if (peelingOrderList.Count > 0 || peelingOrderList != null || prodMeterageList.Count > 0 || prodMeterageList != null)
                    {
                        decimal peelingLogs = 0;
                        if (peelingProductionDetails.Product.Type == "Bulk")
                        {
                            peelingLogs = 1;
                        }
                        else
                        {
                            peelingLogs = Math.Floor(prodMeterageList[0].ExpectedYield / peelingProductionDetails.Product.Tile.MaxYield);
                        }

                        //Reduce PeelingOrders by 1
                        peelingDollarVal = peelingLogs * peelingProductionDetails.Product.UnitPrice;
                        PeelingOrder po = new PeelingOrder();
                        po.ID = peelingProductionDetails.ID;
                        po.Logs = (peelingOrderList[0].Logs - 1) < 0 ? 0 : (peelingOrderList[0].Logs - 1);
                        po.DollarValue = (peelingOrderList[0].DollarValue - peelingDollarVal) < 0 ? 0 : (peelingOrderList[0].DollarValue - peelingDollarVal);
                        po.Qty = (peelingOrderList[0].Qty - peelingLogs) < 0 ? 0 : (peelingOrderList[0].Qty - peelingLogs);
                        int res = DBAccess.UpdatePeeling(this, po);

                        if (res > 0)
                        {
                            //Go to Re-Rolling
                            if (peelingProductionDetails.IsReRollingReq)
                            {
                                if (peelingProductionDetails.Product.Type == "Roll" || peelingProductionDetails.Product.Type == "Standard" || peelingProductionDetails.Product.Type == "LOG")
                                {
                                    //Update Pendilng Slitpeel
                                    decimal rollsReqUpdate = 0;
                                    decimal currRolls = Math.Floor(TotalYieldCut / peelingProductionDetails.Product.Tile.MaxYield);
                                    decimal calPerBl = Math.Floor(prodMeterageList[0].ExpectedYield / peelingProductionDetails.Product.Tile.MaxYield);
                                    decimal peelingCompletedRolls = DBAccess.GetTotalpeelingCompleted(peelingProductionDetails.Order.OrderNo, peelingProductionDetails.Product);
                                    PendingOrder pendingOrder = DBAccess.GetPendingOrder(peelingProductionDetails.Order.OrderNo, peelingProductionDetails.Product.RawProduct.RawProductID, peelingProductionDetails.Product.ProductID);
                                    List<PeelingOrder> peelingOrder = DBAccess.GetAllPeelingOrderByID(peelingProductionDetails.Order.OrderNo, peelingProductionDetails.Product.ProductID, peelingProductionDetails.Product.RawProduct.RawProductID);
                                    PendingSlitPeel ps = new PendingSlitPeel();
                                    ps.Product = new Product() { ProductID = peelingProductionDetails.Product.ProductID, RawProduct = new RawProduct() { RawProductID = peelingProductionDetails.Product.RawProduct.RawProductID } };
                                    ps.Order = peelingProductionDetails.Order;
                                    List<PendingSlitPeel> psList = DBAccess.GetPendingSlitPeelByID(ps);

                                    decimal existingPeelingLogs = peelingOrder.Sum(x => x.Logs);
                                    //decimal peelingCompletedRolls = Math.Floor(totYieldCut / peelingProductionDetails.Product.MaxYield);
                                    decimal peelingOrderRolls = Math.Floor(existingPeelingLogs * calPerBl);

                                    rollsReqUpdate = peelingOrderRolls + peelingCompletedRolls;

                                    rollsReqUpdate = pendingOrder.Qty - rollsReqUpdate;

                                    if (rollsReqUpdate > 0)//Update
                                    {
                                        ps.Qty = rollsReqUpdate;
                                        ps.BlockLogQty = Math.Ceiling(ps.Qty / calPerBl);
                                    }
                                    else
                                    {
                                        ps.Qty = 0;
                                        ps.BlockLogQty = 0;
                                    }

                                    int res1 = DBAccess.UpdatePendingSlitPeel(ps, TotalYieldCut);
                                    if (res1 > 0)
                                    {
                                        Console.WriteLine("PendingSlitPeel has been updated back 1");
                                    }

                                    //Add to re-rolling
                                    if (TotalYieldCut >= peelingProductionDetails.Product.Tile.MaxYield)
                                    {
                                        //Get Re-Rolling completed rolls-TODO

                                        //List<ReRollingOrder> exReRollingList = DBAccess.GetReRollingOrdersByID(peelingProductionDetails.Order.OrderNo, peelingProductionDetails.Product.ProductID, peelingProductionDetails.Product.RawProduct.RawProductID);
                                        //decimal rollSum = exReRollingList.Sum(x => x.Rolls);                                      
                                        //decimal dollarValSum = exReRollingList.Sum(x => x.DollarValue);

                                        //ReRollingOrder remainingRRO = new ReRollingOrder();
                                        //remainingRRO.Rolls = (pendingOrder.Qty - rollSum) < 0 ? 0 : (pendingOrder.Qty - rollSum);
                                        //remainingRRO.DollarValue = ((pendingOrder.Qty * peelingProductionDetails.Product.ProductPrice) - dollarValSum) < 0 ? 0 : ((pendingOrder.Qty * peelingProductionDetails.Product.ProductPrice) - dollarValSum); 

                                        ReRollingOrder reRolling = new ReRollingOrder();
                                        reRolling.Order = peelingProductionDetails.Order;
                                        reRolling.Product = peelingProductionDetails.Product;

                                        //if(remainingRRO.Rolls < Math.Floor(TotalYieldCut / peelingProductionDetails.Product.MaxYield))
                                        //{
                                        //    reRolling.Rolls = remainingRRO.Rolls;
                                        //    reRolling.DollarValue = remainingRRO.Rolls * peelingProductionDetails.Product.ProductPrice;
                                        //}
                                        //else if (remainingRRO.Rolls >= Math.Floor(TotalYieldCut / peelingProductionDetails.Product.MaxYield))
                                        //{
                                        reRolling.Rolls = Math.Floor(TotalYieldCut / peelingProductionDetails.Product.Tile.MaxYield);
                                        reRolling.DollarValue = reRolling.Rolls * peelingProductionDetails.Product.UnitPrice;
                                        //}                                     
                                        ProductionManager.AddToReRolling(reRolling);
                                    }
                                    else
                                    {
                                        Msg.Show("This roll needs to be re-rolled but the cut yield is " + TotalYieldCut.ToString("G29") + "L/m." + System.Environment.NewLine + "It is required to have at least " + peelingProductionDetails.Product.Tile.MaxYield.ToString("G29") + "L/m to Re-Roll" + System.Environment.NewLine + "Please contact your superviser", "Cannot Send To Re-Rolling", MsgBoxButtons.OK, MsgBoxImage.Information_Red, MsgBoxResult.Yes);
                                    }

                                    //Update PendingSlitPeel

                                }
                            }
                        }
                        else
                        {
                            Msg.Show("There has been a problem and the log was not completed!", "Problem Occured", MsgBoxButtons.OK, MsgBoxImage.Error, MsgBoxResult.Yes);
                        }
                    }
                    CloseForm();
                }


            }
        }

        private decimal BlockLogCalculator(decimal qty, decimal yield)
        {
            decimal res = 0;
            if (yield != 0)
            {
                res = Math.Round(qty / yield, 2);
            }
            return res;
        }
        private void CloseForm()
        {
            if (Closed != null)
            {
                Closed();
            }
        }

        private void PrintPeelingSlip()
        {
            if (TotalYieldCut == 0)
            {
                Msg.Show("Enter the total yield cut in L/m", "Input Required", MsgBoxButtons.OK, MsgBoxImage.Error, MsgBoxResult.Yes);
            }
            else if (TotalYieldCut > updatedMaxYield)
            {
                Msg.Show("The total yield cut cannot be greater than " + updatedMaxYield + "L/m", "Invalid Input", MsgBoxButtons.OK, MsgBoxImage.Error, MsgBoxResult.Yes);
            }
            else
            {
                if (Msg.Show("Are you sure you want to print peeling slip?", "Printing Peeling Slip", MsgBoxButtons.YesNo, MsgBoxImage.Warning, MsgBoxResult.Yes) == MsgBoxResult.Yes)
                {
                    PrintPeelingSlipPDF ppspdf = new PrintPeelingSlipPDF(this);
                    ppspdf.CreateDocument();
                }
            }
        }

        #region PUBLIC PROPERTIES

        public string HeaderStr
        {
            get { return _headerStr; }
            set { _headerStr = value; RaisePropertyChanged("HeaderStr"); }
        }
        public string SubHeaderStr
        {
            get { return _subHeaderStr; }
            set { _subHeaderStr = value; RaisePropertyChanged("SubHeaderStr"); }
        }
        public string HdrTotalYieldCut
        {
            get { return _hdrTotalYieldCut; }
            set { _hdrTotalYieldCut = value; RaisePropertyChanged("HdrTotalYieldCut"); }
        }


        public decimal TotalYieldCut
        {
            get
            {
                return _totalYieldCut;
            }
            set
            {
                _totalYieldCut = value;
                RaisePropertyChanged("TotalYieldCut");
                if (TotalYieldCut > 0)
                {
                    TotYieldCutPrint = true;
                    EnDisSubmit = true;
                }
                else
                {
                    TotYieldCutPrint = false;
                    EnDisSubmit = false;
                }

            }
        }

        public bool TotYieldCutPrint
        {
            get { return _totYieldCutPrint; }
            set { _totYieldCutPrint = value; RaisePropertyChanged("TotYieldCutPrint"); }
        }

        public bool EnDisSubmit
        {
            get { return _enDisSubmit; }
            set
            {
                _enDisSubmit = value; RaisePropertyChanged("EnDisSubmit");
            }
        }


        #endregion

        #region COMMANDS


        public ICommand CloseCommand
        {
            get
            {
                return _closeCommand ?? (_closeCommand = new CommandHandler(() => CloseForm(), true));
            }
        }

        public ICommand SubmitCommand
        {
            get
            {
                return _submitCommand ?? (_submitCommand = new CommandHandler(() => SubmitOrder(), true));
            }
        }

        public ICommand PrintYieldCommand
        {
            get
            {
                return _printYieldCommand ?? (_printYieldCommand = new CommandHandler(() => PrintPeelingSlip(), true));
            }
        }



        #endregion
    }
}
