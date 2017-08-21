using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using TheWitnessPuzzles;
using System.Linq;

namespace TWP_Shared
{
    public class PanelGameScreen : GameScreen
    {
        bool drawDebug = false;
        
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

        SpriteFont fntDebug = null;
        Texture2D texPixel = null;
        Texture2D texCircle = null;
        Texture2D texCorner = null;
        Texture2D[] texEndPoint = new Texture2D[2];
        Texture2D texHexagon = null;
        Texture2D texSquare = null;
        Texture2D texSun = null;
        Texture2D texElimination = null;
        Texture2D[] texTriangle = new Texture2D[3];

        RenderTarget2D backgroundTexture;
        RenderTarget2D linesFadeTexture;
        RenderTarget2D errorsBlinkTexture;
        RenderTarget2D eliminatedErrorsTexture;

        Point puzzleDimensions;     // Size of the puzzle in blocks
        Rectangle puzzleConfig;     // Location on screen and size of the puzzle in pixels
        int lineWidth;              // Width of the solution line in pixels
        Point lineWidthPoint;       // Point with both X and Y set to the lineWith
        Point halfLineWidthPoint;   // Point with both X and Y set to the lineWidth / 2
        int blockWidth;             // Size of the block in pixels
        Point blockSizePoint;       // Point with X and Y set to the blockWidth
        int nodePadding;            // Distance between two nodes on screen in pixel, is equal to (lineWidth + blockWidth)

        public PanelGameScreen(Point screenSize, GraphicsDevice device) : base(screenSize, device) { }

        public override void LoadContent(Dictionary<string, Texture2D> TextureProvider, Dictionary<string, SpriteFont> FontProvider)
        {
            texPixel = TextureProvider["img/twp_pixel"];
            texCircle = TextureProvider["img/twp_circle"];
            texCorner = TextureProvider["img/twp_corner"];
            texEndPoint[0] = TextureProvider["img/twp_ending_left"];
            texEndPoint[1] = TextureProvider["img/twp_ending_top"];
            // Puzzle rules textures
            texHexagon = TextureProvider["img/twp_hexagon"];
            texSquare = TextureProvider["img/twp_square"];
            texSun = TextureProvider["img/twp_sun"];
            texElimination = TextureProvider["img/twp_elimination"];
            for (int i = 0; i < 3; i++)
                texTriangle[i] = TextureProvider[$"img/twp_triangle{i + 1}"];

            // Fullscreen textures for 1. background, 2. fading solution lines, 3. red blinking rules for error highlighting and 4. displaying eliminated rules with dim colors
            backgroundTexture = new RenderTarget2D(GraphicsDevice, GraphicsDevice.PresentationParameters.BackBufferWidth, GraphicsDevice.PresentationParameters.BackBufferHeight, false, GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.Depth24);
            linesFadeTexture = new RenderTarget2D(GraphicsDevice, GraphicsDevice.PresentationParameters.BackBufferWidth, GraphicsDevice.PresentationParameters.BackBufferHeight, false, GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.Depth24);
            errorsBlinkTexture = new RenderTarget2D(GraphicsDevice, GraphicsDevice.PresentationParameters.BackBufferWidth, GraphicsDevice.PresentationParameters.BackBufferHeight, false, GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.Depth24);
            eliminatedErrorsTexture = new RenderTarget2D(GraphicsDevice, GraphicsDevice.PresentationParameters.BackBufferWidth, GraphicsDevice.PresentationParameters.BackBufferHeight, false, GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.Depth24);

            fntDebug = FontProvider["font/fnt_constantia12"];

            base.LoadContent(TextureProvider, FontProvider);
        }
        public void LoadNewPanel(Puzzle panel)
        {
            this.panel = panel;

            walls.Clear();
            startPoints.Clear();
            endPoints.Clear();
            line = lineMirror = null;
            panelState.ResetToNeutral();

            InitializePanel();
        }
        private void InitializePanel()
        {
            puzzleDimensions = new Point(panel.Width, panel.Height);

            // Calculation of puzzle sizes for current screen size
            int width = ScreenSize.X;
            int height = ScreenSize.Y;

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

            // Draw static parts of panel onto the texture
            RenderBackgroundTexture();

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
        private Point NodeIdToPoint(int nodeId)
        {
            int y = nodeId / (panel.Width + 1);
            int x = nodeId - y * (panel.Width + 1);
            return new Point(x, y);
        }


        public override void Update(GameTime gameTime)
        {
            HandleScreenTap();
            MoveLine(GetMoveVector());
            panelState.Update();

            if (InputManager.IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.N))
                drawDebug = !drawDebug;

            base.Update(gameTime);
        }

