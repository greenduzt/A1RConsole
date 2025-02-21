using A1RConsole.ViewModels.Orders;
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

namespace A1RConsole.Views.Orders
{
    /// <summary>
    /// Interaction logic for OrderStatusView.xaml
    /// </summary>
    public partial class OrderStatusView : UserControl
    {
        public OrderStatusView()
        {
            InitializeComponent();
            //DataContext = new OrderStatusViewModel();
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (radioSearch.IsChecked == false)
            {
                radioSearch.IsChecked = true;
            }
        }
    }
}
