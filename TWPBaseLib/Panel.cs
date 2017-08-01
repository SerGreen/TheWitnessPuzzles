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
        }
    }
}
