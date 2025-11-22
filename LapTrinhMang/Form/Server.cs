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
                        string dsSvJson = JsonSerializer.Serialize(dsMay);
                        serverSocket.BroadcastMessage($"DSSV|{dsSvJson}");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Lỗi gửi DS SV: {ex.Message}");
                    }
                    LoadDanhSachMay();
                    //MessageBox.Show("Client đã kết nối: " + ip);
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
                        string studentJson = msg.Split(new[] { '|' }, 2)[1];
                        // Yêu cầu thư viện System.Text.Json hoặc Newtonsoft.Json
                        // Giả định bạn có thể Deserialize studentJson thành StudentInfo object.

                        // VÍ DỤ ĐƠN GIẢN: CHỈ LẤY MSSV TỪ JSON
                        // Để đơn giản, ta chỉ cần MSSV:
                        string mssv = "MSSV_TEMP"; // Cần parse JSON để lấy MSSV thực

                        // Tìm sinh viên trong danh sách gốc (dsMay) bằng MSSV
                        var svTrongDS = dsMay.FirstOrDefault(x => x.MSSV == mssv);

                        if (svTrongDS != null)
                        {
                            // Cập nhật thông tin cho máy đó
                            svTrongDS.IP = ip; // Lưu IP tạm thời của máy đang kết nối
                            svTrongDS.IsConnected = true;
                            // Nếu muốn lưu HoTen, Lop vào ClientInfo (nếu ClientInfo có các thuộc tính này)

                            MessageBox.Show($"Sinh viên {mssv} đã điểm danh tại IP {ip}!");
                        }
                        else
                        {
                            // Xử lý trường hợp MSSV không có trong danh sách gốc
                            MessageBox.Show($"MSSV {mssv} không hợp lệ hoặc đã điểm danh!");
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

        private List<ClientInfo> ReadDanhSachSinhVien(string filePath)
        {
            List<ClientInfo> list = new List<ClientInfo>();
            string ext = Path.GetExtension(filePath).ToLower();

            if (ext == ".txt")
            {
                var lines = File.ReadAllLines(filePath);

                for (int i = 1; i < lines.Length; i++)
                {
                    var parts = lines[i].Split(',');
                    if (parts.Length >= 3)
                    {
                        list.Add(new ClientInfo()
                        {
                            MSSV = parts[0].Trim(),
                            IP = "Chưa điểm danh",
                            IsConnected = false
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
                    int row = 2;

                    while (ws.Cells[row, 1].Value != null)
                    {
                        string mssv = ws.Cells[row, 1].Text;

                        list.Add(new ClientInfo()
                        {
                            MSSV = mssv,
                            IP = "Chưa điểm danh",
                            IsConnected = false
                        });

                        row++;
                    }
                }
            }
            return list;
        }

        private void btnLayDS_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Text or Excel|*.txt;*.xlsx";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                dsMay = ReadDanhSachSinhVien(ofd.FileName);
                LoadDanhSachMay();

                MessageBox.Show("Đã tải danh sách thành công!");
            }
        }
    }
}
