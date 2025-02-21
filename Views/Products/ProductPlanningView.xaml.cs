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
    /// Interaction logic for ProductPlanningView.xaml
    /// </summary>
    public partial class ProductPlanningView : UserControl
    {
        public ProductPlanningView()
        {
            InitializeComponent();
            //DataContext = new ProductPlanningViewModel();
            //childWindow.DataContext = ChildWindowManager.Instance;
        }
    }
}
