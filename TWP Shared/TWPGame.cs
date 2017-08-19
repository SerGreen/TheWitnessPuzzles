using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using System;
using System.Collections.Generic;
using System.Linq;
using TWP_Shared;
using TheWitnessPuzzles;
using System.Threading;

namespace TWP_Shared
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class TWPGame : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        const int fullscreenCooldownMax = 120;
        int fullscreenCooldown = 0;

        protected int moveStep = 5;
        protected float moveSensitivity = 0.5f;

        Color backgroundColor = Color.CornflowerBlue;
        Color wallColor = Color.DarkSlateGray;
        Color borderColor = Color.DimGray;

        Puzzle panel = null;
        SolutionLine line = null;
        SolutionLine lineMirror = null;
        PanelState panelState = new PanelState();

        List<Rectangle> startPoints = new List<Rectangle>();
        List<EndPoint> endPoints = new List<EndPoint>();

        List<Rectangle> walls = new List<Rectangle>();

        Texture2D texPixel; //base for the line texture
        Texture2D texCircle = null;
        Texture2D texCorner = null;
        Texture2D texEndPoint = null;
        Texture2D texHexagon = null;
        Texture2D texSquare = null;
        Texture2D texSun = null;
        Texture2D texElimination = null;
        Texture2D[] texTriangle = new Texture2D[3];

        RenderTarget2D backgroundTexture;
        RenderTarget2D linesFadeTexture;
        RenderTarget2D errorsBlinkTexture;

        Point puzzleDimensions;
        Rectangle puzzleConfig;
        int lineWidth;
        Point lineWidthPoint;
        Point halfLineWidthPoint;
        int nodePadding;
        int blockWidth;
        Point blockSizePoint;

        bool isMobile;

        public TWPGame(bool isMobile, Puzzle panel = null)
        {
            panel = new SymmetryPuzzle(5, 5, true, Color.Aqua, Color.Yellow);
            panel.Nodes[25].SetState(NodeState.Start);
            panel.Nodes[28].SetState(NodeState.Start);
            panel.Nodes[7].SetState(NodeState.Start);
            panel.Nodes[10].SetState(NodeState.Start);
            panel.Nodes[1].SetState(NodeState.Exit);
            panel.Nodes[3].SetState(NodeState.Exit);
            panel.Nodes[23].SetState(NodeState.Exit);
            panel.Nodes[22].SetState(NodeState.Exit);
            panel.Nodes[12].SetState(NodeState.Exit);
            panel.Nodes[0].SetState(NodeState.Exit);
            panel.Nodes[20].SetState(NodeState.Exit);
            panel.Nodes[9].SetState(NodeState.Exit);
            panel.Nodes[24].SetState(NodeState.Exit);
            panel.Nodes[32].SetState(NodeState.Exit);
            panel.Edges.Find(x => x.Id == 814)?.SetState(EdgeState.Broken);
            panel.Edges.Find(x => x.Id == 1617)?.SetState(EdgeState.Broken);

            panel.Nodes[14].SetStateAndColor(NodeState.Marked, Color.Yellow);
            panel.Nodes[18].SetState(NodeState.Marked);
            panel.Edges.Find(x => x.Id == 2021)?.SetStateAndColor(EdgeState.Marked, Color.Aqua);

            panel.Grid[2, 0].Rule = new ColoredSquareRule(Color.Magenta);
            panel.Grid[1, 1].Rule = new SunPairRule(Color.Magenta);
            panel.Grid[4, 4].Rule = new ColoredSquareRule(Color.Lime);
            panel.Grid[1, 4].Rule = new SunPairRule(Color.Lime);
            panel.Grid[2, 1].Rule = new TriangleRule(1);
            panel.Grid[3, 2].Rule = new TriangleRule(2);
            panel.Grid[3, 1].Rule = new TriangleRule(3);
            panel.Grid[4, 2].Rule = new EliminationRule();
            panel.Grid[2, 3].Rule = new EliminationRule(Color.Magenta);

            this.isMobile = isMobile;
            this.panel = panel;
            puzzleDimensions = new Point(panel.Width, panel.Height);

            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            if (isMobile)
            {
                graphics.IsFullScreen = true;
                graphics.PreferredBackBufferWidth = 480;
                graphics.PreferredBackBufferHeight = 800;
                graphics.SupportedOrientations = DisplayOrientation.Portrait;
            }
            else
                IsMouseVisible = true;

            //graphics.PreferredBackBufferWidth = 320;
            //graphics.PreferredBackBufferHeight = 400;

            TouchPanel.EnabledGestures = GestureType.Tap | GestureType.FreeDrag;
        }

        private void InitPanel()
        {
            // Calculation of puzzle sizes for current screen size
            int width = GraphicsDevice.Viewport.Width;
            int height = GraphicsDevice.Viewport.Height;

            int maxPuzzleDimension = Math.Max(puzzleDimensions.X, puzzleDimensions.Y);

            int screenMinSize = Math.Min(width, height);
            int puzzleMaxSize = (int) (screenMinSize * PuzzleSpaceRatio(maxPuzzleDimension));
            lineWidth = (int) (puzzleMaxSize * LineSizeRatio(maxPuzzleDimension));
            blockWidth = (int) (puzzleMaxSize * BlockSizeRatio(maxPuzzleDimension));
            halfLineWidthPoint = new Point(lineWidth / 2);
            lineWidthPoint = new Point(lineWidth);
            blockSizePoint = new Point(blockWidth);

            int endAppendixLength = blockWidth / 3;

            int puzzleWidth = lineWidth * (puzzleDimensions.X + 1) + blockWidth * puzzleDimensions.X;
            int puzzleHeight = lineWidth * (puzzleDimensions.Y + 1) + blockWidth * puzzleDimensions.Y;

            int xMargin = (width - puzzleWidth) / 2;
            int yMargin = (height - puzzleHeight) / 2;
            Point margins = new Point(xMargin, yMargin);

            puzzleConfig = new Rectangle(xMargin, yMargin, puzzleWidth, puzzleHeight);

            nodePadding = lineWidth + blockWidth;

            // Creating walls hitboxes
            CreateHorizontalWalls(false);   // Top walls
            CreateHorizontalWalls(true);    // Bottom walls
            CreateVerticalWalls(false);     // Left walls
            CreateVerticalWalls(true);      // Right walls

            for (int i = 0; i < puzzleDimensions.X; i++)
                for (int j = 0; j < puzzleDimensions.Y; j++)
                    walls.Add(new Rectangle(xMargin + lineWidth * (i + 1) + blockWidth * i, yMargin + lineWidth * (j + 1) + blockWidth * j, blockWidth, blockWidth));

            // Creating walls for broken edges
            var brokenEdges = panel.Edges.Where(x => x.State == EdgeState.Broken);
            foreach (Edge edge in brokenEdges)
            {
                Point nodeA = NodeIdToPoint(edge.Id % 100).Multiply(nodePadding) + margins;
                Point nodeB = NodeIdToPoint(edge.Id / 100).Multiply(nodePadding) + margins;
                Point middle = (nodeA + nodeB).Divide(2);
                walls.Add(new Rectangle(middle, new Point(lineWidth)));
            }


            // Creating hitboxes for starting nodes
            foreach (Point start in GetStartNodes())
                startPoints.Add(new Rectangle(xMargin + start.X * nodePadding - lineWidth, yMargin + start.Y * nodePadding - lineWidth, lineWidth * 3, lineWidth * 3));

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
                    walls.Add(new Rectangle(0, yStartPoint, width, yMargin));
                else
                {
                    walls.Add(new Rectangle(0, yStartPoint + (isBottom ? endAppendixLength : 0), width, yMargin - endAppendixLength));

                    int lastXPoint = 0;
                    foreach (Point endPoint in ends)
                    {
                        int x = xMargin + endPoint.X * (nodePadding);
                        walls.Add(new Rectangle(lastXPoint, isBottom ? yStartPoint : (yMargin - endAppendixLength), x - lastXPoint, endAppendixLength));
                        endPoints.Add(new EndPoint(new Rectangle(x, isBottom ? yStartPoint + endAppendixLength - lineWidth : (yMargin - endAppendixLength), lineWidth, lineWidth), isBottom ? Facing.Down : Facing.Up));
                        lastXPoint = x + lineWidth;
                    }
                    walls.Add(new Rectangle(lastXPoint, isBottom ? yStartPoint : (yMargin - endAppendixLength), width - lastXPoint, endAppendixLength));
                }
            }
            void CreateVerticalWalls(bool isRight)
            {
                int xStartPoint = isRight ? xMargin + puzzleWidth : 0;
                var ends = isRight ? GetEndNodesRight() : GetEndNodesLeft();

                if (ends.Count() == 0)
                    walls.Add(new Rectangle(xStartPoint, 0, xMargin, height));
                else
                {
                    walls.Add(new Rectangle(xStartPoint + (isRight ? endAppendixLength : 0), 0, xMargin - endAppendixLength, height));

                    int lastYPoint = 0;
                    foreach (Point endPoint in ends)
                    {
                        int y = yMargin + endPoint.Y * (nodePadding);
                        walls.Add(new Rectangle(isRight ? xStartPoint : (xMargin - endAppendixLength), lastYPoint, endAppendixLength, y - lastYPoint));
                        endPoints.Add(new EndPoint(new Rectangle(isRight ? xStartPoint + endAppendixLength - lineWidth : xMargin - endAppendixLength, y, lineWidth, lineWidth), isRight ? Facing.Right : Facing.Left));
                        lastYPoint = y + lineWidth;
                    }
                    walls.Add(new Rectangle(isRight ? xStartPoint : (xMargin - endAppendixLength), lastYPoint, endAppendixLength, height - lastYPoint));
                }
            }
            #endregion
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            
            // InitPanel is in LoadContent() because GraphicsDevice is not created yet in Initialize() on Android
            InitPanel();

            // One pixel sized white texture, used to draw rectangles of any size
            texPixel = new Texture2D(GraphicsDevice, 1, 1);
            texPixel.SetData(new Color[] { Color.White });
            texCircle = Content.Load<Texture2D>("img/twp_circle");
            texCorner = Content.Load<Texture2D>("img/twp_corner");
            texEndPoint = Content.Load<Texture2D>("img/twp_ending");
            // Puzzle rules textures
            texHexagon = Content.Load<Texture2D>("img/twp_hexagon");
            texSquare = Content.Load<Texture2D>("img/twp_square");
            texSun = Content.Load<Texture2D>("img/twp_sun");
            texElimination = Content.Load<Texture2D>("img/twp_elimination");
            for (int i = 0; i < 3; i++)
                texTriangle[i] = Content.Load<Texture2D>($"img/twp_triangle{i + 1}");

            // Fullscreen textures for 1. background, 2. fading solution lines and 3. red blinking rules with error highlighting
            backgroundTexture = new RenderTarget2D(GraphicsDevice, GraphicsDevice.PresentationParameters.BackBufferWidth, GraphicsDevice.PresentationParameters.BackBufferHeight, false, GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.Depth24);
            linesFadeTexture = new RenderTarget2D(GraphicsDevice, GraphicsDevice.PresentationParameters.BackBufferWidth, GraphicsDevice.PresentationParameters.BackBufferHeight, false, GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.Depth24);
            errorsBlinkTexture = new RenderTarget2D(GraphicsDevice, GraphicsDevice.PresentationParameters.BackBufferWidth, GraphicsDevice.PresentationParameters.BackBufferHeight, false, GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.Depth24);

            // Draw static parts of panel onto the texture
            RenderBackgroundTexture();
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // Toggle fullscreen if Alt+Enter was pressed
            HandleFullscreenToggle();
            // Line activation and line completing logic
            HandleScreenTap();

            Vector2 moveVector = GetMoveVector();
            MoveLine(moveVector);

            panelState.Update();

            base.Update(gameTime);
        }

        private void MoveLine(Vector2 moveVector)
        {
            if (line != null)
            {
                if (moveVector != Vector2.Zero)
                {
                    Vector2 firstMove, secondMove;
                    if (Math.Abs(moveVector.X) > Math.Abs(moveVector.Y))
                    {
                        firstMove = new Vector2(moveVector.X, 0);
                        secondMove = new Vector2(0, moveVector.Y);
                    }
                    else
                    {
                        firstMove = new Vector2(0, moveVector.Y);
                        secondMove = new Vector2(moveVector.X, 0);
                    }

                    IEnumerable<Rectangle> hitboxes;
                    if (panel is SymmetryPuzzle)
                        hitboxes = line.Hitboxes.Concat(lineMirror.Hitboxes).Concat(walls);
                    else
                        hitboxes = line.Hitboxes.Concat(walls);

                    Func<Vector2, IEnumerable<Rectangle>, bool> moveFunc;
                    if (panel is SymmetryPuzzle)
                        moveFunc = MoveBothLines;
                    else
                        moveFunc = MoveOneLine;

                    if (!moveFunc(firstMove, hitboxes))
                    {
                        Vector2 cornerMove = line.GetMoveVectorNearCorner(moveVector, walls);
                        if (cornerMove != Vector2.Zero)
                            moveFunc(cornerMove, hitboxes);
                        else
                            moveFunc(secondMove, hitboxes);
                    }
                }
            }

            bool MoveOneLine(Vector2 moveVect, IEnumerable<Rectangle> hitboxes) => line.Move(moveVect, hitboxes);
            bool MoveBothLines(Vector2 moveVect, IEnumerable<Rectangle> hitboxes)
            {
                // Move first line
                if (line.Move(moveVect, hitboxes.Append(lineMirror.Head)))
                {
                    // If first line succeeded, then move second line
                    Vector2 mirroredVector;
                    if ((panel as SymmetryPuzzle).Y_Mirrored)
                        mirroredVector = -moveVect;
                    else
                        mirroredVector = new Vector2(-moveVect.X, moveVect.Y);

                    if (!lineMirror.Move(mirroredVector, hitboxes.Append(line.Head)))
                    {
                        // If second line failed, move first line back
                        line.Move(-moveVect, hitboxes);
                        return false;
                    }
                    return true;
                }
                else return false;
            }
        }

        private void HandleScreenTap()
        {
            Point? tap = GetTapPosition();
            if (tap.HasValue)
                if (line == null)
                {
                    foreach (Rectangle startPoint in startPoints)
                        if (startPoint.Contains(tap.Value))
                        {
                            panelState.ResetToNeutral();
                            line = new SolutionLine(startPoint.Center - (new Point(lineWidth / 2)), lineWidth, startPoint);
                            if (panel is SymmetryPuzzle symPanel)
                                if (symPanel.Y_Mirrored)
                                {
                                    Point mirPoint = puzzleConfig.Location + puzzleConfig.Location + puzzleConfig.Size - startPoint.Center - (new Point(lineWidth / 2));
                                    Rectangle mirStartPoint = startPoints.Find(x => x.Contains(mirPoint));
                                    lineMirror = new SolutionLine(RectifyLinePosition(mirPoint), lineWidth, mirStartPoint);
                                }
                                else
                                {
                                    Point mirPoint = new Point(puzzleConfig.Location.X * 2 + puzzleConfig.Size.X - startPoint.Center.X - lineWidth / 2, startPoint.Center.Y - lineWidth / 2);
                                    Rectangle mirStartPoint = startPoints.Find(x => x.Contains(mirPoint));
                                    lineMirror = new SolutionLine(RectifyLinePosition(mirPoint), lineWidth, mirStartPoint);
                                }

                            break;
                        }
                }
                else
                {
                    // TODO code for submitting solution for error checking

                    bool error = false;
                    foreach (var endPoint in endPoints)
                        if (endPoint.IntercetionPercent(line.Head) > 0.4f)
                        {
                            Point headOffset = endPoint.Rectangle.Location - line.Head.Location;
                            MoveLine(headOffset.ToVector2());

                            error = true;
                            panelState.InvokeFadeOut(true);
                            break;
                        }

                    if (!error)
                        panelState.InvokeFadeOut(false);
                    
                    RenderLinesToTexture();
                    line = lineMirror = null;
                }
        }

        private Point RectifyLinePosition(Point pos)
        {
            Point zeroedPos = pos - puzzleConfig.Location;
            return new Point(zeroedPos.X / nodePadding * nodePadding, zeroedPos.Y / nodePadding * nodePadding) + puzzleConfig.Location;
        }

        protected virtual Point? GetTapPosition() => null;
        protected virtual Vector2 GetMoveVector() => Vector2.Zero;

        private void HandleFullscreenToggle()
        {
            if (fullscreenCooldown > 0)
                fullscreenCooldown--;

            if ((Keyboard.GetState().IsKeyDown(Keys.LeftAlt) || Keyboard.GetState().IsKeyDown(Keys.RightAlt)) && Keyboard.GetState().IsKeyDown(Keys.Enter))
            {
                graphics.ToggleFullScreen();
                graphics.ApplyChanges();
                fullscreenCooldown = fullscreenCooldownMax;
            }
        }

        private Point NodeIdToPoint(int nodeId)
        {
            int y = nodeId / (panel.Width + 1);
            int x = nodeId - y * (panel.Width + 1);
            return new Point(x, y);
        }

        #region ===== RENDER REGION =====
        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin(SpriteSortMode.Deferred);

            spriteBatch.Draw(backgroundTexture, GraphicsDevice.Viewport.Bounds, Color.White);

            DrawLines(spriteBatch);

            if (panelState.IsFading && linesFadeTexture != null)
                spriteBatch.Draw(linesFadeTexture, GraphicsDevice.Viewport.Bounds, (panelState.State == PanelStates.Error ? Color.Black : Color.White) * panelState.FadeOpacity);
            
            spriteBatch.End();
            base.Draw(gameTime);
        }

        private void DrawLines(SpriteBatch spriteBatch)
        {
            if (line != null)
                line.Draw(spriteBatch, texCircle, texPixel, panel is SymmetryPuzzle sym ? sym.MainColor : Color.White);

            if (lineMirror != null)
                lineMirror.Draw(spriteBatch, texCircle, texPixel, panel is SymmetryPuzzle sym ? sym.MirrorColor : Color.White);
        }

        private void DrawRoundedCorners()
        {
            if (panel.TopLeftNode.State != NodeState.Exit)
                spriteBatch.Draw(texCorner, new Rectangle(puzzleConfig.Location, lineWidthPoint), null, wallColor, 0, Vector2.Zero, SpriteEffects.None, 0);
            if (panel.TopRightNode.State != NodeState.Exit)
                spriteBatch.Draw(texCorner, new Rectangle(puzzleConfig.Location + new Point(puzzleConfig.Width - lineWidth, 0), lineWidthPoint), null, wallColor, 0, Vector2.Zero, SpriteEffects.FlipHorizontally, 0);
            if (panel.BottomLeftNode.State != NodeState.Exit)
                spriteBatch.Draw(texCorner, new Rectangle(puzzleConfig.Location + new Point(0, puzzleConfig.Height - lineWidth), lineWidthPoint), null, wallColor, 0, Vector2.Zero, SpriteEffects.FlipVertically, 0);
            if (panel.BottomRightNode.State != NodeState.Exit)
                spriteBatch.Draw(texCorner, new Rectangle(puzzleConfig.Location + puzzleConfig.Size - lineWidthPoint, lineWidthPoint), null, wallColor, 0, Vector2.Zero, SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically, 0);
        }
        private void DrawBorders()
        {
            int borderWidth = (int) (Math.Min(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height) * 0.03f);
            spriteBatch.Draw(texPixel, new Rectangle(0, 0, GraphicsDevice.Viewport.Width, borderWidth), borderColor);
            spriteBatch.Draw(texPixel, new Rectangle(0, 0, borderWidth, GraphicsDevice.Viewport.Height), borderColor);
            spriteBatch.Draw(texPixel, new Rectangle(GraphicsDevice.Viewport.Width - borderWidth, 0, borderWidth, GraphicsDevice.Viewport.Height), borderColor);
            spriteBatch.Draw(texPixel, new Rectangle(0, GraphicsDevice.Viewport.Height - borderWidth, GraphicsDevice.Viewport.Width, borderWidth), borderColor);
        }
        private void DrawEndPoints()
        {
            foreach (var end in endPoints)
            {
                float angle;
                switch (end.Facing)
                {
                    case Facing.Left:   angle = 0; break;
                    case Facing.Right:  angle = MathHelper.ToRadians(180); break;
                    case Facing.Up:     angle = MathHelper.ToRadians(90); break;
                    case Facing.Down:   angle = MathHelper.ToRadians(270); break;
                    default:            angle = 0; break;
                }
                float scale = (float) end.Rectangle.Width / texEndPoint.Width;
                spriteBatch.Draw(texEndPoint, end.Rectangle.Center.ToVector2(), null, wallColor, angle, texEndPoint.Bounds.Center.ToVector2(), scale, SpriteEffects.None, 0);
            }
        }

        private void RenderLinesToTexture()
        {
            // Set the render target
            GraphicsDevice.SetRenderTarget(linesFadeTexture);

            // Draw the lines
            GraphicsDevice.Clear(Color.Transparent);
            spriteBatch.Begin(SpriteSortMode.Texture);
            DrawLines(spriteBatch);
            spriteBatch.End();
            // Drop the render target
            GraphicsDevice.SetRenderTarget(null);
        }

        private void RenderBackgroundTexture()
        {
            // Set the render target
            GraphicsDevice.SetRenderTarget(backgroundTexture);

            GraphicsDevice.Clear(backgroundColor);
            spriteBatch.Begin(SpriteSortMode.Deferred);

            foreach (var wall in walls)
                spriteBatch.Draw(texPixel, wall, wallColor);

            DrawRoundedCorners();
            DrawEndPoints();

            foreach (var start in startPoints)
                spriteBatch.Draw(texCircle, start, backgroundColor);

            DrawHexagonDots();
            DrawColoredSquares();
            DrawSuns();
            DrawEliminations();
            DrawTriangles();

            DrawBorders();

            spriteBatch.End();
            // Drop the render target
            GraphicsDevice.SetRenderTarget(null);
        }

        private void DrawHexagonDots()
        {
            DrawMarkedNodes();
            DrawMarkedEdges();

            // === Methods ===
            void DrawMarkedNodes()
            {
                var markedNodes = panel.Nodes.Where(x => x.State == NodeState.Marked);
                foreach (var node in markedNodes)
                {
                    Point position = NodeIdToPoint(node.Id).Multiply(nodePadding) + puzzleConfig.Location;
                    spriteBatch.Draw(texHexagon, new Rectangle(position, lineWidthPoint), node.Color ?? Color.Black);
                }
            }
            void DrawMarkedEdges()
            {
                var markedEdges = panel.Edges.Where(x => x.State == EdgeState.Marked);
                foreach (var edge in markedEdges)
                {
                    Point pA = NodeIdToPoint(edge.NodeA.Id).Multiply(nodePadding);
                    Point pB = NodeIdToPoint(edge.NodeB.Id).Multiply(nodePadding);
                    Point position = (pA + pB).Divide(2) + puzzleConfig.Location;
                    spriteBatch.Draw(texHexagon, new Rectangle(position, lineWidthPoint), edge.Color ?? Color.Black);
                }
            }
        }
                
        private Point BlockPositionToOnScreenLocation(int x, int y) => new Point(puzzleConfig.X + x * nodePadding + lineWidth, puzzleConfig.Y + y * nodePadding + lineWidth);

        private void DrawColoredSquares()
        {
            var squareRuleBlocks = panel.Blocks.Where(x => x.Rule is ColoredSquareRule);
            foreach (var block in squareRuleBlocks)
            {
                Color color = (block.Rule as ColoredSquareRule).Color.Value;
                spriteBatch.Draw(texSquare, new Rectangle(BlockPositionToOnScreenLocation(block.X, block.Y), blockSizePoint), color);
            }
        }

        private void DrawSuns()
        {
            var sunRuleBlocks = panel.Blocks.Where(x => x.Rule is SunPairRule);
            foreach (var block in sunRuleBlocks)
            {
                Color color = (block.Rule as SunPairRule).Color.Value;
                spriteBatch.Draw(texSun, new Rectangle(BlockPositionToOnScreenLocation(block.X, block.Y), blockSizePoint), color);
            }
        }

        private void DrawEliminations()
        {
            var eliminationRuleBlocks = panel.Blocks.Where(x => x.Rule is EliminationRule);
            foreach (var block in eliminationRuleBlocks)
            {
                Color color = (block.Rule as EliminationRule).Color ?? Color.White;
                spriteBatch.Draw(texElimination, new Rectangle(BlockPositionToOnScreenLocation(block.X, block.Y), blockSizePoint), color);
            }
        }

        private void DrawTriangles()
        {
            var triangleRuleBlocks = panel.Blocks.Where(x => x.Rule is TriangleRule);
            foreach (var block in triangleRuleBlocks)
            {
                int texIndex = (block.Rule as TriangleRule).Power - 1;
                spriteBatch.Draw(texTriangle[texIndex], new Rectangle(BlockPositionToOnScreenLocation(block.X, block.Y), blockSizePoint), Color.Gold);
            }
        }
        #endregion
    }
}
