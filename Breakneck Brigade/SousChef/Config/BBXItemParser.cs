using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace SousChef
{
    public abstract class BBXItemParser<T>
    {
        protected GameObjectConfig config;
        public BBXItemParser(GameObjectConfig config) { this.config = config; }

        protected Dictionary<string, string> attributes;

        bool _needsReset = false;
        public T Parse(XmlReader reader)
        {
            if (_needsReset)
                _reset();
            else
                _needsReset = true;

            reader.MoveToContent();
            attributes = getAttributes(reader);
            ParseContents(reader);

            return returnItem();
        }

        public virtual void ParseContents(XmlReader reader)
        {
            while (reader.Read())
                if (reader.NodeType == XmlNodeType.Element)
                    handleSubtree(reader.ReadSubtree());
        }

        protected abstract void handleSubtree(XmlReader reader);
        protected abstract T returnItem();

        private void _reset()
        {
            attributes = null;
            reset();
        }
        protected abstract void reset();

        protected List<T> parseList(XmlReader reader, BBXItemParser<T> itemParser)
        {
            return ParseList(reader, itemParser);
        }

        protected S parseSubItem<S>(XmlReader reader, BBXItemParser<S> itemParser) where S : class
        {
            return itemParser.Parse(reader);
        }

        //reads <Item>item 1</Item><Item>item 2</Item> ... into a List
        protected List<string> parseStringList(XmlReader reader)
        {
            return parseStringList(reader, "item");
        }
        protected List<string> parseStringList(XmlReader reader, string tagName)
        {
            return parseStringList(reader, tagName, true);
        }
        protected List<string> parseStringList(XmlReader reader, string tagName, bool readParent)
        {
            List<string> items = new List<string>();
            if (readParent)
            {
                reader.MoveToContent();
            }
            while (reader.Read())
            {
                while (reader.NodeType == XmlNodeType.Element)
                {
                    if (reader.Name != tagName)
                        throw new Exception();
                    items.Add(reader.ReadElementContentAsString());
                }
            }
            return items;
        }

        //reads <... att1="val1" att2="val2" /> ... into a dictionary
        protected Dictionary<string, string> getAttributes(XmlReader reader)
        {
            var attributes = new Dictionary<string, string>();
            if (reader.MoveToFirstAttribute())
            {
                do
                {
                    attributes.Add(reader.Name, reader.Value); //handleAttribute(reader.Name, reader.Value); //protected abstract void handleAttribute(string name, string value);
                } while (reader.MoveToNextAttribute());
            }
            return attributes;
        }

        //reads <item ...>...</item><item ...>...</item>... into a list of items
        public static List<T> ParseList(XmlReader reader, BBXItemParser<T> itemParser)
        {
            var items = new List<T>();
            reader.MoveToContent(); //read to main element
            while (reader.Read()) //and read over it to get to descendents.  then just read to get to the next element.
                if (reader.NodeType == XmlNodeType.Element)
                    items.Add(itemParser.Parse(reader.ReadSubtree()));
            return items;
        }

        public static GeometryInfo getGeomInfo(Dictionary<string, string> attributes)
        {
            var shape = BB.ParseGeomShape(attributes["shape"]);
            string sidesstr = attributes["sides"];
            string[] sidesstrarr = sidesstr.Split(',');
            float[] sides = new float[sidesstrarr.Length];
            for (int i = 0; i < sidesstrarr.Length; i++)
                sides[i] = float.Parse(sidesstrarr[i].Trim());

            GeometryInfo info = new GeometryInfo()
            {
                Shape = shape,
                Mass = float.Parse(attributes["mass"]),
                Sides = sides
            };
            
            return info;

        }
    }
}
