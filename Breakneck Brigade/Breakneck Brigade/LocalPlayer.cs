﻿using System;
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

        public Vector4 Position;
        public float Orientation { get; set; }
        public float Incline { get; set; }
        public Vector4 Velocity;
        public float MoveSpeed = 0.35f;
        public List<ClientEvent> NetworkEvents;

        private bool _fpsToggle = true;
        
        public LocalPlayer()
        {
            Position = new Vector4(0.0f,-10.0f,0.0f);
            Velocity = new Vector4();
            NetworkEvents = new List<ClientEvent>();

            monitorKey(GlfwKeys.GLFW_KEY_SPACE);
        }

        protected HashSet<GlfwKeys> keys;
        public void Update(InputManager IM, Dictionary<int, ClientGameObject> GOs, Graphics.Camera cam)
        {
            keys = IM.GetKeys();

            detectKeys(IM);

            // Orientation & Incline update
            float rotx, roty;
            IM.GetRotAndClear(out rotx, out roty);

            Orientation = Orientation + roty > 360.0f ? Orientation + roty - 360.0f : Orientation + roty;
            Incline = Incline + rotx > 90.0f ? 90.0f : Incline + rotx < -90.0f ? -90.0f : Incline + rotx;

            // Velocity update
            Velocity[0] = (IM[GlfwKeys.GLFW_KEY_A] || IM[GlfwKeys.GLFW_KEY_LEFT]) ? -1 * MoveSpeed : (IM[GlfwKeys.GLFW_KEY_D] || IM[GlfwKeys.GLFW_KEY_RIGHT]) ? 1 * MoveSpeed : 0.0f;
            Velocity[2] = (IM[GlfwKeys.GLFW_KEY_S] || IM[GlfwKeys.GLFW_KEY_DOWN]) ? -1 * MoveSpeed : (IM[GlfwKeys.GLFW_KEY_W] || IM[GlfwKeys.GLFW_KEY_UP]) ? 1 * MoveSpeed : 0.0f;

            Position[0] += Velocity[2] * (float)Math.Sin(Orientation / 180.0f * -1.0f * Math.PI) - Velocity[0] * (float)Math.Cos((Orientation / 180.0f * Math.PI));
            Position[2] += Velocity[2] * (float)Math.Cos(Orientation / 180.0f * -1.0f * Math.PI) - Velocity[0] * (float)Math.Sin((Orientation / 180.0f * Math.PI));

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
                ClientEvent spawnEvent = new ClientEvent();
                spawnEvent.Type = ClientEventType.Test;
                NetworkEvents.Add(spawnEvent);
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

            foreach(KeyValuePair<int,ClientGameObject> go in GOs){

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
                lock (Game)
                {
                    lock (Game.gameObjects)
                    {
                        ClientGameObject x;
                        Game.gameObjects.TryGetValue(Game.PlayerObjId, out x);
                        _player = (ClientPlayer)x;
                    }
                }
            }
            return _player;
        }

    }
}
