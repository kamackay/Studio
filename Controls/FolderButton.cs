using System;
using MaterialSkin;
using System.ComponentModel;
using MaterialSkin.Animations;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using Global;

namespace Electrum.Controls {
    public class FolderButton : Label, IMaterialControl {
        [Browsable(false)]
        public int Depth { get; set; }
        [Browsable(false)]
        public MaterialSkinManager SkinManager { get { return MaterialSkinManager.Instance; } }
        [Browsable(false)]
        public MouseState MouseState { get; set; }
        private readonly AnimationManager animationManager;
        public bool Primary { get; set; }
        public bool textAllCaps = false;

        private bool customFontColor = false, customButtonColor = false;
        private Brush fontBrush = null, backgroundBrush = null;


        public FolderButton(string path = null) {
            Primary = true;
            MinimumSize = new Size(500, 10);
            
            Padding = new Padding(15);

            MouseEnter += delegate (object o, EventArgs args) {
                animationManager.StartNewAnimation(AnimationDirection.In, new Point(Width / 2, Height / 2));
            };

            animationManager = new AnimationManager(false) {
                Increment = 0.03,
                AnimationType = AnimationType.EaseOut
            };
            animationManager.OnAnimationProgress += sender => Invalidate();

            if (path != null) getInfo(path);
        }

        private void getInfo(string path) {
            Text = path;

            F.async(() => {
                long bytes = 0;
                string unit = "Bytes";
                if (File.Exists(path)) bytes = new FileInfo(path).Length;
                else bytes = Tools.getFolderBytes(path);
                this.runOnUiThread(() => { Text = string.Format("{0}   {1:n}{3}", path, bytes, unit); });
            });
        }

        protected override void OnMouseUp(MouseEventArgs mevent) {
            base.OnMouseUp(mevent);

            animationManager.StartNewAnimation(AnimationDirection.In, mevent.Location);
        }

        protected override void OnPaint(PaintEventArgs pevent) {
            var g = pevent.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.AntiAlias;

            g.Clear(Parent.BackColor);

            using (var backgroundPath = DrawHelper.CreateRoundRect(ClientRectangle.X,
                ClientRectangle.Y,
                ClientRectangle.Width - 1,
                ClientRectangle.Height - 1,
                1f)) {
                g.FillPath(customButtonColor ? backgroundBrush : (Primary ? SkinManager.ColorScheme.PrimaryBrush : SkinManager.GetRaisedButtonBackgroundBrush()), backgroundPath);
            }

            if (animationManager.IsAnimating()) {
                for (int i = 0; i < animationManager.GetAnimationCount(); i++) {
                    var animationValue = animationManager.GetProgress(i);
                    var animationSource = animationManager.GetSource(i);
                    var rippleBrush = new SolidBrush(Color.FromArgb((int)(51 - (animationValue * 50)), Color.Black));
                    var rippleSize = (int)(animationValue * Width * 2);
                    g.FillEllipse(rippleBrush, new Rectangle(animationSource.X - rippleSize / 2, animationSource.Y - rippleSize / 2, rippleSize, rippleSize));
                }
            }

            g.DrawString(textAllCaps ? Text.ToUpper() : Text, SkinManager.ROBOTO_MEDIUM_10, customFontColor ? fontBrush : SkinManager.GetRaisedButtonTextBrush(Primary, false), 0, (Height-Font.Height)/2);
        }
    }
}
