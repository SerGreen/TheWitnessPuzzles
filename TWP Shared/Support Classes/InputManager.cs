using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TWP_Shared
{
    public static class InputManager
    {
        private static Game Game;

        private static int moveStep = 5;    // used for keyboard movement
        private static bool mouseLocked = false; // if mouse is locked in center of window

        private static List<GestureSample> gestures;
        private static KeyboardState prevKB;
        private static MouseState prevMouse;

        public static bool IsFocused { get; set; }

        public static void Initialize(Game game)
        {
            Game = game;
            IsFocused = game.IsActive;
            Update();
        }

        public static void Update()
        {
            gestures = GetGestures().ToList();
            prevMouse = Mouse.GetState();
            prevKB = Keyboard.GetState();

            if (mouseLocked && IsFocused)
                ResetMouseToCenter();
        }
        private static IEnumerable<GestureSample> GetGestures()
        {
            if (IsFocused)
                while (TouchPanel.IsGestureAvailable)
                    yield return TouchPanel.ReadGesture();
        }

        public static Point? GetTapPosition()
        {
            if (IsFocused)
            {
                // Touch screen first
                foreach (var gesture in gestures)
                    if (gesture.GestureType == GestureType.Tap)
                        return gesture.Position.ToPoint();

                // Then mouse
                if (prevMouse.LeftButton == ButtonState.Released && Mouse.GetState().LeftButton == ButtonState.Pressed && Game.IsActive)
                {
                    Point mousePoint = Mouse.GetState().Position;
                    if (Game.GraphicsDevice.Viewport.Bounds.Contains(mousePoint))
                        return mousePoint;
                }
            }

            return null;
        }

        public static Vector2 GetDragVector()
        {
            Vector2 result = Vector2.Zero;

            if (IsFocused)
            {
                foreach (var gesture in gestures.Where(x => x.GestureType == GestureType.FreeDrag))
                    result += gesture.Delta;

                if (Keyboard.GetState().IsKeyDown(Keys.Right)) result.X += moveStep;
                if (Keyboard.GetState().IsKeyDown(Keys.Left)) result.X -= moveStep;
                if (Keyboard.GetState().IsKeyDown(Keys.Down)) result.Y += moveStep;
                if (Keyboard.GetState().IsKeyDown(Keys.Up)) result.Y -= moveStep;

                if (mouseLocked)
                {
                    Point center = Game.Window.ClientBounds.Center - Game.Window.ClientBounds.Location;
                    Point mousePos = Mouse.GetState().Position;
                    result += (mousePos - center).ToVector2();
                }
            }

            return result * SettingsManager.Sensitivity;
        }

        public static void LockMouse()
        {
            mouseLocked = true;
            Game.IsMouseVisible = false;
            ResetMouseToCenter();
        }
        public static void UnlockMouse()
        {
            mouseLocked = false;
            Game.IsMouseVisible = true;
        }
        private static void ResetMouseToCenter()
        {
            Point center = Game.Window.ClientBounds.Center - Game.Window.ClientBounds.Location;
            Mouse.SetPosition(center.X, center.Y);
        }

        /// <summary>
        /// Returns True if key has just been pressed down. If not pressed or being held => False
        /// </summary>
        public static bool IsKeyPressed(Keys key) => IsFocused ? prevKB.IsKeyUp(key) && Keyboard.GetState().IsKeyDown(key) : false;
    }
}
