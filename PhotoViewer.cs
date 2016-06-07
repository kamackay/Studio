using Global;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace Electrum {
    /**
     * FileTypes:
     *      Tested: 
     *          -PNG
     *          -JPG/JPEG
     *          -WEBP
     *          -SVG
     *          
     */
    public partial class PhotoViewer : Form {
        private string openFile = null;
        public PhotoViewer() {
            InitializeComponent();
            init();
            PromptOpen();
        }

        public PhotoViewer(string s) {
            InitializeComponent();
            init();
            if (!string.Empty.Equals(s)) LoadImage(s);
            else PromptOpen();
        }

        public PhotoViewer(string[] s) {
            InitializeComponent();
            init();
            if (s.Length > 0 && !string.Empty.Equals(s[0])) LoadImage(s[0]);
            else PromptOpen();
        }

        public event EventHandler<FormOpenEventArgs> subFormOpened;

        public class FormOpenEventArgs : EventArgs {
            public Form subForm { get; set; }
            public FormOpenEventArgs(Form form) {
                subForm = form;
            }
        }

        protected virtual void onSubFormOpened(FormOpenEventArgs args) {
            subFormOpened?.Invoke(this, args);
            if (subFormOpened == null) StudioContext.getCurrentInstance().formOpened(args.subForm);
        }

        private void init() {
            MinimumSize = new Size(50, 50);
            F.runAsync(() => {
                StudioContext.getCurrentInstance().formOpened(this);
            });
        }
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();
        private void PromptOpen() {
            OpenFileDialog o = new OpenFileDialog();
            o.InitialDirectory = @"C:\Documents\Google Drive\Other Files\Wallpapers\";
            o.Multiselect = false;
            if (o.ShowDialog() == DialogResult.OK)
                LoadImage(o.FileName);
            else if (openFile == null) Environment.Exit(0);
        }

        private void This_MouseDown(object sender, MouseEventArgs e) {
            if (e.Button == MouseButtons.Left) {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void LoadImage(string p) {
            showLoading();
            this.runOnUiThread(() => { BackColor = Color.Black; });
            try {
                if (!File.Exists(p)) NoImage();
                string ext = Path.GetExtension(p).ToLower();
                if (".gif".Equals(ext)) {
                    GifViewer viewer = new GifViewer(p);
                    viewer.FormClosed += delegate { Close(); };
                    viewer.Show();
                    onSubFormOpened(new FormOpenEventArgs(viewer));
                    this.exit();
                    return;
                } else if (".svg".Equals(ext)) {
                    const string inkExe = @"c:\program files\inkscape\inkscape.exe";
                    if (!File.Exists(inkExe)) {
                        DialogResult result = MessageBox.Show("You need to install Inkscape in order to view that file. Install Now?", "Inkscape needed", MessageBoxButtons.YesNo);
                        if (result == DialogResult.Yes) {
                            installInkscape();
                        } else Environment.Exit(0);
                    }
                    string png = Path.Combine(Tools.getDataFolder(), "temp.png");
                    string args = string.Format("-f \"{0}\" -e \"{1}\"", p, png);
                    try {
                        Process inkscape = Process.Start(new ProcessStartInfo(inkExe, args));
                        inkscape.WaitForExit();
                    } catch (Exception e) { MessageBox.Show(string.Format("Inkscape Error: " + e.Message)); }
                    Bitmap b = new Bitmap(png);
                    Size s = Shrink(b.Size);
                    Size = s;
                    b = new Bitmap(b, s);//Resize the bitmap so that needlessly large images can still be loaded
                    BackgroundImage = b;
                    img = b;
                } else {
                    Bitmap b = new Bitmap(p);
                    Size s = Shrink(b.Size);
                    Size = s;
                    b = new Bitmap(b, s);//Resize the bitmap so that needlessly large images can still be loaded
                    BackgroundImage = b;
                    img = b;
                    postImageSet();
                }
                BackgroundImageLayout = ImageLayout.Zoom;
                openFile = p;
                Text = Path.GetFileName(openFile);
                Focus();
                BringToFront();
            } catch (Exception e) {
                MessageBox.Show("Error: " + e.Message + "\nfile - " + p);
                Environment.Exit(0);
            }
        }

        void postImageSet() {
            this.runOnUiThread(() => { Focus(); BringToFront(); Show(); Activate(); });
            F.runAsync(() => { Thread.Sleep(100); this.runOnUiThread(() => { TopMost = false; }); });
            Bitmap b = new Bitmap(img);
            Thread t = F.runAsync(() => {
                if (b.isMostlyBlack())
                    this.runOnUiThread(() => { BackColor = Color.White; });
                showLoading(false);
            });
        }

        private Size Shrink(Size s) {
            double h = s.Height, w = s.Width;
            while (h > Screen.PrimaryScreen.WorkingArea.Height * .9 || w > Screen.PrimaryScreen.WorkingArea.Width * .9) {
                h /= 1.1;
                w /= 1.1;
            }
            return new Size(Convert.ToInt32(w), Convert.ToInt32(h));
        }

        protected override void WndProc(ref Message m) {
            if (m.Msg == 0x100 || m.Msg == 0x104) {
                if (m.WParam == (IntPtr)39) NextImage();
                else if (m.WParam == (IntPtr)37) PrevImage();
                else if (m.WParam == (IntPtr)0x4F) PromptOpen();
                else if (m.WParam == (IntPtr)0x43) copy(null, null);
                else if (m.WParam == (IntPtr)0x53) save(null, null);
            }

            const uint WM_NCHITTEST = 0x0084;
            const uint WM_MOUSEMOVE = 0x0200;

            const uint HTLEFT = 10;
            const uint HTRIGHT = 11;
            const uint HTBOTTOMRIGHT = 17;
            const uint HTBOTTOM = 15;
            const uint HTBOTTOMLEFT = 16;
            const uint HTTOP = 12;
            const uint HTTOPLEFT = 13;
            const uint HTTOPRIGHT = 14; const int HTTRANSPARENT = (-1);

            const int RESIZE_HANDLE_SIZE = 10;
            bool handled = false;
            if (m.Msg == WM_NCHITTEST) m.Result = (IntPtr)HTTRANSPARENT;
            if (m.Msg == WM_NCHITTEST || m.Msg == WM_MOUSEMOVE) {
                Size formSize = Size;
                Point screenPoint = new Point(m.LParam.ToInt32());
                Point clientPoint = PointToClient(screenPoint);

                Dictionary<uint, Rectangle> boxes = new Dictionary<uint, Rectangle>() {
            {HTBOTTOMLEFT, new Rectangle(0, formSize.Height - RESIZE_HANDLE_SIZE, RESIZE_HANDLE_SIZE, RESIZE_HANDLE_SIZE)},
            {HTBOTTOM, new Rectangle(RESIZE_HANDLE_SIZE, formSize.Height - RESIZE_HANDLE_SIZE, formSize.Width - 2*RESIZE_HANDLE_SIZE, RESIZE_HANDLE_SIZE)},
            {HTBOTTOMRIGHT, new Rectangle(formSize.Width - RESIZE_HANDLE_SIZE, formSize.Height - RESIZE_HANDLE_SIZE, RESIZE_HANDLE_SIZE, RESIZE_HANDLE_SIZE)},
            {HTRIGHT, new Rectangle(formSize.Width - RESIZE_HANDLE_SIZE, RESIZE_HANDLE_SIZE, RESIZE_HANDLE_SIZE, formSize.Height - 2*RESIZE_HANDLE_SIZE)},
            {HTTOPRIGHT, new Rectangle(formSize.Width - RESIZE_HANDLE_SIZE, 0, RESIZE_HANDLE_SIZE, RESIZE_HANDLE_SIZE) },
            {HTTOP, new Rectangle(RESIZE_HANDLE_SIZE, 0, formSize.Width - 2*RESIZE_HANDLE_SIZE, RESIZE_HANDLE_SIZE) },
            {HTTOPLEFT, new Rectangle(0, 0, RESIZE_HANDLE_SIZE, RESIZE_HANDLE_SIZE) },
            {HTLEFT, new Rectangle(0, RESIZE_HANDLE_SIZE, RESIZE_HANDLE_SIZE, formSize.Height - 2*RESIZE_HANDLE_SIZE) }
        };

                foreach (KeyValuePair<uint, Rectangle> hitBox in boxes) {
                    if (hitBox.Value.Contains(clientPoint)) {
                        m.Result = (IntPtr)hitBox.Key;
                        handled = true;
                        break;
                    }
                }
            }
            if (!handled) base.WndProc(ref m);
        }

        private void installInkscape() {
            try {
                string msi = Path.Combine(Tools.getDataFolder(), "inkscape-installer.msi");
                //File.WriteAllBytes(msi, Properties.Resources.inkscape_installer);
                WebClient Client = new WebClient();
                Client.DownloadFile("https://inkscape.org/en/gallery/item/3956/inkscape-0.91-x64.msi", msi);
                Process.Start(msi);
                Close();
            } catch (Exception) { this.exit(); }
        }

        private void NoImage() {
            BackgroundImage = null;
            openFile = null;
        }

        Bitmap img;

        public void makeBackTransparent(object o, EventArgs args) {
            showLoading();
            if (img != null) {
                Bitmap temp = new Bitmap(img);
                F.runAsync(() => {
                    Color c = temp.findMissingColor();
                    this.runOnUiThread(() => {
                        BackColor = c;
                        TransparencyKey = c;
                    });
                    showLoading(false);
                });
            }
        }

        private void NextImage() {
            if (openFile == null) return;
            DirectoryInfo di = Directory.GetParent(openFile);
            FileInfo[] files = di.GetFiles();
            int i = 0;
            while (string.Compare(files[i++].FullName, openFile) != 0) ;
            if (i == files.Length - 1) i = 0;
            while (!FileTypes.isImage(files[i].FullName)) {
                i++;
                if (i == files.Length - 1) i = 0;
            }
            LoadImage(files[i].FullName);
        }

        private void PrevImage() {
            if (openFile == null) return;
            DirectoryInfo di = Directory.GetParent(openFile);
            FileInfo[] files = di.GetFiles();
            int i = -1;
            while (string.Compare(files[++i].FullName, openFile) != 0) ;
            i--;
            if (i <= 0) i = files.Length - 1;
            while (!FileTypes.isImage(files[i].FullName)) {
                if (i <= 0) i = files.Length;
                i--;
            }
            LoadImage(files[i].FullName);
        }

        private void PhotoViewer_Resize(object sender, EventArgs e) {
            CloseButton.Left = Width - CloseButton.Width;
            CloseButton.Top = 0;
        }

        private void CloseButton_Click(object sender, EventArgs e) {
            Environment.Exit(0);
        }
        protected override void OnPaint(PaintEventArgs e) {
            base.OnPaint(e);
        }
        private void CheckMouse(object a = null, EventArgs b = null) {
            int give = 50;
            if ((Cursor.Position.X >= Right - give) && (Cursor.Position.X <= Right) &&
                (Cursor.Position.Y >= Top) && (Cursor.Position.Y <= Top + give))
                CloseButton.Visible = true;
            else CloseButton.Visible = false;
        }
        private void PhotoViewer_MouseMove(object sender, MouseEventArgs e) {
            CheckMouse();
        }

        private void PhotoViewer_MouseClick(object sender, MouseEventArgs e) {
            if (e.Button == MouseButtons.Right)
                new ContextMenu(new MenuItem[] { new MenuItem("Switch Back Color", FlipBackColor),
                    new MenuItem("Open", new EventHandler(open)),
                    new MenuItem("Copy", new EventHandler(copy)),
                    new MenuItem("Save", new EventHandler(save)),
                    new MenuItem("Make Background Transparent", new EventHandler(makeBackTransparent)),
                    //new MenuItem("ConvertAllInDir", new EventHandler(allinDir)),
                    new MenuItem("Quit", new EventHandler(quit)) }).Show(ActiveForm, e.Location);
        }

        private void quit(object sender, EventArgs e) {
            Close();
        }

        private void allinDir(object sender, EventArgs e) {
            DirectoryInfo dir = new DirectoryInfo(Path.GetDirectoryName(openFile));
            foreach (FileInfo file in dir.GetFiles("*.jpg")) {
                string path = Path.Combine(Path.GetDirectoryName(file.FullName),
                    Path.GetFileNameWithoutExtension(file.FullName) + ".png");
                Bitmap b = new Bitmap(file.FullName);
                b.Save(path);
            }

        }

        public void showLoading(bool shown = true) {
            loadingImage.runOnUiThread(() => { loadingImage.Visible = shown; });
        }

        private void open(object sender, EventArgs e) {
            PromptOpen();
        }

        private void copy(object sender, EventArgs e) {
            Clipboard.SetImage(Image.FromFile(openFile));
        }

        private void FlipBackColor(object sender = null, EventArgs e = null) {
            if (BackColor == Color.Black) BackColor = Color.White;
            else BackColor = Color.Black;
        }

        private void save(object a, EventArgs e) {
            try {
                Bitmap b = new Bitmap(openFile);
                SaveFileDialog saveDialog = new SaveFileDialog();
                saveDialog.InitialDirectory = Path.GetDirectoryName(openFile);
                saveDialog.Filter = "PNG Image|*.png";
                saveDialog.FileName = Path.GetFileNameWithoutExtension(openFile) + ".png";
                saveDialog.Title = "Where do you want to save this file?";
                if (saveDialog.ShowDialog() == DialogResult.OK) {
                    string file = saveDialog.FileName;
                    if (!Directory.Exists(Path.GetDirectoryName(file)))
                        Directory.CreateDirectory(Path.GetDirectoryName(file));
                    b.Save(file);
                } else return;
            } catch (Exception) {
                MessageBox.Show("Error while saving to file");
                return;
            }
            MessageBox.Show("Successfully Saved");
        }
        private bool isImage(string filename) {
            try {
                Bitmap b = new Bitmap(filename);
                return (b != null);
            } catch (Exception) { return false; }
        }

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PhotoViewer));
            CloseButton = new Button();
            loadingImage = new PictureBox();
            ((System.ComponentModel.ISupportInitialize)(loadingImage)).BeginInit();
            SuspendLayout();
            // 
            // CloseButton
            // 
            CloseButton.AutoSize = true;
            CloseButton.BackColor = Color.Transparent;
            CloseButton.BackgroundImage = Properties.Resources.close;
            CloseButton.BackgroundImageLayout = ImageLayout.Stretch;
            CloseButton.FlatAppearance.BorderColor = Color.DarkGray;
            CloseButton.FlatAppearance.BorderSize = 0;
            CloseButton.FlatAppearance.MouseDownBackColor = Color.Transparent;
            CloseButton.FlatAppearance.MouseOverBackColor = Color.Transparent;
            CloseButton.FlatStyle = FlatStyle.Flat;
            CloseButton.ForeColor = Color.DarkGray;
            CloseButton.Location = new Point(239, 12);
            CloseButton.Name = "CloseButton";
            CloseButton.Size = new Size(33, 32);
            CloseButton.TabIndex = 0;
            CloseButton.UseVisualStyleBackColor = false;
            CloseButton.Visible = false;
            CloseButton.Click += new EventHandler(CloseButton_Click);
            CloseButton.MouseEnter += new EventHandler(CheckMouse);
            CloseButton.MouseHover += new EventHandler(CheckMouse);
            // 
            // loadingImage
            // 
            loadingImage.Image = Properties.Resources.material_loading;
            loadingImage.BackgroundImageLayout = ImageLayout.Zoom;
            loadingImage.BackColor = Color.Transparent;
            loadingImage.Location = new Point(0, 0);
            loadingImage.Name = "loadingImage";
            loadingImage.Size = new Size(100, 100);
            loadingImage.SizeMode = PictureBoxSizeMode.Zoom;
            loadingImage.TabIndex = 1;
            loadingImage.TabStop = false;
            // 
            // PhotoViewer
            // 
            AutoScaleDimensions = new SizeF(6F, 13F);
            AutoScaleMode = AutoScaleMode.Font;
            StartPosition = FormStartPosition.CenterParent;
            TopMost = true;
            BackColor = SystemColors.Desktop;
            ClientSize = new Size(284, 264);
            Controls.Add(loadingImage);
            Controls.Add(CloseButton);
            FormBorderStyle = FormBorderStyle.None;
            Icon = ((Icon)(resources.GetObject("$this.Icon")));
            Name = "PhotoViewer";
            MouseClick += new MouseEventHandler(PhotoViewer_MouseClick);
            MouseDown += new MouseEventHandler(This_MouseDown);
            MouseEnter += new EventHandler(CheckMouse);
            MouseHover += new EventHandler(CheckMouse);
            MouseMove += new MouseEventHandler(PhotoViewer_MouseMove);
            Resize += new EventHandler(PhotoViewer_Resize);
            ((System.ComponentModel.ISupportInitialize)(loadingImage)).EndInit();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private Button CloseButton;
        private PictureBox loadingImage;
    }
}
