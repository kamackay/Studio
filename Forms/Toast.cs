using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using System.Threading;
using System;

namespace Global {
    public class Toast : Form {
        public static void show(string message) {
            try {
                new Toast(message).Show();
            } catch (Exception) { }
        }
        System.Threading.Timer _timeoutTimer;
        private Toast(string message = "Toast") {
            _timeoutTimer = new System.Threading.Timer(OnTimerElapsed,
               null, timeout, Timeout.Infinite);
            init(message);
            text.Text = message;
        }

        Label text;
        private IContainer components = null;

        private void init(string message) {
            try {
                text = new Label();
                SuspendLayout();

                text.Dock = DockStyle.Fill;
                text.Font = Tools.getFont(15f);

                Controls.Add(text);
                FormBorderStyle = FormBorderStyle.None;
                ShowInTaskbar = false;
                TopMost = true;
                ShowIcon = false;
                BackColor = ColorTranslator.FromHtml(Tools.getTaskbarColor());
                Name = windowName;
                bool multiLine = false;
                int fifth = Screen.PrimaryScreen.WorkingArea.Width / 5, toastWidth = TextRenderer.MeasureText(message, text.Font).Width;
                if (fifth < toastWidth) {
                    toastWidth = fifth;
                    multiLine = true;
                }
                Cursor = Cursors.Hand;
                text.Cursor = Cursors.Hand;
                MouseEventHandler click = delegate (object sender, MouseEventArgs e) {
                    if (e.Button == MouseButtons.Left) {
                        kill();
                    }
                };
                MouseClick += click;
                text.MouseClick += click;
                Width = toastWidth + 50;
                Shown += delegate {
                    Height = multiLine ? 100 : 50;
                    Size s = Screen.PrimaryScreen.WorkingArea.Size;
                    Top = s.Height;
                    Left = (s.Width - Width) / 2;
                    new Thread(new ThreadStart(() => {
                        try {
                            for (int i = 0; i < (Height) / increment; i++) {
                                Thread.Sleep(animDelay);
                                this.runOnUiThread(() => {
                                    Top -= increment;
                                });
                            }
                        } catch (Exception) { kill(); }
                    })).Start();
                };

                ResumeLayout(false);
                PerformLayout();
            } catch (Exception) { kill(); }
        }

        const int timeout = 4000;
        const string windowName = "ElectrumToast";

        public void kill() {
            this.runOnUiThread(() => {
                running = false;
                Close();
            });
        }

        protected override bool ShowWithoutActivation {
            get {
                return true;
            }
        }

        private const int WM_MOUSEACTIVATE = 0x0021, MA_NOACTIVATE = 0x0003;

        protected override void WndProc(ref Message m) {
            if (m.Msg == WM_MOUSEACTIVATE) {
                m.Result = (IntPtr)MA_NOACTIVATE;
                return;
            }
            base.WndProc(ref m);
        }

        protected override void Dispose(bool disposing) {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private const int increment = 5, animDelay = 30;
        private bool running = true;
        void OnTimerElapsed(object state) {
            try {
                new Thread(new ThreadStart(() => {
                    try {
                        for (int i = 0; i < (Height) / increment; i++) {
                            if (!running) return;
                            Thread.Sleep(animDelay);
                            this.runOnUiThread(() => {
                                Top += increment;
                            });
                        }
                        kill();
                    } catch (Exception) { kill(); }
                })).Start();
            } catch (Exception) { kill(); }
        }
    }
}
