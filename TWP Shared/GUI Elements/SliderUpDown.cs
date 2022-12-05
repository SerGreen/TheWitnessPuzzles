using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace TWP_Shared
{
    public class SliderUpDown : AbstractButton
    {
        const float vScale = 0.5f;

        float minValue, maxValue, step;
        public float Value { get; private set; }

        TouchButton btnDown, btnUp;
        Color bgColor, activeColor;
        Texture2D texPixel;

        Rectangle area;
        Point buttonSize, sliderPadding, sliderSize;

        public event Action ValueChanged;

        public SliderUpDown(Rectangle widgetArea, float minValue, float maxValue, float step, float startingValue, Texture2D texPixel, Texture2D texLeft, Texture2D texRight, Color bgColor, Color activeColor)
        {
            this.area = widgetArea;
            this.minValue = minValue;
            this.maxValue = maxValue;
            this.Value = startingValue;
            this.step = step;
            this.bgColor = bgColor;
            this.activeColor = activeColor;
            this.texPixel = texPixel;

            UpdateSize();
            SpawnButtons(texLeft, texRight);
        }

        private void UpdateSize()
        {
            buttonSize = new Point(area.Height);
            int spaceLeft = area.Width - buttonSize.X * 2;
            if (spaceLeft > 0)
            {
                int sliderPaddingX = spaceLeft / 20;
                int sliderWidth = spaceLeft - sliderPaddingX * 2;
                int sliderHeight = (int) (area.Height * 0.2f);
                int sliderPaddingY = (area.Height - sliderHeight) / 2;
                sliderPadding = new Point(sliderPaddingX + buttonSize.X, sliderPaddingY);
                sliderSize = new Point(sliderWidth, sliderHeight);
            }
        }

        public void SetPositionAndSize(Point pos, Point size)
        {
            Point scaledSize = new Point(size.X, (int) (size.Y * vScale));
            Point scaledPos = new Point(pos.X, pos.Y + (size.Y - scaledSize.Y) / 2);
            area = new Rectangle(scaledPos, scaledSize);
            UpdateSize();
            UpdateButtonsPositionAndSize();
        }

        private void SpawnButtons(Texture2D texLeft, Texture2D texRight)
        {
            btnDown = new TouchButton(new Rectangle(), texLeft);
            btnDown.Click += () =>
            {
                if(Value > minValue)
                {
                    Value = Math.Max(minValue, Value - step);
                    ValueChanged?.Invoke();
                    SoundManager.PlayOnce(Sound.ButtonNext, 0.8f);
                }
            };

            btnUp = new TouchButton(new Rectangle(), texRight);
            btnUp.Click += () =>
            {
                if (Value < maxValue)
                {
                    Value = Math.Min(maxValue, Value + step);
                    ValueChanged?.Invoke();
                    SoundManager.PlayOnce(Sound.ButtonNext, 0.8f);
                }
            };

            UpdateButtonsPositionAndSize();
        }

        private void UpdateButtonsPositionAndSize()
        {
            btnDown.SetPositionAndSize(area.Location, buttonSize);
            btnUp.SetPositionAndSize(new Point(area.X + area.Width - buttonSize.X, area.Y), buttonSize);
        }

        public override void Update(Point? touchPoint)
        {
            btnDown.Update(touchPoint);
            btnUp.Update(touchPoint);
        }

        public override void Draw(SpriteBatch sb, float alpha = 1f)
        {
            btnDown.Draw(sb);
            btnUp.Draw(sb);

            int filledWidth = (int) (sliderSize.X * (Value / maxValue));
            sb.Draw(texPixel, new Rectangle(area.Location + sliderPadding, sliderSize), bgColor);
            sb.Draw(texPixel, new Rectangle(area.Location + sliderPadding, new Point(filledWidth, sliderSize.Y)), activeColor);
        }
    }
}
