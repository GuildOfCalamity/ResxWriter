using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using System.Xml;

namespace ResxWriter
{
    /// <summary>
    /// Class of commonly used helper methods.
    /// </summary>
    /// <remarks>A select few of these were borrowed from AstroGrep v4.4.9.</remarks>
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
        /// Checks the <paramref name="ctrl"/> to determine if a specific property is available.
        /// </summary>
        /// <param name="ctrl"></param>
        /// <param name="propName">e.g. "Enabled"</param>
        /// <example>
        /// <code>
        ///    bool dpi = btnLoad.PropertyExists("DeviceDpi");
        /// </code>
        /// </example>
        /// <returns>true if property exists, false otherwise</returns>
        public static bool PropertyExists(this Control ctrl, string propName)
        {
            BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy;
            PropertyInfo prop = ctrl.GetType().GetProperty(propName, flags);
            if (prop == null)
                return false;

            return true;
        }

        /// <summary>
        /// Retruns the <paramref name="ctrl"/> property value, if available.
        /// </summary>
        /// <param name="ctrl"></param>
        /// <param name="propName">e.g. "Enabled"</param>
        /// <example>
        /// <code>
        ///    int dpi = btnLoad.PropertyValue<int>("DeviceDpi");
        /// </code>
        /// </example>
        /// <returns><typeparamref name="T"/> value if found, default <typeparamref name="T"/> otherwise</returns>
        public static T PropertyValue<T>(this Control ctrl, string propName)
        {
            BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy;
            PropertyInfo prop = ctrl.GetType().GetProperty(propName, flags);
            if (prop == null)
                return default(T);

            return (T)prop.GetValue(ctrl, null);
        }

        /// <summary>
        /// Returns a completely decoded text string from the passed html.
        /// </summary>
        public static string HtmlDecode(this string html)
        {
            if (string.IsNullOrWhiteSpace(html))
                return html;

            return System.Web.HttpUtility.HtmlDecode(html);
        }

        /// <summary>
        /// Returns a completely encoded html string from the passed text.
        /// </summary>
        public static string HtmlEncode(this string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;

            return System.Web.HttpUtility.HtmlEncode(text);
        }

