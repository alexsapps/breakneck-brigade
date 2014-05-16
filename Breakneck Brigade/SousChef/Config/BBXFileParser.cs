using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SousChef
{
    public abstract class BBXFileParser<T> where T : class
    {
        protected GameObjectConfig config;
        public BBXFileParser(GameObjectConfig config) { this.config = config; }
        public abstract BBXItemParser<T> getItemParser();
        public string GetFileName() { return getRootNodeName() + ".xml"; }
        protected abstract string getRootNodeName();
        protected abstract string getListItemNodeName();

        protected Dictionary<string, string> attributes;
        public string attrib(string name) { return BBXItemParser<T>.attrib(attributes, name); }

        public List<T> LoadFile()
        {
            string filename;
            return LoadFile(out filename);
        }
        public List<T> LoadFile(out string filename)
        {
            return LoadFile(GetFileName(), out filename);
        }
        public List<T> LoadFile(string filename, out string fullname)
        {
            fullname = BBXml.CombinePath(config.ConfigDir, filename);
            using (XmlReader reader = XmlReader.Create(fullname, BBXml.getSettings()))
            {
                return parseFile(reader);
            }
        }

        public virtual List<T> parseFile(XmlReader reader)
        {
            if (reader.MoveToContent() == XmlNodeType.Element
                && reader.Name == getRootNodeName())
            {
                attributes = BBXItemParser<T>.getAttributes(reader);
                handleAttributes();
                return parseRoot(reader);
            }
            else
            {
                throw new Exception("bad xml file (1)");
            }
        }

        protected virtual void handleAttributes() { }

        protected virtual List<T> parseRoot(XmlReader reader)
        {
            return BBXItemParser<T>.ParseListItems(reader, getItemParser());
        }
    }
}
