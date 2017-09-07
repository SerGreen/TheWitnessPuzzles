using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TheWitnessPuzzles;
using System.Linq;

namespace TWP_Shared
{
    public class HistoryGameScreen : GameScreen
    {
        enum HistoryTab { Solved, Discarded, Favourites };

        HistoryTab currentTab = HistoryTab.Solved;

        static readonly int defaultPageWidth = 3;
        static readonly int defaultPageHeight = 4;
        int pageWidth, pageHeight;

        SpriteFont font;
        Texture2D texPixel, texLeft, texRight, texSolved, texDiscarded, texFavourite;

        PanelRenderer renderer;

        List<TabButton> tabs = new List<TabButton>();
        List<TouchButton> panels = new List<TouchButton>();
        TouchButton btnBack, btnNextPage, btnPrevPage;
        Point panelsPosition;
        Point panelSize;
        Point tabSize;
        Point btnBackSize;
        Point paginatorPosition;
        int paginatorBtnSize;
        float paginatorTextScale;

        string[] fileNames = null;
        int renderWidth, renderHeight;
        int panelsOnPage;
        int currentPage = 0;
        int maxPage;

        public HistoryGameScreen(Point screenSize, GraphicsDevice device, Dictionary<string, Texture2D> textureProvider, Dictionary<string, SpriteFont> fontProvider, ContentManager Content) 
            : base(screenSize, device, textureProvider, fontProvider, Content)
        {
            font = fontProvider["font/fnt_constantia_big"];
            texPixel = textureProvider["img/pixel"];
            texLeft = textureProvider["img/left"];
            texRight = textureProvider["img/right"];
            texSolved = textureProvider["img/solved"];
            texDiscarded = textureProvider["img/delete"];
            texFavourite = textureProvider["img/like0"];
            renderer = new PanelRenderer(null, screenSize, textureProvider, GraphicsDevice);
            
            InitializeScreenSizeDependent();
            SpawnButtons();
        }

        private void InitializeScreenSizeDependent()
        {
            bool isHorizontal = ScreenSize.X > ScreenSize.Y;
            int screenMin = isHorizontal ? ScreenSize.Y : ScreenSize.X;
            int width, height;
            if (isHorizontal)
            {
                width = defaultPageHeight;
                height = defaultPageWidth;
            }
            else
            {
                width = defaultPageWidth;
                height = defaultPageHeight;
            }

            btnBackSize = new Point((int) (screenMin * 0.3f), (int) (screenMin * 0.1f));

            if (isHorizontal)
                tabSize = new Point((int) (ScreenSize.X * 0.1f), ScreenSize.Y / 3);
            else
                tabSize = new Point(ScreenSize.X / 3, (int) (ScreenSize.Y * 0.1f));
            int tabMinSize = Math.Min(tabSize.X, tabSize.Y);

            int panelsMarginX, panelsMarginY;
            if (isHorizontal)
            {
                panelSize = new Point((int) (ScreenSize.X * 0.8f / width), (int) (ScreenSize.Y * 0.8f / height));

                panelsMarginX = (ScreenSize.X - tabSize.X - panelSize.X * width) / 2 + tabSize.X;
                panelsMarginY = (ScreenSize.Y - btnBackSize.Y - panelSize.Y * height) / 2;
            }
            else
            {
                panelSize = new Point((int) (ScreenSize.X * 0.9f / width), (int) (ScreenSize.X * 0.8f / height));

                panelsMarginX = (ScreenSize.X - panelSize.X * width) / 2;
                panelsMarginY = (ScreenSize.Y - tabSize.Y - btnBackSize.Y - panelSize.Y * height) / 2 + tabSize.Y;
            }

            renderWidth = panelSize.X;
            renderHeight = panelSize.Y;
            if (renderWidth < 200 || renderHeight < 200)
            {
                float panelAspectRatio = panelSize.X / (float) panelSize.Y;
                if (renderWidth < renderHeight)
                {
                    renderWidth = 200;
                    renderHeight = (int) (renderWidth / panelAspectRatio);
                }
                else
                {
                    renderHeight = 200;
                    renderWidth = (int) (renderHeight * panelAspectRatio);
                }
            }
            renderer.SetScreenSize(new Point(renderWidth, renderHeight));

            panelsPosition = new Point(panelsMarginX, panelsMarginY);

            paginatorBtnSize = (int) (screenMin * 0.16f);
            if(isHorizontal)
                paginatorPosition = new Point((int) (ScreenSize.X - paginatorBtnSize * 1.5f), (ScreenSize.Y - paginatorBtnSize * 4) / 2);
            else
                paginatorPosition = new Point((ScreenSize.X - paginatorBtnSize * 4) / 2, (int) (ScreenSize.Y - btnBackSize.Y - paginatorBtnSize * 1.5f));

            float fontHeight = font.MeasureString("V").Y;
            paginatorTextScale = paginatorBtnSize / fontHeight;
        }
        private void UpdatePageSize()
        {
            if (ScreenSize.X > ScreenSize.Y)
            {
                pageWidth = defaultPageHeight;
                pageHeight = defaultPageWidth;
            }
            else
            {
                pageWidth = defaultPageWidth;
                pageHeight = defaultPageHeight;
            }

            if (fileNames.Length > pageWidth * pageHeight)
            {
                if (ScreenSize.X > ScreenSize.Y)
                    pageWidth--;
                else
                    pageHeight--;
            }
            panelsOnPage = pageWidth * pageHeight;
            maxPage = (int) Math.Ceiling(fileNames.Length / (double) panelsOnPage) - 1;
        }

