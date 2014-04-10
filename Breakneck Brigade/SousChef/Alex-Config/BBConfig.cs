using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SousChef
{
    public delegate int IdGetter();

    public class BBConfig
    {
        IdGetter idGetter;
        public string ConfigDir { get; private set; }

        public BBConfig(IdGetter idGetter, string configDir)
        {
            this.idGetter = idGetter;
            this.ConfigDir = configDir;
        }

        public delegate List<T> BBXListHandler<T>(XmlReader node);
        public delegate T BBXItemHandler<T>(XmlReader node);

        public List<Ingredient> GetIngredients()
        {
            return new IngredientsFileParser(this).LoadFile();
        }
        public class IngredientsFileParser : BBXFileParser<Ingredient>
        {
            public IngredientsFileParser(BBConfig config) : base(config) { }
            protected override string getRootNodeName() { return "ingredients"; }
            protected override string getListItemNodeName() { return "ingredient"; }
            public override BBXItemParser<Ingredient> getItemParser() { return new IngredientParser(config); }
        }
        public class IngredientParser : BBXItemParser<Ingredient>
        {
            public IngredientParser(BBConfig config) : base(config) { }
            protected override void handleSubtree(XmlReader reader)
            {
                // example uses

                if (reader.Name == "innerIngredients")
                {
                    List<Ingredient> subIngredients = ParseList<Ingredient>(reader.ReadSubtree(), new IngredientParser(config));
                }
                else if (reader.Name == "innerIngredient")
                {
                    Ingredient ingredient = BBConfig.ParseItem<Ingredient>(reader.ReadSubtree(), new IngredientParser(config));
                }
                else if (reader.Name == "innerStrList")
                {
                    Dictionary<string, string> attirbs = BBConfig.getAttributes(reader);
                    List<string> list = BBConfig.getItems(reader);
                }
            }
            protected override Ingredient returnItem()
            {
                return new Ingredient(config.idGetter(), attributes["name"]);
            }
        }

        public abstract class BBXItemParser<T> where T : class, new()
        {
            protected BBConfig config;
            public BBXItemParser(BBConfig config) { this.config = config; }

            protected Dictionary<string, string> attributes;
            
            public T ParseItem(XmlReader reader)
            {
                attributes = getAttributes(reader);
                while (reader.Read())
                {
                    handleSubtree(reader.ReadSubtree());
                }
                return returnItem();
            }
            protected abstract void handleSubtree(XmlReader reader);
            protected abstract T returnItem();
        }
                   
        public abstract class BBXFileParser<T> where T : class
        {
            protected BBConfig config;
            public BBXFileParser(BBConfig config) { this.config = config; }
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
                string file = Path.Combine(config.ConfigDir, filename);
                using (XmlReader reader = XmlReader.Create(file, getSettings()))
                {
                    return parseFile(reader);
                }
            }
            static XmlReaderSettings getSettings()
            {
                return new XmlReaderSettings()
                {
                    IgnoreComments = true,
                    IgnoreWhitespace = true,
                };
            }
            public List<T> parseFile(XmlReader reader)
            {
                if (reader.MoveToContent() == XmlNodeType.Element 
                    && reader.Name == getRootNodeName())
                {
                    return ParseList(reader.ReadSubtree(), getItemParser());
                }
                else
                {
                    throw new Exception("bad xml file (1)");
                }
            }
        }

        //reads <item ...>...</item><item ...>...</item>... into a list of items
        public static List<T> ParseList<T>(XmlReader reader, BBXItemParser<T> itemParser) where T : class
        {
            var items = new List<T>();
            while (reader.Read())
                items.Add(itemParser.ParseItem(reader.ReadSubtree()));
            return items;
        }
        public static T ParseItem<T>(XmlReader reader, BBXItemParser<T> itemParser) where T : class
        {
            return itemParser.ParseItem(reader);
        }

        //reads <Item>item 1</Item><Item>item 2</Item> ... into a List
        public static List<string> getItems(XmlReader reader)
        {
            return getItems(reader, "Item");
        }
        public static List<string> getItems(XmlReader reader, string tagName)
        {
            List<string> items = new List<string>();
            while (reader.MoveToContent() == XmlNodeType.Element)
            {
                if (reader.Name != tagName)
                    throw new Exception();
                items.Add(reader.ReadContentAsString());
            }
            return items;
        }

        //reads <... att1="val1" att2="val2" /> ... into a dictionary
        public static Dictionary<string, string> getAttributes(XmlReader reader)
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

    }
}