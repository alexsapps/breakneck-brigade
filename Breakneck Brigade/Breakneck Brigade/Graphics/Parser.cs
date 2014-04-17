using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ObjLoader.Loader.Loaders;
using System.IO;
using OL_Vertex     = ObjLoader.Loader.Data.VertexData.Vertex;
using OL_Normal     = ObjLoader.Loader.Data.VertexData.Normal;
using OL_Texture    = ObjLoader.Loader.Data.VertexData.Texture;
using ObjLoader.Loader.Data.Elements;
using SousChef;
using Tao.OpenGl;

namespace Breakneck_Brigade.Graphics
{
    class Parser
    {
        IObjLoader loader;
        public Parser()
        {
            ObjLoaderFactory olFactory = new ObjLoaderFactory();
            loader = olFactory.Create();
        }

        public Model ParseFile(string filename)
        {
            Model result = new Model();
            FileStream fileStream = new FileStream(filename, FileMode.Open);
            LoadResult parsedFile = loader.Load(fileStream);
            IList<OL_Vertex> positions  = parsedFile.Vertices;
            IList<OL_Normal> normals    = parsedFile.Normals;
            IList<OL_Texture> textures  = parsedFile.Textures;

            foreach(Group g in parsedFile.Groups)  //meshes
            {
                TexturedMesh mesh = new TexturedMesh();
                int ii = 0;
                foreach(Face f in g.Faces) //polys
                {
                    TexturedPolygon poly = new TexturedPolygon();
                    for(ii = 0; ii < f.Count; ii++)
                    {
                        int posInd  = f[ii].VertexIndex - 1 ;
                        int normInd = f[ii].NormalIndex - 1;
                        int textInd = f[ii].TextureIndex - 1;
                        Vector4 position = new Vector4
                                (
                                    positions[posInd].X,
                                    positions[posInd].Y,
                                    positions[posInd].Z
                                );
                        Vector4 normal;
                        if(normals.Count > 0)
                        { 
                             normal = new Vector4
                                (
                                    normals[normInd].X,
                                    normals[normInd].Y,
                                    normals[normInd].Z
                                );
                        }
                        else
                        {
                            normal = null;
                        }
                        Vector4 tc;
                        if(textures.Count > 0)
                        { 
                             tc = new Vector4
                                (
                                    textures[textInd].X,
                                    textures[textInd].Y,
                                    0
                                );  
                        }
                        else
                        {
                            tc = null;
                        }

                        Vertex v = new Vertex(position, normal, tc);
                        poly.Vertexes.Add(v);
                    }
                    mesh.Polygons.Add(poly);
                }
                //Set up polygon rendering mode for this mesh
                switch (ii+1)
                {
                    case 3:
                        mesh.GlDrawMode = Gl.GL_TRIANGLES;
                        break;
                    case 4:
                        mesh.GlDrawMode = Gl.GL_QUADS;
                        break;
                    default:
                        mesh.GlDrawMode = Gl.GL_POLYGON;
                        break;
                }

                //Texturing
                Texture diffuseTexture;
                if(Renderer.Textures.ContainsKey(g.Material.DiffuseTextureMap))
                {
                    diffuseTexture = Renderer.Textures[g.Material.DiffuseTextureMap];
                }
                else
                {
                    diffuseTexture = new Texture(g.Material.DiffuseTextureMap);
                    Renderer.Textures[g.Material.DiffuseTextureMap] = diffuseTexture;
                }
                mesh.Texture = diffuseTexture;

                result.Meshes.Add(mesh);
            }

            fileStream.Close();
            return result;
        }
    }
}
