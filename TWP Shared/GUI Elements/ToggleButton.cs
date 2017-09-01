using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TWP_Shared
{
    public class ToggleButton : TouchButton
    {
        public bool IsActivated { get; private set; }

        public ToggleButton(Rectangle bounds, Texture2D textureOn, Texture2D textureOff = null, bool isActivated = true) 
            : base(bounds, textureOn, textureOff)
        {
            IsActivated = isActivated;
            Click += () => IsActivated = !IsActivated;
        }

        public override void Draw(SpriteBatch sb, float alpha = 1)
        {
            if (!IsActivated && textureDown != null)
                sb.Draw(textureDown, hitbox, Color.White * alpha);
            else if(textureUp != null)
                sb.Draw(textureUp, hitbox, Color.White * alpha);
        }
    }
}
