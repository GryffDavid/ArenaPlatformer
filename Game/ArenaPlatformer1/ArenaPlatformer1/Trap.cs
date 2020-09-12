using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace ArenaPlatformer1
{
    public abstract class Trap
    {
        public Texture2D Texture;
        public Vector2 Position;

        public int DetonationLimit;

        public List<Emitter> EmitterList = new List<Emitter>();

        /// <summary>
        /// X = CurrentTime, Y = MaxTime
        /// </summary>
        public Vector2 Time;

        /// <summary>
        /// X = CurrentTime, Y = MaxTime
        /// </summary>
        public Vector2 ResetTime;

        /// <summary>
        /// Whether or not the trap exists in the world any more
        /// </summary>
        public bool Exists = true;


        /// <summary>
        /// Whether or not the trap can be interacted with or not
        /// </summary>
        public bool Active = true;

        public static TrapType TrapType;
        public Rectangle CollisionRectangle, DestinationRectangle;

        public virtual void Update(GameTime gameTime)
        {
            if (Time.Y > 0)
            {
                Time.X += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            }

            if (Active == false && ResetTime.Y > 0)
            {
                ResetTime.X += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

                if (ResetTime.X >= ResetTime.Y)
                {
                    ResetTime.X = 0;
                    Active = true;
                }
            }

            foreach (Emitter emitter in EmitterList)
            {
                emitter.Update(gameTime);
            }
        }

        public abstract void Draw(SpriteBatch spriteBatch);

        /// <summary>
        /// Draw the collision box and other useful debug info
        /// </summary>
        /// <param name="graphics"></param>
        /// <param name="basicEffect"></param>
        public void DrawInfo(SpriteBatch spriteBatch, GraphicsDevice graphics, BasicEffect basicEffect)
        {
            #region Draw the collision box for debugging
            VertexPositionColorTexture[] Vertices = new VertexPositionColorTexture[4];
            int[] Indices = new int[8];

            Vertices[0] = new VertexPositionColorTexture()
            {
                Color = Color.Cyan,
                Position = new Vector3(CollisionRectangle.Left, CollisionRectangle.Top, 0),
                TextureCoordinate = new Vector2(0, 0)
            };

            Vertices[1] = new VertexPositionColorTexture()
            {
                Color = Color.Cyan,
                Position = new Vector3(CollisionRectangle.Right - 1, CollisionRectangle.Top, 0),
                TextureCoordinate = new Vector2(1, 0)
            };

            Vertices[2] = new VertexPositionColorTexture()
            {
                Color = Color.Cyan,
                Position = new Vector3(CollisionRectangle.Right - 1, CollisionRectangle.Bottom - 1, 0),
                TextureCoordinate = new Vector2(1, 1)
            };

            Vertices[3] = new VertexPositionColorTexture()
            {
                Color = Color.Cyan,
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

        /// <summary>
        /// Reset the trap so that it has to cool down
        /// </summary>
        public virtual void Reset()
        {
            DetonationLimit--;
            Active = false;
            ResetTime.X = 0;

            if (DetonationLimit <= 0)
            {
                Active = false;
                Exists = false;
            }
        }
    }
}
