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

        private Puzzle panel = null;

        private void btnDisplay_Click(object sender, EventArgs e)
        {
            panel = new Puzzle(4, 4);
            panel.nodes[0].SetState(NodeState.Start);
            panel.nodes.Last().SetState(NodeState.Exit);

            panel.nodes[11].SetState(NodeState.Marked);
            panel.nodes[16].SetState(NodeState.Marked);

            panel.edges.Find(x => x.Id == 1617).SetState(EdgeState.Marked);
            panel.edges.Find(x => x.Id == 1116).SetState(EdgeState.Broken);
            panel.edges.Find(x => x.Id == 1718).SetState(EdgeState.Broken);

            panel.grid[2, 0].Rule = new SunPairRule(panel.grid[2, 0], Color.Green);
            panel.grid[2, 3].Rule = new SunPairRule(panel.grid[2, 3], Color.Green);
            panel.grid[2, 1].Rule = new SunPairRule(panel.grid[2, 1], Color.Black);
            panel.grid[3, 0].Rule = new ColoredSquareRule(panel.grid[3, 0], Color.Magenta);
            panel.grid[0, 1].Rule = new ColoredSquareRule(panel.grid[0, 1], Color.Black);
            panel.grid[1, 2].Rule = new TriangleRule(panel.grid[1, 2], 2);

            panel.Solution = new List<int>();



            Bitmap bmp = new Bitmap(picCanvas.Width, picCanvas.Height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                RenderPuzzle(panel, g);
            }

            picCanvas.Image = bmp;
        }


        private void btnSolve_Click(object sender, EventArgs e)
        {
            List<int> solution = Array.ConvertAll(txtSolution.Text.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries), int.Parse).ToList();
            panel.Solution = solution;

            Bitmap bmp = new Bitmap(picCanvas.Width, picCanvas.Height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                RenderPuzzle(panel, g);
            }

            picCanvas.Image = bmp;
        }

        private void RenderPuzzle(Puzzle panel, Graphics g)
        {
            // In pixels
            int margin = 50;
            int nodeSpan = 50;
            int nodeRadius = 10;

            Brush brush = new SolidBrush(Color.Black);
            Brush errBrush = new SolidBrush(Color.Red);
            Brush errBrushA = new SolidBrush(Color.FromArgb(120, Color.Red));
            Pen pen = new Pen(brush);
            Pen penLine = new Pen(Color.Black, 5);

            int width = panel.Width + 1;
            int height = panel.Height + 1;

            var errors = panel.CheckForErrors();
            var errNodes = errors.Where(x => x.Subject is Node).Select(x => x.Subject as Node);
            var errEdges = errors.Where(x => x.Subject is Edge).Select(x => x.Subject as Edge);
            var errBlocks = errors.Where(x => x.Subject is Block).Select(x => x.Subject as Block);

            for (int i = 0; i < panel.nodes.Length; i++)
            {
                int row = i / width;
                int x = margin + (i - row * width) * nodeSpan;
                int y = margin + row * nodeSpan;

                //if (panel.nodes[i].State == NodeState.Start)
                //    g.FillEllipse(brush, x - nodeRadius, y - nodeRadius, nodeRadius * 2, nodeRadius * 2);
                //else
                //    g.DrawEllipse(pen, x - nodeRadius, y - nodeRadius, nodeRadius * 2, nodeRadius * 2);

                //if (panel.nodes[i].State == NodeState.Exit)
                //    g.DrawEllipse(pen, x - nodeRadius / 2, y - nodeRadius / 2, nodeRadius, nodeRadius);

                if (panel.nodes[i].State == NodeState.Marked)
                    g.FillEllipse(errNodes.Contains(panel.nodes[i]) ? errBrush : brush, x - 3, y - 3, 6, 6);

                g.DrawString(i.ToString(), Font, brush, x, y);
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

                if (edge.State != EdgeState.Broken)
                    g.DrawLine(solutionEdges.Contains(edge) ? penLine : pen, xA, yA, xB, yB);

                if (edge.State == EdgeState.Marked)
                    g.FillEllipse(errEdges.Contains(edge) ? errBrush : brush, (xA + xB) / 2 - 3, (yA + yB) / 2 - 3, 6, 6);
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

            List<Pen> sectorPens = new List<Pen>
            {
                new Pen(Color.Blue, 3),
                new Pen(Color.Magenta, 3),
                new Pen(Color.Green, 3),
                new Pen(Color.OrangeRed, 3),
                new Pen(Color.Cyan, 3),
                new Pen(Color.Yellow, 3),
                new Pen(Color.Gray, 3),
                new Pen(Color.Violet, 3)
            };

            for (int i = 0; i < sectors.Count; i++)
            {
                foreach (Block block in sectors[i].Blocks)
                {
                    int row = block.Id / panel.Width;
                    int col = block.Id - row * panel.Width;

                    int x = margin + col * nodeSpan + nodeSpan / 2;
                    int y = margin + row * nodeSpan + nodeSpan / 2;

                    g.DrawRectangle(sectorPens[i], x - nodeSpan / 2 + 10, y - nodeSpan / 2 + 10, nodeSpan - 20, nodeSpan - 20);

                    if (block.Rule is ColoredSquareRule sqareRule)
                        g.FillRectangle(new SolidBrush(sqareRule.Color), x - nodeRadius, y - nodeRadius, nodeRadius * 2, nodeRadius * 2);
                    else if (block.Rule is SunPairRule sunRule)
                        g.FillEllipse(new SolidBrush(sunRule.Color), x - nodeRadius, y - nodeRadius, nodeRadius * 2, nodeRadius * 2);
                    else if (block.Rule is TriangleRule triRule)
                    {
                        g.FillPolygon(new SolidBrush(Color.Orange), new PointF[] {
                            new PointF(x -10, y +10),
                            new PointF(x, y -16),
                            new PointF(x +10, y +10)
                        });
                        g.DrawString(triRule.Power.ToString(), Font, brush, x - 5, y - 5);
                    }

                    if(errBlocks.Contains(block))
                        g.FillRectangle(errBrushA, x - nodeSpan / 2 + 4, y - nodeSpan / 2 + 4, nodeSpan - 8, nodeSpan - 8);
                }
            }
        }
    }
}
