using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace ArenaLevelEditor
{    
    public class Game1 : Game
    {
        #region Windows Form Stuff
        private IntPtr drawSurface;
        Form1 Form1;

        void graphics_PreparingDeviceSettings(object sender, PreparingDeviceSettingsEventArgs e)
        {
            e.GraphicsDeviceInformation.PresentationParameters.DeviceWindowHandle = drawSurface;
        }

        private void Game1_VisibleChanged(object sender, EventArgs e)
        {
            if (System.Windows.Forms.Control.FromHandle((this.Window.Handle)).Visible == true)
                System.Windows.Forms.Control.FromHandle((this.Window.Handle)).Visible = false;
        }
        #endregion
        
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        public Map CurrentMap;

        public Game1(IntPtr drawSurface, Form1 form)
        {
            Form1 = form;
            Form1.MyGame = this;

            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            this.drawSurface = drawSurface;
            graphics.PreparingDeviceSettings += new EventHandler<PreparingDeviceSettingsEventArgs>(graphics_PreparingDeviceSettings);
            System.Windows.Forms.Control.FromHandle((this.Window.Handle)).VisibleChanged += new EventHandler(Game1_VisibleChanged);

            graphics.PreferredBackBufferWidth = 1920;
            graphics.PreferredBackBufferHeight = 1080;
        }
        
        protected override void Initialize()
        {
            CurrentMap = new Map();
            base.Initialize();
        }
        
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            Form1.Graphics = GraphicsDevice;

            CurrentMap.LoadContent(Content);
        }
        
        protected override void UnloadContent()
        {

        }
        
        protected override void Update(GameTime gameTime)
        {
            if (Form1.IsDisposed == true)
            {
                this.Exit();
            }

            base.Update(gameTime);
        }
        
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);            
            spriteBatch.Begin();
            CurrentMap.Draw(spriteBatch);
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
