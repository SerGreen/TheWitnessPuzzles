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
        const float btnHeightMod = 1.2f;

        SpriteFont font = null;
        Texture2D texPixel;
        Texture2D[] texCheckbox = new Texture2D[2];
        Texture2D[] texSound = new Texture2D[2];
        Texture2D[] texOrientation = new Texture2D[2];
        Texture2D[] texLeftRight = new Texture2D[2];

        Rectangle buttonsArea;
        int buttonHeight;
        List<AbstractButton> buttons = new List<AbstractButton>();
        SliderUpDown sensitivitySlider, volumeSlider;

        RenderTarget2D backgroundTexture;

        public SettingsGameScreen(Point screenSize, GraphicsDevice device, Dictionary<string, Texture2D> textureProvider, Dictionary<string, SpriteFont> fontProvider, ContentManager Content, params object[] data) 
            : base(screenSize, device, textureProvider, fontProvider, Content, data)
        {
            font = FontProvider["font/fnt_constantia_big"];
            texPixel = textureProvider["img/pixel"];
            texCheckbox[0] = textureProvider["img/checkbox0"];
            texCheckbox[1] = textureProvider["img/checkbox1"];
            texSound[0] = textureProvider["img/sound0"];
            texSound[1] = textureProvider["img/sound1"];
            texOrientation[0] = textureProvider["img/orientation_lock0"];
            texOrientation[1] = textureProvider["img/orientation_lock1"];
            texLeftRight[0] = textureProvider["img/left"];
            texLeftRight[1] = textureProvider["img/right"];

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
            ToggleButton btnMute = new ToggleButton(new Rectangle(), texSound[1], texSound[0], null, !SettingsManager.IsMute);
            btnMute.Click += () => {
                SettingsManager.IsMute = !btnMute.IsActivated;
                SoundManager.PlayOnce(SettingsManager.IsMute ? Sound.MenuUntick : Sound.MenuTick);
            };
            btnMute.UpdateButton += () => {
                btnMute.SetPositionAndSize(buttonsArea.Location + new Point(buttonsArea.Width - buttonHeight, 0), new Point(buttonHeight, buttonHeight));
            };
            buttons.Add(btnMute);

            volumeSlider = new SliderUpDown(new Rectangle(), 0f, 1.0f, 0.05f, SettingsManager.MasterVolume, texPixel, texLeftRight[0], texLeftRight[1], new Color(50, 50, 50), Color.White);
            volumeSlider.ValueChanged += () => {
                SettingsManager.MasterVolume = volumeSlider.Value;
            };
            volumeSlider.UpdateButton += () => {
                volumeSlider.SetPositionAndSize(buttonsArea.Location + new Point(buttonsArea.Width - buttonHeight * 4, (int)(buttonHeight * btnHeightMod)), new Point(buttonHeight * 4, buttonHeight));
            };
            buttons.Add(volumeSlider);

            sensitivitySlider = new SliderUpDown(new Rectangle(), 0.1f, 2.0f, 0.1f, SettingsManager.Sensitivity, texPixel, texLeftRight[0], texLeftRight[1], new Color(50, 50, 50), Color.White);
            sensitivitySlider.ValueChanged += () => {
                SettingsManager.Sensitivity = sensitivitySlider.Value;
            };
            sensitivitySlider.UpdateButton += () => {
                sensitivitySlider.SetPositionAndSize(buttonsArea.Location + new Point(buttonsArea.Width - buttonHeight * 4, (int)(buttonHeight * btnHeightMod * 2)), new Point(buttonHeight * 4, buttonHeight));
            };
            buttons.Add(sensitivitySlider);

            ToggleButton btnFullscreen = new ToggleButton(new Rectangle(), texCheckbox[1], texCheckbox[0], null, SettingsManager.IsFullscreen);
            btnFullscreen.Click += () => {
                SettingsManager.IsFullscreen = btnFullscreen.IsActivated;
                ScreenManager.Instance.UpdateFullscreen();
                SoundManager.PlayOnce(SettingsManager.IsFullscreen ? Sound.MenuTick : Sound.MenuUntick);
            };
            btnFullscreen.UpdateButton += () => {
                btnFullscreen.SetPositionAndSize(buttonsArea.Location + new Point(buttonsArea.Width - buttonHeight, (int)(buttonHeight * btnHeightMod * 3)), new Point(buttonHeight, buttonHeight));
            };
            buttons.Add(btnFullscreen);

            ToggleButton btnBloomFX = new ToggleButton(new Rectangle(), texCheckbox[1], texCheckbox[0], null, SettingsManager.BloomFX);
            btnBloomFX.Click += () => {
                SettingsManager.BloomFX = btnBloomFX.IsActivated;
                SoundManager.PlayOnce(SettingsManager.BloomFX ? Sound.MenuTick : Sound.MenuUntick);
            };
            btnBloomFX.UpdateButton += () => {
                btnBloomFX.SetPositionAndSize(buttonsArea.Location + new Point(buttonsArea.Width - buttonHeight, (int)(buttonHeight * btnHeightMod * 4)), new Point(buttonHeight, buttonHeight));
            };
            buttons.Add(btnBloomFX);

#if ANDROID
            ToggleButton btnOrientationLock = new ToggleButton(new Rectangle(), texOrientation[1], texOrientation[0], null, SettingsManager.IsOrientationLocked);
            btnOrientationLock.Click += () => {
                SettingsManager.IsOrientationLocked = btnOrientationLock.IsActivated;
                SoundManager.PlayOnce(SettingsManager.IsOrientationLocked ? Sound.MenuTick : Sound.MenuUntick);
            };
            btnOrientationLock.UpdateButton += () => {
                btnOrientationLock.SetPositionAndSize(buttonsArea.Location + new Point(buttonsArea.Width - buttonHeight, (int) (buttonHeight * btnHeightMod * 5)), new Point(buttonHeight, buttonHeight));
            };
            buttons.Add(btnOrientationLock);
#endif

            TextButton btnGeneratorSettings = new TextButton(new Rectangle(), font, "Generator settings", texPixel, buttonAlignment: TextButton.ButtonAlignment.Left);
            btnGeneratorSettings.Click += () => {
                // TODO: Open new screen with generator settings
                SettingsManager.isSequentialMode = !SettingsManager.isSequentialMode;
                SoundManager.PlayOnce(Sound.MenuEnter);
            };
            btnGeneratorSettings.UpdateButton += () => {
                btnGeneratorSettings.SetPositionAndSize(buttonsArea.Location + new Point(0, (int)(buttonHeight * btnHeightMod * 
#if ANDROID
                    6 
#else 
                    5 
#endif
                    )), new Point(buttonsArea.Width, buttonHeight));
            };
            buttons.Add(btnGeneratorSettings);

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

                string caption = "Settings";

                Vector2 captionSize = font.MeasureString(caption);

                int minScreenSize = Math.Min(ScreenSize.X, ScreenSize.Y);
                float fontScale = (minScreenSize * 0.6f) / captionSize.X;
                captionSize = new Vector2(captionSize.X * fontScale, captionSize.Y * fontScale);

                Vector2 captionPosition = new Vector2(ScreenSize.X / 2 - captionSize.X / 2, ScreenSize.Y * 0.05f);

                spriteBatch.DrawString(font, caption, captionPosition, Color.White, 0, Vector2.Zero, fontScale, SpriteEffects.None, 0);

                int buttonsCount = 6;
#if ANDROID
                // Android has one extra button
                buttonsCount++;
#endif
                float buttonsAreaWidth = minScreenSize * 0.8f;
                buttonHeight = (int) (captionSize.Y * 0.4f);
                float buttonsAreaHeight = ScreenSize.Y - (captionPosition.Y + captionSize.Y + buttonHeight * 2);
                float buttonsAreaTop = captionPosition.Y + captionSize.Y + (buttonsAreaHeight - buttonHeight * buttonsCount * btnHeightMod) / 2;
                buttonsArea = new Rectangle((int) (ScreenSize.X - buttonsAreaWidth) / 2, (int) buttonsAreaTop, (int) buttonsAreaWidth, (int) (ScreenSize.Y - buttonsAreaTop));

                Vector2 textSize = font.MeasureString("V");
                float scale = buttonHeight / textSize.Y;
                spriteBatch.DrawString(font, "Sound", new Vector2(buttonsArea.X, buttonsArea.Y), Color.White, 0, Vector2.Zero, scale, SpriteEffects.None, 0);
                spriteBatch.DrawString(font, "Volume", new Vector2(buttonsArea.X, buttonsArea.Y + buttonHeight * btnHeightMod), Color.White, 0, Vector2.Zero, scale, SpriteEffects.None, 0);
                spriteBatch.DrawString(font, "Sensitivity", new Vector2(buttonsArea.X, buttonsArea.Y + buttonHeight * btnHeightMod * 2), Color.White, 0, Vector2.Zero, scale, SpriteEffects.None, 0);
                spriteBatch.DrawString(font, "Fullscreen", new Vector2(buttonsArea.X, buttonsArea.Y + buttonHeight * btnHeightMod * 3), Color.White, 0, Vector2.Zero, scale, SpriteEffects.None, 0);
                spriteBatch.DrawString(font, "Bloom FX", new Vector2(buttonsArea.X, buttonsArea.Y + buttonHeight * btnHeightMod * 4), Color.White, 0, Vector2.Zero, scale, SpriteEffects.None, 0);
#if ANDROID
                spriteBatch.DrawString(font, "Lock orientation", new Vector2(buttonsArea.X, buttonsArea.Y + buttonHeight * btnHeightMod * 5), Color.White, 0, Vector2.Zero, scale, SpriteEffects.None, 0);
#endif

                spriteBatch.End();
                GraphicsDevice.SetRenderTarget(null);   // Drop the render target
            }
        }
    }
}
