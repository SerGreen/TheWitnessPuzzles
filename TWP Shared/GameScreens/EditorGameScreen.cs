using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using TheWitnessPuzzles;
using Microsoft.Xna.Framework.Content;

namespace TWP_Shared
{
    public class EditorGameScreen : GameScreen
    {
        enum EditorState { PanelDimensionsSelect, Main, NodeEdit, EdgeEdit, BlockEdit, TetrisSelect }

        PanelRenderer renderer, miniRenderer;
        EditorState state = EditorState.PanelDimensionsSelect;

        Point panelDimensions;
        Puzzle panel;

        SpriteFont font;
        Texture2D texPixel, texSquare;
        RenderTarget2D panelTexture, miniPanelTexture;

        TextButton btnBack, btnNext;

        int dimHitboxSize;
        Vector2 oneDigitSize;
        float digitScale;
        Rectangle[] hDimensions = new Rectangle[6];
        Rectangle[] vDimensions = new Rectangle[6];
        Rectangle[,] dimMatrix = new Rectangle[7, 7];

        public EditorGameScreen(Point screenSize, GraphicsDevice device, Dictionary<string, Texture2D> textureProvider, Dictionary<string, SpriteFont> fontProvider, ContentManager Content) 
            : base(screenSize, device, textureProvider, fontProvider, Content)
        {
            renderer = new PanelRenderer(null, screenSize, textureProvider, GraphicsDevice)
            {
                BackgroundColor = Color.White,
                BorderColor = new Color(39, 166, 204),
                WallColor = new Color(39, 166, 204)
            };

            int minScreenDim = Math.Min(screenSize.X, screenSize.Y);
            miniRenderer = new PanelRenderer(null, new Point((int) (minScreenDim * 0.2f)), textureProvider, GraphicsDevice)
            {
                WallColor = Color.Black,
                BorderColor = Color.Black,
                BackgroundColor = Color.White
            };

            font = fontProvider["font/fnt_constantia_big"];
            texPixel = textureProvider["img/pixel"];
            texSquare = textureProvider["img/square"];
            panelTexture = new RenderTarget2D(GraphicsDevice, GraphicsDevice.PresentationParameters.BackBufferWidth, GraphicsDevice.PresentationParameters.BackBufferHeight, false, GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.Depth24);
            miniPanelTexture = new RenderTarget2D(GraphicsDevice, (int)(minScreenDim * 0.2f), (int)(minScreenDim * 0.2f), false, GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.Depth24);

            panelDimensions = new Point(4, 4);
            oneDigitSize = font.MeasureString("8");
            dimHitboxSize = minScreenDim / 10;
            digitScale = dimHitboxSize / (Math.Max(oneDigitSize.X, oneDigitSize.Y));
            Point dimMatrixStart = new Point((screenSize.X - dimHitboxSize * 10) / 2 + dimHitboxSize, (screenSize.Y - dimHitboxSize * 10) / 2 + dimHitboxSize);
            for (int i = 0; i < 6; i++)
                hDimensions[i] = new Rectangle(dimMatrixStart.X + dimHitboxSize * (i+2), dimMatrixStart.Y, dimHitboxSize, dimHitboxSize);
            for (int i = 0; i < 6; i++)
                vDimensions[i] = new Rectangle(dimMatrixStart.X, dimMatrixStart.Y + dimHitboxSize * (i+2), dimHitboxSize, dimHitboxSize);
            for (int i = 0; i < 7; i++)
                for (int j = 0; j < 7; j++)
                    dimMatrix[i, j] = new Rectangle(dimMatrixStart.X + (i+1) * dimHitboxSize, dimMatrixStart.Y + (j+1) * dimHitboxSize, dimHitboxSize, dimHitboxSize);

            SpawnButtons();
        }

