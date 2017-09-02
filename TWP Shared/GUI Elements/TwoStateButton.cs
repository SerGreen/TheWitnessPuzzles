using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TWP_Shared
{
    public class TwoStateButton : TouchButton
    {
        public bool StateActive { get; set; }
        private Texture2D textureActiveUp, textureActiveDown;

        public TwoStateButton(Rectangle bounds, Texture2D textureUp, Texture2D textureActiveUp, Texture2D textureDown = null, Texture2D textureActiveDown = null, Color? tint = null, bool initialState = false) 
            : base(bounds, textureUp, textureDown, tint)
        {
            this.textureActiveUp = textureActiveUp;
            this.textureActiveDown = textureActiveDown;
            StateActive = initialState;
        }

        public override void Draw(SpriteBatch sb, float alpha = 1)
        {
            if (!StateActive)
                base.Draw(sb, alpha);
            else
            {
                if (textureActiveDown != null)
                    sb.Draw(isPressedDown ? textureActiveDown : textureActiveUp, hitbox, (isPressedDown ? tintDown : tintUp) * alpha);
                else
                    sb.Draw(textureActiveUp, hitbox, (isPressedDown ? tintDown : tintUp) * alpha);
            }
        }
    }
}
