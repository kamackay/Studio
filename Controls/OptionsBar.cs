using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Electrum.Controls {
    public class OptionsBar : UserControl {

        public OptionsBar() {
            this._(() => {
                Form f = FindForm();
                Width = f.Width;
            });
        }

        protected override void OnPaint(PaintEventArgs e) {
            base.OnPaint(e);

        }

        public class Option {
            private string title; private Action onClick;
            public Option(string title, Action onClick) {
                this.title = title;
                this.onClick = onClick;
            }
        }
    }
}
