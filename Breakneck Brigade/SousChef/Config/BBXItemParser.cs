using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using SousChef.MPNamespace;
using System.Diagnostics;

namespace SousChef
{
    public abstract class BBXItemParser<T>
    {
        protected GameObjectConfig config;
        public BBXItemParser(GameObjectConfig config) { this.config = config; }

        protected Dictionary<string, string> attributes;
        public string attrib(string name) { return attributes.get(name); }
        public static Dictionary<string, Vector4[]> scaleVector { get { return BB.modelParser.ScaleVector; } }

        bool _needsReset = false;
        public T Parse(XmlReader reader)
        {
            if (_needsReset)
                _reset();
            else
                _needsReset = true;

            reader.MoveToContent();
            attributes = BBXml.getAttributes(reader);
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

        //reads <item ...>...</item><item ...>...</item>... into a list of items
        public static List<T> ParseList(XmlReader reader, BBXItemParser<T> itemParser)
        {
            reader.MoveToContent(); //read to main element
            return ParseListItems(reader, itemParser);
        }
        public static List<T> ParseListItems(XmlReader reader, BBXItemParser<T> itemParser)
        {
            var items = new List<T>();
            while (reader.Read()) //and read over it to get to descendents.  then just read to get to the next element.
                if (reader.NodeType == XmlNodeType.Element)
                    items.Add(itemParser.Parse(reader.ReadSubtree()));
            return items;
        }

        public static GeometryInfo getGeomInfo(Dictionary<string, string> attributes, float[] defaultSize, float defaultMass, float defaultFriction, float defaultRollingFriction, float defaultRestitution, float defaultAngularDamping, float defaultOrientation, string model)
        {
            //Debug.Assert(attributes.ContainsKey("name")); //should not need name field
            if (model == null && attributes.ContainsKey("name"))
                model = attributes["name"];
            else if (model == null && attributes.ContainsKey("model"))
                model = attributes["model"];
            
            var shape = BB.ParseGeomShape(attributes.get("shape"), GeomShape.Box);
            float[] size = parseFloats(attributes.get("size"), defaultSize);
            float mass = parseFloat(attributes.get("mass"), defaultMass);
            float friction = parseFloat(attributes.get("friction"), defaultFriction);
            float rollingFriction = parseFloat(attributes.get("rollingFriction"), defaultRollingFriction);
            float restitution = parseFloat(attributes.get("restitution"), defaultRestitution);
            float angularDamping = parseFloat(attributes.get("angularDamping"), defaultAngularDamping);
            float orientation = parseFloat(attributes.get("orientation"), defaultOrientation);

            GeometryInfo info = new GeometryInfo()
            {
                Shape = shape,
                Mass = mass,
                Size = size,
                Model = model,
                Friction = friction,
                RollingFriction = rollingFriction,
                Restitution = restitution,
                AngularDamping = angularDamping,
                Orientation = orientation * MathConstants.DEG2RAD
            };
            
            return info;
        }

        public static int parseInt(string str, int @default)
        {
            return str == null ? @default : int.Parse(str);
        }

        public static float parseFloat(string str, float @default)
        {
            return str == null ? @default : float.Parse(str);
        }

        public static float[] parseFloats(string str, float[] @default)
        {
            if (str == null)
                return @default;
            else
                return parseFloats(str);
        }
        public static float[] parseFloats(string str)
        {
            string[] sidesstrarr = str.Split(',');
            float[] sides = new float[sidesstrarr.Length];
            for (int i = 0; i < sidesstrarr.Length; i++)
                sides[i] = float.Parse(sidesstrarr[i].Trim());
            return sides;
        }

        protected void parseName(out string name, out string friendlyName)
        {
            name = attributes["name"];
            friendlyName = null;
            if (!attributes.TryGetValue("friendlyName", out friendlyName))
                friendlyName = name;
        }
    }
}
