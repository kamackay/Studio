using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Global {
    public class Tools {
        public static string getDataFolder() {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Application.ProductName);
            try { if (!Directory.Exists(path)) Directory.CreateDirectory(path); } catch { }
            return path;
        }

        public static long getFolderBytes(string path) {
            long n = 0;
            try {
                foreach (string s in Directory.GetFiles(path, "*", SearchOption.AllDirectories)) {
                    try {
                        n += new FileInfo(s).Length;
                    } catch { }
                }
            } catch (Exception e) { return n; }
            return n;
        }

        /// <summary>
        /// Get the string associated with this increment in 
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static string getUnitString(short num) {
            switch (num) {
                default:
                case 0: return "B";
                case 1: return "KB";
                case 2: return "MB";
                case 3: return "GB";
                case 4: return "TB";
                case 5: return "PB";
            }
        }

        /// <summary>
        /// Wraps necessary functions imported from User32.dll. Code courtesy of MSDN Cold Rooster Consulting example.
        /// </summary>
        public class User32 {
            /// <summary>
            /// Provides access to function required to delete handle. This method is used internally
            /// and is not required to be called separately.
            /// </summary>
            /// <param name="hIcon">Pointer to icon handle.</param>
            /// <returns>N/A</returns>
            [DllImport("User32.dll")]
            public static extern int DestroyIcon(IntPtr hIcon);
        }

        public static Font getFont(float size) {
            string fontName = "Product Sans";
            using (Font fontTester = new Font(
                    fontName, size, FontStyle.Regular, GraphicsUnit.Pixel)) {
                if (fontTester.Name == fontName)
                    return new Font(fontName, size);
                else return new Font("Arial", size);
            }
        }

        public static string getTaskbarColor() {
            try {
                int argbColor = (int)Microsoft.Win32.Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\DWM", "ColorizationColor", null);
                var color = Color.FromArgb(argbColor);
                return ConverterToHex(color);
            } catch (Exception) {
                return "#000000";
            }
        }

        private static string ConverterToHex(System.Drawing.Color c) {
            return string.Format("#{0}{1}{2}", c.R.ToString("X2"), c.G.ToString("X2"), c.B.ToString("X2"));
        }

        public static async void moveFile(string sourceFile, string destinationFile) {
            try {
                using (FileStream sourceStream = File.Open(sourceFile, FileMode.Open)) {
                    using (FileStream destinationStream = File.Create(destinationFile)) {
                        await sourceStream.CopyToAsync(destinationStream);
                        sourceStream.Close();
                        destinationStream.Close();
                        File.Delete(sourceFile);
                    }
                }
            } catch (IOException ioex) {
                Toast.show("An IOException occured during move, " + ioex.Message);
            } catch (Exception ex) {
                Toast.show("An Exception occured during move, " + ex.Message);
            }
        }
    }
}
