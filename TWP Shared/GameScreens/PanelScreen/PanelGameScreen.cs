using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using TheWitnessPuzzles;
using System.Linq;
using BloomFX;
using Microsoft.Xna.Framework.Content;

namespace TWP_Shared
{
    public class PanelGameScreen : GameScreen
    {
        bool drawDebug = false;
        
        // If this flag is true, then no 'Next Panel' button will be spawned
        readonly bool IsStandalonePanel = false;

        PanelRenderer renderer;

        Puzzle panel = null;
        SolutionLine line = null;
        SolutionLine lineMirror = null;
        PanelState panelState = new PanelState();

        List<Rectangle> startPoints;
        List<EndPoint> endPoints;
        List<Rectangle> walls;

        SpriteBatch internalBatch;
        SpriteFont fntDebug = null;
        Texture2D texPixel, texCircle, texClose, texDelete, texNext;
        Texture2D[] texLike = new Texture2D[2];
        
        RenderTarget2D backgroundTexture;
        RenderTarget2D lineTexture;
        RenderTarget2D tempLineTexture;
        RenderTarget2D lineFadeTexture;
        RenderTarget2D errorsBlinkTexture;
        RenderTarget2D eliminatedErrorsTexture;

        List<TouchButton> buttons = new List<TouchButton>();
        FadeTransition fade = new FadeTransition(15, 15, 10);

        // Lines are of a darker tint when they are not finished
        static readonly Color linesTint = new Color(230, 230, 230);
        static readonly Color linesFinishTracingTint = new Color(new Vector3(1f) - linesTint.ToVector3());

        BloomFilter bloomFilter;

        public PanelGameScreen(Puzzle panel, Point screenSize, GraphicsDevice device, Dictionary<string, Texture2D> TextureProvider, Dictionary<string, SpriteFont> FontProvider, ContentManager Content)
            : this(panel, false, screenSize, device, TextureProvider, FontProvider, Content) { }

