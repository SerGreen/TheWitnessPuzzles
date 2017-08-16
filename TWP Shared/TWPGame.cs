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

        const int fullscreenCooldownMax = 30;
        int fullscreenCooldown = 0;

        SolutionLine line;
        //SolutionLine lineMir;
        List<Rectangle> walls = new List<Rectangle>();
        Texture2D t; //base for the line texture

        Point puzzleDimensions;

        public TWPGame(bool isMobile)
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            if (isMobile)
            {
                graphics.IsFullScreen = true;
                graphics.PreferredBackBufferWidth = 480;
                graphics.PreferredBackBufferHeight = 800;
                graphics.SupportedOrientations = DisplayOrientation.Portrait;
            }

            puzzleDimensions = new Point(4, 3);
            int maxPuzzleDimension = Math.Max(puzzleDimensions.X, puzzleDimensions.Y);

            int width = graphics.PreferredBackBufferWidth;
            int height = graphics.PreferredBackBufferHeight;

            int screenMinSize = Math.Min(width, height);
            int puzzleMaxSize = (int) (screenMinSize * PuzzleSpaceRatio(maxPuzzleDimension));
            int lineWidth = (int) (puzzleMaxSize * LineSizeRatio(maxPuzzleDimension));
            int blockWidth = (int) (puzzleMaxSize * BlockSizeRatio(maxPuzzleDimension));

            int puzzleWidth = lineWidth * (puzzleDimensions.X + 1) + blockWidth * puzzleDimensions.X;
            int puzzleHeight = lineWidth * (puzzleDimensions.Y + 1) + blockWidth * puzzleDimensions.Y;

            int xMargin = (width - puzzleWidth) / 2;
            int yMargin = (height - puzzleHeight) / 2;

            int nodePadding = lineWidth + blockWidth;

            walls.Add(new Rectangle(0, 0, xMargin, height));
            walls.Add(new Rectangle(0, 0, width, yMargin));
            walls.Add(new Rectangle(0, yMargin + puzzleHeight, width, yMargin));
            walls.Add(new Rectangle(xMargin + puzzleWidth, 0, xMargin, height));

            for (int i = 0; i < puzzleDimensions.X; i++)
            {
                for (int j = 0; j < puzzleDimensions.Y; j++)
                {
                    walls.Add(new Rectangle(xMargin + lineWidth * (i + 1) + blockWidth * i, yMargin + lineWidth * (j + 1) + blockWidth * j, blockWidth, blockWidth));
                }
            }


            line = new SolutionLine(new Point(xMargin, yMargin), lineWidth);
            //lineMir = new SolutionLine(new Point(300, 100));
        }

        // Returns a coef k. Total free space in pixels * k = puzzle size in pixels
        private float PuzzleSpaceRatio(float puzzleDimension) => (float) (-0.0005 * Math.Pow(puzzleDimension, 4) + 0.0082 * Math.Pow(puzzleDimension, 3) - 0.0439 * Math.Pow(puzzleDimension, 2) + 0.1011 * puzzleDimension + 0.6875);
        // Returns a coef k. Puzzle size in pixels * k = block size in pixels
        private float BlockSizeRatio(float puzzleDimension) => (float) (0.8563 * Math.Pow(puzzleDimension, -1.134));
        // Returns a coef k. Puzzle size in pixels * k = line width in pixels
        private float LineSizeRatio(float puzzleDimension) => -0.0064f * puzzleDimension + 0.0859f;

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

            FullscreenToggleCheck();

            // TODO: Add your update logic here
            var hitboxes = line.Hitboxes.Concat(walls);

            if (Keyboard.GetState().IsKeyDown(Keys.Right))
            {
                line.Move(new Vector2(1, 0), hitboxes);
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Down))
            {
                line.Move(new Vector2(0, 1), hitboxes);
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Left))
            {
                line.Move(new Vector2(-1, 0), hitboxes);
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Up))
            {
                line.Move(new Vector2(0, -1), hitboxes);
            }

            base.Update(gameTime);
        }

        private void FullscreenToggleCheck()
        {
            if (fullscreenCooldown > 0)
                fullscreenCooldown--;

            if ((Keyboard.GetState().IsKeyDown(Keys.LeftAlt) || Keyboard.GetState().IsKeyDown(Keys.RightAlt)) && Keyboard.GetState().IsKeyDown(Keys.Enter))
            {
                graphics.ToggleFullScreen();
                graphics.ApplyChanges();
                fullscreenCooldown = fullscreenCooldownMax;
            }
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
                spriteBatch.Draw(t, hitbox, Color.Cyan);

            foreach (var wall in walls)
            {
                spriteBatch.Draw(t, wall, Color.DarkSlateGray);
            }

            //DrawLine(spriteBatch, //draw line
            //    new Vector2(200, 200), //start of line
            //    new Vector2(100, 50) //end of line
            //);
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
