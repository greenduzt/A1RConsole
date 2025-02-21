using A1RConsole.Models.Discounts;
using A1RConsole.Models.Meta;
using A1RConsole.Models.Orders;
using A1RConsole.Models.Users;
using A1RConsole.ViewModels.Orders.SalesOrders;
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

namespace A1RConsole.Views.Orders.SalesOrders
{
    /// <summary>
    /// Interaction logic for SearchProductView.xaml
    /// </summary>
    public partial class SearchProductView : UserControl
    {
        public SearchProductView(string un, string s, List<UserPrivilages> p, List<MetaData> md,  SalesOrder so, List<DiscountStructure> dsList, string openType)
        {
            InitializeComponent();
            DataContext = new SearchProductViewModel(un, s, p, md, so, dsList, openType);
        }
    }
}
