using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using TheWitnessPuzzles;
using System.Linq;

namespace TWP_Shared
{
    public class ReversePanelGenerator : PanelGenerator
    {
        public static ReversePanelGenerator Instance = new ReversePanelGenerator();
        private ReversePanelGenerator() { }

        readonly static Random randomNumberGenerator = new Random();
        readonly static List<Color> colorPalette = new List<Color>() { Color.Aqua, Color.Magenta, Color.Lime, Color.Blue, Color.OrangeRed, Color.Yellow, Color.White, Color.Black, Color.Purple };

        public override Puzzle GeneratePanel(int? seed = null)
        {
            Random rnd = seed.HasValue ? new Random(seed.Value) : randomNumberGenerator;
            Puzzle panel = null;

            // Generate random panel size
            int panelHeight = rnd.Next(2, 8);
            int panelWidth = rnd.Next(Math.Max(2, panelHeight - 2), Math.Min(8, panelHeight + 2));
            bool symmetry = false;
            bool ySymmetry = false;

            // Panel can be symmetric only if it's big enough
            if (panelWidth >= 4 && panelHeight >= 4 && rnd.NextDouble() > 0.3)
            {
                symmetry = true;
                if (rnd.NextDouble() > 0.5)
                    ySymmetry = true;
            }

            panel = symmetry 
                ? new SymmetryPuzzle(panelWidth, panelHeight, ySymmetry) 
                : new Puzzle(panelWidth, panelHeight);

            // Amount of nodes in one row (1 more than width in blocks)
            int width1 = panelWidth + 1;
            // ID of the last node
            int maxNodeID = width1 * (panelHeight + 1) - 1;

            // Generate start points
            List<int> startPoints = new List<int>();
            double num = rnd.NextDouble();
            int startPointsAmount = symmetry 
                ? num > 0.8 ? (num > 0.96 ? 3 : 2) : 1
                : num > 0.65 ? (num > 0.85 ? (num > 0.95 ? 4 : 3) : 2) : 1;
            for (int i = 0; i < startPointsAmount; i++)
            {
                int index;
                bool acceptable;
                do
                {
                    index = rnd.Next(maxNodeID + 1);
                    // Node should not be already marked as start node
                    acceptable = !startPoints.Contains(index);
                    // Node should not be on the line of symmetry
                    acceptable = acceptable && !(symmetry && index == GetMirrorNodeID(index));
                }
                while (!acceptable);
                startPoints.Add(index);
                if (symmetry)
                    startPoints.Add(GetMirrorNodeID(index));
            }

            foreach (int start in startPoints)
                panel.Nodes[start].SetState(NodeState.Start);

            // Generate end points
            List<int> endPoints = new List<int>();
            num = rnd.NextDouble();
            int endPointsAmount = symmetry
                ? num > 0.8 ? (num > 0.98 ? 3 : 2) : 1
                : num > 0.65 ? (num > 0.85 ? (num > 0.95 ? 4 : 3) : 2) : 1;
            List<int> borderNodes = GetBorderNodes();
            for (int i = 0; i < endPointsAmount; i++)
            {
                int index;
                bool acceptable;
                do
                {
                    index = rnd.Next(borderNodes.Count);
                    // Node should not be already marked as start node or end node
                    acceptable = !startPoints.Contains(borderNodes[index]);
                    acceptable = acceptable && !endPoints.Contains(borderNodes[index]);
                    // Node should not be on the line of symmetry
                    acceptable = acceptable && !(symmetry && borderNodes[index] == GetMirrorNodeID(borderNodes[index]));
                }
                while (!acceptable);
                endPoints.Add(borderNodes[index]);
                if (symmetry)
                    endPoints.Add(GetMirrorNodeID(borderNodes[index]));
            }

            foreach (int end in endPoints)
                panel.Nodes[end].SetState(NodeState.Exit);

            int startIndex = startPoints[rnd.Next(startPoints.Count)];
            var allLines = panel.GetAllPossibleLines(panel.Nodes.First(x => x.Id == startIndex));
            // TODO


            return panel;

            #region == Methods ==

            List<int> GetNodeNeighbours(int nodeID)
            {
                List<int> neighbours = new List<int>();
                if (nodeID < width1)
                    neighbours.Add(nodeID + width1);
                if (nodeID >= panelHeight * width1)
                    neighbours.Add(nodeID - width1);
                if(nodeID % width1 == 0)
                    neighbours.Add(nodeID + 1);
                if ((nodeID + 1) % width1 == 0)
                    neighbours.Add(nodeID - 1);
                return neighbours;
            }
            int GetMirrorNodeID(int nodeID)
            {
                if (ySymmetry)
                    return maxNodeID - nodeID;
                else
                    return (nodeID / width1 * width1) * 2 + panelWidth - nodeID;
            }
            List<int> GetBorderNodes()
            {
                List<int> res = new List<int>();
                for (int i = 0; i < width1; i++)
                    for (int j = 0; j < panelHeight+1; j++)
                        if (i == 0 || i == panelWidth || j == 0 || j == panelHeight)
                            res.Add(j * width1 + i);
                return res;
            }

            #endregion
        }
    }
}
