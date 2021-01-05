using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace TWP_Shared.GameScreens
{
    public class EnterSeedGameScreen : GameScreen
    {
        SpriteBatch internalBatch;
        SpriteFont fntConstantia = null;

        Texture2D[] texDigits = new Texture2D[10];
        Texture2D texDel, texOk, texClose, texPixel;

        List<TouchButton> buttons = new List<TouchButton>();
        FadeTransition fade = new FadeTransition(15, 15, 10);

        private const int SEED_LENGTH = 9;
        int[] seed = new int[SEED_LENGTH];
        int cursorPos = 0;

        public EnterSeedGameScreen(Point screenSize, GraphicsDevice device, Dictionary<string, Texture2D> textureProvider, Dictionary<string, SpriteFont> fontProvider, ContentManager Content) 
            : base(screenSize, device, textureProvider, fontProvider, Content)
        {
            internalBatch = new SpriteBatch(GraphicsDevice);
            texDel = TextureProvider["img/close"]; // TODO: add del texture
            texOk = TextureProvider["img/next"];
            texClose = TextureProvider["img/close"];
            texPixel = TextureProvider["img/pixel"];
            for (int i = 0; i < 10; i++)
                //texDigits[i] = TextureProvider[$"img/{i}"];
                texDigits[i] = TextureProvider[$"img/close"];

            SpawnButtons();
            //UpdateButtonsColor();
        }

        private void SpawnButtons()
        {
            TouchButton btnClose = new TouchButton(new Rectangle(), texClose, null);
            btnClose.Click += () => {
                SoundManager.PlayOnce(Sound.MenuOpen);
                ScreenManager.Instance.GoBack();
            };
            buttons.Add(btnClose);

            TouchButton btnOk = new TouchButton(new Rectangle(), texOk, null);
            btnOk.Click += () => {
                if (cursorPos == SEED_LENGTH)
                {
                    SoundManager.PlayOnce(Sound.ButtonNextSuccess);
                    //ScreenManager.Instance.GoBack();
                    // TODO: create new panel screen
                    // TODO: add deactivated texture
                }
            };
            buttons.Add(btnOk);

            TouchButton btnDel = new TouchButton(new Rectangle(), texDel, null);
            btnDel.Click += () => {
                SoundManager.PlayOnce(Sound.ButtonNext);
                if (cursorPos > 0)
                    cursorPos--;
            };
            buttons.Add(btnDel);

            for (int i = 0; i < 10; i++)
            {
                TouchButton btnDigit = new TouchButton(new Rectangle(), texDigits[i], null);
                btnDigit.Click += () => {
                    if (cursorPos < SEED_LENGTH)
                    {
                        SoundManager.PlayOnce(Sound.ButtonNext);
                        seed[cursorPos] = i;
                        cursorPos++;
                    }
                };
                buttons.Add(btnDel);
            }

            UpdateButtonsPosition();
        }

        private void UpdateButtonsPosition()
        {
            int screenMin = Math.Min(ScreenSize.X, ScreenSize.Y);
            bool screenHorizontal = ScreenSize.X > ScreenSize.Y;

            // Close
            int minOffset = Math.Min((int)(screenMin * 0.05f), 50);
            int smallBtnSize = (int)(screenMin * 0.1f);
            buttons[0].SetPositionAndSize(new Point(minOffset), new Point(smallBtnSize));
        }

        public override void Update(GameTime gameTime)
        {
            foreach (var button in buttons)
                button.Update(InputManager.GetTapPosition());

            fade.Update();

            base.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            // Draw buttons
            foreach (var button in buttons)
                button.Draw(spriteBatch);

            if (fade.IsActive)
                spriteBatch.Draw(texPixel, new Rectangle(Point.Zero, ScreenSize), Color.Black * fade.Opacity);

            base.Draw(spriteBatch);
        }
    }
}
