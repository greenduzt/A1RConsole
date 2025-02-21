using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A1RConsole.Core
{
    public enum OrderStatus
    {
        Hold = 0, //Until recieving payment
        Release = 1, //Payment received release to warehouse (Stock Reserved)
        NoStock = 2,
        HoldNoStock = 3, //Held for no stock,awaiting until stock arrives
        HoldStockAllocated = 4,
        HoldNoCredit = 5,
        HoldNoCreditStockAllocated = 6,
        HoldNoCreditNoStock = 7,
        AwaitingStock = 8,
        InProduction = 9,
        InWarehouse = 10,
        Picking = 11,
        FinalisingShipping = 12,
        ReadyToDispatch = 13,
        Dispatched = 14,
        Return = 15,

        Grading = 16,
        Mixing = 17,
        Curing = 18,
        Peeling = 19,
        ReRolling = 20,
        Bagging = 21,
        Cancel = 22,
        None = 23,
        DispatchedCancelled = 24,
        PreparingInvoice = 25
    }

    public enum DispatchOrderStatus
    {
        Pending = 0,
        Preparing = 1,
        Finalised = 2,
        Held = 3
    }

    public enum StockReserved
    {
        Reserved = 0,
        NotReserved = 1,
        Shipped = 2,
        ReAllocated = 3,
        AwaitingStock = 4,
        Cancelled = 5,
        Returned = 6
    }

    public enum StockStatus
    {
        In,
        Out,
        NotAvailable
    }

    public enum ShiftType
    {
        Day = 1,
        Afternoon = 2,
        Night = 3
    }

    public enum VehicleWorkOrderEnum
    {
        Pending = 1,
        Completed = 2,
        Cancelled = 3
    }
    public enum VehicleWorkOrderTypesEnum
    {
        Maintenance = 1,
        Repair = 2
    }

    public enum SalesOrderStatus
    {
        Hold = 0, //Until recieving payment
        Released = 1, //Payment received release to warehouse
        NoStock = 2,
        InProduction = 3,
        InWarehouse = 4,
        Picking = 5,
        Dispatched = 6,
        AwaitingForCustomer = 7
    }

    //public enum MachineMaintenanceFreq
    //{
    //    Daily = 1,
    //    Weekly = 2,
    //    OneMonth = 3,
    //    SixMonths = 4,
    //    OneYear = 5,
    //    TwoYears = 6,
    //    Daily_Weekly = 7,
    //    Daily_OneMonth = 8,
    //    Daily_SixMonths = 9,
    //    Daily_OneYear = 10,
    //    Daily_TwoYears = 11,
    //    Daily_Weekly_OneMonth = 12,
    //    Daily_Weekly_SixMonths = 13,
    //    Daily_Weekly_OneYear = 14,
    //    Daily_Weekly_TwoYears = 15,
    //    Daily_OneMonth_SixMonths = 16,
    //    Daily_OneMonth_OneYear = 17,
    //    Daily_OneMonth_TwoYears = 18,
    //    Daily_SixMonths_OneYear = 19,
    //    Daily_SixMonths_TwoYears = 20,
    //    Daily_OneYear_TwoYears = 21,
    //    Weekly_OneMonth = 22,
    //    Weekly_SixMonths = 23,
    //    Weekly_OneYear = 24,
    //    Weekly_TwoYears = 25,
    //    Weekly_OneMonth_SixMonths = 26,
    //    Weekly_OneMonth_OneYear = 27,
    //    Weekly_OneMonth_TwoYears = 28,
    //    Weekly_SixMonths_OneYear = 29,
    //    Weekly_SixMonths_TwoYears = 30,
    //    Weekly_OneYear_TwoYears = 31,
    //    OneMonth_SixMonths = 32,
    //    OneMonth_OneYear = 33,
    //    OneMonth_TwoYears = 34,
    //    OneMonth_SixMonths_OneYear = 35,
    //    OneMonth_SixMonths_TwoYears = 36,
    //    SixMonths_OneYear = 37,
    //    SixMonths_TwoYears = 38,
    //    OneYear_TwoYears = 39,
    //    OneOff = 40
    //}

    public enum MachineMaintenanceFreq
    {
        Daily = 1,
        Weekly = 2,
        OneMonth = 3,
        SixMonths = 4,
        OneYear = 5,
        TwoYears = 6,
        OneOff = 7
    }

    public enum Locations
    {
        Warehouse = 1,
        AdminOffice = 2,
        Mixing = 3
    }

    public enum StockUpdateTypes
    {
        Adding = 1,
        Substracting = 2,
        Updating = 3
    }

    public enum LineStatus
    {
        Open = 1,
        Closed = 2
    }


    public enum ProductTransactionsStatus
    {
        SalesOrder = 1,
        Purchasing = 2,
        Shipping = 3,
        WorkOrder_Mixing = 4,
        WorkOrder_Grading = 5,
        Adjustment = 6,
        Returning = 7
    }

    public enum OrderType
    {
        Sales = 1,
        Production = 2
    }

    public enum ReceivingStatus
    {
        Approved = 1,
        Unapproved = 2,
        Cancelled = 3,
        Pending = 4,
        Select = 5
    }

    public enum OrderOriginTypes
    {
        A1RubberOnline = 1,
        Wetpour = 2,
        Sales = 3,
        Admin = 4
    }
}