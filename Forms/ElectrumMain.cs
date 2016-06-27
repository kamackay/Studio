using Electrum.Controls;
using Global;
using MaterialSkin.Controls;
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
            foreach (DriveInfo di in DriveInfo.GetDrives()) {
                FolderButton b = new FolderButton();
                b.Text = di.Name;
                b.AutoSize = true;
                b.DoubleClick += delegate {
                    populate(di.RootDirectory);
                };
                list.Controls.Add(b);
            }

            Resize += delegate {
                optionsBar.Width = Width;
                optionsBar.runResize();
                list.Height = Height - 150;
            };

            Controls.Add(list);
        }

        protected void populate(DirectoryInfo path) {
            //SuspendLayout();
            list.Controls.Clear();
            F.async(() => {
                foreach (string info in Directory.EnumerateFileSystemEntries(path.FullName, "*", SearchOption.TopDirectoryOnly)) {
                    Thread.Sleep(5);
                    this.runOnUiThread(() => {
                        FolderButton button = new FolderButton();
                        button.AutoSize = true;
                        button.Text = info;
                        button.DoubleClick += delegate {
                            if (File.Exists(info)) Process.Start(info);
                            else populate(info);
                        };
                        list.Controls.Add(button);
                    });
                }
                //this.runOnUiThread(() => { ResumeLayout(); });
            });
        }

        protected void populate(string path) { populate(new DirectoryInfo(path)); }

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
        private FlowLayoutPanel list;
    }
}
