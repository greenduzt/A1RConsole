using A1RConsole.Models.Products;
using A1RConsole.Models.Stock;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A1RConsole.DB
{
    public class ProductStockReservedNotifier : IDisposable
    {
        private event EventHandler<SqlNotificationEventArgs> _newMessage;
        private SqlConnection connection;
        public SqlCommand CurrentCommand { get; set; }

        public ProductStockReservedNotifier()
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

        public List<ProductStock> RegisterDependency()
        {
            List<ProductStock> psList = new List<ProductStock>();

            this.CurrentCommand = new SqlCommand("SELECT ProductStockReserved.stock_location_id,ProductStockReserved.product_id,ProductStockReserved.qty_reserved AS QTY_RESERVED " +
                                                 "FROM dbo.ProductStockReserved " +                                                   
                                                 "WHERE dbo.ProductStockReserved.status='Reserved'", this.CurrentConnection);

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
                            ProductStock p = new ProductStock();
                            if (psList.Count == 0)
                            {
                                p.StockLocation = new StockLocation() { ID = Convert.ToInt16(dr["stock_location_id"]) };
                                p.Product = new Product() { ProductID = Convert.ToInt16(dr["product_id"]) };
                                p.QtyOnHold = Convert.ToDecimal(dr["QTY_RESERVED"]);
                                psList.Add(p);
                            }
                            else
                            {
                                var data = psList.SingleOrDefault(x => x.StockLocation.ID == Convert.ToInt16(dr["stock_location_id"]) && x.Product.ProductID == Convert.ToInt16(dr["product_id"]));
                                if (data != null)
                                {
                                    foreach (var item in psList)
                                    {
                                        if(item.StockLocation.ID == Convert.ToInt16(dr["stock_location_id"]) && item.Product.ProductID == Convert.ToInt16(dr["product_id"]))
                                        {
                                            item.QtyOnHold += Convert.ToDecimal(dr["QTY_RESERVED"]);
                                        }
                                    }                                 
                                }
                                else
                                {
                                    p.StockLocation = new StockLocation() { ID = Convert.ToInt16(dr["stock_location_id"]) };
                                    p.Product = new Product() { ProductID = Convert.ToInt16(dr["product_id"]) };
                                    p.QtyOnHold = Convert.ToDecimal(dr["QTY_RESERVED"]);
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
