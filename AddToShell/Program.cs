using System;
using System.Diagnostics;

namespace AddToShell
{
    /// <summary>
    /// To test add the following command-line arguments to the project properties:
    /// "True" "D:\repos\ResxWriter\bin\Debug\ResxWriter.exe" "Open using ResxWriter..."
    /// </summary>
    internal class Program
    {
        public static string AppTitle = "ResxWriter";

        /// <summary>
        /// args: "True" "C:\Apps\ResxWriter.exe" "Open using {0}..."
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
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
            else
            {
                Console.WriteLine($"[INFO] Argument count was not correct.");
                _ = Console.ReadKey(true).Key;
            }
        }

        /// <summary>
        /// Set registry entry to make application a right-click option on a folder.
        /// Two key structures will be created:
        ///   - HKEY_CLASSES_ROOT\Directory\shell\ResxWriter
        ///   - HKEY_CLASSES_ROOT\Directory\background\shell\ResxWriter
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
                Microsoft.Win32.RegistryKey _appKey;
                Microsoft.Win32.RegistryKey _appBGKey;
                Microsoft.Win32.RegistryKey _appDriveKey;

                // Make sure the basic structure exists.
                if (_key != null && _bgKey != null && _driveKey != null)
                {
                    if (setOption)
                    {
                        // create keys
                        _appKey = _key.CreateSubKey(AppTitle);
                        _appBGKey = _bgKey.CreateSubKey(AppTitle);
                        _appDriveKey = _driveKey.CreateSubKey(AppTitle);

                        if (_appKey != null && _appBGKey != null && _appDriveKey != null)
                        {
                            _appKey.SetValue("", String.Format(explorerText, $"&{AppTitle}"));
                            _appBGKey.SetValue("", String.Format(explorerText, $"&{AppTitle}"));
                            _appDriveKey.SetValue("", String.Format(explorerText, $"&{AppTitle}"));

                            // shows icon in Windows 7+
                            _appKey.SetValue("Icon", string.Format("\"{0}\",0", path));
                            _appBGKey.SetValue("Icon", string.Format("\"{0}\",0", path));
                            _appDriveKey.SetValue("Icon", string.Format("\"{0}\",0", path));

                            Microsoft.Win32.RegistryKey _commandKey = _appKey.CreateSubKey("command");
                            Microsoft.Win32.RegistryKey _commandBGKey = _appBGKey.CreateSubKey("command");
                            Microsoft.Win32.RegistryKey _commandDriveKey = _appDriveKey.CreateSubKey("command");
                            if (_commandKey != null && _commandBGKey != null && _commandDriveKey != null)
                            {
                                string keyValue = string.Format("\"{0}\" \"%L\"", path);
                                _commandKey.SetValue("", keyValue);
                                _commandDriveKey.SetValue("", keyValue);
                                _commandBGKey.SetValue("", string.Format("\"{0}\" \"%V\"", path)); // background needs %V
                            }
                        }
                    }
                    else
                    {
                        #region [Remove Keys]
                        try
                        {
                            _key.DeleteSubKeyTree(AppTitle);
                            _bgKey.DeleteSubKeyTree(AppTitle);
                            _driveKey.DeleteSubKeyTree(AppTitle);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[ERROR] DeleteSubKeyTree: {ex.Message}");
                            _ = Console.ReadKey(true).Key;
                        }
                        #endregion
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] SetOpenWithOption: {ex.Message}");
                _ = Console.ReadKey(true).Key;
            }
        }
    }
}
