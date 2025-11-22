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

                    lock (clients)
                        clients.Add(client);

                    string ip = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();
                    OnClientConnected?.Invoke(ip);

                    Thread clientThread = new Thread(() => ClientHandler(client));
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

        private void ClientHandler(TcpClient client)
        {
            NetworkStream stream = client.GetStream();
            string clientIP = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();

            try
            {
                while (isRunning)
                {
                    byte[] headerBuffer = new byte[200];
                    int headerBytes = stream.Read(headerBuffer, 0, headerBuffer.Length);
                    if (headerBytes <= 0)
                        break;

                    string header = Encoding.UTF8.GetString(headerBuffer, 0, headerBytes);
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

        private void SendToAll(string type, byte[] data)
        {
            lock (clients)
            {
                foreach (var client in clients)
                {
                    try
                    {
                        NetworkStream stream = client.GetStream();

                        string header = $"{type}|{data.Length}";
                        byte[] headerBytes = Encoding.UTF8.GetBytes(header);

                        stream.Write(headerBytes, 0, headerBytes.Length);
                        stream.Write(data, 0, data.Length);
                    }
                    catch
                    {

                    }
                }
            }
        }
    }
}
