using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SousChef
{
    public delegate int IdGetter();

    public class GameObjectConfig
    {
        public IdGetter IdGetter { get; private set; }
        public string ConfigDir { get; private set; }

        public GameObjectConfig(IdGetter idGetter) 
            : this(idGetter, BBXml.GetAppConfigFolder())
        {
            
        }
        public GameObjectConfig(IdGetter idGetter, string configDir)
        {
            this.IdGetter = idGetter;
            this.ConfigDir = configDir;
        }

        public delegate List<T> BBXListHandler<T>(XmlReader node);
        public delegate T BBXItemHandler<T>(XmlReader node);

        public List<Ingredient> GetIngredients()
        {
            return new BBXIngredientsFileParser(this).LoadFile();
        }
    }
}