        private void HandleScreenTap()
        {
            Point? tap = GetTapPosition();
            if (tap.HasValue)
            {
                //if (btnRandom.Contains(tap.Value))
                //    LoadNewPanel(PanelGenerator.GeneratePanel());

                if (line == null || panelState.State.HasFlag(PanelStates.Solved))
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
                    // Check if line head is in the end point
                    foreach (var endPoint in endPoints)
                        if (endPoint.IntercetionPercent(line.Head) > 0.4f)
                        {
                            // Place line head rignt in the end, nice and tight
                            Point headOffset = endPoint.Rectangle.Location - line.Head.Location;
                            MoveLine(headOffset.ToVector2());

                            // Retrieve Solution from the line and check for errors
                            List<Error> errors = null;
                            List<int> solution = line.GetSolution(panel.Width, puzzleConfig.Location, nodePadding);
                            if (panel.SetSolution(solution))
                                errors = panel.CheckForErrors();

                            // Split errors into true errors and the ones, that are eliminated by respective rules
                            var trueErrors = errors.Where(x => x.IsEliminated == false).ToList();
                            var eliminatedErrors = errors.Where(x => x.IsEliminated == true).ToList();

                            // If there are eliminated errors, initialize elimination process
                            if (eliminatedErrors.Count() > 0)
                            {
                                // Draw all errors in red as for now
                                RenderErrorsToTexture(errors, false);

                                // Begin black line fade out as if there was an error
                                RenderLinesToTexture();
                                panelState.InvokeFadeOut(true);
                                // And start elimination timer
                                panelState.InitializeElimination();

                                // This event will be executed a few seconds later, after elimination is complete
                                Action handler = null;
                                handler = () =>
                                {
                                    // Re-draw true errors in red and eliminated errors separately
                                    RenderErrorsToTexture(trueErrors, false);
                                    RenderErrorsToTexture(eliminatedErrors, true);

                                    // If there are no true errors, then panel is solved
                                    if (trueErrors.Count() == 0)
                                    {
                                        // Setting success will stop lines fading process, but retain eliminated errors intact
                                        panelState.SetSuccess();
                                    }
                                    else
                                    {
                                        // If there are true errors => delete the lines
                                        // They will continue to fade out and errors will continue to blink
                                        line = lineMirror = null;
                                    }

                                    // Don't forget to unsubscribe ffs!
                                    panelState.EliminationFinished -= handler;
                                };
                                panelState.EliminationFinished += handler;
                            }
                            // If there are no eliminated errors, everything happens instantly
                            else
                            {
                                // If there are no true errors, then panel is solved
                                if (trueErrors.Count() == 0)
                                {
                                    panelState.SetSuccess();
                                }
                                else
                                {
                                    // If there are true errors => start fading process and delete the lines
                                    RenderLinesToTexture();
                                    RenderErrorsToTexture(errors, false);
                                    panelState.InvokeFadeOut(true);
                                    line = lineMirror = null;
                                }
                            }

                            return;
                        }

                    // If we looked through every endpoint and line head is not in any of them
                    // Then simply delete lines and fade them out without errors
                    RenderLinesToTexture();
                    panelState.InvokeFadeOut(false);
                    line = lineMirror = null;
                }
            }
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
        private Point RectifyLinePosition(Point pos)
        {
            Point zeroedPos = pos - puzzleConfig.Location;
            return new Point(zeroedPos.X / nodePadding * nodePadding, zeroedPos.Y / nodePadding * nodePadding) + puzzleConfig.Location;
        }
        private Point? GetTapPosition() => InputManager.GetTapPosition();
        private Vector2 GetMoveVector() => InputManager.GetDragVector();

        #region ===== RENDER REGION =====
        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(backgroundTexture, GraphicsDevice.Viewport.Bounds, Color.White);
            DrawLines(spriteBatch);

