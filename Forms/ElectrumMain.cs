using Electrum.Controls;
using Global;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace Electrum {
    public partial class ElectrumMain : ElectrumForm {
        public ElectrumMain() : base() {
            InitializeComponent();
            initialActions = () => {
                this.sync(() => {
                });
            };
            MinimumSize = new Size(500, 500);
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
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        const int t = 100;
        const string defaultText = "Welcome to Electrum Studios";

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            components = new System.ComponentModel.Container();
            AutoScaleMode = AutoScaleMode.Font;
            KeyPreview = true;
            Text = defaultText;
            optionsBar = new OptionsBar();
            list = new FlowLayoutPanel();
            loadingImage = new PictureBox();
            pathBar = new TextBox();

            pathBar.Font = new Font(Font.FontFamily, 20f);
            pathBar.Text = "Something";
            pathBar.Padding = new Padding(5);
            pathBar.BackColor = Color.FromArgb(0x25, 0x24, 0x23);
            pathBar.ForeColor = Color.Yellow;
            pathBar.AutoSize = false;
            pathBar.Visible = false;
            pathBar.Size = new Size(Width, pathBar.Font.Height);
            pathBar.Location = new Point(0, 100);
            add(pathBar);

            loadingImage.Image = Properties.Resources.material_loading;
            loadingImage.BackgroundImageLayout = ImageLayout.Zoom;
            loadingImage.BackColor = Color.Transparent;
            loadingImage.Size = new Size(50, 40);
            loadingImage.Location = new Point(60, 120 + pathBar.Height);
            loadingImage.SizeMode = PictureBoxSizeMode.Zoom;
            loadingImage.Visible = false;
            add(loadingImage);

            backButton = new BackButton();
            backButton.Size = new Size(50, 50);
            backButton.Location = new Point(0, 110 + pathBar.Height);
            backButton.Visible = false;
            backButton.MouseClick += delegate (object o, MouseEventArgs args) {
                if (args.Button == MouseButtons.Left) {
                    goBack();
                }
            };

            add(backButton);

            optionsBar.setOptions(new OptionsBar.Option[] {
                new OptionsBar.Option { title = "Open File", onClick = () => { this.f(); } },
                new OptionsBar.Option { title = "Exit", onClick = () => { Close(); } },
                new OptionsBar.Option {holdRight = true, title = "Right",
                    onClick = () => { } }/*,
                new OptionsBar.Option {holdRight = true, title = "Right 2",
                    onClick = () => { } }/**/
            });
            optionsBar.setBackgroundColor(Color.FromArgb(0x50, 0x50, 0x50));
            add(optionsBar);

            list.Dock = DockStyle.Bottom;
            list.Height = Height - backButton.Bottom;
            list.FlowDirection = FlowDirection.TopDown;
            list.AutoScroll = true;
            list.MouseClick += delegate (object o, MouseEventArgs args) {
                if (args.Button == MouseButtons.Left) noneSelected();
            };
            list.Margin = new Padding(10);
            populate(currentPath);

            Resize += delegate {
                optionsBar.Width = Width;
                optionsBar.runResize();
                list.Height = Height - backButton.Bottom;
                setButtonsWidth();
                pathBar.Width = Width;
            };

            add(list);

            Shown += delegate {
                loadingImage.SendToBack();
                optionsBar.Height = 30;
                optionsBar.Width = Width;
                optionsBar.Top = 70;
            };

            MouseClick += delegate (object o, MouseEventArgs args) {
                if (args.Button == MouseButtons.XButton1) goBack();
                if (args.Button == MouseButtons.Left) noneSelected();
            };
            formClick += delegate (object o, MouseEventArgs args) {
                if (args.Button == MouseButtons.XButton1) goBack();
            };
            this._(() => { setButtonsWidth(); });
        }

        private void setButtonsWidth() {
            foreach (Control c in list.Controls)
                if (c is FolderButton) c.MinimumSize = new Size(Math.Max(500, (Width - 50) / 2), c.MinimumSize.Height);
        }

        private int mostRecent = -1;
        private PictureBox loadingImage;
        private OptionsBar optionsBar;
        private TextBox pathBar;
        private BackButton backButton;
        private FlowLayoutPanel list;

        private static bool loading = true;
        private string currentPath = string.Empty;

        protected void populate(DirectoryInfo path) {
            loading = false;
            Thread.Sleep(100);
            loading = true;
            this.runOnUiThread(() => {
                Text = path.FullName;
                pathBar.Text = Text;
                if (!backButton.Visible) backButton.Visible = true;
                loadingImage.Visible = true;
                setAnimations(false);
                Invalidate();
            });
            currentPath = path.FullName;
            //SuspendLayout();
            list.Controls.Clear();
            F.async(() => {
                try {
                    MouseEventHandler click = delegate (object o, MouseEventArgs args) {
                        try {
                            if ((ModifierKeys & Keys.Control) != Keys.Control && (ModifierKeys & Keys.Shift) != Keys.Shift)
                                foreach (Control c in list.Controls) if (c is FolderButton) ((FolderButton)c).setSelected(false);
                            if ((ModifierKeys & Keys.Shift) == Keys.Shift) {
                                if (mostRecent == -1) goto selectSingle;
                                int curr = list.Controls.IndexOf((Control)o);
                                if (curr > mostRecent) for (int i = mostRecent; i <= curr; i++)
                                        ((FolderButton)list.Controls[i]).setSelected();
                                else for (int i = mostRecent; i >= curr; i--)
                                        ((FolderButton)list.Controls[i]).setSelected();
                            } else if (o is FolderButton) {
                                goto selectSingle;
                            }
                            return;

                            selectSingle:
                            ((FolderButton)o).setSelected();
                            mostRecent = list.Controls.IndexOf((Control)o);
                        } catch (Exception e) {

                        }
                    };
                    IEnumerable<string> items = Directory.EnumerateFileSystemEntries(path.FullName, "*", SearchOption.TopDirectoryOnly);
                    byte x = 0;
                    foreach (string info in items) {
                        if (x++ >= 5) {
                            this.runOnUiThread(() => {
                                list.PerformLayout();
                            });
                            x = 0;
                        }
                        if (!loading) return;
                        Thread.Sleep(5);
                        this.runOnUiThread(() => {
                            try {
                                FolderButton button = new FolderButton(info);
                                button.AutoSize = true;
                                button.showAnimations = false;
                                button.MinimumSize = new Size(Math.Max(500, (Width - 50) / 2), button.MinimumSize.Height);
                                button.MouseClick += click;
                                button.MouseDoubleClick += delegate (object o, MouseEventArgs args) {
                                    if (args.Button == MouseButtons.Left) {
                                        if (File.Exists(info)) {
                                            this._(() => { ((FolderButton)o).setSelected(false); });
                                            Process.Start(info);
                                        } else populate(info);
                                    }
                                };
                                list.Controls.Add(button);
                                add(button, false);
                            } catch (Exception e) {

                            }
                        });
                    }
                } catch (Exception) {

                }
                this.runOnUiThread(() => {
                    loadingImage.Hide();
                    setAnimations();
                });
            });
        }

        public void populate(string path) {
            if (path != null && !path.Equals(string.Empty)) populate(new DirectoryInfo(path));
            else loadDrives();
        }

        private void setAnimations(bool enabled = true) {
            foreach (Control c in list.Controls) if (c is FolderButton) ((FolderButton)c).showAnimations = enabled;
        }

        private void loadDrives() {
            try {
                this.runOnUiThread(() => { Text = defaultText; });
                list.Controls.Clear();
                this.runOnUiThread(() => { loadingImage.Visible = true; backButton.Visible = false; });
                foreach (DriveInfo di in DriveInfo.GetDrives()) {
                    FolderButton b = new FolderButton();
                    b.Text = string.Format("{0} ({1})", di.ToString(), di.Name);
                    b.AutoSize = true;
                    b.DoubleClick += delegate {
                        populate(di.RootDirectory);
                    };
                    list.Controls.Add(b);
                }
                this.runOnUiThread(() => { loadingImage.Visible = false; });
            } catch (Exception e) { }
        }

        private void noneSelected() {
            foreach (Control c in list.Controls) if (c is FolderButton) ((FolderButton)c).setSelected(false);
        }

        private void goBack() {
            F.async(() => {
                loading = false;
                Thread.Sleep(10); //Tell any running process to stop, then re-allow the next process
                loading = true;
                list.runOnUiThread(() => { list.Controls.Clear(); });
                this.runOnUiThread(() => {
                    if (!string.IsNullOrEmpty(currentPath))
                        populate(Path.GetDirectoryName(currentPath));
                });
            });
        }

        protected override void WndProc(ref Message m) {
            bool h = false;
            if (m.Msg == 0x100 || m.Msg == 0x104) {
                if (m.WParam == o) {
                    this.f();
                    h = true;
                }
            }
            if (!h) base.WndProc(ref m);
        }
    }
}
