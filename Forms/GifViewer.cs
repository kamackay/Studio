using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;

namespace Electrum {
    partial class GifViewer : ElectrumForm {
        public GifViewer() {
            InitializeComponent();
            init();
            //Select Image
        }

        private GifImage gifImage = null;
        private Thread loadThread = null;

        public GifViewer(string filename) {
            InitializeComponent();
            init();
            loadImage(filename);
        }

        protected void init() {
            FormClosing += delegate {
                StudioContext.getCurrentInstance().formClosed(this);
            };
        }

        private void loadImage(string filename) {
            gifImage = new GifImage(filename);
            gifImage.ReverseAtEnd = false; //dont reverse at end
            if (loadThread != null) {
                try {
                    loadThread.Abort();
                    loadThread = null;
                } catch { } finally { loadThread = null; }
            }
            loadThread = F.runAsync(() => {
                image.runOnUiThread(() => { image.Image = gifImage.getNextFrame(); });
                Thread.Sleep(10);
                if (image.Image == null)
                    image.runOnUiThread(() => { image.Image = gifImage.getNextFrame(); });
                this.runOnUiThread(() => { try { Height = image.Image.Height + 40 + image.Top; Width = image.Image.Width + 40; } catch { } });
                /*
                while (runAnim) {
                    try {
                        Thread.Sleep(100);
                    } catch { }
                }/**/
            });
            Text = System.IO.Path.GetFileName(filename);
        }

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            image = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(image)).BeginInit();
            SuspendLayout();
            // 
            // pictureBox1
            // 
            image.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right);
            image.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            image.Location = new Point(20, 80);
            image.Name = "image";
            image.Size = new Size(610, 429);
            image.TabIndex = 0;
            image.TabStop = false;
            // 
            // GifViewer
            // 
            AutoScaleDimensions = new SizeF(6F, 13F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new Size(610, 429);
            Controls.Add(image);
            Name = "GifViewer";
            Text = "GifViewer";
            ((System.ComponentModel.ISupportInitialize)(image)).EndInit();
            ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox image;
    }

    public class GifImage {
        private Image gifImage;
        private FrameDimension dimension;
        private int frameCount;
        private int currentFrame = -1;
        private bool reverse = false;
        private int step = 1;

        public GifImage(string path) {
            gifImage = Image.FromFile(path);
            //initialize
            dimension = new FrameDimension(gifImage.FrameDimensionsList[0]);
            //gets the GUID
            //total frames in the animation
            frameCount = gifImage.GetFrameCount(dimension);
        }

        public bool ReverseAtEnd {
            //whether the gif should play backwards when it reaches the end
            get { return reverse; }
            set { reverse = value; }
        }

        public Image getNextFrame() {

            currentFrame += step;

            //if the animation reaches a boundary...
            if (currentFrame >= frameCount || currentFrame < 1) {
                if (reverse) {
                    step *= -1;
                    //...reverse the count
                    //apply it
                    currentFrame += step;
                } else {
                    currentFrame = 0;
                    //...or start over
                }
            }
            return GetFrame(currentFrame);
        }

        public Image GetFrame(int index) {
            gifImage.SelectActiveFrame(dimension, index);
            //find the frame
            return (Image)gifImage.Clone();
            //return a copy of it
        }
    }
}
