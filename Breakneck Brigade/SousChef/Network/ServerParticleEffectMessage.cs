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
        public int Param { get; set; }
        public int FollowID { get; set; }
        public int Id { get; set; }

        public ServerParticleEffectMessage()
        {
            FollowID = -1;
            Id = -1;
        }

        public override ServerMessageType Type
        {
            get { return ServerMessageType.ParticleEffect; }
        }

        public override void Write(System.IO.BinaryWriter writer)
        {
            writer.Write(Location);
            writer.Write((Int32)ParticleEffect);
            writer.Write(Param);
            writer.Write(FollowID);
            writer.Write(Id);
        }

        public override void Read(System.IO.BinaryReader reader)
        {
            Location = reader.ReadCoordinate();
            ParticleEffect = (BBParticleEffect)reader.ReadInt32();
            Param = reader.ReadInt32();
            FollowID = reader.ReadInt32();
            Id = reader.ReadInt32();
        }
    }
}
