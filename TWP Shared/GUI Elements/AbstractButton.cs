using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace TWP_Shared
{
    public abstract class AbstractButton
    {
        public event Action UpdateButton;
        public void FireUpdateButton () => UpdateButton();
        public abstract void Draw (SpriteBatch sb, float alpha = 1f);
        public abstract void Update (Point? touchPoint = null);
    }
}
