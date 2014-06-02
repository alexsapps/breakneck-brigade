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
        public override BBXItemParser<CookerType> getItemParser() { return new CookerParser(config, this); }

        public float[] defaultSize;
        public float defaultMass;
        public float defaultFriction;
        public float defaultRollingFriction;
        public float defaultRestitution;
        public float defaultAngularDamping;

        protected override void handleAttributes()
        {
            base.handleAttributes();

            defaultSize = BBXItemParser<IngredientType>.parseFloats(attributes["defaultSize"]);
            defaultMass = float.Parse(attrib("defaultMass"));
            defaultFriction = float.Parse(attrib("defaultFriction"));
            defaultRollingFriction = float.Parse(attrib("defaultFriction"));
            defaultRestitution = float.Parse(attrib("defaultRestitution"));
            defaultAngularDamping = float.Parse(attrib("defaultAngularDamping"));
        }
    }
    public class CookerParser : BBXItemParser<CookerType>
    {
        BBXCookersFileParser fileParser;
        public CookerParser(GameObjectConfig config, BBXCookersFileParser fileParser) : base(config) 
        { 
            this.fileParser = fileParser; 
        }

        List<Recipe> recipes = new List<Recipe>();

        public override void ParseContents(XmlReader reader)
        {
            var recipes = this.parseStringList(reader, "recipe", false);
            foreach (var i in recipes)
            {
                this.recipes.Add(config.CurrentSalad.Recipes[i]);
            }
        }

        protected override void handleSubtree(XmlReader reader)
        {
            throw new NotImplementedException();
        }

        static readonly float[] defaultDefaultSides = new float[] { 6f, 6f, 6f };
        protected override CookerType returnItem()
        {
            string name = attributes["name"];

            var geomInfo = getGeomInfo(attributes,
                fileParser.defaultSize, fileParser.defaultMass, fileParser.defaultFriction, fileParser.defaultRollingFriction, fileParser.defaultRestitution, fileParser.defaultAngularDamping, null
                );

            return new CookerType(name, geomInfo, recipes);
        }

        protected override void reset()
        {
            recipes = new List<Recipe>();
        }
    }
}
