using A1RConsole.Models.Stock;
using A1RConsole.Models.Suppliers;
using A1RConsole.ViewModels.Products;
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

namespace A1RConsole.Views.Products
{
    /// <summary>
    /// Interaction logic for UpdateProductView.xaml
    /// </summary>
    public partial class UpdateProductView : UserControl
    {
        public UpdateProductView(ProductStockMaintenance stmvm, string un, bool b, Supplier s)
        {
            InitializeComponent();
            DataContext = new UpdateProductViewModel(stmvm, un, b, s);
        }
    }
}
