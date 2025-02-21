using A1RConsole.Bases;
using A1RConsole.Commands;
using A1RConsole.Core;
using A1RConsole.DB;
using A1RConsole.Models.Customers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace A1RConsole.ViewModels.Customers
{
    public class AddCustomerCreditNoOverlayViewModel : ViewModelBase
    {
        private bool _isEnabledCreditLimit;
        private decimal creditRemaining;
        private string _creditString;
        private decimal _credit;
        private decimal _creditLimit;
        private Customer _customer;
        private bool canExecute;
        public event Action<Customer> Closed;
        private Microsoft.Practices.Prism.Commands.DelegateCommand _closeCommand;
        private ICommand _addCreditCommand;
        public event EventHandler RequestClose;

        public AddCustomerCreditNoOverlayViewModel(Customer cus)
        {
            Customer = cus;
            creditRemaining = Customer.CreditRemaining;
            CreditLimit = Customer.CreditLimit;
            canExecute = true;
            IsEnabledCreditLimit = false;
            if (Customer.CustomerId > 0)
            {
                CreditString = "Add Credit";
            }
            else
            {
                CreditString = "Add Credit Limit";
            }            
        }

        protected void OnRequestClose()
        {
            EventHandler handler = this.RequestClose;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        private void AddCredit()
        {
            if (Credit == 0 && CreditLimit == 0)
            {
                MessageBox.Show("Please enter a valid amount for credit or credit limit", "Input Required", MessageBoxButton.OK, MessageBoxImage.Asterisk);
            }
            else
            {
                //if (Customer.CustomerId != 0)
                //{
                    string activity = string.Empty;
                    decimal cco = Customer.Debt;
                    decimal co = Customer.CreditOwed;
                    Customer.Debt = (cco - Credit) < 0 ? 0 : cco - Credit;
                    Customer.CreditOwed = (co - Credit) < 0 ? 0 : co - Credit;
                    if (Customer.Debt > 0)
                    {
                        Customer.CreditRemaining = 0;
                    }
                    else if (Customer.Debt <= 0)
                    {
                        Customer.CreditRemaining = Customer.CreditRemaining + (Credit - cco);
                    }

                    if (creditRemaining != Customer.CreditRemaining || CreditLimit != Customer.CreditLimit)
                    {
                        Customer.LastUpdatedBy = Customer.LastUpdatedBy;
                        Customer.LastUpdatedDateTime = DateTime.Now;
                        if (Credit > 0)
                        {
                            activity = "$" + Credit.ToString("G29") + " of credit added ";
                        }
                        if (Customer.CreditLimit > CreditLimit)
                        {
                            if (string.IsNullOrWhiteSpace(activity))
                            {
                                activity += "credit limit decreased";
                            }
                            else
                            {
                                activity += "and Credit limit decreased";
                            }
                        }
                        else if (Customer.CreditLimit < CreditLimit)
                        {
                            if (string.IsNullOrWhiteSpace(activity))
                            {
                                activity += "credit limit increased";
                            }
                            else
                            {
                                activity += "and Credit limit increased";
                            }
                        }

                        Customer.CreditLimit = CreditLimit;
                        Global.customer = Customer;
                        Global.creditLimit = Credit;
                        Global.creditRemaining = Credit;
                        Global.credit = Credit;
                        Global.activity = activity;
                        //int res = DBAccess.UpdateCustomerCredit(Customer, Credit, activity);
                        OnRequestClose();
                    }
                    else
                    {
                        MessageBox.Show("No changes detected", "No Changes", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    }
                //}
                //else
                //{
                //    Customer.CreditLimit = Credit;
                //    Customer.Debt = 0;
                //    Customer.CreditRemaining = Credit;
                //    CloseForm();
                //}

            }

        }      


        public string CreditString
        {
            get
            {
                return _creditString;
            }
            set
            {
                _creditString = value;
                RaisePropertyChanged("CreditString");
            }
        }


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

        public Decimal CreditLimit
        {
            get
            {
                return _creditLimit;
            }
            set
            {
                _creditLimit = value;
                RaisePropertyChanged("CreditLimit");
            }
        }

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


        

        public ICommand AddCreditCommand
        {
            get
            {
                return _addCreditCommand ?? (_addCreditCommand = new CommandHandler(() => AddCredit(), canExecute));
            }
        }
    }
}

