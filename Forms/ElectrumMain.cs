using Electrum.Controls;
using Global;
using MaterialSkin.Controls;
using System.Drawing;
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
            openFileButton = new MaterialRaisedButton();
            openFileButton.AutoSize = true;
            openFileButton.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            openFileButton.MouseState = MaterialSkin.MouseState.HOVER;
            openFileButton.Text = "Open A File";
            openFileButton.Font = new Font("Product Sans", 15f);
            openFileButton.UseCompatibleTextRendering = false;
            openFileButton.Location = new Point(20, 10 + t);
            openFileButton.Padding = new Padding(20, 10, 20, 10);
            openFileButton.setTextColor(Color.FromArgb(255, 238, 88));
            openFileButton.setBackColor(KeithApps.grayColor());
            openFileButton.Primary = true;
            openFileButton.Click += this.f;

            optionsBar.setOptions(new OptionsBar.Option[] {
                new OptionsBar.Option { title = "Open File", onClick = () => { this.f(); } },
                new OptionsBar.Option { title = "Exit", onClick = () => { Close(); } },
                new OptionsBar.Option {holdRight = true, title = "Right",
                    onClick = () => { } },
                new OptionsBar.Option {holdRight = true, title = "Right 2",
                    onClick = () => { } }

            });
            optionsBar.setBackgroundColor(Color.FromArgb(0x50, 0x50, 0x50));
            Controls.Add(optionsBar);

            Controls.Add(openFileButton);
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
        private MaterialRaisedButton openFileButton;

    }
}
