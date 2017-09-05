using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TWP_Shared
{
    public class HistoryGameScreen : GameScreen
    {
        enum HistoryTab { Solved, Discarded, Favourites };

        HistoryTab currentTab = HistoryTab.Solved;

        int width = 3;
        int height = 4;

        SpriteFont font;
        Texture2D texPixel;

        List<TouchButton> tabs = new List<TouchButton>();
        List<TouchButton> panels = new List<TouchButton>();
        TextButton btnBack;
        Point panelsPosition;
        Point panelSize;
        Point tabSize;
        Point btnBackSize;

        PanelRenderer renderer;

        public HistoryGameScreen(Point screenSize, GraphicsDevice device, Dictionary<string, Texture2D> textureProvider, Dictionary<string, SpriteFont> fontProvider, ContentManager Content) 
            : base(screenSize, device, textureProvider, fontProvider, Content)
        {
            font = fontProvider["font/fnt_constantia_big"];
            texPixel = textureProvider["img/pixel"];
            InitializeScreenSizeDependent();
            SpawnButtons();
        }

        private void InitializeScreenSizeDependent()
        {
            bool isHorizontal = ScreenSize.X > ScreenSize.Y;
            int screenMin = isHorizontal ? ScreenSize.Y : ScreenSize.X;
            if (isHorizontal)
            {
                int temp = width;
                width = height;
                height = temp;
            }

            btnBackSize = new Point((int) (screenMin * 0.3f), (int) (screenMin * 0.1f));

            if (isHorizontal)
                tabSize = new Point((int) (ScreenSize.X * 0.1f), ScreenSize.Y / 3);
            else
                tabSize = new Point(ScreenSize.X / 3, (int) (ScreenSize.Y * 0.1f));
            int tabMinSize = Math.Min(tabSize.X, tabSize.Y);

            int panelsMarginX, panelsMarginY;
            if(isHorizontal)
            {
                panelSize = new Point((int) (ScreenSize.X * 0.8f / width), (int) (ScreenSize.Y * 0.8f / height));

                panelsMarginX = (int) ((ScreenSize.X - tabSize.X - panelSize.X * width) / 2) + tabSize.X;
                panelsMarginY = (int) (ScreenSize.Y - btnBackSize.Y - panelSize.Y * height) / 2;
            }
            else
            {
                panelSize = new Point((int) (ScreenSize.X * 0.9f / width), (int) (ScreenSize.X * 0.8f / height));
                
                panelsMarginX = (int) (ScreenSize.X - panelSize.X * width) / 2;
                panelsMarginY = (int) ((ScreenSize.Y - tabSize.Y - btnBackSize.Y - panelSize.Y * height) / 2) + tabSize.Y;
            }

            panelsPosition = new Point(panelsMarginX, panelsMarginY);
        }

        private void SpawnButtons()
        {
            TouchButton btnSolved = new TextButton(new Rectangle(), font, "Solved", texPixel);
            btnSolved.Click += () =>
            {
                currentTab = HistoryTab.Solved;
            };
            tabs.Add(btnSolved);

            TouchButton btnDiscarded = new TextButton(new Rectangle(), font, "Discarded", texPixel);
            btnDiscarded.Click += () =>
            {
                currentTab = HistoryTab.Discarded;
            };
            tabs.Add(btnDiscarded);

            TouchButton btnFavs = new TextButton(new Rectangle(), font, "Fav", texPixel);
            btnFavs.Click += () =>
            {
                currentTab = HistoryTab.Favourites;
            };
            tabs.Add(btnFavs);

            btnBack = new TextButton(new Rectangle(), font, "Back", texPixel);
            btnBack.Click += () =>
            {
                ScreenManager.Instance.GoBack();
                SoundManager.PlayOnce(Sound.MenuEscape);
            };

            UpdateButtonsPositions();
        }

        private void UpdateButtonsPositions()
        {
            if(ScreenSize.X > ScreenSize.Y)
            {
                tabs[0].SetPositionAndSize(Point.Zero, tabSize);
                tabs[1].SetPositionAndSize(new Point(0, tabSize.Y), tabSize);
                tabs[2].SetPositionAndSize(new Point(0, tabSize.Y * 2), tabSize);
            }
            else
            {
                tabs[0].SetPositionAndSize(Point.Zero, tabSize);
                tabs[1].SetPositionAndSize(new Point(tabSize.X, 0), tabSize);
                tabs[2].SetPositionAndSize(new Point(tabSize.X * 2, 0), tabSize);
            }

            btnBack.SetPositionAndSize(new Point(ScreenSize.X / 2 - btnBackSize.X / 2, ScreenSize.Y - btnBackSize.Y), btnBackSize);
        }

        public override void SetScreenSize(Point screenSize)
        {
            base.SetScreenSize(screenSize);
            InitializeScreenSizeDependent();
            UpdateButtonsPositions();
        }

        public override void Update(GameTime gameTime)
        {
            Point? touchPoint = InputManager.GetTapPosition();
            foreach (var tab in tabs)
                tab.Update(touchPoint);
            foreach (var panel in panels)
                panel.Update(touchPoint);
            btnBack.Update(touchPoint);
        }


        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texPixel, new Rectangle(Point.Zero, ScreenSize), Color.Black);

            foreach (var tab in tabs)
                tab.Draw(spriteBatch);
            foreach (var panel in panels)
                panel.Draw(spriteBatch);
            btnBack.Draw(spriteBatch);

            base.Draw(spriteBatch);
        }
    }
}
