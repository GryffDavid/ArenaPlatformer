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
using System.IO;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Reflection;

namespace ArenaPlatformer1
{
    #region Enums
    public enum ChangeMessageType
    {
        UpdateParticle,
        DeleteRenderData,
    };

    public enum TrapType
    {
        Mine,
        Glue,
        Spikes,
        Fire,
        Gas
    };

    public enum GrenadeType
    {
        Explosive,
        Fire
    };

    public enum GunType
    {
        RocketLauncher,
        BeamGun,
        Flamethrower,
        GrenadeLauncher,
        Shotgun
    };

    enum GameState
    {
        MainMenu,
        ModeSelect,
        Playing,
        LevelCreator
    };

    public enum DebuffType
    {
        ScrambleButtons,
        ScrambleSticks,
        ReverseGravity
    };

    public enum PowerupTyp
    {
        Shield
    };

    public enum CrateType
    {
        ShieldPickup
    };

    public enum FlagState
    {
        HasRed,
        HasBlue,
        NoFlag
    };

    public enum TeamColor
    {
        BlueTeam,
        RedTeam
    };
    #endregion

    #region Events
    //A player is shooting
    public delegate void PlayerShootHappenedEventHandler(object source, PlayerShootEventArgs e);
    public class PlayerShootEventArgs : EventArgs
    {
        public Player Player { get; set; }
        public Vector2 Velocity;
    }


    public delegate void PlayerGrenadeHappenedEventHandler(object source, PlayerGrenadeEventArgs e);
    public class PlayerGrenadeEventArgs : EventArgs
    {
        public Player Player { get; set; }
    }

    //A player wants to place a trap
    public delegate void PlaceTrapHappenedEventHandler(object source, PlaceTrapEventArgs e);
    public class PlaceTrapEventArgs : EventArgs
    {
        public Player Player { get; set; }
        public Vector2 Position;
        public TrapType TrapType;
    }
    
    //A player wants to place a trap
    public delegate void PlayerDiedHappenedEventHandler(object source, PlayerDiedEventArgs e);
    public class PlayerDiedEventArgs : EventArgs
    {
        public Player Player { get; set; }
    }


    #endregion

    #region Debuff Data Struct
    public struct DebuffData
    {
        public DebuffData(DebuffType debuffType, Vector2 time)
        {
            Active = true;
            DebuffType = debuffType;
            Time = time;
        }

        public void Update(GameTime gameTime)
        {
            Time.X += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            if (Time.X >= Time.Y)
            {
                Active = false;
            }
        }

        public bool Active;
        public DebuffType DebuffType;
        public Vector2 Time;
    }
    #endregion

    public class Game1 : Microsoft.Xna.Framework.Game
    {
        #region Explosion Event
        public EventHandler<ExplosionEventArgs> ExplosionHappenedEvent;
        public class ExplosionEventArgs : EventArgs
        {
            public Explosion Explosion { get; set; }
        }
        protected virtual void CreateExplosion(Explosion explosion, object source)
        {
            if (ExplosionHappenedEvent != null)
                OnExplosionHappened(source, new ExplosionEventArgs() { Explosion = explosion });
        }
        #endregion

        static Random Random = new Random();

        ContentManager GameContentManager;
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        RenderTarget2D UIRenderTarget, GameRenderTarget, MenuRenderTarget;

        GameState GameState;
        
        #region Control States
        KeyboardState CurrentKeyboardState, PreviousKeyboardState;
        GamePadState[] CurrentGamePadStates = new GamePadState[4];
        GamePadState[] PreviousGamePadStates = new GamePadState[4];
        MouseState CurrentMouseState, PreviousMouseState;
        #endregion

        #region Particle System
        DoubleBuffer DoubleBuffer;
        RenderManager RenderManager;
        UpdateManager UpdateManager;

        List<Emitter> EmitterList = new List<Emitter>();
        #endregion

        #region Particle Textures
        Texture2D ExplosionParticle2, BOOMParticle, SplodgeParticle, HitEffectParticle, ToonSmoke2, ToonSmoke3;
        #endregion

        #region Lighting
        RenderTarget2D EmissiveMap, BlurMap, ColorMap, NormalMap, LightMap, FinalMap, SpecMap, DepthMap, ShadowMap;
        RenderTarget2D CrepLightMap, CrepColorMap, OcclusionMap;
        RenderTarget2D Buffer1, Buffer2;

        VertexPositionColorTexture[] LightVertices;
        VertexPositionColorTexture[] EmissiveVertices;
        VertexPositionColorTexture[] CrepVertices;

        List<PolygonShadow> ShadowList = new List<PolygonShadow>();
        List<MyRay> RayList = new List<MyRay>();

        BasicEffect BasicEffect;

        public static BlendState BlendBlack = new BlendState()
        {
            ColorBlendFunction = BlendFunction.Add,
            ColorSourceBlend = Blend.One,
            ColorDestinationBlend = Blend.One,

            AlphaBlendFunction = BlendFunction.Add,
            AlphaSourceBlend = Blend.SourceAlpha,
            AlphaDestinationBlend = Blend.One
        };

        Matrix Projection = Matrix.CreateOrthographicOffCenter(0, 1920, 1080, 0, 0, 1);

        List<Light> LightList = new List<Light>();
        List<Solid> SolidList = new List<Solid>();

        //Color AmbientLight = new Color(0.1f, 0.1f, 0.1f, 1f);
        Color AmbientLight = new Color(0.4f, 0.4f, 0.4f, 1f);
        //Color AmbientLight = new Color(0.25f, 0.25f, 0.25f, 1f);

        #endregion

        #region Effects
        Effect BlurEffect, LightCombined, LightEffect;
        Effect RaysEffect, DepthEffect;
        #endregion

        #region Lists
        List<Trap> TrapList;
        List<Item> ItemList;
        List<Grenade> GrenadeList;
        List<Projectile> ProjectileList;
        List<MovingObject> MovingObjectList;
        List<MovingPlatform> MovingPlatformList;
        #endregion

        #region Debugging
        bool DrawDiagnostics = false;
        bool DebugBoxes = false;
        bool TileBoxes = false;
        bool didDraw = false; 
        #endregion

        SpriteFont Font1;
        Texture2D Block, Texture, NormalTexture, ParticleTexture;

        Map CurrentMap;

        Player[] Players = new Player[4];
        PlayerJoin[] PlayerJoinButtons = new PlayerJoin[4];
        
        Vector2 PlaceTilePosition = new Vector2(64, 64);
        
        Camera Camera = new Camera();

        Rectangle ScreenRectangle = new Rectangle(0, 0, 1920, 1080);

        int BlueTeamScore = 0;
        int RedTeamScore = 0;

