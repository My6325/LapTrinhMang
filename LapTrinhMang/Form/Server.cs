using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using DocumentFormat.OpenXml.Spreadsheet;
using LapTrinhMang.Models;
using LapTrinhMang.Networking;
using Excel = Microsoft.Office.Interop.Excel;

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
                string hoTen = string.IsNullOrEmpty(may.HoTen) ? may.MSSV : $"{may.HoTen} ({may.MSSV})";

                if (may.IsConnected)
                {
                    var uc = new ucMayConnect();
                    uc.SetInfo(may.MSSV, may.IP, hoTen);
                    flpnDanhSachMay.Controls.Add(uc);
                }
                else
                {
                    var uc = new ucMayDisconnect();
                    uc.SetInfo(may.MSSV, may.IP, hoTen);
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
                        dsMay.Add(new ClientInfo { IP = ip, IsConnected = true, MSSV = "Mới/Chưa ĐD", HoTen = "Máy mới kết nối" });
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
                            may.HoTen = sv != null ? sv.HoTen : "Không tìm thấy tên";
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
            else if (ext == ".xlsx" || ext == ".xls") 
            {
                Excel.Application app = new Excel.Application();
                Excel.Workbook wb = null;
                Excel._Worksheet sheet = null;
                Excel.Range range = null;

                try
                {
                    app = new Excel.Application();
                    wb = app.Workbooks.Open(filePath);
                    sheet = wb.Sheets[1];
                    range = sheet.UsedRange;

                    for (int row = 2; row <= range.Rows.Count; row++)
                    {
                        // Đọc cột 1, 2, 3 tương ứng với MSSV, HoTen, Lop
                        string mssv = (range.Cells[row, 1] as Excel.Range)?.Text;
                        string hoTen = (range.Cells[row, 2] as Excel.Range)?.Text;
                        string lop = (range.Cells[row, 3] as Excel.Range)?.Text;

                        // Chỉ thêm vào danh sách nếu MSSV không rỗng
                        if (!string.IsNullOrWhiteSpace(mssv))
                        {
                            list.Add(new Student
                            {
                                MSSV = mssv.Trim(),
                                HoTen = hoTen != null ? hoTen.Trim() : "",
                                Lop = lop != null ? lop.Trim() : ""
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi đọc file Excel: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    list.Clear(); // Xóa danh sách nếu có lỗi
                }
                finally
                {
                    // Đảm bảo đóng và thoát Excel
                    if (range != null)
                    {
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(range);
                    }
                    if (sheet != null)
                    {
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(sheet);
                    }

                    if (wb != null)
                    {
                        wb.Close(false);
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(wb);
                    }

                    if (app != null)
                    {
                        app.Quit();
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(app);
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
