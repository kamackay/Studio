using System.IO;

namespace Electrum.Utils {
    public static class Log {
        public static void log(string s) {
            using (StreamWriter write = new StreamWriter(Path.Combine(Tools.getExeFolder(), "error.log"), true))
                write.WriteLine(string.Format("{0} - {1}", System.DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"), s));
        }
    }
}
