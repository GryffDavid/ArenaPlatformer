﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;

namespace ArenaPlatformer1
{
    enum Facing { Left, Right };
    enum Pose { Standing, Crouching };

    public class Player
    {
        #region Events
        public event PlayerShootHappenedEventHandler PlayerShootHappened;
        public void CreatePlayerShoot(Vector2 velocity)
        {
            OnPlayerShootHappened(velocity);
        }
        protected virtual void OnPlayerShootHappened(Vector2 velocity)
        {
            PlayerShootHappened?.Invoke(this,
                new PlayerShootEventArgs()
                {
                    Player = this,
                    Velocity = velocity
                });
        }

        public event PlayerGrenadeHappenedEventHandler PlayerGrenadeHappened;
        public void CreatePlayerGrenade()
        {
            OnPlayerGrenadeHappened();
        }
        protected virtual void OnPlayerGrenadeHappened()
        {
            PlayerGrenadeHappened?.Invoke(this,
                new PlayerGrenadeEventArgs()
                {
                    Player = this
                });
        }

        public event PlaceTrapHappenedEventHandler PlaceTrapHappened;
        public void CreatePlaceTrap(Vector2 position, TrapType trapType)
        {
            OnPlaceTrapHappened(position, trapType);
        }
        protected virtual void OnPlaceTrapHappened(Vector2 position, TrapType trapType)
        {
            PlaceTrapHappened?.Invoke(this,
                new PlaceTrapEventArgs()
                {
                    Player = this,
                    Position = position,
                    TrapType = trapType
                });
        }

        public event PlayerDiedHappenedEventHandler PlayerDiedHappened;
        public void CreatePlayerDied()
        {
            OnPlayerDied();
        }
        protected virtual void OnPlayerDied()
        {
            PlayerDiedHappened?.Invoke(this,
                new PlayerDiedEventArgs()
                {
                    Player = this
                });
        } 
        #endregion

        public Animation RunRightAnimation, RunRightUpAnimation, RunRightDownAnimation,
                         RunLeftAnimation, RunLeftUpAnimation, RunLeftDownAnimation,
                         StandRightAnimation, StandRightUpAnimation, StandRightDownAnimation,
                         StandLeftAnimation, StandLeftUpAnimation, StandLeftDownAnimation,
                         JumpRightAnimation, JumpRightUpAnimation, JumpRightDownAnimation,
                         JumpLeftAnimation, JumpLeftUpAnimation, JumpLeftDownAnimation,
                         CrouchRightAnimation, CrouchLeftAnimation;

        public Animation CurrentAnimation;

        public Texture2D RunRightTexture, RunRightUpTexture, RunRightDownTexture,
                         RunLeftTexture, RunLeftUpTexture, RunLeftDownTexture,
                         StandRightTexture, StandRightUpTexture, StandRightDownTexture,
                         StandLeftTexture, StandLeftUpTexture, StandLeftDownTexture,
                         JumpRightTexture, JumpRightUpTexture, JumpRightDownTexture,
                         JumpLeftTexture, JumpLeftUpTexture, JumpLeftDownTexture,
                         CrouchRightTexture, CrouchLeftTexture,
                         HeadTexture;

        bool Active = true;
        bool InAir;
        bool DoubleJumped = false;
        public Texture2D Texture;
        public Vector2 Position, PrevPosition, Velocity, MoveStick, AimStick, RumbleValues, AimDirection;
        Vector2 MaxSpeed;
        Vector2 CurrentFriction = new Vector2(0.9999f, 1f);
        public GamePadState CurrentGamePadState, PreviousGamePadState;
        public KeyboardState CurrentKeyboardState, PreviousKeyboardState;
        public MouseState CurrentMouseState, PreviousMouseState;
        public Rectangle DestinationRectangle, CollisionRectangle;
        public float Gravity;
        public PlayerIndex PlayerIndex;

        //Current health, Max health
        public Vector2 Health = new Vector2(100, 100);

