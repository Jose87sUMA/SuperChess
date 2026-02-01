using Unity.Collections;
using Unity.Networking.Transport;

namespace Net.NetMessages
{
    public class NetWelcome : NetMessage
    {
    
        public int AssignedTeam { get; set; }
    
        public NetWelcome()
        {
            Code = OpCode.Welcome;
        }

        public NetWelcome(DataStreamReader reader)
        {
            Code = OpCode.Welcome;
            Deserialize(reader);
        }

        public override void Serialize(ref DataStreamWriter writer)
        {
            writer.WriteByte((byte) Code);
            writer.WriteInt(AssignedTeam);
        }

        public override void Deserialize(DataStreamReader reader)
        {
            AssignedTeam = reader.ReadInt();
        }

        public override void ReceivedOnClient()
        {
            NetUtility.ClientWelcome?.Invoke(this);
        }

        public override void ReceivedOnServer(NetworkConnection conn)
        {
            NetUtility.ServerWelcome?.Invoke(this, conn);
        }
    }
}
