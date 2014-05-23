using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SousChef
{
    public abstract class BinaryServerMessage : ServerMessage
    {
        public byte[] Binary;
        public BinaryServerMessage() { }
        public override void Write(BinaryWriter writer)
        {
            writer.Write(Binary.Length);
            writer.Write(Binary);
        }

        public delegate void BinaryServerMessageWriter(BinaryWriter writer);

        public void Write(BinaryServerMessageWriter writer)
        {
            using (MemoryStream membin = new MemoryStream())
            {
                using (BinaryWriter w = new BinaryWriter(membin))
                {
                    writer(w);
                }
                Binary = membin.ToArray();
            }
        }

        public override void Read(BinaryReader reader)
        {
            int len = reader.ReadInt32();
            Binary = reader.ReadBytes(len);
        }

        public delegate void BinaryServerMessageReader(BinaryReader reader);

        public void Read(BinaryServerMessageReader reader)
        {
            using (MemoryStream mem = new MemoryStream(Binary))
            {
                using (BinaryReader r = new BinaryReader(mem))
                {
                    reader(r);
                }
            }
        }        
    }
}
