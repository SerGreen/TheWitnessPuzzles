using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheWitnessPuzzles;
using static System.Math;

namespace TWP_Shared
{
    public class PanelGenerator
    {
        readonly static Random randomNumberGenerator = new Random();
        readonly static List<Color> colorPalette = new List<Color>() { Color.Aqua, Color.Magenta, Color.Lime, Color.Blue, Color.Red, Color.Yellow, Color.White, Color.Black, Color.Violet };

        public static Puzzle GeneratePanel(int? seed = null)
        {
            Random rnd = seed.HasValue ? new Random(seed.Value) : randomNumberGenerator;
            Puzzle panel = null;

            do
            {
                panel = GenerateUnsafePanel(rnd);
                return panel;
            }
            while (panel.GetAllSolutions().Count() == 0);

            return panel;
        }

        private static Puzzle GenerateUnsafePanel(Random rnd)
        {
            int panelDifficulty = 0;

            int height = rnd.Next(2, 8);
            int width = rnd.Next(Max(2, height - 2), Min(8, height + 2));
            bool isSymmetric = rnd.NextDouble() > 0.5;
            bool isY_mirrored = rnd.NextDouble() > 0.5;

            panelDifficulty += (height + width - 4) / 3;
            panelDifficulty += isSymmetric ? 1 : 0;

            Puzzle panel;
            if (isSymmetric)
                panel = new SymmetryPuzzle(width, height, isY_mirrored, Color.Aqua, Color.Yellow);
            else
                panel = new Puzzle(width, height);

            double num;

            // Generate start points
            num = rnd.NextDouble();
            int startPointsAmount = num > 0.7 ? (num > 0.85 ? (num > 0.95 ? 4 : 3) : 2) : 1;
            for (int i = 0; i < startPointsAmount; i++)
            {
                int x = rnd.Next(0, panel.Width + 1);
                int y = rnd.Next(0, panel.Height + 1);
                int id = y * (panel.Width + 1) + x;
                panel.Nodes[id].SetState(NodeState.Start);
                if(panel is SymmetryPuzzle sym)
                    panel.Nodes[sym.GetMirrorNode(id).Id].SetState(NodeState.Start);
            }
            panelDifficulty += startPointsAmount > 2 ? 1 : 0;

            // Generate exit points
            num = rnd.NextDouble();
            int endPointsAmount = num > 0.6 ? (num > 0.75 ? (num > 0.9 ? 4 : 3) : 2) : 1;
            var borderLine = panel.BorderNodes.ToArray();
            for (int i = 0; i < endPointsAmount; i++)
            {
                int index = rnd.Next(borderLine.Length);
                if(borderLine[index].State == NodeState.Start)
                {
                    i--;
                    continue;
                }

                borderLine[index].SetState(NodeState.Exit);
                if (panel is SymmetryPuzzle sym)
                    panel.Nodes[sym.GetMirrorNode(borderLine[index].Id).Id].SetState(NodeState.Exit);
            }
            panelDifficulty += endPointsAmount > 3 ? 1 : 0;

            // Generate broken edges
            num = rnd.NextDouble();
            int amountOfBrokenEdges = num > 0.6 ? (num > 0.8 ? (num > 0.87 ? (num > 0.92 ? (num > 0.96 ? 5 : 4) : 3) : 2) : 1) : 0;
            for (int i = 0; i < amountOfBrokenEdges; i++)
            {
                int index = rnd.Next(panel.Edges.Count);
                panel.Edges[index].SetState(EdgeState.Broken);
            }
            panelDifficulty += amountOfBrokenEdges > 2 ? 1 : 0;

            // Generate colored squares
            num = rnd.NextDouble();
            int colorsAmount = num > 0.8 ? (num > 0.93 ? 4 : 3) : 2;
            List<Color> colors = new List<Color>();
            for (int i = 0; i < colorsAmount; i++)
                colors.Add(colorPalette[rnd.Next(colorPalette.Count)]);
            panelDifficulty += colorsAmount - 2;

            num = rnd.NextDouble();
            int squaresAmount;
            if (panelDifficulty > 3)
                squaresAmount = num > 0.3 ? (num > 0.6 ? (num > 0.85 ? 5 : 4) : 3) : 2;
            else
                squaresAmount = num > 0.4 ? (num > 0.6 ? (num > 0.75 ? (num > 0.9 ? 7 : 6) : 5) : 4) : 3;

            var blocks = panel.Blocks.ToArray();
            for (int i = 0; i < squaresAmount; i++)
            {
                int index = rnd.Next(blocks.Length);
                Color col = colors[rnd.Next(colors.Count)];
                blocks[index].Rule = new ColoredSquareRule(col);
            }

            panelDifficulty += (int) ((squaresAmount - 2) / 1.5f);

            // Generate suns
            num = rnd.NextDouble();
            int sunsAmount;
            if (width * height > 20)
                sunsAmount = num > 0.4 ? (num > 0.6 ? (num > 0.7 ? (num > 0.94 ? 7 : 5) : 4) : 3) : 2; 
            else
                sunsAmount = num > 0.3 ? (num > 0.7 ? (num > 0.9 ? 4 : 3) : 2) : 1;

            if (panelDifficulty > 6)
                sunsAmount -= 1;
            
            for (int i = 0; i < squaresAmount; i++)
            {
                int index = rnd.Next(blocks.Length);
                Color col = colors[rnd.Next(colors.Count)];
                blocks[index].Rule = new SunPairRule(col);
            }

            var squareRules = blocks.Where(x => x.Rule is ColoredSquareRule).Select(x => x.Rule as ColoredSquareRule);
            var sunRules = blocks.Where(x => x.Rule is SunPairRule).Select(x => x.Rule as SunPairRule);

            foreach (Color color in colors)
            {
                int count = squareRules.Where(x => x.Color == color).Count() + sunRules.Where(x => x.Color == color).Count();
                if (count % 2 != 0 && sunRules.Where(x => x.Color == color).Count() > 0)
                    blocks.First(x => x.Rule is SunPairRule sun && sun.Color == color).Rule = null;
            }

            return panel;
        }
    }
}
