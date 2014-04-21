using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SousChef
{
    public class GlobalsConfigFile
    {
        string filename; //can be null if no file exists.  only defaults will be used.
        Dictionary<string, string> config = new Dictionary<string, string>();
        GlobalsConfigFolder context;

        public GlobalsConfigFile(GlobalsConfigFolder context, string filename)
        {
            this.context = context;
            this.filename = filename;
            Load();
        }

        public void Refresh()
        {
            Load();
        }

        protected void Load()
        {
            config.Clear();
            if (filename != null)
            {
                using (XmlReader reader = XmlReader.Create(filename, BBXml.getSettings()))
                {
                    if (reader.MoveToContent() == XmlNodeType.Element)
                    {
                        if (reader.Name == "settings")
                        {
                            while (reader.Read())
                            {
                                while (reader.NodeType == XmlNodeType.Element)
                                {
                                    config.Add(reader.Name, reader.ReadElementContentAsString());
                                }
                            }
                        }
                        else
                        {
                            throw new Exception();
                        }
                    }
                }
            }
        }

        public string GetSetting(string name)
        {
            return config[name];
        }
        public string GetSetting(string name, object defaultValue)
        {
            string value;
            if (!config.TryGetValue(name, out value))
                value = defaultValue.ToString();
            return value;
        }
    }
}