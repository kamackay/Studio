using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace Electrum {
    static class Program {

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args) {

            bool createdNew = false;
            using (Mutex mutex = new Mutex(true, "Electrum", out createdNew)) {
                if (createdNew) Run(args);
                else {
                    Process current = Process.GetCurrentProcess();
                    foreach (Process process in Process.GetProcessesByName(current.ProcessName)) {
                        if (process.Id != current.Id && process.ProcessName.Equals(current.ProcessName)) {
                            process.Kill();
                            process.WaitForExit();
                            Run(args);
                            break;
                        }
                    }
                }
            }

            
        }

        private static void Run(string[] args =  null) {
            args = args ?? new string[0];
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new StudioContext(args));
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetForegroundWindow(IntPtr hWnd);
    }
}
