using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArenaPlatformer1
{    
    //The base class for trap items which the player picks up to then use to place the actual traps in the level
    abstract class TrapPickup : Item
    {
        public TrapType TrapType;
    }
}
