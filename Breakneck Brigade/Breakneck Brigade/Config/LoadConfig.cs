using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Xml;

namespace Breakneck_Brigade
{
    class LoadConfig
    {
        public Hashtable configFileHash;
        public LoadConfig(string configFile)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(configFile);

            //XmlNode root = doc.DocumentElement.SelectSingleNode("/files");
            foreach(XmlNode node in doc.DocumentElement.ChildNodes)
            {
                string name = node.SelectSingleNode("filename").InnerText;
                string id = node.SelectSingleNode("id").InnerText;
                System.Diagnostics.Debug.Write("anme is " + name);
                System.Diagnostics.Debug.Write("anme is " + id);

                configFileHash.Add(id, name);
            }
        }

        public Hashtable LoadAll()
        {
            //should call all the individual load methods, store the returns and return the hash(or map, I haven't looked into it)
            return configFileHash;

        }

        public void LoadIngredients()
        {
            //load the ingredient xml. Use the id to find the file in the configFileHash
        }
    }
}
