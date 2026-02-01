using Unity.Collections;
using Unity.Networking.Transport;

namespace Net.NetMessages
{
    public class NetStartGame : NetMessage
    {
    
        public NetStartGame()
        {
            Code = OpCode.StartGame;
        }

        public NetStartGame(DataStreamReader reader)
        {
            Code = OpCode.StartGame;
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
            NetUtility.ClientStartGame?.Invoke(this);
        }

        public override void ReceivedOnServer(NetworkConnection conn)
        {
            NetUtility.ServerStartGame?.Invoke(this, conn);
        }
    }
}
