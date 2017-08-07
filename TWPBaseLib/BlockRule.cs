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
        public Block OwnerBlock { get; }

        public BlockRule(Block parentBlock)
        {
            OwnerBlock = parentBlock;
        }
    }

    // Thre're can be squares of only one color in sector
    public class ColoredSquareRule : BlockRule, ISelfCheckableRule, IColorable
    {
        public ColoredSquareRule(Block parentBlock, Color color) : base(parentBlock)
        {
            Color = color;
        }

        public Color Color { get; }

        public Error CheckRule()
        {
            // Retrieve eliminated blocks in sector, they must not interfere with check
            var eliminatedBlocks = OwnerBlock.CurrentSector.EliminatedParts.OfType<Block>();

            var blocksWithDifferentColor = OwnerBlock.CurrentSector.Blocks.Where(x => x.Rule is ColoredSquareRule xx && xx.Color != Color).Except(eliminatedBlocks);

            if (blocksWithDifferentColor.Count() > 0)
            {
                return new Error(OwnerBlock, blocksWithDifferentColor.Select(x => x as IErrorable).ToList());
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
            // Retrieve eliminated blocks in sector, they must not interfere with check
            var eliminatedBlocks = OwnerBlock.CurrentSector.EliminatedParts.OfType<Block>();

            var blocksWithSameColor = OwnerBlock.CurrentSector.Blocks.Where(x => x.Rule is IColorable xx && xx.Color == Color).Except(eliminatedBlocks);

            if (blocksWithSameColor.Count() == 2)
                return null;
            else
            {
                return new Error(OwnerBlock, blocksWithSameColor.Select(x => x as IErrorable).ToList());
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
            if (OwnerBlock.Edges.Intersect(OwnerBlock.ParentPanel.SolutionEdges).Count() == Power)
                return null;
            else
                return new Error(OwnerBlock, null);
        }
    }

    public class TetrisRule : BlockRule
    {
        public bool[,] Shape { get; protected set; }
        public bool IsSubtractive { get; }

        public int Width => Shape.GetLength(0);
        public int Height => Shape.GetLength(1);
        private int _totalBlocks;
        public int TotalBlocks { get => IsSubtractive ? -_totalBlocks : _totalBlocks; }

        public (int X, int Y) TopLeftMost
        {
            get
            {
                for (int j = 0; j < Height; j++)
                    for (int i = 0; i < Width; i++)
                        if (Shape[i, j])
                            return (i, j);

                return (-1, -1);
            }
        }

        public TetrisRule(Block parentBlock, bool[,] shape, bool subtracting = false) : base(parentBlock)
        {
            IsSubtractive = subtracting;
            Shape = new bool[shape.GetLength(1), shape.GetLength(0)];
            for (int i = 0; i < Width; i++)
                for (int j = 0; j < Height; j++)
                    Shape[i, j] = shape[j, i];

            _totalBlocks = 0;
            for (int i = 0; i < Width; i++)
                for (int j = 0; j < Height; j++)
                    if (Shape[i, j])
                        _totalBlocks++;
        }
    }

    public class TetrisRotatableRule : TetrisRule
    {
        public TetrisRotatableRule(Block parentBlock, bool[,] shape, bool subtracting = false) 
            : base(parentBlock, shape, subtracting)
        { }

        public TetrisRotatableRule RotateCW()
        {
            bool[,] newShape = new bool[Height, Width];
            for (int i = 0; i < Width; i++)
                for (int j = Height - 1; j >= 0; j--)
                    newShape[Height - j - 1, i] = Shape[i, j];

            var clone = this.MemberwiseClone() as TetrisRotatableRule;
            clone.Shape = newShape;
            return clone;
        }
    }

    public class EliminationRule : BlockRule
    {
        public EliminationRule(Block parentBlock) : base(parentBlock)
        { }
    }
}
