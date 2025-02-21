using A1RConsole.ViewModels.Stock;
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

namespace A1RConsole.Views.Stock
{
    /// <summary>
    /// Interaction logic for InventoryValueReportView.xaml
    /// </summary>
    public partial class InventoryValueReportView : UserControl
    {
        public InventoryValueReportView()
        {
            InitializeComponent();
            DataContext = new InventoryValueReportViewModel();
        }
    }
}
