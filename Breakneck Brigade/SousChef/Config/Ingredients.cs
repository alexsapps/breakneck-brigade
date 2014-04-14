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
            throw new Exception("content not allowed in ingredient tag");
        }
        protected override IngredientType returnItem()
        {
            string name = attributes["name"];
            int points = int.Parse(attributes["points"]);
            string model;

            if (!attributes.TryGetValue("model", out model))
                model = name;

            return new IngredientType(name, points, model);
        }

        protected override void reset() { }
    }
}
