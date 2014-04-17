using SousChef;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Breakneck_Brigade
{
    public class RenderableGameObject 
    {
        public IGameObject Obj;
        public string Texture;

        public RenderableGameObject(IGameObject obj, string texture)
        {
            this.Obj = obj;
            this.Texture = texture;
        }

        public void Render()
        {
            //APEAR ON SCREEn WITH MAGIC
        }
    }
}
