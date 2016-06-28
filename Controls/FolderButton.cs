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
using System.Threading;
using Etier.IconHelper;

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
        public bool showAnimations { get; set; } = true;
        private Brush fontBrush = null, backgroundBrush = null;

        private Icon img = null;

        public FolderButton(string path = null) {
            Primary = true;
            MinimumSize = new Size(500, 10);

            Text = path;

            Padding = new Padding(15);
            Margin = new Padding(5);

            MouseEnter += delegate (object o, EventArgs args) {
                if (showAnimations) animationManager.StartNewAnimation(AnimationDirection.In, PointToClient(Cursor.Position));
            };

            animationManager = new AnimationManager(false) {
                Increment = 0.03,
                AnimationType = AnimationType.EaseOut
            };
            animationManager.OnAnimationProgress += sender => Invalidate();

            if (path != null) getInfo(path);
        }

        private void getInfo(string path) {
            F.async(() => {
                setText(path);
                Thread.Sleep(new Random().Next(1000));
                double bytes = 0;
                short unit = 0;
                if (File.Exists(path)) bytes = new FileInfo(path).Length;
                else bytes = Tools.getFolderBytes(path);
                if (bytes == -1) return;
                while (bytes > 1024) {
                    bytes /= 1024;
                    unit++;
                }
                setText(string.Format("{0}   {1:#,0.0##} {2}", path, bytes, Tools.getUnitString(unit)));
            });
            F.async(() => {
                if (Directory.Exists(path)) {
                    img = Properties.Resources.folder;
                } else {
                    img = IconReader.GetFileIcon(Path.GetExtension(path), IconReader.IconSize.Large, false);
                }
            });
        }

        private void setText(string newText) {
            int w = TextRenderer.MeasureText(newText, Font).Width + 100;
            this.runOnUiThread(() => {
                Text = newText;
                Width = w;
                Invalidate();
            });
        }

        protected override void OnMouseUp(MouseEventArgs mevent) {
            base.OnMouseUp(mevent);
            if (showAnimations) animationManager.StartNewAnimation(AnimationDirection.In, mevent.Location);
        }

        protected override void OnPaint(PaintEventArgs pevent) {
            var g = pevent.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.AntiAlias;

            g.Clear(Parent.BackColor);
            using (var backgroundPath = DrawHelper.CreateRoundRect(ClientRectangle.X,
                ClientRectangle.Y,
                ClientRectangle.Width - 1,
                ClientRectangle.Height - 1, 1f))
                g.FillPath(customButtonColor ? backgroundBrush : (Primary ? SkinManager.ColorScheme.PrimaryBrush : SkinManager.GetRaisedButtonBackgroundBrush()), backgroundPath);

            if (showAnimations && animationManager.IsAnimating()) {
                for (int i = 0; i < animationManager.GetAnimationCount(); i++) {
                    var animationValue = animationManager.GetProgress(i);
                    var animationSource = animationManager.GetSource(i);
                    var rippleBrush = new SolidBrush(Color.FromArgb((int)(51 - (animationValue * 50)), Color.Black));
                    var rippleSize = (int)(animationValue * Width * 2);
                    g.FillEllipse(rippleBrush, new Rectangle(animationSource.X - rippleSize / 2, animationSource.Y - rippleSize / 2, rippleSize, rippleSize));
                }
            }
            g.DrawString(textAllCaps ? Text.ToUpper() : Text, SkinManager.ROBOTO_MEDIUM_10,
                customFontColor ? fontBrush : SkinManager.GetRaisedButtonTextBrush(Primary, false), img != null ? Padding.Left + img.Width + 10 : Padding.Left, (Height - Font.Height) / 2);
            if (img != null) {
                //g.DrawImage(Properties.Resources.circle_white, new Rectangle(new Point(Padding.Left, Padding.Top), new Size(Height, Height)));
                int y = (Height - img.Height) / 2;
                g.DrawIcon(img, 5, y);
            }
        }
    }
}
