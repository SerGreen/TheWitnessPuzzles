using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace TWP_Shared
{
    public class MenuGameScreen : GameScreen
    {
        SpriteFont font = null;
        SpriteFont fontSmall = null;
        
        Texture2D texPixel;
        FadeInAnimation fadeIn;
        Rectangle menuBounds;
        int menuButtonHeight;
        List<TextButton> buttons = new List<TextButton>();

        RenderTarget2D backgroundTexture;

        public MenuGameScreen(Point screenSize, GraphicsDevice device, Dictionary<string, Texture2D> TextureProvider, Dictionary<string, SpriteFont> FontProvider, ContentManager Content, params object[] data)
            : base(screenSize, device, TextureProvider, FontProvider, Content, data)
        {
            font = FontProvider["font/fnt_constantia_big"];
            fontSmall = FontProvider["font/fnt_small"];
            
            texPixel = TextureProvider["img/pixel"];
            
            InitializeScreenSizeDependent();
            SpawnButtons();
            fadeIn = new FadeInAnimation(120);
            fadeIn.Restart();
        }

        private void InitializeScreenSizeDependent()
        {
            backgroundTexture = new RenderTarget2D(GraphicsDevice, ScreenSize.X, ScreenSize.Y);
            RenderBackgroundToTexture();
        }

        public override void SetScreenSize(Point screenSize)
        {
            base.SetScreenSize(screenSize);
            InitializeScreenSizeDependent();
            UpdateButtonsPosition();
        }

        public override void Activate ()
        {
            base.Activate();
            RenderBackgroundToTexture();
        }

        private void UpdateButtonsPosition()
        {
            for (int i = 0; i < buttons.Count; i++)
            {
                buttons[i].SetPositionAndSize(menuBounds.Location + new Point(0, i * menuButtonHeight), new Point(menuBounds.Width, menuButtonHeight));
            }
        }
        private void SpawnButtons()
        {
            TextButton btnStart = new TextButton(new Rectangle(), font, "Start", texPixel);
            btnStart.Click += () => {
                TheWitnessPuzzles.Puzzle currentPanel = FileStorageManager.LoadCurrentPanel();
                if (currentPanel == null)
                {
                    int? seed = SettingsManager.IsSequentialMode ? (int?)SettingsManager.CurrentSequentialSeed : null;
                    currentPanel = DI.Get<PanelGenerator>().GeneratePanel(seed);
                    FileStorageManager.SaveCurrentPanel(currentPanel);
                }

                ScreenManager.Instance.AddScreen<PanelGameScreen>(false, true, currentPanel);
                SoundManager.PlayOnce(Sound.MenuEnter);
            };

            TextButton btnHistory = new TextButton(new Rectangle(), font, "Library", texPixel);
            btnHistory.Click += () => {
                ScreenManager.Instance.AddScreen<HistoryGameScreen>(false, true);
                SoundManager.PlayOnce(Sound.MenuEnter);
            };

            TextButton btnSettings = new TextButton(new Rectangle(), font, "Settings", texPixel);
            btnSettings.Click += () => {
                ScreenManager.Instance.AddScreen<SettingsGameScreen>(false, true);
                SoundManager.PlayOnce(Sound.MenuEnter);
            };

            TextButton btnAbout = new TextButton(new Rectangle(), font, "About", texPixel);
            btnAbout.Click += () => {
                ScreenManager.Instance.AddScreen<AboutGameScreen>(false, true);
                SoundManager.PlayOnce(Sound.MenuEnter);
            };

            buttons.Add(btnStart);
            buttons.Add(btnHistory);
            buttons.Add(btnSettings);
            buttons.Add(btnAbout);

            UpdateButtonsPosition();
        }

        public override void Update(GameTime gameTime)
        {
            fadeIn.Update();
            foreach (var btn in buttons)
                btn.Update(InputManager.GetTapPosition());

            base.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(backgroundTexture, Vector2.Zero, Color.White);
            
            foreach (var btn in buttons)
                btn.Draw(spriteBatch, fadeIn.Opacity);

            base.Draw(spriteBatch);
        }

        private void RenderBackgroundToTexture()
        {
            using (SpriteBatch spriteBatch = new SpriteBatch(GraphicsDevice))
            {
                // Set the render target
                GraphicsDevice.SetRenderTarget(backgroundTexture);
                
                GraphicsDevice.Clear(Color.Black);
                spriteBatch.Begin();

                string firstLine = "The Witness";
                string secondLine = "Puzzles";
                string sequentialModeLine = $"[ {SettingsManager.CurrentSequentialSeed:D9} ]";

                Vector2 firstLineSize = font.MeasureString(firstLine);
                Vector2 secondLineSize = font.MeasureString(secondLine);
                Vector2 sequentialModeLineSize = fontSmall.MeasureString(sequentialModeLine);

                int minScreenSize = Math.Min(ScreenSize.X, ScreenSize.Y);
                float fontScale = (minScreenSize * 0.75f) / firstLineSize.X;
                float fontScaleSmall = fontScale * 0.6f;
                firstLineSize = new Vector2(firstLineSize.X * fontScale, firstLineSize.Y * fontScale);
                secondLineSize = new Vector2(secondLineSize.X * fontScale, secondLineSize.Y * fontScale);
                sequentialModeLineSize = new Vector2(sequentialModeLineSize.X * fontScaleSmall, sequentialModeLineSize.Y * fontScaleSmall);

                Vector2 firstLinePosition = new Vector2(ScreenSize.X / 2 - firstLineSize.X / 2, ScreenSize.Y * 0.07f);
                Vector2 secondLinePosition = new Vector2(ScreenSize.X / 2 - secondLineSize.X / 2, firstLinePosition.Y + firstLineSize.Y * 0.85f);
                Vector2 sequentialModeLinePosition = new Vector2(ScreenSize.X / 2 - sequentialModeLineSize.X / 2, ScreenSize.Y - sequentialModeLineSize.Y - 10);

                spriteBatch.DrawString(font, firstLine, firstLinePosition, Color.White, 0, Vector2.Zero, fontScale, SpriteEffects.None, 0);
                spriteBatch.DrawString(font, secondLine, secondLinePosition, Color.White, 0, Vector2.Zero, fontScale, SpriteEffects.None, 0);
                
                if (SettingsManager.IsSequentialMode)
                    spriteBatch.DrawString(fontSmall, sequentialModeLine, sequentialModeLinePosition, Color.DarkGray, 0, Vector2.Zero, fontScaleSmall, SpriteEffects.None, 0);
                
                if (ScreenSize.X > ScreenSize.Y)
                    menuButtonHeight = (int) (ScreenSize.Y * 0.1f);
                else
                    menuButtonHeight = (int) (firstLineSize.Y * 0.8f);
                float menuWidth = ScreenSize.X * 0.8f;
                float menuTop = secondLinePosition.Y + secondLineSize.Y;
                float menuHeight = menuButtonHeight * 4;
                menuTop += (ScreenSize.Y - menuTop - menuHeight) / 2;
                
                menuBounds = new Rectangle((int) (ScreenSize.X - menuWidth) / 2, (int) menuTop, (int) menuWidth, (int) menuHeight);

                spriteBatch.End();
                GraphicsDevice.SetRenderTarget(null);   // Drop the render target
            }
        }
    }
}