        public void OnPlayerShoot(object source, PlayerShootEventArgs e)
        {
            //e.Player.Velocity.X += (-e.Velocity.X/2);

            Rocket rocket = new Rocket()
            {
                Position = e.Player.BarrelEnd,
                PlayerIndex = e.Player.PlayerIndex,
                Velocity = e.Velocity
            };

            Vector2 rang = new Vector2(
                                        MathHelper.ToDegrees(-(float)Math.Atan2(rocket.Velocity.Y, rocket.Velocity.X)) - 180 - 60,
                                        MathHelper.ToDegrees(-(float)Math.Atan2(rocket.Velocity.Y, rocket.Velocity.X)) - 180 + 60);


            Emitter emitter = new Emitter(
                ToonSmoke3, new Vector2(rocket.Position.X, rocket.Position.Y), rang,
                new Vector2(0.5f, 1f), new Vector2(640, 960), 1f, false, new Vector2(-35, 35), new Vector2(-0.5f, 0.5f),
                new Vector2(0.025f, 0.05f), Color.DarkGray, Color.Gray, -0.00f, -1, 1, 3, false, new Vector2(0, 720), true, 0,
                null, null, null, null, null, true, new Vector2(0.00f, 0.05f), false, false, 150f,
                false, false, false, false, true);

            ProjectileList.Add(rocket);

            rocket.EmitterList.Add(emitter);


        }

        public void OnPlaceTrap(object source, PlaceTrapEventArgs e)
        {
            Trap trap;
            switch (e.TrapType)
            {
                #region Mine
                case TrapType.Mine:
                    {
                        trap = new Mine()
                        {
                            Texture = Block,
                            Position = e.Position                            
                        };

                        Emitter Emitter2 = new Emitter(ToonSmoke2,
                                new Vector2(trap.Position.X, trap.Position.Y), new Vector2(60, 120), new Vector2(1, 1),
                                new Vector2(500, 1000), 1f, false, new Vector2(-10, 10), new Vector2(-1, 1), new Vector2(0.05f, 0.06f), new Color(255, 128, 0, 6), Color.Black,
                                -0.005f, -0.4f, 50, 10, false, new Vector2(0, 720), true, 0.1f,
                                null, null, null, null, null, false, null, null, null,
                                null, null, null, true, null);

                        Emitter Emitter = new Emitter(ToonSmoke3,
                                new Vector2(trap.Position.X, trap.Position.Y), new Vector2(60, 120), new Vector2(1, 1),
                                new Vector2(500, 1000), 1f, false, new Vector2(-10, 10), new Vector2(-1, 1), new Vector2(0.05f, 0.06f),
                                new Color(255, 128, 0, 6), Color.Black,
                                -0.005f, -0.4f, 50, 10, false, new Vector2(0, 720), true, 0.1f,
                                null, null, null, null, null, false, null, null, null,
                                null, null, null, true, null);

                        trap.EmitterList.Add(Emitter);
                        trap.EmitterList.Add(Emitter2);

                        TrapList.Add(trap);
                    }
                    break; 
                #endregion
            }            
        }

        public void OnPlayerDied(object source, PlayerDiedEventArgs e)
        {
            for (int i = 0; i < 15; i++)
            {
                Emitter emitter = new Emitter(SplodgeParticle, e.Player.Position - new Vector2(0, e.Player.DestinationRectangle.Height),
                    new Vector2(0, 360), new Vector2(1, 3),
                    new Vector2(250, 500), 1, false, new Vector2(0, 360), new Vector2(-3, 3), new Vector2(0.025f, 0.1f),
                    Color.Maroon, Color.DarkRed, 0.01f, 1f, 15, 2, true, new Vector2(1080 - 64, 1080), true, 0, true,
                    true, new Vector2(2, 4), new Vector2(0, 360), 0.1f, true, null, null, null, null, true);

                EmitterList.Add(emitter);
            }

            e.Player.Position = new Vector2(100, 100 + 64);
            e.Player.Health.X = 100;
            e.Player.GunAmmo = 50;
            e.Player.Velocity = new Vector2(0, 0);

            
        }

