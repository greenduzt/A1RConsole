using A1RConsole.ViewModels.Formulas;
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

namespace A1RConsole.Views.Formulas
{
    /// <summary>
    /// Interaction logic for ShowFormulaView.xaml
    /// </summary>
    public partial class ShowFormulaView : UserControl
    {
        public ShowFormulaView(string formula)
        {
            InitializeComponent();
            DataContext = new ShowFormulaViewModel(formula);
        }
    }
}
