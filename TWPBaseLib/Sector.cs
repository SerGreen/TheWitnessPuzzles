using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheWitnessPuzzleGenerator
{
    public class Sector
    {
        public List<Block> Blocks { get; set; }

        public Sector(List<Block> blocks)
        {
            Blocks = blocks;
        }

        public override string ToString() => string.Join(" ", Blocks);
    }
}
