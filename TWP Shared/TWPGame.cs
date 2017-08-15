using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using TheWitnessPuzzles;

namespace TWP_Shared
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class TWPGame : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        SolutionLine line;
        Texture2D t; //base for the line texture

        public TWPGame(bool isMobile)
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            if (isMobile)
            {
                graphics.IsFullScreen = true;
                graphics.PreferredBackBufferWidth = 800;
                graphics.PreferredBackBufferHeight = 480;
                graphics.SupportedOrientations = DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight;
            }

            line = new SolutionLine(new Point(100));
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here

            t = new Texture2D(GraphicsDevice, 1, 1);
            t.SetData<Color>(new Color[] { Color.White });// fill the texture with white
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here
            if (Keyboard.GetState().IsKeyDown(Keys.Right))
                line.Move(new Vector2(2, 0), line.Hitboxes);

            if (Keyboard.GetState().IsKeyDown(Keys.Down))
                line.Move(new Vector2(0, 2), line.Hitboxes);

            if (Keyboard.GetState().IsKeyDown(Keys.Left))
                line.Move(new Vector2(-2, 0), line.Hitboxes);

            if (Keyboard.GetState().IsKeyDown(Keys.Up))
                line.Move(new Vector2(0, -2), line.Hitboxes);

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here

            spriteBatch.Begin();
            foreach (var hitbox in line.Hitboxes.Concat(new List<Rectangle> { line.Head }))
            {
                spriteBatch.Draw(t, hitbox, Color.White);
            }

            DrawLine(spriteBatch, //draw line
                new Vector2(200, 200), //start of line
                new Vector2(100, 50) //end of line
            );
            spriteBatch.End();

            base.Draw(gameTime);
        }

        private void DrawLine(SpriteBatch sb, Vector2 start, Vector2 end)
        {
            Vector2 edge = end - start;
            // calculate angle to rotate line
            float angle =
                (float) Math.Atan2(edge.Y, edge.X);


            sb.Draw(t,
                new Rectangle(// rectangle defines shape of line and position of start of line
                    (int) start.X,
                    (int) start.Y,
                    (int) edge.Length(), //sb will strech the texture to fill this rectangle
                    1), //width of line, change this to make thicker line
                null,
                Color.Red, //colour of line
                angle,     //angle of line (calulated above)
                new Vector2(0, 0), // point in line about which to rotate
                SpriteEffects.None,
                0);

        }
    }
}
