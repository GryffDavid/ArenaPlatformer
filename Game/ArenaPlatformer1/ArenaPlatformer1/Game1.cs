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
    enum GameState { MainMenu, ModeSelect, Playing, LevelCreator };

    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        GameState GameState;

        //Player Player;
        
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

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = 1920,
                PreferredBackBufferHeight = 1080
            };

            Content.RootDirectory = "Content";
        }
        
        protected override void Initialize()
        {
            GameState = GameState.MainMenu;

            //GamePadState[] gamePad = new GamePadState[4];

            //for (int i = 0; i < 4; i++)
            //{
            //    gamePad[i] = GamePad.GetState((PlayerIndex)i);

            //    if (gamePad[i].IsConnected == true)
            //    {
            //        Players[i] = new Player((PlayerIndex)i);
            //        Players[i].LoadContent(Content);
            //    }
            //}

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
                case GameState.MainMenu:
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            PlayerJoinButtons[i].Update(gameTime);

                            #region Player joined
                            if (CurrentGamePadStates[i].IsButtonUp(Buttons.A) &&
                                PreviousGamePadStates[i].IsButtonDown(Buttons.A))
                            {
                                if (PlayerJoinButtons[i].Occupied == true &&
                                    PlayerJoinButtons.Count(Button => Button.Occupied) > 1)
                                {
                                    GameState = GameState.ModeSelect;
                                }

                                PlayerJoinButtons[i].Occupied = true;

                                Players[i] = new Player((PlayerIndex)i);
                                Players[i].LoadContent(Content);
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

                case GameState.Playing:
                    {
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
                    }
                    break;
            }

            PreviousKeyboardState = CurrentKeyboardState;

            for (int i = 0; i < 4; i++)
            {
                PreviousGamePadStates[i] = CurrentGamePadStates[i];
            }

            base.Update(gameTime);
        }
        
        protected override void Draw(GameTime gameTime)
        {
            switch (GameState)
            {
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

                case GameState.ModeSelect:
                    {
                        GraphicsDevice.SetRenderTarget(MenuRenderTarget);
                        GraphicsDevice.Clear(Color.Black);
                        spriteBatch.Begin();
                        spriteBatch.DrawString(Font1, "Mode Select", new Vector2(32, 32), Color.White);

                        spriteBatch.End();
                    }
                    break;

                case GameState.Playing:
                    {
                        GraphicsDevice.SetRenderTarget(GameRenderTarget);
                        GraphicsDevice.Clear(Color.CornflowerBlue);

                        spriteBatch.Begin();

                        foreach (Player player in Players.Where(Player => Player != null))
                        {
                            player.Draw(spriteBatch);
                        }

                        CurrentMap.Draw(spriteBatch);

                        spriteBatch.End();
                    }
                    break;

                case GameState.LevelCreator:
                    {

                    }
                    break;
            }

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

            //Draw to the backbuffer
            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.Black);
            spriteBatch.Begin();

            if (GameState != GameState.Playing)
            {
                spriteBatch.Draw(MenuRenderTarget, MenuRenderTarget.Bounds, Color.White);
            }
            else
            {
                spriteBatch.Draw(GameRenderTarget, GameRenderTarget.Bounds, Color.White);
                spriteBatch.Draw(UIRenderTarget, UIRenderTarget.Bounds, Color.White);
            }

            spriteBatch.End();

            
            base.Draw(gameTime);
        }        
    }
}
