using Microsoft.Win32;
using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace Global {
    public enum eReg // where to write in the Registry
    {
        Main,      // write in MainKey
        WorkDir,   // write in MainKey\WorkDirs\DirectoryName
        WorkState, // write in MainKey\WorkDirs\DirectoryName\WorkStates\Filename
    }

    public static class Functions {
        const string ms_RegistryRoot = @"Software\ElmueSoft\SqlBuilder";

        #region COM / Win32 API

        // Create Shortcut

        [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Unicode)]
        internal struct WIN32_FIND_DATAW {
            public const int MAX_PATH = 260;

            public uint dwFileAttributes;
            public System.Runtime.InteropServices.ComTypes.FILETIME ftCreationTime;
            public System.Runtime.InteropServices.ComTypes.FILETIME ftLastAccessTime;
            public System.Runtime.InteropServices.ComTypes.FILETIME ftLastWriteTime;
            public uint nFileSizeHigh;
            public uint nFileSizeLow;
            public uint dwReserved0;
            public uint dwReserved1;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_PATH)]
            public string cFileName;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
            public string cAlternateFileName;
        }

        [ComImport]
        [Guid("00021401-0000-0000-C000-000000000046")]
        [ClassInterface(ClassInterfaceType.None)]
        internal class ShellLinkObject { }

        [ComImport]
        [Guid("000214F9-0000-0000-C000-000000000046")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IShellLinkW {
            void GetPath([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile, int cch,
                              [MarshalAs(UnmanagedType.Struct)] ref WIN32_FIND_DATAW pfd, uint fFlags);
            void GetIDList(out IntPtr ppidl);
            void SetIDList(IntPtr pidl);
            void GetDescription([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszName, int cch);
            void SetDescription([MarshalAs(UnmanagedType.LPWStr)] string pszName);
            void GetWorkingDirectory([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszDir, int cch);
            void SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] string pszDir);
            void GetArguments([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszArgs, int cch);
            void SetArguments([MarshalAs(UnmanagedType.LPWStr)] string pszArgs);
            void GetHotkey(out ushort pwHotkey);
            void SetHotkey(ushort wHotkey);
            void GetShowCmd(out int piShowCmd);
            void SetShowCmd(int iShowCmd);
            void GetIconLocation([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszIconPath, int cch, out int piIcon);
            void SetIconLocation([MarshalAs(UnmanagedType.LPWStr)] string pszIconPath, int iIcon);
            void SetRelativePath([MarshalAs(UnmanagedType.LPWStr)] string pszPathRel, uint dwReserved);
            void Resolve(IntPtr hwnd, uint fFlags);
            void SetPath([MarshalAs(UnmanagedType.LPWStr)] string pszFile);
        }

        public enum eShowMode {
            Normal = 1,
            Maximized = 3,
            Minimized = 7,
        }

        [DllImport("kernel32.dll", EntryPoint = "OutputDebugStringA", CharSet = CharSet.Ansi)]
        static extern void OutputDebugStringA(string s_Text);

        [DllImport("kernel32.dll", EntryPoint = "FormatMessageA", CharSet = CharSet.Ansi)]
        static extern int FormatMessageA(int Flags, int Unused1, int Error, int Unused2, StringBuilder s_Text, int BufLen, int Unused3);

        [DllImport("kernel32.dll", EntryPoint = "FormatMessageW", CharSet = CharSet.Unicode)]
        static extern int FormatMessageW(int Flags, int Unused1, int Error, int Unused2, StringBuilder s_Text, int BufLen, int Unused3);

        [DllImport("kernel32.dll", EntryPoint = "GetCurrentThreadId")]
        static extern int GetCurrentThreadId();

        [DllImport("user32.dll", EntryPoint = "GetWindowThreadProcessId")]
        static extern int GetWindowThreadProcessId(IntPtr h_Wnd, out int ProcID);

        #endregion

        /// <summary>
        /// Use DebugView from www.sysinternals.com to see this debug output
        /// </summary>
       /*
        public static void PrintDebug(string s_Text, Type t_Origin) {
            if (t_Origin == Defaults.DebugType)
                OutputDebugStringA(s_Text);
        }
        public static void PrintDebug(string s_Format, object o_Para1, Type t_Origin) {
            if (t_Origin == Defaults.DebugType)
                OutputDebugStringA(string.Format(s_Format, o_Para1));
        }
        public static void PrintDebug(string s_Format, object o_Para1, object o_Para2, Type t_Origin) {
            if (t_Origin == Defaults.DebugType)
                OutputDebugStringA(string.Format(s_Format, o_Para1, o_Para2));
        }
        public static void PrintDebug(string s_Format, object o_Para1, object o_Para2, object o_Para3, Type t_Origin) {
            if (t_Origin == Defaults.DebugType)
                OutputDebugStringA(string.Format(s_Format, o_Para1, o_Para2, o_Para3));
        }
        public static void PrintDebug(string s_Format, object o_Para1, object o_Para2, object o_Para3, object o_Para4, Type t_Origin) {
            if (t_Origin == Defaults.DebugType)
                OutputDebugStringA(string.Format(s_Format, o_Para1, o_Para2, o_Para3, o_Para4));
        }*/

        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        // Read / write personal settings to Registry main key
        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        /*
        private static string GetRegPath(eReg e_Registry) {
            string s_Path = ms_RegistryRoot;
            if (e_Registry == eReg.Main)
                return s_Path;

            //s_Path += @"\WorkDirs\" + frmMain.WorkDir.Replace("\\", "/");
            if (e_Registry == eReg.WorkDir)
                return s_Path;

            //ListViewEx.kItem k_Item = frmMain.SelectedFile;
            return e_Regi
        }

        public static void RegistryWrite(eReg e_Registry, string s_Name, object o_Value) {
            RegistryKey i_Key = Registry.CurrentUser.CreateSubKey(GetRegPath(e_Registry));
            i_Key.SetValue(s_Name, o_Value);
            i_Key.Close();
        }
        
        public static object RegistryRead(eReg e_Registry, string s_Name, object o_Default) {
            RegistryKey i_Key = Registry.CurrentUser.OpenSubKey(GetRegPath(e_Registry));
            if (i_Key == null)
                return o_Default;

            object o_Value = i_Key.GetValue(s_Name, o_Default);
            i_Key.Close();
            return o_Value;
        }

        /// <summary>
        /// Retrieves a list of all working directories
        /// </summary>
        public static string[] GetWorkDirectories() {
            RegistryKey i_Key = Registry.CurrentUser.CreateSubKey(ms_RegistryRoot + @"\WorkDirs");
            string[] s_Sub = i_Key.GetSubKeyNames();

            for (int i = 0; i < s_Sub.Length; i++) {
                s_Sub[i] = s_Sub[i].Replace("/", "\\");
            }
            return s_Sub;
        }

        /// <summary>
        /// Adds or removes a working directory subkey in the Registry
        /// </summary>
        public static void AddRemoveWorkDir(bool b_Add, string s_Path) {
            s_Path = ms_RegistryRoot + @"\WorkDirs\" + s_Path.Replace("\\", "/");

            if (b_Add) Registry.CurrentUser.CreateSubKey(s_Path);
            else Registry.CurrentUser.DeleteSubKeyTree(s_Path);
        }

        /// <summary>
        /// Removes a work state subkey in the Registry
        /// or delete ALL workstates
        /// </summary>
        public static void RemoveWorkState(bool b_RemoveALL) {
            try {
                if (b_RemoveALL)
                    Registry.CurrentUser.DeleteSubKeyTree(GetRegPath(eReg.WorkDir) + @"\WorkStates");
                else
                    Registry.CurrentUser.DeleteSubKeyTree(GetRegPath(eReg.WorkState));
            } catch { }
        }

        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

        public static void CreateShortcut(string s_ShortcutPath, string s_ObjectPath, string s_CmdLine, eShowMode e_Mode) {
            if (Environment.OSVersion.Platform != PlatformID.Win32NT) {
                // to run this command on Windows 98/ME use IShellLinkA instead!
                AutoClosingMessageBox.show("CreateShortcut requires NT Platforms!", "Error");
                return;
            }

            IShellLinkW i_ShellLink = null;
            try {
                i_ShellLink = (IShellLinkW)new ShellLinkObject();
                i_ShellLink.SetPath(s_ObjectPath);
                i_ShellLink.SetShowCmd((int)e_Mode);
                i_ShellLink.SetArguments(s_CmdLine);

                UCOMIPersistFile i_Persist = (UCOMIPersistFile)i_ShellLink;
                i_Persist.Save(s_ShortcutPath, true);
            } catch (Exception Ex) { AutoClosingMessageBox.show( "Error creating shortcut:\n" + Ex.Message, "Error"); } finally { Marshal.ReleaseComObject(i_ShellLink); }
        }
        
        /// <summary>
        /// Creates program shortcuts in Quicklaunch bar and Startmenu\Programs
        /// this is done only ONCE at the first run of the program
        /// If the user deletes the shortcuts they will not be created anew
        /// </summary>
        public static void CreateShortcuts() {
            bool b_Created = (int)RegistryRead(eReg.Main, "ShortcutsCreated", 0) == 1;

            string s_Quick = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
                           + @"\Microsoft\Internet Explorer\Quick Launch\SqlBuilder.lnk";

            string s_Progs = Environment.GetFolderPath(Environment.SpecialFolder.Programs)
                           + @"\SqlBuilder.lnk";

            // If the shortcuts already exist -> update them. 
            // (Maybe the user has moved the Exe to another place on his harddisk)
            if (!b_Created || File.Exists(s_Quick))
                CreateShortcut(s_Quick, Application.ExecutablePath, "", eShowMode.Normal);

            if (!b_Created || File.Exists(s_Progs))
                CreateShortcut(s_Progs, Application.ExecutablePath, "", eShowMode.Normal);

            RegistryWrite(eReg.Main, "ShortcutsCreated", 1);
        }*/

        /// <summary>
        /// If there is no yet any program associated with SQL files -> set SqlBuilder as handler for SQL files (create s_NewKey)
        /// If there is already  a program associated with SQL files -> add menu entry "Open with SqlBuilder" to Explorer context menu
        /// s_Ext     = "sql"
        /// s_Menu    = "Open with SqlBuilder"
        /// s_App     = Path to SqlBuilder.exe
        /// s_CmdLine = "/print" (optional)
        /// s_NewKey  = "SqlBuilder.Editor" = name of new Registry key if required
        /// </summary>
        public static bool RegisterFileExtension(string s_Ext, string s_Menu, string s_App, string s_CmdLine, string s_NewKey) {
            RegistryKey i_Ext = Registry.CurrentUser.CreateSubKey(@"Software\Classes\." + s_Ext);
            if (i_Ext == null)
                return false;

            // open existing handler (e.g. "sqlfile.7.1") or create new Handler (e.g. "SqlBuilder.Editor")
            string s_Handler = ObjToString(i_Ext.GetValue(""));
            if (s_Handler.Length == 0) {
                s_Handler = s_NewKey;
                i_Ext.SetValue("", s_Handler);

                RegistryKey i_Icon = i_Ext.CreateSubKey("DefaultIcon");
                i_Icon.SetValue("", s_App + ",0");
            }

            if (string.Compare(s_Handler, s_NewKey, true) == 0)
                s_Menu = "open"; // Open SQL files on doubleclick with SqlBuilder

            RegistryKey i_Handler = Registry.CurrentUser.CreateSubKey(@"Software\Classes\" + s_Handler);
            if (i_Handler == null) // failed
                return false;

            RegistryKey i_Cmd = i_Handler.CreateSubKey("shell").CreateSubKey(s_Menu).CreateSubKey("command");
            if (i_Cmd == null)
                return false;

            i_Cmd.SetValue("", string.Format("\"{0}\" {1} \"%1\"", s_App, s_CmdLine));
            return true;
        }

        /// <summary>
        /// Opens a text file (ANSII, UTF8 or Unicode) and reads its content into a string
        /// returns null on error
        /// </summary>
        static public string ReadFileIntoString(Form i_Owner, string s_Path) {
            FileStream i_Stream = null;
            StreamReader i_Reader = null;
            try {
                i_Stream = File.OpenRead(s_Path);
                i_Reader = new StreamReader(i_Stream, Encoding.Default);
                // single LF -> CR + LF
                return ReplaceCRLF(i_Reader.ReadToEnd().Trim());
            } catch (Exception Ex) {
                AutoClosingMessageBox.show("Error reading\n" + s_Path + "\n" + Ex.Message, "Error");
                return null;
            } finally {
                if (i_Reader != null) i_Reader.Close();
                if (i_Stream != null) i_Stream.Close();
            }
        }

        /// <summary>
        /// Saves the given string into a text file (Unicode, UTF8 or ANSII)
        /// </summary>
        static public bool SaveStringToFile(Form i_Owner, string s_Path, string s_Data, Encoding e_Enc) {
            StreamWriter i_Writer = null;
            try {
                if (!CreateFolderTree(i_Owner, GetPath(s_Path)))
                    return false;

                i_Writer = new StreamWriter(s_Path, false, e_Enc);
                i_Writer.Write(s_Data);
                return true;
            } catch (Exception Ex) {
                AutoClosingMessageBox.error("Error saving file:\n" + s_Path + "\n" + Ex.Message);
                return false;
            } finally {
                if (i_Writer != null) i_Writer.Close();
            }
        }

        /// <summary>
        /// Save a text file and open it with the associated program
        /// </summary>
        public static void SaveAndOpenFile(Form i_Owner, string s_File, string s_Data, Encoding e_Enc, ProcessWindowStyle e_Wnd) {
            if (!SaveStringToFile(i_Owner, s_File, s_Data, e_Enc))
                return;

            OpenFile(i_Owner, s_File, e_Wnd);
        }

        public static void OpenFile(Form i_Owner, string s_File, ProcessWindowStyle e_Wnd) {
            try {
                Process i_Proc = new Process();
                i_Proc.StartInfo.FileName = s_File;
                i_Proc.StartInfo.WindowStyle = e_Wnd;
                i_Proc.Start();
            } catch (Exception Ex) {
                AutoClosingMessageBox.error("Error executing file:\n" + s_File + "\n" + Ex.Message);
            }
        }

        // creates all not yet existing subfolders in s_Path
        // Example: s_Folder = "C:\Temp\Test\Extract\"  or
        //          s_Folder = "C:\Temp\Test\Extract"
        // If only the path    "C:\Temp" exists the subfolders "Test" and "Extract" will be created
        public static bool CreateFolderTree(Form i_Owner, string s_Folder) {
            bool b_OK = false;
            string s_Err = "Error creating folder tree:\n" + s_Folder;

            try { b_OK = CreateFolderTree2(s_Folder); } catch (Exception Ex) { s_Err += "\n" + Ex.Message; }

            if (!b_OK) AutoClosingMessageBox.error(s_Err);
            return b_OK;
        }
        private static bool CreateFolderTree2(string s_Folder) {
            if (s_Folder == null || s_Folder.Length == 0)
                return false;

            if (Directory.Exists(s_Folder))
                return true;

            s_Folder = s_Folder.Trim("\\".ToCharArray());

            // recursively create the parent folders by cutting the last folder
            if (!CreateFolderTree2(Path.GetDirectoryName(s_Folder)))
                return false;

            Directory.CreateDirectory(s_Folder);
            return true;
        }

        public static bool DeleteFile(Form i_Owner, string s_File) {
            try {
                File.Delete(s_File);
                return true;
            } catch (Exception Ex) {
                AutoClosingMessageBox.error("Error deleting file:\n" + s_File + "\n" + Ex.Message);
                return false;
            }
        }

        /// <summary>
        /// This recursive function searches all files in the given folder and its subfolders
        /// s_Filter = "*.Dll|*.Exe|*.Dat" (pipe delimited extensions)
        /// s_Path   = Start path
        /// </summary>
        static public void EnumFiles(ArrayList i_FileList, string s_Path, string s_Filter, int s32_Level) {
            string[] s_ExtList = s_Filter.Split('|');
            foreach (string s_Ext in s_ExtList) {
                RecursiveEnumFiles(i_FileList, s_Path, s_Ext.Trim(), s32_Level);
            }
        }

        /// <summary>
        /// Exception if directory does not exist
        /// </summary>
        static private void RecursiveEnumFiles(ArrayList i_FileList, string s_Path, string s_Filter, int s32_Level) {
            if (s_Path == null || s_Path == "")
                return;

            s_Path = Terminate(s_Path);
            if (s_Path.EndsWith(".svn\\") || s_Path.EndsWith("_svn\\")) // Skip Subversion directories
                return;

            //if (s_Path.EndsWith(Defaults.DelSysObj)) // Skip deleted files
              //  return;

            string[] s_Files = Directory.GetFiles(s_Path, s_Filter);
            foreach (string s_File in s_Files) {
                i_FileList.Add(s_File);
            }

            if (s32_Level > 0) {
                string[] s_Dirs = Directory.GetDirectories(s_Path);
                foreach (string s_Dir in s_Dirs) {
                    RecursiveEnumFiles(i_FileList, s_Dir, s_Filter, s32_Level - 1);
                }
            }
        }

        /// <summary>
        /// "C:\Temp\Test.htm"  -->  "Test.htm"
        /// </summary>
        static public string GetFileName(string s_Path) {
            if (s_Path == null)
                return "";

            int Pos = s_Path.LastIndexOf(@"\");
            if (Pos <= 0)
                return s_Path;

            return s_Path.Substring(Pos + 1);
        }

        /// <summary>
        /// "C:\Temp\Test.htm"  -->  "C:\Temp\"
        /// </summary>
        static public string GetPath(string s_Path) {
            if (s_Path == null)
                return "";

            int Pos = s_Path.LastIndexOf(@"\");
            if (Pos <= 0)
                return "";

            return s_Path.Substring(0, Pos + 1);
        }

        /// <summary>
        /// "C:\Temp\Test.HTM"        --> ".htm"
        /// "C:\Project\.svn\entries" --> ""
        /// </summary>
        static public string GetFileExtension(string s_Path) {
            s_Path = GetFileName(s_Path);

            int Pos = s_Path.LastIndexOf('.');
            if (Pos <= 0)
                return "";

            return s_Path.Substring(Pos).ToLower();
        }

        /// <summary>
        /// "C:\Temp"  -->  "C:\Temp\"
        /// </summary>
        static public string Terminate(string s_Path) {
            if (s_Path == null || s_Path.Length == 0)
                return "";

            if (!s_Path.EndsWith("\\")) s_Path += "\\";
            return s_Path;
        }

        /// <summary>
        /// returns the .ToString() value or "" if object == null
        /// floats and doubles are displayed with a dot instead of a comma
        /// </summary>
        public static string ObjToString(object Obj) {
            if (Obj == null)
                return "";

            string s_Val = Obj.ToString();
            if (Obj is float || Obj is double)
                return s_Val.Replace(',', '.');

            return s_Val;
        }

        /// <summary>
        /// After copying files from CD/DVD with the stupid Windows Explorer they are write protected
        /// This function removes write protection
        /// </summary>
        public static void RemoveWriteProtection(string s_File) {
            // Files which don't exist or which are on a server and are owned by another user 
            // would crash the following command
            try { File.SetAttributes(s_File, FileAttributes.Archive); } catch { }
        }

        /// <summary>
        /// cuts the beginning of the string at the first occurence of s_Delimiter
        /// </summary>
        public static string CutBeginAt(string s_In, string s_Delimiter) {
            if (s_In == null)
                return "";

            int Pos = IndexOf(s_In, s_Delimiter, 0);
            if (Pos >= 0) return s_In.Substring(Pos + s_Delimiter.Length);
            else return s_In;
        }

        /// <summary>
        /// cuts the rest of the string at the LAST occurence of s_Delimiter
        /// </summary>
        public static string CutEndReverseAt(string s_In, string s_Delimiter) {
            if (s_In == null)
                return "";

            int Pos = LastIndexOf(s_In, s_Delimiter, s_In.Length - 1);
            if (Pos >= 0) return s_In.Substring(0, Pos);
            else return s_In;
        }

        /// <summary>
        /// Casting without exceptions
        /// </summary>
        public static int ToInt(object o_Value) {
            if (o_Value == null)
                return 0;

            return (int)o_Value;
        }

        /// <summary>
        /// case insensitive LastIndexOf()
        /// </summary>
        public static int LastIndexOf(string s_Source, string s_Value, int s32_StartIndex) {
            if (s_Source == null || s_Value == null || s_Source.Length == 0 || s_Value.Length == 0)
                return -1;

            s32_StartIndex = Math.Max(s32_StartIndex, 0);
            s32_StartIndex = Math.Min(s32_StartIndex, s_Source.Length - 1);

            return CultureInfo.InvariantCulture.CompareInfo.LastIndexOf(s_Source, s_Value, s32_StartIndex, CompareOptions.IgnoreCase);
        }

        /// <summary>
        /// case insensitive IndexOf()
        /// </summary>
        public static int IndexOf(string s_Source, string s_Value, int s32_StartIndex) {
            if (s_Source == null || s_Value == null || s_Source.Length == 0 || s_Value.Length == 0)
                return -1;

            s32_StartIndex = Math.Max(s32_StartIndex, 0);
            s32_StartIndex = Math.Min(s32_StartIndex, s_Source.Length - 1);

            return CultureInfo.InvariantCulture.CompareInfo.IndexOf(s_Source, s_Value, s32_StartIndex, CompareOptions.IgnoreCase);
        }

        /// <summary>
        /// Case insensitive Split which allows strings as split parameter
        /// </summary>
        public static string[] SplitEx(string s_In, string s_Delim) {
            if (s_In == null || s_In.Trim() == "")
                return new string[] { };

            ArrayList i_PosList = new ArrayList();

            int s32_Pos = 0;
            while (true) {
                s32_Pos = IndexOf(s_In, s_Delim, s32_Pos);
                if (s32_Pos < 0)
                    break;

                i_PosList.Add(s32_Pos);
                s32_Pos += s_Delim.Length;
            }
            i_PosList.Add(s_In.Length);

            string[] s_Split = new string[i_PosList.Count];

            int s32_Start = 0;
            for (int i = 0; i < i_PosList.Count; i++) {
                int s32_End = (int)i_PosList[i];
                s_Split[i] = s_In.Substring(s32_Start, s32_End - s32_Start);
                s32_Start = s32_End + s_Delim.Length;
            }
            return s_Split;
        }

        /// <summary>
        /// The stupid string.Split() command returns an arry with one empty string if s_In is empty
        /// This function returns a string array of zero length if s_In is empty
        /// </summary>
        public static string[] SplitEx(string s_In, char u16_Delim) {
            if (s_In == null || s_In.Trim() == "")
                return new string[] { };

            return s_In.Split(u16_Delim);
        }

        /// <summary>
        /// gets the right n characters of a string
        /// </summary>
        public static string Right(string s_In, int s32_Count) {
            if (s_In == null)
                return "";

            if (s32_Count <= 0)
                return "";

            if (s32_Count >= s_In.Length)
                return s_In;

            return s_In.Substring(s_In.Length - s32_Count);
        }

        /// <summary>
        /// gets the left n characters of a string
        /// </summary>
        public static string Left(string s_In, int s32_Count) {
            if (s_In == null)
                return "";

            if (s32_Count <= 0)
                return "";

            if (s32_Count >= s_In.Length)
                return s_In;

            return s_In.Substring(0, s32_Count);
        }

        /// <summary>
        /// converts "This is a very long long text" -->  "This is a..."
        /// </summary>
        public static string ShortenText(string s_Text, int s32_MaxLen) {
            if (s_Text == null)
                return "";

            if (s32_MaxLen == 0 || s_Text.Length <= s32_MaxLen)
                return s_Text;

            return s_Text.Substring(0, s32_MaxLen) + "...";
        }

        /// <summary>
        /// single LF -> CR + LF
        /// </summary>
        public static string ReplaceCRLF(string s_In) {
            return s_In.Replace("\r", "").Replace("\n", "\r\n");
        }

        /// <summary>
        /// replace characters which are not valid for HTML
        /// </summary>
        static public string ReplaceHtml(string s_Text) {
            if (s_Text == null)
                return "";

            s_Text = s_Text.Replace("&", "&amp;"); // has to be the first replacement !
            s_Text = s_Text.Replace("<", "&lt;");
            s_Text = s_Text.Replace(">", "&gt;");
            s_Text = s_Text.Replace("  ", " &nbsp;");
            s_Text = s_Text.Replace("\"", "&quot;");
            s_Text = s_Text.Replace("\r", "");          // first remove \r
            s_Text = s_Text.Replace("\n", "<br>\r\n");  // then replace remaining \n
            return s_Text;
        }

        /// <summary>
        /// replace characters which are not valid for RTF
        /// </summary>
        static public string ReplaceRtf(string s_Text) {
            if (s_Text == null)
                return "";

            s_Text = s_Text.Replace(@"\", @"\\"); // FIRST !!!
            s_Text = s_Text.Replace(@"{", @"\{");
            s_Text = s_Text.Replace(@"}", @"\}");
            return s_Text;
        }

        /// <summary>
        /// returns colors to be used in <font color=#334455>
        /// </summary>
        static public string GetHtmlColor(Color c_Col) {
            int s32_Color = c_Col.ToArgb() & 0xFFFFFF;
            return "#" + s32_Color.ToString("X6");
        }

        /// <summary>
        /// Limits a rectangle to the screen bounds of the monitor which contains the recatngle
        /// </summary>
        public static void LimitOnScreen(ref int Left, ref int Top, ref int Width, ref int Height) {
            Rectangle k_Screen = Screen.FromRectangle(new Rectangle(Left, Top, Width, Height)).WorkingArea;

            Width = Math.Max(0, Math.Min(k_Screen.Width, Width));
            Height = Math.Max(0, Math.Min(k_Screen.Height, Height));
            Left = Math.Max(k_Screen.X, Math.Min(k_Screen.X + k_Screen.Width - Width, Left));
            Top = Math.Max(k_Screen.Y, Math.Min(k_Screen.Y + k_Screen.Height - Height, Top));
        }

        /// <summary>
        /// returns a location of a rectangle of the size k_Size which is centered inside k_Bounds
        /// </summary>
        public static Point CenterToRectangle(Rectangle k_Bounds, Size k_Size) {
            int X = k_Bounds.Left + (k_Bounds.Width - k_Size.Width) / 2;
            int Y = k_Bounds.Top + (k_Bounds.Height - k_Size.Height) / 2;
            return new Point(X, Y);
        }

        /// <summary>
        /// Centers a window to its owner if exists or to the monitor on which the window has its largest portion
        /// (CenterParent() is not very util because the parent may not be set)
        /// If the owner is still in its OnLoad() processing (which means that the owner is not yet on the screen)
        /// the form will be centered to the owner of the owner
        /// </summary>
        public static void CenterWindow(Form frm) {
            Rectangle k_Bounds;
            if (frm.Owner != null) k_Bounds = new Rectangle(frm.Owner.Location, frm.Owner.Size);
            else k_Bounds = Screen.FromControl(frm).WorkingArea;

            frm.Location = CenterToRectangle(k_Bounds, frm.Size);
        }

        public static string FormatSize(int s32_Size) {
            if (s32_Size < 1024)
                return s32_Size.ToString() + " Byte";

            if (s32_Size < 1024 * 1024) {
                s32_Size *= 10;
                s32_Size /= 1024;
                return string.Format("{0}.{1} KB", s32_Size / 10, s32_Size % 10);
            } else {
                s32_Size *= 10;
                s32_Size /= 1024 * 1024;
                return string.Format("{0}.{1} MB", s32_Size / 10, s32_Size % 10);
            }
        }

        /// <summary>
        /// Returns a text string which explains the Win32 API error code
        /// </summary>
        public static string ExplainApiError(int s32_Error) {
            const int FORMAT_MESSAGE_FROM_SYSTEM = 0x1000;

            StringBuilder s_Msg = new StringBuilder(1000);

            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                FormatMessageW(FORMAT_MESSAGE_FROM_SYSTEM, 0, s32_Error, 0, s_Msg, s_Msg.Capacity, 0);
            else
                FormatMessageA(FORMAT_MESSAGE_FROM_SYSTEM, 0, s32_Error, 0, s_Msg, s_Msg.Capacity, 0);

            if (s_Msg.Length == 0)
                s_Msg.Append("Windows has no explanation for this error code.");

            return string.Format("Error {0}: {1}", s32_Error, s_Msg);
        }

        /// <summary>
        /// Calculates the MD5 of a string
        /// </summary>
        public static string CalcMD5(string s_Text) {
            MD5 i_MD5 = new MD5CryptoServiceProvider();
            byte[] u8_Text = Encoding.Unicode.GetBytes(s_Text);
            byte[] u8_MD5 = i_MD5.ComputeHash(u8_Text);

            // Convert Byte array to Hex value-string
            string s_Out = "";
            for (int i = 0; i < u8_MD5.Length; i++) {
                s_Out += u8_MD5[i].ToString("X2");
            }
            return s_Out;
        }

        /// <summary>
        /// Encrypt/Decrypt a string with a password
        /// returns "" if string cannot be decrypted
        /// </summary>
        public static string Crypt(string s_Data, string s_Password, bool b_Encrypt) {
            byte[] u8_Salt = new byte[] { 0x26, 0x19, 0x81, 0x4E, 0xA0, 0x6D, 0x95, 0x34, 0x26, 0x75, 0x64, 0x05, 0xF6 };

            PasswordDeriveBytes i_Pass = new PasswordDeriveBytes(s_Password, u8_Salt);

            Rijndael i_Alg = Rijndael.Create();
            i_Alg.Key = i_Pass.GetBytes(32);
            i_Alg.IV = i_Pass.GetBytes(16);

            ICryptoTransform i_Trans = (b_Encrypt) ? i_Alg.CreateEncryptor() : i_Alg.CreateDecryptor();

            MemoryStream i_Mem = new MemoryStream();
            CryptoStream i_Crypt = new CryptoStream(i_Mem, i_Trans, CryptoStreamMode.Write);

            try {
                byte[] u8_Data;
                if (b_Encrypt) u8_Data = Encoding.Unicode.GetBytes(s_Data);
                else u8_Data = Convert.FromBase64String(s_Data);

                i_Crypt.Write(u8_Data, 0, u8_Data.Length);
                i_Crypt.Close();

                if (b_Encrypt) return Convert.ToBase64String(i_Mem.ToArray());
                else return Encoding.Unicode.GetString(i_Mem.ToArray());
            } catch { return ""; }
        }

        /// <summary>
        /// Reads a binary resource (The resource must be declared as "embedded" in Visual Studio!!)
        /// Example: s_IconName = "Stop.ico"
        /// </summary>
        public static Icon ReadEmbeddedIconResource(string s_IconName) {
            Assembly i_Ass = Assembly.GetExecutingAssembly();
            Stream i_Strm = i_Ass.GetManifestResourceStream("SqlBuilder.Resources." + s_IconName);
            return new Icon(i_Strm);
        }

        public static int GetCurrentThread() {
            return GetCurrentThreadId();
        }

        public static int GetWindowThread(IntPtr h_Wnd) {
            int s32_ProcessID;
            return GetWindowThreadProcessId(h_Wnd, out s32_ProcessID);
        }

        /// <summary>
        /// returns a string of random characters with the length s32_Len
        /// consisting of only characters found in s_Chars
        /// Each time this function is called with the same Seed it will return the same string!!
        /// </summary>
        public static string GetRandomString(int s32_Len, int s32_Seed, string s_Chars) {
            Random i_Random = new Random(s32_Seed);

            StringBuilder s_Out = new StringBuilder(s32_Len);
            for (int i = 0; i < s32_Len; i++) {
                // Next() returns a nonnegative random number less than the specified maximum.
                int Pos = i_Random.Next(s_Chars.Length);
                s_Out.Append(s_Chars[Pos]);
            }
            return s_Out.ToString();
        }

        public static int GetStringChecksum(string s_String) {
            int s32_Sum = 0;
            foreach (char c_Chr in s_String) {
                s32_Sum += c_Chr;
            }
            return s32_Sum;
        }

        public static bool isCode(this RichTextBox box) {
            return isCode(box.Text);
        }

        public static bool isCode(string text) {
            return isCode(text.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None));
        }

        public static bool isCode(string[] text) {
            return true;
        }
    }
}
