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
    enum GameState
    {
        MainMenu,
        ModeSelect,
        Playing,
        LevelSelect,
        Paused,
        EndMatch,
        EndRound
    };

    public enum ChangeMessageType
    {
        UpdateParticle,
        DeleteRenderData,
    };

    public enum GrenadeType
    {
        Regular, //Just explodes
        Sticky, //Sticks to surfaces (Exept bounce pads)
        Flechette, //Shoots out projectiles in a 360 degree arc when detonated
        Cluster //Drops a bunch of smaller bombs on detonation
    };

    public enum GunType
    {
        RocketLauncher,
        Shotgun,
        MachineGun
    };

    public enum DebuffType
    {
        ScrambleButtons,
        ScrambleSticks,
        ReverseGravity
    };

    public enum PowerupType
    {
        Shield
    };

    public enum GameType
    {
        DeathMatch, //Play continuosly until a score limit or time limit is reached. The player with most kills wins
        LastMan, //Play a series of matches that are won by the last person left alive
        CTF //Play rounds of Capture the flag. Best of 5 wins the whole game
    };

    public enum TrapType
    {
        TripMine
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

    public delegate void LightProjectileHappenedEventHandler(object source, LightProjectileEventArgs e);
    public class LightProjectileEventArgs : EventArgs
    {
        public LightProjectile Projectile { get; set; }
    }

    //A player is throwing a grenade
    public delegate void PlayerGrenadeHappenedEventHandler(object source, PlayerGrenadeEventArgs e);
    public class PlayerGrenadeEventArgs : EventArgs
    {
        public Player Player { get; set; }
    }

    public delegate void PlayerDiedHappenedEventHandler(object source, PlayerDiedEventArgs e);
    public class PlayerDiedEventArgs : EventArgs
    {
        public Player Player { get; set; }
    }

    public delegate void PlaceTrapHappenedEventHandler(object souce, PlaceTrapEventArgs e);
    public class PlaceTrapEventArgs : EventArgs
    {
        public Player Player { get; set; }
        public Vector2 Position;
        public TrapType TrapType;
        public float Rotation;
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

    public class Game1 : Game
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
 
        GameState CurrentGameState;
        GameState PreviousGameState;

        Player[] Players = new Player[4];
        PlayerJoin[] PlayerJoinButtons = new PlayerJoin[4];
        Map CurrentMap;
        Camera Camera = new Camera();
        Rectangle ScreenRectangle = new Rectangle(0, 0, 1920, 1080);
        
        #region RenderTargets
        RenderTarget2D UIRenderTarget, GameRenderTarget, MenuRenderTarget;
        RenderTarget2D ParticleRenderTarget;
        #endregion
        
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

        #region Textures
        #region Particle Textures
        public static Texture2D ExplosionParticle2, BOOMParticle, SNAPParticle, PINGParticle, SplodgeParticle,
                                HitEffectParticle, ToonSmoke2, ToonSmoke3, ParticleTexture, ToonSmoke1;
        #endregion

        Texture2D Block, Texture, NormalTexture;
        Texture2D RedFlagTexture, BlueFlagTexture;

        #region Icon Textures
        public static Texture2D GrenadeIcon;
        #endregion
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

        BasicEffect BasicEffect, BulletTrailEffect;

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

        //List<Light> LightList = new List<Light>();
        List<Solid> SolidList = new List<Solid>();

        //Color AmbientLight = new Color(0.1f, 0.1f, 0.1f, 1f);
        Color AmbientLight = new Color(0.3f, 0.3f, 0.3f, 1f);
        //Color AmbientLight = new Color(0.25f, 0.25f, 0.25f, 1f);

        #endregion

        #region Effects
        Effect BlurEffect, LightCombined, LightEffect;
        Effect RaysEffect, DepthEffect, ShockWaveEffect;
        #endregion

        #region Lists
        List<Trap> TrapList;
        List<Item> ItemList;
        List<Grenade> GrenadeList;
        List<SubGrenade> SubGrenadeList;
        List<Projectile> ProjectileList;
        List<MovingPlatform> MovingPlatformList;
        #endregion

        #region Debugging
        bool DrawDiagnostics = false;
        bool DebugBoxes = false;
        bool TileBoxes = false;

        List<MyRay> myRayList = new List<MyRay>();
        #endregion

        #region Fonts
        SpriteFont Font1;
        #endregion

        #region Menu Options
        List<string> LevelList = new List<string>();
        int SelectedLevelIndex = 0;

        List<string> PauseMenuOptions = new List<string>() { "Level Select", "Main Menu", "Exit" };
        int SelectedPauseMenu = 0;

        List<string> ModeSelectOptions = new List<string>() { "Death Match", "Last Man Standing", "Capture the Flag" };
        int SelectedModeMenu = 0;
        #endregion
        
        Texture2D ShotgunShell;

        List<BulletTrail> BulletTrailList = new List<BulletTrail>();
        List<Gib> GibList = new List<Gib>();
        List<ShockWave> ShockWaveList = new List<ShockWave>();

        Liquid BloodLiquid;

        int MaxScore = 2;
        
        //Time between the final death and actually moving to the EndMatch state
        Vector2 MatchEndTimer = new Vector2(0, 1000);
        Vector2 EndMatchTime = new Vector2(0, 1000);
        float MatchEndFade = 0;
        float PaintSplatterOpacity = 0;

        Player WinningPlayer;

        Texture2D PaintStreak1, PaintStreak2, PaintStreak3, Splatter1, GameOverTexture;
        List<float> PaintStreakIncrements = new List<float>();
        List<float> PaintStreakValues = new List<float>();
        List<Vector2> PaintDelayValues = new List<Vector2>();
        List<int> PaintAngleOffsets = new List<int>();

        #region UI Graphics
        public static Texture2D AButtonTexture, BButtonTexture, XButtonTexture, YButtonTexture;
        #endregion

        #region Events
        public void OnPlayerShoot(object source, PlayerShootEventArgs e)
        {
            if (e == null)
            {
                throw new ArgumentNullException(nameof(e));
            }
            //e.Player.Velocity.X += (-e.Velocity.X/2);

            Rocket rocket = new Rocket()
            {
                Position = e.Player.BarrelEnd,
                PlayerIndex = e.Player.PlayerIndex,
                Velocity = e.Velocity
            };

            Vector2 rang = new Vector2(MathHelper.ToDegrees(-(float)Math.Atan2(rocket.Velocity.Y, rocket.Velocity.X)) - 180 - 60,
                                       MathHelper.ToDegrees(-(float)Math.Atan2(rocket.Velocity.Y, rocket.Velocity.X)) - 180 + 60);

            Emitter emitter = new Emitter(
                ToonSmoke1, new Vector2(rocket.Position.X, rocket.Position.Y), rang,
                new Vector2(1f, 2f), new Vector2(640, 960), 1f, false, new Vector2(-20, 20), new Vector2(-0.5f, 0.5f),
                new Vector2(0.03f, 0.05f), Color.DarkGray, Color.Gray, -0.00f, -1, 1, 5, false, new Vector2(0, 720), true, 0,
                null, null, null, null, null, false, new Vector2(0.08f, 0.13f), false, false, 150f,
                false, false, false, false, true);

            ProjectileList.Add(rocket);

            rocket.EmitterList.Add(emitter);
        }

        public void OnLightProjectileFired(object source, LightProjectileEventArgs e)
        {
            if (e == null)
            {
                throw new ArgumentNullException(nameof(e));
            }

            LightProjectile projectile = e.Projectile;

            //VerletList.Add(new VerletObject(e.Projectile.Position, new Vector2(Math.Sign(projectile.Ray.Direction.X) * Random.Next(2, 8), Random.Next(3, 10))));

            switch (projectile.LightProjectileType)
            {
                #region Shotgun
                case LightProjectileType.Shotgun:
                    {
                        if (source as Player != null)
                        {
                            Emitter HitEffect1 = new Emitter(HitEffectParticle, (source as Player).BarrelEnd, new Vector2(
                                                            MathHelper.ToDegrees(-(float)Math.Atan2(e.Projectile.Ray.Direction.Y, e.Projectile.Ray.Direction.X)) - 50,
                                                            MathHelper.ToDegrees(-(float)Math.Atan2(e.Projectile.Ray.Direction.Y, e.Projectile.Ray.Direction.X)) + 50),
                                                            new Vector2(9f, 14f),
                                                            new Vector2(150f, 250f), 1f, false, new Vector2(0f, 360f), new Vector2(-2f, 2f),
                                                            new Vector2(0.15f, 0.15f),
                                                            new Color(255, 255, 0, 255) * 0.5f,
                                                            new Color(255, 255, 255, 255) * 0.25f, 0f, 0.025f, 50f, 2, false, new Vector2(0f, 1080), true,
                                                            (projectile.Position.Y + 8) / 1080f,
                                                            false, false, null, null, 0f, true, new Vector2(0.21f, 0.21f), false, false, 0f,
                                                            false, false, true, null, true, true)
                            {
                                Emissive = true
                            };

                            (source as Player).FlashEmitterList.Add(HitEffect1);
                            //EmitterList.Add(HitEffect1);

                            //Emitter FlashEmitter = new Emitter(HitEffectParticle, new Vector2((source as Player).BarrelEnd.X, (source as Player).BarrelEnd.Y),
                            //                                new Vector2(
                            //                                MathHelper.ToDegrees(-(float)Math.Atan2(e.Projectile.Ray.Direction.Y, e.Projectile.Ray.Direction.X)) - 50,
                            //                                MathHelper.ToDegrees(-(float)Math.Atan2(e.Projectile.Ray.Direction.Y, e.Projectile.Ray.Direction.X)) + 50),
                            //                                new Vector2(8, 12), new Vector2(100, 200), 1f, false,
                            //                                new Vector2(0, 0), new Vector2(0, 0), new Vector2(0.25f, 0.25f),
                            //                                new Color(Color.Yellow.R, Color.Yellow.G, Color.Yellow.B, 25) * 0.5f,
                            //                                new Color(Color.Orange.R, Color.Orange.G, Color.Orange.B, 25) * 0.5f, 0f, 0.05f, 100, 2, false, new Vector2(0, 1080), true,
                            //                                1.0f, null, null, null, null, null, true, new Vector2(0.25f, 0.25f), false, false,
                            //                                null, false, false, false)
                            //{
                            //    Emissive = false
                            //};
                            //EmitterList.Add(FlashEmitter);
                        }
                        //ShellCasing shell = new ShellCasing(projectile.Position, new Vector2(12, -5), ShotgunShell);
                        //ShellCasingList.Add(shell);
                        //myRayList.Add(new MyRay()
                        //{
                        //    position = new Vector3(projectile.Position, 0),
                        //    direction = projectile.Ray.Direction,
                        //    length = 250
                        //});
                    }
                    break;
                #endregion

                #region MachineGun
                case LightProjectileType.MachineGun:
                    {
                        myRayList.Add(new MyRay()
                        {
                            position = new Vector3(projectile.Position, 0),
                            direction = projectile.Ray.Direction,
                            length = 800
                        });
                    }
                    break;
                    #endregion
            }

            void CheckCollision()
            {
                float? hitDist;
                Vector2 endPosition;
                Vector2 sourcePosition = projectile.Position;
                List<CollisionSolid> ObjectList = new List<CollisionSolid>();
                List<Player> PlayerIntersections = new List<Player>();
                List<Tile> TileIntersections = new List<Tile>();
                CollisionSolid colObject = new CollisionSolid();

                //Vector2 angleRange = new Vector2(
                //                        MathHelper.ToDegrees(-(float)Math.Atan2(projectile.Ray.Direction.Y, projectile.Ray.Direction.X)) - 180 - 60,
                //                        MathHelper.ToDegrees(-(float)Math.Atan2(projectile.Ray.Direction.Y, projectile.Ray.Direction.X)) - 180 + 60);

                Vector2 angleRange = GetAngleRange(projectile, 60);

                for (int x = 0; x < CurrentMap.DrawTiles.GetLength(0); x++)
                {
                    for (int y = 0; y < CurrentMap.DrawTiles.GetLength(1); y++)
                    {
                        if (CurrentMap.DrawTiles[x, y] != null &&
                            CurrentMap.DrawTiles[x, y].BoundingBox.Intersects(projectile.Ray) < projectile.RangeLength)
                        {
                            TileIntersections.Add(CurrentMap.DrawTiles[x, y]);
                        }
                    }
                }

                PlayerIntersections = Players.ToList().FindAll(movingObject =>
                    movingObject != null &&
                    movingObject != source &&
                    movingObject.BoundingBox.Intersects(projectile.Ray) < projectile.RangeLength);


                endPosition = new Vector2(sourcePosition.X + (projectile.Ray.Direction.X * Random.Next((int)(projectile.RangeLength * 0.65f), (int)projectile.RangeLength)),
                                          sourcePosition.Y + (projectile.Ray.Direction.Y * Random.Next((int)(projectile.RangeLength * 0.65f), (int)projectile.RangeLength)));

                ObjectList.AddRange(PlayerIntersections);
                ObjectList.AddRange(TileIntersections);

                colObject = ObjectList.OrderBy(Collision => Collision.BoundingBox.Intersects(projectile.Ray)).FirstOrDefault();

                if (colObject != null)
                {
                    hitDist = colObject.BoundingBox.Intersects(projectile.Ray);

                    if (hitDist != null)
                    {
                        endPosition = new Vector2(sourcePosition.X + (projectile.Ray.Direction.X * (float)hitDist),
                                                  sourcePosition.Y + (projectile.Ray.Direction.Y * (float)hitDist));

                        #region Hit a PLAYER
                        if ((colObject as Player) != null)
                        {
                            (colObject as Player).LastDamageSource = (int)(source as Player).PlayerIndex;
                            endPosition += new Vector2(projectile.Ray.Direction.X, projectile.Ray.Direction.Y) * (32 + Random.Next(-8, 8));
                            (colObject as Player).Health.X -= projectile.Damage;

                            BloodLiquid.AddMetaballs((colObject as Player).Position, 15, new Vector2(0.8f, 1.2f),
                                new Vector2(-5, 5), new Vector2(-1, -5));
                        }
                        #endregion

                        #region Hit a TILE
                        if ((colObject as Tile) != null)
                        {
                            switch (projectile.LightProjectileType)
                            {
                                case LightProjectileType.MachineGun:
                                    {
                                        Emitter HitEffect1 = new Emitter(HitEffectParticle,
                                        endPosition, GetAngleRange(projectile, 80), new Vector2(5f, 8f),
                                        new Vector2(250f, 500f), 1f, false, new Vector2(0f, 360f), new Vector2(-2f, 2f),
                                        new Vector2(0.15f, 0.15f), new Color(255, 255, 191, 255) * 0.5f,
                                        new Color(255, 255, 255, 255) * 0.25f, 0f, 0.05f, 50f, 7, false, new Vector2(0f, 1080), true,
                                        (projectile.Position.Y + 8) / 1080f,
                                        false, false, null, null, 0f, true, new Vector2(0.11f, 0.11f), false, false, 0f,
                                        false, false, false, null)
                                        {
                                            Emissive = true
                                        };
                                        EmitterList.Add(HitEffect1);

                                        for (int i = 0; i < 5; i++)
                                        {
                                            Emitter emitter = new Emitter(ParticleTexture, endPosition,
                                               new Vector2(0, 360), new Vector2(0, 3),
                                               new Vector2(200, 500), 1f, true, Vector2.Zero, new Vector2(-3, 3), new Vector2(0.5f, 1f),
                                               ColorAjustAlpha(Color.OrangeRed, 20), ColorAjustAlpha(Color.Gold, 50),
                                               0f, (float)DoubleRange(0.15d, 0.5d), 15, 3, true, new Vector2(1080 - 64, 1080 - 64),
                                               false, 0, true, true, new Vector2(5, 7), GetAngleRange(projectile, 80), 0.0f,
                                               false, new Vector2(0.05f, 0.03f), null, null, null, true, null, null, true, false);

                                            EmitterList.Add(emitter);
                                        }

                                        //BOOMEmitter = new Emitter(SNAPParticle, collisionEnd,
                                        //  new Vector2(0, 0), new Vector2(0.001f, 0.001f), new Vector2(250, 250), 1f, false,
                                        //  new Vector2(-25, 25), new Vector2(0, 0), new Vector2(0.15f, 0.15f),
                                        //  Color.White, Color.White, 0f, 0.05f, 50, 1, false, new Vector2(0, 1080), true,
                                        //  collisionEnd.Y + 4 / 1080f, null, null, null, null, null, false, new Vector2(0.11f, 0.11f), false, false,
                                        //  null, false, false, true);

                                        //Emitter snap = new Emitter(SNAPParticle, endPosition, 
                                        //    new Vector2(0, 0), new Vector2(0, 0), new Vector2(200, 400), 1.0f, false, 
                                        //    new Vector2(-25, 25), new Vector2(0, 0), new Vector2(0.3f, 0.3f), 
                                        //    Color.White, Color.White, 0f, 0.05f, 100f, 1, false,
                                        //    new Vector2(0, 1080), true);
                                        EmitterList.Add(SnapPing(endPosition));
                                        //Emitter snappy = SnapPing(new Vector2(200, 200));
                                        //EmitterList.Add(snappy);
                                    }
                                    break;

                                case LightProjectileType.Shotgun:
                                    {
                                        Emitter HitEffect1 = new Emitter(HitEffectParticle,
                                        endPosition, GetAngleRange(projectile, 80), new Vector2(5f, 8f),
                                        new Vector2(250f, 500f), 1f, false, new Vector2(0f, 360f), new Vector2(-2f, 2f),
                                        new Vector2(0.15f, 0.15f), new Color(255, 255, 191, 255) * 0.5f,
                                        new Color(255, 255, 255, 255) * 0.25f, 0f, 0.05f, 50f, 7, false, new Vector2(0f, 1080), true,
                                        (projectile.Position.Y + 8) / 1080f,
                                        false, false, null, null, 0f, true, new Vector2(0.11f, 0.11f), false, false, 0f,
                                        false, false, false, null)
                                        {
                                            Emissive = true
                                        };
                                        EmitterList.Add(HitEffect1);

                                        for (int i = 0; i < 3; i++)
                                        {
                                            Emitter emitter = new Emitter(ParticleTexture, endPosition,
                                               new Vector2(0, 360), new Vector2(0, 3),
                                               new Vector2(200, 500), 1f, true, Vector2.Zero, new Vector2(-3, 3), new Vector2(0.5f, 1f),
                                               ColorAjustAlpha(Color.Lime, 20), ColorAjustAlpha(Color.ForestGreen, 50),
                                               0f, (float)DoubleRange(0.15d, 0.5d), 15, 3, true, new Vector2(1080 - 64, 1080 - 64),
                                               false, 0, true, true, new Vector2(5, 7), GetAngleRange(projectile, 80), 0.0f,
                                               false, new Vector2(0.05f, 0.03f), null, null, null, true, null, null, true, false);

                                            EmitterList.Add(emitter);
                                        }
                                    }
                                    break;
                            }



                        }
                        #endregion
                    }
                }
                else
                {
                    Emitter HitEffect2 = new Emitter(HitEffectParticle,
                                            endPosition - new Vector2(projectile.Ray.Direction.X, projectile.Ray.Direction.Y) * 5,
                                            GetAngleRange(projectile, 5, true), new Vector2(2f, 6f),
                                            new Vector2(250f, 350f), 1f, false, new Vector2(0f, 360f), new Vector2(-2f, 2f),
                                            new Vector2(0.05f, 0.05f),
                                            ColorAjustAlpha(Color.ForestGreen, 20), ColorAjustAlpha(Color.White, 60),
                                            //new Color(255, 255, 191, 255), new Color(255, 255, 255, 255), 
                                            0f, 0.05f, 50f, 7, false, new Vector2(0f, 1080), true,
                                            (projectile.Position.Y + 8) / 1080f,
                                            false, false, null, null, 0f, true, new Vector2(0.11f, 0.11f), false, false, 0f,
                                            false, false, false, null)
                    {
                        Emissive = true
                    };
                    EmitterList.Add(HitEffect2);
                }

                switch (projectile.LightProjectileType)
                {
                    case LightProjectileType.MachineGun:
                        {
                            BulletTrailList.Add(new BulletTrail(sourcePosition, endPosition, ColorAjustAlpha(Color.Red, 30)));
                        }
                        break;

                    case LightProjectileType.Shotgun:
                        {
                            BulletTrailList.Add(new BulletTrail(sourcePosition, endPosition, ColorAjustAlpha(Color.LimeGreen, 30)));
                        }
                        break;
                }
            }

            CheckCollision();
        }
        
        public void OnPlayerDied(object source, PlayerDiedEventArgs e)
        {
            if (e == null)
            {
                throw new ArgumentNullException(nameof(e));
            }

            //TODO: These gibs need to react to how the player died. e.g. an explosion should influence their velocity and direction
            //same with rock shots.
            //for (int i = 0; i < 20; i++)
            //{
            //    float angle = MathHelper.ToRadians((float)RandomDouble(-180, 0));
            //    Vector2 dir = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
            //    dir.Normalize();

            //    Gib newGib = new Gib(e.Player.Position, dir, Random.Next(1, 5), Texture, Texture, Color.Red);

            //    Emitter emitter = new Emitter(
            //    SplodgeParticle, e.Player.Position, new Vector2(0, 360),
            //    new Vector2(0f, 0.5f), new Vector2(500, 1500), 0.85f, false, new Vector2(0, 360), new Vector2(-0.5f, 0.5f),
            //    new Vector2(1f, 1.85f), Color.DarkRed, Color.DarkRed, 0.1f, Random.Next(1000, 2000)/1000f, 1, 3, true, new Vector2(1080-58, 1080-58), true, 0,
            //    true, null, null, null, null, true, null, true, true, 150f,
            //    true, false, false, false, true);

            //    newGib.EmitterList.Add(emitter);
            //    GibList.Add(newGib);
            //}

            if (Players[e.Player.LastDamageSource] != null)
                Players[e.Player.LastDamageSource].Score++;

            //Velocity adjusted blood globs
            //BloodLiquid.AddMetaballs(e.Player.Position, 40, Vector2.Zero, new Vector2(-5 + e.Player.Velocity.X, 5 + e.Player.Velocity.X), new Vector2(-5, -1));

            BloodLiquid.AddMetaballs(e.Player.Position, 40, Vector2.Zero, new Vector2(-5, 5), new Vector2(-5, -1));

            e.Player.Health.X = e.Player.Health.Y;
            e.Player.GunAmmo = 15;
            e.Player.Velocity = new Vector2(0, 0);
            e.Player.Active = false;

            e.Player.Respawn();
            
        }

        public void OnExplosionHappened(object source, ExplosionEventArgs e)
        {
            if (e == null)
            {
                throw new ArgumentNullException(nameof(e));
            }

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
                    ColorAjustAlpha(Color.OrangeRed, 20),
                    ColorAjustAlpha(Color.Gold, 50),
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
            #endregion

            //ShockWaveEffect.Parameters["CenterCoords"].SetValue(new Vector2(1 / (1920 / explosion.Position.X), 1 / (1080 / explosion.Position.Y)));
            //ShockWaveEffect.Parameters["WaveParams"].SetValue(new Vector4(10, 0.5f, 0.1f, 60));
            //ShockWaveEffect.Parameters["CurrentTime"].SetValue(0);

            ShockWaveList.Add(new ShockWave(new Vector2(1 / (1920 / explosion.Position.X), 1 / (1080 / explosion.Position.Y)), new Vector3(10.0f, 2.5f, 0.1f), 400));


            Light light = new Light()
            {
                Color = Color.DarkOrange,
                Active = true,
                Power = 1.7f,
                Position = new Vector3(explosion.Position.X, explosion.Position.Y, 100),
                Size = 800,
                CurTime = 0,
                MaxTime = 100
            };

            CurrentMap.LightList.Add(light);

            foreach (Player player in Players.Where(Player => Player != null))
            {
                float dist = Vector2.Distance(new Vector2(player.DestinationRectangle.Center.X, player.DestinationRectangle.Center.Y), explosion.Position);

                Camera.Shake(30, 2);


                if (dist < explosion.BlastRadius)
                {
                    player.LastDamageSource = (int)(explosion.Source as Player).PlayerIndex;
                    player.Health.X -= explosion.Damage;



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
            if (e == null)
            {
                throw new ArgumentNullException(nameof(e));
            }

            Grenade grenade = new Grenade(e.Player.Position, new Vector2(16, 0) * e.Player.AimDirection, e.Player, e.Player.CurrentGrenade);
            GrenadeList.Add(grenade);
        }

        public void OnPlaceTrap(object source, PlaceTrapEventArgs e)
        {
            if (e == null)
            {
                throw new ArgumentNullException(nameof(e));
            }

            Trap trap;

            switch (e.TrapType)
            {
                case TrapType.TripMine:
                    {
                        trap = new TripMine()
                        {
                            Texture = Block,
                            Position = e.Position,
                            Rotation = e.Rotation,
                            SourcePlayer = e.Player
                        };

                        //Get rotation of beam

                        //float myRot = e.Rotation - (float)Math.PI / 2;

                        //Vector2 vec = new Vector2(
                        //    (float)Math.Cos(myRot), 
                        //    (float)Math.Sin(myRot)) * 256;


                        (trap as TripMine).Laser = new LaserBeam(e.Position, trap.Rotation, e.Player.PlayerColor);
                        (trap as TripMine).Laser.SetVerts();

                        TrapList.Add(trap);
                    }
                    break;
            }
        }
        #endregion

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
            CurrentGameState = GameState.MainMenu;

            ExplosionHappenedEvent += OnExplosionHappened;

            base.Initialize();
        }


        protected void LoadGameContent()
        {

            #region End Match Paint
            PaintStreak1 = Content.Load<Texture2D>("PaintStreak1");
            PaintStreak2 = Content.Load<Texture2D>("PaintStreak2");
            PaintStreak3 = Content.Load<Texture2D>("PaintStreak3");

            Splatter1 = Content.Load<Texture2D>("Splatter1");
            GameOverTexture = Content.Load<Texture2D>("GameOver");

            PaintStreakIncrements.Clear();
            PaintStreakValues.Clear();
            PaintDelayValues.Clear();

            MatchEndFade = 0;
            PaintSplatterOpacity = 0;
            EndMatchTime.X = 0;
            MatchEndTimer.X = 0;

            for (int i = 0; i < 6; i++)
            {
                PaintStreakIncrements.Add(Random.Next(110, 160));
                PaintStreakValues.Add(0);

                //PaintDelayValues.Add(new Vector2(0, 100 * i + (Random.Next(-60, 60))));
                PaintDelayValues.Add(new Vector2(0, Random.Next(0 + (20 * i), 400 + (100 * i))));
            }
            #endregion

            MovingObject.Map = CurrentMap;
            LaserBeam.Map = CurrentMap;

            GrenadeList = new List<Grenade>();
            SubGrenadeList = new List<SubGrenade>();

            Player.Players = Players;
            
            ProjectileList = new List<Projectile>();
            Player.ProjectileList = ProjectileList;

            TrapList = new List<Trap>();
            Player.TrapList = TrapList;
            
            MovingPlatformList = new List<MovingPlatform>();

            MovingPlatform platform1 = new MovingPlatform()
            {
                Texture = Block,
                Position = new Vector2(400, 250),
                Velocity = new Vector2(2, 0)
            };

            MovingPlatformList.Add(platform1);

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

            CurrentMap.LightList.Add(new Light()
            {
                Color = Color.OrangeRed,
                Active = true,
                Power = 1.7f,
                Position = new Vector3(100, 100, 5000),
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

            #region Load Item Textures
            ShieldPickup.Texture = GameContentManager.Load<Texture2D>("shieldcomb");
            ShotgunPickup.Texture = GameContentManager.Load<Texture2D>("Gun");
            RocketLauncherPickup.Texture = GameContentManager.Load<Texture2D>("Gun");
            MachineGunPickup.Texture = GameContentManager.Load<Texture2D>("Gun");
            #endregion

            #region Load Icon Textures
            GrenadeIcon = GameContentManager.Load<Texture2D>("Icons/GrenadeIcon");
            #endregion



            BloodLiquid = new Liquid(GraphicsDevice, Color.DarkRed);

            Grenade.Texture = GameContentManager.Load<Texture2D>("GrenadeTexture");
            SubGrenade.Texture = GameContentManager.Load<Texture2D>("GrenadeTexture");
            Gib.Texture = GameContentManager.Load<Texture2D>("Particles/Splodge");

            //RocketLauncher.Texture = GameContentManager.Load<Texture2D>("Gun");
            

            Player.ShieldTexture = GameContentManager.Load<Texture2D>("PlayerShield");
            Player.GunTexture = GameContentManager.Load<Texture2D>("Gun");
            
            ShotgunShell = GameContentManager.Load<Texture2D>("ShotgunShell");

            //MinePickup.Texture = GameContentManager.Load<Texture2D>("Blank");
            //ShieldPickup.Texture = GameContentManager.Load<Texture2D>("Crate");

            //RedFlag.Texture = GameContentManager.Load<Texture2D>("RedFlag");
            //BlueFlag.Texture = GameContentManager.Load<Texture2D>("BlueFlag");
            
            Emitter.Map = CurrentMap;
            VerletObject.Node.Map = CurrentMap;

            //#region Guns
            //RocketLauncher launcher = new RocketLauncher()
            //{
            //    Position = new Vector2(200, 200)
            //};
            //launcher.LoadContent(Content);
            //ItemList.Add(launcher);            

            //FlameThrower flameThrower = new FlameThrower()
            //{
            //    Position = new Vector2(800, 500)
            //};
            //flameThrower.LoadContent(Content);
            //ItemList.Add(flameThrower);

            //FlameThrower flameThrower2 = new FlameThrower()
            //{
            //    Position = new Vector2(300, 500)
            //};
            //flameThrower2.LoadContent(Content);
            //ItemList.Add(flameThrower2);
            //#endregion

            //ShieldPickup shieldPickup = new ShieldPickup()
            //{
            //    Position = new Vector2(1200, 800)
            //};
            //shieldPickup.LoadContent(GameContentManager);
            //ItemList.Add(shieldPickup);

            //MinePickup mine = new MinePickup() { Position = new Vector2(500, 500) };
            //mine.LoadContent(GameContentManager);
            //ItemList.Add(mine);

            //#region Red Flag

            ////RedFlag redFlag = new RedFlag()
            ////{
            ////    Position = new Vector2(400, 400)
            ////};
            ////redFlag.Initialize();
            ////ItemList.Add(redFlag); 
            //#endregion

            //#region Blue Flag

            //BlueFlag blueFlag = new BlueFlag()
            //{
            //    Position = new Vector2(900, 400)
            //};
            //blueFlag.Initialize();
            //ItemList.Add(blueFlag); 
            //#endregion
                        
            Texture = GameContentManager.Load<Texture2D>("Backgrounds/Texture");
            NormalTexture = GameContentManager.Load<Texture2D>("Backgrounds/NormalTexture");

            Camera.CancelShake();

            for (int i = 0; i < 4; i++)
            {
                if (Players[i] != null)
                    Players[i].Respawn();
            }
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
            ShockWaveEffect = Content.Load<Effect>("Shaders/Shockwave");

            RaysEffect.Parameters["Projection"].SetValue(Projection);
            BlurEffect.Parameters["Projection"].SetValue(Projection);
            ShockWaveEffect.Parameters["Projection"].SetValue(Projection);

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

            BulletTrailEffect = new BasicEffect(GraphicsDevice)
            {
                VertexColorEnabled = true,
                Projection = Projection
            };

            UIRenderTarget = new RenderTarget2D(GraphicsDevice, 1920, 1080);
            GameRenderTarget = new RenderTarget2D(GraphicsDevice, 1920, 1080);
            MenuRenderTarget = new RenderTarget2D(GraphicsDevice, 1920, 1080);
            ParticleRenderTarget = new RenderTarget2D(GraphicsDevice, 1920, 1080);

            Font1 = Content.Load<SpriteFont>("Font1");

            //CurrentMap = new Map();
            //CurrentMap.Initialize();
            //CurrentMap.LoadContent(Content);

            Texture2D ButtonTexture = Content.Load<Texture2D>("Blank");

            for (int i = 0; i < 4; i++)
            {
                PlayerJoinButtons[i] = new PlayerJoin(ButtonTexture, new Vector2(106 + (451 * i), 278), new Vector2(356, 524)); 
            }

            Rocket.Texture = Content.Load<Texture2D>("Projectiles/RocketTexture");
            BulletTrail.Texture = Content.Load<Texture2D>("Segment");
            Metaball.MetaballTexture = Content.Load<Texture2D>("Metaball");
            Metaball.MetaballSquashed = Content.Load<Texture2D>("MetaballSquashed");

            Block = Content.Load<Texture2D>("Blank");

            HealthBar.Texture = Block;

            ExplosionParticle2 = Content.Load<Texture2D>("Particles/ExplosionParticle2");
            BOOMParticle = Content.Load<Texture2D>("Particles/BOOM");
            SNAPParticle = Content.Load<Texture2D>("Particles/SNAP");
            PINGParticle = Content.Load<Texture2D>("Particles/PING");
            SplodgeParticle = Content.Load<Texture2D>("Particles/Splodge");
            HitEffectParticle = Content.Load<Texture2D>("Particles/HitEffectParticle");
            ToonSmoke1 = Content.Load<Texture2D>("Particles/ToonSmoke/ToonSmoke1");
            ToonSmoke2 = Content.Load<Texture2D>("Particles/ToonSmoke/ToonSmoke2");
            ToonSmoke3 = Content.Load<Texture2D>("Particles/ToonSmoke/ToonSmoke3");
            
            #region UI Textures
            AButtonTexture = Content.Load<Texture2D>("UI/AButton");
            BButtonTexture = Content.Load<Texture2D>("UI/BButton");
            XButtonTexture = Content.Load<Texture2D>("UI/XButton");
            YButtonTexture = Content.Load<Texture2D>("UI/YButton"); 
            #endregion

            RedFlagTexture = Content.Load<Texture2D>("RedFlag");
            BlueFlagTexture = Content.Load<Texture2D>("BlueFlag");
            Player.BlueFlagTexture = BlueFlagTexture;
            Player.RedFlagTexture = RedFlagTexture;            

            ItemList = new List<Item>();
            Player.ItemList = ItemList;
            ItemSpawn.ItemList = ItemList;
            Map.ItemList = ItemList;
            //ProjectileList.Add(new Rocket() { Position = new Vector2(80, 80), Velocity = new Vector2(1, 0) });

        }
        
        protected override void UnloadContent()
        {

        }

        protected void UnloadGameContent()
        {
            GameContentManager.Unload();
        }

        
        protected override void Update(GameTime gameTime)
        {
            CurrentKeyboardState = Keyboard.GetState();
            CurrentMouseState = Mouse.GetState();
            PreviousGameState = CurrentGameState;

            for (int i = 0; i < 4; i++)
            {
                 CurrentGamePadStates[i] = GamePad.GetState((PlayerIndex)i);
            }
            
            switch (CurrentGameState)
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
                                if (PlayerJoinButtons[i].Occupied == true)
                                {
                                    ListLevels();                                    
                                    CurrentGameState = GameState.LevelSelect;
                                }

                                PlayerJoinButtons[i].Occupied = true;

                                Players[i] = new Player((PlayerIndex)i);
                                PlayerJoinButtons[i].Player = Players[i];
                                Players[i].LoadContent(Content);

                                Players[i].PlayerShootHappened += OnPlayerShoot;
                                Players[i].LightProjectileHappened += OnLightProjectileFired;
                                Players[i].PlayerDiedHappened += OnPlayerDied;
                                Players[i].PlayerGrenadeHappened += OnPlayerGrenade;
                                Players[i].PlaceTrapHappened += OnPlaceTrap;
                            }
                            #endregion

                            #region Player backed out
                            if (CurrentGamePadStates[i].IsButtonUp(Buttons.B) &&
                                PreviousGamePadStates[i].IsButtonDown(Buttons.B))
                            {
                                if (PlayerJoinButtons[i].Occupied == false)
                                {
                                    for (int p = 0; p < 4; p++)
                                    {
                                        PlayerJoinButtons[p].Occupied = false;
                                        Players[p] = null;
                                    }
                                }
                                else
                                {
                                    PlayerJoinButtons[i].Occupied = false;
                                    Players[i] = null;
                                }                                
                            }
                            #endregion
                        }
                    }
                    break;
                #endregion

                #region Level Select
                case GameState.LevelSelect:
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            if (CurrentGamePadStates[i].IsButtonUp(Buttons.DPadDown) &&
                                PreviousGamePadStates[i].IsButtonDown(Buttons.DPadDown))
                            {
                                SelectedLevelIndex++;
                            }

                            if (CurrentGamePadStates[i].IsButtonUp(Buttons.DPadUp) &&
                                PreviousGamePadStates[i].IsButtonDown(Buttons.DPadUp))
                            {
                                SelectedLevelIndex--;
                            }

                            if (CurrentGamePadStates[i].IsButtonUp(Buttons.A) &&
                                PreviousGamePadStates[i].IsButtonDown(Buttons.A))
                            {
                                LoadLevel(Path.GetFileName(LevelList[SelectedLevelIndex]));
                                LoadGameContent();
                                CurrentGameState = GameState.Playing;
                            }

                            if (CurrentGamePadStates[i].IsButtonUp(Buttons.B) &&
                                PreviousGamePadStates[i].IsButtonDown(Buttons.B))
                            {
                                CurrentGameState = GameState.MainMenu;
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
                            if (CurrentGamePadStates[i].IsButtonUp(Buttons.Start) &&
                                PreviousGamePadStates[i].IsButtonDown(Buttons.Start))
                            {
                                CurrentGameState = GameState.Paused;
                            }
                        }

                        foreach (Light light in CurrentMap.LightList)
                        {
                            light.Update(gameTime);
                        }

                        foreach (ShockWave shockWave in ShockWaveList)
                        {
                            shockWave.Update(gameTime);
                        }

                        BloodLiquid.Update(gameTime);

                        ShockWaveList.RemoveAll(Shock => Shock.Active == false);

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

                            CurrentMap.LightList.Add(light);                           
                        }

                        //ShockWaveEffect.Parameters["CurrentTime"].SetValue(ShockWaveEffect.Parameters["CurrentTime"].GetValueSingle() + (float)(gameTime.ElapsedGameTime.TotalSeconds));

                        foreach (Gib gib in GibList)
                        {
                            gib.Update(gameTime);
                            gib.UpdateEmitters(gameTime);                            
                        }
                        
                        GibList.RemoveAll(Gib => Gib.Active == false);

                        foreach (BulletTrail trail in BulletTrailList)
                        {
                            trail.Update(gameTime);
                        }
                        

                        CurrentMap.CheckCollisions();
                        CurrentMap.Update(gameTime);
                        
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

                        foreach (SubGrenade subGrenade in SubGrenadeList)
                        {
                            subGrenade.Update(gameTime);

                            if (subGrenade.Active == false)
                            {
                                Explosion explosion = new Explosion()
                                {
                                    BlastRadius = 50,
                                    Damage = 40,
                                    Position = subGrenade.Position,
                                    Source = subGrenade.Source
                                };

                                CreateExplosion(explosion, subGrenade);
                            }
                        }

                        foreach (Grenade grenade in GrenadeList)
                        {
                            grenade.Update(gameTime);

                            if (grenade.Active == false)
                            {
                                switch (grenade.GrenadeType)
                                {
                                    case GrenadeType.Regular:
                                        {
                                            Explosion explosion = new Explosion()
                                            {
                                                BlastRadius = grenade.BlastRadius,
                                                Damage = grenade.Damage,
                                                Position = grenade.Position,
                                                Source = grenade.Source
                                            };

                                            CreateExplosion(explosion, grenade);
                                        }
                                        break;

                                    case GrenadeType.Cluster:
                                        {
                                            Explosion explosion = new Explosion()
                                            {
                                                BlastRadius = 50,
                                                Damage = 20,
                                                Position = grenade.Position,
                                                Source = grenade.Source
                                            };

                                            CreateExplosion(explosion, grenade);

                                            for (int i = 0; i < 5; i++)
                                            {
                                                SubGrenade subGrenade = new SubGrenade(grenade.Position, new Vector2((int)Random.Next(-5, 5), (int)Random.Next(-10, 0)), grenade.Source, Random.Next(800, 1300));
                                                SubGrenadeList.Add(subGrenade);
                                            }
                                        }
                                        break;
                                }                            
                                
                            }
                        }

                        foreach (Trap trap in TrapList.Where(Trap => Trap.Active == true))
                        {
                            trap.Update(gameTime);
                        }

                        Camera.Update(gameTime);

                        GrenadeList.RemoveAll(Grenade => Grenade.Active == false);
                        SubGrenadeList.RemoveAll(SubGrenade => SubGrenade.Active == false);


                        ProjectileList.RemoveAll(Projectile => !ScreenRectangle.Contains(new Point((int)Projectile.Position.X, (int)Projectile.Position.Y)));

                        if (CurrentMouseState.ScrollWheelValue > PreviousMouseState.ScrollWheelValue)
                        {
                            CurrentMap.LightList[0].Power += 0.1f;
                        }

                        if (CurrentMouseState.ScrollWheelValue < PreviousMouseState.ScrollWheelValue)
                        {
                            CurrentMap.LightList[0].Power -= 0.1f;
                        }

                        CurrentMap.LightList[0].Position = new Vector3(Mouse.GetState().X, Mouse.GetState().Y, 90000);                        

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

                            //if (player.IsShooting == true && 
                            //    player.WasShooting == false)
                            //{
                            //    switch (player.CurrentGun)
                            //    {
                            //        case GunType.Flamethrower:
                            //            {
                            //                if (player.flameEmitter == null)
                            //                {
                            //                    player.flameEmitter = new Emitter(ToonSmoke3,
                            //                    new Vector2(player.Position.X, player.Position.Y), new Vector2(60, 120), new Vector2(6, 8),
                            //                    new Vector2(650, 800), 1f, false, new Vector2(-10, 10), new Vector2(-1, 1), new Vector2(0.05f, 0.06f),
                            //                    new Color(255, 128, 0, 6), Color.Black,
                            //                    -0.005f, -0.4f, 16, 6, false, new Vector2(0, 720), true, 0.1f,
                            //                    null, null, null, null, null, false, new Vector2(0.02f, 0.01f), null, null,
                            //                    null, null, null, true, null);
                            //                }
                            //                else
                            //                {
                            //                    player.flameEmitter.AddMore = true;
                            //                }
                            //            }
                            //            break;
                            //    }
                            //}
                        }

                        if (Players.Any(Player => Player != null && Player.Score > MaxScore))
                        {
                            WinningPlayer = Players.First(player => player.Score > MaxScore);
                            MatchEndTimer.X += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

                            if (MatchEndTimer.X >= MatchEndTimer.Y)
                            {
                                CurrentGameState = GameState.EndMatch;
                            }
                        }

                        foreach (Projectile projectile in ProjectileList)
                        {
                            projectile.Update(gameTime);
                            projectile.UpdateEmitters(gameTime);

                            if (projectile.Active == false)
                            {
                                Vector2 rang = new Vector2(
                                        MathHelper.ToDegrees(-(float)Math.Atan2(projectile.Velocity.Y, projectile.Velocity.X)) - 180 - 60,
                                        MathHelper.ToDegrees(-(float)Math.Atan2(projectile.Velocity.Y, projectile.Velocity.X)) - 180 + 60);

                                for (int i = 0; i < 10; i++)
                                {
                                    Emitter emitter = new Emitter(ParticleTexture, projectile.Position,
                                        new Vector2(0, 360), new Vector2(0, 2),
                                        new Vector2(280, 500), 1f, true, Vector2.Zero, new Vector2(-3, 3), new Vector2(0.5f, 1f),
                                        ColorAjustAlpha(Color.HotPink, 80),
                                        ColorAjustAlpha(Color.HotPink, 20),
                                        -0.01f, (float)DoubleRange(0.5d, 1.5d), 1, 3, true, new Vector2(1080, 1080),
                                        false, 0, true, true, new Vector2(3, 5), rang, 0.2f,
                                        null, null, null, null, null, true, null, null, true, false);

                                    EmitterList.Add(emitter);
                                }

                                Emitter HitEffect1 = new Emitter(HitEffectParticle,
                                        new Vector2(projectile.Position.X, projectile.Position.Y), rang, new Vector2(5f, 8f),
                                        new Vector2(250f, 500f), 1f, false, new Vector2(0f, 360f), new Vector2(-2f, 2f),
                                        new Vector2(0.15f, 0.15f), new Color(255, 255, 191, 255),
                                        new Color(255, 255, 255, 255), 0f, 0.05f, 50f, 7, false, new Vector2(0f, 1080), true,
                                        (projectile.Position.Y + 8) / 1080f,
                                        false, false, null, null, 0f, true, new Vector2(0.11f, 0.11f), false, false, 0f,
                                        false, false, false, null)
                                {
                                    Emissive = true
                                };
                                EmitterList.Add(HitEffect1);
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

                #region Paused
                case GameState.Paused:
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            if (CurrentGamePadStates[i].IsButtonUp(Buttons.Start) &&
                                PreviousGamePadStates[i].IsButtonDown(Buttons.Start))
                            {
                                CurrentGameState = GameState.Playing;
                            }

                            if (CurrentGamePadStates[i].IsButtonUp(Buttons.A) &&
                                PreviousGamePadStates[i].IsButtonDown(Buttons.A))
                            {
                                switch (SelectedPauseMenu)
                                {
                                    case 0:
                                        {
                                            UnloadGameContent();
                                            foreach (Player player in Players)
                                            {
                                                if (player != null)
                                                    player.Initialize();
                                            }

                                            //if (EmitterList != null)
                                            //    EmitterList.Clear();

                                            //if (ProjectileList != null)
                                            //    ProjectileList.Clear();
                                            
                                            //if (TrapList != null)
                                            //    TrapList.Clear();

                                            //if (GibList != null)
                                            //    GibList.Clear();

                                            ClearLists();

                                            CurrentGameState = GameState.LevelSelect;
                                        }
                                        break;
                                }
                            }

                            if (CurrentGamePadStates[i].IsButtonUp(Buttons.DPadDown) &&
                                PreviousGamePadStates[i].IsButtonDown(Buttons.DPadDown))
                            {
                                SelectedPauseMenu++;
                            }

                            if (CurrentGamePadStates[i].IsButtonUp(Buttons.DPadUp) &&
                                PreviousGamePadStates[i].IsButtonDown(Buttons.DPadUp))
                            {
                                SelectedPauseMenu--;
                            }
                        }
                    }
                    break;
                #endregion

                case GameState.EndMatch:
                    {
                        if (EndMatchTime.X < EndMatchTime.Y)
                        {
                            EndMatchTime.X += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                        }

                        if (EndMatchTime.X >= EndMatchTime.Y)
                        {
                            for (int i = 0; i < 6; i++)
                            {
                                PaintDelayValues[i] = new Vector2(PaintDelayValues[i].X + (float)gameTime.ElapsedGameTime.TotalMilliseconds, PaintDelayValues[i].Y);
                            }

                            for (int i = 0; i < 6; i++)
                            {
                                if (PaintDelayValues[i].X >= PaintDelayValues[i].Y)
                                {
                                    if (PaintStreakValues[i] < 2500)
                                        PaintStreakValues[i] += PaintStreakIncrements[i];
                                }
                            }

                            if (MatchEndFade < 100)
                                MatchEndFade += 0.45f;

                            if (PaintSplatterOpacity < 100)
                                PaintSplatterOpacity += 17;

                            if (MatchEndFade / 100f >= 1.0f)
                            {
                                for (int i = 0; i < 4; i++)
                                {
                                    if (CurrentGamePadStates[i].IsButtonUp(Buttons.A) &&
                                        PreviousGamePadStates[i].IsButtonDown(Buttons.A))
                                    {
                                        CurrentGameState = GameState.LevelSelect;
                                    }

                                    if (CurrentGamePadStates[i].IsButtonUp(Buttons.B) &&
                                        PreviousGamePadStates[i].IsButtonDown(Buttons.B))
                                    {
                                        CurrentGameState = GameState.MainMenu;
                                    }
                                }
                            }

                        }
                    }
                    break;
            }

            //if (PreviousGameState == GameState.Playing &&
            //    CurrentGameState == GameState.EndMatch)
            //{

            //}
            
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
            switch (CurrentGameState)
            {
                #region Main Menu
                case GameState.MainMenu:
                    {
                        GraphicsDevice.SetRenderTarget(MenuRenderTarget);
                        GraphicsDevice.Clear(Color.Black);
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

                //#region Mode Select
                //case GameState.ModeSelect:
                //    {
                //        GraphicsDevice.SetRenderTarget(MenuRenderTarget);
                //        GraphicsDevice.Clear(Color.Black);
                //        spriteBatch.Begin();
                //        spriteBatch.DrawString(Font1, "Mode Select", new Vector2(32, 32), Color.White);

                //        for (int i = 0; i < ModeSelectOptions.Count; i++)
                //        {
                //            Color col = Color.Gray;

                //            if (SelectedModeMenu == i)
                //                col = Color.White;

                //            spriteBatch.DrawString(Font1, ModeSelectOptions[i], new Vector2(32, 64 + (25 * i)), col);
                //        }

                //        spriteBatch.End();
                //    }
                //    break;
                //#endregion

                #region Playing
                case GameState.Playing:
                case GameState.Paused:
                case GameState.EndMatch:
                    {
                        if (CurrentGameState == GameState.Playing)
                        {
                            DoubleBuffer.GlobalStartFrame(gameTime);
                            RenderManager.DoFrame();
                        }                        

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

                        foreach (BulletTrail trail in BulletTrailList)
                        {
                            trail.Draw(GraphicsDevice, BulletTrailEffect);
                        }

                        //foreach (Trap trap in TrapList.Where(Trap => Trap.TrapType == TrapType.TripMine && Trap.Active == true))
                        //{                            
                        //    (trap as TripMine).Laser.Draw(GraphicsDevice, BulletTrailEffect);
                        //}

                        foreach (Item item in ItemList)
                        {
                            switch (item.ItemType)
                            {
                                case ItemType.Shield:
                                    {
                                        item.Draw(spriteBatch);
                                    }
                                    break;
                            }
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
                        GraphicsDevice.Clear(Color.LightGray);
                        spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);

                        foreach (Player player in Players.Where(Player => Player != null))
                        {
                            player.Draw(spriteBatch);
                        }

                        foreach (Trap trap in TrapList.Where(Trap => Trap.Active == true))
                        {
                            trap.Draw(spriteBatch);
                        }

                        foreach (Gib gib in GibList)
                        {
                            gib.Draw(spriteBatch);
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

                        spriteBatch.Draw(ParticleRenderTarget, ParticleRenderTarget.Bounds, Color.White);
                        CurrentMap.Draw(spriteBatch);
                        
                        foreach (Item item in ItemList)
                        {
                            switch (item.ItemType)
                            {
                                case ItemType.RocketLauncher:
                                case ItemType.Shotgun:
                                case ItemType.MachineGun:
                                case ItemType.RedFlag:
                                case ItemType.BlueFlag:
                                    {
                                        item.Draw(spriteBatch);
                                    }
                                break;
                            }                            
                        }

                        foreach (Grenade grenade in GrenadeList)
                        {
                            grenade.Draw(spriteBatch);
                        }

                        foreach (SubGrenade subGrenade in SubGrenadeList)
                        {
                            subGrenade.Draw(spriteBatch);
                        }

                        spriteBatch.Draw(EmissiveMap, EmissiveMap.Bounds, Color.White);

                        RenderManager.DrawLit(spriteBatch);
                        
                        spriteBatch.End();


                        #endregion

                        BloodLiquid.Draw(spriteBatch);

                        GraphicsDevice.SetRenderTarget(ParticleRenderTarget);
                        GraphicsDevice.Clear(Color.Transparent);
                        spriteBatch.Begin(0, null, null, null, null, BloodLiquid.AlphaTest);

                        spriteBatch.Draw(BloodLiquid.MetaballTarget, BloodLiquid.MetaballTarget.Bounds, Color.White);

                        //RenderManager.DrawLit(spriteBatch);
                        ////foreach (Player player in Players.Where(Player => Player != null))
                        //{
                        //    player.Draw(spriteBatch);
                        //}
                        spriteBatch.End();

                        #region Draw to NormalMap
                        GraphicsDevice.SetRenderTarget(NormalMap);
                        GraphicsDevice.Clear(new Color(127, 127, 255));
                        spriteBatch.Begin();
                        //spriteBatch.Draw(NormalTexture, new Rectangle(0, 0, 1920, 1080), Color.White);
                        //CurrentMap.Draw(spriteBatch);
                        //spriteBatch.Draw(ParticleRenderTarget, ParticleRenderTarget.Bounds, Color.Gray);
                        spriteBatch.End();
                        #endregion

                        #region Draw to SpecMap

                        #endregion

                        #region Draw to DepthMap

                        #endregion

                        #region Draw to LightMap
                        GraphicsDevice.SetRenderTarget(LightMap);
                        GraphicsDevice.Clear(Color.Transparent);

                        foreach (Light light in CurrentMap.LightList)
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
                        foreach (Trap trap in TrapList.Where(Trap => Trap.TrapType == TrapType.TripMine && Trap.Active == true))
                        {
                            (trap as TripMine).Laser.Draw(GraphicsDevice, BulletTrailEffect);
                        }
                        spriteBatch.End();
                        #endregion

                        #region Combine Normals, Lighting and Color
                        GraphicsDevice.SetRenderTarget(FinalMap);
                        GraphicsDevice.Clear(Color.DeepSkyBlue);

                        spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, null, null, null, LightCombined);
                        #region Draw the lightmap and color map combined
                        LightCombined.CurrentTechnique = LightCombined.Techniques["DeferredCombined2"];
                        LightCombined.Parameters["ambient"].SetValue(1.0f);
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

                        //foreach (BulletTrail trail in BulletTrailList)
                        //{
                        //    trail.Draw(GraphicsDevice, BasicEffect);
                        //}
                        spriteBatch.End();
                        #endregion

                        if (ShockWaveList.Count(Shock => Shock.Active == true) > 0)
                        {
                            foreach (ShockWave shockWave in ShockWaveList)
                            {
                                ShockWaveEffect.Parameters["CenterCoords"].SetValue(shockWave.Position);
                                ShockWaveEffect.Parameters["WaveParams"].SetValue(new Vector4(10, 0.5f, 0.1f, 60));
                                ShockWaveEffect.Parameters["CurrentTime"].SetValue(shockWave.CurrentTime/1000f);

                                GraphicsDevice.SetRenderTarget(Buffer1);
                                GraphicsDevice.Clear(Color.Transparent);
                                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, null, null, null, ShockWaveEffect);
                                spriteBatch.Draw(FinalMap, FinalMap.Bounds, Color.White);
                                spriteBatch.End();

                                GraphicsDevice.SetRenderTarget(FinalMap);
                                GraphicsDevice.Clear(Color.Transparent);
                                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
                                spriteBatch.Draw(Buffer1, Buffer1.Bounds, Color.White);
                                spriteBatch.End();
                            }
                        }
                        //else
                        //{
                        //    GraphicsDevice.SetRenderTarget(Buffer1);
                        //    GraphicsDevice.Clear(Color.Transparent);
                        //    spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
                        //    spriteBatch.Draw(FinalMap, FinalMap.Bounds, Color.White);
                        //    spriteBatch.End();
                        //}


                        #region Occlusion Map

                        #endregion

                        #region Crepuscular ColorMap

                        #endregion

                        if (CurrentGameState == GameState.Playing)
                        {
                            DoubleBuffer.SubmitRender();
                        }
                    }
                    break;
                #endregion

                #region Level Select
                case GameState.LevelSelect:
                    {
                        GraphicsDevice.SetRenderTarget(MenuRenderTarget);
                        GraphicsDevice.Clear(Color.Black);
                        spriteBatch.Begin();
                        spriteBatch.DrawString(Font1, "Level Select", new Vector2(32, 32), Color.White);

                        for (int i = 0; i < LevelList.Count; i++)
                        {
                            Color col = Color.Gray;

                            if (SelectedLevelIndex == i)
                                col = Color.White;

                            spriteBatch.DrawString(Font1, LevelList[i], new Vector2(32, 64 + (25 * i)), col);
                        }

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

            if (CurrentGameState == GameState.EndMatch)
            {
                
                if (PreviousGameState == GameState.Playing)
                {
                    for (int i = 0; i < 6; i++)
                    {
                        PaintAngleOffsets.Add(Random.Next(-3, 3));
                    }
                }

                Color PaintColor = WinningPlayer.PlayerColor;

                spriteBatch.Draw(Splatter1, new Rectangle(0, 0, 1920, 1080), PaintColor * (PaintSplatterOpacity / 100));
                                
                //Could adjust each brush stroke color with this:   ColorAdjust(PaintColor, PaintAngleOffsets[0] * 5)

                if (PaintDelayValues[0].X >= PaintDelayValues[0].Y)
                    spriteBatch.Draw(PaintStreak1, new Rectangle(2400, 500, 2500, PaintStreak1.Height), 
                                     new Rectangle(0, 0, (int)PaintStreakValues[0], PaintStreak1.Height),
                                     PaintColor * 0.95f, MathHelper.ToRadians(180 + 8), 
                                     new Vector2(0, PaintStreak1.Height / 2), SpriteEffects.FlipHorizontally, 0);

                if (PaintDelayValues[1].X >= PaintDelayValues[1].Y)
                    spriteBatch.Draw(PaintStreak2, new Rectangle(-400, 200, 2500, PaintStreak2.Height), 
                                     new Rectangle(0, 0, (int)PaintStreakValues[1], PaintStreak2.Height),
                                     PaintColor * 0.85f, MathHelper.ToRadians(17), 
                                     new Vector2(0, PaintStreak2.Height / 2), SpriteEffects.FlipHorizontally, 0);

                if (PaintDelayValues[2].X >= PaintDelayValues[2].Y)
                    spriteBatch.Draw(PaintStreak3, new Rectangle(2100, 0, 2500, PaintStreak3.Height), 
                                     new Rectangle(0, 0, (int)PaintStreakValues[2], PaintStreak3.Height),
                                     PaintColor * 0.85f, MathHelper.ToRadians(180 - 13), 
                                     new Vector2(0, PaintStreak3.Height / 2), SpriteEffects.FlipHorizontally, 0);

                if (PaintDelayValues[3].X >= PaintDelayValues[3].Y)
                    spriteBatch.Draw(PaintStreak1, new Rectangle(-600, 800, 2500, PaintStreak1.Height), 
                                     new Rectangle(0, 0, (int)PaintStreakValues[3], PaintStreak1.Height),
                                     PaintColor * 0.85f, MathHelper.ToRadians(-6), 
                                     new Vector2(0, PaintStreak1.Height / 2), SpriteEffects.FlipHorizontally, 0);

                if (PaintDelayValues[4].X >= PaintDelayValues[4].Y)
                    spriteBatch.Draw(PaintStreak2, new Rectangle(2100, 600, 2500, PaintStreak2.Height), 
                                     new Rectangle(0, 0, (int)PaintStreakValues[4], PaintStreak2.Height),
                                     PaintColor * 0.85f, MathHelper.ToRadians(180 + 4), 
                                     new Vector2(0, PaintStreak2.Height / 2), SpriteEffects.FlipHorizontally, 0);

                if (PaintDelayValues[5].X >= PaintDelayValues[5].Y)
                    spriteBatch.Draw(PaintStreak3, new Rectangle(-600, 1100, 2500, PaintStreak3.Height), 
                                     new Rectangle(0, 0, (int)PaintStreakValues[5], PaintStreak3.Height),
                                     PaintColor * 0.95f, MathHelper.ToRadians(-3), 
                                     new Vector2(0, PaintStreak3.Height / 2), SpriteEffects.FlipHorizontally, 0);

                //MAYBE DON'T COMPLETELY FILL THE SCREEN. 
                //IT LOOKS PRETTY COOL WHEN NOT FILLED ALTHOUGH IT MAY MAKE FOR AN AWKWARD TRANSITION TO THE NEXT SCREEN
                spriteBatch.Draw(Block, new Rectangle(0, 0, 1920, 1080), PaintColor * (MatchEndFade / 100));

                if ((MatchEndFade / 100) >= 1.0f)
                {
                    int numplayers = Players.Count(Player => Player != null);
                    float boxAndGapTotal = (numplayers * 356) + ((numplayers - 1) * 95);
                    float startpoint = 960 - (boxAndGapTotal / 2);

                    int pIndex = 0;

                    foreach (Player player in Players.Where(Player => Player != null))
                    {
                        Color myCol = player.PlayerColor;

                        if (player == WinningPlayer)
                        {
                            myCol = Color.Red;
                        }

                        //PLAYER RESULTS BOXES
                        spriteBatch.Draw(Block, new Rectangle((int)startpoint + (451 * (int)pIndex), 278, 356, 524), myCol);


                        //Button Divider
                        spriteBatch.Draw(Block, new Rectangle(960, 1080 - 100, 3, 48), null, Color.White, 0, new Vector2(16, 16), SpriteEffects.None, 0);

                        //NEW MATCH
                        Vector2 textSize = Font1.MeasureString("NEW MATCH");
                        spriteBatch.Draw(AButtonTexture, new Rectangle(960 + 16, 1080 - 100, AButtonTexture.Width, AButtonTexture.Height), 
                                         null, Color.White, 0, new Vector2(0, AButtonTexture.Height/2), SpriteEffects.None, 0);
                        spriteBatch.DrawString(Font1, "NEW MATCH", new Vector2(960 + AButtonTexture.Width + 32, 1080 - 99), 
                                               Color.White, 0, new Vector2(0, textSize.Y/2), 1f, SpriteEffects.None, 0);

                        //MAIN MENU
                        textSize = Font1.MeasureString("MAIN MENU");
                        spriteBatch.Draw(BButtonTexture, new Rectangle(960 - (int)textSize.X - 32, 1080 - 100, AButtonTexture.Width, AButtonTexture.Height), 
                                                                       null, Color.White, 0, new Vector2(AButtonTexture.Width, AButtonTexture.Height / 2), SpriteEffects.None, 0);
                        spriteBatch.DrawString(Font1, "MAIN MENU", new Vector2(960 - 16, 1080 - 99), 
                                               Color.White, 0, new Vector2(textSize.X, textSize.Y/2), 1f, SpriteEffects.None, 0);

                        pIndex++;
                    }
                }


                #region MyRegion
                //if (PaintDelayValues[0].X >= PaintDelayValues[0].Y)
                //    spriteBatch.Draw(PaintStreak1, new Rectangle(1990, 500, (int)PaintStreakValues[0], PaintStreak1.Height), null, 
                //                     PaintColor * 0.85f, MathHelper.ToRadians(15 + PaintAngleOffsets[0]), new Vector2(PaintStreak1.Width, PaintStreak1.Height / 2), PaintOrientations[0], 0);

                //if (PaintDelayValues[1].X >= PaintDelayValues[1].Y)
                //    spriteBatch.Draw(PaintStreak2, new Rectangle(-150, 0, (int)PaintStreakValues[2], PaintStreak2.Height), null, 
                //                     PaintColor * 0.85f, MathHelper.ToRadians(20 + PaintAngleOffsets[1]), new Vector2(0, PaintStreak2.Height / 2), PaintOrientations[1], 0);

                //if (PaintDelayValues[2].X >= PaintDelayValues[2].Y)
                //    spriteBatch.Draw(PaintStreak3, new Rectangle(2080, 1200, (int)PaintStreakValues[2], PaintStreak3.Height), null, 
                //                     PaintColor * 0.85f, MathHelper.ToRadians(24 + PaintAngleOffsets[2]), new Vector2(PaintStreak3.Width, PaintStreak3.Height / 2), PaintOrientations[2], 0);

                //if (PaintDelayValues[3].X >= PaintDelayValues[3].Y)
                //    spriteBatch.Draw(PaintStreak1, new Rectangle(-230, 800, (int)PaintStreakValues[3], PaintStreak1.Height), null, 
                //                     PaintColor * 0.85f, MathHelper.ToRadians(6 + PaintAngleOffsets[3]), new Vector2(0, PaintStreak1.Height / 2), PaintOrientations[3], 0);

                //if (PaintDelayValues[4].X >= PaintDelayValues[4].Y)
                //    spriteBatch.Draw(PaintStreak2, new Rectangle(2020, 400, (int)PaintStreakValues[4], PaintStreak2.Height), null, 
                //                     PaintColor * 0.85f, MathHelper.ToRadians(-10 + PaintAngleOffsets[4]), new Vector2(PaintStreak2.Width, PaintStreak2.Height / 2), PaintOrientations[4], 0);

                //if (PaintDelayValues[5].X >= PaintDelayValues[5].Y)
                //    spriteBatch.Draw(PaintStreak3, new Rectangle(-200, 950, (int)PaintStreakValues[5], PaintStreak3.Height), null, 
                //                     PaintColor * 0.85f, MathHelper.ToRadians(-24 + PaintAngleOffsets[5]), new Vector2(0, PaintStreak3.Height / 2), PaintOrientations[5], 0);


                //spriteBatch.Draw(GameOverTexture, new Rectangle(0, 0, 1920, 1080), Color.White * (MatchEndDoorPos / 250));  
                #endregion

                #region This Works
                //Color PaintColor = Color.DodgerBlue;

                //spriteBatch.Draw(Splatter1, new Rectangle(0, 0, 1920, 1080), PaintColor * (PaintSplatterOpacity / 100));

                ////MAYBE DON'T COMPLETELY FILL THE SCREEN. 
                ////IT LOOKS PRETTY COOL WHEN NOT FILLED ALTHOUGH IT MAY MAKE FOR AN AWKWARD TRANSITION TO THE NEXT SCREEN
                ////spriteBatch.Draw(Block, new Rectangle(0, 0, 1920, 1080), PaintColor * (MatchEndDoorPos / 100));

                //if (PaintDelayValues[0].X >= PaintDelayValues[0].Y)
                //    spriteBatch.Draw(PaintStreak, new Rectangle(-200, 500, (int)PaintStreakValues[0], PaintStreak.Height), null, PaintColor * 0.85f, MathHelper.ToRadians(-12), Vector2.Zero, SpriteEffects.None, 0);

                //if (PaintDelayValues[1].X >= PaintDelayValues[1].Y)
                //    spriteBatch.Draw(PaintStreak, new Rectangle(-200, 0, (int)PaintStreakValues[1], PaintStreak.Height), null, PaintColor * 0.85f, MathHelper.ToRadians(24), Vector2.Zero, SpriteEffects.None, 0);

                //if (PaintDelayValues[2].X >= PaintDelayValues[2].Y)
                //    spriteBatch.Draw(PaintStreak, new Rectangle(-200, 1200, (int)PaintStreakValues[2], PaintStreak.Height), null, PaintColor * 0.85f, MathHelper.ToRadians(-45), Vector2.Zero, SpriteEffects.None, 0);

                //if (PaintDelayValues[3].X >= PaintDelayValues[3].Y)
                //    spriteBatch.Draw(PaintStreak, new Rectangle(-200, 800, (int)PaintStreakValues[3], PaintStreak.Height), null, PaintColor * 0.85f, MathHelper.ToRadians(-30), Vector2.Zero, SpriteEffects.None, 0);

                //if (PaintDelayValues[4].X >= PaintDelayValues[4].Y)
                //    spriteBatch.Draw(PaintStreak, new Rectangle(2040, 800, (int)PaintStreakValues[4], PaintStreak.Height), null, PaintColor * 0.85f, MathHelper.ToRadians(-160), Vector2.Zero, SpriteEffects.None, 0);

                //if (PaintDelayValues[5].X >= PaintDelayValues[5].Y)
                //    spriteBatch.Draw(PaintStreak, new Rectangle(2040, 950, (int)PaintStreakValues[5], PaintStreak.Height), null, PaintColor * 0.85f, MathHelper.ToRadians(-170), Vector2.Zero, SpriteEffects.None, 0);


                ////spriteBatch.Draw(GameOverTexture, new Rectangle(0, 0, 1920, 1080), Color.White * (MatchEndDoorPos / 250)); 
                #endregion
            }

            if (DrawDiagnostics == true)
            {
                int y = 16;
                spriteBatch.DrawString(Font1, "Particles: " + RenderManager.RenderDataObjects.Count, new Vector2(32, y), Color.White);
                y += 16;
                spriteBatch.DrawString(Font1, "Emitters: " + EmitterList.Count.ToString(), new Vector2(32, y), Color.White);
                y += 16;
                //spriteBatch.DrawString(Font1, "Items: " + ItemList.Count, new Vector2(32, y), Color.White);
                //y += 16;
                spriteBatch.DrawString(Font1, "Projectiles: " + ProjectileList.Count, new Vector2(32, y), Color.White);
                y += 16;
                spriteBatch.DrawString(Font1, "Grenades: " + GrenadeList.Count, new Vector2(32, y), Color.White);
            }

            if (CurrentGameState == GameState.Paused)
            {
                for (int i = 0; i < PauseMenuOptions.Count; i++)
                {
                    Color col = Color.Gray;

                    if (SelectedPauseMenu == i)
                        col = Color.White;

                    spriteBatch.DrawString(Font1, PauseMenuOptions[i], new Vector2(32, 64 + (25 * i)), col);
                }
            }
            
            #region Draw Debug Boxes
            if (DebugBoxes == true)
                if (CurrentGameState == GameState.Playing)
                {
                    foreach (MyRay ray in myRayList)
                    {
                        //Draw the debug bounding boxes here
                        VertexPositionColor[] Vertices = new VertexPositionColor[2];
                        int[] Indices = new int[2];

                        Vertices[0] = new VertexPositionColor()
                        {
                            Color = Color.Orange,
                            Position = ray.position
                        };

                        Vertices[1] = new VertexPositionColor()
                        {
                            Color = Color.Orange,
                            Position = ray.position + (ray.direction * ray.length)
                        };

                        Indices[0] = 0;
                        Indices[1] = 1;

                        foreach (EffectPass pass in BasicEffect.CurrentTechnique.Passes)
                        {
                            pass.Apply();
                            GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.LineStrip, Vertices, 0, 2, Indices, 0, 1, VertexPositionColorTexture.VertexDeclaration);
                        }
                    }

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

                    //foreach (Item item in ItemList)
                    //{
                    //    item.DrawInfo(spriteBatch, GraphicsDevice, BasicEffect);
                    //}
                                    
                    foreach (Grenade grenade in GrenadeList)
                    {
                        grenade.DrawInfo(GraphicsDevice, BasicEffect);
                    }

                    foreach (Projectile projectile in ProjectileList)
                    {
                        projectile.DrawInfo(GraphicsDevice, BasicEffect);
                    }
                }
            #endregion

            #region Draw Tile Boxes
            if (TileBoxes == true)
                if (CurrentGameState == GameState.Playing)
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
            #endregion

            #region Player info
            if (CurrentGameState == GameState.Playing)
            {
                for (int i = 0; i < Players.Count(); i++)
                {
                    if (Players[i] != null)
                    {
                        int yO = 0;
                        Players[i].DrawHUD(spriteBatch);

                        spriteBatch.DrawString(Font1, Players[i].Score.ToString(), Players[i].Position, Color.Red);
                        yO += 16;
                        spriteBatch.DrawString(Font1, "Shots:" + Players[i].NumShots.ToString(), Players[i].Position + new Vector2(0, yO), Color.Red);
                        yO += 16;
                        spriteBatch.DrawString(Font1, "Grenades:" + Players[i].NumGrenades.ToString(), Players[i].Position + new Vector2(0, yO), Color.Red);
                        yO += 16;
                        spriteBatch.DrawString(Font1, "Deaths:" + Players[i].NumDeaths.ToString(), Players[i].Position + new Vector2(0, yO), Color.Red);
                        yO += 16;
                        spriteBatch.DrawString(Font1, "Jumps:" + Players[i].NumJumps.ToString(), Players[i].Position + new Vector2(0, yO), Color.Red);



                        //Players[i].HealthBar.Draw(spriteBatch);
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
            }
            #endregion
            

            spriteBatch.End();
            #endregion
            
            #region Draw to the backbuffer
            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.Black);
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, null, null, null, null, Camera.Transform);

            if (CurrentGameState != GameState.Playing &&
                CurrentGameState != GameState.Paused &&
                CurrentGameState != GameState.EndMatch &&
                CurrentGameState != GameState.EndRound)
            {
                spriteBatch.Draw(MenuRenderTarget, MenuRenderTarget.Bounds, Color.White);
            }
            else
            {
                spriteBatch.Draw(FinalMap, FinalMap.Bounds, Color.White);
                spriteBatch.Draw(BlurMap, BlurMap.Bounds, Color.White);
            }

            spriteBatch.End();
            
            spriteBatch.Begin();
            spriteBatch.Draw(UIRenderTarget, UIRenderTarget.Bounds, Color.White);
            spriteBatch.End();
            #endregion

            base.Draw(gameTime);
        }



        protected override void EndDraw()
        {
            base.EndDraw();

            if (CurrentGameState == GameState.Playing)
                DoubleBuffer.GlobalSynchronize();
        }

        protected override void OnExiting(object sender, EventArgs args)
        {            
            if (UpdateManager != null && UpdateManager.RunningThread != null)
                UpdateManager.RunningThread.Abort();

            if (DoubleBuffer != null)
                DoubleBuffer.CleanUp();
        }

        public void EndMatch(Player winner)
        {

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

        protected Color ColorAjustAlpha(Color color, byte alpha)
        {
            return new Color(color.R, color.G, color.B, alpha);
        }

        protected Color ColorAdjust(Color color, int adjust)
        {
            return new Color(color.R + adjust, color.G + adjust, color.B + adjust);
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

        public void ClearLists()
        {
            ShockWaveList.Clear();
            SolidList.Clear();
            MovingPlatformList.Clear();
            ItemList.Clear();
            ProjectileList.Clear();
            BulletTrailList.Clear();
            EmitterList.Clear();
            GibList.Clear();
            GrenadeList.Clear();
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

        public void LoadLevel(string levelName)
        {
            CurrentMap = new Map();

            string dir = Environment.CurrentDirectory;
            string newPath = Path.GetFullPath(Path.Combine(dir, @"..\..\..\..\..\..\Levels\\"));
            newPath += levelName;

            IFormatter formatter = new BinaryFormatter
            {
                Binder = new SerializationHelper()
            };

            Stream stream = new FileStream(newPath, FileMode.Open);
            Map loadMap = (Map)formatter.Deserialize(stream);

            stream.Close();
            
            CurrentMap = loadMap;
            CurrentMap.LoadContent(Content);
            CurrentMap.Initialize();
        }

        public void ListLevels()
        {
            LevelList.Clear();
            string dir = Environment.CurrentDirectory;
            string newPath = Path.GetFullPath(Path.Combine(dir, @"..\..\..\..\..\..\Levels"));

            var thing = Directory.GetFiles(newPath, "*.lvl");

            LevelList.AddRange(thing);
        }

        public static double RandomDouble(double a, double b)
        {
            return a + Random.NextDouble() * (b - a);
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

        public Vector2 GetAngleRange(LightProjectile projectile, float difference, bool? negative = false)
        {
            if (negative == true)
            {
                return new Vector2(
                               MathHelper.ToDegrees(-(float)Math.Atan2(projectile.Ray.Direction.Y, projectile.Ray.Direction.X)) - difference,
                               MathHelper.ToDegrees(-(float)Math.Atan2(projectile.Ray.Direction.Y, projectile.Ray.Direction.X)) + difference);
            }

            return new Vector2(
                               MathHelper.ToDegrees(-(float)Math.Atan2(projectile.Ray.Direction.Y, projectile.Ray.Direction.X)) - 180 - difference,
                               MathHelper.ToDegrees(-(float)Math.Atan2(projectile.Ray.Direction.Y, projectile.Ray.Direction.X)) - 180 + difference);
        }

        private static Texture2D RandomTexture(params Texture2D[] textures)
        {
            return textures[Random.Next(0, textures.Length)];
        }

        public Emitter SnapPing(Vector2 collisionEnd)
        {
            Emitter BOOMEmitter;

            if (Random.NextDouble() >= 0.5f)
            {
                BOOMEmitter = new Emitter(SNAPParticle, collisionEnd,
                                            new Vector2(0, 0), new Vector2(0, 0), new Vector2(200, 400), 1.0f, false,
                                            new Vector2(-25, 25), new Vector2(0, 0), new Vector2(0.15f, 0.15f),
                                            Color.White, Color.White, 0f, 0.05f, 100f, 1, false,
                                            new Vector2(0, 1080), true) { Grow = true };
            }
            else
            {
                BOOMEmitter = new Emitter(PINGParticle, collisionEnd,
                                            new Vector2(0, 0), new Vector2(0, 0), new Vector2(200, 400), 1.0f, false,
                                            new Vector2(-25, 25), new Vector2(0, 0), new Vector2(0.15f, 0.15f),
                                            Color.White, Color.White, 0f, 0.05f, 100f, 1, false,
                                            new Vector2(0, 1080), true) { Grow = true };
            }

            return BOOMEmitter;
        }


        public static SpriteEffects RandomOrientation()
        {
            //List<SpriteEffects> Orientations= new List<SpriteEffects>();
            //Orientations =

            var Orientations = Enum.GetValues(typeof(SpriteEffects));

            return (SpriteEffects)Orientations.GetValue(Random.Next(Orientations.Length));
        }
    }
}
