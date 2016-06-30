using Global;
using MaterialSkin.Controls;
using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Electrum {
    public class MP3Form : KeithForm {
        public MP3Form() {
            InitializeComponent();
            init();
            open();
        }
        public MP3Form(string filename) {
            InitializeComponent();
            init();
            loadFile(filename);
        }

        void init() {
            Font f = new Font("Product Sans", 13F);
            /*tagArtist.Font = f;
            tagAlbumName.Font = f;
            tagTitle.Font = f;
            tagAlbumArtist.Font = f;
            label1.Font = f;
            label2.Font = f;
            label3.Font = f;
            label4.Font = f;
            saveButton.Font = f;*/
            foreach (Control c in Controls) {
                if (c is TextBox || c is Button || c is Label)
                    c.Font = f;
            }
        }

        string currentFileName;

        void open() {
            OpenFileDialog o = new OpenFileDialog();
            o.InitialDirectory = (currentFileName == null) ? Properties.Settings.Default.MusicFilePath : Path.GetDirectoryName(currentFileName);
            o.Multiselect = false;
            if (o.ShowDialog() == DialogResult.OK)
                loadFile(o.FileName);
            else if (currentFileName == null) Close();
        }

        void loadFile(string filename) {
            try {
                currentFileName = filename;
                Text = Path.GetFileName(filename);
                TagLib.File file = TagLib.File.Create(filename);
                tagTitle.Text = file.Tag.Title;
                tagAlbumName.Text = file.Tag.Album;
                StringBuilder trackArtists = new StringBuilder(), albumArtists = new StringBuilder();
                int count = 0;
                foreach (string artist in file.Tag.Performers) trackArtists.Append(count++ == 0 ? "" : "; ").Append(artist);
                count = 0;
                foreach (string albumArtist in file.Tag.AlbumArtists) albumArtists.Append(count++ == 0 ? "" : "; ").Append(albumArtist);
                tagArtist.Text = trackArtists.ToString();
                tagAlbumArtist.Text = albumArtists.ToString();
                this._(() => {
                    int longest = Math.Max(TextRenderer.MeasureText(tagAlbumName.Text, Font).Width,
                        Math.Max(TextRenderer.MeasureText(tagAlbumArtist.Text, Font).Width,
                            Math.Max(TextRenderer.MeasureText(tagTitle.Text, Font).Width,
                                TextRenderer.MeasureText(tagArtist.Text, Font).Width)));
                    int possibleW = longest + tagTitle.Left + 200;
                    if (Width < possibleW) animateWidth(possibleW);
                }, 500);
                try {
                    byte[] bin = file.Tag.Pictures[0].Data.Data;
                    loadImage(Image.FromStream(new MemoryStream(bin)));
                } catch (Exception) {
                    Icon = Properties.Resources.electrum;
                }
            } catch (Exception) {
                MessageBox.Show("Error Loading File");
            }
            tagTitle.Focus();
        }

        void loadImage(Image i) {
            try {
                Image image = i.GetThumbnailImage(150, 150, null, IntPtr.Zero);
                pictureBox1.Image = image;
                Icon = Icon.FromHandle(new Bitmap(i).GetHicon());
                this.i = i;
            } catch (Exception) {
                Icon = Properties.Resources.electrum;
            }
        }

        void pasteImage(object a = null, EventArgs b = null) {
            if (Clipboard.ContainsImage()) {
                loadImage(Clipboard.GetImage());
            } else MessageBox.Show("That isn't an image");
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e) {
            if (e.Button == MouseButtons.Right) {
                pasteImage();
                textChanged();
            }
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        extern static bool DestroyIcon(IntPtr handle);

        private Image i;

        bool saveAction = true;

        private void saveButton_Click(object sender, EventArgs e) {
            saveFile();
        }

        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing) {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        void textChanged() {
            saveButton.Text = "Save";
            saveAction = true;
        }

        void saved() {
            saveButton.Text = "Main Screen";
            saveAction = false;
        }

        const short minTop = 60;

        private void InitializeComponent() {
            saveAction = true;
            int initialHeight = 550, initialWidth = 350;
            label1 = new MaterialLabel();
            tagTitle = new TextBox();
            tagAlbumName = new TextBox();
            label2 = new MaterialLabel();
            label3 = new MaterialLabel();
            tagArtist = new TextBox();
            pictureBox1 = new PictureBox();
            saveButton = new MaterialRaisedButton();
            label4 = new MaterialLabel();
            tagAlbumArtist = new TextBox();
            ((System.ComponentModel.ISupportInitialize)(pictureBox1)).BeginInit();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 21);
            label1.Name = "label1";
            label1.BackColor = Color.Transparent;
            label1.Size = new Size(27, 13);
            label1.Text = "Title";
            // 
            // tagTitle
            // 
            tagTitle.BackColor = SystemColors.ButtonShadow;
            tagTitle.Location = new Point(89, 18);
            tagTitle.Name = "tagTitle";
            tagTitle.Size = new Size(208, 20);
            tagTitle.TextChanged += delegate {
                textChanged();
            };
            tagTitle.TabIndex = 1;
            // 
            // tagAlbumName
            // 
            tagAlbumName.BackColor = SystemColors.ButtonShadow;
            tagAlbumName.Location = new Point(89, 112);
            tagAlbumName.Name = "tagAlbumName";
            tagAlbumName.TextChanged += delegate {
                textChanged();
            };
            tagAlbumName.Size = new Size(208, 20);
            tagAlbumName.TabIndex = 3;
            tagAlbumName.TextChanged += delegate {
                textChanged();
            };
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.BackColor = Color.Transparent;
            label2.Location = new Point(12, 115);
            label2.Name = "label2";
            label2.Size = new Size(36, 13);
            label2.Text = "Album";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(12, 69);
            label3.Name = "label3";
            label3.BackColor = Color.Transparent;
            label3.Size = new Size(30, 13);
            label3.Text = "Artist";
            // 
            // tagArtist
            // 
            tagArtist.BackColor = SystemColors.ButtonShadow;
            tagArtist.Location = new Point(89, 66);
            tagArtist.Name = "tagArtist";
            tagArtist.Size = new Size(208, 20);
            tagArtist.TextChanged += delegate {
                textChanged();
            };
            tagArtist.TabIndex = 2;
            // 
            // pictureBox1
            // 
            pictureBox1.Location = new Point(89, 220);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(150, 150);
            pictureBox1.TabStop = false;
            pictureBox1.MouseDown += new MouseEventHandler(pictureBox1_MouseDown);
            // 
            // saveButton
            // 
            saveButton.BackColor = SystemColors.ControlDarkDark;
            saveButton.Dock = DockStyle.Bottom;
            saveButton.FlatAppearance.BorderColor = Color.Black;
            saveButton.FlatStyle = FlatStyle.Flat;
            saveButton.Location = new Point(0, initialHeight - 55);
            saveButton.Name = "saveButton";
            saveButton.Size = new Size(initialWidth, 55);
            saveButton.TabIndex = 5;
            saveButton.Text = "Save";
            saveButton.UseVisualStyleBackColor = false;
            saveButton.Click += new EventHandler(saveButton_Click);
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(12, 158);
            label4.Name = "label4";
            label4.BackColor = Color.Transparent;
            label4.Size = new Size(62, 13);
            label4.Text = "Album Artist";
            // 
            // tagAlbumArtist
            // 
            tagAlbumArtist.BackColor = SystemColors.ButtonShadow;
            tagAlbumArtist.Location = new Point(89, 155);
            tagAlbumArtist.Name = "tagAlbumArtist";
            tagAlbumArtist.Size = new Size(208, 20);
            tagAlbumArtist.TabIndex = 4;
            // 
            // Form1
            // 
            MinimumSize = new Size(initialWidth, initialHeight);
            SizeChanged += sizeChanged;
            Shown += delegate {
                sizeChanged();
                tagTitle.Focus();
            };
            StartPosition = FormStartPosition.CenterScreen;
            AutoScaleDimensions = new SizeF(6F, 13F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.ButtonShadow;
            ClientSize = new Size(initialWidth, initialHeight);
            Controls.Add(label4);
            Controls.Add(tagAlbumArtist);
            Controls.Add(saveButton);
            Controls.Add(pictureBox1);
            Controls.Add(label3);
            Controls.Add(tagArtist);
            Controls.Add(label2);
            Controls.Add(tagAlbumName);
            Controls.Add(tagTitle);
            Controls.Add(label1);
            KeyPreview = true;
            KeyDown += delegate (object o, KeyEventArgs args) {
                if ((args.KeyCode == Keys.S && args.Control && !args.Shift && !args.Alt) || args.KeyCode == Keys.Enter) {
                    args.Handled = true;
                    args.SuppressKeyPress = true;
                    this.runLater(() => { saveFile(); });
                }
                if (args.KeyCode == Keys.O && args.Control && !args.Shift && !args.Alt) {
                    args.Handled = true;
                    args.SuppressKeyPress = true;
                    this.runLater(() => { open(); });
                }
            };
            Name = "Form1";
            Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)(pictureBox1)).EndInit();
            ResumeLayout(false);
            PerformLayout();
            foreach (Control c in Controls) c.Top += minTop;
        }

        void saveFile(bool save = false) {
            if (saveAction || save) {
                using (TagLib.File file = TagLib.File.Create(currentFileName)) {
                    try {
                        file.Tag.Album = tagAlbumName.Text;
                        file.Tag.Title = tagTitle.Text;
                        string[] artists = tagArtist.Text.Split(';');
                        for (int i = 0; i < artists.Length; i++) artists[i] = artists[i].Trim();
                        file.Tag.Performers = artists;
                        string[] al_artists = tagAlbumArtist.Text.Split(';');
                        for (int i = 0; i < al_artists.Length; i++) al_artists[i] = al_artists[i].Trim();
                        file.Tag.AlbumArtists = al_artists;
                        if (i != null) {
                            TagLib.Picture pic = new TagLib.Picture();
                            pic.Type = TagLib.PictureType.FrontCover;
                            pic.MimeType = System.Net.Mime.MediaTypeNames.Image.Jpeg;
                            pic.Description = "Cover";
                            using (MemoryStream ms = new MemoryStream()) {
                                i.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                                ms.Position = 0;
                                pic.Data = TagLib.ByteVector.FromStream(ms);
                            }
                            file.Tag.Pictures = new TagLib.IPicture[1] { pic };
                        }
                        file.Save();
                        AutoClosingMessageBox.show("Saved Successfully", "File Saved", 1000);
                        loadFile(currentFileName);
                        saved();
                    } catch (Exception ex) {
                        //Temporarily write the file to AppData, for safety
                        //string appDataPath = Path.Combine(Tools.getDataFolder(), Path.GetFileName(currentFileName));
                        //TagLib doesn't support that...
                        //Give Up?
                        MessageBox.Show("Error Occurred while saving file: " + ex.Message);
                    }
                }
            } else {
                Hide();
                //new OptionsForm().Show();
            }
        }

        private void sizeChanged(object sender = null, EventArgs e = null) {/**/
            tagAlbumArtist.Left = label4.Right + 10;
            tagAlbumArtist.Width = Width - tagAlbumArtist.Left - 25;
            tagAlbumName.Width = Width - tagAlbumName.Left - 25;
            tagArtist.Width = Width - tagArtist.Left - 25;
            tagTitle.Width = Width - tagTitle.Left - 25;
            pictureBox1.Left = (Width / 2) - (pictureBox1.Width / 2);/**/
        }

        private MaterialLabel label1;
        private TextBox tagTitle;
        private TextBox tagAlbumName;
        private MaterialLabel label2;
        private MaterialLabel label3;
        private TextBox tagArtist;
        private PictureBox pictureBox1;
        private MaterialRaisedButton saveButton;
        private MaterialLabel label4;
        private TextBox tagAlbumArtist;
    }

    public static class TagOverrides {
        public static void runLater(this Control control, Action runnable) {
            new Thread(new ThreadStart(() => {
                Thread.Sleep(10);
                control.Invoke(runnable);
            })).Start();
        }
    }
}