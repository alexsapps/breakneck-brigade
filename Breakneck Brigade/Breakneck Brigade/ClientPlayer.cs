using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SousChef;

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
        }

        public void Update(InputManager IM)
        {
            // Orientation & Incline update
            float rotx, roty;
            IM.GetRotAndClear(out rotx, out roty);

            Orientation = Orientation + roty > 360.0f ? Orientation + roty - 360.0f : Orientation + roty;
            Incline = Incline + rotx > 90.0f ? 90.0f : Incline + rotx < -90.0f ? -90.0f : Incline + rotx;

            // Velocity update
            Velocity[0] = (Convert.ToBoolean(IM.GetKeys()[InputManager.GLFW_KEY_A]) || Convert.ToBoolean(IM.GetKeys()[InputManager.GLFW_KEY_LEFT])) ? -1 * MoveSpeed : (Convert.ToBoolean(IM.GetKeys()[InputManager.GLFW_KEY_D]) || Convert.ToBoolean(IM.GetKeys()[InputManager.GLFW_KEY_RIGHT])) ? 1 * MoveSpeed : 0.0f;
            Velocity[2] = (Convert.ToBoolean(IM.GetKeys()[InputManager.GLFW_KEY_S]) || Convert.ToBoolean(IM.GetKeys()[InputManager.GLFW_KEY_DOWN])) ? -1 * MoveSpeed : (Convert.ToBoolean(IM.GetKeys()[InputManager.GLFW_KEY_W]) || Convert.ToBoolean(IM.GetKeys()[InputManager.GLFW_KEY_UP])) ? MoveSpeed : 0.0f;

            Position[0] += Velocity[2] * (float)Math.Sin(Orientation / 180.0f * -1.0f * Math.PI) - Velocity[0] * (float)Math.Cos((Orientation / 180.0f * Math.PI));
            Position[2] += Velocity[2] * (float)Math.Cos(Orientation / 180.0f * -1.0f * Math.PI) - Velocity[0] * (float)Math.Sin((Orientation / 180.0f * Math.PI));

            if(Convert.ToBoolean(IM.GetKeys()[InputManager.GLFW_KEY_ESCAPE]))
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

            if(Convert.ToBoolean(IM.GetKeys()[InputManager.GLFW_MOUSE_BUTTON_LEFT]))
            {
                if (!IM.fpsMode)
                {
                    IM.EnableFPSMode();
                }
            }

            if(Convert.ToBoolean(IM.GetKeys()[InputManager.GLFW_KEY_SPACE]))
            {
                //Test code
                ClientEvent spawnEvent = new ClientEvent();
                spawnEvent.Type = ClientEventType.Test;
                NetworkEvents.Add(spawnEvent);
            }
        }
    }
}
