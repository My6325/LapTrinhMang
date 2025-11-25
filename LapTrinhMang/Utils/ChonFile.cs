using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LapTrinhMang.Utils
{
    public class ChonFile
    {
        public static string ChonFileFromPC(string title = "Chọn file", string filter = "Tất cả|*.*")
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Title = title;
                dialog.Filter = filter;

                if (dialog.ShowDialog() == DialogResult.OK)
                    return dialog.FileName;
            }
            return string.Empty;
        }

        public static string ChonThuMuc(string description = "Chọn thư mục")
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = description;

                if (dialog.ShowDialog() == DialogResult.OK)
                    return dialog.SelectedPath;
            }
            return string.Empty;
        }
    }
}
