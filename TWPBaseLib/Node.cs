using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheWitnessPuzzles
{
    public class Node : IColorable, IErrorable
    {
        // Serial number
        public int Id { get; }
        // Color of dot if present
        public Color? Color { get; set; }
        public bool HasColor => Color.HasValue;
        // List of connected edges
        private List<Edge> _edges = new List<Edge>();
        public IReadOnlyList<Edge> Edges { get; }
        // State: contains dot / is a starting node / regular node
        public NodeState State { get; private set; }
        
        public bool SetState(NodeState state)
        {
            // Node can not be exit node if it's not on the border of panel (has 2 or 3 edges)
            if (state == NodeState.Exit && _edges.Count >= 4)
                return false;

            State = state;
            return true;
        }

        public bool SetStateAndColor(NodeState state, Color color)
        {
            Color = color;
            return SetState(state);
        }

        public override string ToString() => $"[{Id}]";

        public Node(int id, NodeState state = NodeState.Normal, Color? color = null)
        {
            Id = id;
            State = state;
            Color = color;
            Edges = _edges.AsReadOnly();
        }

        /// <summary>
        /// Connect two nodes with an edge
        /// </summary>
        /// <param name="other">Node, to which current node is being connected</param>
        /// <param name="edgeState">State of the edge: Normal, Broken or Marked</param>
        /// <param name="edgeColor">Color of edge if Marked</param>
        /// <returns>Created edge or existing edge, if nodes were already linked</returns>
        public Edge LinkToNode(Node other, EdgeState edgeState = EdgeState.Normal, Color? edgeColor = null)
        {
            // If nodes are already linked => return existing edge
            if (_edges.SelectMany(x => x.Nodes).Contains(other))
                return _edges.Find(x => x.Nodes.Contains(other));
            
            Edge edge = new Edge(this, other, edgeState, edgeColor);

            // Add created edge to edges list of this and other node
            _edges.Add(edge);
            other._edges.Add(edge);

            return edge;
        }
    }
}