        /// <summary>
        /// Uses the supplied <see cref="Dictionary{TKey, TValue}"/> as a look-up table to replace matched values.
        /// </summary>
        public static string Filter(this string input, Dictionary<char, char> replacements)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            for (int i = 0; i < input.Length; i++)
            {
                if (replacements.ContainsKey(input[i]))
                {
                    input = input.Replace(input[i], replacements[input[i]]);
                }
            }
            return input;
        }

        /// <summary>
        /// Uses the supplied <see cref="Dictionary{TKey, TValue}"/> as a look-up table to replace matched values.
        /// </summary>
        public static string Filter(this string input, Dictionary<string, string> replacements)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            foreach (var replacement in replacements.Keys)
            {
                input = input.Replace(replacement, replacements[replacement]);
            }
            return input;
        }


        /// <summary>
        /// Returns the <see cref="Encoding"/> based on the <paramref name="codePage"/> passed.
        /// Common code pages are "ISO-8859-1" or "Windows-1252".
        /// </summary>
        public static Encoding GetEncodingByCodePage(string codePage, Encoding fallback)
        {
            try 
            { 
                return Encoding.GetEncoding(codePage); 
            }
            catch (Exception) 
            {
                Debug.WriteLine($"Invalid code page: {codePage}");
                if (fallback == null)
                    return System.Text.Encoding.Default;
                else
                    return fallback;
            }
        }

        /// <summary>
        /// Returns the file's <see cref="Encoding"/> using the <see cref="StreamReader"/>.
        /// </summary>
        public static Encoding DetermineFileEncoding(this string path, Encoding fallback)
        {
            try
            {
                System.IO.FileStream fs = new System.IO.FileStream(path, System.IO.FileMode.Open);
                System.IO.StreamReader sr = new System.IO.StreamReader(fs);
                System.Text.Encoding coding = sr.CurrentEncoding;
                sr.Close(); sr.Dispose();
                fs.Close(); fs.Dispose();
                return coding;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"DetermineFileEncoding: {ex.Message}");
                if (fallback == null)
                    return System.Text.Encoding.Default;
                else
                    return fallback;
            }
        }

        /// <summary>
        /// Returns the <see cref="XmlDocument"/>'s <see cref="Encoding"/>.
        /// </summary>
        public static Encoding EncodingFromXMLDoc(this XmlDocument doc, Encoding fallback)
        {
            try
            {
                return Encoding.GetEncoding(((XmlDeclaration)doc.FirstChild).Encoding);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"EncodingFromXMLDoc: {ex.Message}");
                if (fallback == null)
                    return System.Text.Encoding.Default;
                else
                    return fallback;
            }
        }

        /// <summary>
        /// In this method, the first step is to check for the presence of a BOM (Byte Order Mark) in the byte
        /// array. A BOM is a sequence of bytes at the beginning of a file that indicates the encoding of the 
        /// data. If a BOM is present, the method returns the appropriate encoding.
        /// If a BOM is not present, the method takes a sample of the data and tries to determine the encoding 
        /// by counting the number of characters with values greater than 127 in the sample.The assumption is 
        /// that encodings such as UTF-8 and UTF-7 will have fewer such characters than encodings like UTF-32 
        /// and Unicode.
        /// This method is not foolproof, and there may be cases where it fails to correctly detect the encoding, 
        /// especially if the data contains a mixture of characters from multiple encodings. Nevertheless, it can 
        /// be a useful starting point for determining the encoding of a byte array.
        /// </summary>
        /// <param name="byteArray">the array to analyze</param>
        /// <returns><see cref="System.Text.Encoding"/></returns>
        public static Encoding IdentifyEncoding(this byte[] byteArray)
        {
            // Nothing to do.
            if (byteArray.Length == 0)
                return Encoding.Default;

            // Try to detect the encoding using the ByteOrderMark.
            if (byteArray.Length >= 4)
            {
                if (byteArray[0] == 0x2b && byteArray[1] == 0x2f && byteArray[2] == 0x76) return Encoding.UTF7;
                if (byteArray[0] == 0xef && byteArray[1] == 0xbb && byteArray[2] == 0xbf) return Encoding.UTF8;
                if (byteArray[0] == 0xff && byteArray[1] == 0xfe) return Encoding.Unicode; //UTF-16LE
                if (byteArray[0] == 0xfe && byteArray[1] == 0xff) return Encoding.BigEndianUnicode; //UTF-16BE
                if (byteArray[0] == 0 && byteArray[1] == 0 && byteArray[2] == 0xfe && byteArray[3] == 0xff) return Encoding.UTF32;
            }
            else if (byteArray.Length >= 2)
            {
                if (byteArray[0] == 0xFE && byteArray[1] == 0xFF)
                    return Encoding.BigEndianUnicode;
                else if (byteArray[0] == 0xFF && byteArray[1] == 0xFE)
                    return Encoding.Unicode;
                else if (byteArray.Length >= 3 && byteArray[0] == 0xEF && byteArray[1] == 0xBB && byteArray[2] == 0xBF)
                    return Encoding.UTF8;
            }

            // If the BOM is not present, try to detect the encoding using a sample of the data.
            Encoding[] encodingsToTry = { Encoding.UTF8, Encoding.UTF7, Encoding.UTF32, Encoding.Unicode, Encoding.BigEndianUnicode };
            // Encoding.ASCII should not be used since it does not preserve the 8th bit 
            // and will result in all chars being lower than 127/0x7F. 01111111 = 127
            int sampleSize = Math.Min(byteArray.Length, 1024);
            foreach (Encoding encoding in encodingsToTry)
            {
                string sample = encoding.GetString(byteArray, 0, sampleSize);

                int count = 0;
                foreach (char c in sample)
                {
                    if (c > 127) // 0x7F (DEL)
                        count++;
                }

                double ratio = (double)count / sampleSize;
                Debug.WriteLine($"{encoding.EncodingName} => {ratio:N3}");
                if (ratio <= 0.1)
                    return encoding;
            }

            // If the encoding could not be determined, return default encoding.
            return Encoding.Default;
        }

        /// <summary>
        /// Iterates type infos for the <see cref="System.Windows.Forms.Form"/>.
        /// </summary>
        public static IEnumerable<Type> GetHierarchyFromForm(this Type element)
        {
            if (element.GetTypeInfo().IsSubclassOf(typeof(System.Windows.Forms.Form)) != true)
                yield break;

            Type current = element;
            while (current != null && current != typeof(System.Windows.Forms.Form))
            {
                yield return current;
                current = current.GetTypeInfo().BaseType;
            }
        }

        /// <summary>
        /// Blends the provided two colors together.
        /// </summary>
        /// <param name="foreColor">Color to blend onto the background color.</param>
        /// <param name="backColor">Color to blend the other color onto.</param>
        /// <param name="amount">How much of <paramref name="foreColor"/> to keep, on top of <paramref name="backColor"/>.</param>
        /// <returns>The blended color.</returns>
        /// <remarks>The alpha channel is not altered.</remarks>
        public static Color ColorBlend(Color foreColor, Color backColor, double amount = 0.3)
        {
            byte r = (byte)(foreColor.R * amount + backColor.R * (1 - amount));
            byte g = (byte)(foreColor.G * amount + backColor.G * (1 - amount));
            byte b = (byte)(foreColor.B * amount + backColor.B * (1 - amount));
            return Color.FromArgb(r, g, b);
        }
        public static Color DarkenColor(Color baseColor, float percentage = 0.3F) => ControlPaint.Dark(baseColor, percentage);
        public static Color LightenColor(Color baseColor, float percentage = 0.3F) => ControlPaint.Light(baseColor, percentage);


        /// <summary>
        /// Applies a color scheme to the control and its children.
        /// </summary>
        /// <remarks>This method is recursive.</remarks>
        public static void EnumerateAllControls(this Control root, int red = 230, int green = 230, int blue = 230)
        {
            if (root is Form frm)
                frm.ForeColor = Color.FromArgb(red, green, blue);

            foreach (Control cntrl in root.Controls)
            {
                Debug.WriteLine("Control: {0}, Parent: {1}, HasChildren: {2}", cntrl.Name, root.Name, cntrl.HasChildren);
                if (cntrl is GroupBox gb) { gb.ForeColor = Color.FromArgb((int)(red / 1.5f), (int)(green / 1.5f), (int)(blue / 1.5f)); }
                else if (cntrl is TextBox tb) { tb.ForeColor = Color.FromArgb((int)(red / 1.5f), (int)(green / 1.5f), (int)(blue / 1.5f)); }
                else if (cntrl is RichTextBox rtb) { rtb.ForeColor = Color.FromArgb(red, green, blue); }
                else if (cntrl is RadioButton rb) { rb.ForeColor = Color.FromArgb(red, green, blue); }
                else if (cntrl is ComboBox cmb) { cmb.ForeColor = Color.FromArgb(red, green, blue); }
                else if (cntrl is CheckBox cb) { cb.ForeColor = Color.FromArgb(red, green, blue); }
                else if (cntrl is CheckedListBox clb) { clb.ForeColor = Color.FromArgb(red, green, blue); }
                else if (cntrl is Label lbl) { lbl.ForeColor = Color.FromArgb(red / 2, green / 2, blue / 2); }
                else if (cntrl is ListBox lb) { lb.ForeColor = Color.FromArgb((int)(red / 1.5f), (int)(green / 1.5f), (int)(blue / 1.5f)); }
                else if (cntrl is ListView lv) { lv.ForeColor = Color.FromArgb(red, green, blue); }
                else if (cntrl is TreeView tv) { tv.ForeColor = Color.FromArgb(red, green, blue); }
                else if (cntrl is Panel pnl) { pnl.ForeColor = Color.FromArgb(red, green, blue); }
                else if (cntrl is DateTimePicker dtp) { dtp.ForeColor = Color.FromArgb(red, green, blue); }
                else if (cntrl is ProgressBar pb) { pb.ForeColor = Color.FromArgb(red, green, blue); }
                else if (cntrl is NumericUpDown nud) { nud.ForeColor = Color.FromArgb(red, green, blue); }
                else if (cntrl is Button bc)
                {
                    bc.FlatAppearance.BorderColor = Color.FromArgb(red / 2, blue / 2, green / 2);
                    bc.ForeColor = Color.FromArgb(red, green, blue);
                }

                if (cntrl.Controls != null)
                    EnumerateAllControls(cntrl);
            }
        }

        /// <summary>
        /// Determines if <paramref name="c1"/> intersects with <paramref name="c2"/>.
        /// </summary>
        /// <returns>true if controls overlap, false otherwise</returns>
        public static bool CollidesWith(this Control c1, Control c2)
        {
            bool result = false;

            var p1 = c1.Parent.PointToScreen(c1.Location);
            var p2 = c2.Parent.PointToScreen(c2.Location);
            if (p1.X < p2.X + c2.Width && p2.X < p1.X + c1.Width && p1.Y < p2.Y + c2.Height)
                result = p2.Y < p1.Y + c1.Height;

            return result;
        }

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
            if (obj == null)
                return;

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
        /// Compares two <see cref="Image"/>s by their bytes to determine if they are the same.
        /// </summary>
        /// <returns>true if identical, false otherwise</returns>
        public static bool BytewiseCompare(this System.Drawing.Image img1, System.Drawing.Image img2)
        {
            var i1bytes = new byte[1];
            i1bytes = (byte[])(new ImageConverter()).ConvertTo(img1, i1bytes.GetType());

            var i2bytes = new byte[1];
            i2bytes = (byte[])(new ImageConverter()).ConvertTo(img2, i2bytes.GetType());

            if (i1bytes.Length != i2bytes.Length)
                return false; // difference found

            for (int i = 0; i < i1bytes.Length; i++)
            {
                if (i1bytes[i] != i2bytes[i])
                    return false; // difference found
            }
           
            return true; // no differences found
        }

        /// <summary>
        /// Bitmap bitmap = Assembly.GetExecutingAssembly().LoadBitmapFromResource("Resources.Button01.png");
        /// </summary>
        /// <returns><see cref="System.Drawing.Bitmap"/></returns>
        public static System.Drawing.Bitmap LoadBitmapFromResource(this System.Reflection.Assembly assembly, string imageResourcePath)
        {
            var stream = assembly.GetManifestResourceStream(imageResourcePath);
            return stream != null ? new System.Drawing.Bitmap(stream) : null;
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
        /// Splits the given string by a string. Includes a minimum length to match.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="delimiter"></param>
        /// <param name="minLength">Tokens must be at least this length or greater to be considered. Value is 1 by default.</param>
        /// <returns><see cref="String[]"/></returns>
        public static string[] SplitStringToArray(this string source, string delimiter, int minLength = 1)
        {
            string[] tokens = source.Split(new string[] { delimiter }, StringSplitOptions.RemoveEmptyEntries);
            return tokens.Where(token => token.Length >= minLength).ToArray();
        }

        /// <summary>
        /// Splits the given string by a string. Includes a minimum length to match.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="delimiter"></param>
        /// <param name="minLength">Tokens must be at least this length or greater to be considered. Value is 1 by default.</param>
        /// <returns><see cref="IEnumerable{T}"/></returns>
        public static IEnumerable<string> SplitStringToEnumerable(this string source, string delimiter, int minLength = 1)
        {
            string[] tokens = source.Split(new string[] { delimiter }, StringSplitOptions.RemoveEmptyEntries);
            return tokens.Where(token => token.Length >= minLength);
        }

        /// <summary>
        /// <see cref="Func{T, TResult}"/> helper.
        /// </summary>
        /// <example>
        /// <code>
        ///   Func<int> f = () =>
        ///   {
        ///       var n = new Random().Next(1, 11);
        ///       if (n < 9) { throw new Exception($"I don't like this number: {n}"); }
        ///       return n;
        ///   };
        ///   try
        ///   {
        ///       var result = f.Retry(3);
        ///       Debug.WriteLine($"Passed: {result}");
        ///   }
        ///   catch (Exception) { Debug.WriteLine($"Attempts exhausted!"); }
        /// </code>
        /// </example>
        public static T Retry<T>(this Func<T> operation, int attempts = 3)
        {
            while (true)
            {
                try
                {
                    attempts--;
                    return operation();
                }
                catch (Exception ex) when (attempts > 0)
                {
                    Debug.WriteLine($"Retry: {ex.Message}");
                    Thread.Sleep(2000);
                }
            }
        }

        /// <summary>
        /// IEnumerable file reader.
        /// </summary>
        public static IEnumerable<string> ReadFileLines(string path)
        {
            string line = string.Empty;

            if (!File.Exists(path))
                yield return line;
            else
            {
                using (TextReader reader = File.OpenText(path))
                {
                    while ((line = reader.ReadLine()) != null)
                    {
                        yield return line;
                    }
                }
            }
        }

        /// <summary>
        /// File reader with filtering.
        /// </summary>
        public static List<string> ReadAllLines(string filePath)
        {
            List<string> results = new List<string>();
            string[] lines = File.ReadAllLines(filePath);
            //string[] filtered = Array.Filter(lines, line => !string.IsNullOrWhiteSpace(line));
            Array.ForEach(lines, (l) =>
            {
                if (!string.IsNullOrWhiteSpace(l))
                    results.Add(l);
            });

            return results;
        }

        public static void CreateAllLines(this string[] lines, string filePath)
        {
            try
            {
                using (StreamWriter sw = File.CreateText(filePath))
                {
                    foreach (string line in lines)
                    {
                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            sw.WriteLine(line);
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"CreateAllLines: {ex.Message}");
            }        
        }

        public static void AppendAllLines(this string[] lines, string filePath)
        {
            try
            {
                using (StreamWriter sw = File.AppendText(filePath))
                {
                    foreach (string line in lines)
                    {
                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            sw.WriteLine(line);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"CreateAllLines: {ex.Message}");
            }
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

        /// <summary>
        /// Uses <see cref="XmlWriter"/> and <see cref="XmlWriterSettings"/> to create formatted output.
        /// </summary>
        /// <param name="xmlString">input XML</param>
        /// <returns>formatted XML</returns>
        public static string FormatXml(string xmlString)
        {
            try
            {
                var stringBuilder = new StringBuilder();
                var element = System.Xml.Linq.XElement.Parse(xmlString);
                var settings = new XmlWriterSettings();
                settings.NewLineOnAttributes = true;
                settings.OmitXmlDeclaration = true;
                settings.Indent = true;
                // XmlWriter offers a StringBuilder as an output target.
                using (var xmlWriter = XmlWriter.Create(stringBuilder, settings))
                {
                    element.Save(xmlWriter);
                }
                return stringBuilder.ToString();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"FormatXml: {ex.Message}");
                return string.Empty;
            }
        }

        /// <summary>
        /// Similar to <see cref="GetReadableTime(TimeSpan)"/>.
        /// </summary>
        /// <param name="timeSpan"><see cref="TimeSpan"/></param>
        /// <returns>formatted text</returns>
        public static string ToReadableString(this TimeSpan span)
        {
            var parts = new StringBuilder();
            if (span.Days > 0)
                parts.Append($"{span.Days} day{(span.Days == 1 ? string.Empty : "s")} ");
            if (span.Hours > 0)
                parts.Append($"{span.Hours} hour{(span.Hours == 1 ? string.Empty : "s")} ");
            if (span.Minutes > 0)
                parts.Append($"{span.Minutes} minute{(span.Minutes == 1 ? string.Empty : "s")} ");
            if (span.Seconds > 0)
                parts.Append($"{span.Seconds} second{(span.Seconds == 1 ? string.Empty : "s")} ");
            if (span.Milliseconds > 0)
                parts.Append($"{span.Milliseconds} millisecond{(span.Milliseconds == 1 ? string.Empty : "s")} ");

            if (parts.Length == 0) // result was less than 1 millisecond
                return $"{span.TotalMilliseconds:N4} milliseconds"; // similar to span.Ticks
            else
                return parts.ToString().Trim();
        }

        /// <summary>
        /// Similar to <see cref="GetReadableTime(DateTime, bool)"/>.
        /// </summary>
        /// <param name="timeSpan"><see cref="TimeSpan"/></param>
        /// <returns>formatted text</returns>
        public static string GetReadableTime(this TimeSpan timeSpan)
        {
            var ts = new TimeSpan(DateTime.Now.Ticks - timeSpan.Ticks);
            var totMinutes = ts.TotalSeconds / 60;
            var totHours = ts.TotalSeconds / 3_600;
            var totDays = ts.TotalSeconds / 86_400;
            var totWeeks = ts.TotalSeconds / 604_800;
            var totMonths = ts.TotalSeconds / 2_592_000;
            var totYears = ts.TotalSeconds / 31_536_000;

            var parts = new StringBuilder();
            if (totYears > 0.1)
                parts.Append($"{totYears:N1} years ");
            if (totMonths > 0.1)
                parts.Append($"{totMonths:N1} months ");
            if (totWeeks > 0.1)
                parts.Append($"{totWeeks:N1} weeks ");
            if (totDays > 0.1)
                parts.Append($"{totDays:N1} days ");
            if (totHours > 0.1)
                parts.Append($"{totHours:N1} hours ");
            if (totMinutes > 0.1)
                parts.Append($"{totMinutes:N1} minutes ");

            return parts.ToString().Trim();
        }

        /// <summary>
        /// Similar to <see cref="GetReadableTime(TimeSpan)"/>.
        /// </summary>
        /// <param name="timeSpan"><see cref="TimeSpan"/></param>
        /// <returns>formatted text</returns>
        public static string GetReadableTime(this DateTime dateTime, bool addMilliseconds = false)
        {
            var timeSpan = new TimeSpan(DateTime.Now.Ticks - dateTime.Ticks);
            //double totalSecs = timeSpan.TotalSeconds;

            var parts = new StringBuilder();
            if (timeSpan.Days > 0)
                parts.AppendFormat("{0} {1} ", timeSpan.Days, timeSpan.Days == 1 ? "day" : "days");
            if (timeSpan.Hours > 0)
                parts.AppendFormat("{0} {1} ", timeSpan.Hours, timeSpan.Hours == 1 ? "hour" : "hours");
            if (timeSpan.Minutes > 0)
                parts.AppendFormat("{0} {1} ", timeSpan.Minutes, timeSpan.Minutes == 1 ? "minute" : "minutes");
            if (timeSpan.Seconds > 0)
                parts.AppendFormat("{0} {1} ", timeSpan.Seconds, timeSpan.Seconds == 1 ? "second" : "seconds");
            if (addMilliseconds && timeSpan.Milliseconds > 0)
                parts.AppendFormat("{0} {1}", timeSpan.Milliseconds, timeSpan.Milliseconds == 1 ? "millisecond" : "milliseconds");

            return parts.ToString().TrimEnd();
        }

        public static TimeSpan Multiply(this TimeSpan timeSpan, double scalar) => new TimeSpan((long)(timeSpan.Ticks * scalar));

        public static bool Between(this DateTime dt, DateTime rangeBeg, DateTime rangeEnd) => dt.Ticks >= rangeBeg.Ticks && dt.Ticks <= rangeEnd.Ticks;

        public static int GetDecimalPlacesCount(this string valueString) => valueString.SkipWhile(c => c.ToString(System.Globalization.CultureInfo.CurrentCulture) != System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator).Skip(1).Count();

        public static string RemoveDiacritics(this string str)
        {
            if (str == null)
                return string.Empty;

            var sb = new StringBuilder();
            foreach (char c in str.Normalize(NormalizationForm.FormD))
            {
                if (System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) != System.Globalization.UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Generic iterator function that is useful to replace a foreach loop with at your discretion.  A provided action is performed on each element.
        /// </summary>
        public static void ForEach<T>(this IEnumerable<T> ie, Action<T> action)
        {
            foreach (var i in ie)
                action(i);
        }

        /// <summary>
        /// Generic iterator function that is useful to replace a foreach loop with at your discretion.  A provided action is performed on each element.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="action">Function that takes in the current value in the sequence. 
        public static IEnumerable<T> Each<T>(this IEnumerable<T> source, Action<T> action)
        {
            return source.Each((value, index) =>
            {
                action(value);
                return true;
            });
        }


        /// <summary>
        /// Generic iterator function that is useful to replace a foreach loop with at your discretion.  A provided action is performed on each element.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="action">Function that takes in the current value and its index in the sequence.  
        public static IEnumerable<T> Each<T>(this IEnumerable<T> source, Action<T, int> action)
        {
            return source.Each((value, index) =>
            {
                action(value, index);
                return true;
            });
        }

        /// <summary>
        /// Generic iterator function that is useful to replace a foreach loop with at your discretion.  A provided action is performed on each element.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="action">Function that takes in the current value in the sequence.  Returns a value indicating whether the iteration should continue.  So return false if you don't want to iterate anymore.</param>
        public static IEnumerable<T> Each<T>(this IEnumerable<T> source, Func<T, bool> action)
        {
            return source.Each((value, index) =>
            {
                return action(value);
            });
        }

        /// <summary>
        /// Generic iterator function that is useful to replace a foreach loop with at your discretion.  A provided action is performed on each element.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="action">Function that takes in the current value and its index in the sequence.  Returns a value indicating whether the iteration should continue.  So return false if you don't want to iterate anymore.</param>
        public static IEnumerable<T> Each<T>(this IEnumerable<T> source, Func<T, int, bool> action)
        {
            if (source == null)
                return source;

            int index = 0;
            foreach (var sourceItem in source)
            {
                if (!action(sourceItem, index))
                    break;
                index++;
            }
            return source;
        }

        /// <summary>
        /// Debugging helper method.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>type name and basetype name</returns>
        public static string NameOf(this object obj)
        {
            return $"{obj.GetType().Name} --> {obj.GetType().BaseType.Name}";
            // Similar: System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name
        }

        public static bool IsDisposable(this Type type)
        {
            return (!typeof(IDisposable).IsAssignableFrom(type)) ? false : true;
        }

        public static bool IsClonable(this Type type)
        {
            return (!typeof(ICloneable).IsAssignableFrom(type)) ? false : true;
        }

        public static bool IsComparable(this Type type)
        {
            return (!typeof(IComparable).IsAssignableFrom(type)) ? false : true;
        }

        public static bool IsConvertible(this Type type)
        {
            return (!typeof(IConvertible).IsAssignableFrom(type)) ? false : true;
        }

        public static bool IsFormattable(this Type type)
        {
            return (!typeof(IFormattable).IsAssignableFrom(type)) ? false : true;
        }

        public static IEnumerable<T> GetCustomAttributes<T>(this ICustomAttributeProvider attributeProvider, bool inherit) where T : Attribute
        {
            return attributeProvider.GetCustomAttributes(typeof(T), inherit).Cast<T>();
        }

        public static bool HasPublicInstanceProperty(this IReflect type, string name)
        {
            return type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance) != null;
        }

        public static IComparer<string> GetStringComparer(this System.Globalization.CultureInfo cultureInfo, System.Globalization.CompareOptions options = System.Globalization.CompareOptions.None)
        {
            if (cultureInfo != null)
            {
                var func = new Func<string, string, int>((a, b) => cultureInfo.CompareInfo.Compare(a, b, options));
                return func.ToComparer();
            }
            else return null;
        }

        public static IEqualityComparer<T> ToEqualityComparer<T>(this Func<T, T, bool> func)
        {
            return new FuncEqualityComparer<T>(func);
        }

        public static IComparer<T> ToComparer<T>(this Func<T, T, int> compareFunction)
        {
            return new FuncComparer<T>(compareFunction);
        }

        public static IComparer<T> ToComparer<T>(this Comparison<T> compareFunction)
        {
            return new ComparisonComparer<T>(compareFunction);
        }

        public static IComparer<string> ToComparer<T>(this System.Globalization.CompareInfo compareInfo)
        {
            return new FuncComparer<string>(compareInfo.Compare);
        }

        /// <summary>
        /// Returns the basic assemblies needed by the application.
        /// </summary>
        public static Dictionary<string, Version> GetReferencedAssemblies()
        {
            Dictionary<string, Version> values = new Dictionary<string, Version>();
            try
            {
                var assem = Assembly.GetExecutingAssembly();
                int idx = 0; // to prevent key collisions only
                values.Add($"{(++idx).AddOrdinal()}: {assem.GetName().Name}", assem.GetName().Version); // add self
                IOrderedEnumerable<AssemblyName> names = assem.GetReferencedAssemblies().OrderBy(o => o.Name);
                foreach (var sas in names)
                {
                    values.Add($"{(++idx).AddOrdinal()}: {sas.Name}", sas.Version);
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Write($"GetReferencedAssemblies: {ex.Message}", LogLevel.Error);
            }
            return values;
        }

        /// <summary>
        /// Returns an exhaustive list of all modules involved in the current process.
        /// </summary>
        public static List<string> GetProcessDependencies()
        {
            List<string> result = new List<string>();
            try
            {
                string self = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name + ".exe";
                System.Diagnostics.ProcessModuleCollection pmc = System.Diagnostics.Process.GetCurrentProcess().Modules;
                IOrderedEnumerable<System.Diagnostics.ProcessModule> pmQuery = pmc
                    .OfType<System.Diagnostics.ProcessModule>()
                    .Where(pt => pt.ModuleMemorySize > 0)
                    .OrderBy(o => o.ModuleName);
                foreach (var item in pmQuery)
                {
                    //if (!item.ModuleName.Contains($"{self}"))
                    result.Add($"Module name: {item.ModuleName}, {(string.IsNullOrEmpty(item.FileVersionInfo.FileVersion) ? "version unknown" : $"v{item.FileVersionInfo.FileVersion}")}");
                    try { item.Dispose(); }
                    catch { }
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Write($"ProcessModuleCollection: {ex.Message}", LogLevel.Error);
            }
            return result;
        }

        /// <summary>
        /// Adds an ordinal to a number.
        /// int number = 1;
        /// var ordinal = number.AddOrdinal(); // 1st
        /// </summary>
        /// <param name="number">The number to add the ordinal too.</param>
        /// <returns>A string with an number and ordinal</returns>
        public static string AddOrdinal(this int number)
        {
            if (number <= 0)
                return number.ToString();

            switch (number % 100)
            {
                case 11:
                case 12:
                case 13:
                    return number + "th";
            }

            switch (number % 10)
            {
                case 1:
                    return number + "st";
                case 2:
                    return number + "nd";
                case 3:
                    return number + "rd";
                default:
                    return number + "th";
            }
        }

        /// <summary>
        /// var ints = new[] {1,2,3,4,5,6,7,8,2,3,4,12,243,4,5,34,24,3,45,45,6,45,6,34,56,534};
        /// var scrambled = ints.Randomize();
        /// </summary>
        /// <returns><see cref="IEnumerable{T}"/></returns>
        public static IEnumerable<T> Randomize<T>(this IEnumerable<T> source)
        {
            return source.OrderBy(s => Guid.NewGuid());
        }

        /// <summary>
        /// Shuffles any type of array
        /// </summary>
        /// <returns>random mixed up array</returns>
        static public IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
        {
            if (source == null)
                throw new ArgumentNullException(source.NameOf());

            return ShuffleIterator(source);
        }
        static private IEnumerable<T> ShuffleIterator<T>(this IEnumerable<T> source)
        {
            T[] array = source.ToArray();
            Random rnd = new Random();
            for (int n = array.Length; n > 1;)
            {
                int k = rnd.Next(n--); // 0 <= k < n
                if (n != k)
                {   //Swap items
                    T tmp = array[k];
                    array[k] = array[n];
                    array[n] = tmp;
                }
            }
            foreach (var item in array)
                yield return item;
        }
        /// <summary>
        /// Shuffles any type of array
        /// </summary>
        /// <returns>random mixed up array</returns>
        public static T[] Shuffle<T>(this T[] array)
        {
            Random random = new Random();
            int n = array.Length;
            while (n > 1)
            {
                n--;
                int r = random.Next(n + 1);
                T t = array[r];
                array[r] = array[n];
                array[n] = t;
            }
            return array;
        }

        /// <summary>
        /// Tests whether an array contains the index, and returns the value if true or the defaultValue if false
        /// </summary>
        public static string GetIndex(this string[] array, int index, string defaultValue = "") => (index < array.Length) ? array[index] : defaultValue;

        /// <summary>
        /// Tests whether an array contains the index, and returns the value if true or the defaultValue if false
        /// </summary>
        public static int GetIndex(this int[] array, int index, int defaultValue = -1) => (index < array.Length) ? array[index] : defaultValue;

        /// <summary>
        /// Tests whether an array contains the index, and returns the value if true or the defaultValue if false
        /// </summary>
        public static T GetIndex<T>(this T[] array, int index, T defaultValue) => (index < array.Length) ? array[index] : defaultValue;

        /// <summary>
        /// CopyTo without the second parameter, for when you just want to copy array A to array B verbatim and size is not a concern.
        /// array.CopyTo(target); 
        /// </summary>
        public static void CopyTo<T>(this T[] source, T[] target)
        {
            source.CopyTo(target, 0);
        }

        /// <summary>
        /// Returns a List of T except what's in a second list, without doing a distinct
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static List<TSource> ExceptWithDuplicates<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second)
        {
            /* EXAMPLE...
            List<int> a = new List<int> {1,8,8,3};
            List<int> b = new List<int> {1,8,3};
            var x = a.ExceptWithDuplicates(b);    //returns list with a single element: 8
            */
            List<TSource> s1 = second.ToList();
            List<TSource> ret = new List<TSource>();

            first.ToList().ForEach(n =>
            {
                if (s1.Contains(n))
                    s1.Remove(n);
                else
                    ret.Add(n);

            });

            return ret;
        }

        /// <summary>
        /// var duplicates = list.GetDuplicates();
        /// </summary>
        /// <param name="source">list to operate on</param>
        /// <returns>list of duplicates</returns>
        public static IEnumerable<T> GetDuplicates<T>(this IEnumerable<T> source)
        {
            HashSet<T> itemsSeen = new HashSet<T>();
            HashSet<T> itemsYielded = new HashSet<T>();
            foreach (T item in source)
            {
                if (!itemsSeen.Add(item))
                {
                    if (itemsYielded.Add(item))
                        yield return item;
                }
            }
        }

        /// <summary>
        /// double number = 2.2365182936409;
        /// string display = number.DisplayDouble(2); // 2.24
        /// </summary>
        public static string DisplayDouble(this double value, int precision)
        {
            return value.ToString("N" + precision);
        }

        /// <summary>
        /// Extract a string from an other string between 2 characters.
        /// </summary>
        /// <example>
        /// "message {yes} {no}".Extract("{", "}");   => returns "yes"
        /// "message {yes} {no}".Extract("{", "}",2); => returns "no"
        /// "message {yes} {no}".Extract("", "{");    => returns "message"
        /// </example>
        public static string Extract(this string value, string begin_text, string end_text, int occurrence = 1)
        {
            if (string.IsNullOrEmpty(value) == false)
            {
                int start = -1;

                for (int i = 1; i <= occurrence; i++)
                    start = value.IndexOf(begin_text, start + 1);

                if (start < 0)
                    return value;

                start += begin_text.Length;

                if (string.IsNullOrEmpty(end_text))
                    return value.Substring(start);

                int end = value.IndexOf(end_text, start);
                if (end < 0)
                    return value.Substring(start);

                end -= start;

                return value.Substring(start, end);
            }
            else
                return value;
        }

        /// <summary>
        /// You could also use the Thread.IsAlive bool.
        /// </summary>
        /// <param name="ts"><see cref="System.Threading.ThreadState"/></param>
        /// <returns>true if running, false otherwise</returns>
        public static bool IsRunning(this System.Threading.ThreadState ts)
        {
            if (ts != System.Threading.ThreadState.Stopped && ts != System.Threading.ThreadState.Aborted && ts != System.Threading.ThreadState.Suspended)
                return true;
            else
                return false;
        }

        public static Stream ToStream(this string str)
        {
            byte[] byteArray = Encoding.UTF8.GetBytes(str);
            return new MemoryStream(byteArray);
        }

        public static string ToString(this Stream stream)
        {
            StreamReader reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

        /// <summary>
        /// Serialize object to stream using <see cref="System.Xml.Serialization.XmlSerializer"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="stream"></param>
        public static void ToXml<T>(this T value, Stream stream) where T : new()
        {
            var _serializer = GetValue(typeof(T));
            _serializer.Serialize(stream, value);
        }

        /// <summary>
        /// Deserialize object from stream using <see cref="System.Xml.Serialization.XmlSerializer"/>.
        /// </summary>
        /// <typeparam name="T">Type of deserialized object</typeparam>
        /// <param name="source">Xml source</param>
        /// <returns>deserialized object</returns>
        public static T FromXml<T>(this Stream source) where T : new()
        {
            var _serializer = GetValue(typeof(T));
            return (T)_serializer.Deserialize(source);
        }

        private static readonly Dictionary<RuntimeTypeHandle, System.Xml.Serialization.XmlSerializer> ms_serializers = new Dictionary<RuntimeTypeHandle, System.Xml.Serialization.XmlSerializer>();
        private static System.Xml.Serialization.XmlSerializer GetValue(Type type)
        {
            System.Xml.Serialization.XmlSerializer _serializer;
            if (!ms_serializers.TryGetValue(type.TypeHandle, out _serializer))
            {
                lock (ms_serializers)
                {
                    if (!ms_serializers.TryGetValue(type.TypeHandle, out _serializer))
                    {
                        _serializer = new System.Xml.Serialization.XmlSerializer(type);
                        ms_serializers.Add(type.TypeHandle, _serializer);
                    }
                }
            }
            return _serializer;
        }

        /// <summary>
        /// Retrieve Querystring,Params or Namevalue Collection with default values.
        /// EXAMPLE: var count = Request.QueryString.GetValue("count", 0);
        /// </summary>
        public static T GetValue<T>(this System.Collections.Specialized.NameValueCollection collection, string key, T defaultValue)
        {
            if (collection != null && collection.Count > 0)
            {
                if (!string.IsNullOrEmpty(key) && collection[key] != null)
                {
                    var val = collection[key];

                    return (T)Convert.ChangeType(val, typeof(T));
                }
            }

            return (T)defaultValue;
        }

        public static T GetValue<T>(this Dictionary<string, string> dict, string key)
        {
            try
            {
                return (T)Convert.ChangeType(dict[key], typeof(T));
            }
            catch (KeyNotFoundException)
            {
                return default(T);
            }
        }

        /// <summary>
        /// <para>Creates a log-string from the Exception.</para>
        /// <para>The result includes the stacktrace, innerexception et cetera, separated by <see cref="Environment.NewLine"/>.</para>
        /// </summary>
        /// <param name="ex">The exception to create the string from.</param>
        /// <param name="additionalMessage">Additional message to place at the top of the string, maybe be empty or null.</param>
        /// <returns>formatted string</returns>
        public static string ToLogString(this Exception ex, string additionalMessage = "")
        {
            System.Text.StringBuilder msg = new System.Text.StringBuilder();

            if (!string.IsNullOrEmpty(additionalMessage))
            {
                msg.Append($"---[{additionalMessage}]---");
                msg.Append(Environment.NewLine);
            }
            else
            {
                msg.Append($"---[{DateTime.Now.ToString("hh:mm:ss.fff tt")}]---");
                msg.Append(Environment.NewLine);
            }

            if (ex != null)
            {
                try
                {
                    Exception orgEx = ex;
                    msg.Append("[Exception]: ");
                    while (orgEx != null)
                    {
                        msg.Append(orgEx.Message);
                        msg.Append(Environment.NewLine);
                        orgEx = orgEx.InnerException;
                    }

                    if (ex.Source != null)
                    {
                        msg.Append("[Source]: ");
                        msg.Append(ex.Source);
                        msg.Append(Environment.NewLine);
                    }

                    if (ex.Data != null)
                    {
                        foreach (object i in ex.Data)
                        {
                            msg.Append("[Data]: ");
                            msg.Append(i.ToString());
                            msg.Append(Environment.NewLine);
                        }
                    }

                    if (ex.StackTrace != null)
                    {
                        msg.Append("[StackTrace]: ");
                        msg.Append(ex.StackTrace.ToString());
                        msg.Append(Environment.NewLine);
                    }

                    if (ex.TargetSite != null)
                    {
                        msg.Append("[TargetSite]: ");
                        msg.Append(ex.TargetSite.ToString());
                        msg.Append(Environment.NewLine);
                    }

                    Exception baseException = ex.GetBaseException();
                    if (baseException != null)
                    {
                        msg.Append("[BaseException]: ");
                        msg.Append(ex.GetBaseException());
                    }
                }
                catch (Exception iex) { Console.WriteLine($"ToLogString: {iex.Message}"); }
            }
            return msg.ToString();
        }

        public static char CharAt(this string s, int index)
        {
            if (index < s.Length)
                return s[index];

            return '\0';
        }

        /// <summary>
        /// Convert number to bytes size notation
        /// </summary>
        /// <param name="size">bytes</param>
        /// <returns>formatted string</returns>
        public static string ToFileSize(this long size)
        {
            if (size < 1024) { return (size).ToString("F0") + " Bytes"; }
            if (size < Math.Pow(1024, 2)) { return (size / 1024).ToString("F0") + " KB"; }
            if (size < Math.Pow(1024, 3)) { return (size / Math.Pow(1024, 2)).ToString("F2") + " MB"; }
            if (size < Math.Pow(1024, 4)) { return (size / Math.Pow(1024, 3)).ToString("F3") + " GB"; }
            if (size < Math.Pow(1024, 5)) { return (size / Math.Pow(1024, 4)).ToString("F3") + " TB"; }
            if (size < Math.Pow(1024, 6)) { return (size / Math.Pow(1024, 5)).ToString("F3") + " PB"; }
            return (size / Math.Pow(1024, 6)).ToString("F3") + " EB";
        }

        public static bool IsInvalidFileNameChar(this Char c) => c < 64U ? (1UL << c & 0xD4008404FFFFFFFFUL) != 0 : c == '\\' || c == '|';
        public static bool FilePathHasInvalidChars(this string path) => (!string.IsNullOrEmpty(path) && path.IndexOfAny(System.IO.Path.GetInvalidPathChars()) >= 0);
        public static string RemoveInvalidCharacters(this string path) => Path.GetInvalidFileNameChars().Aggregate(path, (current, c) => current.Replace(c.ToString(), string.Empty));
        public static string SanitizeFilePath(this string path)
        {
            if (!string.IsNullOrEmpty(path))
            {
                while (path.IndexOfAny(System.IO.Path.GetInvalidPathChars()) >= 0)
                {
                    int idx = path.IndexOfAny(System.IO.Path.GetInvalidPathChars());
                    path = path.Remove(idx, 1).Insert(idx, string.Empty);
                }
                return path;
            }
            else
            {
                return path;
            }
        }

        /// <summary>
        /// Open with default 'open' program
        /// new FileInfo("C:\image.jpg").Open();
        /// </summary>
        /// <param name="value"></param>
        public static Process OpenFile(this FileInfo value)
        {
            if (!value.Exists)
                throw new FileNotFoundException("File doesn't exist");
            Process p = new Process();
            p.StartInfo.FileName = value.FullName;
            p.StartInfo.Verb = "Open";
            p.Start();
            return p;
        }

        #region [Task Helpers]
        /// <summary>
        /// Task.Factory.StartNew (() => { throw null; }).IgnoreExceptions();
        /// </summary>
        public static void IgnoreExceptions(this Task task, bool logEx = false)
        {
            task.ContinueWith(t =>
            {
                AggregateException ignore = t.Exception;

                ignore?.Flatten().Handle(ex =>
                {
                    if (logEx)
                        Debug.WriteLine("Exception type: {0}\r\nException Message: {1}", ex.GetType(), ex.Message);
                    return true; // don't re-throw
                });

            }, TaskContinuationOptions.OnlyOnFaulted);
        }

        /// <summary>
        /// Chainable task helper.
        /// var result = await SomeLongAsyncFunction().WithTimeout(TimeSpan.FromSeconds(2));
        /// </summary>
        /// <typeparam name="TResult">the type of task result</typeparam>
        /// <returns><see cref="Task"/>TResult</returns>
        public async static Task<TResult> WithTimeout<TResult>(this Task<TResult> task, TimeSpan timeout)
        {
            Task winner = await (Task.WhenAny(task, Task.Delay(timeout)));

            if (winner != task)
                throw new TimeoutException();

            return await task;   // Unwrap result/re-throw
        }

        /// <summary>
        /// Task extension to add a timeout.
        /// </summary>
        /// <returns>The task with timeout.</returns>
        /// <param name="task">Task.</param>
        /// <param name="timeoutInMilliseconds">Timeout duration in Milliseconds.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public async static Task<T> WithTimeout<T>(this Task<T> task, int timeoutInMilliseconds)
        {
            var retTask = await Task.WhenAny(task, Task.Delay(timeoutInMilliseconds))
                .ConfigureAwait(false);

            #pragma warning disable CS8603 // Possible null reference return.
            return retTask is Task<T> ? task.Result : default;
            #pragma warning restore CS8603 // Possible null reference return.
        }

        /// <summary>
        /// Chainable task helper.
        /// var result = await SomeLongAsyncFunction().WithCancellation(cts.Token);
        /// </summary>
        /// <typeparam name="TResult">the type of task result</typeparam>
        /// <returns><see cref="Task"/>TResult</returns>
        public static Task<TResult> WithCancellation<TResult>(this Task<TResult> task, CancellationToken cancelToken)
        {
            TaskCompletionSource<TResult> tcs = new TaskCompletionSource<TResult>();
            CancellationTokenRegistration reg = cancelToken.Register(() => tcs.TrySetCanceled());
            task.ContinueWith(ant =>
            {
                reg.Dispose(); // NOTE: it's important to dispose of CancellationTokenRegistrations or they will hang around in memory until the application closes
                if (ant.IsCanceled)
                    tcs.TrySetCanceled();
                else if (ant.IsFaulted)
                    tcs.TrySetException(ant.Exception.InnerException);
                else
                    tcs.TrySetResult(ant.Result);
            });
            return tcs.Task;  // Return the TaskCompletionSource result
        }

        public static Task<T> WithAllExceptions<T>(this Task<T> task)
        {
            TaskCompletionSource<T> tcs = new TaskCompletionSource<T>();

            task.ContinueWith(ignored =>
            {
                switch (task.Status)
                {
                    case TaskStatus.Canceled:
                        Debug.WriteLine($"[TaskStatus.Canceled]");
                        tcs.SetCanceled();
                        break;
                    case TaskStatus.RanToCompletion:
                        tcs.SetResult(task.Result);
                        //Debug.WriteLine($"[TaskStatus.RanToCompletion({task.Result})]");
                        break;
                    case TaskStatus.Faulted:
                        // SetException will automatically wrap the original AggregateException
                        // in another one. The new wrapper will be removed in TaskAwaiter, leaving
                        // the original intact.
                        Debug.WriteLine($"[TaskStatus.Faulted]: {task.Exception.Message}");
                        tcs.SetException(task.Exception);
                        break;
                    default:
                        Debug.WriteLine($"[TaskStatus: Continuation called illegally.]");
                        tcs.SetException(new InvalidOperationException("Continuation called illegally."));
                        break;
                }
            });

            return tcs.Task;
        }

        #pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
        /// <summary>
        /// Attempts to await on the task and catches exception
        /// </summary>
        /// <param name="task">Task to execute</param>
        /// <param name="onException">What to do when method has an exception</param>
        /// <param name="continueOnCapturedContext">If the context should be captured.</param>
        public static async void SafeFireAndForget(this Task task, Action<Exception> onException = null, bool continueOnCapturedContext = false)
        #pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
        {
            try
            {
                await task.ConfigureAwait(continueOnCapturedContext);
            }
            catch (Exception ex) when (onException != null)
            {
                onException.Invoke(ex);
            }
            catch (Exception ex) when (onException == null)
            {
                Console.WriteLine($"SafeFireAndForget: {ex.Message}");
            }
        }
        #endregion

        #region [Shortcut Stuff]
        /// <summary>
        /// Creates a shortcut (lnk file) used for the application.
        /// </summary>
        /// <param name="location">directory where the shortcut should be created</param>
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
        /// Add the UAC shield to the given <see cref="Button"/>.
        /// </summary>
        /// <param name="b">the <see cref="Button"/> to add shield to</param>
        public static void AddShieldToButton(Button b)
        {
            if (IsWindowsVistaOrLater)// System.Environment.OSVersion.Version.Major >= 6)
            {
                b.FlatStyle = FlatStyle.System;
                SendMessage(b.Handle, BCM_SETSHIELD, 0, 0xFFFFFFFF);
            }
        }

        /// <summary>
        /// Removes the UAC shield from the given <see cref="Button"/>.
        /// </summary>
        /// <param name="b">the <see cref="Button"/> to remove shield from</param>
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

            // Only do this for Vista+ since XP has an older "runas" dialog. If runas set to
            // false we'll assume that the application has a manifest and so we won't need this.
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

    #region [File Deletion]
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

    #region [ShellLink Object]
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

    #region [Support Classes]
    public class FuncComparer<T> : IComparer<T>
    {
        public FuncComparer(Func<T, T, int> func)
        {
            if (func != null)
                m_func = func;
        }

        public int Compare(T x, T y)
        {
            return m_func(x, y);
        }

        private readonly Func<T, T, int> m_func;
    }

    public class ComparisonComparer<T> : IComparer<T>
    {
        public ComparisonComparer(Comparison<T> func)
        {
            if (func != null)
                m_func = func;
        }

        public int Compare(T x, T y)
        {
            return m_func(x, y);
        }

        private readonly Comparison<T> m_func;
    }

    public class FuncEqualityComparer<T> : IEqualityComparer<T>
    {
        public FuncEqualityComparer(Func<T, T, bool> func)
        {
            if (func != null)
                m_func = func;
        }
        public bool Equals(T x, T y)
        {
            return m_func(x, y);
        }

        public int GetHashCode(T obj)
        {
            return 0; // This is on purpose. Should only use function, not short-cut by hashcode compare.
        }

        private readonly Func<T, T, bool> m_func;
    }
    #endregion
}
