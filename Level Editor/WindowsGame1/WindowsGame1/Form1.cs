using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace ArenaLevelEditor
{


    //Texture2D Texture;

    //Vector2 Position, PreviousPosition, AngleRange, ScaleRange, TimeRange,  SpeedRange, 
    //        RotationIncrementRange, StartingRotationRange, 
    //        EmitterDirection, EmitterVelocity, YRange, Friction;

    //float Transparency, Gravity, ActiveSeconds, Interval, EmitterSpeed, 
    //      EmitterAngle, EmitterGravity, FadeDelay, StartingInterval, BounceY;

    //Color StartColor, EndColor, ThirdColor;

    //bool Fade, CanBounce, AddMore, Shrink, StopBounce, HardBounce, 
    //     BouncedOnGround, RotateVelocity, FlipHor, FlipVer, ReduceDensity, SortParticles, Grow;

    //int Burst;

    //double IntervalTime, CurrentTime;

    //SpriteEffects Orientation = SpriteEffects.None;

    public partial class Form1 : Form
    {
        public Vector2 EmitterPosition = new Vector2(1280 / 2, 720 / 2);
        public System.Drawing.Color backgroundColor = System.Drawing.Color.CornflowerBlue;
        public string TextureLocation = "";
        public Texture2D EmitterTexture;

        private GraphicsDevice _graphics;

        public GraphicsDevice Graphics
        {
            get { return _graphics; }
            set { _graphics = value; }
        }        

        public Form1()
        {
            InitializeComponent();
            Form2 form2 = new Form2();
            form2.Show();
        }

        public IntPtr getDrawSurface()
        {
            return pctSurface.Handle;
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            //Exit the XNA code here
            //Unload content
        }
        
        Texture2D BitmapToTexture(Bitmap bmap)
        {
            Microsoft.Xna.Framework.Color[] colors = new Microsoft.Xna.Framework.Color[bmap.Width * bmap.Height];
            for (int x = 0; x < bmap.Width; x++)
            {
                for (int y = 0; y < bmap.Height; y++)
                {
                    int index = x + y * bmap.Width;
                    System.Drawing.Color color = bmap.GetPixel(x, y);
                    Vector4 colorVector =
                        new Vector4((float)color.R / 255f,
                                    (float)color.G / 255f,
                                    (float)color.B / 255f, color.A/255f);
                    colors[index] = Microsoft.Xna.Framework.Color.FromNonPremultiplied(colorVector);
                    
                }
            }

            Texture2D texture = new Texture2D(Graphics, bmap.Width, bmap.Height);
            texture.SetData<Microsoft.Xna.Framework.Color>(colors);
            return texture;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
