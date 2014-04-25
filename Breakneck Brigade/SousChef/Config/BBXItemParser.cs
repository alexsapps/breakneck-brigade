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
            HandleAttributes();
            ParseContents(reader);

            return returnItem();
        }

        //this is for processing after attributes are available but before parsing contents.  useful if processing contents depends on result of processing attributes.
        protected virtual void HandleAttributes() { }

        //process any contents inside an xml element.  by default, handle each element individually
        public virtual void ParseContents(XmlReader reader)
        {
            while (reader.Read())
                if (reader.NodeType == XmlNodeType.Element)
                    handleSubtree(reader.ReadSubtree());
        }

        //process a single element inside this element
        protected virtual void handleSubtree(XmlReader reader) { throw new NotImplementedException("not expecting content inside this tag."); }
        
        //after all processing has completed, returns the result of the parsing
        protected abstract T returnItem();

        private void _reset()
        {
            attributes = null;
            reset();
        }

        //this function is guaranteed to be called between re-uses of this parser.  reset state here to cleanly begin parsing the next object.
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

        protected GeometryInfo getGeomInfo(Dictionary<string, string> attributes)
        {
            var shape = BB.ParseGeomShape(attributes["shape"]);
            string sidesstr = attributes["sides"];
            
            GeometryInfo info = new GeometryInfo()
            {
                Shape = shape,
                Mass = float.Parse(attributes["mass"]),
                Sides = getFloats(sidesstr)
            };
            
            return info;
        }

        protected float[] getFloats(string str)
        {
            string[] sidesstrarr = str.Split(',');
            float[] sides = new float[sidesstrarr.Length];
            for (int i = 0; i < sidesstrarr.Length; i++)
                sides[i] = float.Parse(sidesstrarr[i].Trim());
            return sides;
        }
    }
}
