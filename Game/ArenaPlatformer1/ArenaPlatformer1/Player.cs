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

    public class Player : MovingObject
    {
        public PlayerIndex PlayerIndex;
        public static Random Random = new Random();

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

        #region Shared Static
        public static List<Item> ItemList;
        public static List<Trap> TrapList;
        #endregion

        #region Animations
        public Animation RunRightAnimation, RunRightUpAnimation, RunRightDownAnimation,
                         RunLeftAnimation, RunLeftUpAnimation, RunLeftDownAnimation,
                         StandRightAnimation, StandRightUpAnimation, StandRightDownAnimation,
                         StandLeftAnimation, StandLeftUpAnimation, StandLeftDownAnimation,
                         JumpRightAnimation, JumpRightUpAnimation, JumpRightDownAnimation,
                         JumpLeftAnimation, JumpLeftUpAnimation, JumpLeftDownAnimation,
                         CrouchRightAnimation, CrouchLeftAnimation;

        public Animation CurrentAnimation; 
        #endregion

        #region Textures
        public Texture2D RunRightTexture, RunRightUpTexture, RunRightDownTexture,
                        RunLeftTexture, RunLeftUpTexture, RunLeftDownTexture,
                        StandRightTexture, StandRightUpTexture, StandRightDownTexture,
                        StandLeftTexture, StandLeftUpTexture, StandLeftDownTexture,
                        JumpRightTexture, JumpRightUpTexture, JumpRightDownTexture,
                        JumpLeftTexture, JumpLeftUpTexture, JumpLeftDownTexture,
                        CrouchRightTexture, CrouchLeftTexture,
                        HeadTexture;
        #endregion

        #region Controls
        GamePadThumbSticks Sticks;
        Buttons JumpButton, ShootButton, GrenadeButton, TrapButton;
        Buttons CurrentJumpButton, CurrentShootButton, CurrentGrenadeButton, CurrentTrapButton;
        public GamePadState CurrentGamePadState, PreviousGamePadState;
        public KeyboardState CurrentKeyboardState, PreviousKeyboardState;
        public MouseState CurrentMouseState, PreviousMouseState;
        public Vector2 MoveStick, AimStick, RumbleValues, RumbleTime;
        #endregion

        #region Movement
        bool InAir = true;
        bool DoubleJumped = false;

        public Vector2 AimDirection, MaxSpeed;
        public float Gravity;

        Facing CurrentFacing = Facing.Right;
        Facing PreviousFacing = Facing.Right;

        Pose CurrentPose = Pose.Standing;
        Pose PreviousPose = Pose.Standing;
        #endregion
        
        #region Gameplay Variables
        public int Deaths = 0;
        public int GunAmmo = 15;
        public int TrapAmmo = 0;
        public int GrenadeAmmo = 0;

        public GunType CurrentGun;
        public TrapType CurrentTrap;

        public Vector2 Health = new Vector2(100, 100);
        #endregion

        /// <summary>
        /// For determining whether the player is currently holding down the 
        /// shoot button to fire a constant beam/fire etc.
        /// </summary>
        public bool IsShooting = false;
        public bool WasShooting = false;

        public Emitter flameEmitter;

        #region Debuff
        private DebuffData _CurrentDebuff;
        public DebuffData CurrentDebuff
        {
            get { return _CurrentDebuff; }
            set
            {
                _CurrentDebuff = value;

                switch (CurrentDebuff.DebuffType)
                {
                    case DebuffType.ScrambleButtons:
                        {
                            ScrambleButtons();
                        }
                        break;
                }
            }
        }
        #endregion

        public Vector2 BarrelEnd;

        public Rectangle DestinationRectangle;
        
        public Player(PlayerIndex playerIndex)
        {
            PlayerIndex = playerIndex;
            Position = new Vector2(500, 500);
            MaxSpeed = new Vector2(5f, 6);
            Gravity = 0.6f;
            Size = new Vector2(59, 98);
        }

        public new void Initialize()
        {

        }

        public void LoadContent(ContentManager content)
        {
            JumpButton = Buttons.A;
            ShootButton = Buttons.X;
            GrenadeButton = Buttons.B;
            TrapButton = Buttons.Y;

            CurrentJumpButton = JumpButton;
            CurrentShootButton = ShootButton;
            CurrentGrenadeButton = GrenadeButton;
            CurrentTrapButton = TrapButton;

            #region Load Textures
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
            #endregion

            #region Set up animations
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
            #endregion

            CurrentAnimation = StandRightAnimation;
        }

        public override void Update(GameTime gameTime)
        {
            PreviousPose = CurrentPose;
            PreviousFacing = CurrentFacing;
            PreviousGamePadState = CurrentGamePadState;
            PreviousKeyboardState = CurrentKeyboardState;
            PreviousMouseState = CurrentMouseState;
            WasShooting = IsShooting;

       


            #region Control States
            CurrentGamePadState = GamePad.GetState(PlayerIndex);
            CurrentKeyboardState = Keyboard.GetState();
            CurrentMouseState = Mouse.GetState();

            Sticks = CurrentGamePadState.ThumbSticks;
            MoveStick = Sticks.Left;
            AimStick = Sticks.Right; 
            #endregion
            
            #region Jump
            if (CurrentGamePadState.IsButtonDown(CurrentJumpButton) &&
                PreviousGamePadState.IsButtonUp(CurrentJumpButton) &&
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
            #endregion

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

            if (CurrentPose == Pose.Standing)
            {
                Size = new Vector2(59, 98);
            }
            else
            {
                Size = new Vector2(59, 74);
            }

            #region Move stick down
            if (MoveStick.Y < -0.75f)
            {
                CurrentPose = Pose.Crouching;

                if (PreviousPose == Pose.Standing)
                {
                    Position.Y += 12;
                }
            }
            else
            {
                CurrentPose = Pose.Standing;

                if (PreviousPose == Pose.Crouching)
                {
                    Position.Y -= 12;
                }
            }
            #endregion

            Velocity.Y += Gravity * ((float)gameTime.ElapsedGameTime.TotalSeconds * 60f);

            base.Update(gameTime);

            #region Collision Reactions
            if (PushesBottomTile == true)
            {
                Velocity.Y = 0;
                InAir = false;
                DoubleJumped = false;
            }
            else
            {
                InAir = true;
            }

            if (PushesTopTile == true)
            {
                Velocity.Y = 0;
            }

            if (PushesLeftTile == true ||
                PushesRightTile == true)
            {
                Velocity.X = 0;
            }
            #endregion

            #region Stop Moving
            if (MoveStick.X == 0)
            {
                if (InAir == false)
                    Velocity.X *= 0.85f;
                else
                    Velocity.X *= 0.95f;
            }

            if (Velocity.X <= 0.5f &&
                Velocity.X >= -0.5f)
            {
                Velocity.X = 0f;
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
            if (CurrentGamePadState.IsButtonDown(CurrentShootButton) &&
                PreviousGamePadState.IsButtonUp(CurrentShootButton) &&
                CurrentGun != GunType.Flamethrower)
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

            if (CurrentGamePadState.IsButtonDown(CurrentShootButton) &&
                PreviousGamePadState.IsButtonDown(CurrentShootButton))
            {
                switch (CurrentGun)
                {
                    case GunType.Flamethrower:
                        {
                            IsShooting = true;

                            if (flameEmitter != null)
                            {
                                Vector2 rang = new Vector2(
                                            MathHelper.ToDegrees(-(float)Math.Atan2(AimDirection.Y, AimDirection.X)) - 5,
                                            MathHelper.ToDegrees(-(float)Math.Atan2(AimDirection.Y, AimDirection.X)) + 5);

                                rang.X += 10 * (float)Math.Sin(gameTime.TotalGameTime.TotalSeconds*5);
                                rang.Y += 10 * (float)Math.Sin(gameTime.TotalGameTime.TotalSeconds*5);

                                flameEmitter.AngleRange = rang;
                            }

                        }
                        break;
                }
            }

            if (CurrentGamePadState.IsButtonUp(CurrentShootButton) &&
                PreviousGamePadState.IsButtonUp(CurrentShootButton))
            {
                IsShooting = false;
            }

            if (IsShooting == false && WasShooting == true && flameEmitter != null)
            {
                flameEmitter.AddMore = false;
            }
            #endregion

            #region Grenade
            if (CurrentGamePadState.IsButtonDown(CurrentGrenadeButton) &&
                PreviousGamePadState.IsButtonUp(CurrentGrenadeButton))
            {
                //Create grenades!
                CreatePlayerGrenade();
            }
            #endregion

            #region Trap
            if (CurrentGamePadState.IsButtonDown(CurrentTrapButton) &&
                PreviousGamePadState.IsButtonUp(CurrentTrapButton))
            {
                if (TrapAmmo > 0)
                {
                    //Create traps!
                    CreatePlaceTrap(Position, TrapType.Mine);
                    TrapAmmo--;
                }
            }
            #endregion

            #region Change Animation Direction
            switch (CurrentFacing)
            {
                #region Left
                case Facing.Left:
                    {
                        AimDirection = new Vector2(-1, 0);

                        if (InAir == false)
                        {
                            if (Velocity.X != 0)
                                CurrentAnimation = RunLeftAnimation;
                            else
                            {
                                if (CurrentPose == Pose.Standing)
                                    CurrentAnimation = StandLeftAnimation;
                                else
                                    CurrentAnimation = CrouchLeftAnimation;
                            }
                        }
                        else
                            CurrentAnimation = JumpLeftAnimation;
                    }
                    break;
                #endregion

                #region Right
                case Facing.Right:
                    {
                        AimDirection = new Vector2(1, 0);

                        if (InAir == false)
                        {
                            if (Velocity.X != 0)
                                CurrentAnimation = RunRightAnimation;
                            else
                            {
                                if (CurrentPose == Pose.Standing)
                                    CurrentAnimation = StandRightAnimation;
                                else
                                    CurrentAnimation = CrouchRightAnimation;
                            }
                        }
                        else
                            CurrentAnimation = JumpRightAnimation;
                    }
                    break; 
                    #endregion
            } 
            #endregion            

            #region Update Animations
            if (CurrentAnimation != null)
            {
                CurrentAnimation.Position = Position;
                CurrentAnimation.Update(gameTime, DestinationRectangle);
            }
            #endregion            

            #region Item Collisions
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

                            switch ((Item as Gun).GunType)
                            {
                                case GunType.RocketLauncher:
                                    {
                                        CurrentGun = GunType.RocketLauncher;
                                    }
                                    break;

                                case GunType.Flamethrower:
                                    {
                                        CurrentGun = GunType.Flamethrower;
                                        //CurrentDebuff = new DebuffData(DebuffType.ScrambleButtons, new Vector2(0, 15000));
                                    }
                                    break;
                            }
                        }

                        ItemList.Remove(Item);
                    }
                }); 
            #endregion

            #region Trap Collisions
            if (TrapList != null)
                TrapList.ForEach(Trap =>
                {
                    if (Trap.Active == true && Trap.CollisionRectangle.Intersects(CollisionRectangle))
                    {
                        Health.X -= 20;
                        Trap.Reset();
                    }
                }); 
            #endregion

            #region Player Died
            if (Health.X <= 0)
            {
                Deaths++;
                flameEmitter = null;
                IsShooting = false;
                WasShooting = false;
                CreatePlayerDied();
                UnscrambleButtons();
            }
            #endregion

            #region Debuffs
            if (_CurrentDebuff.Active == true)
            {
                _CurrentDebuff.Update(gameTime);

                //The debuff has expired based on the previous Update                
                if (_CurrentDebuff.Active == false)
                {
                    switch (_CurrentDebuff.DebuffType)
                    {
                        case DebuffType.ScrambleButtons:
                            {
                                UnscrambleButtons();
                            }
                            break;
                    }
                }
            }
            #endregion

            BarrelEnd = Position + new Vector2(AimDirection.X * 28, -12);

            if (flameEmitter != null)
            {
                flameEmitter.Update(gameTime);
                flameEmitter.Position = BarrelEnd;
                flameEmitter.SpeedRange = new Vector2(
                    6 + Math.Abs(Velocity.X * 2),
                    8 + Math.Abs(Velocity.X * 2));

                if (Math.Abs(Velocity.X) > 0)
                {
                    flameEmitter.Friction.X = 0;
                }

                flameEmitter.Friction.Y = 0.05f;
            }

            DestinationRectangle = new Rectangle((int)(Position.X - CurrentAnimation.FrameSize.X / 2),
                                                 (int)(Position.Y - CurrentAnimation.FrameSize.Y / 2),
                                                 (int)CurrentAnimation.FrameSize.X,
                                                 (int)CurrentAnimation.FrameSize.Y);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (CurrentAnimation != null)
                CurrentAnimation.Draw(spriteBatch, Position);
        }
        
        public void DrawInfo(SpriteBatch spriteBatch, GraphicsDevice graphics, BasicEffect basicEffect)
        {
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


        public void ScrambleButtons()
        {
            Buttons[] buttons = new Buttons[] { Buttons.A, Buttons.X, Buttons.B, Buttons.Y};

            int n = buttons.Length;

            for (int i = 0; i < n; i++)
            {
                int r = i + Random.Next(n - i);
                Buttons but = buttons[r];
                buttons[r] = buttons[i];
                buttons[i] = but;
            }
            
            CurrentJumpButton = buttons[0];
            CurrentShootButton = buttons[1];
            CurrentGrenadeButton = buttons[2];
            CurrentTrapButton = buttons[3];
        }

        public void UnscrambleButtons()
        {
            CurrentJumpButton = JumpButton;
            CurrentShootButton = ShootButton;
            CurrentGrenadeButton = GrenadeButton;
            CurrentTrapButton = TrapButton;
        }
    }
}
