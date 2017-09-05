using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace TWP_Shared
{
    public class SplashGameScreen : GameScreen
    {
        SpriteFont font = null;

        Random rnd = new Random();
        List<Texture2D> texSymbols;
        Texture2D currentSymbol;
        FadeOutAnimation fadeOut;
        Vector2 symbolPosition;
        float symbolScale;

        Texture2D texPixel;
        FadeInAnimation fadeIn;
        Rectangle menuBounds;
        int menuButtonHeight;
        List<TextButton> buttons = new List<TextButton>();

        RenderTarget2D backgroundTexture;

        public SplashGameScreen(Point screenSize, GraphicsDevice device, Dictionary<string, Texture2D> TextureProvider, Dictionary<string, SpriteFont> FontProvider, ContentManager Content) 
            : base(screenSize, device, TextureProvider, FontProvider, Content)
        {
            font = FontProvider["font/fnt_constantia_big"];

            texSymbols = new List<Texture2D>()
            {
                TextureProvider["img/hexagon"],
                TextureProvider["img/square"],
                //TextureProvider["img/triangle1"],
                TextureProvider["img/elimination"],
                TextureProvider["img/sun"]
            };
            currentSymbol = texSymbols[rnd.Next(texSymbols.Count)];
            //fadeOut = new FadeOutAnimation(60, 150);
            fadeOut = new FadeOutAnimation(60, 20);
            Action fadeOutCallback = null;
            fadeOutCallback = () => {
                currentSymbol = null;
                SpawnButtons();
                fadeIn.Restart();
                fadeOut.FadeComplete -= fadeOutCallback;
            };
            fadeOut.FadeComplete += fadeOutCallback;
            fadeOut.Restart();
            
            texPixel = TextureProvider["img/pixel"];
            fadeIn = new FadeInAnimation(60);

            InitializeScreenSizeDependent();
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
                    currentPanel = DI.Get<PanelGenerator>().GeneratePanel();
                    FileStorageManager.SaveCurrentPanel(currentPanel);
                }

                ScreenManager.Instance.AddScreen<PanelGameScreen>(false, true, currentPanel);
                SoundManager.PlayOnce(Sound.MenuEnter);
            };

            TextButton btnHistory = new TextButton(new Rectangle(), font, "History", texPixel);
            btnHistory.Click += () => {
                ScreenManager.Instance.AddScreen<HistoryGameScreen>(false, true);
                SoundManager.PlayOnce(Sound.MenuEnter);
            };

            TextButton btnSettings = new TextButton(new Rectangle(), font, "Settings", texPixel);
            btnSettings.Click += () => {
                ScreenManager.Instance.AddScreen<SettingsGameScreen>(false, true);
                SoundManager.PlayOnce(Sound.MenuEnter);
            };

            buttons.Add(btnStart);
            buttons.Add(btnHistory);
            buttons.Add(btnSettings);

            UpdateButtonsPosition();
        }

        public override void Update(GameTime gameTime)
        {
            fadeOut.Update();
            fadeIn.Update();
            foreach (var btn in buttons)
                btn.Update(InputManager.GetTapPosition());

            base.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(backgroundTexture, Vector2.Zero, Color.White);

            if(currentSymbol != null)
                spriteBatch.Draw(currentSymbol, symbolPosition, null, Color.White * fadeOut.Opacity, 0, currentSymbol.Bounds.Center.ToVector2(), symbolScale, SpriteEffects.None, 0);

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

                Vector2 firstLineSize = font.MeasureString(firstLine);
                Vector2 secondLineSize = font.MeasureString(secondLine);

                int minScreenSize = Math.Min(ScreenSize.X, ScreenSize.Y);
                float fontScale = (minScreenSize * 0.8f) / firstLineSize.X;
                firstLineSize = new Vector2(firstLineSize.X * fontScale, firstLineSize.Y * fontScale);
                secondLineSize = new Vector2(secondLineSize.X * fontScale, secondLineSize.Y * fontScale);

                Vector2 firstLinePosition = new Vector2(ScreenSize.X / 2 - firstLineSize.X / 2, ScreenSize.Y * 0.1f);
                Vector2 secondLinePosition = new Vector2(ScreenSize.X / 2 - secondLineSize.X / 2, firstLinePosition.Y + firstLineSize.Y * 0.85f);

                spriteBatch.DrawString(font, firstLine, firstLinePosition, Color.White, 0, Vector2.Zero, fontScale, SpriteEffects.None, 0);
                spriteBatch.DrawString(font, secondLine, secondLinePosition, Color.White, 0, Vector2.Zero, fontScale, SpriteEffects.None, 0);

                if (currentSymbol != null)
                {
                    symbolPosition = new Vector2(ScreenSize.X / 2, secondLinePosition.Y + (ScreenSize.Y - secondLinePosition.Y) / 2);
                    symbolScale = (minScreenSize * 0.25f) / currentSymbol.Width;
                }

                menuButtonHeight = (int) (firstLineSize.Y * 0.8f);
                float menuWidth = ScreenSize.X * 0.8f;
                float menuTop = secondLinePosition.Y + secondLineSize.Y;
                float menuHeight = menuButtonHeight * 3;
                menuTop += (ScreenSize.Y - menuTop - menuHeight) / 2;
                
                menuBounds = new Rectangle((int) (ScreenSize.X - menuWidth) / 2, (int) menuTop, (int) menuWidth, (int) menuHeight);

                spriteBatch.End();
                GraphicsDevice.SetRenderTarget(null);   // Drop the render target
            }
        }
    }
}
