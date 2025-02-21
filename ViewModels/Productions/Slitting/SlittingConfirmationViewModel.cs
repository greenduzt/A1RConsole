using A1RConsole.Bases;
using A1RConsole.Commands;
using A1RConsole.Core;
using A1RConsole.DB;
using A1RConsole.Models.Orders;
using A1RConsole.Models.Production;
using A1RConsole.Models.Production.Slitting;
using A1RConsole.Models.Products;
using MsgBox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace A1RConsole.ViewModels.Productions.Slitting
{
    public class SlittingConfirmationViewModel : ViewModelBase
    {
        public event Action Closed;
        private DelegateCommand _closeCommand;
        private ICommand _printYieldCommand;
        private ICommand _printOffSpecCommand;
        private ICommand _submitCommand;
        private decimal _totalYieldCut;
        private decimal _offSpecTiles;
        private decimal _shreddedTiles;
        private string _shreddedReasonsVisiblity;
        private string _shreddedLabelMargin;
        private string _shreddedTextBoxMargin;
        //private decimal _extraTiles;
        private string _tileForeGColor;
        private string _shreddedGridMargin;
        private bool _isContaminated;
        private bool _isLiftedOffBoard;
        private bool _isUnevenThickness;
        private bool _isTooThick;
        private bool _isTooThin;
        private bool _isStonelines;
        private bool _isDamaged;
        private bool _isOther;
        private bool _isOperatorError;

        private bool _isContaminatedShredded;
        private bool _isLiftedOffBoardShredded;
        private bool _isUnevenThicknessShredded;
        private bool _isTooThickShredded;
        private bool _isTooThinShredded;
        private bool _isStonelinesShredded;
        private bool _isDamagedShredded;
        private bool _isOtherShredded;
        private bool _isOperatorErrorShredded;

        private bool canExecute;
        private bool _totYieldCutPrint;
        private bool _offSpecPrint;
        private bool _offSpecEnabled;
        private bool _totYieldCutEnabled;
        private bool _shreddedEnabled;
        private bool _submitEnabled;
        private List<int> _maxBlockCount;
        private int _noOfBlocksSlit;
        private decimal blocksToUpdate;
        private decimal tilesToUpdate;
        private decimal soTiles;
        private decimal soBlocks;
        private decimal soDollarVal;
        private decimal pspTiles;
        private decimal pspBlocks;
        private decimal pspDollarVal;
        public int currentShift;
        private decimal totMaxYieldCut;
        private string _offSpeccReasonsVisiblity;
        public SlittingOrder slittingProductionDetails { get; set; }
        public decimal blocksCompleted;
        public decimal dollarsCompleted;

        public SlittingConfirmationViewModel(SlittingOrder so)
        {
            //slittingProductionDetails = so;
            //_closeCommand = new DelegateCommand(CloseForm);

            //canExecute = true;
            //MaxBlockCount = new List<int>();
            //OffSpeccReasonsVisiblity = "Collapsed";
            //_shreddedReasonsVisiblity = "Collapsed";
            //ShreddedLabelMargin = "122,169,0,0";
            //ShreddedTextBoxMargin = "271,172,0,0";
            //ShreddedGridMargin = "5,189,-5,0";
            //decimal maxB = slittingProductionDetails.Blocks;

            //for (int i = 0; i <= maxB; i++)
            //{
            //    MaxBlockCount.Add(i);
            //}
            //NoOfBlocksSlit = 0;
            //OffSpecEnabled = false;
            //TotYieldCutEnabled = false;
            //ShreddedEnabled = false;
            //SubmitEnabled = false;
        }

        public void SubmitOrder()
        {
            //totMaxYieldCut = Math.Floor(NoOfBlocksSlit * slittingProductionDetails.Product.Tile.MaxYield);

            //if (NoOfBlocksSlit == 0)
            //{
            //    Msg.Show("Select the no of blocks slit", "Selection Required", MsgBoxButtons.OK, MsgBoxImage.Error, MsgBoxResult.Yes);
            //}
            //else if (OffSpecTiles == 0 && TotalYieldCut == 0 && ShreddedTiles == 0)
            //{
            //    Msg.Show("Input required for either of yield cut, Off-Spec or Shredding tiles", "Input Required", MsgBoxButtons.OK, MsgBoxImage.Error, MsgBoxResult.Yes);
            //}
            //else if (TotalYieldCut > totMaxYieldCut)
            //{
            //    Msg.Show("Total yield cut cannot be greater than " + totMaxYieldCut + " tiles", "Invalid Input", MsgBoxButtons.OK, MsgBoxImage.Error, MsgBoxResult.Yes);
            //}
            //else if ((TotalYieldCut + OffSpecTiles) > totMaxYieldCut)
            //{
            //    Msg.Show("Total for both yield cut and off-spec cannot be greater than " + totMaxYieldCut + " tiles", "Invalid Input", MsgBoxButtons.OK, MsgBoxImage.Error, MsgBoxResult.Yes);
            //}
            //else if ((TotalYieldCut + ShreddedTiles) > totMaxYieldCut)
            //{
            //    Msg.Show("Total for both yield cut and shredded cannot be greater than " + totMaxYieldCut + " tiles", "Invalid Input", MsgBoxButtons.OK, MsgBoxImage.Error, MsgBoxResult.Yes);
            //}
            //else if ((OffSpecTiles != 0) && (IsLiftedOffBoard == false && IsUnevenThickness == false && IsTooThick == false && IsTooThin == false && IsStonelines == false && IsDamaged == false && IsContaminated == false && IsOperatorError == false && IsOther == false))
            //{
            //    Msg.Show("Tick reasons for adding tiles to Off-Spec", "Selection Required", MsgBoxButtons.OK, MsgBoxImage.Error, MsgBoxResult.Yes);
            //}
            //else if ((ShreddedTiles != 0) && (IsLiftedOffBoardShredded == false && IsUnevenThicknessShredded == false && IsTooThickShredded == false && IsTooThinShredded == false && IsStonelinesShredded == false && IsDamagedShredded == false && IsContaminatedShredded == false && IsOperatorErrorShredded == false && IsOtherShredded == false))
            //{
            //    Msg.Show("Tick reasons for adding tiles to Shredding", "Selection Required", MsgBoxButtons.OK, MsgBoxImage.Error, MsgBoxResult.Yes);
            //}
            //else
            //{
            //    ShiftManager sm = new ShiftManager();
            //    SlittingOrder so1 = DBAccess.GetSlittingOrderByID(slittingProductionDetails.ID);
            //    PendingSlitPeel ps = DBAccess.GetPendingSlittingOrder(this);
            //    PendingOrder po = DBAccess.GetPendingOrder(slittingProductionDetails.Order.OrderNo, slittingProductionDetails.Product.RawProduct.RawProductID, slittingProductionDetails.Product.ProductID);
            //    currentShift = sm.GetCurrentShift();

            //    /**Updating SlittingOrder**/
            //    SlittingOrder soUpdate = new SlittingOrder();
            //    soUpdate.Blocks = so1.Blocks - NoOfBlocksSlit;
            //    soUpdate.Qty = (so1.Qty - Math.Floor((NoOfBlocksSlit * slittingProductionDetails.Product.Tile.MaxYield))) < 0 ? 0 : so1.Qty - Math.Floor((NoOfBlocksSlit * slittingProductionDetails.Product.MaxYield));
            //    soUpdate.DollarValue = so1.DollarValue - (Math.Floor((NoOfBlocksSlit * slittingProductionDetails.Product.Tile.MaxYield)) * slittingProductionDetails.Product.UnitPrice) < 0 ? 0 : so1.DollarValue - (Math.Floor((NoOfBlocksSlit * slittingProductionDetails.Product.MaxYield)) * slittingProductionDetails.Product.UnitPrice);
            //    //Update SlittingOrder
            //    int r = DBAccess.UpdateSlittingOrder(this, soUpdate);
            //    if (r > 0)
            //    {
            //        /**Updating PendingSlitPeel**/
            //        List<SlittingOrder> slittingOrderList = DBAccess.GetAllSlittingOrderByID(slittingProductionDetails.Order.OrderNo, slittingProductionDetails.Product.ProductID, slittingProductionDetails.Product.RawProduct.RawProductID);
            //        decimal existingTiles = slittingOrderList.Sum(x => x.Qty);
            //        blocksCompleted = BlockLogCalculator(TotalYieldCut, slittingProductionDetails.Product.Tile.MaxYield);
            //        dollarsCompleted = TotalYieldCut * slittingProductionDetails.Product.UnitPrice;

            //        //Calculating difference to update PendingSlitPeel
            //        decimal totTilesCompleted = 0;
            //        Tuple<decimal, decimal> e1 = DBAccess.GetTotalSlittingCompleted(slittingProductionDetails.Order.OrderNo, slittingProductionDetails.Product.RawProduct.RawProductID, slittingProductionDetails.Product.ProductID);
            //        totTilesCompleted = e1.Item1;
            //        pspTiles = (po.Qty - (totTilesCompleted + existingTiles)) > 0 ? po.Qty - (totTilesCompleted + existingTiles) : 0;

            //        pspBlocks = BlockLogCalculator(pspTiles, slittingProductionDetails.Product.Tile.MaxYield);
            //        //Now update PendingSlitPeel table
            //        PendingSlitPeel psp = new PendingSlitPeel();
            //        psp.Order = new Order() { OrderNo = slittingProductionDetails.Order.OrderNo };
            //        psp.Product = new Product() { ProductID = slittingProductionDetails.Product.ProductID, RawProduct = new RawProduct() { RawProductID = slittingProductionDetails.Product.RawProduct.RawProductID } };
            //        psp.BlockLogQty = BlockLogCalculator(pspTiles, slittingProductionDetails.Product.Tile.MaxYield);
            //        psp.Qty = pspTiles;
            //        DBAccess.UpdatePendingSlitPeel(psp);

            //        //Check if the order has been completed
            //        Tuple<decimal, decimal> e2 = DBAccess.GetTotalSlittingCompleted(slittingProductionDetails.Order.OrderNo, slittingProductionDetails.Product.RawProduct.RawProductID, slittingProductionDetails.Product.ProductID);
            //        totTilesCompleted = e2.Item1;
            //        PendingOrder po2 = DBAccess.GetPendingOrder(slittingProductionDetails.Order.OrderNo, slittingProductionDetails.Product.RawProduct.RawProductID, slittingProductionDetails.Product.ProductID);
            //        if (totTilesCompleted >= po2.Qty)
            //        {
            //            //Update PendingOrder status to SlittingCompleted
            //            int s2 = DBAccess.UpdatePendingOrder(slittingProductionDetails.Order.OrderNo, slittingProductionDetails.Product.RawProduct.RawProductID, slittingProductionDetails.Product.ProductID, "SlittingCompleted");
            //        }

            //        //Display message if the ordered qty has not been met
            //        if (po.BlockLogQty == e2.Item2 && po.Qty > e2.Item1)
            //        {
            //            //Update PendingSlitPeel active =false
            //            DBAccess.UpdatePendingSlitPeelActive(psp, false);

            //            Msg.Show("Total blocks of " + e2.Item2.ToString("G29") + " have been slit for the Order - " + slittingProductionDetails.Order.OrderNo + System.Environment.NewLine +
            //                     "The total no of " + e2.Item1.ToString("G29") + " tiles were slit for " + slittingProductionDetails.Product.ProductDescription + System.Environment.NewLine +
            //                     "The order requires another " + (po.Qty - e2.Item1).ToString("G29") + " tiles" + System.Environment.NewLine +
            //                     "Please contact administration", "Ordered qty has not been met for the order - " + slittingProductionDetails.Order.OrderNo, MsgBoxButtons.OK, MsgBoxImage.Information_Red);
            //        }
            //        else if (po.BlockLogQty == e2.Item2 && po.Qty <= e2.Item1)
            //        {
            //            //Update PendingSlitPeel active =false
            //            DBAccess.UpdatePendingSlitPeelActive(psp, false);
            //        }
            //    }
            //    else
            //    {
            //        Msg.Show("Problem occured while processing this request! Please close the application and re-open", "Problem Occured", MsgBoxButtons.OK, MsgBoxImage.Error);
            //    }

            //    CloseForm();
            //}
        }

        private decimal BlockLogCalculator(decimal qty, decimal yield)
        {
            decimal res = 0;
            if (yield != 0)
            {
                res = Math.Ceiling(qty / yield);
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


        private void PrintYieldOrder()
        {
            decimal combinedVal = 0;
            totMaxYieldCut = Math.Floor(NoOfBlocksSlit * slittingProductionDetails.Product.Tile.MaxYield);
            combinedVal = TotalYieldCut + OffSpecTiles;

            if (NoOfBlocksSlit == 0)
            {
                Msg.Show("Select the no of blocks slit", "Selection Required", MsgBoxButtons.OK, MsgBoxImage.Error, MsgBoxResult.Yes);
            }
            else if (TotalYieldCut == 0)
            {
                Msg.Show("Total yield required", "Input Required", MsgBoxButtons.OK, MsgBoxImage.Error, MsgBoxResult.Yes);
            }
            else if (TotalYieldCut > totMaxYieldCut)
            {
                Msg.Show("Total yield cut cannot be greater than " + totMaxYieldCut + " tiles", "Invalid Input", MsgBoxButtons.OK, MsgBoxImage.Error, MsgBoxResult.Yes);
            }
            else if (OffSpecTiles > totMaxYieldCut)
            {
                Msg.Show("Off - Spec tiles cannot be greater than " + totMaxYieldCut + " tiles", "Invalid Input", MsgBoxButtons.OK, MsgBoxImage.Error, MsgBoxResult.Yes);
            }
            else if (combinedVal > totMaxYieldCut)
            {
                Msg.Show("Total of both yield cut and Off-Spec cannot be greater than " + totMaxYieldCut + " tiles", "Invalid Input", MsgBoxButtons.OK, MsgBoxImage.Error, MsgBoxResult.Yes);
            }
            else
            {
                //if (Msg.Show("Are you sure you want to print slitting slip?", "Printing Slitting Slip", MsgBoxButtons.YesNo, MsgBoxImage.Warning, MsgBoxResult.Yes) == MsgBoxResult.Yes)
                //{
                //    bool isOffSpec = false;
                //    PrintSlittingOrder printOrder = new PrintSlittingOrder(this, isOffSpec);
                //}
            }
        }


        private void PrintOffSpecOrder()
        {
            //decimal combinedVal = 0;
            ////totMaxYieldCut = Math.Floor(NoOfBlocksSlit * slittingProductionDetails.Product.Tile.TilePerBlock);
            //combinedVal = TotalYieldCut + OffSpecTiles;

            //if (NoOfBlocksSlit == 0)
            //{
            //    Msg.Show("Select the no of blocks slit", "Selection Required", MsgBoxButtons.OK, MsgBoxImage.Error, MsgBoxResult.Yes);
            //}
            //else if (OffSpecTiles == 0)
            //{
            //    Msg.Show("Off-Spec tiles required", "Input Required", MsgBoxButtons.OK, MsgBoxImage.Error, MsgBoxResult.Yes);
            //}
            //else if (TotalYieldCut > totMaxYieldCut)
            //{
            //    Msg.Show("Total yield cut cannot be greater than " + totMaxYieldCut + " tiles", "Invalid Input", MsgBoxButtons.OK, MsgBoxImage.Error, MsgBoxResult.Yes);
            //}
            //else if (OffSpecTiles > totMaxYieldCut)
            //{
            //    Msg.Show("Off - Spec tiles cannot be greater than " + totMaxYieldCut + " tiles", "Invalid Input", MsgBoxButtons.OK, MsgBoxImage.Error, MsgBoxResult.Yes);
            //}
            //else if (combinedVal > totMaxYieldCut)
            //{
            //    Msg.Show("Total of both yield cut and Off-Spec cannot be greater than " + totMaxYieldCut + " tiles", "Invalid Input", MsgBoxButtons.OK, MsgBoxImage.Error, MsgBoxResult.Yes);
            //}
            //else
            //{
            //    if (Msg.Show("Are you sure you want to print Off-Spec slip?", "Printing Off-Spec Slip", MsgBoxButtons.YesNo, MsgBoxImage.Warning, MsgBoxResult.Yes) == MsgBoxResult.Yes)
            //    {
            //        bool isOffSpec = true;
            //        PrintSlittingOrder printOrder = new PrintSlittingOrder(this, isOffSpec);
            //    }
            //}
        }

        private void EnDisSubmit()
        {
            if (TotalYieldCut == 0 && OffSpecTiles == 0 && ShreddedTiles == 0)
            {
                SubmitEnabled = false;
            }
            else
            {
                SubmitEnabled = true;
            }
        }

        #region PUBLIC PROPERTIES



        public bool SubmitEnabled
        {
            get
            {
                return _submitEnabled;
            }
            set
            {
                _submitEnabled = value;
                RaisePropertyChanged("SubmitEnabled");
            }
        }

        public bool TotYieldCutEnabled
        {
            get
            {
                return _totYieldCutEnabled;
            }
            set
            {
                _totYieldCutEnabled = value;
                RaisePropertyChanged("TotYieldCutEnabled");
            }
        }

        public bool OffSpecEnabled
        {
            get
            {
                return _offSpecEnabled;
            }
            set
            {
                _offSpecEnabled = value;
                RaisePropertyChanged("OffSpecEnabled");
            }
        }


        public bool ShreddedEnabled
        {
            get
            {
                return _shreddedEnabled;
            }
            set
            {
                _shreddedEnabled = value;
                RaisePropertyChanged("ShreddedEnabled");
            }
        }


        public bool TotYieldCutPrint
        {
            get
            {
                return _totYieldCutPrint;
            }
            set
            {
                _totYieldCutPrint = value;
                RaisePropertyChanged("TotYieldCutPrint");
            }
        }

        public bool OffSpecPrint
        {
            get
            {
                return _offSpecPrint;
            }
            set
            {
                _offSpecPrint = value;
                RaisePropertyChanged("OffSpecPrint");
            }
        }




        public string ShreddedLabelMargin
        {
            get
            {
                return _shreddedLabelMargin;
            }
            set
            {
                _shreddedLabelMargin = value;
                RaisePropertyChanged("ShreddedLabelMargin");

            }
        }

        public string ShreddedTextBoxMargin
        {
            get
            {
                return _shreddedTextBoxMargin;
            }
            set
            {
                _shreddedTextBoxMargin = value;
                RaisePropertyChanged("ShreddedTextBoxMargin");

            }
        }


        public string ShreddedReasonsVisiblity
        {
            get
            {
                return _shreddedReasonsVisiblity;
            }
            set
            {
                _shreddedReasonsVisiblity = value;
                RaisePropertyChanged("ShreddedReasonsVisiblity");

            }
        }



        public string OffSpeccReasonsVisiblity
        {
            get
            {
                return _offSpeccReasonsVisiblity;
            }
            set
            {
                _offSpeccReasonsVisiblity = value;
                RaisePropertyChanged("OffSpeccReasonsVisiblity");

            }
        }

        public int NoOfBlocksSlit
        {
            get
            {
                return _noOfBlocksSlit;
            }
            set
            {
                _noOfBlocksSlit = value;
                RaisePropertyChanged("NoOfBlocksSlit");

                if (NoOfBlocksSlit > 0)
                {

                    OffSpecEnabled = true;
                    TotYieldCutEnabled = true;
                    ShreddedEnabled = true;
                }
                else
                {
                    OffSpecEnabled = false;
                    TotYieldCutEnabled = false;
                    ShreddedEnabled = false;
                    TotalYieldCut = 0;
                    OffSpecTiles = 0;
                    ShreddedTiles = 0;
                }

            }
        }
        public List<int> MaxBlockCount
        {
            get
            {
                return _maxBlockCount;
            }
            set
            {
                _maxBlockCount = value;
                RaisePropertyChanged("MaxBlockCount");

            }
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
                }
                else
                {
                    TotYieldCutPrint = false;
                }

                EnDisSubmit();
            }
        }

        public decimal OffSpecTiles
        {
            get
            {
                return _offSpecTiles;
            }
            set
            {
                _offSpecTiles = value;
                RaisePropertyChanged("OffSpecTiles");
                if (OffSpecTiles > 0)
                {
                    OffSpecPrint = true;
                    OffSpeccReasonsVisiblity = "Visible";
                    ShreddedLabelMargin = "121,264,0,0";
                    ShreddedTextBoxMargin = "271,264,0,0";

                    if (ShreddedTiles > 0)
                    {
                        ShreddedGridMargin = "5,292,-5,0";
                    }
                    else
                    {
                        ShreddedGridMargin = "5,193,-5,0";
                    }
                }
                else
                {
                    OffSpecPrint = false;
                    OffSpeccReasonsVisiblity = "Collapsed";
                    ShreddedLabelMargin = "122,169,0,0";
                    ShreddedTextBoxMargin = "271,172,0,0";
                    ShreddedGridMargin = "5,193,-5,0";
                }
                EnDisSubmit();

            }
        }


        public decimal ShreddedTiles
        {
            get
            {
                return _shreddedTiles;
            }
            set
            {
                _shreddedTiles = value;
                RaisePropertyChanged("ShreddedTiles");

                if (ShreddedTiles > 0)
                {
                    ShreddedReasonsVisiblity = "Visible";

                    if (OffSpecTiles > 0)
                    {
                        ShreddedGridMargin = "5,292,-5,0";
                    }
                    else
                    {
                        ShreddedGridMargin = "5,193,-5,0";
                    }
                }
                else
                {
                    ShreddedReasonsVisiblity = "Collapsed";
                }
                EnDisSubmit();
            }
        }



        public string ShreddedGridMargin
        {
            get
            {
                return _shreddedGridMargin;
            }
            set
            {
                _shreddedGridMargin = value;
                RaisePropertyChanged("ShreddedGridMargin");
            }
        }

        //public decimal ExtraTiles 
        //{
        //    get
        //    {
        //        return _extraTiles;
        //    }
        //    set
        //    {
        //        _extraTiles = value ;
        //        RaisePropertyChanged("ExtraTiles");            
        //    }
        //}



        public string TileForeGColor
        {
            get
            {
                return _tileForeGColor;
            }
            set
            {
                _tileForeGColor = value;
                RaisePropertyChanged("TileForeGColor");
            }
        }

        public bool IsContaminated
        {
            get
            {
                return _isContaminated;
            }
            set
            {
                _isContaminated = value;
                RaisePropertyChanged("IsContaminated");
            }
        }

        public bool IsLiftedOffBoard
        {
            get
            {
                return _isLiftedOffBoard;
            }
            set
            {
                _isLiftedOffBoard = value;
                RaisePropertyChanged("IsLiftedOffBoard");
            }
        }

        public bool IsUnevenThickness
        {
            get
            {
                return _isUnevenThickness;
            }
            set
            {
                _isUnevenThickness = value;
                RaisePropertyChanged("IsUnevenThickness");
            }
        }

        public bool IsTooThick
        {
            get
            {
                return _isTooThick;
            }
            set
            {
                _isTooThick = value;
                RaisePropertyChanged("IsTooThick");
            }
        }

        public bool IsTooThin
        {
            get
            {
                return _isTooThin;
            }
            set
            {
                _isTooThin = value;
                RaisePropertyChanged("IsTooThin");
            }
        }

        public bool IsStonelines
        {
            get
            {
                return _isStonelines;
            }
            set
            {
                _isStonelines = value;
                RaisePropertyChanged("IsStonelines");

            }
        }
        public bool IsDamaged
        {
            get
            {
                return _isDamaged;
            }
            set
            {
                _isDamaged = value;
                RaisePropertyChanged("IsDamaged");
            }
        }

        public bool IsOther
        {
            get
            {
                return _isOther;
            }
            set
            {
                _isOther = value;
                RaisePropertyChanged("IsOther");
            }
        }

        public bool IsOperatorError
        {
            get
            {
                return _isOperatorError;
            }
            set
            {
                _isOperatorError = value;
                RaisePropertyChanged("IsOperatorError");

            }
        }

        public bool IsContaminatedShredded
        {
            get
            {
                return _isContaminatedShredded;
            }
            set
            {
                _isContaminatedShredded = value;
                RaisePropertyChanged("IsContaminatedShredded");
            }
        }

        public bool IsLiftedOffBoardShredded
        {
            get
            {
                return _isLiftedOffBoardShredded;
            }
            set
            {
                _isLiftedOffBoardShredded = value;
                RaisePropertyChanged("IsLiftedOffBoardShredded");
            }
        }

        public bool IsUnevenThicknessShredded
        {
            get
            {
                return _isUnevenThicknessShredded;
            }
            set
            {
                _isUnevenThicknessShredded = value;
                RaisePropertyChanged("IsUnevenThicknessShredded");
            }
        }

        public bool IsTooThickShredded
        {
            get
            {
                return _isTooThickShredded;
            }
            set
            {
                _isTooThickShredded = value;
                RaisePropertyChanged("IsTooThickShredded");
            }
        }

        public bool IsTooThinShredded
        {
            get
            {
                return _isTooThinShredded;
            }
            set
            {
                _isTooThinShredded = value;
                RaisePropertyChanged("IsTooThinShredded");
            }
        }

        public bool IsStonelinesShredded
        {
            get
            {
                return _isStonelinesShredded;
            }
            set
            {
                _isStonelinesShredded = value;
                RaisePropertyChanged("IsStonelinesShredded");

            }
        }
        public bool IsDamagedShredded
        {
            get
            {
                return _isDamagedShredded;
            }
            set
            {
                _isDamagedShredded = value;
                RaisePropertyChanged("IsDamagedShredded");
            }
        }

        public bool IsOtherShredded
        {
            get
            {
                return _isOtherShredded;
            }
            set
            {
                _isOtherShredded = value;
                RaisePropertyChanged("IsOtherShredded");
            }
        }

        public bool IsOperatorErrorShredded
        {
            get
            {
                return _isOperatorErrorShredded;
            }
            set
            {
                _isOperatorErrorShredded = value;
                RaisePropertyChanged("IsOperatorErrorShredded");

            }
        }





        #endregion

        #region COMMANDS

        public ICommand SubmitCommand
        {
            get
            {
                return _submitCommand ?? (_submitCommand = new CommandHandler(() => SubmitOrder(), canExecute));
            }
        }

        public ICommand PrintYieldCommand
        {
            get
            {
                return _printYieldCommand ?? (_printYieldCommand = new CommandHandler(() => PrintYieldOrder(), canExecute));
            }
        }

        public ICommand PrintOffSpecCommand
        {
            get
            {
                return _printOffSpecCommand ?? (_printOffSpecCommand = new CommandHandler(() => PrintOffSpecOrder(), canExecute));
            }
        }

        public DelegateCommand CloseCommand
        {
            get { return _closeCommand; }
        }

        #endregion
    }
}