using Unity.Collections;
using Unity.Networking.Transport;

namespace Net.NetMessages
{
    public class NetRematch : NetMessage
    {
    
        public int TeamId { get; set; }
        public byte WantRematch { get; set; }
    
        public NetRematch()
        {
            Code = OpCode.Rematch;
        }

        public NetRematch(DataStreamReader reader)
        {
            Code = OpCode.Rematch;
            Deserialize(reader);
        }

        public override void Serialize(ref DataStreamWriter writer)
        {
            writer.WriteByte((byte) Code);
            writer.WriteInt(TeamId);
            writer.WriteByte(WantRematch);
        }

        public override void Deserialize(DataStreamReader reader)
        {
            TeamId = reader.ReadInt();
            WantRematch = reader.ReadByte();
        }

        public override void ReceivedOnClient()
        {
            NetUtility.ClientRematch?.Invoke(this);
        }

        public override void ReceivedOnServer(NetworkConnection conn)
        {
            NetUtility.ServerRematch?.Invoke(this, conn);
        }
    }
}
