using System;
using Net.NetMessages;
using Unity.Collections;
using Unity.Networking.Transport;
using UnityEngine;

namespace Net
{
    public class Client : MonoBehaviour
    {
    
        #region Singleton Implementation

        public static Client Instance {set; get;}
        private void Awake()
        {
            Instance = this;
        }

        #endregion
    
        public NetworkDriver Driver;
        private NetworkConnection _connection;

        private bool _isActive = false;

        public Action ConnectionDropped;
    
        public void Init(string ip, ushort port)
        {
            Driver = NetworkDriver.Create();
            NetworkEndpoint endpoint = NetworkEndpoint.Parse(ip, port);

            _connection = Driver.Connect(endpoint);
            _isActive = true;

            RegisterToEvent();
            Debug.Log("Client initialized");
        }

        public void Shutdown()
        {
            if (_isActive)
            {
                UnregisterToEvent();
                Driver.Dispose();
                _connection = default(NetworkConnection);
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
        
            Driver.ScheduleUpdate().Complete();

            CheckAlive();
        
            UpdateMessagePump();
        }

        private void CheckAlive()
        {
            if (!_connection.IsCreated && _isActive)
            {
                ConnectionDropped?.Invoke();
                Shutdown();
            }
        }
    
        private void UpdateMessagePump()
        {

            NetworkEvent.Type cmd;
            while ((cmd = _connection.PopEvent(Driver, out var stream)) != NetworkEvent.Type.Empty)
            {
                switch (cmd)
                {
                    case NetworkEvent.Type.Connect:
                        SendToServer(new NetWelcome());
                        break;
                    case NetworkEvent.Type.Data:
                        NetUtility.OnData(stream, default(NetworkConnection));
                        break;
                    case NetworkEvent.Type.Disconnect:
                        _connection = default(NetworkConnection);
                        ConnectionDropped?.Invoke();
                        Shutdown();
                        break;
                }
            }
        
        }

        public void SendToServer(NetMessage msg)
        {
            DataStreamWriter writer;
            Driver.BeginSend(_connection, out writer);
            msg.Serialize(ref writer);
            Driver.EndSend(writer);
        
        }

        private void RegisterToEvent()
        {
            NetUtility.ClientKeepAlive += OnKeepAlive;
        }
    
        private void UnregisterToEvent()
        {
            NetUtility.ClientKeepAlive -= OnKeepAlive;
        }
    
        private void OnKeepAlive(NetMessage msg)
        {
            SendToServer(msg);
        }

    }
}
