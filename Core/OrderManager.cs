using A1RConsole.DB;
using A1RConsole.Models.Customers;
using A1RConsole.Models.Orders;
using A1RConsole.Models.Orders.OnlineOrders;
using A1RConsole.Models.Production.Grading;
using A1RConsole.Models.Production.Mixing;
using A1RConsole.Models.Production.Peeling;
using A1RConsole.Models.Production.Slitting;
using A1RConsole.Models.Products;
using A1RConsole.Models.RawMaterials;
using A1RConsole.Models.Stock;
using A1RConsole.ViewModels.Productions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace A1RConsole.Core
{

    public class Pair<T1, T2>
    {
        public T1 ProdID { get; set; }
        public T2 Qty { get; set; }
    }

    public class SystemError
    {
        public bool IsError { get; set; }
        public string ErrorName { get; set; }
    }

    public class OrderManager
    {
        private List<ProductStock> productStockList;
        private ObservableCollection<Customer> CustomerList;
        private List<ProductStockReserved> productStockReserved;
        private int stockLocationId;
        private List<ProductStock> prodStockUpdate;
        public OrderManager() { }

        public void InitiateSalesOrder(int slid, List<ProductStock> ps)
        {
            stockLocationId = slid;
            productStockList = new List<ProductStock>();
            productStockList = ps;
            //productStockList = DBAccess.GetProductStockByStock(stockLocationId);
            CustomerList = new ObservableCollection<Customer>();
            prodStockUpdate = new List<ProductStock>();
            CustomerList = DBAccess.GetCustomerData();
        }


        public Tuple<bool, SalesOrder, List<ProductStockReserved>, List<ProductStock>, bool, Tuple<CustomerCreditHistory, CustomerCreditActivity>,SystemError> ProcessSalesOrder1(SalesOrder so, string selectedCustomer, string userName)
        {
            bool canSubmit = false;
            bool clearFields = false;
            Tuple<SalesOrder, string, List<ProductStockReserved>, List<ProductStock>, SystemError> reservedStockEle = null;
            Tuple<CustomerCreditHistory, CustomerCreditActivity> creditTuple = null;

            //so.SalesMadeBy = "";
            so.LastModifiedBy = UserData.FirstName + " " + UserData.LastName;
            so.LastModifiedDate = DateTime.Now;
            so.SalesCompletedBy = so.LastModifiedBy;
            var data = CustomerList.SingleOrDefault(c => c.CompanyName == selectedCustomer);

            if (so.Customer.CustomerType == "Prepaid" && data != null)
            {
                so.Customer.CustomerId = data.CustomerId;
            }
            else
            {
                so.Customer.CompanyName = selectedCustomer;
                so.Customer.Debt = data != null ? data.Debt : 0;
            }

            if (so.OrderAction == OrderStatus.Release.ToString() || so.OrderAction == OrderStatus.Hold.ToString())
            {
                if (so.Customer.CustomerType == "Account")
                {
                    if (data == null)
                    {
                        canSubmit = false;
                        MessageBox.Show("Please select correct customer name", "Customer Name Required", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else
                    {
                        so.Customer.CustomerId = data.CustomerId;
                        so.PaymentRecieved = true;
                        so.Customer.CreditLimit = data.CreditLimit;
                        so.Customer.CreditRemaining = data.CreditRemaining;
                        so.Customer.CreditOwed = data.CreditOwed;

                        if (data.CreditRemaining < so.TotalAmount)//Low credit
                        {
                            so.PaymentRecieved = false;
                            if (MessageBox.Show("Credit is too low to process this order" + System.Environment.NewLine + "Available credit is " + data.CreditRemaining.ToString("C", CultureInfo.CurrentCulture) + System.Environment.NewLine + System.Environment.NewLine + "Do you want to hold this order?", "Low Credit", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                            {
                                //Check stock if there are enough stock to hold
                                bool isNoStockAvailable = CheckProductStock(so);

                                if (isNoStockAvailable == false)
                                {
                                    //Don't Reserve Stock just hold
                                    so.OrderStatus = OrderStatus.HoldNoCredit.ToString();
                                    reservedStockEle = HoldProductStockNoDeduct(so);
                                    canSubmit = reservedStockEle.Item5.IsError ? false : true;                            
                                }
                                else
                                {
                                    //Hold No Stock
                                    so.OrderStatus = OrderStatus.HoldNoCreditNoStock.ToString();
                                    reservedStockEle = HoldProductStockNoDeduct(so);
                                    if (MessageBox.Show("Insufficient stock for the following products(s)" + System.Environment.NewLine + System.Environment.NewLine + reservedStockEle.Item2 + System.Environment.NewLine + "Do you want to hold this order until stock is being made?", "Insufficient Stock", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                                    {
                                        //Hold Order
                                        so.OrderStatus = OrderStatus.HoldNoCreditNoStock.ToString();
                                        canSubmit = reservedStockEle.Item5.IsError ? false : true;
                                    }
                                    else
                                    {
                                        //Cancel Order
                                        if (MessageBox.Show("Do you want to cancel this order?", "Cancel Order", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                                        {
                                            //CancelOrder(SalesOrder so)
                                            canSubmit = false;
                                            clearFields = true;
                                        }
                                        else
                                        {
                                            canSubmit = false;
                                            clearFields = false;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                canSubmit = false;
                                reservedStockEle = CancelOrder(so);
                            }
                        }
                        else
                        {
                            //Good credit proceed check stock if there is enough stock to hold
                            bool isNoStockAvailable = CheckProductStock(so);
                            if (isNoStockAvailable == false)//Stock available
                            {
                                if (so.OrderAction == OrderStatus.Hold.ToString())
                                {
                                    if (MessageBox.Show("Do you want to reserve stock while holding this order?" + System.Environment.NewLine + "'YES' to reserve stock 'NO' to just hold this order without stock", "Stock Allocation Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                                    {
                                        so.OrderStatus = OrderStatus.HoldStockAllocated.ToString();
                                        reservedStockEle = ReserveProductStock(so);
                                        canSubmit = reservedStockEle.Item5.IsError ? false : true;
                                    }
                                    else
                                    {
                                        so.OrderStatus = OrderStatus.Hold.ToString();
                                        reservedStockEle = HoldProductStock(so);
                                        canSubmit = reservedStockEle.Item5.IsError ? false : true;
                                    }
                                }
                                else
                                {
                                    so.OrderStatus = OrderStatus.InWarehouse.ToString();
                                    reservedStockEle = ReserveProductStock(so);
                                    canSubmit = reservedStockEle.Item5.IsError ? false : true;
                                }
                            }
                            else //No stock
                            {
                                if (so.OrderAction == OrderStatus.Release.ToString())
                                {
                                    reservedStockEle = HoldProductStockNoDeduct(so);
                                    //If no stock ask if it is to hold order  
                                    if (MessageBox.Show("Insufficient stock for the following products(s)" + System.Environment.NewLine + System.Environment.NewLine + reservedStockEle.Item2 + System.Environment.NewLine + "Do you want to hold this order until stock is being made?", "Insufficient Stock", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                                    {
                                        //Hold Order
                                        so.OrderStatus = OrderStatus.HoldNoStock.ToString();
                                        canSubmit = reservedStockEle.Item5.IsError ? false : true;
                                    }
                                    else
                                    {
                                        //Cancel Order
                                        if (MessageBox.Show("Do you want to cancel this order?", "Cancel Order", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                                        {
                                            canSubmit = false;
                                            clearFields = true;
                                        }
                                        else
                                        {
                                            canSubmit = false;
                                            clearFields = false;
                                        }
                                    }
                                }
                                else if (so.OrderAction == OrderStatus.Hold.ToString())
                                {
                                    //Don't Reserve Stock just hold
                                    reservedStockEle = HoldProductStockNoDeduct(so);
                                    reservedStockEle.Item1.OrderStatus = OrderStatus.Hold.ToString();
                                    canSubmit = reservedStockEle.Item5.IsError ? false : true;
                                }
                            }
                        }

                        if (reservedStockEle.Item5 != null && reservedStockEle.Item5.IsError == false)
                        {
                            //Process Customer credit
                            creditTuple = ProcessCustomerCredit(so);
                            so.Customer.CustomerCreditHistory = creditTuple.Item1;
                            so.Customer.CustomerCreditHistory.UpdatedDate = DateTime.Now;
                            so.Customer.CustomerCreditHistory.UpdatedBy = userName;
                        }
                    }
                }
                else if (so.Customer.CustomerType == "Prepaid")
                {
                    so.PaymentRecieved = true;

                    //No credit check good to proceed
                    //Check stock if there is enough stock to hold
                    bool isNoStockAvailable = CheckProductStock(so);
                    if (isNoStockAvailable == false)
                    {
                        if (so.OrderAction == OrderStatus.Release.ToString())
                        {
                            so.OrderStatus = OrderStatus.InWarehouse.ToString();
                            reservedStockEle = ReserveProductStock(so);
                        }
                        else if (so.OrderAction == OrderStatus.Hold.ToString())
                        {  
                            if (MessageBox.Show("Do you want to reserve stock while holding this order?" + System.Environment.NewLine + "'YES' to reserve stock 'NO' to just hold this order without stock", "Stock Allocation Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                            {
                                so.OrderStatus = OrderStatus.HoldStockAllocated.ToString();
                                reservedStockEle = ReserveProductStock(so);
                                canSubmit = reservedStockEle.Item5.IsError ? false : true;
                            }
                            else
                            {
                                so.OrderStatus = OrderStatus.Hold.ToString();
                                reservedStockEle = HoldProductStock(so);
                                canSubmit = reservedStockEle.Item5.IsError ? false : true;
                            }
                        }
                        canSubmit = reservedStockEle.Item5.IsError ? false : true;
                    }
                    else
                    {
                        //If no stock ask if it is to hold stock for the order
                        reservedStockEle = HoldProductStockNoDeduct(so);
                        if (!string.IsNullOrWhiteSpace(reservedStockEle.Item2))
                        {
                            if (MessageBox.Show("Insufficient stock for the following products(s)" + System.Environment.NewLine + System.Environment.NewLine + reservedStockEle.Item2 + System.Environment.NewLine + "Do you want to hold this order until stock is being made?", "Insufficient Stock", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                            {
                                //Hold Order
                                so.OrderStatus = OrderStatus.HoldNoStock.ToString();
                                canSubmit = reservedStockEle.Item5.IsError ? false : true;
                            }
                            else
                            {
                                if (MessageBox.Show("Do you want to cancel this order?", "Cancel Order", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                                {
                                    canSubmit = false;
                                    clearFields = true;
                                }
                                else
                                {
                                    canSubmit = false;
                                    clearFields = false;
                                }
                            }
                        }
                    }
                }
            }
            else if (so.OrderAction == OrderStatus.Cancel.ToString())
            {
                if (MessageBox.Show("Are you sure you want to cancel this order?", "Cancelling Order Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    reservedStockEle = RemoveAllocatedStock(so);
                    reservedStockEle.Item1.OrderStatus = OrderStatus.Cancel.ToString();
                    canSubmit = true;
                }
                else
                {
                    reservedStockEle = CancelOrder(so);
                }
            }
            //If there is no error
            if (reservedStockEle != null && reservedStockEle.Item5.IsError == false)
            {
                so.PaymentDueDate = CalculatePaymentDueDate(so);
            }          

            return Tuple.Create(canSubmit, reservedStockEle.Item1, reservedStockEle.Item3, reservedStockEle.Item4, clearFields, creditTuple, reservedStockEle.Item5);
        }

        

        public Tuple<bool, SalesOrder, List<ProductStockReserved>, List<ProductStock>, bool, Tuple<CustomerCreditHistory, CustomerCreditActivity>> ProcessSalesOrder2(SalesOrder so, string selectedCustomer, string userName)
        {
            bool canSubmit = false;
            bool clearFields = false;
            Tuple<SalesOrder, string, List<ProductStockReserved>, List<ProductStock>,SystemError> reservedStockEle = null;
            Tuple<CustomerCreditHistory, CustomerCreditActivity> creditTuple = null;
            //CustomerCreditHistory updatedCustomerCreditHistory = new CustomerCreditHistory();
            productStockReserved = new List<ProductStockReserved>();

            //so.SalesMadeBy = "";
            so.LastModifiedBy = UserData.FirstName + " " + UserData.LastName;
            so.LastModifiedDate = DateTime.Now;
            so.SalesCompletedBy = so.LastModifiedBy;
            var data = CustomerList.SingleOrDefault(c => c.CompanyName == selectedCustomer);
            productStockReserved = DBAccess.GetReservedProductStock(so);

            if (so.Customer.CustomerType == "Prepaid" && data != null)
            {
                so.Customer.CustomerId = data.CustomerId;
            }
            else if (so.Customer.CustomerType == "Account")
            {
                so.Customer = data;
            }

            if (so.OrderAction == OrderStatus.Release.ToString() || so.OrderAction == OrderStatus.Hold.ToString())
            {
                if (so.Customer != null && so.Customer.CustomerType == "Account")
                {
                    if (data == null)
                    {
                        canSubmit = false;
                        MessageBox.Show("Please select correct customer name", "Customer Name Required", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else
                    {
                        bool isLowCredit = false;
                        //Process Customer credit
                        creditTuple = ProcessCustomerCredit(so);

                        if (creditTuple.Item1.CreditDeducted <= so.TotalAmount && creditTuple.Item1.CreditDeducted > 0)
                        {
                            isLowCredit = false;
                        }
                        else if (creditTuple.Item1.CreditDeducted > so.TotalAmount && creditTuple.Item1.CreditDeducted > 0)
                        {
                            decimal c = (so.TotalAmount - creditTuple.Item1.CreditDeducted) < 0 ? 0 : (so.TotalAmount - creditTuple.Item1.CreditDeducted);
                            isLowCredit = creditTuple.Item1.CreditRemaining >= c ? false : true;
                        }
                        else if (creditTuple.Item1.CreditDeducted == 0)
                        {
                            isLowCredit = creditTuple.Item1.CreditRemaining >= so.TotalAmount ? false : true;
                        }

                        so.Customer.CustomerId = data.CustomerId;
                        so.PaymentRecieved = true;
                        so.Customer.CreditLimit = data.CreditLimit;
                        so.Customer.CreditRemaining = data.CreditRemaining;

                        if (creditTuple.Item1.Debt > 0 || isLowCredit)//Low credit
                        {
                            so.PaymentRecieved = false;

                            if (MessageBox.Show("Credit is too low to process this order" + System.Environment.NewLine + "Available credit is " + data.CreditRemaining.ToString("C", CultureInfo.CurrentCulture) + System.Environment.NewLine + System.Environment.NewLine + "Do you want to hold this order?", "Low Credit", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                            {
                                //Check stock if there are enough stock to hold
                                bool isNoStockAvailable = CheckProductStock(so);

                                if (isNoStockAvailable == false)
                                {                                   
                                    //Don't Reserve Stock just hold
                                    so.OrderStatus = OrderStatus.HoldNoCredit.ToString();
                                    reservedStockEle = ReleaseProductStockHolding(so, userName);
                                    canSubmit = true;                                    
                                }
                                else
                                {
                                    //Hold No Stock
                                    so.OrderStatus = OrderStatus.HoldNoCreditNoStock.ToString();
                                    reservedStockEle = HoldProductStockNoDeduct(so);
                                    if (MessageBox.Show("Insufficient stock for the following products(s)" + System.Environment.NewLine + System.Environment.NewLine + reservedStockEle.Item2 + System.Environment.NewLine + "Do you want to hold this order until stock is being made?", "Insufficient Stock", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                                    {
                                        //Hold Order
                                        so.OrderStatus = OrderStatus.HoldNoCreditNoStock.ToString();                                        
                                        canSubmit = reservedStockEle.Item5.IsError ? false : true;
                                    }
                                    else
                                    {
                                        //Cancel Order
                                        if (MessageBox.Show("Do you want to cancel this order?", "Cancel Order", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                                        {
                                            so.OrderStatus = OrderStatus.Cancel.ToString();
                                            reservedStockEle = RemoveAllocatedStock(so);
                                            canSubmit = reservedStockEle.Item5.IsError ? false : true;
                                            clearFields = true;
                                        }
                                        else
                                        {
                                            canSubmit = false;
                                            clearFields = false;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                canSubmit = false;
                                reservedStockEle = CancelOrder(so);
                            }
                        }
                        else
                        {
                            //Good credit proceed check stock if there is enough stock to hold
                            Tuple<SalesOrder, string, List<ProductStockReserved>, List<ProductStock>, bool, bool,SystemError> tup = CheckStockHoldCancel(so, userName);
                            reservedStockEle = Tuple.Create(tup.Item1, tup.Item2, tup.Item3, tup.Item4, tup.Item7);
                            canSubmit = tup.Item5;
                            clearFields = tup.Item6;
                        }
                        //Process Customer credit
                        creditTuple = ProcessCustomerCredit(so);
                    }
                }
                else if (so.Customer == null || so.Customer.CustomerType == "Prepaid")
                {
                    so.PaymentRecieved = true;

                    //No credit check good to proceed
                    //Check stock if there is enough stock to hold
                    Tuple<SalesOrder, string, List<ProductStockReserved>, List<ProductStock>, bool, bool, SystemError> tup = CheckStockHoldCancel(so, userName);
                    reservedStockEle = Tuple.Create(tup.Item1, tup.Item2, tup.Item3, tup.Item4, tup.Item7);
                    canSubmit = tup.Item5;
                    clearFields = tup.Item6;
                }

                so.PaymentDueDate = CalculatePaymentDueDate(so);

            }
            else if (so.OrderAction == OrderStatus.Return.ToString())
            {
                if (MessageBox.Show("Are you sure you want to return this order?", "Returning Order Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    reservedStockEle = RemoveAllocatedStock(so);
                    canSubmit = reservedStockEle.Item5.IsError ? false : true;
                    so.OrderStatus = OrderStatus.Return.ToString();

                    if (so.Customer.CustomerType == "Account")
                    {
                        //Re allocate Customer credit
                        creditTuple = ProcessCustomerCredit(so);
                    }
                }
                else
                {
                    reservedStockEle = CancelOrder(so);
                    canSubmit = false;
                    clearFields = false;
                }
            }
            else if (so.OrderAction == OrderStatus.Cancel.ToString())
            {
                if (MessageBox.Show("Are you sure you want to cancel this order?", "Cancelling Order Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    reservedStockEle = RemoveAllocatedStock(so);
                    reservedStockEle.Item1.OrderStatus = OrderStatus.Cancel.ToString();
                    canSubmit = reservedStockEle.Item5.IsError ? false : true;

                    if (so.Customer.CustomerType == "Account")
                    {
                        //Re allocate Customer credit
                        creditTuple = ProcessCustomerCredit(so);
                    }
                }
                else
                {
                    reservedStockEle = CancelOrder(so);
                }
            }

            //Check payment received
            //reservedStockEle.Item1.PaymentRecieved = ProcessPaymentReceived(so);

            return Tuple.Create(canSubmit, reservedStockEle.Item1, reservedStockEle.Item3, reservedStockEle.Item4, clearFields, creditTuple);
        }

        /******************************************************************************/
        /***************************SILENT ORDER PROCESS*******************************/
        /******************************************************************************/

        public Tuple<bool, SalesOrder, List<ProductStockReserved>, List<ProductStock>, bool, Tuple<CustomerCreditHistory, CustomerCreditActivity>> SilentProcessSalesOrder(SalesOrder so, string selectedCustomer, string userName)
        {
            bool canSubmit = false;
            bool clearFields = false;
            Tuple<SalesOrder, string, List<ProductStockReserved>, List<ProductStock>, SystemError> reservedStockEle = null;
            Tuple<CustomerCreditHistory, CustomerCreditActivity> creditTuple = null;
            //CustomerCreditHistory updatedCustomerCreditHistory = new CustomerCreditHistory();
            productStockReserved = new List<ProductStockReserved>();

            //so.SalesMadeBy = "";
            so.LastModifiedBy = UserData.FirstName + " " + UserData.LastName;
            so.LastModifiedDate = DateTime.Now;
            so.SalesCompletedBy = userName;
            var data = CustomerList.SingleOrDefault(c => c.CompanyName == selectedCustomer);
            productStockReserved = DBAccess.GetReservedProductStock(so);

            if (so.Customer.CustomerType == "Prepaid" && data != null)
            {
                so.Customer.CustomerId = data.CustomerId;
            }
            else if (so.Customer.CustomerType == "Account")
            {
                so.Customer = data;
            }

            if (so.OrderAction == OrderStatus.Release.ToString() || so.OrderAction == OrderStatus.Hold.ToString())
            {
                if (so.Customer != null && so.Customer.CustomerType == "Account")
                {
                    if (data == null)
                    {
                        canSubmit = false;
                    }
                    else
                    {
                        bool isLowCredit = false;
                        //Process Customer credit
                        creditTuple = ProcessCustomerCredit(so);

                        if (creditTuple.Item1.CreditDeducted <= so.TotalAmount && creditTuple.Item1.CreditDeducted > 0)
                        {
                            isLowCredit = false;
                        }
                        else if (creditTuple.Item1.CreditDeducted > so.TotalAmount && creditTuple.Item1.CreditDeducted > 0)
                        {
                            decimal c = (so.TotalAmount - creditTuple.Item1.CreditDeducted) < 0 ? 0 : (so.TotalAmount - creditTuple.Item1.CreditDeducted);
                            isLowCredit = creditTuple.Item1.CreditRemaining >= c ? false : true;
                        }
                        else if (creditTuple.Item1.CreditDeducted == 0)
                        {
                            isLowCredit = creditTuple.Item1.CreditRemaining >= so.TotalAmount ? false : true;
                        }

                        so.Customer.CustomerId = data.CustomerId;
                        so.PaymentRecieved = true;
                        so.Customer.CreditLimit = data.CreditLimit;
                        so.Customer.CreditRemaining = data.CreditRemaining;
                        
                        if (creditTuple.Item1.Debt > 0 || isLowCredit)//Low credit
                        {
                            //No credit hold order
                            so.PaymentRecieved = false;
                            so.OrderStatus = OrderStatus.HoldNoCredit.ToString();
                            reservedStockEle = ReleaseProductStockHolding(so, userName);
                            canSubmit = true;                            
                        }
                        else
                        {
                            //Good credit proceed check stock if there is enough stock to hold
                            Tuple<SalesOrder, string, List<ProductStockReserved>, List<ProductStock>, bool, bool, SystemError> tup = SilentCheckStockHoldCancel(so, userName);
                            reservedStockEle = Tuple.Create(tup.Item1, tup.Item2, tup.Item3, tup.Item4, tup.Item7);
                            canSubmit = tup.Item5;
                            clearFields = tup.Item6;
                        }
                        //Process Customer credit
                        creditTuple = ProcessCustomerCredit(so);
                    }
                }
                else if (so.Customer == null || so.Customer.CustomerType == "Prepaid")
                {
                    so.PaymentRecieved = true;

                    //No credit check good to proceed
                    //Check stock if there is enough stock to hold
                    Tuple<SalesOrder, string, List<ProductStockReserved>, List<ProductStock>, bool, bool, SystemError> tup = CheckStockHoldCancel(so, userName);
                    reservedStockEle = Tuple.Create(tup.Item1, tup.Item2, tup.Item3, tup.Item4, tup.Item7);
                    canSubmit = tup.Item5;
                    clearFields = tup.Item6;
                }

                so.PaymentDueDate = CalculatePaymentDueDate(so);

            }                     

            return Tuple.Create(canSubmit, reservedStockEle.Item1, reservedStockEle.Item3, reservedStockEle.Item4, clearFields, creditTuple);
        }

        private Tuple<SalesOrder, string, List<ProductStockReserved>, List<ProductStock>, bool, bool, SystemError> SilentCheckStockHoldCancel(SalesOrder so, string userName)
        {
            bool canSubmit = false, clearFields = false;
            
            Tuple<SalesOrder, string, List<ProductStockReserved>, List<ProductStock>,SystemError> reservedStockEle = null;

            bool isNoStockAvailable = CheckProductStock(so);
            if (isNoStockAvailable == false)
            {
                if (so.OrderAction == OrderStatus.Hold.ToString())
                {
                    so.OrderStatus = OrderStatus.Hold.ToString();
                    reservedStockEle = ReleaseProductStockHolding(so, userName);
                    canSubmit = true;
                }
                else
                {
                    so.OrderStatus = GetOrderLocation(so).ToString();
                    reservedStockEle = ReserveProductStock(so);
                    canSubmit = true;
                }
            }
            else
            {
                //If no stock ask if it is to hold stock for the order
                reservedStockEle = HoldProductStockNoDeduct(so);
                if (!string.IsNullOrWhiteSpace(reservedStockEle.Item2))
                {                    
                    //Hold Order
                    so.OrderStatus = OrderStatus.HoldNoStock.ToString();
                    canSubmit = true;
                }
            }
            return Tuple.Create(reservedStockEle.Item1, reservedStockEle.Item2, reservedStockEle.Item3, reservedStockEle.Item4, canSubmit, clearFields, reservedStockEle.Item5);
        }
        /******************************************************************************/
        /*************************END OF SILENT ORDER PROCESS**************************/
        /******************************************************************************/


        private Tuple<SalesOrder, string, List<ProductStockReserved>, List<ProductStock>, bool, bool, SystemError> CheckStockHoldCancel(SalesOrder so, string userName)
        {
            bool canSubmit = false, clearFields = false;
            SystemError se = new SystemError();
            Tuple<SalesOrder, string, List<ProductStockReserved>, List<ProductStock>,SystemError> reservedStockEle = null;

            bool isNoStockAvailable = CheckProductStock(so);
            if (isNoStockAvailable == false)
            {
                if (so.OrderAction == OrderStatus.Hold.ToString())
                {
                    so.OrderStatus = OrderStatus.Hold.ToString();
                    reservedStockEle = ReleaseProductStockHolding(so, userName);
                    canSubmit = true;
                    
                }
                else
                {
                    so.OrderStatus = GetOrderLocation(so).ToString();
                    reservedStockEle = ReserveProductStock(so);
                    canSubmit = true;
                }
            }
            else
            {
                //If no stock ask if it is to hold stock for the order
                reservedStockEle = HoldProductStockNoDeduct(so);
                if (!string.IsNullOrWhiteSpace(reservedStockEle.Item2))
                {
                    if (MessageBox.Show("Insufficient stock for the following products(s)" + System.Environment.NewLine + System.Environment.NewLine + reservedStockEle.Item2 + System.Environment.NewLine + "Do you want to hold this order until stock is being made?", "Insufficient Stock", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        //Hold Order
                        so.OrderStatus = OrderStatus.HoldNoStock.ToString();
                        canSubmit = true;
                    }
                    else
                    {
                        if (MessageBox.Show("Do you want to cancel this order?", "Cancel Order", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                        {
                            so.OrderStatus = OrderStatus.Cancel.ToString();
                            reservedStockEle = RemoveAllocatedStock(so);
                            canSubmit = true;
                            clearFields = true;
                        }
                        else
                        {
                            canSubmit = false;
                            clearFields = false;
                        }
                    }
                }
            }
            return Tuple.Create(reservedStockEle.Item1, reservedStockEle.Item2, reservedStockEle.Item3, reservedStockEle.Item4, canSubmit, clearFields, reservedStockEle.Item5);
        }

        private OrderStatus GetOrderLocation(SalesOrder so)
        {
            OrderStatus area = OrderStatus.InWarehouse;
            //Check previous status
            SalesOrder oldSalesOrder = DBAccess.GetSalesOrderDetails(so.SalesOrderNo);
            bool changeNotDetected = true;
            //Check collections are equal
            changeNotDetected = (oldSalesOrder.SalesOrderDetails.Count != so.SalesOrderDetails.Count) ? false : true;
            if (changeNotDetected)
            {
                foreach (var item in so.SalesOrderDetails)
                {
                    changeNotDetected = oldSalesOrder.SalesOrderDetails.Any(z => z.SalesOrderDetailsID == item.SalesOrderDetailsID && z.Quantity == item.Quantity);
                    if (changeNotDetected == false)
                    {
                        break;
                    }
                }
            }

            if (changeNotDetected == false)
            {
                if (oldSalesOrder.OrderStatus == "FinalisingShipping" || oldSalesOrder.OrderStatus == "PreparingInvoice")
                {
                    area = OrderStatus.InWarehouse;
                }
            }
            else
            {
                //Check which area had the order last
                area = DBAccess.CheckWhichAreaHadOrder(so);
            }
            return area;
        }


        public Tuple<Int32, Int32> SendToSalesOrderDB(SalesOrder salesOrder, List<ProductStockReserved> productStockReserved, List<ProductStock> productStockToDeductList, Tuple<CustomerCreditHistory, CustomerCreditActivity> creditTuple,OrderOrigin<object,string> oo)
        {
            Tuple<Int32, Int32> result = null;
            result = DBAccess.AddToSalesOrders(salesOrder, productStockReserved, productStockToDeductList, creditTuple, oo);
            return result;
        }

        public int UpdateSalesOrderDB(SalesOrder salesOrder, List<ProductStockReserved> productStockReserved, List<ProductStock> productStockToDeductList, string userName, Tuple<CustomerCreditHistory, CustomerCreditActivity> creditTuple, List<Tuple<string, Int16, string>> oldTimeStamps)
        {
            int result = 0;
            salesOrder.LastModifiedDate = DateTime.Now;
            salesOrder.LastModifiedBy = UserData.FirstName + " " + UserData.LastName;

            //For returns
            if (salesOrder.OrderStatus == OrderStatus.Return.ToString())
            {
                result = DBAccess.ReturnSalesOrder(salesOrder, productStockReserved, productStockToDeductList, creditTuple, oldTimeStamps);
            }
            else
            {
                result = DBAccess.UpdateSalesOrder(salesOrder, productStockReserved, productStockToDeductList, prodStockUpdate, creditTuple);
            }
            return result;
        }

        /*****************ADD NEW SALES ORDER**************/

        private List<ProductStock> ConsolidateProductStock(SalesOrder so, string userName)
        {
            List<ProductStock> prodStock = new List<ProductStock>();
            List<ProductStockReserved> currReserStock = DBAccess.GetReservedProductStockBySalesNoAndStock(so);//Reserved Stock

            foreach (var item in currReserStock)
            {
                var data = so.SalesOrderDetails.FirstOrDefault(x => x.SalesOrderDetailsID == item.ProductStockReservedID && so.StockLocation.ID == stockLocationId);//Product Stock
                var prodStData = productStockList.FirstOrDefault(x => x.Product.ProductID == item.Product.ProductID && x.StockLocation.ID == stockLocationId);//Product Stock
                if (data == null)
                {
                    prodStock.Add(new ProductStock() { StockLocation = new StockLocation() { ID = item.StockLocation.ID }, Product = new Product() { ProductID = item.Product.ProductID }, QtyAvailable = prodStData.QtyAvailable + item.QtyReserved, LastUpdatedDate = DateTime.Now, UpdatedBy = userName });
                }
            }

            return prodStock;
        }

        public Tuple<SalesOrder, string, List<ProductStockReserved>, List<ProductStock>,SystemError> ReserveProductStock(SalesOrder so)
        {
            string userName = UserData.FirstName + " " + UserData.LastName;
            SystemError se = new SystemError();
            List<ProductStockReserved> productStockReserved = new List<ProductStockReserved>();
            List<ProductStock> productStockToDeductList = new List<ProductStock>();
            prodStockUpdate = ConsolidateProductStock(so, userName);
            List<ProductStockReserved> currReserStock = DBAccess.GetReservedProductStockBySalesNoAndStock(so);
            
            var clonedProdStock = productStockList.Select(objEntity => (ProductStock)objEntity.Clone()).ToList();
            List<ProductStock> dummyProductStock = new List<ProductStock>(clonedProdStock);

            string errMsg = string.Empty;

            foreach (var item in so.SalesOrderDetails)
            {
                var data = dummyProductStock.FirstOrDefault(x => x.Product.ProductID == item.Product.ProductID && x.StockLocation.ID == stockLocationId);//Product Stock
                var curResStock = currReserStock.FirstOrDefault(x => x.ProductStockReservedID == item.SalesOrderDetailsID);//Product Stock
               
                if (data != null)
                {
                    if (curResStock == null || curResStock.QtyReserved != item.Quantity)
                    {
                        decimal r = curResStock == null ? 0 : curResStock.QtyReserved;
                        decimal diff = 0, qRes = 0, qRem = 0;

                        if (item.Quantity > r)
                        {
                            diff = item.Quantity - r;
                            if (data.QtyAvailable >= diff)
                            {
                                qRes = item.Quantity;
                                qRem = 0;
                                data.QtyAvailable = (data.QtyAvailable - diff) < 0 ? 0 : data.QtyAvailable - diff;
                            }
                            else if (diff > data.QtyAvailable)
                            {
                                qRes = 0;
                                qRem = item.Quantity;
                            }
                        }
                        else if (r >= item.Quantity)
                        {
                            diff = r - item.Quantity;
                            qRes = item.Quantity;
                            qRem = 0;
                            data.QtyAvailable = data.QtyAvailable + diff;
                            //data.QtyAvailable = (data.QtyAvailable - item.Quantity) < 0 ? 0 : data.QtyAvailable - item.Quantity;
                        }

                        //ProductStock to reserve
                        ProductStockReserved psr = new ProductStockReserved();
                        psr.ProductStockReservedID = item.SalesOrderDetailsID;
                        psr.SalesNo = so.SalesOrderNo;
                        psr.StockLocation = new StockLocation() { ID = so.StockLocation.ID };
                        psr.Product = new Product() { ProductID = item.Product.ProductID };
                        psr.QtyOrdered = item.Quantity;
                        psr.QtyReserved = qRes;
                        psr.QtyRemaining = qRem;
                        psr.ReservedDate = DateTime.Now;
                        psr.Status = psr.QtyRemaining > 0 ? StockReserved.AwaitingStock.ToString() : StockReserved.Reserved.ToString();
                        psr.ActivityDate = DateTime.Now;
                        psr.OrderLine = item.OrderLine;
                        productStockReserved.Add(psr);

                        //ProductStock to deduct
                        ProductStock ps = new ProductStock();
                        ps.StockLocation = new StockLocation() { ID = so.StockLocation.ID };
                        ps.Product = new Product() { ProductID = item.Product.ProductID };
                        ps.QtyAvailable = data.QtyAvailable;
                        ps.LastUpdatedDate = DateTime.Now;
                        ps.UpdatedBy = userName;
                        ps.TimeStamp = data.TimeStamp;
                        productStockToDeductList.Add(ps);                       
                    }
                }
                else
                {
                    se.IsError = true;
                    se.ErrorName = "There has been a problem when reserving stock to this order" + System.Environment.NewLine + "Cannot find " + item.Product.ProductDescription + " [" + item.Product.ProductCode + "] in stock";
                    break;
                }
            }

            if (se.IsError == false)
            {
                errMsg = ConstructNoStockErrorMsg(productStockReserved, dummyProductStock, so.StockLocation.ID);
            }

            return Tuple.Create(so, errMsg, productStockReserved, productStockToDeductList, se);
        }             

        //This function does not allocate stock
        public Tuple<SalesOrder, string, List<ProductStockReserved>, List<ProductStock>,SystemError> HoldProductStock(SalesOrder so)
        {
            string userName = UserData.FirstName + " " + UserData.LastName;
            SystemError se = new SystemError();
            List<ProductStockReserved> productStockReserved = new List<ProductStockReserved>();
            List<ProductStock> productStockToDeductList = new List<ProductStock>();
            List<ProductStockReserved> currReserStock = DBAccess.GetReservedProductStock(so);
            string errMsg = string.Empty;

            foreach (var item in so.SalesOrderDetails)
            {
                decimal reserved = 0;
                var data = productStockList.FirstOrDefault(x => x.Product.ProductID == item.Product.ProductID && x.StockLocation.ID == stockLocationId);
                var data1 = currReserStock.SingleOrDefault(c => c.ProductStockReservedID == item.SalesOrderDetailsID);

                if (data != null)
                {
                    //ProductStock to reserve
                    ProductStockReserved psr = new ProductStockReserved();
                    psr.ProductStockReservedID = item.SalesOrderDetailsID;
                    psr.SalesNo = so.SalesOrderNo;
                    psr.StockLocation = new StockLocation() { ID = so.StockLocation.ID };
                    psr.Product = new Product() { ProductID = item.Product.ProductID };
                    psr.QtyOrdered = item.Quantity;
                    psr.QtyReserved = 0;
                    psr.QtyRemaining = item.Quantity;
                    psr.ReservedDate = DateTime.Now;
                    psr.Status = StockReserved.NotReserved.ToString();
                    psr.OrderLine = item.OrderLine;
                    productStockReserved.Add(psr);

                    if (data1 != null && data1.QtyReserved > 0)
                    {
                        reserved = data1.QtyReserved;
                    }

                    ProductStock ps = new ProductStock();
                    ps.StockLocation = new StockLocation() { ID = so.StockLocation.ID };
                    ps.Product = new Product() { ProductID = item.Product.ProductID };
                    ps.QtyAvailable = data.QtyAvailable + reserved;
                    ps.LastUpdatedDate = DateTime.Now;
                    ps.UpdatedBy = userName;
                    ps.TimeStamp = data.TimeStamp;
                    productStockToDeductList.Add(ps);

                    if (item.Quantity > data.QtyAvailable)
                    {
                        errMsg += item.Product.ProductDescription + " requires " + item.Quantity.ToString("G29") + CultureInfo.CurrentCulture.TextInfo.ToTitleCase(item.Product.ProductUnit.ToUpper()) + " but " + data.QtyAvailable.ToString("G29") + CultureInfo.CurrentCulture.TextInfo.ToTitleCase(item.Product.ProductUnit.ToUpper()) + " on hand!" + System.Environment.NewLine;
                    }
                }
                else
                {
                    se.IsError = true;
                    se.ErrorName = "There has been a problem when holding this order" + System.Environment.NewLine + "Cannot find product " + item.Product.ProductDescription + " [" + item.Product.ProductCode + "] in stock" + System.Environment.NewLine + "Please try again later";
                    break;
                }
            }

            return Tuple.Create(so, errMsg, productStockReserved, productStockToDeductList, se);
        }


        public Tuple<SalesOrder, string, List<ProductStockReserved>, List<ProductStock>,SystemError> HoldProductStockNoDeduct(SalesOrder so)
        {
            SystemError se = new SystemError();
            string userName = UserData.FirstName + " " + UserData.LastName;
            List<ProductStockReserved> productStockReserved = new List<ProductStockReserved>();
            List<ProductStock> productStockToDeductList = new List<ProductStock>();
            List<ProductStockReserved> currReserStock = DBAccess.GetReservedProductStock(so);
            string errMsg = string.Empty;

            var clonedProdStock = productStockList.Select(objEntity => (ProductStock)objEntity.Clone()).ToList();
            List<ProductStock> dummyProductStock = new List<ProductStock>(clonedProdStock);

            foreach (var item in so.SalesOrderDetails)
            {
                var data = dummyProductStock.FirstOrDefault(x => x.Product.ProductID == item.Product.ProductID && x.StockLocation.ID == stockLocationId);//Product Stock
                var curResStock = currReserStock.FirstOrDefault(x => x.ProductStockReservedID == item.SalesOrderDetailsID);//Product Stock
                if (data != null)
                {
                    if (curResStock == null || curResStock.QtyReserved != item.Quantity)
                    {
                        decimal r = curResStock == null ? 0 : curResStock.QtyReserved;

                        decimal diff = 0, qRes = 0, qRem = 0;
                        if (item.Quantity > r)
                        {
                            diff = item.Quantity - r;
                            if (data.QtyAvailable >= diff)
                            {
                                qRes = item.Quantity;
                                qRem = 0;
                                data.QtyAvailable = (data.QtyAvailable - diff) < 0 ? 0 : data.QtyAvailable - diff;
                            }
                            else if (diff > data.QtyAvailable)
                            {
                                qRes = 0;
                                qRem = item.Quantity;
                            }
                        }
                        else if (r >= item.Quantity)
                        {
                            diff = r - item.Quantity;
                            qRes = item.Quantity;
                            qRem = 0;
                            data.QtyAvailable = (data.QtyAvailable - item.Quantity) < 0 ? 0 : data.QtyAvailable - item.Quantity;
                        }


                        //ProductStock to reserve
                        ProductStockReserved psr = new ProductStockReserved();
                        psr.ProductStockReservedID = item.SalesOrderDetailsID;
                        psr.SalesNo = so.SalesOrderNo;
                        psr.StockLocation = new StockLocation() { ID = so.StockLocation.ID };
                        psr.Product = new Product() { ProductID = item.Product.ProductID,ProductDescription = item.QuoteProductDescription,ProductUnit = item.Product.ProductUnit };
                        psr.QtyOrdered = item.Quantity;
                        psr.QtyReserved = qRes;
                        psr.QtyRemaining = qRem;
                        psr.ReservedDate = DateTime.Now;
                        psr.Status = psr.QtyRemaining > 0 ? StockReserved.AwaitingStock.ToString() : StockReserved.Reserved.ToString();
                        psr.OrderLine = item.OrderLine;
                        productStockReserved.Add(psr);

                        //ProductStock to deduct
                        ProductStock ps = new ProductStock();
                        ps.StockLocation = new StockLocation() { ID = so.StockLocation.ID };
                        ps.Product = new Product() { ProductID = item.Product.ProductID };
                        ps.QtyAvailable = (curResStock == null ? 0 : curResStock.QtyReserved) + data.QtyAvailable;
                        ps.LastUpdatedDate = DateTime.Now;
                        ps.UpdatedBy = userName;
                        ps.TimeStamp = data.TimeStamp;
                        productStockToDeductList.Add(ps);                        
                    }
                }
                else
                {
                    se.IsError = true;
                    se.ErrorName = "There has been a problem when holding this order" + System.Environment.NewLine + "Cannot find product " + item.Product.ProductDescription + " [" + item.Product.ProductCode + "] in stock" + System.Environment.NewLine + "Please try again later";
                    break;
                }
            }
            if (se.IsError == false)
            {
                errMsg = ConstructNoStockErrorMsg(productStockReserved, dummyProductStock, so.StockLocation.ID);
            }
            return Tuple.Create(so, errMsg, productStockReserved, productStockToDeductList, se);
        }

        

        public Tuple<SalesOrder, string, List<ProductStockReserved>, List<ProductStock>, SystemError> ReleaseProductStockHolding(SalesOrder so, string userName)
        {
            SystemError se = new SystemError();
            List<ProductStockReserved> productStockReserved = new List<ProductStockReserved>();
            List<ProductStock> productStockToDeductList = new List<ProductStock>();
            List<ProductStockReserved> currReserStock = DBAccess.GetReservedProductStock(so);
            string errMsg = string.Empty;
            userName = UserData.FirstName + " " + UserData.LastName;

            var clonedProdStock = productStockList.Select(objEntity => (ProductStock)objEntity.Clone()).ToList();
            List<ProductStock> dummyProductStock = new List<ProductStock>(clonedProdStock);

            foreach (var item in so.SalesOrderDetails)
            {
                decimal reserved = 0;
                ProductStock data = dummyProductStock.FirstOrDefault(x => x.Product.ProductID == item.Product.ProductID && x.StockLocation.ID == stockLocationId);
                var data1 = currReserStock.SingleOrDefault(c => c.ProductStockReservedID == item.SalesOrderDetailsID);

                if (data != null)
                {
                    //ProductStock to reserve
                    ProductStockReserved psr = new ProductStockReserved();
                    psr.ProductStockReservedID = item.SalesOrderDetailsID;
                    psr.SalesNo = so.SalesOrderNo;
                    psr.StockLocation = new StockLocation() { ID = so.StockLocation.ID };
                    psr.Product = new Product() { ProductID = item.Product.ProductID };
                    psr.QtyOrdered = item.Quantity;
                    psr.QtyReserved = 0;
                    psr.QtyRemaining = item.Quantity;
                    psr.ReservedDate = DateTime.Now;
                    psr.Status = so.OrderStatus.ToString();
                    psr.OrderLine = item.OrderLine;
                    productStockReserved.Add(psr);
                   
                    if (data1 != null && data1.QtyReserved > 0)
                    {
                        reserved = data1.QtyReserved;
                    }
                    ProductStock ps = new ProductStock();
                    ps.StockLocation = new StockLocation() { ID = so.StockLocation.ID };
                    ps.Product = new Product() { ProductID = item.Product.ProductID };
                    ps.QtyAvailable = data.QtyAvailable += reserved;
                    ps.LastUpdatedDate = DateTime.Now;
                    ps.UpdatedBy = userName;
                    ps.TimeStamp = data.TimeStamp;
                    productStockToDeductList.Add(ps);
                   
                }
                else
                {
                    se.IsError = true;
                    se.ErrorName = "There has been a problem when processing this order" + System.Environment.NewLine + "Cannot retrieve stock for the product " + item.Product.ProductDescription + " [" + item.Product.ProductCode + "]" + System.Environment.NewLine + "Please try again later";
                    break;
                }
            }          

            return Tuple.Create(so, errMsg, productStockReserved, productStockToDeductList,se);
        }

        public bool CheckProductStock(SalesOrder so)
        {
            List<Pair<int, decimal>> ele = new List<Pair<int, decimal>>();
            List<bool> data = new List<bool>();
            string[] prods = null;
            prods = MetaDataManager.GetPriceEditingProducts();
            List<ProductStockReserved> currReserStock = DBAccess.GetReservedProductStock(so);

            foreach (var item in so.SalesOrderDetails)
            {
                //First check if it has stock reserved
                var curResStock = currReserStock.FirstOrDefault(x => x.ProductStockReservedID == item.SalesOrderDetailsID);
                decimal diff, r = curResStock == null ? 0 : curResStock.QtyReserved;


                if (curResStock == null || item.Quantity > r)
                {
                    diff = item.Quantity - r;
                    //diff = (item.Quantity - r) < 0 ? item.Quantity : item.Quantity - r;

                    if (prods != null)
                    {
                        bool exists = prods.Any(x => Convert.ToInt16(x) == item.Product.ProductID);
                        if (exists)
                        {
                            //OffSpec custom                        
                            var d = ele.SingleOrDefault(x => x.ProdID == item.Product.ProductID);
                            if (d != null)
                            {
                                d.Qty += diff;
                            }
                            else
                            {
                                Pair<int, decimal> p = new Pair<int, decimal>();
                                p.ProdID = item.Product.ProductID;
                                p.Qty = diff;
                                ele.Add(p);
                            }
                        }
                        else
                        {
                            //regular product
                            data.Add(productStockList.Any(x => x.Product.ProductID == item.Product.ProductID && x.QtyAvailable >= diff));
                        }
                    }
                }
            }

            if(ele.Count > 0)
            {
                foreach (var item in ele)
                {
                    data.Add(productStockList.Any(x => x.Product.ProductID == item.ProdID && x.QtyAvailable >= item.Qty));
                }                
            }

            bool noProdStock = data.Any(x => x == false);
            return noProdStock;
        }


        public Tuple<SalesOrder, string, List<ProductStockReserved>, List<ProductStock>, SystemError> CancelOrder(SalesOrder so)
        {
            List<ProductStockReserved> productStockReserved = new List<ProductStockReserved>();
            List<ProductStock> productStockToDeductList = new List<ProductStock>();
            string errMsg = string.Empty;
            SystemError se = new SystemError();

            return Tuple.Create(so, errMsg, productStockReserved, productStockToDeductList,se);
        }

        /*************END OF ADD NEW SALES ORDER**************/

        public Tuple<SalesOrder, string, List<ProductStockReserved>, List<ProductStock>,SystemError> RemoveAllocatedStock(SalesOrder so)
        {
            SystemError se = new SystemError();
            List<ProductStockReserved> productStockReserved = new List<ProductStockReserved>();
            List<ProductStock> productStockToDeductList = new List<ProductStock>();
            List<ProductStockReserved> currReserStock = DBAccess.GetReservedProductStock(so);
            string errMsg = string.Empty;
            string userName = UserData.FirstName + " " + UserData.LastName;

            foreach (var item in so.SalesOrderDetails)
            {
                decimal reserved = 0;
                ProductStock data = productStockList.FirstOrDefault(x => x.Product.ProductID == item.Product.ProductID && x.StockLocation.ID == stockLocationId);
                var data1 = currReserStock.SingleOrDefault(c => c.ProductStockReservedID == item.SalesOrderDetailsID);

                if (data != null)
                {
                    //ProductStock to reserve
                    ProductStockReserved psr = new ProductStockReserved();
                    psr.ProductStockReservedID = item.SalesOrderDetailsID;
                    psr.SalesNo = so.SalesOrderNo;
                    psr.StockLocation = new StockLocation() { ID = so.StockLocation.ID };
                    psr.Product = new Product() { ProductID = item.Product.ProductID };
                    psr.QtyOrdered = item.Quantity;
                    psr.QtyReserved = 0;
                    psr.QtyRemaining = item.Quantity;
                    psr.ReservedDate = DateTime.Now;
                    psr.Status = OrderStatus.Cancel.ToString();
                    psr.OrderLine = item.OrderLine;
                    productStockReserved.Add(psr);

                    if (data1 != null && data1.QtyReserved > 0)
                    {
                        reserved = data1.QtyReserved;
                    }
                    ProductStock ps = new ProductStock();
                    ps.StockLocation = new StockLocation() { ID = so.StockLocation.ID };
                    ps.Product = new Product() { ProductID = item.Product.ProductID };
                    ps.QtyAvailable = data.QtyAvailable += reserved;
                    ps.LastUpdatedDate = DateTime.Now;
                    ps.UpdatedBy = userName;
                    ps.TimeStamp = data.TimeStamp;
                    productStockToDeductList.Add(ps);

                }
                else
                {
                    se.IsError = true;
                    se.ErrorName = "There has been a problem when processing this order" + System.Environment.NewLine + "Cannot retrieve stock for the product " + item.Product.ProductDescription + " [" + item.Product.ProductCode + "]" + System.Environment.NewLine + "Please try again later";
                    break;
                }
            }
            return Tuple.Create(so, "", productStockReserved, productStockToDeductList, se);
        }

        public Tuple<SalesOrder, string, List<ProductStockReserved>, List<ProductStock>,SystemError> ReturnStock(SalesOrder so, string userName)
        {
            List<ProductStockReserved> psrL = new List<ProductStockReserved>();
            List<ProductStock> productStock = new List<ProductStock>();
            so.OrderStatus = OrderStatus.Return.ToString();
            so.LastModifiedDate = DateTime.Now;
            so.LastModifiedBy = UserData.FirstName + " " + UserData.LastName;
            SystemError se = new SystemError();
            se.IsError = false;
            se.ErrorName = string.Empty;

            foreach (var item in so.SalesOrderDetails)
            {
                ProductStock ps = new ProductStock();
                ps.StockLocation = new StockLocation() { ID = so.StockLocation.ID };
                ps.Product = new Product() { ProductID = item.Product.ProductID };
                ps.QtyAvailable = item.Quantity;
                ps.LastUpdatedDate = DateTime.Now;
                ps.UpdatedBy = userName;
                productStock.Add(ps);

            }
            return Tuple.Create(so, "", psrL, productStock,se);
        }

        public int ProcessOrder(Order order)
        {
            int res = 0;
            Int32 orderID = 0;
            Tuple<List<GradingOrder>, List<MixingOrder>> elem = null;
            Tuple<Order, Order> splitOrder = null;

            //Create OrderID
            orderID = DBAccess.GenerateNewOrderID();
            if (orderID > 0)
            {
                //Assign the order id
                order.OrderNo = orderID;

                //Convert QTY to Blocks and Logs
                order = CalculateBlocksLogs(order);

                List<Curing> toCuringList = ProcessCuringList(order);

                //if (toCuringList.Count > 0)
                //{
                //Save to Orders/OrderDetails/PendingOrders/PendingSlitPeel/Curing
                Int32 resOP = DBAccess.AddToOrders(order, toCuringList);
                if (resOP > 0)
                {
                    //Check Blocks/Logs Stock
                    StockManager sm = new StockManager();
                    splitOrder = sm.CheckBlockLogStock(order);

                    //Add to Status tables
                    //int statusRes = DBAccess.AddToStatus(splitOrder, order);

                    //Production
                    if (splitOrder.Item1.OrderDetails.Count > 0)
                    {
                        //Seperate Mixing and Grading
                        elem = SeperateMixingAndGrading(splitOrder.Item1);
                        if (elem.Item1.Count > 0)
                        {
                            //Add to Grading
                            int r1 = ProductionManager.AddToGrading(elem.Item1);
                            res = r1 > 0 ? 10 : 0;
                        }
                        if (elem.Item2.Count > 0)
                        {
                            //Add to Mixing
                            int r2 = ProductionManager.AddToMixing(elem.Item2);
                            res = r2 > 0 ? 10 : 0;
                        }

                        //if (elem.Item1.Count > 0 && elem.Item2.Count > 0)
                        //{
                        //    res = 4;
                        //}
                    }
                    //Slitting/Peeling
                    if (splitOrder.Item2.OrderDetails.Count > 0)
                    {
                        List<SlittingOrder> slittingOrderList = new List<SlittingOrder>();
                        List<PeelingOrder> peelingOrderList = new List<PeelingOrder>();

                        foreach (var item in splitOrder.Item2.OrderDetails)
                        {
                            //Decides whether to go to Slitting or Peeling
                            if (item.Product.Type == "Tile")
                            {
                                SlittingOrder so = new SlittingOrder();
                                so.Order = splitOrder.Item2;
                                so.Product = item.Product;
                                so.Qty = item.Quantity;
                                so.Blocks = item.BlocksLogsToMake;
                                so.DollarValue = item.Quantity * item.Product.UnitPrice;
                                slittingOrderList.Add(so);
                            }
                            else if (item.Product.Type == "Bulk")
                            {
                                PeelingOrder po = new PeelingOrder();
                                po.Order = splitOrder.Item2;
                                po.Product = item.Product;
                                po.Logs = item.BlocksLogsToMake;
                                po.DollarValue = item.Quantity * item.Product.UnitPrice;
                                po.IsReRollingReq = false;
                                peelingOrderList.Add(po);
                            }
                            else if (item.Product.Type == "Roll" || item.Product.Type == "Standard")
                            {
                                PeelingOrder po = new PeelingOrder();
                                po.Order = splitOrder.Item2;
                                po.Product = item.Product;
                                po.Logs = item.BlocksLogsToMake;
                                po.DollarValue = item.Quantity * item.Product.UnitPrice;
                                po.IsReRollingReq = true;
                                peelingOrderList.Add(po);
                            }
                        }

                        if (slittingOrderList.Count > 0)
                        {
                            //Add to slitting
                            int r3 = ProductionManager.AddToSlitting(slittingOrderList);
                            res = r3 > 0 ? 10 : 0;
                        }

                        if (peelingOrderList.Count > 0)
                        {
                            //Add to peeling
                            int r4 = ProductionManager.AddToPeeling(peelingOrderList);
                            res = r4 > 0 ? 10 : 0;
                        }
                    }
                }
                else
                {
                    res = 5;
                }
                //}
                //else
                //{
                //    res = 6;
                //}
            }
            else
            {
                res = 3;
            }
            return res;
        }


        public Order CalculateBlocksLogs(Order order)
        {
            foreach (var item in order.OrderDetails)
            {

                if (item.Product.Type == "Tile")
                {
                    item.Quantity = item.Quantity;
                    item.BlocksLogsToMake = Math.Ceiling(item.Quantity / item.Product.Tile.MaxYield);
                }
                else if (item.Product.Type == "Bulk")
                {
                    item.Quantity = item.Quantity;
                    item.BlocksLogsToMake = Math.Ceiling(item.Quantity);
                }
                else if (item.Product.Type == "Roll" || item.Product.Type == "Standard")
                {
                    ProductMeterage pm = new ProductMeterage();
                    pm.Thickness = item.Product.Tile.Thickness;
                    pm.MouldType = item.Product.MouldType;
                    pm.MouldSize = item.Product.Width;
                    List<ProductMeterage> productMeterageList = DBAccess.GetProductMeterageByValues(pm);
                    if (productMeterageList.Count > 0)
                    {
                        decimal maxRollsPerLog = Math.Floor(productMeterageList[0].ExpectedYield / item.Product.Tile.MaxYield);
                        decimal noOfLogsReq = Math.Ceiling(item.Quantity / maxRollsPerLog);
                        item.Quantity = item.Quantity;
                        item.BlocksLogsToMake = noOfLogsReq;
                    }
                }
                else if (item.Product.Type == "Block" || item.Product.Type == "Log" || item.Product.Type == "Curvedge" || item.Product.Type == "Box" || item.Product.Type == "BoxPallet" || item.Product.Type == "Pallet")
                {
                    item.Quantity = item.Quantity;
                    item.BlocksLogsToMake = item.Quantity;
                }
                else if (item.Product.Type == "Custom")//TODO
                {

                }
            }
            return order;
        }

        private List<Curing> ProcessCuringList(Order order)
        {
            List<Curing> toCuringList = new List<Curing>();

            foreach (var item in order.OrderDetails)
            {
                if (item.Product.Type != "Box" && item.Product.Type != "BoxPallet" && item.Product.Type != "Kg" && item.Product.Type != "Pallet")
                {

                    for (int i = 0; i < item.BlocksLogsToMake; i++)
                    {
                        toCuringList.Add(new Curing() { Product = new Product() { ProductID = item.Product.ProductID, Type = item.Product.Type, RawProduct = new RawProduct() { RawProductID = item.Product.RawProduct.RawProductID } }, OrderNo = order.OrderNo, Qty = 1, StartTime = new DateTime(2000, 01, 01), EndTime = new DateTime(2000, 01, 01), IsCured = false, IsEnabled = false });
                    }
                }
            }

            return toCuringList;
        }

        private Tuple<List<GradingOrder>, List<MixingOrder>> SeperateMixingAndGrading(Order order)
        {
            List<MixingOrder> mixingOrderList = new List<MixingOrder>();
            List<GradingOrder> gradingOrderList = new List<GradingOrder>();
            List<MixingOnly> mixingOnlyList = new List<MixingOnly>();
            mixingOnlyList = DBAccess.GetMixingOnly();

            if (mixingOnlyList != null)
            {
                foreach (var item in order.OrderDetails)
                {
                    if (mixingOnlyList.Select(x => x.RawProductID).Contains(item.Product.RawProduct.RawProductID))
                    {
                        MixingOrder mixingOrder = new MixingOrder()
                        {
                            MixingTimeTableID = 0,
                            Product = item.Product,
                            Shift = 0,
                            BlocksLogs = item.BlocksLogsToMake,
                            Qty = item.Quantity,
                            Order = new Order()
                            {
                                OrderNo = order.OrderNo,
                                RequiredDate = order.RequiredDate,
                                OrderType = order.OrderType,
                                SalesNo = order.SalesNo,
                                Comments = order.Comments,
                                Customer = order.Customer
                            }
                        };
                        mixingOrderList.Add(mixingOrder);
                    }
                    else
                    {
                        GradingOrder gradingOrder = new GradingOrder()
                        {
                            GradingTimeTableID = 0,
                            Product = item.Product,
                            Shift = 0,
                            BlocksLogs = item.BlocksLogsToMake,
                            Qty = item.Quantity,
                            Order = new Order()
                            {
                                OrderNo = order.OrderNo,
                                RequiredDate = order.RequiredDate,
                                OrderType = order.OrderType,
                                SalesNo = order.SalesNo,
                                Comments = order.Comments,
                                Customer = order.Customer
                            }
                        };
                        gradingOrderList.Add(gradingOrder);
                    }
                }
            }
            else
            {

                MessageBox.Show("Cannot load mixing only list!!!", "Mixing Only List Failed To Load", MessageBoxButton.OK, MessageBoxImage.Asterisk, MessageBoxResult.Yes);
            }

            Tuple<List<GradingOrder>, List<MixingOrder>> elem = new Tuple<List<GradingOrder>, List<MixingOrder>>(gradingOrderList, mixingOrderList);

            return elem;

        }

        private Tuple<CustomerCreditHistory, CustomerCreditActivity> ProcessCustomerCredit(SalesOrder so)
        {
            CustomerCreditHistory cchnew = new CustomerCreditHistory();
            CustomerCreditActivity customerCreditActivity = new CustomerCreditActivity();
            CustomerCreditHistory cch = DBAccess.GetCustomerCreditHistoryRecord(so.SalesOrderNo, so.Customer.CustomerId);

            if (so.OrderAction == OrderStatus.Cancel.ToString() || so.OrderAction == OrderStatus.Return.ToString())
            {
                string action = "Cancelled";
                if (so.OrderAction == OrderStatus.Return.ToString())
                {
                    action = "Returned";
                }

                
                customerCreditActivity.Amount = so.TotalAmount;
                customerCreditActivity.Type = action;
                customerCreditActivity.Activity = "Order " + action.ToLower() + " credit reallocated : " + String.Format("{0:C}", cch.CreditDeducted);
                cchnew.CreditDeducted = 0;
                cchnew.TotalDebt = (so.Customer.CreditOwed - cch.CreditDeducted) < 0 ? 0 : (so.Customer.CreditOwed - cch.CreditDeducted);
                cchnew.TotalCreditOwed = (so.Customer.CreditOwed - cch.CreditDeducted) < 0 ? 0 : (so.Customer.CreditOwed - cch.CreditDeducted);
                cchnew.TotalCreditRemaining = (so.Customer.CreditRemaining + cch.CreditDeducted) > so.Customer.CreditLimit ? so.Customer.CreditLimit : (so.Customer.CreditRemaining + cch.CreditDeducted);
                cchnew.CreditRemaining = cchnew.TotalCreditRemaining;
                cchnew.Active = false;


                ////Check if previously credit has been deducted
                //if (cch.CreditDeducted == so.TotalAmount)
                //{
                //    //Check if it has any credit owe
                //    decimal x = so.Customer.Debt - so.TotalAmount;
                //    customerCreditActivity.Amount = so.TotalAmount;
                //    customerCreditActivity.Type = "Cancelled";
                //    customerCreditActivity.Activity = "Order cancelled credit reallocated : " + String.Format("{0:C}", so.TotalAmount);

                //    if (x <= 0)
                //    {
                //        cchnew.TotalDebt = x <= 0 ? 0 : so.Customer.Debt;
                //        cchnew.TotalCreditRemaining = so.Customer.CreditRemaining + Math.Abs(x);
                //    }
                //    else
                //    {
                //        cchnew.TotalDebt = x < 0 ? 0 : x;
                //        cchnew.TotalCreditRemaining = so.Customer.CreditRemaining + x;
                //    }

                //    cchnew.CreditDeducted = 0;
                //    if (so.OrderAction == OrderStatus.Return.ToString())
                //    {
                //        cchnew.CreditDeducted = so.TotalAmount;
                //        customerCreditActivity.Amount = so.TotalAmount;
                //        customerCreditActivity.Type = "Returned";
                //        customerCreditActivity.Activity = "Order returned credit reallocated : " + String.Format("{0:C}", so.TotalAmount);
                //    }

                //    cchnew.CreditRemaining = cchnew.TotalCreditRemaining;
                //    cchnew.Debt = cchnew.TotalDebt;
                //    cchnew.Active = false;
                //}
                //else
                //{   //  100            500             600
                //    decimal x = cch.CreditDeducted - so.TotalAmount;
                //    if (x > 0)
                //    {
                //        cchnew.TotalDebt = (so.Customer.Debt - so.TotalAmount) < 0 ? 0 : so.Customer.Debt - so.TotalAmount;
                //        if (cchnew.TotalDebt > 0)
                //        {
                //            cchnew.TotalCreditRemaining = so.Customer.CreditRemaining;
                //        }
                //        else
                //        {
                //            decimal c = so.TotalAmount - so.Customer.Debt;
                //            cchnew.TotalCreditRemaining = cchnew.TotalCreditRemaining + c;
                //        }
                //    }

                //    customerCreditActivity.Type = "Returned";
                //    customerCreditActivity.Activity = "Order returned credit reallocated : " + String.Format("{0:C}", cch.CreditDeducted);
                //    cchnew.CreditDeducted = x;
                //    cchnew.CreditRemaining = cchnew.TotalCreditRemaining;
                //    cchnew.Debt = cchnew.TotalDebt;
                //    cchnew.Active = false;
                //}
                //cchnew.TotalCreditOwed = so.Customer.CreditOwed - so.TotalAmount;
            }
            else if (so.OrderStatus == OrderStatus.Hold.ToString() || so.OrderStatus == OrderStatus.HoldNoCredit.ToString() || so.OrderStatus == OrderStatus.HoldNoCreditNoStock.ToString() ||
                so.OrderStatus == OrderStatus.HoldNoStock.ToString() || so.OrderStatus == OrderStatus.NoStock.ToString())
            {
                //If credit deducted, allocate back to customer//Check if previously credit has been deducted
                if (cch.CreditDeducted > 0)
                {
                    cchnew.TotalDebt = (so.Customer.Debt - cch.CreditDeducted) <= 0 ? 0 : so.Customer.Debt - cch.CreditDeducted;
                    if (cchnew.TotalDebt == 0)
                    {
                        cchnew.TotalCreditRemaining = so.Customer.CreditRemaining + (cch.CreditDeducted - so.Customer.Debt);//) > so.Customer.CreditLimit ? so.Customer.CreditLimit : (so.Customer.CreditRemaining + (cch.CreditDeducted - so.Customer.Debt));
                    }
                    cchnew.TotalCreditOwed = (so.Customer.CreditOwed - cch.CreditDeducted) < 0 ? 0 : (so.Customer.CreditOwed - cch.CreditDeducted);
                }
                else
                {
                    cchnew.TotalDebt = so.Customer.Debt;
                    cchnew.TotalCreditRemaining = so.Customer.CreditRemaining;
                    cchnew.TotalCreditOwed = so.Customer.CreditOwed;
                }
                string str = cch.CreditDeducted > 0 ? " credit reallocated : " : " new order : ";
                customerCreditActivity.Type = so.OrderStatus.ToString();
                customerCreditActivity.Activity = "Order " + so.OrderStatus.ToString() + str + String.Format("{0:C}", cch.CreditDeducted);
                cchnew.CreditDeducted = 0;
                cchnew.CreditRemaining = cchnew.TotalCreditRemaining;
                cchnew.Debt = cchnew.TotalDebt;
                cchnew.Active = true;
            }
            else
            {
                cchnew.Active = true;
                decimal differce = 0;

                //First check if the new order total is bigger/smaller than the previous total
                //80                      100
                if (so.TotalAmount > cch.CreditDeducted)
                {
                    differce = so.TotalAmount - cch.CreditDeducted;

                    if (differce <= so.Customer.CreditRemaining)
                    {
                        cchnew.TotalDebt = 0;
                        cchnew.TotalCreditRemaining = so.Customer.CreditRemaining - differce;
                        cchnew.CreditDeducted = so.TotalAmount;
                    }
                    else
                    {
                        decimal x = differce - so.Customer.CreditRemaining;
                        cchnew.TotalCreditRemaining = 0;
                        cchnew.TotalDebt = so.Customer.Debt + Math.Abs(x);
                        cchnew.CreditDeducted = so.TotalAmount;
                        //cchnew.TotalCreditOwed = so.Customer.CreditOwed + x;
                    }
                    cchnew.TotalCreditOwed = so.Customer.CreditOwed + differce;
                }
                else
                {   // 20         100                     80
                    differce = cch.CreditDeducted - so.TotalAmount;
                    //       10                   30          20
                    cchnew.TotalDebt = (so.Customer.Debt - differce) < 0 ? 0 : so.Customer.Debt - differce;

                    if (cchnew.TotalDebt == 0)
                    {
                        cchnew.TotalCreditRemaining = so.Customer.CreditRemaining + Math.Abs(so.Customer.Debt - differce);
                        cchnew.CreditDeducted = so.TotalAmount;
                        cchnew.TotalCreditOwed = so.Customer.CreditOwed - differce;
                    }
                    else
                    {
                        //                                                30                        10      
                        cchnew.TotalCreditRemaining = (so.Customer.CreditRemaining - cchnew.TotalDebt) <= 0 ? 0 : so.Customer.CreditRemaining - cchnew.TotalDebt;
                        //cchnew.TotalCreditOwe = (cchnew.TotalCreditOwe - so.Customer.CreditRemaining) <= 0 ? 0 : cchnew.TotalCreditOwe - so.Customer.CreditRemaining;
                        cchnew.CreditDeducted = so.TotalAmount;
                        cchnew.TotalCreditOwed = so.Customer.CreditOwed - cchnew.TotalDebt < 0 ? 0 : so.Customer.CreditOwed - cchnew.TotalDebt;
                    }
                }
                customerCreditActivity.Amount = so.TotalAmount;
                customerCreditActivity.Type = cch.CreditDeducted == 0 ? "New Order" : "Updated";
                customerCreditActivity.Activity = cch.CreditDeducted == 0 ? "New Order : " + String.Format("{0:C}", so.TotalAmount) : "Order updated : " + String.Format("{0:C}", so.TotalAmount);

                cchnew.CreditRemaining = cchnew.TotalCreditRemaining;
                cchnew.Debt = cchnew.TotalDebt;
                cchnew.Active = true;

            }
            customerCreditActivity.UpdatedBy = UserData.FirstName + " " + UserData.LastName;
            customerCreditActivity.UpdatedDate = DateTime.Now;

            return Tuple.Create(cchnew, customerCreditActivity);
        }

        private DateTime CalculatePaymentDueDate(SalesOrder so)
        {
            DateTime date = DateTime.Now;

            if (so.Customer != null && so.Customer.CustomerType == "Account")
            {
                date.AddDays(30);
            }
            else if (so.Customer == null || so.Customer.CustomerType == "Prepaid")
            {
                date = (DateTime)so.DesiredDispatchDate;
            }
            return date;
        }

        public string ConvertToOrderStatus(string os)
        {
            string orderStatus = string.Empty;
            switch (os)
            {
                case "ReadyToDispatch": orderStatus = "Release";
                    break;
                case "PreparingInvoice": orderStatus = "Release";
                    break;
                case "Dispatched": orderStatus = "Dispatched";
                    break;
                case "InWarehouse": orderStatus = "Release";
                    break;
                case "Picking": orderStatus = "Release";
                    break;
                case "FinalisingShipping": orderStatus = "Release";
                    break;
                case "Hold": orderStatus = "Hold";
                    break;
                case "HoldNoStock": orderStatus = "Hold";
                    break;
                case "HoldStockAllocated": orderStatus = "Hold";
                    break;
                case "HoldNoCredit": orderStatus = "Hold";
                    break;
                case "HoldNoCreditStockAllocated": orderStatus = "Hold";
                    break;
                case "HoldNoCreditNoStock": orderStatus = "Hold";
                    break;
                case "AwaitingStock": orderStatus = "Release";
                    break;
                case "Cancel": orderStatus = "Cancel";
                    break;
                case "Return": orderStatus = "Return";
                    break;
                default:
                    break;
            }
            return orderStatus;
        }

        public string ConstructNoStockErrorMsg(List<ProductStockReserved> productStockReserved, List<ProductStock> dummyProductStock, int stockLocationId)
        {
            string errMsg = string.Empty;

            foreach (var item in productStockReserved)
            {
                if (item.QtyRemaining > 0)
                {
                    var ps = dummyProductStock.SingleOrDefault(x => x.StockLocation.ID == stockLocationId && x.Product.ProductID == item.Product.ProductID);
                    if (ps != null)
                    {
                        errMsg += item.Product.ProductDescription + " requires " + item.QtyOrdered.ToString("G29") + CultureInfo.CurrentCulture.TextInfo.ToTitleCase(item.Product.ProductUnit.ToUpper())
                        + " but " + ps.QtyAvailable.ToString("G29") + CultureInfo.CurrentCulture.TextInfo.ToTitleCase(item.Product.ProductUnit.ToUpper()) + " on hand!" + System.Environment.NewLine;
                    }
                }
            }
            return errMsg;
        }
    }
}