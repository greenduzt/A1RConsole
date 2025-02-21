using A1RConsole.Bases;
using A1RConsole.Commands;
using A1RConsole.Core;
using A1RConsole.DB;
using A1RConsole.Models.Stock;
using A1RConsole.Models.Suppliers;
using Microsoft.Practices.Prism.Commands;
using MsgBox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;

namespace A1RConsole.ViewModels.Purchasing
{
    public class AddSupplierViewModel : ViewModelBase
    {
        private string userName;
        private ProductStockMaintenance productStockMaintenance;
        private Supplier _supplier;
        private bool canExecute;
        private string _selectedActive;
        private bool _isAddToProduct;
        private string _addToString;
        public event Action Closed;
        private Microsoft.Practices.Prism.Commands.DelegateCommand _closeCommand;
        private ICommand _addCommand;

        public AddSupplierViewModel(ProductStockMaintenance psm, string un)
        {
            productStockMaintenance = psm;
            userName = un;
            Supplier = new Supplier();
            canExecute = true;
            SelectedActive = "Yes";
            AddToString = "Do you want to add this supplier to product " + productStockMaintenance.Product.ProductDescription + "?";
            _closeCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(CloseForm);


        }

        private void AddSupplier()
        {
            if (String.IsNullOrWhiteSpace(Supplier.SupplierName))
            {
                Msg.Show("Please enter supplier name", "Supplier Name Required", MsgBoxButtons.OK, MsgBoxImage.Error);
            }
            else if (String.IsNullOrWhiteSpace(Supplier.SupplierCode))
            {
                Msg.Show("Please enter supplier code", "Supplier Code Required", MsgBoxButtons.OK, MsgBoxImage.Error);
            }
            else if (!String.IsNullOrWhiteSpace(Supplier.Email1) && !Regex.IsMatch(Supplier.Email1, @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z", RegexOptions.IgnoreCase))
            {
                Msg.Show("Please enter a correct email address for Email1", "Invalid Email Address", MsgBoxButtons.OK, MsgBoxImage.Error);
            }
            else if (!String.IsNullOrWhiteSpace(Supplier.Email2) && !Regex.IsMatch(Supplier.Email2, @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z", RegexOptions.IgnoreCase))
            {
                Msg.Show("Please enter a correct email address for Email2", "Invalid Email Address", MsgBoxButtons.OK, MsgBoxImage.Error);
            }
            else
            {
                Supplier.Active = ConvertBackToBoolean(SelectedActive);
                int res = DBAccess.InsertSupplier(Supplier);
                if (res == 2)
                {
                    Msg.Show("Supplier Name or Supplier Code exist", "Data Exist", MsgBoxButtons.OK, MsgBoxImage.Error);
                }
                else if (res == 1)
                {
                    Msg.Show("New supplier was added successfully", "Supplier Added", MsgBoxButtons.OK, MsgBoxImage.OK);
                    LoadProdInventory();
                }
            }
        }

        private void CloseForm()
        {
            LoadProdInventory();
        }

        private void LoadProdInventory()
        {
            var childWindow = new ChildWindow();
            childWindow.updateProductInventory_Closed += (r =>
            {
                //LoadData();

            });
            childWindow.ShowUpdateProductInventory(productStockMaintenance, userName, IsAddToProduct, Supplier);
        }

        private bool ConvertBackToBoolean(string name)
        {
            bool ex = false;
            switch (name)
            {
                case "Yes": ex = true;
                    break;
                case "No": ex = false;
                    break;
                default:
                    break;
            }

            return ex;
        }


        public Supplier Supplier
        {
            get
            {
                return _supplier;
            }
            set
            {
                _supplier = value;
                RaisePropertyChanged("Supplier");
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

        public bool IsAddToProduct
        {
            get
            {
                return _isAddToProduct;
            }
            set
            {
                _isAddToProduct = value;
                RaisePropertyChanged("IsAddToProduct");
            }
        }

        public string AddToString
        {
            get
            {
                return _addToString;
            }
            set
            {
                _addToString = value;
                RaisePropertyChanged("AddToString");
            }
        }


        public Microsoft.Practices.Prism.Commands.DelegateCommand CloseCommand
        {
            get { return _closeCommand; }
        }

        public ICommand AddCommand
        {
            get
            {
                return _addCommand ?? (_addCommand = new CommandHandler(() => AddSupplier(), canExecute));
            }
        }
    }
}