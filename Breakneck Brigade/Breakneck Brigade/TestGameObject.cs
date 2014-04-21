using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SousChef;
using Breakneck_Brigade.Graphics;

namespace Breakneck_Brigade
{
    /// <summary>
    /// A test class for rendering models in Renderer
    /// </summary>
    class TestClientGameObject : ClientGameObject
    {
        public TestClientGameObject(Model model) : base(-1, new Vector4(0,0,0), null)
        {
            Model = model;
        }
    }
}
