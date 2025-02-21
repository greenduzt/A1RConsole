using A1RConsole.ViewModels.Orders.NewOrderPDF;
using A1RConsole.ViewModels.Quoting;
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

namespace A1RConsole.Views.Quoting
{
    /// <summary>
    /// Interaction logic for ViewUpdateQuote.xaml
    /// </summary>
    public partial class ViewUpdateQuoteView : UserControl
    {
        public ViewUpdateQuoteView()
        {
            InitializeComponent();
        }

        private void txtSearchQuoteNumber_KeyUp(object sender, KeyEventArgs e)
        {
            //txtSearchProjectName.Text = "";
            //txtSearchCustomer.Text = "";
        }

        private void txtSearchProjectName_KeyUp(object sender, KeyEventArgs e)
        {
            //txtSearchQuoteNumber.Text = "";
            //txtSearchCustomer.Text = "";
        }

        private void txtSearchCustomer_KeyUp(object sender, KeyEventArgs e)
        {
            //txtSearchProjectName.Text = "";
            //txtSearchQuoteNumber.Text = "";
        }

        private void DataGrid_CellGotFocus(object sender, RoutedEventArgs e)
        {
            // Lookup for the source to be DataGridCell
            if (e.OriginalSource.GetType() == typeof(DataGridCell))
            {
                // Starts the Edit on the row;
                DataGrid grd = (DataGrid)sender;
                grd.BeginEdit(e);

                Control control = GetFirstChildByType<Control>(e.OriginalSource as DataGridCell);
                if (control != null)
                {
                    control.Focus();
                }
            }
        }
                

        private T GetFirstChildByType<T>(DependencyObject prop) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(prop); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild((prop), i) as DependencyObject;
                if (child == null)
                    continue;

                T castedProp = child as T;
                if (castedProp != null)
                    return castedProp;

                castedProp = GetFirstChildByType<T>(child);

                if (castedProp != null)
                    return castedProp;
            }
            return null;
        }

        private void FreightCode_Datagrid_CellGotFocus(object sender, RoutedEventArgs e)
        {
            // Lookup for the source to be DataGridCell
            if (e.OriginalSource.GetType() == typeof(DataGridCell))
            {
                // Starts the Edit on the row;
                DataGrid grd = (DataGrid)sender;
                grd.BeginEdit(e);

                Control control = GetFirstChildByType<Control>(e.OriginalSource as DataGridCell);
                if (control != null)
                {
                    control.Focus();
                }
            }
        }
    }
}
