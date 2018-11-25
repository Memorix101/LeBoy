using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using LeBoyLib;
using System.Threading;
using System.IO;
using MonoGame.Forms.Controls;
using System;

namespace LeBoy
{
    public class LeBoyForm : UpdateWindow
    {
        SpriteBatch spriteBatch;

        GBZ80 emulator;
        public Thread emulatorThread;
        Texture2D emulatorBackbuffer;
        Texture2D wallpaper;
        Texture2D display;

        public bool emuRunning = false;

        public LeBoyForm()
        {

        }

        protected override void Initialize()
        {
            base.Initialize();            

            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            wallpaper = Editor.Content.Load<Texture2D>("file");
            display = Editor.Content.Load<Texture2D>("display");
            emulatorBackbuffer = new Texture2D(GraphicsDevice, 160, 144);
        }

        protected override void OnClientSizeChanged(EventArgs e)
        {
            base.OnClientSizeChanged(e);
            Invalidate();
        }

        public void loadROM()
        {

            emulator = new GBZ80();

            // loading a rom and starting emulation
            System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
            ofd.DefaultExt = ".gb";
            ofd.Filter = "ROM files (.gb)|*.gb";
            ofd.Multiselect = false;

            System.Windows.Forms.DialogResult result = ofd.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                string filename = ofd.FileName;

                using (FileStream fs = new FileStream(filename, FileMode.Open))
                {
                    using (BinaryReader br = new BinaryReader(fs))
                    {
                        byte[] rom = new byte[fs.Length];
                        for (int i = 0; i < fs.Length; i++)
                            rom[i] = br.ReadByte();
                        emulator.Load(rom);
                    }
                }

                emulatorThread = new Thread(EmulatorWork);
                emulatorThread.Start();
                emuRunning = true;
            }
        }

        /*protected override void UnloadContent()
        {
            // stopping emulation
            if (emulatorThread != null && emulatorThread.IsAlive)
                emulatorThread.Abort();
        }*/

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            GamePadState gamePadState = GamePad.GetState(PlayerIndex.One);

            if (emuRunning)
            {

                // inputs
                emulator.JoypadState[0] = (gamePadState.DPad.Right == ButtonState.Pressed);
                emulator.JoypadState[1] = (gamePadState.DPad.Left == ButtonState.Pressed);
                emulator.JoypadState[2] = (gamePadState.DPad.Up == ButtonState.Pressed);
                emulator.JoypadState[3] = (gamePadState.DPad.Down == ButtonState.Pressed);
                emulator.JoypadState[4] = (gamePadState.Buttons.B == ButtonState.Pressed);
                emulator.JoypadState[5] = (gamePadState.Buttons.A == ButtonState.Pressed);
                emulator.JoypadState[6] = (gamePadState.Buttons.Back == ButtonState.Pressed);
                emulator.JoypadState[7] = (gamePadState.Buttons.Start == ButtonState.Pressed);

                // upload backbuffer to texture
                byte[] backbuffer = emulator.GetScreenBuffer();
                if (backbuffer != null)
                    emulatorBackbuffer.SetData<byte>(backbuffer);
            }

        }

        protected override void Draw()
        {
            base.Draw();

            GraphicsDevice.Clear(Color.Black);

            // compute bounds
            Rectangle bounds = GraphicsDevice.Viewport.Bounds;

            float aspectRatio = GraphicsDevice.Viewport.Bounds.Width / (float)GraphicsDevice.Viewport.Bounds.Height;
            float targetAspectRatio = 160.0f / 144.0f;

            if (aspectRatio > targetAspectRatio)
            {
                int targetWidth = (int)(bounds.Height * targetAspectRatio);
                bounds.X = (bounds.Width - targetWidth) / 2;
                bounds.Width = targetWidth;
            }
            else if (aspectRatio < targetAspectRatio)
            {
                int targetHeight = (int)(bounds.Width / targetAspectRatio);
                bounds.Y = (bounds.Height - targetHeight) / 2;
                bounds.Height = targetHeight;
            }

            // draw backbuffer
            Editor.ShowCursorPosition = false;
            Editor.spriteBatch.Begin(samplerState: SamplerState.PointClamp);

            if (emuRunning)
            {
                Editor.ShowFPS = true;
                //Editor.spriteBatch.Draw(emulatorBackbuffer, new Rectangle(0, 0, Editor.graphics.Viewport.Width, Editor.graphics.Viewport.Height), Color.White);
                Editor.spriteBatch.Draw(emulatorBackbuffer, bounds, Color.White);
            }
            else
            {
                Editor.ShowFPS = false;
                Editor.spriteBatch.Draw(wallpaper, new Rectangle(0, 0, Editor.graphics.Viewport.Width, Editor.graphics.Viewport.Height), Color.White);
            }

            //Editor.spriteBatch.Draw(display, new Rectangle(0, 0, Editor.graphics.Viewport.Width, Editor.graphics.Viewport.Height), Color.White);
            Editor.spriteBatch.End();

            Editor.DrawDisplay();
        }

        private void EmulatorWork()
        {
            double cpuSecondsElapsed = 0.0f;

            MicroStopwatch s = new MicroStopwatch();
            s.Start();

            while (true)
            {
                uint cycles = emulator.DecodeAndDispatch();

                // timer handling
                // note: there's nothing quite reliable / precise enough in cross-platform .Net
                // so this is quite hack-ish / dirty
                cpuSecondsElapsed += cycles / GBZ80.ClockSpeed;

                double realSecondsElapsed = s.ElapsedMicroseconds * 1000000;

                if (realSecondsElapsed - cpuSecondsElapsed > 0.0) // dirty wait
                {
                    realSecondsElapsed = s.ElapsedMicroseconds * 1000000;
                }

                if (s.ElapsedMicroseconds > 1000000) // dirty restart every seconds to not loose too many precision
                {
                    s.Restart();
                    cpuSecondsElapsed -= 1.0;
                }
            }
        }
    }
}
