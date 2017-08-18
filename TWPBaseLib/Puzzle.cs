using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheWitnessPuzzles
{
    public class Puzzle
    {
        public int Width { get; }
        public int Height { get; }

        public Block[,] grid;
        public Node[] nodes;
        public List<Edge> edges;
        // Auxilary array for sector splitting. It contains only vertical edges in a grid fashion
        protected Edge[,] edgesAlignment;

        public List<int> Solution { get; set; } = null;
        public virtual IEnumerable<Node> SolutionNodes => Solution.Select(x => nodes.First(z => z.Id == x));
        // Zip creates sequence from the pair of elements of original and second collection; Skip(1) forms the second collection
        public virtual IEnumerable<Edge> SolutionEdges => Solution.Zip(Solution.Skip(1), (idA, idB) => edges.First(x => (idA, idB) == x));

        public virtual IEnumerable<Node> BorderNodes => nodes.Where(x => x.Edges.Count < 4);

        public Node TopLeftNode => nodes[0];
        public Node TopRightNode => nodes[Width];
        public Node BottomLeftNode => nodes[nodes.Length - Width - 1];
        public Node BottomRightNode => nodes[nodes.Length - 1];

        public Puzzle(int width, int height)
        {
            Width = width;
            Height = height;

            nodes = new Node[(width + 1) * (height + 1)];
            grid = new Block[width, height];

            for (int i = 0; i < nodes.Length; i++)
                nodes[i] = new Node(i);

            for (int j = 0; j < height; j++)
            {
                for (int i = 0; i < width; i++)
                {
                    grid[i, j] = new Block(j * width + i,
                                           nodes[(j + 1) * (width + 1) + i],
                                           nodes[j * (width + 1) + i],
                                           nodes[j * (width + 1) + i + 1],
                                           nodes[(j + 1) * (width + 1) + i + 1],
                                           this,
                                           i, j);
                }
            }

            edges = nodes.SelectMany(x => x.Edges).Distinct().ToList();

            edgesAlignment = new Edge[width + 1, height];
            for (int i = 0; i < width+1; i++)
                for (int j = 0; j < height; j++)
                {
                    int a = j * (width + 1) + i;
                    int b = (j + 1) * (width + 1) + i;
                    edgesAlignment[i, j] = nodes[a].LinkToNode(nodes[b]);
                }
        }

        public List<Error> CheckForErrors()
        {
            List<Error> errors = new List<Error>();
            
            foreach (Sector sector in GetSectors())
                errors.AddRange(sector.CheckSectorErrors());

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
                            sectorBlocks.Add(grid[col - 1, row]);
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

        protected virtual void DistributeUnusedBlocksToSectors(List<Sector> sectors, bool[,] usedBlocks)
        {
            List<Block> sectorBlocks = new List<Block>();
            for (int x = 0; x < usedBlocks.GetLength(0); x++)
                for (int y = 0; y < usedBlocks.GetLength(1); y++)
                    if (!usedBlocks[x, y])
                        sectorBlocks.Add(grid[x, y]);

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

                for (int i = 0; i < nodes.Length - 1; i++)
                {
                    // Current and next position of solution line parser
                    Node nodeNow = nodes[i];
                    Node nodeNext = nodes[i + 1];

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
                                            if ((indexInOtherSector > 0 && sectorLines[j][indexInOtherSector - 1].Edges.Count > 3) ||
                                                (indexInOtherSector < sectorLines[j].Count - 1 && sectorLines[j][indexInOtherSector + 1].Edges.Count > 3))
                                            {
                                                followSectorLine = true;
                                                otherSectorIndex = j;
                                                // If the next node in other sector line is the one not on border, then we are going to move forward
                                                // Otherwise we should move along other sector line backwards
                                                int indexForwardDirection = (indexInOtherSector + 1) % sectorLines[j].Count;
                                                otherSectorDirection = sectorLines[j][indexForwardDirection].Edges.Count > 3 ? 1 : -1;

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
                                        followSectorLine = false;
                                        // Change backPrev to the node as if we were following the border
                                        backPrev = theRightWay[theRightWay.FindIndex(x => x.Id == backNow.Id) - 1];
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
    }
}
