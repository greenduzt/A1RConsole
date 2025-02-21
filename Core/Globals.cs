using A1RConsole.Interfaces;
using A1RConsole.Models.Customers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A1RConsole.Core
{
    static class Global
    {
        public static Customer customer;
        public static decimal creditLimit;
        public static decimal creditRemaining;
        public static decimal credit;
        public static string activity;
       // public static ObservableCollection<IContent> GlobalViews;
    }
}
