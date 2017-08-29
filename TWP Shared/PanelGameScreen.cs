using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using TheWitnessPuzzles;
using System.Linq;
using BloomFX;

namespace TWP_Shared
{
    public class PanelGameScreen : GameScreen
    {
        bool drawDebug = false;

        PanelRenderer renderer;

        Puzzle panel = null;
        SolutionLine line = null;
        SolutionLine lineMirror = null;
        PanelState panelState = new PanelState();

        List<Rectangle> startPoints;
        List<EndPoint> endPoints;
        List<Rectangle> walls;

        SpriteFont fntDebug = null;
        Texture2D texPixel, texCircle;
        
        RenderTarget2D backgroundTexture;
        RenderTarget2D lineTexture;
        RenderTarget2D lineFadeTexture;
        RenderTarget2D errorsBlinkTexture;
        RenderTarget2D eliminatedErrorsTexture;

        BloomFilter bloomFilter;

        public PanelGameScreen(Puzzle panel, Point screenSize, GraphicsDevice device, Dictionary<string, Texture2D> TextureProvider, Dictionary<string, SpriteFont> FontProvider) 
            : base(screenSize, device, TextureProvider, FontProvider)
        {
            if (panel == null)
                panel = new Puzzle(2, 2);

            this.panel = panel;
            renderer = new PanelRenderer(panel, screenSize, TextureProvider, device);
            startPoints = renderer.StartPoints;
            endPoints = renderer.EndPoints;
            walls = renderer.Walls;

            texPixel = TextureProvider["img/twp_pixel"];
            texCircle = TextureProvider["img/twp_circle"];
            fntDebug = FontProvider["font/fnt_constantia12"];

            // Fullscreen textures for 1. background, 2. fading solution lines, 3. red blinking rules for error highlighting and 4. displaying eliminated rules with dim colors
            backgroundTexture = new RenderTarget2D(GraphicsDevice, screenSize.X, screenSize.Y);
            lineTexture = new RenderTarget2D(GraphicsDevice, screenSize.X, screenSize.Y);
            lineFadeTexture = new RenderTarget2D(GraphicsDevice, screenSize.X, screenSize.Y);
            errorsBlinkTexture = new RenderTarget2D(GraphicsDevice, screenSize.X, screenSize.Y);
            eliminatedErrorsTexture = new RenderTarget2D(GraphicsDevice, screenSize.X, screenSize.Y);

            // Load bloom shader
            bloomFilter = new BloomFilter();
            bloomFilter.Load(GraphicsDevice, Content, screenSize.X, screenSize.Y);
            bloomFilter.BloomPreset = BloomFilter.BloomPresets.Focussed;

            renderer.RenderPanelToTexture(backgroundTexture);
        }
        public void LoadNewPanel(Puzzle panel)
        {
            if (renderer.SetPanel(panel))
            {
                line = lineMirror = null;
                this.panel = panel;
                panelState.ResetToNeutral();
                startPoints = renderer.StartPoints;
                endPoints = renderer.EndPoints;
                walls = renderer.Walls;
            }
        }

