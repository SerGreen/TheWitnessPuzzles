using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace TWP_Shared
{
    enum Facing { Left, Right, Up, Down }

    struct EndPoint
    {
        public Rectangle Rectangle { get; set; }
        public Facing Facing { get; set; }

        public EndPoint(Rectangle bounds, Facing facing)
        {
            Rectangle = bounds;
            Facing = facing;
        }

        public float IntercetionPercent(Rectangle otherRect)
        {
            // Intersection rectangle
            int x_overlap = Math.Max(0, Math.Min(Rectangle.Right, otherRect.Right) - Math.Max(Rectangle.Left, otherRect.Left));
            int y_overlap = Math.Max(0, Math.Min(Rectangle.Bottom, otherRect.Bottom) - Math.Max(Rectangle.Top, otherRect.Top));
            int overlapArea = x_overlap * y_overlap;
            // Union area of two rectangles
            int unionArea = Rectangle.Width * Rectangle.Height + otherRect.Width * otherRect.Height - overlapArea;
            // Percent of intersection
            return (float) overlapArea / unionArea;
        }

        public void Draw(SpriteBatch sb, Color wallColor, Texture2D[] textures)
        {
            switch (Facing)
            {
                case Facing.Left:   sb.Draw(textures[0], Rectangle, wallColor); break;
                case Facing.Right:  sb.Draw(textures[0], Rectangle, null, wallColor, 0, Vector2.Zero, SpriteEffects.FlipHorizontally, 0); break;
                case Facing.Up:     sb.Draw(textures[1], Rectangle, wallColor); break;
                case Facing.Down:   sb.Draw(textures[1], Rectangle, null, wallColor, 0, Vector2.Zero, SpriteEffects.FlipVertically, 0); break;
            }
        }
    }
}
