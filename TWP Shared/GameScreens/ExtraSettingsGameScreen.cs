using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TWP_Shared
{
    public class ExtraSettingsGameScreen : GameScreen
    {
        const float btnHeightMod = 1.2f;

        SpriteFont font = null;
        Texture2D texPixel;
        Texture2D[] texCheckbox = new Texture2D[2];

        Rectangle buttonsArea;
        int buttonHeight;
        List<AbstractButton> buttons = new List<AbstractButton>();

        RenderTarget2D backgroundTexture;

        public ExtraSettingsGameScreen(Point screenSize, GraphicsDevice device, Dictionary<string, Texture2D> textureProvider, Dictionary<string, SpriteFont> fontProvider, ContentManager Content, params object[] data) 
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
            ToggleButton btnSequential = new ToggleButton(new Rectangle(), texCheckbox[1], texCheckbox[0], null, SettingsManager.IsSequentialMode);
            btnSequential.Click += () => {
                SettingsManager.IsSequentialMode = btnSequential.IsActivated;
                if (SettingsManager.IsSequentialMode)
                    FileStorageManager.BackupCurrentPanel();
                else
                    FileStorageManager.RestoreCurrentPanel();
                SoundManager.PlayOnce(SettingsManager.IsSequentialMode ? Sound.MenuTick : Sound.MenuUntick);
            };
            btnSequential.UpdateButton += () => {
                btnSequential.SetPositionAndSize(buttonsArea.Location + new Point(buttonsArea.Width - buttonHeight, 0), new Point(buttonHeight, buttonHeight));
            };
            buttons.Add(btnSequential);

            TextButton btnResetSequential = new TextButton(new Rectangle(), font, "Reset seed", texPixel, buttonAlignment: TextButton.ButtonAlignment.Left);
            btnResetSequential.Click += () => {
                ScreenManager.Instance.AddScreen<ConfirmSeedResetGameScreen>(false, true);
                SoundManager.PlayOnce(Sound.PotentialFailure);
            };
            btnResetSequential.UpdateButton += () => {
                btnResetSequential.SetPositionAndSize(buttonsArea.Location + new Point(0, (int)(buttonHeight * btnHeightMod * 1)), new Point(buttonsArea.Width, buttonHeight));
            };
            buttons.Add(btnResetSequential);
                        
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

                string caption = "Extra settings";

                Vector2 captionSize = font.MeasureString(caption);

                int minScreenSize = Math.Min(ScreenSize.X, ScreenSize.Y);
                float fontScale = (minScreenSize * 0.6f) / captionSize.X;
                captionSize = new Vector2(captionSize.X * fontScale, captionSize.Y * fontScale);

                Vector2 captionPosition = new Vector2(ScreenSize.X / 2 - captionSize.X / 2, ScreenSize.Y * 0.05f);

                spriteBatch.DrawString(font, caption, captionPosition, Color.White, 0, Vector2.Zero, fontScale, SpriteEffects.None, 0);

                int buttonsCount = 2;

                float buttonsAreaWidth = minScreenSize * 0.8f;
                buttonHeight = (int)(minScreenSize * 0.075f);
                float buttonsAreaHeight = ScreenSize.Y - (captionPosition.Y + captionSize.Y + buttonHeight * 2);
                float buttonsAreaTop = captionPosition.Y + captionSize.Y + (buttonsAreaHeight - buttonHeight * buttonsCount * btnHeightMod) / 2;
                buttonsArea = new Rectangle((int) (ScreenSize.X - buttonsAreaWidth) / 2, (int) buttonsAreaTop, (int) buttonsAreaWidth, (int) (ScreenSize.Y - buttonsAreaTop));

                Vector2 textSize = font.MeasureString("V");
                float scale = buttonHeight / textSize.Y;
                spriteBatch.DrawString(font, "Sequential mode", new Vector2(buttonsArea.X, buttonsArea.Y  + buttonHeight * btnHeightMod * 0), Color.White, 0, Vector2.Zero, scale, SpriteEffects.None, 0);
                
                spriteBatch.End();
                GraphicsDevice.SetRenderTarget(null);   // Drop the render target
            }
        }
    }
}
