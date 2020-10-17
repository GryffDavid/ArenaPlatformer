using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
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
        public bool Active = true;

        #region Gameplay Variables
        public int Score = 0;
        public int Deaths = 0;
        public int GunAmmo = 0;
        public int TrapAmmo = 5;
        public int GrenadeAmmo = 5;

        public GunType CurrentGun;
        public GrenadeType CurrentGrenade;
        public TrapType CurrentTrap;

        public Vector2 Health = new Vector2(100, 100);
        public HealthBar HealthBar, SpecialBar;
        #endregion

        #region Events
        public event PlayerShootHappenedEventHandler PlayerShootHappened;

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

            NumGrenades++;
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

        public event LightProjectileHappenedEventHandler LightProjectileHappened;
        public void CreateLightProjectile(LightProjectile lightProjectile, object source)
        {
            OnLightProjectileFired(lightProjectile, source);
        }
        protected virtual void OnLightProjectileFired(LightProjectile lightProjectile, object source)
        {
            LightProjectileHappened?.Invoke(this,
                new LightProjectileEventArgs()
                {
                    //Projectile = new ShotgunProjectile(Position, new Vector2(0, 0), 10)
                    Projectile = lightProjectile
                });
        }

        public event PlaceTrapHappenedEventHandler PlaceTrapHappened;
        public void CreatePlaceTrap()
        {
            OnPlaceTrapHappened();
        }
        protected virtual void OnPlaceTrapHappened()
        {
            float rot = 0;
            Vector2 pos = Position;

            switch (CurrentTrap)
            {
                case TrapType.TripMine:
                    {
                        if (PushesLeft == true)
                        {
                            rot = MathHelper.ToRadians(90);
                            pos.X -= (DestinationRectangle.Width / 2) - 8;
                        }
                        else if (PushesRight == true)
                        {
                            rot = MathHelper.ToRadians(-90);
                            pos.X += (DestinationRectangle.Width / 2) - 8;
                        }
                        else if (PushesBottom == true)
                        {
                            rot = 0;
                            pos.Y += (DestinationRectangle.Height / 2) - 8;
                        }
                        else if (PushesTop == true)
                        {

                        }
                        else
                        {
                            return;
                        }

                        PlaceTrapHappened?.Invoke(this,
                            new PlaceTrapEventArgs()
                            {
                                Player = this,
                                TrapType = TrapType.TripMine,
                                Position = pos,
                                Rotation = rot
                            });
                    }
                break;                
            }

            TrapAmmo--;
        }
        #endregion

        #region Shared Static
        public static Random Random = new Random();
        public static Player[] Players = new Player[4];
        public static List<Item> ItemList;
        public static List<Projectile> ProjectileList;
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

        public static Texture2D RedFlagTexture, BlueFlagTexture;
        public static Texture2D ShieldTexture;
        public static Texture2D GunTexture;
        //public static Texture2D GrenadeIcon;
        #endregion

        Texture2D SkullIcon;

        #region Controls
        GamePadThumbSticks Sticks;
        Buttons JumpButton, ShootButton, GrenadeButton, TrapButton, MeleeButton;
        Buttons CurrentJumpButton, CurrentShootButton, CurrentGrenadeButton, CurrentTrapButton, CurrentMeleeButton;
        public GamePadState CurrentGamePadState, PreviousGamePadState;
        public KeyboardState CurrentKeyboardState, PreviousKeyboardState;
        public MouseState CurrentMouseState, PreviousMouseState;
        public Vector2 MoveStick, AimStick, RumbleValues, RumbleTime;
        #endregion

        #region Movement
        bool InAir = true;
        bool DoubleJumped = false;

        public Vector2 BarrelEnd;        
        public Vector2 AimDirection, MaxSpeed;
        public float Gravity;

        Facing CurrentFacing = Facing.Right;
        Facing PreviousFacing = Facing.Right;

        Pose CurrentPose = Pose.Standing;
        Pose PreviousPose = Pose.Standing;

        public Rectangle DestinationRectangle;
        #endregion
        
        #region Timing
        public Vector2 RespawnTime;
        #endregion

        /// <summary>
        /// For determining whether the player is currently holding down the 
        /// shoot button to fire a constant beam/fire etc.
        /// </summary>
        public bool IsShooting = false;
        public bool WasShooting = false;
        public bool ShieldActive = false;
        
        /// <summary>
        /// Used to purchase upgrades and items between levels
        /// </summary>
        public int Credits;
        
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

        #region Statistics
        public int NumKills, NumDeaths, NumShots, NumHits, NumGrenades, NumJumps;
        #endregion

        Vector2 ShotTiming = new Vector2(0, 200);
        Vector2 GrenadeTiming = new Vector2(0, 1500);

        public List<Emitter> FlashEmitterList = new List<Emitter>(); //For the muzzle flash
        public List<Emitter> EmitterList = new List<Emitter>(); //For fire, healing etc.
        
        //Index of the player that last did damage to this player
        public int LastDamageSource;

        public Color PlayerColor = Color.White;

        public Player(PlayerIndex playerIndex)
        {
            PlayerIndex = playerIndex;
            Position = new Vector2(500 + (150 * (int)PlayerIndex), 500);
            MaxSpeed = new Vector2(3.5f, 6);
            Gravity = 0.6f;
            Size = new Vector2(59, 98);
            CurrentGun = GunType.Shotgun;
            CurrentGrenade = GrenadeType.Cluster;
            IsKinematic = true;

            PlayerColor = Color.White;

            switch (PlayerIndex)
            {
                case PlayerIndex.One:
                    {
                        PlayerColor = Color.LimeGreen;
                    }
                    break;

                case PlayerIndex.Two:
                    {
                        PlayerColor = Color.RoyalBlue;
                    }
                    break;

                case PlayerIndex.Three:
                    {
                        PlayerColor = Color.Gold;
                    }
                    break;

                case PlayerIndex.Four:
                    {
                        PlayerColor = Color.Red;
                    }
                    break;
            }

            HealthBar = new HealthBar()
            {
                Position = new Vector2(40 + (480 * (int)PlayerIndex), 40),
                Size = new Vector2(440, 25),
                Player = this,
                BackColor = Color.Gray,
                FrontColor = PlayerColor
            };

            SpecialBar = new HealthBar(Color.DeepSkyBlue, Color.White)
            {
                Position = new Vector2(40 + (480 * (int)PlayerIndex), 40+25),
                Size = new Vector2(440, 10),
                Player = this,                
            };
        }

        public new void Initialize()
        {
            base.Initialize();
        }

        public void LoadContent(ContentManager content)
        {
            JumpButton = Buttons.A;
            ShootButton = Buttons.X;
            GrenadeButton = Buttons.B;
            TrapButton = Buttons.Y;
            MeleeButton = Buttons.RightShoulder;

            CurrentJumpButton = JumpButton;
            CurrentShootButton = ShootButton;
            CurrentGrenadeButton = GrenadeButton;
            CurrentTrapButton = TrapButton;
            CurrentMeleeButton = MeleeButton;

            #region Load Textures
            RunRightTexture = content.Load<Texture2D>("Player" + ((int)PlayerIndex + 1) + "/Running/RunRight");
            RunLeftTexture = content.Load<Texture2D>("Player" + ((int)PlayerIndex + 1) + "/Running/RunLeft");
            StandRightTexture = content.Load<Texture2D>("Player" + ((int)PlayerIndex + 1) + "/Standing/StandRight");
            StandLeftTexture = content.Load<Texture2D>("Player" + ((int)PlayerIndex + 1) + "/Standing/StandLeft");

            JumpRightTexture = content.Load<Texture2D>("Player" + ((int)PlayerIndex + 1) + "/Jumping/JumpRight");
            JumpLeftTexture = content.Load<Texture2D>("Player" + ((int)PlayerIndex + 1) + "/Jumping/JumpLeft");

            JumpRightDownTexture = content.Load<Texture2D>("Player" + ((int)PlayerIndex + 1) + "/Jumping/JumpRightDown");
            JumpLeftDownTexture = content.Load<Texture2D>("Player" + ((int)PlayerIndex + 1) + "/Jumping/JumpLeftDown");

            CrouchRightTexture = content.Load<Texture2D>("Player" + ((int)PlayerIndex + 1) + "/CrouchRight");
            CrouchLeftTexture = content.Load<Texture2D>("Player" + ((int)PlayerIndex + 1) + "/CrouchLeft");
            #endregion

            #region Set up animations
            RunRightAnimation = new Animation(RunRightTexture, 12, 50);

            RunLeftAnimation = new Animation(RunLeftTexture, 12, 50);

            StandLeftAnimation = new Animation(StandLeftTexture, 1, 50);

            StandRightAnimation = new Animation(StandRightTexture, 1, 50);

            JumpLeftAnimation = new Animation(JumpLeftTexture, 1, 50);
            JumpLeftDownAnimation = new Animation(JumpLeftDownTexture, 1, 50);


            JumpRightAnimation = new Animation(JumpRightTexture, 1, 50);
            JumpRightDownAnimation = new Animation(JumpRightDownTexture, 1, 50);


            CrouchRightAnimation = new Animation(CrouchRightTexture, 1, 50);
            CrouchLeftAnimation = new Animation(CrouchLeftTexture, 1, 50);
            #endregion

            SkullIcon = content.Load<Texture2D>("Icons/SkullIcon");

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

            if (Active == true)
            {
                HealthBar.Update(Health);
                SpecialBar.Update(new Vector2(87, 100));

                foreach (Emitter emitter in FlashEmitterList)
                {
                    emitter.Position.X = BarrelEnd.X;
                    emitter.Update(gameTime);
                }
                FlashEmitterList.RemoveAll(Emitter => Emitter.Active == false);

                foreach (Emitter emitter in EmitterList)
                {
                    //emitter.Position = Position;
                    emitter.Update(gameTime);
                }
                EmitterList.RemoveAll(Emitter => Emitter.Active == false);

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
                    DoubleJumped == false)
                {
                    NumJumps++;

                    if (InAir == true)
                    {
                        DoubleJumped = true;
                        Velocity.Y -= 15f;
                    }
                    else
                    {
                        //NOT SURE ABOUT HAVING BOTH THIS AND THE SMOKE PUFF WHEN LANDING. 
                        //I LIKE THE SMOKE MORE SO I'M LEAVING THIS OUT FOR NOW

                        //Emitter jumpEmitter1 = new Emitter(Game1.HitEffectParticle, new Vector2(Position.X + 5, Position.Y + DestinationRectangle.Height / 2),
                        //     new Vector2(90, 180), new Vector2(5, 8), new Vector2(500, 500), 1f, false, new Vector2(0, 0),
                        //     new Vector2(0, 0), new Vector2(0.1f, 0.1f), Color.White, Color.White, 0f, 0.1f, 100, 5, false,
                        //     Vector2.Zero, true, null, null, null, null, null, null, true, new Vector2(0.15f, 0.15f), null, null, null, null, null, true);


                        //Emitter jumpEmitter2 = new Emitter(Game1.HitEffectParticle, new Vector2(Position.X + 5, Position.Y + DestinationRectangle.Height / 2),
                        //    new Vector2(0, 90), new Vector2(5, 8), new Vector2(500, 500), 1f, false, new Vector2(0, 0),
                        //    new Vector2(0, 0), new Vector2(0.1f, 0.1f), Color.White, Color.White, 0f, 0.1f, 100, 5, false,
                        //    Vector2.Zero, true, null, null, null, null, null, null, true, new Vector2(0.15f, 0.15f), null, null, null, null, null, true);

                        //EmitterList.Add(jumpEmitter1);
                        //EmitterList.Add(jumpEmitter2);

                        Velocity.Y = -15f;
                    }
                }
                #endregion

                if (CurrentGamePadState.IsButtonDown(Buttons.LeftStick) &&
                    PreviousGamePadState.IsButtonUp(Buttons.LeftStick))
                {
                    Health.X = 0;
                }

                #region Move stick left
                if (MoveStick.X < -0.15f)
                {
                    AimDirection.X = -1f;
                    CurrentFacing = Facing.Left;
                }
                #endregion

                #region Move stick right
                if (MoveStick.X > 0.15f)
                {
                    AimDirection.X = 1f;
                    CurrentFacing = Facing.Right;
                }
                #endregion

                if (Math.Abs(MoveStick.X) > 0.15f)
                    Velocity.X += (MoveStick.X * 3f);

                if (CurrentPose == Pose.Standing)
                {
                    Size = new Vector2(59, 98);
                }
                else
                {
                    Size = new Vector2(59, 74);
                }

                Velocity.Y += Gravity * ((float)gameTime.ElapsedGameTime.TotalSeconds * 60f);

                base.Update(gameTime);

                #region Collision Reactions
                if (PushesBottomTile == true)
                {
                    if (InAir == true)
                    {
                        Emitter jumpEmitter1 = new Emitter(Game1.ToonSmoke1, new Vector2(Position.X - 2, Position.Y + DestinationRectangle.Height / 2),
                            new Vector2(110, 180), new Vector2(2, 5), new Vector2(300, 500), 1f, false, new Vector2(-7, 7),
                            new Vector2(0, 0), new Vector2(0.03f, 0.05f), Color.White, Color.White, 0f, 0.1f, 100, 9, false,
                            Vector2.Zero, true, null, null, null, null, null, null, false, new Vector2(0.08f, 0.08f), null, null, null, false, false, true);

                        Emitter jumpEmitter2 = new Emitter(Game1.ToonSmoke1, new Vector2(Position.X + 2, Position.Y + DestinationRectangle.Height / 2),
                            new Vector2(20, 90), new Vector2(2, 5), new Vector2(300, 500), 1f, false, new Vector2(-7, 7),
                            new Vector2(0, 0), new Vector2(0.03f, 0.05f), Color.White, Color.White, 0f, 0.1f, 100, 9, false,
                            Vector2.Zero, true, null, null, null, null, null, null, false, new Vector2(0.08f, 0.08f), null, null, null, false, false, true);


                        EmitterList.Add(jumpEmitter1);
                        EmitterList.Add(jumpEmitter2);
                    }

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
                        Velocity.X = 0f;
                    else
                        Velocity.X *= 0.98f;
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

                Shoot(gameTime);

                #region Grenade
                if (GrenadeTiming.X >= GrenadeTiming.Y)
                {
                    if (CurrentGamePadState.IsButtonDown(CurrentGrenadeButton) &&
                        PreviousGamePadState.IsButtonUp(CurrentGrenadeButton))
                    {
                        CreatePlayerGrenade();
                        GrenadeTiming.X = 0;

                        if (CurrentGrenade != GrenadeType.Regular)
                        {
                            GrenadeAmmo--;

                            if (GrenadeAmmo == 0)
                            {
                                CurrentGrenade = GrenadeType.Regular;
                            }
                        }
                    }
                }
                else
                {
                    GrenadeTiming.X += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                }
                #endregion

                #region Trap
                if (CurrentGamePadState.IsButtonDown(CurrentTrapButton) &&
                    PreviousGamePadState.IsButtonUp(CurrentTrapButton))
                {
                    if (TrapAmmo > 0)
                    {
                        //CreatePlaceTrap(Position, TrapType.Mine);
                        CreatePlaceTrap();
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
                            {
                                if (Velocity.Y > 2)
                                    CurrentAnimation = JumpLeftDownAnimation;
                                else
                                    CurrentAnimation = JumpLeftAnimation;
                            }
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
                            {
                                if (Velocity.Y > 2)
                                    CurrentAnimation = JumpRightDownAnimation;
                                else
                                    CurrentAnimation = JumpRightAnimation;
                            }
                        }
                        break;
                        #endregion
                }
                #endregion

                DestinationRectangle = new Rectangle((int)(Position.X - CurrentAnimation.FrameSize.X / 2),
                                                     (int)(Position.Y - CurrentAnimation.FrameSize.Y / 2),
                                                     (int)CurrentAnimation.FrameSize.X,
                                                     (int)CurrentAnimation.FrameSize.Y);

                BoundingBox = new BoundingBox(new Vector3(Position - (CurrentAnimation.FrameSize / 2), 0),
                                              new Vector3(Position + (CurrentAnimation.FrameSize / 2), 0));

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
                        bool removeItem = true;

                        if (Item.CollisionRectangle.Intersects(CollisionRectangle))
                        {
                            switch (Item.ItemType)
                            {
                                case ItemType.Shield:
                                    {
                                        ShieldActive = true;
                                        removeItem = true;
                                    }
                                    break;

                                case ItemType.Shotgun:
                                    {
                                        CurrentGun = GunType.Shotgun;
                                        ShotTiming = new Vector2(0, 1000);
                                        removeItem = true;
                                    }
                                    break;

                                case ItemType.RocketLauncher:
                                    {
                                        CurrentGun = GunType.RocketLauncher;
                                        ShotTiming = new Vector2(0, 2000);
                                        removeItem = true;
                                    }
                                    break;

                                case ItemType.MachineGun:
                                    {
                                        ShotTiming = new Vector2(0, 200);
                                        CurrentGun = GunType.MachineGun;
                                        removeItem = true;
                                    }
                                    break;                                    
                            }                                                   

                        if (removeItem == true)
                            ItemList.Remove(Item);
                        }
                    });
                #endregion

                #region Trap Collisions
                if (TrapList != null)
                    TrapList.ForEach(Trap =>
                    {
                        if (Trap.Active == true)
                        switch (Trap.TrapType)
                        {
                            case TrapType.TripMine:
                                {
                                    if ((Trap as TripMine).SourcePlayer != this && (Trap as TripMine).Laser.Ray.Intersects(BoundingBox) != null)
                                    {
                                        //Mine was triggered. Time to explode
                                        Health.X -= 50;
                                        Trap.Active = false;
                                    }
                                }
                                break;
                        }

                        //if (Trap.Active == true && Trap.CollisionRectangle.Intersects(CollisionRectangle))
                        //{
                        //    //switch (Trap.TrapType)
                        //    //{
                        //    //#region Fire
                        //    //case TrapType.Fire:
                        //    //        {

                        //    //        }
                        //    //        break;
                        //    //    #endregion
                        //    //}

                        //    Health.X -= 20;
                        //    Trap.Reset();
                        //}
                    });
                #endregion

                #region Projectile Collisions
                if (ProjectileList != null)
                    ProjectileList.ForEach(Projectile =>
                    {
                        if (Projectile.PlayerIndex != PlayerIndex &&
                            Projectile.CollisionRectangle.Intersects(CollisionRectangle))
                        {
                            LastDamageSource = (int)Projectile.PlayerIndex;

                            Projectile.Active = false;

                            if (ShieldActive == true)
                            {
                                ShieldActive = false;
                            }
                            else
                            {
                                Health.X = 0;
                            }                            
                        }
                    });
                #endregion

                #region Player Died
                if (Health.X <= 0)
                {
                    //#region Flag behaviour
                    //switch (CurrentFlagState)
                    //{
                    //    case FlagState.NoFlag:
                    //        break;

                    //    case FlagState.HasRed:
                    //        {
                    //            RedFlag replacementFlag = new RedFlag() { Position = Position };
                    //            replacementFlag.Initialize();

                    //            ItemList.Add(replacementFlag);
                    //            CurrentFlagState = FlagState.NoFlag;
                    //        }
                    //        break;

                    //    case FlagState.HasBlue:
                    //        {
                    //            BlueFlag replacementFlag = new BlueFlag() { Position = Position };
                    //            replacementFlag.Initialize();

                    //            ItemList.Add(replacementFlag);
                    //            CurrentFlagState = FlagState.NoFlag;
                    //        }
                    //        break;
                    //} 
                    //#endregion

                    Deaths++;
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

                BarrelEnd = Position + new Vector2(AimDirection.X * 28, 10);
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (CurrentAnimation != null && Active == true)
                CurrentAnimation.Draw(spriteBatch, Position);
        }

        public void DrawEmissive(SpriteBatch spriteBatch)
        {
            #region Draw the Shield
            if (ShieldActive == true)
            {
                spriteBatch.Draw(ShieldTexture,
                    new Rectangle((int)Position.X, (int)Position.Y,
                                  CollisionRectangle.Height + 8, CollisionRectangle.Height + 8),
                    null, Color.White * 0.5f, 0, new Vector2(ShieldTexture.Width / 2, ShieldTexture.Height / 2),
                    SpriteEffects.None, 0);
            }
            #endregion
        }

        public void DrawInfo(SpriteBatch spriteBatch, GraphicsDevice graphics, BasicEffect basicEffect)
        {
            #region Draw the collision box for debugging
            Color Color = Color.Red;

            VertexPositionColorTexture[] Vertices = new VertexPositionColorTexture[4];
            int[] Indices = new int[8];

            Vertices[0] = new VertexPositionColorTexture()
            {
                Color = Color,
                Position = new Vector3(CollisionRectangle.Left, CollisionRectangle.Top, 0),
                TextureCoordinate = new Vector2(0, 0)
            };

            Vertices[1] = new VertexPositionColorTexture()
            {
                Color = Color,
                Position = new Vector3(CollisionRectangle.Right - 1, CollisionRectangle.Top, 0),
                TextureCoordinate = new Vector2(1, 0)
            };

            Vertices[2] = new VertexPositionColorTexture()
            {
                Color = Color,
                Position = new Vector3(CollisionRectangle.Right - 1, CollisionRectangle.Bottom - 1, 0),
                TextureCoordinate = new Vector2(1, 1)
            };

            Vertices[3] = new VertexPositionColorTexture()
            {
                Color = Color,
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

        public void DrawHUD(SpriteBatch spriteBatch)
        {
            HealthBar.Draw(spriteBatch);
            SpecialBar.Draw(spriteBatch);

            //float perc = ((100 / GrenadeTiming.Y) * GrenadeTiming.X) / 100;
            //int height = (int)(perc * Game1.GrenadeIcon.Height);
            //spriteBatch.Draw(Game1.GrenadeIcon, 
            //    new Rectangle(40 + (480 * (int)PlayerIndex), 40 + 25 + 10 + (int)((1.0f - perc) * 24), 24, (int)(perc * 24)), 
            //    new Rectangle(0, Game1.GrenadeIcon.Height - height, Game1.GrenadeIcon.Width, height), Color.White);

            //spriteBatch.Draw(Game1.GrenadeIcon, new Rectangle(40 + (480 * (int)PlayerIndex), 40+25+10, 24, 24), Color.White * 0.5f);

            //for (int i = 0; i < Deaths; i++)
            //{
            //    spriteBatch.Draw(SkullIcon, new Vector2(40 + (480 * (int)PlayerIndex) + (32*i), 40 + 25 + 10 + 24), Color.White);
            //}
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


        public void Respawn()
        {
            Vector2 index = Map.FindSpawn();
            Position = Map.GetTilePosition((int)index.X, (int)index.Y) + (Map.TileSize * new Vector2(0.5f, 1f));
            Velocity = Vector2.Zero;
            Active = true;
        }

        public void Shoot(GameTime gameTime)
        {
            switch (CurrentGun)
            {
                #region RocketLauncher
                case GunType.RocketLauncher:
                    {
                        if (PreviousGamePadState.IsButtonUp(CurrentShootButton) &&
                            CurrentGamePadState.IsButtonDown(CurrentShootButton))
                        {
                            switch (CurrentFacing)
                            {
                                case Facing.Left:
                                    OnPlayerShootHappened(new Vector2(-35, 0));
                                    //CreatePlayerShoot(new Vector2(-35, 0));
                                    break;

                                case Facing.Right:
                                    //CreatePlayerShoot(new Vector2(35, 0));
                                    OnPlayerShootHappened(new Vector2(35, 0));
                                    break;
                            }

                            NumShots++;
                        }
                    }
                    break;
                #endregion

                #region Shotgun
                case GunType.Shotgun:
                    {
                        if (PreviousGamePadState.IsButtonUp(CurrentShootButton) &&
                            CurrentGamePadState.IsButtonDown(CurrentShootButton))
                        {
                            Vector2 direction = Vector2.Zero;

                            switch (CurrentFacing)
                            {
                                case Facing.Left:
                                    direction = new Vector2(-1, 0);
                                    break;

                                case Facing.Right:
                                    direction = new Vector2(1, 0);
                                    break;
                            }

                            for (int i = 0; i < 4; i++)
                            {
                                float angle = MathHelper.ToRadians((float)Math.Atan2(direction.Y, direction.X) + Random.Next(-15, 15));

                                direction = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * AimDirection.X;

                                LightProjectile newProjectile = new ShotgunProjectile(BarrelEnd, direction, 1);
                                CreateLightProjectile(newProjectile, this);
                            }

                            NumShots++;
                        }
                    }
                    break;
                #endregion

                #region MachineGun
                case GunType.MachineGun:
                    {
                        if (ShotTiming.X < ShotTiming.Y)
                            ShotTiming.X += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

                        if (PreviousGamePadState.IsButtonDown(CurrentShootButton) &&
                            CurrentGamePadState.IsButtonDown(CurrentShootButton) &&
                            ShotTiming.X >= ShotTiming.Y)
                        {
                            Vector2 direction = Vector2.Zero;

                            switch (CurrentFacing)
                            {
                                case Facing.Left:
                                    direction = new Vector2(-1, 0);
                                    break;

                                case Facing.Right:
                                    direction = new Vector2(1, 0);
                                    break;
                            }

                            //float angle = MathHelper.ToRadians((float)Math.Atan2(direction.Y, direction.X) + Random.Next(-15, 15));
                            float thing = Random.Next(-2, 2);
                            direction = new Vector2(MathHelper.ToRadians((float)Math.Cos(thing)), MathHelper.ToRadians((float)Math.Sin(thing))) + AimDirection;

                            LightProjectile newProjectile = new MachineGunProjectile(BarrelEnd, direction, 1);
                            CreateLightProjectile(newProjectile, this);                            

                            ShotTiming.X = 0;
                            NumShots++;
                        }
                    }
                    break;
                    #endregion
            }
        }
    }
}
