using A1RConsole.Models.Orders;
using A1RConsole.ViewModels.Invoicing;
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

namespace A1RConsole.Views.Invoicing
{
    /// <summary>
    /// Interaction logic for ChangeInvoiceDateView.xaml
    /// </summary>
    public partial class ChangeInvoiceDateView : UserControl
    {
        public ChangeInvoiceDateView(SalesOrder so)
        {
            InitializeComponent();
            DataContext = new ChangeInvoiceDateViewModel(so);
        }
    }
}
