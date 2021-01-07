using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheWitnessPuzzles;

namespace TWP_Shared
{
    public class PanelRenderer
    {
        static readonly float bordersWidth = 0.035f; // Size of the border frame, % of total screen min(width, height)
        static readonly float endPointLegth = 0.3f; // Size of ending appendix at the end point, % of the block width
        static readonly Random rnd = new Random();

        GraphicsDevice GraphicsDevice;

        public Color BackgroundColor { get; set; } = Color.CornflowerBlue;
        public Color WallColor { get; set; } = Color.DarkSlateGray;
        public Color BorderColor { get; set; } = Color.DimGray;

        Puzzle panel = null;
        Point screenSize;

        internal List<Rectangle> StartPoints { get; private set; } = new List<Rectangle>();
        internal List<EndPoint> EndPoints { get; private set; } = new List<EndPoint>();
        internal List<Rectangle> Walls { get; private set; } = new List<Rectangle>();

        Texture2D texPixel, texCircle, texCorner, texHexagon, texSquare, texSun, texElimination;
        Texture2D[] texEndPoint = new Texture2D[2];
        Texture2D[] texTriangle = new Texture2D[3];
        Texture2D[] texTetris = new Texture2D[2];
        Dictionary<int, RenderTarget2D> renderedTetrisTextures = new Dictionary<int, RenderTarget2D>();
        SpriteFont font;
        
        public Point PuzzleDimensions { get; private set; }     // Size of the puzzle in blocks
        public Rectangle PuzzleConfig { get; private set; }     // Location on screen and size of the puzzle in pixels
        public int LineWidth { get; private set; }              // Width of the solution line in pixels
        public Point LineWidthPoint { get; private set; }       // Point with both X and Y set to the lineWith
        public Point HalfLineWidthPoint { get; private set; }   // Point with both X and Y set to the lineWidth / 2
        public int BlockWidth { get; private set; }             // Size of the block in pixels
        public Point BlockSizePoint { get; private set; }       // Point with X and Y set to the blockWidth
        public int NodePadding { get; private set; }            // Distance between two nodes on screen in pixel, is equal to (lineWidth + blockWidth)
        private Point tetrisTextureSize;

        public PanelRenderer(Puzzle panel, Point screenSize, Dictionary<string, Texture2D> TextureProvider, Dictionary<string, SpriteFont> FontProvider, GraphicsDevice graphicsDevice)
        {
            this.panel = panel;
            this.screenSize = screenSize;
            GraphicsDevice = graphicsDevice;
            font = FontProvider["font/fnt_mono_digits"];
            LoadContent(TextureProvider);
            InitializePanel();
            if (panel != null)
                SetColorScheme(panel.BackgroundColor, panel.WallsColor);
        }
        private void LoadContent(Dictionary<string, Texture2D> TextureProvider)
        {
            texPixel = TextureProvider["img/pixel"];
            texCircle = TextureProvider["img/circle"];
            texCorner = TextureProvider["img/corner"];
            texEndPoint[0] = TextureProvider["img/ending_left"];
            texEndPoint[1] = TextureProvider["img/ending_top"];
            // Puzzle rules textures
            texHexagon = TextureProvider["img/hexagon"];
            texSquare = TextureProvider["img/square"];
            texSun = TextureProvider["img/sun"];
            texElimination = TextureProvider["img/elimination"];
            for (int i = 0; i < 3; i++)
                texTriangle[i] = TextureProvider[$"img/triangle{i + 1}"];
            texTetris[0] = TextureProvider["img/tetris"];
            texTetris[1] = TextureProvider["img/tetris_sub"];
        }

        public bool SetScreenSize(Point newScreenSize)
        {
            if(screenSize != newScreenSize)
            {
                screenSize = newScreenSize;
                InitializePanel();
                return true;
            }
            return false;
        }
        public bool SetPanel(Puzzle newPanel)
        {
            if(panel == null || !panel.Equals(newPanel))
            {
                panel = newPanel;
                InitializePanel();
                SetColorScheme(panel.BackgroundColor, panel.WallsColor);
                return true;
            }
            return false;
        }

        public void SetColorScheme(Color backgroundColor, Color wallsColor)
        {
            BackgroundColor = backgroundColor;
            WallColor = wallsColor;
            BorderColor = new Color(backgroundColor.R / 4, backgroundColor.G / 4, backgroundColor.B / 4);
        }

