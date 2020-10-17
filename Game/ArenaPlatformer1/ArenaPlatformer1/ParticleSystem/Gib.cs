using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace ArenaPlatformer1
{
    public class Gib : MovingObject
    {
        static Random Random = new Random();
        public new static Texture2D Texture;
        
        public Vector2 Time;
        public object Source;
        public float Rotation, RotationIncrement;
        public float Gravity = 0.3f;
        public bool Active = true;
        public float Speed;

        public Rectangle DestinationRectangle;
        public List<Emitter> EmitterList = new List<Emitter>();
        

        
        public Gib(Vector2 position, Vector2 direction, float speed, Texture2D decalTexture, Texture2D emitterTexture, Color gibColor)
        {
            Position = position;
            Position = position;
            Speed = speed;
            Velocity = direction * Speed;
            //Velocity = velocity;
            //Source = source;

            Rotation = MathHelper.ToRadians(Random.Next(0, 360));
            RotationIncrement = MathHelper.ToRadians(3);

            Time.Y = 2000f;

            Size = new Vector2(5, 6);
        }

        public override void Update(GameTime gameTime)
        {
            Time.X += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            if (Time.X >= Time.Y)
            {
                Active = false;
            }

            Velocity.Y += Gravity;
            base.Update(gameTime);

            if (PushesBottomTile == true ||
                PushesTopTile == true)
            {
                Velocity.Y = -Velocity.Y * 0.45f;                
            }

            if (PushesBottomTile == true)
            {
                Velocity.X *= 0.55f;
            }

            if (PushesLeftTile == true ||
                PushesRightTile == true)
            {
                Velocity.X = -Velocity.X * 0.55f;
            }

            //STICK GIBS TO WALLS
            //if (PushesLeftTile == true ||
            //    PushesRightTile == true ||
            //    PushesTopTile == true ||
            //    PushesBottomTile == true)
            //{
            //    Velocity = Vector2.Zero;
            //    Gravity = 0f;
            //}

            #region Rotation
            if (Math.Abs(Velocity.X) >= 1 || Math.Abs(Velocity.Y) >= 1)
            {
                RotationIncrement = MathHelper.ToRadians(Random.Next(0, 4)) * (Velocity.X + Velocity.Y);
                Rotation += RotationIncrement;
            }
            #endregion

            DestinationRectangle = new Rectangle(
                (int)Position.X,
                (int)Position.Y,
                Texture.Width, Texture.Height);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            //spriteBatch.Draw(Texture, DestinationRectangle, null, Color.Red, Rotation, new Vector2(Texture.Width/2, Texture.Height/2), SpriteEffects.None, 0);
        }

        public void DrawInfo(GraphicsDevice graphics, BasicEffect basicEffect)
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

        public void UpdateEmitters(GameTime gameTime)
        {
            foreach (Emitter emitter in EmitterList)
            {
                emitter.Position = Position;
                emitter.Update(gameTime);
            }
        }
    }
}