        public PanelGameScreen(Puzzle panel, bool standalonePanel, Point screenSize, GraphicsDevice device, Dictionary<string, Texture2D> TextureProvider, Dictionary<string, SpriteFont> FontProvider, ContentManager Content) 
            : base(screenSize, device, TextureProvider, FontProvider, Content)
        {
            if (panel == null)
                panel = new Puzzle(2, 2);

            this.panel = panel;
            IsStandalonePanel = standalonePanel;
            internalBatch = new SpriteBatch(GraphicsDevice);
            renderer = new PanelRenderer(panel, screenSize, TextureProvider, device);
            renderer.SetColorScheme(panel.BackgroundColor, panel.WallsColor);
            startPoints = renderer.StartPoints;
            endPoints = renderer.EndPoints;
            walls = renderer.Walls;

            texPixel = TextureProvider["img/pixel"];
            texCircle = TextureProvider["img/circle"];
            texClose = TextureProvider["img/close"];
            texLike[0] = TextureProvider["img/like0"];
            texLike[1] = TextureProvider["img/like1"];
            texNext = TextureProvider["img/next"];
            texDelete = TextureProvider["img/delete"];
            fntDebug = FontProvider["font/fnt_constantia_small"];

            // Fullscreen textures for 1. background, 2. fading solution lines, 3. red blinking rules for error highlighting and 4. displaying eliminated rules with dim colors
            backgroundTexture = new RenderTarget2D(GraphicsDevice, screenSize.X, screenSize.Y);
            tempLineTexture = new RenderTarget2D(GraphicsDevice, screenSize.X, screenSize.Y);
            lineTexture = new RenderTarget2D(GraphicsDevice, screenSize.X, screenSize.Y, false, GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.Depth24, 1, RenderTargetUsage.PreserveContents);
            lineFadeTexture = new RenderTarget2D(GraphicsDevice, screenSize.X, screenSize.Y);
            errorsBlinkTexture = new RenderTarget2D(GraphicsDevice, screenSize.X, screenSize.Y);
            eliminatedErrorsTexture = new RenderTarget2D(GraphicsDevice, screenSize.X, screenSize.Y);

            // Load bloom shader
            bloomFilter = new BloomFilter();
            bloomFilter.Load(GraphicsDevice, Content, screenSize.X, screenSize.Y);
            bloomFilter.BloomPreset = BloomFilter.BloomPresets.Small;
            bloomFilter.BloomThreshold = 0;
            bloomFilter.BloomStrengthMultiplier = 0.75f;

            renderer.RenderPanelToTexture(backgroundTexture);

            SpawnButtons();
            UpdateButtonsColor();
        }
        private void SpawnButtons()
        {
            TouchButton btnClose = new TouchButton(new Rectangle(), texClose, null);
            btnClose.Click += () => {
                AbortTracing();
                SoundManager.PlayOnce(Sound.MenuOpen);
                ScreenManager.Instance.GoBack();
            };
            buttons.Add(btnClose);

            ToggleButton btnLike = new ToggleButton(new Rectangle(), texLike[1], texLike[0], null, false);
            btnLike.Click += () =>
            {
                // TODO add panel to the list of saved panels

                SoundManager.PlayOnce(btnLike.IsActivated ? Sound.ButtonLike : Sound.ButtonUnlike);
            };
            buttons.Add(btnLike);

            // When panel is standalone, it cannot have Next button
            if (!IsStandalonePanel)
            {
                TwoStateButton btnNext = new TwoStateButton(new Rectangle(), texDelete, texNext, null, null, null, false);
                btnNext.Click += () =>
                {
                    Action callback = null;
                    callback = () =>
                    {
                        AbortTracing();
                        Puzzle nextPanel = DI.Get<PanelGenerator>().GeneratePanel();
                        FileStorageManager.SaveCurrentPanel(nextPanel);
                        LoadNewPanel(nextPanel);
                        btnNext.StateActive = false;
                        btnLike.Reset();
                        fade.FadeOutComplete -= callback;
                    };

                // Add panel to the list of last 10 discarded panels
                if (!btnNext.StateActive)
                        FileStorageManager.AddPanelToDiscardedList(panel);

                    SoundManager.PlayOnce(btnNext.StateActive ? Sound.ButtonNextSuccess : Sound.ButtonNext);
                    fade.FadeOutComplete += callback;
                    fade.Restart();
                };
                buttons.Add(btnNext);
            }

            UpdateButtonsPosition();
        }
        private void UpdateButtonsPosition()
        {
            int screenMin = Math.Min(ScreenSize.X, ScreenSize.Y);
            bool screenHorizontal = ScreenSize.X > ScreenSize.Y;

            // Close
            buttons[0].SetPositionAndSize(new Point(Math.Min((int) (screenMin * 0.05f), 50)), new Point((int) (screenMin * 0.1f)));

            int buttonSize = (int) (screenMin * 0.16f);
            if (screenHorizontal)
            {
                int marginX = renderer.PuzzleConfig.X + renderer.PuzzleConfig.Width;
                int freeSpaceX = ScreenSize.X - marginX;
                marginX += freeSpaceX / 2 - buttonSize / 2;

                if (!IsStandalonePanel)
                {
                    int marginY = ScreenSize.Y / 4 * 3 - buttonSize / 2;
                    // Like
                    buttons[1].SetPositionAndSize(new Point(marginX, marginY), new Point(buttonSize));
                    // Next
                    buttons[2].SetPositionAndSize(new Point(marginX, ScreenSize.Y - marginY - buttonSize), new Point(buttonSize));
                }
                else
                {
                    int marginY = ScreenSize.Y / 2 - buttonSize / 2;
                    // Like
                    buttons[1].SetPositionAndSize(new Point(marginX, marginY), new Point(buttonSize));
                }
            }
            else
            {
                int marginY = renderer.PuzzleConfig.Y + renderer.PuzzleConfig.Height;
                int freeSpaceY = ScreenSize.Y - marginY;
                marginY += freeSpaceY / 2 - buttonSize / 2;

                if (!IsStandalonePanel)
                {
                    int marginX = ScreenSize.X / 4 - buttonSize / 2;
                    // Like
                    buttons[1].SetPositionAndSize(new Point(marginX, marginY), new Point(buttonSize));
                    // Next
                    buttons[2].SetPositionAndSize(new Point(ScreenSize.X - marginX - buttonSize, marginY), new Point(buttonSize));
                }
                else
                {
                    int marginX = ScreenSize.X / 2 - buttonSize / 2;
                    // Like
                    buttons[1].SetPositionAndSize(new Point(marginX, marginY), new Point(buttonSize));
                }
            }
        }
        private void UpdateButtonsColor()
        {
            foreach (var button in buttons)
                button.ChangeTint(panel.ButtonsColor);
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
                renderer.SetColorScheme(panel.BackgroundColor, panel.WallsColor);
                renderer.RenderPanelToTexture(backgroundTexture);
                UpdateButtonsColor();
            }
        }

