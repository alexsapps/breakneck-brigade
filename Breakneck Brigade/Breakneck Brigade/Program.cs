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
        static Client client;
        static ClientGame game;

        static object ProgramLock = new object();

        static void Main(string[] args)
        {

#if PROJECT_GRAPHICS_MODE
            
            GlobalsConfigFolder config = new GlobalsConfigFolder();
            GlobalsConfigFile globalConfig;
            globalConfig = config.Open(BB.GlobalConfigFilename);

            //Thread playThread = null;
            //playThread = new Thread(new ThreadStart(play));
            //playThread.Start();

            while (true)
            {
                client = promptConnect(
                    globalConfig.GetSetting("server-host", BB.DefaultServerHost),
                    globalConfig.GetSetting("server-port", BB.DefaultServerPort));

                if (client != null)
                {
                    client.Disconnected += client_Disconnected; //TODO: this should happen before connecting
                    client.GameModeChanged += client_GameModeChanged;
                    game = client.Game;

                    lock (ProgramLock)
                    {
                        while (true)
                        {
                            if (client.GameMode == GameMode.Init)
                            {
                                Console.WriteLine("Waiting for other players to join.");
                            }
                            else if (client.GameMode == GameMode.Started)
                            {
                                play();
                                Console.WriteLine("Game started.");
                            }
                            else if (client.GameMode == GameMode.Stopping)
                            {
                                Console.WriteLine("Game ended.");
                                break; //reconnect
                            }

                            Monitor.Wait(ProgramLock);
                        }
                    }

                    //if(playThread != null)
                    //    playThread.Join();
                }
                else
                {
                    break;
                }
            }

            Environment.Exit(0); //TODO: do we need this?
#endif
        }

        static void client_GameModeChanged(object sender, EventArgs e)
        {
            lock (ProgramLock)
            {
                Monitor.PulseAll(ProgramLock);
            }
        }

        static void client_Disconnected(object sender, EventArgs e)
        {
            lock (ProgramLock)
            {
                disconnecting = true;
            }
        }

        static Client promptConnect(string defaultHost, string defaultPort)
        {
            using (var prompt = new frmConnect())
            {
                prompt.DefaultHost = defaultHost;
                prompt.DefaultPort = defaultPort;
                if (prompt.ShowDialog() == DialogResult.OK)
                    return prompt.ConnectionClient;
                else
                    return null;
            }
        }

        static bool disconnecting;

        static void play()
        {
            Console.Write("test"); //does this work in another thread?
            lock(client.Lock)
                game = client.Game;

            using (var renderer = new Renderer())
            {
                while (true)
                {
                    if (renderer.ShouldExit())
                    {
                        onClosed();
                        break;
                    }
                    
                    lock(ProgramLock)
                    {
                        if (disconnecting)
                        {
                            disconnecting = false;
                            break;
                        }
                    }
                    
                    lock (client.Lock)
                        if (!(client.GameMode == GameMode.Started || client.GameMode == GameMode.Paused))
                            break;

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

        static void sendEvent(ClientEvent @event)
        {
            lock (client.Lock)
            {
                if (client.GameMode == GameMode.Started)
                {
                    lock (client.ClientEvents)
                    {
                        client.ClientEvents.Add(@event);
                        Monitor.PulseAll(client.ClientEvents);
                    }
                }
                else
                {
                    MessageBox.Show("game hasn't started yet.  first, issue 'play' command to server.");
                }
            }
        }

        static void onClosed()
        {
            if (client != null)
            {
                lock (client.Lock)
                {
                    if (client.IsConnected) //check if connected because we don't know if we will start a disconnection by closing, or if we're closing because we got disconnected.
                    {
                        client.Disconnect();
                    }
                }
            }
        }

    }
}
