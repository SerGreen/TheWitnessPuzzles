using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using System.Linq;
using TheWitnessPuzzles;

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
            SymmetryPuzzle symPanel = null;

            Color mainColor = Color.Black, mirrorColor = Color.Black;

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
                ? new SymmetryPuzzle(panelWidth, panelHeight, ySymmetry, Color.Aqua, Color.Yellow) 
                : new Puzzle(panelWidth, panelHeight);
            if (symmetry)
            {
                symPanel = panel as SymmetryPuzzle;
                mainColor = symPanel.MainColor;
                mirrorColor = symPanel.MirrorColor;
            }

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

            int startPoint = startPoints[rnd.Next(startPoints.Count)];
            int endPoint = endPoints[rnd.Next(endPoints.Count)];
            
            // If panel is X-Symmetric, we can't cross the line of symmetry
            // Therefor we should check that our start and end nodes are on the same side
            if(symmetry && !ySymmetry)
            {
                float lineOfSymmetry = panelWidth / 2f;
                int startNodeX = startPoint % width1;
                int endNodeX = endPoint % width1;
                // If the start node and the end node are lying on the different sides across the line of symmetry => swap the end node to the mirror one
                if (Math.Sign(lineOfSymmetry - startNodeX) != Math.Sign(lineOfSymmetry - endNodeX))
                    endPoint = GetMirrorNodeID(endPoint);
            }

            List<int> randomSolution = GetRandomSolutionLine(startPoint, endPoint);
            panel.SetSolution(randomSolution);

            num = rnd.NextDouble();
            int amountOfHexagonMarks = num > 0.6 ? (num > 0.65 ? (num > 0.85 ? (num > 0.92 ? (num > 0.97 ? 6 : 4) : 3) : 2) : 1) : 0;
            int amountOfMarkedNodes = (int) (amountOfHexagonMarks * (0.3 + 0.7 * rnd.NextDouble()));
            int amountOfMarkedEdges = amountOfHexagonMarks - amountOfMarkedNodes;

            var allSolutionNodes = panel.SolutionNodes.ToList();
            amountOfMarkedNodes = Math.Min(amountOfMarkedNodes, allSolutionNodes.Count);
            var mainsolutionNodes = symPanel?.MainSolutionNodes;
            var mirSolutionNodes = symPanel?.MirrorSolutionNodes;
            for (int i = 0; i < amountOfMarkedNodes; i++)
            {
                int index;
                bool acceptable;
                do
                {
                    // Node should be Normal state
                    index = rnd.Next(allSolutionNodes.Count);
                    acceptable = allSolutionNodes[index].State == NodeState.Normal;
                }
                while (!acceptable);
                if (symmetry)
                {
                    bool applyColor = rnd.NextDouble() > 0.65;
                    if (applyColor)
                        if (mainsolutionNodes.Contains(allSolutionNodes[index]))
                            allSolutionNodes[index].SetStateAndColor(NodeState.Marked, mainColor);
                        else
                            allSolutionNodes[index].SetStateAndColor(NodeState.Marked, mirrorColor);
                    else
                        allSolutionNodes[index].SetState(NodeState.Marked);
                }
                else
                    allSolutionNodes[index].SetState(NodeState.Marked);
            }

            var allSolutionEdges = panel.SolutionEdges.ToList();
            amountOfMarkedEdges = Math.Min(amountOfMarkedEdges, allSolutionEdges.Count);
            var mainsolutionEdges = symPanel?.MainSolutionEdges;
            var mirSolutionEdges = symPanel?.MirrorSolutionEdges;
            for (int i = 0; i < amountOfMarkedEdges; i++)
            {
                int index;
                bool acceptable;
                do
                {
                    // Edge should not be Marked or Broken
                    index = rnd.Next(allSolutionEdges.Count);
                    acceptable = allSolutionEdges[index].State == EdgeState.Normal;
                }
                while (!acceptable);
                if (symmetry)
                {
                    bool applyColor = rnd.NextDouble() > 0.65;
                    if (applyColor)
                        if (mainsolutionEdges.Contains(allSolutionEdges[index]))
                            allSolutionEdges[index].SetStateAndColor(EdgeState.Marked, mainColor);
                        else
                            allSolutionEdges[index].SetStateAndColor(EdgeState.Marked, mirrorColor);
                    else
                        allSolutionEdges[index].SetState(EdgeState.Marked);
                }
                else
                    allSolutionEdges[index].SetState(EdgeState.Marked);
            }

            return panel;

            #region == Methods ==

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

            List<int> GetRandomSolutionLine(int startNode, int endNode)
            {
                // This is the solution line
                List<int> line = new List<int>();
                // This contains nodes from mirror line if present
                List<int> mirrorLine = new List<int>();
                // Alternative neighbour nodes of [i] node
                List<List<int>> forkRoad = new List<List<int>>();

                AddNodeToSolution(startNode);

                // Loop until we get our line to the end node
                while (line[line.Count - 1] != endNode)
                {
                    int last = line[line.Count - 1];

                    // If the last node has forks, it means that we were here before and returned back to this node
                    if (forkRoad.Count == line.Count)
                    {
                        // If there are available forks => pick one randomly and try it
                        if (forkRoad[forkRoad.Count - 1].Count > 0)
                        {
                            int nextNodeIndex = rnd.Next(forkRoad[forkRoad.Count - 1].Count);
                            AddNodeToSolution(forkRoad[forkRoad.Count - 1][nextNodeIndex]);
                            forkRoad[forkRoad.Count - 1].RemoveAt(nextNodeIndex);
                            continue;
                        }
                        // If we've tried all the forks already => delete last node and its forks and go back to the previous node
                        else
                        {
                            RemoveLastNodeFromSolution();
                            forkRoad.RemoveAt(forkRoad.Count - 1);
                            continue;
                        }
                    }
                    // If the last node does not have forks yet => we're first time here
                    else
                    {
                        // Get all nodes we can go to from current last node
                        var neighbours = GetAllViableNeighbours(last);

                        // If we have options to move
                        if (neighbours.Count > 0)
                        {
                            // Randomly choose the next node
                            int nextNodeIndex = rnd.Next(neighbours.Count);
                            // Add it to the solution line
                            AddNodeToSolution(neighbours[nextNodeIndex]);
                            // Remove it from other neighbours and add them to the fork roads
                            neighbours.RemoveAt(nextNodeIndex);
                            forkRoad.Add(neighbours);
                            continue;
                        }
                        // If we are in dead end
                        else
                        {
                            // Delete the last node from the solution and move on to try other forks of the previous node
                            RemoveLastNodeFromSolution();
                            continue;
                        }
                    }
                }

                return line;

                List<int> GetAllViableNeighbours(int nodeID)
                {
                    IEnumerable<int> neighbours = GetNodeNeighbours(nodeID);
                    neighbours = neighbours.Except(line).Except(mirrorLine);
                    if (symmetry)
                        neighbours = neighbours.Where(x => x != GetMirrorNodeID(x));
                    return neighbours.ToList();
                }
                List<int> GetNodeNeighbours(int nodeID)
                {
                    List<int> neighbours = new List<int>();
                    if (nodeID > panelWidth)
                        neighbours.Add(nodeID - width1);
                    if (nodeID < panelHeight * width1)
                        neighbours.Add(nodeID + width1);
                    if (nodeID % width1 != 0)
                        neighbours.Add(nodeID - 1);
                    if ((nodeID + 1) % width1 != 0)
                        neighbours.Add(nodeID + 1);
                    return neighbours;
                }
                void AddNodeToSolution(int nodeID)
                {
                    line.Add(nodeID);
                    if (symmetry)
                        mirrorLine.Add(GetMirrorNodeID(nodeID));
                }
                void RemoveLastNodeFromSolution()
                {
                    line.RemoveAt(line.Count - 1);
                    if (symmetry)
                        mirrorLine.RemoveAt(mirrorLine.Count - 1);
                }
            }

            #endregion
        }
    }
}
