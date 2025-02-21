using A1RConsole.Commands;
using A1RConsole.DB;
using A1RConsole.Models.Meta;
using A1RConsole.Models.Products;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace A1RConsole.ViewModels.Login
{
    public class LoginViewModel : CommonBase
    {
        private LoginCommand loginCommand = null;
        private bool _loginFailed;
        private string _errorMessage;
        private string _username;
        private SecureString _password;
        private bool _loginActive;
        private bool _isUserNameFocused;
        private int _id;
        private string _firstName;
        private string _lastName;
        private string _version;
        

        public LoginViewModel()
        {
            this.LoginActive = false;
            loginCommand = new LoginCommand(this);
            IsUserNameFocused = true;

            LoadMetaData();

            //Username = "caseyb";
     
        }

        private void LoadMetaData()
        {
            List<MetaData> metaData = new List<MetaData>();
            metaData = DBAccess.GetMetaData();
            if (metaData != null)
            {
                var data = metaData.SingleOrDefault(x => x.KeyName == "version");
                Version = data.Description;
            }
            else
            {
                MessageBox.Show("Cannot connect to the server" + System.Environment.NewLine + "Please check your Internet/VPN connection", "Connection To The Server Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }
        }

        
        private void CheckUserNamePassLength()
        {
            if (!String.IsNullOrWhiteSpace(Username) && PasswordSecureString != null && PasswordSecureString.Length > 0)
            {
                LoginActive = true;
                ErrorMessage = string.Empty;
            }
            else
            {
                LoginActive = false;
            }
        }

        private void CloseWindow(object o)
        {
            if (MessageBox.Show("Are you sure you want to exit from A1 Rubber Console?", "Exit Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes) == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown();
            }
        }

        #region PUBLIC PROPERTIES

        public LoginCommand UserLoginCommand
        {
            get { return loginCommand; }

        }

        public bool LoginFailed
        {
            get { return _loginFailed; }
            set
            {
                _loginFailed = value;
                RaisePropertyChanged("FailedLogin");
            }
        }

        public string ErrorMessage
        {
            get { return _errorMessage; }
            set
            {
                if (value != _errorMessage)
                {
                    _errorMessage = value;
                    RaisePropertyChanged("ErrorMessage");
                }
            }
        }

        public string Version
        {
            get { return _version; }
            set
            {
                if (value != _version)
                {
                    _version = value;
                    RaisePropertyChanged("Version");
                }
            }
        }

        public string Username
        {
            get { return _username; }
            set
            {
                if (!string.Equals(value.ToString(), _username, StringComparison.OrdinalIgnoreCase))
                {
                    _username = value;
                    RaisePropertyChanged("Username");

                }

                CheckUserNamePassLength();
                if (String.IsNullOrWhiteSpace(Username))
                {
                    ErrorMessage = "User name required!";
                }
                else
                {
                    ErrorMessage = string.Empty;
                }

            }
        }
        public SecureString PasswordSecureString
        {
            get { return _password; }
            set
            {
                if (value != null)
                {
                    _password = value;
                    RaisePropertyChanged("Password");

                }

                CheckUserNamePassLength();

                if (!String.IsNullOrWhiteSpace(Username) && PasswordSecureString != null && PasswordSecureString.Length > 0)
                {
                    ErrorMessage = string.Empty;
                }
                else
                {
                    ErrorMessage = "Password required!";

                }

            }
        }

        public bool LoginActive
        {
            get
            {
                return _loginActive;
            }
            set
            {
                _loginActive = value;
                RaisePropertyChanged("LoginActive");
            }
        }

        public int ID
        {
            get
            {
                return _id;
            }
            set
            {
                _id = value;
            }
        }

        public string FirstName
        {
            get
            {
                return _firstName;
            }
            set
            {
                _firstName = value;
                RaisePropertyChanged("FirstName");
            }
        }

        public string LastName
        {
            get
            {
                return _lastName;
            }
            set
            {
                _lastName = value;
                RaisePropertyChanged("LastName");
            }
        }

        public bool IsUserNameFocused
        {
            get
            {
                return _isUserNameFocused;
            }
            set
            {
                _isUserNameFocused = value;
                RaisePropertyChanged("IsUserNameFocused");
            }
        }
        
        


        #endregion

        #region COMMANDS


        public ICommand CloseCommand
        {
            get { return new RelayCommand((o) => CloseWindow(o), (o) => true); }

        }

        #endregion

    }
}