        public override void Update(GameTime gameTime)
        {
            HandleScreenTap();
            MoveLine(GetMoveVector());
            panelState.Update();

            if (!panelState.State.HasFlag(PanelStates.ErrorHappened) && !panelState.State.HasFlag(PanelStates.Solved))
            {
                // If line touched end point => Produce sound
                bool lineAtEndpoint = IsLineAtEndPoint();
                if (lineAtEndpoint && !panelState.State.HasFlag(PanelStates.FinishTracing))
                {
                    panelState.SetFinishTracing(true);
                    RenderLinesToTexture(lineFadeTexture, Color.White);
                    SoundManager.PlayOnce(Sound.FinishTracing);
                    SoundManager.PlayLoop(SoundLoop.PathComplete);
                }
                // If line left end point => Produce another sound
                else if (!lineAtEndpoint && panelState.State.HasFlag(PanelStates.FinishTracing))
                {
                    panelState.SetFinishTracing(false);
                    SoundManager.PlayOnce(Sound.AbortFinishTracing);
                    SoundManager.StopLoop(SoundLoop.PathComplete);
                }
            }

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

                if (!(panelState.State.HasFlag(PanelStates.EliminationStarted) && !panelState.State.HasFlag(PanelStates.EliminationFinished)))
                {
                    if (line == null || panelState.State.HasFlag(PanelStates.Solved))
                    {
                        foreach (Rectangle startPoint in startPoints)
                            if (startPoint.Contains(tap.Value))
                            {
                                panelState.ResetToNeutral();
                                SoundManager.PlayOnce(Sound.StartTracing);
                                SoundManager.PlayLoop(SoundLoop.Tracing);
                                line = new SolutionLine(startPoint.Center - (new Point(renderer.LineWidth / 2)), renderer.LineWidth, startPoint);
                                if (panel is SymmetryPuzzle symPanel)
                                    if (symPanel.Y_Mirrored)
                                    {
                                        Point mirPoint = renderer.PuzzleConfig.Location + renderer.PuzzleConfig.Location + renderer.PuzzleConfig.Size - startPoint.Center - (new Point(renderer.LineWidth / 2));
                                        Rectangle mirStartPoint = startPoints.Find(x => x.Contains(mirPoint));
                                        lineMirror = new SolutionLine(RectifyLinePosition(mirPoint), renderer.LineWidth, mirStartPoint);
                                    }
                                    else
                                    {
                                        Point mirPoint = new Point(renderer.PuzzleConfig.Location.X * 2 + renderer.PuzzleConfig.Size.X - startPoint.Center.X - renderer.LineWidth / 2, startPoint.Center.Y - renderer.LineWidth / 2);
                                        Rectangle mirStartPoint = startPoints.Find(x => x.Contains(mirPoint));
                                        lineMirror = new SolutionLine(RectifyLinePosition(mirPoint), renderer.LineWidth, mirStartPoint);
                                    }

                                break;
                            }
                    }
                    else
                    {
                        SoundManager.StopLoop(SoundLoop.Tracing);
                        SoundManager.StopLoop(SoundLoop.PathComplete);

                        // Check if line head is in the end point
                        foreach (var endPoint in endPoints)
                            if (endPoint.IntercetionPercent(line.Head) > 0.4f)
                            {
                                // Place line head rignt in the end, nice and tight
                                Point headOffset = endPoint.Rectangle.Location - line.Head.Location;
                                MoveLine(headOffset.ToVector2());

                                // Retrieve Solution from the line and check for errors
                                List<Error> errors = null;
                                List<int> solution = line.GetSolution(panel.Width, renderer.PuzzleConfig.Location, renderer.NodePadding);
                                if (panel.SetSolution(solution))
                                    errors = panel.CheckForErrors();

                                // Split errors into true errors and the ones, that are eliminated by respective rules
                                var trueErrors = errors.Where(x => x.IsEliminated == false).ToList();
                                var eliminatedErrors = errors.Where(x => x.IsEliminated == true).ToList();

                                // If there are eliminated errors, initialize elimination process
                                if (eliminatedErrors.Count() > 0)
                                {
                                    SoundManager.PlayOnce(Sound.PotentialFailure);
                                    // Draw all errors in red as for now
                                    renderer.RenderErrorsToTexture(errorsBlinkTexture, errors, false);

                                    // Begin black line fade out as if there was an error
                                    RenderLinesToTexture(lineFadeTexture);
                                    panelState.InvokeFadeOut(true);
                                    // And start elimination timer
                                    panelState.InitializeElimination();

                                    // This event will be executed a few seconds later, after elimination is complete
                                    Action handler = null;
                                    handler = () =>
                                    {
                                        SoundManager.PlayOnce(Sound.EliminatorApply);
                                    // Re-draw true errors in red and eliminated errors separately
                                    renderer.RenderErrorsToTexture(errorsBlinkTexture, trueErrors, false);
                                        renderer.RenderErrorsToTexture(eliminatedErrorsTexture, eliminatedErrors, true);

                                    // If there are no true errors, then panel is solved
                                    if (trueErrors.Count() == 0)
                                        {
                                        // Setting success will stop lines fading process, but retain eliminated errors intact
                                        panelState.SetSuccess();
                                            SoundManager.PlayOnce(Sound.Success);
                                        }
                                        else
                                        {
                                        // If there are true errors => delete the lines
                                        // They will continue to fade out and errors will continue to blink
                                        line = lineMirror = null;
                                            SoundManager.PlayOnce(Sound.Failure);
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
                                        SoundManager.PlayOnce(Sound.Success);
                                    }
                                    else
                                    {
                                        // If there are true errors => start fading process and delete the lines
                                        RenderLinesToTexture(lineFadeTexture);
                                        renderer.RenderErrorsToTexture(errorsBlinkTexture, errors, false);
                                        panelState.InvokeFadeOut(true);
                                        line = lineMirror = null;
                                        SoundManager.PlayOnce(Sound.Failure);
                                    }
                                }

                                return;
                            }

                        // If we looked through every endpoint and line head is not in any of them
                        // Then simply delete lines and fade them out without errors
                        RenderLinesToTexture(lineFadeTexture);
                        panelState.InvokeFadeOut(false);
                        line = lineMirror = null;
                        SoundManager.PlayOnce(Sound.AbortTracing);
                    } 
                }
            }
        }
        private void MoveLine(Vector2 moveVector)   
        {
            if (line != null && !panelState.State.HasFlag(PanelStates.EliminationStarted))
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
            Point zeroedPos = pos - renderer.PuzzleConfig.Location;
            return new Point(zeroedPos.X / renderer.NodePadding * renderer.NodePadding, zeroedPos.Y / renderer.NodePadding * renderer.NodePadding) + renderer.PuzzleConfig.Location;
        }
        private Point? GetTapPosition() => InputManager.GetTapPosition();
        private Vector2 GetMoveVector() => InputManager.GetDragVector();
        private bool IsLineAtEndPoint() => line != null ? endPoints.Any(x => x.IntercetionPercent(line.Head) > 0.4f) : false;