            if (panelState.State.HasFlag(PanelStates.LineFade) && linesFadeTexture != null)
                spriteBatch.Draw(linesFadeTexture, GraphicsDevice.Viewport.Bounds, (panelState.State.HasFlag(PanelStates.ErrorHappened) ? Color.Black : Color.White) * panelState.LineFadeOpacity);

            if (panelState.State.HasFlag(PanelStates.ErrorBlink) && errorsBlinkTexture != null)
                spriteBatch.Draw(errorsBlinkTexture, GraphicsDevice.Viewport.Bounds, Color.White * panelState.BlinkOpacity);

            if (panelState.State.HasFlag(PanelStates.EliminationFinished) && eliminatedErrorsTexture != null)
                spriteBatch.Draw(eliminatedErrorsTexture, GraphicsDevice.Viewport.Bounds, Color.White * panelState.EliminationFadeOpacity);

            if (drawDebug)
                DebugDrawNodeIDs(spriteBatch);

            //spriteBatch.Draw(texPixel, btnRandom, Color.Red * 0.5f);
            
            base.Draw(spriteBatch);
        }

        #region === Render sub-methods ===

        private void DrawLines(SpriteBatch spriteBatch)
        {
            if (line != null)
                line.Draw(spriteBatch, texCircle, texPixel, panel is SymmetryPuzzle sym ? sym.MainColor : Color.White);

            if (lineMirror != null)
                lineMirror.Draw(spriteBatch, texCircle, texPixel, panel is SymmetryPuzzle sym ? sym.MirrorColor : Color.White);
        }

        private void DrawRoundedCorners(SpriteBatch spriteBatch)
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
        private void DrawBorders(SpriteBatch spriteBatch)
        {
            int borderWidth = (int) (Math.Min(ScreenSize.X, ScreenSize.Y) * 0.03f);
            spriteBatch.Draw(texPixel, new Rectangle(0, 0, ScreenSize.X, borderWidth), borderColor);
            spriteBatch.Draw(texPixel, new Rectangle(0, 0, borderWidth, ScreenSize.Y), borderColor);
            spriteBatch.Draw(texPixel, new Rectangle(ScreenSize.X - borderWidth, 0, borderWidth, ScreenSize.Y), borderColor);
            spriteBatch.Draw(texPixel, new Rectangle(0, ScreenSize.Y - borderWidth, ScreenSize.X, borderWidth), borderColor);
        }

