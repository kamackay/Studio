using Global;

namespace Studio {
    public static class FileTypes {
        public static string[] image = new string[] { ".png", ".jpeg", ".jpg", ".webp", ".bmp", ".ico", ".tiff", ".svg", ".gif" };
        public static string[] audio = new string[] { ".mp3", ".ogg", ".wav" };
        public static string[] code = new string[] { ".cs" };
        public static string[] browserBased = new string[] { ".pdf", ".html" };

        public static bool isAudio(string file) {
            return audio.contains(System.IO.Path.GetExtension(file).ToLower());

        }

        public static bool isCode(string file) {
            return code.contains(System.IO.Path.GetExtension(file).ToLower());
        }

        public static bool isImage(string file) {
            return image.contains(System.IO.Path.GetExtension(file).ToLower());

        }

        public static bool shouldOpenInBrowser(string file) {
            return browserBased.contains(System.IO.Path.GetExtension(file).ToLower());
        }
    }
}
