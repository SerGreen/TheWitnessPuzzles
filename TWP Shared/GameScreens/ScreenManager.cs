﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace TWP_Shared
{
    public class ScreenManager
    {
        public static ScreenManager Instance = new ScreenManager();
        private ScreenManager() { }

        TWPGame TheGameWindow;
        GraphicsDevice Device;
        ContentManager Content;
        public Point ScreenSize { get; set; }
        Stack<GameScreen> screenStack = new Stack<GameScreen>();
        public GameScreen CurrentScreen { get; private set; }
        public Dictionary<string, Texture2D> TextureProvider { get; private set; } = new Dictionary<string, Texture2D>();
        public Dictionary<string, SpriteFont> FontProvider { get; private set; } = new Dictionary<string, SpriteFont>();
        readonly static string[] texturesToLoad = 
        {
            "img/pixel",
            "img/circle",
            "img/corner",
            "img/ending_left",
            "img/ending_top",
            "img/hexagon",
            "img/square",
            "img/sun",
            "img/elimination",
            "img/triangle1..3",
            "img/tetris",
            "img/tetris_sub",
            "img/checkbox0..1",
            "img/sound0..1",
            "img/next",
            "img/delete",
            "img/like0..1",
            "img/close"
        };
        readonly static string[] fontsToLoad =
        {
            "font/fnt_constantia_small",
            "font/fnt_constantia_big"
        };
        System.Text.RegularExpressions.Regex texIsMultiple = new System.Text.RegularExpressions.Regex(@"\d+\.\.\d+");
        System.Text.RegularExpressions.Regex texMultipleGetName = new System.Text.RegularExpressions.Regex(@".+(?=\d+\.\.)");

        FadeTransition transitionAnimation = new FadeTransition(15, 20, 10);
        Texture2D texPixel;

        public void AddScreen<TScreen>(bool replaceCurrent = false, bool doFadeAnimation = false, object data = null) where TScreen : GameScreen
        {
            if (doFadeAnimation)
            {
                Action callback = null;
                callback = () =>
                {
                    _addScreen<TScreen>(replaceCurrent, data);
                    transitionAnimation.FadeOutComplete -= callback;
                };
                transitionAnimation.FadeOutComplete += callback;
                transitionAnimation.Restart();
            }
            else
                _addScreen<TScreen>(replaceCurrent, data);
        }
        private void _addScreen<TScreen>(bool replaceCurrent, object data) where TScreen : GameScreen
        {
            GameScreen screen;

            if (typeof(TScreen) == typeof(PanelGameScreen))
                screen = (TScreen) Activator.CreateInstance(typeof(TScreen), (TheWitnessPuzzles.Puzzle) data, ScreenSize, Device, TextureProvider, FontProvider, Content);
            else
                screen = (TScreen) Activator.CreateInstance(typeof(TScreen), ScreenSize, Device, TextureProvider, FontProvider, Content);
            //screen = new PanelGameScreen(DI.Get<PanelGenerator>().GeneratePanel(), ScreenSize, Device, TextureProvider, FontProvider, Content);

            if (replaceCurrent)
                screenStack.Pop();
            screenStack.Push(screen);
            CurrentScreen = screen;
        }

        public void GoBack(bool doFadeAnimation = true)
        {
            if (screenStack.Count > 1)
            {
                if (doFadeAnimation)
                {
                    Action callback = null;
                    callback = () =>
                    {
                        _goBack();
                        transitionAnimation.FadeOutComplete -= callback;
                    };
                    transitionAnimation.FadeOutComplete += callback;
                    transitionAnimation.Restart();
                }
                else
                    _goBack();
            }
        }
        private void _goBack()
        {
            screenStack.Pop();
            CurrentScreen = screenStack.Peek();
            if (CurrentScreen.ScreenSize != ScreenSize)
                CurrentScreen.SetScreenSize(ScreenSize);
        }

        public void Initialize(TWPGame game, GraphicsDevice device, ContentManager contentManager)
        {
            TheGameWindow = game;
            Device = device;
            Content = contentManager;
        }
        public void LoadContent()
        {
            // Load all textures from the list
            foreach (string texName in texturesToLoad)
            {
                if (!texIsMultiple.IsMatch(texName))
                    TextureProvider.Add(texName, Content.Load<Texture2D>(texName));
                // If texture is specified in the form of "name0..20", it means that we should load "name0", "name1" ... "name20"
                else
                {
                    string name = texMultipleGetName.Match(texName).Value;
                    int[] range = Array.ConvertAll(texIsMultiple.Match(texName).Value.Split(new string[] { ".." }, StringSplitOptions.RemoveEmptyEntries), int.Parse);
                    for (int i = range[0]; i <= range[1]; i++)
                        TextureProvider.Add(name + i, Content.Load<Texture2D>(name + i));
                }
            }
            texPixel = TextureProvider["img/pixel"];

            // Load all fonts from the list
            foreach (string fontName in fontsToLoad)
                FontProvider.Add(fontName, Content.Load<SpriteFont>(fontName));
        }
        public void Update(GameTime gameTime)
        {
            CurrentScreen?.Update(gameTime);
            transitionAnimation.Update();
        }
        public void Draw(SpriteBatch spriteBatch)
        {
            CurrentScreen?.Draw(spriteBatch);

            if (transitionAnimation.IsActive)
                spriteBatch.Draw(texPixel, new Rectangle(Point.Zero, ScreenSize), Color.Black * transitionAnimation.Opacity);
        }

        public void UpdateScreenSize(Point screenSize)
        {
            ScreenSize = screenSize;
            if (CurrentScreen != null && CurrentScreen.ScreenSize != screenSize)
                CurrentScreen.SetScreenSize(screenSize);
        }

        public void UpdateFullscreen() => TheGameWindow.SetFullscreen(SettingsManager.IsFullscreen);
    }
}