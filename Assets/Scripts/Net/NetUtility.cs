using System;
using Net.NetMessages;
using Unity.Collections;
using Unity.Networking.Transport;
using UnityEngine;

namespace Net
{
    public enum OpCode
    {
        KeepAlive = 1,
        Welcome = 2,
        StartGame = 3,
        MakeMove = 4,
        Rematch = 5,
        PlayCard = 6,
    }

    public static class NetUtility
    {

        public static Action<NetMessage> ClientKeepAlive;
        public static Action<NetMessage> ClientWelcome;
        public static Action<NetMessage> ClientStartGame;
        public static Action<NetMessage> ClientMakeMove;
        public static Action<NetMessage> ClientRematch;
        public static Action<NetMessage> ClientPlayCard;
        public static Action<NetMessage, NetworkConnection> ServerKeepAlive;
        public static Action<NetMessage, NetworkConnection> ServerWelcome;
        public static Action<NetMessage, NetworkConnection> ServerStartGame;
        public static Action<NetMessage, NetworkConnection> ServerMakeMove;
        public static Action<NetMessage, NetworkConnection> ServerRematch;
        public static Action<NetMessage, NetworkConnection> ServerPlayCard;

        public static void OnData(DataStreamReader stream, NetworkConnection conn, Server server = null)
        {
            NetMessage msg = null;
            var opCode = (OpCode)stream.ReadByte();
            switch (opCode)
            {
                case OpCode.KeepAlive: msg = new NetKeepAlive(stream); break;
                case OpCode.Welcome: msg = new NetWelcome(stream); break;
                case OpCode.StartGame: msg = new NetStartGame(stream); break;
                case OpCode.MakeMove: msg = new NetMakeMove(stream); break;
                case OpCode.Rematch: msg = new NetRematch(stream); break;
                case OpCode.PlayCard: msg = new NetPlayCard(stream); break;
                default:
                    Debug.Log("Message received had no OpCode");
                    return;
            }
        
            if (server)
                msg.ReceivedOnServer(conn);
            else
                msg.ReceivedOnClient();
                
        }

    }
}