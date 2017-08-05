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

    // In sector can only be one color of squares
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

    // In sector must be exactly one more object of the same color (it may be other sun or different block of same color)
    // Blocks colored in different color do not matter
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

    // Solution line must pass by triangle block exactly <Power> times (1 to 3)
    public class TriangleRule : BlockRule, ISelfCheckableRule
    {
        public int Power { get; }

        public TriangleRule(Block parentBlock, int power) : base(parentBlock)
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

    public class TetrisRule : BlockRule
    {
        public bool[,] Shape { get; protected set; }
        public bool IsSubtracting { get; }

        public int Width => Shape.GetLength(0);
        public int Height => Shape.GetLength(1);
        public int TotalBlocks { get; }

        public (int X, int Y) TopLeftMost { get
            {
                for (int j = 0; j < Height; j++)
                    for (int i = 0; i < Width; i++)
                        if (Shape[i, j])
                            return (i, j);

                return (-1, -1);
            } }

        public TetrisRule(Block parentBlock, bool[,] shape, bool subtracting = false) : base(parentBlock)
        {
            IsSubtracting = subtracting;
            Shape = shape;

            TotalBlocks = 0;
            for (int i = 0; i < Width; i++)
                for (int j = 0; j < Height; j++)
                    if (shape[i, j])
                        TotalBlocks++;
        }
    }

    public class TetrisRotatableRule : TetrisRule
    {
        public TetrisRotatableRule(Block parentBlock, bool[,] shape, bool subtracting = false) 
            : base(parentBlock, shape, subtracting)
        { }

        public void RotateCW()
        {
            bool[,] newShape = new bool[Height, Width];
            for (int i = 0; i < Width; i++)
                for (int j = Height - 1; j >= 0; j--)
                    newShape[j, i] = Shape[i, j];
            Shape = newShape;
        }
    }
}
