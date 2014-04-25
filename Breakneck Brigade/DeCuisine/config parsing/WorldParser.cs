using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SousChef;
using System.Xml;

namespace DeCuisine
{
    class BBSpace
    {
        public List<ServerGameObject> initGameObjects { get; set; }
    }
    class BBWorld
    {
        public IntPtr World { get; set; } //TODO use this
        public List<BBSpace> Spaces { get; set; }
        public float[] Gravity { get; set; }
    }

    class WorldFileParser
    {
        protected GameObjectConfig config;
        protected ServerGame serverGame;
        
        public string GetFileName() { return getRootNodeName() + ".xml"; }
        protected string getRootNodeName() { return "world"; }
        public WorldParser getItemParser() { return new WorldParser(config, serverGame); }

        public WorldFileParser(GameObjectConfig config, ServerGame serverGame)
        { 
            this.config = config; 
            this.serverGame = serverGame; 
        }

        public void LoadFile()
        {
            LoadFile(GetFileName());
        }
        public void LoadFile(string filename)
        {
            string file = BBXml.CombinePath(config.ConfigDir, filename);
            using (XmlReader reader = XmlReader.Create(file, BBXml.getSettings()))
            {
                parseFile(reader);
            }
        }

        public virtual void parseFile(XmlReader reader)
        {
            if (reader.MoveToContent() == XmlNodeType.Element
                && reader.Name == getRootNodeName())
            {
                new WorldParser(config, serverGame).Parse(reader.ReadSubtree());
            }
            else
            {
                throw new Exception("bad xml file (1)");
            }
        }
    }
    class WorldParser : BBXItemParser<BBWorld>
    {
        ServerGame serverGame;
        public WorldParser(GameObjectConfig config, ServerGame serverGame) : base(config)
        {
            this.serverGame = serverGame;
        }

        protected override void handleSubtree(System.Xml.XmlReader reader)
        {
            reader.MoveToContent();

            switch(reader.Name)
            {
                case "space":
                    new SpaceParser(config, serverGame).Parse(reader.ReadSubtree());
                    break;
                default:
                    throw new Exception(reader.Name + " tag not expected in <game>");
            }
        }

        protected override void reset()
        {
            throw new NotImplementedException();
        }

        protected override BBWorld returnItem()
        {
            throw new NotImplementedException();
        }
    }
    class SpaceParser : BBXItemParser<BBSpace>
    {
        ServerGame serverGame;
        public SpaceParser(GameObjectConfig config, ServerGame serverGame) : base (config)
        {
            this.serverGame = serverGame;
        }

        protected override void handleSubtree(System.Xml.XmlReader reader)
        {
            reader.MoveToContent();

            switch (reader.Name)
            {
                case "space":
                    new SpaceParser(config, serverGame).Parse(reader.ReadSubtree());
                    break;
                default:
                    throw new Exception(reader.Name + " tag not expected in <game>");
            }
        }

        protected override void reset()
        {
            throw new NotImplementedException();
        }

        protected override BBSpace returnItem()
        {
            throw new NotImplementedException();
        }
    }

    class IngredientParser : BBXItemParser<ServerIngredient>
    {
        ServerGame serverGame;
        public IngredientParser(GameObjectConfig config, ServerGame serverGame)
            : base(config)
        {
            this.serverGame = serverGame;
        }

        protected override void reset()
        {
            throw new NotImplementedException();
        }

        protected override ServerIngredient returnItem()
        {
            throw new NotImplementedException();
        }
    }

}