        GamePadThumbSticks Sticks;
        Buttons JumpButton, ShootButton, GrenadeButton, TrapButton;

        Facing CurrentFacing = Facing.Right;
        Facing PreviousFacing = Facing.Right;

        Pose CurrentPose = Pose.Standing;
        Pose PreviousPose = Pose.Standing;

        public GunType CurrentGun;
        public TrapType CurrentTrap;

        float RumbleTime, MaxRumbleTime;

        public int Deaths = 0;
        public int GunAmmo = 15;
        public int TrapAmmo = 0;
        public int GrenadeAmmo = 0;
        
        public static Map Map;
        public static List<Item> ItemList;
        public static List<Trap> TrapList;
                
        public Player(PlayerIndex playerIndex)
        {
            PlayerIndex = playerIndex;
            Position = new Vector2(500, 500);
            MaxSpeed = new Vector2(5f, 6);
            Gravity = 0.6f;            
        }

        public void Initialize()
        {

        }

        public void LoadContent(ContentManager content)
        {
            Texture = content.Load<Texture2D>("Blank");

            JumpButton = Buttons.A;
            ShootButton = Buttons.X;
            GrenadeButton = Buttons.B;
            TrapButton = Buttons.Y;
            
            RunRightTexture = content.Load<Texture2D>("Player" + ((int)PlayerIndex + 1) + "/Running/RunRight");
            RunRightUpTexture = content.Load<Texture2D>("Player" + ((int)PlayerIndex + 1) + "/Running/RunRightUp");
            RunRightDownTexture = content.Load<Texture2D>("Player" + ((int)PlayerIndex + 1) + "/Running/RunRightDown");

            RunLeftTexture = content.Load<Texture2D>("Player" + ((int)PlayerIndex + 1) + "/Running/RunLeft");
            RunLeftUpTexture = content.Load<Texture2D>("Player" + ((int)PlayerIndex + 1) + "/Running/RunLeftUp");
            RunLeftDownTexture = content.Load<Texture2D>("Player" + ((int)PlayerIndex + 1) + "/Running/RunLeftDown");

            StandRightTexture = content.Load<Texture2D>("Player" + ((int)PlayerIndex + 1) + "/Standing/StandRight");
            StandRightUpTexture = content.Load<Texture2D>("Player" + ((int)PlayerIndex + 1) + "/Standing/StandRightUp");
            StandRightDownTexture = content.Load<Texture2D>("Player" + ((int)PlayerIndex + 1) + "/Standing/StandRightDown");

            StandLeftTexture = content.Load<Texture2D>("Player" + ((int)PlayerIndex + 1) + "/Standing/StandLeft");
            StandLeftUpTexture = content.Load<Texture2D>("Player" + ((int)PlayerIndex + 1) + "/Standing/StandLeftUp");
            StandLeftDownTexture = content.Load<Texture2D>("Player" + ((int)PlayerIndex + 1) + "/Standing/StandLeftDown");

            JumpRightTexture = content.Load<Texture2D>("Player" + ((int)PlayerIndex + 1) + "/Jumping/JumpRight");
            JumpRightUpTexture = content.Load<Texture2D>("Player" + ((int)PlayerIndex + 1) + "/Jumping/JumpRightUp");
            JumpRightDownTexture = content.Load<Texture2D>("Player" + ((int)PlayerIndex + 1) + "/Jumping/JumpRightDown");

            JumpLeftTexture = content.Load<Texture2D>("Player" + ((int)PlayerIndex + 1) + "/Jumping/JumpLeft");
            JumpLeftUpTexture = content.Load<Texture2D>("Player" + ((int)PlayerIndex + 1) + "/Jumping/JumpLeftUp");
            JumpLeftDownTexture = content.Load<Texture2D>("Player" + ((int)PlayerIndex + 1) + "/Jumping/JumpLeftDown");

            CrouchRightTexture = content.Load<Texture2D>("Player" + ((int)PlayerIndex + 1) + "/CrouchRight");
            CrouchLeftTexture = content.Load<Texture2D>("Player" + ((int)PlayerIndex + 1) + "/CrouchLeft");

            RunRightAnimation = new Animation(RunRightTexture, 8, 50);
            RunRightUpAnimation = new Animation(RunRightUpTexture, 8, 50);
            RunRightDownAnimation = new Animation(RunRightDownTexture, 8, 50);

            RunLeftAnimation = new Animation(RunLeftTexture, 8, 50);
            RunLeftUpAnimation = new Animation(RunLeftUpTexture, 8, 50);
            RunLeftDownAnimation = new Animation(RunLeftDownTexture, 8, 50);

            StandLeftAnimation = new Animation(StandLeftTexture, 1, 50);
            StandLeftUpAnimation = new Animation(StandLeftUpTexture, 1, 50);
            StandLeftDownAnimation = new Animation(StandLeftDownTexture, 1, 50);

            StandRightAnimation = new Animation(StandRightTexture, 1, 50);
            StandRightUpAnimation = new Animation(StandRightUpTexture, 1, 50);
            StandRightDownAnimation = new Animation(StandRightDownTexture, 1, 50);

            JumpLeftAnimation = new Animation(JumpLeftTexture, 1, 50);
            JumpLeftUpAnimation = new Animation(JumpLeftUpTexture, 1, 50);
            JumpLeftDownAnimation = new Animation(JumpLeftDownTexture, 1, 50);

            JumpRightAnimation = new Animation(JumpRightTexture, 1, 50);
            JumpRightUpAnimation = new Animation(JumpRightUpTexture, 1, 50);
            JumpRightDownAnimation = new Animation(JumpRightDownTexture, 1, 50);

            CrouchRightAnimation = new Animation(CrouchRightTexture, 1, 50);
            CrouchLeftAnimation = new Animation(CrouchLeftTexture, 1, 50);

            CurrentAnimation = StandRightAnimation;
            
            //DestinationRectangle = new Rectangle((int)Position.X, (int)Position.Y, Texture.Width, Texture.Height);
            //CollisionRectangle = new Rectangle((int)(Position.X - 30), (int)(Position.Y - 90 + Velocity.Y), 60, 90);
        }

