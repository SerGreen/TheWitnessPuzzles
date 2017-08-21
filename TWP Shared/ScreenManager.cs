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
        Dictionary<string, Texture2D> TextureProvider = new Dictionary<string, Texture2D>();
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
            "img/twp_triangle1..3"
        };
        System.Text.RegularExpressions.Regex texIsMultiple = new System.Text.RegularExpressions.Regex(@"\d+\.\.\d+");
        System.Text.RegularExpressions.Regex texMultipleGetName = new System.Text.RegularExpressions.Regex(@".+(?=\d+\.\.)");

        public void AddScreen(GameScreen screen)
        {
            screenStack.Push(screen);
            CurrentScreen = screen;
            CurrentScreen.LoadContent(TextureProvider);
        }

        public void ReplaceScreen(GameScreen screen)
        {
            screenStack.Pop();
            screenStack.Push(screen);
            CurrentScreen = screen;
            CurrentScreen.LoadContent(TextureProvider);
        }

        public void Initialize(GraphicsDevice device)
        {
            Device = device;
        }
        public void LoadContent(ContentManager contentManager)
        {
            Content = contentManager;
            
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
        }
        public void Update(GameTime gameTime)
        {
            CurrentScreen?.Update(gameTime);
        }
        public void Draw(SpriteBatch spriteBatch) {
            CurrentScreen?.Draw(spriteBatch);
        }
    }
}
