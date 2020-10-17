using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace ArenaPlatformer1
{
    public enum LightProjectileType
    {
        Shotgun,
        MachineGun
    };

    public abstract class LightProjectile
    {
        public Ray Ray;
        public bool Active;
        public Vector2 Position;
        public LightProjectileType LightProjectileType;
        public float Damage;
        public float RangeLength;

        public LightProjectile(Vector2 position, Vector2 Direction, float? damage = null)
        {
            Active = true;
            Position = position;
            Ray = new Ray(new Vector3(Position.X, Position.Y, 0), new Vector3(Direction.X, Direction.Y, 0));

            if (damage != null)
                Damage = damage.Value;
            else
                Damage = 0;
        }
    }
}
