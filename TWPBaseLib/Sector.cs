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

        public int TotalBlocks => Blocks.Count;
        public Puzzle Panel { get; }

        public Sector(List<Block> blocks)
        {
            Blocks = blocks;
            foreach (var block in Blocks)
                block.CurrentSector = this;

            Panel = Blocks.FirstOrDefault()?.ParentPanel;
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

            int[,] baseBoard = new int[Panel.Width, Panel.Height];
            for (int j = 0; j < Panel.Height; j++)
                for (int i = 0; i < Panel.Width; i++)
                    if (Blocks.Contains(Panel.grid[i, j]))
                        baseBoard[i, j] = 0;
                    else
                        baseBoard[i, j] = 1;

            // All variations of board, created by different placement of subtractive shapes
            IEnumerable<int[,]> allBoards = new List<int[,]>();

            var tetrises = Blocks.Where(x => x.Rule is TetrisRule).Select(x => x.Rule as TetrisRule);

            // If net sum of all tetris blocks is not equal to total blocks of sector, then it's an error outright
            if (tetrises.Sum(x => x.TotalBlocks) != TotalBlocks)
            {
                foreach (var tetrino in tetrises)
                    errorsList.Add(new Error(tetrino.OwnerBlock, null));
                return errorsList;
            }

            var rotatableTetrises = tetrises.Where(x => x is TetrisRotatableRule).Select(x => x as TetrisRotatableRule).ToList();
            var stationaryTetrises = tetrises.Except(rotatableTetrises);

            // If there are no subtracting tetrominos, then we are lucky to deal with only one version of board
            if (!tetrises.Any(x => x.IsSubtractive))
                allBoards = new List<int[,]> { baseBoard };
            // Otherwise we have to create variations of base board, where all subtractive tetrominos are placed in every possible combination
            else
            {
                var rotatableSubtractive = rotatableTetrises.Where(x => x.IsSubtractive);
                var stationarySubtractive = stationaryTetrises.Where(x => x.IsSubtractive);

                // Rotatable Subtractive Shapes Configurations - list of array of 4 rotations for every rotatable shape
                List<TetrisRotatableRule[]> rSubConfigurations = new List<TetrisRotatableRule[]>();
                foreach (var shape in rotatableSubtractive)
                {
                    TetrisRotatableRule[] shapes = new TetrisRotatableRule[4];
                    shapes[0] = shape;
                    for (int i = 1; i < 4; i++)
                    {
                        shapes[i] = shapes[i-1].RotateCW();
                    }
                    rSubConfigurations.Add(shapes);
                }
                
                List<List<TetrisRule>> allSubtractiveRotations = new List<List<TetrisRule>>();
                // If there're no rotatable shapes, then there's only one combination of shape rotations
                if (rSubConfigurations.Count == 0)
                    allSubtractiveRotations.Add(new List<TetrisRule>(stationarySubtractive));
                // Otherwise get all combinations of rotations of every rotatable (and add non-rotatables to every combination)
                else
                    foreach (var permut in GetPermutationsWithRepetitions(rSubConfigurations.Count, 4))
                    {
                        List<TetrisRule> combination = new List<TetrisRule>();
                        for (int i = 0; i < rSubConfigurations.Count; i++)
                            combination.Add(rSubConfigurations[i][permut[i]]);
                        combination.AddRange(stationarySubtractive);

                        allSubtractiveRotations.Add(combination);
                    }

                // Now for every combination of rotations create all combinations of positions of every shape on board
                // Create a board version from every combination and add it to all boards
                allBoards = GetAllBoards();
                
                IEnumerable<int[,]> GetAllBoards()
                {
                    foreach (var rotatCombination in allSubtractiveRotations)
                    {
                        foreach (var permut in GetPermutationsWithRepetitions(rotatCombination.Count, Panel.Width * Panel.Height))
                        {
                            int[,] boardCopy = baseBoard.Clone() as int[,];
                            bool failedCombination = false;

                            for (int i = 0; i < rotatCombination.Count; i++)
                            {
                                int y = permut[i] / Panel.Width;
                                int x = permut[i] - y * Panel.Width;

                                if (!ApplyShapeToBoard(boardCopy, rotatCombination[i].Shape, (x, y), true))
                                {
                                    failedCombination = true;
                                    break;
                                }
                            }

                            if (failedCombination)
                                continue;

                            yield return boardCopy;
                        }
                    }
                }
            }

            // TO DO

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

            // == Local methods ==
            bool ApplyShapeToBoard(int[,] board, bool[,] shape, (int x, int y) point, bool isSubtractive = false)
            {
                if (point.x + shape.GetLength(0) - 1 >= board.GetLength(0) ||
                   point.y + shape.GetLength(1) - 1 >= board.GetLength(1))
                    return false;

                for (int i = 0; i < shape.GetLength(0); i++)
                    for (int j = 0; j < shape.GetLength(1); j++)
                        if (shape[i, j])
                            board[point.x + i, point.y + j] += isSubtractive ? -1 : 1;

                return true;
            }

            IEnumerable<List<int>> GetPermutationsWithRepetitions(int places, int options = 4)
            {
                int numericMax = (int) Math.Pow(options, places);

                for (int i = 0; i < numericMax; i++)
                {
                    List<int> li = new List<int>(places);
                    for (int digit = 0; digit < places; digit++)
                    {
                        li.Add((int) (i / Math.Pow(options, digit)) % options);
                    }
                    yield return li;
                }
            }

            IEnumerable<IEnumerable<int>> GetPermutations(int places) => Enumerable.Range(0, places).Permute();
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
