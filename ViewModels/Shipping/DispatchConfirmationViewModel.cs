using A1RConsole.Bases;
using A1RConsole.Commands;
using A1RConsole.Core;
using A1RConsole.DB;
using A1RConsole.Models.Dispatch;
using A1RConsole.PdfGeneration;
using A1RConsole.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace A1RConsole.ViewModels.Shipping
{
    public class DispatchConfirmationViewModel : ViewModelBase
    {
        //private A1QSystem.PageSwitcher PageSwitcher;
        List<Tuple<string, Int16, string>> timeStamps;
        private DispatchOrder DispatchOrder;
        private Microsoft.Practices.Prism.Commands.DelegateCommand _closeCommand;
        public event Action Closed;
        private bool _isPrintDeliveryDocket;
        private bool _sendConfirmation;
        private string _customerEmailVisibility;
        private bool _customerEmailEnabled;

        private ICommand _okCommand;

        public DispatchConfirmationViewModel(DispatchOrder diso, List<Tuple<string, Int16, string>> ts)
        {
            DispatchOrder = diso;
            timeStamps = ts;
            IsPrintDeliveryDocket = true;
            FindCustomerEmail();
            _closeCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(CloseForm);
        }

        private void CloseForm()
        {
            Closed();
        }

        private void SubmitData()
        {
            CloseForm();
            int res = 0;
            Exception ex = null;
            DispatchOrder.OrderStatus = DispatchOrderStatus.Finalised.ToString();
            DispatchOrder.DispatchedDate = DateTime.Now.Date;
            DispatchOrder.CompletedDateTime = DateTime.Now;
            DispatchOrder.IsActive = false;
            res = DBAccess.DispatchOrder(DispatchOrder, timeStamps);
            if (res > 0)
            {
                if (IsPrintDeliveryDocket)
                {
                    ex = Print();
                }

                if (SendConfirmation && ex == null)
                {
                    SendEmail();
                }

            }
            else if (res == -1)
            {
                MessageBox.Show("Data has been changed since you opened this form!!!" + System.Environment.NewLine + "Please close the form and re-open again", "Data Changed", MessageBoxButton.OK, MessageBoxImage.Stop);
            }
            else
            {
                MessageBox.Show("There has been a problem and the dispatch was not successfull", "Dispatch Was Not Successful", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private Exception Print()
        {
            BackgroundWorker worker = new BackgroundWorker();
            ChildWindow LoadingScreen = new ChildWindow();
            Exception exception = null;
            LoadingScreen.ShowWaitingScreen("Printing");
            worker.DoWork += (_, __) =>
            {
                PrintDeliveryDocketPDF pdd = new PrintDeliveryDocketPDF(DispatchOrder);
                exception = pdd.CreateDeliveryDocket();
            };

            worker.RunWorkerCompleted += delegate(object s, RunWorkerCompletedEventArgs args)
            {
                LoadingScreen.CloseWaitingScreen();
                if (exception != null)
                {
                    MessageBox.Show("A problem has occured while the work order is prining. Please try again later." + System.Environment.NewLine + exception, "Printing Error", MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.Yes);
                }

                //CloseForm();
            };
            worker.RunWorkerAsync();


            return exception;
        }

        private void SendEmail()
        {
            bool isError = false;
            BackgroundWorker worker = new BackgroundWorker();
            ChildWindow LoadingScreen = new ChildWindow();
            Exception ex = null;
            Tuple<Exception, string, string> tuple = null;
            LoadingScreen.ShowWaitingScreen("Sending Email");
            worker.DoWork += (_, __) =>
            {
                try
                {
                    OrderHasLeftPDF pdd = new OrderHasLeftPDF(DispatchOrder);
                    tuple = pdd.CreateDeliveryDocket();

                    if (tuple.Item1 == null)
                    {
                        Microsoft.Office.Interop.Outlook.Application otApp = new Microsoft.Office.Interop.Outlook.Application();// create outlook object
                        Microsoft.Office.Interop.Outlook.MailItem otMsg = (Microsoft.Office.Interop.Outlook.MailItem)otApp.CreateItem(Microsoft.Office.Interop.Outlook.OlItemType.olMailItem); // Create mail object
                        //Microsoft.Office.Interop.Outlook.Recipient otRecip = (Microsoft.Office.Interop.Outlook.Recipient)otMsg.Recipients.Add(FindCustomerEmail());
                        Microsoft.Office.Interop.Outlook.Recipient otRecip = (Microsoft.Office.Interop.Outlook.Recipient)otMsg.Recipients.Add("chamara_sachintha@yahoo.com");
                        otRecip.Resolve();// validate recipient address
                        otMsg.Subject = "A1 Rubber - Your Order Has Left";
                        otMsg.Body = "Hi" + System.Environment.NewLine + System.Environment.NewLine + "This email is to confirm that your order has left" + System.Environment.NewLine + "Please see attached document for further details" + System.Environment.NewLine + System.Environment.NewLine + System.Environment.NewLine + "Thanks";
                        //String sSource = AppDomain.CurrentDomain.BaseDirectory + "Test.txt";
                        String sSource = tuple.Item2;
                        String sDisplayName = tuple.Item3;
                        int iPos = (int)otMsg.Body.Length + 1;
                        int iAttType = (int)Microsoft.Office.Interop.Outlook.OlAttachmentType.olByValue;
                        Microsoft.Office.Interop.Outlook.Attachment oAttach = otMsg.Attachments.Add(sSource, iAttType, iPos, sDisplayName); // add attachment
                        otMsg.Save();
                        otMsg.Send();
                        otRecip = null;
                        otMsg = null;
                        otApp = null;
                    }
                }
                catch (Exception e)
                {
                    isError = true;
                    ex = e;
                }
            };
            worker.RunWorkerCompleted += delegate(object s, RunWorkerCompletedEventArgs args)
            {
                LoadingScreen.CloseWaitingScreen();
                if (isError)
                {
                    MessageBox.Show("Could not send email." + System.Environment.NewLine + ex, "Email Sending Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.Yes);
                }
            };
            worker.RunWorkerAsync();
        }

        private string FindCustomerEmail()
        {
            string em = string.Empty;
            CustomerEmailVisibility = "Collapsed";
            CustomerEmailEnabled = true;
            SendConfirmation = true;

            if (DispatchOrder.FreightCarrier.Id != 6)
            {
                if (string.IsNullOrWhiteSpace(DispatchOrder.Customer.CompanyEmail))
                {
                    if (string.IsNullOrWhiteSpace(DispatchOrder.Customer.Email1))
                    {
                        if (string.IsNullOrWhiteSpace(DispatchOrder.Customer.Email2))
                        {
                            if (string.IsNullOrWhiteSpace(DispatchOrder.Customer.Email3))
                            {
                                //No email and cant send the email
                                em = string.Empty;
                                CustomerEmailVisibility = "Visible";
                                CustomerEmailEnabled = false;
                                SendConfirmation = false;
                            }
                            else
                            {
                                em = DispatchOrder.Customer.Email3;
                            }
                        }
                        else
                        {
                            em = DispatchOrder.Customer.Email2;
                        }
                    }
                    else
                    {
                        em = DispatchOrder.Customer.Email1;
                    }
                }
                else
                {
                    em = DispatchOrder.Customer.CompanyEmail;
                }

            }
            else
            {
                SendConfirmation = false;
                CustomerEmailEnabled = false;
            }

            Console.WriteLine(em);
            return em;
        }




        public string CustomerEmailVisibility
        {
            get { return _customerEmailVisibility; }
            set
            {
                _customerEmailVisibility = value;
                RaisePropertyChanged("CustomerEmailVisibility");
            }
        }

        public bool IsPrintDeliveryDocket
        {
            get { return _isPrintDeliveryDocket; }
            set
            {
                _isPrintDeliveryDocket = value;
                RaisePropertyChanged("IsPrintDeliveryDocket");
            }
        }

        public bool SendConfirmation
        {
            get { return _sendConfirmation; }
            set
            {
                _sendConfirmation = value;
                RaisePropertyChanged("SendConfirmation");
            }
        }

        public bool CustomerEmailEnabled
        {
            get { return _customerEmailEnabled; }
            set
            {
                _customerEmailEnabled = value;
                RaisePropertyChanged("CustomerEmailEnabled");
            }
        }



        public ICommand OkCommand
        {
            get
            {
                return _okCommand ?? (_okCommand = new CommandHandler(() => SubmitData(), true));
            }
        }

        public Microsoft.Practices.Prism.Commands.DelegateCommand CloseCommand
        {
            get { return _closeCommand; }
        }
    }
}