        private void SpawnButtons()
        {
            TabButton btnSolved = new TabButton(new Rectangle(), texPixel, texSolved, null, Color.DarkGray, new Color(14, 14, 14));
            btnSolved.Click += () =>
            {
                currentTab = HistoryTab.Solved;
                currentPage = 0;
                fileNames = FileStorageManager.GetSolvedPanelsNames();
                UpdatePageSize();
                RespawnPanelButtons();
            };
            tabs.Add(btnSolved);

            TabButton btnDiscarded = new TabButton(new Rectangle(), texPixel, texDiscarded, null, Color.DarkGray, new Color(14, 14, 14));
            btnDiscarded.Click += () =>
            {
                currentTab = HistoryTab.Discarded;
                currentPage = 0;
                fileNames = FileStorageManager.GetDiscardedPanelsNames();
                UpdatePageSize();
                RespawnPanelButtons();
            };
            tabs.Add(btnDiscarded);

            TabButton btnFavs = new TabButton(new Rectangle(), texPixel, texFavourite, null, Color.DarkGray, new Color(14, 14, 14));
            btnFavs.Click += () =>
            {
                currentTab = HistoryTab.Favourites;
                currentPage = 0;
                fileNames = FileStorageManager.GetFavouritePanelsNames();
                UpdatePageSize();
                RespawnPanelButtons();
            };
            tabs.Add(btnFavs);

            foreach (TabButton tab in tabs)
                foreach (TabButton otherTab in tabs.Where(x => x != tab))
                    tab.ConnectTab(otherTab);
            btnSolved.Activate();

            btnBack = new TextButton(new Rectangle(), font, "Back", texPixel);
            btnBack.Click += () =>
            {
                ScreenManager.Instance.GoBack();
                SoundManager.PlayOnce(Sound.MenuEscape);
            };

            btnPrevPage = new TouchButton(new Rectangle(), texLeft);
            btnPrevPage.Click += () =>
            {
                if (currentPage > 0)
                {
                    currentPage--;
                    RespawnPanelButtons();
                }
                SoundManager.PlayOnce(Sound.ButtonNext);
            };

            btnNextPage= new TouchButton(new Rectangle(), texRight);
            btnNextPage.Click += () =>
            {
                if (currentPage < maxPage)
                {
                    currentPage++;
                    RespawnPanelButtons();
                }
                SoundManager.PlayOnce(Sound.ButtonNext);
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

            btnBack.SetPositionAndSize(new Point(ScreenSize.X / 2 - btnBackSize.X / 2, (int) (ScreenSize.Y - btnBackSize.Y * 1.2f)), btnBackSize);
            
            if (ScreenSize.X > ScreenSize.Y)
            {
                btnNextPage.SetPositionAndSize(new Point(paginatorPosition.X, paginatorPosition.Y), new Point(paginatorBtnSize));
                btnPrevPage.SetPositionAndSize(new Point(paginatorPosition.X, paginatorPosition.Y + paginatorBtnSize * 3), new Point(paginatorBtnSize));
            }
            else
            {
                btnPrevPage.SetPositionAndSize(new Point(paginatorPosition.X, paginatorPosition.Y), new Point(paginatorBtnSize));
                btnNextPage.SetPositionAndSize(new Point(paginatorPosition.X + paginatorBtnSize* 3, paginatorPosition.Y), new Point(paginatorBtnSize));
            }
        }

        private void SpawnPanelButtons()
        {
            int fileIndex = currentPage * panelsOnPage;
            for (int j = 0; j < pageHeight; j++)
                for (int i = 0; i < pageWidth; i++, fileIndex++)
                {
                    if (fileIndex >= fileNames.Length)
                        break;

                    Puzzle panel = FileStorageManager.LoadPanelFromFile(fileNames[fileIndex]);
                    renderer.SetPanel(panel);
                    RenderTarget2D texture = new RenderTarget2D(GraphicsDevice, renderWidth, renderHeight);
                    renderer.RenderPanelToTexture(texture);

                    Point size = new Point((int) (panelSize.X * 0.9f), (int) (panelSize.Y * 0.9f));
                    Point margin = (panelSize - size).Divide(2);
                    Point pos = new Point(panelsPosition.X + panelSize.X * i + margin.X, panelsPosition.Y + panelSize.Y * j + margin.Y);

                    TouchButton btnPanel = new TouchButton(new Rectangle(pos, size), texture);
                    btnPanel.Click += () =>
                    {
                        ScreenManager.Instance.AddScreen<PanelGameScreen>(false, true, panel, true);
                        SoundManager.PlayOnce(Sound.MenuEnter);
                    };
                    panels.Add(btnPanel);
                }
        }
        private void RespawnPanelButtons()
        {
            panels.Clear();
            SpawnPanelButtons();
        }

        public override void Activate()
        {
            switch (currentTab)
            {
                case HistoryTab.Solved:     fileNames = FileStorageManager.GetSolvedPanelsNames();    break;
                case HistoryTab.Discarded:  fileNames = FileStorageManager.GetDiscardedPanelsNames(); break;
                case HistoryTab.Favourites: fileNames = FileStorageManager.GetFavouritePanelsNames(); break;
            }

            UpdatePageSize();
            RespawnPanelButtons();
        }

        public override void SetScreenSize(Point screenSize)
        {
            base.SetScreenSize(screenSize);
            UpdatePageSize();
            InitializeScreenSizeDependent();
            UpdateButtonsPositions();
            RespawnPanelButtons();
        }

        public override void Update(GameTime gameTime)
        {
            Point? touchPoint = InputManager.GetTapPosition();
            foreach (var tab in tabs)
                tab.Update(touchPoint);
            foreach (var panel in panels)
                panel.Update(touchPoint);
            btnBack.Update(touchPoint);

            if (maxPage > 0)
            {
                btnPrevPage.Update(touchPoint);
                btnNextPage.Update(touchPoint);
            }
        }


        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texPixel, new Rectangle(Point.Zero, ScreenSize), Color.Black);

