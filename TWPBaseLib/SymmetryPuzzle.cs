using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheWitnessPuzzles
{
    public class SymmetryPuzzle : Puzzle
    {
        public bool Y_Mirror { get; }
        public Color MainColor { get; }
        public Color MirrorColor { get; }

        public SymmetryPuzzle(int width, int height, bool y_mirrored, Color? mainColor = null, Color? mirrorColor = null) : base(width, height)
        {
            Y_Mirror = y_mirrored;
            MainColor = mainColor ?? Color.Black;
            MirrorColor = mirrorColor ?? Color.Black;
        }

        public IEnumerable<Node> MainSolutionNodes => base.SolutionNodes;
        public IEnumerable<Node> MirrorSolutionNodes
        {
            get
            {
                if (Y_Mirror)
                {
                    // Both axes mirroring
                    int maxNodeId = nodes.Max(x => x.Id);
                    return MainSolutionNodes.Select(x => nodes.First(n => n.Id == maxNodeId - x.Id));
                }
                else
                {
                    // Only X-axis mirroring
                    int width1 = Width + 1;
                    return MainSolutionNodes.Select(x => nodes.First(n => n.Id == (x.Id / width1 * width1) * 2 + Width - x.Id));
                }
            }
        }

        public override IEnumerable<Node> SolutionNodes => MainSolutionNodes.Concat(MirrorSolutionNodes);

        public IEnumerable<Edge> MainSolutionEdges => base.SolutionEdges;
        public IEnumerable<Edge> MirrorSolutionEdges => MirrorSolutionNodes.Zip(MirrorSolutionNodes.Skip(1), (idA, idB) => edges.First(x => (idA, idB) == x));

        public override IEnumerable<Edge> SolutionEdges => MainSolutionEdges.Concat(MirrorSolutionEdges);

        protected override IEnumerable<Node> GetSolutionNodesForSectorLinesCalculation() => MainSolutionNodes;

        protected override void ModifySectorLinesBefore(List<List<Node>> sectorLines) => sectorLines.Insert(0, MirrorSolutionNodes.ToList());
        protected override void ModifySectorLinesAfter(List<List<Node>> sectorLines) => sectorLines.RemoveAt(0);

        protected override void DistributeUnusedBlocksToSectors(List<Sector> sectors, bool[,] usedBlocks)
        {
            // Get unused blocks as list
            List<Block> unusedBlocksList = new List<Block>();
            for (int x = 0; x < usedBlocks.GetLength(0); x++)
                for (int y = 0; y < usedBlocks.GetLength(1); y++)
                    if (!usedBlocks[x, y])
                        unusedBlocksList.Add(grid[x, y]);

            List<Block> newSector = new List<Block>();

            // Create mirrored sectors of existing ones
            int maxBlockId = Width * Height - 1;
            for (int i = sectors.Count - 1; i >= 0; i--)
            {
                newSector = new List<Block>();

                foreach (Block block in sectors[i].Blocks)
                {
                    int mirrorBlockId;
                    Block mirrorBlock;

                    // XY mirror
                    if (Y_Mirror)
                        mirrorBlockId = maxBlockId - block.Id;
                    // X mirror
                    else
                        mirrorBlockId = block.Y * Width * 2 + Width - block.Id - 1;

                    mirrorBlock = unusedBlocksList.Find(x => x.Id == mirrorBlockId);
                    // If mirrored block is alredy used, then skip whole sector
                    if (mirrorBlock == null)
                        break;
                    else
                    {
                        newSector.Add(mirrorBlock);
                        unusedBlocksList.Remove(mirrorBlock);
                    }
                }

                if (newSector.Count > 0)
                    sectors.Add(new Sector(newSector));
            }

            // All remainig unused blocks should be split into two symmetric sectors
            newSector = new List<Block>();
            int prevCount = 0;

            // Collect all connected (with edges) blocks into one sector and all remainig after that should form another sector
            if (unusedBlocksList.Count > 0)
            {
                newSector.Add(unusedBlocksList[0]);
                unusedBlocksList.RemoveAt(0);
            }
            
            while (newSector?.Count != prevCount)
            {
                prevCount = newSector.Count;
                var addition = unusedBlocksList.Where(z => newSector.SelectMany(x => x.Edges).Intersect(z.Edges).Count() > 0);
                newSector.AddRange(addition);
                unusedBlocksList.RemoveAll(x => addition.Contains(x));
            }
            if (newSector.Count > 0)
                sectors.Add(new Sector(newSector));

            if (unusedBlocksList.Count > 0)
                sectors.Add(new Sector(unusedBlocksList));
        }
    }
}
