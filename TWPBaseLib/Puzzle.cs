using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheWitnessPuzzles
{
    public class Puzzle
    {
        /// <summary>
        /// Seed of panel generator's RNG, can be used to generate this exact panel (providing that the same version of Generator is used)
        /// </summary>
        public int Seed { get; }

        /// <summary>
        /// Panel width in blocks (normally 2 to 7)
        /// </summary>
        public int Width { get; }
        /// <summary>
        /// Panel height in blocks (normally 2 to 7)
        /// </summary>
        public int Height { get; }

        public virtual Color MainColor { get; }
        public Color BackgroundColor { get; }
        public Color WallsColor { get; }
        public Color ButtonsColor { get; }

        /// <summary>
        /// Panel blocks in two-dimensional array
        /// </summary>
        public Block[,] Grid { get; private set; }
        public Node[] Nodes { get; private set; }
        public List<Edge> Edges { get; private set; }
        // Auxilary array for sector splitting. It contains only vertical edges in a grid fashion
        protected Edge[,] edgesAlignment;

        /// <summary>
        /// Panel blocks in the form of enumeration
        /// </summary>
        public IEnumerable<Block> Blocks
        {
            get
            {
                for (int i = 0; i < Grid.GetLength(0); i++)
                    for (int j = 0; j < Grid.GetLength(1); j++)
                        yield return Grid[i, j];
            }
        }

        /// <summary>
        /// Sequence of node IDs that represents solution to the puzzle
        /// Main line only
        /// </summary>
        public List<int> Solution { get; private set; } = null;
        /// <summary>
        /// All nodes, that are in solution (including mirror line)
        /// </summary>
        public virtual IEnumerable<Node> SolutionNodes => Solution.Select(x => Nodes.First(z => z.Id == x));
        /// <summary>
        /// All edges, that are in solution (including mirror line)
        /// </summary>
        // Zip creates sequence from the pair of elements of original and second collection; Skip(1) forms the second collection
        public virtual IEnumerable<Edge> SolutionEdges => Solution.Zip(Solution.Skip(1), (idA, idB) => Edges.First(x => (idA, idB) == x));

        /// <summary>
        /// Nodes, that are located on top, bottom, left or right border of puzzle
        /// </summary>
        public virtual IEnumerable<Node> BorderNodes => Nodes.Where(x => x.Edges.Count < 4);

        public Node TopLeftNode => Nodes[0];
        public Node TopRightNode => Nodes[Width];
        public Node BottomLeftNode => Nodes[Nodes.Length - Width - 1];
        public Node BottomRightNode => Nodes[Nodes.Length - 1];

        /// <summary>
        /// Checks that proposed solution contains only existing node IDs and sets new Solution
        /// </summary>
        /// <param name="newSolution">List of node IDs that form a solution line</param>
        /// <returns>True if new solution is set or False if proposed solution contains invalid nodes</returns>
        public bool SetSolution(List<int> newSolution)
        {
            if (newSolution.Any(x => x < 0 || x >= Nodes.Length))
                return false;

            Solution = newSolution;
            return true;
        }

        public Puzzle(int width, int height, Color? lineColor = null, Color? backgroundColor = null, Color? wallsColor = null, Color? buttonsColor = null, int seed = -1)
        {
            Seed = seed;
            Width = width;
            Height = height;
            MainColor = lineColor ?? Color.White;
            BackgroundColor = backgroundColor ?? Color.DimGray;
            WallsColor = wallsColor ?? Color.Black;
            ButtonsColor = buttonsColor ?? Color.DimGray;

            Nodes = new Node[(width + 1) * (height + 1)];
            Grid = new Block[width, height];

            for (int i = 0; i < Nodes.Length; i++)
                Nodes[i] = new Node(i);

            for (int j = 0; j < height; j++)
            {
                for (int i = 0; i < width; i++)
                {
                    Grid[i, j] = new Block(j * width + i,
                                           Nodes[(j + 1) * (width + 1) + i],
                                           Nodes[j * (width + 1) + i],
                                           Nodes[j * (width + 1) + i + 1],
                                           Nodes[(j + 1) * (width + 1) + i + 1],
                                           this,
                                           i, j);
                }
            }

            Edges = Nodes.SelectMany(x => x.Edges).Distinct().ToList();

            edgesAlignment = new Edge[width + 1, height];
            for (int i = 0; i < width+1; i++)
                for (int j = 0; j < height; j++)
                {
                    int a = j * (width + 1) + i;
                    int b = (j + 1) * (width + 1) + i;
                    edgesAlignment[i, j] = Nodes[a].LinkToNode(Nodes[b]);
                }
        }


        /// <summary>
        /// Invokes sector splitting and evaluets errors in the puzzle with current solution
        /// </summary>
        /// <returns>List with found errors or empty list if Solution is null</returns>
        public List<Error> CheckForErrors()
        {
            List<Error> errors = new List<Error>();
            
            if (Solution != null)
            {
                foreach (Sector sector in GetSectors())
                    errors.AddRange(sector.CheckSectorErrors());
            }

            return errors;
        }

        /// <summary>
        /// Splits panel's blocks into sectors using current Solution line
        /// </summary>
        /// <returns>List of sectors</returns>
        public virtual List<Sector> GetSectors()
        {
            List<Sector> sectors = new List<Sector>();
            // All unused blocks in the end will form another sector
            bool[,] usedBlocks = new bool[Width, Height];
            List<Block> sectorBlocks;

            List<List<Node>> sectorLines = GetSectorLines();

            // Compiling sector blocks from each sector outline
            for (int i = 0; i < sectorLines.Count; i++)
            {
                var line = sectorLines[i];
                var sectorEdges = line.Zip(line.Skip(1).Concat(line.Take(1)), (a, b) => a.LinkToNode(b));
                sectorBlocks = new List<Block>();

                // Sector active means that we will be adding blocks to sector. If not active, then we wait until sector begins
                bool sectorActive = false;

                for (int row = 0; row < edgesAlignment.GetLength(1); row++)
                    for (int col = 0; col < edgesAlignment.GetLength(0); col++)
                    {
                        // If we are inside sector, then add to the block list the block, that has current edge as the right edge
                        if (sectorActive)
                        {
                            sectorBlocks.Add(Grid[col - 1, row]);
                            usedBlocks[col - 1, row] = true;
                        }

                        // If sector outline contains current edge, then switch active state (we are going either in or out of sector
                        if (sectorEdges.Contains(edgesAlignment[col, row]))
                            sectorActive = !sectorActive;
                    }

                sectors.Add(new Sector(sectorBlocks));
            }

            // Compiling the last sector from unused blocks
            DistributeUnusedBlocksToSectors(sectors, usedBlocks);

            return sectors;
        }

        /// <summary>
        /// Forms last sector from unused blocks in the end of sector splitting
        /// </summary>
        /// <param name="sectors">List of found sectors</param>
        /// <param name="usedBlocks">Array with flags of the same size as Grid</param>
        protected virtual void DistributeUnusedBlocksToSectors(List<Sector> sectors, bool[,] usedBlocks)
        {
            List<Block> sectorBlocks = new List<Block>();
            for (int x = 0; x < usedBlocks.GetLength(0); x++)
                for (int y = 0; y < usedBlocks.GetLength(1); y++)
                    if (!usedBlocks[x, y])
                        sectorBlocks.Add(Grid[x, y]);

            sectors.Add(new Sector(sectorBlocks));
        }

        /// <summary>
        /// Fucking abomination that is not meant to be either readable, or understandable.
        /// It just works. Don't try to touch it. I mean it.
        /// </summary>
        /// <returns>List if sectors represented by list of nodes of sector outline</returns>
        protected virtual List<List<Node>> GetSectorLines()
        {
            List<List<Node>> sectorLines = new List<List<Node>>();
            ModifySectorLinesBefore(sectorLines);

            if (Solution != null)
            {
                // Flag, means that we started recording new secor line
                bool sectorStarted = false;
                // List that stores line of new sector, that is being parsed
                List<Node> currentSector = new List<Node>();
                // Temp line from sector line start to end along the border, used to determine the direction of completion of sector line
                List<Node> theRightWay = new List<Node>();
                // Solution line nodes
                var nodes = GetSolutionNodesForSectorLinesCalculation().ToArray();

                for (int i = 0; i < nodes.Length; i++)
                {
                    // Current and next position of solution line parser
                    Node nodeNow = nodes[i];
                    Node nodeNext = i < nodes.Length - 1 ? nodes[i + 1] : nodes[i - 1];

                    if (!sectorStarted)
                    {
                        // Lift off from border, start recording line
                        if (nodeNow.Edges.Count < 4 && nodeNext.Edges.Count > 3)
                        {
                            sectorStarted = true;
                            currentSector.Add(nodeNow);
                        }
                    }
                    // If sector is started
                    else
                    {
                        // While we are not on border => record the line
                        if(nodeNow.Edges.Count > 3)
                            currentSector.Add(nodeNow);
                        // Back to border, need to complete the line back to it's start along the border
                        else
                        {
                            Node backPrev = nodeNext;
                            Node backNow = nodeNow;
                            Node backNext = null;

                            // Firstly, build Right Way Line
                            theRightWay.Clear();
                            while (backNext?.Id != currentSector[0].Id)
                            {
                                theRightWay.Add(backNow);
                                backNext = backNow.Edges.SelectMany(x => x.Nodes).Where(x => x.Id != backPrev.Id && x.Id != backNow.Id && x.Edges.Count < 4).First();
                                backPrev = backNow;
                                backNow = backNext;
                            }

                            backPrev = nodeNext;
                            backNow = nodeNow;
                            backNext = null;

                            bool followSectorLine = false;
                            int otherSectorIndex = -1;
                            int otherSectorDirection = 0;
                            int indexInOtherSector = -1;
                            int savedIndexInOtherSector = -1;

                            // Until we finish full circle
                            while (backNext?.Id != currentSector[0].Id)
                            {
                                currentSector.Add(backNow);

                                // If we do not follow other sector line, check if we should do so
                                if (!followSectorLine)
                                {
                                    // If current node does belong to any existing sector, then follow along that sector line, not border
                                    for (int j = 0; j < sectorLines.Count; j++)
                                        if (sectorLines[j].Contains(backNow))
                                        {
                                            indexInOtherSector = sectorLines[j].FindIndex(x => x.Id == backNow.Id);
                                            savedIndexInOtherSector = indexInOtherSector;
                                            if ((indexInOtherSector > 0 && sectorLines[j][indexInOtherSector - 1].Edges.Count > 3) ||
                                                (indexInOtherSector < sectorLines[j].Count - 1 && sectorLines[j][indexInOtherSector + 1].Edges.Count > 3))
                                            {
                                                followSectorLine = true;
                                                otherSectorIndex = j;
                                                // If the next node in other sector line is the one not on border, then we are going to move forward
                                                // Otherwise we should move along other sector line backwards
                                                otherSectorDirection = (indexInOtherSector < sectorLines[j].Count - 1 && sectorLines[j][indexInOtherSector + 1].Edges.Count > 3) ? 1 : -1;

                                                break;
                                            }
                                        }
                                }
                                // If we do follow other sector line, check if we are finished doing so
                                else
                                {
                                    if (backNow.Edges.Count < 4 ||
                                        (indexInOtherSector == 0 && otherSectorDirection == -1) ||
                                        (indexInOtherSector == sectorLines[otherSectorIndex].Count - 1 && otherSectorDirection == 1))
                                    {
                                        // Change backPrev to the node as if we were following the border
                                        int rightWayIndex = theRightWay.FindIndex(x => x.Id == backNow.Id);
                                        // If we couldn't find our index in the right way border line, it means that we were moving in the wrong direction
                                        if (rightWayIndex == -1)
                                        {
                                            // We have to reset to the saved first postition in other line and move in other direction
                                            // Also remove last N nodes from current sector (diff amount) as they are wrong
                                            int diff = Math.Abs(indexInOtherSector - savedIndexInOtherSector);
                                            currentSector.RemoveRange(currentSector.Count - diff, diff);
                                            indexInOtherSector = savedIndexInOtherSector;
                                            otherSectorDirection = -otherSectorDirection;
                                            backNow = sectorLines[otherSectorIndex][savedIndexInOtherSector];
                                            rightWayIndex = theRightWay.FindIndex(x => x.Id == backNow.Id);
                                        }

                                        followSectorLine = false;
                                        backPrev = theRightWay[rightWayIndex - 1];
                                    }
                                }

                                // If we are following the border line
                                if (!followSectorLine)
                                    // Out of all adjacent nodes select the one that is neither current, nor previous, nor the one not on the border
                                    backNext = backNow.Edges.SelectMany(x => x.Nodes).Where(x => x.Id != backPrev.Id && x.Id != backNow.Id && x.Edges.Count < 4).First();
                                //If we are following another sector line, then Next node is the next node from the line, until we are back to border
                                else
                                {
                                    backNext = sectorLines[otherSectorIndex][indexInOtherSector + otherSectorDirection];
                                    indexInOtherSector += otherSectorDirection;
                                }

                                backPrev = backNow;
                                backNow = backNext;
                            }

                            // Current sector complete, save it and reset, then continue along the solution line
                            sectorLines.Add(currentSector);
                            currentSector = new List<Node>();
                            sectorStarted = false;
                        }
                    }
                }
            }

            ModifySectorLinesAfter(sectorLines);
            return sectorLines;
        }

        protected virtual IEnumerable<Node> GetSolutionNodesForSectorLinesCalculation() => SolutionNodes;

        // Two methods override are used by Symmetry Puzzle
        // Mirrored solution line is used as another sector line for the time of sector line calculation and should be removed from list in After method
        // Returns true if sectorLines was modified 
        protected virtual void ModifySectorLinesBefore(List<List<Node>> sectorLines) { }
        protected virtual void ModifySectorLinesAfter(List<List<Node>> sectorLines) { }

        /// <summary>
        /// Returns the list of all solution lines that can solve the puzzle
        /// WARNING: It is VERY slow for big puzzles
        /// </summary>
        /// <returns>Collection of solution lines in the form of a sequence of node IDs</returns>
        public IEnumerable<List<int>> GetAllSolutions(bool reportProgress = false)
        {
            // Get all lines that you can possibly draw
            var allLines = GetAllPossibleLines();
            
            // Save the current Solution to restore it later
            var savedSolution = Solution;

            // Check every line for being the right solution line
            for (int i = 0; i < allLines.Count; i++)
            {
                Solution = allLines[i];
                var errors = CheckForErrors();
                if (errors.Where(x => x.IsEliminated == false).Count() == 0)
                    yield return allLines[i];

                if (reportProgress)
                    GetAllSolutionsProgressUpdate?.Invoke((float) i / allLines.Count);
            }

            // Restore Solution
            Solution = savedSolution;
        }
        
        public event Action<float> GetAllSolutionsProgressUpdate;

        // Returns all lines that you can possibly draw on this panel
        // If startNode is not specified then it generates lines for all start nodes
        public virtual List<List<int>> GetAllPossibleLines(Node startNode = null)
        {
            List<List<Node>> solutions = new List<List<Node>>();
            List<List<Node>> finishedSolutions = new List<List<Node>>();

            // Get the list of exit nodes
            var endNodes = GAPL_GetEndNodes();
            // Inititate line from specified startNode
            if (startNode != null)
                GAPL_AddStartNodes(solutions, new Node[] { startNode });
            else
                // Initiate one line from every start node
                GAPL_AddStartNodes(solutions, Nodes.Where(x => x.State == NodeState.Start));

            // Loop until all lines will get to the exit nodes
            bool allFinished = false;
            while (!allFinished)
            {
                allFinished = true;

                // Loop through every line and try to move it further
                for (int i = solutions.Count - 1; i >= 0; i--)
                {
                    // If line is completed (it's last node is the exit node) => copy it to the finished list
                    List<Node> line = solutions[i];
                    Node last = line[line.Count - 1];
                    if (endNodes.Contains(last))
                        finishedSolutions.Add(new List<Node>(line));

                    // Get all nodes, where we can move from current (last) node
                    // This excludes nodes over the broken edges, nodes in the solution line(s) and nodes, that can not be entered by two symmetry lines at the same time (nodes on the line of symmetry)
                    var possibleMoves = GAPL_GetNeighbourNodes(last);
                    possibleMoves = GAPL_RemoveImpossibleNodes(possibleMoves, solutions[i], i);

                    // If there are no possible ways to move from current position, then we are in the dead end, so we have to delete current line
                    if (possibleMoves.Count() == 0)
                        GAPL_DeleteDeadEndLine(solutions, i);
                    else
                    {
                        // If there are possible moves => remove the flag of readiness
                        allFinished = false;
                        // And then create new line for each new node
                        foreach (Node node in possibleMoves.Skip(1))
                            GAPL_AddNewLineToAllSolutions(solutions, i, node);

                        // For the sake of saving some memory we are not deleting current line, but extending it with skipped earlier node
                        GAPL_ExtendLineWithNode(solutions, i, possibleMoves.First());
                    }
                }
            }

            return finishedSolutions.Select(x => x.Select(z => z.Id).ToList()).ToList();
        }
        
        protected virtual IEnumerable<Node> GAPL_GetEndNodes() => Nodes.Where(x => x.State == NodeState.Exit);
        protected virtual void GAPL_AddStartNodes(List<List<Node>> solutions, IEnumerable<Node> startNodes)
        {
            foreach (var start in startNodes)
                solutions.Add(new List<Node>() { start });
        }
        protected virtual IEnumerable<Node> GAPL_GetNeighbourNodes(Node last) => last.Edges.Where(x => x.State != EdgeState.Broken).SelectMany(x => x.Nodes).Where(x => x.Id != last.Id);
        protected virtual IEnumerable<Node> GAPL_RemoveImpossibleNodes(IEnumerable<Node> possibleMoves, IEnumerable<Node> line, int lineIndexInSolutions) => possibleMoves.Except(line);
        protected virtual void GAPL_DeleteDeadEndLine(List<List<Node>> solutions, int index) => solutions.RemoveAt(index);
        protected virtual void GAPL_AddNewLineToAllSolutions(List<List<Node>> solutions, int index, Node node) => solutions.Add(new List<Node>(solutions[index]) { node });
        protected virtual void GAPL_ExtendLineWithNode(List<List<Node>> solutions, int index, Node node) => solutions[index].Add(node);
    }
}
