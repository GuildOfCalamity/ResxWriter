using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ResxWriter
{
    /// <summary>
    /// Filter for notifications reported by <see cref="RegistryMonitor"/>.
    /// </summary>
    [Flags]
    public enum RegChangeNotifyFilter
    {
        /// <summary>
        /// Notify the caller if a subkey is added or deleted.
        /// </summary>
        Key = 1,

        /// <summary>
        /// Notify the caller of changes to the attributes of the key, such as the security descriptor information.
        /// </summary>
        Attribute = 2,

        /// <summary>
        /// Notify the caller of changes to a value of the key. This can include adding or deleting a value, or changing an existing value.
        /// </summary>
        Value = 4,

        /// <summary>
        /// Notify the caller of changes to the security descriptor of the key.
        /// </summary>
        Security = 8,
    }

    /// <summary>
    /// Spins up a thread that monitors a registry key for changes by the system or user.
    /// </summary>
    public class RegistryMonitor : IDisposable
    {
        #region [Props]
        private bool _disposed = false;
        private IntPtr _registryHive;
        private string _registrySubName;
        private Thread _thread;
        private readonly object _threadLock = new object();
        private const int KEY_NOTIFY = 0x0010;
        private const int KEY_QUERY_VALUE = 0x0001;
        private const int KEY_WOW64_64KEY = 0x0100;
        private const int STANDARD_RIGHTS_ALL = 0x001F0000;      // Combines DELETE, READ_CONTROL, WRITE_DAC, WRITE_OWNER, and SYNCHRONIZE access.
        private const int STANDARD_RIGHTS_EXECUTE = 0x00020000;  // Currently defined to equal READ_CONTROL.
        private const int STANDARD_RIGHTS_READ = 0x00020000;     // Currently defined to equal READ_CONTROL.
        private const int STANDARD_RIGHTS_REQUIRED = 0x000F0000; // Combines DELETE, READ_CONTROL, WRITE_DAC, and WRITE_OWNER access.
        private const int STANDARD_RIGHTS_WRITE = 0x00020000;    // Currently defined to equal READ_CONTROL.
        /* https://learn.microsoft.com/en-us/windows/win32/secauthz/access-mask
           DELETE                       0x00010000L
           READ_CONTROL                 0x00020000L
           WRITE_DAC                    0x00040000L
           WRITE_OWNER                  0x00080000L
           SYNCHRONIZE                  0x00100000L
           STANDARD_RIGHTS_REQUIRED     0x000F0000L
           STANDARD_RIGHTS_READ         0x00020000L
           STANDARD_RIGHTS_WRITE        0x00020000L
           STANDARD_RIGHTS_EXECUTE      0x00020000L
           STANDARD_RIGHTS_ALL          0x001F0000L
           SPECIFIC_RIGHTS_ALL          0x0000FFFFL
        */
        private static readonly IntPtr HKEY_CLASSES_ROOT = new IntPtr(unchecked((int)0x80000000));
        private static readonly IntPtr HKEY_CURRENT_CONFIG = new IntPtr(unchecked((int)0x80000005));
        private static readonly IntPtr HKEY_CURRENT_USER = new IntPtr(unchecked((int)0x80000001));
        private static readonly IntPtr HKEY_DYN_DATA = new IntPtr(unchecked((int)0x80000006));
        private static readonly IntPtr HKEY_LOCAL_MACHINE = new IntPtr(unchecked((int)0x80000002));
        private static readonly IntPtr HKEY_PERFORMANCE_DATA = new IntPtr(unchecked((int)0x80000004));
        private static readonly IntPtr HKEY_USERS = new IntPtr(unchecked((int)0x80000003));
        private readonly ManualResetEvent _eventTerminate = new ManualResetEvent(false);
        private RegChangeNotifyFilter _regFilter = RegChangeNotifyFilter.Key | 
                                                   RegChangeNotifyFilter.Attribute |
                                                   RegChangeNotifyFilter.Value | 
                                                   RegChangeNotifyFilter.Security;
        #endregion

        #region [Public]
        /// <summary>
        /// Initializes a new instance of the <see cref="RegistryMonitor"/> class.
        /// </summary>
        /// <param name="registryKey">The registry key to monitor.</param>
        public RegistryMonitor(Microsoft.Win32.RegistryKey registryKey)
        {
            InitRegistryKey(registryKey.Name);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RegistryMonitor"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public RegistryMonitor(string name)
        {
            if (name == null || name.Length == 0)
                throw new ArgumentNullException("name");

            InitRegistryKey(name);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RegistryMonitor"/> class.
        /// </summary>
        /// <param name="registryHive">The registry hive.</param>
        /// <param name="subKey">The sub key.</param>
        public RegistryMonitor(Microsoft.Win32.RegistryHive registryHive, string subKey)
        {
            InitRegistryKey(registryHive, subKey);
        }

        /// <summary>
        /// Occurs when the access to the registry fails.
        /// </summary>
        public event ErrorEventHandler Error;

        /// <summary>
        /// Occurs when the specified registry key has changed.
        /// </summary>
        public event EventHandler RegChanged;

        /// <summary>
        /// <b>true</b> if this <see cref="RegistryMonitor"/> object is currently monitoring;
        /// otherwise, <b>false</b>.
        /// </summary>
        public bool IsMonitoring
        {
            get { return _thread != null; }
        }

        /// <summary>
        /// Gets or sets the <see cref="RegChangeNotifyFilter">RegChangeNotifyFilter</see>.
        /// </summary>
        public RegChangeNotifyFilter RegChangeNotifyFilter
        {
            get { return _regFilter; }
            set
            {
                lock (_threadLock)
                {
                    if (IsMonitoring)
                        throw new InvalidOperationException("Monitoring thread is already running.");

                    _regFilter = value;
                }
            }
        }

        /// <summary>
        /// Disposes this object.
        /// </summary>
        public void Dispose()
        {
            Stop();
            _disposed = true;
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Finalizer for safety (if the Dispose method isn't explicitly called)
        /// </summary>
        ~RegistryMonitor() => Dispose();

        /// <summary>
        /// Start monitoring.
        /// </summary>
        public void Start()
        {
            if (_disposed)
                throw new ObjectDisposedException(null, "This instance is already disposed.");

            lock (_threadLock)
            {
                if (!IsMonitoring)
                {
                    _eventTerminate.Reset();
                    _thread = new Thread(new ThreadStart(MonitorThread)) { IsBackground = true, Priority = ThreadPriority.Lowest };
                    _thread.Start();
                }
            }
        }

        /// <summary>
        /// Stops the monitoring thread.
        /// </summary>
        public void Stop()
        {
            if (_disposed)
                throw new ObjectDisposedException(null, "This instance is already disposed");

            lock (_threadLock)
            {
                Thread thread = _thread;
                if (thread != null)
                {
                    _eventTerminate.Set();
                    try
                    {
                        // Wait some time for the thread to wrap-up.
                        // If timeout occurs, an exception will be thrown.
                        thread.Join(3000);
                    }
                    catch (Exception) { }
                }
            }
        }
        #endregion

        #region [Private]
        void InitRegistryKey(Microsoft.Win32.RegistryHive hive, string name)
        {
            switch (hive)
            {
                case Microsoft.Win32.RegistryHive.ClassesRoot:
                    _registryHive = HKEY_CLASSES_ROOT;
                    break;

                case Microsoft.Win32.RegistryHive.CurrentConfig:
                    _registryHive = HKEY_CURRENT_CONFIG;
                    break;

                case Microsoft.Win32.RegistryHive.CurrentUser:
                    _registryHive = HKEY_CURRENT_USER;
                    break;

                case Microsoft.Win32.RegistryHive.DynData:
                    _registryHive = HKEY_DYN_DATA;
                    break;

                case Microsoft.Win32.RegistryHive.LocalMachine:
                    _registryHive = HKEY_LOCAL_MACHINE;
                    break;

                case Microsoft.Win32.RegistryHive.PerformanceData:
                    _registryHive = HKEY_PERFORMANCE_DATA;
                    break;

                case Microsoft.Win32.RegistryHive.Users:
                    _registryHive = HKEY_USERS;
                    break;

                default:
                    throw new InvalidEnumArgumentException("hive", (int)hive, typeof(Microsoft.Win32.RegistryHive));
            }
            _registrySubName = name;
        }

        void InitRegistryKey(string name)
        {
            string[] nameParts = name.Split('\\');

            switch (nameParts[0])
            {
                case "HKEY_CLASSES_ROOT":
                case "HKCR":
                    _registryHive = HKEY_CLASSES_ROOT;
                    break;

                case "HKEY_CURRENT_USER":
                case "HKCU":
                    _registryHive = HKEY_CURRENT_USER;
                    break;

                case "HKEY_LOCAL_MACHINE":
                case "HKLM":
                    _registryHive = HKEY_LOCAL_MACHINE;
                    break;

                case "HKEY_USERS":
                    _registryHive = HKEY_USERS;
                    break;

                case "HKEY_CURRENT_CONFIG":
                    _registryHive = HKEY_CURRENT_CONFIG;
                    break;

                default:
                    _registryHive = IntPtr.Zero;
                    throw new ArgumentException("The registry hive '" + nameParts[0] + "' is not supported", "value");
            }

            _registrySubName = string.Join("\\", nameParts, 1, nameParts.Length - 1);
            Debug.WriteLine($"[INFO] Watch path is now '{_registrySubName}'");
        }

        void MonitorThread()
        {
            try
            {
                ThreadLoop(); // blocking call
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
            _thread = null;
        }

        void ThreadLoop()
        {
            int result = RegOpenKeyEx(
                _registryHive, 
                _registrySubName, 
                0, 
                STANDARD_RIGHTS_READ | 
                KEY_QUERY_VALUE | 
                KEY_NOTIFY | 
                KEY_WOW64_64KEY,
                out IntPtr registryKey);

            if (result != 0)
                throw new Win32Exception(result);

            try
            {
                AutoResetEvent _eventNotify = new AutoResetEvent(false);
                WaitHandle[] waitHandles = new WaitHandle[] { _eventNotify, _eventTerminate };
                while (!_eventTerminate.WaitOne(0, true))
                {
                    result = RegNotifyChangeKeyValue(registryKey, true, _regFilter, _eventNotify.SafeWaitHandle.DangerousGetHandle(), true);
                    if (result != 0)
                        throw new Win32Exception(result);

                    if (WaitHandle.WaitAny(waitHandles) == 0)
                    {
                        OnRegChanged();
                    }
                }
            }
            finally
            {
                if (registryKey != IntPtr.Zero)
                {
                    try
                    {
                        RegCloseKey(registryKey);
                    }
                    catch { }
                }
            }
        }
        #endregion

        #region [Protected]
        /// <summary>
        /// Raises the <see cref="Error"/> event.
        /// </summary>
        /// <param name="e">The <see cref="Exception"/> which occured while watching the registry.</param>
        /// <remarks>
        /// <b>OnError</b> is called when an exception occurs while watching the registry.
        /// When overriding <see cref="OnError"/> in a derived class, be sure to call
        /// the base class's <see cref="OnError"/> method.
        /// </remarks>
        protected virtual void OnError(Exception e)
        {
            Error?.Invoke(this, new ErrorEventArgs(e));
        }

        /// <summary>
        /// Raises the <see cref="RegChanged"/> event.
        /// </summary>
        /// <remarks>
        /// <b>OnRegChanged</b> is called when the specified registry key has changed.
        /// When overriding <see cref="OnRegChanged"/> in a derived class, be sure to call
        /// the base class's <see cref="OnRegChanged"/> method.
        /// </remarks>
        protected virtual void OnRegChanged()
        {
            RegChanged?.Invoke(this, null);
        }
        #endregion

        #region [Imports]
        [DllImport("advapi32.dll", SetLastError = true)]
        static extern int RegCloseKey(IntPtr hKey);

        [DllImport("advapi32.dll", SetLastError = true)]
        static extern int RegNotifyChangeKeyValue(IntPtr hKey, bool bWatchSubtree, RegChangeNotifyFilter dwNotifyFilter, IntPtr hEvent, bool fAsynchronous);

        [DllImport("advapi32.dll", SetLastError = true)]
        static extern int RegOpenKeyEx(IntPtr hKey, string subKey, uint options, int samDesired, out IntPtr phkResult);
        #endregion
    }
}