        private bool InitializePanel()
        {
            if (panel != null)
            {
                Walls.Clear();
                StartPoints.Clear();
                EndPoints.Clear();
                renderedTetrisTextures.Clear();

                PuzzleDimensions = new Point(panel.Width, panel.Height);

                // Calculation of puzzle sizes for current screen size
                int width = screenSize.X;
                int height = screenSize.Y;

                int maxPuzzleDimension = Math.Max(PuzzleDimensions.X, PuzzleDimensions.Y);

                int screenMinSize = Math.Min(width, height);
                int puzzleMaxSize = (int) (screenMinSize * PuzzleSpaceRatio(maxPuzzleDimension));
                LineWidth = (int) (puzzleMaxSize * LineSizeRatio(maxPuzzleDimension));
                BlockWidth = (int) (puzzleMaxSize * BlockSizeRatio(maxPuzzleDimension));
                HalfLineWidthPoint = new Point(LineWidth / 2);
                LineWidthPoint = new Point(LineWidth);
                BlockSizePoint = new Point(BlockWidth);

                int endAppendixLength = (int) (BlockWidth * endPointLegth);

                int puzzleWidth = LineWidth * (PuzzleDimensions.X + 1) + BlockWidth * PuzzleDimensions.X;
                int puzzleHeight = LineWidth * (PuzzleDimensions.Y + 1) + BlockWidth * PuzzleDimensions.Y;

                int xMargin = (width - puzzleWidth) / 2;
                int yMargin = (height - puzzleHeight) / 2;
                Point margins = new Point(xMargin, yMargin);

                PuzzleConfig = new Rectangle(xMargin, yMargin, puzzleWidth, puzzleHeight);

                NodePadding = LineWidth + BlockWidth;

                // Creating walls hitboxes
                CreateHorizontalWalls(false);   // Top walls
                CreateHorizontalWalls(true);    // Bottom walls
                CreateVerticalWalls(false);     // Left walls
                CreateVerticalWalls(true);      // Right walls

                for (int i = 0; i < PuzzleDimensions.X; i++)
                    for (int j = 0; j < PuzzleDimensions.Y; j++)
                        Walls.Add(new Rectangle(xMargin + LineWidth * (i + 1) + BlockWidth * i, yMargin + LineWidth * (j + 1) + BlockWidth * j, BlockWidth, BlockWidth));

                // Creating walls for broken edges
                var brokenEdges = panel.Edges.Where(x => x.State == EdgeState.Broken);
                foreach (Edge edge in brokenEdges)
                {
                    Point nodeA = NodeIdToPoint(edge.Id % 100).Multiply(NodePadding) + margins;
                    Point nodeB = NodeIdToPoint(edge.Id / 100).Multiply(NodePadding) + margins;
                    Point middle = (nodeA + nodeB).Divide(2);
                    Walls.Add(new Rectangle(middle, new Point(LineWidth)));
                }

                // Creating hitboxes for starting nodes
                foreach (Point start in GetStartNodes())
                    StartPoints.Add(new Rectangle(xMargin + start.X * NodePadding - LineWidth, yMargin + start.Y * NodePadding - LineWidth, LineWidth * 3, LineWidth * 3));

                // Generate textures for tetris rules
                CreateTetrisRuleTextures();

                #region === Inner methods region ===
                // Returns a coef k: Total free space in pixels * k = puzzle size in pixels
                float PuzzleSpaceRatio(float puzzleDimension) => (float) (-0.0005 * Math.Pow(puzzleDimension, 4) + 0.0082 * Math.Pow(puzzleDimension, 3) - 0.0439 * Math.Pow(puzzleDimension, 2) + 0.1011 * puzzleDimension + 0.6875);
                // Returns a coef k: Puzzle size in pixels * k = block size in pixels
                float BlockSizeRatio(float puzzleDimension) => (float) (0.8563 * Math.Pow(puzzleDimension, -1.134));
                // Returns a coef k: Puzzle size in pixels * k = line width in pixels
                float LineSizeRatio(float puzzleDimension) => -0.0064f * puzzleDimension + 0.0859f;

                IEnumerable<Point> GetStartNodes() => panel.Nodes.Where(x => x.State == NodeState.Start).Select(x => NodeIdToPoint(x.Id));
                IEnumerable<Point> GetEndNodesTop()
                {
                    for (int i = 1; i < panel.Width; i++)
                        if (panel.Nodes[i].State == NodeState.Exit)
                            yield return new Point(i, 0);
                }
                IEnumerable<Point> GetEndNodesBot()
                {
                    for (int i = 1; i < panel.Width; i++)
                    {
                        int index = panel.Height * (panel.Width + 1) + i;
                        if (panel.Nodes[index].State == NodeState.Exit)
                            yield return new Point(i, panel.Height);
                    }
                }
                IEnumerable<Point> GetEndNodesLeft()
                {
                    for (int j = 0; j < panel.Height + 1; j++)
                    {
                        int index = j * (panel.Width + 1);
                        if (panel.Nodes[index].State == NodeState.Exit)
                            yield return new Point(0, j);
                    }
                }
                IEnumerable<Point> GetEndNodesRight()
                {
                    for (int j = 0; j < panel.Height + 1; j++)
                    {
                        int index = j * (panel.Width + 1) + panel.Width;
                        if (panel.Nodes[index].State == NodeState.Exit)
                            yield return new Point(panel.Width, j);
                    }
                }
                void CreateHorizontalWalls(bool isBottom)
                {
                    int yStartPoint = isBottom ? yMargin + puzzleHeight : 0;
                    var ends = isBottom ? GetEndNodesBot() : GetEndNodesTop();

                    if (ends.Count() == 0)
                        Walls.Add(new Rectangle(0, yStartPoint, width, yMargin));
                    else
                    {
                        Walls.Add(new Rectangle(0, yStartPoint + (isBottom ? endAppendixLength : 0), width, yMargin - endAppendixLength));

                        int lastXPoint = 0;
                        foreach (Point endPoint in ends)
                        {
                            int x = xMargin + endPoint.X * (NodePadding);
                            Walls.Add(new Rectangle(lastXPoint, isBottom ? yStartPoint : (yMargin - endAppendixLength), x - lastXPoint, endAppendixLength));
                            EndPoints.Add(new EndPoint(new Rectangle(x, isBottom ? yStartPoint + endAppendixLength - LineWidth : (yMargin - endAppendixLength), LineWidth, LineWidth), isBottom ? Facing.Down : Facing.Up));
                            lastXPoint = x + LineWidth;
                        }
                        Walls.Add(new Rectangle(lastXPoint, isBottom ? yStartPoint : (yMargin - endAppendixLength), width - lastXPoint, endAppendixLength));
                    }
                }
                void CreateVerticalWalls(bool isRight)
                {
                    int xStartPoint = isRight ? xMargin + puzzleWidth : 0;
                    var ends = isRight ? GetEndNodesRight() : GetEndNodesLeft();

                    if (ends.Count() == 0)
                        Walls.Add(new Rectangle(xStartPoint, 0, xMargin, height));
                    else
                    {
                        Walls.Add(new Rectangle(xStartPoint + (isRight ? endAppendixLength : 0), 0, xMargin - endAppendixLength, height));

                        int lastYPoint = 0;
                        foreach (Point endPoint in ends)
                        {
                            int y = yMargin + endPoint.Y * (NodePadding);
                            Walls.Add(new Rectangle(isRight ? xStartPoint : (xMargin - endAppendixLength), lastYPoint, endAppendixLength, y - lastYPoint));
                            EndPoints.Add(new EndPoint(new Rectangle(isRight ? xStartPoint + endAppendixLength - LineWidth : xMargin - endAppendixLength, y, LineWidth, LineWidth), isRight ? Facing.Right : Facing.Left));
                            lastYPoint = y + LineWidth;
                        }
                        Walls.Add(new Rectangle(isRight ? xStartPoint : (xMargin - endAppendixLength), lastYPoint, endAppendixLength, height - lastYPoint));
                    }
                }
                void CreateTetrisRuleTextures()
                {
                    // Render shape into texture for each tetris rule
                    var tetrisBlocks = panel.Blocks.Where(x => x.Rule is TetrisRule).ToList();
                    if (tetrisBlocks.Count > 0)
                    {
                        int maxDimension = tetrisBlocks.Select(x => x.Rule as TetrisRule).Select(x => Math.Max(x.Shape.GetLength(0), x.Shape.GetLength(1))).Max();
                        if (maxDimension < 3)
                            maxDimension++;
                        tetrisTextureSize = new Point((maxDimension + 1) * texTetris[0].Width);
                        foreach (Block block in tetrisBlocks)
                        {
                            TetrisRule rule = block.Rule as TetrisRule;
                            bool[,] shape = rule.Shape;
                            // If shape is rotatable, then rotate it randomly before render
                            if (rule is TetrisRotatableRule r)
                                shape = TetrisRotatableRule.RotateShapeCW(shape, rnd.Next(0, 4));
                            // Draw shape on a texture
                            RenderTarget2D texture = CreateTetrisTexture(shape, rule is TetrisRotatableRule, rule.IsSubtractive);
                            // Save it to the dictionary
                            renderedTetrisTextures.Add(block.Id, texture);
                        }
                    }
                }
                #endregion 
                return true;
            }
            return false;
        }
        private Point NodeIdToPoint(int nodeId)
        {
            int y = nodeId / (panel.Width + 1);
            int x = nodeId - y * (panel.Width + 1);
            return new Point(x, y);
        }

