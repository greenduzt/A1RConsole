using A1RConsole.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A1RConsole.Models
{
    public class Pet : ViewModelBase
    {
        public Pet(string name, string owner)
        {
            this.Name = name;
            this.Owner = owner;
        }

        public string Name { get; set; }
        public string Owner { get; set; }
    }
}