        public void Update(GameTime gameTime)
        {
            CurrentGamePadState = GamePad.GetState(PlayerIndex);
            CurrentKeyboardState = Keyboard.GetState();
            CurrentMouseState = Mouse.GetState();

            Sticks = CurrentGamePadState.ThumbSticks;
            MoveStick = Sticks.Left;
            AimStick = Sticks.Right;

            Velocity.Y += Gravity * ((float)gameTime.ElapsedGameTime.TotalSeconds * 60f);

            bool leftCol, rightCol;
            bool upCol, downCol;
            Vector2 lPos, rPos;

            //Ceiling, Ground
            //float cPos, gPos;


            #region Move stick left
            if (MoveStick.X < 0f)
            {
                AimDirection.X = -1f;
                CurrentFacing = Facing.Left;

                Velocity.X += (MoveStick.X * 3f);
            }
            #endregion

            #region Move stick right
            if (MoveStick.X > 0f)
            {
                AimDirection.X = 1f;
                CurrentFacing = Facing.Right;

                Velocity.X += (MoveStick.X * 3f);
            }
            #endregion




            leftCol = CheckLeft(out lPos);
            rightCol = CheckRight(out rPos);
            
            downCol = OnGround(Velocity, Position, out float gPos);

            if (Velocity.X < 0)
            {
                if (leftCol == true)
                {
                    Velocity.X = 0;
                    Position.X = lPos.X + 64 + (CollisionRectangle.Width / 2) + 1;
                }
            }

            if (Velocity.X > 0)
            {
                if (rightCol == true)
                {
                    Velocity.X = 0;
                    Position.X = rPos.X - (CollisionRectangle.Width / 2) - 1;
                }
            }

            if (Velocity.Y >= 0)
            {
                if (downCol == true)
                {
                    InAir = false;
                    DoubleJumped = false;
                    Velocity.Y = 0;
                    Position.Y = gPos - (CollisionRectangle.Height / 2);
                }
                else
                {
                    InAir = true;
                }
            }

           
            if (CurrentGamePadState.IsButtonDown(JumpButton) &&
                PreviousGamePadState.IsButtonUp(JumpButton) &&
                Velocity.Y >= 0 &&
                DoubleJumped == false)
            {
                if (InAir == true)
                {
                    DoubleJumped = true;
                    Velocity.Y -= 15f;
                }
                else
                {
                    Velocity.Y = -15f;
                }                
            }

            upCol = OnCeiling(Velocity, Position, out float cPos);

            if (Velocity.Y <= 0)
            {
                if (upCol == true)
                {
                    Velocity.Y = 0;
                    Position.Y = cPos + 64 + (CollisionRectangle.Height / 2) + 1;
                }
            }

            #region Move stick down
            if (MoveStick.Y < -0.75f)
            {
                Velocity.X = 0;
                CurrentPose = Pose.Crouching;

                if (PreviousPose == Pose.Standing)
                {
                    Position.Y += 12;
                }
            }
            else
            {
                if (PreviousPose == Pose.Crouching)
                {
                    Position.Y -= 12;
                }

                CurrentPose = Pose.Standing;
            }
            #endregion


            //Handle horizontal control+movement

            #region Stop Moving
            if (MoveStick.X == 0)
            {
                if (InAir == false)
                    Velocity.X *= 0.85f;
                else
                    Velocity.X *= 0.95f;
            }
            #endregion

            #region Limit Speed
            if (Velocity.X > MaxSpeed.X)
            {
                Velocity.X = MaxSpeed.X;
            }

            if (Velocity.X < -MaxSpeed.X)
            {
                Velocity.X = -MaxSpeed.X;
            }

            //if (Velocity.Y < -25)
            //{
            //    Velocity.Y = -25;
            //}

            //if (Velocity.Y > 25)
            //{
            //    Velocity.Y = 25;
            //}
            #endregion

            #region Shoot
            if (CurrentGamePadState.IsButtonDown(ShootButton) &&
                PreviousGamePadState.IsButtonUp(ShootButton))
            {
                if (GunAmmo > 0)
                {
                    switch (CurrentFacing)
                    {
                        case Facing.Left:
                            CreatePlayerShoot(new Vector2(-35, 0));
                            break;

                        case Facing.Right:
                            CreatePlayerShoot(new Vector2(35, 0));
                            break;
                    }

                    GunAmmo--;
                }
            }
            #endregion

            #region Grenade
            if (CurrentGamePadState.IsButtonDown(GrenadeButton) &&
                        PreviousGamePadState.IsButtonUp(GrenadeButton))
            {
                //Create grenades!
                CreatePlayerGrenade();
            }
            #endregion

            #region Trap
            if (CurrentGamePadState.IsButtonDown(TrapButton) &&
                PreviousGamePadState.IsButtonUp(TrapButton))
            {
                if (TrapAmmo > 0)
                {
                    //Create traps!
                    CreatePlaceTrap(Position, TrapType.Mine);
                    TrapAmmo--;
                }
            }
            #endregion

            if (Velocity.X != 0)
            {
                if (InAir == false)
                {
                    switch (CurrentFacing)
                    {
                        case Facing.Left:
                            CurrentAnimation = RunLeftAnimation;
                            break;

                        case Facing.Right:
                            CurrentAnimation = RunRightAnimation;
                            break;
                    }
                }
            }

            #region Player has stopped moving - Display Stand/Crouch animation
            if (Velocity.X > -2f &&
                Velocity.X < 2f)
            {
                switch (CurrentFacing)
                {
                    case Facing.Left:
                        if (CurrentPose == Pose.Standing)
                            CurrentAnimation = StandLeftAnimation;
                        else
                            CurrentAnimation = CrouchLeftAnimation;
                        break;

                    case Facing.Right:
                        if (CurrentPose == Pose.Standing)
                            CurrentAnimation = StandRightAnimation;
                        else
                            CurrentAnimation = CrouchRightAnimation;
                        break;
                }
            }
            #endregion


            //Handle Collisions 

            if (upCol== false &&
                downCol == false)
            {
                Position.Y += Velocity.Y * ((float)gameTime.ElapsedGameTime.TotalSeconds * 60f);
            }

            if ((leftCol == false && Velocity.X < 0) ||
                (rightCol == false && Velocity.X > 0))
            {
                Position.X += Velocity.X * ((float)gameTime.ElapsedGameTime.TotalSeconds * 60f);
            }

            if (Velocity.X <= 0.5f &&
                Velocity.X >= -0.5f)
            {
                Velocity.X = 0f;
            }

       
            DestinationRectangle = new Rectangle((int)(Position.X - CurrentAnimation.FrameSize.X/2), 
                                                 (int)(Position.Y - CurrentAnimation.FrameSize.Y/2), 
                                                 (int)CurrentAnimation.FrameSize.X, 
                                                 (int)CurrentAnimation.FrameSize.Y);

            if (CurrentPose == Pose.Standing)
                CollisionRectangle = new Rectangle((int)(Position.X - 30), (int)(Position.Y - 45), 60, 90);
            else
                CollisionRectangle = new Rectangle((int)(Position.X - 30), (int)(Position.Y - 33), 60, 66);

            if (CurrentAnimation != null)
            {
                CurrentAnimation.Position = Position;
                CurrentAnimation.Update(gameTime, DestinationRectangle);
            }

            switch (CurrentFacing)
            {
                case Facing.Left:
                    {
                        AimDirection = new Vector2(-1, 0);
                    }
                    break;

                case Facing.Right:
                    {
                        AimDirection = new Vector2(1, 0);
                    }
                    break;
            }

            if (ItemList != null)
                ItemList.ForEach(Item =>
                {
                    if (Item.CollisionRectangle.Intersects(CollisionRectangle))
                    {
                        if (Item as TrapPickup != null)
                        {
                            if (TrapAmmo <= 0)
                            {
                                CurrentTrap = (Item as TrapPickup).TrapType;
                            }

                            if (Item as MinePickup != null)
                            {
                                TrapAmmo++;
                            }
                        }

                        if (Item as Gun != null)
                        {
                            if (GunAmmo <= 0)
                            {
                                GunAmmo += 15;
                            }
                        }

                        ItemList.Remove(Item);
                    }
                });

            if (TrapList != null)
                TrapList.ForEach(Trap =>
                {
                    if (Trap.Active == true && Trap.CollisionRectangle.Intersects(CollisionRectangle))
                    {
                        Health.X -= 20;
                        Trap.Reset();
                    }
                });

            if (Health.X <= 0)
            {
                Deaths++;
                CreatePlayerDied();
            }

            PrevPosition = Position;
            PreviousPose = CurrentPose;
            PreviousFacing = CurrentFacing;
            PreviousGamePadState = CurrentGamePadState;
            PreviousKeyboardState = CurrentKeyboardState;
            PreviousMouseState = CurrentMouseState;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            //spriteBatch.Draw(Texture, DestinationRectangle, null, Color.White, 0, new Vector2(Texture.Width / 2, Texture.Height), SpriteEffects.None, 0);

            if (CurrentAnimation != null)
                CurrentAnimation.Draw(spriteBatch, Position);
        }

