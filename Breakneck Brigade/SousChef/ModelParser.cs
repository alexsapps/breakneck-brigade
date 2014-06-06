using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ObjLoader.Loader.Loaders;
using System.IO;
using System.Xml;
using OL_Vertex = ObjLoader.Loader.Data.VertexData.Vertex;
using OL_Normal = ObjLoader.Loader.Data.VertexData.Normal;
using OL_Texture = ObjLoader.Loader.Data.VertexData.Texture;
using ObjLoader.Loader.Data.Elements;
using System.Diagnostics;
//using Tao.OpenGl;

namespace SousChef.MPNamespace
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

    public class ModelParser
    {
        public Dictionary<string, Vector4[]> ScaleVector;
        
        private const string MODEL_DIRECTORY = "res\\models\\";
        private const string MTL_DIRECTORY = "res\\materials\\";
        private const string RESOURCES_XML_PATH = "res\\resources.xml";
        

        ObjLoaderFactory olFactory = new ObjLoaderFactory();
        IObjLoader loader;
        public ModelParser()
        {
            ScaleVector = new Dictionary<string, Vector4[]>();
            // parse at creation time 
            using (FileStream resFile = new FileStream(RESOURCES_XML_PATH, FileMode.Open))
            {
                int numberOfModels = 0;
                using (XmlReader firstPass = XmlReader.Create(resFile))
                {
                    firstPass.ReadToFollowing("resources");
                    firstPass.ReadToFollowing("models");
                    if (firstPass.ReadToDescendant("model"))
                    {
                        numberOfModels++;
                    }
                    while (firstPass.ReadToNextSibling("model"))
                    {
                        numberOfModels++;
                    }
                }
                resFile.Seek(0, SeekOrigin.Begin);
                using (XmlReader reader = XmlReader.Create(resFile))
                {
                    reader.ReadToFollowing("resources");
                    reader.ReadToFollowing("models");
                    reader.ReadToDescendant("model");
                    for (int ii = 0; ii < numberOfModels; ii++)
                    {
                        XmlReader modelSubtree = reader.ReadSubtree();

                        modelSubtree.ReadToDescendant("filename");
                        string filename = modelSubtree.ReadElementContentAsString();

                        // scan the rest of the xml file. TODO: Scan better.
                        modelSubtree.ReadToNextSibling("posX");
                        float posX = modelSubtree.ReadElementContentAsFloat();

                        modelSubtree.ReadToNextSibling("posY");
                        float posY = modelSubtree.ReadElementContentAsFloat();

                        modelSubtree.ReadToNextSibling("posZ");
                        float posZ = modelSubtree.ReadElementContentAsFloat();

                        modelSubtree.ReadToNextSibling("scaleX");
                        float scaleX = modelSubtree.ReadElementContentAsFloat();

                        modelSubtree.ReadToNextSibling("scaleY");
                        float scaleY = modelSubtree.ReadElementContentAsFloat();

                        modelSubtree.ReadToNextSibling("scaleZ");
                        float scaleZ = modelSubtree.ReadElementContentAsFloat();

                        modelSubtree.ReadToNextSibling("rotX");
                        float rotX = modelSubtree.ReadElementContentAsFloat();

                        modelSubtree.ReadToNextSibling("rotY");
                        float rotY = modelSubtree.ReadElementContentAsFloat();

                        modelSubtree.ReadToNextSibling("rotZ");
                        float rotZ = modelSubtree.ReadElementContentAsFloat();

                        Vector4 pos   = new Vector4(posX, posY, posZ);
                        Vector4 scale = new Vector4(scaleX, scaleY, scaleZ);
                        Vector4 rot   = new Vector4(rotX, rotY, rotZ);

                        Debug.Assert(filename != null);
                        ScaleVector.Add(filename, this.ParseFile(filename, pos, scale, rot));
                        if (ii != numberOfModels - 1)
                            reader.ReadEndElement();
                        reader.ReadToNextSibling("model");
                    }
                }
            }

        }

        public Vector4[] ParseFile(string modelName, Vector4 pos, Vector4 scale, Vector4 rot)
        {
            using (var objFileStream = new FileStream(MODEL_DIRECTORY + modelName + ".obj", FileMode.Open))
            {
                IMaterialStreamProvider msp = new ParserMaterialStreamProvider(MTL_DIRECTORY + modelName + ".mtl");

                loader = olFactory.Create(msp);

                LoadResult parsedFile = loader.Load(objFileStream);
                Vector4 maxVerts = new Vector4();
                Vector4 minVerts = new Vector4();

                maxVerts.X = parsedFile.Vertices[0].X;
                maxVerts.Y = parsedFile.Vertices[0].Y;
                maxVerts.Z = parsedFile.Vertices[0].Z;

                minVerts.X = parsedFile.Vertices[0].X;
                minVerts.Y = parsedFile.Vertices[0].Y;
                minVerts.Z = parsedFile.Vertices[0].Z;

                foreach(var vert in parsedFile.Vertices)
                {
                    if (vert.X < minVerts.X) 
                        minVerts.X = vert.X;
                    if (vert.X > maxVerts.X)
                        maxVerts.X = vert.X;

                    if (vert.Y < minVerts.Y)
                        minVerts.Y = vert.Y;
                    if (vert.Y > maxVerts.Y)
                        maxVerts.Y = vert.Y;

                    if (vert.Z < minVerts.Z)
                        minVerts.Z = vert.Z;
                    if (vert.Z > maxVerts.Z)
                        maxVerts.Z = vert.Z;
                }
                Vector4 width = maxVerts - minVerts;
                width.Scale(.5f);
                Vector4 trans = minVerts + width;
                trans.Negate();


                Matrix4 transMat = new Matrix4();
                transMat *= Matrix4.MakeRotateYDeg(rot.Y);
                transMat *= Matrix4.MakeRotateZDeg(rot.Z);
                transMat *= Matrix4.MakeRotateXDeg(rot.X);
                transMat *= Matrix4.MakeScalingMat(scale.X, scale.Y, scale.Z);
                transMat *= Matrix4.MakeTranslationMat(trans.X, trans.Y, trans.Z);

                return new Vector4[] { transMat * minVerts, transMat * maxVerts };
            }
        }
    }
}
