using Godot;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Threading;

namespace WoldVirtual3D.Viewer
{
    /// <summary>
    /// Sistema de red P2P distribuida
    /// Responsabilidad: Gestión de conexiones peer-to-peer y sincronización
    /// </summary>
    public partial class P2PNetwork : Node
    {
        private UdpClient _udpClient;
        private TcpListener _tcpListener;
        private Dictionary<string, P2PNode> _connectedNodes;
        private string _localNodeId;
        private string _localUserId;
        private int _udpPort = 7777;
        private int _tcpPort = 7778;
        private bool _isRunning = false;
        private Thread _networkThread;

        public event Action<P2PNode> OnNodeConnected;
        public event Action<string> OnNodeDisconnected;
        public event Action<P2PMessage> OnMessageReceived;

        public Dictionary<string, P2PNode> ConnectedNodes => _connectedNodes;
        public bool IsRunning => _isRunning;
        public string LocalNodeId => _localNodeId;

        public override void _Ready()
        {
            base._Ready();
            _connectedNodes = new Dictionary<string, P2PNode>();
            _localNodeId = Guid.NewGuid().ToString();
            GD.Print($"P2PNetwork: Nodo local inicializado con ID: {_localNodeId}");
        }

        /// <summary>
        /// Inicializa la red P2P para un usuario
        /// </summary>
        public async Task InitializeAsync(string userId)
        {
            if (_isRunning)
            {
                GD.Print("P2PNetwork: Red ya está inicializada");
                return;
            }

            _localUserId = userId;
            GD.Print($"P2PNetwork: Inicializando red P2P para usuario: {userId}");

            try
            {
                await StartNetworkServices();
                _isRunning = true;
                GD.Print("P2PNetwork: Red P2P inicializada correctamente");
            }
            catch (Exception ex)
            {
                GD.PrintErr($"P2PNetwork: Error al inicializar red: {ex.Message}");
            }
        }

        private async Task StartNetworkServices()
        {
            await Task.Run(() =>
            {
                try
                {
                    // Iniciar servidor UDP para descubrimiento
                    _udpClient = new UdpClient(_udpPort);
                    GD.Print($"P2PNetwork: Servidor UDP iniciado en puerto {_udpPort}");

                    // Iniciar servidor TCP para conexiones estables
                    _tcpListener = new TcpListener(IPAddress.Any, _tcpPort);
                    _tcpListener.Start();
                    GD.Print($"P2PNetwork: Servidor TCP iniciado en puerto {_tcpPort}");

                    // Iniciar hilo de red
                    _networkThread = new Thread(NetworkLoop);
                    _networkThread.IsBackground = true;
                    _networkThread.Start();
                }
                catch (Exception ex)
                {
                    GD.PrintErr($"P2PNetwork: Error al iniciar servicios: {ex.Message}");
                    throw;
                }
            });
        }

        private void NetworkLoop()
        {
            while (_isRunning)
            {
                try
                {
                    // Procesar mensajes UDP
                    if (_udpClient != null && _udpClient.Available > 0)
                    {
                        ProcessUdpMessages();
                    }

                    // Aceptar conexiones TCP
                    if (_tcpListener != null && _tcpListener.Pending())
                    {
                        ProcessTcpConnection();
                    }

                    // Mantener conexiones activas
                    MaintainConnections();

                    Thread.Sleep(100); // Evitar uso excesivo de CPU
                }
                catch (Exception ex)
                {
                    GD.PrintErr($"P2PNetwork: Error en loop de red: {ex.Message}");
                    Thread.Sleep(1000);
                }
            }
        }

        private void ProcessUdpMessages()
        {
            try
            {
                IPEndPoint remoteEndPoint = null;
                var data = _udpClient.Receive(ref remoteEndPoint);
                
                if (data != null && data.Length > 0)
                {
                    var message = System.Text.Encoding.UTF8.GetString(data);
                    HandleNetworkMessage(message, remoteEndPoint);
                }
            }
            catch (Exception ex)
            {
                GD.PrintErr($"P2PNetwork: Error al procesar mensaje UDP: {ex.Message}");
            }
        }

        private void ProcessTcpConnection()
        {
            try
            {
                var client = _tcpListener.AcceptTcpClient();
                GD.Print($"P2PNetwork: Nueva conexión TCP desde: {client.Client.RemoteEndPoint}");
                
                // Manejar conexión en hilo separado
                Task.Run(() => HandleTcpClient(client));
            }
            catch (Exception ex)
            {
                GD.PrintErr($"P2PNetwork: Error al aceptar conexión TCP: {ex.Message}");
            }
        }

        private void HandleTcpClient(TcpClient client)
        {
            try
            {
                var stream = client.GetStream();
                var buffer = new byte[4096];
                
                while (client.Connected)
                {
                    var bytesRead = stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead > 0)
                    {
                        var message = System.Text.Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        var remoteEndPoint = (IPEndPoint)client.Client.RemoteEndPoint;
                        HandleNetworkMessage(message, remoteEndPoint);
                    }
                }
            }
            catch (Exception ex)
            {
                GD.PrintErr($"P2PNetwork: Error al manejar cliente TCP: {ex.Message}");
            }
            finally
            {
                client.Close();
            }
        }

