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

    public enum TrapType
    {
        Mine,
        Glue,
        Spikes,
        Fire,
        Gas
    }

    public enum GunType
    {
        RocketLauncher,
        BeamGun,
        Flamethrower,
        GrenadeLauncher,
        Shotgun
    }

    enum GameState
    {
        MainMenu,
        ModeSelect,
        Playing,
        LevelCreator
    };
    
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

    public class Game1 : Microsoft.Xna.Framework.Game
    {
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

        ContentManager GameContentManager;

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        GameState GameState;
        
        RenderTarget2D UIRenderTarget, GameRenderTarget, MenuRenderTarget;        

        bool DrawDiagnostics = false;
        bool DebugBoxes = false;
        bool TileBoxes = false;

        KeyboardState CurrentKeyboardState, PreviousKeyboardState;
        SpriteFont Font1;

        Map CurrentMap;

        Player[] Players = new Player[4];
        PlayerJoin[] PlayerJoinButtons = new PlayerJoin[4];

        MouseState CurrentMouseState, PreviousMouseState;

        //Specifically for menu interactions before the Player objects have been created
        GamePadState[] CurrentGamePadStates = new GamePadState[4];
        GamePadState[] PreviousGamePadStates = new GamePadState[4];
        

        Vector2 PlaceTilePosition = new Vector2(64, 64);

        Texture2D Block;
        static Random Random = new Random();

        #region Particle System
        DoubleBuffer DoubleBuffer;
        RenderManager RenderManager;
        UpdateManager UpdateManager;
                
        List<Emitter> EmitterList = new List<Emitter>();
        #endregion

        Texture2D Texture, NormalTexture, ParticleTexture;

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
        Color AmbientLight = new Color(0.2f, 0.2f, 0.2f, 1f);
        //Color AmbientLight = new Color(0.25f, 0.25f, 0.25f, 1f);

        #endregion

        #region Effects
        Effect BlurEffect, LightCombined, LightEffect;
        Effect RaysEffect, DepthEffect;
        #endregion

        List<Trap> TrapList;
        List<Item> ItemList;
        List<Grenade> GrenadeList;
        List<Projectile> ProjectileList;

        List<MovingPlatform> MovingPlatformList;

        Rectangle ScreenRectangle = new Rectangle(0, 0, 1920, 1080);
        
        public void OnPlayerShoot(object source, PlayerShootEventArgs e)
        {
            ProjectileList.Add(new Bullet()
            {
                Position = e.Player.Position + new Vector2(0, -60),
                PlayerIndex = e.Player.PlayerIndex,
                Velocity = e.Velocity
            });
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
            #endregion

            //ExplosionEffect explosionEffect = new ExplosionEffect(new Vector2(heavyProjectile.Position.X, heavyProjectile.BoundingBox.Max.Y))
            //{
            //    Texture = ExplosionRingSprite
            //};
            //ExplosionEffectList.Add(explosionEffect);

            //Camera.Shake(15, 1.5f);
            #endregion

        }

        public void OnPlayerGrenade(object source, PlayerGrenadeEventArgs e)
        {
            Grenade grenade = new Grenade(e.Player.Position, new Vector2(1, 0), 2, e.Player);
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
            GrenadeList = new List<Grenade>();
            ProjectileList = new List<Projectile>();
            

            MovingPlatformList = new List<MovingPlatform>();
            Player.MovingPlatformList = MovingPlatformList;

            MovingPlatformList.Add(new MovingPlatform()
            {
                Position = new Vector2(400, 400),
                Size = new Vector2(100, 32),
                Speed = new Vector2(2, 0),
                Texture = Block
            });


            TrapList = new List<Trap>();
            Player.TrapList = TrapList;

            ItemList = new List<Item>();
            Player.ItemList = ItemList;

            DoubleBuffer = new DoubleBuffer();
            RenderManager = new RenderManager(DoubleBuffer);
            RenderManager.LoadContent(Content);

            UpdateManager = new UpdateManager(DoubleBuffer);

            UpdateManager.StartOnNewThread();

            Emitter.UpdateManager = UpdateManager;
            Emitter.RenderManager = RenderManager;

            ParticleTexture = Content.Load<Texture2D>("Particles/diamond");



            //Emitter newEmitter4 = new Emitter(ParticleTexture, new Vector2(800, 200), new Vector2(-40, 40), new Vector2(6, 10),
            //        new Vector2(1000, 1000), 0.99f, true, new Vector2(0, 360), new Vector2(-3, 3), new Vector2(0.25f, 0.5f),
            //        new Color(Color.Orange.R, Color.Orange.G, Color.Orange.B, 100),
            //        new Color(Color.OrangeRed.R, Color.OrangeRed.G, Color.OrangeRed.B, 20),
            //        0.03f, -2f, 60, 1, false, new Vector2(1080, 1080), false,
            //        null, true, true, new Vector2(0, 0), new Vector2(0, 0), 0, true, new Vector2(0, 0), true, true, 2000, null, null, false);

            //EmitterList.Add(newEmitter4);

            //Emitter newEmitter5 = new Emitter(ParticleTexture, new Vector2(800, 200), new Vector2(-40, 40), new Vector2(6, 10),
            //        new Vector2(1000, 1000), 0.99f, true, new Vector2(0, 360), new Vector2(-3, 3), new Vector2(0.25f, 0.5f),
            //        new Color(Color.Orange.R, Color.Orange.G, Color.Orange.B, 35),
            //        new Color(Color.OrangeRed.R, Color.OrangeRed.G, Color.OrangeRed.B, 5),
            //        -0.008f, -2f, 150, 2, false, new Vector2(1080, 1080), true,
            //        null, true, true, new Vector2(0, 0), new Vector2(0, 0), 0, true, new Vector2(0, 0), true, true, 1500, null, null, false);

            //EmitterList.Add(newEmitter5);

            Emitter emitter = new Emitter(ParticleTexture, new Vector2(70, 1080 / 2),
                    new Vector2(0, 360), new Vector2(0, 0.5f),
                    new Vector2(500, 1500), 1f, true, Vector2.Zero, new Vector2(-3, 3), new Vector2(0.5f, 1f),
                    new Color(Color.LimeGreen.R, Color.LimeGreen.G, Color.LimeGreen.B, 20),
                    new Color(Color.Lime.R, Color.Lime.G, Color.Lime.B, 50),
                    -0.0f, -1f, 15, 10, true, new Vector2(1080 - 64, 1080 - 64),
                    false, 0, true, true, null, null, 0.0f,
                    false, new Vector2(0.05f, 0.08f), null, null, null, true, null, null, true, false);
            emitter.CurrentChange = new Emitter.ChangeEmitter()
            {
                Active = true,
                angleRange = new Vector2(0, 180),
                ChangeTime = new Vector2(0, 1500),
                speedRange = new Vector2(0, 1),
                startColor = Color.Red,
                endColor = Color.Orange,
                gravity = 0.2f,
                timeRange = new Vector2(250, 500)
            };

            EmitterList.Add(emitter);

            Emitter emitter2 = new Emitter(ParticleTexture, new Vector2(70, 1080/2),
                    new Vector2(0, 360), new Vector2(0, 0.5f),
                    new Vector2(500, 1500), 1f, true, Vector2.Zero, new Vector2(-3, 3), new Vector2(0.5f, 1f),
                    new Color(Color.HotPink.R, Color.HotPink.G, Color.HotPink.B, 20),
                    new Color(Color.Fuchsia.R, Color.Fuchsia.G, Color.Fuchsia.B, 50),
                    0.0f, -1f, 15, 10, true, new Vector2(1080 - 64, 1080 - 64),
                    false, 0, true, true, null, null, 0.0f,
                    false, new Vector2(0.05f, 0.08f), null, null, null, true, null, null, true, false);

            EmitterList.Add(emitter2);

            

            LightList.Add(new Light()
            {
                //Color = new Color(141, 38, 10, 42),
                //Color = new Color(10, 25, 70, 5),
                //Color = Color.LightGreen,
                Color = Color.Plum,
                Active = true,
                Power = 1.7f,
                Position = new Vector3(100, 100, 100),
                Size = 800                
            });


            foreach (Tile tile in CurrentMap.TileList)
            {
                Solid solid = new Solid(Block, tile.Position, new Vector2(64, 64));
                solid.LoadContent(GameContentManager);
                SolidList.Add(solid);
            }

            RocketLauncher.Texture = Content.Load<Texture2D>("Gun");
            RocketLauncher launcher = new RocketLauncher();
            launcher.Position = new Vector2(200, 200);
            launcher.LoadContent(Content);
            ItemList.Add(launcher);

            MinePickup.Texture = Content.Load<Texture2D>("Blank");
            MinePickup mine = new MinePickup()
            {
                Position = new Vector2(500, 500)
            };
            mine.LoadContent(Content);
            ItemList.Add(mine);

            Grenade.GrenadeTexture = Content.Load<Texture2D>("GrenadeTexture");
            Grenade.Map = CurrentMap;

            Emitter.Map = CurrentMap;
            MovingPlatform.Map = CurrentMap;

            Texture = Content.Load<Texture2D>("Backgrounds/Texture");
            NormalTexture = Content.Load<Texture2D>("Backgrounds/NormalTexture");
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
            CurrentMap.LoadContent(Content);

            Texture2D ButtonTexture = Content.Load<Texture2D>("Blank");

            for (int i = 0; i < 4; i++)
            {
                PlayerJoinButtons[i] = new PlayerJoin(ButtonTexture, new Vector2(106 + (451 * i), 278), new Vector2(356, 524)); 
            }

            Player.Map = CurrentMap;            
            Projectile.Map = CurrentMap;

            Rocket.Texture = Content.Load<Texture2D>("Projectiles/RocketTexture");
            Bullet.Texture = Content.Load<Texture2D>("Projectiles/BulletTexture");

            Block = Content.Load<Texture2D>("Blank");

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

                        foreach (Solid solid in SolidList)
                        {
                            solid.Update(gameTime);
                        }

                        foreach (Item item in ItemList)
                        {
                            item.Update(gameTime);                           
                        }

                        foreach (MovingPlatform platform in MovingPlatformList)
                        {
                            platform.Update(gameTime);
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
                        }

                        foreach (Projectile projectile in ProjectileList)
                        {
                            projectile.Update(gameTime);

                            if (CurrentMap.TileList.Any(Tile => Tile.CollisionRectangle.Intersects(projectile.CollisionRectangle)))
                            {
                                for (int i = 0; i < 10; i++)
                                {
                                    Vector2 rang = new Vector2(
                                        MathHelper.ToDegrees(-(float)Math.Atan2(projectile.Velocity.Y, projectile.Velocity.X)) - 180 - 60,
                                        MathHelper.ToDegrees(-(float)Math.Atan2(projectile.Velocity.Y, projectile.Velocity.X)) - 180 + 60);

                                    Emitter emitter = new Emitter(ParticleTexture, projectile.Position - (projectile.Velocity * 0.75f),
                                        new Vector2(0, 360), new Vector2(1, 3),
                                        new Vector2(500, 1500), 1f, true, Vector2.Zero, new Vector2(-3, 3), new Vector2(0.5f, 1f),
                                        Color.HotPink, Color.Pink, 0f, (float)DoubleRange(0.5d, 1.5d), 15, 3, true, new Vector2(1080 - 64, 1080 - 64),
                                        false, 0, true, true, new Vector2(5, 7), rang, 0.2f,
                                        true, null, null, null, null, true, null, null, true, false);

                                    EmitterList.Add(emitter);
                                }

                                projectile.Active = false;
                            }
                        }

                        ProjectileList.RemoveAll(Projectile => Projectile.Active == false);

                        EmitterList[0].Position = new Vector2(EmitterList[0].Position.X + 3, 1080 / 2) + new Vector2(0, 7 * (float)Math.Sin((float)gameTime.TotalGameTime.TotalSeconds * 7));
                        EmitterList[1].Position = new Vector2(EmitterList[1].Position.X + 3, 1080 / 2) + new Vector2(0, -7 * (float)Math.Sin((float)gameTime.TotalGameTime.TotalSeconds * 7));

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

                        foreach (MovingPlatform platform in MovingPlatformList)
                        {
                            platform.Draw(spriteBatch);
                        }

                        spriteBatch.Draw(Texture, new Rectangle(0, 0, 1920, 1080), null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 1);

                        spriteBatch.Draw(Block, new Rectangle(0, 750, 1920, 80), null, Color.Red, 0, Vector2.Zero, SpriteEffects.None, 0.15f);

                        foreach (Projectile projectile in ProjectileList)
                        {
                            projectile.Draw(spriteBatch);
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
            {
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
            {
                foreach (Tile tile in CurrentMap.TileList)
                {
                    tile.DrawInfo(GraphicsDevice, BasicEffect);
                }
            }

            #region Player 1 info
            for (int i = 0; i < Players.Count(); i++)
            {
                if (Players[i] != null)
                {
                    int y = 16;
                    spriteBatch.DrawString(Font1, "Health: " + Players[i].Health.X.ToString() + "/" + Players[i].Health.Y.ToString(), new Vector2(256 + (i * 256), y), Color.Red);
                    y += 16;
                    spriteBatch.DrawString(Font1, "Trap: " + Players[i].CurrentTrap.ToString(), new Vector2(256 + (i * 256), y), Color.Red);
                    y += 16;
                    spriteBatch.DrawString(Font1, "Trap Ammo: " + Players[i].TrapAmmo.ToString(), new Vector2(256 + (i * 256), y), Color.Red);
                    y += 16;
                    spriteBatch.DrawString(Font1, "Gun: " + Players[i].CurrentGun.ToString(), new Vector2(256 + (i * 256), y), Color.Red);
                    y += 16;
                    spriteBatch.DrawString(Font1, "Gun Ammo: " + Players[i].GunAmmo.ToString(), new Vector2(256 + (i * 256), y), Color.Red);
                    y += 16;
                    spriteBatch.DrawString(Font1, "Deahts: " + Players[i].Deaths.ToString(), new Vector2(256 + (i * 256), y), Color.Red);
                }
            }
            #endregion



            spriteBatch.End();
            #endregion
            
            #region Draw to the backbuffer
            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.Black);
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive);

            if (GameState != GameState.Playing && GameState != GameState.LevelCreator)
            {
                spriteBatch.Draw(MenuRenderTarget, MenuRenderTarget.Bounds, Color.White);
            }
            else
            {
                spriteBatch.Draw(FinalMap, FinalMap.Bounds, Color.White);
                //spriteBatch.Draw(ColorMap, ColorMap.Bounds, Color.White);
                spriteBatch.Draw(BlurMap, BlurMap.Bounds, Color.White);

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
                    if (CurrentKeyboardState.IsKeyDown(Keys.P) &&
                        PreviousKeyboardState.IsKeyUp(Keys.P))
                    {
                        int stop = 10;
                    }

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

    }
}
