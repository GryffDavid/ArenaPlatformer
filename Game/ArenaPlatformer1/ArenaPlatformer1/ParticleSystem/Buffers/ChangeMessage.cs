using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.Runtime.InteropServices;

namespace ArenaPlatformer1
{
    [StructLayout(LayoutKind.Explicit)]
    public struct ChangeMessage
    {
        [FieldOffset(0)]
        public ChangeMessageType MessageType;

        [FieldOffset(4)]
        public int ID;

        [FieldOffset(8)]
        public Vector2 Position;

        [FieldOffset(16)]
        public float Rotation;

        [FieldOffset(20)]
        public Color Color;

        [FieldOffset(24)]
        public float Velocity;

        [FieldOffset(28)]
        public float Scale;

        [FieldOffset(32)]
        public float Transparency;
    }
}
