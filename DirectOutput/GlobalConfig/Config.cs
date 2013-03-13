﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using System.Xml;
using System.Reflection;
using DirectOutput.FX;

using DirectOutput.General.Generic;

namespace DirectOutput.GlobalConfig
{

    /// <summary>
    /// Global configuration for the DirectOutput framework.<br/> 
    /// </summary>
    public class Config
    {

        #region Led Control files
        private LedControlIniFileList _LedControlIniFiles = new LedControlIniFileList();

        //TODO: Serialisierung anpassen, doku auch anpassen
        /// <summary>
        /// Gets or sets the list of LedControl.ini files.<br/>
        /// </summary>
        /// <value>The list of LedControl.ini files.
        /// </value>
        public LedControlIniFileList LedControlIniFiles
        {
            get { return _LedControlIniFiles; }
            set { _LedControlIniFiles = value; }
        }

        #endregion


        #region Cabinet

        #region Cabinet config file
        private FilePattern _CabinetConfigFilePattern=new FilePattern();

        /// <summary>
        /// Gets or sets the cabinet config file pattern.
        /// </summary>
        /// <value>
        /// The cabinet config file pattern.
        /// </value>
        public FilePattern CabinetConfigFilePattern
        {
            get { return _CabinetConfigFilePattern; }
            set { _CabinetConfigFilePattern = value; }
        }


        /// <summary>
        ///  FileInfo object for the file containing the configuration of the cabinet (outputs, toys and so on). 
        /// </summary>
        /// <returns>FileInfo object for the file containing the configuration of the cabinet or null if no file has been specified.</returns>
        public FileInfo GetCabinetConfigFile()
        {
            if (CabinetConfigFilePattern != null)
            {
                return CabinetConfigFilePattern.GetFirstMatchingFile(GetReplaceValuesDictionary());
            }
            return null;
        }


        /// <summary>
        /// Gets the cabinet config directory.
        /// </summary>
        /// <returns>The DirectoryInfo object for the cabinet config directory or null if no CabinetConfigFile is available.</returns>
        public DirectoryInfo GetCabinetConfigDirectory()
        {
            FileInfo CC = GetCabinetConfigFile();
            if (CC != null)
            {
                return CC.Directory;
            }
            return null;
        } 
        #endregion

        #region Cabinet Scripts file patterns
        private FilePatternList _CabinetScriptFilePatterns = new FilePatternList() ;

        /// <summary>
        /// Gets or sets the search patterns for cabinet scripts files.
        /// </summary>
        /// <value>
        /// The search patterns for cabinet scripts files.
        /// </value>
        public FilePatternList CabinetScriptFilePatterns
        {
            get { return _CabinetScriptFilePatterns; }
            set { _CabinetScriptFilePatterns = value; }
        }


        /// <summary>
        /// Gets a list of FileInfo objects representing the cabinet script files.
        /// </summary>
        /// <returns>A list of FileOnfo objects or a empty list of the files have been found.</returns>
        public List<FileInfo> GetCabinetScriptFiles()
        {
            if (CabinetScriptFilePatterns != null)
            {
                return CabinetScriptFilePatterns.GetMatchingFiles(GetReplaceValuesDictionary());
            }
            return new List<FileInfo>();
        }


        #endregion



        #endregion

        //private EffectList _Effects = new EffectList();
        ///// <summary>
        ///// List containing global effects. 
        ///// </summary>
       
        //[XmlElementAttribute(Order = 5)]
        //public EffectList Effects
        //{
        //    get { return _Effects; }
        //    set { _Effects = value; }
        //}


        #region Table



        #region Table script files
        private FilePatternList _TableScriptFilePatterns=new FilePatternList();

        /// <summary>
        /// Gets or sets the table script file patterns.
        /// </summary> 
        /// <value>
        /// The table script file patterns.
        /// </value>
        public FilePatternList TableScriptFilePatterns
        {
            get { return _TableScriptFilePatterns; }
            set { _TableScriptFilePatterns = value; }
        }

        /// <summary>
        /// Gets the list of table script files.
        /// </summary>
        /// <param name="FullTableFilename">The filename, inckuding path, to the table.</param>
        /// <returns>
        /// The list of table script files or a empty list if no table script files have been found.
        /// </returns>
        public List<FileInfo> GetTableScriptFiles(string FullTableFilename)
        {
            if (TableScriptFilePatterns != null)
            {
                return TableScriptFilePatterns.GetMatchingFiles(GetReplaceValuesDictionary(FullTableFilename));
            }
            return new List<FileInfo>();
        } 
        #endregion


