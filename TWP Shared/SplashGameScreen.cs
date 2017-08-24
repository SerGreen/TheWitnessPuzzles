using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

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
        List<TextTouchButton> buttons = new List<TextTouchButton>();

        RenderTarget2D backgroundTexture;

        public SplashGameScreen(Point screenSize, GraphicsDevice device, Dictionary<string, Texture2D> TextureProvider, Dictionary<string, SpriteFont> FontProvider) 
            : base(screenSize, device, TextureProvider, FontProvider)
        {
            font = FontProvider["font/fnt_constantia36"];

            texSymbols = new List<Texture2D>()
            {
                TextureProvider["img/twp_hexagon"],
                TextureProvider["img/twp_square"],
                //TextureProvider["img/twp_triangle1"],
                TextureProvider["img/twp_elimination"],
                TextureProvider["img/twp_sun"]
            };
            currentSymbol = texSymbols[rnd.Next(texSymbols.Count)];
            fadeOut = new FadeOutAnimation(60, 150);
            Action fadeOutCallback = null;
            fadeOutCallback = () => {
                currentSymbol = null;
                SpawnButtons();
                fadeIn.Restart();
                fadeOut.FadeComplete -= fadeOutCallback;
            };
            fadeOut.FadeComplete += fadeOutCallback;
            fadeOut.Restart();
            
            backgroundTexture = new RenderTarget2D(GraphicsDevice, GraphicsDevice.PresentationParameters.BackBufferWidth, GraphicsDevice.PresentationParameters.BackBufferHeight, false, GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.Depth24);
            RenderBackgroundToTexture();

            texPixel = TextureProvider["img/twp_pixel"];
            fadeIn = new FadeInAnimation(60);
        }

        private void SpawnButtons()
        {
            TextTouchButton btnStart = new TextTouchButton(new Rectangle(menuBounds.Location + new Point(0, 0), new Point(menuBounds.Width, menuButtonHeight)), font, "Start", texPixel);
            btnStart.Click += () => {
                ScreenManager.Instance.AddScreen<PanelGameScreen>(false, true, PanelGenerator.GeneratePanel());
                SoundManager.PlayOnce(Sound.MenuEnter);
            };

            TextTouchButton btnEditor = new TextTouchButton(new Rectangle(menuBounds.Location + new Point(0, menuButtonHeight), new Point(menuBounds.Width, menuButtonHeight)), font, "Editor", texPixel);
            btnEditor.Click += () => {
                //ScreenManager.Instance.AddScreen(new PanelGameScreen(ScreenSize, GraphicsDevice), false, true);
                SoundManager.PlayOnce(Sound.MenuEnter);
            };

            buttons.Add(btnStart);
            buttons.Add(btnEditor);
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

                // Draw the lines
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

                symbolPosition = new Vector2(ScreenSize.X / 2, secondLinePosition.Y + (ScreenSize.Y - secondLinePosition.Y) / 2);
                symbolScale = (minScreenSize * 0.25f) / currentSymbol.Width;

                //spriteBatch.Draw(currentSymbol, symbolPosition, null, Color.White, 0, currentSymbol.Bounds.Center.ToVector2(), symbolScale, SpriteEffects.None, 0);

                float menuWidth = ScreenSize.X * 0.8f;
                float menuHeight = secondLinePosition.Y + secondLineSize.Y * 2;
                menuButtonHeight = (int) (firstLineSize.Y * 0.8f);
                menuBounds = new Rectangle((int) (ScreenSize.X - menuWidth) / 2, (int) menuHeight, (int) menuWidth, (int) (ScreenSize.Y - menuHeight));

                spriteBatch.End();
                // Drop the render target
                GraphicsDevice.SetRenderTarget(null);
            }
        }
    }
}
