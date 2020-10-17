using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace ArenaPlatformer1
{
    public class ShockWave
    {
        public float MaxTime, CurrentTime;
        public Vector3 WaveParams;
        public Vector2 Position;
        public bool Active = true;

        public ShockWave(Vector2 position, Vector3 waveParams, float maxTime)
        {
            MaxTime = maxTime;
            WaveParams = waveParams;
            Position = position;
        }

        public void Update(GameTime gameTime)
        {
            CurrentTime += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            if (CurrentTime >= MaxTime)
            {
                Active = false;
            }
        }
    }
}
