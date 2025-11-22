using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using LapTrinhMang.Models;
using LapTrinhMang.Networking;
using System.Text.Json;
using OfficeOpenXml;

namespace LapTrinhMang
{
    public partial class Server : Form
    {
        private List<ClientInfo> dsMay;
        private ServerSocket serverSocket = new ServerSocket();
        private List<Student> dsSinhVien = new List<Student>();
        public Server()
        {
            InitializeComponent();
        }

        public Server(List<ClientInfo> ds)
        {
            InitializeComponent();
            dsMay = ds;
            LoadDanhSachMay();
        }

        private void LoadDanhSachMay()
        {
            flpnDanhSachMay.Controls.Clear();

            foreach (var may in dsMay)
            {
                if (may.IsConnected)
                {
                    var uc = new ucMayConnect();
                    uc.SetInfo(may.MSSV, may.IP);
                    flpnDanhSachMay.Controls.Add(uc);
                }
                else
                {
                    var uc = new ucMayDisconnect();
                    uc.SetInfo(may.MSSV, may.IP);
                    flpnDanhSachMay.Controls.Add(uc);
                }
            }
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            FrmNhapIP frm = new FrmNhapIP();
            frm.OnDanhSachMayCreated += (ds) =>
            {
                dsMay = ds;
                LoadDanhSachMay();
                StartServerAfterLoad();
            };
            frm.ShowDialog();
        }

        private void StartServerAfterLoad()
        {
            serverSocket.Start(8888);
            MessageBox.Show("Server đã khởi động (port 8888)!");

            // Khi client kết nối
            serverSocket.OnClientConnected += (ip) =>
            {
                Invoke(new Action(() =>
                {
                    var may = dsMay.FirstOrDefault(x => x.IP == ip);
                    if (may != null)
                    {
                        may.IsConnected = true;
                    }
                    else // Thêm máy mới vào dsMay để theo dõi
                    {
                        dsMay.Add(new ClientInfo { IP = ip, IsConnected = true, MSSV = "Mới/Chưa ĐD" });
                    }
                    try
                    {
                        if (dsSinhVien != null && dsSinhVien.Count > 0)
                        {
                            string dsSvJson = JsonSerializer.Serialize(dsSinhVien);
                            serverSocket.BroadcastMessage($"DSSV|{dsSvJson}");
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Lỗi gửi DS SV: {ex.Message}");
                    }
                    LoadDanhSachMay();
                }));
            };

            // Khi client ngắt kết nối
            serverSocket.OnClientDisconnected += (ip) =>
            {
                Invoke(new Action(() =>
                {
                    var may = dsMay.FirstOrDefault(x => x.IP == ip);
                    if (may != null)
                    {
                        may.IsConnected = false;
                        LoadDanhSachMay();
                    }

                    //MessageBox.Show("Client ngắt kết nối: " + ip);
                }));
            };

            // Khi nhận tin nhắn từ client
            serverSocket.OnReceiveMessage += (ip, msg) =>
            {
                Invoke(new Action(() =>
                {
                    // 1. Kiểm tra tin nhắn Điểm danh: Format: "DIEMDANH|{JSON_StudentInfo}"
                    if (msg.StartsWith("DIEMDANH|"))
                    {
                        string mssv = msg.Split(new[] { '|' }, 2)[1].Trim();

                        if (string.IsNullOrEmpty(mssv))
                        {
                            MessageBox.Show("Gói điểm danh không hợp lệ (MSSV rỗng).");
                            return;
                        }

                        // Tìm sinh viên trong danh sách sinh viên
                        var sv = dsSinhVien?.FirstOrDefault(x => x.MSSV == mssv);

                        // Tìm máy theo IP đang kết nối
                        var may = dsMay.FirstOrDefault(x => x.IP == ip);
                        if (may != null)
                        {
                            may.MSSV = mssv;
                            may.IsConnected = true;
                        }

                        if (sv != null)
                        {
                            MessageBox.Show($"Sinh viên {sv.HoTen} ({sv.MSSV} - {sv.Lop}) đã điểm danh tại IP {ip}!");
                        }
                        else
                        {
                            MessageBox.Show($"MSSV {mssv} đã điểm danh tại IP {ip} (không tìm thấy trong danh sách).");
                        }

                        LoadDanhSachMay();
                    }
                    else
                    {
                        MessageBox.Show($"[{ip}] gửi: {msg}");
                    }
                }));
            };

            // Khi nhận file
            serverSocket.OnReceiveFile += (ip, bytes) =>
            {
                Invoke(new Action(() =>
                {
                    string path = "bai_" + ip.Replace(".", "_") + ".zip";
                    System.IO.File.WriteAllBytes(path, bytes);
                    //MessageBox.Show($"Đã nhận file từ {ip}");
                }));
            };
        }

        private List<Student> ReadDanhSachSinhVien(string filePath)
        {
            List<Student> list = new List<Student>();
            string ext = Path.GetExtension(filePath).ToLower();

            if (ext == ".txt")
            {
                var lines = File.ReadAllLines(filePath);

                // bỏ dòng tiêu đề, bắt đầu từ dòng 2
                for (int i = 1; i < lines.Length; i++)
                {
                    if (string.IsNullOrWhiteSpace(lines[i])) continue;

                    var parts = lines[i].Split(',');
                    if (parts.Length >= 3)
                    {
                        list.Add(new Student
                        {
                            MSSV = parts[0].Trim(),
                            HoTen = parts[1].Trim(),
                            Lop = parts[2].Trim()
                        });
                    }
                }
            }
            else if (ext == ".xlsx")
            {
                OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

                using (var package = new ExcelPackage(new FileInfo(filePath)))
                {
                    var ws = package.Workbook.Worksheets[0];
                    int row = 2; // bỏ dòng tiêu đề

                    while (ws.Cells[row, 1].Value != null)
                    {
                        list.Add(new Student
                        {
                            MSSV = ws.Cells[row, 1].Text.Trim(),
                            HoTen = ws.Cells[row, 2].Text.Trim(),
                            Lop = ws.Cells[row, 3].Text.Trim()
                        });
                        row++;
                    }
                }
            }

            return list;
        }

        private void btnLayDS_Click(object sender, EventArgs e)
        {
            if (dsMay == null || dsMay.Count == 0)
            {
                MessageBox.Show("Bạn phải tạo danh sách máy trước");
                return;
            }

            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Text or Excel|*.txt;*.xlsx";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                dsSinhVien = ReadDanhSachSinhVien(ofd.FileName);

                if (dsSinhVien.Count == 0)
                {
                    MessageBox.Show("Không đọc được sinh viên nào từ file!");
                    return;
                }

                if (dsSinhVien.Count > dsMay.Count)
                {
                    MessageBox.Show("Số sinh viên nhiều hơn số máy! Không thể ghép.");
                    return;
                }

                LoadDanhSachMay();
                MessageBox.Show("Đã đọc danh sách sinh viên!");

                try
                {
                    string json = JsonSerializer.Serialize(dsSinhVien);
                    serverSocket.BroadcastMessage("DSSV|" + json);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi gửi danh sách sinh viên cho Client: " + ex.Message);
                }
            }
        }
    }
}
