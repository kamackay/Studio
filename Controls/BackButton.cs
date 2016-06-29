using MaterialSkin;
using System.Windows.Forms;
using MaterialSkin.Animations;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Text;
using System.Drawing.Drawing2D;

namespace Electrum.Controls {
    public class BackButton : ElectrumControl, IMaterialControl {

        public BackButton() : base() {
            Size = new Size(50, 50);
            Primary = true;

            img = Properties.Resources.back_sm;
            r = new Rectangle(new Point(5, 5), new Size(40, 40));

            animationManager = new AnimationManager(false) {
                Increment = 0.03,
                AnimationType = AnimationType.EaseOut
            };
            hoverAnimationManager = new AnimationManager {
                Increment = 0.07,
                AnimationType = AnimationType.EaseOut
            };

            hoverAnimationManager.OnAnimationProgress += sender => Invalidate();
            animationManager.OnAnimationProgress += sender => Invalidate();

            AutoSize = false;
            Margin = new Padding(4, 6, 4, 6);
            Padding = new Padding(0);
        }

        private Bitmap img;
        private Rectangle r;

        private readonly AnimationManager animationManager;
        private readonly AnimationManager hoverAnimationManager;

        [Browsable(false)]
        public int Depth { get; set; }
        [Browsable(false)]
        public MaterialSkinManager SkinManager { get { return MaterialSkinManager.Instance; } }
        [Browsable(false)]
        public MouseState MouseState { get; set; }
        public bool Primary { get; set; }

        /// <summary>
        /// The Draw event
        /// </summary>
        /// <param name="e">The Paint Event Arguments</param>
        protected override void OnPaint(PaintEventArgs e) {
            var g = e.Graphics;
            g.TextRenderingHint = TextRenderingHint.AntiAlias;

            g.Clear(Parent.BackColor);

            /**///Hover
            Color c = SkinManager.GetFlatButtonHoverBackgroundColor();
            using (Brush b = new SolidBrush(Color.FromArgb((int)(hoverAnimationManager.GetProgress() * c.A), c.RemoveAlpha())))
                g.FillRectangle(b, ClientRectangle);/**/

            /**///Ripple
            if (animationManager.IsAnimating()) {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                for (int i = 0; i < animationManager.GetAnimationCount(); i++) {
                    var animationValue = animationManager.GetProgress(i);
                    var animationSource = animationManager.GetSource(i);

                    using (Brush rippleBrush = new SolidBrush(Color.FromArgb((int)(101 - (animationValue * 100)), Color.Black))) {
                        var rippleSize = (int)(animationValue * Width * 2);
                        g.FillEllipse(rippleBrush, new Rectangle(animationSource.X - rippleSize / 2, animationSource.Y - rippleSize / 2, rippleSize, rippleSize));
                    }
                }
                g.SmoothingMode = SmoothingMode.None;
            }/**/
            if (img != null) g.DrawImage(img, r);
        }

        /// <summary>
        /// When the Control is Created
        /// </summary>
        protected override void OnCreateControl() {
            base.OnCreateControl();
            if (DesignMode) return;

            MouseState = MouseState.OUT;
            MouseEnter += (sender, args) => {
                MouseState = MouseState.HOVER;
                hoverAnimationManager.StartNewAnimation(AnimationDirection.In);
                Invalidate();
            };
            MouseLeave += (sender, args) => {
                MouseState = MouseState.OUT;
                hoverAnimationManager.StartNewAnimation(AnimationDirection.Out);
                Invalidate();
            };
            MouseDown += (sender, args) => {
                if (args.Button == MouseButtons.Left) {
                    MouseState = MouseState.DOWN;

                    animationManager.StartNewAnimation(AnimationDirection.In, args.Location);
                    Invalidate();
                }
            };
            MouseUp += (sender, args) => {
                MouseState = MouseState.HOVER;

                Invalidate();
            };
        }/**/
    }
}
