using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;

namespace TWP_Shared.GameScreens
{
    public class EnterSeedGameScreen : GameScreen
    {
        SpriteBatch internalBatch;
        SpriteFont fntRobotoMono = null;

        Texture2D[] texDigits = new Texture2D[10];
        Texture2D texDel, texOk, texClose, texPixel;

        TouchButton btnClose, btnOk, btnDel;
        TouchButton[] btnDigits;
        List<TouchButton> buttons = new List<TouchButton>();
        FadeTransition fade = new FadeTransition(15, 15, 10);

        private readonly Color DIM_COLOR = Color.Gray;
        private const int SEED_LENGTH = 9;
        int[] seed = new int[SEED_LENGTH];
        int cursorPos = 0;
        string seedString = new string('_', SEED_LENGTH);
        Vector2 seedStringSize = Vector2.Zero;
        void UpdateSeedString() => seedString = string.Join("", seed.Take(cursorPos)) + new string('_', SEED_LENGTH - cursorPos);
        void UpdateOkButton() => btnOk.ChangeTint(cursorPos == SEED_LENGTH ? Color.White : DIM_COLOR);

        public EnterSeedGameScreen(Point screenSize, GraphicsDevice device, Dictionary<string, Texture2D> textureProvider, Dictionary<string, SpriteFont> fontProvider, ContentManager Content) 
            : base(screenSize, device, textureProvider, fontProvider, Content)
        {
            internalBatch = new SpriteBatch(GraphicsDevice);
            fntRobotoMono = FontProvider["font/fnt_mono_digits"];

            texDel = TextureProvider["img/del"];
            texOk = TextureProvider["img/next"];
            texClose = TextureProvider["img/close"];
            texPixel = TextureProvider["img/pixel"];
            for (int i = 0; i < 10; i++)
                texDigits[i] = TextureProvider[$"img/digit{i}"];

            // since the font is monospace, rendered string will have fixed size, which we can calculate right now
            seedStringSize = fntRobotoMono.MeasureString(seedString);

            SpawnButtons();
            //UpdateButtonsColor();
        }

        private void SpawnButtons()
        {
            btnDigits = new TouchButton[10];
            for (int i = 0; i < 10; i++)
            {
                btnDigits[i] = new TouchButton(new Rectangle(), texDigits[i], null);
                int value = i;
                btnDigits[i].Click += () => {
                    if (cursorPos < SEED_LENGTH)
                    {
                        SoundManager.PlayOnce(Sound.ButtonNext);
                        seed[cursorPos] = value;
                        cursorPos++;
                        UpdateSeedString();
                        UpdateOkButton();
                    }
                };
                buttons.Add(btnDigits[i]);
            }

            btnClose = new TouchButton(new Rectangle(), texClose, null);
            btnClose.Click += () => {
                SoundManager.PlayOnce(Sound.MenuOpen);
                ScreenManager.Instance.GoBack();
            };
            buttons.Add(btnClose);

            btnOk = new TouchButton(new Rectangle(), texOk, null, DIM_COLOR);
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

            btnDel = new TouchButton(new Rectangle(), texDel, null);
            btnDel.Click += () => {
                SoundManager.PlayOnce(Sound.ButtonNext);
                if (cursorPos > 0)
                {
                    cursorPos--;
                    UpdateSeedString();
                    UpdateOkButton();
                }
            };
            buttons.Add(btnDel);

            UpdateButtonsPosition();
        }

