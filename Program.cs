using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
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
            using (Mutex mutex = new Mutex(true, Application.ProductName, out createdNew)) {
                if (createdNew) Run(args);
                else {
                    Process current = Process.GetCurrentProcess();
                    foreach (Process process in Process.GetProcessesByName(current.ProcessName)) {
                        if (process.Id != current.Id && (process.ProcessName.Equals(current.ProcessName) || process.MainModule.FileName.Equals(current.MainModule.FileName))) {
                            // Application is already running, will need to pass any given data to it
                            using (NamedPipeClientStream pipeStream = new NamedPipeClientStream(".", Application.ProductName, PipeDirection.Out)) {
                                pipeStream.Connect();
                                using (StreamWriter sr = new StreamWriter(pipeStream)) {
                                    //Pass these parameters to the other application and allow it to handle them
                                    sr.WriteLine(F.paramsToString(args));
                                }
                            }
                            Environment.Exit(0);
                        }
                    }
                }
            }
        }

        private static void Run(string[] args = null) {
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
