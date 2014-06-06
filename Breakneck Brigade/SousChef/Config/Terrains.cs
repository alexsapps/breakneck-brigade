using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SousChef
{
    public class BBXTerrainsFileParser : BBXFileParser<TerrainType>
    {
        public BBXTerrainsFileParser(GameObjectConfig config) : base(config) { }
        protected override string getRootNodeName() { return "terrains"; }
        protected override string getListItemNodeName() { return "terrain"; }

        public float[] defaultSize;
        public float defaultMass;
        public float defaultFriction;
        public float defaultRollingFriction;
        public float defaultRestitution;

        protected override void handleAttributes()
        {
            base.handleAttributes();

            defaultSize = BBXItemParser<IngredientType>.parseFloats(attrib("defaultSize"));
            defaultMass = float.Parse(attrib("defaultMass"));
            defaultFriction = float.Parse(attrib("defaultFriction"));
            defaultRollingFriction = float.Parse(attrib("defaultRollingFriction"));
            defaultRestitution = float.Parse(attrib("defaultRestitution"));
        }

        public override BBXItemParser<TerrainType> getItemParser() { return new TerrainParser(config, this); }
    }

    public class TerrainParser : BBXItemParser<TerrainType>
    {
        BBXTerrainsFileParser fileParser;
        public TerrainParser(GameObjectConfig config, BBXTerrainsFileParser fileParser)
            : base(config)
        {
            this.fileParser = fileParser;
        }

        protected override void handleSubtree(XmlReader reader)
        {
            throw new Exception("content not allowed in terrain tag");
        }
        protected override TerrainType returnItem()
        {
            string name;
            string friendlyName;
            parseName(out name, out friendlyName);

            var geomInfo = getGeomInfo(attributes,
                fileParser.defaultSize, fileParser.defaultMass, fileParser.defaultFriction, fileParser.defaultRollingFriction, fileParser.defaultRestitution, 0, 0.0f, null
                );

            return new TerrainType(name, friendlyName, geomInfo);
        }

        protected override void reset() { }
    }

}
