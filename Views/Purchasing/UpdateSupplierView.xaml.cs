using A1RConsole.Models.Stock;
using A1RConsole.Models.Suppliers;
using A1RConsole.ViewModels.Purchasing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace A1RConsole.Views.Purchasing
{
    /// <summary>
    /// Interaction logic for UpdateSupplierView.xaml
    /// </summary>
    public partial class UpdateSupplierView : UserControl
    {
        public UpdateSupplierView(ProductStockMaintenance psm, string un, Supplier sp)
        {
            InitializeComponent();
            DataContext = new UpdateSupplierViewModel(psm, un, sp);
        }
    }
}