        public void RenderPanelToTexture(RenderTarget2D canvas, bool drawPanelSeed = false)
        {
            using (SpriteBatch spriteBatch = new SpriteBatch(GraphicsDevice))
            {
                // Set the render target
                GraphicsDevice.SetRenderTarget(canvas);

                GraphicsDevice.Clear(BackgroundColor);
                spriteBatch.Begin(SpriteSortMode.Deferred);

                foreach (var wall in Walls)
                    spriteBatch.Draw(texPixel, wall, WallColor);

                DrawRoundedCorners(spriteBatch);

                foreach (var end in EndPoints)
                    end.Draw(spriteBatch, WallColor, texEndPoint);

                foreach (var start in StartPoints)
                {
                    float scale = (float) start.Width / texCircle.Width;
                    spriteBatch.Draw(texCircle, start.Center.ToVector2(), null, BackgroundColor, 0, texCircle.Bounds.Center.ToVector2(), scale, SpriteEffects.None, 0);
                }

                DrawAllRules(spriteBatch);
                DrawBorders(spriteBatch);

                if (drawPanelSeed && panel.Seed >= 0)
                    DrawSeed(spriteBatch);

                spriteBatch.End();
                // Drop the render target
                GraphicsDevice.SetRenderTarget(null);
            }
        }
        public void RenderErrorsToTexture(RenderTarget2D canvas, IEnumerable<Error> errors, bool isEliminated)
        {
            using (SpriteBatch spriteBatch = new SpriteBatch(GraphicsDevice))
            {
                var nodes = errors.Select(x => x.Source).OfType<Node>();
                var edges = errors.Select(x => x.Source).OfType<Edge>();
                var blocks = errors.Select(x => x.Source).OfType<Block>();

                // Set the render target
                GraphicsDevice.SetRenderTarget(canvas);

                // Draw rules onto the texture
                GraphicsDevice.Clear(Color.Transparent);
                spriteBatch.Begin(SpriteSortMode.Texture);

                Color fillColor = isEliminated ? Color.Gray * 0.7f : Color.Red;

                DrawMarkedNodes(spriteBatch, nodes, fillColor);
                DrawMarkedEdges(spriteBatch, edges, fillColor);
                DrawColoredSquares(spriteBatch, blocks, fillColor);
                DrawSuns(spriteBatch, blocks, fillColor);
                DrawEliminations(spriteBatch, blocks, fillColor);
                DrawTriangles(spriteBatch, blocks, fillColor);
                DrawTetrises(spriteBatch, blocks, fillColor);

                spriteBatch.End();
                // Drop the render target
                GraphicsDevice.SetRenderTarget(null);
            }
        }

