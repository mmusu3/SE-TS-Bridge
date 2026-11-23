using System.Runtime.InteropServices;

namespace Installer;

static class FolderPicker
{
    const uint PickFolders = 0x20;
    const uint ForceFileSystem = 0x40;
    const uint FileSystemPath = 0x80058000;
    const int Cancelled = unchecked((int)0x800704C7);

    internal static string? Show(IntPtr owner, string title, string initialPath)
    {
        var dialog = (IFileDialog)new FileOpenDialog();

        try
        {
            dialog.GetOptions(out uint options);
            dialog.SetOptions(options | PickFolders | ForceFileSystem);
            dialog.SetTitle(title);

            if (Directory.Exists(initialPath))
            {
                Guid shellItemId = typeof(IShellItem).GUID;

                Marshal.ThrowExceptionForHR(SHCreateItemFromParsingName(
                    Path.GetFullPath(initialPath), IntPtr.Zero, ref shellItemId, out IShellItem folder));

                try
                {
                    dialog.SetFolder(folder);
                }
                finally
                {
                    Marshal.ReleaseComObject(folder);
                }
            }

            int result = dialog.Show(owner);

            if (result == Cancelled)
                return null;

            Marshal.ThrowExceptionForHR(result);

            dialog.GetResult(out IShellItem selectedFolder);

            try
            {
                selectedFolder.GetDisplayName(FileSystemPath, out IntPtr name);

                try
                {
                    return Marshal.PtrToStringUni(name)
                        ?? throw new InvalidOperationException("The selected folder has no filesystem path.");
                }
                finally
                {
                    Marshal.FreeCoTaskMem(name);
                }
            }
            finally
            {
                Marshal.ReleaseComObject(selectedFolder);
            }
        }
        finally
        {
            Marshal.ReleaseComObject(dialog);
        }
    }

    [DllImport("shell32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
    static extern int SHCreateItemFromParsingName(
        string path, IntPtr bindContext, ref Guid interfaceId, [MarshalAs(UnmanagedType.Interface)] out IShellItem item);

    [ComImport, Guid("DC1C5A9C-E88A-4DDE-A5A1-60F82A20AEF7")]
    class FileOpenDialog;

    [ComImport, Guid("42F85136-DB7E-439C-85F1-E4075D135FC8"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IFileDialog
    {
        [PreserveSig] int Show(IntPtr owner);
        void SetFileTypes(uint count, IntPtr filterSpec);
        void SetFileTypeIndex(uint index);
        void GetFileTypeIndex(out uint index);
        void Advise(IntPtr events, out uint cookie);
        void Unadvise(uint cookie);
        void SetOptions(uint options);
        void GetOptions(out uint options);
        void SetDefaultFolder(IShellItem folder);
        void SetFolder(IShellItem folder);
        void GetFolder(out IShellItem folder);
        void GetCurrentSelection(out IShellItem item);
        void SetFileName([MarshalAs(UnmanagedType.LPWStr)] string name);
        void GetFileName(out IntPtr name);
        void SetTitle([MarshalAs(UnmanagedType.LPWStr)] string title);
        void SetOkButtonLabel([MarshalAs(UnmanagedType.LPWStr)] string text);
        void SetFileNameLabel([MarshalAs(UnmanagedType.LPWStr)] string label);
        void GetResult(out IShellItem item);
    }

    [ComImport, Guid("43826D1E-E718-42EE-BC55-A1E261C37BFE"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IShellItem
    {
        void BindToHandler(IntPtr bindContext, ref Guid handlerId, ref Guid interfaceId, out IntPtr result);
        void GetParent(out IShellItem parent);
        void GetDisplayName(uint displayNameType, out IntPtr name);
    }
}