        private void AbortTracing()
        {
            if (SoundManager.IsPlaying(SoundLoop.Tracing))
                SoundManager.PlayOnce(Sound.AbortTracing);
            SoundManager.StopLoop(SoundLoop.Tracing);
            SoundManager.StopLoop(SoundLoop.PathComplete);
            panelState.ResetToNeutral();
            line = lineMirror = null;
        }
        
        public override void SetScreenSize(Point screenSize)
        {
            base.SetScreenSize(screenSize);
            
            renderer.SetScreenSize(screenSize);
            startPoints = renderer.StartPoints;
            endPoints = renderer.EndPoints;
            walls = renderer.Walls;

            backgroundTexture = new RenderTarget2D(GraphicsDevice, screenSize.X, screenSize.Y);
            tempLineTexture = new RenderTarget2D(GraphicsDevice, screenSize.X, screenSize.Y);
            lineTexture = new RenderTarget2D(GraphicsDevice, screenSize.X, screenSize.Y, false, GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.Depth24, 1, RenderTargetUsage.PreserveContents);
            lineFadeTexture = new RenderTarget2D(GraphicsDevice, screenSize.X, screenSize.Y);
            errorsBlinkTexture = new RenderTarget2D(GraphicsDevice, screenSize.X, screenSize.Y);
            eliminatedErrorsTexture = new RenderTarget2D(GraphicsDevice, screenSize.X, screenSize.Y);

            bloomFilter.UpdateResolution(screenSize.X, screenSize.Y);
            renderer.RenderPanelToTexture(backgroundTexture);

            UpdateButtonsPosition();

            // It's nearly impossible to update line hitboxes to the new screen resolution, because they are created real-time and are pixel perfect
            // So simply abort tracing if lines are active in the moment of screen resize
            AbortTracing();
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
                if (lineAtEndpoint)
                {
                    RenderLinesToTexture(lineFadeTexture, Color.White);

                    if (!panelState.State.HasFlag(PanelStates.FinishTracing))
                    {
                        panelState.SetFinishTracing(true);
                        SoundManager.PlayOnce(Sound.FinishTracing);
                        SoundManager.PlayLoop(SoundLoop.PathComplete);
                    }
                }
                // If line left end point => Produce another sound
                else if (!lineAtEndpoint && panelState.State.HasFlag(PanelStates.FinishTracing))
                {
                    panelState.SetFinishTracing(false);
                    SoundManager.PlayOnce(Sound.AbortFinishTracing);
                    SoundManager.StopLoop(SoundLoop.PathComplete);
                }
            }

            foreach (var button in buttons)
                button.Update(InputManager.GetTapPosition());

            fade.Update();

