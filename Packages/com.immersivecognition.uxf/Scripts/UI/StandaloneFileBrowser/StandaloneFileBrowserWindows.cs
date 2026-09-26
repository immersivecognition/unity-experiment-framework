#if UNITY_STANDALONE_WIN

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace SFB {
    /// <summary>
    /// Windows file dialogs implemented through the Win32 common-dialog APIs.
    /// Keeping this adapter in the package avoids a runtime dependency on
    /// legacy managed desktop dialog assemblies.
    /// </summary>
    public class StandaloneFileBrowserWindows : IStandaloneFileBrowser {
        private const int OFN_ALLOWMULTISELECT = 0x00000200;
        private const int OFN_EXPLORER = 0x00080000;
        private const int OFN_FILEMUSTEXIST = 0x00001000;
        private const int OFN_NOCHANGEDIR = 0x00000008;
        private const int OFN_OVERWRITEPROMPT = 0x00000002;
        private const int OFN_PATHMUSTEXIST = 0x00000800;
        private const uint BIF_RETURNONLYFSDIRS = 0x00000001;
        private const uint BIF_NEWDIALOGSTYLE = 0x00000040;
        private const uint BFFM_INITIALIZED = 0x00000001;
        private const uint BFFM_SETSELECTIONW = 0x00000467;

        private const int FileBufferCapacity = 32768;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct OpenFileName {
            public int lStructSize;
            public IntPtr hwndOwner;
            public IntPtr hInstance;
            [MarshalAs(UnmanagedType.LPWStr)] public string lpstrFilter;
            [MarshalAs(UnmanagedType.LPWStr)] public string lpstrCustomFilter;
            public int nMaxCustFilter;
            public int nFilterIndex;
            public StringBuilder lpstrFile;
            public int nMaxFile;
            public StringBuilder lpstrFileTitle;
            public int nMaxFileTitle;
            [MarshalAs(UnmanagedType.LPWStr)] public string lpstrInitialDir;
            [MarshalAs(UnmanagedType.LPWStr)] public string lpstrTitle;
            public int Flags;
            public short nFileOffset;
            public short nFileExtension;
            [MarshalAs(UnmanagedType.LPWStr)] public string lpstrDefExt;
            public IntPtr lCustData;
            public IntPtr lpfnHook;
            [MarshalAs(UnmanagedType.LPWStr)] public string lpTemplateName;
            public IntPtr pvReserved;
            public int dwReserved;
            public int FlagsEx;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct BrowseInfo {
            public IntPtr hwndOwner;
            public IntPtr pidlRoot;
            public StringBuilder pszDisplayName;
            [MarshalAs(UnmanagedType.LPWStr)] public string lpszTitle;
            public uint ulFlags;
            public IntPtr lpfn;
            public IntPtr lParam;
            public int iImage;
        }

        private delegate int BrowseCallbackProc(IntPtr windowHandle, uint message, IntPtr lParam, IntPtr data);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr GetActiveWindow();

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr SendMessage(IntPtr windowHandle, uint message, IntPtr wParam, [MarshalAs(UnmanagedType.LPWStr)] string lParam);

        [DllImport("comdlg32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetOpenFileName(ref OpenFileName fileName);

        [DllImport("comdlg32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetSaveFileName(ref OpenFileName fileName);

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr SHBrowseForFolder(ref BrowseInfo browseInfo);

        [DllImport("shell32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SHGetPathFromIDList(IntPtr itemIdList, StringBuilder path);

        [DllImport("ole32.dll")]
        private static extern void CoTaskMemFree(IntPtr memory);

        public string[] OpenFilePanel(string title, string directory, ExtensionFilter[] extensions, bool multiselect) {
            StringBuilder fileBuffer = new StringBuilder(FileBufferCapacity);
            OpenFileName fileName = CreateOpenFileName(title, directory, extensions, fileBuffer, multiselect);

            if (!GetOpenFileName(ref fileName)) return new string[0];
            return ParseOpenFileResult(fileBuffer.ToString(), multiselect);
        }

        public void OpenFilePanelAsync(string title, string directory, ExtensionFilter[] extensions, bool multiselect, Action<string[]> cb) {
            cb.Invoke(OpenFilePanel(title, directory, extensions, multiselect));
        }

        public string[] OpenFolderPanel(string title, string directory, bool multiselect) {
            BrowseCallbackProc callback = null;
            if (!string.IsNullOrEmpty(directory)) {
                callback = (windowHandle, message, lParam, data) => {
                    if (message == BFFM_INITIALIZED) {
                        SendMessage(windowHandle, BFFM_SETSELECTIONW, IntPtr.Zero, directory);
                    }
                    return 0;
                };
            }

            BrowseInfo browseInfo = new BrowseInfo {
                hwndOwner = GetActiveWindow(),
                pszDisplayName = new StringBuilder(FileBufferCapacity),
                lpszTitle = title,
                ulFlags = BIF_RETURNONLYFSDIRS | BIF_NEWDIALOGSTYLE,
                lpfn = callback == null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(callback)
            };

            IntPtr itemIdList = SHBrowseForFolder(ref browseInfo);
            GC.KeepAlive(callback);
            if (itemIdList == IntPtr.Zero) return new string[0];

            try {
                StringBuilder path = new StringBuilder(FileBufferCapacity);
                return SHGetPathFromIDList(itemIdList, path)
                    ? new[] { path.ToString() }
                    : new string[0];
            }
            finally {
                CoTaskMemFree(itemIdList);
            }
        }

        public void OpenFolderPanelAsync(string title, string directory, bool multiselect, Action<string[]> cb) {
            cb.Invoke(OpenFolderPanel(title, directory, multiselect));
        }

        public string SaveFilePanel(string title, string directory, string defaultName, ExtensionFilter[] extensions) {
            StringBuilder fileBuffer = new StringBuilder(FileBufferCapacity);
            if (!string.IsNullOrEmpty(directory) || !string.IsNullOrEmpty(defaultName)) {
                string initialPath = string.IsNullOrEmpty(directory)
                    ? defaultName
                    : Path.Combine(directory, defaultName ?? string.Empty);
                fileBuffer.Append(initialPath);
            }

            OpenFileName fileName = CreateOpenFileName(title, directory, extensions, fileBuffer, false);
            fileName.Flags = OFN_EXPLORER | OFN_NOCHANGEDIR | OFN_OVERWRITEPROMPT | OFN_PATHMUSTEXIST;
            fileName.lpstrDefExt = GetDefaultExtension(extensions);

            return GetSaveFileName(ref fileName) ? fileBuffer.ToString() : string.Empty;
        }

        public void SaveFilePanelAsync(string title, string directory, string defaultName, ExtensionFilter[] extensions, Action<string> cb) {
            cb.Invoke(SaveFilePanel(title, directory, defaultName, extensions));
        }

        private static OpenFileName CreateOpenFileName(string title, string directory, ExtensionFilter[] extensions, StringBuilder fileBuffer, bool multiselect) {
            int flags = OFN_EXPLORER | OFN_FILEMUSTEXIST | OFN_NOCHANGEDIR | OFN_PATHMUSTEXIST;
            if (multiselect) flags |= OFN_ALLOWMULTISELECT;

            return new OpenFileName {
                lStructSize = Marshal.SizeOf(typeof(OpenFileName)),
                hwndOwner = GetActiveWindow(),
                lpstrFilter = GetFilterFromFileExtensionList(extensions),
                nFilterIndex = 1,
                lpstrFile = fileBuffer,
                nMaxFile = fileBuffer.Capacity,
                lpstrInitialDir = string.IsNullOrEmpty(directory) ? null : directory,
                lpstrTitle = title,
                Flags = flags
            };
        }

        private static string[] ParseOpenFileResult(string result, bool multiselect) {
            string[] parts = result.Split(new[] { '\0' }, StringSplitOptions.RemoveEmptyEntries);
            if (!multiselect || parts.Length <= 1) return parts;

            string directory = parts[0];
            string[] paths = new string[parts.Length - 1];
            for (int i = 1; i < parts.Length; i++) paths[i - 1] = Path.Combine(directory, parts[i]);
            return paths;
        }

        private static string GetFilterFromFileExtensionList(ExtensionFilter[] extensions) {
            if (extensions == null || extensions.Length == 0) return "All files\0*.*\0\0";

            var filterParts = new StringBuilder();
            foreach (var filter in extensions) {
                if (filterParts.Length > 0) filterParts.Append('\0');
                filterParts.Append(string.IsNullOrEmpty(filter.Name) ? "Files" : filter.Name);
                filterParts.Append('\0');
                if (filter.Extensions == null || filter.Extensions.Length == 0) {
                    filterParts.Append("*.*");
                }
                else {
                    for (int i = 0; i < filter.Extensions.Length; i++) {
                        if (i > 0) filterParts.Append(';');
                        string extension = NormalizeExtension(filter.Extensions[i]);
                        filterParts.Append(string.IsNullOrEmpty(extension) ? "*.*" : "*." + extension);
                    }
                }
            }
            filterParts.Append("\0\0");
            return filterParts.ToString();
        }

        private static string GetDefaultExtension(ExtensionFilter[] extensions) {
            if (extensions == null || extensions.Length == 0 || extensions[0].Extensions == null || extensions[0].Extensions.Length == 0)
                return string.Empty;
            return NormalizeExtension(extensions[0].Extensions[0]);
        }

        private static string NormalizeExtension(string extension) {
            if (string.IsNullOrEmpty(extension)) return string.Empty;

            extension = extension.Trim();
            if (extension == "*" || extension == "*.*") return string.Empty;
            if (extension.StartsWith("*.", StringComparison.Ordinal)) extension = extension.Substring(2);
            return extension.TrimStart('.');
        }
    }
}

#endif
