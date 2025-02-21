using A1RConsole.DB;
using A1RConsole.Models.Customers;
using A1RConsole.Models.Freights;
using A1RConsole.Models.Orders;
using A1RConsole.Models.Products;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A1RConsole.Core
{
    public static class CheckDataChangeManager
    {

        public static List<Tuple<string, string, Int32>> CheckSalesOrderChanged(ObservableCollection<Customer> customerList, SalesOrder so, string customer, int locationID)
        {
            Int32 cusID=0;
            List<Tuple<string, string, Int32>> resTup = new List<Tuple<string, string, Int32>>();
            var data = customerList.FirstOrDefault(s => s.CompanyName == customer);
            cusID = data != null ? data.CustomerId : 0;
            List<Tuple<string, string, Int32>> tup = DBAccess.GetSalesOrderTimeStamps(cusID, so, locationID);
            //string[] prods = null;
            //prods = MetaDataManager.GetPriceEditingProducts();

            /***Check Sales Order***/
            if (so.SalesOrderNo > 0)
            {               

                /*****Check Sales Order*****/
                foreach (var item in tup)
                {
                    if (item.Item1 == "SO" && !item.Item2.Equals(so.TimeStamp))
                    {
                        resTup.Add(Tuple.Create("SO", "Order details have been changed", item.Item3));
                        break;
                    }
                }
                //Sales Order Details
                foreach (var items in so.SalesOrderDetails) 
                {                  
                    var has = tup.SingleOrDefault(x => x.Item1 == "SOD" && x.Item3 == items.SalesOrderDetailsID);
                    if (has != null)
                    {
                        if (!has.Item2.Equals(items.SODTimeStamp))
                        {
                            resTup.Add(Tuple.Create("SOD", "Sales order details of [" + items.Product.ProductCode + "] has been changed", 0));
                        }
                    }
                    else
                    {
                        resTup.Add(Tuple.Create("SOD", "Product [" + items.Product.ProductCode + "] has been removed!", 0));
                    }                   
                }
                //Check someone has added a product to the sales order while the form is opened
                foreach (var items in tup)
                {
                    if (items.Item1 == "SOD")
                    {
                        var has = so.SalesOrderDetails.SingleOrDefault(x => x.SalesOrderDetailsID == items.Item3);
                        if (has == null)
                        {
                            resTup.Add(Tuple.Create("SOD", "New product has been added!", 0));                            
                        }                        
                    }
                }

                //Product Details
                foreach (var item in tup)
                {
                    foreach (var items in so.SalesOrderDetails)
                    {
                        if (item.Item3 == items.SalesOrderDetailsID && item.Item1 == "PROD" && !item.Item2.Equals(items.Product.TimeStamp))
                        {
                            resTup.Add(Tuple.Create("PROD", "Product details of [" + items.Product.ProductCode + "] have been changed", 0));
                        }
                    }
                }
                //Product Stock Reserved
                foreach (var items in so.SalesOrderDetails) 
                {
                    var has = tup.SingleOrDefault(x => x.Item1 == "PRODSTOCK" && x.Item3 == items.SalesOrderDetailsID);
                    if (has != null)
                    {
                        if (!has.Item2.Equals(items.PSTimeStamp))
                        {
                            resTup.Add(Tuple.Create("PRODSTOCK", "Stock qty of [" + items.Product.ProductCode + "] has been changed", 0));
                        }
                    }
                    else
                    {
                        resTup.Add(Tuple.Create("PRODSTOCK", "Product stock [" + items.Product.ProductCode + "] has been changed!", 0));
                    }                   
                }
                //Freight Details
                foreach (var items in so.FreightDetails)
                {
                    var has = tup.SingleOrDefault(x=>x.Item1 == "FD" && x.Item3 == items.FreightCodeDetails.FreightCodeID);
                    if(has != null)
                    {
                        if(!has.Item2.Equals(items.TimeStamp))
                        {
                            resTup.Add(Tuple.Create("FD", "Freight details of [" + items.FreightCodeDetails.Code + "] has been changed", 0));
                        }
                    }
                    else
                    {
                        resTup.Add(Tuple.Create("FD", "Freight [" + items.FreightCodeDetails.Code + "] has been removed!", 0));
                    }
                }
                //Check someone has added freight to the sales order while the form is opened
                foreach (var items in tup)
                {
                    if (items.Item1 == "FD")
                    {
                        var has = so.FreightDetails.SingleOrDefault(x => x.FreightCodeDetails.FreightCodeID == items.Item3);
                        if (has == null)
                        {
                            resTup.Add(Tuple.Create("FD", "New freight has been added to the sales order!", 0));
                        }
                    }
                }

                foreach (var item in tup)
                {
                    foreach (var items in so.FreightDetails)
                    {
                        if (item.Item3 == items.FreightCodeDetails.FreightCodeID && item.Item1 == "FD" && !item.Item2.Equals(items.TimeStamp))
                        {
                            resTup.Add(Tuple.Create("FD", "Freight details of [" + items.FreightCodeDetails.Code + "] has been changed", 0));
                        }
                    }
                }

                //Comments
                foreach (var items in tup)
                {
                    if (items.Item1 == "COM")
                    {
                        foreach (var item in so.Comments)
                        {
                            if (!items.Item2.Equals(item.TimeStamp) && item.LocationID == items.Item3)
                            {
                                resTup.Add(Tuple.Create("COM", "Comment details have been changed", item.LocationID));
                            }
                        }
                    }
                }
            }
            else
            {
                foreach (var item in tup)
                {
                    foreach (var items in so.SalesOrderDetails)
                    {
                        if (item.Item3 == items.SalesOrderDetailsID && item.Item1 == "PROD" && !item.Item2.Equals(items.Product.TimeStamp))
                        {
                            resTup.Add(Tuple.Create("PROD", "Product details of [" + items.Product.ProductCode + "] have been changed", 0));
                        }
                    }
                }

                foreach (var item in tup)
                {
                    foreach (var items in so.SalesOrderDetails)
                    {
                        if (item.Item3 == items.SalesOrderDetailsID && item.Item1 == "PRODSTOCK" && !item.Item2.Equals(items.SODTimeStamp))
                        {
                            resTup.Add(Tuple.Create("PRODSTOCK", "Stock qty of [" + items.Product.ProductCode + "] has been changed", 0));
                        }
                    }
                }

                /***Check Freight Details***/
                foreach (var item in tup)
                {
                    foreach (var items in so.FreightDetails)
                    {
                        if (item.Item3 == items.FreightCodeDetails.FreightCodeID)
                        {
                            if (item.Item1 == "FREIGHT" && !item.Item2.Equals(items.FreightCodeDetails.TimeStamp))
                            {
                                resTup.Add(Tuple.Create("FREIGHT", "Freight details of [" + items.FreightCodeDetails.Code + "] have been changed", 0));
                            }
                        }
                    }
                }
            }


            /***Check customer***/           

            if (data != null)
            {
                //Check customer
                foreach (var item in tup)
                {
                    if (item.Item1 == "CUS")
                    {
                        if (!item.Item2.Equals(data.TimeStamp))
                        {
                            resTup.Add(Tuple.Create("CUS", "Customer details of [" + data.CompanyName + "] have been changed", item.Item3));
                            break;
                        }
                    }
                }
                
                //Discount

                foreach (var item in tup)
                {
                    if (item.Item1 == "DIS")
                    {
                        var has = data.DiscountStructure.SingleOrDefault(x => x.Category.CategoryID == item.Item3 && x.Discount > 0);
                        if (has != null)
                        {
                            if (!item.Item2.Equals(has.TimeStamp))
                            {
                                resTup.Add(Tuple.Create("DIS", "Discount of [" + has.Category.CategoryName + "] has been changed", item.Item3));
                            }
                        }
                        else
                        {
                            resTup.Add(Tuple.Create("DIS", "New Discount has been added", item.Item3));
                        }
                    }
                }

                foreach (var item in data.DiscountStructure)
                {
                    if (item.Discount > 0)
                    {
                        var has = tup.SingleOrDefault(x => x.Item1 == "DIS" && x.Item3 == item.Category.CategoryID);
                        if (has == null)
                        {
                            resTup.Add(Tuple.Create("DIS", "Discount [" + item.Category.CategoryName + "] has been removed!", 0));
                        }
                    }                  
                }    
            }
            else
            {
                //Prepaid customer not in db

            }

            /***Check Carrier Details***/
            foreach (var item in tup)
            {
                if (item.Item3 == so.FreightCarrier.Id && item.Item1 == "CARRIER" && so.FreightCarrier.TimeStamp != null && !item.Item2.Equals(so.FreightCarrier.TimeStamp))
                {
                    resTup.Add(Tuple.Create("CARRIER", "Carrier details of [" + so.FreightCarrier.FreightName + "] have been changed", 0));
                }
            }

            return resTup;
        }


        public static Tuple<Customer, List<ProductStock>, List<FreightCode>, FreightCarrier, SalesOrder> GetUpdatedSalesOrderDetails(Int32 cusID, int stockLocation, SalesOrder so)
        {

            /***Get updated details***/
            Tuple<Customer, List<ProductStock>, List<FreightCode>, FreightCarrier, SalesOrder> updatedTup = DBAccess.GetUpdatedSalesOrderDetailsDB(cusID, stockLocation, so);

            return updatedTup;
        }
    }
}
