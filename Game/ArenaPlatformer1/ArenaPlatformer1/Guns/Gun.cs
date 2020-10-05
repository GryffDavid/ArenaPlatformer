using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace ArenaPlatformer1
{
    /// <summary>
    /// The class representing the physical object in the game which the player can pick up to get access to the weapon
    /// </summary>
    public abstract class Gun : Item
    {
        public GunType GunType;
    }
}
