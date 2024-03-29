﻿using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Xml;

namespace ResxWriter
{
    public sealed class SettingsManager
    {
        const string VERSION = "1.0";
        const string EXTENSION = ".config.xml";
        private SettingsManager() { }

        #region [Backing Members]
        static SettingsManager _Settings = null;
        bool darkTheme = true;
        bool makeShortcut = true;
        bool runAnimation = false;
        bool devMode = false;
        bool firstRun = true;
        string lastPath = "";
        string lastDelimiter = "";
        string language = "en-US";
        string windowFontName = "Calibri";
        int windowWidth = -1;
        int windowHeight = -1;
        int windowTop = -1;
        int windowLeft = -1;
        int windowState = -1;
        int windowsDPI = 100;
        string codePage = "windows-1252";
        #endregion

        #region [Public Properties]
        /// <summary>
        /// Static reference to this class.
        /// </summary>
        /// <remarks>
        /// The first time this property is used the existing settings will be loaded.
        /// </remarks>
        public static SettingsManager AppSettings
        {
            get
            {
                if (_Settings == null)
                {
                    _Settings = new SettingsManager();
                    Load(_Settings, Location, VERSION);
                }
                return _Settings;
            }
        }

        public static string Version
        {
            get => VERSION;
        }

        public static string Location
        {
            get => Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}{EXTENSION}");

            //if (IsPortable)
            //   return Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            //else
            //   return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}");
        }

        public static string WindowFontName
        {
            get => AppSettings.windowFontName;
            set => AppSettings.windowFontName = value;
        }

        public static int WindowWidth
        {
            get => AppSettings.windowWidth;
            set => AppSettings.windowWidth = value;
        }

        public static int WindowHeight
        {
            get => AppSettings.windowHeight;
            set => AppSettings.windowHeight = value;
        }

        public static int WindowTop
        {
            get => AppSettings.windowTop;
            set => AppSettings.windowTop = value;
        }

        public static int WindowLeft
        {
            get => AppSettings.windowLeft;
            set => AppSettings.windowLeft = value;
        }

        public static int WindowState
        {
            get => AppSettings.windowState;
            set => AppSettings.windowState = value;
        }

        public static int WindowsDPI
        {
            get => AppSettings.windowsDPI;
            set => AppSettings.windowsDPI = value;
        }

        public static string CodePage
        {
            get => AppSettings.codePage;
            set => AppSettings.codePage = value;
        }

        public static bool DarkTheme
        {
            get => AppSettings.darkTheme;
            set => AppSettings.darkTheme = value;
        }

        public static bool MakeShortcut
        {
            get => AppSettings.makeShortcut;
            set => AppSettings.makeShortcut = value;
        }

        public static bool RunAnimation
        {
            get => AppSettings.runAnimation;
            set => AppSettings.runAnimation = value;
        }

        public static bool FirstRun
        {
            get => AppSettings.firstRun;
            set => AppSettings.firstRun = value;
        }

        public static bool DevMode
        {
            get => AppSettings.devMode;
            set => AppSettings.devMode = value;
        }

        public static string LastPath
        {
            get => AppSettings.lastPath;
            set => AppSettings.lastPath = value;
        }

        public static string LastDelimiter
        {
            get => AppSettings.lastDelimiter;
            set => AppSettings.lastDelimiter = value;
        }

        public static string Language
        {
            get => AppSettings.language;
            set => AppSettings.language = value;
        }
        #endregion

