using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;

namespace TWP_Shared
{
    public class AboutGameScreen : GameScreen
    {
        private const string githubUrl = @"https://github.com/SerGreen/TheWitnessPuzzles";
        private const string theWitnessUrl = @"https://store.steampowered.com/app/210970";

        TouchButton btnBack, btnLinkGithub, btnLinkTheWitness;
        RenderTarget2D backgroundTexture;

        SpriteFont fntBig, fntSmall;
        Texture2D texPixel;
        
        string[] text =
            {
                "DISCLAIMER",
                "",
                "This game is inspired by amazing game The Witness, ",
                "which was created by Jonathan Blow.",
                "",
                "If you have not played his games yet ",
                "(The Witness and Braid), you should definitely do it.",
                "",
                "All rights to The Witness and its sound assets ",
                "belong to Jonathan Blow and Thekla Inc.",
                "",
                "SerGreen — 2017"
            };
        Rectangle textArea;
        int lineHeight;
        float textScale;
        Point backButtonSize, linkButtonSize;
        Point backButtonPosition, linkGithubPosition, linkTheWitnessPosition;

        public AboutGameScreen(Point screenSize, GraphicsDevice device, Dictionary<string, Texture2D> textureProvider, Dictionary<string, SpriteFont> fontProvider, ContentManager Content) 
            : base(screenSize, device, textureProvider, fontProvider, Content)
        {
            fntBig = FontProvider["font/fnt_constantia_big"];
            fntSmall = FontProvider["font/fnt_small"];
            texPixel = textureProvider["img/pixel"];

            InitializeScreenSizeDependent();
            SpawnButtons(textureProvider["img/btn_github"], textureProvider["img/btn_thewitness"]);
        }

        private void InitializeScreenSizeDependent()
        {
            int minScreenSize = Math.Min(ScreenSize.X, ScreenSize.Y);

            backButtonSize = new Point((int) (minScreenSize * 0.5f), (int) (minScreenSize * 0.1f));
            backButtonPosition = new Point(ScreenSize.X / 2 - backButtonSize.X / 2, (int) (ScreenSize.Y - backButtonSize.Y * 1.5f));
            
            int heightLeft = (int) (ScreenSize.Y - backButtonSize.Y * 1.5f);
            
            int textAreaHeight = (int) (heightLeft * 0.7f);
            int textAreaTop = (heightLeft - textAreaHeight) / 2;

            textArea = new Rectangle((int) (ScreenSize.X * 0.1f), textAreaTop, (int) (ScreenSize.X * 0.8f), textAreaHeight);

            lineHeight = (int) (textAreaHeight / ((text.Length + 3) * 1.2f));

            string longestString = text.First(x => x.Length == text.Max(z => z.Length));
            Vector2 longestMeasure = fntSmall.MeasureString(longestString);
            textScale = Math.Min(lineHeight / longestMeasure.Y, textArea.Width / longestMeasure.X);

            int linkButtonWidth = (int) (minScreenSize / 2.75f);
            linkButtonSize = new Point(linkButtonWidth, linkButtonWidth / 3);
            linkGithubPosition = new Point(textArea.X + textArea.Width / 2 - (int) (linkButtonWidth * 1.12f), textArea.Y + textArea.Height - linkButtonSize.Y / 2);
            linkTheWitnessPosition = new Point(textArea.X + textArea.Width / 2 + (int) (linkButtonWidth * 0.12f), textArea.Y + textArea.Height - linkButtonSize.Y / 2);

            backgroundTexture = new RenderTarget2D(GraphicsDevice, ScreenSize.X, ScreenSize.Y);
            RenderBackgroundToTexture();
        }

        private void SpawnButtons(Texture2D texGithub, Texture2D texTheWitness)
        {
            btnLinkGithub = new TouchButton(new Rectangle(), texGithub, null, Color.LightGray);
            btnLinkGithub.Click += () =>
            {
                OpenURL(githubUrl);
                SoundManager.PlayOnce(Sound.ButtonNext);
            };

            btnLinkTheWitness = new TouchButton(new Rectangle(), texTheWitness, null, Color.LightGray);
            btnLinkTheWitness.Click += () =>
            {
                OpenURL(theWitnessUrl);
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

        private void OpenURL(string url)
        {
            // Opening URL takes a bit of time, so we are using beckground thread to not delay the sound of pressed button
            System.Threading.Tasks.Task.Run(() =>
            {
#if WINDOWS
                System.Diagnostics.Process.Start(url);
#elif ANDROID
                var intent = new Android.Content.Intent(Android.Content.Intent.ActionView, Android.Net.Uri.Parse(url));
                intent.AddFlags(Android.Content.ActivityFlags.NewTask);
                Android.App.Application.Context.StartActivity(intent);
#endif
            });
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

                for (int i = 0; i < text.Length; i++)
                {
                    float mod = i == 0 ? 1.6f : 1;
                    Vector2 measure = fntSmall.MeasureString(text[i]) * textScale * mod;
                    spriteBatch.DrawString(fntSmall, text[i], textArea.Location.ToVector2() + new Vector2(textArea.Width / 2 - measure.X / 2, lineHeight * i * 1.2f), Color.White, 0, Vector2.Zero, textScale * mod, SpriteEffects.None, 0);
                }

                spriteBatch.End();
                GraphicsDevice.SetRenderTarget(null);
            }
        }
    }
}
