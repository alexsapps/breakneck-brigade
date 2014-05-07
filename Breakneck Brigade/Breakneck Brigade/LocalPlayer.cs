using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SousChef;
using System.Collections;
using System.Diagnostics;
using Tao.Glfw;
using Tao.OpenGl;

namespace Breakneck_Brigade
{
    /// <summary>
    /// Client-side player object.
    /// </summary>
    class LocalPlayer
    {
        ClientPlayer _player;
        public ClientPlayer Player { get { return _getPlayer(); } }

        public ClientGame Game { get; set; }

        public float Orientation { get; set; }
        public float Incline { get; set; }
        public Vector4 Velocity;
        public float MoveSpeed = 0.35f;
        public List<ClientEvent> NetworkEvents;

        private bool _fpsToggle = true;
        
        public LocalPlayer()
        {
            Velocity = new Vector4();
            NetworkEvents = new List<ClientEvent>();

            monitorKey(GlfwKeys.GLFW_KEY_SPACE);
            monitorKey(GlfwKeys.GLFW_KEY_W);
        }

        protected HashSet<GlfwKeys> keys;
        static float lastx, lastz;
        long lasttime = 0;
        public void Update(InputManager IM, ClientGame game, Graphics.Camera cam)
        {
            game.Lock.AssertHeld();
            cam.Lock.AssertHeld();

            long timediff = (DateTime.Now.Ticks - lasttime) / TimeSpan.TicksPerMillisecond;
            if (timediff > 200)
                timediff = 0;
            lasttime = DateTime.Now.Ticks;

            keys = IM.GetKeys();

            detectKeys(IM);

            // Orientation & Incline update
            float rotx, roty;
            IM.GetRotAndClear(out rotx, out roty);

            Orientation = Orientation + roty > 360.0f ? Orientation + roty - 360.0f : Orientation + roty;
            Incline = Incline + rotx > 90.0f ? 90.0f : Incline + rotx < -90.0f ? -90.0f : Incline + rotx;

            //If there was a change in player facing orientation, send an orientation update to the server
            if(roty != 0.0f)
            {
                NetworkEvents.Add(new ClientChangeOrientationEvent() { Roty = roty });
            }

            // Velocity update
            Velocity.X = (IM[GlfwKeys.GLFW_KEY_A] || IM[GlfwKeys.GLFW_KEY_LEFT]) ? -1 * MoveSpeed : (IM[GlfwKeys.GLFW_KEY_D] || IM[GlfwKeys.GLFW_KEY_RIGHT]) ? 1 * MoveSpeed : 0.0f;
            Velocity.Z = (IM[GlfwKeys.GLFW_KEY_S] || IM[GlfwKeys.GLFW_KEY_DOWN]) ? -1 * MoveSpeed : (IM[GlfwKeys.GLFW_KEY_W] || IM[GlfwKeys.GLFW_KEY_UP]) ? 1 * MoveSpeed : 0.0f;

            var xDiff = Velocity.Z * (float)Math.Sin(Orientation / 180.0f * -1.0f * Math.PI) - Velocity.X * (float)Math.Cos((Orientation / 180.0f * Math.PI));
            var zDiff = Velocity.Z * (float)Math.Cos(Orientation / 180.0f * -1.0f * Math.PI) - Velocity.X * (float)Math.Sin((Orientation / 180.0f * Math.PI));
            Coordinate diff = new Coordinate(-xDiff * timediff / 10, 0, -zDiff * timediff / 10);
            if (diff.x != 0 || diff.z != 0)
            {
                NetworkEvents.Add(new ClientBeginMoveEvent() { Delta = diff });
                var pos = GetPosition();
                lastx = pos.X;
                lastz = pos.Z;
            }
            
            if (IM[GlfwKeys.GLFW_KEY_ESCAPE] || Glfw.glfwGetWindowParam(Glfw.GLFW_ACTIVE) == Gl.GL_FALSE)
            {
                if (_fpsToggle)
                {
                    if (IM.fpsMode)
                    {
                        IM.DisableFPSMode();  
                    }
                    else
                    {
                        IM.EnableFPSMode();
                    }
                    _fpsToggle = false;
                }
            }
            else
            {
                _fpsToggle = true;
            }

            if (IM[GlfwKeys.GLFW_MOUSE_BUTTON_LEFT])
            {
                if (!IM.fpsMode)
                {
                    IM.EnableFPSMode();
                }
            }

            if (keyDown(GlfwKeys.GLFW_KEY_SPACE))
            {
                //Test code
                var spawnEvent = new ClientTestEvent();
                NetworkEvents.Add(spawnEvent);
            }

            if (keyDown(GlfwKeys.GLFW_KEY_W))
            {
                //NetworkEvents.Add(new ClientBeginMoveEvent()); //this is being done above at the moment.
            }
            // 3D picking stuff
            /*int width, height;
            Glfw.glfwGetWindowSize(out width, out height);

            Vector4 view = cam.LookingAt - cam.Position;
            view.Normalize();

            Vector4 h = view.CrossProduct(cam.Up);
            h.Normalize();

            Vector4 v = h.CrossProduct(view);
            v.Normalize();

            // convert fov to radians
            float rad = (float)(cam.FOV * Math.PI / 180.0);
            int vLength = (int)((float)Math.Tan((double)rad / 2) * cam.NearClip);
            int hLength = vLength * (int)((float)width / (float)height);
            v *= vLength;
            h *= hLength;

            int x = IM.originX - (width / 2);
            int y = IM.originY - (height / 2);

            y /= (height / 2);
            x /= (width / 2);*/

            Vector4 c = new Vector4();
            Vector4 v = cam.Position;
            Vector4 d = v - cam.LookingAt;

            foreach (var go in game.gameObjects.Values)
            {

            }
        }

