using A1RConsole.Models.Dispatch;
using A1RConsole.ViewModels.Shipping;
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

namespace A1RConsole.Views.Shipping
{
    /// <summary>
    /// Interaction logic for DispatchConfirmationView.xaml
    /// </summary>
    public partial class DispatchConfirmationView : UserControl
    {
        public DispatchConfirmationView(DispatchOrder dis, List<Tuple<string, Int16, string>> timeStamps)
        {
            InitializeComponent();
            DataContext = new DispatchConfirmationViewModel(dis, timeStamps);
        }
    }
}
