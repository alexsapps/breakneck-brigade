using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SousChef
{
    public class BBXIngredientsFileParser : BBXFileParser<IngredientType>
    {
        public BBXIngredientsFileParser(GameObjectConfig config) : base(config) { }
        protected override string getRootNodeName() { return "ingredients"; }
        protected override string getListItemNodeName() { return "ingredient"; }
        public override BBXItemParser<IngredientType> getItemParser() { return new IngredientParser(config); }
    }
    public class IngredientParser : BBXItemParser<IngredientType>
    {
        public IngredientParser(GameObjectConfig config) : base(config) { }
        protected override void handleSubtree(XmlReader reader)
        {
            // example uses
            reader.MoveToContent();

            if (reader.Name == "innerIngredients")
            {
                List<IngredientType> subIngredients = ParseList(reader.ReadSubtree(), new IngredientParser(config));
                Console.WriteLine(subIngredients);
            }
            else if (reader.Name == "innerIngredient")
            {
                IngredientType ingredient = parseSubItem<IngredientType>(reader.ReadSubtree(), new IngredientParser(config));
                Console.WriteLine(ingredient);
            }
            else if (reader.Name == "innerStrList")
            {
                Dictionary<string, string> attirbs = getAttributes(reader);
                List<string> list = parseStringList(reader);
                Console.WriteLine(list);
                list = null;
            }
        }
        protected override IngredientType returnItem()
        {
            Console.WriteLine("parsed! " + attributes["name"]);
            return new IngredientType(attributes["name"], null);
        }
    }
}
