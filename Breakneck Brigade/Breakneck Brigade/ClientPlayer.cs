using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SousChef;
using System.Collections;
using System.Diagnostics;

namespace Breakneck_Brigade
{
    /// <summary>
    /// Client-side player object.
    /// </summary>
    public class ClientPlayer //TODO: subclass ClientGameObject or use Composition, then Position variable
    {
        public Vector4 Position;
        public float Orientation { get; set; }
        public float Incline { get; set; }
        public Vector4 Velocity;
        public float MoveSpeed = 0.35f;
        public List<ClientEvent> NetworkEvents;

        private bool _fpsToggle = true;
        
        public ClientPlayer()
        {
            Position = new Vector4(0.0f,0.0f,-50.0f);
            Velocity = new Vector4();
            NetworkEvents = new List<ClientEvent>();

            monitorKey(InputManager.GLFW_KEY_SPACE);
        }

        protected HashSet<int> keys;
        public void Update(InputManager IM)
        {
            keys = IM.GetKeys();

            detectKeys(IM);

            // Orientation & Incline update
            float rotx, roty;
            IM.GetRotAndClear(out rotx, out roty);

            Orientation = Orientation + roty > 360.0f ? Orientation + roty - 360.0f : Orientation + roty;
            Incline = Incline + rotx > 90.0f ? 90.0f : Incline + rotx < -90.0f ? -90.0f : Incline + rotx;

            // Velocity update
            Velocity[0] = (IM[InputManager.GLFW_KEY_A] || IM[InputManager.GLFW_KEY_LEFT]) ? -1 * MoveSpeed : (IM[InputManager.GLFW_KEY_D] || IM[InputManager.GLFW_KEY_RIGHT]) ? 1 * MoveSpeed : 0.0f;
            Velocity[2] = (IM[InputManager.GLFW_KEY_S] || IM[InputManager.GLFW_KEY_DOWN]) ? -1 * MoveSpeed : (IM[InputManager.GLFW_KEY_W] || IM[InputManager.GLFW_KEY_UP]) ? 1 * MoveSpeed : 0.0f;

            Position[0] += Velocity[2] * (float)Math.Sin(Orientation / 180.0f * -1.0f * Math.PI) - Velocity[0] * (float)Math.Cos((Orientation / 180.0f * Math.PI));
            Position[2] += Velocity[2] * (float)Math.Cos(Orientation / 180.0f * -1.0f * Math.PI) - Velocity[0] * (float)Math.Sin((Orientation / 180.0f * Math.PI));

            if(IM[InputManager.GLFW_KEY_ESCAPE])
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

            if(IM[InputManager.GLFW_MOUSE_BUTTON_LEFT])
            {
                if (!IM.fpsMode)
                {
                    IM.EnableFPSMode();
                }
            }

            if(keyDown(InputManager.GLFW_KEY_SPACE))
            {
                //Test code
                ClientEvent spawnEvent = new ClientEvent();
                spawnEvent.Type = ClientEventType.Test;
                NetworkEvents.Add(spawnEvent);
                Console.WriteLine("TEST: spawn event " + spawnEvent.Type.ToString());
            }
        }

        HashSet<int> downKeys = new HashSet<int>();
        HashSet<int> upKeys = new HashSet<int>();

        bool[] setKeys = new bool[512];
        List<int> monitorKeys = new List<int>();
        protected void monitorKey(int key)
        {
            monitorKeys.Add(key);
        }
        protected void unmonitorKey(int key)
        {
            monitorKeys.Remove(key);
        }
        protected void detectKeys(InputManager IM)
        {
           downKeys.Clear();
           upKeys.Clear();
           foreach(int i in monitorKeys)
           {
               if(IM[i])
               {
                   if (!setKeys[i])
                   {
                       setKeys[i] = true;
                       downKeys.Add(i);
                   }
               }
               else
               {
                   if (setKeys[i])
                   {
                       setKeys[i] = false;
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
        protected bool keyDown(int key)
        {
            return downKeys.Contains(key);
        }

        /// <summary>
        /// determines if key was lifted up
        /// </summary>
        /// <param name="key">the key</param>
        /// <returns>true iff the key was just now lifted up.</returns>
        protected bool keyUp(int key)
        {
            return upKeys.Contains(key);
        }
    }
}
