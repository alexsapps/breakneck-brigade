using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SousChef
{
    public class BBXCookersFileParser : BBXFileParser<CookerType>
    {
        public BBXCookersFileParser(GameObjectConfig config) : base(config) { }
        protected override string getRootNodeName() { return "cookers"; }
        protected override string getListItemNodeName() { return "cooker"; }
        public override BBXItemParser<CookerType> getItemParser() { return new CookerParser(config); }
    }
    public class CookerParser : BBXItemParser<CookerType>
    {
        public CookerParser(GameObjectConfig config) : base(config) { }

        List<Recipe> recipes = new List<Recipe>();
        
        public override void ParseContents(XmlReader reader)
        {
            var recipes = this.parseStringList(reader, "recipe", false);
            foreach (var i in recipes)
            {
                this.recipes.Add(config.CurrentSalad.Recipies[i]);
            }
        }

        protected override void handleSubtree(XmlReader reader)
        {
            throw new NotImplementedException();
        }

        protected override CookerType returnItem()
        {
            string name = attributes["name"];
            string model;

            if (!attributes.TryGetValue("model", out model))
                model = name;

            var geomInfo = getGeomInfo(attributes);

            return new CookerType(name, geomInfo, recipes, model);
        }

        protected override void reset()
        {
            recipes = new List<Recipe>();
        }
    }
}
