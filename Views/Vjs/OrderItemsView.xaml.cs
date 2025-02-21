using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using A1RConsole.DB;
using A1RConsole.Models.Vjs;
using A1RConsole.ViewModels.Vjs;


namespace A1RConsole.Views.Vjs
{   

    /// <summary>
    /// Interaction logic for OrderItemsView.xaml
    /// </summary>
    public partial class OrderItemsView : UserControl
    {
        public OrderItemsView()
        {
            InitializeComponent();

            DataContext = new OrderItemsViewModel();            
        }
    }


    
}
