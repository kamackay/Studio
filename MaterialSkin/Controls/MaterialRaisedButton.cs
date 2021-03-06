﻿using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;
using MaterialSkin.Animations;

namespace MaterialSkin.Controls {
    public class MaterialRaisedButton : Button, IMaterialControl {
        [Browsable(false)]
        public int Depth { get; set; }
        [Browsable(false)]
        public MaterialSkinManager SkinManager { get { return MaterialSkinManager.Instance; } }
        [Browsable(false)]
        public MouseState MouseState { get; set; }
        public bool Primary { get; set; }
        public bool textAllCaps = true;
        private readonly AnimationManager animationManager;

        private bool customFontColor = false, customButtonColor = false;
        private Brush fontBrush = null, backgroundBrush = null;

        public void setTextColor(Color c) {
            ForeColor = c;
            customFontColor = true;
            fontBrush = new SolidBrush(c);
        }

        public void setBackColor(Color c) {
            BackColor = c;
            customButtonColor = true;
            backgroundBrush = new SolidBrush(c);
        }

        public MaterialRaisedButton() {
            Primary = true;

            MouseEnter += delegate (object o, System.EventArgs args) {
                animationManager.StartNewAnimation(AnimationDirection.In, new Point(Width / 2, Height / 2));
            };

            animationManager = new AnimationManager(false) {
                Increment = 0.03,
                AnimationType = AnimationType.EaseOut
            };
            animationManager.OnAnimationProgress += sender => Invalidate();
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
                    var rippleBrush = new SolidBrush(Color.FromArgb((int)(51 - (animationValue * 50)), Color.White));
                    var rippleSize = (int)(animationValue * Width * 2);
                    g.FillEllipse(rippleBrush, new Rectangle(animationSource.X - rippleSize / 2, animationSource.Y - rippleSize / 2, rippleSize, rippleSize));
                }
            }

            g.DrawString(
                textAllCaps ? Text.ToUpper() : Text,
                SkinManager.ROBOTO_MEDIUM_10,
                customFontColor ? fontBrush : SkinManager.GetRaisedButtonTextBrush(Primary, false),
                ClientRectangle,
                new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
        }
    }
}