        private void RenderLinesToTexture()
        {
            using (SpriteBatch spriteBatch = new SpriteBatch(GraphicsDevice))
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
        }
        private void RenderBackgroundTexture()
        {
            using (SpriteBatch spriteBatch = new SpriteBatch(GraphicsDevice))
            {
                // Set the render target
                GraphicsDevice.SetRenderTarget(backgroundTexture);

                GraphicsDevice.Clear(backgroundColor);
                spriteBatch.Begin(SpriteSortMode.Deferred);

                foreach (var wall in walls)
                    spriteBatch.Draw(texPixel, wall, wallColor);

                DrawRoundedCorners(spriteBatch);

                foreach (var end in endPoints)
                    end.Draw(spriteBatch, wallColor, texEndPoint);

                foreach (var start in startPoints)
                {
                    //spriteBatch.Draw(texCircle, start, backgroundColor);
                    float scale = (float) start.Width / texCircle.Width;
                    spriteBatch.Draw(texCircle, start.Center.ToVector2(), null, backgroundColor, 0, texCircle.Bounds.Center.ToVector2(), scale, SpriteEffects.None, 0);
                }

                DrawAllRules(spriteBatch);
                DrawBorders(spriteBatch);

                spriteBatch.End();
                // Drop the render target
                GraphicsDevice.SetRenderTarget(null);
            }
        }
        private void RenderErrorsToTexture(IEnumerable<Error> errors, bool isEliminated)
        {
            using (SpriteBatch spriteBatch = new SpriteBatch(GraphicsDevice))
            {
                var nodes = errors.Select(x => x.Source).OfType<Node>();
                var edges = errors.Select(x => x.Source).OfType<Edge>();
                var blocks = errors.Select(x => x.Source).OfType<Block>();

                // Set the render target
                GraphicsDevice.SetRenderTarget(isEliminated ? eliminatedErrorsTexture : errorsBlinkTexture);

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

                spriteBatch.End();
                // Drop the render target
                GraphicsDevice.SetRenderTarget(null);
            }
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
        }
        private void DrawMarkedNodes(SpriteBatch spriteBatch, IEnumerable<Node> allNodes, Color? fillColor = null)
        {
            var markedNodes = allNodes.Where(x => x.State == NodeState.Marked);
            foreach (var node in markedNodes)
            {
                Point position = NodeIdToPoint(node.Id).Multiply(nodePadding) + puzzleConfig.Location;
                spriteBatch.Draw(texHexagon, new Rectangle(position, lineWidthPoint), fillColor ?? node.Color ?? Color.Black);
            }
        }
        private void DrawMarkedEdges(SpriteBatch spriteBatch, IEnumerable<Edge> allEdges, Color? fillColor = null)
        {
            var markedEdges = allEdges.Where(x => x.State == EdgeState.Marked);
            foreach (var edge in markedEdges)
            {
                Point pA = NodeIdToPoint(edge.NodeA.Id).Multiply(nodePadding);
                Point pB = NodeIdToPoint(edge.NodeB.Id).Multiply(nodePadding);
                Point position = (pA + pB).Divide(2) + puzzleConfig.Location;
                spriteBatch.Draw(texHexagon, new Rectangle(position, lineWidthPoint), fillColor ?? edge.Color ?? Color.Black);
            }
        }
        private Point BlockPositionToOnScreenLocation(int x, int y) => new Point(puzzleConfig.X + x * nodePadding + lineWidth, puzzleConfig.Y + y * nodePadding + lineWidth);
        private void DrawColoredSquares(SpriteBatch spriteBatch, IEnumerable<Block> allBlocks, Color? fillColor = null)
        {
            var squareRuleBlocks = allBlocks.Where(x => x.Rule is ColoredSquareRule);
            foreach (var block in squareRuleBlocks)
            {
                Color color = (block.Rule as ColoredSquareRule).Color.Value;
                spriteBatch.Draw(texSquare, new Rectangle(BlockPositionToOnScreenLocation(block.X, block.Y), blockSizePoint), fillColor ?? color);
            }
        }
        private void DrawSuns(SpriteBatch spriteBatch, IEnumerable<Block> allBlocks, Color? fillColor = null)
        {
            var sunRuleBlocks = allBlocks.Where(x => x.Rule is SunPairRule);
            foreach (var block in sunRuleBlocks)
            {
                Color color = (block.Rule as SunPairRule).Color.Value;
                spriteBatch.Draw(texSun, new Rectangle(BlockPositionToOnScreenLocation(block.X, block.Y), blockSizePoint), fillColor ?? color);
            }
        }
        private void DrawEliminations(SpriteBatch spriteBatch, IEnumerable<Block> allBlocks, Color? fillColor = null)
        {
            var eliminationRuleBlocks = allBlocks.Where(x => x.Rule is EliminationRule);
            foreach (var block in eliminationRuleBlocks)
            {
                Color color = (block.Rule as EliminationRule).Color ?? Color.White;
                spriteBatch.Draw(texElimination, new Rectangle(BlockPositionToOnScreenLocation(block.X, block.Y), blockSizePoint), fillColor ?? color);
            }
        }
        private void DrawTriangles(SpriteBatch spriteBatch, IEnumerable<Block> allBlocks, Color? fillColor = null)
        {
            var triangleRuleBlocks = allBlocks.Where(x => x.Rule is TriangleRule);
            foreach (var block in triangleRuleBlocks)
            {
                int texIndex = (block.Rule as TriangleRule).Power - 1;
                spriteBatch.Draw(texTriangle[texIndex], new Rectangle(BlockPositionToOnScreenLocation(block.X, block.Y), blockSizePoint), fillColor ?? Color.Gold);
            }
        }


        private void DebugDrawNodeIDs(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < panel.Width + 1; i++)
            {
                for (int j = 0; j < panel.Height + 1; j++)
                {
                    int id = j * (panel.Width + 1) + i;
                    spriteBatch.DrawString(fntDebug, id.ToString(), (puzzleConfig.Location + new Point(i, j).Multiply(nodePadding)).ToVector2(), Color.Black);
                }
            }
        }
        #endregion
        #endregion
    }
}
