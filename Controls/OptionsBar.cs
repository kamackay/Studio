using MaterialSkin.Controls;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Electrum.Controls {
    public class OptionsBar : UserControl {

        private Option[] options;
        private Color textColor = Color.Black;

        public OptionsBar() {
            Resize += delegate {
                /*try {
                    Width = FindForm().Width;
                } catch { }/**/
            };
        }

        public void setOptions(Option[] options) {
            Controls.Clear();
            this.options = options;
            this._(() => {
                int x = 0, x_r = Width - 20;
                foreach (Option o in options) {
                    MaterialFlatButton button = new MaterialFlatButton();
                    button.Text = o.title;
                    button.setForeColor(textColor);
                    button.Height = (int)(Height * .75);
                    button.Location = new Point(o.holdRight ? x_r : x, 0);
                    if (o.holdRight)
                        Resize += delegate {
                            button.Left = Width - (button.Width + 20);
                        };/**/
                    button.textAllCaps = false;
                    button.onClick(() => { o.run(); });
                    Controls.Add(button);
                    if (o.holdRight) {
                        x_r -= button.Width;
                        button.Location = new Point(x_r, 0);
                    } else x += button.Width;
                }
            }, 1000);
        }

        public void runResize() { OnResize(new EventArgs()); }

        public void setTextColor(Color color) {
            textColor = color;
            foreach (Control c in Controls)
                if (c is MaterialFlatButton) ((MaterialFlatButton)c).setForeColor(color);
        }

        public void setBackgroundColor(Color c) {
            BackColor = c;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e) {
            base.OnPaint(e);

        }

        public class Option {
            public string title { get; set; } = "";
            public Action onClick { get; set; } = null;
            public bool holdRight { get; set; } = false;
            public void run() { onClick?.Invoke(); }
        }
    }
}
