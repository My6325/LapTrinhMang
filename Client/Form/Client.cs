using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Client.Models;
using Client.Networking;
using System.Text.Json;
using System.IO;
using System.IO.Compression;
using System.Threading;

namespace Client
{
    public partial class Client : Form
    {
        private ClientSocket socket = new ClientSocket();
        private StudentInfo selectedStudent;
        private int serverPort = 8888;

        private List<StudentInfo> dsSinhVienClient = new List<StudentInfo>();
        private bool eventsRegistered = false;
        private string currentFileName = "";
        private string currentSavePath = "";

        private bool IsConnected => socket.IsConnected;

        public Client()
        {
            InitializeComponent();
            LoadInitialData();
        }

        private void LoadInitialData()
        {
            txtIP.Text = GetLocalIPAddress();

            cbTTSV.DataSource = null;

            UpdateUI(false);
            currentSavePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "DeThiBaiLam");
           // Console.WriteLine($"Đề thi sẽ được lưu tại: {deThiPath}");
        }

        private void UpdateUI(bool connected)
        {
            btnConnect.Text = connected ? "Disconnect" : "Connect";
            btnĐiemDanh.Enabled = connected;
            txtIP.Enabled = !connected;

            // Chỉ cho phép chọn TTSV khi đã kết nối và chưa điểm danh
            cbTTSV.Enabled = connected;
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            string ip = txtIP.Text.Trim();
            int port = serverPort;

            if (string.IsNullOrEmpty(ip))
            {
                MessageBox.Show("Vui lòng nhập IP Server!");
                return;
            }

            if (IsConnected)
            {
                socket.Disconnect();
                UpdateUI(false);
                return;
            }

            if (!eventsRegistered)
            {
                socket.OnReceiveMessage += (msg) =>
                {
                    if (msg.StartsWith("DSSV|"))
                    {
                        string json = msg.Substring("DSSV|".Length);

                        try
                        {
                            var ds = JsonSerializer.Deserialize<List<StudentInfo>>(json);
                            if (ds != null)
                            {
                                Invoke(new Action(() =>
                                {
                                    dsSinhVienClient = ds;
                                    cbTTSV.DataSource = null;
                                    cbTTSV.DataSource = dsSinhVienClient;
                                    cbTTSV.DisplayMember = "HoTen";
                                    cbTTSV.ValueMember = "MSSV";
                                }));
                            }
                        }
                        catch (Exception ex)
                        {
                            Invoke(new Action(() =>
                            {
                                MessageBox.Show("Lỗi parse DSSV từ server: " + ex.Message);
                            }));
                        }
                    }
                    else if (msg.StartsWith("SAVEPATH|")) 
                    {
                        currentSavePath = msg.Substring("SAVEPATH|".Length).Trim();
                        Invoke(new Action(() =>
                        {
                            Console.WriteLine($"Đã nhận đường dẫn lưu: {currentSavePath}");
                        }));
                    }
                    else if (msg.StartsWith("FILENAME|"))
                    {
                        // Lưu tên file để dùng khi nhận file
                        currentFileName = msg.Substring("FILENAME|".Length);
                        Invoke(new Action(() =>
                        {
                            Console.WriteLine($"Chuẩn bị nhận file: {currentFileName}");
                        }));
                    }
                    else if (msg == "YEUCAU_NOPBAI")
                    {
                        Invoke(new Action(() =>
                        {
                            SendBaiLamToServer();
                        }));
                    }
                    else if (msg.StartsWith("BATDAU|"))
                    {
                        int soPhut = int.Parse(msg.Split('|')[1]);
                        Invoke(new Action(() =>
                        {
                            MessageBox.Show($"Bài thi bắt đầu! Thời gian: {soPhut} phút", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }));
                    }
                    else if (msg.StartsWith("TIME|"))
                    {
                        double seconds = double.Parse(msg.Split('|')[1]);
                        int phut = (int)(seconds / 60);
                        int giay = (int)(seconds % 60);
                    }
                    else if (msg == "HETGIO")
                    {
                        Invoke(new Action(() =>
                        {
                            MessageBox.Show("Đã hết thời gian làm bài!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }));
                    }
                    else
                    {
                        Invoke(new Action(() =>
                        {
                            MessageBox.Show("Server gửi: " + msg);
                        }));
                    }
                };

                socket.OnReceiveFile += (fileBytes) =>
                {
                    Invoke(new Action(() =>
                    {
                        try
                        {
                            string folder = string.IsNullOrEmpty(currentSavePath)
                                ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "DefaultDeThi") // Dùng Documents làm dự phòng
                                : currentSavePath;
                            if (!Directory.Exists(folder))
                                Directory.CreateDirectory(folder);

                            // Lưu file với tên đã nhận hoặc tên mặc định
                            string fileName = string.IsNullOrEmpty(currentFileName)
                                ? $"DeThi_{DateTime.Now:yyyyMMdd_HHmmss}.pdf"
                                : currentFileName;

                            string filePath = Path.Combine(folder, fileName);
                            File.WriteAllBytes(filePath, fileBytes);

                            currentFileName = "";
                            //currentSavePath = "";
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Lỗi khi lưu file: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }));
                };

                socket.OnDisconnected += () =>
                {
                    Invoke(new Action(() =>
                    {
                        MessageBox.Show("Mất kết nối Server!");
                        UpdateUI(false);
                    }));
                };

                eventsRegistered = true;
            }

            // 2SAU KHI GẮN EVENT MỚI CONNECT
            bool ok = socket.Connect(ip, port);
            if (ok)
            {
                MessageBox.Show("Kết nối server thành công!");
                UpdateUI(true);
            }
            else
            {
                MessageBox.Show("Kết nối thất bại! Hãy kiểm tra IP hoặc Server chưa chạy.");
                return;
            }
        }

        private void btnĐiemDanh_Click(object sender, EventArgs e)
        {
            if (selectedStudent == null)
            {
                MessageBox.Show("Vui lòng chọn tên sinh viên trước khi điểm danh.", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!IsConnected)
            {
                MessageBox.Show("Chưa kết nối Server.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                // Gửi MSSV đã chọn lên Server
                string mssv = selectedStudent.MSSV;
                socket.SendMessage($"DIEMDANH|{mssv}");

                MessageBox.Show($"Đã gửi điểm danh cho {selectedStudent.HoTen} ({mssv})!");
                btnĐiemDanh.Enabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi gửi điểm danh: {ex.Message}");
            }
        }

        private void cbTTSV_SelectedIndexChanged(object sender, EventArgs e)
        {
            selectedStudent = cbTTSV.SelectedItem as StudentInfo;
            if (selectedStudent != null)
            {
                txtLop.Text = selectedStudent.Lop;
                txtMSSV.Text = selectedStudent.MSSV;
                txtHoTen.Text = selectedStudent.HoTen;
            }
        }

        private string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            return "127.0.0.1";
        }
        private void SendBaiLamToServer()
        {
            if (selectedStudent == null)
            {
                MessageBox.Show("Chưa chọn thông tin sinh viên để nộp bài", "Lỗi Nộp bài", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (!IsConnected) return;

            string sourceFolder = currentSavePath;
            string zipFileName = $"{selectedStudent.MSSV}_{DateTime.Now:yyyyMMdd_HHmmss}.zip";

            // Tạo đường dẫn file ZIP tạm thời (sử dụng thư mục Temp của hệ thống)
            string tempZipPath = Path.Combine(Path.GetTempPath(), zipFileName);

            if (string.IsNullOrEmpty(sourceFolder) || !Directory.Exists(sourceFolder))
            {
                MessageBox.Show($"Thư mục bài làm không tồn tại", "Lỗi Nén", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                if (File.Exists(tempZipPath)) File.Delete(tempZipPath);

                // Thực hiện nén
                ZipFile.CreateFromDirectory(sourceFolder, tempZipPath, CompressionLevel.Fastest, false);
                socket.SendMessage($"NOPBAI_FILENAME|{zipFileName}");
                Thread.Sleep(300);

                //Gửi nội dung file ZIP
                byte[] fileBytes = File.ReadAllBytes(tempZipPath);
                socket.SendFile(fileBytes);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi gửi bài làm: {ex.Message}", "Lỗi ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            finally
            {
                if (File.Exists(tempZipPath)) File.Delete(tempZipPath);
            }
        }
    }
}
