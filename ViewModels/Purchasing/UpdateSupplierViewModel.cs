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
    public class UpdateSupplierViewModel : ViewModelBase
    {
        private string userName;
        private List<Supplier> _supplierList;
        private ProductStockMaintenance _productStockMaintenance;
        private Supplier _supplier;
        private Supplier _selectedSupplier;
        private string _selectedActive;
        private string _addToString;
        private bool canExecute;
        private bool _isAddToProduct;
        public event Action Closed;
        private Microsoft.Practices.Prism.Commands.DelegateCommand _closeCommand;
        private ICommand _updateCommand;

        public UpdateSupplierViewModel(ProductStockMaintenance psm, string un, Supplier sp)
        {
            Supplier = sp;
            userName = un;
            productStockMaintenance = psm;
            SupplierList = new List<Supplier>();
            canExecute = true;
            SelectedActive = ConvertToStringActive(Supplier.Active);
            AddToString = "Do you want to add this supplier to product " + productStockMaintenance.Product.ProductDescription + "?";
            _closeCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(CloseForm);
            LoadSuppliers();
        }

        private void LoadSuppliers()
        {

            Supplier = DBAccess.GetSupplierByProductID(productStockMaintenance.Product.ProductID);
            SupplierList.Clear();
            SupplierList = DBAccess.GetAllSuppliers();
            SupplierList.Add(new Supplier() { SupplierID = 0, SupplierName = "Select" });
            SelectedSupplier = Supplier;
            if (Supplier.SupplierID == 0)
            {
                if (SelectedSupplier != null)
                {
                    SelectedSupplier = new Supplier() { SupplierID = 0, SupplierName = "Select" };
                }
            }
        }


        private void UpdateSupplier()
        {
            if (Supplier.SupplierName == "Select")
            {
                Msg.Show("Please select supplier", "Supplier Required", MsgBoxButtons.OK, MsgBoxImage.Error);
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
                int res = DBAccess.UpdateSupplier(Supplier, userName);
                if (res > 0)
                {
                    Msg.Show("Supplier updated successfully", "Supplier Updated", MsgBoxButtons.OK, MsgBoxImage.OK);
                    LoadProdInventory();
                }
                else
                {
                    Msg.Show("You haven't made any changes", "No Changes Were Made", MsgBoxButtons.OK, MsgBoxImage.Information);
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
            childWindow.ShowUpdateProductInventory(productStockMaintenance, userName, IsAddToProduct, SelectedSupplier);
        }

        private string ConvertToStringActive(bool b)
        {
            string name = string.Empty;
            switch (b)
            {
                case true: name = "Yes";
                    break;
                case false: name = "No";
                    break;
                default:
                    break;
            }

            return name;
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

        #region PUBLIC_PROPERTIES

        public ProductStockMaintenance productStockMaintenance
        {
            get
            {
                return _productStockMaintenance;
            }
            set
            {
                _productStockMaintenance = value;
                RaisePropertyChanged("productStockMaintenance");
            }
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

        public List<Supplier> SupplierList
        {
            get
            {
                return _supplierList;
            }
            set
            {
                _supplierList = value;
                RaisePropertyChanged("SupplierList");
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

        public Supplier SelectedSupplier
        {
            get
            {
                return _selectedSupplier;
            }
            set
            {
                _selectedSupplier = value;
                RaisePropertyChanged("SelectedSupplier");

                if (SelectedSupplier != null)
                {
                    //LoadSuppliers();
                    Supplier = null;
                    Supplier = SelectedSupplier;
                }
            }
        }

        #endregion

        public Microsoft.Practices.Prism.Commands.DelegateCommand CloseCommand
        {
            get { return _closeCommand; }
        }

        public ICommand UpdateCommand
        {
            get
            {
                return _updateCommand ?? (_updateCommand = new CommandHandler(() => UpdateSupplier(), canExecute));
            }
        }
    }
}
