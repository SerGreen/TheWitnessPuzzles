using Microsoft.Xna.Framework;
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

        GraphicsDevice Device;
        ContentManager Content;
        public Point ScreenSize { get; set; }
        Stack<GameScreen> screenStack = new Stack<GameScreen>();
        public GameScreen CurrentScreen { get; private set; }
        public Dictionary<string, Texture2D> TextureProvider { get; private set; } = new Dictionary<string, Texture2D>();
        public Dictionary<string, SpriteFont> FontProvider { get; private set; } = new Dictionary<string, SpriteFont>();
        readonly static string[] texturesToLoad = 
        {
            "img/twp_pixel",
            "img/twp_circle",
            "img/twp_corner",
            "img/twp_ending_left",
            "img/twp_ending_top",
            "img/twp_hexagon",
            "img/twp_square",
            "img/twp_sun",
            "img/twp_elimination",
            "img/twp_triangle1..3",
            "img/twp_tetris",
            "img/twp_tetris_sub"
        };
        readonly static string[] fontsToLoad =
        {
            "font/fnt_constantia12",
            "font/fnt_constantia36"
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

            //if (typeof(TScreen) == typeof(PanelGameScreen))
            //    screen = (TScreen) Activator.CreateInstance(typeof(TScreen), (TheWitnessPuzzles.Puzzle) data, ScreenSize, Device, TextureProvider, FontProvider, Content);
            //else
            //    screen = (TScreen) Activator.CreateInstance(typeof(TScreen), ScreenSize, Device, TextureProvider, FontProvider, Content);
            screen = new PanelGameScreen(DI.Get<PanelGenerator>().GeneratePanel(), ScreenSize, Device, TextureProvider, FontProvider, Content);

            if (replaceCurrent)
                screenStack.Pop();
            screenStack.Push(screen);
            CurrentScreen = screen;
        }

        public void GoBack()
        {
            if(screenStack.Count > 1)
            {
                screenStack.Pop();
                CurrentScreen = screenStack.Peek();
            }
        }

        public void Initialize(GraphicsDevice device, ContentManager contentManager)
        {
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
            texPixel = TextureProvider["img/twp_pixel"];

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
    }
}
