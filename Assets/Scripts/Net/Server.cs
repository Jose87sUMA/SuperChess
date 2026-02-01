using System;
using Net.NetMessages;
using Unity.Collections;
using Unity.Networking.Transport;
using UnityEngine;

namespace Net
{
    public class Server : MonoBehaviour
    {
        #region Singleton Implementation

        public static Server Instance {set; get;}
        private void Awake()
        {
            Instance = this;
        }

        #endregion

        public NetworkDriver Driver;
        private NativeList<NetworkConnection> _connections;

        private bool _isActive = false;
        private const float KeepAliveTickRate = 20.0f;
        private float _lastKeepAlive;

        public Action ConnectionDropped;

        public void Init(ushort port)
        {
            Driver = NetworkDriver.Create();
            NetworkEndpoint endpoint = NetworkEndpoint.AnyIpv4;
            endpoint.Port = port;

            if (Driver.Bind(endpoint) != 0)
            {
                Debug.Log("Unable to bind on port:" + endpoint.Port);
                return;
            }

            Driver.Listen();
            Debug.Log("Currently listening on port " + endpoint.Port);
        
            _connections = new NativeList<NetworkConnection>(2, Allocator.Persistent);
            _isActive = true;
        }

        public void Shutdown()
        {
            if (_isActive)
            {
                Driver.Dispose();
                _connections.Dispose();
                _isActive = false;
            }
        }

        public void OnDestroy()
        {
            Shutdown();
        }

        public void Update()
        {
            if(!_isActive)
                return;

            KeepAlive();
        
            Driver.ScheduleUpdate().Complete();

            CleanupConnections();
            AcceptNewConnections();
            UpdateMessagePump();
        }

        private void KeepAlive()
        {
            if (Time.time - _lastKeepAlive > KeepAliveTickRate)
            {
                _lastKeepAlive = Time.time;
                Broadcast(new NetKeepAlive());
            }
        }

        private void CleanupConnections()
        {
            for (int i = 0; i < _connections.Length; i++)
            {
                if (!_connections[i].IsCreated)
                {
                    _connections.RemoveAtSwapBack(i);
                    --i;
                }
            }
        }

        private void AcceptNewConnections()
        {
            NetworkConnection c;
            while ((c = Driver.Accept()) != default(NetworkConnection))
            {
                _connections.Add(c);
            }
        }

        private void UpdateMessagePump()
        {
            for (int i = 0; i < _connections.Length; i++)
            {
                NetworkEvent.Type cmd;
                while ((cmd = Driver.PopEventForConnection(_connections[i], out var stream)) != NetworkEvent.Type.Empty)
                {
                    switch (cmd)
                    {
                        case NetworkEvent.Type.Data:
                            NetUtility.OnData(stream, _connections[i], this);
                            break;
                        case NetworkEvent.Type.Disconnect:
                            _connections[i] = default(NetworkConnection);
                            ConnectionDropped?.Invoke();
                            Shutdown();
                            break;
                        case NetworkEvent.Type.Empty:
                        case NetworkEvent.Type.Connect:
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
        }

        public void SendToClient(NetworkConnection conn, NetMessage msg)
        {
            DataStreamWriter writer;
            Driver.BeginSend(conn, out writer);
            msg.Serialize(ref writer);
            Driver.EndSend(writer);
        }
    
        public void Broadcast(NetMessage msg)
        {
            foreach (var conn in _connections)
            {
                if (conn.IsCreated) 
                    SendToClient(conn, msg);
            }
        }
    
    

    }
}
