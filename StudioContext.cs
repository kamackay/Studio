using Global;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace Electrum {
    class StudioContext : ApplicationContext {

        private static StudioContext currentInstance;

        public static StudioContext getCurrentInstance() {
            return currentInstance;
        }

        public StudioContext(string[] args) {
            currentInstance = this;
            init(args);
            openForm(args);
        }

        internal NotifyIcon trayIcon;

        private void init(string[] args) {
            openForms = new List<Form>();
            settings = Settings.getDefault();
            F.async(() => {/**/
                while (runBackground) {
                    Thread.Sleep(5000);
                    try {
                        if (openForms.Count == 0 /*|| Application.OpenForms.Count == 0/**/) quit();
                    } catch (Exception e) {
                        MessageBox.Show(e.Message);
                    }
                }/**/
            });
            F.async(() => {
                while (runBackground) {
                    try {
                        using (NamedPipeServerStream pipeStream = new NamedPipeServerStream(Application.ProductName, PipeDirection.In)) {
                            pipeStream.WaitForConnection();
                            using (StreamReader reader = new StreamReader(pipeStream)) {
                                //Read in data from other instances of this application
                                while (!reader.EndOfStream) {
                                    string s = reader.ReadLine();
                                    Application.OpenForms[0].sync(() => { otherProcessCommand(Regex.Matches(s, @"[\""].+?[\""]|[^ ]+").Cast<Match>().Select(m => m.Value.Trim()).ToList().ToArray()); });
                                }
                            }
                        }
                    } catch { }
                }
            });
            trayIcon = new NotifyIcon();
            trayIcon.Text = "Electrum Studios";
            trayIcon.Icon = Properties.Resources.electrum;
            trayIcon.Visible = true;
            trayIcon.MouseClick += delegate (object o, MouseEventArgs a) {
                if (a.Button == MouseButtons.Right) {
                    //Show some sort of settings screen
                } else if (a.Button == MouseButtons.Left) openForm(new ElectrumMain());
            };
        }

        bool runBackground = true;

        public Settings settings;

        private void openForm(string[] args) {
            args = args.trimAll();
            List<string> parameters = new List<string>(), arguments = new List<string>();
            foreach (string temp in args) {
                if (temp.StartsWith("--")) parameters.Add(temp.ToLower());
                else arguments.Add(temp.ToLower());
            }
            if (parameters.Contains("--picture")) settings.picture = true;
            if (arguments.Count > 0) {
                if (File.Exists(arguments[0].Trim())) {
                    if (FileTypes.isImage(arguments[0].Trim())) settings.picture = true;
                } else {
                    MessageBox.Show("Unexpected Parameter: " + arguments[0].Trim(), "Electrum");
                    quit();
                }
            }
            if (settings.picture) {
                try {
                    if (arguments.Count > 0 && ".gif".Equals(Path.GetExtension(arguments[0]))) {
                        GifViewer f = new GifViewer(arguments[0]);
                        f.FormClosing += delegate {
                            openForms.Remove(f);
                        };
                        openForms.Add(f);
                        f.Show();
                    } else {
                        PhotoViewer f = arguments.Count > 0 ? new PhotoViewer(arguments[0]) : new PhotoViewer();
                        f.FormClosing += delegate {
                            openForms.Remove(f);
                        };
                        f.subFormOpened += delegate (object o, PhotoViewer.FormOpenEventArgs arg) {
                            openForms.Add(arg.subForm);
                            arg.subForm.FormClosing += delegate {
                                openForms.Remove(arg.subForm);
                            };
                        };
                        openForms.Add(f);
                        f.Show();
                    }
                } catch { }
            } else {
                try {
                    if (arguments.Count == 0) openHomeScreen();
                    else if (arguments.Count == 1) {
                        if (File.Exists(arguments[0].Trim())) openFile(arguments[0].Trim());
                        else MessageBox.Show(string.Format("Not sure what to do with '{0}' - It is not a file.", arguments[0]));
                    } else openHomeScreen();
                } catch (Exception e) {
                    MessageBox.Show("Error: " + e.Message);
                }
            }
        }

        public void openHomeScreen() {
            ElectrumMain f = new ElectrumMain();
            f.FormClosing += delegate {
                openForms.Remove(f);
            };
            f.subFormOpened += delegate (object o, KeithForm.FormOpenEventArgs arg) {
                openForms.Add(arg.subForm);
                arg.subForm.FormClosing += delegate {
                    openForms.Remove(arg.subForm);
                };
            };
            openForms.Add(f);
            f.Show();
        }

        public void otherProcessCommand(string[] args) {
            args = args.trimAll();
            if (args.Length == 1) {
                if (File.Exists(args[0])) openFile(args[0]);
                //MessageBox.Show("Recieved Message from another Process!");
            }
        }

        public void formOpened(Form f) {
            try {
                if (f != null && !openForms.Contains(f)) openForms.Add(f);
            } catch { }
        }

        public void formClosed(Form f = null) {
            try {
                if (f != null) openForms.Remove(f);
                if (openForms.Count == 0) quit();
            } catch { }
        }

        void openForm(Form f) {
            f.FormClosing += delegate {
                openForms.Remove(f);
                if (openForms.Count == 0) quit();
            };
            if (f is KeithForm)
                ((KeithForm)f).subFormOpened += delegate (object o, KeithForm.FormOpenEventArgs arg) {
                    openForms.Add(arg.subForm);
                    arg.subForm.FormClosing += delegate {
                        openForms.Remove(arg.subForm);
                    };
                };
            //else if (f is MainForm)
            //    ((MainForm)f).FormClosed += delegate {
            //        formClosed(f);
            //    };
            openForms.Add(f);
            f.Show();
        }

        public void quit() {
            if (trayIcon != null) trayIcon.Visible = false;
            Application.Exit();
            Environment.Exit(0);//In Case the last one didn't work
        }

        public void openFile(string filename, Form f = null) {
            try {
                string ext = Path.GetExtension(filename);
                switch (ext.ToLower()) {
                    default: break;
                    case ".pdf":
                        if (!(f is MainForm)) openForm(new MainForm(filename));
                        else openForm(new MainForm(filename)); //I'll work on that later
                        return;
                    case ".gif":
                        openForm(new GifViewer(filename));
                        return;
                }

                // Handle all filetypes here
                handleFile(filename);
            } catch (Exception e) { MessageBox.Show(e.Message, "Electrum Studios error"); }
        }

        public void handleFile(string filename, Form f = null) {
            if (FileTypes.isImage(filename)) {
                if (string.Equals(".gif", Path.GetExtension(filename), StringComparison.OrdinalIgnoreCase)) openForm(new GifViewer(filename));
                else openForm(new PhotoViewer(filename));
            } else if (FileTypes.shouldOpenInBrowser(filename)) {
                openForm(new MainForm(filename));
            } else if (FileTypes.isAudio(filename)) {

            }
        }

        private List<Form> openForms;
    }

    public class Settings {
        public bool logging { get; set; }
        public bool picture { get; set; }

        private Settings() {
            logging = false;
        }

        public static Settings getDefault() {
            return new Settings();
        }
    }
}
