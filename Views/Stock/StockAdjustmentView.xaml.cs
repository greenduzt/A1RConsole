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
    /// Interaction logic for StockAdjustmentView.xaml
    /// </summary>
    public partial class StockAdjustmentView : UserControl
    {
        public StockAdjustmentView(StockAdjustmentViewModel rc)
        {
            InitializeComponent();
            DataContext = new StockAdjustmentViewModel(rc);
        }

        public StockAdjustmentView()
        {
            InitializeComponent();
            //DataContext = new StockAdjustmentViewModel();
        }
    }
}
