using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TWP_Shared
{
    public class TextButton : TouchButton
    {
        SpriteFont font;
        string text;
        Color textColor;
        Color textColorPressed;
        Texture2D texPixel;
        float scale;
        Vector2 position;
        ButtonAlignment alignment;

        public enum ButtonAlignment { Left, Center, Right };

        public TextButton(Rectangle bounds, SpriteFont font, string text, Texture2D texPixel, Color? textColor = null, Color? textColorPressed = null, ButtonAlignment buttonAlignment = ButtonAlignment.Center) 
            : base(bounds, null, null)
        {
            this.font = font;
            this.texPixel = texPixel;
            this.text = text;
            this.textColor = textColor ?? Color.White;
            this.textColorPressed = textColorPressed ?? Color.DarkGray;
            this.alignment = buttonAlignment;

            CalculateRenderDetails();
        }

        private void CalculateRenderDetails()
        {
            Vector2 renderSize = font.MeasureString(text);
            float xScale = hitbox.Width / renderSize.X;
            float yScale = hitbox.Height / renderSize.Y;
            scale = Math.Min(xScale, yScale);
            renderSize.X *= scale;
            renderSize.Y *= scale;

            switch (alignment)
            {
                case ButtonAlignment.Left:
                    position = new Vector2(hitbox.X, hitbox.Y + (hitbox.Height - renderSize.Y) / 2);
                    break;
                case ButtonAlignment.Center:
                    position = new Vector2(hitbox.X + (hitbox.Width - renderSize.X) / 2, hitbox.Y + (hitbox.Height - renderSize.Y) / 2);
                    break;
                case ButtonAlignment.Right:
                    position = new Vector2(hitbox.X + hitbox.Width - renderSize.X, hitbox.Y + (hitbox.Height - renderSize.Y) / 2);
                    break;
                default:
                    break;
            }
        }

        public override void SetPositionAndSize(Point pos, Point size)
        {
            base.SetPositionAndSize(pos, size);
            CalculateRenderDetails();
        }

        public override void Draw(SpriteBatch sb, float alpha = 1f)
        {
            sb.DrawString(font, text, position, (isPressedDown ? textColorPressed : textColor) * alpha, 0, Vector2.Zero, scale, SpriteEffects.None, 0);
        }
    }
}
