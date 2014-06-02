using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;

namespace SousChef
{
    public class SoundThing
    {
        private static Dictionary<string, string> l = makeDict();

        public static void Play(string key)
        {
            new Thread(new SoundThread(l.get(key)).DoWork).Start();
        }

        private static Dictionary<string, string> makeDict()
        {
            Dictionary<string, string> m = new Dictionary<string, string>();
            foreach (string s in Directory.GetFiles("res\\sounds\\", "*.mp3"))
                m.Add(s.Substring(s.LastIndexOf('\\') + 1, s.LastIndexOf('.')), s);

            return m;
        }

        private class SoundThread
        {
            private string path;

            public SoundThread(string path)
            {
                this.path = path;
            }

            public void DoWork()
            {
                var player = new WMPLib.WindowsMediaPlayer();
                player.URL = @path;
            }
        }
    }
}
