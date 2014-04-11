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
        public List<T> LoadFile()
        {
            return LoadFile(GetFileName());
        }
        public List<T> LoadFile(string filename)
        {
            string file = BBXml.CombinePath(config.ConfigDir, filename);
            using (XmlReader reader = XmlReader.Create(file, BBXml.getSettings()))
            {
                return parseFile(reader);
            }
        }
        public List<T> parseFile(XmlReader reader)
        {
            if (reader.MoveToContent() == XmlNodeType.Element
                && reader.Name == getRootNodeName())
            {
                return BBXItemParser<T>.ParseList(reader.ReadSubtree(), getItemParser());
            }
            else
            {
                throw new Exception("bad xml file (1)");
            }
        }
    }
}
