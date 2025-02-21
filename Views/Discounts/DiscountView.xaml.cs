using A1RConsole.Models.Discounts;
using A1RConsole.Models.Orders;
using A1RConsole.ViewModels.Discounts;
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

namespace A1RConsole.Views.Discounts
{
    /// <summary>
    /// Interaction logic for DiscountView.xaml
    /// </summary>
    public partial class DiscountView : UserControl
    {
        public DiscountView(int cusId, ObservableCollection<DiscountStructure> dsL)
        {
            InitializeComponent();
            DataContext = new DiscountViewModel(cusId, dsL);
        }

        //private void txtCommercial_PreviewKeyUp(object sender, KeyEventArgs e)
        //{
        //    if (string.IsNullOrEmpty(txtCommercial.Text))
        //    {
        //        txtCommercial.Text = "0";
        //    }

        //    var textbox = sender as TextBox;
        //    int value;
        //    if (int.TryParse(textbox.Text, out value))
        //    {
        //        if (value > 100)
        //            textbox.Text = "100";
        //        else if (value < 0)
        //            textbox.Text = "0";
        //    }
        //}

        //private void txtFitness_PreviewKeyUp(object sender, KeyEventArgs e)
        //{
        //    if (string.IsNullOrEmpty(txtFitness.Text))
        //    {
        //        txtFitness.Text = "0";
        //    }

        //    var textbox = sender as TextBox;
        //    int value;
        //    if (int.TryParse(textbox.Text, out value))
        //    {
        //        if (value > 100)
        //            textbox.Text = "100";
        //        else if (value < 0)
        //            textbox.Text = "0";
        //    }
        //}

        //private void txtAcoustic_PreviewKeyUp(object sender, KeyEventArgs e)
        //{
        //    if (string.IsNullOrEmpty(txtAcoustic.Text))
        //    {
        //        txtAcoustic.Text = "0";
        //    }

        //    var textbox = sender as TextBox;
        //    int value;
        //    if (int.TryParse(textbox.Text, out value))
        //    {
        //        if (value > 100)
        //            textbox.Text = "100";
        //        else if (value < 0)
        //            textbox.Text = "0";
        //    }
        //}

        //private void txtRecreational_PreviewKeyUp(object sender, KeyEventArgs e)
        //{
        //    if (string.IsNullOrEmpty(txtRecreational.Text))
        //    {
        //        txtRecreational.Text = "0";
        //    }

        //    var textbox = sender as TextBox;
        //    int value;
        //    if (int.TryParse(textbox.Text, out value))
        //    {
        //        if (value > 100)
        //            textbox.Text = "100";
        //        else if (value < 0)
        //            textbox.Text = "0";
        //    }
        //}

        //private void txtSports_PreviewKeyUp(object sender, KeyEventArgs e)
        //{
        //    if (string.IsNullOrEmpty(txtSports.Text))
        //    {
        //        txtSports.Text = "0";
        //    }

        //    var textbox = sender as TextBox;
        //    int value;
        //    if (int.TryParse(textbox.Text, out value))
        //    {
        //        if (value > 100)
        //            textbox.Text = "100";
        //        else if (value < 0)
        //            textbox.Text = "0";
        //    }
        //}

        //private void txtGeneral_PreviewKeyUp(object sender, KeyEventArgs e)
        //{
        //    if (string.IsNullOrEmpty(txtGeneral.Text))
        //    {
        //        txtGeneral.Text = "0";
        //    }

        //    var textbox = sender as TextBox;
        //    int value;
        //    if (int.TryParse(textbox.Text, out value))
        //    {
        //        if (value > 100)
        //            textbox.Text = "100";
        //        else if (value < 0)
        //            textbox.Text = "0";
        //    }
        //}

        //private void txtAdhesives_PreviewKeyUp(object sender, KeyEventArgs e)
        //{
        //    if (string.IsNullOrEmpty(txtAdhesives.Text))
        //    {
        //        txtAdhesives.Text = "0";
        //    }

        //    var textbox = sender as TextBox;
        //    int value;
        //    if (int.TryParse(textbox.Text, out value))
        //    {
        //        if (value > 100)
        //            textbox.Text = "100";
        //        else if (value < 0)
        //            textbox.Text = "0";
        //    }
        //}

        //private void txtAnimal_PreviewKeyUp(object sender, KeyEventArgs e)
        //{
        //    if (string.IsNullOrEmpty(txtAnimal.Text))
        //    {
        //        txtAnimal.Text = "0";
        //    }

        //    var textbox = sender as TextBox;
        //    int value;
        //    if (int.TryParse(textbox.Text, out value))
        //    {
        //        if (value > 100)
        //            textbox.Text = "100";
        //        else if (value < 0)
        //            textbox.Text = "0";
        //    }
        //}
    }
}