        /// <summary>
        /// Draw the collision box and other useful debug info
        /// </summary>
        /// <param name="graphics"></param>
        /// <param name="basicEffect"></param>
        public void DrawInfo(SpriteBatch spriteBatch, GraphicsDevice graphics, BasicEffect basicEffect)
        {
            //Mark the Position of the player
            spriteBatch.Draw(Texture, new Rectangle((int)Position.X - 1, (int)Position.Y - 4, 2, 8), Color.Red);

            #region Draw the collision box for debugging
            VertexPositionColorTexture[] Vertices = new VertexPositionColorTexture[4];
            int[] Indices = new int[8];

            Vertices[0] = new VertexPositionColorTexture()
            {
                Color = Color.Red,
                Position = new Vector3(CollisionRectangle.Left, CollisionRectangle.Top, 0),
                TextureCoordinate = new Vector2(0, 0)
            };

            Vertices[1] = new VertexPositionColorTexture()
            {
                Color = Color.Red,
                Position = new Vector3(CollisionRectangle.Right - 1, CollisionRectangle.Top, 0),
                TextureCoordinate = new Vector2(1, 0)
            };

            Vertices[2] = new VertexPositionColorTexture()
            {
                Color = Color.Red,
                Position = new Vector3(CollisionRectangle.Right - 1, CollisionRectangle.Bottom - 1, 0),
                TextureCoordinate = new Vector2(1, 1)
            };

            Vertices[3] = new VertexPositionColorTexture()
            {
                Color = Color.Red,
                Position = new Vector3(CollisionRectangle.Left, CollisionRectangle.Bottom - 1, 0),
                TextureCoordinate = new Vector2(0, 1)
            };

            Indices[0] = 0;
            Indices[1] = 1;

            Indices[2] = 2;
            Indices[3] = 3;

            Indices[4] = 0;

            Indices[5] = 2;
            Indices[6] = 0;

            foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphics.DrawUserIndexedPrimitives(PrimitiveType.LineStrip, Vertices, 0, 4, Indices, 0, 6, VertexPositionColorTexture.VertexDeclaration);
            } 
            #endregion
        }

