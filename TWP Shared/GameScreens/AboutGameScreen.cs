using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TWP_Shared
{
    public class AboutGameScreen : GameScreen
    {
        private const string githubUrl = @"https://github.com/SerGreen/BowRunner";
        private const string theWitnessUrl = @"https://store.steampowered.com/app/210970";

        TouchButton btnBack, btnLinkGithub, btnLinkTheWitness;
        RenderTarget2D backgroundTexture;

        SpriteFont fntBig, fntSmall;
        Texture2D texPixel;

        string caption = "About";
        string[] text =
            {
                "Hello",
                "World",
                "!!!",
                "text",
                "text",
                "text",
                "text",
                "text"
            };
        Rectangle textArea;
        int lineHeight;
        float captionFontScale, textScale;
        Point backButtonSize, linkButtonSize, captionSize;
        Point backButtonPosition, linkGithubPosition, linkTheWitnessPosition, captionPosition;

        public AboutGameScreen(Point screenSize, GraphicsDevice device, Dictionary<string, Texture2D> textureProvider, Dictionary<string, SpriteFont> fontProvider, ContentManager Content) 
            : base(screenSize, device, textureProvider, fontProvider, Content)
        {
            fntBig = FontProvider["font/fnt_constantia_big"];
            fntSmall = FontProvider["font/fnt_small"];
            texPixel = textureProvider["img/pixel"];

            InitializeScreenSizeDependent();
            SpawnButtons();
        }

        private void InitializeScreenSizeDependent()
        {
            Vector2 stringSize = fntBig.MeasureString(caption);

            int minScreenSize = Math.Min(ScreenSize.X, ScreenSize.Y);
            captionFontScale = (minScreenSize * 0.4f) / stringSize.X;
            captionSize = new Point((int) (stringSize.X * captionFontScale), (int) (stringSize.Y * captionFontScale));

            captionPosition = new Point(ScreenSize.X / 2 - captionSize.X / 2, (int) (ScreenSize.Y * 0.05f));

            backButtonSize = new Point((int) (minScreenSize * 0.5f), (int) (minScreenSize * 0.1f));
            backButtonPosition = new Point(ScreenSize.X / 2 - backButtonSize.X / 2, (int) (ScreenSize.Y - backButtonSize.Y * 1.5f));

            int textAreaTop = captionPosition.Y + captionSize.Y;
            int heightLeft = (int) (ScreenSize.Y - textAreaTop - backButtonSize.Y * 1.5f);
            
            int textAreaHeight = (int) (heightLeft * 0.7f);
            textAreaTop += (int) (heightLeft - textAreaHeight) / 2;

            textArea = new Rectangle((int) (ScreenSize.X * 0.1f), textAreaTop, (int) (ScreenSize.X * 0.8f), textAreaHeight);

            lineHeight = (int) (textAreaHeight / ((text.Length + 4) * 1.2f));
            textScale = lineHeight / fntSmall.MeasureString("V").Y;

            linkButtonSize = new Point((int) (textArea.Width * 0.75f), lineHeight);
            linkGithubPosition = new Point(textArea.X + textArea.Width - linkButtonSize.X, (int) (textArea.Y + textArea.Height - lineHeight * 1.2f * 3));
            linkTheWitnessPosition = new Point(textArea.X + textArea.Width - linkButtonSize.X, (int) (textArea.Y + textArea.Height - lineHeight * 1.2f));

            backgroundTexture = new RenderTarget2D(GraphicsDevice, ScreenSize.X, ScreenSize.Y);
            RenderBackgroundToTexture();
        }

        private void SpawnButtons()
        {
            btnLinkGithub = new TextButton(new Rectangle(), fntSmall, githubUrl, texPixel);
            btnLinkGithub.Click += () =>
            {
                System.Threading.Tasks.Task.Run(() => System.Diagnostics.Process.Start(githubUrl));
                SoundManager.PlayOnce(Sound.ButtonNext);
            };

            btnLinkTheWitness = new TextButton(new Rectangle(), fntSmall, theWitnessUrl, texPixel);
            btnLinkTheWitness.Click += () =>
            {
                System.Threading.Tasks.Task.Run(() => System.Diagnostics.Process.Start(theWitnessUrl));
                SoundManager.PlayOnce(Sound.ButtonNext);
            };

            btnBack = new TextButton(new Rectangle(), fntBig, "Back", texPixel);
            btnBack.Click += () =>
            {
                ScreenManager.Instance.GoBack();
                SoundManager.PlayOnce(Sound.MenuEscape);
            };

            UpdateButtonsPositionAndSize();
        }

        private void UpdateButtonsPositionAndSize()
        {
            btnLinkGithub.SetPositionAndSize(linkGithubPosition, linkButtonSize);
            btnLinkTheWitness.SetPositionAndSize(linkTheWitnessPosition, linkButtonSize);
            btnBack.SetPositionAndSize(backButtonPosition, backButtonSize);
        }

        public override void SetScreenSize(Point screenSize)
        {
            base.SetScreenSize(screenSize);
            InitializeScreenSizeDependent();
            UpdateButtonsPositionAndSize();
        }

        public override void Update(GameTime gameTime)
        {
            Point? tap = InputManager.GetTapPosition();
            btnLinkGithub.Update(tap);
            btnLinkTheWitness.Update(tap);
            btnBack.Update(tap);
        }


        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(backgroundTexture, Vector2.Zero, Color.White);
            btnLinkGithub.Draw(spriteBatch);
            btnLinkTheWitness.Draw(spriteBatch);
            btnBack.Draw(spriteBatch);
            base.Draw(spriteBatch);
        }

        private void RenderBackgroundToTexture()
        {
            using (SpriteBatch spriteBatch = new SpriteBatch(GraphicsDevice))
            {
                GraphicsDevice.SetRenderTarget(backgroundTexture);

                GraphicsDevice.Clear(Color.Black);
                spriteBatch.Begin();

                spriteBatch.DrawString(fntBig, caption, captionPosition.ToVector2(), Color.White, 0, Vector2.Zero, captionFontScale, SpriteEffects.None, 0);

                for (int i = 0; i < text.Length; i++)
                {
                    spriteBatch.DrawString(fntSmall, text[i], textArea.Location.ToVector2() + new Vector2(0, lineHeight * i * 1.2f), Color.White, 0, Vector2.Zero, textScale, SpriteEffects.None, 0);
                }

                spriteBatch.End();
                GraphicsDevice.SetRenderTarget(null);
            }
        }
    }
}