        private void UpdateButtonsPosition()
        {
            int screenMin = Math.Min(ScreenSize.X, ScreenSize.Y);
            bool screenHorizontal = ScreenSize.X > ScreenSize.Y;

            // Close
            int minOffset = Math.Min((int)(screenMin * 0.05f), 50);
            int smallBtnSize = (int)(screenMin * 0.1f);
            btnClose.SetPositionAndSize(new Point(minOffset), new Point(smallBtnSize));

            int keypadMaxSize = screenHorizontal
                ? Math.Min(ScreenSize.Y, ScreenSize.X / 2)
                : Math.Min(ScreenSize.X, ScreenSize.Y / 2);
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
            // buttons[0] - buttons[9] are digits
            buttons[1].SetPositionAndSize(keypadTopLeft, new Point(buttonSize));
            buttons[2].SetPositionAndSize(keypadTopLeft + new Point(buttonSize + buttonMargin, 0), new Point(buttonSize));
            buttons[3].SetPositionAndSize(keypadTopLeft + new Point(2 * (buttonSize + buttonMargin), 0), new Point(buttonSize));
            buttons[4].SetPositionAndSize(keypadTopLeft + new Point(0, buttonSize + buttonMargin), new Point(buttonSize));
            buttons[5].SetPositionAndSize(keypadTopLeft + new Point(buttonSize + buttonMargin, buttonSize + buttonMargin), new Point(buttonSize));
            buttons[6].SetPositionAndSize(keypadTopLeft + new Point(2 * (buttonSize + buttonMargin), buttonSize + buttonMargin), new Point(buttonSize));
            buttons[7].SetPositionAndSize(keypadTopLeft + new Point(0, 2 * (buttonSize + buttonMargin)), new Point(buttonSize));
            buttons[8].SetPositionAndSize(keypadTopLeft + new Point(buttonSize + buttonMargin, 2 * (buttonSize + buttonMargin)), new Point(buttonSize));
            buttons[9].SetPositionAndSize(keypadTopLeft + new Point(2 * (buttonSize + buttonMargin), 2 * (buttonSize + buttonMargin)), new Point(buttonSize));
            buttons[0].SetPositionAndSize(keypadTopLeft + new Point(buttonSize + buttonMargin, 3 * (buttonSize + buttonMargin)), new Point(buttonSize));

            // Delete
            btnDel.SetPositionAndSize(keypadTopLeft + new Point(0, 3 * (buttonSize + buttonMargin)), new Point(buttonSize));
            // Ok
            btnOk.SetPositionAndSize(keypadTopLeft + new Point(2 * (buttonSize + buttonMargin), 3 * (buttonSize + buttonMargin)), new Point(buttonSize));
        }

        public override void SetScreenSize(Point screenSize)
        {
            base.SetScreenSize(screenSize);
            UpdateButtonsPosition();
        }

        public override void Update(GameTime gameTime)
        {
            int? digit = null;
            if (InputManager.IsKeyPressed(Keys.D0) || InputManager.IsKeyPressed(Keys.NumPad0))
                digit = 0;
            if (InputManager.IsKeyPressed(Keys.D1) || InputManager.IsKeyPressed(Keys.NumPad1))
                digit = 1;
            if (InputManager.IsKeyPressed(Keys.D2) || InputManager.IsKeyPressed(Keys.NumPad2))
                digit = 2;
            if (InputManager.IsKeyPressed(Keys.D3) || InputManager.IsKeyPressed(Keys.NumPad3))
                digit = 3;
            if (InputManager.IsKeyPressed(Keys.D4) || InputManager.IsKeyPressed(Keys.NumPad4))
                digit = 4;
            if (InputManager.IsKeyPressed(Keys.D5) || InputManager.IsKeyPressed(Keys.NumPad5))
                digit = 5;
            if (InputManager.IsKeyPressed(Keys.D6) || InputManager.IsKeyPressed(Keys.NumPad6))
                digit = 6;
            if (InputManager.IsKeyPressed(Keys.D7) || InputManager.IsKeyPressed(Keys.NumPad7))
                digit = 7;
            if (InputManager.IsKeyPressed(Keys.D8) || InputManager.IsKeyPressed(Keys.NumPad8))
                digit = 8;
            if (InputManager.IsKeyPressed(Keys.D9) || InputManager.IsKeyPressed(Keys.NumPad9))
                digit = 9;

            if (digit != null)
                btnDigits[digit.Value].Press();

            if (cursorPos == SEED_LENGTH && InputManager.IsKeyPressed(Keys.Enter))
                btnOk.Press();

            if (cursorPos > 0 && (InputManager.IsKeyPressed(Keys.Back) || InputManager.IsKeyPressed(Keys.Delete)))
                btnDel.Press();

            foreach (var button in buttons)
                button.Update(InputManager.GetTapPosition());

            fade.Update();

            base.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            bool screenHorizontal = ScreenSize.X > ScreenSize.Y;
            int availableWidth = screenHorizontal
                ? ScreenSize.X / 2
                : ScreenSize.X;
            int padding = ScreenSize.X / 10; // leave 10% empty space to the sides of the seed
            availableWidth -= padding * 2;

            float seedScale = availableWidth / seedStringSize.X;
            float posY = screenHorizontal
                ? ScreenSize.Y / 2 - seedStringSize.Y * seedScale / 2
                : ScreenSize.Y / 4 - seedStringSize.Y * seedScale / 2;

            GraphicsDevice.Clear(Color.Black);
            spriteBatch.DrawString(fntRobotoMono, seedString, new Vector2(padding, posY), Color.White, 0, Vector2.Zero, seedScale, SpriteEffects.None, 0);

            // Draw buttons
            foreach (var button in buttons)
                button.Draw(spriteBatch);

            if (fade.IsActive)
                spriteBatch.Draw(texPixel, new Rectangle(Point.Zero, ScreenSize), Color.Black * fade.Opacity);

            base.Draw(spriteBatch);
        }
    }
}
