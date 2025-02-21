using A1RConsole.Bases;
using A1RConsole.Commands;
using A1RConsole.Core;
using A1RConsole.DB;
using A1RConsole.Models.Customers;
using A1RConsole.Models.Formulas;
using A1RConsole.Models.Meta;
using A1RConsole.Models.Products;
using A1RConsole.Models.Stock;
using A1RConsole.Models.Transactions;
using MsgBox;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;


namespace A1RConsole.Models.RawMaterials
{
    public class RawProductionDetails : ViewModelBase
    {
        public int GradingSchedulingID { get; set; }
        public string CurrentCapacityID { get; set; }
        public int ProdTimeTableID { get; set; }
        private int _RawProDetailsID;
        private RawProduct _rawProduct;
        private decimal _blockLogQty;
        private bool _moveEnabled;
        private bool _deleteEnabled;

        public string ProductionDate { get; set; }
        public DateTime PDate { get; set; }
        public int Shift { get; set; }
        public string ShiftName { get; set; }
        public string OriginType { get; set; }
        public int PrintCounter { get; set; }
        public TransactionLog TransactionLog { get; set; }
        public bool MachineActive { get; set; }
        public bool DayActive { get; set; }
        public bool EveningActive { get; set; }
        public bool NightActive { get; set; }
        public bool IsExpanded { get; set; }
        public string ItemPresenterVivibility { get; set; }
        //public bool IsExpandedEnabled { get; set; }



        public DateTime FreightArrDate { get; set; }
        public DateTime OrderRequiredDate { get; set; }
        private bool _activeOrder;
        public string FreightArrTime { get; set; }
        private Customer _customer;
        private string _isOrderActive;
        private string _activeText;
        private string _activeOrderBehaviour;
        private int _ordertype;
        private string _rowBackgroundColour;
        private string _freightName;
        private bool _freightTimeAvailable;
        private bool _freightDateAvailable;
        private string _isNotesVisible;
        private string _notes;
        private string _salesOrder;
        private bool _reqDateSelected;
        private string _salesOrderVisibility;
        public string FreightDateTimeVisibility { get; set; }
        public int SalesOrderId { get; set; }
        public string FreightVisiblity { get; set; }
        public string ArrivalDateTime { get; set; }
        public string RequiredDate { get; set; }
        public string IsReqDateVisible { get; set; }
        private string _shiftText;
        private string _shiftBtnBackColour;
        private bool _convertEnable;
        private string _itemBackgroundColour;
        //private ICommand _acceptCommand;
        private ICommand _moveCommand;
        private ICommand _deleteCommand;
        private ICommand _convertCommand;

        private bool canExecute;

        public RawProductionDetails()
        {
            canExecute = true;
            RowBackgroundColour = "#ffffff";
            IsNotesVisible = "Collapsed";
            MoveEnabled = true;
            DeleteEnabled = true;
            IsReqDateVisible = "Collapsed";
            SalesOrderVisibility = "Collapsed";
            ReqDateSelected = false;
        }


        private bool EnableDisableGradingBttn(decimal reqAmo)
        {
            bool res = false;

            if (reqAmo == 0)
            {
                res = false;
            }
            else if (reqAmo > 0)
            {
                res = true;
            }

            return res;
        }

        private string ChangeTextGradingBttn(decimal reqAmo)
        {
            string res = string.Empty;

            if (reqAmo == 0)
            {
                res = "Allocatated";
            }
            else if (reqAmo > 0)
            {
                res = "Allocate";
            }
            return res;
        }

        private void MoveOrder()
        {
            List<SystemParameters> systemParameters = DBAccess.GetAllSystemParametersByValue(true);
            bool has = systemParameters.Any(x => x.Value == true);
            if (has == true)
            {
                Msg.Show("System is busy, performing some operations. Please try again in few minutes ", "System Busy", MsgBoxButtons.OK, MsgBoxImage.Information, MsgBoxResult.Yes);
            }
            else
            {
                int sp1 = DBAccess.UpdateSystemParameter("MoveOrder", true);
                if (sp1 > 0)
                {
                    var childWindow = new ChildWindow();
                    childWindow.ShowShiftOrderWindow(this, OriginType);
                    int sp2 = DBAccess.UpdateSystemParameter("MoveOrder", false);
                }
            }
        }

