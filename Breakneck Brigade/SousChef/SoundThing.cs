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
    public class SoundThing
    {
        /// <summary>
        /// Internal dictionary/hashmap which links files with their shorthand names.
        /// </summary>
        private static Dictionary<string, string> _l;

        /// <summary>
        /// I have no idea what the fuck this does, but Alex says we need it :o
        /// </summary>
        private static Dictionary<string, string> l { get { return _l ?? (_l = makeDict()); } }

        /// <summary>
        /// Plays a sound. See class header comments for details.
        /// </summary>
        /// <param name="key">The key of the sound to play.  Note that this *is* case-sensitive.</param>
        /// <param name="volume">The volume to play at, on a scale of 0-1, where 0 is silent and 1 is loudest.</param>
        public static void Play(BBSound key, double volume)
        {
            String temp = "";
            if (l.TryGetValue(key.ToString(), out temp))
                new Thread(new SoundThread(temp, volume).DoWork).Start();
            else
                throw new Exception("no sound file for " + key.ToString());
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

        /// <summary>
        /// Internal class representing a thread playing a sound.
        /// </summary>
        private class SoundThread
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
            /// Constructor, creates a SoundThread with a specified sound to play.
            /// 
            /// </summary>
            /// <param name="path">The path to the file to play</param>
            /// <param name="volume">The volume level to play a sound between 0-1, where 0 is queit, and 1 is loudest.</param>
            public SoundThread(string path, double volume)
            {
                this.path = path;
                this.volume = volume;
            }
            
             /// <summary>
             /// Actually plays the sound.  Should only be called by Thread.
             /// </summary>

            public void DoWork()
            {
                MediaPlayer player = new MediaPlayer();
                player.Open(new Uri(path, UriKind.Relative));
                player.Volume = volume;
                player.Play();
                Thread.Sleep(4000); //FIXME: Should be managed by an event.  Thread suicides in 4.5 seconds
            }
        }
    }
}
