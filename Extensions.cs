using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Diagnostics;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Threading;
using Electrum.Utils;

namespace Electrum {
    public static class Extensions {

        /// <summary>
        /// Resize the image to the specified width and height.
        /// </summary>
        /// <param name="image">The image to resize.</param>
        /// <param name="width">The width to resize to.</param>
        /// <param name="height">The height to resize to.</param>
        /// <returns>The resized image.</returns>
        public static Bitmap ResizeImage(this Image image, int width, int height) {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage)) {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes()) {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        public static bool isEmpty(this string s) {
            if (s == null) return true;
            return s.Equals(string.Empty);
        }

        public static bool isDirectory(this string path) {
            FileAttributes attr = File.GetAttributes(path);
            return attr.HasFlag(FileAttributes.Directory);
        }

        public static void runOnUiThread(this Control control, Action runnable) {
            try {
                if (control.InvokeRequired) control.Invoke(runnable);
                else runnable.Invoke();
            } catch (Exception e) {
                Log.log(e.Message);
            }
        }

        public static void emptyControls(this Panel panel) {
            panel.removeAllControls();
        }

        public static void removeAllControls(this Panel panel) {
            foreach (Control c in panel.Controls) {
                if (!(c is VScrollBar)) {
                    c.Visible = false;
                    c.Invalidate();
                }
            }
            panel.Invalidate();
        }

        public static bool IsBeingALittleShit(this Process p, StringCollection WhiteList) {
            if (WhiteList.Contains(p.ProcessName.ToLower()))
                return false;
            else if (p.ProcessName.Contains("System")) return false;
            else if (p.WorkingSet64 > (419430400)) return true;
            else if (!p.Responding && p.WorkingSet64 > (104857600)) return true;
            else return !p.Responding;
        }

        public static string[] getFolderContents(this DirectoryInfo directory) {
            return Directory.EnumerateFileSystemEntries(directory.FullName).ToArray();
        }

        public static bool IsFolderOrFile(this string s) {
            try { return (s[2] == '\\' && char.IsLetter(s[0]) && s[1] == ':'); } catch (Exception) { return false; }
        }

        public static bool IsURL(this string s) {
            Uri result;
            return (Uri.TryCreate(s, UriKind.Absolute, out result) && result.Scheme == Uri.UriSchemeHttp)
                || (s.StartsWith("www.") && (s.Contains(".com") || s.Contains(".edu") || s.Contains(".org")));
        }

        public static void appendText(this RichTextBox textBox, string text, Color c) {
            textBox.SelectionStart = textBox.TextLength;
            textBox.SelectionLength = 0;
            textBox.SelectionColor = c;
            textBox.AppendText(text);
        }

        public static void appendLine(this RichTextBox textBox, string text, Color? c = null) {
            textBox.SelectionStart = textBox.TextLength;
            textBox.SelectionLength = 0;
            textBox.AppendText(text + "\n");
            textBox.SelectionColor = c ?? Color.White;
        }

        /// <summary>
        /// Color all instances of a string 
        /// </summary>
        /// <param name="box">the RichTextBox</param>
        /// <param name="text">The string to search for</param>
        /// <param name="c">The Color to paint that string</param>
        public static void colorAll(this RichTextBox box, string text, Color c) {
            box.runOnUiThread(() => {
                Match result = Regex.Match(box.Text, text, RegexOptions.Multiline);
                while (result.Success) {
                    box.SelectionStart = result.Index;
                    box.SelectionLength = result.Length;
                    while (!char.IsLetterOrDigit(box.SelectedText[0])) {
                        box.SelectionStart++;
                        box.SelectionLength--;
                    }
                    while (!char.IsLetterOrDigit(box.SelectedText[box.SelectionLength - 1]))
                        box.SelectionLength--;
                    box.SelectionColor = c;
                    result = result.NextMatch();
                }
                box.SelectionLength = 0;
            });
        }


        public static void exit(this Form f) {
            F.runAsync(() => {
                Thread.Sleep(100);
                f.runOnUiThread(() => { f.Close(); });
                StudioContext.getCurrentInstance().formClosed(f);
            });

        }

        public static void colorStrings(this RichTextBox box, Color? color = null) {
            Color c = color ?? Color.FromArgb(74, 166, 53);
            box.runOnUiThread(() => {
                Match m = Regex.Match(box.Text, "\"[^\"\\\\]*(?:\\\\.[^\"\\\\]*)*\"", RegexOptions.Multiline);
                while (m.Success) {
                    box.SelectionStart = m.Index;
                    box.SelectionLength = m.Length;
                    box.SelectionColor = c;
                    m = m.NextMatch();
                }
                m = Regex.Match(box.Text, "'[^'\\\\]*(?:\\\\.[^'\\\\]*)*'", RegexOptions.Multiline);
                while (m.Success) {
                    box.SelectionStart = m.Index;
                    box.SelectionLength = m.Length;
                    box.SelectionColor = c;
                    m = m.NextMatch();
                }
                box.SelectionLength = 0;
            });
        }

        public static void scrollToTop(this RichTextBox box) {
            box.runOnUiThread(() => {
                box.SelectionStart = 0;
                box.SelectionLength = 0;
            });
        }

        public static bool contains(this string[] arr, string s) {
            try {
                foreach (string temp in arr)
                    try {
                        if (temp.Equals(s)) return true;
                    } catch { }
            } catch { }
            return false;
        }

        /// <summary>
        /// List of colors Contains this color
        /// </summary>
        /// <param name="list"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        public static bool contains(this List<Color> list, Color color) {
            try {
                int argb = color.ToArgb();
                foreach (Color col in list) if (col.ToArgb() == argb) return true;
            } catch { }
            return false;
        }

