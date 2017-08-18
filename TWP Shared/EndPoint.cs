using Microsoft.Xna.Framework;
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
    }
}
