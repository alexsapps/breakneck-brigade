using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SousChef
{
    /// <summary>
    /// Top level gameobject interface. Specifies data that all objects need, client and server.
    /// </summary>
    public interface IGameObject
    {
        int Id { get; set; }
        Matrix4 Transform { get; set; } // position + the rotation of the object
    }
}
