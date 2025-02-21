using A1RConsole.DB;
using A1RConsole.Models.Customers;
using A1RConsole.Models.Deliveries;
using A1RConsole.Models.Freights;
using A1RConsole.Models.Orders;
using A1RConsole.Models.Production;
using A1RConsole.Models.Products;
using A1RConsole.Models.Stock;
using A1RConsole.Models.Transactions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace A1RConsole.Core
{
    public class StockManager
    {
        private List<ProductMeterage> prodMeterageList;

        public Tuple<Order, Order> CheckBlockLogStock(Order order)
        {
            Tuple<Order, Order> splitOrder = null;
            prodMeterageList = new List<ProductMeterage>();

            Order prodOrder = new Order();
            Order slitPeelOrder = new Order();

            prodOrder = CopyOrder(order);
            slitPeelOrder = CopyOrder(order);
            prodOrder.OrderDetails = new BindingList<OrderDetails>();
            slitPeelOrder.OrderDetails = new BindingList<OrderDetails>();

            //prodOrder.OrderDetails.Clear();
            //slitPeelOrder.OrderDetails.Clear();
            prodMeterageList = DBAccess.GetProductMeterage();


            if (prodMeterageList.Count > 0 || prodMeterageList != null)
            {
                foreach (var itemOD in order.OrderDetails)
                {
                    if (itemOD.BlocksLogsToMake > 0)
                    {
                        PendingSlitPeel psp = new PendingSlitPeel() { Product = itemOD.Product };
                        RawStock rawStock = DBAccess.GetBlockLogStockByID(psp);

                        if (itemOD.Product.RawProduct.RawProductID == rawStock.RawProductID)
                        {
                            decimal toMakeBL = 0;
                            decimal toSlitPeelBL = 0;
                            decimal qtyToDeductBL = 0;
                            decimal toMakeQty = 0;
                            decimal toSlitPeelQty = 0;
                            OrderDetails odProd = new OrderDetails();
                            OrderDetails odSlitPeel = new OrderDetails();

                            if (order.OrderPriority == 1)
                            {
                                //Check block/log stock and slit/peel or produce
                                if (itemOD.Product.Type != "Block" || itemOD.Product.Type != "Log" || itemOD.Product.Type != "Box")
                                {
                                    //Block/log checking
                                    if (itemOD.BlocksLogsToMake <= rawStock.Qty && rawStock.Qty > 0)//Full stock available
                                    {
                                        toMakeBL = 0;
                                        toSlitPeelBL = itemOD.BlocksLogsToMake;
                                        qtyToDeductBL = itemOD.BlocksLogsToMake;
                                        toMakeQty = 0;
                                        toSlitPeelQty = itemOD.Quantity;
                                    }
                                    else if (itemOD.BlocksLogsToMake > rawStock.Qty && rawStock.Qty > 0)//Partial stock available
                                    {
                                        toMakeBL = itemOD.BlocksLogsToMake - rawStock.Qty;
                                        toSlitPeelBL = rawStock.Qty;
                                        qtyToDeductBL = rawStock.Qty;
                                        toSlitPeelQty = CalculateQty(itemOD.Product, toSlitPeelBL);
                                        toMakeQty = itemOD.Quantity - toSlitPeelQty;
                                    }
                                    else if (rawStock.Qty <= 0)
                                    {
                                        toMakeBL = itemOD.BlocksLogsToMake;//No stock available
                                        toSlitPeelBL = 0;
                                        qtyToDeductBL = 0;
                                        toMakeQty = itemOD.Quantity;
                                        toSlitPeelQty = 0;
                                    }

                                    //Production
                                    if (toMakeBL > 0)
                                    {
                                        odProd.Product = itemOD.Product;
                                        odProd.Quantity = toMakeQty;
                                        odProd.BlocksLogsToMake = toMakeBL;
                                        prodOrder.OrderDetails.Add(odProd);
                                    }

                                    //Deduct from block/log stock and PendingSlitPeel
                                    if (qtyToDeductBL > 0 && toSlitPeelBL > 0)
                                    {
                                        //For Slitting and Peeling
                                        odSlitPeel.Product = itemOD.Product;
                                        odSlitPeel.Quantity = toSlitPeelQty;
                                        odSlitPeel.BlocksLogsToMake = toSlitPeelBL;
                                        slitPeelOrder.OrderDetails.Add(odSlitPeel);

                                        //For Block/Log Stock deduction
                                        RawStock rs = new RawStock();
                                        rs.RawProductID = itemOD.Product.RawProduct.RawProductID;
                                        rs.Qty = (rawStock.Qty - qtyToDeductBL) < 0 ? 0 : (rawStock.Qty - qtyToDeductBL);

                                        //For PendingSlitpleel deduction
                                        PendingSlitPeel ps = new PendingSlitPeel();
                                        ps.Order = order;
                                        ps.Product = itemOD.Product;
                                        ps.Qty = toSlitPeelQty;
                                        ps.BlockLogQty = toSlitPeelBL;

                                        //Update Block/Log Stock and PendingSlitPeel
                                        int res = DBAccess.UpdateBlockLogPendingSlitPeel(rs, ps);
                                        Console.WriteLine(res > 0 ? "Block/Log and SlitPeel Updated" : "Block/Log and SlitPeel Failed");
                                    }
                                }
                            }
                            else if (order.OrderPriority == 2)
                            {
                                //No block/log stock checking. Add to production and no slit/peel
                                //Production
                                odProd.Product = itemOD.Product;
                                odProd.Quantity = itemOD.Quantity;
                                odProd.BlocksLogsToMake = itemOD.BlocksLogsToMake;
                                prodOrder.OrderDetails.Add(odProd);
                            }
                        }
                    }
                }
            }
            else
            {
                //Msg.Show("Cannot load the product meterage data ", "Failed To Load Product Meterage", MsgBoxButtons.OK, MsgBoxImage.Error, MsgBoxResult.Yes);
            }


            splitOrder = new Tuple<Order, Order>(prodOrder, slitPeelOrder);

            return splitOrder;
        }

        private Order CopyOrder(Order order)
        {
            Order o = new Order();
            o.OrderNo = order.OrderNo;
            o.OrderType = order.OrderType;
            o.OrderPriority = order.OrderPriority;
            o.RequiredDate = order.RequiredDate;
            o.SalesNo = order.SalesNo;
            o.Comments = order.Comments;
            o.DeliveryDetails = new List<Delivery>() { new Delivery() { FreightID = order.DeliveryDetails[0].FreightID } };
            o.IsRequiredDateSelected = order.IsRequiredDateSelected;
            o.OrderCreatedDate = order.OrderCreatedDate;
            o.Customer = new Customer() { CustomerId = order.Customer.CustomerId };

            return o;
        }

        private decimal CalculateQty(Product product, decimal blockLog)
        {
            decimal qty = 0;

            if (product.Type == "Tile")
            {
                qty = Math.Floor(blockLog * product.Tile.MaxYield);
            }
            else if (product.Type == "Bulk" || product.Type == "Box" || product.Type == "BoxPallet")
            {
                qty = blockLog;
            }
            else if (product.Type == "Roll" || product.Type == "Standard")
            {
                if (prodMeterageList.Count > 0)
                {
                    var data = prodMeterageList.Single(c => c.Thickness == product.Tile.Thickness && c.MouldType == product.MouldType && c.MouldSize == product.Width);
                    decimal maxRollsPerLog = Math.Floor(data.ExpectedYield / product.Tile.MaxYield);
                    qty = maxRollsPerLog * blockLog;
                }
            }
            else if (product.Type == "Block" || product.Type == "Log" || product.Type == "Curvedge")
            {
                qty = blockLog;
            }
            else if (product.Type == "Custom")//TODO
            {

            }

            return qty;
        }


        private List<RawStock> LoadRawStock()
        {
            return DBAccess.GetAllBlockLogStock();
        }

        /*****Allocate order for stock*****/
        public int AllocateStockForOrder(List<SalesOrder> so)
        {
            int res = AllocateStock(so,"Normal");

            return res;
        }

        /*****Allocate stock for single order*****/
        public void AllocateOrderForStock(ProductStock ps)
        {
            List<SalesOrder> so = DBAccess.GetNoStockSalesOrders(ps);

            AllocateStock(so, "Silent");
        }

        private int AllocateStock(List<SalesOrder> so, string processType)
        {
            int finalRes = 0;

            if (so.Count == 1)
            {
                //Process this order automatically
                List<Tuple<string, Int16, string>> timeStamps = null;
                Tuple<bool, SalesOrder, List<ProductStockReserved>, List<ProductStock>, bool, Tuple<CustomerCreditHistory, CustomerCreditActivity>> result = null;
                OrderManager om = new OrderManager();
                List<ProductStock> productStockList = new List<ProductStock>();
                

                //Get the freight details
                so[0] = DBAccess.GetSalesOrderDetails(so[0].SalesOrderNo);
                if (so[0].FreightDetails != null)
                {
                    timeStamps = DBAccess.GetUpdateSalesOrderTimeStamp(so[0].SalesOrderNo);
                    productStockList = DBAccess.GetProductStockByStock(1);//Get product stock
                    Customer customer = DBAccess.GetCustomerDataByCustomerID(so[0].Customer.CustomerId);
                    so[0].OrderStatus = OrderStatus.Release.ToString();
                    so[0].OrderAction = OrderStatus.Release.ToString();

                    om.InitiateSalesOrder(1, productStockList);

                    //Get customer name
                    //var data = CustomerList.SingleOrDefault(c => c.CustomerId == so[0].Customer.CustomerId);
                    if (customer.CompanyName != null)
                    {
                        so[0].Customer.CompanyName = customer.CompanyName;
                    }
                    else
                    {
                        so[0].Customer.CompanyName = DBAccess.GetPrePaidCustomer(0,so[0].SalesOrderNo);
                        so[0].Customer.CustomerType = "Prepaid";
                    }

                    if (processType == "Silent")
                    {
                        //Silent process
                        result = om.SilentProcessSalesOrder(so[0], so[0].Customer.CompanyName, UserData.FirstName + " " + UserData.LastName);
                    }
                    else
                    {
                        //Usual process
                        result = om.ProcessSalesOrder2(so[0], so[0].Customer.CompanyName, UserData.FirstName + " " + UserData.LastName);
                    }

                    if (result.Item1)
                    {
                        finalRes = om.UpdateSalesOrderDB(result.Item2, result.Item3, result.Item4, UserData.FirstName + " " + UserData.LastName, result.Item6, timeStamps);
                        if (finalRes == 1)
                        {
                            List<TransactionRecord> tr = new List<TransactionRecord>();
                            tr.Add(new TransactionRecord() { DateTime = DateTime.Now, Area = "Product Maintenance", Description = "Update qty available through text box then send HOLDNOSTOCK order to warehouse", ScriptName = "Stock Manager", Values = "", Result = "Success - " + so[0].SalesOrderNo + " stock allocated", UserName = UserData.FirstName + " " + UserData.LastName });
                            TransactionManager.CreateTransaction(tr);
                        }
                        else if (finalRes == -1)
                        {
                            List<TransactionRecord> tr = new List<TransactionRecord>();
                            tr.Add(new TransactionRecord() { DateTime = DateTime.Now, Area = "Product Maintenance", Description = "Update qty available through text box then send HOLDNOSTOCK order to warehouse", ScriptName = "Stock Manager", Values = "", Result = "Data has been changed - " + so[0].SalesOrderNo, UserName = UserData.FirstName + " " + UserData.LastName });
                            TransactionManager.CreateTransaction(tr);
                        }
                        else if (finalRes == 0)
                        {
                            //MessageBox.Show("You haven't made any changes to update", "No Changes Were Made", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            List<TransactionRecord> tr = new List<TransactionRecord>();
                            tr.Add(new TransactionRecord() { DateTime = DateTime.Now, Area = "Product Maintenance", Description = "Update qty available through text box then send HOLDNOSTOCK order to warehouse", ScriptName = "Stock Manager", Values = "", Result = "Problem when updating this order - " + so[0].SalesOrderNo, UserName = UserData.FirstName + " " + UserData.LastName });
                            TransactionManager.CreateTransaction(tr);
                        }
                    }
                }
            }
            else
            {
                //There are multiple orders for the single item so send the alert to the user to maually process order
            }

            return finalRes;
        }


        
    }
}
