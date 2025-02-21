using A1RConsole.DB;
using A1RConsole.Models.Customers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A1RConsole.Core
{
    public static class CreditManager
    {

        public static Tuple<Customer, List<CustomerCreditActivity>,string> AddCustomerCredit(Customer customer, decimal newCredit, decimal incrCredLim, decimal decCredLim)
        {
            string msgStr = string.Empty;
            //decimal creditRem = 0;
            decimal creditOwed = 0;
            bool checkCredit = true;
            List<CustomerCreditActivity>  ccaList = new List<CustomerCreditActivity>();

            //Get customer credit info
            Customer cus = DBAccess.GetCustomerCreditDetails(customer.CustomerId);
            if(cus != null)
            {
                
                /*CREDIT LIMIT*/

                //Adding Credit for the first time
                if (cus.CreditLimit == 0)
                {
                    cus.CreditLimit = newCredit;
                    ccaList.Add(new CustomerCreditActivity() { Customer = new Customer() { CustomerId = customer.CustomerId }, SalesOrderNo = 0, Amount = newCredit, Type = "Credit Limit Increased", Activity = "Credit limit increased by " + newCredit.ToString("C", CultureInfo.CurrentCulture), UpdatedDate = DateTime.Now, UpdatedBy = UserData.FirstName + " " + UserData.LastName });

                    cus.CreditRemaining = newCredit;
                    ccaList.Add(new CustomerCreditActivity() { Customer = new Customer() { CustomerId = customer.CustomerId }, SalesOrderNo = 0, Amount = newCredit, Type = "Credit Added", Activity = "Credit added : " + newCredit.ToString("C", CultureInfo.CurrentCulture) , UpdatedDate = DateTime.Now, UpdatedBy = UserData.FirstName + " " + UserData.LastName });
                
                }
                else
                {
                    if (incrCredLim > 0)
                    {
                        cus.CreditLimit += incrCredLim;
                        ccaList.Add(new CustomerCreditActivity() { Customer = new Customer() { CustomerId = customer.CustomerId }, SalesOrderNo = 0, Amount = incrCredLim, Type = "Credit Limit Increased", Activity = "Credit limit increased by " + incrCredLim.ToString("C", CultureInfo.CurrentCulture), UpdatedDate = DateTime.Now, UpdatedBy = UserData.FirstName + " " + UserData.LastName });

                        cus.CreditRemaining += (cus.CreditRemaining + incrCredLim) > cus.CreditLimit ? cus.CreditLimit : incrCredLim;
                        ccaList.Add(new CustomerCreditActivity() { Customer = new Customer() { CustomerId = customer.CustomerId }, SalesOrderNo = 0, Amount = newCredit, Type = "Credit Remaining Increased", Activity = "Credit remaining updated : " + newCredit.ToString("C", CultureInfo.CurrentCulture), UpdatedDate = DateTime.Now, UpdatedBy = UserData.FirstName + " " + UserData.LastName });

                    }

                    if (decCredLim > 0)
                    {
                        if (cus.CreditLimit >= decCredLim)
                        {
                            cus.CreditLimit -= decCredLim;
                            ccaList.Add(new CustomerCreditActivity() { Customer = new Customer() { CustomerId = customer.CustomerId }, SalesOrderNo = 0, Amount = decCredLim, Type = "Credit Limit Decreased", Activity = "Credit Limit decreased by " + decCredLim.ToString("C", CultureInfo.CurrentCulture) + " | New Credit Limit : " + cus.CreditLimit.ToString("C", CultureInfo.CurrentCulture), UpdatedDate = DateTime.Now, UpdatedBy = UserData.FirstName + " " + UserData.LastName });

                            //If the customer credit limit is less than or equal to credit remaining then credit remaining == credit limit
                            if(cus.CreditLimit <= cus.CreditRemaining)
                            {
                                cus.CreditRemaining = cus.CreditLimit;
                                ccaList.Add(new CustomerCreditActivity() { Customer = new Customer() { CustomerId = customer.CustomerId }, SalesOrderNo = 0, Amount = decCredLim, Type = "Credit Remaining Decreased", Activity = "Credit Remaining decreased by " + decCredLim.ToString("C", CultureInfo.CurrentCulture) + " | New Credit Remaining : " + cus.CreditRemaining.ToString("C", CultureInfo.CurrentCulture), UpdatedDate = DateTime.Now, UpdatedBy = UserData.FirstName + " " + UserData.LastName });
                            }
                        }
                        else
                        {
                             msgStr = "You are trying to decrease more credit than Credit Limit"
                                    + System.Environment.NewLine
                                    + "Credit Limit is " + cus.CreditLimit.ToString("C", CultureInfo.CurrentCulture);

                             checkCredit = false;
                        }                        
                    }

                    /*CREDIT*/
                    if (newCredit > 0 && checkCredit)
                    {
                        creditOwed = (cus.CreditOwed - newCredit) <= 0 ? 0 : cus.CreditOwed - newCredit;
                        //creditRem = (newCredit - cus.CreditOwed) <= 0 ? 0 : newCredit - cus.CreditOwed;

                        cus.CreditOwed = creditOwed;
                        cus.CreditRemaining += newCredit;

                        if ((cus.CreditRemaining + creditOwed) <= cus.CreditLimit)
                        {
                            //cus.CreditRemaining = creditRem;
                            ccaList.Add(new CustomerCreditActivity() { Customer = new Customer() { CustomerId = customer.CustomerId }, SalesOrderNo = 0, Amount = newCredit, Type = "Credit Added", Activity = "Credit added : " + newCredit.ToString("C", CultureInfo.CurrentCulture) + " | Credit Owed : " + cus.CreditOwed.ToString("C", CultureInfo.CurrentCulture) + " | Credit Remaining : " + cus.CreditRemaining.ToString("C", CultureInfo.CurrentCulture), UpdatedDate = DateTime.Now, UpdatedBy = UserData.FirstName + " " + UserData.LastName });
                        }
                        else
                        {
                            if (incrCredLim > 0)
                            {
                                msgStr = "Cannot add more credit than the Credit Limit (Credit Remaining is grater than the Credit Limit)"
                                        + System.Environment.NewLine
                                        + System.Environment.NewLine
                                        + "Credit Limit is " + cus.CreditLimit.ToString("C", CultureInfo.CurrentCulture)
                                        + System.Environment.NewLine
                                        + "Credit Remaining is " + cus.CreditRemaining.ToString("C", CultureInfo.CurrentCulture);
                            }
                            else if (decCredLim > 0)
                            {
                                msgStr = "Cannot decrease Credit Limit than the Credit Remaining and Credit Owe (Credit Remaining and Credit Owe is grater than the Credit Limit)"
                                        + System.Environment.NewLine
                                        + System.Environment.NewLine
                                        + "Credit Limit is " + cus.CreditLimit.ToString("C", CultureInfo.CurrentCulture)
                                        + System.Environment.NewLine
                                        + "Credit Owe is " + cus.CreditOwed.ToString("C", CultureInfo.CurrentCulture)
                                        + System.Environment.NewLine
                                        + "Credit Remaining is " + cus.CreditRemaining.ToString("C", CultureInfo.CurrentCulture);
                            }
                            else if (incrCredLim == 0 && decCredLim == 0)
                            {
                                msgStr = "Cannot add more credit than the Credit Limit (Credit Remaining is grater than the Credit Limit)"
                                        + System.Environment.NewLine
                                        + System.Environment.NewLine
                                        + "Credit Limit is " + cus.CreditLimit.ToString("C", CultureInfo.CurrentCulture)
                                        + System.Environment.NewLine
                                        + "Credit Remaining is " + cus.CreditRemaining.ToString("C", CultureInfo.CurrentCulture);
                            }
                        }
                    }
                }
               
            }
            else
            {
                msgStr = "Cannot retrieve credit information";
            }

            return Tuple.Create(cus, ccaList, msgStr);
        }

        public static Customer AddNewCreditDB(Customer cus, decimal newCredit, List<CustomerCreditActivity> ccaList)
        {
            cus.LastUpdatedBy = UserData.FirstName + " " + UserData.LastName;
            cus.LastUpdatedDateTime = DateTime.Now;
            Customer upCreditInfo  = DBAccess.UpdateCustomerCredit(cus, newCredit, ccaList);

            return upCreditInfo;
        }
        
    }
}