        public void MakeRumble(float time, Vector2 value)
        {
            GamePad.SetVibration(PlayerIndex, value.X, value.Y);            
            RumbleTime = 0;
            MaxRumbleTime = time;
        }


        public bool CheckLeft(out Vector2 tPos)
        {
            Vector2 bottomLeft = new Vector2(
                Position.X - (CollisionRectangle.Width / 2) + Velocity.X - 1,
                Position.Y + (CollisionRectangle.Height / 2) - 1);

            Vector2 topLeft = new Vector2(
                Position.X - (CollisionRectangle.Width / 2) + Velocity.X - 1, 
                Position.Y - (CollisionRectangle.Height / 2) + 1);

            int tileIndexX, tileIndexY;
            tPos = Vector2.Zero;

            for (var checkedTile = topLeft; ; checkedTile.Y += Map.TileSize.Y)
            {
                checkedTile.Y = Math.Min(checkedTile.Y, bottomLeft.Y);

                tileIndexX = Map.GetMapTileXAtPoint((int)checkedTile.X);
                tileIndexY = Map.GetMapTileYAtPoint((int)checkedTile.Y);

                if (Map.IsObstacle(tileIndexX, tileIndexY))
                {
                    //Map.DrawTiles[tileIndexX, tileIndexY].Color = Color.Red;
                    tPos = Map.DrawTiles[tileIndexX, tileIndexY].Position;
                    return true;
                }

                if (checkedTile.Y >= bottomLeft.Y)
                    break;
            }

            return false;
        }

