﻿using System;
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

        public float[] defaultSize;
        public float defaultMass;
        public float defaultFriction;
        public float defaultRollingFriction;
        public float defaultRestitution;
        public float defaultAngularDamping;
        public float defaultOrientation;

        protected override void handleAttributes()
        {
            base.handleAttributes();

            defaultSize = BBXItemParser<IngredientType>.parseFloats(attrib("defaultSize"));
            defaultMass = float.Parse(attrib("defaultMass"));
            defaultFriction = float.Parse(attrib("defaultFriction"));
            defaultRollingFriction = float.Parse(attrib("defaultRollingFriction"));
            defaultRestitution = float.Parse(attrib("defaultRestitution"));
            defaultAngularDamping = float.Parse(attrib("defaultAngularDamping"));
            defaultOrientation = float.Parse(attrib("defaultOrientation"));
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
            string name;
            string friendlyName;
            parseName(out name, out friendlyName);

            int points = int.Parse(attributes["points"]);

            var geomInfo = getGeomInfo(attributes,
                fileParser.defaultSize, fileParser.defaultMass, fileParser.defaultFriction, fileParser.defaultRollingFriction, fileParser.defaultRestitution, fileParser.defaultAngularDamping, fileParser.defaultOrientation, null
                );
            double powerUp = 0;
            if (attributes.ContainsKey("powerup"))
            {
                if (!Double.TryParse(attributes["powerup"], out powerUp))
                    throw new Exception("powerup not a double");
            }
            return new IngredientType(name, friendlyName, geomInfo, points, powerUp);
        }

        protected override void reset() { }
    }
}
