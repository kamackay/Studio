using Global;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace Electrum.Forms {
    public class PhotoEditor : ElectrumForm {

        private string openFile;
        PictureBox pic, loadingImage;
        ListBox colors;
        Label colorsLabel, selectedColorLabel;
        Panel selectedColor;

        private MouseEventHandler itemClick = delegate (object o, MouseEventArgs args) {
            Control con = (Control)o;
            if (args.Button == MouseButtons.Right) {
                ContextMenu menu = new ContextMenu();
                menu.MenuItems.AddRange(new MenuItem[] {
                        new MenuItem("Delete This Color", delegate {
                            //Delete all pixels of that color
                            ((PhotoEditor)con.FindForm()).deleteColor();
                        })
                    });
                menu.Show(con, new Point(0, 0));
            }
        };

        public PhotoEditor(string filename) {
            loadingImage = new PictureBox();
            loadingImage.Image = Properties.Resources.material_loading;
            loadingImage.BackgroundImageLayout = ImageLayout.Zoom;
            loadingImage.BackColor = Color.Transparent;
            loadingImage.Location = new Point(0, 0);
            loadingImage.Size = new Size(100, 100);
            loadingImage.SizeMode = PictureBoxSizeMode.Zoom;
            Controls.Add(loadingImage);

            pic = new PictureBox();
            pic.Location = new Point(10, 100);
            pic.SizeMode = PictureBoxSizeMode.Zoom;
            pic.BackgroundImageLayout = ImageLayout.Zoom;
            Controls.Add(pic);

            colors = new ListBox();
            colors.Location = new Point(500, 150);
            colors.Size = new Size(250, 500);
            colors.Font = new Font("Arial", 12f);
            colors.MinimumSize = new Size(100, 100);
            colors.MouseDown += itemClick;
            Controls.Add(colors);

            colorsLabel = new Label();
            colorsLabel.Font = new Font("Arial", 12f);
            colorsLabel.Location = new Point(500, 100);
            colorsLabel.ForeColor = Color.White;
            colorsLabel.AutoSize = false;
            colorsLabel.Height = colorsLabel.PreferredHeight;
            colorsLabel.Width = 250;
            colorsLabel.Text = "Colors present in image";
            Controls.Add(colorsLabel);

            selectedColorLabel = new Label();
            selectedColorLabel.AutoSize = false;
            selectedColorLabel.Font = new Font("Arial", 12f);
            selectedColorLabel.Width = 250;
            selectedColorLabel.Location = new Point(700, 100);
            selectedColorLabel.ForeColor = Color.White;
            selectedColorLabel.Text = "Selected Color";
            Controls.Add(selectedColorLabel);

            selectedColor = new Panel();
            selectedColor.Size = new Size(100, 100);
            selectedColor.Location = new Point(700, 175);
            selectedColor.BackColor = Color.White;
            Controls.Add(selectedColor);

            MinimumSize = new Size(500, 500);

            pic.MouseClick += delegate (object o, MouseEventArgs args) {
                Color c = new Bitmap(pic.BackgroundImage).GetPixel(args.Location.X, args.Location.Y);
                selectedColor.BackColor = c;
                selectedColor.Invalidate();
                selectedColorLabel.Text = string.Format("Selected Color {0}{1}{0}[{2},{3}]", "\n    ", c.toHex(), args.Location.X, args.Location.Y);
                selectedColorLabel.Height = selectedColorLabel.PreferredHeight;
            };

            Resize += delegate {
                int left = pic.Left + pic.Width + 100;
                colors.Left = left;
                colorsLabel.Left = left;
                selectedColorLabel.Left = selectedColor.Left = left + 300;
                colors.Height = (int)(pic.Height * .75);
            };

            FormClosed += delegate {
                StudioContext.getCurrentInstance().formClosed(this);
            };

            LoadImage(filename);
        }

        private void LoadImage(string p) {
            showLoading();
            this.runOnUiThread(() => { BackColor = Color.Black; });
            try {
                if (!File.Exists(p)) Close();
                string ext = Path.GetExtension(p).ToLower();
                if (".gif".Equals(ext)) {/*
                    GifViewer viewer = new GifViewer(p);
                    viewer.FormClosed += delegate { Close(); };
                    viewer.Show();
                    onSubFormOpened(new FormOpenEventArgs(viewer));
                    this.exit();
                    return;/**/
                } else if (".svg".Equals(ext)) {
                    const string inkExe = @"c:\program files\inkscape\inkscape.exe";
                    if (!File.Exists(inkExe)) {
                        DialogResult result = MessageBox.Show("You need to install Inkscape in order to view that file. Install Now?", "Inkscape needed", MessageBoxButtons.YesNo);
                        if (result == DialogResult.Yes) {
                            //installInkscape();
                        } else Environment.Exit(0);
                    }
                    string png = Path.Combine(Tools.getDataFolder(), "temp.png");
                    string args = string.Format("-f \"{0}\" -e \"{1}\"", p, png);
                    try {
                        Process inkscape = Process.Start(new ProcessStartInfo(inkExe, args));
                        inkscape.WaitForExit();
                    } catch (Exception e) { MessageBox.Show(string.Format("Inkscape Error: " + e.Message)); }
                    Bitmap b = new Bitmap(png);
                    Size s = Shrink(b.Size);
                    pic.Size = s;
                    b = new Bitmap(b, s);// Resize the bitmap so that needlessly large images can still be loaded
                    //BackgroundImage = b;
                    pic.BackgroundImage = b;
                    postImageSet();
                } else {
                    Bitmap b = new Bitmap(p);
                    Size s = Shrink(b.Size);
                    pic.Size = s;
                    b = new Bitmap(b, s);// Resize the bitmap so that needlessly large images can still be loaded
                    BackgroundImage = b;
                    pic.BackgroundImage = b;
                    postImageSet();
                }
                openFile = p;
                Text = Path.GetFileName(openFile);
                Focus();
                BringToFront();
            } catch (Exception e) {
                MessageBox.Show("Error: " + e.Message + "\nfile - " + p);
                Environment.Exit(0);
            }
        }

        const float screenFactor = .6f;

        private Size Shrink(Size s) {
            double h = s.Height, w = s.Width;
            while (h > Screen.PrimaryScreen.WorkingArea.Height * screenFactor || w > Screen.PrimaryScreen.WorkingArea.Width * screenFactor) {
                h /= 1.1;
                w /= 1.1;
            }
            return new Size(Convert.ToInt32(w), Convert.ToInt32(h));
        }

        void postImageSet() {
            this.runOnUiThread(() => { Focus(); BringToFront(); Show(); Activate(); });
            this._(() => { TopMost = false; Height = pic.Height + 110; showLoading(false); });
            Bitmap b = new Bitmap(pic.BackgroundImage);
            Thread t = F.async(() => {
                Dictionary<Color, int> map = new Dictionary<Color, int>();
                for (int i = 0; i < b.Height; i++) {
                    for (int j = 0; j < b.Width; j++) {
                        int n = -1;
                        Color p = b.GetPixel(j, i);
                        if (!map.TryGetValue(p, out n)) {
                            map.Add(p, 1);
                        } else {
                            map[p] = n + 1;
                        }
                    }
                }
                foreach (Color c in map.Keys) {
                    colors.runOnUiThread(() => { colors.Items.Add(string.Format("{0} - {1}", c.toHex(), map[c])); });
                }
                /*this.runOnUiThread(() => {
                    foreach (var c in colors.Items) {
                        ((Control)c).MouseClick += itemClick;
                    }
                });/**/
            });/**/
        }

        public void showLoading(bool shown = true) {
            loadingImage.runOnUiThread(() => { loadingImage.Visible = shown; });
        }
      

        private void deleteColor() {
            try {
                string s = colors.Items[colors.SelectedIndex].ToString();
                string hex = s.Split('#')[1].Split(' ')[0];
                Color c = (Color)new ColorConverter().ConvertFromString("#" + hex);
                Bitmap b = new Bitmap(pic.BackgroundImage);
                if (c != null) {
                    for (int i = 0; i < b.Height; i++) {
                        for (int j = 0; j < b.Width; j++) {
                            if (b.GetPixel(j, i).Equals(c)) {
                                b.SetPixel(j, i, Color.Transparent);
                            }
                        }
                    }
                    pic.BackgroundImage = b;
                }
            } catch (Exception e) {
                MessageBox.Show(e.Message);
            }
        }
    }
}