        #region Table Config
        private FilePattern _TableConfigFilePattern=new FilePattern();

        /// <summary>
        /// Gets or sets the table config file pattern.
        /// </summary>
        /// <value>
        /// The table config file pattern.
        /// </value>
        public FilePattern TableConfigFilePattern
        {
            get { return _TableConfigFilePattern; }
            set { _TableConfigFilePattern = value; }
        }




        /// <summary>
        /// Gets a FileInfo object for the table config file.<br/>
        /// The file is lookued up using the value of the property TableConfigFilePattern.
        /// If more than one file matches the search pattern, only the first file is returned.
        /// </summary>
        /// <param name="FullTableFilename">The table filename (The *.vpt file for the table, not the config file).</param>
        /// <returns>A FileInfo object for the table config file or null if no matching file was found.</returns>
        public FileInfo GetTableConfigFile(string FullTableFilename)
        {
            if (TableConfigFilePattern != null)
            {
                return TableConfigFilePattern.GetFirstMatchingFile(GetReplaceValuesDictionary(FullTableFilename));
            }
            return null;
        }
        
        #endregion


        #endregion


        #region Update timer
        private int _UpdateTimerIntervall;
        /// <summary>
        /// Intervall for the update timer in milliseconds.
        /// </summary>
        /// <value>
        /// int representing the intervall in milliseconds for the update timer.
        /// </value>
        public int UpdateTimerIntervall
        {
            get { return _UpdateTimerIntervall; }
            set { _UpdateTimerIntervall = value; }
        } 
        #endregion


