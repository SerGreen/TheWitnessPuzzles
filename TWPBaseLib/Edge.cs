using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheWitnessPuzzles
{
    public class Edge : IColorable,  IErrorable
    {
        // Serial number
        public int Id { get; }
        // Color of dot if present
        public Color? Color { get; set; }
        public bool HasColor => Color.HasValue;
        // List of two nodes, which are connected by this edge
        private List<Node> _nodes;
        public IReadOnlyList<Node> Nodes { get; }
        // State: regular edge / broken edge / edge with dot
        public EdgeState State { get; private set; }
        public void SetState(EdgeState state) => State = state;
        public void SetStateAndColor(EdgeState state, Color color)
        {
            SetState(state);
            Color = color;
        }

        public Node NodeA => _nodes[0];
        public Node NodeB => _nodes[1];

        public bool IsVertical => Math.Abs(NodeA.Id - NodeB.Id) != 1;

        public override string ToString() => $"[{_nodes[0].Id} - {_nodes[1].Id}]";

        public Edge(Node nodeA, Node nodeB, EdgeState state = EdgeState.Normal, Color? color = null)
        {
            Id = GetEdgeId(nodeA, nodeB);
            _nodes = new List<Node>(2) { nodeA, nodeB };
            Nodes = _nodes.AsReadOnly();
            State = state;
            Color = color;
        }

        public static bool operator ==((Node a, Node b) nodes, Edge edge) => GetEdgeId(nodes.a, nodes.b) == edge.Id;
        public static bool operator !=((Node a, Node b) nodes, Edge edge) => !(nodes == edge);

        public static bool operator ==((int a, int b) nodesIds, Edge edge) => GetEdgeId(nodesIds.a, nodesIds.b) == edge.Id;
        public static bool operator !=((int a, int b) nodesIds, Edge edge) => !(nodesIds == edge);

        // Edge ID for nodes 2 and 9 is 209; for nodes 19 and 13 is 1319
        public static int GetEdgeId(Node a, Node b) => a.Id < b.Id ?
                                                       a.Id * 100 + b.Id :
                                                       b.Id * 100 + a.Id;

        public static int GetEdgeId(int idNodeA, int idNodeB) => idNodeA < idNodeB ?
                                                                 idNodeA * 100 + idNodeB :
                                                                 idNodeB * 100 + idNodeA;

        public override bool Equals(object obj) => base.Equals(obj);
        public override int GetHashCode() => base.GetHashCode();
    }
}