        /// <summary>
        /// Get a color that does not exist anywhere in the Bitmap
        /// </summary>
        /// <param name="img"></param>
        public static Color findMissingColor(this Bitmap img) {
            List<Color> colors = new List<Color>();
            for (int x = 0; x < img.Width; x++) {
                for (int y = 0; y < img.Height; y++) {
                    Color c = img.GetPixel(x, y);
                    if (!colors.contains(c)) colors.Add(c);
                }
            }
            for (int a = 0; a < 256; a++) for (int b = 0; b < 256; b++) for (int c = 0; c < 256; c++) if (!colors.contains(Color.FromArgb(a, b, c))) return Color.FromArgb(a, b, c);
            return Color.Black;
        }

        public static bool isMostlyBlack(this Bitmap image) {
            int darkPixels = 0, calculated = 0;
            for (int x = 0; x < image.Width; x++) {
                for (int y = 0; y < image.Height; y++) {
                    Color c = image.GetPixel(x, y);
                    if (c.isDark()) darkPixels++;
                    if (c.A > 128) calculated++;
                }
            }
            return darkPixels * 2 > calculated;
        }

        public static bool isDark(this Color c) {
            return c.A > 128 && c.R < 128 && c.G < 128 && c.B < 128;
        }

        /// <summary>
        /// Open a window to open a file, then handle that file
        /// </summary>
        public static void f(this Form f, object ob = null, EventArgs args = null) {
            f._(() => {
                OpenFileDialog o = new OpenFileDialog();
                o.InitialDirectory = @"C:\";
                o.Multiselect = false;
                if (o.ShowDialog() == DialogResult.OK) {
                    StudioContext.getCurrentInstance().openFile(o.FileName, f);
                    f.Close();
                }
            });
        }

        public static string[] trimAll(this string[] strings, bool removeQuotes = true) {
            for (int i = 0; i < strings.Length; i++) strings[i] = strings[i].Replace("\"", "").Trim(); return strings;
        }

        public static void onClick(this Control control, Action a) { control.Click += delegate { a(); }; }

        public static string toHex(this Color c) {
            return "#" +c.A.ToString("X2") + c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2");
        }

    }

    public static class KeithApps {
        public static Color grayColor() {
            return ColorTranslator.FromHtml("#4C4A48");
        }
        public static Color darkGrayColor() {
            return ColorTranslator.FromHtml("#252423");
        }
    }
}