        #region ===== RENDER REGION =====
        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(backgroundTexture, GraphicsDevice.Viewport.Bounds, Color.White);
            
            RenderLinesToTexture(lineTexture);
            Texture2D bloom = bloomFilter.Draw(lineTexture, ScreenSize.X, ScreenSize.Y);
            spriteBatch.Draw(lineTexture, GraphicsDevice.Viewport.Bounds, panelState.State.HasFlag(PanelStates.Solved) ? Color.White : Color.LightGray);
            spriteBatch.Draw(bloom, GraphicsDevice.Viewport.Bounds, Color.White);

            if (panelState.State.HasFlag(PanelStates.FinishTracing) && lineFadeTexture != null)
                spriteBatch.Draw(lineFadeTexture, GraphicsDevice.Viewport.Bounds, Color.White * panelState.FinishTracingBlinkOpacity);

            if (panelState.State.HasFlag(PanelStates.LineFade) && lineFadeTexture != null)
                spriteBatch.Draw(lineFadeTexture, GraphicsDevice.Viewport.Bounds, (panelState.State.HasFlag(PanelStates.ErrorHappened) ? Color.Black : Color.White) * panelState.LineFadeOpacity);

            if (panelState.State.HasFlag(PanelStates.ErrorBlink) && errorsBlinkTexture != null)
                spriteBatch.Draw(errorsBlinkTexture, GraphicsDevice.Viewport.Bounds, Color.White * panelState.ErrorBlinkOpacity);

            if (panelState.State.HasFlag(PanelStates.EliminationFinished) && eliminatedErrorsTexture != null)
                spriteBatch.Draw(eliminatedErrorsTexture, GraphicsDevice.Viewport.Bounds, Color.White * panelState.EliminationFadeOpacity);

            if (drawDebug)
                DebugDrawNodeIDs(spriteBatch);
            
            base.Draw(spriteBatch);
        }
        
        private void DrawLines(SpriteBatch spriteBatch, Color? fillColor = null)
        {
            if (line != null)
                line.Draw(spriteBatch, texCircle, texPixel, fillColor ?? panel.MainColor);

            if (lineMirror != null)
                lineMirror.Draw(spriteBatch, texCircle, texPixel, fillColor ?? (panel as SymmetryPuzzle).MirrorColor);
        }
        private void RenderLinesToTexture(RenderTarget2D canvas, Color? fillColor = null)
        {
            using (SpriteBatch spriteBatch = new SpriteBatch(GraphicsDevice))
            {
                // Set the render target
                GraphicsDevice.SetRenderTarget(canvas);

                // Draw the lines
                GraphicsDevice.Clear(Color.Transparent);
                spriteBatch.Begin(SpriteSortMode.Texture);
                DrawLines(spriteBatch, fillColor);
                spriteBatch.End();
                // Drop the render target
                GraphicsDevice.SetRenderTarget(null);
            }
        }

        private void DebugDrawNodeIDs(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < panel.Width + 1; i++)
            {
                for (int j = 0; j < panel.Height + 1; j++)
                {
                    int id = j * (panel.Width + 1) + i;
                    spriteBatch.DrawString(fntDebug, id.ToString(), (renderer.PuzzleConfig.Location + new Point(i, j).Multiply(renderer.NodePadding)).ToVector2(), Color.Black);
                }
            }
        }
        #endregion
    }
}
