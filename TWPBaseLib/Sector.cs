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

            errorsList.AddRange(CheckTetrisErrors());

            return errorsList;
        }

        private IEnumerable<Error> CheckTetrisErrors()
        {
            List<Error> errorsList = new List<Error>();

            var tetrises = Blocks.Where(x => x.Rule is TetrisRule).Select(x => x.Rule as TetrisRule);
            var rotatableTetrises = tetrises.Where(x => x is TetrisRotatableRule).Select(x => x as TetrisRotatableRule).ToList();
            var stationaryTetrises = tetrises.Except(rotatableTetrises);

            List<TetrisRotatableRule[]> rotatableShapesConfigurations = new List<TetrisRotatableRule[]>();
            foreach (var shape in rotatableTetrises)
            {
                TetrisRotatableRule[] shapes = new TetrisRotatableRule[4];
                for (int i = 0; i < 4; i++)
                {
                    shapes[i] = shape;
                    shape.RotateCW();
                }
                rotatableShapesConfigurations.Add(shapes);
            }

            // Go through every rotatable and try every possible combination with other rotatables
            for (int i = 0; i < rotatableTetrises.Count; i++)
            {

            }

            return errorsList;
        }

        private IEnumerable<List<int>> GetPermutations(int places, int options = 4)
        {
            int n = options;
            int numericMax = (int) Math.Pow(n, places);

            for (int i = 0; i < numericMax; i++)
            {
                List<int> li = new List<int>(places);
                for (int digit = 0; digit < places; digit++)
                {
                    li.Add((int) (i / Math.Pow(n, digit)) % n);
                }
                yield return li;
            }
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
