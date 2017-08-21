using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace TWP_Shared
{
    public abstract class GameScreen
    {
        protected ContentManager Content;
        protected Point ScreenSize;
        protected GraphicsDevice GraphicsDevice;

        public GameScreen(Point screenSize, GraphicsDevice device)
        {
            GraphicsDevice = device;
            ScreenSize = screenSize;
        }

        public virtual void Initialize() { }
        public virtual void LoadContent(Dictionary<string, Texture2D> TextureProvider, Dictionary<string, SpriteFont> FontProvider) { }
        public virtual void Update(GameTime gameTime) { }
        public virtual void Draw(SpriteBatch spriteBatch) { }
    }
}
