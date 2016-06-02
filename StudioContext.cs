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
            init();
            openForm();
        }

        private void init() {
            openForms = new List<Form>();
        }

        private void openForm() {
            KeithForm f = new KeithForm();
            f.FormClosed += delegate {
                if (openForms.Count == 1) Application.Exit();
            };
            f.subFormOpened += delegate (object o, KeithForm.FormOpenEventArgs args) {
                openForms.Add(args.subForm);
                args.subForm.FormClosed += delegate {
                    if (openForms.Count == 1) Application.Exit();
                };
            };
            openForms.Add(f);
            f.Show();
        }

        private List<Form> openForms;
    }
}
