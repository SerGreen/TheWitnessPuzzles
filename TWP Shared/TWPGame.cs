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
        protected float sensitivity = 0.5f;

        Color backgroundColor = Color.CornflowerBlue;
        Color wallColor = Color.DarkSlateGray;
        Color borderColor = Color.DimGray;

        Puzzle panel = null;
        SolutionLine line = null;
        SolutionLine lineMirror = null;

        List<Rectangle> startPoints = new List<Rectangle>();
        List<EndPoint> endPoints = new List<EndPoint>();

        List<Rectangle> walls = new List<Rectangle>();

        Texture2D texPixel; //base for the line texture
        Texture2D texCircle = null;
        Texture2D texCorner = null;
        Texture2D texEnd = null;

        RenderTarget2D backgroundTexture;
        RenderTarget2D linesFadeTexture;
        RenderTarget2D errorsBlinkTexture;

        const int fadeAwayTimeMax = 400;
        float fadeAwayTime = 0;

        Point puzzleDimensions;
        Rectangle puzzleConfig;
        int lineWidth;
        Point lineWidthPoint;
        Point halfLineWidthPoint;
        int nodePadding;
        bool isMobile;

        public TWPGame(bool isMobile, Puzzle panel = null)
        {
            panel = new SymmetryPuzzle(5, 5, true, Color.Aqua, Color.Yellow);
            panel.nodes[25].SetState(NodeState.Start);
            panel.nodes[28].SetState(NodeState.Start);
            panel.nodes[7].SetState(NodeState.Start);
            panel.nodes[10].SetState(NodeState.Start);
            panel.nodes[1].SetState(NodeState.Exit);
            panel.nodes[3].SetState(NodeState.Exit);
            panel.nodes[23].SetState(NodeState.Exit);
            panel.nodes[22].SetState(NodeState.Exit);
            panel.nodes[12].SetState(NodeState.Exit);
            panel.nodes[0].SetState(NodeState.Exit);
            panel.nodes[20].SetState(NodeState.Exit);
            panel.nodes[9].SetState(NodeState.Exit);
            panel.nodes[24].SetState(NodeState.Exit);
            panel.nodes[32].SetState(NodeState.Exit);
            panel.edges.Find(x => x.Id == 814)?.SetState(EdgeState.Broken);
            panel.edges.Find(x => x.Id == 1617)?.SetState(EdgeState.Broken);

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
            int blockWidth = (int) (puzzleMaxSize * BlockSizeRatio(maxPuzzleDimension));
            halfLineWidthPoint = new Point(lineWidth / 2);
            lineWidthPoint = new Point(lineWidth);

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
            var brokenEdges = panel.edges.Where(x => x.State == EdgeState.Broken);
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

            IEnumerable<Point> GetStartNodes() => panel.nodes.Where(x => x.State == NodeState.Start).Select(x => NodeIdToPoint(x.Id));
            IEnumerable<Point> GetEndNodesTop()
            {
                for (int i = 1; i < panel.Width; i++)
                    if (panel.nodes[i].State == NodeState.Exit)
                        yield return new Point(i, 0);
            }
            IEnumerable<Point> GetEndNodesBot()
            {
                for (int i = 1; i < panel.Width; i++)
                {
                    int index = panel.Height * (panel.Width + 1) + i;
                    if (panel.nodes[index].State == NodeState.Exit)
                        yield return new Point(i, panel.Height);
                }
            }
            IEnumerable<Point> GetEndNodesLeft()
            {
                for (int j = 0; j < panel.Height + 1; j++)
                {
                    int index = j * (panel.Width + 1);
                    if (panel.nodes[index].State == NodeState.Exit)
                        yield return new Point(0, j);
                }
            }
            IEnumerable<Point> GetEndNodesRight()
            {
                for (int j = 0; j < panel.Height + 1; j++)
                {
                    int index = j * (panel.Width + 1) + panel.Width;
                    if (panel.nodes[index].State == NodeState.Exit)
                        yield return new Point(panel.Width, j);
                }
            }
            Point NodeIdToPoint(int id)
            {
                int y = id / (panel.Width + 1);
                int x = id - y * (panel.Width + 1);
                return new Point(x, y);
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

            // TODO: use this.Content to load your game content here
            InitPanel();

            texCircle = Content.Load<Texture2D>("img/twp_circle");
            texCorner = Content.Load<Texture2D>("img/twp_corner");
            texEnd = Content.Load<Texture2D>("img/twp_ending");
            texPixel = new Texture2D(GraphicsDevice, 1, 1);
            texPixel.SetData<Color>(new Color[] { Color.White }); // fill the texture with white

            backgroundTexture = new RenderTarget2D(
                GraphicsDevice,
                GraphicsDevice.PresentationParameters.BackBufferWidth,
                GraphicsDevice.PresentationParameters.BackBufferHeight,
                false,
                GraphicsDevice.PresentationParameters.BackBufferFormat,
                DepthFormat.Depth24);
            linesFadeTexture = new RenderTarget2D(
                GraphicsDevice,
                GraphicsDevice.PresentationParameters.BackBufferWidth,
                GraphicsDevice.PresentationParameters.BackBufferHeight,
                false,
                GraphicsDevice.PresentationParameters.BackBufferFormat,
                DepthFormat.Depth24);
            errorsBlinkTexture = new RenderTarget2D(
                GraphicsDevice,
                GraphicsDevice.PresentationParameters.BackBufferWidth,
                GraphicsDevice.PresentationParameters.BackBufferHeight,
                false,
                GraphicsDevice.PresentationParameters.BackBufferFormat,
                DepthFormat.Depth24);

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

            if (line != null)
            {
                Vector2 moveVector = GetMoveVector();
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

            if (fadeAwayTime > 0)
                fadeAwayTime--;

            base.Update(gameTime);
        }

        private bool MoveOneLine(Vector2 moveVector, IEnumerable<Rectangle> hitboxes) => line.Move(moveVector, hitboxes);
        private bool MoveBothLines(Vector2 moveVector, IEnumerable<Rectangle> hitboxes)
        {
            // Move first line
            if (line.Move(moveVector, hitboxes.Append(lineMirror.Head)))
            {
                // If first line succeeded, then move second line
                Vector2 mirroredVector;
                if ((panel as SymmetryPuzzle).Y_Mirrored)
                    mirroredVector = -moveVector;
                else
                    mirroredVector = new Vector2(-moveVector.X, moveVector.Y);

                if (!lineMirror.Move(mirroredVector, hitboxes.Append(line.Head)))
                {
                    // If second line failed, move first line back
                    line.Move(-moveVector, hitboxes);
                    return false;
                }
                return true;
            }
            else return false;
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
                            fadeAwayTime = 0;
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
                    RenderLinesToTexture();
                    line = lineMirror = null;
                    fadeAwayTime = fadeAwayTimeMax;
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

            if (fadeAwayTime > 0 && linesFadeTexture != null)
                spriteBatch.Draw(linesFadeTexture, GraphicsDevice.Viewport.Bounds, Color.Black * (fadeAwayTime / fadeAwayTimeMax));
            
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
                    case Facing.Left:
                        angle = 0; break;
                    case Facing.Right:
                        angle = MathHelper.ToRadians(180); break;
                    case Facing.Up:
                        angle = MathHelper.ToRadians(90); break;
                    case Facing.Down:
                        angle = MathHelper.ToRadians(270); break;
                    default:
                        angle = 0; break;
                }
                float scale = (float) end.Rectangle.Width / texEnd.Width;
                spriteBatch.Draw(texEnd, end.Rectangle.Center.ToVector2(), null, wallColor, angle, texEnd.Bounds.Center.ToVector2(), scale, SpriteEffects.None, 0);
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

            DrawBorders();

            spriteBatch.End();
            // Drop the render target
            GraphicsDevice.SetRenderTarget(null);
        }
        #endregion
    }
}
