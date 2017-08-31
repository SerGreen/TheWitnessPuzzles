using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace TWP_Shared
{
    public class TouchButton
    {
        protected Rectangle hitbox;
        protected Texture2D textureUp, textureDown;
        protected bool isPressedDown;
        protected readonly int timeDownMax = 15;
        protected int timeDown;

        public event Action Click;

        public TouchButton(Rectangle bounds, Texture2D textureUp, Texture2D textureDown = null)
        {
            hitbox = bounds;
            this.textureUp = textureUp;
            this.textureDown = textureDown;
            timeDown = 0;
            isPressedDown = false;
        }

        public virtual void SetPositionAndSize(Point pos, Point size) => hitbox = new Rectangle(pos, size);

        public void Update(Point? touchPoint = null)
        {
            if (touchPoint != null && hitbox.Contains(touchPoint.Value))
            {
                isPressedDown = true;
                timeDown = timeDownMax;
                Click?.Invoke();
            }

            if(isPressedDown && timeDown > 0)
            {
                timeDown--;
                if (timeDown == 0)
                    isPressedDown = false;
            }
        }

        public virtual void Draw(SpriteBatch sb, float alpha = 1f)
        {
            if (textureDown != null)
                sb.Draw(isPressedDown ? textureDown : textureUp, hitbox, Color.White * alpha);
            else
                sb.Draw(textureUp, hitbox, isPressedDown ? Color.LightGray * alpha : Color.White * alpha);
        }
    }
}
