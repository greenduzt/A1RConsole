using A1RConsole.Models.Categories;
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
    public class ProductStockNotifier : IDisposable
    {
        private event EventHandler<SqlNotificationEventArgs> _newMessage;
        private SqlConnection connection;
        public SqlCommand CurrentCommand { get; set; }

        public ProductStockNotifier()
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
            //this.CurrentCommand = new SqlCommand("SELECT Products.id AS ProductId,Category.id AS CategoryID,Category.category_name,Products.commodity_code,Products.product_code,Products.product_description,Products.unit,Products.material_cost,Products.units_per_package,Products.unit_cost,Products.unit_price,Products.is_manufactured,Products.is_purchased,Products.active,Products.last_modified_by,Products.last_modified_date,Products.minimum_order_qty,Products.orders_in_multiples_of,Products.safety_stock_qty,Products.order_point,Products.is_auto_order,Products.time_stamp AS ProdTimeStamp, " +
            //                                     "ProductStock.id AS ProductStockID, ProductStock.stock_location_id,ProductStock.qty_available,ProductStock.last_updated_date,ProductStock.updated_by,ProductStock.updated_by,ProductStock.time_stamp, " +
            //                                     "(SELECT ISNULL(SUM(ProductStockReserved.qty_reserved),0) AS QTY_RESERVED " +
            //                                     "FROM dbo.SalesOrder  " +
            //                                     "INNER JOIN dbo.ProductStockReserved ON SalesOrder.sales_no = ProductStockReserved.sales_no " +
            //                                     "WHERE ProductStockReserved.product_id = Products.id AND ProductStockReserved.stock_location_id=ProductStock.stock_location_id ) " +
            //                                     "FROM dbo.Products  " +
            //                                     "INNER JOIN dbo.Category ON Products.category_id = Category.id  " +
            //                                     "INNER JOIN dbo.ProductStock ON Products.id = ProductStock.product_id", this.CurrentConnection);

            this.CurrentCommand = new SqlCommand("SELECT Products.id AS ProductId,Category.id AS CategoryID,Category.category_name,Products.commodity_code,Products.product_code,Products.product_description,Products.unit,Products.material_cost,Products.units_per_package,Products.unit_cost,Products.unit_price,Products.is_manufactured,Products.is_purchased,Products.active,Products.last_modified_by,Products.last_modified_date,Products.minimum_order_qty,Products.orders_in_multiples_of,Products.safety_stock_qty,Products.order_point,Products.is_auto_order,Products.time_stamp AS ProdTimeStamp, " +
                                                 "ProductStock.id AS ProductStockID, ProductStock.stock_location_id,ProductStock.qty_available,ProductStock.last_updated_date,ProductStock.updated_by,ProductStock.updated_by,ProductStock.time_stamp " +
                                                 "FROM dbo.Products " +
                                                 "INNER JOIN dbo.Category ON Products.category_id = Category.id " +
                                                 "INNER JOIN dbo.ProductStock ON Products.id = ProductStock.product_id", this.CurrentConnection);

            this.CurrentCommand.Notification = null;


            SqlDependency dependency = new SqlDependency(this.CurrentCommand);
            dependency.OnChange += this.dependency_OnChange;

            if (this.CurrentConnection.State == ConnectionState.Closed)
                this.CurrentConnection.Open();
            try
            {
                List<ProductStock> psList = new List<ProductStock>();
                using (SqlDataReader dr = this.CurrentCommand.ExecuteReader(CommandBehavior.CloseConnection))
                {
                    if (dr != null)
                    {
                        while (dr.Read())
                        {

                            ProductStock ps = new ProductStock();
                            ps.Product = new Product()
                            {
                                ProductID = Convert.ToInt16(dr["ProductId"]),
                                ProductCode = dr["product_code"].ToString(),
                                ProductDescription = dr["product_description"].ToString(),
                                ProductUnit = dr["unit"].ToString(),
                                MaterialCost = Convert.ToDecimal(dr["material_cost"]),
                                UnitsPerPack = Convert.ToInt16(dr["units_per_package"]),
                                UnitCost = Convert.ToDecimal(dr["unit_cost"]),
                                UnitPrice = Convert.ToDecimal(dr["unit_price"]),
                                IsManufactured = Convert.ToBoolean(dr["is_manufactured"]),
                                IsPurchased = Convert.ToBoolean(dr["is_purchased"]),
                                Active = Convert.ToBoolean(dr["active"]),
                                IsAutoOrder = Convert.ToBoolean(dr["is_auto_order"]),
                                LastModifiedBy = CheckNull<DateTime>(dr["last_updated_date"]) >= CheckNull<DateTime>(dr["last_modified_date"]) ? dr["updated_by"].ToString() + " " + CheckNull<DateTime>(dr["last_updated_date"]) : dr["last_modified_by"].ToString() + " " + CheckNull<DateTime>(dr["last_modified_date"]),
                                LastModifiedDate = Convert.ToDateTime(dr["last_modified_date"]),
                                MinimumOrderQty = Convert.ToInt16(dr["minimum_order_qty"]),
                                OrderInMultiplesOf = Convert.ToInt16(dr["orders_in_multiples_of"]),
                                SafetyStockQty = Convert.ToInt32(dr["safety_stock_qty"]),
                                OrderPoint = Convert.ToInt32(dr["order_point"]),
                                Category = new Category() { CategoryID = Convert.ToInt16(dr["CategoryID"]), CategoryName = dr["category_name"].ToString() },
                                CommodityCode = dr["commodity_code"].ToString(),
                                TimeStamp = Convert.ToBase64String(dr["ProdTimeStamp"] as byte[])
                            };
                            ps.ID = Convert.ToInt16(dr["ProductStockID"]);
                            ps.QtyAvailable = Convert.ToDecimal(dr["qty_available"]);
                            //ps.LastUpdatedDate = CheckNull<DateTime>(dr["last_updated_date"]);
                            //ps.UpdatedBy = dr["updated_by"].ToString();
                            //ps.TotalSupply = Convert.ToDecimal(dr["total_supply"]);
                            //ps.ProjectedAvailable = Convert.ToDecimal(dr["qty_available"]) + Convert.ToDecimal(dr["TotalSupply"]);
                            //ps.QtyOnHold = Convert.ToDecimal(dr["QTY_RESERVED"]);
                            ps.StockLocation = new StockLocation() { ID = Convert.ToInt16(dr["stock_location_id"]) };
                            ps.TimeStamp = Convert.ToBase64String(dr["time_stamp"] as byte[]);
                            //ps.NetDemand = Convert.ToDecimal(dr["net_demand"]);
                            psList.Add(ps);
                        }
                    }
                }

                return psList;
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

        public static T CheckNull<T>(object obj)
        {
            return (obj == DBNull.Value ? default(T) : (T)obj);

        }

        #endregion
    }
}
