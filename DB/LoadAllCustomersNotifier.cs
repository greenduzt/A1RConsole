using A1RConsole.Models.Categories;
using A1RConsole.Models.Customers;
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
    public class LoadAllCustomersNotifier : IDisposable
    {
        private event EventHandler<SqlNotificationEventArgs> _newMessage;
        private SqlConnection connection;
        public SqlCommand CurrentCommand { get; set; }

        public LoadAllCustomersNotifier()
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


        public ObservableCollection<Customer> RegisterDependency()
        {
            ObservableCollection<Customer> psList = new ObservableCollection<Customer>();
            
            this.CurrentCommand = new SqlCommand("SELECT Customers.id,Customers.customer_type,Customers.company_name, " +
                                                 "Customers.company_address,Customers.company_city,Customers.company_state,Customers.company_postcode,Customers.company_country,Customers.company_email,Customers.company_telephone,Customers.company_fax,  " +
                                                 "Customers.designation1,Customers.first_name1,Customers.last_name1,Customers.telephone1,Customers.mobile1,Customers.fax1,Customers.email1,  " +
                                                 "Customers.designation2,Customers.first_name2,Customers.last_name2,Customers.telephone2,Customers.mobile2,Customers.fax2,Customers.email2,  " +
                                                 "Customers.designation3,Customers.first_name3,Customers.last_name3,Customers.telephone3,Customers.mobile3,Customers.fax3,Customers.email3,  " +
                                                 "Customers.ship_address,Customers.ship_city,Customers.ship_state,Customers.ship_postcode,Customers.ship_country,Customers.credit_limit,Customers.credit_remaining,Customers.debt,Customers.credit_owed,Customers.last_updated_by,Customers.last_updated_datetime,Customers.active,Customers.time_stamp,Customers.stop_credit,  " +
                                                 "Customers.primary_business,Category.category_name " +
                                                 "FROM dbo.Customers  " +
                                                 "INNER JOIN dbo.Category ON Customers.primary_business = Category.id  " +
                                                 "WHERE dbo.Customers.primary_business IS NOT NULL  " +
                                                 "ORDER BY dbo.Customers.company_name", this.CurrentConnection);
                       

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
                            Customer customer = new Customer();
                            customer.CustomerId = Convert.ToInt16(dr["id"]);
                            customer.CustomerType = dr["customer_type"].ToString();
                            customer.CompanyName = dr["company_name"].ToString();
                            customer.CompanyAddress = dr["company_address"].ToString();
                            customer.CompanyCity = dr["company_city"].ToString();
                            customer.CompanyState = dr["company_state"].ToString();
                            customer.CompanyPostCode = dr["company_postcode"].ToString();
                            customer.CompanyCountry = dr["company_country"].ToString();
                            customer.CompanyEmail = dr["company_email"].ToString();
                            customer.CompanyTelephone = dr["company_telephone"].ToString();
                            customer.CompanyFax = dr["company_fax"].ToString();
                            customer.Designation1 = dr["designation1"].ToString();
                            customer.FirstName1 = dr["first_name1"].ToString();
                            customer.LastName1 = dr["last_name1"].ToString();
                            customer.Telephone1 = dr["telephone1"].ToString();
                            customer.Mobile1 = dr["mobile1"].ToString();
                            customer.Email1 = dr["email1"].ToString();
                            customer.Fax1 = dr["fax1"].ToString();
                            customer.Designation2 = dr["designation2"].ToString();
                            customer.FirstName2 = dr["first_name2"].ToString();
                            customer.LastName2 = dr["last_name2"].ToString();
                            customer.Telephone2 = dr["telephone2"].ToString();
                            customer.Mobile2 = dr["mobile2"].ToString();
                            customer.Email2 = dr["email2"].ToString();
                            customer.Fax2 = dr["fax2"].ToString();
                            customer.Designation3 = dr["designation3"].ToString();
                            customer.FirstName3 = dr["first_name3"].ToString();
                            customer.LastName3 = dr["last_name3"].ToString();
                            customer.Telephone3 = dr["telephone3"].ToString();
                            customer.Mobile3 = dr["mobile3"].ToString();
                            customer.Email3 = dr["email3"].ToString();
                            customer.Fax3 = dr["fax3"].ToString();
                            customer.CreditLimit = CheckNull<decimal>(dr["credit_limit"]);
                            customer.CreditRemaining = CheckNull<decimal>(dr["credit_remaining"]);
                            customer.Debt = CheckNull<decimal>(dr["debt"]);
                            customer.CreditOwed = CheckNull<decimal>(dr["credit_owed"]);
                            //customer.CustomerCreditHistory = new CustomerCreditHistory() { CreditRemaining = Convert.ToDecimal(dr["credit_remaining"]) };
                            customer.ShipAddress = dr["ship_address"].ToString();
                            customer.ShipCity = dr["ship_city"].ToString();
                            customer.ShipState = dr["ship_state"].ToString();
                            customer.ShipPostCode = dr["ship_postcode"].ToString();
                            customer.ShipCountry = dr["ship_country"].ToString();
                            customer.LastUpdatedBy = dr["last_updated_by"].ToString();
                            customer.LastUpdatedDateTime = CheckNull<DateTime>(dr["last_updated_datetime"]);
                            customer.Active = Convert.ToBoolean(dr["active"]);
                            customer.StopCredit = dr["stop_credit"].ToString();
                            customer.TimeStamp = Convert.ToBase64String(dr["time_stamp"] as byte[]);
                            customer.PrimaryBusiness = new Category();
                            customer.PrimaryBusiness.CategoryID = Convert.ToInt16(dr["primary_business"]);
                            customer.PrimaryBusiness.CategoryName = dr["category_name"].ToString();

                            psList.Add(customer);
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


