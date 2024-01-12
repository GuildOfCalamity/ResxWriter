using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ResxWriter
{
    /// <summary>
    /// Some parts of this class were borrowed from AstroGrep v4.4.9.
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
        /// Determine if current OS is Vista or higher.
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
                Icon icon = null;
                try
                {
                    // Use ExtractIconEx to get the icon.
                    IntPtr[] hIconEx = new IntPtr[1] { IntPtr.Zero };
                    int iconCount = 0;

                    if (large)
                        iconCount = UnManagedMethods.ExtractIconEx(filePath, iconIndex, hIconEx, null, 1);
                    else
                        iconCount = UnManagedMethods.ExtractIconEx(filePath, iconIndex, null, hIconEx, 1);

                    // If success then return as a GDI+ object
                    if (hIconEx[0] != IntPtr.Zero)
                    {
                        icon = Icon.FromHandle(hIconEx[0]);
                        //Utils.UnManagedMethods.DestroyIcon(hIconEx[0]);
                    }
                }
                catch (RuntimeWrappedException rwe) // catch any non-CLS exceptions
                {
                    var s = rwe.WrappedException as string;
                    if (s != null)
                        Logger.Instance.Write($"{s}", LogLevel.Error);
                }
                catch (Win32Exception ex) //occurs when the user has clicked Cancel on the UAC prompt.
                {
                    Logger.Instance.Write($"[{ex.ErrorCode}]{ex.Message}", LogLevel.Error);
                }

                return icon;
            }
        }

        #region [Shortcut Stuff]
        /// <summary>
        /// Creates a shortcut (lnk file) using for the application.
        /// </summary>
        /// <param name="location">Directory where the shortcut should be created.</param>
        /// <param name="create">true to create shortcut, false to delete it</param>
        public static bool CreateApplicationShortcut(string location, string appName, bool create)
        {
            bool result = false;
            string path = System.IO.Path.Combine(location, string.Format("{0}.lnk", appName));
            string oldPath = string.Format("{0}\\{1}.url", location, appName);

            if (create)
            {
                try
                {
                    // Create application shortcut.
                    using (ShellLink shortcut = new ShellLink())
                    {
                        shortcut.Target = Application.ExecutablePath;
                        shortcut.WorkingDirectory = System.IO.Path.GetDirectoryName(Application.ExecutablePath);
                        shortcut.Description = string.Empty;
                        shortcut.DisplayMode = ShellLink.LinkDisplayMode.edmNormal;
                        shortcut.Save(path);
                    }
                    result = true;
                }
                catch (Exception ex)
                {
                    Logger.Instance.Write($"Unable to create shortcut at {location} with message {ex.Message}", LogLevel.Error);
                    result = false;
                }
            }
            else
            {
                try
                {
                    // Delete shortcut if it exists.
                    if (System.IO.File.Exists(path))
                        System.IO.File.Delete(path);

                    // Delete old url if exists.
                    if (System.IO.File.Exists(oldPath))
                        System.IO.File.Delete(oldPath);

                    result = true;
                }
                catch (Exception ex)
                {
                    Logger.Instance.Write($"Unable to delete shortcut at {location} with message {ex.Message}", LogLevel.Error);
                    result = false;
                }
            }

            return result;
        }

        /// <summary>
        /// Checks to see if a shortcut exists.
        /// </summary>
        /// <param name="location">Directory where the shortcut could be</param>
        /// <returns>true if the shortcut exists, false otherwise</returns>
        public static bool DoesShortcutExist(string location, string appName)
        {
            string path = Path.Combine(location, string.Format("{0}.lnk", appName));
            string oldPath = string.Format("{0}\\{1}.url", location, appName);

            if (File.Exists(path))
                return true;

            // Check for older url based shortcut and create new one.
            if (File.Exists(oldPath))
            {
                // Remove existing.
                CreateApplicationShortcut(location, appName, false);

                // Recreate shortcut.
                CreateApplicationShortcut(location, appName, true);

                return true;
            }

            return false;
        }
        #endregion

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
                throw new FileNotFoundException(path);

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.UseShellExecute = true;
            startInfo.FileName = path;
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;

            // Only do this for Vista+ since xp has an older runas dialog.
            // If runas set to false will assume that the application has
            // a manifest and so we don't need this.
            if (IsWindowsVistaOrLater && runas)
                startInfo.Verb = "runas"; // will bring up the UAC run-as menu when this ProcessStartInfo is used

            if (!string.IsNullOrEmpty(args))
                startInfo.Arguments = args;

            try
            {
                Process p = Process.Start(startInfo);
                //p.WaitForExit();
            }
            catch (RuntimeWrappedException rwe) // catch any non-CLS exceptions
            {
                var s = rwe.WrappedException as string;
                if (s != null)
                    Logger.Instance.Write($"{s}", LogLevel.Error);
            }
            catch (Win32Exception ex) //occurs when the user has clicked Cancel on the UAC prompt.
            {
                Logger.Instance.Write($"[{ex.ErrorCode}]{ex.Message}", LogLevel.Error);
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

    #region ShellLink Object
    /// <summary>
    /// Summary description for ShellLink.
    /// </summary>
    public class ShellLink : IDisposable
    {
        #region ComInterop for IShellLink

        #region IPersist Interface
        [ComImportAttribute()]
        [GuidAttribute("0000010C-0000-0000-C000-000000000046")]
        [InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IPersist
        {
            [PreserveSig]
            // Returns the class identifier for the component object
            void GetClassID(out Guid pClassID);
        }
        #endregion

        #region IPersistFile Interface
        [ComImportAttribute()]
        [GuidAttribute("0000010B-0000-0000-C000-000000000046")]
        [InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IPersistFile
        {
            [PreserveSig]
            void GetClassID(out Guid pClassID);

            // Checks for changes since last file write
            void IsDirty();

            // Opens the specified file and initializes the object from its contents
            void Load([MarshalAs(UnmanagedType.LPWStr)] string pszFileName, uint dwMode);

            // Saves the object into the specified file
            void Save([MarshalAs(UnmanagedType.LPWStr)] string pszFileName, [MarshalAs(UnmanagedType.Bool)] bool fRemember);

            // Notifies the object that save is completed
            void SaveCompleted([MarshalAs(UnmanagedType.LPWStr)] string pszFileName);

            // Gets the current name of the file associated with the object
            void GetCurFile([MarshalAs(UnmanagedType.LPWStr)] out string ppszFileName);
        }
        #endregion

        #region IShellLink Interface
        [ComImportAttribute()]
        [GuidAttribute("000214EE-0000-0000-C000-000000000046")]
        [InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IShellLinkA
        {
            // Retrieves the path and filename of a shell link object
            void GetPath([Out(), MarshalAs(UnmanagedType.LPStr)] StringBuilder pszFile, int cchMaxPath, ref _WIN32_FIND_DATAA pfd, uint fFlags);

            // Retrieves the list of shell link item identifiers
            void GetIDList(out IntPtr ppidl);

            // Sets the list of shell link item identifiers
            void SetIDList(IntPtr pidl);

            // Retrieves the shell link description string
            void GetDescription([Out(), MarshalAs(UnmanagedType.LPStr)] StringBuilder pszFile, int cchMaxName);

            // Sets the shell link description string
            void SetDescription([MarshalAs(UnmanagedType.LPStr)] string pszName);

            // Retrieves the name of the shell link working directory
            void GetWorkingDirectory([Out(), MarshalAs(UnmanagedType.LPStr)] StringBuilder pszDir, int cchMaxPath);

            // Sets the name of the shell link working directory
            void SetWorkingDirectory([MarshalAs(UnmanagedType.LPStr)] string pszDir);

            // Retrieves the shell link command-line arguments
            void GetArguments([Out(), MarshalAs(UnmanagedType.LPStr)] StringBuilder pszArgs, int cchMaxPath);

            // Sets the shell link command-line arguments
            void SetArguments([MarshalAs(UnmanagedType.LPStr)] string pszArgs);

            // Retrieves or sets the shell link hot key
            void GetHotkey(out short pwHotkey);

            // Retrieves or sets the shell link hot key
            void SetHotkey(short pwHotkey);

            // Retrieves or sets the shell link show command
            void GetShowCmd(out uint piShowCmd);
            // Retrieves or sets the shell link show command
            void SetShowCmd(uint piShowCmd);

            // Retrieves the location (path and index) of the shell link icon
            void GetIconLocation([Out(), MarshalAs(UnmanagedType.LPStr)] StringBuilder pszIconPath, int cchIconPath, out int piIcon);

            // Sets the location (path and index) of the shell link icon
            void SetIconLocation([MarshalAs(UnmanagedType.LPStr)] string pszIconPath, int iIcon);

            // Sets the shell link relative path
            void SetRelativePath([MarshalAs(UnmanagedType.LPStr)] string pszPathRel, uint dwReserved);

            // Resolves a shell link. The system searches for the shell link object and updates the shell link path and its list of identifiers (if necessary)
            void Resolve(IntPtr hWnd, uint fFlags);

            // Sets the shell link path and filename
            void SetPath([MarshalAs(UnmanagedType.LPStr)] string pszFile);
        }

        [ComImportAttribute()]
        [GuidAttribute("000214F9-0000-0000-C000-000000000046")]
        [InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IShellLinkW
        {
            // Retrieves the path and filename of a shell link object
            void GetPath([Out(), MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile, int cchMaxPath, ref _WIN32_FIND_DATAW pfd, uint fFlags);

            // Retrieves the list of shell link item identifiers
            void GetIDList(out IntPtr ppidl);

            // Sets the list of shell link item identifiers
            void SetIDList(IntPtr pidl);

            // Retrieves the shell link description string
            void GetDescription([Out(), MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile, int cchMaxName);

            // Sets the shell link description string
            void SetDescription([MarshalAs(UnmanagedType.LPWStr)] string pszName);

            // Retrieves the name of the shell link working directory
            void GetWorkingDirectory([Out(), MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszDir, int cchMaxPath);

            // Sets the name of the shell link working directory
            void SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] string pszDir);

            // Retrieves the shell link command-line arguments
            void GetArguments([Out(), MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszArgs, int cchMaxPath);

            // Sets the shell link command-line arguments
            void SetArguments([MarshalAs(UnmanagedType.LPWStr)] string pszArgs);

            // Retrieves or sets the shell link hot key
            void GetHotkey(out short pwHotkey);

            // Retrieves or sets the shell link hot key
            void SetHotkey(short pwHotkey);

            // Retrieves or sets the shell link show command
            void GetShowCmd(out uint piShowCmd);

            // Retrieves or sets the shell link show command
            void SetShowCmd(uint piShowCmd);

            // Retrieves the location (path and index) of the shell link icon
            void GetIconLocation([Out(), MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszIconPath, int cchIconPath, out int piIcon);

            // Sets the location (path and index) of the shell link icon
            void SetIconLocation([MarshalAs(UnmanagedType.LPWStr)] string pszIconPath, int iIcon);

            // Sets the shell link relative path
            void SetRelativePath([MarshalAs(UnmanagedType.LPWStr)] string pszPathRel, uint dwReserved);

            // Resolves a shell link. The system searches for the shell link object and updates the shell link path and its list of identifiers (if necessary)
            void Resolve(IntPtr hWnd, uint fFlags);

            // Sets the shell link path and filename
            void SetPath([MarshalAs(UnmanagedType.LPWStr)] string pszFile);
        }
        #endregion

        #region ShellLinkCoClass
        [GuidAttribute("00021401-0000-0000-C000-000000000046")]
        [ClassInterfaceAttribute(ClassInterfaceType.None)]
        [ComImportAttribute()]
        private class CShellLink { }
        #endregion

        #region Private IShellLink enumerations
        private enum EShellLinkGP : uint
        {
            SLGP_SHORTPATH = 1,
            SLGP_UNCPRIORITY = 2
        }

        [Flags]
        private enum EShowWindowFlags : uint
        {
            SW_HIDE = 0,
            SW_SHOWNORMAL = 1,
            SW_NORMAL = 1,
            SW_SHOWMINIMIZED = 2,
            SW_SHOWMAXIMIZED = 3,
            SW_MAXIMIZE = 3,
            SW_SHOWNOACTIVATE = 4,
            SW_SHOW = 5,
            SW_MINIMIZE = 6,
            SW_SHOWMINNOACTIVE = 7,
            SW_SHOWNA = 8,
            SW_RESTORE = 9,
            SW_SHOWDEFAULT = 10,
            SW_MAX = 10
        }
        #endregion

        #region IShellLink Private structs

        [StructLayoutAttribute(LayoutKind.Sequential, Pack = 4, Size = 0, CharSet = CharSet.Unicode)]
        private struct _WIN32_FIND_DATAW
        {
            public uint dwFileAttributes;
            public _FILETIME ftCreationTime;
            public _FILETIME ftLastAccessTime;
            public _FILETIME ftLastWriteTime;
            public uint nFileSizeHigh;
            public uint nFileSizeLow;
            public uint dwReserved0;
            public uint dwReserved1;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)] // MAX_PATH
            public string cFileName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
            public string cAlternateFileName;
        }

        [StructLayoutAttribute(LayoutKind.Sequential, Pack = 4, Size = 0, CharSet = CharSet.Ansi)]
        private struct _WIN32_FIND_DATAA
        {
            public uint dwFileAttributes;
            public _FILETIME ftCreationTime;
            public _FILETIME ftLastAccessTime;
            public _FILETIME ftLastWriteTime;
            public uint nFileSizeHigh;
            public uint nFileSizeLow;
            public uint dwReserved0;
            public uint dwReserved1;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)] // MAX_PATH
            public string cFileName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
            public string cAlternateFileName;
        }

        [StructLayoutAttribute(LayoutKind.Sequential, Pack = 4, Size = 0)]
        private struct _FILETIME
        {
            public uint dwLowDateTime;
            public uint dwHighDateTime;
        }
        #endregion

        #region UnManaged Methods
        private class UnManagedMethods
        {
            [DllImport("Shell32", CharSet = CharSet.Auto)]
            internal extern static int ExtractIconEx(
               [MarshalAs(UnmanagedType.LPTStr)]
               string lpszFile,
               int nIconIndex,
               IntPtr[] phIconLarge,
               IntPtr[] phIconSmall,
               int nIcons);

            [DllImport("user32")]
            internal static extern int DestroyIcon(IntPtr hIcon);
        }
        #endregion

        #endregion

        #region Enumerations
        /// <summary>
        /// Flags determining how the links with missing
        /// targets are resolved.
        /// </summary>
        [Flags]
        public enum EShellLinkResolveFlags : uint
        {
            /// <summary>
            /// Allow any match during resolution.  Has no effect
            /// on ME/2000 or above, use the other flags instead.
            /// </summary>
            SLR_ANY_MATCH = 0x2,
            /// <summary>
            /// Call the Microsoft Windows Installer. 
            /// </summary>
            SLR_INVOKE_MSI = 0x80,
            /// <summary>
            /// Disable distributed link tracking. By default, 
            /// distributed link tracking tracks removable media 
            /// across multiple devices based on the volume name. 
            /// It also uses the UNC path to track remote file 
            /// systems whose drive letter has changed. Setting 
            /// SLR_NOLINKINFO disables both types of tracking.
            /// </summary>
            SLR_NOLINKINFO = 0x40,
            /// <summary>
            /// Do not display a dialog box if the link cannot be resolved. 
            /// When SLR_NO_UI is set, a time-out value that specifies the 
            /// maximum amount of time to be spent resolving the link can 
            /// be specified in milliseconds. The function returns if the 
            /// link cannot be resolved within the time-out duration. 
            /// If the timeout is not set, the time-out duration will be 
            /// set to the default value of 3,000 milliseconds (3 seconds). 
            /// </summary>										    
            SLR_NO_UI = 0x1,
            /// <summary>
            /// Not documented in SDK.  Assume same as SLR_NO_UI but 
            /// intended for applications without a hWnd.
            /// </summary>
            SLR_NO_UI_WITH_MSG_PUMP = 0x101,
            /// <summary>
            /// Do not update the link information. 
            /// </summary>
            SLR_NOUPDATE = 0x8,
            /// <summary>
            /// Do not execute the search heuristics. 
            /// </summary>																																																																																																																																																																																																														
            SLR_NOSEARCH = 0x10,
            /// <summary>
            /// Do not use distributed link tracking. 
            /// </summary>
            SLR_NOTRACK = 0x20,
            /// <summary>
            /// If the link object has changed, update its path and list 
            /// of identifiers. If SLR_UPDATE is set, you do not need to 
            /// call IPersistFile::IsDirty to determine whether or not 
            /// the link object has changed. 
            /// </summary>
            SLR_UPDATE = 0x4
        }

        /// <summary>
        /// 
        /// </summary>
        public enum LinkDisplayMode : uint
        {
            /// <summary></summary>
            edmNormal = EShowWindowFlags.SW_NORMAL,
            /// <summary></summary>
            edmMinimized = EShowWindowFlags.SW_SHOWMINNOACTIVE,
            /// <summary></summary>
            edmMaximized = EShowWindowFlags.SW_MAXIMIZE
        }
        #endregion

        #region Member Variables
        // Use Unicode (W) under NT, otherwise use ANSI		
        private IShellLinkW linkW;
        private IShellLinkA linkA;
        private string shortcutFile = "";
        #endregion

        #region Constructor
        /// <summary>
        /// Creates an instance of the Shell Link object.
        /// </summary>
        public ShellLink()
        {
            if (System.Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                linkW = (IShellLinkW)new CShellLink();
            }
            else
            {
                linkA = (IShellLinkA)new CShellLink();
            }
        }

        /// <summary>
        /// Creates an instance of a Shell Link object
        /// from the specified link file
        /// </summary>
        /// <param name="linkFile">The Shortcut file to open</param>
        public ShellLink(string linkFile) : this()
        {
            Open(linkFile);
        }
        #endregion

        #region Destructor and Dispose
        /// <summary>
        /// Call dispose just in case it hasn't happened yet
        /// </summary>
        ~ShellLink()
        {
            Dispose();
        }

        /// <summary>
        /// Dispose the object, releasing the COM ShellLink object
        /// </summary>
        public void Dispose()
        {
            if (linkW != null)
            {
                Marshal.ReleaseComObject(linkW);
                linkW = null;
            }
            if (linkA != null)
            {
                Marshal.ReleaseComObject(linkA);
                linkA = null;
            }
        }
        #endregion

        #region Implementation
        /// <summary>
        /// 
        /// </summary>
        public string ShortCutFile
        {
            get
            {
                return this.shortcutFile;
            }
            set
            {
                this.shortcutFile = value;
            }
        }

        /// <summary>
        /// Gets a System.Drawing.Icon containing the icon for this
        /// ShellLink object.
        /// </summary>
        public Icon LargeIcon
        {
            get
            {
                return getIcon(true);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public Icon SmallIcon
        {
            get
            {
                return getIcon(false);
            }
        }

        private Icon getIcon(bool large)
        {
            // Get icon index and path:
            int iconIndex = 0;
            StringBuilder iconPath = new StringBuilder(260, 260);
            if (linkA == null)
            {
                linkW.GetIconLocation(iconPath, iconPath.Capacity, out iconIndex);
            }
            else
            {
                linkA.GetIconLocation(iconPath, iconPath.Capacity, out iconIndex);
            }
            string iconFile = iconPath.ToString();

            // If there are no details set for the icon, then we must use
            // the shell to get the icon for the target:
            if (iconFile.Length == 0)
            {
                // Use the FileIcon object to get the icon:
                FileIcon.SHGetFileInfoConstants flags = FileIcon.SHGetFileInfoConstants.SHGFI_ICON |
                   FileIcon.SHGetFileInfoConstants.SHGFI_ATTRIBUTES;
                if (large)
                {
                    flags = flags | FileIcon.SHGetFileInfoConstants.SHGFI_LARGEICON;
                }
                else
                {
                    flags = flags | FileIcon.SHGetFileInfoConstants.SHGFI_SMALLICON;
                }
                FileIcon fileIcon = new FileIcon(Target, flags);
                return fileIcon.ShellIcon;
            }
            else
            {
                // Use ExtractIconEx to get the icon:
                IntPtr[] hIconEx = new IntPtr[1] { IntPtr.Zero };
                int iconCount = 0;
                if (large)
                {
                    iconCount = UnManagedMethods.ExtractIconEx(
                       iconFile,
                       iconIndex,
                       hIconEx,
                       null,
                       1);
                }
                else
                {
                    iconCount = UnManagedMethods.ExtractIconEx(
                       iconFile,
                       iconIndex,
                       null,
                       hIconEx,
                       1);
                }
                // If success then return as a GDI+ object
                Icon icon = null;
                if (hIconEx[0] != IntPtr.Zero)
                {
                    icon = Icon.FromHandle(hIconEx[0]);
                    //UnManagedMethods.DestroyIcon(hIconEx[0]);
                }
                return icon;
            }
        }

        /// <summary>
        /// Gets the path to the file containing the icon for this shortcut.
        /// </summary>
        public string IconPath
        {
            get
            {
                StringBuilder iconPath = new StringBuilder(260, 260);
                int iconIndex = 0;
                if (linkA == null)
                {
                    linkW.GetIconLocation(iconPath, iconPath.Capacity, out iconIndex);
                }
                else
                {
                    linkA.GetIconLocation(iconPath, iconPath.Capacity, out iconIndex);
                }
                return iconPath.ToString();
            }
            set
            {
                StringBuilder iconPath = new StringBuilder(260, 260);
                int iconIndex = 0;
                if (linkA == null)
                {
                    linkW.GetIconLocation(iconPath, iconPath.Capacity, out iconIndex);
                }
                else
                {
                    linkA.GetIconLocation(iconPath, iconPath.Capacity, out iconIndex);
                }
                if (linkA == null)
                {
                    linkW.SetIconLocation(value, iconIndex);
                }
                else
                {
                    linkA.SetIconLocation(value, iconIndex);
                }
            }
        }

        /// <summary>
        /// Gets the index of this icon within the icon path's resources
        /// </summary>
        public int IconIndex
        {
            get
            {
                StringBuilder iconPath = new StringBuilder(260, 260);
                int iconIndex = 0;
                if (linkA == null)
                {
                    linkW.GetIconLocation(iconPath, iconPath.Capacity, out iconIndex);
                }
                else
                {
                    linkA.GetIconLocation(iconPath, iconPath.Capacity, out iconIndex);
                }
                return iconIndex;
            }
            set
            {
                StringBuilder iconPath = new StringBuilder(260, 260);
                int iconIndex = 0;
                if (linkA == null)
                {
                    linkW.GetIconLocation(iconPath, iconPath.Capacity, out iconIndex);
                }
                else
                {
                    linkA.GetIconLocation(iconPath, iconPath.Capacity, out iconIndex);
                }
                if (linkA == null)
                {
                    linkW.SetIconLocation(iconPath.ToString(), value);
                }
                else
                {
                    linkA.SetIconLocation(iconPath.ToString(), value);
                }
            }
        }

        /// <summary>
        /// Gets/sets the fully qualified path to the link's target
        /// </summary>
        public string Target
        {
            get
            {
                StringBuilder target = new StringBuilder(260, 260);
                if (linkA == null)
                {
                    _WIN32_FIND_DATAW fd = new _WIN32_FIND_DATAW();
                    linkW.GetPath(target, target.Capacity, ref fd, (uint)EShellLinkGP.SLGP_UNCPRIORITY);
                }
                else
                {
                    _WIN32_FIND_DATAA fd = new _WIN32_FIND_DATAA();
                    linkA.GetPath(target, target.Capacity, ref fd, (uint)EShellLinkGP.SLGP_UNCPRIORITY);
                }
                return target.ToString();
            }
            set
            {
                if (linkA == null)
                {
                    linkW.SetPath(value);
                }
                else
                {
                    linkA.SetPath(value);
                }
            }
        }

        /// <summary>
        /// Gets/sets the Working Directory for the Link
        /// </summary>
        public string WorkingDirectory
        {
            get
            {
                StringBuilder path = new StringBuilder(260, 260);
                if (linkA == null)
                {
                    linkW.GetWorkingDirectory(path, path.Capacity);
                }
                else
                {
                    linkA.GetWorkingDirectory(path, path.Capacity);
                }
                return path.ToString();
            }
            set
            {
                if (linkA == null)
                {
                    linkW.SetWorkingDirectory(value);
                }
                else
                {
                    linkA.SetWorkingDirectory(value);
                }
            }
        }

        /// <summary>
        /// Gets/sets the description of the link
        /// </summary>
        public string Description
        {
            get
            {
                StringBuilder description = new StringBuilder(1024, 1024);
                if (linkA == null)
                    linkW.GetDescription(description, description.Capacity);
                else
                    linkA.GetDescription(description, description.Capacity);

                return description.ToString();
            }
            set
            {
                if (linkA == null)
                    linkW.SetDescription(value);
                else
                    linkA.SetDescription(value);
            }
        }

        /// <summary>
        /// Gets/sets any command line arguments associated with the link
        /// </summary>
        public string Arguments
        {
            get
            {
                StringBuilder arguments = new StringBuilder(260, 260);
                if (linkA == null)
                    linkW.GetArguments(arguments, arguments.Capacity);
                else
                    linkA.GetArguments(arguments, arguments.Capacity);

                return arguments.ToString();
            }
            set
            {
                if (linkA == null)
                    linkW.SetArguments(value);
                else
                    linkA.SetArguments(value);
            }
        }

        /// <summary>
        /// Gets/sets the initial display mode when the shortcut is
        /// run
        /// </summary>
        public LinkDisplayMode DisplayMode
        {
            get
            {
                uint cmd = 0;
                if (linkA == null)
                    linkW.GetShowCmd(out cmd);
                else
                    linkA.GetShowCmd(out cmd);

                return (LinkDisplayMode)cmd;
            }
            set
            {
                if (linkA == null)
                    linkW.SetShowCmd((uint)value);
                else
                    linkA.SetShowCmd((uint)value);
            }
        }

        /// <summary>
        /// Gets/sets the HotKey to start the shortcut (if any)
        /// </summary>
        public Keys HotKey
        {
            get
            {
                short key = 0;
                if (linkA == null)
                    linkW.GetHotkey(out key);
                else
                    linkA.GetHotkey(out key);

                return (Keys)key;
            }
            set
            {
                if (linkA == null)
                    linkW.SetHotkey((short)value);
                else
                    linkA.SetHotkey((short)value);
            }
        }

        /// <summary>
        /// Saves the shortcut to ShortCutFile.
        /// </summary>
        public void Save()
        {
            Save(shortcutFile);
        }

        /// <summary>
        /// Saves the shortcut to the specified disk.
        /// </summary>
        /// <param name="linkFile">The shortcut file (.lnk)</param>
        public void Save(string linkFile)
        {
            if (linkA == null)
            {
                ((IPersistFile)linkW).Save(linkFile, true);
                shortcutFile = linkFile;
            }
            else
            {
                ((IPersistFile)linkA).Save(linkFile, true);
                shortcutFile = linkFile;
            }
        }

        /// <summary>
        /// Loads a shortcut from the specified file
        /// </summary>
        /// <param name="linkFile">The shortcut file (.lnk) to load</param>
        public void Open(string linkFile)
        {
            Open(linkFile, IntPtr.Zero, (EShellLinkResolveFlags.SLR_ANY_MATCH | EShellLinkResolveFlags.SLR_NO_UI), 1);
        }

        /// <summary>
        /// Loads a shortcut from the specified file, and allows flags controlling
        /// the UI behaviour if the shortcut's target isn't found to be set.
        /// </summary>
        /// <param name="linkFile">The shortcut file (.lnk) to load</param>
        /// <param name="hWnd">The window handle of the application's UI, if any</param>
        /// <param name="resolveFlags">Flags controlling resolution behaviour</param>
        public void Open(string linkFile, IntPtr hWnd, EShellLinkResolveFlags resolveFlags)
        {
            Open(linkFile, hWnd, resolveFlags, 1);
        }

        /// <summary>
        /// Loads a shortcut from the specified file, and allows flags controlling
        /// the UI behaviour if the shortcut's target isn't found to be set.  If
        /// no SLR_NO_UI is specified, you can also specify a timeout.
        /// </summary>
        /// <param name="linkFile">The shortcut file (.lnk) to load</param>
        /// <param name="hWnd">The window handle of the application's UI, if any</param>
        /// <param name="resolveFlags">Flags controlling resolution behaviour</param>
        /// <param name="timeOut">Timeout if SLR_NO_UI is specified, in ms.</param>
        public void Open(string linkFile, IntPtr hWnd, EShellLinkResolveFlags resolveFlags, ushort timeOut)
        {
            uint flags;

            if ((resolveFlags & EShellLinkResolveFlags.SLR_NO_UI) == EShellLinkResolveFlags.SLR_NO_UI)
            {
                flags = (uint)((int)resolveFlags | (timeOut << 16));
            }
            else
            {
                flags = (uint)resolveFlags;
            }

            if (linkA == null)
            {
                ((IPersistFile)linkW).Load(linkFile, 0); //STGM_DIRECT)
                linkW.Resolve(hWnd, flags);
                this.shortcutFile = linkFile;
            }
            else
            {
                ((IPersistFile)linkA).Load(linkFile, 0); //STGM_DIRECT)
                linkA.Resolve(hWnd, flags);
                this.shortcutFile = linkFile;
            }
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
