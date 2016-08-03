using MaterialSkin.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace Electrum {
    public class MP3Options : ElectrumForm {
        public MP3Options() : base() {
            init();
            currentMusicFolder.Text += "\n" + Properties.Settings.Default.MusicFilePath;
        }

        bool cancel;

        private void init() {
            cancel = false;
            organizeButton = new MaterialRaisedButton();
            tagButton = new MaterialRaisedButton();
            outputLabel = new MaterialLabel();
            currentMusicFolder = new MaterialLabel();
            // 
            // organizeButton
            // 
            organizeButton.BackColor = Color.FromArgb(96, 94, 92);
            organizeButton.Font = new Font("Product Sans", 15F);
            organizeButton.ForeColor = Color.White;
            organizeButton.Location = new Point(0, 0);
            organizeButton.Cursor = Cursors.Hand;
            organizeButton.Name = "organizeButton";
            organizeButton.Size = new Size(325, 63);
            organizeButton.Text = "Format Music Files";
            organizeButton.UseVisualStyleBackColor = true;
            organizeButton.MouseClick += new MouseEventHandler(organizeClick);
            // 
            // tagButton
            // 
            tagButton.BackColor = Color.FromArgb(96, 94, 92);
            tagButton.Dock = DockStyle.Bottom;
            tagButton.Font = new Font("Product Sans", 15F);
            tagButton.ForeColor = Color.White;
            tagButton.Location = new Point(0, 201);
            tagButton.Name = "tagButton";
            tagButton.Size = new Size(325, 63);
            tagButton.Cursor = Cursors.Hand;
            tagButton.Text = "Tag Music File";
            tagButton.UseVisualStyleBackColor = true;
            tagButton.MouseClick += new MouseEventHandler(tagClick);
            // 
            // outputLabel
            // 
            outputLabel.Font = new Font("Product Sans", 12F);
            outputLabel.ForeColor = Color.White;
            outputLabel.Location = new Point(0, 63 );
            outputLabel.Name = "outputLabel";
            outputLabel.Size = new Size(325, 138);
            outputLabel.TextAlign = ContentAlignment.TopCenter;
            outputLabel.MouseClick += delegate (object o, MouseEventArgs args) {
                if (args.Button == MouseButtons.Left)
                    cancel = true;
            };
            // 
            // currentMusicFolder
            // 
            currentMusicFolder.AutoSize = true;
            currentMusicFolder.ForeColor = Color.White;
            currentMusicFolder.Location = new Point(4, 160 );
            currentMusicFolder.Cursor = Cursors.Hand;
            currentMusicFolder.Name = "currentMusicFolder";
            currentMusicFolder.AutoSize = true;
            currentMusicFolder.Size = new Size(279, 13);
            currentMusicFolder.Font = new Font("Product Sans", 12f);
            currentMusicFolder.TabIndex = 3;
            currentMusicFolder.MouseEnter += delegate {
                currentMusicFolder.runOnUiThread(() => {
                    currentMusicFolder.Font = new Font(currentMusicFolder.Font, FontStyle.Underline);
                });
            };
            currentMusicFolder.MouseLeave += delegate {
                currentMusicFolder.runOnUiThread(() => {
                    currentMusicFolder.Font = new Font(currentMusicFolder.Font, FontStyle.Regular);
                });
            };
            currentMusicFolder.MouseClick += delegate (object o, MouseEventArgs args) {
                if (args.Button == MouseButtons.Left) {
                    //FolderBrowserDialog dialog = new FolderBrowserDialog();
                    //dialog.Description = "Select Main Music Folder";
                    //dialog.ShowNewFolderButton = true;
                    //dialog.RootFolder = Environment.SpecialFolder.MyComputer;
                    //if (dialog.ShowDialog() == DialogResult.OK) {
                    //    Properties.Settings.Default.MusicFilePath = dialog.SelectedPath;
                    //    Properties.Settings.Default.Save();
                    //    currentMusicFolder.Text = "Current Music Folder: \n" + dialog.SelectedPath;
                    //}
                    string newFolder = Tools.selectFolder();
                    if (newFolder != null) {
                        Properties.Settings.Default.MusicFilePath = newFolder;
                        Properties.Settings.Default.Save();
                        currentMusicFolder.Text = "Current Music Folder: \n" + newFolder;
                    }
                }
            };
            currentMusicFolder.Text = "Current Music Folder:";
            StartPosition = FormStartPosition.CenterScreen;
            AutoScaleDimensions = new SizeF(6F, 13F);
            AutoScaleMode = AutoScaleMode.Font;
            Size = new Size(325, 350);
            FormClosing += delegate {
                StudioContext.getCurrentInstance().formClosed(this);
            };
            Controls.Add(currentMusicFolder);
            Controls.Add(outputLabel);
            Controls.Add(organizeButton);
            Controls.Add(tagButton);
            Name = "OptionsForm";
            Icon = Properties.Resources.electrum;
            Text = "Electrum Music Options";
        }

        private Button organizeButton, tagButton;
        private Label outputLabel;
        private Label currentMusicFolder;
        private IContainer components = null;

        private void tagClick(object sender, MouseEventArgs e) {
            Hide();
            MP3Form f = new MP3Form();
            StudioContext.getCurrentInstance().formOpened(f);
        }

        private void organizeClick(object sender, MouseEventArgs e) {
            organize();
        }

        public void organize() {
            deleteEmptyFolders(Properties.Settings.Default.MusicFilePath);
            new Thread(new ThreadStart(() => {
                try {
                    string filesPath = Properties.Settings.Default.MusicFilePath;
                    int changes = 0;
                    IEnumerable<string> files = Directory.EnumerateFiles(filesPath, "*.mp3", SearchOption.AllDirectories);
                    StringBuilder output = new StringBuilder();
                    int count = files.Count(), i = 0;
                    foreach (string path in files) {
                        if (cancel) break;
                        TagLib.File f = TagLib.File.Create(path);
                        string artist = "" + f.getArtist(), album = "" + f.Tag.Album;
                        if (artist == null || artist.Equals(string.Empty)) artist = "Unknown";
                        string filename = f.Tag.Title;
                        if (filename == null || filename.Equals(string.Empty))
                            filename = Path.GetFileName(path);
                        else filename = filename + ".mp3";
                        if (filename.StartsWith("0")) filename = filename.Substring(2).Trim();
                        string regexSearch = new string(Path.GetInvalidFileNameChars());
                        Regex r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
                        filename = r.Replace(filename, "");
                        artist = r.Replace(artist, "");
                        album = r.Replace(album, "");
                        filename = filename.CleanFileName();
                        album = album.CleanFolderName();
                        artist = artist.CleanFolderName();
                        if (!filename.EndsWith(".mp3")) filename = filename + ".mp3";
                        string newPath = Path.Combine(filesPath, artist, filename);
                        if (!string.IsNullOrEmpty(album))
                            newPath = Path.Combine(filesPath, artist, album, filename);
                        if (!Directory.Exists(Path.GetDirectoryName(newPath)))
                            Directory.CreateDirectory(Path.GetDirectoryName(newPath));
                        try {
                            if (!path.ToLower().Equals(newPath.ToLower())) {
                                File.Move(path, newPath);
                                output.AppendFormat("\\{0}\\{1}\\{2}\n    {3}\n",
                                    artist, album, filename, path);
                                changes++;
                            }
                        } catch (Exception) {
                            //output.AppendLine(e.Message);
                        }
                        i++;
                        outputLabel.runOnUiThread(() => {
                            outputLabel.Text = string.Format("Finished {0} of {1} files\n   {2} changes", i, count, changes);
                            outputLabel.Invalidate();
                        });
                    }
                    string outputText = output.ToString();
                    if (!outputText.Trim().Equals(string.Empty)) {
                        if (outputText.Length <= 1000)
                            Toast.show(outputText);
                        else Toast.show(string.Format("{0} changes made", changes));
                    }
                    deleteEmptyFolders(Properties.Settings.Default.MusicFilePath);
                } catch (Exception e) { this.runOnUiThread(() => { MessageBox.Show(e.Message); }); }
            })).Start();
        }

        void deleteEmptyFolders(string dir) {
            foreach (string sub in Directory.GetDirectories(dir))
                deleteEmptyFolders(sub);
            IEnumerable<string> entries = Directory.EnumerateFileSystemEntries(dir);
            if (dir != null && Directory.Exists(dir) && entries != null && !entries.Any()) {
                try {
                    Directory.Delete(dir);
                } catch (Exception) { }
            }
        }

        protected override void Dispose(bool disposing) {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }
    }
    public static class Overrides {
        public static string getArtist(this TagLib.File file) {
            string faa = file.Tag.FirstAlbumArtist;
            if (!string.IsNullOrEmpty(faa)) return faa.Replace("/", "-").Replace("\\", "-");
            List<string> potentials = new List<string>();
            potentials.AddRange(file.Tag.AlbumArtists);
            potentials.AddRange(file.Tag.Performers);
            foreach (string f in potentials) if (!string.IsNullOrEmpty(f))
                    return f.Replace("/", "-").Replace("\\", "-");
            return "Unknown";
        }
        public static string CleanFolderName(this string s) {
            string temp = s.Split('[')[0].Split('{')[0].Replace("'", "").Trim();
            if (!temp.EndsWith(".")) return temp;
            else {
                while (temp.EndsWith(".")) temp = temp.Substring(0, temp.Length - 1);
                return temp;
            }
        }
        public static string CleanFileName(this string s) {
            s = s.Split('[')[0].Split('{')[0].Trim();
            while (s.Contains("..")) s = s.Replace("..", ".").Trim();
            if (s.Contains("(") && s.IndexOf("s") > 10)
                s = s.Split('(')[0].Trim();
            if (!s.EndsWith(".mp3")) s = s + ".mp3";
            return s;
        }
    }
}