        public void OnExplosionHappened(object source, ExplosionEventArgs e)
        {
            Explosion explosion = e.Explosion;

            #region Explosion Effect
            #region Regular ground

            #region Smoke
            Emitter Emitter2 = new Emitter(ToonSmoke2,
                    new Vector2(explosion.Position.X, explosion.Position.Y), new Vector2(60, 120), new Vector2(1, 1),
                    new Vector2(500, 1000), 1f, false, new Vector2(-10, 10), new Vector2(-1, 1), new Vector2(0.05f, 0.06f), new Color(255, 128, 0, 6), Color.Black,
                    -0.005f, 0.4f, 50, 10, false, new Vector2(0, 720), true, 0.1f,
                    null, null, null, null, null, false, null, null, null,
                    null, null, null, true, null);

            Emitter Emitter = new Emitter(ToonSmoke3,
                    new Vector2(explosion.Position.X, explosion.Position.Y), new Vector2(60, 120), new Vector2(1, 1),
                    new Vector2(500, 1000), 1f, false, new Vector2(-10, 10), new Vector2(-1, 1), new Vector2(0.05f, 0.06f), 
                    new Color(255, 128, 0, 6), Color.Black,
                    -0.005f, 0.4f, 50, 10, false, new Vector2(0, 720), true, 0.1f,
                    null, null, null, null, null, false, null, null, null,
                    null, null, null, true, null);

            EmitterList.Add(Emitter);
            EmitterList.Add(Emitter2);
            #endregion

            //EMISSIVE
            Emitter ExplosionEmitter = new Emitter(ExplosionParticle2,
                    new Vector2(explosion.Position.X, explosion.Position.Y),
                    new Vector2(20, 160), new Vector2(0.3f, 0.8f), new Vector2(500, 1000), 1f, true, new Vector2(-2, 2),
                    new Vector2(-1, 1), new Vector2(0.15f, 0.25f), new Color(255, 128, 0, 6), new Color(0, 0, 0, 255), -0.2f, 0.1f, 10, 1, false,
                    new Vector2(explosion.Position.Y, explosion.Position.Y + 8), false, explosion.Position.Y / 1080f,
                    null, null, null, null, null, null, new Vector2(0.1f, 0.2f), true, true, null, null, null, true)
            {
                Emissive = true
            };
            EmitterList.Add(ExplosionEmitter);

            //EMISSIVE
            Emitter ExplosionEmitter3 = new Emitter(ExplosionParticle2,
                    new Vector2(explosion.Position.X, explosion.Position.Y),
                    new Vector2(85, 95), new Vector2(2, 4), new Vector2(400, 640), 1f, false, new Vector2(0, 0),
                    new Vector2(0, 0), new Vector2(0.085f, 0.2f), new Color(255, 128, 0, 6), new Color(0, 0, 0, 255), -0.1f, 0.05f, 10, 1, false,
                    new Vector2(explosion.Position.Y, explosion.Position.Y + 8), true, explosion.Position.Y / 1080f,
                    null, null, null, null, null, null, new Vector2(0.0025f, 0.0025f), true, true, 50)
            {
                Emissive = true
            };
            EmitterList.Add(ExplosionEmitter3);

            Emitter BOOMEmitter = new Emitter(BOOMParticle, 
                    new Vector2(explosion.Position.X, explosion.Position.Y - 12),
                    new Vector2(0, 0), new Vector2(0.001f, 0.001f), new Vector2(400, 400), 1f, false,
                    new Vector2(-25, 25), new Vector2(0, 0), new Vector2(0.35f, 0.35f),
                    Color.White, Color.White, 0f, 0.05f, 50, 1, false, new Vector2(0, 1080), true,
                    0.05f, null, null, null, null, null, false, new Vector2(0.11f, 0.11f), false, false,
                    null, false, false, true);
            EmitterList.Add(BOOMEmitter);


            //EMISSIVE
            Emitter HitEffect1 = new Emitter(HitEffectParticle,
                    new Vector2(explosion.Position.X, explosion.Position.Y), new Vector2(20f, 160f), new Vector2(5f, 8f),
                    new Vector2(250f, 500f), 1f, false, new Vector2(0f, 360f), new Vector2(-2f, 2f),
                    new Vector2(0.35f, 0.35f), new Color(255, 255, 191, 255),
                    new Color(255, 255, 255, 255), 0f, 0.05f, 50f, 7, false, new Vector2(0f, 1080), true,
                    (explosion.Position.Y + 8) / 1080f,
                    false, false, null, null, 0f, true, new Vector2(0.11f, 0.11f), false, false, 0f,
                    false, false, false, null)
            {
                Emissive = true
            };
            EmitterList.Add(HitEffect1);


            for (int i = 0; i < 12; i++)
            {
                Emitter emitter = new Emitter(ParticleTexture, explosion.Position,
                    new Vector2(0, 360), new Vector2(0, 3),
                    new Vector2(500, 1500), 1f, true, Vector2.Zero, new Vector2(-3, 3), new Vector2(0.5f, 1f),
                    new Color(Color.OrangeRed.R, Color.OrangeRed.G, Color.OrangeRed.B, 20),
                    new Color(Color.Gold.R, Color.Gold.G, Color.Gold.B, 50),
                    0f, (float)DoubleRange(0.15d, 0.5d), 15, 3, true, new Vector2(1080 - 64, 1080 - 64),
                    false, 0, true, true, new Vector2(5, 7), new Vector2(0, 360), 0.0f,
                    false, new Vector2(0.05f, 0.03f), null, null, null, true, null, null, true, false);

                EmitterList.Add(emitter);
            }

            //for (int i = 0; i < 20; i++)
            //{
            //    Emitter emitter = new Emitter(ParticleTexture, explosion.Position,
            //        new Vector2(0, 360), new Vector2(0, 3),
            //        new Vector2(500, 1500), 1f, true, Vector2.Zero, new Vector2(-3, 3), new Vector2(0.5f, 1f),
            //        new Color(Color.OrangeRed.R, Color.OrangeRed.G, Color.OrangeRed.B, 20),
            //        new Color(Color.Gold.R, Color.Gold.G, Color.Gold.B, 50),
            //        0f, (float)DoubleRange(1.15d, 2.5d), 15, 3, true, new Vector2(1080 - 64, 1080 - 64),
            //        false, 0, true, true, new Vector2(5, 7), new Vector2(0, 360), 0.0f,
            //        false, new Vector2(0.05f, 0.03f), null, null, null, true, null, null, true, false);

            //    EmitterList.Add(emitter);
            //}
            #endregion

            //ExplosionEffect explosionEffect = new ExplosionEffect(new Vector2(heavyProjectile.Position.X, heavyProjectile.BoundingBox.Max.Y))
            //{
            //    Texture = ExplosionRingSprite
            //};
            //ExplosionEffectList.Add(explosionEffect);

            //Camera.Shake(15, 1.5f);
            #endregion

            foreach (Player player in Players.Where(Player => Player != null))
            {
                float dist = Vector2.Distance(new Vector2(player.DestinationRectangle.Center.X, player.DestinationRectangle.Center.Y), explosion.Position);

                Camera.Shake(30, 2);


                if (dist < explosion.BlastRadius)
                {
                    player.Health.X -= 50;


                    //if (e.Explosion.Source == player)
                    //{
                    //    //player.Score--;
                    //}
                    //else
                    //{
                    //    (e.Explosion.Source as Player).Score++;
                    //}

                    //player.MakeRumble(250, new Vector2(0.9f, 0.1f));
                }
            }

        }

        public void OnPlayerGrenade(object source, PlayerGrenadeEventArgs e)
        {
            Grenade grenade = new Grenade(e.Player.Position, new Vector2(16, 0) * e.Player.AimDirection, e.Player);
            GrenadeList.Add(grenade);
        }


        public Game1()
        {
            graphics = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = 1920,
                PreferredBackBufferHeight = 1080
            };

            Content.RootDirectory = "Content";
            GameContentManager = new ContentManager(Content.ServiceProvider, Content.RootDirectory);

            graphics.SynchronizeWithVerticalRetrace = true;
            this.IsMouseVisible = true;
            this.IsFixedTimeStep = false;
        }
        
        protected override void Initialize()
        {
            GameState = GameState.MainMenu;

            ExplosionHappenedEvent += OnExplosionHappened;

            base.Initialize();
        }


        protected void LoadGameContent()
        {
            LoadLevel();
            MovingObject.Map = CurrentMap;

            GrenadeList = new List<Grenade>();

            Player.Players = Players;

            ProjectileList = new List<Projectile>();
            Player.ProjectileList = ProjectileList;

            TrapList = new List<Trap>();
            Player.TrapList = TrapList;

            ItemList = new List<Item>();
            Player.ItemList = ItemList;

            MovingObjectList = new List<MovingObject>();
            MovingPlatformList = new List<MovingPlatform>();

            //MovingPlatform platform1 = new MovingPlatform()
            //{
            //    Texture = Block,
            //    Position = new Vector2(400, 250),
            //    Velocity = new Vector2(2, 0)
            //};

            //MovingPlatformList.Add(platform1);

            //MovingPlatform platform2 = new MovingPlatform()
            //{
            //    Texture = Block,
            //    Position = new Vector2(848, 609)
            //};

            //MovingPlatformList.Add(platform2);

            #region Particle System
            DoubleBuffer = new DoubleBuffer();
            RenderManager = new RenderManager(DoubleBuffer);
            RenderManager.LoadContent(GameContentManager);

            UpdateManager = new UpdateManager(DoubleBuffer);

            UpdateManager.StartOnNewThread();

            Emitter.UpdateManager = UpdateManager;
            Emitter.RenderManager = RenderManager;

            ParticleTexture = GameContentManager.Load<Texture2D>("Particles/diamond");
            #endregion

            LightList.Add(new Light()
            {
                Color = Color.OrangeRed,
                Active = true,
                Power = 1.7f,
                Position = new Vector3(100, 100, 100),
                Size = 800
            });

            foreach (Tile tile in CurrentMap.DrawTiles)
            {
                if (tile != null)
                {
                    Solid solid = new Solid(Block, tile.Position, new Vector2(64, 64));
                    solid.LoadContent(GameContentManager);
                    SolidList.Add(solid);
                }
            }

            #region Guns
            RocketLauncher.Texture = GameContentManager.Load<Texture2D>("Gun");
            FlameThrower.Texture = GameContentManager.Load<Texture2D>("Gun");

            RocketLauncher launcher = new RocketLauncher()
            {
                Position = new Vector2(200, 200)
            };
            launcher.LoadContent(Content);
            ItemList.Add(launcher);            

            FlameThrower flameThrower = new FlameThrower()
            {
                Position = new Vector2(800, 500)
            };
            flameThrower.LoadContent(Content);
            ItemList.Add(flameThrower);
            
            FlameThrower flameThrower2 = new FlameThrower()
            {
                Position = new Vector2(300, 500)
            };
            flameThrower2.LoadContent(Content);
            ItemList.Add(flameThrower2);
            #endregion

            #region Traps
            MinePickup.Texture = GameContentManager.Load<Texture2D>("Blank");
            MinePickup mine = new MinePickup()
            {
                Position = new Vector2(500, 500)
            };
            mine.LoadContent(GameContentManager);
            ItemList.Add(mine);
            #endregion

            Player.ShieldTexture = GameContentManager.Load<Texture2D>("PlayerShield");
            Player.MeleeEffectTexture = GameContentManager.Load<Texture2D>("MeleeEffect1");
            Player.GunTexture = GameContentManager.Load<Texture2D>("Gun");

            ShieldPickup.Texture = GameContentManager.Load<Texture2D>("Crate");
            ShieldPickup shieldPickup = new ShieldPickup()
            {
                Position = new Vector2(1200, 800)
            };
            shieldPickup.LoadContent(GameContentManager);

            ItemList.Add(shieldPickup);

            Player.BlueFlagTexture = GameContentManager.Load<Texture2D>("BlueFlag");
            Player.RedFlagTexture = GameContentManager.Load<Texture2D>("RedFlag");

            #region Red Flag
            RedFlag.Texture = GameContentManager.Load<Texture2D>("RedFlag");
            RedFlag redFlag = new RedFlag()
            {
                Position = new Vector2(400, 400)
            };
            redFlag.Initialize();
            ItemList.Add(redFlag); 
            #endregion

            #region Blue Flag
            BlueFlag.Texture = GameContentManager.Load<Texture2D>("BlueFlag");
            BlueFlag blueFlag = new BlueFlag()
            {
                Position = new Vector2(900, 400)
            };
            blueFlag.Initialize();
            ItemList.Add(blueFlag); 
            #endregion

            Grenade.Texture = GameContentManager.Load<Texture2D>("GrenadeTexture");                        
            Emitter.Map = CurrentMap;
            Trap.Map = CurrentMap;

            Texture = GameContentManager.Load<Texture2D>("Backgrounds/Texture");
            NormalTexture = GameContentManager.Load<Texture2D>("Backgrounds/NormalTexture");            
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);


