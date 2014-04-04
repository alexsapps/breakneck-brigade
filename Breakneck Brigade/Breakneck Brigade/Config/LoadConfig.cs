using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Breakneck_Brigade
{
    class LoadConfig
    {
        public Dictionary<string, string> configFiles;
        public LoadConfig(string configFile)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(configFile);

            //XmlNode root = doc.DocumentElement.SelectSingleNode("/files");
            foreach(XmlNode node in doc.DocumentElement.ChildNodes)
            {
                string name = node.SelectSingleNode("filename").InnerText;
                string id = node.SelectSingleNode("id").InnerText;
                configFiles.Add(id, name);
            }
        }

        public Dictionary<string, string> LoadAll()
        {
            return configFiles;
        }

        public Dictionary<string, string> LoadIngredientsFile()
        {
            return configFiles;
        }

        public Dictionary<string, string> LoadCookerFile()
        {
            return configFiles;
        }

        public Dictionary<string, string> LoadRecipiesFile()
        {
            return configFiles;
        }
    }
}
