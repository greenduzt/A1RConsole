using A1RConsole.Models.Customers;
using A1RConsole.Models.Freights;
using A1RConsole.Models.Orders;
using A1RConsole.Models.Products;
using A1RConsole.Models.Stock;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A1RConsole.DB
{
    public class CustomerCreditSalesNotifier : IDisposable
    {

        private event EventHandler<SqlNotificationEventArgs> _newMessage;
        private SqlConnection connection;
        public SqlCommand CurrentCommand { get; set; }

        public CustomerCreditSalesNotifier()
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


        public ObservableCollection<SalesOrder> RegisterDependency()
        {
            ObservableCollection<SalesOrder> soList = new ObservableCollection<SalesOrder>();
            List<CustomerCreditHistory> customerCreditHistory = new List<CustomerCreditHistory>();

            this.CurrentCommand = new SqlCommand("SELECT SalesOrder.sales_no,SalesOrder.customer_id,SalesOrder.dispatch_date, SalesOrder.stock_location, SalesOrder.freight_id, ProductStock.product_id,Products.product_code ,SalesOrderDetails.qty,ProductStock.qty_available,ProductStock.time_stamp,Freight.FreightName,ProductStockReserved.qty_reserved, SalesOrder.terms_id, SalesOrder.total_amount, " +
                                                 "Customers.credit_remaining,Customers.credit_limit,Customers.credit_owed " +
                                                 "FROM dbo.ProductStock " +
                                                 "INNER JOIN dbo.SalesOrderDetails ON ProductStock.product_id = SalesOrderDetails.product_id " +
                                                 "INNER JOIN dbo.SalesOrder ON SalesOrderDetails.sales_no = SalesOrder.sales_no " +
                                                 "INNER JOIN dbo.Products ON SalesOrderDetails.product_id = Products.id " +
                                                 "INNER JOIN dbo.Freight ON SalesOrder.freight_id = Freight.ID " +
                                                 "INNER JOIN dbo.ProductStockReserved ON SalesOrderDetails.id = ProductStockReserved.prod_stock_reserved_id " +
												 "INNER JOIN dbo.Customers ON SalesOrder.customer_id = Customers.id " +
                                                 "WHERE dbo.ProductStock.stock_location_id=1 AND SalesOrder.terms_id = '30EOM' AND (dbo.SalesOrder.order_status ='HoldNoStock' OR dbo.SalesOrder.order_status ='HoldNoCredit' OR dbo.SalesOrder.order_status='HoldNoCreditNoStock') " +
                                                 "ORDER BY dbo.ProductStock.product_id", this.CurrentConnection);



            this.CurrentCommand.Notification = null;


            SqlDependency dependency = new SqlDependency(this.CurrentCommand);
            dependency.OnChange += this.dependency_OnChange;

            if (this.CurrentConnection.State == ConnectionState.Closed)
                this.CurrentConnection.Open();
            try
            {
                using (SqlDataReader dr = this.CurrentCommand.ExecuteReader(CommandBehavior.CloseConnection))
                {
                    if (dr != null)
                    {
                        while (dr.Read())
                        {
                            if (soList.Count == 0)
                            {
                                SalesOrder so = new SalesOrder();
                                so.SalesOrderNo = Convert.ToInt32(dr["sales_no"]);
                                so.DesiredDispatchDate = Convert.ToDateTime(dr["dispatch_date"]);
                                so.FreightCarrier = new FreightCarrier() { Id = Convert.ToInt16(dr["freight_id"]), FreightName = dr["FreightName"].ToString() };
                                so.StockLocation = new StockLocation() { ID = Convert.ToInt16(dr["stock_location"]) };
                                so.Customer = new Customer() { CustomerId = Convert.ToInt16(dr["customer_id"]), CreditLimit = Convert.ToDecimal(dr["credit_limit"]), CreditRemaining = Convert.ToDecimal(dr["credit_remaining"]) };
                                so.TotalAmount = Convert.ToDecimal(dr["total_amount"]);
                                so.TermsID = dr["terms_id"].ToString();
                                so.SalesOrderDetails = new ObservableCollection<SalesOrderDetails>();
                                so.SalesOrderDetails.Add(new SalesOrderDetails()
                                {
                                    Product = new Product()
                                    {
                                        ProductID = Convert.ToInt16(dr["product_id"]),
                                        ProductCode = dr["product_code"].ToString()
                                    },
                                    Quantity = Convert.ToDecimal(dr["qty"]),
                                    QtyInStock = Convert.ToDecimal(dr["qty_available"]),
                                    QtyToMake = Convert.ToDecimal(dr["qty_reserved"])//This is qty reserved
                                });
                                soList.Add(so);
                            }
                            else
                            {
                                var has = soList.FirstOrDefault(x => x.SalesOrderNo == Convert.ToInt32(dr["sales_no"]));
                                if (has != null)
                                {
                                    has.SalesOrderDetails.Add(new SalesOrderDetails()
                                    {
                                        Product = new Product()
                                        {
                                            ProductID = Convert.ToInt16(dr["product_id"]),
                                            ProductCode = dr["product_code"].ToString()
                                        },
                                        Quantity = Convert.ToDecimal(dr["qty"]),
                                        QtyInStock = Convert.ToDecimal(dr["qty_available"]),
                                        QtyToMake = Convert.ToDecimal(dr["qty_reserved"])//This is qty reserved
                                    });
                                }
                                else
                                {
                                    SalesOrder so = new SalesOrder();
                                    so.SalesOrderNo = Convert.ToInt32(dr["sales_no"]);
                                    so.DesiredDispatchDate = Convert.ToDateTime(dr["dispatch_date"]);
                                    so.FreightCarrier = new FreightCarrier() { Id = Convert.ToInt16(dr["freight_id"]), FreightName = dr["FreightName"].ToString() };
                                    so.StockLocation = new StockLocation() { ID = Convert.ToInt16(dr["stock_location"]) };
                                    so.Customer = new Customer() { CustomerId = Convert.ToInt16(dr["customer_id"]), CreditLimit = Convert.ToDecimal(dr["credit_limit"]), CreditRemaining = Convert.ToDecimal(dr["credit_remaining"]) };
                                    so.TotalAmount = Convert.ToDecimal(dr["total_amount"]);
                                    so.TermsID = dr["terms_id"].ToString();
                                    so.SalesOrderDetails = new ObservableCollection<SalesOrderDetails>();
                                    so.SalesOrderDetails.Add(new SalesOrderDetails()
                                    {
                                        Product = new Product()
                                        {
                                            ProductID = Convert.ToInt16(dr["product_id"]),
                                            ProductCode = dr["product_code"].ToString()
                                        },
                                        Quantity = Convert.ToDecimal(dr["qty"]),
                                        QtyInStock = Convert.ToDecimal(dr["qty_available"]),
                                        QtyToMake = Convert.ToDecimal(dr["qty_reserved"])//This is qty reserved
                                    });
                                    soList.Add(so);
                                }
                            }
                        }
                    }
                }
                return soList;
            }
            catch { return null; }
        }
        void dependency_OnChange(object sender, SqlNotificationEventArgs e)
        {
            // DBAccess.RescheduleOrdersByDate(Convert.ToDateTime("21/08/2015"));

            SqlDependency dependency = sender as SqlDependency;

            dependency.OnChange -= new OnChangeEventHandler(dependency_OnChange);

            this.OnNewMessage(e);
        }

        #region IDisposable Members

        public void Dispose()
        {
            SqlDependency.Stop(DBConfiguration.DbConnectionString);
        }

        #endregion
    }
}

