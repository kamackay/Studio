using MaterialSkin;
using MaterialSkin.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace Electrum {
    public abstract partial class KeithForm : MaterialForm {
        public KeithForm() {
            InitializeComponent();
            init();
            materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = MaterialSkinManager.Themes.DARK;
            materialSkinManager.ColorScheme = new ColorScheme(Primary.BlueGrey800, Primary.BlueGrey900, Primary.BlueGrey500, Accent.LightBlue200, TextShade.WHITE);
        }
        private readonly MaterialSkinManager materialSkinManager;
        public Action initialActions = null;

        private void init() {
            F.runAsync(() => {
                try {
                    if (initialActions != null) {
                        Thread.Sleep(100);
                        initialActions.Invoke();
                    }
                } catch { }
            });
        }

        public event EventHandler<FormOpenEventArgs> subFormOpened;

        public class FormOpenEventArgs : EventArgs {
            public Form subForm { get; set; }
        }


        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

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

        [DllImport("user32.dll")]
        public static extern new int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImport("user32.dll")]
        public static extern new bool ReleaseCapture();

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            components = new Container();
            AutoScaleMode = AutoScaleMode.Font;
            StartPosition = FormStartPosition.CenterScreen;
            Size screenSize = Screen.PrimaryScreen.WorkingArea.Size;
            Size = new Size((int)(screenSize.Width * .75), (int)(screenSize.Height * .75));
            Padding = new Padding(0);
            Icon = Properties.Resources.keithapps;
            KeyPreview = true;
            KeyPress += delegate (object o, KeyPressEventArgs args) {
                this._(() => { });
            };
        }


        protected override void WndProc(ref Message m) {
            bool handled = false;
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


        const uint WM_NCHITTEST = 0x0084;
        const int RESIZE_HANDLE_SIZE = 10;
        const uint HTLEFT = 10;
        const uint HTRIGHT = 11;
        const uint HTBOTTOMRIGHT = 17;
        const uint HTBOTTOM = 15;
        const uint HTBOTTOMLEFT = 16;
        const uint HTTOP = 12;
        const uint HTTOPLEFT = 13;
        const uint HTTOPRIGHT = 14;
    }
}
