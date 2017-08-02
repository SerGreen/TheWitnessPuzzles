using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheWitnessPuzzleGenerator
{
    public class Puzzle
    {
        public int Width { get; }
        public int Height { get; }

        public Block[,] grid;
        public Node[] nodes;
        public List<Edge> edges;
        // Auxilary array for sector splitting. It contains only vertical edges in a grid fashion
        private Edge[,] edgesAlignment;

        public List<int> Solution { get; set; } = null;
        public IEnumerable<Node> SolutionNodes => Solution.Select(x => nodes.First(z => z.Id == x));
        // Zip creates sequence from the pair of elements of original and second collection; Skip(1) forms the second collection
        public IEnumerable<Edge> SolutionEdges => Solution.Zip(Solution.Skip(1), (idA, idB) => edges.First(x => (idA, idB) == x));

        public IEnumerable<Node> BorderNodes => nodes.Where(x => x.Edges.Count < 4);

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
                                           nodes[(j + 1) * (width + 1) + i + 1]);
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

        /// <summary>
        /// Splits panel's blocks into sectors using current Solution line
        /// </summary>
        /// <returns>List of sectors</returns>
        public List<Sector> GetSectors()
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
            sectorBlocks = new List<Block>();
            for (int x = 0; x < usedBlocks.GetLength(0); x++)
                for (int y = 0; y < usedBlocks.GetLength(0); y++)
                    if (!usedBlocks[x, y])
                        sectorBlocks.Add(grid[x, y]);

            sectors.Add(new Sector(sectorBlocks));

            return sectors;
        }

        /// <summary>
        /// Fucking abomination that is not meant to be neither readable, nor understandable.
        /// It just works. Don't try to touch it. I mean it.
        /// </summary>
        /// <returns>List if sectors represented by list of nodes of sector outline</returns>
        private List<List<Node>> GetSectorLines()
        {
            List<List<Node>> sectorLines = new List<List<Node>>();

            if (Solution != null)
            {
                bool sectorStarted = false;
                List<Node> currentSector = new List<Node>();
                var nodes = SolutionNodes.ToArray();

                for (int i = 0; i < nodes.Length - 1; i++)
                {
                    Node nodeNow = nodes[i];
                    Node nodeNext = nodes[i + 1];

                    if (!sectorStarted)
                    {
                        // Lift off from border, start recording line
                        if (nodeNext.Edges.Count > 3)
                        {
                            sectorStarted = true;
                            currentSector.Add(nodeNow);
                        }
                    }
                    // If sector is started
                    else
                    {
                        // Drawing the line
                        if(nodeNow.Edges.Count > 3)
                            currentSector.Add(nodeNow);
                        // Back to border, need to complete the line back to it's start along the border
                        else
                        {
                            Node backPrev = nodeNext;
                            Node backNow = nodeNow;
                            Node backNext = null;
                            bool followSectorLine = false;
                            int otherSectorIndex = -1;
                            int otherSectorDirection = 0;
                            int indexInOtherSector = -1;

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
                                            followSectorLine = true;
                                            otherSectorIndex = j;
                                            // If backNow is the beginning of the other sector, then we will loop it forward, otherwise - backwards
                                            otherSectorDirection = sectorLines[j][0].Id == backNow.Id ? 1 : -1;
                                            indexInOtherSector = sectorLines[j].FindIndex(x => x.Id == backNow.Id);
                                            break;
                                        }
                                }
                                // If we do follow other sector line, check if we are finished doing so
                                else
                                {
                                    if (backNow.Edges.Count < 4)
                                    {
                                        followSectorLine = false;
                                        // Change backPrev to the node as if we were following the border
                                        backPrev = sectorLines[otherSectorIndex][0].Id == backNow.Id
                                            ? sectorLines[otherSectorIndex].Last()
                                            : sectorLines[otherSectorIndex][(indexInOtherSector + otherSectorDirection) % sectorLines[otherSectorIndex].Count];
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

            return sectorLines;
        }
    }
}
