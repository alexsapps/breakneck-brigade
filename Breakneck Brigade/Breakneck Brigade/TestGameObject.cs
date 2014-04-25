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
        static int testId = -133709999;
        public TestClientGameObject(Model model) : base(testId++, new Vector4(0,0,0), null)
        {
            Model = model;
        }

        public override string ModelName
        {
            get { return "should be ignored"; }
        }
    }
}