        private void SpawnButtons()
        {
            int btnSize = (int) (Math.Min(ScreenSize.X, ScreenSize.Y) * 0.2f);
            btnBack = new TextButton(new Rectangle(btnSize / 2, ScreenSize.Y - btnSize, btnSize, btnSize / 2), font, "Back", texPixel);
            btnBack.Click += () =>
            {
                SoundManager.PlayOnce(Sound.MenuEscape);

                switch (state)
                {
                    case EditorState.PanelDimensionsSelect:
                        ScreenManager.Instance.GoBack();
                        break;
                    case EditorState.Main:
                        state = EditorState.PanelDimensionsSelect;
                        break;
                    case EditorState.NodeEdit:
                        break;
                    case EditorState.EdgeEdit:
                        break;
                    case EditorState.BlockEdit:
                        break;
                    case EditorState.TetrisSelect:
                        break;
                }
            };

            btnNext = new TextButton(new Rectangle(ScreenSize.X -btnSize - btnSize / 2, ScreenSize.Y - btnSize, btnSize, btnSize / 2), font, "Next", texPixel);
            btnNext.Click += () =>
            {
                SoundManager.PlayOnce(Sound.MenuEnter);

                switch (state)
                {
                    case EditorState.PanelDimensionsSelect:
                        state = EditorState.Main;
                        if (panel == null)
                            panel = new Puzzle(panelDimensions.X, panelDimensions.Y);
                        renderer.SetPanel(panel);
                        renderer.RenderPanelToTexture(panelTexture);
                        break;
                    case EditorState.Main:
                        break;
                    case EditorState.NodeEdit:
                        break;
                    case EditorState.EdgeEdit:
                        break;
                    case EditorState.BlockEdit:
                        break;
                    case EditorState.TetrisSelect:
                        break;
                }
            };
        }

        public override void Update(GameTime gameTime)
        {
            Point? tap = InputManager.GetTapPosition();
            switch (state)
            {
                case EditorState.PanelDimensionsSelect:
                    HandlePanelDimensions(tap);
                    btnBack.Update(tap);
                    btnNext.Update(tap);
                    break;
                case EditorState.Main:
                    btnBack.Update(tap);
                    break;
                case EditorState.NodeEdit:
                    break;
                case EditorState.EdgeEdit:
                    break;
                case EditorState.BlockEdit:
                    break;
                case EditorState.TetrisSelect:
                    break;
            }
        }

        private void HandlePanelDimensions(Point? tap)
        {   
            if (tap != null)
            {
                bool changed = false;

                for (int i = 0; i < hDimensions.Length; i++)
                    if (hDimensions[i].Contains(tap.Value))
                    {
                        panelDimensions.X = i + 2;
                        changed = true;
                    }
                for (int i = 0; i < vDimensions.Length; i++)
                    if (vDimensions[i].Contains(tap.Value))
                    {
                        panelDimensions.Y = i + 2;
                        changed = true;
                    }

                for (int i = 0; i < 7; i++)
                    for (int j = 0; j < 7; j++)
                        if (i > 0 && j > 0 && dimMatrix[i, j].Contains(tap.Value))
                        {
                            panelDimensions.X = i + 1;
                            panelDimensions.Y = j + 1;
                            changed = true;
                        }

                if(changed)
                {
                    panel = new Puzzle(panelDimensions.X, panelDimensions.Y);
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            switch (state)
            {
                case EditorState.PanelDimensionsSelect:
                    spriteBatch.Draw(texPixel, new Rectangle(Point.Zero, ScreenSize), Color.Black);
                    for (int i = 0; i < hDimensions.Length; i++)
                    {
                        spriteBatch.DrawString(font, (i + 2).ToString(), hDimensions[i].Center.ToVector2(), i + 2 == panelDimensions.X ? Color.Gold : Color.LightGray, 0, new Vector2(oneDigitSize.X / 2, oneDigitSize.Y / 2), digitScale, SpriteEffects.None, 0);
                    }
                    for (int i = 0; i < vDimensions.Length; i++)
                    {
                        spriteBatch.DrawString(font, (i + 2).ToString(), vDimensions[i].Center.ToVector2(), i + 2 == panelDimensions.Y ? Color.Gold : Color.LightGray, 0, new Vector2(oneDigitSize.X / 2, oneDigitSize.Y / 2), digitScale, SpriteEffects.None, 0);
                    }
                    for (int i = 0; i < 7; i++)
                        for (int j = 0; j < 7; j++)
                            spriteBatch.Draw(texSquare, dimMatrix[i, j], (i + 1 <= panelDimensions.X && j + 1 <= panelDimensions.Y) ? Color.Gold : Color.Gray);

                    btnBack.Draw(spriteBatch);
                    btnNext.Draw(spriteBatch);
                    break;
                case EditorState.Main:
                    if (panelTexture != null)
                        spriteBatch.Draw(panelTexture, Vector2.Zero, Color.White);

                    btnBack.Draw(spriteBatch);
                    break;
                case EditorState.NodeEdit:
                    break;
                case EditorState.EdgeEdit:
                    break;
                case EditorState.BlockEdit:
                    break;
                case EditorState.TetrisSelect:
                    break;
            }
        }

        public override void SetScreenSize(Point screenSize)
        {
            throw new NotImplementedException();
        }
    }
}
