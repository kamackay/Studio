using System.Windows.Forms;

namespace Electrum.Controls {
    public abstract class ElectrumControl : UserControl {
        public ElectrumControl() : base() {

        }

        public void runClick(MouseEventArgs args) { OnMouseClick(args); }

    }
}
