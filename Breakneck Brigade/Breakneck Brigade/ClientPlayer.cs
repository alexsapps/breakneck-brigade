﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SousChef;

namespace Breakneck_Brigade
{
    public class ClientPlayer
    {
        public Vector4 Position;
        public float Orientation { get; set; }
        public float Incline { get; set; }
        public Vector4 Velocity;
        public float MoveSpeed = 0.1f;

        public ClientPlayer()
        {
            Position = new Vector4();
        }

        public void Update(InputManager IM)
        {
            // Orientation & Incline update
            float rotx, roty;
            IM.GetRotAndClear(out rotx, out roty);

            Orientation = Orientation + roty > 360.0f ? Orientation + roty - 360.0f : Orientation + roty;
            Incline = Incline + rotx > 90.0f ? 90.0f : Incline + rotx < -90.0f ? -90.0f : Incline + rotx;

            // Velocity update
            Velocity[0] = Convert.ToBoolean(IM.GetKeys()[InputManager.GLFW_KEY_A]) ? -1 * MoveSpeed : Convert.ToBoolean(IM.GetKeys()[InputManager.GLFW_KEY_D]) ? MoveSpeed : 0.0f;
            Velocity[2] = Convert.ToBoolean(IM.GetKeys()[InputManager.GLFW_KEY_S]) ? -1 * MoveSpeed : Convert.ToBoolean(IM.GetKeys()[InputManager.GLFW_KEY_W]) ? MoveSpeed : 0.0f;         
        }
    }
}