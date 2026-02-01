using Unity.Collections;
using Unity.Networking.Transport;

namespace Net.NetMessages
{
    public class NetMakeMove : NetMessage
    {

        public int OriginalX;
        public int OriginalY;
        public int DestinationX;
        public int DestinationY;
        public int TeamId;
    
        public NetMakeMove()
        {
            Code = OpCode.MakeMove;
        }

        public NetMakeMove(DataStreamReader reader)
        {
            Code = OpCode.MakeMove;
            Deserialize(reader);
        }

        public override void Serialize(ref DataStreamWriter writer)
        {
            writer.WriteByte((byte) Code);
            writer.WriteInt(OriginalX);
            writer.WriteInt(OriginalY);
            writer.WriteInt(DestinationX);
            writer.WriteInt(DestinationY);
            writer.WriteInt(TeamId);
        }

        public override void Deserialize(DataStreamReader reader)
        {
            OriginalX = reader.ReadInt();
            OriginalY = reader.ReadInt();
            DestinationX = reader.ReadInt();
            DestinationY = reader.ReadInt();
            TeamId = reader.ReadInt();
        }

        public override void ReceivedOnClient()
        {
            NetUtility.ClientMakeMove?.Invoke(this);
        }

        public override void ReceivedOnServer(NetworkConnection conn)
        {
            NetUtility.ServerMakeMove?.Invoke(this, conn);
        }
    }
}