        private void HandleNetworkMessage(string message, IPEndPoint endPoint)
        {
            try
            {
                // Parsear mensaje JSON básico
                var messageData = ParseNetworkMessage(message);
                
                if (messageData != null)
                {
                    switch (messageData.Type)
                    {
                        case "discovery":
                            HandleDiscoveryMessage(messageData, endPoint);
                            break;
                        case "connect":
                            HandleConnectMessage(messageData, endPoint);
                            break;
                        case "data":
                            HandleDataMessage(messageData);
                            break;
                        case "disconnect":
                            HandleDisconnectMessage(messageData);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                GD.PrintErr($"P2PNetwork: Error al manejar mensaje: {ex.Message}");
            }
        }

        private P2PMessage ParseNetworkMessage(string jsonMessage)
        {
            try
            {
                var json = new Godot.Collections.Dictionary<string, Variant>();
                var jsonParse = Json.ParseString(jsonMessage);
                
                if (jsonParse != null && jsonParse.AsGodotDictionary() != null)
                {
                    json = jsonParse.AsGodotDictionary();
                    
                    return new P2PMessage
                    {
                        MessageId = json.ContainsKey("message_id") ? json["message_id"].AsString() : "",
                        FromNodeId = json.ContainsKey("from_node_id") ? json["from_node_id"].AsString() : "",
                        ToNodeId = json.ContainsKey("to_node_id") ? json["to_node_id"].AsString() : "",
                        Type = json.ContainsKey("type") ? json["type"].AsString() : "",
                        Data = json.ContainsKey("data") ? json["data"].AsString() : "",
                        Timestamp = DateTime.UtcNow
                    };
                }
            }
            catch (Exception ex)
            {
                GD.PrintErr($"P2PNetwork: Error al parsear mensaje: {ex.Message}");
            }

            return null;
        }

        private void HandleDiscoveryMessage(P2PMessage message, IPEndPoint endPoint)
        {
            // Responder con información del nodo local
            var response = new Godot.Collections.Dictionary<string, Variant>
            {
                { "type", "discovery_response" },
                { "from_node_id", _localNodeId },
                { "node_id", _localNodeId },
                { "user_id", _localUserId },
                { "port", _tcpPort }
            };

            SendMessage(response, endPoint);
        }

        private void HandleConnectMessage(P2PMessage message, IPEndPoint endPoint)
        {
            var nodeId = message.FromNodeId;
            
            if (!_connectedNodes.ContainsKey(nodeId))
            {
                var node = new P2PNode
                {
                    NodeId = nodeId,
                    UserId = message.Data, // Asumimos que data contiene user_id
                    IpAddress = endPoint.Address.ToString(),
                    Port = endPoint.Port,
                    LastSeen = DateTime.UtcNow,
                    IsConnected = true
                };

                _connectedNodes[nodeId] = node;
                OnNodeConnected?.Invoke(node);
                
                GD.Print($"P2PNetwork: Nodo conectado: {nodeId} desde {endPoint}");
            }
        }

        private void HandleDataMessage(P2PMessage message)
        {
            OnMessageReceived?.Invoke(message);
        }

        private void HandleDisconnectMessage(P2PMessage message)
        {
            var nodeId = message.FromNodeId;
            
            if (_connectedNodes.ContainsKey(nodeId))
            {
                _connectedNodes.Remove(nodeId);
                OnNodeDisconnected?.Invoke(nodeId);
                GD.Print($"P2PNetwork: Nodo desconectado: {nodeId}");
            }
        }

        private void MaintainConnections()
        {
            var nodesToRemove = new List<string>();
            var now = DateTime.UtcNow;

            foreach (var kvp in _connectedNodes)
            {
                var node = kvp.Value;
                var timeSinceLastSeen = now - node.LastSeen;

                // Desconectar si no se ha visto en 30 segundos
                if (timeSinceLastSeen.TotalSeconds > 30)
                {
                    nodesToRemove.Add(kvp.Key);
                }
            }

            foreach (var nodeId in nodesToRemove)
            {
                _connectedNodes.Remove(nodeId);
                OnNodeDisconnected?.Invoke(nodeId);
            }
        }

        private void SendMessage(Godot.Collections.Dictionary<string, Variant> message, IPEndPoint endPoint)
        {
            try
            {
                var json = Json.Stringify(message);
                var data = System.Text.Encoding.UTF8.GetBytes(json);
                _udpClient?.Send(data, data.Length, endPoint);
            }
            catch (Exception ex)
            {
                GD.PrintErr($"P2PNetwork: Error al enviar mensaje: {ex.Message}");
            }
        }

        /// <summary>
        /// Envía un mensaje a un nodo específico
        /// </summary>
        public void SendMessageToNode(string nodeId, string messageType, string data)
        {
            if (!_connectedNodes.ContainsKey(nodeId))
            {
                GD.PrintErr($"P2PNetwork: Nodo no encontrado: {nodeId}");
                return;
            }

            var node = _connectedNodes[nodeId];
            var endPoint = new IPEndPoint(IPAddress.Parse(node.IpAddress), node.Port);

            var message = new Godot.Collections.Dictionary<string, Variant>
            {
                { "type", messageType },
                { "from_node_id", _localNodeId },
                { "to_node_id", nodeId },
                { "data", data },
                { "timestamp", DateTime.UtcNow.ToString() }
            };

            SendMessage(message, endPoint);
        }

        /// <summary>
        /// Desconecta la red P2P
        /// </summary>
        public void Disconnect()
        {
            if (!_isRunning)
            {
                return;
            }

            GD.Print("P2PNetwork: Desconectando red P2P...");

            _isRunning = false;

            _networkThread?.Join(1000);

            _udpClient?.Close();
            _tcpListener?.Stop();

            _connectedNodes.Clear();

            GD.Print("P2PNetwork: Red P2P desconectada");
        }

        public override void _ExitTree()
        {
            Disconnect();
            base._ExitTree();
        }
    }
}

