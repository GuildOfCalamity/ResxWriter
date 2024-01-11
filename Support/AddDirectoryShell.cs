using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace ResxWriter
{
    public static class AddDirectoryShell
    {
        public static string AppTitle = "ResxWriter";

        /// <summary>
        /// args: "True" "C:\Apps\ResxWriter.exe" "Open using {0}..."
        /// </summary>
        /// <param name="args"></param>
        public static void Add(string[] args)
        {
            if (args.Length > 0 && args.Length <= 3)
            {
                bool setOption = false;
                string path = string.Empty;
                string explorerText = "Open using {0}...";

                // Configure args
                bool.TryParse(args[0], out setOption);
                path = args[1].Replace("\"", "");
                explorerText = args[2].Replace("\"", "");

                SetOpenWithOption(setOption, path, explorerText);
            }
        }

        /// <summary>
        /// Set registry entry to make application a right-click option on a folder.
        /// </summary>
        /// <param name="setOption">True = Set registry value, False = remove registry value</param>
        /// <param name="path">Full path to AppTitle.exe</param>
        /// <param name="explorerText">Text to be displayed in Explorer context shell menu</param>
        /// <remarks>This must be run with elevated privileges!</remarks>
        static void SetOpenWithOption(bool setOption, string path, string explorerText)
        {
            try
            {
                Microsoft.Win32.RegistryKey _key = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(@"Directory\shell", true);
                Microsoft.Win32.RegistryKey _bgKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(@"Directory\Background\shell", true);
                Microsoft.Win32.RegistryKey _driveKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(@"Drive\shell", true);
                Microsoft.Win32.RegistryKey _astroGrepKey;
                Microsoft.Win32.RegistryKey _astroGrepBGKey;
                Microsoft.Win32.RegistryKey _astroGrepDriveKey;

                if (_key != null && _bgKey != null && _driveKey != null)
                {
                    if (setOption)
                    {
                        // create keys
                        _astroGrepKey = _key.CreateSubKey(AppTitle);
                        _astroGrepBGKey = _bgKey.CreateSubKey(AppTitle);
                        _astroGrepDriveKey = _driveKey.CreateSubKey(AppTitle);

                        if (_astroGrepKey != null && _astroGrepBGKey != null && _astroGrepDriveKey != null)
                        {
                            _astroGrepKey.SetValue("", String.Format(explorerText, $"&{AppTitle}"));
                            _astroGrepBGKey.SetValue("", String.Format(explorerText, $"&{AppTitle}"));
                            _astroGrepDriveKey.SetValue("", String.Format(explorerText, $"&{AppTitle}"));

                            // shows icon in Windows 7+
                            _astroGrepKey.SetValue("Icon", string.Format("\"{0}\",0", path));
                            _astroGrepBGKey.SetValue("Icon", string.Format("\"{0}\",0", path));
                            _astroGrepDriveKey.SetValue("Icon", string.Format("\"{0}\",0", path));

                            Microsoft.Win32.RegistryKey _commandKey = _astroGrepKey.CreateSubKey("command");
                            Microsoft.Win32.RegistryKey _commandBGKey = _astroGrepBGKey.CreateSubKey("command");
                            Microsoft.Win32.RegistryKey _commandDriveKey = _astroGrepDriveKey.CreateSubKey("command");
                            if (_commandKey != null && _commandBGKey != null && _commandDriveKey != null)
                            {
                                string keyValue = string.Format("\"{0}\" \"%L\"", path);
                                _commandKey.SetValue("", keyValue);
                                _commandDriveKey.SetValue("", keyValue);
                                // background needs %V
                                _commandBGKey.SetValue("", string.Format("\"{0}\" \"%V\"", path));
                            }
                        }
                    }
                    else
                    {
                        // remove keys
                        try
                        {
                            _key.DeleteSubKeyTree(AppTitle);
                            _bgKey.DeleteSubKeyTree(AppTitle);
                            _driveKey.DeleteSubKeyTree(AppTitle);
                        }
                        catch { }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] SetOpenWithOption: {ex.Message}");
            }
        }
    }
}
