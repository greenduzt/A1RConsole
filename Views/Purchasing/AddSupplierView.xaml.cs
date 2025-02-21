using A1RConsole.Models.Stock;
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
    /// Interaction logic for AddSupplierView.xaml
    /// </summary>
    public partial class AddSupplierView : UserControl
    {
        public AddSupplierView(ProductStockMaintenance psm, string un)
        {
            InitializeComponent();
            DataContext = new AddSupplierViewModel(psm, un);
        }
    }
}
