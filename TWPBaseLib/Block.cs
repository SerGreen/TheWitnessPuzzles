using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheWitnessPuzzleGenerator
{
    public class Block : IColorable
    {
        // Serial number
        public int Id { get; }
        // List of four nodes, which are corners of this block
        private List<Node> _nodes;
        public IReadOnlyList<Node> Nodes { get; }
        // List of four edges, which are sides of this block
        private List<Edge> _edges;
        public IReadOnlyList<Edge> Edges { get; }

        public Edge LeftEdge => _edges[0];
        public Edge TopEdge => _edges[1];
        public Edge RightEdge => _edges[2];
        public Edge BottomEdge => _edges[3];

        public override string ToString() => $"[{Id}]";

        public Color Color { get; private set; }

        public Block(int id, Node botLeft, Node topLeft, Node topRight, Node botRight)
        {
            Id = id;
            _nodes = new List<Node>(4) { botLeft, topLeft, topRight, botRight };
            _edges = new List<Edge>(4)
            {
                botLeft.LinkToNode(topLeft),
                topLeft.LinkToNode(topRight),
                botRight.LinkToNode(topRight),
                botLeft.LinkToNode(botRight)
            };

            Nodes = _nodes.AsReadOnly();
            Edges = _edges.AsReadOnly();
        }
    }
}
