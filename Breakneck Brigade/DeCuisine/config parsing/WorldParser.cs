using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SousChef;
using System.Xml;
using System.Diagnostics;
using Tao.Ode;

namespace DeCuisine
{
    class BBSpace
    {
        public IntPtr Space { get; set; }
        public List<ServerGameObject> initGameObjects { get; set; }
    }
    class BBWorld
    {
        public IntPtr World { get; set; } //TODO use this
        public List<BBSpace> Spaces { get; set; }
    }

    class WorldFileParser
    {
        protected GameObjectConfig config;
        protected ServerGame serverGame;
        
        public string GetFileName() { return "world_{0}.xml"; }
        protected string getRootNodeName() { return "world"; }
        public WorldParser getItemParser() { return new WorldParser(config, serverGame); }

        public WorldFileParser(GameObjectConfig config, ServerGame serverGame)
        { 
            this.config = config; 
            this.serverGame = serverGame; 
        }

        public void LoadFile(int level)
        {
            LoadFile(string.Format(GetFileName(), level.ToString()));
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
        IntPtr World;
        List<BBSpace> spaces = new List<BBSpace>();

        public WorldParser(GameObjectConfig config, ServerGame serverGame) : base(config)
        {
            this.serverGame = serverGame;
        }

        protected override void HandleAttributes()
        {
            World = Ode.dWorldCreate();
            var gravity = getFloats(attributes["gravity"]);
            Debug.Assert(gravity.Length == 3);
            Ode.dWorldSetGravity(World, gravity[0], gravity[1], gravity[2]);

            Debug.Assert(serverGame.World == IntPtr.Zero);
            serverGame.World = World;
        }

        protected override void handleSubtree(System.Xml.XmlReader reader)
        {
            reader.MoveToContent();

            switch(reader.Name)
            {
                case "space":
                    spaces.Add(new SpaceParser(config, serverGame).Parse(reader.ReadSubtree()));
                    break;
                default:
                    throw new Exception(reader.Name + " tag not expected in <game>");
            }
        }

        protected override void reset()
        {
            World = default(IntPtr);
            spaces = new List<BBSpace>();
        }

        protected override BBWorld returnItem()
        {
            return new BBWorld() { World = World, Spaces = spaces };
        }
    }
    class SpaceParser : BBXItemParser<BBSpace>
    {
        IntPtr space;
        List<ServerGameObject> gameObjects = new List<ServerGameObject>();
        ServerGame serverGame;

        public SpaceParser(GameObjectConfig config, ServerGame serverGame) : base(config) { this.serverGame = serverGame; }

        protected override void HandleAttributes()
        {
            space = Ode.dHashSpaceCreate(IntPtr.Zero);
            Debug.Assert(attributes.Count == 0);

            Debug.Assert(serverGame.Space == IntPtr.Zero); //only one space currently supported
            serverGame.Space = space;
        }

        protected override void handleSubtree(System.Xml.XmlReader reader)
        {
            reader.MoveToContent();
            ServerGameObject obj = parseGameObject(reader);
            gameObjects.Add(obj);
        }

        protected override void reset()
        {
            gameObjects = new List<ServerGameObject>();
        }

        protected override BBSpace returnItem()
        {
            return new BBSpace() { Space = space, initGameObjects = gameObjects };
        }

        private ServerGameObject parseGameObject(XmlReader reader)
        {
            ServerGameObject obj;
            switch (reader.Name)
            {
                case "plane":
                    obj = parseSubItem<ServerPlane>(reader, new PlaneParser(config, serverGame, space)); break;
                case "box":
                    obj = parseSubItem<ServerBox>(reader, new BoxParser(config, serverGame, space)); break;
                case "ingredient":
                    obj = parseSubItem<ServerIngredient>(reader, new IngredientParser(config, serverGame)); break;
                case "cooker":
                    obj = parseSubItem<ServerCooker>(reader, new CookerParser(config, serverGame)); break;
                default:
                    throw new Exception(reader.Name + " tag not expected in <game>");
            }
            return obj;
        }
    }

    class IngredientParser : GameObjectParser<ServerIngredient>
    {
        public IngredientParser(GameObjectConfig config, ServerGame serverGame) : base(config, serverGame) { }

        protected override void reset() { }

        protected override ServerIngredient returnItem()
        {   
            return new ServerIngredient(serverGame.Config.Ingredients[attributes["type"]], serverGame, getCoordinateAttrib());
        }
    }

    class CookerParser : GameObjectParser<ServerCooker>
    {
        public CookerParser(GameObjectConfig config, ServerGame serverGame) : base(config, serverGame) { }

        protected override void reset() { }

        protected override ServerCooker returnItem()
        {
            return new ServerCooker(serverGame.Config.Cookers[attributes["type"]], serverGame, getCoordinateAttrib());
        }
    }

    class PlaneParser : GameObjectParser<ServerPlane>
    {
        IntPtr space;

        ServerPlane serverPlane;

        public PlaneParser(GameObjectConfig config, ServerGame serverGame, IntPtr space) : base (config, serverGame) 
        {
            this.space = space;
        }
        protected override void HandleAttributes()
        {
            float height = float.Parse(attributes["height"]);
            serverPlane = new ServerPlane(serverGame, attributes["texture"], height);
        }
        protected override void reset()
        {
            serverPlane = null;
        }
        protected override ServerPlane returnItem()
        {
            return serverPlane;
        }
    }

    class BoxParser : GameObjectParser<ServerBox>
    {
        IntPtr space;

        ServerBox serverBox;

        public BoxParser(GameObjectConfig config, ServerGame serverGame, IntPtr space)
            : base(config, serverGame)
        {
            this.space = space;
        }
        protected override void HandleAttributes()
        {
            var coord1 = getCoordinateAttrib("coordinate1");
            var coord2 = getCoordinateAttrib("coordinate2");
            serverBox = new ServerBox(serverGame, attributes["texture"], coord1, coord2);
        }
        protected override void reset()
        {
            serverBox = null;
        }
        protected override ServerBox returnItem()
        {
            return serverBox;
        }
    }

    abstract class GameObjectParser<T> : BBXItemParser<T> where T : ServerGameObject
    {
        protected ServerGame serverGame;
        public GameObjectParser(GameObjectConfig config, ServerGame serverGame) : base(config)
        {
            this.serverGame = serverGame;
        }
        protected Coordinate getCoordinateAttrib()
        {
            return getCoordinateAttrib("coordinate");
        }
        protected Coordinate getCoordinateAttrib(string attrib)
        {
            return getCoordinate(attributes[attrib]);
        }
        protected Coordinate getCoordinate(string str)
        {
            var floats = getFloats(str);
            Debug.Assert(floats.Length == 3);
            return new Coordinate(floats[0], floats[1], floats[2]);
        }
    }

}