        public bool CheckRight(out Vector2 tPos)
        {
            Vector2 bottomRight = new Vector2(
                Position.X + (CollisionRectangle.Width / 2) + Velocity.X + 1, 
                Position.Y + (CollisionRectangle.Height / 2) - 1);

            Vector2 topRight = new Vector2(
                Position.X + (CollisionRectangle.Width / 2) + Velocity.X + 1, 
                Position.Y - (CollisionRectangle.Height/2) + 1);

            int tileIndexX, tileIndexY;
            tPos = Vector2.Zero;

            for (var checkedTile = topRight; ; checkedTile.Y += Map.TileSize.Y)
            {
                checkedTile.Y = Math.Min(checkedTile.Y, bottomRight.Y);

                tileIndexX = Map.GetMapTileXAtPoint((int)checkedTile.X);
                tileIndexY = Map.GetMapTileYAtPoint((int)checkedTile.Y);

                if (Map.IsObstacle(tileIndexX, tileIndexY))
                {
                    //Map.DrawTiles[tileIndexX, tileIndexY].Color = Color.Yellow;
                    tPos = Map.DrawTiles[tileIndexX, tileIndexY].Position;
                    return true;
                }

                if (checkedTile.Y >= bottomRight.Y)
                    break;
            }

            return false;
        }

