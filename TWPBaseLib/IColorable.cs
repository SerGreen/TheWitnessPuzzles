using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheWitnessPuzzles
{
    public interface IColorable
    {
        Color? Color { get; }
        bool HasColor { get; }
    }
}