        private Dictionary<string, string> GetReplaceValuesDictionary(string TableFileName=null)
        {
            Dictionary<string, string> D = new Dictionary<string, string>();
            D.Add("GlobalConfigDirectory", GlobalConfigDirectory.FullName);
            D.Add("GlobalConfigDir", GlobalConfigDirectory.FullName);
            D.Add("DllDirectory", Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
            D.Add("DllDir", Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
            D.Add("AssemblyDirectory", Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
            D.Add("AssemblyDir", Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
            if (!TableFileName.IsNullOrWhiteSpace())
            {
                D.Add("TableDirectory", Path.GetFullPath(TableFileName));
                D.Add("TableDir", Path.GetFullPath(TableFileName));
                D.Add("TableDirectoryName", new DirectoryInfo(Path.GetFullPath(TableFileName)).Name);
                D.Add("TableDirName", new DirectoryInfo(Path.GetFullPath(TableFileName)).Name);
                D.Add("TableName", Path.GetFileNameWithoutExtension(TableFileName));
            }


            return D;

        }


        #region Static Properties


        /// <summary>
        /// Gets an list of FileInfo objects for the global script files.<br/>
        /// If no script files are found or if a error occurs when looking for the script files a list array is returned.<br/>
        /// The script files are looked up using the value of the property GlobalScriptsFilePattern.
        /// </summary>
        /// <returns>List of FileInfo objects for the global script files.</returns>
        public List<FileInfo> GetGlobalScriptFiles()
        {
            return GlobalScriptsFilePattern.GetMatchingFiles();
        }


        /// <summary>
        /// Search pattern for global script files (readonly).
        /// </summary>
        public static FilePattern GlobalScriptsFilePattern
        {
            get { return new FilePattern("*.cs"); }
        }

        /// <summary>
        /// Path to the directory where the global config is stored (readonly).
        /// </summary>
        /// <value>string containing to the global config directory.</value>
        public static string GlobalConfigDirectoryName
        {
            get
            {
                return Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Config");
            }
        }

        /// <summary>
        /// Gets a DirectoryInfo object for the global config directory.
        /// </summary>
        /// <value>
        /// The DirectoryInfo object for the global config directory.
        /// </value>
        public static DirectoryInfo GlobalConfigDirectory
        {
            get { return new DirectoryInfo(GlobalConfigDirectoryName); }
        }


        /// <summary>
        /// Filename of the global config file (readonly).
        /// </summary>
        /// <value>string containg the full path and filename of the global config file.</value>
        public static string GlobalConfigFilename
        {
            //TODO: CHeck if this should be loaded from the registry.
            get { return Path.Combine(GlobalConfigDirectoryName, "GlobalConfig.xml"); }
        }


        /// <summary>
        /// Gets a FileInfo object for the global config file.
        /// </summary>
        /// <returns>FileInfo object for the global config file.</returns>
        public static FileInfo GetGlobalConfigFile()
        {
             return new FileInfo(GlobalConfigFilename); 
        }

        #endregion


        #region Serialization
        /// <summary>
        /// Returns a serialized XML representation of the global configuration.
        /// </summary>
        /// <returns>XMLString</returns>
        public string GetGlobalConfigXml()
        {

            XmlSerializerNamespaces Namespaces = new XmlSerializerNamespaces();
            Namespaces.Add(string.Empty, string.Empty);
            XmlSerializer Serializer = new XmlSerializer(typeof(Config));
            MemoryStream Stream = new MemoryStream();
            XmlWriterSettings Settings = new XmlWriterSettings();
            Settings.Indent = true;
            Settings.NewLineOnAttributes = true;

            XmlWriter Writer = XmlWriter.Create(Stream, Settings);
            Writer.WriteStartDocument();
            Writer.WriteComment("Global configuration for the DirectOutput framework.");
            Writer.WriteComment("Saved by DirectOutput Version {1}: {0}".Build(DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss"), System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString()));
            Serializer.Serialize(Writer, this, Namespaces);
            Writer.WriteEndDocument();
            Writer.Flush();
            Stream.Position = 0;
            string XML;
            using (StreamReader sr = new StreamReader(Stream, Encoding.Default))
            {
                XML = sr.ReadToEnd();
                sr.Dispose();
            }
            Stream.Dispose();

            return XML;
        }



        /// <summary>
        /// Instanciates a GlobalConfig object from a global configuration in a XML file.<br/>
        /// If the global config file does not exist or can not be loaded, null will be returned.
        /// </summary>
        /// <param name="GlobalConfigFileName">(Optional) Name of the global config XML file. If no value is supplied, the default global config filename is used.</param>
        /// <returns>GlobalConfig object or null.</returns>
        public static Config GetGlobalConfigFromConfigXmlFile(string GlobalConfigFileName="")
        {
            string GCFileName = (GlobalConfigFilename.IsNullOrWhiteSpace() ? Config.GlobalConfigFilename : GlobalConfigFilename);
            //try
            {
                if (File.Exists(GCFileName))
                {
                    //try
                    {

                        string Xml = General.FileReader.ReadFileToString(GCFileName);

                        return GetGlobalConfigFromGlobalConfigXml(Xml);
                    }
                    //catch
                    {
                      //  return null;
                    }
                }
                else
                {
                    return null;
                }
            }
            //catch
            {
              //  return null;
            }
        }


        /// <summary>
        /// Instanciates a GlobalConfig object from a global configuration in a XML string.
        /// </summary>
        /// <param name="ConfigXml">XML string</param>
        /// <returns>GlobalConfig object for the specified ConfigXML.</returns>
        public static Config GetGlobalConfigFromGlobalConfigXml(string ConfigXml)
        {


            byte[] xmlBytes = Encoding.Default.GetBytes(ConfigXml);
            using (MemoryStream ms = new MemoryStream(xmlBytes))
            {
                return (Config)new XmlSerializer(typeof(Config)).Deserialize(ms);
            }
        }

        /// <summary>
        /// Saves the GlobalConfig to the file specified in GlobalConfigFilename.<br />
        /// Before saving the current global config file is backed up.
        /// </summary>
        /// <param name="GlobalConfigFilename">(Optional)Global config filename. If new value is supplied the default global config filename is used.</param>
        public void SaveGlobalConfig(string GlobalConfigFilename="")
        {
            string GCFileName=(GlobalConfigFilename.IsNullOrWhiteSpace()?Config.GlobalConfigFilename:GlobalConfigFilename);

            if (File.Exists(GCFileName))
            {
                //Create a backup of the current global config file
                File.Copy(GCFileName, Path.Combine(Path.GetDirectoryName(GCFileName), "{1} old (replaced {0}){2}".Build(DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss"), Path.GetFileNameWithoutExtension(GCFileName),Path.GetExtension(GCFileName))));
            };
            DirectoryInfo GCDirectory = new FileInfo(GCFileName).Directory;
            //TODO: D8ir dreate recursiv machen
            GCDirectory.Create();
            GetGlobalConfigXml().WriteToFile(GCFileName, false);

        }
        #endregion


        /// <summary>
        /// Initializes a new instance of the <see cref="Config"/> class.
        /// </summary>
        public Config()
        {

            UpdateTimerIntervall = 20;

        }

    }
}