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
        private bool isActivatedDefault;

        public ToggleButton(Rectangle bounds, Texture2D textureOn, Texture2D textureOff = null, Color? tint = null, bool isActivated = true) 
            : base(bounds, textureOn, textureOff, tint)
        {
            IsActivated = isActivatedDefault = isActivated;
            Click += () => IsActivated = !IsActivated;
        }

        public void Reset() => IsActivated = isActivatedDefault;
        public void Activate() => IsActivated = true;

        public override void Draw(SpriteBatch sb, float alpha = 1)
        {
            if (!IsActivated && textureDown != null)
                sb.Draw(textureDown, hitbox, tintUp * alpha);
            else if(textureUp != null)
                sb.Draw(textureUp, hitbox, tintUp * alpha);
        }
    }
}
