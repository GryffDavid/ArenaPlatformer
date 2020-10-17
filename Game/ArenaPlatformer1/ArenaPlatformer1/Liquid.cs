using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace ArenaPlatformer1
{
    class Liquid
    {
        public RenderTarget2D MetaballTarget;
        List<Metaball> MetaballList = new List<Metaball>();
        public AlphaTestEffect AlphaTest;
        static Random Random = new Random();

        int LeftToAdd = 0;
        Vector2 AddPosition;
        float AddInterval = 5;
        float CurrentAddInterval = 0;
        Color Color;

        public Liquid(GraphicsDevice device, Color color)
        {
            MetaballTarget = new RenderTarget2D(device, 1920, 1080);

            AlphaTest = new AlphaTestEffect(device);
            AlphaTest.ReferenceAlpha = 127;

            AlphaTest.Projection = Matrix.CreateOrthographicOffCenter(0, 1920, 1080, 0, 0, 1);
            Color = color;
        }

        public void AddMetaballs(Vector2 position, int num, Vector2 scale, Vector2 xVelRange, Vector2 yVelRange)
        {
            //AddPosition = position;
            //LeftToAdd = num;

            for (int i = 0; i < num; i++)
            {
                float myScale;

                if (scale == Vector2.Zero)
                    myScale = (float)DoubleRange(1.8, 2.5);
                else
                    myScale = (float)DoubleRange(scale.X, scale.Y);

                Metaball newBall = new Metaball(this, position, new Vector2((float)DoubleRange(xVelRange.X, xVelRange.Y), (float)DoubleRange(yVelRange.X, yVelRange.Y)), 0, myScale);
                MetaballList.Add(newBall);
            }

            MetaballList.RemoveAll(Ball => Ball.Active == false || Ball.Scale <= 0);
        }

        public void Update(GameTime gameTime)
        {
            //if (LeftToAdd > 0)
            //{
            //    CurrentAddInterval += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            //    if (CurrentAddInterval >= AddInterval)
            //    {
            //        LeftToAdd--;
            //        Metaball newBall = new Metaball(AddPosition, new Vector2((float)DoubleRange(-3, 3), (float)DoubleRange(-3, -1)), 0);
            //        MetaballList.Add(newBall);
            //        CurrentAddInterval = 0;
            //    }
            //}

            for (int i = 0; i < MetaballList.Count; i++)
            {
                //var thing = MetaballList.Where(theBall =>
                //theBall != ball &&
                //Math.Abs(ball.Velocity.X) < 0.5 && Math.Abs(ball.Velocity.Y) < 0.5 &&
                //Math.Abs(theBall.Velocity.X) < 0.5 && Math.Abs(theBall.Velocity.Y) < 0.5 &&
                //ball.PushesBottomTile == true &&
                //theBall.PushesBottomTile == true);

                //if (thing.Count() > 0)
                //{
                //    float minDist = thing.Min(myBall => Vector2.Distance(myBall.Position, ball.Position));

                //    if (minDist > 20)
                //    {
                //        if (ball.Scale > 0)
                //            ball.Scale -= 0.01f;
                //    }

                //}

                MetaballList[i].Update(gameTime);
            }

            MetaballList.RemoveAll(Ball => Ball.Active == false || Ball.Scale <= 0);
        }

        public void LoadContent(ContentManager content)
        {

        }
        
        public void Draw(SpriteBatch spriteBatch)
        {
            GraphicsDevice device = spriteBatch.GraphicsDevice;
            device.SetRenderTarget(MetaballTarget);
            device.Clear(Color.Transparent);

            spriteBatch.Begin(0, BlendState.AlphaBlend);

            foreach (Metaball ball in MetaballList)
            {
                Vector2 origin = new Vector2(ball.Texture.Width, ball.Texture.Height) / 2;
                spriteBatch.Draw(ball.Texture, 
                    new Rectangle((int)ball.Position.X, 
                                  (int)ball.Position.Y, 
                                  (int)(ball.Texture.Width * Math.Max(ball.Scale, Math.Abs(ball.Velocity.Y) * 0.3)), 
                                  (int)(ball.Texture.Height * ball.Scale)), 
                    null, Color, ball.Orientation, origin, SpriteEffects.None, 0f);
            }

            spriteBatch.End();
        }

        public double DoubleRange(double one, double two)
        {
            return one + Random.NextDouble() * (two - one);
        }
    }
}
