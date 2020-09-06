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
    public enum ChangeMessageType
    {
        UpdateParticle,
        DeleteRenderData,
    }

    enum GameState { MainMenu, ModeSelect, Playing, LevelCreator };

    public delegate void PlayerShootHappenedEventHandler(object source, PlayerShootEventArgs e);
    public class PlayerShootEventArgs : EventArgs
    {
        public Player Player { get; set; }
        public Vector2 Velocity;
    }

    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        GameState GameState;
        
        RenderTarget2D UIRenderTarget, GameRenderTarget, MenuRenderTarget;
        BasicEffect BasicEffect;

        bool DrawDiagnostics = false;

        KeyboardState CurrentKeyboardState, PreviousKeyboardState;
        SpriteFont Font1;

        Map CurrentMap;

        Player[] Players = new Player[4];
        PlayerJoin[] PlayerJoinButtons = new PlayerJoin[4];

        //Specifically for menu interactions before the Player objects have been created
        GamePadState[] CurrentGamePadStates = new GamePadState[4];
        GamePadState[] PreviousGamePadStates = new GamePadState[4];

        List<Projectile> ProjectileList = new List<Projectile>();

        Vector2 PlaceTilePosition = new Vector2(64, 64);

        Texture2D Block;


        DoubleBuffer DoubleBuffer;
        RenderManager RenderManager;
        UpdateManager UpdateManager;

        static Random Random = new Random();

        List<Emitter> EmitterList = new List<Emitter>();


        public void OnPlayerShoot(object source, PlayerShootEventArgs e)
        {
            ProjectileList.Add(new Bullet()
            {
                Position = e.Player.Position,
                PlayerIndex = e.Player.PlayerIndex,
                Velocity = e.Velocity
            });
        }


        public Game1()
        {
            graphics = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = 1920,
                PreferredBackBufferHeight = 1080
            };

            Content.RootDirectory = "Content";

            graphics.SynchronizeWithVerticalRetrace = true;
            this.IsFixedTimeStep = false;
        }
        
        protected override void Initialize()
        {
            GameState = GameState.MainMenu;

            base.Initialize();
        }

        protected void LoadGameContent()
        {

        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);



     

            BasicEffect = new BasicEffect(GraphicsDevice)
            {
                VertexColorEnabled = true,
                Projection = Matrix.CreateOrthographicOffCenter(0, 1920, 1080, 0, 0, 1)
            };

            UIRenderTarget = new RenderTarget2D(GraphicsDevice, 1920, 1080);
            GameRenderTarget = new RenderTarget2D(GraphicsDevice, 1920, 1080);
            MenuRenderTarget = new RenderTarget2D(GraphicsDevice, 1920, 1080);

            Font1 = Content.Load<SpriteFont>("Font1");

            CurrentMap = new Map();
            CurrentMap.LoadContent(Content);

            Texture2D ButtonTexture = Content.Load<Texture2D>("Blank");

            for (int i = 0; i < 4; i++)
            {
                PlayerJoinButtons[i] = new PlayerJoin(ButtonTexture, new Vector2(106 + (451 * i), 278), new Vector2(356, 524)); 
            }

            Player.Map = CurrentMap;
            Projectile.Map = CurrentMap;

            Rocket.Texture = Content.Load<Texture2D>("RocketTexture");
            Bullet.Texture = Content.Load<Texture2D>("BulletTexture");

            Block = Content.Load<Texture2D>("Blank");

            ProjectileList.Add(new Rocket() { Position = new Vector2(80, 80), Velocity = new Vector2(1, 0) });
        }
        
        protected override void UnloadContent()
        {

        }
        
        protected override void Update(GameTime gameTime)
        {
            CurrentKeyboardState = Keyboard.GetState();

            for (int i = 0; i < 4; i++)
            {
                CurrentGamePadStates[i] = GamePad.GetState((PlayerIndex)i);
            }

            switch (GameState)
            {
                #region MainMenu
                case GameState.MainMenu:
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            PlayerJoinButtons[i].Update(gameTime);

                            #region Player joined
                            if (CurrentGamePadStates[i].IsButtonUp(Buttons.A) &&
                                PreviousGamePadStates[i].IsButtonDown(Buttons.A))
                            {
                                //if (PlayerJoinButtons[i].Occupied == true &&
                                //    PlayerJoinButtons.Count(Button => Button.Occupied) > 1)
                                //{
                                //    GameState = GameState.ModeSelect;
                                //}

                                if (PlayerJoinButtons[i].Occupied == true)
                                {
                                    DoubleBuffer = new DoubleBuffer();
                                    RenderManager = new RenderManager(DoubleBuffer);
                                    RenderManager.LoadContent(Content);

                                    UpdateManager = new UpdateManager(DoubleBuffer);

                                    UpdateManager.StartOnNewThread();

                                    Emitter.UpdateManager = UpdateManager;
                                    Emitter.RenderManager = RenderManager;

                                    Texture2D ParticleTexture = Content.Load<Texture2D>("Particles/diamond");

                                    Emitter newEmitter4 = new Emitter(ParticleTexture, new Vector2(800, 200), new Vector2(-40, 40), new Vector2(6, 10),
                                            new Vector2(1000, 1000), 0.99f, true, new Vector2(0, 360), new Vector2(-3, 3), new Vector2(0.25f, 0.5f),
                                            new Color(Color.Orange.R, Color.Orange.G, Color.Orange.B, 100),
                                            new Color(Color.OrangeRed.R, Color.OrangeRed.G, Color.OrangeRed.B, 20),
                                            0.03f, -2f, 60, 1, false, new Vector2(1080, 1080), false,
                                            null, true, true, new Vector2(0, 0), new Vector2(0, 0), 0, true, new Vector2(0, 0), true, true, 2000, null, null, false);

                                    EmitterList.Add(newEmitter4);

                                    Emitter newEmitter5 = new Emitter(ParticleTexture, new Vector2(800, 200), new Vector2(-40, 40), new Vector2(6, 10),
                                            new Vector2(1000, 1000), 0.99f, true, new Vector2(0, 360), new Vector2(-3, 3), new Vector2(0.25f, 0.5f),
                                            new Color(Color.Orange.R, Color.Orange.G, Color.Orange.B, 35),
                                            new Color(Color.OrangeRed.R, Color.OrangeRed.G, Color.OrangeRed.B, 5),
                                            -0.008f, -2f, 150, 2, false, new Vector2(1080, 1080), true,
                                            null, true, true, new Vector2(0, 0), new Vector2(0, 0), 0, true, new Vector2(0, 0), true, true, 1500, null, null, false);

                                    EmitterList.Add(newEmitter5);


                                    GameState = GameState.Playing;


                                }

                                PlayerJoinButtons[i].Occupied = true;

                                Players[i] = new Player((PlayerIndex)i);
                                Players[i].LoadContent(Content);

                                Players[i].PlayerShootHappened += OnPlayerShoot;
                            }
                            #endregion

                            #region Player backed out
                            if (CurrentGamePadStates[i].IsButtonUp(Buttons.B) &&
                                PreviousGamePadStates[i].IsButtonDown(Buttons.B))
                            {
                                PlayerJoinButtons[i].Occupied = false;
                                Players[i] = null;
                            }
                            #endregion
                        }

                        //If all 4 players have joined, move to the next menu without waiting for a button press
                        //No need to wait because all slots are full
                        if (PlayerJoinButtons.All(Button => Button.Occupied == true))
                        {
                            GameState = GameState.ModeSelect;
                        }
                    }
                    break;
                #endregion

                #region ModeSelect
                case GameState.ModeSelect:
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            if (CurrentGamePadStates[i].IsButtonUp(Buttons.B) &&
                                PreviousGamePadStates[i].IsButtonDown(Buttons.B))
                            {
                                GameState = GameState.MainMenu;
                            }

                            if (CurrentGamePadStates[i].IsButtonUp(Buttons.Start) &&
                                PreviousGamePadStates[i].IsButtonDown(Buttons.Start))
                            {
                                GameState = GameState.Playing;
                            }
                        }
                    }
                    break;
                #endregion

                #region LevelCreator
                case GameState.LevelCreator:
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            if (CurrentGamePadStates[i].IsButtonUp(Buttons.Back) &&
                                PreviousGamePadStates[i].IsButtonDown(Buttons.Back))
                            {
                                GameState = GameState.Playing;
                            }

                            if (CurrentGamePadStates[i].ThumbSticks.Left.Y > 0 &&
                                PreviousGamePadStates[i].ThumbSticks.Left.Y <= 0)
                            {
                                PlaceTilePosition.Y -= 64;
                            }

                            if (CurrentGamePadStates[i].ThumbSticks.Left.Y < 0 &&
                                PreviousGamePadStates[i].ThumbSticks.Left.Y >= 0)
                            {
                                PlaceTilePosition.Y += 64;
                            }

                            if (CurrentGamePadStates[i].ThumbSticks.Left.X > 0 &&
                                PreviousGamePadStates[i].ThumbSticks.Left.X <= 0)
                            {
                                PlaceTilePosition.X += 64;
                            }

                            if (CurrentGamePadStates[i].ThumbSticks.Left.X < 0 &&
                                PreviousGamePadStates[i].ThumbSticks.Left.X >= 0)
                            {
                                PlaceTilePosition.X -= 64;
                            }

                            if (CurrentGamePadStates[i].IsButtonUp(Buttons.A) &&
                                PreviousGamePadStates[i].IsButtonDown(Buttons.A))
                            {
                                Tile tile = new Tile()
                                {
                                    Position = PlaceTilePosition,
                                    Color = Color.Purple,
                                    Size = new Vector2(64, 64),
                                    TileType = TileType.Solid
                                };

                                tile.LoadContent(Content);

                                CurrentMap.TileList.Add(tile);
                            }
                        }


                    }
                    break;
                #endregion

                #region Playing
                case GameState.Playing:
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            if (CurrentGamePadStates[i].IsButtonUp(Buttons.Back) &&
                                PreviousGamePadStates[i].IsButtonDown(Buttons.Back))
                            {
                                GameState = GameState.LevelCreator;
                            }
                        }

                        #region Turn on diagnostics with F3
                        if (CurrentKeyboardState.IsKeyUp(Keys.F3) &&
                            PreviousKeyboardState.IsKeyDown(Keys.F3))
                        {
                            DrawDiagnostics = !DrawDiagnostics;
                        }
                        #endregion

                        foreach (Player player in Players.Where(Player => Player != null))
                        {
                            player.Update(gameTime);
                        }

                        foreach (Projectile projectile in ProjectileList)
                        {
                            projectile.Update(gameTime);
                        }

                        EmitterList[0].Position = new Vector2(Random.Next(-200, -50), Random.Next(1080 / 2, 1080));
                        EmitterList[1].Position = new Vector2(Random.Next(-200, -50), Random.Next(1080 / 2, 1080));

                        foreach (Emitter emitter in EmitterList)
                        {
                            emitter.Update(gameTime);
                        }
                    }
                    break;
                    #endregion
            }

            for (int i = 0; i < 4; i++)
            {
                PreviousGamePadStates[i] = CurrentGamePadStates[i];
            }

            PreviousKeyboardState = CurrentKeyboardState;


            base.Update(gameTime);
        }
        
        protected override void Draw(GameTime gameTime)
        {
            switch (GameState)
            {
                #region Main Menu
                case GameState.MainMenu:
                    {
                        GraphicsDevice.SetRenderTarget(MenuRenderTarget);
                        GraphicsDevice.Clear(Color.CornflowerBlue);
                        spriteBatch.Begin();
                        spriteBatch.DrawString(Font1, "Main Menu", new Vector2(32, 32), Color.White);

                        foreach (PlayerJoin joinButton in PlayerJoinButtons)
                        {
                            joinButton.Draw(spriteBatch);
                        }
                        spriteBatch.End();
                    }
                    break;
                #endregion

                #region Mode Select
                case GameState.ModeSelect:
                    {
                        GraphicsDevice.SetRenderTarget(MenuRenderTarget);
                        GraphicsDevice.Clear(Color.Black);
                        spriteBatch.Begin();
                        spriteBatch.DrawString(Font1, "Mode Select", new Vector2(32, 32), Color.White);

                        spriteBatch.End();
                    }
                    break;
                #endregion

                #region Playing
                case GameState.Playing:
                    {
                        DoubleBuffer.GlobalStartFrame(gameTime);
                        GraphicsDevice.SetRenderTarget(GameRenderTarget);
                        GraphicsDevice.Clear(Color.Black);

                        spriteBatch.Begin();

                        //Draw the particles
                        RenderManager.DoFrame(spriteBatch);

                        foreach (Projectile projectile in ProjectileList)
                        {
                            projectile.Draw(spriteBatch);
                        }

                        foreach (Player player in Players.Where(Player => Player != null))
                        {
                            player.Draw(spriteBatch);
                        }

                        CurrentMap.Draw(spriteBatch);

                        

                        spriteBatch.End();
                    }
                    break;
                #endregion

                #region Level Creator
                case GameState.LevelCreator:
                    {
                        GraphicsDevice.SetRenderTarget(GameRenderTarget);
                        GraphicsDevice.Clear(Color.CornflowerBlue);

                        spriteBatch.Begin();

                        CurrentMap.Draw(spriteBatch);

                        spriteBatch.Draw(Block, new Rectangle((int)PlaceTilePosition.X, (int)PlaceTilePosition.Y, 64, 64), Color.White * 0.5f);


                        spriteBatch.End();
                    }
                    break; 
                #endregion
            }

            #region Draw UI
            GraphicsDevice.SetRenderTarget(UIRenderTarget);
            GraphicsDevice.Clear(Color.Transparent);
            spriteBatch.Begin();

            if (DrawDiagnostics == true)
            {
                foreach (Player player in Players.Where(Player => Player != null))
                {
                    player.DrawInfo(spriteBatch, GraphicsDevice, BasicEffect);
                }
            }

            spriteBatch.End();
            #endregion
            
            #region Draw to the backbuffer
            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.Black);
            spriteBatch.Begin();

            if (GameState != GameState.Playing && GameState != GameState.LevelCreator)
            {
                spriteBatch.Draw(MenuRenderTarget, MenuRenderTarget.Bounds, Color.White);
            }
            else
            {
                spriteBatch.Draw(GameRenderTarget, GameRenderTarget.Bounds, Color.White);
                spriteBatch.Draw(UIRenderTarget, UIRenderTarget.Bounds, Color.White);
            }

            spriteBatch.End(); 
            #endregion

            base.Draw(gameTime);
        }


        protected override void EndDraw()
        {
            base.EndDraw();

            if (GameState == GameState.Playing)
                DoubleBuffer.GlobalSynchronize();
        }


        protected override void OnExiting(object sender, EventArgs args)
        {
            if (UpdateManager.RunningThread != null)
                UpdateManager.RunningThread.Abort();

            DoubleBuffer.CleanUp();

        }
    }
}
