using A1RConsole.Bases;
using A1RConsole.Commands;
using A1RConsole.Core;
using A1RConsole.DB;
using A1RConsole.Models;
using A1RConsole.Models.Customers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;


namespace A1RConsole.ViewModels.Customers
{
    public class AddCustomerCreditViewModel : ViewModelBase
    {
        private List<CustomerCreditActivity> ccaList;
        private bool _isEnabledCreditLimit;
        private decimal creditRemaining;
        //private string _creditString;
        private string _updateCreditLimitVisibility;
        private decimal _credit;
        //private decimal _creditLimit;
        private decimal _increaseCreditLimit;
        private decimal _decreaseCreditLimit;
        private Customer _customer;
        private bool canExecute;
        private bool _creditLimitEnabled;
        public event Action<Customer> Closed;
        private Microsoft.Practices.Prism.Commands.DelegateCommand _closeCommand;
        private ICommand _addCreditCommand;

        public AddCustomerCreditViewModel(Customer cus)
        {

            ccaList = new List<CustomerCreditActivity>();
            CreditLimitEnabled = false;
            IsEnabledCreditLimit = true;
            canExecute = true;
            IncreaseCreditLimit = 0;
            DecreaseCreditLimit = 0;

            foreach (var item in UserData.UserPrivilages)
            {
                if(item.Area.Trim() == "CustomerCreditIncDec")
                {
                    CreditLimitEnabled = Convert.ToBoolean(item.Visibility);
                    break;
                }
            }

            Customer = cus;

            Customer c = DBAccess.GetCustomerCreditDetails(Customer.CustomerId);
            if (c != null)
            {
                if(c.CreditLimit != cus.CreditLimit || c.CreditRemaining != cus.CreditRemaining || c.CreditOwed != cus.CreditOwed)
                {
                    MessageBox.Show("Customer credit details have been changed since you opened this form" + System.Environment.NewLine + "Form will be updated with new data", "Information Has Been Changed", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                }

                Customer.CreditLimit = c.CreditLimit;
                Customer.CreditRemaining = c.CreditRemaining;
                Customer.CreditOwed = c.CreditOwed;
                Customer.Debt = c.Debt;
            }
          

            creditRemaining = Customer.CreditRemaining;
            
            if(Customer.CustomerId ==0)
            {
                IsEnabledCreditLimit = false;
            }

            if (creditRemaining > 0 && Customer.CreditLimit > 0)
            {
                UpdateCreditLimitVisibility="Visible";
            }
            else
            {
                UpdateCreditLimitVisibility = "Collapsed";
            }
                       

            _closeCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(CloseForm);
        }

        private void AddCredit()
        {
            if (Credit == 0 && (IncreaseCreditLimit == 0 && DecreaseCreditLimit==0))
            {
                MessageBox.Show("Please enter a valid amount to Add Credit field", "Input Required", MessageBoxButton.OK, MessageBoxImage.Asterisk);
            }
            else
            {
                if (Customer.CustomerId != 0)
                {
                    ccaList.Clear();
                    string activity = string.Empty;                    

                    ////Existing customer but no existing credit, wish to add new credit
                    //if (Customer.CreditLimit == 0 && Customer.CreditRemaining == 0 && Customer.CreditOwed == 0 && Customer.Debt == 0)
                    //{
                    //    Customer.CreditLimit = Credit;
                    //    Customer.CreditRemaining = Credit;
                    //    ccaList.Add(new CustomerCreditActivity() { Customer = new Customer() { CustomerId = Customer.CustomerId }, SalesOrderNo = 0, Amount = Credit, Type = "Credit Added", Activity ="Credit added " +  "$" + Credit.ToString("G29") , UpdatedDate = DateTime.Now, UpdatedBy = UserData.FirstName + " " + UserData.LastName });
                    //    ccaList.Add(new CustomerCreditActivity() { Customer = new Customer() { CustomerId = Customer.CustomerId }, SalesOrderNo = 0, Amount = Credit, Type = "Credit Limit Increased", Activity = "Credit limit increased by $" + Credit.ToString("G29"), UpdatedDate = DateTime.Now, UpdatedBy = UserData.FirstName + " " + UserData.LastName });

                    //    if (MessageBox.Show("You are adding $" + Credit + " of credit to customer " + Customer.CompanyName + System.Environment.NewLine + System.Environment.NewLine + "The new total credit remaining is " + Customer.CreditRemaining.ToString("C", CultureInfo.CurrentCulture) 
                    //        + System.Environment.NewLine
                    //        + "The Credit Owed is " + Customer.CreditOwed.ToString("C", CultureInfo.CurrentCulture) 
                    //        + System.Environment.NewLine 
                    //        + System.Environment.NewLine 
                    //        + "Click YES to proceed", "Confirm Details", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.OK) == MessageBoxResult.Yes)
                    //    {
                    //        Customer cusRes = DBAccess.UpdateCustomerCredit(Customer, Credit, ccaList);
                    //        CloseForm();
                    //    }                        
                    //}
                  //  else
                  //  {

                    string msgStr = String.Empty;
                    Tuple<Customer, List<CustomerCreditActivity>,string> cusTup = CreditManager.AddCustomerCredit(Customer, Credit, IncreaseCreditLimit, DecreaseCreditLimit);


                    if (!string.IsNullOrWhiteSpace(cusTup.Item3))
                    {
                        MessageBox.Show(cusTup.Item3, "Invalid Credit Amount", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    }
                    else if (cusTup.Item1.CreditRemaining > cusTup.Item1.CreditLimit)
                    {
                        MessageBox.Show("Customer Credit Remaining cannot be greater than Credit Limit"
                            + System.Environment.NewLine
                            + System.Environment.NewLine
                            + "Customer Credit Limit " + (cusTup.Item1.CreditLimit).ToString("C", CultureInfo.CurrentCulture)
                            + System.Environment.NewLine
                            + "Customer Credit Remaining " + (cusTup.Item1.CreditRemaining).ToString("C", CultureInfo.CurrentCulture)
                            + System.Environment.NewLine
                            + "Customer Credit Owed " + (cusTup.Item1.CreditOwed).ToString("C", CultureInfo.CurrentCulture), "Invalid Credit Amount", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    }
                    //else if (Credit == 0 && ((IncreaseCreditLimit == 0 && DecreaseCreditLimit == 0) && (Customer.CreditLimit) < (Customer.CreditRemaining + Credit)))
                    //{
                    //    MessageBox.Show("Customer Credit Remaining cannot be greater than Credit Limit" + System.Environment.NewLine + System.Environment.NewLine + "Customer Credit Limit " + (Customer.CreditLimit).ToString("C", CultureInfo.CurrentCulture) + System.Environment.NewLine + "Customer Credit Remaining " + (Customer.CreditRemaining).ToString("C", CultureInfo.CurrentCulture), "Invalid Credit Amount", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    //}
                    //else if (IncreaseCreditLimit > 0 && (cusTup.Item1.CreditLimit < cusTup.Item1.CreditRemaining))
                    //{
                    //    MessageBox.Show("Customer Credit Remaining cannot be greater than the Credit Limit" + System.Environment.NewLine + System.Environment.NewLine + "Customer Credit Limit " + (cusTup.Item1.CreditLimit + IncreaseCreditLimit).ToString("C", CultureInfo.CurrentCulture) + System.Environment.NewLine + "Customer Credit Remaining " + (Customer.CreditRemaining + Credit).ToString("C", CultureInfo.CurrentCulture), "Invalid Credit Amount", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    //}
                    //else if (DecreaseCreditLimit > 0 && (cusTup.Item1.CreditLimit < cusTup.Item1.CreditRemaining))
                    //{
                    //    MessageBox.Show("Customer Credit Remaining cannot be greater than the Credit Limit" + System.Environment.NewLine + System.Environment.NewLine + "Customer Credit Limit " + ((cusTup.Item1.CreditLimit - DecreaseCreditLimit) < 0 ? 0 : (Customer.CreditLimit - DecreaseCreditLimit)).ToString("C", CultureInfo.CurrentCulture) + System.Environment.NewLine + "Customer Credit Remaining " + (Customer.CreditRemaining + Credit).ToString("C", CultureInfo.CurrentCulture), "Invalid Credit Amount", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    //}
                    else
                    {
                        if (cusTup.Item1 != null)
                        {
                            if (Credit > 0)
                            {
                                msgStr = "You are adding $" + Credit + " of credit to customer " + Customer.CompanyName + System.Environment.NewLine + System.Environment.NewLine + "The new total credit remaining is " + cusTup.Item1.CreditRemaining.ToString("C", CultureInfo.CurrentCulture)
                                         + System.Environment.NewLine
                                         + "The Credit Owed is " + cusTup.Item1.CreditOwed.ToString("C", CultureInfo.CurrentCulture);
                            }

                            if (IncreaseCreditLimit > 0)
                            {
                                if (msgStr != "")
                                {
                                    msgStr += System.Environment.NewLine + System.Environment.NewLine + "Increased credit limit by " + IncreaseCreditLimit.ToString("C", CultureInfo.CurrentCulture) + System.Environment.NewLine + "The new credit limit is " + cusTup.Item1.CreditLimit.ToString("C", CultureInfo.CurrentCulture);
                                }
                                else
                                {
                                    msgStr += "Increased credit limit by " + IncreaseCreditLimit.ToString("C", CultureInfo.CurrentCulture)
                                           + System.Environment.NewLine 
                                           + System.Environment.NewLine + "The new credit limit is " + cusTup.Item1.CreditLimit.ToString("C", CultureInfo.CurrentCulture)
                                           + System.Environment.NewLine + "Credit Remaining is " + cusTup.Item1.CreditRemaining.ToString("C", CultureInfo.CurrentCulture)
                                           + System.Environment.NewLine + "Credit Owed is " + cusTup.Item1.CreditOwed.ToString("C", CultureInfo.CurrentCulture);
                                }
                            }
                            else if (DecreaseCreditLimit > 0)
                            {
                                if (msgStr != "")
                                {
                                    msgStr += System.Environment.NewLine + System.Environment.NewLine + "Decreased credit limit by " + DecreaseCreditLimit.ToString("C", CultureInfo.CurrentCulture) + System.Environment.NewLine + "The new credit limit is " + cusTup.Item1.CreditLimit.ToString("C", CultureInfo.CurrentCulture);
                                }
                                else
                                {
                                    msgStr += "Decreased credit limit by " + DecreaseCreditLimit.ToString("C", CultureInfo.CurrentCulture)
                                        + System.Environment.NewLine 
                                        + System.Environment.NewLine + "The new credit limit is " + cusTup.Item1.CreditLimit.ToString("C", CultureInfo.CurrentCulture)
                                        + System.Environment.NewLine + "Credit Remaining is " + cusTup.Item1.CreditRemaining.ToString("C", CultureInfo.CurrentCulture)
                                        + System.Environment.NewLine + "Credit Owed is " + cusTup.Item1.CreditOwed.ToString("C", CultureInfo.CurrentCulture);
                                }
                            }

                            if (MessageBox.Show(msgStr + System.Environment.NewLine + System.Environment.NewLine + "Click YES to proceed", "Confirm Details", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.OK) == MessageBoxResult.Yes)
                            {
                                Customer upCreditInfo = CreditManager.AddNewCreditDB(cusTup.Item1, Credit, cusTup.Item2);

                                Customer.CreditRemaining = upCreditInfo.CreditRemaining;
                                Customer.CreditLimit = upCreditInfo.CreditLimit;
                                Customer.Debt = upCreditInfo.Debt;
                                Customer.CreditOwed = upCreditInfo.CreditOwed;

                                CloseForm();
                            }
                        }
                        else
                        {
                            MessageBox.Show("Cannot load Customer information! Please try again later", "Failed To Load Info", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                   // }
                }
                else
                {
                    ccaList.Clear();
                    Customer.CreditLimit = Credit;
                    Customer.Debt = 0;
                    Customer.CreditRemaining = Credit;

                    ccaList.Add(new CustomerCreditActivity() { Customer = new Customer() { CustomerId = 0 }, SalesOrderNo = 0, Amount = Credit, Type = "Credit Added", Activity = "Credit added " + "$" + Credit.ToString("G29"), UpdatedDate = DateTime.Now, UpdatedBy = UserData.FirstName + " " + UserData.LastName });
                    ccaList.Add(new CustomerCreditActivity() { Customer = new Customer() { CustomerId = 0 }, SalesOrderNo = 0, Amount = Credit, Type = "Credit Limit Added", Activity = "Credit limit added by $" + Credit.ToString("G29"), UpdatedDate = DateTime.Now, UpdatedBy = UserData.FirstName + " " + UserData.LastName });

                    CloseForm();

                }

            }

        }

        private void CloseForm()
        {
            if (Closed != null)
            {
                Closed(Customer);
            }
        }


        //public string CreditString
        //{
        //    get
        //    {
        //        return _creditString;
        //    }
        //    set
        //    {
        //        _creditString = value;
        //        RaisePropertyChanged("CreditString");
        //    }
        //}


        public Decimal Credit
        {
            get
            {
                return _credit;
            }
            set
            {
                _credit = value;
                RaisePropertyChanged("Credit");
            }
        }

        //public Decimal CreditLimit
        //{
        //    get
        //    {
        //        return _creditLimit;
        //    }
        //    set
        //    {
        //        _creditLimit = value;
        //        RaisePropertyChanged("CreditLimit");
        //    }
        //}

        public Customer Customer
        {
            get
            {
                return _customer;
            }
            set
            {
                _customer = value;
                RaisePropertyChanged("Customer");
            }
        }

        public bool IsEnabledCreditLimit
        {
            get
            {
                return _isEnabledCreditLimit;
            }
            set
            {
                _isEnabledCreditLimit = value;
                RaisePropertyChanged("IsEnabledCreditLimit");
            }
        }

        public decimal IncreaseCreditLimit
        {
            get
            {
                return _increaseCreditLimit;
            }
            set
            {
                _increaseCreditLimit = value;
                RaisePropertyChanged("IncreaseCreditLimit");

                if (IncreaseCreditLimit > 0)
                {
                    DecreaseCreditLimit = 0;
                }
            }
        }

        public string UpdateCreditLimitVisibility
        {
            get
            {
                return _updateCreditLimitVisibility;
            }
            set
            {
                _updateCreditLimitVisibility = value;
                RaisePropertyChanged("UpdateCreditLimitVisibility");
            }
        }

        public decimal DecreaseCreditLimit
        {
            get
            {
                return _decreaseCreditLimit;
            }
            set
            {
                _decreaseCreditLimit = value;
                RaisePropertyChanged("DecreaseCreditLimit");

                if(DecreaseCreditLimit > 0)
                {
                    IncreaseCreditLimit = 0;
                }
            }
        }



        public bool CreditLimitEnabled
        {
            get
            {
                return _creditLimitEnabled;
            }
            set
            {
                _creditLimitEnabled = value;
                RaisePropertyChanged("CreditLimitEnabled");
            }
        }

        public Microsoft.Practices.Prism.Commands.DelegateCommand CloseCommand
        {
            get { return _closeCommand; }
        }

        public ICommand AddCreditCommand
        {
            get
            {
                return _addCreditCommand ?? (_addCreditCommand = new CommandHandler(() => AddCredit(), canExecute));
            }
        }
    }
}