        HashSet<GlfwKeys> downKeys = new HashSet<GlfwKeys>();
        HashSet<GlfwKeys> upKeys = new HashSet<GlfwKeys>();

        bool[] setKeys = new bool[512];
        List<GlfwKeys> monitorKeys = new List<GlfwKeys>();
        protected void monitorKey(GlfwKeys key)
        {
            monitorKeys.Add(key);
        }
        protected void unmonitorKey(GlfwKeys key)
        {
            monitorKeys.Remove(key);
        }
        protected void detectKeys(InputManager IM)
        {
           downKeys.Clear();
           upKeys.Clear();
           foreach (GlfwKeys i in monitorKeys)
           {
               if(IM[i])
               {
                   if (!setKeys[(int)i])
                   {
                       setKeys[(int)i] = true;
                       downKeys.Add(i);
                   }
               }
               else
               {
                   if (setKeys[(int)i])
                   {
                       setKeys[(int)i] = false;
                       upKeys.Add(i);
                   }
               }
           }
        }

        /// <summary>
        /// determines if key was pressed down
        /// </summary>
        /// <param name="key">the key</param>
        /// <returns>true iff key was just now pressed down.</returns>
        protected bool keyDown(GlfwKeys key)
        {
            return downKeys.Contains(key);
        }

        /// <summary>
        /// determines if key was lifted up
        /// </summary>
        /// <param name="key">the key</param>
        /// <returns>true iff the key was just now lifted up.</returns>
        protected bool keyUp(GlfwKeys key)
        {
            return upKeys.Contains(key);
        }

        private ClientPlayer _getPlayer()
        {
            if (_player == null)
            {
                lock (Game.Lock)
                {
                    ClientGameObject x;
                    Game.gameObjects.TryGetValue(Game.PlayerObjId, out x);
                    _player = (ClientPlayer)x;
                }
            }
            return _player;
        }

        public Vector4 GetPosition()
        {
            if (Game != null)
            {
                lock (Game.Lock) //game could have become null by now.  ideally we would lock the client first
                {
                    if (Player != null)
                    {
                        var result = new Vector4(Player.Position);
                        result.X = -result.X;
                        result.Y -= 10;
                        result.Z = -result.Z;
                        return result;
                    }
                }
            }
            return new Vector4();
        }
    }
}
