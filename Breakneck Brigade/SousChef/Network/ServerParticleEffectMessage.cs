using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SousChef
{
    public class ServerParticleEffectMessage : ServerMessage
    {
        public Vector4 Location { get; set; }
        public BBParticleEffect ParticleEffect { get; set; }

        public override ServerMessageType Type
        {
            get { return ServerMessageType.ParticleEffect; }
        }

        public override void Write(System.IO.BinaryWriter writer)
        {
            writer.Write(Location);
            writer.Write((Int32)ParticleEffect);
        }

        public override void Read(System.IO.BinaryReader reader)
        {
            Location = reader.ReadCoordinate();
            ParticleEffect = (BBParticleEffect)reader.ReadInt32();
        }
    }
}
