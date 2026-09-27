#if UNITY_STANDALONE_WIN

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace SFB {
    /// <summary>
    /// Windows file/save dialogs use Win32 common-dialog APIs; folder picking uses the Shell Common Item Dialog.
    /// Keeping this adapter in the package avoids a runtime dependency on
    /// legacy managed desktop dialog assemblies.
    /// </summary>
    public class StandaloneFileBrowserWindows : IStandaloneFileBrowser {
        private bool folderDialogOpen;
        private readonly object folderDialogLock = new object();
        private const int OFN_ALLOWMULTISELECT = 0x00000200;
        private const int OFN_EXPLORER = 0x00080000;
        private const int OFN_FILEMUSTEXIST = 0x00001000;
        private const int OFN_NOCHANGEDIR = 0x00000008;
        private const int OFN_OVERWRITEPROMPT = 0x00000002;
        private const int OFN_PATHMUSTEXIST = 0x00000800;
        private const uint CLSCTX_INPROC_SERVER = 0x1;
        private const int COINIT_APARTMENTTHREADED = 0x2;
        private const int ERROR_CANCELLED = unchecked((int)0x800704C7);
        private const uint SIGDN_FILESYSPATH = 0x80058000;
        private const uint FOS_PICKFOLDERS = 0x00000020;
        private const uint FOS_FORCEFILESYSTEM = 0x00000040;
        private const uint FOS_ALLOWMULTISELECT = 0x00000200;
        private const uint FOS_PATHMUSTEXIST = 0x00000800;
        private const uint FOS_NOCHANGEDIR = 0x00000008;
        private const uint FOS_DONTADDTORECENT = 0x02000000;

        private static readonly Guid FileOpenDialogClassId = new Guid("DC1C5A9C-E88A-4DDE-A5A1-60F82A20AEF7");
        private static readonly Guid FileOpenDialogInterfaceId = new Guid("D57C7288-D4AD-4768-BE02-9D969532D960");
        private static readonly Guid ShellItemInterfaceId = new Guid("43826D1E-E718-42EE-BC55-A1E261C37BFE");

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

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr GetActiveWindow();

        [DllImport("comdlg32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetOpenFileName(ref OpenFileName fileName);

        [DllImport("comdlg32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetSaveFileName(ref OpenFileName fileName);

        [DllImport("ole32.dll")]
        private static extern int CoInitializeEx(IntPtr reserved, int coInit);

        [DllImport("ole32.dll")]
        private static extern void CoUninitialize();

        [DllImport("ole32.dll", PreserveSig = true)]
        private static extern int CoCreateInstance(ref Guid classId, IntPtr outer, uint context, ref Guid interfaceId, out IFileOpenDialog dialog);

        [DllImport("shell32.dll", CharSet = CharSet.Unicode, PreserveSig = true)]
        private static extern int SHCreateItemFromParsingName([MarshalAs(UnmanagedType.LPWStr)] string path, IntPtr bindingContext, ref Guid interfaceId, out IShellItem item);

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
            lock (folderDialogLock) {
                if (folderDialogOpen) return new string[0];
                folderDialogOpen = true;
            }
            try {
                return multiselect
                    ? OpenFolderPanelCore(title, directory, true)
                    : OpenFolderPanelCore(title, directory);
            }
            finally {
                lock (folderDialogLock) folderDialogOpen = false;
            }
        }

        public void OpenFolderPanelAsync(string title, string directory, bool multiselect, Action<string[]> cb) {
            if (cb == null) throw new ArgumentNullException(nameof(cb));

            lock (folderDialogLock) {
                if (folderDialogOpen) {
                    PostFolderPanelResult(SynchronizationContext.Current, cb, new string[0]);
                    return;
                }
                folderDialogOpen = true;
            }

            SynchronizationContext callbackContext = SynchronizationContext.Current;
            IntPtr owner = GetActiveWindow();
            Thread dialogThread = new Thread(() => {
                string[] result = new string[0];
                Exception dialogException = null;
                try {
                    result = ShowFolderDialog(owner, title, directory, multiselect);
                }
                catch (Exception exception) {
                    dialogException = exception;
                }
                finally {
                    lock (folderDialogLock) folderDialogOpen = false;
                }

                PostFolderPanelResult(callbackContext, cb, result, dialogException);
            });
            dialogThread.IsBackground = true;
            dialogThread.Name = "UXF Windows folder picker";
            dialogThread.SetApartmentState(ApartmentState.STA);
            try {
                dialogThread.Start();
            }
            catch {
                lock (folderDialogLock) folderDialogOpen = false;
                throw;
            }
        }

        private static void PostFolderPanelResult(SynchronizationContext context, Action<string[]> callback, string[] result, Exception exception = null) {
            SendOrPostCallback invokeCallback = _ => {
                if (exception != null) UnityEngine.Debug.LogException(exception);
                callback(exception == null && result != null ? result : new string[0]);
            };

            if (context != null) context.Post(invokeCallback, null);
            else invokeCallback(null);
        }

        protected virtual string[] OpenFolderPanelCore(string title, string directory) {
            return OpenFolderPanelCore(title, directory, false);
        }

        protected virtual string[] OpenFolderPanelCore(string title, string directory, bool multiselect) {
            // IFileDialog requires an STA COM apartment. Keep COM and shell objects
            // on a dedicated STA thread instead of relying on Unity's thread model.
            IntPtr owner = GetActiveWindow();
            string[] result = null;
            Exception dialogException = null;
            Thread dialogThread = new Thread(() => {
                try {
                    result = ShowFolderDialog(owner, title, directory, multiselect);
                }
                catch (Exception exception) {
                    dialogException = exception;
                }
            });
            dialogThread.IsBackground = true;
            dialogThread.SetApartmentState(ApartmentState.STA);
            dialogThread.Start();
            dialogThread.Join();

            if (dialogException != null) throw dialogException;
            return result ?? new string[0];
        }

        private static string[] ShowFolderDialog(IntPtr owner, string title, string directory, bool multiselect) {
            Guid fileOpenDialogClassId = FileOpenDialogClassId;
            Guid fileOpenDialogInterfaceId = FileOpenDialogInterfaceId;
            Guid shellItemInterfaceId = ShellItemInterfaceId;
            IFileOpenDialog dialog = null;
            bool comInitialized = false;
            string operation = "CoInitializeEx";
            try {
                int initializeResult = CoInitializeEx(IntPtr.Zero, COINIT_APARTMENTTHREADED);
                if (initializeResult < 0) Marshal.ThrowExceptionForHR(initializeResult);
                comInitialized = true;

                operation = "CoCreateInstance";
                int result = CoCreateInstance(ref fileOpenDialogClassId, IntPtr.Zero, CLSCTX_INPROC_SERVER,
                    ref fileOpenDialogInterfaceId, out dialog);
                ThrowIfFailed(result);

                uint options = FOS_PICKFOLDERS | FOS_FORCEFILESYSTEM | FOS_PATHMUSTEXIST | FOS_NOCHANGEDIR | FOS_DONTADDTORECENT;
                if (multiselect) options |= FOS_ALLOWMULTISELECT;
                operation = "IFileDialog.SetOptions";
                ThrowIfFailed(dialog.SetOptions(options));

                if (!string.IsNullOrEmpty(title)) {
                    operation = "IFileDialog.SetTitle";
                    ThrowIfFailed(dialog.SetTitle(title));
                }
                if (!string.IsNullOrEmpty(directory) && Directory.Exists(directory)) {
                    IShellItem initialFolder = null;
                    try {
                        operation = "SHCreateItemFromParsingName";
                        result = SHCreateItemFromParsingName(directory, IntPtr.Zero, ref shellItemInterfaceId, out initialFolder);
                        if (result >= 0) {
                            operation = "IFileDialog.SetDefaultFolder";
                            ThrowIfFailed(dialog.SetDefaultFolder(initialFolder));
                        }
                    }
                    finally {
                        ReleaseComObject(initialFolder);
                    }
                }

                operation = "IFileDialog.Show";
                result = dialog.Show(owner);
                if (result == ERROR_CANCELLED) return new string[0];
                ThrowIfFailed(result);

                if (!multiselect) {
                    IShellItem selectedItem = null;
                    try {
                        operation = "IFileDialog.GetResult";
                        ThrowIfFailed(dialog.GetResult(out selectedItem));
                        operation = "IShellItem.GetDisplayName";
                        return new[] { GetFileSystemPath(selectedItem) };
                    }
                    finally {
                        ReleaseComObject(selectedItem);
                    }
                }

                IShellItemArray selectedItems = null;
                try {
                    operation = "IFileOpenDialog.GetResults";
                    ThrowIfFailed(dialog.GetResults(out selectedItems));
                    uint count;
                    operation = "IShellItemArray.GetCount";
                    ThrowIfFailed(selectedItems.GetCount(out count));
                    string[] paths = new string[checked((int)count)];
                    for (uint i = 0; i < count; i++) {
                        IShellItem item = null;
                        try {
                            operation = "IShellItemArray.GetItemAt";
                            ThrowIfFailed(selectedItems.GetItemAt(i, out item));
                            operation = "IShellItem.GetDisplayName";
                            paths[i] = GetFileSystemPath(item);
                        }
                        finally {
                            ReleaseComObject(item);
                        }
                    }
                    return paths;
                }
                finally {
                    ReleaseComObject(selectedItems);
                }
            }
            catch (COMException exception) {
                throw new COMException("Windows folder dialog failed during " + operation + " (HRESULT 0x" + exception.ErrorCode.ToString("X8") + ").", exception.ErrorCode);
            }
            finally {
                ReleaseComObject(dialog);
                if (comInitialized) CoUninitialize();
            }
        }

        private static string GetFileSystemPath(IShellItem item) {
            IntPtr pathPointer = IntPtr.Zero;
            try {
                ThrowIfFailed(item.GetDisplayName(SIGDN_FILESYSPATH, out pathPointer));
                return Marshal.PtrToStringUni(pathPointer);
            }
            finally {
                if (pathPointer != IntPtr.Zero) Marshal.FreeCoTaskMem(pathPointer);
            }
        }

        private static void ThrowIfFailed(int result) {
            if (result < 0) Marshal.ThrowExceptionForHR(result);
        }

        private static void ReleaseComObject(object instance) {
            if (instance != null && Marshal.IsComObject(instance)) Marshal.ReleaseComObject(instance);
        }

        [ComImport, Guid("D57C7288-D4AD-4768-BE02-9D969532D960"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IFileOpenDialog {
            [PreserveSig] int Show(IntPtr owner);
            [PreserveSig] int SetFileTypes(uint count, IntPtr filters);
            [PreserveSig] int SetFileTypeIndex(uint index);
            [PreserveSig] int GetFileTypeIndex(out uint index);
            [PreserveSig] int Advise(IntPtr events, out uint cookie);
            [PreserveSig] int Unadvise(uint cookie);
            [PreserveSig] int SetOptions(uint options);
            [PreserveSig] int GetOptions(out uint options);
            [PreserveSig] int SetDefaultFolder(IShellItem item);
            [PreserveSig] int SetFolder(IShellItem item);
            [PreserveSig] int GetFolder(out IShellItem item);
            [PreserveSig] int GetCurrentSelection(out IShellItem item);
            [PreserveSig] int SetFileName([MarshalAs(UnmanagedType.LPWStr)] string name);
            [PreserveSig] int GetFileName(out IntPtr name);
            [PreserveSig] int SetTitle([MarshalAs(UnmanagedType.LPWStr)] string title);
            [PreserveSig] int SetOkButtonLabel([MarshalAs(UnmanagedType.LPWStr)] string text);
            [PreserveSig] int SetFileNameLabel([MarshalAs(UnmanagedType.LPWStr)] string label);
            [PreserveSig] int GetResult(out IShellItem item);
            [PreserveSig] int AddPlace(IShellItem item, uint location);
            [PreserveSig] int SetDefaultExtension([MarshalAs(UnmanagedType.LPWStr)] string extension);
            [PreserveSig] int Close(int result);
            [PreserveSig] int SetClientGuid(ref Guid guid);
            [PreserveSig] int ClearClientData();
            [PreserveSig] int SetFilter(IntPtr filter);
            [PreserveSig] int GetResults(out IShellItemArray items);
            [PreserveSig] int GetSelectedItems(out IShellItemArray items);
        }

        [ComImport, Guid("43826D1E-E718-42EE-BC55-A1E261C37BFE"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IShellItem {
            [PreserveSig] int BindToHandler(IntPtr bindingContext, ref Guid handlerId, ref Guid interfaceId, out IntPtr result);
            [PreserveSig] int GetParent(out IShellItem parent);
            [PreserveSig] int GetDisplayName(uint nameKind, out IntPtr name);
            [PreserveSig] int GetAttributes(uint mask, out uint attributes);
            [PreserveSig] int Compare(IShellItem other, uint hint, out int order);
        }

        [ComImport, Guid("B63EA76D-1F85-456F-A19C-48159EFA858B"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IShellItemArray {
            [PreserveSig] int BindToHandler(IntPtr bindingContext, ref Guid handlerId, ref Guid interfaceId, out IntPtr result);
            [PreserveSig] int GetPropertyStore(uint flags, ref Guid interfaceId, out IntPtr result);
            [PreserveSig] int GetPropertyDescriptionList(IntPtr key, ref Guid interfaceId, out IntPtr result);
            [PreserveSig] int GetAttributes(uint flags, uint attributes, out uint result);
            [PreserveSig] int GetCount(out uint count);
            [PreserveSig] int GetItemAt(uint index, out IShellItem item);
            [PreserveSig] int EnumItems(out IntPtr enumerator);
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
