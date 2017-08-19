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
        KeyboardState prevKB;
        MouseState prevMouse;

        public TWPGameDesktop(Puzzle panel = null) : base(false, panel)
        {
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            prevMouse = Mouse.GetState();
            prevKB = Keyboard.GetState();
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
            if (prevMouse.LeftButton == ButtonState.Released && Mouse.GetState().LeftButton == ButtonState.Pressed && this.IsActive)
            {
                Point mousePoint = Mouse.GetState().Position;
                if (GraphicsDevice.Viewport.Bounds.Contains(mousePoint))
                    return mousePoint;
            }

            if (!prevKB.IsKeyDown(Keys.N) && Keyboard.GetState().IsKeyDown(Keys.N))
                drawDebug = !drawDebug;

            return null;
        }
    }
}
