using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;
using System.Windows.Forms;

namespace Studio {
    partial class GifViewer : KeithForm {
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

        void init() {
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
            loadThread = Functions.runAsync(() => {
                image.runOnUiThread(() => { image.Image = gifImage.getNextFrame(); });
                Thread.Sleep(10);
                if (image.Image == null)
                    image.runOnUiThread(() => { image.Image = gifImage.getNextFrame(); });
                this.runOnUiThread(() => { try { Height = image.Image.Height + 40; Width = image.Image.Width + 20; } catch { } });
                /*
                while (runAnim) {
                    try {
                        Thread.Sleep(100);
                    } catch { }
                }/**/
            });
            Text = System.IO.Path.GetFileName(filename);
        }
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
