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
        public Point ScreenSize { get; protected set; }
        protected GraphicsDevice GraphicsDevice;
        protected Dictionary<string, Texture2D> TextureProvider;
        protected Dictionary<string, SpriteFont> FontProvider;

        public GameScreen(Point screenSize, GraphicsDevice device, Dictionary<string, Texture2D> textureProvider, Dictionary<string, SpriteFont> fontProvider, ContentManager Content)
        {
            GraphicsDevice = device;
            ScreenSize = screenSize;
            TextureProvider = textureProvider;
            FontProvider = fontProvider;
        }

        public virtual void Activate() { }
        public virtual void Deactivate() { }

        public virtual void Update(GameTime gameTime) { }
        public virtual void Draw(SpriteBatch spriteBatch) { }
        public virtual void SetScreenSize(Point screenSize)
        {
            if (screenSize.X > 0 && screenSize.Y > 0)
                ScreenSize = screenSize;
        }
    }
}
