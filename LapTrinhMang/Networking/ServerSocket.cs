using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Text;

namespace LapTrinhMang.Networking
{
    public class ServerSocket
    {
        private TcpListener listener;
        private bool isRunning = false;

        private readonly List<TcpClient> clients = new List<TcpClient>();
        private readonly Dictionary<string, TcpClient> clientByIP = new Dictionary<string, TcpClient>();

        public event Action<string> OnClientConnected;
        public event Action<string> OnClientDisconnected;
        public event Action<string, byte[]> OnReceiveFile;
        public event Action<string, string> OnReceiveMessage;

        public void Start(int port)
        {
            listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
            isRunning = true;

            Thread acceptThread = new Thread(AcceptClients);
            acceptThread.IsBackground = true;
            acceptThread.Start();
        }

        public void Stop()
        {
            isRunning = false;

            try
            {
                listener?.Stop();
            }
            catch
            {

            }

            lock (clients)
            {
                foreach (var c in clients)
                {
                    try
                    {
                        c.Close();
                    }
                    catch
                    {

                    }
                }
                clients.Clear();
            }
        }

        //Chờ Client
        private void AcceptClients()
        {
            while (isRunning)
            {
                try
                {
                    TcpClient client = listener.AcceptTcpClient();

                    string ip = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();
                    
                    lock (clients)
                    {
                        clients.Add(client);
                        clientByIP[ip] = client;
                    }

                    OnClientConnected?.Invoke(ip);

                    Thread clientThread = new Thread(() => ClientHandler(client, ip));
                    clientThread.IsBackground = true;
                    clientThread.Start();
                }
                catch
                {
                    if (!isRunning)
                        break;
                }
            }
        }
        private string ReadHeader(NetworkStream stream)
        {
            List<byte> bytes = new List<byte>();
            while (true)
            {
                int b = stream.ReadByte();
                if (b == -1) throw new IOException("Client disconnected");
                if (b == '\n') break;
                bytes.Add((byte)b);
            }
            return Encoding.UTF8.GetString(bytes.ToArray());
            
        }
        private void ClientHandler(TcpClient client, string clientIP)
        {
            NetworkStream stream = client.GetStream();
            Console.WriteLine($"Client {clientIP} đã kết nối");
            try
            {
                while (isRunning)
                {
                    string header = ReadHeader(stream);
                    string[] parts = header.Split('|');

                    if (parts.Length < 2)
                        continue;

                    string type = parts[0];
                    int length = int.Parse(parts[1]);

                    byte[] dataBuffer = new byte[length];
                    int bytesRead = 0;

                    while (bytesRead < length)
                    {
                        int read = stream.Read(dataBuffer, bytesRead, length - bytesRead);
                        if (read <= 0)
                            break;
                        bytesRead += read;
                    }

                    if (type == "MSG")
                    {
                        string msg = Encoding.UTF8.GetString(dataBuffer);
                        OnReceiveMessage?.Invoke(clientIP, msg);
                    }
                    else if (type == "FILE")
                    {
                        OnReceiveFile?.Invoke(clientIP, dataBuffer);
                    }
                }
            }
            catch
            {

            }
            lock (clients)
            {
                clients.Remove(client);
                if (clientByIP.ContainsKey(clientIP))
                    clientByIP.Remove(clientIP);
            }

            OnClientDisconnected?.Invoke(clientIP);
        }

        public void BroadcastMessage(string msg)
        {
            byte[] msgBytes = Encoding.UTF8.GetBytes(msg);
            SendToAll("MSG", msgBytes);
        }

        public void BroadcastFile(byte[] fileBytes)
        {
            SendToAll("FILE", fileBytes);
        }
        public void BroadcastLink(string link)
        {
            byte[] data = Encoding.UTF8.GetBytes(link);
            SendToAll("LINK", data);
        }

        public void SendMessageToClient(string ip, string msg)
        {
            byte[] msgBytes = Encoding.UTF8.GetBytes(msg);
            SendToClient(ip, "MSG", msgBytes);
        }

        public void SendFileToClient(string ip, byte[] fileBytes)
        {
            SendToClient(ip, "FILE", fileBytes);
        }

        private void SendToClient(string ip, string type, byte[] data)
        {
            lock (clients)
            {
                if (!clientByIP.ContainsKey(ip))
                {
                    throw new Exception($"Không tìm thấy client với IP: {ip}");
                }

                TcpClient client = clientByIP[ip];
                try
                {
                    if (!client.Connected)
                    {
                        throw new Exception($"Client {ip} đã ngắt kết nối");
                    }

                    NetworkStream stream = client.GetStream();

                    string header = $"{type}|{data.Length}\n";
                    byte[] headerBytes = Encoding.UTF8.GetBytes(header);

                    stream.Write(headerBytes, 0, headerBytes.Length);
                    stream.Write(data, 0, data.Length);
                    stream.Flush();
                }
                catch (Exception ex)
                {
                    throw new Exception($"Lỗi khi gửi tin nhắn đến {ip}: {ex.Message}");
                }
            }
        }

        private void SendToAll(string type, byte[] data)
        {
            lock (clients)
            {
                foreach (var client in clients)
                {
                    try
                    {
                        if (!client.Connected) continue;

                        NetworkStream stream = client.GetStream();

                        string header = $"{type}|{data.Length}\n";
                        byte[] headerBytes = Encoding.UTF8.GetBytes(header);

                        stream.Write(headerBytes, 0, headerBytes.Length);
                        stream.Write(data, 0, data.Length);
                        stream.Flush();
                    }
                    catch
                    {

                    }
                }
            }
        }
    }
}
