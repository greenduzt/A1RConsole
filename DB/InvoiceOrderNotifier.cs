using A1RConsole.Core;
using A1RConsole.Models.Dispatch;
using A1RConsole.Models.Freights;
using A1RConsole.Models.Invoices;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A1RConsole.DB
{
    public class InvoiceOrderNotifier : IDisposable
    {

        private event EventHandler<SqlNotificationEventArgs> _newMessage;
        private SqlConnection connection;
        public SqlCommand CurrentCommand { get; set; }

        public InvoiceOrderNotifier()
        {
            SqlDependency.Start(DBConfiguration.DbConnectionString);
        }

        public SqlConnection CurrentConnection
        {
            get
            {
                this.connection = this.connection ?? new SqlConnection(DBConfiguration.DbConnectionString);
                return this.connection;
            }
        }

        public event EventHandler<SqlNotificationEventArgs> NewMessage
        {
            add
            {
                this._newMessage += value;
            }
            remove
            {
                this._newMessage -= value;
            }
        }

        public virtual void OnNewMessage(SqlNotificationEventArgs notification)
        {
            if (this._newMessage != null)
                this._newMessage(this, notification);
        }
        public ObservableCollection<Invoice> RegisterDependency(string type, DateTime from, DateTime to)
        {
            string where_clause = string.Empty;
            DateTime days2 = DateTime.Now.AddDays(-2);

            ObservableCollection<Invoice> invoicingList = new ObservableCollection<Invoice>();

            //switch (type)
            //{
            //    case "ReadyToDispatchChecked": where_clause = "dbo.DispatchOrders.order_dispatched = 'False' AND dbo.DispatchOrders.order_status ='Preparing'";
            //        break;
            //    case "LastTwoDays": where_clause = "dbo.DispatchOrders.order_dispatched = 'True' AND DispatchOrders.dispatched_date >= @Days2 AND dbo.DispatchOrders.order_status ='Finalised'";
            //        break;
            //    case "DateRange":
            //        //where_clause = "dbo.DispatchOrders.order_dispatched = 'True' AND DispatchOrders.dispatched_date >= @FromDate AND DispatchOrders.dispatched_date <= @ToDate AND dbo.DispatchOrders.order_status ='Finalised'";
            //        where_clause = "dbo.DispatchOrders.order_dispatched = 'True' AND DispatchOrders.dispatched_date BETWEEN @FromDate AND @ToDate AND dbo.DispatchOrders.order_status ='Finalised'";

            //        break;
            //    default:
            //        break;
            //}

            this.CurrentCommand = new SqlCommand("SELECT Invoice.id,Invoice.sales_order_no,DispatchOrders.id AS delivery_docket_no,Invoice.invoiced_date,SalesOrder.customer_id,Invoice.exported_to_myob, " + 
                                                 "Invoice.completed_date,Invoice.completed_by,Invoice.is_completed,Invoice.is_active,DispatchOrders.dispatched_date,SalesOrder.dispatch_date, " + 
                                                 "SalesOrder.total_amount,SalesOrder.order_status,SalesOrder.payment_recieved,SalesOrder.customer_order_no,SalesOrder.order_date,SalesOrder.terms_id, " + 
                                                 "Freight.FreightName " +
                                                 "FROM dbo.Invoice " +
                                                 "INNER JOIN dbo.DispatchOrders ON Invoice.sales_order_no = DispatchOrders.sales_no " +
                                                 "INNER JOIN dbo.SalesOrder ON DispatchOrders.sales_no = SalesOrder.sales_no " +
                                                 "INNER JOIN dbo.Freight ON SalesOrder.freight_id=Freight.id  " +
                                                 "ORDER BY dbo.DispatchOrders.dispatched_date desc", this.CurrentConnection);

           // SqlCommand cmdGeCustomer = new SqlCommand("SELECT company_name FROM Customers " +
           //                                           "WHERE id = @CustomerID", CurrentConnection);

           // SqlCommand cmdGePrepaidCustomer = new SqlCommand("SELECT prepaid_customer_name FROM PrePaidCustomers " +
           //                                                  "WHERE sales_no = @SalesNo", CurrentConnection);


            this.CurrentCommand.Notification = null;


            SqlDependency dependency = new SqlDependency(this.CurrentCommand);
            dependency.OnChange += this.dependency_OnChange;

            if (this.CurrentConnection.State == ConnectionState.Closed)
                this.CurrentConnection.Open();
            try
            {

                //CurrentCommand.Parameters.AddWithValue("@Days2", days2);
                //CurrentCommand.Parameters.AddWithValue("@FromDate", from.Date);
                //CurrentCommand.Parameters.AddWithValue("@ToDate", to.Date);
                using (SqlDataReader dr = this.CurrentCommand.ExecuteReader(CommandBehavior.CloseConnection))
                {
                    if (dr != null)
                    {
                        while (dr.Read())
                        {
                            Invoice invoice = new Invoice();
                            invoice.InvoiceNo = Convert.ToInt16(dr["id"]);
                            invoice.SalesOrderNo = Convert.ToInt16(dr["sales_order_no"]);
                            invoice.DesiredDispatchDate = Convert.ToDateTime(dr["dispatch_date"]);
                            invoice.CustomerOrderNo = dr["customer_order_no"].ToString();
                            invoice.OrderDate = Convert.ToDateTime(dr["order_date"]);
                            invoice.TermsID = dr["terms_id"].ToString();
                            invoice.TotalAmount = Convert.ToDecimal(dr["total_amount"]);
                            invoice.FreightCarrier = new FreightCarrier() { FreightName = dr["FreightName"].ToString() };
                            invoice.DispatchOrder = new DispatchOrder() { DeliveryDocketNo = Convert.ToInt16(dr["delivery_docket_no"]), DispatchedDate = CheckNull<DateTime>(dr["dispatched_date"]) };

                            //if (Convert.ToInt16(dr["CustomerID"]) > 0)
                            //{
                            //    invoice.Customer = new Customer() { CompanyName = dr["company_name"].ToString(), ShipAddress = dr["ship_address"].ToString(), ShipCity = dr["ship_city"].ToString(), ShipState = dr["ship_state"].ToString() };
                            //}
                            //else
                            //{
                            //    invoice.Customer = new Customer() { CompanyName = dr["prepaid_customer_name"].ToString() };
                            //}

                            invoice.InvoicedDate = Convert.ToDateTime(dr["invoiced_date"]);
                            //invoice.ExportToMyOb = Convert.ToInt16(dr["ExportToMyOb"]) == 1 ? true : false;
                            invoice.ExportedToMyOb = Convert.ToBoolean(dr["exported_to_myob"]);
                            invoice.ExportedToMyObStr = invoice.ExportedToMyOb == true ? "Yes" : "No";
                            invoice.CompletedDate = CheckNull<DateTime>(dr["completed_date"]);
                            invoice.CompletedBy = dr["completed_by"].ToString();
                            invoice.IsCompleted = Convert.ToBoolean(dr["is_completed"]);
                            invoice.IsActive = Convert.ToBoolean(dr["is_active"]);
                            invoice.SendToMyObEnabled = (Convert.ToBoolean(dr["exported_to_myob"]) == true || dr["order_status"].ToString() == OrderStatus.Return.ToString()) ? false : true;
                            invoice.OrderStatus = dr["order_status"].ToString() == "Return" ? "Returned" : dr["order_status"].ToString();
                            invoice.StatusBackgroundCol = dr["order_status"].ToString() == OrderStatus.Dispatched.ToString() ? "#084FAA" : "#cc0000";
                            invoice.StatusForeGroundCol = "White";
                            invoice.PrintInvoiceActive = dr["order_status"].ToString() == OrderStatus.Return.ToString() ? false : true;
                            invoice.PrintInvoiceBackGroundColour = dr["order_status"].ToString() == OrderStatus.Return.ToString() ? "#a6a6a6" : "#666666";
                            invoice.PaymentRecieved = Convert.ToBoolean(dr["payment_recieved"]);
                            invoice.PaymentFinalisedBackGround = invoice.PaymentRecieved == true ? "#084FAA" : "#E12222";
                            invoice.PaymentFinalisedForeGround = invoice.PaymentRecieved == true ? "White" : "White";
                            invoicingList.Add(invoice);


                            //string companyName = string.Empty;
                            //if (Convert.ToInt16(dr["customer_id"]) == 0)
                            //{
                            //    cmdGePrepaidCustomer.Parameters.AddWithValue("@SalesNo", Convert.ToInt32(dr["sales_no"]));
                            //    using (SqlDataReader dr2 = cmdGePrepaidCustomer.ExecuteReader())
                            //    {
                            //        if (dr2 != null)
                            //        {
                            //            while (dr2.Read())
                            //            {
                            //                companyName = dr2["prepaid_customer_name"].ToString();
                            //            }
                            //        }
                            //    }
                            //    cmdGePrepaidCustomer.Parameters.Clear();
                            //}
                            //else
                            //{
                            //    cmdGeCustomer.Parameters.AddWithValue("@CustomerID", Convert.ToInt16(dr["customer_id"]));
                            //    using (SqlDataReader dr1 = cmdGeCustomer.ExecuteReader())
                            //    {
                            //        if (dr1 != null)
                            //        {
                            //            while (dr1.Read())
                            //            {
                            //                companyName = dr1["company_name"].ToString();
                            //            }
                            //        }
                            //    }
                            //    cmdGeCustomer.Parameters.Clear();
                            //}

                            //DispatchOrder dispatchOrder = new DispatchOrder();
                            //dispatchOrder.DispatchOrderID = Convert.ToInt32(dr["DispatchID"]);
                            //dispatchOrder.SalesOrderNo = Convert.ToInt32(dr["sales_no"]);
                            //dispatchOrder.CustomerOrderNo = dr["customer_order_no"].ToString();
                            //dispatchOrder.DeliveryDocketNo = Convert.ToInt32(dr["delivery_docket_no"]);
                            //dispatchOrder.ConNoteNumber = dr["con_note_number"].ToString();
                            //dispatchOrder.OrderDispatched = Convert.ToBoolean(dr["order_dispatched"]);
                            //dispatchOrder.DispatchedDate = CheckNull<DateTime>(dr["dispatched_date"]);
                            //dispatchOrder.DispatchOrderStatus = dr["order_status"].ToString();
                            //dispatchOrder.OrderStatus = dr["SalesOrderStatus"].ToString();
                            //dispatchOrder.IsProcessing = Convert.ToBoolean(dr["is_processing"]);
                            //dispatchOrder.DispatchedBy = CheckNull<string>(dr["completed_by"]);
                            //dispatchOrder.CompletedDateTime = CheckNull<DateTime>(dr["completed_date_time"]);
                            //dispatchOrder.IsActive = Convert.ToBoolean(dr["is_active"]);
                            //dispatchOrder.DispatchTimeStamp = dr["DisTimeStampo"].ToString();
                            //dispatchOrder.DesiredDispatchDate = Convert.ToDateTime(dr["DesiredShipDate"]);
                            //dispatchOrder.ShipTo = dr["ship_to"].ToString();
                            //dispatchOrder.BillTo = dr["bill_to"].ToString();
                            //dispatchOrder.Customer = new Customer() { CustomerId = Convert.ToInt16(dr["customer_id"]), CompanyName = companyName };
                            //dispatchOrder.FreightCarrier = new FreightCarrier() { FreightDescription = dr["FreightDescription"].ToString() };
                            //psList.Add(dispatchOrder);
                        }
                    }
                }


            }
            catch (Exception e)
            {

                Debug.WriteLine("Error reading dispatch details: " + e);
            }
            return invoicingList;
        }

        void dependency_OnChange(object sender, SqlNotificationEventArgs e)
        {
            // DBAccess.RescheduleOrdersByDate(Convert.ToDateTime("21/08/2015"));

            SqlDependency dependency = sender as SqlDependency;

            dependency.OnChange -= new OnChangeEventHandler(dependency_OnChange);

            this.OnNewMessage(e);
        }

        public static T CheckNull<T>(object obj)
        {
            return (obj == DBNull.Value ? default(T) : (T)obj);

        }

        #region IDisposable Members

        public void Dispose()
        {
            SqlDependency.Stop(DBConfiguration.DbConnectionString);
        }

        #endregion

    }
}


