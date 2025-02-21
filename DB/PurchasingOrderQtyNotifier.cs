using A1RConsole.Models.Products;
using A1RConsole.Models.Purchasing;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A1RConsole.DB
{
    public class PurchasingOrderQtyNotifier : IDisposable
    {
        private event EventHandler<SqlNotificationEventArgs> _newMessage;
        private SqlConnection connection;
        public SqlCommand CurrentCommand { get; set; }

        public PurchasingOrderQtyNotifier()
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

        public List<PurchaseOrderDetails> RegisterDependency()
        {
            List<PurchaseOrderDetails> psList = new List<PurchaseOrderDetails>();

            this.CurrentCommand = new SqlCommand("SELECT PurchasingOrders.supplier_id,PurchasingOrderItems.product_id,PurchasingOrderItems.order_qty,ISNULL(PurchasingOrderItems.received_qty,0) AS received_qty " +
                                                 "FROM dbo.PurchasingOrders " +
                                                 "INNER JOIN dbo.PurchasingOrderItems ON PurchasingOrders.purchasing_order_no=PurchasingOrderItems.purchase_order_no " +
                                                 "WHERE PurchasingOrderItems.line_status='Open' AND PurchasingOrders.status ='Pending'", this.CurrentConnection);

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
                            PurchaseOrderDetails p = new PurchaseOrderDetails();
                            if (psList.Count == 0)
                            {
                                p.Product = new Product() { ProductID = Convert.ToInt16(dr["product_id"]) };
                                p.OrderQty = Convert.ToDecimal(dr["order_qty"]);
                                p.RecievedQty = Convert.ToDecimal(dr["received_qty"]);                             
                                psList.Add(p);
                            }
                            else
                            {
                                var data = psList.SingleOrDefault(x => x.Product.ProductID == Convert.ToInt16(dr["product_id"]));
                                if (data != null)
                                {
                                    foreach (var item in psList)
                                    {
                                        if (item.Product.ProductID == Convert.ToInt16(dr["product_id"]))
                                        {
                                            item.OrderQty += Convert.ToDecimal(dr["order_qty"]);
                                            item.RecievedQty += Convert.ToDecimal(dr["received_qty"]);
                                        }
                                    }
                                }
                                else
                                {
                                    p.Product = new Product() { ProductID = Convert.ToInt16(dr["product_id"]) };
                                    p.OrderQty = Convert.ToDecimal(dr["order_qty"]);
                                    p.RecievedQty = Convert.ToDecimal(dr["received_qty"]);
                                    psList.Add(p);
                                }
                            }
                        }
                    }
                }

                return psList;
            }
            catch { return null; }

        }

        void dependency_OnChange(object sender, SqlNotificationEventArgs e)
        {
            SqlDependency dependency = sender as SqlDependency;
            dependency.OnChange -= new OnChangeEventHandler(dependency_OnChange);
            this.OnNewMessage(e);
        }

        #region IDisposable Members

        public void Dispose()
        {
            SqlDependency.Stop(DBConfiguration.DbConnectionString);
        }

        public static T CheckNull<T>(object obj)
        {
            return (obj == DBNull.Value ? default(T) : (T)obj);

        }

        #endregion
    }
}
