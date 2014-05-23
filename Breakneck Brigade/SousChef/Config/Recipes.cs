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
            string name;
            if (!attributes.TryGetValue("name", out name))
                name = finalProductName;
            
            IngredientType finalProduct = config.CurrentSalad.Ingredients[finalProductName];

            return new Recipe(name, ingredients, finalProduct);
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
                    string optional = reader.GetAttribute("optional");
                    IngredientType ing = config.CurrentSalad.Ingredients[reader.ReadElementContentAsString()];
                    if(optional == "yes")
                        items.Add(new RecipeIngredient(ing, true));
                    else
                        items.Add(new RecipeIngredient(ing, false));

                }
            }
            return items;
        }
    }
}
