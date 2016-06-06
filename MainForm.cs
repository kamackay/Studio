using CefSharp;
using CefSharp.WinForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Electrum {
    public partial class MainForm : KeithForm {
        private static readonly bool DebuggingSubProcess = Debugger.IsAttached;
        Color darkColor;
        string getHomepage() {
            return home;
        }

        public MainForm() : base() {
            preInit();
            InitializeComponent();
        }

        string currentPage;

        public MainForm(string param) : base() {
            preInit();
            //if (!param.Equals("debug")) currentPage = param;
            InitializeComponent();
            if (param.Equals("debug")) runDebug();
            else open(param);
        }

        protected void init() {

        }

        void preInit() {
            buttonSize = 30;
            float fontSize = 18f;
            f = new Font("Product Sans", fontSize);
            if (f == null) f = new Font("Arial", fontSize);
            darkColor = ColorTranslator.FromHtml("#4C4A48");
        }
        
        public void findOnPageCommand() {
            findBar.runOnUiThread(() => {
                findBar.Visible = true;
                findBar.Location = new Point(Width - findBar.Width,
                    headerContainer.Bottom);
                findBar.BringToFront();
            });
        }

        public void goHome() {
            open(getHomepage());
        }

        void runDebug() {
            goHome();
        }

        void redirect(string redirectPath) {
            browser.Load(redirectPath);
            currentPage = redirectPath;
        }

        string getTempFolder(string filename = "") {
            string path = Path.Combine(Environment.CurrentDirectory, "temp.html", "");
            if (!Directory.Exists(Path.GetDirectoryName(path))) Directory.CreateDirectory(path);
            return path;
        }

        void openMarkdown(string mdFile) {
            string tempfile = getTempFolder("temp.html");
            ProcessStartInfo psi = new ProcessStartInfo("pandoc");
            psi.Arguments = "-s " + mdFile + " -o " + tempfile;
            psi.CreateNoWindow = true;
            Process p = Process.Start(psi);
            p.WaitForExit();
            openFile(tempfile);
        }

        void open(string path) {
            if (File.Exists(path)) openFile(path);
            else redirect(path);
        }

        void openFile(string file) {
            if (file.EndsWith(".md")) openMarkdown(file);
            else redirect("file://" + file);
        }

        void back() {
            if (browser.CanGoBack) browser.Back();
        }

        void forward() {
            if (browser.CanGoForward) browser.Forward();
        }
        const string home = "http://keithmackay.com";
        private void Browser_FrameLoadEnd(object sender, FrameLoadEndEventArgs e) {
            urlBar.runOnUiThread(() => {
                string address = browser.Address;
                urlBar.Text = (address.ToLower().Equals(home.ToLower())) ? "Home" : address;
                backButton.Visible = browser.CanGoBack;
                forwardButton.Visible = browser.CanGoForward;
            });
        }

        public void RefreshBrowser() {
            browser.Reload();
        }

        void search(string search) {
            if (search.isURL()) open(search);
            else if (File.Exists(search)) openFile(search);
            else {
                StringBuilder sb = new StringBuilder("https://www.google.com/webhp?sourceid=chrome-instant&ion=1&espv=2&ie=UTF-8#q=");
                foreach (char c in search.ToCharArray()) {
                    if (char.IsLetterOrDigit(c)) sb.Append(c);
                    else sb.Append("%").Append(Convert.ToInt32(c).ToString("X"));
                }
                redirect(sb.ToString());
            }
        }
        int buttonSize;

        private IContainer components = null;

        protected override void Dispose(bool disposing) {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }
        Font f;

        private void InitializeComponent() {
            SuspendLayout();
            browser = new ChromiumWebBrowser(currentPage == null ? getHomepage() : currentPage);
            urlBar = new TextBox();
            headerContainer = new ContainerControl();
            forwardButton = new PictureBox();
            backButton = new PictureBox();
            navigationContainer = new ContainerControl();
            findBar = new FindBar();
            //
            //findBar
            //
            findBar.Font = f;
            findBar.Location = new Point(Width - 50, 200);
            findBar.Visible = false;
            findBar.SearchChanged += searchTextChanged;
            //
            //urlBar
            //
            urlBar.Dock = DockStyle.Fill;
            urlBar.Font = f;
            urlBar.KeyDown += delegate (object a, KeyEventArgs b) {
                if (b.KeyCode == Keys.Enter)
                    search(urlBar.Text);
                else if (b.KeyCode == Keys.Escape)
                    goHome();
            };
            urlBar.GotFocus += UrlBar_GotFocus;
            urlBar.BackColor = darkColor;
            urlBar.ForeColor = Color.White;
            urlBar.BorderStyle = BorderStyle.None;
            urlBar.MouseDoubleClick += delegate { urlBar.SelectAll(); };
            // 
            // browserContent
            // 
            browser.Dock = DockStyle.Fill;
            browser.Location = new Point(0, 0);
            browser.MinimumSize = new Size(100, 400);
            browser.Size = new Size((int)(Screen.PrimaryScreen.WorkingArea.Width / 1.25f),
                (int)(Screen.PrimaryScreen.WorkingArea.Height / 1.25f));
            browser.TabIndex = 0;
            browser.FrameLoadEnd += Browser_FrameLoadEnd;
            browser.TitleChanged += Browser_TitleChanged;
            browser.DownloadHandler = new DownloadHandler();
            browser.MenuHandler = new MenuHandler();
            browser.DialogHandler = new DialogHandler();
            browser.RequestHandler = new RequestHandler();
            browser.KeyboardHandler = new KeyboardHandler();
            browser.LifeSpanHandler = new LifespanHandler();
            browser.DisplayHandler = new DisplayHandler();
            browser.JsDialogHandler = new JsDialogHandler();
            browser.DragHandler = new DragHandler();
            browser.FocusHandler = new FocusHandler();
            var settings = new CefSettings();
            settings.CachePath = "cache";

            Cef.OnContextInitialized = delegate {
                var cookieMan = Cef.GetGlobalCookieManager();
                cookieMan.SetStoragePath("cookies", true);
                cookieMan.SetSupportedSchemes("custom");
            };
            //
            //headerContainer
            //
            headerContainer.Dock = DockStyle.Top;
            headerContainer.BackColor = darkColor;
            headerContainer.Padding = new Padding(0, 15, 0, 0);
            navigationContainer.Dock = DockStyle.Left;
            navigationContainer.BackColor = darkColor;
            navigationContainer.Padding = new Padding(5, 0, 10, 5);
            //
            //backButton
            //
            backButton.Dock = DockStyle.Left;
            backButton.Size = new Size(buttonSize, buttonSize);
            backButton.AutoSize = true;
            backButton.SizeMode = PictureBoxSizeMode.CenterImage;
            backButton.Image = new Bitmap(Properties.Resources.back, new Size(buttonSize, buttonSize));
            backButton.MouseEnter += delegate {
                backButton.Image = new Bitmap(Properties.Resources.back_red, new Size(buttonSize, buttonSize));
            };
            backButton.MouseLeave += delegate {
                backButton.Image = new Bitmap(Properties.Resources.back, new Size(buttonSize, buttonSize));
            };
            backButton.MouseClick += delegate (object a, MouseEventArgs e) {
                if (e.Button == MouseButtons.Left) back();
            };
            //
            //forwardButton
            //
            forwardButton.Dock = DockStyle.Right;
            forwardButton.Size = new Size(buttonSize, buttonSize);
            forwardButton.SizeMode = PictureBoxSizeMode.CenterImage;
            forwardButton.Image = new Bitmap(Properties.Resources.forward, new Size(buttonSize, buttonSize));
            forwardButton.MouseEnter += delegate {
                forwardButton.Image = new Bitmap(Properties.Resources.forward_red, new Size(buttonSize, buttonSize));
            };
            forwardButton.MouseLeave += delegate {
                forwardButton.Image = new Bitmap(Properties.Resources.forward, new Size(buttonSize, buttonSize));
            };
            forwardButton.MouseClick += delegate (object a, MouseEventArgs e) {
                if (e.Button == MouseButtons.Left) forward();
            };
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(6F, 13F);
            BackColor = Color.Black;
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size((int)(Screen.PrimaryScreen.WorkingArea.Width / 1.25f),
                (int)(Screen.PrimaryScreen.WorkingArea.Height / 1.25f));
            Shown += MainForm_Shown;
            Font = f;
            Icon = Properties.Resources.browserIcon;
            Controls.Add(browser);
            Resize += delegate {
                this.runOnUiThread(() => {
                    findBar.Location = new Point(Width - findBar.Width, headerContainer.Bottom);
                });
            };
            navigationContainer.Controls.Add(backButton);
            navigationContainer.Controls.Add(forwardButton);
            headerContainer.Controls.Add(urlBar);
            headerContainer.Controls.Add(navigationContainer);
            navigationContainer.Width = buttonSize * 3;
            navigationContainer.Height = (int)(buttonSize * 1.1);
            urlBar.Height = buttonSize;
            Controls.Add(headerContainer);
            Controls.Add(findBar);
            //WindowState = FormWindowState.Maximized;
            //WindowState = KeithBrowser.Properties.Settings.Default.FullScreen ? FormWindowState.Maximized : FormWindowState.Normal;
            Name = "MainForm";
            ResumeLayout(false);
        }

        private void searchTextChanged(object sender, FindBar.SearchEventArgs e) {
            browser.StopFinding(true);
            browser.Find(1, e.text, true, false, true);
        }

        public void setTitle(string title) {
            this.runOnUiThread(() => { Text = title; });
        }

        private void Browser_TitleChanged(object sender, TitleChangedEventArgs e) {
            //browser.runOnUiThread(() => {
            //Text = e.Title;
            //});
            headerContainer.runOnUiThread(() => {
                if (e.Title.ToLower().EndsWith(".pdf")) headerContainer.Visible = false;
                else headerContainer.Visible = true;
                Invalidate();
            });
        }

        private void UrlBar_GotFocus(object sender, EventArgs e) {
            //urlBar.SelectAll();
        }

        private void MainForm_Shown(object sender, EventArgs e) {
            float factor = 1.25f;
            ClientSize = new Size((int)(Screen.PrimaryScreen.WorkingArea.Width / factor),
                (int)(Screen.PrimaryScreen.WorkingArea.Height / factor));
            Size = new Size((int)(Screen.PrimaryScreen.WorkingArea.Width / factor),
                (int)(Screen.PrimaryScreen.WorkingArea.Height / factor));
            Location = new Point(100, 100);
            int max = 25;
            foreach (Control c in headerContainer.Controls) if (c.Height > max) max = c.Height;
            headerContainer.Height = max + buttonSize;
            WindowState = FormWindowState.Maximized;
        }

        private ChromiumWebBrowser browser;
        TextBox urlBar;
        ContainerControl headerContainer;
        PictureBox forwardButton, backButton;
        ContainerControl navigationContainer;
        FindBar findBar;

        public Color DarkColor {
            get {
                return darkColor;
            }

            set {
                darkColor = value;
            }
        }
    }

    public class MenuHandler : IContextMenuHandler {
        private const int ReloadPage = 9999;
        private const int DevTools = 666, Home = 420;
        public void OnBeforeContextMenu(IWebBrowser browserControl, IBrowser browser,
            IFrame frame, IContextMenuParams parameters, IMenuModel model) {
            model.AddSeparator();
            model.AddItem((CefMenuCommand)ReloadPage, "Reload Page");
            model.AddItem((CefMenuCommand)DevTools, "Show Dev Tools");
            model.AddItem((CefMenuCommand)Home, "Home");
        }

        public bool OnContextMenuCommand(IWebBrowser browserControl, IBrowser browser,
            IFrame frame, IContextMenuParams parameters, CefMenuCommand commandId, CefEventFlags eventFlags) {
            if (commandId == (CefMenuCommand)ReloadPage) browser.Reload();
            else if (commandId == (CefMenuCommand)DevTools) browser.ShowDevTools();
            else if (commandId == (CefMenuCommand)Home) ((MainForm)Form.ActiveForm).goHome();
            return false;
        }

        public void OnContextMenuDismissed(IWebBrowser browserControl, IBrowser browser, IFrame frame) {

        }

        public bool RunContextMenu(IWebBrowser browserControl, IBrowser browser, IFrame frame,
            IContextMenuParams parameters, IMenuModel model, IRunContextMenuCallback callback) {
            return false;
        }
    }

    public class KeyboardHandler : IKeyboardHandler {
        public bool OnKeyEvent(IWebBrowser browserControl, IBrowser browser, KeyType type,
            int windowsKeyCode, int nativeKeyCode, CefEventFlags modifiers, bool isSystemKey) {
            if (windowsKeyCode == 0x46 && modifiers == CefEventFlags.ControlDown) {
                ((MainForm)Form.ActiveForm).findOnPageCommand();
                return true;
            } else if (windowsKeyCode == 0x1b) {
                ((MainForm)Form.ActiveForm).goHome();
                return true;
            } else if (windowsKeyCode == 0x74) {
                ((MainForm)Form.ActiveForm).RefreshBrowser();
                return true;
            }
            return false;
        }

        public bool OnPreKeyEvent(IWebBrowser browserControl, IBrowser browser, KeyType type,
            int windowsKeyCode, int nativeKeyCode, CefEventFlags modifiers, bool isSystemKey, ref bool isKeyboardShortcut) {
            return false;
        }
    }

    public class DownloadHandler : IDownloadHandler {
        public void OnBeforeDownload(IBrowser browser, DownloadItem downloadItem,
            IBeforeDownloadCallback callback) {
            string filename = Path.Combine(KnownFolders.GetPath(KnownFolder.Downloads),
                downloadItem.SuggestedFileName);
            callback.Continue(filename,
                !downloadItem.SuggestedFileName.ToLower().EndsWith(".zip"));
        }

        public void OnDownloadUpdated(IBrowser browser, DownloadItem downloadItem,
            IDownloadItemCallback callback) {
            if (downloadItem.IsComplete) {
                if (downloadItem.FullPath.ToLower().EndsWith(".zip")) Process.Start(downloadItem.FullPath);
                else MessageBox.Show("Download Complete");
            }
        }
    }

    public class LifespanHandler : ILifeSpanHandler {
        public bool DoClose(IWebBrowser browserControl, IBrowser browser) {
            return true;
        }

        public void OnAfterCreated(IWebBrowser browserControl, IBrowser browser) {

        }

        public void OnBeforeClose(IWebBrowser browserControl, IBrowser browser) {

        }

        public bool OnBeforePopup(IWebBrowser browserControl, IBrowser browser, IFrame frame,
            string targetUrl, string targetFrameName, WindowOpenDisposition targetDisposition,
            bool userGesture, IWindowInfo windowInfo, ref bool noJavascriptAccess, out IWebBrowser newBrowser) {
            newBrowser = null;
            try {
                //Process.Start(Environment.CurrentDirectory + "\\KeithBrowser.exe", targetUrl);
            } catch (Exception) { }
            return true;
        }

        public bool OnBeforePopup(IWebBrowser browserControl, IBrowser browser, IFrame frame, string targetUrl, 
            string targetFrameName, WindowOpenDisposition targetDisposition, bool userGesture, IPopupFeatures popupFeatures, 
            IWindowInfo windowInfo, IBrowserSettings browserSettings, ref bool noJavascriptAccess, out IWebBrowser newBrowser) {
            throw new NotImplementedException();
        }
    }

    public class DialogHandler : IDialogHandler {
        public bool OnFileDialog(IWebBrowser browserControl, IBrowser browser, CefFileDialogMode mode,
            string title, string defaultFilePath, List<string> acceptFilters, int selectedAcceptFilter, IFileDialogCallback callback) {
            MessageBox.Show(defaultFilePath, title);
            return true;
        }
    }

    public class DisplayHandler : IDisplayHandler {
        public void OnAddressChanged(IWebBrowser browserControl, AddressChangedEventArgs addressChangedArgs) {

        }

        public bool OnConsoleMessage(IWebBrowser browserControl, ConsoleMessageEventArgs consoleMessageArgs) {
            return false;
        }

        public void OnFaviconUrlChange(IWebBrowser browserControl, IBrowser browser, IList<string> urls) {
            /*foreach(string uri in urls) {
                if (uri.ToLower().EndsWith(".ico")) {
                    using (Stream stream = Application.GetResourceStream(new Uri(uri))) {

                    }
                } else continue;
            }*/
        }

        public void OnFullscreenModeChange(IWebBrowser browserControl, IBrowser browser, bool fullscreen) {

        }

        public void OnStatusMessage(IWebBrowser browserControl, StatusMessageEventArgs statusMessageArgs) {

        }

        public void OnTitleChanged(IWebBrowser browserControl, TitleChangedEventArgs titleChangedArgs) {
            try { ((MainForm)Form.ActiveForm).setTitle(titleChangedArgs.Title); } catch (Exception) { //Probably means dev tools opened
            }
        }

        public bool OnTooltipChanged(IWebBrowser browserControl, string text) {
            return false;
        }
    }

    public class JsDialogHandler : IJsDialogHandler {
        public void OnDialogClosed(IWebBrowser browserControl, IBrowser browser) {

        }

        public bool OnJSBeforeUnload(IWebBrowser browserControl, IBrowser browser,
            string message, bool isReload, IJsDialogCallback callback) {
            return false;
        }

        public bool OnJSDialog(IWebBrowser browserControl, IBrowser browser, string originUrl,
            string acceptLang, CefJsDialogType dialogType, string messageText, string defaultPromptText,
            IJsDialogCallback callback, ref bool suppressMessage) {
            return false;
        }

        public void OnResetDialogState(IWebBrowser browserControl, IBrowser browser) {

        }
    }

    public class DragHandler : IDragHandler {
        public bool OnDragEnter(IWebBrowser browserControl, IBrowser browser, IDragData dragData, DragOperationsMask mask) {
            return false;
        }

        public void OnDraggableRegionsChanged(IWebBrowser browserControl, IBrowser browser, IList<DraggableRegion> regions) {

        }
    }

    public class RequestHandler : IRequestHandler {
        public bool GetAuthCredentials(IWebBrowser browserControl, IBrowser browser, IFrame frame, bool isProxy, string host, int port, string realm, string scheme, IAuthCallback callback) {
            return false;
        }

        public IResponseFilter GetResourceResponseFilter(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, IResponse response) {
            throw new NotImplementedException();
        }

        public bool OnBeforeBrowse(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, bool isRedirect) {
            return false;
        }

        public CefReturnValue OnBeforeResourceLoad(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, IRequestCallback callback) {
            return CefReturnValue.Continue;
        }

        public bool OnCertificateError(IWebBrowser browserControl, IBrowser browser, CefErrorCode errorCode, string requestUrl, ISslInfo sslInfo, IRequestCallback callback) {
            return false;
        }

        public bool OnOpenUrlFromTab(IWebBrowser browserControl, IBrowser browser,
            IFrame frame, string targetUrl, WindowOpenDisposition targetDisposition, bool userGesture) {
            return false;
        }

        public void OnPluginCrashed(IWebBrowser browserControl, IBrowser browser, string pluginPath) {

        }

        public bool OnProtocolExecution(IWebBrowser browserControl, IBrowser browser, string url) {
            return false;
        }

        public bool OnQuotaRequest(IWebBrowser browserControl, IBrowser browser, string originUrl, long newSize, IRequestCallback callback) {
            return false;
        }

        public void OnRenderProcessTerminated(IWebBrowser browserControl, IBrowser browser, CefTerminationStatus status) {

        }

        public void OnRenderViewReady(IWebBrowser browserControl, IBrowser browser) {

        }

        public void OnResourceLoadComplete(IWebBrowser browserControl, IBrowser browser, IFrame frame,
            IRequest request, IResponse response, UrlRequestStatus status, long receivedContentLength) {

        }

        public void OnResourceRedirect(IWebBrowser browserControl, IBrowser browser, IFrame frame,
            IRequest request, ref string newUrl) {

        }

        public bool OnResourceResponse(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, IResponse response) {
            return false;
        }
    }

    public class FocusHandler : IFocusHandler {
        public void OnGotFocus() {

        }

        public bool OnSetFocus(CefFocusSource source) {
            return false;
        }

        public void OnTakeFocus(bool next) {

        }
    }

    public static class Helpers {
        public static bool isURL(this string s) {
            Uri result;
            return (Uri.TryCreate(s, UriKind.Absolute, out result) && result.Scheme == Uri.UriSchemeHttp) ||
                s.StartsWith("www.") || s.Contains(".com") || s.Contains(".edu") || s.Contains(".org");
        }

        public static void runOnUiThread(this Control control, Action runnable) {
            try {
                control.Invoke(runnable);
            } catch (Exception) {
                //Eat the exception
            }
        }
    }
}
