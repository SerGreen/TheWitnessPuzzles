using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TWP_Shared
{
    public class TabButton : TouchButton
    {
        public bool IsActive { get; private set; }
        Color bgColorUp, bgColorDown;
        Point iconPosition, iconSize;
        List<TabButton> otherTabs;
        Texture2D texPixel;

        public TabButton(Rectangle bounds, Texture2D texPixel, Texture2D textureUp, Texture2D textureDown = null, Color? tint = null, Color? backgroundColor = null, List<TabButton> otherTabs = null) 
            : base(bounds, textureUp, textureDown, tint)
        {
            this.texPixel = texPixel;
            if (backgroundColor != null)
            {
                bgColorUp = backgroundColor.Value;
                bgColorDown = Color.Lerp(bgColorUp, Color.Gray, 0.5f);
            }
            else
            {
                bgColorUp = Color.Transparent;
                bgColorDown = Color.Gray * 0.5f;
            }

            this.otherTabs = otherTabs ?? new List<TabButton>();
            Click += () => Activate();

            UpdateIconPositionAndSize();
        }

        public override void SetPositionAndSize(Point pos, Point size)
        {
            base.SetPositionAndSize(pos, size);
            UpdateIconPositionAndSize();
        }
        private void UpdateIconPositionAndSize()
        {
            int minSize = Math.Min(hitbox.Width, hitbox.Height);
            iconSize = new Point((int) (minSize * 0.8f));
            iconPosition = new Point(hitbox.X + (hitbox.Width - iconSize.X) / 2, hitbox.Y + (hitbox.Height - iconSize.Y) / 2);
        }

        public void ConnectTab(TabButton tab)
        {
            if (!otherTabs.Contains(tab))
            {
                otherTabs.Add(tab);
                tab.otherTabs.Add(this);
            }
        }

        public void Activate()
        {
            IsActive = true;
            foreach (var tab in this.otherTabs)
                tab.IsActive = false;
        }

        public override void Draw(SpriteBatch sb, float alpha = 1)
        {
            sb.Draw(texPixel, hitbox, IsActive ? bgColorDown : bgColorUp);

            if (textureDown != null)
                sb.Draw(IsActive ? textureDown : textureUp, new Rectangle(iconPosition, iconSize), (isPressedDown ? tintDown : tintUp) * alpha);
            else
                sb.Draw(textureUp, new Rectangle(iconPosition, iconSize), (isPressedDown ? tintDown : tintUp) * alpha);
        }
    }
}
