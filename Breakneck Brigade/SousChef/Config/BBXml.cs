using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SousChef
{
    public static class BBXml
    {
        public static XmlReaderSettings getSettings()
        {
            return new XmlReaderSettings()
            {
                IgnoreComments = true,
                IgnoreWhitespace = true,
            };
        }

/*
pushd $(ProjectDir)
mkdir $(OutDir)\config
xcopy ..\config $(OutDir)\config\ /s /i /y
popd
exit 0
*/

        public static string GetLocalConfigFolder()
        {
            var path = Path.Combine(Environment.GetFolderPath(
                Environment.SpecialFolder.ApplicationData), @"BreakneckBrigade\Config");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            return path;
        }
        public static string GetLocalConfigFile(string filename)
        {
            return Path.Combine(GetLocalConfigFolder(), filename);
        }
        public static string GetAppConfigFolder()
        {
            return null;
        }

        public static string CombinePath(string folder, string file)
        {
            if (folder != null)
            {
                string result = Path.Combine(folder, file);
                return File.Exists(result) ? result : null;
            }
            else
            {
                string[] paths = new string[]
                { 
                    @".\Config", 
                    @"..\Config",
                    @"..\..\Config",
                    @"..\..\..\Config" 
                }; //i'm really sorry about this

                foreach (var path in paths)
                {
                    string result = Path.Combine(path, file);
                    if (File.Exists(result))
                        return result;
                }

                return null;
            }
        }

        //reads <... att1="val1" att2="val2" /> ... into a dictionary
        public static Dictionary<string, string> getAttributes(XmlReader reader)
        {
            var attributes = new Dictionary<string, string>();
            if (reader.MoveToFirstAttribute())
            {
                do
                {
                    attributes.Add(reader.Name, reader.Value); //handleAttribute(reader.Name, reader.Value); //protected abstract void handleAttribute(string name, string value);
                } while (reader.MoveToNextAttribute());
            }
            return attributes;
        }

        public static string get(this Dictionary<string, string> d, string attribName)
        {
            string val;
            return d.TryGetValue(attribName, out val) ? val : null;
        }

    }
}
