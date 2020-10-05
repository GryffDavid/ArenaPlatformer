using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace ArenaPlatformer1
{
    public enum LightProjectileType { MachineGun };
    
    public abstract class LightProjectile
    {
        public Vector2 StartPosition;
        public Ray Ray;
        public LightProjectileType LightProjectileType;
    }
}
