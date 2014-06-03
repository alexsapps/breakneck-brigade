using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SousChef
{
    public class BBXRecipesFileParser : BBXFileParser<Recipe>
    {
        public BBXRecipesFileParser(GameObjectConfig config) : base(config) { }
        protected override string getRootNodeName() { return "recipes"; }
        protected override string getListItemNodeName() { return "recipe"; }
        public override BBXItemParser<Recipe> getItemParser() { return new RecipeParser(config); }
    }
    public class RecipeParser : BBXItemParser<Recipe>
    {
        public RecipeParser(GameObjectConfig config) : base(config) { }

        List<RecipeIngredient> ingredients = new List<RecipeIngredient>();

        public override void ParseContents(XmlReader reader)
        {
            var ingredients = this.parseIngredient(reader, "ingredient");
            foreach (var i in ingredients)
            {
                this.ingredients.Add(i);
            }
        }

        protected override void handleSubtree(XmlReader reader) 
        {
            throw new NotImplementedException();
        }

        protected override Recipe returnItem()
        {
            string finalProductName = attributes["product"];
            IngredientType finalProduct = config.CurrentSalad.Ingredients[finalProductName];

            string name;
            string friendlyName = null;
            attributes.TryGetValue("friendlyName", out friendlyName);

            if (attributes.TryGetValue("name", out name))
            {
                friendlyName = friendlyName ?? (name);
            }
            else
            {
                name = finalProductName;
                friendlyName = friendlyName ?? finalProduct.FriendlyName;
            }
            
            return new Recipe(name, friendlyName, ingredients, finalProduct);
        }

        protected override void reset()
        {
            ingredients = new List<RecipeIngredient>();
        }

        protected List<RecipeIngredient> parseIngredient(XmlReader reader, string tagName)
        {

            List<RecipeIngredient> items = new List<RecipeIngredient>();
            while (reader.Read())
            {
                while (reader.NodeType == XmlNodeType.Element)
                {
                    if (reader.Name != tagName)
                        throw new Exception();
                    
                    int optional = parseInt(reader.GetAttribute("optional"), -1);
                    int count = parseInt(reader.GetAttribute("count"), -1);

                    if (count < 0)
                        if (optional >= 0)
                            count = 0; //if optional specified, but not count, then it's an optional ingredient, 0 required
                        else
                            count = 1; //if neither count nor optional specified, it means it's required and just 1 is needed

                    if (optional < 0)
                        optional = 0; //if optional not specified, this means 0 optional
                    
                    IngredientType ing = config.CurrentSalad.Ingredients[reader.ReadElementContentAsString()];
                    items.Add(new RecipeIngredient(ing, count, optional));
                }
            }
            return items;
        }
    }
}
