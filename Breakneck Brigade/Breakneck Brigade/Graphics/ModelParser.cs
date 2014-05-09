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
    class ParserMaterialStreamProvider : IMaterialStreamProvider
    {
        MaterialStreamProvider msp;

        string Filename;
        public ParserMaterialStreamProvider(string filename)
        {
            msp = new MaterialStreamProvider();
            Filename = filename;
        }
        public Stream Open(string path)
        {
            return msp.Open(Filename);
        }
    }

    class ModelParser
    {
        private const string MODEL_DIRECTORY    = "res\\models\\";
        private const string MTL_DIRECTORY      = "res\\materials\\";

        ObjLoaderFactory olFactory = new ObjLoaderFactory();
        IObjLoader loader;
        public ModelParser()
        {
        }

        public Model ParseFile(string modelName)
        {
            Model result = new Model();
            using(var objFileStream = new FileStream(MODEL_DIRECTORY + modelName + ".obj", FileMode.Open))
            {
                IMaterialStreamProvider msp = new ParserMaterialStreamProvider(MTL_DIRECTORY + modelName + ".mtl");

                loader = olFactory.Create(msp);
         
                LoadResult parsedFile = loader.Load(objFileStream);
                IList<OL_Vertex> positions  = parsedFile.Vertices;
                IList<OL_Normal> normals    = parsedFile.Normals;
                IList<OL_Texture> texCords  = parsedFile.Textures;

                foreach(Group g in parsedFile.Groups)
                {
                    TexturedMesh mesh = new TexturedMesh();
                    int vertInd = 0;
                    foreach(Face f in g.Faces)
                    {
                        ///Parse and interpret everything as triangles
                        if(f.Count == 3)
                        { 
                            for(int ii = 0; ii < f.Count; ii++)
                            {
                                int posInd  = f[ii].VertexIndex - 1;
                                int normInd = f[ii].NormalIndex - 1;
                                int textInd = f[ii].TextureIndex - 1;

                                /// Data format
                                /// { 
                                ///     v0.x, v0.y, v0.z, 
                                ///     n0.x, n0.y, n0.z, 
                                ///     t0.x, t0.y,
                                ///     v1.x, v1.y, v1.z,
                                ///     n1.x, n1.y, n1.z, 
                                ///     t1.x, t1.y, 
                                ///     ...
                                /// }

                                mesh.VBO.Indices.Add(vertInd++);

                                mesh.VBO.Data.Add(positions[posInd].X);
                                mesh.VBO.Data.Add(positions[posInd].Y);
                                mesh.VBO.Data.Add(positions[posInd].Z);

                                mesh.VBO.Data.Add(normals[normInd].X);
                                mesh.VBO.Data.Add(normals[normInd].Y);
                                mesh.VBO.Data.Add(normals[normInd].Z);

                                mesh.VBO.Data.Add(texCords[textInd].X);
                                mesh.VBO.Data.Add(texCords[textInd].Y);
                            }
                        }
                        else if(f.Count == 4)
                        {
                            int posInd;
                            int normInd;
                            int textInd;
                            for (int ii = 0; ii < 3; ii++)
                            {
                                posInd  = f[ii].VertexIndex - 1;
                                normInd = f[ii].NormalIndex - 1;
                                textInd = f[ii].TextureIndex - 1;

                                /// Data format
                                /// { 
                                ///     v0.x, v0.y, v0.z, 
                                ///     n0.x, n0.y, n0.z, 
                                ///     t0.x, t0.y,
                                ///     v1.x, v1.y, v1.z,
                                ///     n1.x, n1.y, n1.z, 
                                ///     t1.x, t1.y, 
                                ///     ...
                                /// }

                                mesh.VBO.Indices.Add(vertInd++);

                                mesh.VBO.Data.Add(positions[posInd].X);
                                mesh.VBO.Data.Add(positions[posInd].Y);
                                mesh.VBO.Data.Add(positions[posInd].Z);

                                mesh.VBO.Data.Add(normals[normInd].X);
                                mesh.VBO.Data.Add(normals[normInd].Y);
                                mesh.VBO.Data.Add(normals[normInd].Z);

                                mesh.VBO.Data.Add(texCords[textInd].X);
                                mesh.VBO.Data.Add(texCords[textInd].Y);
                            }

                                posInd  = f[2].VertexIndex - 1;
                                normInd = f[2].NormalIndex - 1;
                                textInd = f[2].TextureIndex - 1;

                                mesh.VBO.Indices.Add(vertInd++);

                                mesh.VBO.Data.Add(positions[posInd].X);
                                mesh.VBO.Data.Add(positions[posInd].Y);
                                mesh.VBO.Data.Add(positions[posInd].Z);

                                mesh.VBO.Data.Add(normals[normInd].X);
                                mesh.VBO.Data.Add(normals[normInd].Y);
                                mesh.VBO.Data.Add(normals[normInd].Z);

                                mesh.VBO.Data.Add(texCords[textInd].X);
                                mesh.VBO.Data.Add(texCords[textInd].Y);

                                posInd  = f[3].VertexIndex - 1;
                                normInd = f[3].NormalIndex - 1;
                                textInd = f[3].TextureIndex - 1;

                                mesh.VBO.Indices.Add(vertInd++);

                                mesh.VBO.Data.Add(positions[posInd].X);
                                mesh.VBO.Data.Add(positions[posInd].Y);
                                mesh.VBO.Data.Add(positions[posInd].Z);

                                mesh.VBO.Data.Add(normals[normInd].X);
                                mesh.VBO.Data.Add(normals[normInd].Y);
                                mesh.VBO.Data.Add(normals[normInd].Z);

                                mesh.VBO.Data.Add(texCords[textInd].X);
                                mesh.VBO.Data.Add(texCords[textInd].Y);

                                posInd  = f[0].VertexIndex - 1;
                                normInd = f[0].NormalIndex - 1;
                                textInd = f[0].TextureIndex - 1;

                                mesh.VBO.Indices.Add(vertInd++);

                                mesh.VBO.Data.Add(positions[posInd].X);
                                mesh.VBO.Data.Add(positions[posInd].Y);
                                mesh.VBO.Data.Add(positions[posInd].Z);

                                mesh.VBO.Data.Add(normals[normInd].X);
                                mesh.VBO.Data.Add(normals[normInd].Y);
                                mesh.VBO.Data.Add(normals[normInd].Z);

                                mesh.VBO.Data.Add(texCords[textInd].X);
                                mesh.VBO.Data.Add(texCords[textInd].Y);
                        }
                    }

                    if(g.Material != null && g.Material.DiffuseTextureMap != null)
                    {
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
                    }
                    else
                    {
                        mesh.Texture = Renderer.DefaultTexture;
                    }
                    mesh.VBO.LoadData();
                    result.Meshes.Add(mesh);
                }
            }
            return result;
        }
    }
}
