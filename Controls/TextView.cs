using System.Windows.Forms;

namespace Electrum.Controls {
    public class TextView : ElectrumControl {
        public TextView() :base(){
            Text = "Just a Test";
        }

        protected override void OnPaint(PaintEventArgs e) {
            e.Graphics.DrawString(Text, Font, System.Drawing.Brushes.Black, new System.Drawing.PointF(0, 0));
            //base.OnPaint(e);
        }

        protected override void OnPaintBackground(PaintEventArgs pevent) {
            base.OnPaintBackground(pevent);
        }
    }
}
