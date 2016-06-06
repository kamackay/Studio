using System;
using System.Collections.Generic;
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

        private void init(string[] args) {
            openForms = new List<Form>();
            settings = Settings.getDefault();
            Functions.runAsync(() => {/**/
                while (runBackground) {
                    try {
                        Thread.Sleep(5000);
                        if (openForms.Count == 0) Environment.Exit(0);
                    } catch { }
                }/**/
            });
        }

        bool runBackground = true;

        public Settings settings;

        private void openForm(string[] args) {
            List<string> parameters = new List<string>(), arguments = new List<string>();
            foreach (string temp in args) {
                if (temp.StartsWith("--")) parameters.Add(temp.ToLower());
                else arguments.Add(temp.ToLower());
            }
            if (parameters.Contains("--picture")) settings.picture = true;
            if (arguments.Count > 0) {
                if (System.IO.File.Exists(arguments[0])) {
                    if (FileTypes.isImage(arguments[0])) settings.picture = true;
                } else AutoClosingMessageBox.show("Unexpected Parameter: " + arguments[0], "Studio");
            }
            if (settings.picture) {
                try {
                    if (arguments.Count > 0 && ".gif".Equals(System.IO.Path.GetExtension(arguments[0]))) {
                        GifViewer f = new GifViewer(arguments[0]);
                        f.FormClosing += delegate {
                            if (openForms.Count == 1) Application.Exit();
                            openForms.Remove(f);
                        };
                        openForms.Add(f);
                        f.Show();
                    } else {
                        PhotoViewer f = arguments.Count > 0 ? new PhotoViewer(arguments[0]) : new PhotoViewer();
                        f.FormClosing += delegate {
                            if (openForms.Count == 1) Application.Exit();
                            openForms.Remove(f);
                        };
                        f.subFormOpened += delegate (object o, PhotoViewer.FormOpenEventArgs arg) {
                            openForms.Add(arg.subForm);
                            arg.subForm.FormClosing += delegate {
                                if (openForms.Count == 1) Application.Exit();
                                openForms.Remove(arg.subForm);
                            };
                        };
                        openForms.Add(f);
                        f.Show();
                    }
                } catch { }
            } else {

                try {
                    MainForm f = new MainForm();
                    f.FormClosing += delegate {
                        if (openForms.Count == 1) Application.Exit();
                        openForms.Remove(f);
                    };
                    f.subFormOpened += delegate (object o, KeithForm.FormOpenEventArgs arg) {
                        openForms.Add(arg.subForm);
                        arg.subForm.FormClosing += delegate {
                            if (openForms.Count == 1) Application.Exit();
                            openForms.Remove(arg.subForm);
                        };
                    };
                    openForms.Add(f);
                    f.Show();
                } catch (Exception e) { MessageBox.Show("Error: " + e.Message); }
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
                if (openForms.Count == 0) System.Environment.Exit(0);
            } catch { }
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
