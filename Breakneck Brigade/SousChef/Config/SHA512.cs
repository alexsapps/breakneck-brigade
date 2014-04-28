using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SousChef
{
    public static class SHA512
    {
        /*
         * http://peterkellner.net/2010/11/24/efficiently-generating-sha256-checksum-for-files-using-csharp/
         */
        public static byte[] GetChecksum(string file)
        {
            using (FileStream stream = File.OpenRead(file))
            {
                var sha = new SHA256Managed();
                byte[] checksum = sha.ComputeHash(stream);
                return checksum;
                //return BitConverter.ToString(checksum).Replace("-", String.Empty);
            }
        }

        /*
         * http://peterkellner.net/2010/11/24/efficiently-generating-sha256-checksum-for-files-using-csharp/
         */
        public static byte[] GetChecksumBuffered(Stream stream)
        {
            using (var bufferedStream = new BufferedStream(stream, 1024 * 32))
            {
                var sha = new SHA256Managed();
                byte[] checksum = sha.ComputeHash(bufferedStream);
                return checksum;
                //return BitConverter.ToString(checksum).Replace("-", String.Empty);
            }
        }

        /*
         * alexander taylor
         */
        public static string GetCombinedHash(byte[][] checksums)
        {
            byte[] checksum = new byte[32];

            foreach (byte[] c in checksums)
            {
                for(int i = 0; i < 32; i++)
                {
                    checksum[i] ^= c[i];
                }
            }
            return BitConverter.ToString(checksum).Replace("-", String.Empty);
        }

    }
}
