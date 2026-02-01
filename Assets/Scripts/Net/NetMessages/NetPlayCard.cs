using System;
using Unity.Collections;
using Unity.Networking.Transport;
using UnityEngine;

namespace Net.NetMessages
{
    public sealed class NetPlayCard : NetMessage
    {
        public int CardId;
        public int PayloadLen;
        public int TeamId;
        public bool Confirmed;
        public byte[] Payload = Array.Empty<byte>();

        public NetPlayCard()
        {
            Code = OpCode.PlayCard;
        }
        public NetPlayCard(DataStreamReader r)
        {
            Code = OpCode.PlayCard; 
            Deserialize(r);
        }

        public override void Serialize(ref DataStreamWriter w)
        {
            w.WriteByte((byte)Code);
            w.WriteInt(CardId);
            w.WriteInt(PayloadLen);
            w.WriteInt(TeamId);
            w.WriteByte(Confirmed ? (byte)1 : (byte)0);     
            for (int i = 0; i < PayloadLen; i++) w.WriteByte(Payload[i]);
        }
        public override void Deserialize(DataStreamReader r)
        {
            CardId     = r.ReadInt();
            PayloadLen = r.ReadInt();
            TeamId = r.ReadInt();
            Confirmed  = r.ReadByte() == 1;                
            Payload    = new byte[PayloadLen];
            for (int i = 0; i < PayloadLen; i++) Payload[i] = r.ReadByte();
        }

        public override void ReceivedOnClient() =>
            NetUtility.ClientPlayCard?.Invoke(this);
        
        public override void ReceivedOnServer(NetworkConnection c) =>
            NetUtility.ServerPlayCard?.Invoke(this, c);
    }

}
