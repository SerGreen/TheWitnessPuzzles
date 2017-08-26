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
            double num = rnd.NextDouble();
            int panelHeight = num > 0.05 ? (num > 0.3 ? (num > 0.75 ? (num > 0.9 ? (num > 0.95 ? 7 : 6) : 5) : 4) : 3) : 2;
            num = rnd.NextDouble();
            int panelWidth = num > 0.05 ? (num > 0.3 ? (num > 0.75 ? (num > 0.9 ? (num > 0.95 ? 7 : 6) : 5) : 4) : 3) : 2;
            bool symmetry = false;
            bool ySymmetry = false;

            // Panel can be symmetric only if it's big enough
            #region Symmetry
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
            #endregion

            // Amount of nodes in one row (1 more than width in blocks)
            int width1 = panelWidth + 1;
            // ID of the last node
            int maxNodeID = width1 * (panelHeight + 1) - 1;

            // Generate start points
            #region Start points
            List<int> startPoints = new List<int>();
            num = rnd.NextDouble();
            int startPointsAmount = symmetry
                ? num > 0.8 ? (num > 0.97 ? 3 : 2) : 1
                : num > 0.7 ? (num > 0.89 ? (num > 0.98 ? 4 : 3) : 2) : 1;
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
            #endregion

            // Generate end points
            #region End points
            List<int> endPoints = new List<int>();
            num = rnd.NextDouble();
            int endPointsAmount = symmetry
                ? num > 0.88 ? 2 : 1
                : num > 0.75 ? (num > 0.91 ? (num > 0.99 ? 4 : 3) : 2) : 1;
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
            #endregion

            int startPoint = startPoints[rnd.Next(startPoints.Count)];
            int endPoint = endPoints[rnd.Next(endPoints.Count)];

            // If panel is X-Symmetric, we can't cross the line of symmetry
            // Therefore we should check that our start and end nodes are on the same side
            if (symmetry && !ySymmetry)
            {
                float lineOfSymmetry = panelWidth / 2f;
                int startNodeX = startPoint % width1;
                int endNodeX = endPoint % width1;
                // If the start node and the end node are lying on the different sides across the line of symmetry => swap the end node to the mirror one
                if (Math.Sign(lineOfSymmetry - startNodeX) != Math.Sign(lineOfSymmetry - endNodeX))
                    endPoint = GetMirrorNodeID(endPoint);
            }

            // Generate random line on the panel
            List<int> randomSolution = GetRandomSolutionLine(startPoint, endPoint);
            panel.SetSolution(randomSolution);

            var allSolutionNodes = panel.SolutionNodes.ToList();
            var allSolutionEdges = panel.SolutionEdges.ToList();

            // Generate amount of hexagon rules on the panel
            #region Hexagon rule
            num = rnd.NextDouble();
            int amountOfHexagonMarks = num > 0.6 ? (num > 0.65 ? (num > 0.85 ? (num > 0.92 ? (num > 0.97 ? 6 : 4) : 3) : 2) : 1) : 0;
            // Can't be more than 40% of total amount of nodes/edges
            amountOfHexagonMarks = Math.Min(amountOfHexagonMarks, (int) ((allSolutionNodes.Count + allSolutionEdges.Count) * 0.4f));
            int amountOfMarkedNodes = Math.Min((int) (amountOfHexagonMarks * (0.3 + 0.7 * rnd.NextDouble())), (int) (allSolutionNodes.Count * 0.4f));
            int amountOfMarkedEdges = Math.Min(amountOfHexagonMarks - amountOfMarkedNodes, (int) (allSolutionEdges.Count * 0.4f));

            // Spawn hexagons on nodes
            #region Marked nodes
            var mainsolutionNodes = symPanel?.MainSolutionNodes;
            var mirSolutionNodes = symPanel?.MirrorSolutionNodes;
            for (int i = 0; i < amountOfMarkedNodes; i++)
            {
                var acceptableNodes = allSolutionNodes.Where(x => x.State == NodeState.Normal).ToList();
                if (acceptableNodes.Count > 0)
                {
                    int index = rnd.Next(acceptableNodes.Count);
                    if (symmetry)
                    {
                        bool applyColor = rnd.NextDouble() > 0.65;
                        if (applyColor)
                            if (mainsolutionNodes.Contains(acceptableNodes[index]))
                                acceptableNodes[index].SetStateAndColor(NodeState.Marked, mainColor);
                            else
                                acceptableNodes[index].SetStateAndColor(NodeState.Marked, mirrorColor);
                        else
                            acceptableNodes[index].SetState(NodeState.Marked);
                    }
                    else
                        acceptableNodes[index].SetState(NodeState.Marked);
                }
            }
            #endregion

            // Spawn hexagons on edges
            #region Marked edges
            var mainsolutionEdges = symPanel?.MainSolutionEdges;
            var mirSolutionEdges = symPanel?.MirrorSolutionEdges;
            for (int i = 0; i < amountOfMarkedEdges; i++)
            {
                int index;
                // Edge should not be Marked or Broken and both its nodes should not be marked too
                var acceptableEdges = allSolutionEdges.Where(x => x.State == EdgeState.Normal && x.NodeA.State == NodeState.Normal && x.NodeB.State == NodeState.Normal).ToList();
                if (acceptableEdges.Count == 0)
                    break;
                index = rnd.Next(acceptableEdges.Count);
                if (symmetry)
                {
                    bool applyColor = rnd.NextDouble() > 0.65;
                    if (applyColor)
                        if (mainsolutionEdges.Contains(acceptableEdges[index]))
                            acceptableEdges[index].SetStateAndColor(EdgeState.Marked, mainColor);
                        else
                            acceptableEdges[index].SetStateAndColor(EdgeState.Marked, mirrorColor);
                    else
                        acceptableEdges[index].SetState(EdgeState.Marked);
                }
                else
                    acceptableEdges[index].SetState(EdgeState.Marked);
            }
            #endregion
            #endregion

            // Generate amount of broken edges
            #region Broken edges
            num = rnd.NextDouble();
            int amountOfBrokenEdges;
            // 0 for small panels and 0..6 for bigger panels
            amountOfBrokenEdges = panelWidth >= 3 && panelHeight >= 3
                                ? panelWidth >= 5 || panelHeight >= 5
                ? num > 0.3 ? (num > 0.45 ? (num > 0.65 ? (num > 0.77 ? (num > 0.87 ? (num > 0.94 ? 6 : 5) : 4) : 3) : 2) : 1) : 0
                : num > 0.5 ? (num > 0.7 ? (num > 0.85 ? (num > 0.95 ? 4 : 3) : 2) : 1) : 0
                : 0;

            // Spawn broken edges
            for (int i = 0; i < amountOfBrokenEdges; i++)
            {
                // Edge should not be Marked or already Broken
                var allBreakableEdges = panel.Edges.Except(panel.SolutionEdges).Where(x => x.State == EdgeState.Normal).ToList();
                int index = rnd.Next(allBreakableEdges.Count);
                allBreakableEdges[index].SetState(EdgeState.Broken);
            }
            #endregion

            float usedRuleTypesCount = 0;
            if (amountOfMarkedNodes + amountOfMarkedEdges > 0)
                usedRuleTypesCount++;
            if (amountOfBrokenEdges > 0)
                usedRuleTypesCount++;

            List<Color> colors = new List<Color>();
            num = rnd.NextDouble();
            int amountOfColors = num > 0.6 ? (num > 0.8 ? (num > 0.94 ? 5 : 4) : 3) : 2;
            for (int i = 0; i < amountOfColors; i++)
            {
                int index = -1;
                bool acceptable = false;
                while (!acceptable)
                {
                    index = rnd.Next(colorPalette.Count);
                    acceptable = !colors.Contains(colorPalette[index]);
                }
                colors.Add(colorPalette[index]);
            }

            List<Sector> sectors = panel.GetSectors();

            // Colored square rule
            #region Colored Squares
            num = rnd.NextDouble();
            int amountOfColoredSquares = num > 0.5 ? (num > 0.55 ? (num > 0.62 ? (num > 76 ? (num > 86 ? (num > 92 ? (num > 96 ? 8 : 7) : 6) : 5) : 4) : 3) : 2) : 0;
            if (amountOfColoredSquares > 0)
            {
                List<(int amount, Color color)> squaresInSector = new List<(int, Color)>();
                int squaresLeft = amountOfColoredSquares;
                int colorsUsed = 0;
                for (int i = 0; i < sectors.Count; i++)
                {
                    int amount = squaresLeft > 0
                        ? i < sectors.Count - 1
                            ? Math.Min(rnd.Next(1, squaresLeft), sectors[i].Blocks.Count)
                            : Math.Min(squaresLeft, sectors[i].Blocks.Count)
                        : 0;
                    squaresLeft -= amount;

                    Color col = colors.Count == colorsUsed
                        ? colors[rnd.Next(colors.Count)]
                        : colors[colorsUsed++];

                    squaresInSector.Add((amount, col));
                }

                // Spawn colored squares
                for (int i = 0; i < sectors.Count; i++)
                {
                    for (int j = 0; j < squaresInSector[i].amount; j++)
                    {
                        int index;
                        bool acceptable;
                        do
                        {
                            index = rnd.Next(sectors[i].Blocks.Count);
                            acceptable = sectors[i].Blocks[index].Rule == null;
                        }
                        while (!acceptable);
                        sectors[i].Blocks[index].Rule = new ColoredSquareRule(squaresInSector[i].color);
                    }
                }
            }
            #endregion
            usedRuleTypesCount += amountOfColoredSquares > 0 ? 1 : 0;


            //  Sun rules
            for (int i = 0; i < sectors.Count; i++)
            {
                var freeBlocks = sectors[i].Blocks.Where(x => x.Rule == null).ToList();
                var coloredBlocks = sectors[i].Blocks.Where(x => x.Rule is IColorable).ToList();

                if (((freeBlocks.Count > 0  && coloredBlocks.Count ==  1)  || (freeBlocks.Count  > 1))  &&  rnd.NextDouble() > 0.5)
                {
                    Color? blocksColor = null;
                    if (coloredBlocks.Count > 0)
                        blocksColor = (coloredBlocks[0].Rule as IColorable).Color;

                    if (coloredBlocks.Count == 1)
                    {
                        int index = rnd.Next(freeBlocks.Count);
                        freeBlocks[index].Rule = new SunPairRule(blocksColor ?? colors[rnd.Next(colors.Count)]);
                    }
                    else
                    {
                        Color col;
                        if (coloredBlocks.Count == 0)
                            col = colors[rnd.Next(colors.Count)];
                        else
                            do
                                col = colors[rnd.Next(colors.Count)];
                            while (col == blocksColor);

                        int indexA = rnd.Next(freeBlocks.Count);
                        int indexB;
                        do
                            indexB = rnd.Next(freeBlocks.Count);
                        while (indexB == indexA);
                        
                        freeBlocks[indexA].Rule = new SunPairRule(col);
                        freeBlocks[indexB].Rule = new SunPairRule(col);
                    }
                }
            }

            // Triangle rules
            #region Triangles
            num = rnd.NextDouble();
            int amountOfTriangles = usedRuleTypesCount <= 2
                ? num > 0.5 ? (num > 0.7 ? (num > 0.9 ? 5 : 4) : 3) : 2
                : num > 0.5 ? (num > 0.75 ? (num > 95 ? 3 : 2) : 1) : 0;

            for (int i = 0; i < amountOfTriangles; i++)
            {
                // Get all blocks near solution line without rules
                var acceptableBlocks = panel.Blocks.Where(x => x.Rule == null && x.Edges.Intersect(allSolutionEdges).Count() > 0).ToList();
                if (acceptableBlocks.Count == 0)
                    break;

                int index = rnd.Next(acceptableBlocks.Count);
                acceptableBlocks[index].Rule = new TriangleRule(acceptableBlocks[index].Edges.Intersect(allSolutionEdges).Count());
            }
            #endregion

            // Elimination rules
            #region Elimination
            num = rnd.NextDouble();
            int eliminatorsAmount = num > 0.8 ? (num > 0.97 ? 2 : 1) : 0;
            for (int i = 0; i < eliminatorsAmount; i++)
            {
                // Get sectors that have 1 or more free blocks
                var acceptableSectors = sectors.Where(x => x.Blocks.Where(z => z.Rule == null).Count() >= 1).ToList();
                Sector sec = acceptableSectors[rnd.Next(acceptableSectors.Count)];
                int freeBlocksCount = sec.Blocks.Where(z => z.Rule == null).Count();
                bool isHexagon = freeBlocksCount > 1
                    ? rnd.NextDouble() > 0.9
                    : true;

                if (freeBlocksCount >= 3 && colors.Count > 1 && rnd.NextDouble() > 0.5)
                {
                    var secBlocks = sec.Blocks.Where(x => x.Rule == null).ToList();
                    int indexA = rnd.Next(secBlocks.Count);
                    int indexB;
                    do
                    {
                        indexB = rnd.Next(secBlocks.Count);
                    }
                    while (indexB == indexA);

                    Color elimCol = colors[rnd.Next(colors.Count)];
                    Color col;
                    do
                    {
                        col = colors[rnd.Next(colors.Count)];
                    }
                    while (col == elimCol);
                    
                    secBlocks[indexA].Rule = new EliminationRule(elimCol);
                    secBlocks[indexB].Rule = new SunPairRule(col);
                }
                else
                {
                    var secBlocks = sec.Blocks.Where(x => x.Rule == null).ToList();
                    int index = rnd.Next(secBlocks.Count);
                    secBlocks[index].Rule = new EliminationRule();
                }

                if (isHexagon)
                {
                    bool isNode = rnd.NextDouble() > 0.5;
                    if (isNode)
                    {
                        var secNodes = sec.Blocks.SelectMany(x => x.Nodes).Where(x => x.State == NodeState.Normal).Distinct().Except(allSolutionNodes).ToList();
                        if (secNodes.Count > 0)
                        {
                            int index = rnd.Next(secNodes.Count);
                            secNodes[index].SetState(NodeState.Marked);
                        }
                    }
                    else
                    {
                        var secEdges = sec.Blocks.SelectMany(x => x.Edges).Where(x => x.State == EdgeState.Normal).Distinct().Except(allSolutionEdges).ToList();
                        if (secEdges.Count > 0)
                        {
                            int index = rnd.Next(secEdges.Count);
                            secEdges[index].SetState(EdgeState.Marked);
                        }
                    }
                }
                else
                {
                    var secBlocks = sec.Blocks.Where(x => x.Rule == null).ToList();
                    int index = rnd.Next(secBlocks.Count);
                    int ruleType = rnd.Next(3);
                    if (ruleType == 0)
                        secBlocks[index].Rule = new ColoredSquareRule(colors[rnd.Next(colors.Count)]);
                    else if (ruleType == 1)
                    {
                        Color? squareCol = sec.Blocks.Select(x => x.Rule).OfType<ColoredSquareRule>().FirstOrDefault()?.Color;
                        Color? sunCol = sec.Blocks.Select(x => x.Rule).OfType<SunPairRule>().FirstOrDefault()?.Color;
                        Color? col;
                        do
                        {
                            col = colors[rnd.Next(colors.Count)];
                        }
                        while ((squareCol != null && col == squareCol) || (sunCol != null && col == sunCol));
                        secBlocks[index].Rule = new SunPairRule(col ?? colors[rnd.Next(colors.Count)]);
                    }
                    else if (ruleType == 2)
                        secBlocks[index].Rule = new TriangleRule(rnd.Next(1, 4));
                }
            }
            #endregion 

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
                // If the search process will start taking too long in try to find longer solution, we will break and return this
                List<int> failsafeSolution = null;

                AddNodeToSolution(startNode);
                int iteration = 0;

                // Loop until we get our line to the end node (dont' accept too short lines)
                while (line[line.Count - 1] != endNode || line.Count <= 4 || (panelWidth * panelHeight >= 12 && line.Count <= 6))
                {
                    iteration++;
                    if (iteration > 5000 && failsafeSolution != null)
                        return failsafeSolution;

                    int last = line[line.Count - 1];

                    // If last node is end node but we're still looping => line's too short, therefore delete last node and forks and go back
                    if(last == endNode)
                    {
                        if (failsafeSolution == null)
                            failsafeSolution = new List<int>(line);
                        RemoveLastNodeFromSolution();
                        forkRoad.RemoveAt(forkRoad.Count - 1);
                        continue;
                    }

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
