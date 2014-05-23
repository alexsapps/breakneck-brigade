using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SousChef;
using System.Xml;
using System.Diagnostics;

using BulletSharp;

namespace DeCuisine
{
    class BBSpace
    {
        public List<ServerGameObject> initGameObjects { get; set; }
    }
    class BBWorld
    {
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
        ServerGame serverGame = null;
        DynamicsWorld world;
        List<BBSpace> spaces = new List<BBSpace>();

        public WorldParser(GameObjectConfig config, ServerGame serverGame) : base(config)
        {
            this.serverGame = serverGame;
        }

        protected override void HandleAttributes()
        {
            serverGame.CollisionConf = new DefaultCollisionConfiguration();
            serverGame.Dispatcher = new CollisionDispatcher(serverGame.CollisionConf);
            serverGame.Broadphase = new DbvtBroadphase();
            this.world = new DiscreteDynamicsWorld(serverGame.Dispatcher, serverGame.Broadphase, null, serverGame.CollisionConf);

            var gravity = parseFloats(attributes["gravity"]);
            Debug.Assert(gravity.Length == 3);
            this.world.Gravity = new Vector3(gravity[0], gravity[1], gravity[2]);

            Debug.Assert(this.serverGame.World == null);
            serverGame.World = this.world;
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
            this.world = null;
            spaces = new List<BBSpace>();
        }

        protected override BBWorld returnItem()
        {
            return new BBWorld() { Spaces = spaces };
        }
    }
    class SpaceParser : BBXItemParser<BBSpace>
    {
        List<ServerGameObject> gameObjects = new List<ServerGameObject>();
        ServerGame serverGame;

        public SpaceParser(GameObjectConfig config, ServerGame serverGame) : base(config) 
        { 
            this.serverGame = serverGame; 
        }

        protected override void HandleAttributes()
        {
            // this.space = new Space(); // Ode.dHashSpaceCreate(IntPtr.Zero);
            Debug.Assert(attributes.Count == 0);

            // Debug.Assert(this.serverGame.Space == null); //only one space currently supported
            // this.serverGame.Space = this.space;
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
            return new BBSpace() { initGameObjects = gameObjects };
        }

        private ServerGameObject parseGameObject(XmlReader reader)
        {
            ServerGameObject obj;
            // Enum.Parse(GameObjectClass, reader.Name, true);
            switch (reader.Name)
            {
                case "terrain":
                    obj = parseSubItem<ServerTerrain>(reader, new PlaneParser(config, serverGame)); 
                    break;
                case "static":
                    obj = parseSubItem<ServerStaticObject>(reader, new StaticParser(config, serverGame)); 
                    break;
                case "ingredient":
                    obj = parseSubItem<ServerIngredient>(reader, new IngredientParser(config, serverGame)); 
                    break;
                case "cooker":
                    obj = parseSubItem<ServerCooker>(reader, new CookerParser(config, serverGame)); 
                    break;
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

    class PlaneParser : GameObjectParser<ServerTerrain>
    {

        ServerTerrain serverPlane;

        public PlaneParser(GameObjectConfig config, ServerGame serverGame) : base (config, serverGame) 
        {
            //this.space = space;
        }
        protected override void HandleAttributes()
        {
            GeometryInfo info = new GeometryInfo();
            // float height = float.Parse(attributes["height"]);
            info.Friction = float.Parse(attributes.ContainsKey("friction") ? attributes["friction"] : "0");
            info.Position = this.getCoordinateAttrib("coordinate");
            info.Euler = this.getCoordinateAttrib("euler");
            Vector3 sides = this.getCoordinateAttrib("sides");
            info.Sides = new float[] { sides.X, sides.Y, sides.Z };
            info.Shape = attributes.ContainsKey("shape") ? ((GeomShape) Enum.Parse(typeof(GeomShape), attributes["shape"], true)) : GeomShape.None;
            serverPlane = new ServerTerrain(serverGame, attributes.ContainsKey("texture") ? attributes["texture"] : "blank", info);
        }
        protected override void reset()
        {
            serverPlane = null;
        }
        protected override ServerTerrain returnItem()
        {
            return serverPlane;
        }
    }

    class StaticParser : GameObjectParser<ServerStaticObject>
    {
        // Space space;

        ServerStaticObject serverStatic;

        public StaticParser(GameObjectConfig config, ServerGame serverGame)
            : base(config, serverGame)
        {
            // this.space = space;
        }
        protected override void HandleAttributes()
        {
            var position = getCoordinateAttrib("coordinate");
            var x = this.serverGame.Config;
            var geomInfo = getGeomInfo(attributes, new float[] {5, 5, 5}, 1000, 5, 5, 0, .999f);
            serverStatic = new ServerStaticObject(serverGame, geomInfo, attributes["model"], position);
        }
        protected override void reset()
        {
            serverStatic = null;
        }
        protected override ServerStaticObject returnItem()
        {
            return serverStatic;
        }
    }

    abstract class GameObjectParser<T> : BBXItemParser<T> where T : ServerGameObject
    {
        protected ServerGame serverGame;
        public GameObjectParser(GameObjectConfig config, ServerGame serverGame) : base(config)
        {
            this.serverGame = serverGame;
        }
        protected Vector3 getCoordinateAttrib()
        {
            return getCoordinateAttrib("coordinate");
        }
        protected Vector3 getCoordinateAttrib(string attrib)
        {
            return attributes.ContainsKey(attrib) ? getCoordinate(attributes[attrib]) : new Vector3();
        }
        protected Vector3 getCoordinate(string str)
        {
            var floats = parseFloats(str);
            Debug.Assert(floats.Length == 3);
            return new Vector3(floats[0], floats[1], floats[2]);
        }
    }

}
