using System;

namespace ArenaLevelEditor
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Form1 form = new Form1();
            form.Show();
            Game1 game = new Game1(form.getDrawSurface(), form);
            game.Run();
        }
    }
#endif
}

