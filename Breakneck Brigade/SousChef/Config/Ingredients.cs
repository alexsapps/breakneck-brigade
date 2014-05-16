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

        public float[] defaultSides;
        public float defaultMass;
        public float defaultFriction;
        public float defaultRestitution;

        protected override void handleAttributes()
        {
            base.handleAttributes();

            defaultSides = BBXItemParser<IngredientType>.parseFloats(attrib("defaultSides"));
            defaultMass = float.Parse(attrib("defaultMass"));
            defaultFriction = float.Parse(attrib("defaultFriction"));
            defaultRestitution = float.Parse(attrib("defaultRestitution"));
        }
        
        public override BBXItemParser<IngredientType> getItemParser() { return new IngredientParser(config, this); }
    }
    public class IngredientParser : BBXItemParser<IngredientType>
    {
        BBXIngredientsFileParser fileParser;
        public IngredientParser(GameObjectConfig config, BBXIngredientsFileParser fileParser) : base(config) 
        { 
            this.fileParser = fileParser; 
        }

        protected override void handleSubtree(XmlReader reader)
        {
            throw new Exception("content not allowed in ingredient tag");
        }
        protected override IngredientType returnItem()
        {
            string name = attributes["name"];
            int points = int.Parse(attributes["points"]);

            var geomInfo = getGeomInfo(attributes,
                fileParser.defaultSides, fileParser.defaultMass, fileParser.defaultFriction, fileParser.defaultRestitution
                );

            return new IngredientType(name, geomInfo, points);
        }

        protected override void reset() { }
    }
}