        private void DeleteOrder()
        {
            List<SystemParameters> systemParameters = DBAccess.GetAllSystemParametersByValue(true);
            bool has = systemParameters.Any(x => x.Value == true);
            if (has == true)
            {
                Msg.Show("System is busy, performing some operations. Please try again in few minutes ", "System Busy", MsgBoxButtons.OK, MsgBoxImage.Information, MsgBoxResult.Yes);
            }
            else
            {

                int sp1 = DBAccess.UpdateSystemParameter("DeleteOrder", true);
                if (sp1 > 0)
                {
                    if (Msg.Show("Are you sure, you want to delete this order?", "Order Delete Confirmation", MsgBoxButtons.YesNo, MsgBoxImage.Information_Red, MsgBoxResult.Yes) == MsgBoxResult.Yes)
                    {
                        List<ShredStock> tempShredStock = new List<ShredStock>();
                        //List<FormulaOptions> formulaOptions = DBAccess.GetFormulaOptions();
                        List<ShredStock> shredStock = DBAccess.GetShredStock();
                        ObservableCollection<Formula> formulas = DBAccess.GetFormulas();
                        var f = formulas.SingleOrDefault(x => x.RawProductID == RawProduct.RawProductID);
                        if (f != null)
                        {
                            var a = shredStock.SingleOrDefault(z => z.Shred.ID == f.ProductCapacity1);
                            var b = shredStock.SingleOrDefault(y => y.Shred.ID == f.ProductCapacity2);
                            if (a != null || b != null)
                            {
                                if (a != null)
                                {
                                    tempShredStock.Add(new ShredStock() { Shred = new Shred() { ID = f.ProductCapacity1 }, Qty = a.Qty + (f.GradingWeight1 * BlockLogQty) });
                                }

                                if (b != null)
                                {
                                    tempShredStock.Add(new ShredStock() { Shred = new Shred() { ID = f.ProductCapacity2 }, Qty = b.Qty + (f.GradingWeight2 * BlockLogQty) });
                                }
                            }
                        }


                        int res = DBAccess.DeleteGradingOrder(GradingSchedulingID, tempShredStock);
                        if (res == 1)
                        {
                            string userName = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                            if (String.IsNullOrEmpty(userName))
                            {
                                userName = "Unknown";
                            }
                            TransactionLog = new TransactionLog()
                            {
                                TransDateTime = DateTime.Now,
                                Transtype = "Deleted",
                                SalesOrderID = SalesOrderId,
                                Products = new List<RawStock>()
                                    {
                                      new RawStock(){RawProductID = RawProduct.RawProductID,Qty=BlockLogQty},  
                                    },
                                CreatedBy = userName
                            };
                            int r = DBAccess.InsertTransaction(TransactionLog);
                            Msg.Show("The selected order has been successfully deleted!", "Order Deleted", MsgBoxButtons.OK, MsgBoxImage.OK, MsgBoxResult.Yes);
                        }
                        else
                        {
                            Msg.Show("There was an error and the Order has not been deleted!", "Order Deleted Failure", MsgBoxButtons.OK, MsgBoxImage.Information, MsgBoxResult.Yes);
                        }
                    }

                    int sp2 = DBAccess.UpdateSystemParameter("DeleteOrder", false);
                }
            }
        }

        private void EnableDisableShift()
        {
            List<SystemParameters> systemParameters = DBAccess.GetAllSystemParametersByValue(true);
            bool has = systemParameters.Any(x => x.Value == true);
            if (has == true)
            {
                Msg.Show("System is busy, performing some operations. Please try again in few minutes ", "System Busy", MsgBoxButtons.OK, MsgBoxImage.Information, MsgBoxResult.Yes);
            }
            else
            {

                int sp1 = DBAccess.UpdateSystemParameter("DisableEnableDates", true);
                if (sp1 > 0)
                {
                    GradingManager gm = new GradingManager();
                    if (ShiftText == "Enable")
                    {
                        if (Msg.Show("Are you sure, you want to enable this shift?", "Confirmation", MsgBoxButtons.YesNo, MsgBoxImage.Warning, MsgBoxResult.Yes) == MsgBoxResult.Yes)
                        {

                            int r = DBAccess.EnableDisableSingleShift(Convert.ToDateTime(ProductionDate), 1, true, Shift);
                            if (r > 0)
                            {
                                if (Msg.Show("Do you want to allocate existing orders to this shift?", "Shift Enabling Confirmation", MsgBoxButtons.YesNo, MsgBoxImage.Warning, MsgBoxResult.Yes) == MsgBoxResult.Yes)
                                {
                                    gm.ShiftOrders("Enable", Shift, Convert.ToDateTime(ProductionDate), "ShiftButton");
                                    ShiftText = "Disable";
                                    ShiftBtnBackColour = "Green";

                                }
                            }
                        }
                    }
                    else
                    {
                        if (Msg.Show("Are you sure, you want to disable this shift?" + System.Environment.NewLine + "Existing orders will be allocated to the next available shifts", "Shift Disabling Confirmation", MsgBoxButtons.YesNo, MsgBoxImage.Warning, MsgBoxResult.Yes) == MsgBoxResult.Yes)
                        {
                            int r = DBAccess.EnableDisableSingleShift(Convert.ToDateTime(ProductionDate), 1, false, Shift);
                            if (r > 0)
                            {
                                gm.ShiftOrders("Disable", Shift, Convert.ToDateTime(ProductionDate), "ShiftButton");
                                ShiftText = "Enable";
                                ShiftBtnBackColour = "#FFCC3700";
                            }
                        }
                    }

                    int sp2 = DBAccess.UpdateSystemParameter("DisableEnableDates", false);
                }
            }
        }

