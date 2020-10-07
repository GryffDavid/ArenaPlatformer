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
        private IntPtr drawSurface;

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Texture2D Texture;
        Form1 Form1;

        Color BackgroundColor = Color.CornflowerBlue;

        bool blendState = false;
        

        void graphics_PreparingDeviceSettings(object sender, PreparingDeviceSettingsEventArgs e)
        {
                e.GraphicsDeviceInformation.PresentationParameters.DeviceWindowHandle =
                drawSurface;
        }

        private void Game1_VisibleChanged(object sender, EventArgs e)
        {
                if (System.Windows.Forms.Control.FromHandle((this.Window.Handle)).Visible == true)
                    System.Windows.Forms.Control.FromHandle((this.Window.Handle)).Visible = false;
        }

        public Game1(IntPtr drawSurface, Form1 form)
        {
            Form1 = form;
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

            base.Initialize();
        }
        
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            Form1.Graphics = GraphicsDevice;

            Texture = Content.Load<Texture2D>("Sprite");
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
            GraphicsDevice.Clear(BackgroundColor);

            if (blendState == false)
                spriteBatch.Begin();
            else
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive);
            
            spriteBatch.End();

            base.Draw(gameTime);
        }


    }
}
