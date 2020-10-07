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
using System.Xml;
using System.IO;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace ArenaLevelEditor
{
    public partial class Form1 : Form
    {
        private Game1 _myGame;
        public Game1 MyGame
        {
            get { return _myGame; }
            set { _myGame = value; }
        }

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
            string dir = Environment.CurrentDirectory;
            string newPath = Path.GetFullPath(Path.Combine(dir, @"..\..\..\..\..\..\..\Levels\\"));
            openFileDialog1.InitialDirectory = newPath;
        }

        private void pctSurface_MouseDown(object sender, MouseEventArgs e)
        {
            Vector2 index = MyGame.CurrentMap.GetMapTileAtPoint(new Vector2(pctSurface.PointToClient(MousePosition).X, pctSurface.PointToClient(MousePosition).Y));

            if (e.Button == MouseButtons.Left)
            {
                MyGame.CurrentMap.AddTile((int)index.X, (int)index.Y, TileType.Solid);
            }

            if (e.Button == MouseButtons.Right)
            {
                MyGame.CurrentMap.AddTile((int)index.X, (int)index.Y, TileType.Empty);
            }
        }

        private void saveLevelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string dir = Environment.CurrentDirectory;
            string newPath = Path.GetFullPath(Path.Combine(dir, @"..\..\..\..\..\..\..\Levels\\"));
            newPath += "Level1.lvl";

            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream(newPath, FileMode.Create);
            formatter.Serialize(stream, MyGame.CurrentMap);
            stream.Close();
        }

        private void loadLevelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            string name = openFileDialog1.FileName;

            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream(name, FileMode.Open);
            Map loadMap = (Map)formatter.Deserialize(stream);

            stream.Close();

            MyGame.CurrentMap = loadMap;
            MyGame.CurrentMap.LoadContent(MyGame.Content);
            //MyGame.CurrentMap.ReloadContent();
        }
    }
}
