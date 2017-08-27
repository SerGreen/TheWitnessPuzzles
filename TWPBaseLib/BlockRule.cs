using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheWitnessPuzzles
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
        // Owner block is set by the Block itself when we assign rule to it
        public Block OwnerBlock { get; internal set; }
    }

    // Thre're can be squares of only one color in sector
    public class ColoredSquareRule : BlockRule, ISelfCheckableRule, IColorable
    {
        public ColoredSquareRule(Color color)
        {
            Color = color;
        }

        public Color? Color { get; }
        public bool HasColor => Color.HasValue;

        public Error CheckRule()
        {
            // Retrieve eliminated blocks in sector, they must not interfere with check
            var eliminatedBlocks = OwnerBlock.CurrentSector.EliminatedParts.OfType<Block>();

            var blocksWithDifferentColor = OwnerBlock.CurrentSector.Blocks.Where(x => x.Rule is ColoredSquareRule xx && xx.Color != Color).Except(eliminatedBlocks);

            if (blocksWithDifferentColor.Count() > 0)
            {
                return new Error(OwnerBlock);
            }
            else
                return null;
        }
    }

    // In sector must be exactly one more object of the same color (it may be other sun or different block of same color)
    // Blocks colored in different color do not matter
    public class SunPairRule : BlockRule, ISelfCheckableRule, IColorable
    {
        public SunPairRule(Color color)
        {
            Color = color;
        }

        public Color? Color { get; }
        public bool HasColor => Color.HasValue;

        public Error CheckRule()
        {
            // Retrieve eliminated blocks in sector, they must not interfere with check
            var eliminatedBlocks = OwnerBlock.CurrentSector.EliminatedParts.OfType<Block>();

            var blocksWithSameColor = OwnerBlock.CurrentSector.Blocks.Where(x => x.Rule is IColorable xx && xx.HasColor && xx.Color == Color).Except(eliminatedBlocks);

            if (blocksWithSameColor.Count() == 2)
                return null;
            else
            {
                return new Error(OwnerBlock);
            }
        }
    }

    // Solution line must pass by triangle block exactly <Power> times (1 to 3)
    public class TriangleRule : BlockRule, ISelfCheckableRule
    {
        public int Power { get; }

        public TriangleRule(int power)
        {
            // Power can be between 1 and 3
            Power = power < 1 ? 1 : power > 3 ? 3 : power;
        }

        public Error CheckRule()
        {
            if (OwnerBlock.Edges.Intersect(OwnerBlock.ParentPanel.SolutionEdges).Count() == Power)
                return null;
            else
                return new Error(OwnerBlock);
        }
    }

    public class TetrisRule : BlockRule, IColorable
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

        public Color? Color { get; }
        public bool HasColor => Color.HasValue;

        public TetrisRule(bool[,] shape, bool subtractive = false, Color? color = null)
        {
            IsSubtractive = subtractive;
            Color = color;
            Shape = new bool[shape.GetLength(0), shape.GetLength(1)];
            for (int i = 0; i < Width; i++)
                for (int j = 0; j < Height; j++)
                    Shape[i, j] = shape[i, j];

            _totalBlocks = 0;
            for (int i = 0; i < Width; i++)
                for (int j = 0; j < Height; j++)
                    if (Shape[i, j])
                        _totalBlocks++;
        }
    }

    public class TetrisRotatableRule : TetrisRule
    {
        public TetrisRotatableRule(bool[,] shape, bool subtractive = false, Color? color = null) 
            : base(shape, subtractive, color)
        { }

        public TetrisRotatableRule RotateCW()
        {
            bool[,] newShape = RotateShapeCW(Shape);

            var clone = this.MemberwiseClone() as TetrisRotatableRule;
            clone.Shape = newShape;
            return clone;
        }

        public static bool[,] RotateShapeCW(bool[,] originalShape, int times = 1)
        {
            times %= 4;
            if (times == 0)
                return originalShape;

            int w = originalShape.GetLength(0);
            int h = originalShape.GetLength(1);
            bool[,] newShape = new bool[h, w];
            for (int i = 0; i < w; i++)
                for (int j = h - 1; j >= 0; j--)
                    newShape[h - j - 1, i] = originalShape[i, j];

            return RotateShapeCW(newShape, times - 1);
        }

        public static bool AreShapesIdentical(bool[,] shapeA, bool[,] shapeB)
        {
            if (shapeA.GetLength(0) != shapeB.GetLength(0) || shapeA.GetLength(1) != shapeB.GetLength(1))
                return false;

            for (int x = 0; x < shapeA.GetLength(0); x++)
                for (int y = 0; y < shapeA.GetLength(1); y++)
                    if (shapeA[x, y] != shapeB[x, y])
                        return false;
            return true;
        }
    }

    public class EliminationRule : BlockRule, IColorable
    {
        public EliminationRule(Color? color = null)
        {
            Color = color;
        }

        public Color? Color { get; }
        public bool HasColor => Color.HasValue;
    }
}
