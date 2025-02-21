using A1RConsole.Models.Customers;
using A1RConsole.Models.Dispatch;
using A1RConsole.Models.Freights;
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
    public class DispatchOrderNotifier : IDisposable
    {
        private event EventHandler<SqlNotificationEventArgs> _newMessage;
        private SqlConnection connection;
        public SqlCommand CurrentCommand { get; set; }

        public DispatchOrderNotifier()
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

        public ObservableCollection<DispatchOrder> RegisterDependency(string type,DateTime from, DateTime to)
        {
            string where_clause = string.Empty;
            DateTime days2 = DateTime.Now.AddDays(-2);
           
            ObservableCollection<DispatchOrder> psList = new ObservableCollection<DispatchOrder>();
            
            switch (type)
            {
                case "ReadyToDispatchChecked": where_clause = "dbo.DispatchOrders.order_dispatched = 'False' AND dbo.DispatchOrders.order_status ='Preparing'";
                    break;
                case "LastTwoDays": where_clause = "dbo.DispatchOrders.order_dispatched = 'True' AND DispatchOrders.dispatched_date >= @Days2 AND dbo.DispatchOrders.order_status ='Finalised'";
                    break;
                case "DateRange":
                    //where_clause = "dbo.DispatchOrders.order_dispatched = 'True' AND DispatchOrders.dispatched_date >= @FromDate AND DispatchOrders.dispatched_date <= @ToDate AND dbo.DispatchOrders.order_status ='Finalised'";
                    where_clause = "dbo.DispatchOrders.order_dispatched = 'True' AND DispatchOrders.dispatched_date BETWEEN @FromDate AND @ToDate AND dbo.DispatchOrders.order_status ='Finalised'";

                    break;
                default:
                    break;
            }

            this.CurrentCommand = new SqlCommand("SELECT DispatchOrders.id as DispatchID,DispatchOrders.sales_no,DispatchOrders.delivery_docket_no,DispatchOrders.con_note_number,DispatchOrders.order_dispatched,DispatchOrders.dispatched_date,DispatchOrders.order_status,DispatchOrders.is_processing,DispatchOrders.completed_by,DispatchOrders.completed_date_time,DispatchOrders.is_active,DispatchOrders.time_stamp AS DisTimeStampo, " +
                                                 "SalesOrder.dispatch_date AS DesiredShipDate,SalesOrder.bill_to,SalesOrder.ship_to,SalesOrder.customer_id,SalesOrder.order_status AS SalesOrderStatus,SalesOrder.customer_order_no, " +
	                                             "Freight.FreightDescription " +
                                                 "FROM dbo.DispatchOrders  " +
                                                 "INNER JOIN dbo.SalesOrder On DispatchOrders.sales_no = SalesOrder.sales_no " +
                                                 "INNER JOIN dbo.Freight ON SalesOrder.freight_id = Freight.ID " +
                                                 "WHERE " + where_clause, this.CurrentConnection);

            SqlCommand cmdGeCustomer = new SqlCommand("SELECT company_name FROM Customers " +
                                                      "WHERE id = @CustomerID", CurrentConnection);

            SqlCommand cmdGePrepaidCustomer = new SqlCommand("SELECT prepaid_customer_name FROM PrePaidCustomers " +
                                                             "WHERE sales_no = @SalesNo", CurrentConnection);


            this.CurrentCommand.Notification = null;


            SqlDependency dependency = new SqlDependency(this.CurrentCommand);
            dependency.OnChange += this.dependency_OnChange;

            if (this.CurrentConnection.State == ConnectionState.Closed)
                this.CurrentConnection.Open();
            try
            {

                CurrentCommand.Parameters.AddWithValue("@Days2", days2);
                CurrentCommand.Parameters.AddWithValue("@FromDate", from.Date);
                CurrentCommand.Parameters.AddWithValue("@ToDate", to.Date);
                using (SqlDataReader dr = this.CurrentCommand.ExecuteReader(CommandBehavior.CloseConnection))
                {
                    if (dr != null)
                    {
                        while (dr.Read())
                        {
                            string companyName=string.Empty;
                            if(Convert.ToInt16(dr["customer_id"]) == 0)
                            {
                                cmdGePrepaidCustomer.Parameters.AddWithValue("@SalesNo", Convert.ToInt32(dr["sales_no"]));
                                using (SqlDataReader dr2 = cmdGePrepaidCustomer.ExecuteReader())
                                {
                                    if (dr2 != null)
                                    {
                                        while (dr2.Read())
                                        {
                                            companyName = dr2["prepaid_customer_name"].ToString();
                                        }
                                    }
                                }
                                cmdGePrepaidCustomer.Parameters.Clear();
                            }                     
                            else
                            {
                                cmdGeCustomer.Parameters.AddWithValue("@CustomerID", Convert.ToInt16(dr["customer_id"]));
                                using (SqlDataReader dr1 = cmdGeCustomer.ExecuteReader())
                                {
                                    if (dr1 != null)
                                    {
                                        while (dr1.Read())
                                        {
                                            companyName = dr1["company_name"].ToString();
                                        }
                                    }
                                }
                                cmdGeCustomer.Parameters.Clear();
                            }

                            DispatchOrder dispatchOrder = new DispatchOrder();
                            dispatchOrder.DispatchOrderID = Convert.ToInt32(dr["DispatchID"]);
                            dispatchOrder.SalesOrderNo = Convert.ToInt32(dr["sales_no"]);
                            dispatchOrder.CustomerOrderNo = dr["customer_order_no"].ToString();
                            dispatchOrder.DeliveryDocketNo = Convert.ToInt32(dr["delivery_docket_no"]);
                            dispatchOrder.ConNoteNumber = dr["con_note_number"].ToString();
                            dispatchOrder.OrderDispatched = Convert.ToBoolean(dr["order_dispatched"]);
                            dispatchOrder.DispatchedDate = CheckNull<DateTime>(dr["dispatched_date"]);
                            dispatchOrder.DispatchOrderStatus = dr["order_status"].ToString();
                            dispatchOrder.OrderStatus = dr["SalesOrderStatus"].ToString();
                            dispatchOrder.IsProcessing = Convert.ToBoolean(dr["is_processing"]);
                            dispatchOrder.DispatchedBy = CheckNull<string>(dr["completed_by"]);
                            dispatchOrder.CompletedDateTime = CheckNull<DateTime>(dr["completed_date_time"]);
                            dispatchOrder.IsActive = Convert.ToBoolean(dr["is_active"]);
                            dispatchOrder.DispatchTimeStamp = dr["DisTimeStampo"].ToString();
                            dispatchOrder.DesiredDispatchDate = Convert.ToDateTime(dr["DesiredShipDate"]);
                            dispatchOrder.ShipTo = dr["ship_to"].ToString();
                            dispatchOrder.BillTo = dr["bill_to"].ToString();
                            dispatchOrder.Customer = new Customer() { CustomerId = Convert.ToInt16(dr["customer_id"]), CompanyName = companyName };
                            dispatchOrder.FreightCarrier = new FreightCarrier() { FreightDescription = dr["FreightDescription"].ToString() };
                            psList.Add(dispatchOrder);
                        }
                    }
                }

                
            }
            catch (Exception e)
            {

                Debug.WriteLine("Error reading dispatch details: " + e);
            }
            return psList;
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

