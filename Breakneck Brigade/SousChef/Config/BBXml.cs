using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SousChef
{
    public class BBXml
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
            return Path.Combine(Environment.GetFolderPath(
                Environment.SpecialFolder.ApplicationData), @"BreakneckBrigade\Config");
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
    }
}
