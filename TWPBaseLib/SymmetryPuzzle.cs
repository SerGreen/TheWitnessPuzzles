using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheWitnessPuzzles
{
    class SymmetryPuzzle : Puzzle
    {
        public bool Y_Mirror { get; }

        public SymmetryPuzzle(int width, int height, bool y_mirrored) : base(width, height)
        {
            Y_Mirror = y_mirrored;
        }

        public override IEnumerable<Node> SolutionNodes
        {
            get
            {
                IEnumerable<Node> mainNodes = base.SolutionNodes;
                IEnumerable<Node> mirrorNodes;

                if (Y_Mirror)
                {
                    int maxNodeId = nodes.Max(x => x.Id);
                    mirrorNodes = mainNodes.Select(x => nodes.First(n => n.Id == maxNodeId - x.Id));
                }
                else
                {
                    mirrorNodes = mainNodes.Select(x => nodes.First(n => n.Id == x.Id + (((x.Id / (Width+1)) * (Width + 1) + (Width+1)/2) -x.Id) * 2));
                }

                return mainNodes.Concat(mirrorNodes);
            }

            //  0  1  2  3  4
            //  5  6  7  8  9
            // 10 11 12 13 14
            // 15 16 17 18 19

            // 1 3
            // 6 8
            // 5 9
            // 10 14
            // 15 19
            // 16 18
        }
    }
}
