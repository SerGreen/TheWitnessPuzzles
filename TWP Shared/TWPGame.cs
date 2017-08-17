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

        int moveStep = 5;
        float sensitivity = 0.5f;

        Puzzle panel = null;
        SolutionLine line = null;
        SolutionLine lineMirror = null;

        List<Rectangle> startPoints = new List<Rectangle>();
        List<Rectangle> endPoints = new List<Rectangle>();

        List<Rectangle> walls = new List<Rectangle>();

        Texture2D t; //base for the line texture

        Point puzzleDimensions;
        int lineWidth;
        bool isMobile;

        public TWPGame(bool isMobile, Puzzle panel = null)
        {
            panel = new Puzzle(4, 4);
            panel.nodes[21].SetState(NodeState.Start);
            panel.nodes[7].SetState(NodeState.Start);
            panel.nodes[1].SetState(NodeState.Exit);
            panel.nodes[3].SetState(NodeState.Exit);
            panel.nodes[23].SetState(NodeState.Exit);
            panel.nodes[22].SetState(NodeState.Exit);
            panel.nodes[0].SetState(NodeState.Exit);
            panel.nodes[20].SetState(NodeState.Exit);
            panel.nodes[9].SetState(NodeState.Exit);
            panel.nodes[24].SetState(NodeState.Exit);

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
            
            TouchPanel.EnabledGestures = GestureType.Tap | GestureType.FreeDrag;
        }

        private void InitPanel()
        {
            int width = GraphicsDevice.Viewport.Width;
            int height = GraphicsDevice.Viewport.Height;

            int maxPuzzleDimension = Math.Max(puzzleDimensions.X, puzzleDimensions.Y);

            int screenMinSize = Math.Min(width, height);
            int puzzleMaxSize = (int) (screenMinSize * PuzzleSpaceRatio(maxPuzzleDimension));
            int lineWidth = (int) (puzzleMaxSize * LineSizeRatio(maxPuzzleDimension));
            int blockWidth = (int) (puzzleMaxSize * BlockSizeRatio(maxPuzzleDimension));

            int endAppendixLength = blockWidth / 4;

            int puzzleWidth = lineWidth * (puzzleDimensions.X + 1) + blockWidth * puzzleDimensions.X;
            int puzzleHeight = lineWidth * (puzzleDimensions.Y + 1) + blockWidth * puzzleDimensions.Y;

            int xMargin = (width - puzzleWidth) / 2;
            int yMargin = (height - puzzleHeight) / 2;

            int nodePadding = lineWidth + blockWidth;
            
            DoHorizontalWalls(false);   // Top walls
            DoHorizontalWalls(true);    // Bottom walls
            DoVerticalWalls(false);     // Left walls
            DoVerticalWalls(true);      // Right walls
            
            for (int i = 0; i < puzzleDimensions.X; i++)
                for (int j = 0; j < puzzleDimensions.Y; j++)
                    walls.Add(new Rectangle(xMargin + lineWidth * (i + 1) + blockWidth * i, yMargin + lineWidth * (j + 1) + blockWidth * j, blockWidth, blockWidth));

            foreach (Point start in GetStartNodes())
                startPoints.Add(new Rectangle(xMargin + start.X * nodePadding, yMargin + start.Y * nodePadding, lineWidth, lineWidth));

            // Inner methods
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
            void DoHorizontalWalls(bool isBottom)
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
                        endPoints.Add(new Rectangle(x, isBottom ? yStartPoint + endAppendixLength * 3 / 4 : (yMargin - endAppendixLength), lineWidth, endAppendixLength / 4));
                        lastXPoint = x + lineWidth;
                    }
                    walls.Add(new Rectangle(lastXPoint, isBottom ? yStartPoint : (yMargin - endAppendixLength), width - lastXPoint, endAppendixLength));
                }
            }
            void DoVerticalWalls(bool isRight)
            {
                int xStartPoint = isRight ? xMargin + puzzleWidth : 0;
                var ends = isRight ? GetEndNodesRight() : GetEndNodesLeft();

                if (ends.Count() == 0)
                    walls.Add(new Rectangle(xStartPoint, 0, xMargin, height));
                else
                {
                    var aa = endAppendixLength;
                    walls.Add(new Rectangle(xStartPoint + (isRight ? endAppendixLength : 0), 0, xMargin - endAppendixLength, height));

                    int lastYPoint = 0;
                    foreach (Point endPoint in ends)
                    {
                        int y = yMargin + endPoint.Y * (nodePadding);
                        walls.Add(new Rectangle(isRight ? xStartPoint : (xMargin - endAppendixLength), lastYPoint, endAppendixLength, y - lastYPoint));
                        endPoints.Add(new Rectangle(isRight ? xStartPoint + endAppendixLength * 3 /4 : xMargin - endAppendixLength, y, endAppendixLength / 4, lineWidth));
                        lastYPoint = y + lineWidth;
                    }
                    walls.Add(new Rectangle(isRight ? xStartPoint : (xMargin - endAppendixLength), lastYPoint, endAppendixLength, height - lastYPoint));
                }
            }
        }

        // Returns a coef k. Total free space in pixels * k = puzzle size in pixels
        private float PuzzleSpaceRatio(float puzzleDimension) => (float) (-0.0005 * Math.Pow(puzzleDimension, 4) + 0.0082 * Math.Pow(puzzleDimension, 3) - 0.0439 * Math.Pow(puzzleDimension, 2) + 0.1011 * puzzleDimension + 0.6875);
        // Returns a coef k. Puzzle size in pixels * k = block size in pixels
        private float BlockSizeRatio(float puzzleDimension) => (float) (0.8563 * Math.Pow(puzzleDimension, -1.134));
        // Returns a coef k. Puzzle size in pixels * k = line width in pixels
        private float LineSizeRatio(float puzzleDimension) => -0.0064f * puzzleDimension + 0.0859f;

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

            t = new Texture2D(GraphicsDevice, 1, 1);
            t.SetData<Color>(new Color[] { Color.White });// fill the texture with white
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

            FullscreenToggleCheck();

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

                    var hitboxes = line.Hitboxes.Concat(walls);
                    if (!line.Move(firstMove, hitboxes))
                    {
                        Vector2 cornerMove = line.MoveNearEdge(moveVector, walls);
                        if (cornerMove != Vector2.Zero)
                            line.Move(cornerMove, hitboxes);
                        else
                            line.Move(secondMove, hitboxes);
                    }
                }
            }

            base.Update(gameTime);
        }

        protected virtual Vector2 GetMoveVector()
        {
            Vector2 result = Vector2.Zero;

            if (!isMobile)
            {
                if (Keyboard.GetState().IsKeyDown(Keys.Right))
                    result.X += moveStep;

                if (Keyboard.GetState().IsKeyDown(Keys.Left))
                    result.X -= moveStep;

                if (Keyboard.GetState().IsKeyDown(Keys.Down))
                    result.Y += moveStep;

                if (Keyboard.GetState().IsKeyDown(Keys.Up))
                    result.Y -= moveStep;
            }
            else
            {
                GestureSample gesture;
                while (TouchPanel.IsGestureAvailable)
                {
                    gesture = TouchPanel.ReadGesture();
                    if (gesture.GestureType == GestureType.FreeDrag)
                        result += gesture.Delta * sensitivity;
                }
            }

            return result;
        }

        private void FullscreenToggleCheck()
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

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            spriteBatch.Begin();


            if (line != null)
                foreach (var hitbox in line.Hitboxes.Concat(new List<Rectangle> { line.Head }))
                    spriteBatch.Draw(t, hitbox, Color.Cyan);

            if (lineMirror != null)
                foreach (var hitbox in lineMirror.Hitboxes.Concat(new List<Rectangle> { lineMirror.Head }))
                    spriteBatch.Draw(t, hitbox, Color.Yellow);

            foreach (var wall in walls)
                spriteBatch.Draw(t, wall, Color.DarkSlateGray);

            foreach (var end in endPoints)
                spriteBatch.Draw(t, end, Color.IndianRed);

            foreach (var start in startPoints)
                spriteBatch.Draw(t, start, Color.ForestGreen);

            spriteBatch.End();
            base.Draw(gameTime);
        }

        private void DrawLine(SpriteBatch sb, Vector2 start, Vector2 end)
        {
            Vector2 edge = end - start;
            // calculate angle to rotate line
            float angle =
                (float) Math.Atan2(edge.Y, edge.X);


            sb.Draw(t,
                new Rectangle(// rectangle defines shape of line and position of start of line
                    (int) start.X,
                    (int) start.Y,
                    (int) edge.Length(), //sb will strech the texture to fill this rectangle
                    1), //width of line, change this to make thicker line
                null,
                Color.Red, //colour of line
                angle,     //angle of line (calulated above)
                new Vector2(0, 0), // point in line about which to rotate
                SpriteEffects.None,
                0);

        }
    }
}
