using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheWitnessPuzzleGenerator
{
    public class Sector
    {
        public List<Block> Blocks { get; set; }

        public Sector(List<Block> blocks)
        {
            Blocks = blocks;
            foreach (var block in Blocks)
                block.CurrentSector = this;
        }

        public override string ToString() => string.Join(" ", Blocks);

        public List<Error> CheckSectorErrors(IEnumerable<Node> solutionNodes, IEnumerable<Edge> solutionEdges)
        {
            List<Error> errorsList = new List<Error>();
            errorsList.AddRange(CheckSectorBlockErrors());
            errorsList.AddRange(CheckSectorNodeErrors(solutionNodes));
            errorsList.AddRange(CheckSectorEdgeErrors(solutionEdges));
            return errorsList;
        }

        private List<Error> CheckSectorBlockErrors()
        {
            List<Error> errorsList = new List<Error>();
            foreach (Block block in Blocks)
            {
                if (block.Rule is ISelfCheckableRule rule)
                {
                    var error = rule.CheckRule();
                    if (error != null)
                        errorsList.Add(error);
                }
            }

            return errorsList;
        }

        private List<Error> CheckSectorNodeErrors(IEnumerable<Node> solutionNodes)
        {
            List<Error> errorsList = new List<Error>();

            var markedNodes = Blocks.SelectMany(x => x.Nodes).Distinct().Where(x => x.State == NodeState.Marked);

            // Each marked node without solution going through it is an error
            foreach (var node in markedNodes.Except(solutionNodes))
                errorsList.Add(new Error(node, null));

            return errorsList;
        }

        private List<Error> CheckSectorEdgeErrors(IEnumerable<Edge> solutionEdges)
        {
            List<Error> errorsList = new List<Error>();

            var markedEdges = Blocks.SelectMany(x => x.Edges).Distinct().Where(x => x.State == EdgeState.Marked);
            
            // Each marked edge without solution going through it is an error
            foreach (var edge in markedEdges.Except(solutionEdges))
                errorsList.Add(new Error(edge, null));

            return errorsList;
        }
    }
}
