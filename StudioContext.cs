using System.Collections.Generic;
using System.Windows.Forms;

namespace Studio {
    class StudioContext : ApplicationContext {
        public StudioContext(string[] args) {
            openForms = new List<Form>();
            openForm();
        }

        private void openForm() {
            KeithForm f = new KeithForm();
            f.FormClosed += delegate {
                if (openForms.Count == 1) Application.Exit();
            };
            openForms.Add(f);
            f.Show();
        }

        private List<Form> openForms;
    }
}
