using A1RConsole.Models.Customers;
using A1RConsole.ViewModels.Customers;
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

namespace A1RConsole.Views.Customers
{
    /// <summary>
    /// Interaction logic for AddUpdateContactPersonView.xaml
    /// </summary>
    public partial class AddUpdateContactPersonView : UserControl
    {
        public AddUpdateContactPersonView(ContactPerson cp)
        {
            InitializeComponent();
            DataContext = new AddUpdateContactPersonViewModel(cp);
        }
    }
}
