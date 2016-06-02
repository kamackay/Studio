using System.Collections.Generic;
using System.Windows.Forms;

namespace Studio {
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
        }

        public Settings settings;

        private void openForm(string[] args) {
            List<string> parameters = new List<string>(), arguments = new List<string>();
            foreach (string temp in args) {
                if (temp.StartsWith("--")) parameters.Add(temp.ToLower());
                else arguments.Add(temp.ToLower());
            }
            if (parameters.Contains("--picture")) settings.picture = true;
            if (settings.picture) {
                Form f = new PhotoViewer();
                f.FormClosed += delegate {
                    if (openForms.Count == 1) Application.Exit();
                };
                if (f is KeithForm)
                    ((KeithForm)f).subFormOpened += delegate (object o, KeithForm.FormOpenEventArgs arg) {
                        openForms.Add(arg.subForm);
                        arg.subForm.FormClosed += delegate {
                            if (openForms.Count == 1) Application.Exit();
                        };
                    };
                openForms.Add(f);
                f.Show();
            } else {
                Form f = new KeithForm();
                f.FormClosed += delegate {
                    if (openForms.Count == 1) Application.Exit();
                };
                if (f is KeithForm)
                    ((KeithForm)f).subFormOpened += delegate (object o, KeithForm.FormOpenEventArgs arg) {
                        openForms.Add(arg.subForm);
                        arg.subForm.FormClosed += delegate {
                            if (openForms.Count == 1) Application.Exit();
                        };
                    };
                openForms.Add(f);
                f.Show();
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
