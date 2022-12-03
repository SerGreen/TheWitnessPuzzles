using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TWP_Shared
{
    public class ConfirmSeedResetGameScreen : GameScreen
    {
        const float btnHeightMod = 1.2f;

        SpriteFont font = null;
        Texture2D texPixel;
        Texture2D[] texCheckbox = new Texture2D[2];

        Rectangle buttonsArea;
        int buttonHeight;
        List<AbstractButton> buttons = new List<AbstractButton>();

        RenderTarget2D backgroundTexture;

        public ConfirmSeedResetGameScreen(Point screenSize, GraphicsDevice device, Dictionary<string, Texture2D> textureProvider, Dictionary<string, SpriteFont> fontProvider, ContentManager Content, params object[] data) 
            : base(screenSize, device, textureProvider, fontProvider, Content, data)
        {
            font = FontProvider["font/fnt_constantia_big"];
            texPixel = textureProvider["img/pixel"];
            texCheckbox[0] = textureProvider["img/checkbox0"];
            texCheckbox[1] = textureProvider["img/checkbox1"];

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
            TextButton btnReset = new TextButton(new Rectangle(), font, "Reset seed", texPixel, textColor: Color.Red, textColorPressed: Color.DarkRed);
            btnReset.Click += () => {
                SettingsManager.CurrentSequentialSeed = 0;
                FileStorageManager.DeleteCurrentPanel();
                ScreenManager.Instance.GoBack();
                SoundManager.PlayOnce(Sound.EliminatorApply);
            };
            btnReset.UpdateButton += () => {
                btnReset.SetPositionAndSize(buttonsArea.Location + new Point(0, (int)(buttonHeight * btnHeightMod * 1)), new Point(buttonsArea.Width, (int)(buttonHeight * 1.5f)));
            };
            buttons.Add(btnReset);
                        
            TextButton btnBack = new TextButton(new Rectangle(), font, "Back", texPixel);
            btnBack.Click += () => {
                ScreenManager.Instance.GoBack();
                SoundManager.PlayOnce(Sound.MenuEscape);
            };
            btnBack.UpdateButton += () => {
                btnBack.SetPositionAndSize(new Point(buttonsArea.X, ScreenSize.Y - buttonHeight * 2), new Point(buttonsArea.Width, (int)(buttonHeight * 1.5f)));
            };
            buttons.Add(btnBack);

            UpdateButtonsSizeAndPosition();
        }

        private void UpdateButtonsSizeAndPosition()
        {
            foreach (AbstractButton btn in buttons)
                btn.FireUpdateButton();
        }

        public override void SetScreenSize(Point screenSize)
        {
            base.SetScreenSize(screenSize);
            InitializeScreenSizeDependent();
            UpdateButtonsSizeAndPosition();
        }

        public override void Update(GameTime gameTime)
        {
            foreach (var btn in buttons)
                btn.Update(InputManager.GetTapPosition());

            base.Update(gameTime);
        }

        public override void Deactivate()
        {
            base.Deactivate();
            SettingsManager.SaveSettings();
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

                string caption_1 = "Reset progress for";
                string caption_2 = "sequential mode?";

                Vector2 captionSize_1 = font.MeasureString(caption_1);
                Vector2 captionSize_2 = font.MeasureString(caption_2);

                int minScreenSize = Math.Min(ScreenSize.X, ScreenSize.Y);
                float fontScale = (minScreenSize * 0.6f) / captionSize_2.X;
                captionSize_1 = new Vector2(captionSize_1.X * fontScale, captionSize_1.Y * fontScale);
                captionSize_2 = new Vector2(captionSize_2.X * fontScale, captionSize_2.Y * fontScale);

                Vector2 captionPosition_1 = new Vector2(ScreenSize.X / 2 - captionSize_1.X / 2, ScreenSize.Y * 0.05f);
                Vector2 captionPosition_2 = new Vector2(ScreenSize.X / 2 - captionSize_2.X / 2, captionPosition_1.Y + captionSize_1.Y * 1.2f);

                spriteBatch.DrawString(font, caption_1, captionPosition_1, Color.White, 0, Vector2.Zero, fontScale, SpriteEffects.None, 0);
                spriteBatch.DrawString(font, caption_2, captionPosition_2, Color.White, 0, Vector2.Zero, fontScale, SpriteEffects.None, 0);

                int buttonsCount = 1;

                float buttonsAreaWidth = minScreenSize * 0.8f;
                buttonHeight = (int)(minScreenSize * 0.075f);
                float buttonsAreaHeight = ScreenSize.Y - (captionPosition_1.Y + captionSize_2.Y + buttonHeight * 2);
                float buttonsAreaTop = captionPosition_1.Y + captionSize_2.Y + (buttonsAreaHeight - buttonHeight * buttonsCount * btnHeightMod) / 2;
                buttonsArea = new Rectangle((int) (ScreenSize.X - buttonsAreaWidth) / 2, (int) buttonsAreaTop, (int) buttonsAreaWidth, (int) (ScreenSize.Y - buttonsAreaTop));
                                
                spriteBatch.End();
                GraphicsDevice.SetRenderTarget(null);   // Drop the render target
            }
        }
    }
}
