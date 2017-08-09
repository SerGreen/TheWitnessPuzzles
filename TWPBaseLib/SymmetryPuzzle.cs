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

        protected override IEnumerable<Node> GetNodesForSectorLinesCalculation() => MainSolutionNodes;

        protected override void ModifySectorLinesBefore(List<List<Node>> sectorLines) => sectorLines.Insert(0, MirrorSolutionNodes.ToList());
        protected override void ModifySectorLinesAfter(List<List<Node>> sectorLines) => sectorLines.RemoveAt(0);
    }
}
