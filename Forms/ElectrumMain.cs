using Electrum.Controls;
using Global;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace Electrum {
    public partial class ElectrumMain : KeithForm {
        public ElectrumMain() {
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

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            components = new System.ComponentModel.Container();
            AutoScaleMode = AutoScaleMode.Font;
            KeyPreview = true;
            Text = "Welcome to Electrum Studios";
            optionsBar = new OptionsBar();
            list = new FlowLayoutPanel();

            backButton = new BackButton();
            backButton.Size = new Size(50, 50);
            backButton.Location = new Point(0, 100);
            backButton.Visible = false;
            backButton.Click += delegate {
                F.async(() => {
                    loading = false;
                    Thread.Sleep(10);
                    list.runOnUiThread(() => { list.Controls.Clear(); });
                    this.runOnUiThread(() => { populate(Path.GetDirectoryName(currentPath)); });
                });
            };

            Controls.Add(backButton);

            optionsBar.setOptions(new OptionsBar.Option[] {
                new OptionsBar.Option { title = "Open File", onClick = () => { this.f(); } },
                new OptionsBar.Option { title = "Exit", onClick = () => { Close(); } },
                new OptionsBar.Option {holdRight = true, title = "Right",
                    onClick = () => { } }/*,
                new OptionsBar.Option {holdRight = true, title = "Right 2",
                    onClick = () => { } }/**/
            });
            optionsBar.setBackgroundColor(Color.FromArgb(0x50, 0x50, 0x50));
            Controls.Add(optionsBar);

            list.Dock = DockStyle.Bottom;
            list.Height = Height - 150;
            list.FlowDirection = FlowDirection.TopDown;
            list.AutoScroll = true;
            populate(currentPath);

            Resize += delegate {
                optionsBar.Width = Width;
                optionsBar.runResize();
                list.Height = Height - 150;
            };

            Controls.Add(list);

            Shown += delegate {
                optionsBar.Height = 30;
                optionsBar.Width = Width;
                optionsBar.Top = 70;
            };
        }

        private bool loading = true;
        private string currentPath = string.Empty;

        protected void populate(DirectoryInfo path) {
            this.runOnUiThread(() => { if (!backButton.Visible) backButton.Visible = true; });
            currentPath = path.FullName;
            //SuspendLayout();
            list.Controls.Clear();
            F.async(() => {
                try {
                    foreach (string info in Directory.EnumerateFileSystemEntries(path.FullName, "*", SearchOption.TopDirectoryOnly)) {
                        if (!loading) break;
                        Thread.Sleep(5);
                        this.runOnUiThread(() => {
                            FolderButton button = new FolderButton(info);
                            button.AutoSize = true;
                            button.DoubleClick += delegate {
                                if (File.Exists(info)) Process.Start(info);
                                else populate(info);
                            };
                            list.Controls.Add(button);
                        });
                    }
                } catch (Exception) {

                }
                //this.runOnUiThread(() => { ResumeLayout(); });
            });
        }

        protected void populate(string path) {
            if (path != null && !path.Equals(string.Empty)) populate(new DirectoryInfo(path));
            else loadDrives();
        }

        private void loadDrives() {
            list.Controls.Clear();
            foreach (DriveInfo di in DriveInfo.GetDrives()) {
                FolderButton b = new FolderButton();
                b.Text = string.Format("{0} ({1})", di.ToString(), di.Name);
                b.AutoSize = true;
                b.DoubleClick += delegate {
                    populate(di.RootDirectory);
                };
                list.Controls.Add(b);
            }
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

        private OptionsBar optionsBar;
        private BackButton backButton;
        private FlowLayoutPanel list;
    }
}
