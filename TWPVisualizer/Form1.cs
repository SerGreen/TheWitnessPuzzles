using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TheWitnessPuzzleGenerator;

namespace TWPVisualizer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnDisplay_Click(object sender, EventArgs e)
        {
            Puzzle panel = new Puzzle(4, 4);
            panel.nodes[0].SetState(NodeState.Start);
            panel.nodes.Last().SetState(NodeState.Exit);

            panel.Solution = new List<int> { 0, 5, 6, 11, 10, 15, 20, 21, 22, 17, 12, 13, 8, 7, 2, 3, 4, 9, 14, 19, 18, 23, 24 };



            Bitmap bmp = new Bitmap(picCanvas.Width, picCanvas.Height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                RenderPuzzle(panel, g);
            }

            picCanvas.Image = bmp;

            panel.GetSectors();
        }

        private void RenderPuzzle(Puzzle panel, Graphics g)
        {
            // In pixels
            int margin = 50;
            int nodeSpan = 50;
            int nodeRadius = 10;

            Brush brush = new SolidBrush(Color.Black);
            Pen pen = new Pen(brush);
            Pen penLine = new Pen(Color.Red, 3);

            int width = panel.Width + 1;
            int height = panel.Height + 1;

            for (int i = 0; i < panel.nodes.Length; i++)
            {
                int row = i / width;
                int x = margin + (i - row * width) * nodeSpan;
                int y = margin + row * nodeSpan;

                if (panel.nodes[i].State == NodeState.Start)
                    g.FillEllipse(brush, x - nodeRadius, y - nodeRadius, nodeRadius * 2, nodeRadius * 2);
                else
                    g.DrawEllipse(pen, x - nodeRadius, y - nodeRadius, nodeRadius * 2, nodeRadius * 2);

                if (panel.nodes[i].State == NodeState.Exit)
                    g.DrawEllipse(pen, x - nodeRadius / 2, y - nodeRadius / 2, nodeRadius, nodeRadius);
            }

            var solutionEdges = panel.SolutionEdges;
            for (int i = 0; i < panel.edges.Count; i++)
            {
                Edge edge = panel.edges[i];
                
                int rowA = edge.NodeA.Id / width;
                int xA = margin + (edge.NodeA.Id - rowA * width) * nodeSpan;
                int yA = margin + rowA * nodeSpan;

                int rowB = edge.NodeB.Id / width;
                int xB = margin + (edge.NodeB.Id - rowB * width) * nodeSpan;
                int yB = margin + rowB * nodeSpan;
                
                g.DrawLine(solutionEdges.Contains(edge) ? penLine : pen, xA, yA, xB, yB);
            }

            List<Sector> sectors = panel.GetSectors();
            List<Brush> sectorBrushes = new List<Brush>
            {
                new SolidBrush(Color.Blue),
                new SolidBrush(Color.Magenta),
                new SolidBrush(Color.Green),
                new SolidBrush(Color.OrangeRed),
                new SolidBrush(Color.Cyan),
                new SolidBrush(Color.Yellow),
                new SolidBrush(Color.Gray),
                new SolidBrush(Color.Violet)
            };

            for (int i = 0; i < sectors.Count; i++)
            {
                foreach (Block block in sectors[i].Blocks)
                {
                    int row = block.Id / panel.Width;
                    int col = block.Id - row * panel.Width;

                    int x = margin + col * nodeSpan + nodeSpan / 2;
                    int y = margin + row * nodeSpan + nodeSpan / 2;

                    g.FillRectangle(sectorBrushes[i], x - nodeRadius, y - nodeRadius, nodeRadius * 2, nodeRadius * 2);
                }
            }
        }
    }
}
