using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using SousChef;
using Tao.OpenGl;

namespace Breakneck_Brigade.Graphics
{
    
    /// <summary>
    /// Holds data about a VBO
    /// </summary>
    class VBO
    {
        public int GlVBOIndices;
        public int GlVBOData;

        /// <summary>
        /// The draw mode that should be used to render this mesh
        /// </summary>
        public int GlDrawMode;
        /// <summary>
        /// A list of indices that represents the model. Depending on the drawing mode, 
        /// </summary>
        public List<Single> Indices;
        /// <summary>
        /// Data
        /// { 
        ///     v0.x, v0.y, v0.z, 
        ///     n0.x, n0.y, n0.z, 
        ///     t0.x, t0.y,
        ///     v1.x, v1.y, v1.z,
        ///     n1.x, n1.y, n1.z, 
        ///     t1.x, t1.y, 
        ///     ...
        /// }
        /// </summary>
        public List<Single> Data;

        private const  int STRIDE       = 8*sizeof(float);
        private IntPtr VERTEX_OFFSET    = IntPtr.Zero;
        private IntPtr NORMAL_OFFSET    = IntPtr.Zero + 3*sizeof(float);
        private IntPtr TEXTURE_OFFSET   = IntPtr.Zero + 6*sizeof(float);

        /// <summary>
        /// Initializes an empty VBO
        /// </summary>
        public VBO()
        {
            GlDrawMode          = Gl.GL_TRIANGLES;

            Indices = new List<Single>();
            Data    = new List<Single>();

            Gl.glGenBuffersARB(1, out GlVBOIndices);
            Gl.glGenBuffersARB(1, out GlVBOData);
        }

        /// <summary>
        /// Loads the data from the lists into VBO memory. Only call after
        /// Indices, Vertices, Normals, and TextureCoordinates are populated
        /// </summary>
        public void LoadData()
        {
            loadBuffer(Gl.GL_ELEMENT_ARRAY_BUFFER_ARB,  GlVBOIndices,   Indices);
            loadBuffer(Gl.GL_ARRAY_BUFFER_ARB,          GlVBOData,      Data);
        }

        /// <summary>
        /// Renders this object represented by these VBOs
        /// </summary>
        public void Render()
        {
            Gl.glBindBufferARB(Gl.GL_ELEMENT_ARRAY_BUFFER_ARB, GlVBOIndices);

            Gl.glBindBufferARB(Gl.GL_ARRAY_BUFFER_ARB, GlVBOData);
            Gl.glVertexPointer(3, Gl.GL_FLOAT, STRIDE, VERTEX_OFFSET);
            Gl.glNormalPointer(Gl.GL_FLOAT, STRIDE, NORMAL_OFFSET);
            Gl.glTexCoordPointer(2, Gl.GL_FLOAT, STRIDE, TEXTURE_OFFSET);

            Gl.glDrawArrays(Gl.GL_TRIANGLES, 0, Indices.Count);
        }

        private void loadBuffer(int glBufferType, int glBufferLocation, List<Single> list)
        {
            IntPtr size = IntPtr.Zero + list.Count * sizeof(Single);

            Gl.glBindBufferARB(glBufferType, glBufferLocation);
            IntPtr listPtr = Marshal.AllocHGlobal(size);
            Marshal.Copy(list.ToArray(), 0, listPtr, list.Count);
            Gl.glBufferDataARB(glBufferType, size, listPtr, Gl.GL_STATIC_DRAW_ARB);
            Gl.glBindBufferARB(glBufferType, 0);
        }
    }
}