            #region Lighting
            Buffer2 = new RenderTarget2D(GraphicsDevice, 1920, 1080, false, SurfaceFormat.Rgba64, DepthFormat.None, 1, RenderTargetUsage.PreserveContents);
            Buffer1 = new RenderTarget2D(GraphicsDevice, 1920, 1080);

            OcclusionMap = new RenderTarget2D(GraphicsDevice, 1920, 1080);

            EmissiveMap = new RenderTarget2D(GraphicsDevice, 1920, 1080);
            BlurMap = new RenderTarget2D(GraphicsDevice, 1920, 1080);
            ColorMap = new RenderTarget2D(GraphicsDevice, 1920, 1080);
            NormalMap = new RenderTarget2D(GraphicsDevice, 1920, 1080);
            LightMap = new RenderTarget2D(GraphicsDevice, 1920, 1080, false, SurfaceFormat.Rgba64, DepthFormat.None, 8, RenderTargetUsage.PreserveContents);
            FinalMap = new RenderTarget2D(GraphicsDevice, 1920, 1080);
            SpecMap = new RenderTarget2D(GraphicsDevice, 1920, 1080);
            CrepLightMap = new RenderTarget2D(GraphicsDevice, 1920, 1080);
            CrepColorMap = new RenderTarget2D(GraphicsDevice, 1920, 1080);
            DepthMap = new RenderTarget2D(GraphicsDevice, 1920, 1080);
            ShadowMap = new RenderTarget2D(GraphicsDevice, 1920, 1080);


            BlurEffect = Content.Load<Effect>("Shaders/Blur");
            LightCombined = Content.Load<Effect>("Shaders/LightCombined");
            LightEffect = Content.Load<Effect>("Shaders/LightEffect");
            RaysEffect = Content.Load<Effect>("Shaders/Crepuscular");

            RaysEffect.Parameters["Projection"].SetValue(Projection);
            BlurEffect.Parameters["Projection"].SetValue(Projection);

            LightVertices = new VertexPositionColorTexture[4];
            LightVertices[0] = new VertexPositionColorTexture(new Vector3(-1, 1, 0), Color.White, new Vector2(0, 0));
            LightVertices[1] = new VertexPositionColorTexture(new Vector3(1, 1, 0), Color.White, new Vector2(1, 0));
            LightVertices[2] = new VertexPositionColorTexture(new Vector3(-1, -1, 0), Color.White, new Vector2(0, 1));
            LightVertices[3] = new VertexPositionColorTexture(new Vector3(1, -1, 0), Color.White, new Vector2(1, 1));

            CrepVertices = new VertexPositionColorTexture[4];
            CrepVertices[0] = new VertexPositionColorTexture(new Vector3(-1, 1, 0), Color.White, new Vector2(0, 0));
            CrepVertices[1] = new VertexPositionColorTexture(new Vector3(1, 1, 0), Color.White, new Vector2(1, 0));
            CrepVertices[2] = new VertexPositionColorTexture(new Vector3(-1, -1, 0), Color.White, new Vector2(0, 1));
            CrepVertices[3] = new VertexPositionColorTexture(new Vector3(1, -1, 0), Color.White, new Vector2(1, 1));

            EmissiveVertices = new VertexPositionColorTexture[6];
            EmissiveVertices[0] = new VertexPositionColorTexture(new Vector3(0, 0, 0), Color.White, new Vector2(0, 0));
            EmissiveVertices[1] = new VertexPositionColorTexture(new Vector3(1920, 0, 0), Color.White, new Vector2(1, 0));
            EmissiveVertices[2] = new VertexPositionColorTexture(new Vector3(1920, 1080, 0), Color.White, new Vector2(1, 1));
            EmissiveVertices[3] = new VertexPositionColorTexture(new Vector3(1920, 1080, 0), Color.White, new Vector2(1, 1));
            EmissiveVertices[4] = new VertexPositionColorTexture(new Vector3(0, 1080, 0), Color.White, new Vector2(0, 1));
            EmissiveVertices[5] = new VertexPositionColorTexture(new Vector3(0, 0, 0), Color.White, new Vector2(0, 0));
            #endregion

            BasicEffect = new BasicEffect(GraphicsDevice)
            {
                VertexColorEnabled = true,
                Projection = Projection
            };

            UIRenderTarget = new RenderTarget2D(GraphicsDevice, 1920, 1080);
            GameRenderTarget = new RenderTarget2D(GraphicsDevice, 1920, 1080);
            MenuRenderTarget = new RenderTarget2D(GraphicsDevice, 1920, 1080);

            Font1 = Content.Load<SpriteFont>("Font1");

            CurrentMap = new Map();
            CurrentMap.Initialize();
            CurrentMap.LoadContent(Content);

            Texture2D ButtonTexture = Content.Load<Texture2D>("Blank");

            for (int i = 0; i < 4; i++)
            {
                PlayerJoinButtons[i] = new PlayerJoin(ButtonTexture, new Vector2(106 + (451 * i), 278), new Vector2(356, 524)); 
            }

