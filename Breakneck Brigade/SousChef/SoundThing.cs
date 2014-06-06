using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Windows.Media;

namespace SousChef
{
    /// <summary>
    /// Plays a sound.  Name was chosen by ET.  Usage:
    ///1) Add the sound file you'd like to include in the game to (supported types: wma, mp3, wav) "res\sounds\".
    ///2) Let's say that your file is titled 'scratch.wav'.  Method call then, is SoundThing.Play("scratch").  The key *is* case sensitive.
    ///3) Rebuild project & listen to beautiful music.
    ///
    /// Method is case sensitive, and omits the file extension.  Files to be played MUST be in "res\sounds\"
    /// 
    /// </summary>
    public static class SoundThing
    {
        public static bool off;

        /// <summary>
        /// Internal dictionary/hashmap which links files with their shorthand names.
        /// </summary>
        private static Dictionary<string, string> _l;

        /// <summary>
        /// I have no idea what the fuck this does, but Alex says we need it :o
        /// </summary>
        private static Dictionary<string, string> l { get { return _l ?? (_l = makeDict()); } }

        /// <summary>
        /// Currently playing list of sounds.  This makes the cdj possible.
        /// </summary>
        private static List<SoundThread> soundThreads = new List<SoundThread>();

        /// <summary>
        /// Plays a sound. See class header comments for details.
        /// </summary>
        /// <param name="key">The key of the sound to play.  Note that this *is* case-sensitive.</param>
        /// <param name="volume">The volume to play at, on a scale of 0-1, where 0 is silent and 1 is loudest.</param>
        public static SoundThread Play(BBSound key, double volume)
        {
            if (off)
                return null;

            String temp = "";
            if (l.TryGetValue(key.ToString(), out temp))
            {
                var soundThread = new SoundThread(temp, volume);
                new Thread(soundThread.DoWork).Start();
                soundThreads.Add(soundThread);
                return soundThread;
            }
            else
                throw new Exception("no sound file for " + key.ToString());
        }


        /// <summary>
        /// Stop all currently playing sounds.
        /// </summary>
        public static void Stop()
        {
            foreach (var soundThread in soundThreads)
                soundThread.Stop();
        }

        /// <summary>
        /// Generates our dictionary at runtime.
        /// </summary>
        /// <returns>The dictionary linking each keys to file paths</returns>
        private static Dictionary<string, string> makeDict()
        {
            Dictionary<string, string> m = new Dictionary<string, string>();
            Console.WriteLine(@"res\sounds\");
            Console.WriteLine( @"*\.(mp3|wav)");

            foreach (string s in Directory.GetFiles(@"res\sounds\", @"*")) //@"*\.(mp3|wav)", SearchOption.AllDirectories))
            {
                int start = s.LastIndexOf(@"\") + 1;
                int length = s.LastIndexOf('.')  - start;
                m.Add(s.Substring(start, length), s);
            }
            return m;
        }
    }

    /// <summary>
    /// class representing a thread playing a sound.
    /// </summary>
    public class SoundThread
    {
        /// <summary>
        /// The path to the file to play
        /// </summary>
        private string path;

        /// <summary>
        /// The volume to play the sound at
        /// </summary>
        private double volume;

        /// <summary>
        /// flag for if we've quit
        /// </summary>
        private bool quit = false;
       
        /// <summary>
        /// Monitor for multi-threading
        /// </summary>
        private BBLock endLock = new BBLock();

        /// <summary>
        /// ?
        /// </summary>
        public bool Finished { get { return quit; } }

        /// <summary>
        /// Constructor, creates a SoundThread with a specified sound to play.
        /// </summary>
        /// <param name="path">The path to the file to play</param>
        /// <param name="volume">The volume level to play a sound between 0-1, where 0 is queit, and 1 is loudest.</param>
        public SoundThread(string path, double volume)
        {
            this.path = path;

            if (volume > 0.99)
                this.volume = 0.99;
            else if (volume < 0.0)
                this.volume = 0.0;
            else
                this.volume = volume;
        }

        /// <summary>
        /// Actually plays the sound.  Should only be called by Thread.
        /// </summary>
        public void DoWork()
        {
            MediaPlayer player = new MediaPlayer();
            player.MediaEnded += player_MediaEnded;
            player.MediaFailed += player_MediaEnded;
            player.Open(new Uri(path, UriKind.Relative));
            player.Volume = volume;
            lock (endLock)
            {
                player.Play();
                while (!quit)
                {
                    Monitor.Wait(endLock, 10);
                    if (player.Position >= player.NaturalDuration)
                    {
                        quit = true;
                        break; //workaround.  see http://answers.flyppdevportal.com/categories/metro/csharpvb.aspx?ID=4af53ba0-906d-4f99-9d70-a91ed7a27c0b
                    }
                }
                player.Stop();
            }
        }


        void player_MediaEnded(object sender, EventArgs e)
        {
            Stop();
        }

        public void Stop()
        {
            lock (endLock)
            {
                quit = true;
                Monitor.PulseAll(endLock);
            }
        }
    }
}
