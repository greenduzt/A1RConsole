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
    /// Interaction logic for PrintPurchaseOrderView.xaml
    /// </summary>
    public partial class PrintPurchaseOrderView : UserControl
    {
        public PrintPurchaseOrderView()
        {
            InitializeComponent();
            DataContext = new PrintPurchaseOrderViewModel();
        }
    }
}
