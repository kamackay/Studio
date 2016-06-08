﻿using MaterialSkin.Controls;
using System;
using Global;
using System.Drawing;
using System.Windows.Forms;

namespace Electrum.Controls {
    public class OptionsBar : UserControl {

        private Option[] options;

        public OptionsBar() {
            this._(() => {
                Form f = FindForm();
                Width = f.Width;
                Height = 30;
                Top = 70;
            });
        }

        public void setOptions(Option[] options) {
            this.options = options;
            this._(() => {
                int x = 0, x_r = Width;
                foreach (Option o in options) {
                    MaterialFlatButton button = new MaterialFlatButton();
                    button.Text = o.title;
                    button.Height = (int)(Height * .75);
                    button.Location = new Point(o.holdRight ? x_r : x, 0);
                    button.textAllCaps = false;
                    button.onClick(() => { o.run(); });
                    Controls.Add(button);
                    if (o.holdRight) {
                        x_r -= button.Width;
                        button.Location = new Point(x_r, 0);
                    } else x += button.Width;
                }
            });
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
