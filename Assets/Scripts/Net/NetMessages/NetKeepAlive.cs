using Unity.Collections;
using Unity.Networking.Transport;

namespace Net.NetMessages
{
    public class NetKeepAlive : NetMessage
    {
        public NetKeepAlive()
        {
            Code = OpCode.KeepAlive;
        }

        public NetKeepAlive(DataStreamReader reader)
        {
            Code = OpCode.KeepAlive;
            Deserialize(reader);
        }

        public override void Serialize(ref DataStreamWriter writer)
        {
            writer.WriteByte((byte) Code);
        }

        public override void Deserialize(DataStreamReader reader)
        {
        
        }

        public override void ReceivedOnClient()
        {
            NetUtility.ClientKeepAlive?.Invoke(this);
        }

        public override void ReceivedOnServer(NetworkConnection conn)
        {
            NetUtility.ServerKeepAlive?.Invoke(this, conn);
        }
    }
}