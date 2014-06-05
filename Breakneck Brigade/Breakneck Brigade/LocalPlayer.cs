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
using System.Threading;

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
        public float WalkSpeed = 140f;
        public float RunSpeed = 380f;
        
        public List<ClientEvent> NetworkEvents;
        private bool _stopped = false;

        
        
        public LocalPlayer()
        {
            Velocity = new Vector4();
            NetworkEvents = new List<ClientEvent>();
        }

        protected HashSet<GlfwKeys> keys;
        float lastx, lastz;
        bool running;
        DateTime lastDownRun;
        GlfwKeys lastDownRunKey;

        bool throwing;
        DateTime lastDownThrow;
        DateTime lastThrow;

        public void Update(InputManager IM, ClientGame game, Graphics.Camera cam)
        {
            game.Lock.AssertHeld();
            cam.Lock.AssertHeld();
            IM.Lock.AssertHeld();

            keys = IM.GetKeys();
            downKeys = IM.GetDownKeyEdges();
            upKeys = IM.GetUpKeyEdges();

            // Orientation & Incline update
            float rotx, roty;
            IM.GetRotAndClear(out rotx, out roty);

            Orientation = Orientation + roty > 360.0f ? Orientation + roty - 360.0f : Orientation + roty;
            Incline = Incline + rotx > 90.0f ? 90.0f : Incline + rotx < -90.0f ? -90.0f : Incline + rotx;

            //If there was a change in player facing orientation, send an orientation update to the server
            if(roty != 0.0f)
            {
                NetworkEvents.Add(new ClientChangeOrientationEvent() { Orientation = Orientation, Incline = Incline });
            }

            {
                GlfwKeys downKey = GlfwKeys.None;
                
                if (checkKey(GlfwKeys.GLFW_KEY_A, ref downKey) || checkKey(GlfwKeys.GLFW_KEY_D, ref downKey) || checkKey(GlfwKeys.GLFW_KEY_S, ref downKey) || checkKey(GlfwKeys.GLFW_KEY_W, ref downKey) || checkKey(GlfwKeys.GLFW_KEY_LEFT, ref downKey) || checkKey(GlfwKeys.GLFW_KEY_RIGHT, ref downKey) || checkKey(GlfwKeys.GLFW_KEY_DOWN, ref downKey) || checkKey(GlfwKeys.GLFW_KEY_UP, ref downKey))
                {
                    if ((DateTime.Now.Subtract(lastDownRun).TotalMilliseconds < 600) && lastDownRunKey == downKey)
                        running = true;

                    lastDownRun = DateTime.Now;
                    lastDownRunKey = downKey;
                }
            }
            float MoveSpeed = running || IM[GlfwKeys.GLFW_KEY_LEFT_SHIFT] || IM[GlfwKeys.GLFW_KEY_RIGHT_SHIFT] ? RunSpeed : WalkSpeed;

            // Velocity update
            Velocity.X = -((IM[GlfwKeys.GLFW_KEY_A] || IM[GlfwKeys.GLFW_KEY_LEFT]) ? -1 * MoveSpeed : (IM[GlfwKeys.GLFW_KEY_D] || IM[GlfwKeys.GLFW_KEY_RIGHT]) ? 1 * MoveSpeed : 0.0f);
            Velocity.Z = -((IM[GlfwKeys.GLFW_KEY_S] || IM[GlfwKeys.GLFW_KEY_DOWN]) ? -1 * MoveSpeed : (IM[GlfwKeys.GLFW_KEY_W] || IM[GlfwKeys.GLFW_KEY_UP]) ? 1 * MoveSpeed : 0.0f);

            var xDiff = Velocity.Z * (float)Math.Sin(Orientation / 180.0f * -1.0f * Math.PI) - Velocity.X * (float)Math.Cos((Orientation / 180.0f * Math.PI));
            var zDiff = Velocity.Z * (float)Math.Cos(Orientation / 180.0f * -1.0f * Math.PI) - Velocity.X * (float)Math.Sin((Orientation / 180.0f * Math.PI));
            Coordinate diff = new Coordinate(xDiff, 0, zDiff);
            if (diff.x != 0 || diff.z != 0)
            {
                _stopped = false;
                NetworkEvents.Add(new ClientBeginMoveEvent() { Delta = diff });
                var pos = GetPosition();
                lastx = pos.X;
                lastz = pos.Z;
            }
            else if(!_stopped)
            {
                _stopped = true;
                running = false;
                NetworkEvents.Add(new ClientBeginMoveEvent() { Delta = diff });
                var pos = GetPosition();
                lastx = pos.X;
                lastz = pos.Z;
            }


            // handle jumping key
            if (this.downKeys.Contains(GlfwKeys.GLFW_KEY_SPACE))
            {
                NetworkEvents.Add(new ClientJumpEvent() { isJumping = true});
            }
            // handle throwing key
            if (this.downKeys.Contains(GlfwKeys.GLFW_MOUSE_BUTTON_LEFT))
            {
                throwSomething();
                lastDownThrow = DateTime.Now;
                throwing = true;
                lastThrow = DateTime.Now;
            }
            if(this.downKeys.Contains(GlfwKeys.GLFW_KEY_H))
            {
                NetworkEvents.Add(new ClientHintEvent() { });
            }

            if(IM[GlfwKeys.GLFW_MOUSE_BUTTON_LEFT])
            {
                if (throwing && DateTime.Now.Subtract(lastDownThrow).TotalMilliseconds > 1000) //user holds mouse down for one second
                {
                    if (DateTime.Now.Subtract(lastThrow).TotalMilliseconds > 60) //spawn oranges every other server tick
                    {
                        lastThrow = DateTime.Now;
                        throwSomething();
                    }
                }
            }
            if(this.upKeys.Contains(GlfwKeys.GLFW_MOUSE_BUTTON_LEFT))
            {
                throwing = false;
            }

            // handle dashing key
            if (this.downKeys.Contains(GlfwKeys.GLFW_MOUSE_BUTTON_RIGHT))
            {
                NetworkEvents.Add(new ClientDashEvent() { isDashing = true});
            }

            // Handle cooker eject
            if (this.downKeys.Contains(GlfwKeys.GLFW_KEY_LEFT_SHIFT))
            {
                this.NetworkEvents.Add(new ClientEjectEvent());
            }

            // Handle cook event
            if (this.downKeys.Contains(GlfwKeys.GLFW_KEY_E))
            {
                this.NetworkEvents.Add(new ClientCookEvent());
            }
            // Handle drop event
            if(this.downKeys.Contains(GlfwKeys.GLFW_KEY_Q))
            {
                NetworkEvents.Add(new ClientLeftClickEvent() { Hand = "left", Orientation = Orientation, Incline = Incline, Force = 0.0f });
            }

            IM.FpsOk = Glfw.glfwGetWindowParam(Glfw.GLFW_ACTIVE) == Gl.GL_TRUE;
            
            if (IM[GlfwKeys.GLFW_MOUSE_BUTTON_LEFT])
            {
                IM.FpsOk = true;
                IM.CaptureMouse = true;
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

            // Change page down.
            if(keyDown(GlfwKeys.GLFW_KEY_R))
            {
                this.Game.DecrementPage();

                // Play sound
                //double distance = Program.getDistance(this.GetPosition(), this.GetPosition());
                int volume = (int)Math.Log(4, 2.0);
                SoundThing.Play(BBSound.pageturn1, volume);
            }

            // Change page up.
            if (keyDown(GlfwKeys.GLFW_KEY_F))
            {
                this.Game.IncrementPage();

                // Play sound
                //double distance = Program.getDistance(this.GetPosition(), this.GetPosition());
                int volume = (int)Math.Log(4, 2.0);
                SoundThing.Play(BBSound.pageturn2, volume);
            }

            if (this.Player != null)
            {
                this.Game.LookatId = this.Player.LookingAtId;
                this.Game.HeldId = this.Player.HeldId;
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

            foreach (var go in game.GameObjectsCache.Values)
            {

            }
        }

        protected void throwSomething()
        {
            NetworkEvents.Add(new ClientLeftClickEvent() { Hand = "left", Orientation = Orientation, Incline = Incline, Force = 1.0f });
        }

        private bool checkKey(GlfwKeys key, ref GlfwKeys def)
        {
            bool result = downKeys.Contains(key);
            if (result)
                def = key;
            return result;
        }

        HashSet<GlfwKeys> downKeys = new HashSet<GlfwKeys>();
        HashSet<GlfwKeys> upKeys = new HashSet<GlfwKeys>();

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
                    if (Game.PlayerObjId.HasValue)
                    {
                        Game.GameObjectsCache.TryGetValue(Game.PlayerObjId.Value, out x);
                        _player = (ClientPlayer)x;
                    }
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
                        return new Vector4(Player.Position);
                    }
                }
            }
            return new Vector4();
        }
    }
}
