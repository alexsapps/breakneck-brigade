using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SousChef;

namespace Breakneck_Brigade
{
    class ClientPlayer : ClientGameObject
    {
        Vector4 position;
        public float Orientation { get; set; }
        public float Incline { get; set; }

        public ClientPlayer(int id, Vector4 transform, ClientGame game) : base(id,transform,game)
        {
            position = new Vector4();
        }

        void Update(InputManager IM)
        {
            // Orientation & Incline update
            float rotx, roty;
            IM.GetRotAndClear(out rotx, out roty);

            Orientation = Orientation + roty > 360.0f ? Orientation + roty - 360.0f : Orientation + roty;
            Incline = Incline + rotx > 90.0f ? 90.0f : Incline + rotx < -90.0f ? -90.0f : Incline + rotx;

            // Position update
        }
    }
}
