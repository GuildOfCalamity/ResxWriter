using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ResxWriter
{
    /// <summary>
    /// Parts borrowed from AstroGrep v4.4.9 source.
    /// </summary>
    public static class Utils
    {
        #region [ListView Stuff]
        public const uint LVM_FIRST = 0x1000;
        public const uint LVM_GETHEADER = LVM_FIRST + 31;
        public const uint LVM_SETSELECTEDCOLUMN = LVM_FIRST + 140;
        public const uint LVM_SETEXTENDEDLISTVIEWSTYLE = LVM_FIRST + 54;
        public const uint LVS_EX_GRIDLINES        = 0x00000001;
        public const uint LVS_EX_SUBITEMIMAGES    = 0x00000002;
        public const uint LVS_EX_CHECKBOXES       = 0x00000004;
        public const uint LVS_EX_TRACKSELECT      = 0x00000008;
        public const uint LVS_EX_HEADERDRAGDROP   = 0x00000010;
        public const uint LVS_EX_FULLROWSELECT    = 0x00000020; // Applies to report mode only
        public const uint LVS_EX_ONECLICKACTIVATE = 0x00000040;
        public const uint LVS_EX_TWOCLICKACTIVATE = 0x00000080;
        public const uint LVS_EX_FLATSB           = 0x00000100;
        public const uint LVS_EX_REGIONAL         = 0x00000200;
        public const uint LVS_EX_INFOTIP          = 0x00000400; // ListView does InfoTips for you
        public const uint LVS_EX_UNDERLINEHOT     = 0x00000800;
        public const uint LVS_EX_UNDERLINECOLD    = 0x00001000;
        public const uint LVS_EX_MULTIWORKAREAS   = 0x00002000;
        public const uint LVS_EX_LABELTIP         = 0x00004000; // ListView unfolds partly hidden labels if it does not have infotip text
        public const uint LVS_EX_BORDERSELECT     = 0x00008000; // Border selection style instead of highlight
        public const uint LVS_EX_DOUBLEBUFFER     = 0x00010000; // Used with LVM_SETEXTENDEDLISTVIEWSTYLE
        public const uint LVS_EX_HIDELABELS       = 0x00020000;
        public const uint LVS_EX_SINGLEROW        = 0x00040000;
        public const uint LVS_EX_SNAPTOGRID       = 0x00080000;  // Icons automatically snap to grid.
        public const uint LVS_EX_SIMPLESELECT     = 0x00100000;  // Also changes overlay rendering to top right for icon mode.
        public const uint LVS_EX_JUSTIFYCOLUMNS   = 0x00200000;  // Icons are lined up in columns that use up the whole view area.
        public const uint LVS_EX_TRANSPARENTBKGND = 0x00400000;  // Background is painted by the parent via WM_PRINTCLIENT
        public const uint LVS_EX_TRANSPARENTSHADOWTEXT = 0x00800000; // Enable shadow text on transparent backgrounds only (useful with bitmaps)
        public const uint LVS_EX_AUTOAUTOARRANGE  = 0x01000000;  // Icons automatically arrange if no icon positions have been set
        public const uint LVS_EX_HEADERINALLVIEWS = 0x02000000;  // Display column header in all view modes
        public const uint LVS_EX_AUTOCHECKSELECT  = 0x08000000;
        public const uint LVS_EX_AUTOSIZECOLUMNS  = 0x10000000;
        public const uint LVS_EX_COLUMNSNAPPOINTS = 0x40000000;
        public const uint LVS_EX_COLUMNOVERFLOW  = 0x80000000;

        [StructLayout(LayoutKind.Sequential)]
        struct HDITEM
        {
            public Mask mask;
            public int cxy;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string pszText;
            public IntPtr hbm;
            public int cchTextMax;
            public Format fmt;
            public IntPtr lParam;
            // _WIN32_IE >= 0x0300 
            public int iImage;
            public int iOrder;
            // _WIN32_IE >= 0x0500
            public uint type;
            public IntPtr pvFilter;
            // _WIN32_WINNT >= 0x0600
            public uint state;

            [Flags]
            public enum Mask
            {
                Format = 0x4,       // HDI_FORMAT
            };

            [Flags]
            public enum Format
            {
                SortDown = 0x200,   // HDF_SORTDOWN
                SortUp = 0x400,     // HDF_SORTUP
            };
        };

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr SendMessage(IntPtr hWnd, UInt32 msg, IntPtr wParam, IntPtr lParam);

        [DllImport("uxtheme.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
        public static extern int SetWindowTheme(IntPtr hwnd, string pszSubAppName, string pszSubIdList);
#endregion

        /// <summary>
        /// Invoke action delegate on main thread if required.
        /// </summary>
        /// <param name="obj">Object to check for InvokeRequired</param>
        /// <param name="action">Action delegate to perform (either on the current thread or invoked).</param>
        /// <remarks>
        /// Extension for any object that supports the ISynchronizeInvoke interface (such as WinForms
        /// controls). This will handle the InvokeRequired check and call the action delegate from
        /// the appropriate thread.
        /// </remarks>
        public static void InvokeIfRequired(this ISynchronizeInvoke obj, System.Windows.Forms.MethodInvoker action)
        {
            if (obj.InvokeRequired)
            {
                var args = new object[0];
                try
                {
                    obj.Invoke(action, args);
                }
                catch { }
            }
            else
            {
                action();
            }
        }

        /// <summary>
        /// Converts a delimited string to a <see cref="System.Drawing.Font"/>.
        /// </summary>
        /// <param name="font">"Calibri,9,Regular</param>
        /// <returns><see cref="System.Drawing.Font"/></returns>
        public static System.Drawing.Font ConvertStringToFont(string font)
        {
            string[] fontValues = font.Split(new char[] { ',', '|', ';' }, StringSplitOptions.RemoveEmptyEntries);

            return new System.Drawing.Font(fontValues[0], float.Parse(fontValues[1], System.Globalization.CultureInfo.InvariantCulture), (System.Drawing.FontStyle)Enum.Parse(typeof(System.Drawing.FontStyle), fontValues[2], true), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        }

        /// <summary>
        /// Returns the declaring type's namespace.
        /// </summary>
        public static string GetCurrentNamespace() => System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType?.Namespace;

        /// <summary>
        /// Returns the declaring type's assembly name.
        /// </summary>
        public static string GetCurrentAssemblyName() => System.Reflection.Assembly.GetExecutingAssembly().GetName().Name; //System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType?.Assembly.FullName;

        /// <summary>
        /// Returns the AssemblyVersion, not the FileVersion.
        /// </summary>
        public static Version GetCurrentAssemblyVersion() => System.Reflection.Assembly.GetExecutingAssembly().GetName().Version ?? new Version();

        /// <summary>
        /// Deteremine if current OS is Vista or higher.
        /// </summary>
        public static bool IsWindowsVistaOrLater
        {
            get => Environment.OSVersion.Platform == PlatformID.Win32NT && Environment.OSVersion.Version >= new Version(6, 0, 6000);
        }

        /// <summary>
        /// Determine if current OS is Windows 7 or higher.
        /// </summary>
        public static bool IsWindows7OrLater
        {
            get => Environment.OSVersion.Version >= new Version(6, 1);
        }

        /// <summary>
        /// Gets Windows DPI percent scale setting.
        /// </summary>
        /// <param name="gfx">Current graphics context</param>
        /// <returns>Windows DPI percent scale setting</returns>
        /// <remarks>We don't close the Graphics parameter here and rely on calling method to handle it since it could be used later.</remarks>
        public static int GetCurrentDPI(Graphics gfx)
        {
            float dpix_percent = (gfx.DpiX / 96F) * 100F;

            // Lose the decimal precision.
            int dpi_percent = (int)dpix_percent;

            // Set 100 percent DPI setting as minimum value.
            if (dpi_percent < 100)
                dpi_percent = 100;

            return dpi_percent;
        }

        /// <summary>
        /// Returns the <paramref name="filePath"/>'s <see cref="System.Drawing.Icon"/>.
        /// </summary>
        public static System.Drawing.Icon GetFileIcon(string filePath, bool large = false)
        {
            int iconIndex = 0;

            // If there are no details set for the icon, then we must use the shell to get the icon for the target.
            if (filePath.Length == 0)
            {
                // Use the FileIcon object to get the icon.
                FileIcon.SHGetFileInfoConstants flags = FileIcon.SHGetFileInfoConstants.SHGFI_ICON | FileIcon.SHGetFileInfoConstants.SHGFI_ATTRIBUTES;

                if (large)
                    flags = flags | FileIcon.SHGetFileInfoConstants.SHGFI_LARGEICON;
                else
                    flags = flags | FileIcon.SHGetFileInfoConstants.SHGFI_SMALLICON;

                FileIcon fileIcon = new FileIcon(filePath, flags);

                return fileIcon.ShellIcon;
            }
            else
            {
                // Use ExtractIconEx to get the icon.
                IntPtr[] hIconEx = new IntPtr[1] { IntPtr.Zero };
                int iconCount = 0;

                if (large)
                    iconCount = UnManagedMethods.ExtractIconEx(filePath, iconIndex, hIconEx, null, 1);
                else
                    iconCount = UnManagedMethods.ExtractIconEx(filePath, iconIndex, null, hIconEx, 1);

                // If success then return as a GDI+ object
                Icon icon = null;
                if (hIconEx[0] != IntPtr.Zero)
                {
                    icon = Icon.FromHandle(hIconEx[0]);
                    //Utils.UnManagedMethods.DestroyIcon(hIconEx[0]);
                }

                return icon;
            }
        }

        #region [UAC Stuff]
        /// <summary>
        /// Determines if current user has admin privileges.
        /// </summary>
        /// <returns>true if does, false if not.</returns>
        public static bool HasAdminPrivileges()
        {
            WindowsIdentity id = WindowsIdentity.GetCurrent();
            WindowsPrincipal p = new WindowsPrincipal(id);
            return p.IsInRole(WindowsBuiltInRole.Administrator);
        }

        [DllImport("user32")]
        public static extern UInt32 SendMessage(IntPtr hWnd, UInt32 msg, UInt32 wParam, UInt32 lParam);

        internal const int BCM_FIRST = 0x1600; //Normal button
        internal const int BCM_SETSHIELD = (BCM_FIRST + 0x000C); //Elevated button

        /// <summary>
        /// Add the UAC shield to the given Button.
        /// </summary>
        /// <param name="b">Button to add shield</param>
        public static void AddShieldToButton(Button b)
        {
            if (IsWindowsVistaOrLater)// System.Environment.OSVersion.Version.Major >= 6)
            {
                b.FlatStyle = FlatStyle.System;
                SendMessage(b.Handle, BCM_SETSHIELD, 0, 0xFFFFFFFF);
            }
        }

        /// <summary>
        /// Removes the UAC shield from the given Button.
        /// </summary>
        /// <param name="b">Button to remove shield</param>
        public static void RemoveShieldFromButton(Button b)
        {
            if (IsWindowsVistaOrLater)// System.Environment.OSVersion.Version.Major >= 6)
            {
                b.FlatStyle = FlatStyle.System;
                SendMessage(b.Handle, BCM_SETSHIELD, 0, 0x0);
            }
        }

        /// <summary>
        /// Attempts to run the given process as an admim process.
        /// </summary>
        /// <param name="path">Full path to process</param>
        /// <param name="args">Arguments to process</param>
        /// <param name="runas">true will use runas verb, false assumes manifest is part of process</param>
        public static void AttemptPrivilegeEscalation(string path, string args, bool runas)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException("path");

            if (!File.Exists(path))
                throw new FileNotFoundException("path");

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.UseShellExecute = true;
            startInfo.FileName = path;
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;

            // Only do this for Vista+ since xp has an older runas dialog.
            // If runas set to false will assume that the application has a manifest and so we don't need this.
            if (IsWindowsVistaOrLater && runas)
            {
                startInfo.Verb = "runas"; // will bring up the UAC run-as menu when this ProcessStartInfo is used
            }

            if (!string.IsNullOrEmpty(args))
            {
                startInfo.Arguments = args;
            }

            try
            {
                Process p = Process.Start(startInfo);
                //if (modal)
                //   p.WaitForExit();
            }

            catch (System.ComponentModel.Win32Exception) //occurs when the user has clicked Cancel on the UAC prompt.
            {
                return;
            }
        }
        #endregion
    }

    #region [Windows 7 (or higher) TaskBar Progress]
    /// <summary>
    /// Helper class to set taskbar progress on Windows 7+ systems.
    /// </summary>
    public static class TaskbarProgress
    {
        /// <summary>
        /// Available taskbar progress states
        /// </summary>
        public enum TaskbarStates
        {
            /// <summary>No progress displayed</summary>
            NoProgress = 0,
            /// <summary>Indeterminate </summary>
            Indeterminate = 0x1,
            /// <summary>Normal</summary>
            Normal = 0x2,
            /// <summary>Error</summary>
            Error = 0x4,
            /// <summary>Paused</summary>
            Paused = 0x8
        }

        [ComImportAttribute()]
        [GuidAttribute("ea1afb91-9e28-4b86-90e9-9e9f8a5eefaf")]
        [InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
        private interface ITaskbarList3
        {
            // ITaskbarList
            [PreserveSig]
            void HrInit();
            [PreserveSig]
            void AddTab(IntPtr hwnd);
            [PreserveSig]
            void DeleteTab(IntPtr hwnd);
            [PreserveSig]
            void ActivateTab(IntPtr hwnd);
            [PreserveSig]
            void SetActiveAlt(IntPtr hwnd);

            // ITaskbarList2
            [PreserveSig]
            void MarkFullscreenWindow(IntPtr hwnd, [MarshalAs(UnmanagedType.Bool)] bool fFullscreen);

            // ITaskbarList3
            [PreserveSig]
            void SetProgressValue(IntPtr hwnd, UInt64 ullCompleted, UInt64 ullTotal);
            [PreserveSig]
            void SetProgressState(IntPtr hwnd, TaskbarStates state);
        }

        [GuidAttribute("56FDF344-FD6D-11d0-958A-006097C9A090")]
        [ClassInterfaceAttribute(ClassInterfaceType.None)]
        [ComImportAttribute()]
        private class TaskbarInstance
        {
        }

        private static bool taskbarSupported = Utils.IsWindows7OrLater;
        private static ITaskbarList3 taskbarInstance = taskbarSupported ? (ITaskbarList3)new TaskbarInstance() : null;

        /// <summary>
        /// Sets the state of the taskbar progress.
        /// </summary>
        /// <param name="windowHandle">current form handle</param>
        /// <param name="taskbarState">desired state</param>
        public static void SetState(IntPtr windowHandle, TaskbarStates taskbarState)
        {
            if (taskbarSupported)
            {
                taskbarInstance.SetProgressState(windowHandle, taskbarState);
            }
        }

        /// <summary>
        /// Sets the value of the taskbar progress.
        /// </summary>
        /// <param name="windowHandle">currnet form handle</param>
        /// <param name="progressValue">desired progress value</param>
        /// <param name="progressMax">maximum progress value</param>
        public static void SetValue(IntPtr windowHandle, double progressValue, double progressMax)
        {
            if (taskbarSupported)
            {
                taskbarInstance.SetProgressValue(windowHandle, (ulong)progressValue, (ulong)progressMax);
            }
        }
    }
    #endregion

    #region File Deletion
    /// <summary>
    /// Helper class to delete a file via the recycle bin.
    /// </summary>
    public class FileDeletion
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct SHFILEOPSTRUCT
        {
            public IntPtr hwnd;
            [MarshalAs(UnmanagedType.U4)]
            public int wFunc;
            public string pFrom;
            public string pTo;
            public short fFlags;
            [MarshalAs(UnmanagedType.Bool)]
            public bool fAnyOperationsAborted;
            public IntPtr hNameMappings;
            public string lpszProgressTitle;
        }

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        private static extern int SHFileOperation(ref SHFILEOPSTRUCT FileOp);

        private const int FO_DELETE = 3;
        private const int FOF_ALLOWUNDO = 0x40;
        private const int FOF_NOCONFIRMATION = 0x10;    //Don't prompt the user.;

        /// <summary>
        /// Do not show a dialog during the process
        /// </summary>
        private const int FOF_SILENT = 0x0004;
        ///// <summary>
        ///// Do not ask the user to confirm selection
        ///// </summary>
        //private const int FOF_NOCONFIRMATION = 0x0010;
        ///// <summary>
        ///// Delete the file to the recycle bin.  (Required flag to send a file to the bin
        ///// </summary>
        //private const int FOF_ALLOWUNDO = 0x0040;
        /// <summary>
        /// Do not show the names of the files or folders that are being recycled.
        /// </summary>
        private const int FOF_SIMPLEPROGRESS = 0x0100;
        /// <summary>
        /// Surpress errors, if any occur during the process.
        /// </summary>
        private const int FOF_NOERRORUI = 0x0400;
        /// <summary>
        /// Warn if files are too big to fit in the recycle bin and will need
        /// to be deleted completely.
        /// </summary>
        private const int FOF_WANTNUKEWARNING = 0x4000;

        /// <summary>
        /// Deletes the file using the recycle bin.
        /// </summary>
        /// <param name="path"></param>
        public static void Delete(string path)
        {
            SHFILEOPSTRUCT shf = new SHFILEOPSTRUCT();
            shf.wFunc = FO_DELETE;
            shf.fFlags = FOF_ALLOWUNDO | FOF_NOCONFIRMATION | FOF_NOERRORUI | FOF_SILENT;
            shf.pFrom = path + "\0" + "\0";

            SHFileOperation(ref shf);
        }
    }
    #endregion

    #region [File Icon]
    /// <summary>
    /// Enables extraction of icons for any file type from
    /// the Shell.
    /// </summary>
    public class FileIcon
    {
        #region [Win32_API]
        private const int MAX_PATH = 260;
        private const int FORMAT_MESSAGE_ALLOCATE_BUFFER = 0x100;
        private const int FORMAT_MESSAGE_ARGUMENT_ARRAY = 0x2000;
        private const int FORMAT_MESSAGE_FROM_HMODULE = 0x800;
        private const int FORMAT_MESSAGE_FROM_STRING = 0x400;
        private const int FORMAT_MESSAGE_FROM_SYSTEM = 0x1000;
        private const int FORMAT_MESSAGE_IGNORE_INSERTS = 0x200;
        private const int FORMAT_MESSAGE_MAX_WIDTH_MASK = 0xFF;

        [StructLayout(LayoutKind.Sequential)]
        private struct SHFILEINFO
        {
            public IntPtr hIcon;
            public int iIcon;
            public int dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_PATH)]
            public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        }

        [DllImport("shell32")]
        private static extern int SHGetFileInfo(
           string pszPath,
           int dwFileAttributes,
           ref SHFILEINFO psfi,
           uint cbFileInfo,
           uint uFlags);

        [DllImport("user32.dll")]
        private static extern int DestroyIcon(IntPtr hIcon);

        [DllImport("kernel32")]
        private extern static int FormatMessage(
           int dwFlags,
           IntPtr lpSource,
           int dwMessageId,
           int dwLanguageId,
           string lpBuffer,
           uint nSize,
           int argumentsLong);

        [DllImport("kernel32")]
        private extern static int GetLastError();
        #endregion

        #region [Member Variables]
        private string fileName;
        private string displayName;
        private string typeName;
        private SHGetFileInfoConstants flags;
        private Icon fileIcon;
        #endregion

        #region [Enumerations]
        /// <summary>
        /// 
        /// </summary>
        [Flags]
        public enum SHGetFileInfoConstants : int
        {
            /// <summary>get icon</summary>
            SHGFI_ICON = 0x100,
            /// <summary>get display name</summary>
            SHGFI_DISPLAYNAME = 0x200,
            /// <summary>get type name</summary>
            SHGFI_TYPENAME = 0x400,
            /// <summary>get attributes</summary>
            SHGFI_ATTRIBUTES = 0x800,
            /// <summary>get icon location </summary>
            SHGFI_ICONLOCATION = 0x1000,
            /// <summary>return exe type </summary>
            SHGFI_EXETYPE = 0x2000,
            /// <summary>get system icon index </summary>
            SHGFI_SYSICONINDEX = 0x4000,
            /// <summary>put a link overlay on icon </summary>
            SHGFI_LINKOVERLAY = 0x8000,
            /// <summary>show icon in selected state </summary>
            SHGFI_SELECTED = 0x10000,
            /// <summary>get only specified attributes </summary>
            SHGFI_ATTR_SPECIFIED = 0x20000,
            /// <summary>get large icon </summary>
            SHGFI_LARGEICON = 0x0,
            /// <summary>get small icon </summary>
            SHGFI_SMALLICON = 0x1,
            /// <summary>get open icon </summary>
            SHGFI_OPENICON = 0x2,
            /// <summary>get shell size icon </summary>
            SHGFI_SHELLICONSIZE = 0x4,
            /// <summary>use passed dwFileAttribute</summary>
            //SHGFI_PIDL = 0x8,                  // pszPath is a pidl 
            SHGFI_USEFILEATTRIBUTES = 0x10,
            /// <summary>apply the appropriate overlays</summary>
            SHGFI_ADDOVERLAYS = 0x000000020,
            /// <summary>Get the index of the overlay</summary>
            SHGFI_OVERLAYINDEX = 0x000000040
        }
        #endregion

        #region [Implementation]
        /// <summary>
        /// Get/Set the flags used to extract the icon.
        /// </summary>
        public FileIcon.SHGetFileInfoConstants Flags
        {
            get
            {
                return flags;
            }
            set
            {
                flags = value;
            }
        }

        /// <summary>
        /// Get/Set the filename to get the icon for.
        /// </summary>
        public string FileName
        {
            get
            {
                return fileName;
            }
            set
            {
                fileName = value;
            }
        }

        /// <summary>
        /// Gets the icon for the chosen file.
        /// </summary>
        public Icon ShellIcon
        {
            get
            {
                return fileIcon;
            }
        }

        /// <summary>
        /// Gets the display name for the selected file if the SHGFI_DISPLAYNAME flag was set.
        /// </summary>
        public string DisplayName
        {
            get
            {
                return displayName;
            }
        }

        /// <summary>
        /// Gets the type name for the selected file if the SHGFI_TYPENAME flag was set.
        /// </summary>
        public string TypeName
        {
            get
            {
                return typeName;
            }
        }

        /// <summary>
        ///  Gets the information for the specified file name and flags.
        /// </summary>
        public void GetInfo()
        {
            fileIcon = null;
            typeName = "";
            displayName = "";

            SHFILEINFO shfi = new SHFILEINFO();
            uint shfiSize = (uint)Marshal.SizeOf(shfi.GetType());

            int ret = SHGetFileInfo(
               fileName, 0, ref shfi, shfiSize, (uint)(flags));
            if (ret != 0)
            {
                if (shfi.hIcon != IntPtr.Zero)
                {
                    fileIcon = System.Drawing.Icon.FromHandle(shfi.hIcon);
                    // Now owned by the GDI+ object
                    //DestroyIcon(shfi.hIcon);
                }
                typeName = shfi.szTypeName;
                displayName = shfi.szDisplayName;
            }
            else
            {

                int err = GetLastError();
                Console.WriteLine("Error {0}", err);
                string txtS = new string('\0', 256);
                int len = FormatMessage(
                   FORMAT_MESSAGE_FROM_SYSTEM | FORMAT_MESSAGE_IGNORE_INSERTS,
                   IntPtr.Zero, err, 0, txtS, 256, 0);
                Console.WriteLine("Len {0} text {1}", len, txtS);

                // throw exception

            }
        }

        /// <summary>
        /// Constructs a new, default instance of the FileIcon
        /// class.  Specify the filename and call GetInfo()
        /// to retrieve an icon.
        /// </summary>
        public FileIcon()
        {
            flags = SHGetFileInfoConstants.SHGFI_ICON |
               SHGetFileInfoConstants.SHGFI_DISPLAYNAME |
               SHGetFileInfoConstants.SHGFI_TYPENAME |
               SHGetFileInfoConstants.SHGFI_ATTRIBUTES |
               SHGetFileInfoConstants.SHGFI_EXETYPE;
        }
        /// <summary>
        /// Constructs a new instance of the FileIcon class
        /// and retrieves the icon, display name and type name
        /// for the specified file.		
        /// </summary>
        /// <param name="fileName">The filename to get the icon, 
        /// display name and type name for</param>
        public FileIcon(string fileName) : this()
        {
            this.fileName = fileName;
            GetInfo();
        }
        /// <summary>
        /// Constructs a new instance of the FileIcon class
        /// and retrieves the information specified in the 
        /// flags.
        /// </summary>
        /// <param name="fileName">The filename to get information
        /// for</param>
        /// <param name="flags">The flags to use when extracting the
        /// icon and other shell information.</param>
        public FileIcon(string fileName, FileIcon.SHGetFileInfoConstants flags)
        {
            this.fileName = fileName;
            this.flags = flags;
            GetInfo();
        }
        #endregion
    }
    #endregion

    #region [Unmanaged Methods]
    public class UnManagedMethods
    {
        [DllImport("Shell32", CharSet = CharSet.Auto)]
        internal extern static int ExtractIconEx(
           [MarshalAs(UnmanagedType.LPTStr)] string lpszFile,
           int nIconIndex,
           IntPtr[] phIconLarge,
           IntPtr[] phIconSmall,
           int nIcons);

        [DllImport("user32")]
        internal static extern int DestroyIcon(IntPtr hIcon);
    }
    #endregion
}
