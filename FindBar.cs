using System;
using System.Drawing;
using System.Windows.Forms;

namespace Studio {
    public partial class FindBar : UserControl {
        public FindBar() {
            InitializeComponent();
            BackColor = ColorTranslator.FromHtml("#4C4A48");
            textBox1.BackColor = ColorTranslator.FromHtml("#4C4A48");
            textBox1.BorderStyle = BorderStyle.None;
            pictureBox1.Height = 20;
            textBox1.ForeColor = Color.White;
            textBox1.TextChanged += delegate {
                EventHandler<SearchEventArgs> handler = SearchChanged;
                if (handler != null) handler(this, new SearchEventArgs { text = textBox1.Text });
            };
            Height = 20;
            pictureBox1.MouseClick += delegate (object o, MouseEventArgs args) {
                if (args.Button == MouseButtons.Left) close();
            };
            VisibleChanged += delegate {
                if (Visible) {
                    textBox1.BorderStyle = BorderStyle.None;
                    pictureBox1.Size = new Size(30, textBox1.Height);
                    textBox1.Left = pictureBox1.Right + 10;
                    textBox1.Width = Width - pictureBox1.Width - 25;
                    textBox1.Font = new Font(textBox1.Font.FontFamily, Height / 2);
                    textBox1.Focus();
                }
            };
            LocationChanged += delegate {
                pictureBox1.runOnUiThread(() => {
                    pictureBox1.Dock = DockStyle.Left;
                    pictureBox1.Size = new Size(30, 32);
                    pictureBox1.Location = new Point(0, 0);
                });
            };
        }
        public void close() {
            Visible = false;
        }
        public event EventHandler<SearchEventArgs> SearchChanged;

        public class SearchEventArgs : EventArgs {
            public string text { get; set; }
        }
    }
}
