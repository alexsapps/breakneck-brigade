using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SousChef;
using Tao.Glfw;
using Tao.OpenGl;
using System.IO;
using Breakneck_Brigade.Graphics;
using System.Threading;
using System.Windows.Forms;

namespace Breakneck_Brigade
{
    class Program
    {

        static void Main(string[] args)
        {
#if PROJECT_NETWORK_MODE
                (new FakeClient()).ShowDialog();
                return;
#endif

#if PROJECT_GRAPHICS_MODE
            Renderer renderer = new Renderer();
            while(!renderer.ShouldExit())
            {
                renderer.Render();
            }
            Environment.Exit(0);
#endif

#if PROJECT_GAMECODE_TEST
            var client = promptConnect();
            if (client != null)
            {
                play(client);
            }
            Environment.Exit(0); //TODO: do we need this?
#endif
        }

        static Client promptConnect()
        {
            Client retClient = new Client();
            string  host = "127.0.0.1";
            int     port = 54320;
            try
            {
                retClient.Connect(host, port);
            }catch(Exception ex) {
                MessageBox.Show("Error connecting. Is the server running? " + ex.Message);
            }
            return retClient;
        }

        static void play(Client client)
        {

            ClientGame game;
            lock(client.Lock)
                game = client.Game;

            using (var renderer = new Renderer())
            {
                while (true)
                {
                    if (Glfw.glfwGetWindowParam(Glfw.GLFW_OPENED) != Gl.GL_TRUE)
                        break;

                    lock (client.Lock)
                        if (!(client.GameMode == GameMode.Started || client.GameMode == GameMode.Paused))
                            continue;

                    if(game != null)
                    { 
                        lock (game.gameObjects)
                        {
                            game.HasUpdates = false;
                            do
                            {
                                Monitor.Wait(game.gameObjects);
                            } while (!game.HasUpdates);
                        }
                    }
                    renderer.Render();
                }
            }
        }
    }
}
