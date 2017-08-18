using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using TheWitnessPuzzles;
using TWP_Shared;
using Microsoft.Xna.Framework.Input;

namespace TWP_Desktop
{
    public class TWPGameDesktop : TWPGame
    {
        ButtonState prevLMB = ButtonState.Released;

        public TWPGameDesktop(Puzzle panel = null) : base(false, panel)
        {
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            prevLMB = Mouse.GetState().LeftButton;
        }

        protected override Vector2 GetMoveVector()
        {
            Vector2 result = Vector2.Zero;

            if (Keyboard.GetState().IsKeyDown(Keys.Right))
                result.X += moveStep;

            if (Keyboard.GetState().IsKeyDown(Keys.Left))
                result.X -= moveStep;

            if (Keyboard.GetState().IsKeyDown(Keys.Down))
                result.Y += moveStep;

            if (Keyboard.GetState().IsKeyDown(Keys.Up))
                result.Y -= moveStep;

            return result;
        }

        protected override Point? GetTapPosition()
        {
            if (prevLMB == ButtonState.Released && Mouse.GetState().LeftButton == ButtonState.Pressed && this.IsActive)
            {
                Point mousePoint = Mouse.GetState().Position;
                if (GraphicsDevice.Viewport.Bounds.Contains(mousePoint))
                    return mousePoint;
            }

            return null;
        }
    }
}
