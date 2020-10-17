using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace ArenaPlatformer1
{
    public class MachineGunProjectile : LightProjectile
    {
        public MachineGunProjectile(Vector2 position, Vector2 direction, float? damage = null) : base(position, direction, damage)
        {
            LightProjectileType = LightProjectileType.MachineGun;
            RangeLength = float.PositiveInfinity;
        }
    }
}