        #region [I/O Methods]
        /// <summary>
        /// Loads the specified file into the given class with the given version.
        /// </summary>
        /// <param name="classRecord">Class</param>
        /// <param name="path">File path</param>
        /// <param name="version">Version of class</param>
        /// <returns>true if class contains values from file, false otherwise</returns>
        public static bool Load(object classRecord, string path, string version)
        {
            try
            {
                Type recordType = classRecord.GetType();
                XmlDocument xmlDoc = new XmlDocument();
                XmlNode rootNode = null;

                if (!File.Exists(path))
                    return false;

                xmlDoc.Load(path);
                // The root must match the name of the class
                rootNode = xmlDoc.SelectSingleNode(recordType.Name);

                if (rootNode != null)
                {
                    // check for correct version
                    if (rootNode.Attributes.Count > 0 && rootNode.Attributes["version"] != null && rootNode.Attributes["version"].Value.Equals(version))
                    {
                        XmlNodeList propertyNodes = rootNode.SelectNodes("property");

                        Logger.Instance.Write($"Discovered {propertyNodes.Count} properties.", LogLevel.Debug);

                        // Do we have any properties to traverse?
                        if (propertyNodes != null && propertyNodes.Count > 0)
                        {
                            // Gather all properties of the provided class.
                            PropertyInfo[] properties = recordType.GetProperties(BindingFlags.Static | BindingFlags.Public | BindingFlags.Instance);
                            
                            // Walk through each property in the provided class and try to match them with the XML data.
                            foreach (XmlNode node in propertyNodes)
                            {
                                try
                                {
                                    string name = node.Attributes["name"].Value;
                                    string data = node.FirstChild.InnerText;

                                    foreach (PropertyInfo property in properties)
                                    {
                                        if (property.Name.Equals(name))
                                        {
                                            try
                                            {
                                                // Attempt to use the type's Parse method with a string parameter.
                                                MethodInfo method = property.PropertyType.GetMethod("Parse", new Type[] { typeof(string) });
                                                if (method != null)
                                                {
                                                    //property contains a parse
                                                    property.SetValue(classRecord, method.Invoke(property, new object[] { data }), null);
                                                }
                                                else
                                                {
                                                    // If we don't have a reflected Parse method, then try to set the object directly.
                                                    if (property.CanWrite)
                                                        property.SetValue(classRecord, data, null);
                                                }
                                                method = null;
                                            }
                                            catch (Exception ex)
                                            {
                                                Logger.Instance.Write($"During load method reflection: {ex.Message}", LogLevel.Debug);
                                            }

                                            break;
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Logger.Instance.Write($"Property node issue: {ex.Message}", LogLevel.Warning);
                                }
                            }

                            return true;
                        }
                    }
                    else
                    {
                        Logger.Instance.Write($"Version \"{version}\" mismatch during load settings.", LogLevel.Warning);
                    }
                }
                else
                {
                    Logger.Instance.Write($"Root name \"{recordType.Name}\" not found in settings.", LogLevel.Warning);
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Write($"Unable to load settings \"{path}\", version {version}, error: {ex.Message}", LogLevel.Error);
            }

            return false;
        }

        /// <summary>
        /// Saves the given class' properties to the given file with the given version.
        /// </summary>
        /// <param name="classRecord">Class to save</param>
        /// <param name="path">File path</param>
        /// <param name="version">Version of class</param>
        /// <returns>true if succesfull, false otherwise</returns>
        public static bool Save(object classRecord, string path, string version)
        {
            try
            {
                Type recordType = classRecord.GetType();
                XmlDocument xmlDoc = new XmlDocument();
                XmlDeclaration decl = xmlDoc.CreateXmlDeclaration("1.0", "utf-8", "yes");
                XmlNode rootNode = xmlDoc.CreateElement(recordType.Name);
                XmlAttribute attrib = xmlDoc.CreateAttribute("version");
                XmlNode propertyNode = null;
                XmlNode valueNode = null;

                attrib.Value = version;
                rootNode.Attributes.Append(attrib);

                // Gather all properties of the provided class.
                PropertyInfo[] properties = recordType.GetProperties(BindingFlags.Static | BindingFlags.Public | BindingFlags.Instance);

                foreach (PropertyInfo property in properties)
                {
                    if (property.CanWrite)
                    {
                        try
                        {
                            propertyNode = xmlDoc.CreateElement("property");
                            valueNode = xmlDoc.CreateElement("value");

                            attrib = xmlDoc.CreateAttribute("name");
                            attrib.Value = property.Name;
                            propertyNode.Attributes.Append(attrib);

                            attrib = xmlDoc.CreateAttribute("type");
                            attrib.Value = property.PropertyType.ToString();
                            propertyNode.Attributes.Append(attrib);

                            if (property.GetValue(classRecord, null) != null)
                                valueNode.InnerText = property.GetValue(classRecord, null).ToString();

                            propertyNode.AppendChild(valueNode);
                            rootNode.AppendChild(propertyNode);
                        }
                        catch (Exception ex)
                        {
                            Logger.Instance.Write($"Could not create property element: {ex.Message}", LogLevel.Warning);
                        }
                    }
                }

                xmlDoc.AppendChild(decl);
                xmlDoc.AppendChild(rootNode);

                FileInfo info = new FileInfo(path);
                if (!info.Directory.Exists)
                    info.Directory.Create();

                // Save the new XML data to disk.
                xmlDoc.Save(path);

                recordType = null;
                properties = null;
                xmlDoc = null;

                return true;
            }
            catch (Exception ex)
            {
                Logger.Instance.Write($"Unable to save settings \"{path}\", version {version}, error: {ex.Message}", LogLevel.Error);
            }

            return false;
        }
        #endregion

        public override string ToString()
        {
            return "{Top=" + WindowTop.ToString(CultureInfo.CurrentCulture) + ",Left=" + WindowLeft.ToString(CultureInfo.CurrentCulture) + ",Width=" + WindowWidth.ToString(CultureInfo.CurrentCulture) + ",Height=" + WindowHeight.ToString(CultureInfo.CurrentCulture) + ",DarkTheme=" + DarkTheme.ToString(CultureInfo.CurrentCulture) + "}";
        }
    }
}
