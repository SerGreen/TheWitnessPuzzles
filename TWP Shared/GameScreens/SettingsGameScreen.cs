using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TWP_Shared
{
    public class SettingsGameScreen : GameScreen
    {
        SpriteFont font = null;
        Texture2D texPixel;
        Texture2D[] texCheckbox = new Texture2D[2];
        Texture2D[] texSound = new Texture2D[2];

        Rectangle buttonsArea;
        int buttonHeight;
        List<TouchButton> buttons = new List<TouchButton>();

        RenderTarget2D backgroundTexture;

        public SettingsGameScreen(Point screenSize, GraphicsDevice device, Dictionary<string, Texture2D> textureProvider, Dictionary<string, SpriteFont> fontProvider, ContentManager Content) 
            : base(screenSize, device, textureProvider, fontProvider, Content)
        {
            font = FontProvider["font/fnt_constantia_big"];
            texPixel = textureProvider["img/pixel"];
            texCheckbox[0] = textureProvider["img/checkbox0"];
            texCheckbox[1] = textureProvider["img/checkbox1"];
            texSound[0] = textureProvider["img/sound0"];
            texSound[1] = textureProvider["img/sound1"];

            InitializeScreenSizeDependent();
            SpawnButtons();
        }

        private void InitializeScreenSizeDependent()
        {
            backgroundTexture = new RenderTarget2D(GraphicsDevice, ScreenSize.X, ScreenSize.Y);
            RenderBackgroundToTexture();
        }

        private void SpawnButtons()
        {
            ToggleButton btnMute = new ToggleButton(new Rectangle(buttonsArea.Location + new Point(buttonsArea.Width - buttonHeight, 0), new Point(buttonHeight, buttonHeight)), texSound[1], texSound[0], null, !SettingsManager.IsMute);
            btnMute.Click += () => {
                SettingsManager.IsMute = !btnMute.IsActivated;
                SoundManager.PlayOnce(Sound.MenuEnter);
            };

            ToggleButton btnFullscreen = new ToggleButton(new Rectangle(buttonsArea.Location + new Point(buttonsArea.Width - buttonHeight, (int) (buttonHeight * 1.2f)), new Point(buttonHeight, buttonHeight)), texCheckbox[1], texCheckbox[0], null, SettingsManager.IsFullscreen);
            btnFullscreen.Click += () => {
                SettingsManager.IsFullscreen = btnFullscreen.IsActivated;
                ScreenManager.Instance.UpdateFullscreen();
                SoundManager.PlayOnce(Sound.MenuEnter);
            };

            ToggleButton btnVFX = new ToggleButton(new Rectangle(buttonsArea.Location + new Point(buttonsArea.Width - buttonHeight, (int) (buttonHeight * 2.4f)), new Point(buttonHeight, buttonHeight)), texCheckbox[1], texCheckbox[0], null, SettingsManager.VFX);
            btnVFX.Click += () => {
                SettingsManager.VFX = btnVFX.IsActivated;
                SoundManager.PlayOnce(Sound.MenuEnter);
            };

            TextTouchButton btnBack = new TextTouchButton(new Rectangle(new Point(buttonsArea.X, ScreenSize.Y - buttonHeight * 2), new Point(buttonsArea.Width, (int) (buttonHeight * 1.5f))), font, "Back", texPixel);
            btnBack.Click += () => {
                SettingsManager.SaveSettings();
                ScreenManager.Instance.GoBack();
                SoundManager.PlayOnce(Sound.MenuEscape);
            };

            buttons.Add(btnMute);
            buttons.Add(btnFullscreen);
            buttons.Add(btnVFX);
            buttons.Add(btnBack);
        }

        public override void SetScreenSize(Point screenSize)
        {
            base.SetScreenSize(screenSize);
            InitializeScreenSizeDependent();
            for (int i = 0; i < buttons.Count - 1; i++)
                buttons[i].SetPositionAndSize(buttonsArea.Location + new Point(buttonsArea.Width - buttonHeight, (int) (buttonHeight * i * 1.2f)), new Point(buttonHeight, buttonHeight));

            buttons[buttons.Count-1].SetPositionAndSize(new Point(buttonsArea.X, screenSize.Y - buttonHeight * 2), new Point(buttonsArea.Width, (int) (buttonHeight*1.5f)));
        }

        public override void Update(GameTime gameTime)
        {
            foreach (var btn in buttons)
                btn.Update(InputManager.GetTapPosition());

            base.Update(gameTime);
        }


        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(backgroundTexture, Vector2.Zero, Color.White);
            
            foreach (var btn in buttons)
                btn.Draw(spriteBatch);

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

                string caption = "Settings";

                Vector2 captionSize = font.MeasureString(caption);

                int minScreenSize = Math.Min(ScreenSize.X, ScreenSize.Y);
                float fontScale = (minScreenSize * 0.6f) / captionSize.X;
                captionSize = new Vector2(captionSize.X * fontScale, captionSize.Y * fontScale);

                Vector2 captionPosition = new Vector2(ScreenSize.X / 2 - captionSize.X / 2, ScreenSize.Y * 0.1f);

                spriteBatch.DrawString(font, caption, captionPosition, Color.White, 0, Vector2.Zero, fontScale, SpriteEffects.None, 0);
                
                float buttonsAreaWidth = minScreenSize * 0.8f;
                buttonHeight = (int) (captionSize.Y * 0.5f);
                float buttonsAreaTop = captionPosition.Y + captionSize.Y;
                buttonsAreaTop += (ScreenSize.Y - buttonsAreaTop - buttonHeight * 4 * 1.2f) / 2;
                buttonsArea = new Rectangle((int) (ScreenSize.X - buttonsAreaWidth) / 2, (int) buttonsAreaTop, (int) buttonsAreaWidth, (int) (ScreenSize.Y - buttonsAreaTop));

                Vector2 textSize = font.MeasureString("V");
                float scale = buttonHeight / textSize.Y;
                spriteBatch.DrawString(font, "Sound FX", new Vector2(buttonsArea.X, buttonsArea.Y), Color.White, 0, Vector2.Zero, scale, SpriteEffects.None, 0);
                spriteBatch.DrawString(font, "Fullscreen", new Vector2(buttonsArea.X, buttonsArea.Y + buttonHeight * 1.2f), Color.White, 0, Vector2.Zero, scale, SpriteEffects.None, 0);
                spriteBatch.DrawString(font, "Visual FX", new Vector2(buttonsArea.X, buttonsArea.Y + buttonHeight * 2.4f), Color.White, 0, Vector2.Zero, scale, SpriteEffects.None, 0);


                spriteBatch.End();
                GraphicsDevice.SetRenderTarget(null);   // Drop the render target
            }
        }
    }
}