        private void DrawRoundedCorners(SpriteBatch spriteBatch)
        {
            if (panel.TopLeftNode.State != NodeState.Exit)
                spriteBatch.Draw(texCorner, new Rectangle(PuzzleConfig.Location, LineWidthPoint), null, WallColor, 0, Vector2.Zero, SpriteEffects.None, 0);
            if (panel.TopRightNode.State != NodeState.Exit)
                spriteBatch.Draw(texCorner, new Rectangle(PuzzleConfig.Location + new Point(PuzzleConfig.Width - LineWidth, 0), LineWidthPoint), null, WallColor, 0, Vector2.Zero, SpriteEffects.FlipHorizontally, 0);
            if (panel.BottomLeftNode.State != NodeState.Exit)
                spriteBatch.Draw(texCorner, new Rectangle(PuzzleConfig.Location + new Point(0, PuzzleConfig.Height - LineWidth), LineWidthPoint), null, WallColor, 0, Vector2.Zero, SpriteEffects.FlipVertically, 0);
            if (panel.BottomRightNode.State != NodeState.Exit)
                spriteBatch.Draw(texCorner, new Rectangle(PuzzleConfig.Location + PuzzleConfig.Size - LineWidthPoint, LineWidthPoint), null, WallColor, 0, Vector2.Zero, SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically, 0);
        }
        private void DrawBorders(SpriteBatch spriteBatch)
        {
            int borderWidth = (int) (Math.Min(screenSize.X, screenSize.Y) * bordersWidth);
            spriteBatch.Draw(texPixel, new Rectangle(0, 0, screenSize.X, borderWidth), BorderColor);
            spriteBatch.Draw(texPixel, new Rectangle(0, 0, borderWidth, screenSize.Y), BorderColor);
            spriteBatch.Draw(texPixel, new Rectangle(screenSize.X - borderWidth, 0, borderWidth, screenSize.Y), BorderColor);
            spriteBatch.Draw(texPixel, new Rectangle(0, screenSize.Y - borderWidth, screenSize.X, borderWidth), BorderColor);
        }
        private void DrawSeed(SpriteBatch spriteBatch)
        {
            float targetHeight = (int) (Math.Min(screenSize.X, screenSize.Y) * bordersWidth);
            Vector2 textSize = font.MeasureString(panel.Seed.ToString("D9"));
            float scale = targetHeight / textSize.Y;
            textSize *= scale;

            // Pick color that has better contrast
            int getColorBrightness (Color c) => (299 * c.R + 587 * c.G + 114 * c.B) / 1000;

            int brightnessColBorder  = getColorBrightness(BorderColor);
            int brightnessColButtons = getColorBrightness(panel.ButtonsColor);
            int brightnessColWalls   = getColorBrightness(panel.WallsColor);
            Color seedColor = Math.Abs(brightnessColButtons - brightnessColBorder) > Math.Abs(brightnessColWalls - brightnessColBorder) 
                ? panel.ButtonsColor 
                : panel.WallsColor;

            spriteBatch.DrawString(font, panel.Seed.ToString("D9"), new Vector2(screenSize.X / 2 - textSize.X / 2, 0), seedColor, 0, Vector2.Zero, scale, SpriteEffects.None, 0);
        }
        private void DrawAllRules(SpriteBatch sb)
        {
            var allBlocks = panel.Blocks;

            DrawMarkedNodes(sb, panel.Nodes);
            DrawMarkedEdges(sb, panel.Edges);
            DrawColoredSquares(sb, allBlocks);
            DrawSuns(sb, allBlocks);
            DrawEliminations(sb, allBlocks);
            DrawTriangles(sb, allBlocks);
            DrawTetrises(sb, allBlocks);
        }
        private void DrawMarkedNodes(SpriteBatch spriteBatch, IEnumerable<Node> allNodes, Color? fillColor = null)
        {
            var markedNodes = allNodes.Where(x => x.State == NodeState.Marked);
            foreach (var node in markedNodes)
            {
                Point position = NodeIdToPoint(node.Id).Multiply(NodePadding) + PuzzleConfig.Location;
                spriteBatch.Draw(texHexagon, new Rectangle(position, LineWidthPoint), fillColor ?? node.Color ?? Color.Black);
            }
        }
        private void DrawMarkedEdges(SpriteBatch spriteBatch, IEnumerable<Edge> allEdges, Color? fillColor = null)
        {
            var markedEdges = allEdges.Where(x => x.State == EdgeState.Marked);
            foreach (var edge in markedEdges)
            {
                Point pA = NodeIdToPoint(edge.NodeA.Id).Multiply(NodePadding);
                Point pB = NodeIdToPoint(edge.NodeB.Id).Multiply(NodePadding);
                Point position = (pA + pB).Divide(2) + PuzzleConfig.Location;
                spriteBatch.Draw(texHexagon, new Rectangle(position, LineWidthPoint), fillColor ?? edge.Color ?? Color.Black);
            }
        }
        private Point BlockPositionToOnScreenLocation(int x, int y) => new Point(PuzzleConfig.X + x * NodePadding + LineWidth, PuzzleConfig.Y + y * NodePadding + LineWidth);
        private void DrawColoredSquares(SpriteBatch spriteBatch, IEnumerable<Block> allBlocks, Color? fillColor = null)
        {
            var squareRuleBlocks = allBlocks.Where(x => x.Rule is ColoredSquareRule);
            foreach (var block in squareRuleBlocks)
            {
                Color color = (block.Rule as ColoredSquareRule).Color.Value;
                spriteBatch.Draw(texSquare, new Rectangle(BlockPositionToOnScreenLocation(block.X, block.Y), BlockSizePoint), fillColor ?? color);
            }
        }
        private void DrawSuns(SpriteBatch spriteBatch, IEnumerable<Block> allBlocks, Color? fillColor = null)
        {
            var sunRuleBlocks = allBlocks.Where(x => x.Rule is SunPairRule);
            foreach (var block in sunRuleBlocks)
            {
                Color color = (block.Rule as SunPairRule).Color.Value;
                spriteBatch.Draw(texSun, new Rectangle(BlockPositionToOnScreenLocation(block.X, block.Y), BlockSizePoint), fillColor ?? color);
            }
        }
        private void DrawEliminations(SpriteBatch spriteBatch, IEnumerable<Block> allBlocks, Color? fillColor = null)
        {
            var eliminationRuleBlocks = allBlocks.Where(x => x.Rule is EliminationRule);
            foreach (var block in eliminationRuleBlocks)
            {
                Color color = (block.Rule as EliminationRule).Color ?? Color.White;
                spriteBatch.Draw(texElimination, new Rectangle(BlockPositionToOnScreenLocation(block.X, block.Y), BlockSizePoint), fillColor ?? color);
            }
        }
        private void DrawTriangles(SpriteBatch spriteBatch, IEnumerable<Block> allBlocks, Color? fillColor = null)
        {
            var triangleRuleBlocks = allBlocks.Where(x => x.Rule is TriangleRule);
            foreach (var block in triangleRuleBlocks)
            {
                int texIndex = (block.Rule as TriangleRule).Power - 1;
                spriteBatch.Draw(texTriangle[texIndex], new Rectangle(BlockPositionToOnScreenLocation(block.X, block.Y), BlockSizePoint), fillColor ?? Color.Gold);
            }
        }
        private void DrawTetrises(SpriteBatch spriteBatch, IEnumerable<Block> allBlocks, Color? fillColor = null)
        {
            var tetrisBlocks = allBlocks.Where(x => x.Rule is TetrisRule);
            foreach (var block in tetrisBlocks)
            {
                RenderTarget2D texture = renderedTetrisTextures[block.Id];
                TetrisRule rule = block.Rule as TetrisRule;
                spriteBatch.Draw(texture, new Rectangle(BlockPositionToOnScreenLocation(block.X, block.Y), BlockSizePoint), fillColor ?? rule.Color ?? (rule.IsSubtractive ? Color.Blue : Color.Gold));
            }
        }
        private RenderTarget2D CreateTetrisTexture(bool[,] shape, bool isRotatable, bool isSubtractive)
        {
            int texW = texTetris[0].Width;
            int shapeW = shape.GetLength(0);
            int shapeH = shape.GetLength(1);
            int xMargin = (tetrisTextureSize.X - shapeW * texW) / 2;
            int yMargin = (tetrisTextureSize.Y - shapeH * texW) / 2;
            RenderTarget2D canvas = new RenderTarget2D(GraphicsDevice, tetrisTextureSize.X, tetrisTextureSize.Y);
            GraphicsDevice.SetRenderTarget(canvas);
            GraphicsDevice.Clear(Color.Transparent);
            using (SpriteBatch batch = new SpriteBatch(GraphicsDevice))
            {
                batch.Begin();

                for (int i = 0; i < shapeW; i++)
                    for (int j = 0; j < shapeH; j++)
                        if (shape[i, j] == true)
                            batch.Draw(texTetris[isSubtractive ? 1 : 0], new Vector2(xMargin + i * texW, yMargin + j * texW), Color.White);

                batch.End();

                if (isRotatable)
                {
                    RenderTarget2D canvasRotated = new RenderTarget2D(GraphicsDevice, canvas.Width, canvas.Height);
                    GraphicsDevice.SetRenderTarget(canvasRotated);
                    GraphicsDevice.Clear(Color.Transparent);
                    batch.Begin();
                    batch.Draw(canvas, canvasRotated.Bounds.Center.ToVector2(), null, Color.White, MathHelper.ToRadians(-30), canvas.Bounds.Center.ToVector2(), 1f, SpriteEffects.None, 0);
                    batch.End();
                    canvas = canvasRotated;
                }
            }

            GraphicsDevice.SetRenderTarget(null);
            return canvas;   
        }
    }
}
