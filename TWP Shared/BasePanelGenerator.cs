using System;
using System.Collections.Generic;
using System.Text;
using TheWitnessPuzzles;

namespace TWP_Shared
{
    public abstract class PanelGenerator
    {
        public abstract Puzzle GeneratePanel(int? seed = null);
    }
}
