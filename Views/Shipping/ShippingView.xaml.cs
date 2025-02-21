﻿using A1RConsole.ViewModels.Shipping;
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
    /// Interaction logic for ShippingView.xaml
    /// </summary>
    public partial class ShippingView : UserControl
    {
        public ShippingView()
        {
            InitializeComponent();
            DataContext = new ShippingViewModel();
        }
    }
}