        private void ConvertOrder()
        {
            List<SystemParameters> systemParameters = DBAccess.GetAllSystemParametersByValue(true);
            bool has = systemParameters.Any(x => x.Value == true);
            if (has == true)
            {
                Msg.Show("System is busy, performing some operations. Please try again in few minutes ", "System Busy", MsgBoxButtons.OK, MsgBoxImage.Information, MsgBoxResult.Yes);
            }
            else
            {
                int sp1 = DBAccess.UpdateSystemParameter("ConvertOrder", true);
                if (sp1 > 0)
                {
                    var childWindow = new ChildWindow();
                    childWindow.ShowConvertOrderWindow(this);

                    int sp2 = DBAccess.UpdateSystemParameter("ConvertOrder", false);
                }
            }
        }

        #region PUBLIC PROPERTIES

        public Customer Customer
        {
            get { return _customer; }
            set
            {
                _customer = value;
                RaisePropertyChanged("Customer");
            }
        }

        public string FreightDescription
        {
            get { return _freightName; }
            set
            {
                _freightName = value;
                if (FreightDescription == "A1Rubber Stock")
                {
                    FreightVisiblity = "Hidden";
                    FreightDateTimeVisibility = "Hidden";
                }
                else
                {
                    FreightVisiblity = "Visible";
                    FreightDateTimeVisibility = "Visible";
                }
            }
        }




        public string ItemBackgroundColour
        {
            get { return _itemBackgroundColour; }
            set
            {
                _itemBackgroundColour = value;
                RaisePropertyChanged("ItemBackgroundColour");
            }
        }


        public string SalesOrderVisibility
        {
            get { return _salesOrderVisibility; }
            set
            {
                _salesOrderVisibility = value;
                RaisePropertyChanged("SalesOrderVisibility");
            }
        }

        public string SalesOrder
        {
            get { return _salesOrder; }
            set
            {
                _salesOrder = value;
                RaisePropertyChanged("SalesOrder");

                if (String.IsNullOrEmpty(SalesOrder))
                {
                    SalesOrderVisibility = "Collapsed";
                }
                else
                {
                    SalesOrderVisibility = "Visible";
                }
            }
        }


        public bool ConvertEnable
        {
            get { return _convertEnable; }
            set
            {
                _convertEnable = value;
                RaisePropertyChanged("ConvertEnable");
            }
        }

        public string ShiftBtnBackColour
        {
            get { return _shiftBtnBackColour; }
            set
            {
                _shiftBtnBackColour = value;
                RaisePropertyChanged("ShiftBtnBackColour");
            }
        }

        public bool ReqDateSelected
        {
            get { return _reqDateSelected; }
            set
            {
                _reqDateSelected = value;
                RaisePropertyChanged("ReqDateSelected");
            }
        }

        public string ActiveText
        {
            get { return _activeText; }
            set
            {
                _activeText = value;
                RaisePropertyChanged("ActiveText");
            }
        }

        public bool ActiveOrder
        {
            get { return _activeOrder; }
            set
            {
                _activeOrder = value;
                RaisePropertyChanged("ActiveOrder");

                if (ActiveOrder == true)
                {
                    IsOrderActive = "Visible";
                    ActiveOrderBehaviour = "Forever";
                    ActiveText = "Now Working";
                    //MoveEnabled = false;
                    //DeleteEnabled = false;
                    //ConvertEnable = false;
                }
                else
                {
                    //IsOrderActive = "Collapsed";
                    ActiveOrderBehaviour = "1x";
                    ActiveText = string.Empty;
                    //MoveEnabled = true;
                    //DeleteEnabled = true;
                    //ConvertEnable = true;
                }
            }
        }



        public string ActiveOrderBehaviour
        {
            get { return _activeOrderBehaviour; }
            set
            {
                _activeOrderBehaviour = value;
                RaisePropertyChanged("ActiveOrderBehaviour");
            }
        }

        public string RowBackgroundColour
        {
            get { return _rowBackgroundColour; }
            set
            {
                _rowBackgroundColour = value;
                RaisePropertyChanged("RowBackgroundColour");
            }
        }


