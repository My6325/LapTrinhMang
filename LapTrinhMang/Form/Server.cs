using DocumentFormat.OpenXml.Spreadsheet;
using LapTrinhMang.Models;
using LapTrinhMang.Networking;
using LapTrinhMang.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Excel = Microsoft.Office.Interop.Excel;

namespace LapTrinhMang
{
    public partial class Server : Form
    {
        private List<ClientInfo> dsMay;
        private ServerSocket serverSocket = new ServerSocket();
        private List<Student> dsSinhVien = new List<Student>();
        private Dictionary<string, string> duongDanDeThi = new Dictionary<string, string>();//ListBox hiển thị tên file, Dictionary lưu đường dẫn thật để gửi
        private Dictionary<string, string> duongDanBaiLam = new Dictionary<string, string>();
        private Dictionary<string, string> copyDataTarget = new Dictionary<string, string>(); // IP nguồn -> IP đích cho copy data 
        private TimeSpan thoiGianConLai;
        private System.Windows.Forms.Timer timerDemNguoc;

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
                    uc.ClientInfo = may;
                    uc.ContextMenuStrip = contextMenuStrip1;
                    flpnDanhSachMay.Controls.Add(uc);
                }
                else
                {
                    var uc = new ucMayDisconnect();
                    uc.SetInfo(may.MSSV, may.IP, hoTen);
                    uc.ClientInfo = may;
                    uc.ContextMenuStrip = contextMenuStrip1;
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
            int port = 8888;
            serverSocket.Start(port);

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

                        // Ghi log điểm danh
                        string hoTen = sv != null ? sv.HoTen : "Không tìm thấy tên";
                        string lop = sv != null ? sv.Lop : "N/A";
                        LogDiemDanh(mssv, hoTen, lop);

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
                    else if (msg.StartsWith("NOPBAI_FILENAME|"))
                    {
                        string fileName = msg.Substring("NOPBAI_FILENAME|".Length).Trim();
                        duongDanBaiLam[ip] = fileName; 
                    }
                    else if (msg == "COPY_DATA_READY")
                    {
                        // Client sẵn sàng gửi dữ liệu, không cần làm gì, chỉ cần chờ file
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
                    try
                    {
                        // Kiểm tra xem có phải là copy data request không
                        if (copyDataTarget.ContainsKey(ip))
                        {
                            // Đây là dữ liệu từ máy nguồn cần chuyển đến máy đích
                            string targetIP = copyDataTarget[ip];
                            copyDataTarget.Remove(ip); // Xóa sau khi sử dụng

                            // Gửi dữ liệu đến máy đích
                            try
                            {
                                serverSocket.SendFileToClient(targetIP, bytes);
                                MessageBox.Show(
                                    $"Đã copy dữ liệu bài làm từ máy {ip} sang máy {targetIP} thành công!",
                                    "Thành công",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Information);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(
                                    $"Lỗi khi gửi dữ liệu đến máy đích {targetIP}: {ex.Message}",
                                    "Lỗi",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Error);
                            }
                            return;
                        }

                        // Xử lý bình thường: lưu bài làm
                        string saveFolder = txtLuuBaiThi.Text; 

                        if (!Directory.Exists(saveFolder))
                            Directory.CreateDirectory(saveFolder);

                        string fileName = $"BaiLam_{ip.Replace(".", "_")}.zip";

                        if (duongDanBaiLam.ContainsKey(ip))
                        {
                            fileName = duongDanBaiLam[ip];
                            duongDanBaiLam.Remove(ip); // Xóa khỏi Dictionary sau khi sử dụng
                        }

                        string path = Path.Combine(saveFolder, fileName);
                        File.WriteAllBytes(path, bytes);

                        MessageBox.Show($"Đã nhận bài làm {fileName} từ {ip}", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Lỗi khi lưu bài làm từ {ip}: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
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

        private void btnChonGuiDT_Click(object sender, EventArgs e)
        {
            string path = ChonFile.ChonThuMuc("Chọn thư mục để gửi đề thi");

            if (!string.IsNullOrEmpty(path))
            {
                txtGuiDeThi.Text = path;   // hiển thị đường dẫn gửi    
            }
        }

        private void btnChonLuuBT_Click(object sender, EventArgs e)
        {
            string folder = ChonFile.ChonThuMuc("Chọn nơi lưu bài thi");

            if (!string.IsNullOrEmpty(folder))
                txtLuuBaiThi.Text = folder;//Hiển thị đường dẫn lưu
        }

        private void btnThem_Click(object sender, EventArgs e)
        {
            string filePath = ChonFile.ChonFileFromPC("Chọn đề thi");

            if (!string.IsNullOrEmpty(filePath))
            {
                string fileName = Path.GetFileName(filePath); // chỉ lấy tên file
                lboxDSDeThi.Items.Add(fileName);

                // Lưu đường dẫn thật vào Dictionary
                duongDanDeThi[fileName] = filePath;
            }
        }

        private void btnXoa_Click(object sender, EventArgs e)
        {
            if (lboxDSDeThi.SelectedIndex == -1) return;

            string fileName = lboxDSDeThi.SelectedItem.ToString();

            // Xóa khỏi ListBox
            lboxDSDeThi.Items.Remove(fileName);

            // Xóa khỏi dictionary
            if (duongDanDeThi.ContainsKey(fileName))
                duongDanDeThi.Remove(fileName);
        }

        private void btnPhatDe_Click(object sender, EventArgs e)
        {
            string linkDeThi = txtGuiDeThi.Text;
            if (duongDanDeThi.Count == 0)
            {
                MessageBox.Show("Chưa có đề thi nào được thêm!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int soPhut = (int)nupThoiGian.Value;
            if (soPhut <= 0)
            {
                MessageBox.Show("Thời gian phải lớn hơn 0!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!string.IsNullOrEmpty(linkDeThi))
            {
                serverSocket.BroadcastMessage($"SAVEPATH|{linkDeThi}");
                Thread.Sleep(500); // Đợi Client nhận đường dẫn
            }

            int count = 0;
            foreach (var kvp in duongDanDeThi)
            {
                string fileName = kvp.Key;
                string filePath = kvp.Value;

                if (File.Exists(filePath))
                {
                    // Gửi tên file trước
                    serverSocket.BroadcastMessage($"FILENAME|{fileName}");
                    Thread.Sleep(300); // Đợi client nhận tên file

                    // Sau đó gửi nội dung file
                    byte[] bytes = File.ReadAllBytes(filePath);
                    serverSocket.BroadcastFile(bytes);

                    count++;
                    Console.WriteLine($"Đã gửi file {count}/{duongDanDeThi.Count}: {fileName} ({bytes.Length} bytes)");

                    Thread.Sleep(500); // Đợi giữa các file
                }
                else
                {
                    MessageBox.Show($"Không tìm thấy file: {filePath}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            // Tạo thời gian đếm ngược
            thoiGianConLai = TimeSpan.FromMinutes(soPhut);
            lblDemTG.Text = thoiGianConLai.ToString(@"hh\:mm\:ss");

            // Gửi lệnh bắt đầu countdown
            serverSocket.BroadcastMessage($"BATDAU|{soPhut}");
            timerDemNguoc.Start();
            MessageBox.Show($"Đã phát {duongDanDeThi.Count} đề thi!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

        }

        private void Server_Load(object sender, EventArgs e)
        {
            int port = 8888;
            string localIp = GetLocalIPAddress();
            this.Text = $"Server is running at: {localIp}:{port}";

            //Khởi tạo thời gian
            timerDemNguoc = new System.Windows.Forms.Timer();
            timerDemNguoc.Interval = 1000; 
            timerDemNguoc.Tick += TimerDemNguoc_Tick;
        }

        private void TimerDemNguoc_Tick(object sender, EventArgs e)
        {

        }

        private void btnThuBai_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Đã gửi yêu cầu thu bài cho tất cả Client!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            timerDemNguoc.Stop();

            lblDemTG.Text = "00:00:00";
            serverSocket.BroadcastMessage("YEUCAU_NOPBAI");
            thoiGianConLai = TimeSpan.FromSeconds(0);
        }

        private void LogDiemDanh(string mssv, string hoTen, string lop)
        {
            try
            {
                //1. Tạo tên file log: DiemDanh-ddMMyyyy.txt
                string today = DateTime.Now.ToString("ddMMyyyy");
                string logFileName = $"DiemDanh-{today}.txt";

                string currentDir = Directory.GetCurrentDirectory();
                string solutionDir = Path.GetFullPath(Path.Combine(currentDir, @"..\..\.."));

                string logFilePath = Path.Combine(solutionDir, logFileName); // Lưu tại thư mục chạy của Server

                //2. Định dạng nội dung log: MSSV, Tên, Lớp, Giờ điểm danh
                string logTime = DateTime.Now.ToString("HH:mm:ss");
                string logEntry = $"{mssv},{hoTen},{lop},{logTime}";

                //3. Kiểm tra và tạo header nếu file chưa tồn tại
                if (!File.Exists(logFilePath))
                {
                    string header = "MSSV,HoTen,Lop,GioDiemDanh\n";
                    File.WriteAllText(logFilePath, header);
                }

                //4. Ghi nội dung log vào file (append)
                File.AppendAllText(logFilePath, logEntry + Environment.NewLine);
            }
            catch (Exception ex)
            {
                // Bắt lỗi nếu có vấn đề khi ghi file
                MessageBox.Show($"Lỗi khi ghi log điểm danh: {ex.Message}", "Lỗi Log", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnDisConnect_Click(object sender, EventArgs e)
        {
            try
            {
                serverSocket.Stop();
                MessageBox.Show("Server đã ngắt kết nối!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Dừng bộ đếm thời gian nếu nó đang chạy
                if (timerDemNguoc != null && timerDemNguoc.Enabled)
                {
                    timerDemNguoc.Stop();
                    lblDemTG.Text = "00:00:00";
                    thoiGianConLai = TimeSpan.FromSeconds(0);
                }

                // Cập nhật trạng thái các máy con trên giao diện là "Disconnect"
                foreach (var may in dsMay)
                    may.IsConnected = false;
                LoadDanhSachMay();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi ngắt kết nối Server: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string GetLocalIPAddress()
        {
            var host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
            foreach (var ip in host.AddressList)
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    return ip.ToString();
            return "127.0.0.1"; // Địa chỉ Loopback nếu không tìm thấy IP cục bộ
        }

        private void btnGuiTinNhan_Click(object sender, EventArgs e)
        {
            if (serverSocket == null || dsMay == null)
            {
                MessageBox.Show("Vui lòng khởi động server và tạo danh sách máy trước!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            GuiTinNhanClients form = new GuiTinNhanClients(serverSocket, dsMay);
            form.ShowDialog();
        }

        

        private ClientInfo GetClientInfoFromContextMenu()
        {
            // Lấy control được click (có thể là UserControl hoặc control con)
            var clickedControl = contextMenuStrip1.SourceControl;
            if (clickedControl == null) return null;

            // Tìm UserControl cha nếu clickedControl là control con
            UserControl userControl = clickedControl as UserControl;
            if (userControl == null)
            {
                // Tìm UserControl cha
                System.Windows.Forms.Control parent = clickedControl.Parent;
                while (parent != null && !(parent is UserControl))
                {
                    parent = parent.Parent;
                }
                userControl = parent as UserControl;
            }

            if (userControl == null) return null;

            // Lấy ClientInfo từ UserControl
            if (userControl is ucMayConnect ucConnect)
            {
                return ucConnect.ClientInfo;
            }
            else if (userControl is ucMayDisconnect ucDisconnect)
            {
                return ucDisconnect.ClientInfo;
            }

            return null;
        }

        private void copyDữLiệuToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ClientInfo clientInfo = GetClientInfoFromContextMenu();
            if (clientInfo == null) return;

            // Mở form Copy dữ liệu với thông tin client
            if (serverSocket == null || dsMay == null)
            {
                MessageBox.Show("Vui lòng khởi động server và tạo danh sách máy trước!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            CopyDuLieu form = new CopyDuLieu(serverSocket, dsMay, clientInfo, (sourceIP, targetIP) =>
            {
                // Callback khi copy hoàn tất - lưu thông tin để chuyển tiếp file
                if (!string.IsNullOrEmpty(sourceIP) && !string.IsNullOrEmpty(targetIP))
                {
                    copyDataTarget[sourceIP] = targetIP;
                }
                // Refresh danh sách máy sau khi copy
                LoadDanhSachMay();
            });
            form.ShowDialog();
        }

        private void gửiTinNhắnToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ClientInfo clientInfo = GetClientInfoFromContextMenu();
            if (clientInfo == null) return;

            // Mở form Gửi tin nhắn với client được chọn
            if (serverSocket == null || dsMay == null)
            {
                MessageBox.Show("Vui lòng khởi động server và tạo danh sách máy trước!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Truyền clientInfo vào form để tự động set giá trị mặc định
            GuiTinNhanClients form = new GuiTinNhanClients(serverSocket, dsMay, clientInfo);
            form.ShowDialog();
        }
    }
}
