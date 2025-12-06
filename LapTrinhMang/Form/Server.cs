using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DocumentFormat.OpenXml.Spreadsheet;
using LapTrinhMang.Models;
using LapTrinhMang.Networking;
using LapTrinhMang.Utils;
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
        private Dictionary<string, string> dichCopyDuLieu = new Dictionary<string, string>(); // IP nguồn -> IP đích cho copy data
        private Dictionary<string, string> tenFileCopyDuLieu = new Dictionary<string, string>(); // IP nguồn -> tên file hiện tại đang copy
        private Dictionary<string, bool> daGuiFileDauTienCopy = new Dictionary<string, bool>(); // IP nguồn -> đã gửi file đầu tiên trong copy chưa 
        private TimeSpan thoiGianConLai;
        private System.Windows.Forms.Timer timerDemNguoc;
        private DanhSachDiemDanh formDSDD = null;
        private bool dangGiaHan = false;

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
                        // Xóa client khỏi danh sách khi ngắt kết nối
                        dsMay.Remove(may);
                        
                        // Dọn dẹp các dictionary liên quan đến client này
                        if (dichCopyDuLieu.ContainsKey(ip))
                        {
                            dichCopyDuLieu.Remove(ip);
                        }
                        if (tenFileCopyDuLieu.ContainsKey(ip))
                        {
                            tenFileCopyDuLieu.Remove(ip);
                        }
                        if (daGuiFileDauTienCopy.ContainsKey(ip))
                        {
                            daGuiFileDauTienCopy.Remove(ip);
                        }
                        if (duongDanDeThi.ContainsKey(ip))
                        {
                            duongDanDeThi.Remove(ip);
                        }
                        if (duongDanBaiLam.ContainsKey(ip))
                        {
                            duongDanBaiLam.Remove(ip);
                        }
                        
                        LoadDanhSachMay();
                    }
                }));
            };

            // Khi nhận tin nhắn từ client
            serverSocket.OnReceiveMessage += (ip, msg) =>
            {
                Invoke(new Action(() =>
                {
                    // Kiểm tra tin nhắn Điểm danh: Format: "DIEMDANH|{JSON_StudentInfo}"
                    if (msg.StartsWith("DIEMDANH|"))
                    {
                        string mssv = msg.Split(new[] { '|' }, 2)[1].Trim();

                        if (string.IsNullOrEmpty(mssv))
                        {
                            return;
                        }
                        string today = DateTime.Now.ToString("ddMMyyyy");
                        string currentDir = Directory.GetCurrentDirectory();

                        // Kiểm tra sinh viên đã điểm danh chưa
                        string logFileName = $"DiemDanh-{today}.txt";
                        string solutionDir = Path.GetFullPath(Path.Combine(currentDir, @"..\..\.."));
                        string logFilePath = Path.Combine(solutionDir, logFileName);

                        bool daDiemDanh = false;
                        if (File.Exists(logFilePath))
                        {
                            var lines = File.ReadAllLines(logFilePath);
                            daDiemDanh = lines.Any(line => line.StartsWith(mssv + ","));
                        }

                        if (daDiemDanh)
                        {
                            // Nếu đã điểm danh rồi, gửi thông báo hoặc ignore
                            serverSocket.SendMessageToClient(ip, $"DIEMDANH_DA_CO|{mssv}");
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
                        serverSocket.SendMessageToClient(ip, $"DIEMDANH|{mssv}");

                        //if (sv != null)
                        //{
                        //    MessageBox.Show($"Sinh viên {sv.HoTen} ({sv.MSSV} - {sv.Lop}) đã điểm danh tại IP {ip}!");
                        //}
                        //else
                        //{
                        //    MessageBox.Show($"MSSV {mssv} đã điểm danh tại IP {ip} (không tìm thấy trong danh sách).");
                        //}

                        LoadDanhSachMay();

                        if (formDSDD != null && !formDSDD.IsDisposed)
                            formDSDD.LoadLogDiemDanh();
                    }
                    else if (msg.StartsWith("NOPBAI_FILENAME|"))
                    {
                        string tenFile = msg.Substring("NOPBAI_FILENAME|".Length).Trim();
                        duongDanBaiLam[ip] = tenFile;
                        // Nếu đang trong quá trình copy, lưu tên file này
                        if (dichCopyDuLieu.ContainsKey(ip))
                        {
                            tenFileCopyDuLieu[ip] = tenFile;
                        }
                    }
                    else if (msg.StartsWith("DIRECTORY|"))
                    {
                        // Nhận thông tin thư mục từ máy nguồn trong quá trình copy
                        if (dichCopyDuLieu.ContainsKey(ip))
                        {
                            string ipDich = dichCopyDuLieu[ip];
                            string duongDanThuMuc = msg.Substring("DIRECTORY|".Length).Trim();
                            
                            // Chuyển tiếp thông tin thư mục đến máy đích
                            var mayDich = dsMay?.FirstOrDefault(x => x.IP == ipDich);
                            if (mayDich != null && mayDich.IsConnected)
                            {
                                serverSocket.SendMessageToClient(ipDich, $"DIRECTORY|{duongDanThuMuc}");
                            }
                        }
                    }
                    else if (msg.StartsWith("FILENAME|"))
                    {
                        // Nhận tên file từ máy nguồn trong quá trình copy
                        string tenFile = msg.Substring("FILENAME|".Length).Trim();
                        
                        if (dichCopyDuLieu.ContainsKey(ip))
                        {
                            // Đây là file trong quá trình copy dữ liệu
                            tenFileCopyDuLieu[ip] = tenFile;
                        }
                    }
                    else if (msg == "COPY_DATA_READY")
                    {
                        // Client sẵn sàng gửi dữ liệu, không cần làm gì, chỉ cần chờ file
                    }
                    else if (msg.StartsWith("COPY_STUDENT_INFO|"))
                    {
                        // Nhận thông tin sinh viên từ máy nguồn và chuyển tiếp đến máy đích
                        if (dichCopyDuLieu.ContainsKey(ip))
                        {
                            string ipDich = dichCopyDuLieu[ip];
                            string jsonSinhVien = msg.Substring("COPY_STUDENT_INFO|".Length).Trim();

                            // Reset flag cho file đầu tiên khi bắt đầu copy
                            daGuiFileDauTienCopy[ip] = false;
                            
                            // Xóa các tên file cũ nếu có (để tránh nhầm lẫn)
                            if (tenFileCopyDuLieu.ContainsKey(ip))
                            {
                                tenFileCopyDuLieu.Remove(ip);
                            }

                            // Chuyển tiếp thông tin sinh viên đến máy đích
                            var mayDich = dsMay?.FirstOrDefault(x => x.IP == ipDich);
                            if (mayDich != null && mayDich.IsConnected)
                            {
                                // Gửi đường dẫn lưu đề thi TRƯỚC khi gửi COPY_STUDENT_INFO
                                // Đảm bảo máy đích biết nơi lưu file trước khi nhận dữ liệu
                                string linkDeThi = txtGuiDeThi.Text;
                                if (!string.IsNullOrEmpty(linkDeThi))
                                {
                                    serverSocket.SendMessageToClient(ipDich, $"SAVEPATH|{linkDeThi}");
                                    Thread.Sleep(300);
                                }
                                
                                serverSocket.SendMessageToClient(ipDich, $"COPY_STUDENT_INFO|{jsonSinhVien}");
                            }
                        }
                    }
                    else if (msg == "COPY_DATA_COMPLETE")
                    {
                        // Máy nguồn đã gửi xong tất cả dữ liệu (các file đề thi)
                        if (dichCopyDuLieu.ContainsKey(ip))
                        {
                            dichCopyDuLieu.Remove(ip); // Xóa sau khi hoàn thành

                            // Xóa các dictionary liên quan
                            if (tenFileCopyDuLieu.ContainsKey(ip))
                            {
                                tenFileCopyDuLieu.Remove(ip);
                            }
                            if (daGuiFileDauTienCopy.ContainsKey(ip))
                            {
                                daGuiFileDauTienCopy.Remove(ip);
                            }

                            // Không hiển thị thông báo (theo yêu cầu)
                        }
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
                        if (dichCopyDuLieu.ContainsKey(ip))
                        {
                            // Đây là dữ liệu từ máy nguồn cần chuyển tiếp đến máy đích
                            string ipDich = dichCopyDuLieu[ip];
                            // KHÔNG xóa dichCopyDuLieu ở đây vì có thể còn nhiều file khác đang được gửi

                            // Kiểm tra máy đích có kết nối không
                            var mayDich = dsMay?.FirstOrDefault(x => x.IP == ipDich);
                            if (mayDich == null || !mayDich.IsConnected)
                            {
                                MessageBox.Show(
                                    $"Không thể copy dữ liệu: Máy đích {ipDich} chưa kết nối đến server!",
                                    "Lỗi",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Error);
                                // Xóa dichCopyDuLieu nếu máy đích không kết nối
                                dichCopyDuLieu.Remove(ip);
                                return;
                            }

                            // Gửi dữ liệu đến máy đích
                            try
                            {
                                // Lấy tên file từ dictionary tenFileCopyDuLieu (đã được lưu khi nhận FILENAME message)
                                string tenFileCopy = null;
                                
                                if (tenFileCopyDuLieu.ContainsKey(ip))
                                {
                                    tenFileCopy = tenFileCopyDuLieu[ip];
                                    // KHÔNG xóa tên file ngay vì có thể còn nhiều file khác đang được gửi
                                    // Chỉ xóa khi nhận COPY_DATA_COMPLETE
                                }
                                else if (duongDanBaiLam.ContainsKey(ip))
                                {
                                    tenFileCopy = duongDanBaiLam[ip];
                                    // Không xóa duongDanBaiLam vì có thể còn dùng cho các file khác
                                }
                                else
                                {
                                    // Nếu không có tên file, tạo tên mặc định
                                    tenFileCopy = $"DeThi_{ip.Replace(".", "_")}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                                }
                                
                                if (string.IsNullOrEmpty(tenFileCopy))
                                {
                                    return;
                                }
                                
                                // Kiểm tra xem đây có phải là file đầu tiên trong quá trình copy không
                                bool laFileDauTien = !daGuiFileDauTienCopy.ContainsKey(ip) || !daGuiFileDauTienCopy[ip];
                                
                                // Nếu là file đầu tiên trong copy dữ liệu, gửi đường dẫn lưu
                                if (laFileDauTien)
                                {
                                    daGuiFileDauTienCopy[ip] = true;
                                    
                                    // Gửi đường dẫn lưu đề thi (từ txtGuiDeThi) cho máy đích
                                    // Đảm bảo máy đích lưu vào đúng thư mục đề thi
                                    string linkDeThi = txtGuiDeThi.Text;
                                    if (!string.IsNullOrEmpty(linkDeThi))
                                    {
                                        serverSocket.SendMessageToClient(ipDich, $"SAVEPATH|{linkDeThi}");
                                        Thread.Sleep(500);
                                    }
                                }

                                // Gửi tên file trước (giống như khi gửi đề thi)
                                serverSocket.SendMessageToClient(ipDich, $"FILENAME|{tenFileCopy}");
                                Thread.Sleep(300); // Đợi client nhận tên file

                                // Sau đó gửi nội dung file
                                serverSocket.SendFileToClient(ipDich, bytes);
                                
                                // Không hiển thị thông báo ở đây, chờ đến khi nhận COPY_DATA_COMPLETE
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(
                                    $"Lỗi khi gửi dữ liệu đến máy đích {ipDich}: {ex.Message}",
                                    "Lỗi",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Error);
                                // Xóa dichCopyDuLieu nếu có lỗi
                                dichCopyDuLieu.Remove(ip);
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

            if (string.IsNullOrEmpty(linkDeThi))
            {
                MessageBox.Show("Chưa chọn thư mục để gửi đề thi!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int soPhut = (int)nupThoiGian.Value;
            if (soPhut <= 0)
            {
                MessageBox.Show("Thời gian phải lớn hơn 0!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Kiểm tra có client nào đang kết nối không
            var dsMayKetNoi = dsMay?.Where(m => m.IsConnected).ToList();
            if (dsMayKetNoi == null || dsMayKetNoi.Count == 0)
            {
                MessageBox.Show("Không có client nào đang kết nối!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // Gửi đường dẫn lưu đề thi đến tất cả client
                serverSocket.BroadcastMessage($"SAVEPATH|{linkDeThi}");
                Thread.Sleep(500);

                // Gửi từng file đề thi
                foreach (var item in duongDanDeThi)
                {
                    string tenFile = item.Key;
                    string duongDanFile = item.Value;

                    if (!File.Exists(duongDanFile))
                    {
                        MessageBox.Show($"File không tồn tại: {duongDanFile}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        continue;
                    }

                    // Gửi tên file trước
                    serverSocket.BroadcastMessage($"FILENAME|{tenFile}");
                    Thread.Sleep(300);

                    // Đọc và gửi nội dung file
                    byte[] duLieuFile = File.ReadAllBytes(duongDanFile);
                    serverSocket.BroadcastFile(duLieuFile);
                    Thread.Sleep(500); // Đợi giữa các file
                }
                // Tạo thời gian đếm ngược
                thoiGianConLai = TimeSpan.FromMinutes(soPhut);
                lblDemTG.Text = thoiGianConLai.ToString(@"hh\:mm\:ss");

                // Gửi lệnh bắt đầu countdown
                serverSocket.BroadcastMessage($"BATDAU|{soPhut}");
                timerDemNguoc.Start();

                MessageBox.Show(
                    $"Đã phát {duongDanDeThi.Count} đề thi đến {dsMayKetNoi.Count} client đang kết nối.",
                    "Thông báo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Lỗi khi phát đề thi: {ex.Message}",
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void Server_Load(object sender, EventArgs e)
        {
            int port = 8888;
            string localIp = GetLocalIPAddress();
            this.Text = $"Server is running at: {localIp}:{port}";

            timerDemNguoc = new System.Windows.Forms.Timer();
            timerDemNguoc.Interval = 1000; 
            timerDemNguoc.Tick += TimerDemNguoc_Tick;
        }

        private void TimerDemNguoc_Tick(object sender, EventArgs e)
        {
            thoiGianConLai = thoiGianConLai.Subtract(TimeSpan.FromSeconds(1));
            lblDemTG.Text = thoiGianConLai.ToString(@"hh\:mm\:ss"); // Cập nhật trên giao diện Server

            if (!dangGiaHan && thoiGianConLai.TotalSeconds == 0)
            {
                dangGiaHan = true;
                serverSocket.BroadcastMessage("HETGIO");

                MessageBox.Show("Đã hết thời gian làm bài",
                    "Thông báo", MessageBoxButtons.OK);

                // Bắt đầu đếm ngược 1 phút gia hạn
                thoiGianConLai = TimeSpan.FromSeconds(60);
                return;
            }

            // Nếu hết thời gian gia hạn
            if (dangGiaHan && thoiGianConLai.TotalSeconds == 0)
            {
                timerDemNguoc.Stop();
                lblDemTG.Text = "00:00:00";

                serverSocket.BroadcastMessage("YEUCAU_NOPBAI");
                return;
            }
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

            // Kiểm tra máy có kết nối đến server không
            if (!clientInfo.IsConnected)
            {
                MessageBox.Show("Máy không có kết nối Server", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

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
                    dichCopyDuLieu[sourceIP] = targetIP;
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

        private void btnXemDSDD_Click(object sender, EventArgs e)
        {
            if (formDSDD == null || formDSDD.IsDisposed)
            {
                formDSDD = new DanhSachDiemDanh();
                formDSDD.FormClosed += (s, args) => formDSDD = null;
                formDSDD.Show();
            }
            else
                formDSDD.Activate();
        }

        /// <summary>
        /// Gửi các file đề thi từ server đến một client cụ thể
        /// </summary>
        private void SendDeThiToClient(string targetIP)
        {
            try
            {
                // Kiểm tra có đề thi nào không
                if (duongDanDeThi == null || duongDanDeThi.Count == 0)
                {
                    return; // Không có đề thi để gửi
                }

                // Gửi đường dẫn lưu đề thi trước (nếu có)
                string linkDeThi = txtGuiDeThi.Text;
                if (!string.IsNullOrEmpty(linkDeThi))
                {
                    serverSocket.SendMessageToClient(targetIP, $"SAVEPATH|{linkDeThi}");
                    Thread.Sleep(500); // Đợi Client nhận đường dẫn
                }

                // Gửi từng file đề thi
                foreach (var kvp in duongDanDeThi)
                {
                    string fileName = kvp.Key;
                    string filePath = kvp.Value;

                    if (File.Exists(filePath))
                    {
                        // Gửi tên file trước
                        serverSocket.SendMessageToClient(targetIP, $"FILENAME|{fileName}");
                        Thread.Sleep(300); // Đợi client nhận tên file

                        // Sau đó gửi nội dung file
                        byte[] bytes = File.ReadAllBytes(filePath);
                        serverSocket.SendFileToClient(targetIP, bytes);

                        Thread.Sleep(500); // Đợi giữa các file
                    }
                }
            }
            catch (Exception ex)
            {
                // Không hiển thị lỗi để không làm gián đoạn quá trình copy
                Console.WriteLine($"Lỗi khi gửi đề thi đến {targetIP}: {ex.Message}");
            }
        }

        private void btnThuLaiDe_Click(object sender, EventArgs e)
        {
            // Kiểm tra có đề thi nào trong danh sách không
            if (lboxDSDeThi.Items.Count == 0)
            {
                MessageBox.Show("Không có đề thi nào trong danh sách để thu hồi!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Lấy danh sách tên file từ lboxDSDeThi
            List<string> danhSachTenFile = new List<string>();
            foreach (var item in lboxDSDeThi.Items)
            {
                danhSachTenFile.Add(item.ToString());
            }
            // Xác nhận trước khi thu hồi
            string danhSachFile = string.Join(", ", danhSachTenFile);
            DialogResult result = MessageBox.Show(
                $"Bạn có chắc muốn thu hồi các đề thi sau cho tất cả client?\n\n" +
                $"Danh sách đề thi:\n{danhSachFile}\n\n" +
                $"Các file đề thi này trong thư mục đề thi của client sẽ bị xóa.\n" +
                $"Thời gian làm bài sẽ được reset về 0.",
                "Xác nhận thu hồi đề thi",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result != DialogResult.Yes)
                return;
            try
            {
                // Reset thời gian về 0
                timerDemNguoc.Stop();
                thoiGianConLai = TimeSpan.Zero;
                dangGiaHan = false;
                lblDemTG.Text = "00:00:00";

                // Gửi yêu cầu thu hồi đề thi với danh sách tên file (không hiển thị thông báo)
                string danhSachFileJson = JsonSerializer.Serialize(danhSachTenFile);
                serverSocket.BroadcastMessage($"THU_HOI_DE_THI|{danhSachFileJson}");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Lỗi khi thu hồi đề thi: {ex.Message}",
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);  
            }
        }

        private void ngắtKếtNốiToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ClientInfo clientInfo = GetClientInfoFromContextMenu();

            if (clientInfo == null)
            {
                MessageBox.Show("Không tìm thấy thông tin máy trạm.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!clientInfo.IsConnected)
            {
                MessageBox.Show($"Máy {clientInfo.IP} hiện không kết nối.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult result = MessageBox.Show($"Bạn có chắc chắn muốn ngắt kết nối máy {clientInfo.IP} ({clientInfo.HoTen}) không?", "Xác nhận ngắt kết nối", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
                return;

            try
            {
                serverSocket.SendMessageToClient(clientInfo.IP, "DISCONNECT_REQUEST");

                MessageBox.Show($"Đã gửi yêu cầu ngắt kết nối đến máy {clientInfo.IP}. Server sẽ cập nhật trạng thái ngay sau đó.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi gửi lệnh ngắt kết nối: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
