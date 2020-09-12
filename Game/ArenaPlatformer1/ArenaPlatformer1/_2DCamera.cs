using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace ArenaPlatformer1
{
    class _2DCamera
    {
        public Matrix Transform;

        public Vector2 Position;
        int ShakeCount = 30;
        int Shakes = 31;
        float ShakeIntensity = 2.0f;
        static Random Random = new Random();
        
        public _2DCamera()
        {
            Transform = Matrix.CreateTranslation(-new Vector3(1920 / 2, 1080 / 2, 0)) *
                        Matrix.CreateTranslation(new Vector3(Position.X, Position.Y, 0)) *
                        Matrix.CreateTranslation(new Vector3(1920 / 2, 1080 / 2, 0));
        }

        public void Update(GameTime gameTime)
        {
            if (Shakes < ShakeCount)
            {
                Position = new Vector2(RandomFloat(-ShakeIntensity, ShakeIntensity), RandomFloat(-ShakeIntensity, ShakeIntensity));
                Shakes++;
            }

            if (Shakes >= ShakeCount)
            {
                ShakeCount = 0;
                Shakes = 0;
                Position = Vector2.Zero;
            }

            Transform = Matrix.CreateTranslation(-new Vector3(1920 / 2, 1080 / 2, 0)) *
                        Matrix.CreateTranslation(new Vector3(Position.X, Position.Y, 0)) *
                        Matrix.CreateTranslation(new Vector3(1920 / 2, 1080 / 2, 0));
        }

        public static float RandomFloat(float a, float b)
        {
            return (float)(a + Random.NextDouble() * (b - a));
        }

        public void Shake(int shakeCount, float intensity)
        {
            ShakeIntensity = intensity;
            ShakeCount = shakeCount;
            Shakes = 0;
        }
    }
}
