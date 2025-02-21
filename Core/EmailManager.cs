using A1RConsole.DB;
using A1RConsole.Models.Meta;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using A1RConsole.Models.Customers;
using A1RConsole.Models.Discounts;
using Org.BouncyCastle.Asn1.Ocsp;

namespace A1RConsole.Core
{
    public static class EmailManager
    {
        public static bool SendEmailAlertDiscountChange(int customerID, List<DiscountStructure> discountList)
        {
            bool isEmailSent = false;
            
            List<MetaData> metaDataList = DBAccess.GetMetaData();
            if(metaDataList != null)
            {
                var data = metaDataList.FirstOrDefault(x=>x.KeyName.Equals("send_email_discount_change"));
                if(data != null)
                {
                    Exception exceptionEmail = null;
                    string[] emails = data.Description.Split('|');
                    if(emails.Length > 0)
                    {
                        try
                        {
                            //Get customer name from customer id
                            Customer customer = DBAccess.GetCustomerNameByID(customerID);

                            if (customer != null && customer.CustomerId > 0 && discountList != null && discountList.Count > 0)
                            {
                                string discountStr = string.Empty;
                                MailAddress mailfrom = new MailAddress("a1rubber.orders@gmail.com");
                                MailMessage msg = new MailMessage();
                                foreach (var item in emails)
                                {
                                    msg.To.Add(item);
                                }

                                foreach (var item in discountList)
                                {
                                    discountStr +=   item.Category.CategoryName + "   " + item.Discount + "%" + System.Environment.NewLine;
                                }

                                msg.From = mailfrom;
                                msg.Subject = string.Format("Customer Discount Changed {0}", DateTime.Now.ToLongDateString());
                                msg.Body = "Hi" + System.Environment.NewLine +
                                                                                               System.Environment.NewLine +
                                           "Customer Name: " + customer.CompanyName          + System.Environment.NewLine +
                                                                                               System.Environment.NewLine +
                                                                                               System.Environment.NewLine +
                                           "New Discount Structure" + System.Environment.NewLine +
                                                                                               System.Environment.NewLine +
                                            discountStr + System.Environment.NewLine +
                                                                                               System.Environment.NewLine +
                                                                                               System.Environment.NewLine +
                                                                                               System.Environment.NewLine +
                                           "A1R Console";



                                SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587);
                                smtp.UseDefaultCredentials = false;
                                smtp.Credentials = new NetworkCredential("a1rubber.orders@gmail.com", "bgdobybwyownoeou");
                                smtp.EnableSsl = true;
                                smtp.Send(msg);
                            }
                        }
                        catch (Exception ex)
                        {
                            exceptionEmail = ex;
                        }
                    }
                }
            }

            return isEmailSent;
        }
    }
}
