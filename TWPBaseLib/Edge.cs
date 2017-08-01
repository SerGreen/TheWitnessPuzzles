using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheWitnessPuzzleGenerator
{
    public class Edge : IColorable
    {
        // Serial number
        public int Id { get; }
        // Color of dot if present
        public Color Color { get; private set; }
        public void SetColor(Color? color) => Color = color ?? Color.Black;
        // List of two nodes, which are connected by this edge
        private List<Node> _nodes;
        public IReadOnlyList<Node> Nodes { get; }
        // State: regular edge / broken edge / edge with dot
        public EdgeState State { get; private set; }
        public void SetState(EdgeState state) => State = state;

        public Node NodeA => _nodes[0];
        public Node NodeB => _nodes[1];

        public override string ToString() => $"[{_nodes[0].Id} - {_nodes[1].Id}]";

        public Edge(Node nodeA, Node nodeB, EdgeState state = EdgeState.Normal, Color? color = null)
        {
            // Edge ID for nodes 2 and 9 is 209; for nodes 19 and 13 is 1319
            Id = nodeA.Id < nodeB.Id ? nodeA.Id * 100 + nodeB.Id : nodeB.Id * 100 + nodeA.Id;
            _nodes = new List<Node>(2) { nodeA, nodeB };
            Nodes = _nodes.AsReadOnly();
            State = state;
            Color = color ?? Color.Black;
        }
    }
}
