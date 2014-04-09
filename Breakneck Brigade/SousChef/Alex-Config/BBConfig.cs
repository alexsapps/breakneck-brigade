using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SousChef.Parsing
{
    public class BBConfig
    {

        public delegate List<T> BBXListHandler<T>(XmlNode node);
        public delegate T BBXItemHandler<T>(XmlNode node);
        
        public List<Ingredient> GetIngredients() {
            return GetFile<Ingredient>("ingredients", GetIngredient);
        }

        Ingredient GetIngredient(XmlNode node)
        {
            throw new NotImplementedException();
        }

        List<T> GetFile<T>(string filename, BBXItemHandler<T> listHandler) {
            XmlNode node = null; //TODO: open file
            return getList<T>(filename, listHandler);
        }

        List<T> getList<T> (string name, BBXItemHandler<T> itemHandler) {
            throw new NotImplementedException();
        }

        Dictionary<string, string> getNameValuePairs(XmlNode node)
        {
            throw new NotImplementedException();
        }
    }

}
