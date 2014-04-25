using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SousChef.Config
{
    class GlobalConfigParser
    {
        public static void getText()
        { 
            String xmlString = "test";
            XmlReader reader = XmlReader.Create(new StringReader(xmlString));
        }
    }
}
