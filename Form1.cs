using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Runtime.InteropServices;
using System.Net;
using System.IO;
using System.Drawing.Imaging;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            FolderPath = System.Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            Text = Application.ProductName + " Ver." + Application.ProductVersion;
        }

        string _folderPath = "";

        string FolderPath
        {
            get { return _folderPath; }
            set
            {
                _folderPath = value;
                label1.Text = String.Format("ファイルは {0} に保存されます。", value);
            }
        }

        WMPLib.WindowsMediaPlayer mediaPlayerBGM = new WMPLib.WindowsMediaPlayer();
        void GetSe()
        {
            string path = Application.StartupPath + "\\get.mp3";
            if (System.IO.File.Exists(path))
            {
                mediaPlayerBGM.settings.autoStart = true;
                mediaPlayerBGM.URL = path;
            }
        }


        [DllImport("user32.dll", SetLastError = true)]
        private extern static void AddClipboardFormatListener(IntPtr hwnd);

        [DllImport("user32.dll", SetLastError = true)]
        private extern static void RemoveClipboardFormatListener(IntPtr hwnd);

        private void Form1_Load(object sender, EventArgs e)
        {
            AddClipboardFormatListener(Handle);
        }

        void OnClipboardUpdate()
        {
            // クリップボードのデータが変更された
            if (Clipboard.ContainsImage())
            {
                Image image = Clipboard.GetImage();
                if (!Directory.Exists(FolderPath))
                {
                    MessageBox.Show("保存先のフォルダを指定してください", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                string filePath = FolderPath + "\\" + GetFileName() + ".png";
                image.Save(filePath, ImageFormat.Png);
                if (pictureBox1.Image != null)
                    pictureBox1.Image.Dispose();
                pictureBox1.Image = image;
                GetSe();
            }
            if (Clipboard.ContainsText())
            {
                string url = Clipboard.GetText();
                if (!Directory.Exists(FolderPath))
                {
                    MessageBox.Show("保存先のフォルダを指定してください", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (!IsUrl(url))
                    return;

                int index = url.LastIndexOf(".");
                string kaku = url.Substring(index);
                string filePath = FolderPath + "\\" + GetFileName() + kaku;
                GetJpgFile(filePath, url);

                Image image = Image.FromFile(filePath);

                if (pictureBox1.Image != null)
                    pictureBox1.Image.Dispose();
                pictureBox1.Image = new Bitmap(image);
                image.Dispose();
                GetSe();
            }
        }

        // 指定したファイルパスに画像ファイルを保存する
        public bool GetJpgFile(string filePath, string url)
        {
            var ms = new System.IO.MemoryStream();

            try
            {
                var req = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(url);
                var res = (System.Net.HttpWebResponse)req.GetResponse();
                var st = res.GetResponseStream();

                Byte[] buf = new Byte[1000];
                while (true)
                {
                    // ストリームから一時バッファに読み込む
                    int read = st.Read(buf, 0, buf.Length);

                    if (read > 0)
                    {
                        // 一時バッファの内容をメモリ・ストリームに書き込む
                        ms.Write(buf, 0, read);
                    }
                    else
                    {
                        break;
                    }
                }
                System.IO.File.WriteAllBytes(filePath, ms.ToArray());
                return true;
            }
            catch
            {
                return false;
            }
        }

        bool IsUrl(string str)
        {
            if (str.Length < 8)
                return false;

            int index1 = str.LastIndexOf("://");
            if (index1 == -1)
                return false;

            if (str.Substring(0, 4) == "http")
            {
                int index = str.LastIndexOf(".");
                if (index == -1)
                    return false;
                string kaku = str.Substring(index);
                if (kaku.ToLower() == ".jpg")
                    return true;
                if (kaku.ToLower() == ".png")
                    return true;
                if (kaku.ToLower() == ".bmp")
                    return true;
                if (kaku.ToLower() == ".gif")
                    return true;
            }
            return false;
        }


        string GetFileName()
        {
            return DateTime.Now.ToString("yyyyMMddHHmmss");
        }

        private const int WM_CLIPBOARDUPDATE = 0x31D;

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_CLIPBOARDUPDATE)
            {
                OnClipboardUpdate();
                m.Result = IntPtr.Zero;
            }
            else
                base.WndProc(ref m);
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            RemoveClipboardFormatListener(Handle);
        }


        private void button1_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                FolderPath = folderBrowserDialog1.SelectedPath;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (Directory.Exists(FolderPath))
            {
                System.Diagnostics.Process.Start(FolderPath);
            }
            
        }
    }

}

