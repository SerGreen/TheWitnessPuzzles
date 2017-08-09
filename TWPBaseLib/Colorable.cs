using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheWitnessPuzzles
{
    interface IColorable
    {
        Color? Color { get; }
        bool HasColor { get; }
    }
}
