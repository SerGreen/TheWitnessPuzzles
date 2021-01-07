using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
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
        string seedString = new string('_', SEED_LENGTH);
        void UpdateSeedString() => seedString = string.Join("", seed.Take(cursorPos)) + new string('_', SEED_LENGTH - cursorPos);

        public EnterSeedGameScreen(Point screenSize, GraphicsDevice device, Dictionary<string, Texture2D> textureProvider, Dictionary<string, SpriteFont> fontProvider, ContentManager Content) 
            : base(screenSize, device, textureProvider, fontProvider, Content)
        {
            internalBatch = new SpriteBatch(GraphicsDevice);
            fntConstantia = FontProvider["font/fnt_constantia_big"];
            texDel = TextureProvider["img/close"]; // TODO: add del texture
            texOk = TextureProvider["img/next"];
            texClose = TextureProvider["img/close"];
            texPixel = TextureProvider["img/pixel"];
            for (int i = 0; i < 10; i++)
                //texDigits[i] = TextureProvider[$"img/{i}"];
                texDigits[i] = TextureProvider["img/close"];

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
                {
                    cursorPos--;
                    UpdateSeedString();
                }
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
                        UpdateSeedString();
                    }
                };
                buttons.Add(btnDigit);
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

            int keypadMaxSize = screenHorizontal
                ? Math.Min(ScreenSize.Y, ScreenSize.X / 2)
                : ScreenSize.Y / 2;
            keypadMaxSize = (int)(keypadMaxSize * 0.9f); // leave 10% free space to the edge of the window

            // keypad is 3 x 4 buttons matrix, so largest side is 4 buttons and 3 margins
            float btnMarginCoef = 0.25f; // distance between two buttons is 25% of buttonSize
            int buttonSize = (int)Math.Ceiling(keypadMaxSize / (4 + btnMarginCoef * 3));
            int buttonMargin = (int)(btnMarginCoef * buttonSize);
            // keypad width is (1 button + 1 margin) smaller than its height, hence this offset to center it
            int offsetX = (buttonSize + buttonMargin) / 2;

            Point keypadTopLeft = screenHorizontal
                ? new Point((ScreenSize.X + ScreenSize.X / 2 - keypadMaxSize) / 2 + offsetX, (ScreenSize.Y - keypadMaxSize) / 2)
                : new Point((ScreenSize.X - keypadMaxSize) / 2 + offsetX, ScreenSize.Y / 2);

            // position buttons
            // gonna keep it dirty and manually index every button
            // buttons[3] - buttons[12] are digits
            buttons[4].SetPositionAndSize(keypadTopLeft, new Point(buttonSize));                                                                                // 1
            buttons[5].SetPositionAndSize(keypadTopLeft + new Point(buttonSize + buttonMargin, 0), new Point(buttonSize));                                      // 2
            buttons[6].SetPositionAndSize(keypadTopLeft + new Point(2 * (buttonSize + buttonMargin), 0), new Point(buttonSize));                                // 3
            buttons[7].SetPositionAndSize(keypadTopLeft + new Point(0, buttonSize + buttonMargin), new Point(buttonSize));                                      // 4
            buttons[8].SetPositionAndSize(keypadTopLeft + new Point(buttonSize + buttonMargin, buttonSize + buttonMargin), new Point(buttonSize));              // 5
            buttons[9].SetPositionAndSize(keypadTopLeft + new Point(2 * (buttonSize + buttonMargin), buttonSize + buttonMargin), new Point(buttonSize));        // 6
            buttons[10].SetPositionAndSize(keypadTopLeft + new Point(0, 2 * (buttonSize + buttonMargin)), new Point(buttonSize));                               // 7
            buttons[11].SetPositionAndSize(keypadTopLeft + new Point(buttonSize + buttonMargin, 2 * (buttonSize + buttonMargin)), new Point(buttonSize));       // 8
            buttons[12].SetPositionAndSize(keypadTopLeft + new Point(2 * (buttonSize + buttonMargin), 2 * (buttonSize + buttonMargin)), new Point(buttonSize)); // 9
            buttons[3].SetPositionAndSize(keypadTopLeft + new Point(buttonSize + buttonMargin, 3 * (buttonSize + buttonMargin)), new Point(buttonSize));        // 0

            // Delete
            buttons[2].SetPositionAndSize(keypadTopLeft + new Point(0, 3 * (buttonSize + buttonMargin)), new Point(buttonSize));
            // Ok
            buttons[1].SetPositionAndSize(keypadTopLeft + new Point(2 * (buttonSize + buttonMargin), 3 * (buttonSize + buttonMargin)), new Point(buttonSize));
        }

        public override void SetScreenSize(Point screenSize)
        {
            base.SetScreenSize(screenSize);
            UpdateButtonsPosition();
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
            GraphicsDevice.Clear(Color.Black);
            spriteBatch.DrawString(fntConstantia, seedString, new Vector2(100, 50), Color.White);

            // Draw buttons
            foreach (var button in buttons)
                button.Draw(spriteBatch);

            if (fade.IsActive)
                spriteBatch.Draw(texPixel, new Rectangle(Point.Zero, ScreenSize), Color.Black * fade.Opacity);

            base.Draw(spriteBatch);
        }
    }
}