            foreach (var tab in tabs)
                tab.Draw(spriteBatch);
            foreach (var panel in panels)
                panel.Draw(spriteBatch);
            btnBack.Draw(spriteBatch);

            if (maxPage > 0)
            {
                btnPrevPage.Draw(spriteBatch);
                btnNextPage.Draw(spriteBatch);

                string text = $"{currentPage + 1}/{maxPage + 1}";
                Vector2 textSize = font.MeasureString(text) * paginatorTextScale;

                if (ScreenSize.X > ScreenSize.Y)
                    spriteBatch.DrawString(font, text, new Vector2(paginatorPosition.X + (paginatorBtnSize - textSize.X) / 2, paginatorPosition.Y + paginatorBtnSize + (paginatorBtnSize * 2 - textSize.Y) / 2), Color.White, 0, Vector2.Zero, paginatorTextScale, SpriteEffects.None, 0);
                else
                    spriteBatch.DrawString(font, text, new Vector2(paginatorPosition.X + paginatorBtnSize + (paginatorBtnSize * 2 - textSize.X) / 2, paginatorPosition.Y + paginatorBtnSize * 0.2f), Color.White, 0, Vector2.Zero, paginatorTextScale, SpriteEffects.None, 0);
            }

            base.Draw(spriteBatch);
        }
    }
}
