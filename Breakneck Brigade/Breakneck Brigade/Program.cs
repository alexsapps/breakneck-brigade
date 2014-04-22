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

namespace Breakneck_Brigade
{
    class Program
    {
        public InputManager IM = new InputManager();

        static void Main(string[] args)
        {
#if PROJECT_NETWORK_MODE
                (new FakeClient()).ShowDialog();
                return;
#endif

#if PROJECT_GRAPHICS_MODE
            var client = promptConnect();
            if (client != null)
            {
                play(client);
            }
            Environment.Exit(0); //TODO: do we need this?
#endif

#if PROJECT_GAMECODE_TEST

#endif
        }

        static Client promptConnect()
        {
            throw new NotImplementedException(); //TODO: can do if (new FakeClient().ShowDialog() == DialogResult.Yes) return new Client ( .txtServer, .txtPort ) else return null and rename FakeClient to frmConnect
        }

        static void play(Client client)
        {
            ClientGame game;
            ClientPlayer cPlayer;

            lock (client.Lock)
            {
                game = client.Game;
                cPlayer = new ClientPlayer(0, new Vector4(), game);
            }

            using (var renderer = new Renderer())
            {
                while (true)
                {
                    if (Glfw.glfwGetWindowParam(Glfw.GLFW_OPENED) != Gl.GL_TRUE)
                        break;

                    lock (client.Lock)
                        if (!(client.GameMode == GameMode.Started || client.GameMode == GameMode.Paused))
                            break;

                    lock (game.gameObjects)
                    {
                        renderer.Render(cPlayer);
                        game.HasUpdates = false;
                        do
                        {
                            Monitor.Wait(game.gameObjects);
                        } while (!game.HasUpdates);
                    }
                }
            }
        }
    }
}
