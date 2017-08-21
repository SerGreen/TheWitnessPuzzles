using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheWitnessPuzzles
{
    public class Block : IErrorable
    {
        // Serial number
        public int Id { get; }
        // List of four nodes, which are corners of this block
        private List<Node> _nodes;
        public IReadOnlyList<Node> Nodes { get; }
        // List of four edges, which are sides of this block
        private List<Edge> _edges;
        public IReadOnlyList<Edge> Edges { get; }

        private BlockRule _rule;
        public BlockRule Rule
        {
            get => _rule;
            set
            {
                if (value != null)
                    value.OwnerBlock = this;
                _rule = value;
            }
        }

        public Edge LeftEdge => _edges[0];
        public Edge TopEdge => _edges[1];
        public Edge RightEdge => _edges[2];
        public Edge BottomEdge => _edges[3];

        public int X { get; }
        public int Y { get; }

        public Sector CurrentSector { get; set; }
        public Puzzle ParentPanel { get; }

        public override string ToString() => $"[{Id}]";
        
        public Block(int id, Node botLeft, Node topLeft, Node topRight, Node botRight, Puzzle parent, int x, int y)
        {
            Id = id;
            X = x;
            Y = y;
            ParentPanel = parent;
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
