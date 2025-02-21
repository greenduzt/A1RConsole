using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A1RConsole.Interfaces
{
    public interface IContent
    {
        string Title { get; }
        bool CanClose { get; }
    }
}