            if (InputManager.IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.N))
                drawDebug = !drawDebug;
            
            base.Update(gameTime);
        }

        private void HandleScreenTap()
        {
            Point? tap = GetTapPosition();
            if (tap.HasValue)
            {
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
                                            SetPanelSuccess();
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
                                        SetPanelSuccess();
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
        private void SetPanelSuccess()
        {
            panelState.SetSuccess();
            SoundManager.PlayOnce(Sound.Success);
            if (!IsStandalonePanel)
                (buttons[2] as TwoStateButton).StateActive = true;

            // Add panel to the list of last 10 solved panels
            FileStorageManager.AddPanelToSolvedList(panel);
            // Delete current panel save file so it won't be loaded again next time
            FileStorageManager.DeleteCurrentPanel();
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
            // Draw lines to texture and apply blink-highlighting effect if line is at end point
            RenderLinesToTexture(lineTexture);
            AddFinishTracingBlink();

            // Draw bloom when elimination is not not started or when panel is solved
            if (line != null && (!panelState.State.HasFlag(PanelStates.EliminationStarted) || panelState.State.HasFlag(PanelStates.Solved)))
            {
                // We are using separate SpriteBatch to draw bloom, so we have to draw background in this batch too, so it will be below bloom texture
                internalBatch.Begin();
                internalBatch.Draw(backgroundTexture, GraphicsDevice.Viewport.Bounds, Color.White);

                // Idk, more weird majiks, but if i call this Draw outside of this batch or inside next (additive) batch, background goes black
                // So yeah... I guess it stays here now...
                Texture2D bloom = bloomFilter.Draw(lineTexture, ScreenSize.X, ScreenSize.Y);

                internalBatch.End();

                // Now draw bloom texture in additive blend mode
                internalBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive);
                internalBatch.Draw(bloom, GraphicsDevice.Viewport.Bounds, Color.White * 0.8f);
                internalBatch.End();
            }
            else
            {
                // If we don't draw bloom, then we can use the same SpriteBatch to draw background texture
                spriteBatch.Draw(backgroundTexture, GraphicsDevice.Viewport.Bounds, Color.White);
            }

            // Draw line to the backbuffer
            spriteBatch.Draw(lineTexture, GraphicsDevice.Viewport.Bounds, Color.White);
            
            // Draw fading line after aborting tracing or submitting wrong solution
            if (panelState.State.HasFlag(PanelStates.LineFade) && lineFadeTexture != null)
                spriteBatch.Draw(lineFadeTexture, GraphicsDevice.Viewport.Bounds, (panelState.State.HasFlag(PanelStates.ErrorHappened) ? Color.Black : Color.White) * panelState.LineFadeOpacity);

            // Draw red blinking rules after submitting the wrong solution
            if (panelState.State.HasFlag(PanelStates.ErrorBlink) && errorsBlinkTexture != null)
                spriteBatch.Draw(errorsBlinkTexture, GraphicsDevice.Viewport.Bounds, Color.White * panelState.ErrorBlinkOpacity);

            // Animate fading into gray of eliminated rules
            if (panelState.State.HasFlag(PanelStates.EliminationFinished) && eliminatedErrorsTexture != null)
                spriteBatch.Draw(eliminatedErrorsTexture, GraphicsDevice.Viewport.Bounds, Color.White * panelState.EliminationFadeOpacity);

            // Draw buttons
            foreach (var button in buttons)
                button.Draw(spriteBatch);

            // Draw node IDs
            if (drawDebug)
                DebugDrawNodeIDs(spriteBatch);

            if (fade.IsActive)
                spriteBatch.Draw(texPixel, new Rectangle(Point.Zero, ScreenSize), Color.Black * fade.Opacity);
            
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
            // First draw the lines to the temp texture
            GraphicsDevice.SetRenderTarget(tempLineTexture);
            GraphicsDevice.Clear(Color.Transparent);

            internalBatch.Begin(SpriteSortMode.Texture);
            DrawLines(internalBatch, fillColor);
            internalBatch.End();

            // Now switch render target to the actual target texture and draw temp one onto it with color tint if necessary
            GraphicsDevice.SetRenderTarget(canvas);
            GraphicsDevice.Clear(Color.Transparent);

            internalBatch.Begin();
            internalBatch.Draw(tempLineTexture, canvas.Bounds, (fillColor == null && !panelState.State.HasFlag(PanelStates.Solved)) ? linesTint : Color.White);
            internalBatch.End();

            // Drop the render target
            GraphicsDevice.SetRenderTarget(null);
        }
        private void AddFinishTracingBlink()
        {
            if (panelState.State.HasFlag(PanelStates.FinishTracing))
            {
                // When finish tracing is happening, lineFadeTexture has the image of lines in white color
                // Draw this image on top of the lines with required tint in additive blend mode
                GraphicsDevice.SetRenderTarget(lineTexture);
                internalBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive);

                internalBatch.Draw(lineFadeTexture, lineTexture.Bounds, linesFinishTracingTint * panelState.FinishTracingBlinkOpacity);

                internalBatch.End();
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
