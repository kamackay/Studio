using MaterialSkin.Controls;
using Global;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;

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

        #region Windows Form Designer generated code

        const int t = 65;

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            components = new System.ComponentModel.Container();
            AutoScaleMode = AutoScaleMode.Font;
            Text = "Welcome to Electrum Studios";
            openFileButton = new MaterialRaisedButton();
            openFileButton.AutoSize = true;
            openFileButton.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            openFileButton.MouseState = MaterialSkin.MouseState.HOVER;
            openFileButton.Text = "Open A File";
            openFileButton.Font = new Font("Product Sans", 15f);
            openFileButton.UseCompatibleTextRendering = false;
            openFileButton.Location = new Point(20, 20 + t);
            openFileButton.Padding = new Padding(20, 10, 20, 10);
            openFileButton.ForeColor = Color.White;
            openFileButton.Primary = true;
            openFileButton.Click += this.f;
            Controls.Add(openFileButton);
        }
        private MaterialRaisedButton openFileButton;

        #endregion
    }
}
