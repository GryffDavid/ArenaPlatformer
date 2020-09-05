using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace ArenaPlatformer1
{    
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        enum GameState { MainMenu, Playing };

        Player Player;

        List<Tile> TileList = new List<Tile>();

        RenderTarget2D UIRenderTarget, GameRenderTarget;
        BasicEffect BasicEffect;

        bool DrawDiagnostics = false;

        KeyboardState CurrentKeyboardState, PreviousKeyboardState;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 1920;
            graphics.PreferredBackBufferHeight = 1080;
            Content.RootDirectory = "Content";
        }
        
        protected override void Initialize()
        {
            //Bottom
            for (int i = 0; i < 60; i++)
            {
                Tile tile = new Tile()
                {
                    Position = new Vector2(i * 32, 1048)
                };

                TileList.Add(tile);
            }

            //Top
            for (int i = 0; i < 60; i++)
            {
                Tile tile = new Tile()
                {
                    Position = new Vector2(i * 32, 0)
                };

                TileList.Add(tile);
            }

            //Left
            for (int i = 0; i < 34; i++)
            {
                Tile tile = new Tile()
                {
                    Position = new Vector2(0, 32 * i)
                };

                TileList.Add(tile);
            }

            //Right
            for (int i = 0; i < 34; i++)
            {
                Tile tile = new Tile()
                {
                    Position = new Vector2(1888, 32 * i)
                };

                TileList.Add(tile);
            }



            //Platform
            for (int i = 16; i < 28; i++)
            {
                Tile tile = new Tile()
                {
                    Position = new Vector2(32 * i, 800)
                };

                TileList.Add(tile);
            }

            //Platform
            for (int i = 6; i < 21; i++)
            {
                Tile tile = new Tile()
                {
                    Position = new Vector2(32 * i, 900)
                };

                TileList.Add(tile);
            }

            Player = new Player(PlayerIndex.One, TileList);


            base.Initialize();
        }
        
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            BasicEffect = new BasicEffect(GraphicsDevice);
            BasicEffect.VertexColorEnabled = true;
            BasicEffect.Projection = Matrix.CreateOrthographicOffCenter(0, 1920, 1080, 0, 0, 1);

            UIRenderTarget = new RenderTarget2D(GraphicsDevice, 1920, 1080);
            GameRenderTarget = new RenderTarget2D(GraphicsDevice, 1920, 1080);

            Player.LoadContent(Content);

            foreach (Tile tile in TileList)
            {
                tile.LoadContent(Content);
            }
        }
        
        protected override void UnloadContent()
        {

        }
        
        protected override void Update(GameTime gameTime)
        {
            CurrentKeyboardState = Keyboard.GetState();


            #region Turn on diagnostics with F3
            if (CurrentKeyboardState.IsKeyUp(Keys.F3) &&
                    PreviousKeyboardState.IsKeyDown(Keys.F3))
            {
                DrawDiagnostics = !DrawDiagnostics;
            }
            #endregion

            
            Player.Update(gameTime);

            PreviousKeyboardState = CurrentKeyboardState;
            base.Update(gameTime);
        }
        
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.SetRenderTarget(GameRenderTarget);
            GraphicsDevice.Clear(Color.CornflowerBlue);
            spriteBatch.Begin();
            Player.Draw(spriteBatch);

            foreach (Tile tile in TileList)
            {
                tile.Draw(spriteBatch);
            }

            spriteBatch.End();

            GraphicsDevice.SetRenderTarget(UIRenderTarget);
            GraphicsDevice.Clear(Color.Transparent);
            spriteBatch.Begin();

            if (DrawDiagnostics == true)
                Player.DrawInfo(spriteBatch, GraphicsDevice, BasicEffect);

            spriteBatch.End();

            //Draw to the backbuffer
            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.Black);
            spriteBatch.Begin();

            spriteBatch.Draw(GameRenderTarget, GameRenderTarget.Bounds, Color.White);
            spriteBatch.Draw(UIRenderTarget, UIRenderTarget.Bounds, Color.White);

            spriteBatch.End();

            
            base.Draw(gameTime);
        }
    }
}