        public bool FreightTimeAvailable
        {
            get { return _freightTimeAvailable; }
            set
            {
                _freightTimeAvailable = value;
                if (FreightTimeAvailable == true)
                {
                    FreightArrTime = string.Empty;
                }
            }
        }

        public bool FreightDateAvailable
        {
            get { return _freightDateAvailable; }
            set
            {
                _freightDateAvailable = value;
                if (FreightDateAvailable == true)
                {
                    FreightDateTimeVisibility = "Hidden";
                }
                else
                {
                    FreightDateTimeVisibility = "Visible";
                }
            }
        }

        public decimal BlockLogQty
        {
            get { return _blockLogQty; }
            set
            {
                _blockLogQty = value;
                RaisePropertyChanged("BlockLogQty");
            }
        }

        public int RawProDetailsID
        {
            get { return _RawProDetailsID; }
            set
            {
                _RawProDetailsID = value;
                RaisePropertyChanged("RawProDetailsID");
            }
        }

        public int OrderType
        {
            get { return _ordertype; }
            set
            {
                _ordertype = value;
                RaisePropertyChanged("OrderType");

                //Order Types:
                //1 - Urgent
                //2 - Urgent Graded 
                //3 - Normal
                //4 - Normal Graded


                if (OrderType == 1)
                {
                    IsOrderActive = "Visible";
                    RowBackgroundColour = "#cc3700";//Urgent                   
                }
                if (OrderType == 2)
                {
                    IsOrderActive = "Visible";
                    RowBackgroundColour = "#175a44";//Urgent Graded                    
                }
                if (OrderType == 3)
                {
                    RowBackgroundColour = "#ffffff";//Normal

                    if (ActiveOrder == true)
                    {
                        IsOrderActive = "Visible";
                    }
                    else
                    {
                        IsOrderActive = "Collapsed";
                    }
                }
                if (OrderType == 4)
                {
                    IsOrderActive = "Visible";
                    RowBackgroundColour = "#3cce5f";//Normal    
                    //ConvertEnable = false;
                }
                else
                {
                    //ConvertEnable = true;
                }
            }
        }


        public RawProduct RawProduct
        {
            get { return _rawProduct; }
            set
            {
                _rawProduct = value;
                RaisePropertyChanged("RawProduct");
                //if (RawProduct.RawProductID == 12)
                //{
                //    ConvertEnable = false;
                //}
                //else
                //{
                //    ConvertEnable = true;
                //}
            }
        }

        public string IsNotesVisible
        {
            get { return _isNotesVisible; }
            set
            {
                _isNotesVisible = value;
                RaisePropertyChanged("IsNotesVisible");
            }
        }

        public string Notes
        {
            get { return _notes; }
            set
            {
                _notes = value;
                RaisePropertyChanged("Notes");
                if (String.IsNullOrEmpty(Notes))
                {
                    IsNotesVisible = "Collapsed";
                }
                else
                {
                    IsNotesVisible = "Visible";
                }
            }
        }

        public string IsOrderActive
        {
            get { return _isOrderActive; }
            set
            {
                _isOrderActive = value;
                RaisePropertyChanged("IsOrderActive");

            }
        }

        public bool MoveEnabled
        {
            get { return _moveEnabled; }
            set
            {
                _moveEnabled = value;
                RaisePropertyChanged("MoveEnabled");

            }
        }

        public bool DeleteEnabled
        {
            get { return _deleteEnabled; }
            set
            {
                _deleteEnabled = value;
                RaisePropertyChanged("DeleteEnabled");

            }
        }

        public string ShiftText
        {
            get { return _shiftText; }
            set
            {
                _shiftText = value;
                RaisePropertyChanged("ShiftText");

            }
        }



        #endregion

        #region COMMANDS
        //public ICommand AcceptCommand
        //{
        //    get
        //    {
        //        return _acceptCommand ?? (_acceptCommand = new LogOutCommandHandler(() => AcceptOrder(), canExecute));
        //    }
        //}
        public ICommand MoveCommand
        {
            get
            {
                return _moveCommand ?? (_moveCommand = new CommandHandler(() => MoveOrder(), canExecute));
            }
        }
        public ICommand DeleteCommand
        {
            get
            {
                return _deleteCommand ?? (_deleteCommand = new CommandHandler(() => DeleteOrder(), canExecute));
            }
        }

        private ICommand _enDisCommand;
        public ICommand EnDisCommand
        {
            get
            {
                return _enDisCommand ?? (_enDisCommand = new CommandHandler(() => EnableDisableShift(), canExecute));
            }
        }

        public ICommand ConvertCommand
        {
            get
            {
                return _convertCommand ?? (_convertCommand = new CommandHandler(() => ConvertOrder(), canExecute));
            }
        }


        #endregion
    }
}

