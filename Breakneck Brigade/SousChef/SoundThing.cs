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
    /**
     * Plays a sound.  Name was chosen by ET.  Usage:
     * 
     * 1) Add the sound file you'd like to include in the game to (supported types: wma, mp3, wav) "res\sounds\".
     * 2) Let's say that your file is titled 'scratch.wav'.  Method call then, is SoundThing.Play("scratch").  The key *is* case sensitive.
     * 3) Rebuild project & listen to music.
     * 
     * Method is case sensitive, and omits the file extension.  Files to be played MUST be in "res\sounds\"
     * 
     * @author Alec
     */
    public class SoundThing
    {
        /**
         * Internal dictionary/hashmap which links files with their shorthand names.
         */
        private static Dictionary<string, string> l = makeDict();

        /**
         * Plays a sound. See class header comments for details.
         * 
         * @param key The key of the sound to play.  Note that this *is* case-sensitive.
         */
        public static void Play(string key)
        {
            String temp = "";
            l.TryGetValue(key, out temp);
            new Thread(new SoundThread(temp).DoWork).Start();
        }

        /**
         * Generates our dictionary at runtime.
         */
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

        /**
         * Internal class representing a thread playing a sound.
         * 
         * @author Alec
         */
        private class SoundThread
        {
            /**
             * The path to the file to play
             */
            private string path;

            /**
             * Constructor, creates a SoundThread with a specified sound to play.
             * 
             * @param path The path to the file to play
             */
            public SoundThread(string path)
            {
                this.path = path;
            }

            /**
             * Actually plays the sound.  Should only be called by Thread.
             */
            public void DoWork()
            {
                MediaPlayer player = new MediaPlayer();
                player.Open(new Uri(path, UriKind.Relative));
                player.Play();
                Thread.Sleep(10000); //FIXME: This really should be managed by an event
                Console.WriteLine("Done playing sound");
            }
        }
    }
}
