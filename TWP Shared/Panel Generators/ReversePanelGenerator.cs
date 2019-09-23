using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using System.Linq;
using TheWitnessPuzzles;

namespace TWP_Shared
{
    /// <summary>
    /// 
    ///                                     !!! DISCLAIMER !!!
    ///                       This class is bad. I mean really really REALLY bad.
    ///                 I'm not even sure that i understand how and why it works myself
    ///                      It is impossible to read. It is impossible to maintain.
    ///                                 But hey, at least it works...
    /// 
    /// </summary>
    public class ReversePanelGenerator : PanelGenerator
    {
        public static ReversePanelGenerator Instance = new ReversePanelGenerator();
        private ReversePanelGenerator() { }

        readonly static Random seedGenerator = new Random();
        readonly static List<Color> colorPalette = new List<Color>() { Color.Aqua, Color.Magenta, Color.Lime, Color.Blue, Color.OrangeRed, Color.Yellow, Color.White, Color.Black };

        // Yeah, i know, it's a method 700+ lines big...
        public override Puzzle GeneratePanel(int? seed = null)
        {
            // Use provided seed or generate random one -- a number from 0 to 1 billion minus one (9 digits)
            if (seed == null)
                seed = seedGenerator.Next(1000000000);
            Random rnd = new Random(seed.Value);

            Puzzle panel = null;
            SymmetryPuzzle symPanel = null;

            //var panelPalette = ColorPalettesLibrary.Palettes[0];
            var panelPalette = ColorPalettesLibrary.Palettes[rnd.Next(ColorPalettesLibrary.Size)];
            Color mainColor = panelPalette.MainLineColor, mirrorColor = panelPalette.MirrorLineColor;

            // Generate random panel size
            double num = rnd.NextDouble();
            int panelHeight = num > 0.05 ? (num > 0.3 ? (num > 0.75 ? (num > 0.9 ? (num > 0.95 ? 7 : 6) : 5) : 4) : 3) : 2;
            num = rnd.NextDouble();
            int panelWidth = num > 0.05 ? (num > 0.3 ? (num > 0.75 ? (num > 0.9 ? (num > 0.95 ? 7 : 6) : 5) : 4) : 3) : 2;
            bool symmetry = false;
            bool ySymmetry = false;
            bool mirrorTransparent = false;

            // Panel can be symmetric only if it's big enough
            #region Symmetry and panel creation
            if (panelWidth >= 4 && panelHeight >= 4 && rnd.NextDouble() > 0.3)
            {
                symmetry = true;
                if (rnd.NextDouble() > 0.5)
                    ySymmetry = true;

                if (rnd.NextDouble() > 0.84)
                    mirrorTransparent = true;
            }

            panel = symmetry
                ? new SymmetryPuzzle(panelWidth, panelHeight, ySymmetry, mirrorTransparent, panelPalette.MainLineColor, panelPalette.MirrorLineColor, panelPalette.BackgroundColor, panelPalette.WallsColor, panelPalette.ButtonsColor, seed.Value)
                : new Puzzle(panelWidth, panelHeight, panelPalette.SingleLineColor, panelPalette.BackgroundColor, panelPalette.WallsColor, panelPalette.ButtonsColor, seed.Value);

            if (symmetry)
                symPanel = panel as SymmetryPuzzle;
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

            // Tetris rules
            int totalTetrisRulesAdded = 0;
            for (int i = 0; i < sectors.Count; i++)
            {
                int totalBlocks = sectors[i].Blocks.Count;
                var freeBlocks = GetFreeSectorBlocks();

                // If all tetrominos will be this size, we still have enough free blocks to fit them in
                // Tetrominos can grow larger than minimum size though
                // For empty sector the number is 1
                // But we don't really want a large sector with five or more tetris rules, even if we have five or more free blocks, so here comes the limit
                int tetrominoMinSize = (int) Math.Ceiling((double) totalBlocks / Math.Min(4, freeBlocks.Count));
                int tetrominoTrueMinSize = (int) Math.Ceiling((double) totalBlocks / freeBlocks.Count);
                int exprectedTetrominosAmount = (int) Math.Ceiling((double) totalBlocks / tetrominoMinSize);

                // Create tetrominos only if sector is good sized and expected size of one tetromino is not too big
                if (totalTetrisRulesAdded < 5 && totalBlocks > 1 && totalBlocks < 14 && tetrominoMinSize <= 4 && rnd.NextDouble() > 0.2)
                {
                    List<Block> unusedBlocks = new List<Block>(sectors[i].Blocks);

                    // By default we are not gonna create subtractive shapes
                    int subtractiveQuota = 0;
                    int subtractiveTetrominoSize = -1;

                    // If we have plenty of free space, we can spawn subtractive shape
                    if(tetrominoTrueMinSize <= 2 && freeBlocks.Count > 1 && totalBlocks < 18 && exprectedTetrominosAmount <= 3)
                    {
                        subtractiveQuota++;
                        // Compensate subtractive tetromino with bigger regular ones
                        if (tetrominoMinSize != tetrominoTrueMinSize)
                            tetrominoMinSize++;
                        // Average size of subtractive shape should be about the amount of freed space by the increase of tetrominoMinSize
                        // Every block yields one bit of space, but also we need one free block to store subtractive tetromino, so it's (Count - 1)
                        // But also we wouldn't want a shape much bigger than 4 bits
                        int subtractiveTetrominoAvgSize = Math.Min(4, freeBlocks.Count - 1);
                        // Exact size of subtractive tetromino
                        subtractiveTetrominoSize = MathHelper.Clamp(rnd.Next(subtractiveTetrominoAvgSize - 1, subtractiveTetrominoAvgSize + 2), 1, 5);
                    }
                    
                    // Loop untill we use all sector blocks
                    while (unusedBlocks.Count > 0)
                    {
                        // Just a precaution. May happen, but extremely rarely, when RNGesus is really mad at us
                        if(freeBlocks.Count == 0)
                        {
                            // Undo all spawned tetris rules in sector
                            var tetrisBlocks = sectors[i].Blocks.Where(x => x.Rule is TetrisRule);
                            foreach (var block in tetrisBlocks)
                                block.Rule = null;
                            break;
                        }
                        
                        List<Block> tetrominoBlocks = new List<Block>();

                        // Decide if new tetromino will be subtractive
                        bool isSubtractiveTetromino = false;
                        if(subtractiveQuota > 0)
                        {
                            isSubtractiveTetromino = true;
                            subtractiveQuota--;
                        }

                        CreateTetrominoShape(isSubtractiveTetromino);

                        // Turn tetromino blocks into bool[,] shape
                        int tetroMinX = tetrominoBlocks.Min(x => x.X);
                        int tetroMaxX = tetrominoBlocks.Max(x => x.X);
                        int tetroMinY = tetrominoBlocks.Min(x => x.Y);
                        int tetroMaxY = tetrominoBlocks.Max(x => x.Y);
                        bool[,] shape = new bool[tetroMaxX - tetroMinX + 1, tetroMaxY - tetroMinY + 1];
                        foreach (Block block in tetrominoBlocks)
                            shape[block.X - tetroMinX, block.Y - tetroMinY] = true;

                        // Randomly make shape rotatable
                        bool rotatable = false;
                        if (rnd.NextDouble() > 0.85 && !TetrisRotatableRule.AreShapesIdentical(shape, TetrisRotatableRule.RotateShapeCW(shape)))
                            rotatable = true;

                        // Choose random free block to plant the new tetris rule
                        Block tetrisBlock = freeBlocks[rnd.Next(freeBlocks.Count)];
                        if (rotatable)
                            tetrisBlock.Rule = new TetrisRotatableRule(shape, isSubtractiveTetromino);
                        else
                            tetrisBlock.Rule = new TetrisRule(shape, isSubtractiveTetromino);
                        totalTetrisRulesAdded++;
                        
                        // Update free blocks
                        freeBlocks = GetFreeSectorBlocks();

                        // Repeat untill all unused blocks will be used in tetris rules
                        continue;

                        // == Inner methods ==
                        void AddBlockToTetromino(Block block, bool isSubtractive = false)
                        {
                            tetrominoBlocks.Add(block);
                            if (isSubtractive)
                                unusedBlocks.Add(block);
                            else
                                unusedBlocks.Remove(block);
                        }
                        void CreateTetrominoShape(bool isSubtractive)
                        {
                            if (isSubtractive)
                                // Take random unused block as first tetromino
                                AddBlockToTetromino(unusedBlocks[rnd.Next(unusedBlocks.Count)], true);
                            else
                                // Take first unused block as the first bit of tetromino
                                AddBlockToTetromino(unusedBlocks[0]);

                            // Do minimum amount of cycles and then shape has some chance to grow bigger (the smaller shape the bigger chance; for 4-sized shape it's 10%)
                            // But if shape is subtractive then do exact amount of cycles
                            for (int k = 1; isSubtractive ? k < subtractiveTetrominoSize : (k < tetrominoMinSize || rnd.Next(6 + tetrominoMinSize) == 0); k++)
                            {
                                // Get all blocks that are next to current shape
                                // For subtractive shape use not only unused blocks, but all of them
                                var sourceBlocks = isSubtractive ? panel.Blocks : unusedBlocks;
                                var neighbours = sourceBlocks.Where(x => x.Edges.Intersect(tetrominoBlocks.SelectMany(z => z.Edges)).Count() > 0 && !tetrominoBlocks.Contains(x)).ToList();
                                if (neighbours.Count == 0)
                                    break;

                                // Take one random neighbour block
                                Block neighbour = null;
                                bool acceptable;
                                int retries = 8;
                                do
                                {
                                    neighbour = neighbours[rnd.Next(neighbours.Count)];
                                    int minX = tetrominoBlocks.Min(x => x.X);
                                    int maxX = tetrominoBlocks.Max(x => x.X);
                                    int minY = tetrominoBlocks.Min(x => x.Y);
                                    int maxY = tetrominoBlocks.Max(x => x.Y);
                                    // This neighbour block is not acceptable if it would make tetromino bigger than 4x4 bits
                                    acceptable = !(maxX - minX + 1 >= 4 && (neighbour.X < minX || neighbour.X > maxX)) || (maxY - minY + 1 >= 4 && (neighbour.Y < minY || neighbour.Y > maxY));
                                    retries--;
                                }
                                while (!acceptable && retries > 0);

                                // If we used all retries and couldn't find acceptable block to add to tetromino, 
                                // then don't, just finalize current tetromino and move on to next one
                                if (!acceptable)
                                    break;

                                AddBlockToTetromino(neighbour, isSubtractive);
                            }
                        }
                    }
                }

                // Method to update free blocks
                List<Block> GetFreeSectorBlocks() => sectors[i].Blocks.Where(x => x.Rule == null).ToList();
            }

            // Elimination rules
            #region Elimination
            num = rnd.NextDouble();
            int eliminatorsAmount = num > 0.8 ? (num > 0.97 ? 2 : 1) : 0;
            for (int i = 0; i < eliminatorsAmount; i++)
            {
                // Get sectors that have 1 or more free blocks
                var acceptableSectors = sectors.Where(x => x.Blocks.Where(z => z.Rule == null).Count() >= 1).ToList();
                if (acceptableSectors.Count == 0)
                    break;

                Sector sec = acceptableSectors[rnd.Next(acceptableSectors.Count)];
                int freeBlocksCount = sec.Blocks.Where(z => z.Rule == null).Count();
                bool isHexagon = freeBlocksCount > 1
                    ? rnd.NextDouble() > 0.9
                    : true;

                bool coloredEliminationSpawned = false;
                // Colored eliminator can spawn alongside the sun block and only if sector has exactly one colored square without suns of the same color
                // Sun will be colored like the square and eliminator will have different color
                if (freeBlocksCount >= 3 && colors.Count > 1/* && rnd.NextDouble() > 0.5*/)
                {
                    var secBlocks = sec.Blocks.Where(x => x.Rule == null).ToList();
                    var squareSecBlocks = secBlocks.Where(x => x.Rule is ColoredSquareRule).ToList();
                    if (squareSecBlocks.Count == 1)
                    {
                        Color squareCol = (squareSecBlocks[0].Rule as ColoredSquareRule).Color.Value;
                        if (secBlocks.Where(x => x.Rule is IColorable icol && icol.Color == squareCol).Count() == 1)
                        {
                            int indexA = rnd.Next(secBlocks.Count);
                            int indexB;
                            do
                            {
                                indexB = rnd.Next(secBlocks.Count);
                            }
                            while (indexB == indexA);

                            Color elimCol;
                            do
                            {
                                elimCol = colors[rnd.Next(colors.Count)];
                            }
                            while (elimCol == squareCol);

                            secBlocks[indexA].Rule = new EliminationRule(elimCol);
                            secBlocks[indexB].Rule = new SunPairRule(squareCol);
                            coloredEliminationSpawned = true;
                        }
                    }
                }

                if(!coloredEliminationSpawned)
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
                        else
                            isNode = false;
                    }
                    if(!isNode)
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
                    int ruleType = rnd.Next(4);
                    
                    Color? squareCol = sec.Blocks.Select(x => x.Rule).OfType<ColoredSquareRule>().FirstOrDefault()?.Color;
                    Color? sunCol = sec.Blocks.Select(x => x.Rule).OfType<SunPairRule>().FirstOrDefault()?.Color;
                    
                    // Create colored square
                    if (ruleType == 0)
                    {
                        if (squareCol != null && colors.Count > 1)
                        {
                            Color col;
                            do
                                col = colors[rnd.Next(colors.Count)];
                            while (col == squareCol.Value);
                            secBlocks[index].Rule = new ColoredSquareRule(col);
                        }
                        else
                            ruleType++;
                    }
                    // Create sun
                    if (ruleType == 1)
                    {
                        if ((squareCol != null && sunCol != null && colors.Count > 2) || ((squareCol == null || sunCol == null) && colors.Count > 1))
                        {
                            Color col;
                            do
                                col = colors[rnd.Next(colors.Count)];
                            while ((squareCol != null && col == squareCol) || (sunCol != null && col == sunCol));
                            secBlocks[index].Rule = new SunPairRule(col);
                        }
                        else
                            ruleType++;
                    }
                    // Create triangle
                    if (ruleType == 2)
                    {
                        int trianglePower = rnd.Next(1, 4);
                        // Make sure that we are not creating triangle that actually fit to solution line
                        if (secBlocks[index].Edges.Intersect(allSolutionEdges).Count() == trianglePower)
                            trianglePower = (trianglePower % 3) + 1;
                        secBlocks[index].Rule = new TriangleRule(trianglePower);
                    }
                    // Create tetris
                    if (ruleType == 3)
                    {
                        int tetrominoSize = rnd.Next(1, Math.Min(4, secBlocks.Count));
                        // If there are no tetrominos in sector, make sure that we are not creating tetromino that actually fit into sector
                        if (secBlocks.Where(x => x.Rule is TetrisRule).Count() == 0 && tetrominoSize == secBlocks.Count)
                            tetrominoSize = tetrominoSize == 1 ? 2 : tetrominoSize - 1;
                        int w = rnd.Next(1, tetrominoSize + 1);
                        int h = (int) Math.Ceiling((double) tetrominoSize / w);
                        bool[,] shape = new bool[w, h];
                        for (int k = 0; k < tetrominoSize; k++)
                        {
                            int x,y;
                            do
                            {
                                x = rnd.Next(0, w);
                                y = rnd.Next(0, h);
                            }
                            while (shape[x, y] == true);
                            shape[x, y] = true;
                        }

                        bool rotattable = rnd.NextDouble() > 0.9 && !TetrisRotatableRule.AreShapesIdentical(shape, TetrisRotatableRule.RotateShapeCW(shape));
                        bool subtractive = rnd.NextDouble() > 0.93;

                        if (rotattable)
                            secBlocks[index].Rule = new TetrisRotatableRule(shape, subtractive);
                        else
                            secBlocks[index].Rule = new TetrisRule(shape, subtractive);
                    }
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
