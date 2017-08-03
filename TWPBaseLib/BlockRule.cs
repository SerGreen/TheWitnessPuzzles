using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheWitnessPuzzleGenerator
{
    public interface ISelfCheckableRule
    {
        /// <summary>
        /// Checks whether rule is satisfied
        /// </summary>
        /// <returns>Instance of Error, containing info about the reasons of error
        /// If no error, then returns null</returns>
        Error CheckRule();
    }

    public abstract class BlockRule
    {
        protected Block block;

        public BlockRule(Block parentBlock)
        {
            block = parentBlock;
        }
    }

    public class ColoredSquareRule : BlockRule, ISelfCheckableRule, IColorable
    {
        public ColoredSquareRule(Block parentBlock, Color color) : base(parentBlock)
        {
            Color = color;
        }

        public Color Color { get; }

        public Error CheckRule()
        {
            var blocksWithDifferentColor = block.CurrentSector.Blocks.Where(x => x.Rule is ColoredSquareRule xx && xx.Color != Color);

            if (blocksWithDifferentColor.Count() > 0)
            {
                return new Error(block, blocksWithDifferentColor.Select(x => x as IErrorable).ToList());
            }
            else
                return null;
        }
    }

    public class SunPairRule : BlockRule, ISelfCheckableRule, IColorable
    {
        public SunPairRule(Block parentBlock, Color color) : base(parentBlock)
        {
            Color = color;
        }

        public Color Color { get; }

        public Error CheckRule()
        {
            var blocksWithSameColor = block.CurrentSector.Blocks.Where(x => x.Rule is IColorable xx && xx.Color == Color);

            if (blocksWithSameColor.Count() == 2)
                return null;
            else
            {
                return new Error(block, blocksWithSameColor.Select(x => x as IErrorable).ToList());
            }
        }
    }

    public class DoritoRule : BlockRule, ISelfCheckableRule
    {
        public int Power { get; }

        public DoritoRule(Block parentBlock, int power) : base(parentBlock)
        {
            // Power can be between 1 and 3
            Power = power < 1 ? 1 : power > 3 ? 3 : power;
        }

        public Error CheckRule()
        {
            if (block.Edges.Intersect(block.ParentPanel.SolutionEdges).Count() == Power)
                return null;
            else
                return new Error(block, null);
        }
    }
}
