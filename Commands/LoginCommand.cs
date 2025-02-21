using A1RConsole.Core;
using A1RConsole.DB;
using A1RConsole.Models.Meta;
using A1RConsole.Models.Users;
using A1RConsole.ViewModels.Login;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace A1RConsole.Commands
{
    public class LoginCommand : ICommand
    {
        LoginViewModel loginViewModel = null;
        List<UserPrivilages> uPriv;
        Grid TopContent = null;
        //Pass an instance of the ViewModel into the constructor
        public LoginCommand(LoginViewModel loginViewModel, Grid topContent)
        {
            this.loginViewModel = loginViewModel;
            this.TopContent = topContent;
        }

        public LoginCommand(LoginViewModel loginViewModel)
        {
            this.loginViewModel = loginViewModel;

        }

        public bool CanExecute(object parameter)
        {
            //Execution should only be possible if both Username and Password have been supplied
            if (!string.IsNullOrWhiteSpace(this.loginViewModel.Username) && this.loginViewModel.PasswordSecureString != null && this.loginViewModel.PasswordSecureString.Length > 0)
                return true;
            else
                return false;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter)
        {
            User userLogin = DBAccess.Login(this.loginViewModel.Username);

            if (userLogin.Username == null)
            {
                this.loginViewModel.ErrorMessage = "User not found!";
                return;
            }
            else
            {
                byte[] enteredValueHash = PasswordHashing.CalculateHash(SecureStringManipulation.ConvertSecureStringToByteArray(this.loginViewModel.PasswordSecureString));


                if (!PasswordHashing.SequenceEquals(enteredValueHash, userLogin.Password))
                {
                    this.loginViewModel.ErrorMessage = "Incorrect Password entered!";
                    return;
                }

                this.loginViewModel.ErrorMessage = "Login Successful!";

                string userDetails = userLogin.FirstName + " " + userLogin.LastName;
                string userstate = userLogin.State;

                uPriv = new List<UserPrivilages>();
                uPriv = DBAccess.GetUserPrivilages(userLogin.ID);

                UserData.UserPrivilages = uPriv;

                List<MetaData> metaData = new List<MetaData>();
                metaData = DBAccess.GetMetaData();
                UserData.MetaData = metaData;

                userLogin.FullName = userLogin.FirstName + " " + userLogin.LastName;
                UserData.UserID = userLogin.ID;
                UserData.FirstName = userLogin.FirstName;
                UserData.LastName = userLogin.LastName;
                UserData.State = userstate;
                UserData.UserName = this.loginViewModel.Username;

                MainWindow mw = new MainWindow();
                mw.Show();
                Application.Current.Windows[0].Close();
            }
        }

        private bool GetPrivBool(string str)
        {
            bool r = false;
            if (str == "Visible")
            {
                r = true;
            }
            else if (str == "Collapsed")
            {
                r = false;
            }
            return r;
        }
    }
}
