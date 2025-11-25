using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Client.Networking
{
    public class ClientSocket
    {
        private TcpClient client;
        private NetworkStream stream;
        private bool isConnected = false;

        public bool IsConnected => isConnected;

        public event Action<byte[]> OnReceiveFile;
        public event Action<string> OnReceiveMessage;
        public event Action OnDisconnected;

        public bool Connect(string ip, int port)
        {
            try
            {
                client = new TcpClient();
                client.Connect(ip, port);

                stream = client.GetStream();
                isConnected = true;

                Thread listenThread = new Thread(Listen);
                listenThread.IsBackground = true;
                listenThread.Start();

                return true;
            }
            catch
            {
                return false;
            }
        }

        private void Listen()
        {
            try
            {
                while (isConnected)
                {
                    string header = ReadHeader(stream); 

                    if (string.IsNullOrEmpty(header))
                    {
                        break;
                    }
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
                        OnReceiveMessage?.Invoke(msg);
                    }
                    else if (type == "FILE")
                    {
                        OnReceiveFile?.Invoke(dataBuffer);
                    }
                }
            }
            catch
            {

            }
            Disconnect();
        }

        public void SendMessage(string msg)
        {
            if (!isConnected)
                return;

            try
            {
                byte[] msgBytes = Encoding.UTF8.GetBytes(msg);
                SendPacket("MSG", msgBytes);
            }
            catch
            {

            }
        }

        public void SendFile(byte[] fileBytes)
        {
            if (!isConnected)
                return;

            try
            {
                SendPacket("FILE", fileBytes);
            }
            catch
            {

            }
        }

        private void SendPacket(string type, byte[] data)
        {
            string header = $"{type}|{data.Length}\n";
            byte[] headerBytes = Encoding.UTF8.GetBytes(header);

            stream.Write(headerBytes, 0, headerBytes.Length);
            stream.Write(data, 0, data.Length);
        }

        public void Disconnect()
        {
            if (!isConnected)
                return;

            isConnected = false;

            try
            {
                stream?.Close();
            }
            catch
            {

            }

            try
            {
                client?.Close();
            }
            catch
            {

            }
            OnDisconnected?.Invoke();
        }
        private string ReadHeader(NetworkStream stream)
        {
            List<byte> bytes = new List<byte>();

            try
            {
                while (true)
                {
                    // Kiểm tra xem stream còn mở không
                    if (!client.Connected)
                    {
                        Console.WriteLine("Client đã ngắt kết nối");
                        return null;
                    }

                    // Kiểm tra có dữ liệu không, nếu không thì đợi
                    while (!stream.DataAvailable)
                    {
                        Thread.Sleep(50); 
                        if (!client.Connected || !isConnected)
                            return null;
                    }

                    int b = stream.ReadByte();

                    if (b == -1)
                    {
                        Console.WriteLine("Stream trả về -1");
                        return null;
                    }

                    if (b == '\n')
                        break; // Ký tự kết thúc Header

                    bytes.Add((byte)b);
                }

                string result = Encoding.UTF8.GetString(bytes.ToArray()).Trim();
                Console.WriteLine($"Nhận header: {result}");
                return result;
            }
            catch (IOException ex)
            {
                Console.WriteLine($"IOException trong ReadHeader: {ex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi trong ReadHeader: {ex.Message}");
                return null;
            }
        }
    }
}