        public bool OnGround(Vector2 velocity, Vector2 position, out float groundY)
        {
            Vector2 bottomLeft = new Vector2(
                Position.X,
                Position.Y + (CollisionRectangle.Height / 2) + Velocity.Y + 1);

            Vector2 bottomRight = new Vector2(
                Position.X + (CollisionRectangle.Width / 2),
                Position.Y + (CollisionRectangle.Height/2) + Velocity.Y + 1);

            int tileIndexX, tileIndexY;

            for (var checkedTile = bottomLeft; ; checkedTile.X += Map.TileSize.X)
            {
                checkedTile.X = Math.Min(checkedTile.X, bottomRight.X);

                tileIndexX = Map.GetMapTileXAtPoint((int)checkedTile.X);
                tileIndexY = Map.GetMapTileYAtPoint((int)checkedTile.Y);

                groundY = (float)tileIndexY * Map.TileSize.Y;

                if (Map.IsGround(tileIndexX, tileIndexY))
                {
                    return true;
                }

                if (Map.IsBounce(tileIndexX, tileIndexY) == true)
                {
                    Velocity.Y = -25f;
                    return false;
                }

                if (checkedTile.X >= bottomRight.X)
                    break;
            }


            bottomLeft = new Vector2(
                Position.X,
                Position.Y + +(CollisionRectangle.Height / 2) + Velocity.Y + 1);

            bottomRight = new Vector2(
                Position.X - (CollisionRectangle.Width / 2),
                Position.Y + +(CollisionRectangle.Height / 2) + Velocity.Y + 1);
            
            for (var checkedTile = bottomLeft; ; checkedTile.X -= Map.TileSize.X)
            {
                checkedTile.X = Math.Min(checkedTile.X, bottomRight.X);

                tileIndexX = Map.GetMapTileXAtPoint((int)checkedTile.X);
                tileIndexY = Map.GetMapTileYAtPoint((int)checkedTile.Y);

                groundY = (float)tileIndexY * Map.TileSize.Y;

                if (Map.IsGround(tileIndexX, tileIndexY))
                {
                    return true;
                }

                if (Map.IsBounce(tileIndexX, tileIndexY) == true)
                {
                    Velocity.Y = -25f;
                    return false;
                }

                if (checkedTile.X <= bottomRight.X)
                    break;
            }

            return false;
        }        

        public bool OnCeiling(Vector2 velocity, Vector2 position, out float ceilingY)
        {
            Vector2 topLeft = new Vector2(
                Position.X - (CollisionRectangle.Width / 2), 
                Position.Y - (CollisionRectangle.Height / 2) + Velocity.Y - 2);

            Vector2 topRight = new Vector2(
                Position.X + (CollisionRectangle.Width / 2),
                Position.Y - (CollisionRectangle.Height / 2) + Velocity.Y - 2);

            int tileIndexX, tileIndexY;

            for (var checkedTile = topLeft; ; checkedTile.X += Map.TileSize.X)
            {
                checkedTile.X = Math.Min(checkedTile.X, topRight.X);

                tileIndexX = Map.GetMapTileXAtPoint((int)checkedTile.X);
                tileIndexY = Map.GetMapTileYAtPoint((int)checkedTile.Y);

                ceilingY = (float)tileIndexY * Map.TileSize.Y;

                if (Map.IsObstacle(tileIndexX, tileIndexY))
                    return true;

                if (checkedTile.X >= topRight.X)
                    break;
            }

            return false;
        }
    }
}