            Rocket.Texture = Content.Load<Texture2D>("Projectiles/RocketTexture");
            Bullet.Texture = Content.Load<Texture2D>("Projectiles/BulletTexture");
            
            Block = Content.Load<Texture2D>("Blank");

            HealthBar.Texture = Block;

            ExplosionParticle2 = Content.Load<Texture2D>("Particles/ExplosionParticle2");
            BOOMParticle = Content.Load<Texture2D>("Particles/BOOM");
            SplodgeParticle = Content.Load<Texture2D>("Particles/Splodge");
            HitEffectParticle = Content.Load<Texture2D>("Particles/HitEffectParticle");
            ToonSmoke2 = Content.Load<Texture2D>("Particles/ToonSmoke/ToonSmoke2");
            ToonSmoke3 = Content.Load<Texture2D>("Particles/ToonSmoke/ToonSmoke3");
            //ProjectileList.Add(new Rocket() { Position = new Vector2(80, 80), Velocity = new Vector2(1, 0) });
        }
        
        protected override void UnloadContent()
        {

        }


        
        protected override void Update(GameTime gameTime)
        {
            CurrentKeyboardState = Keyboard.GetState();
            CurrentMouseState = Mouse.GetState();

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
                                    LoadGameContent();
                                    GameState = GameState.Playing;
                                }

                                PlayerJoinButtons[i].Occupied = true;

                                Players[i] = new Player((PlayerIndex)i);
                                Players[i].LoadContent(Content);

                                Players[i].PlayerShootHappened += OnPlayerShoot;
                                Players[i].PlaceTrapHappened += OnPlaceTrap;
                                Players[i].PlayerDiedHappened += OnPlayerDied;
                                Players[i].PlayerGrenadeHappened += OnPlayerGrenade;
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
                        }
                    }
                    break;
                #endregion
                
                #region Playing
                case GameState.Playing:
                    {
                        if (CurrentMouseState.LeftButton == ButtonState.Released &&
                            PreviousMouseState.LeftButton == ButtonState.Pressed &&
                            this.IsActive == true)
                        {
                            Light light = new Light()
                            {
                                Color = Color.Plum,
                                Active = true,
                                Power = 1.7f,
                                Position = new Vector3(CurrentMouseState.X, CurrentMouseState.Y, 100),
                                Size = 800
                            };

                            LightList.Add(light);                           
                        }

                        for (int i = 0; i < 4; i++)
                        {
                            if (CurrentGamePadStates[i].IsButtonUp(Buttons.Back) &&
                                PreviousGamePadStates[i].IsButtonDown(Buttons.Back))
                            {
                                GameState = GameState.LevelCreator;
                            }
                        }

                        foreach (Trap trap in TrapList)
                        {
                            trap.Update(gameTime);
                        }
                        TrapList.RemoveAll(Trap => Trap.Exists == false);


                        CurrentMap.CheckCollisions();
                        
                        foreach (MovingPlatform platform in MovingPlatformList)
                        {
                            platform.Update(gameTime);

                            CurrentMap.UpdateAreas(platform);
                            platform.CollisionDataList.Clear();
                        }

                        foreach (Solid solid in SolidList)
                        {
                            solid.Update(gameTime);
                        }

                        foreach (Item item in ItemList)
                        {
                            item.Update(gameTime);                           
                        }

                        foreach (Grenade grenade in GrenadeList)
                        {
                            grenade.Update(gameTime);

                            if (grenade.Active == false)
                            {
                                Explosion explosion = new Explosion()
                                {
                                    BlastRadius = 200,
                                    Damage = 20,
                                    Position = grenade.Position
                                };

                                CreateExplosion(explosion, grenade);
                            }
                        }

                        Camera.Update(gameTime);

                        GrenadeList.RemoveAll(Grenade => Grenade.Active == false);

                        ProjectileList.RemoveAll(Projectile => !ScreenRectangle.Contains(new Point((int)Projectile.Position.X, (int)Projectile.Position.Y)));

                        LightList[0].Position = new Vector3(Mouse.GetState().X, Mouse.GetState().Y, 0);                        

                        #region Turn on diagnostics with F3
                        if (CurrentKeyboardState.IsKeyUp(Keys.F3) &&
                            PreviousKeyboardState.IsKeyDown(Keys.F3))
                        {
                            DrawDiagnostics = !DrawDiagnostics;
                        }
                        #endregion

                        #region Turn on debug boxes with F4
                        if (CurrentKeyboardState.IsKeyUp(Keys.F4) &&
                            PreviousKeyboardState.IsKeyDown(Keys.F4))
                        {
                            DebugBoxes = !DebugBoxes;
                        }
                        #endregion

                        #region Turn on tile boxes with F5
                        if (CurrentKeyboardState.IsKeyUp(Keys.F5) &&
                            PreviousKeyboardState.IsKeyDown(Keys.F5))
                        {
                            TileBoxes = !TileBoxes;
                        }
                        #endregion
                        
                        foreach (Player player in Players.Where(Player => Player != null))
                        {
                            player.Update(gameTime);
                            CurrentMap.UpdateAreas(player);
                            player.CollisionDataList.Clear();

                            if (player.IsShooting == true && 
                                player.WasShooting == false)
                            {
                                switch (player.CurrentGun)
                                {
                                    case GunType.Flamethrower:
                                        {
                                            if (player.flameEmitter == null)
                                            {
                                                player.flameEmitter = new Emitter(ToonSmoke3,
                                                new Vector2(player.Position.X, player.Position.Y), new Vector2(60, 120), new Vector2(6, 8),
                                                new Vector2(650, 800), 1f, false, new Vector2(-10, 10), new Vector2(-1, 1), new Vector2(0.05f, 0.06f),
                                                new Color(255, 128, 0, 6), Color.Black,
                                                -0.005f, -0.4f, 16, 6, false, new Vector2(0, 720), true, 0.1f,
                                                null, null, null, null, null, false, new Vector2(0.02f, 0.01f), null, null,
                                                null, null, null, true, null);
                                            }
                                            else
                                            {
                                                player.flameEmitter.AddMore = true;
                                            }
                                        }
                                        break;
                                }
                            }
                        }

                        foreach (Projectile projectile in ProjectileList)
                        {
                            projectile.Update(gameTime);
                            projectile.UpdateEmitters(gameTime);

                            if (projectile.Active == false)
                            {
                                for (int i = 0; i < 10; i++)
                                {
                                    Vector2 rang = new Vector2(
                                        MathHelper.ToDegrees(-(float)Math.Atan2(projectile.Velocity.Y, projectile.Velocity.X)) - 180 - 60,
                                        MathHelper.ToDegrees(-(float)Math.Atan2(projectile.Velocity.Y, projectile.Velocity.X)) - 180 + 60);

                                    Emitter emitter = new Emitter(ParticleTexture, projectile.Position,
                                        new Vector2(0, 360), new Vector2(1, 3),
                                        new Vector2(500, 1500), 1f, true, Vector2.Zero, new Vector2(-3, 3), new Vector2(0.5f, 1f),
                                        new Color(Color.HotPink.R, Color.HotPink.G, Color.HotPink.B, 80),
                                        new Color(Color.HotPink.R, Color.HotPink.G, Color.HotPink.B, 20),
                                        0f, (float)DoubleRange(0.5d, 1.5d), 1, 3, true, new Vector2(1080 - 64, 1080 - 64),
                                        false, 0, true, true, new Vector2(3, 5), rang, 0.2f,
                                        true, null, null, null, null, true, null, null, true, false);

                                    EmitterList.Add(emitter);
                                }
                            }
                        }

                        ProjectileList.RemoveAll(Projectile => Projectile.Active == false);

                        //EmitterList[0].Position = new Vector2(EmitterList[0].Position.X + 3, 1080 / 2) + new Vector2(0, 7 * (float)Math.Sin((float)gameTime.TotalGameTime.TotalSeconds * 7));
                        //EmitterList[1].Position = new Vector2(EmitterList[1].Position.X + 3, 1080 / 2) + new Vector2(0, -7 * (float)Math.Sin((float)gameTime.TotalGameTime.TotalSeconds * 7));

                        foreach (Emitter emitter in EmitterList)
                        {
                            emitter.Update(gameTime);
                        }

                        EmitterList.RemoveAll(Emitter => Emitter.AddMore == false);
                    }
                    break;
                    #endregion
            }

            for (int i = 0; i < 4; i++)
            {
                PreviousGamePadStates[i] = CurrentGamePadStates[i];
            }

            PreviousMouseState = CurrentMouseState;
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
                        RenderManager.DoFrame();

                        #region Emissive
                        #region Draw to EmissiveMap
                        GraphicsDevice.SetRenderTarget(EmissiveMap);
                        GraphicsDevice.Clear(Color.Transparent);
                        spriteBatch.Begin();

                        RenderManager.DrawEmissive(spriteBatch);
                        

                        foreach (Projectile projectile in ProjectileList)
                        {
                            projectile.Draw(spriteBatch);
                        }

                        foreach (Player player in Players.Where(Player => Player != null))
                        {
                            player.DrawEmissive(spriteBatch);
                        }
                        spriteBatch.End();

                        
                        #endregion

                        #region Blur
                        GraphicsDevice.SetRenderTarget(BlurMap);
                        GraphicsDevice.Clear(Color.Transparent);

                        BlurEffect.Parameters["InputTexture"].SetValue(EmissiveMap);
                        BlurEffect.CurrentTechnique = BlurEffect.Techniques["Technique1"];

                        foreach (EffectPass pass in BlurEffect.CurrentTechnique.Passes)
                        {
                            pass.Apply();
                            GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, EmissiveVertices, 0, 2);
                        }
                        #endregion
                        #endregion

                        #region Draw to ColorMap                        
                        GraphicsDevice.SetRenderTarget(ColorMap);
                        GraphicsDevice.Clear(Color.Gray);
                        spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);
                        
                        foreach (Player player in Players.Where(Player => Player != null))
                        {
                            player.Draw(spriteBatch);
                        }

                        spriteBatch.Draw(Texture, new Rectangle(0, 0, 1920, 1080), null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 1);
                        
                        foreach (Projectile projectile in ProjectileList)
                        {
                            projectile.Draw(spriteBatch);
                        }

                        foreach (MovingPlatform platform in MovingPlatformList)
                        {
                            platform.Draw(spriteBatch);                            
                        }

                        CurrentMap.Draw(spriteBatch);

                        foreach (Trap trap in TrapList)
                        {
                            trap.Draw(spriteBatch);
                        }

                        foreach (Item item in ItemList)
                        {
                            item.Draw(spriteBatch);
                        }

                        foreach (Grenade grenade in GrenadeList)
                        {
                            grenade.Draw(spriteBatch);
                        }

                        spriteBatch.Draw(EmissiveMap, EmissiveMap.Bounds, Color.White);

                        RenderManager.DrawLit(spriteBatch);
                        spriteBatch.End();
                        #endregion

                        #region Draw to NormalMap
                        GraphicsDevice.SetRenderTarget(NormalMap);
                        GraphicsDevice.Clear(new Color(127, 127, 255));
                        spriteBatch.Begin();
                        //spriteBatch.Draw(NormalTexture, new Rectangle(0, 0, 1920, 1080), Color.White);
                        CurrentMap.Draw(spriteBatch);
                        spriteBatch.End();
                        #endregion

                        #region Draw to SpecMap

                        #endregion

                        #region Draw to DepthMap

                        #endregion

                        #region Draw to LightMap
                        GraphicsDevice.SetRenderTarget(LightMap);
                        GraphicsDevice.Clear(Color.Transparent);

                        foreach (Light light in LightList)
                        {
                            if (light.Active == true)
                            {
                                MyShadow(light);

                                GraphicsDevice.SetRenderTarget(LightMap);

                                LightEffect.Parameters["ShadowMap"].SetValue(ShadowMap);

                                LightEffect.Parameters["LightPosition"].SetValue(light.Position);
                                LightEffect.Parameters["LightColor"].SetValue(ColorToVector(light.Color));
                                LightEffect.Parameters["LightPower"].SetValue(light.Power);
                                LightEffect.Parameters["LightSize"].SetValue(light.Size);
                                LightEffect.Parameters["NormalMap"].SetValue(NormalMap);
                                LightEffect.Parameters["ColorMap"].SetValue(ColorMap);
                                LightEffect.Parameters["DepthMap"].SetValue(DepthMap);
                                LightEffect.Parameters["lightDepth"].SetValue(0.5f);

                                LightEffect.CurrentTechnique = LightEffect.Techniques["DeferredPointLight"];
                                LightEffect.CurrentTechnique.Passes[0].Apply();

                                GraphicsDevice.BlendState = BlendBlack;
                                GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, LightVertices, 0, 2);

                            }
                        }

                        //TODO: This is here to have the emissive sprites also "cast" light on the LightMap. 
                        //Not sure if it looks as good as I'd like though
                        //may need to be removed
                        spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive);
                        spriteBatch.Draw(BlurMap, BlurMap.Bounds, Color.White);
                        spriteBatch.End();
                        #endregion

                        #region Combine Normals, Lighting and Color
                        GraphicsDevice.SetRenderTarget(FinalMap);
                        GraphicsDevice.Clear(Color.DeepSkyBlue);

                        spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, null, null, null, LightCombined);
                        #region Draw the lightmap and color map combined
                        LightCombined.CurrentTechnique = LightCombined.Techniques["DeferredCombined2"];
                        LightCombined.Parameters["ambient"].SetValue(1f);
                        LightCombined.Parameters["lightAmbient"].SetValue(4f);
                        LightCombined.Parameters["ambientColor"].SetValue(AmbientLight.ToVector4());

                        LightCombined.Parameters["ColorMap"].SetValue(ColorMap);
                        LightCombined.Parameters["ShadingMap"].SetValue(LightMap);
                        LightCombined.Parameters["NormalMap"].SetValue(NormalMap);

                        LightCombined.CurrentTechnique.Passes[0].Apply();

                        spriteBatch.Draw(ColorMap, Vector2.Zero, Color.White);
                        #endregion
                        spriteBatch.End();

                        spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);
                        spriteBatch.Draw(EmissiveMap, ColorMap.Bounds, null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0.99f);
                        spriteBatch.Draw(BlurMap, BlurMap.Bounds, null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0.99f);
                        foreach (Solid solid in SolidList)
                        {
                            solid.Draw(spriteBatch, Color.Black);
                        }

                        RenderManager.Draw(spriteBatch);
                        spriteBatch.End();
                        #endregion

                        #region Occlusion Map

                        #endregion

                        #region Crepuscular ColorMap

                        #endregion

                        DoubleBuffer.SubmitRender();
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

            //spriteBatch.DrawString(Font1, CurrentMap.GetTile((int)Mouse.GetState().X/64, (int)Mouse.GetState().Y/64).ToString(), Vector2.Zero, Color.Yellow);
            //spriteBatch.DrawString(Font1, CurrentMap.GetTilePosition((int)Mouse.GetState().X / 64, (int)Mouse.GetState().Y / 64).ToString(), new Vector2(0, 32), Color.Yellow);

            if (DrawDiagnostics == true)
            {
                int y = 16;
                spriteBatch.DrawString(Font1, "Particles: " + RenderManager.RenderDataObjects.Count, new Vector2(32, y), Color.White);
                y += 16;
                spriteBatch.DrawString(Font1, "Emitters: " + EmitterList.Count.ToString(), new Vector2(32, y), Color.White);
                y += 16;
                spriteBatch.DrawString(Font1, "Items: " + ItemList.Count, new Vector2(32, y), Color.White);
                y += 16;
                spriteBatch.DrawString(Font1, "Projectiles: " + ProjectileList.Count, new Vector2(32, y), Color.White);
                y += 16;
                spriteBatch.DrawString(Font1, "Traps: " + TrapList.Count, new Vector2(32, y), Color.White);
                y += 16;
                spriteBatch.DrawString(Font1, "Grenades: " + GrenadeList.Count, new Vector2(32, y), Color.White);

                foreach (Trap trap in TrapList)
                {
                    spriteBatch.DrawString(Font1, "ResetTime: " + trap.ResetTime.ToString(), new Vector2(trap.DestinationRectangle.Right, trap.DestinationRectangle.Top), Color.White);
                }
            }

            if (DebugBoxes == true)
                if (GameState == GameState.Playing)
            {
                for (int x = 0; x < 30; x++)
                {
                    for (int y = 0; y < 17; y++)
                    {
                        VertexPositionColorTexture[] Vertices = new VertexPositionColorTexture[4];
                        int[] Indices = new int[8];

                        Vertices[0] = new VertexPositionColorTexture()
                        {
                            Color = Color.White,
                            Position = new Vector3(x * 64, y * 64, 0),
                            TextureCoordinate = new Vector2(0, 0)
                        };

                        Vertices[1] = new VertexPositionColorTexture()
                        {
                            Color = Color.White,
                            Position = new Vector3((x * 64) + 64, (y * 64), 0),
                            TextureCoordinate = new Vector2(1, 0)
                        };

                        Vertices[2] = new VertexPositionColorTexture()
                        {
                            Color = Color.White,
                            Position = new Vector3((x * 64) + 64, (y * 64) + 64, 0),
                            TextureCoordinate = new Vector2(1, 1)
                        };

                        Vertices[3] = new VertexPositionColorTexture()
                        {
                            Color = Color.White,
                            Position = new Vector3((x * 64), (y * 64) + 64, 0),
                            TextureCoordinate = new Vector2(0, 1)
                        };

                        Indices[0] = 0;
                        Indices[1] = 1;

                        Indices[2] = 2;
                        Indices[3] = 3;

                        Indices[4] = 0;

                        Indices[5] = 2;
                        Indices[6] = 0;

                        foreach (EffectPass pass in BasicEffect.CurrentTechnique.Passes)
                        {
                            pass.Apply();
                            GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.LineStrip, Vertices, 0, 4, Indices, 0, 6, VertexPositionColorTexture.VertexDeclaration);
                        }
                    }
                }

                foreach (Player player in Players.Where(Player => Player != null))
                {
                    player.DrawInfo(spriteBatch, GraphicsDevice, BasicEffect);
                }

                foreach (Item item in ItemList)
                {
                    item.DrawInfo(spriteBatch, GraphicsDevice, BasicEffect);
                }

                foreach (Trap trap in TrapList)
                {
                    trap.DrawInfo(spriteBatch, GraphicsDevice, BasicEffect);
                }

                foreach (Grenade grenade in GrenadeList)
                {
                    grenade.DrawInfo(GraphicsDevice, BasicEffect);
                }

                foreach (Projectile projectile in ProjectileList)
                {
                    projectile.DrawInfo(GraphicsDevice, BasicEffect);
                }
            }

            if (TileBoxes == true)
                if (GameState == GameState.Playing)
            {
                foreach (Tile tile in CurrentMap.DrawTiles)
                {
                    if (tile != null)
                    {
                        tile.DrawInfo(GraphicsDevice, BasicEffect);

                        spriteBatch.Draw(Block, new Rectangle((int)tile.Position.X, (int)tile.Position.Y, 4, 4), Color.Red);
                        spriteBatch.DrawString(Font1, tile.Index.X.ToString(), tile.Position, Color.Yellow);
                        spriteBatch.DrawString(Font1, tile.Index.Y.ToString(), tile.Position + new Vector2(0, 24), Color.Yellow);

                    }
                }
            }

            #region Player 1 info
            for (int i = 0; i < Players.Count(); i++)
            {
                if (Players[i] != null)
                {
                    Players[i].HealthBar.Draw(spriteBatch);
                    //int y = 16;
                    //spriteBatch.DrawString(Font1, "Health: " + Players[i].Health.X.ToString() + "/" + Players[i].Health.Y.ToString(), new Vector2(256 + (i * 256), y), Color.Red);
                    //y += 16;
                    //spriteBatch.DrawString(Font1, "Trap: " + Players[i].CurrentTrap.ToString(), new Vector2(256 + (i * 256), y), Color.Red);
                    //y += 16;
                    //spriteBatch.DrawString(Font1, "Trap Ammo: " + Players[i].TrapAmmo.ToString(), new Vector2(256 + (i * 256), y), Color.Red);
                    //y += 16;
                    //spriteBatch.DrawString(Font1, "Gun: " + Players[i].CurrentGun.ToString(), new Vector2(256 + (i * 256), y), Color.Red);
                    //y += 16;
                    //spriteBatch.DrawString(Font1, "Gun Ammo: " + Players[i].GunAmmo.ToString(), new Vector2(256 + (i * 256), y), Color.Red);
                    //y += 16;
                    //spriteBatch.DrawString(Font1, "Deahts: " + Players[i].Deaths.ToString(), new Vector2(256 + (i * 256), y), Color.Red);
                }
            }
            #endregion



            spriteBatch.End();
            #endregion
            
            #region Draw to the backbuffer
            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.Black);
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, null, null, null, null, Camera.Transform);

            if (GameState != GameState.Playing && GameState != GameState.LevelCreator)
            {
                spriteBatch.Draw(MenuRenderTarget, MenuRenderTarget.Bounds, Color.White);
            }
            else
            {
                spriteBatch.Draw(FinalMap, FinalMap.Bounds, Color.White);
                //spriteBatch.Draw(ColorMap, ColorMap.Bounds, Color.White);
                spriteBatch.Draw(BlurMap, BlurMap.Bounds, Color.White);
            }

            spriteBatch.End();
            
            spriteBatch.Begin();
            spriteBatch.Draw(UIRenderTarget, UIRenderTarget.Bounds, Color.White);
            spriteBatch.End();
            #endregion

            didDraw = true;

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
            if (UpdateManager != null && UpdateManager.RunningThread != null)
                UpdateManager.RunningThread.Abort();

            if (DoubleBuffer != null)
                DoubleBuffer.CleanUp();
        }
        

        public void DrawShadows(Light light)
        {
            Vector3 LightPos;

            LightPos = new Vector3(Mouse.GetState().X, Mouse.GetState().Y, 250);
            //LightList[0].Position = new Vector3(SpritePos.X, SpritePos.Y, 0) + new Vector3(16, 16, 0);
            //LightList[0].Position = LightPos;

            //LightList[LightList.Count - 1].Position = LightPos;

            Vector2 SourcePosition = new Vector2(light.Position.X, light.Position.Y);

            RayList.Clear();
            ShadowList.Clear();

            foreach (Solid solid in SolidList)
            {
                Vector3 lightVector, check1, check2, thing, thing2;

                for (int i = 0; i < solid.vertices.Count(); i++)
                {

                    lightVector = solid.vertices[i].Position - new Vector3(SourcePosition, 0);
                    //lightVector.Normalize();

                    //lightVector *= light.Size;

                    int nextIndex, prevIndex;

                    nextIndex = Wrap(i + 1, 4);
                    prevIndex = Wrap(i - 1, 4);

                    check1 = solid.vertices[nextIndex].Position - new Vector3(SourcePosition, 0);
                    check2 = solid.vertices[prevIndex].Position - new Vector3(SourcePosition, 0);

                    thing = Vector3.Cross(lightVector, check1);
                    thing2 = Vector3.Cross(lightVector, check2);

                    //NOTE: THIS LINE SEEMS TO FIX THE 0 VALUE CHECK VARIABLE RESULTING IN A DISAPPEARING SHADOW
                    thing.Normalize();

                    //SHADOWS DON'T SHOW UP IF THE Y OR X VALUES FOR THE THING AND CHECK ARE THE SAME.
                    //i.e. check1.y = 158 AND thing1.y = 158. Then the next if evaluates to false and a ray isn't added.
                    //meaning that there's a blank side for the polygon
                    //The Check variables use the previous and next vertex positions to calculate a vector
                    //This can end up with the vector having a 0 in it if the light lines up with a side
                    //This makes the cross product values messed up

                    if ((thing.Z <= 0 && thing2.Z <= 0) ||
                        (thing.Z >= 0 && thing2.Z >= 0))
                    {
                        RayList.Add(new MyRay() { direction = lightVector, position = solid.vertices[i].Position, length = 10f });
                    }
                }

                if (RayList.Count > 1)
                {
                    int p = RayList.Count() - 2;

                    VertexPositionColor[] vertices = new VertexPositionColor[6];

                    vertices[0].Position = RayList[p].position;
                    vertices[1].Position = RayList[p].position + (RayList[p].direction * 100);
                    vertices[2].Position = RayList[p + 1].position + (RayList[p + 1].direction * 100);

                    vertices[3].Position = RayList[p + 1].position + (RayList[p + 1].direction * 100);
                    vertices[4].Position = RayList[p + 1].position;
                    vertices[5].Position = RayList[p].position;

                    vertices[0].Color = Color.Black;
                    vertices[1].Color = Color.Black;
                    vertices[2].Color = Color.Black;
                    vertices[3].Color = Color.Black;
                    vertices[4].Color = Color.Black;
                    vertices[5].Color = Color.Black;

                    ShadowList.Add(new PolygonShadow() { Vertices = vertices });
                }
            }
        }

        public Texture2D MyShadow(Light light)
        {
            GraphicsDevice.SetRenderTarget(ShadowMap);
            GraphicsDevice.Clear(Color.White);

            DrawShadows(light);

            GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            GraphicsDevice.BlendState = PSBlendState.Multiply;
            BasicEffect.Techniques[0].Passes[0].Apply();

            foreach (PolygonShadow shadow in ShadowList)
            {
                shadow.Draw(GraphicsDevice);
            }

            return ShadowMap;
        }

        protected Vector4 ColorToVector(Color color)
        {
            return new Vector4(color.R, color.G, color.B, color.A);
        }

        
        public static int Wrap(int index, int n)
        {
            return ((index % n) + n) % n;
        }

        private int GetEven(int num)
        {
            if (num % 2 == 0)
            {
                return num;
            }
            else
            {
                return num + 1;
            }
        }

        #region MyRay
        public class MyRay
        {
            public Vector3 position, direction;
            public float length;
        }
        #endregion

        #region PS Blend States
        public static class PSBlendState
        {
            public static BlendState Multiply = new BlendState
            {
                ColorSourceBlend = Blend.DestinationColor,
                ColorDestinationBlend = Blend.Zero,
                ColorBlendFunction = BlendFunction.Add
            };
            public static BlendState Screen = new BlendState
            {
                ColorSourceBlend = Blend.InverseDestinationColor,
                ColorDestinationBlend = Blend.One,
                ColorBlendFunction = BlendFunction.Add
            };
            public static BlendState Darken = new BlendState
            {
                ColorSourceBlend = Blend.One,
                ColorDestinationBlend = Blend.One,
                ColorBlendFunction = BlendFunction.Min
            };
            public static BlendState Lighten = new BlendState
            {
                ColorSourceBlend = Blend.One,
                ColorDestinationBlend = Blend.One,
                ColorBlendFunction = BlendFunction.Max
            };
        }
        #endregion

        public double DoubleRange(double one, double two)
        {
            return one + Random.NextDouble() * (two - one);
        }

        public void LoadLevel()
        {
            string dir = Environment.CurrentDirectory;
            string newPath = Path.GetFullPath(Path.Combine(dir, @"..\..\..\..\..\..\Levels\\"));
            newPath += "Level1.lvl";

            IFormatter formatter = new BinaryFormatter();
            formatter.Binder = new SerializationHelper();


            Stream stream = new FileStream(newPath, FileMode.Open);
            Map loadMap = (Map)formatter.Deserialize(stream);

            stream.Close();

            CurrentMap = loadMap;
            CurrentMap.LoadContent(Content);
            CurrentMap.Initialize();
        }

        sealed class SerializationHelper : SerializationBinder
        {
            public override Type BindToType(string assemblyName, string typeName)
            {
                Type typetoD = null;
                string currentAssembly = Assembly.GetExecutingAssembly().FullName;
                
                assemblyName = currentAssembly;

                int index = typeName.IndexOf('.');
                string obj = typeName.Substring(index + 1);

                index = currentAssembly.IndexOf(',');
                currentAssembly = currentAssembly.Substring(0, index);

                string objType = currentAssembly + "." + obj;
                typetoD = Type.GetType(objType);

                return typetoD;
            }
        }
    }
}
