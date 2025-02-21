using A1RConsole.Models.Orders.OnlineOrders;
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
    public class OnlineOrdersNotifier : IDisposable
    {
        private event EventHandler<SqlNotificationEventArgs> _newMessage;
        private SqlConnection connection;
        public SqlCommand CurrentCommand { get; set; }

        public OnlineOrdersNotifier()
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

        public ObservableCollection<OnlineOrder> RegisterDependency()
        {


            this.CurrentCommand = new SqlCommand("SELECT OnlineOrders.order_no,OnlineOrders.order_ref_no,OnlineOrders.user_id,OnlineOrders.user_nicename,OnlineOrders.user_email,OnlineOrders.display_name,OnlineOrders.user_business_name,OnlineOrders.job_name,OnlineOrders.order_placed_by,OnlineOrders.gst,OnlineOrders.sub_total,OnlineOrders.less_discount,OnlineOrders.total_amount,OnlineOrders.is_length_required,OnlineOrders.lengths,OnlineOrders.delivery_method,OnlineOrders.collect_option,OnlineOrders.collect_date,OnlineOrders.order_time_stamp " +
                                                 "FROM dbo.OnlineOrders", this.CurrentConnection);

            this.CurrentCommand.Notification = null;


            SqlDependency dependency = new SqlDependency(this.CurrentCommand);
            dependency.OnChange += this.dependency_OnChange;

            if (this.CurrentConnection.State == ConnectionState.Closed)
                this.CurrentConnection.Open();
            try
            {
                ObservableCollection<OnlineOrder> psList = new ObservableCollection<OnlineOrder>();
                using (SqlDataReader dr = this.CurrentCommand.ExecuteReader(CommandBehavior.CloseConnection))
                {
                    if (dr != null)
                    {
                        while (dr.Read())
                        {

                            OnlineOrder ps = new OnlineOrder();
                            ps.OnlineOrderNo = Convert.ToInt32(dr["order_no"]);
                            ps.OnlineOrderCustomer = new OnlineOrderCustomer() { CustomerId = Convert.ToInt16(dr["user_id"]), UserNiceName = dr["user_nicename"].ToString(), UserEmail = dr["user_email"].ToString(), DisplayName = dr["display_name"].ToString(), CompanyName = dr["user_business_name"].ToString() };
                            ps.OrderRefNo = dr["order_ref_no"].ToString();
                            ps.JobName = dr["job_name"].ToString();
                            ps.OrderPlacedBy = dr["order_placed_by"].ToString();
                            ps.TotalAmount = Convert.ToDouble(dr["total_amount"]);
                            ps.CollectOption = dr["collect_option"].ToString();
                            ps.CollectDate = dr["collect_date"].ToString();
                            ps.OrderTimeStamp = Convert.ToDateTime(dr["order_time_stamp"]);
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

        #endregion
    }
}
