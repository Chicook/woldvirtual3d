using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace WoldVirtual3D.Viewer.Services
{
    /// <summary>
    /// Servidor interno para comunicación con Godot
    /// Responsabilidad: Gestionar comunicación IPC entre el visor y Godot
    /// </summary>
    public class InternalServer : IDisposable
    {
        private TcpListener? tcpListener;
        private bool isRunning = false;
        private int port = 5000;

        /// <summary>
        /// Inicia el servidor interno
        /// </summary>
        public async Task<bool> StartAsync(int serverPort = 5000)
        {
            port = serverPort;

            try
            {
                tcpListener = new TcpListener(IPAddress.Loopback, port);
                tcpListener.Start();
                isRunning = true;

                // Iniciar loop de aceptación de conexiones
                _ = AcceptConnectionsAsync();

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al iniciar servidor: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Acepta conexiones de forma asíncrona
        /// </summary>
        private async Task AcceptConnectionsAsync()
        {
            while (isRunning && tcpListener != null)
            {
                try
                {
                    var client = await tcpListener.AcceptTcpClientAsync();
                    _ = HandleClientAsync(client);
                }
                catch (ObjectDisposedException)
                {
                    // El listener fue cerrado
                    break;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error al aceptar conexión: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Maneja un cliente conectado
        /// </summary>
        private async Task HandleClientAsync(TcpClient client)
        {
            try
            {
                using (client)
                {
                    var stream = client.GetStream();
                    var buffer = new byte[4096];

                    while (client.Connected)
                    {
                        var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                        
                        if (bytesRead == 0)
                            break;

                        // Procesar mensaje recibido
                        var message = System.Text.Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        await ProcessMessageAsync(message);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al manejar cliente: {ex.Message}");
            }
        }

        /// <summary>
        /// Procesa un mensaje recibido
        /// </summary>
        private async Task ProcessMessageAsync(string message)
        {
            // Aquí se procesarían los mensajes de Godot
            System.Diagnostics.Debug.WriteLine($"[Server] Mensaje recibido: {message}");
            await Task.CompletedTask;
        }

        /// <summary>
        /// Detiene el servidor
        /// </summary>
        public void Stop()
        {
            isRunning = false;
            tcpListener?.Stop();
        }

        public void Dispose()
        {
            Stop();
            tcpListener?.Stop();
        }
    }
}

