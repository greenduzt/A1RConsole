using A1RConsole.Bases;
using A1RConsole.Commands;
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
    public class AddUpdateContactPersonViewModel : ViewModelBase
    {
        private string _addCommandVisibility;
        private string _updateCommandVisibility;
        private string _headerText;
        private string _selectedActive;
        private bool _activeEnabled;
        private ContactPerson _contactPerson;
        public event Action<ContactPerson> Closed;
        private ICommand _closeCommand;
        private ICommand _addCommand;
        private ICommand _updateCommand;

        public AddUpdateContactPersonViewModel(ContactPerson cp)
        {
            ContactPerson = new ContactPerson();

            if (!string.IsNullOrWhiteSpace(cp.ContactPersonName))
            {
                AddCommandVisibility = "Collapsed";
                UpdateCommandVisibility = "Visible";
                HeaderText = "Edit Contact Person";
                ContactPerson = cp;
                SelectedActive = cp.Active ? "Yes" : "No";
                ActiveEnabled = true;
            }
            else
            {
                ActiveEnabled = false;
                SelectedActive = "Yes";
                ContactPerson.CustomerID = cp.CustomerID;
                ContactPerson.ContactPersonName = string.Empty;
                ContactPerson.PhoneNumber1 = string.Empty;
                ContactPerson.PhoneNumber2 = string.Empty;
                ContactPerson.Email = string.Empty;

                AddCommandVisibility = "Visible";
                UpdateCommandVisibility = "Collapsed";
                HeaderText = "Add New Contact Person";
            }
            _closeCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(CloseForm);
        }


        private void CloseForm()
        {
            Closed(ContactPerson);
        }

        private void AddContactPerson()
        {
            if (string.IsNullOrWhiteSpace(ContactPerson.ContactPersonName))
            {
                MessageBox.Show("Contact person name required", "Contact Person Name Required", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            //else if (string.IsNullOrWhiteSpace(ContactPerson.PhoneNumber1) && string.IsNullOrWhiteSpace(ContactPerson.PhoneNumber2))
            //{
            //    MessageBox.Show("Atleast one phone number required", "Phone Number Required", MessageBoxButton.OK, MessageBoxImage.Error);
            //}
            else
            {
                if(string.IsNullOrWhiteSpace(ContactPerson.PhoneNumber1) && !string.IsNullOrWhiteSpace(ContactPerson.PhoneNumber2))
                {                    
                    ContactPerson.PhoneNumber1 = ContactPerson.PhoneNumber2;
                    ContactPerson.PhoneNumber2 = string.Empty;
                }

                int res = 0;
                ContactPerson.CustomerID = ContactPerson.CustomerID;
                ContactPerson.Active = true;

                if (ContactPerson.CustomerID > 0)
                {
                    res = DBAccess.AddContactPerson(ContactPerson);
                    if (res == 0)
                    {
                        MessageBox.Show("There has been an error and the details have not been saved to the database" + System.Environment.NewLine + "Please try again later", "Details Were Not Saved", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else if (res == -1)
                    {
                        MessageBox.Show("Contact person name [" + ContactPerson.ContactPersonName + "] exist in the database!", "Name Exists", MessageBoxButton.OK, MessageBoxImage.Error);

                    }
                }
                else
                {

                }

                if (res != -1)
                {
                    CloseForm();
                }
            }
           
        }

        private void UpdateContactPerson()
        {
            if (string.IsNullOrWhiteSpace(ContactPerson.ContactPersonName))
            {
                MessageBox.Show("Contact person name required", "Contact Person Name Required", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            //else if (string.IsNullOrWhiteSpace(ContactPerson.PhoneNumber1) && string.IsNullOrWhiteSpace(ContactPerson.PhoneNumber2))
            //{
            //    MessageBox.Show("Atleast one phone number required", "Phone Number Required", MessageBoxButton.OK, MessageBoxImage.Error);
            //}
            else
            {
                if (string.IsNullOrWhiteSpace(ContactPerson.PhoneNumber1) && !string.IsNullOrWhiteSpace(ContactPerson.PhoneNumber2))
                {
                    ContactPerson.PhoneNumber1 = ContactPerson.PhoneNumber2;
                    ContactPerson.PhoneNumber2 = string.Empty;
                }

                ContactPerson.Active = SelectedActive == "Yes" ? true : false;

                if (ContactPerson.CustomerID > 0)
                {

                    int res = DBAccess.UpdateContactPerson(ContactPerson);
                    if (res > 0)
                    {
                        MessageBox.Show("Details successfully updated!", "Contact Details Updated", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("No changes were detected to update this record!", "No Changes Found", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                    }
                }

                CloseForm();
            }
            
        }


        public string AddCommandVisibility
        {
            get { return _addCommandVisibility; }
            set
            {
                _addCommandVisibility = value;
                RaisePropertyChanged("AddCommandVisibility");
            }
        }

        public string UpdateCommandVisibility
        {
            get { return _updateCommandVisibility; }
            set
            {
                _updateCommandVisibility = value;
                RaisePropertyChanged("UpdateCommandVisibility");
            }
        }

       
        public string SelectedActive
        {
            get
            {
                return _selectedActive;
            }
            set
            {
                _selectedActive = value;
                RaisePropertyChanged("SelectedActive");
            }
        }
        

        public ContactPerson ContactPerson
        {
            get { return _contactPerson; }
            set
            {
                _contactPerson = value;
                RaisePropertyChanged("ContactPerson");

                if (ContactPerson != null)
                {
                    if (ContactPerson.PhoneNumber1 != null && ContactPerson.PhoneNumber1.Equals("Not Available"))
                    {
                        ContactPerson.PhoneNumber1 = string.Empty;
                    }
                    if (ContactPerson.PhoneNumber2 != null && ContactPerson.PhoneNumber2.Equals("Not Available"))
                    {
                        ContactPerson.PhoneNumber2 = string.Empty;
                    }
                }
                
            }
        }

        public string HeaderText
        {
            get { return _headerText; }
            set
            {
                _headerText = value;
                RaisePropertyChanged("HeaderText");
            }
        }

        public bool ActiveEnabled
        {
            get { return _activeEnabled; }
            set
            {
                _activeEnabled = value;
                RaisePropertyChanged("ActiveEnabled");
            }
        }

        
        

        #region Commands



        public ICommand UpdateCommand
        {
            get
            {
                return _updateCommand ?? (_updateCommand = new CommandHandler(() => UpdateContactPerson(), true));
            }
        }

        public ICommand CloseCommand
        {
            get
            {
                return _closeCommand ?? (_closeCommand = new CommandHandler(() => CloseForm(), true));
            }
        }

        public ICommand AddCommand
        {
            get
            {
                return _addCommand ?? (_addCommand = new CommandHandler(() => AddContactPerson(), true));
            }
        }

        

        #endregion
    }
}
