using SourcepawnCondenser;
using SourcepawnCondenser.SourcemodDefinition;
using Spedit.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Xml;

namespace Spedit.Interop
{
    public static class ConfigLoader
    {
        public static Config[] Load()
        {
            List<Config> configs = new List<Config>();
            if (File.Exists("sourcepawn\\configs\\Configs.xml"))
            {
                try
                {
                    XmlDocument document = new XmlDocument();
                    document.Load("sourcepawn\\configs\\Configs.xml");
                    if (document.ChildNodes.Count < 1)
                    {
                        throw new Exception("No main 'Configurations' node.");
                    }
                    XmlNode mainNode = document.ChildNodes[0];
                    if (mainNode.ChildNodes.Count < 1)
                    {
                        throw new Exception("No 'config' nodes found.");
                    }
                    for (int i = 0; i < mainNode.ChildNodes.Count; ++i)
                    {
                        XmlNode node = mainNode.ChildNodes[i];
                        string _Name = ReadAttributeStringSafe(ref node, "Name", "UNKOWN CONFIG " + (i + 1).ToString());
                        string _SMDirectoryStr = ReadAttributeStringSafe(ref node, "SMDirectory", "");
                        string[] SMDirectoriesSplitted = _SMDirectoryStr.Split(';');
                        List<string> SMDirs = new List<string>();
                        foreach (string dir in SMDirectoriesSplitted)
                        {
                            string d = dir.Trim();
                            if (d.Equals(""))
                            {
                                continue; // Skip unset directories to avoid duplicate "semicolon" in path
                            }

                            // Allow relative paths that may not exist when the programs is started.
                            // For many projects includes are required, mostly placed in the project/include folder.
                            // Allowing just the entry "include" (relative path) as scripting directory should be fine.
                            // The sourcemod compiler can handle folders that does not exist (no error is thrown)
                            if (!Directory.Exists(d))
                            {
                                MessageBox.Show(
                                    string.Format("The scripting directory '{0}' specified in the configuration does not exists globally."
                                    + Environment.NewLine + "If this is an in-project folder, everything is fine. Other projects should ignore it.", d)
                                        ,"Scripting directory may not available"
                                        , MessageBoxButton.OK
                                        , MessageBoxImage.Information);
                            }
                            SMDirs.Add(d);
                        }
                        string _Standard = ReadAttributeStringSafe(ref node, "Standard", "0");
                        bool IsStandardConfig = false;
                        if (_Standard != "0" && !string.IsNullOrWhiteSpace(_Standard))
                        {
                            IsStandardConfig = true;
                        }
                        string _AutoCopyStr = ReadAttributeStringSafe(ref node, "AutoCopy", "0");
                        bool _AutoCopy = false;
                        if (_AutoCopyStr != "0" && !string.IsNullOrWhiteSpace(_AutoCopyStr))
                        {
                            _AutoCopy = true;
                        }
                        string _CopyDirectory = ReadAttributeStringSafe(ref node, "CopyDirectory", "");
                        string _ServerFile = ReadAttributeStringSafe(ref node, "ServerFile", "");
                        string _ServerArgs = ReadAttributeStringSafe(ref node, "ServerArgs", "");
                        string _PostCmd = ReadAttributeStringSafe(ref node, "PostCmd", "");
                        string _PreCmd = ReadAttributeStringSafe(ref node, "PreCmd", "");
                        int _OptimizationLevel = 2, _VerboseLevel = 1;
                        int subValue;
                        if (int.TryParse(ReadAttributeStringSafe(ref node, "OptimizationLevel", "2"), out subValue))
                        {
                            _OptimizationLevel = subValue;
                        }
                        if (int.TryParse(ReadAttributeStringSafe(ref node, "VerboseLevel", "1"), out subValue))
                        {
                            _VerboseLevel = subValue;
                        }
                        bool _DeleteAfterCopy = false;
                        string DeleteAfterCopyStr = ReadAttributeStringSafe(ref node, "DeleteAfterCopy", "0");
                        if (!(DeleteAfterCopyStr == "0" || string.IsNullOrWhiteSpace(DeleteAfterCopyStr)))
                        {
                            _DeleteAfterCopy = true;
                        }
                        string _FTPHost = ReadAttributeStringSafe(ref node, "FTPHost", "ftp://localhost/");
                        string _FTPUser = ReadAttributeStringSafe(ref node, "FTPUser", "");
                        string encryptedFTPPW = ReadAttributeStringSafe(ref node, "FTPPassword", "");
                        string _FTPPW = ManagedAES.Decrypt(encryptedFTPPW);
                        string _FTPDir = ReadAttributeStringSafe(ref node, "FTPDir", "");
                        string _RConEngineSourceStr = ReadAttributeStringSafe(ref node, "RConSourceEngine", "1");
                        bool _RConEngineTypeSource = false;
                        if (!(_RConEngineSourceStr == "0" || string.IsNullOrWhiteSpace(_RConEngineSourceStr)))
                        {
                            _RConEngineTypeSource = true;
                        }
                        string _RConIP = ReadAttributeStringSafe(ref node, "RConIP", "127.0.0.1");
                        string _RConPortStr = ReadAttributeStringSafe(ref node, "RConPort", "27015");
                        ushort _RConPort = 27015;
                        if (!ushort.TryParse(_RConPortStr, NumberStyles.Any, CultureInfo.InvariantCulture, out _RConPort))
                        {
                            _RConPort = 27015;
                        }
                        string encryptedRConPassword = ReadAttributeStringSafe(ref node, "RConPassword", "");
                        string _RConPassword = ManagedAES.Decrypt(encryptedRConPassword);
                        string _RConCommands = ReadAttributeStringSafe(ref node, "RConCommands", "");
                        Config c = new Config()
                        {
                            Name = _Name,
                            SMDirectories = SMDirs.ToArray(),
                            Standard = IsStandardConfig
                            ,
                            AutoCopy = _AutoCopy,
                            CopyDirectory = _CopyDirectory,
                            ServerFile = _ServerFile,
                            ServerArgs = _ServerArgs
                            ,
                            PostCmd = _PostCmd,
                            PreCmd = _PreCmd,
                            OptimizeLevel = _OptimizationLevel,
                            VerboseLevel = _VerboseLevel,
                            DeleteAfterCopy = _DeleteAfterCopy
                            ,
                            FTPHost = _FTPHost,
                            FTPUser = _FTPUser,
                            FTPPassword = _FTPPW,
                            FTPDir = _FTPDir
                            ,
                            RConUseSourceEngine = _RConEngineTypeSource,
                            RConIP = _RConIP,
                            RConPort = _RConPort,
                            RConPassword = _RConPassword,
                            RConCommands = _RConCommands
                        };
                        if (IsStandardConfig)
                        {
                            c.LoadSMDef();
                        }
                        configs.Add(c);
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show("An error appeared while reading the configs. Without them, the editor wont start. Reinstall program!" + Environment.NewLine + "Details: " + e.Message
                        , "Error while reading configs."
                        , MessageBoxButton.OK
                        , MessageBoxImage.Warning);
                    Environment.Exit(Environment.ExitCode);
                }
            }
            else
            {
                MessageBox.Show("The Editor could not find the Configs.xml file. Without it, the editor wont start. Reinstall program.", "File not found.", MessageBoxButton.OK, MessageBoxImage.Warning);
                Environment.Exit(Environment.ExitCode);
            }
            return configs.ToArray();
        }

        private static string ReadAttributeStringSafe(ref XmlNode node, string attributeName, string defaultValue = "")
        {
            for (int i = 0; i < node.Attributes.Count; ++i)
            {
                if (node.Attributes[i].Name == attributeName)
                {
                    return node.Attributes[i].Value;
                }
            }
            return defaultValue;
        }
    }

    public class Config
    {
        public string Name = string.Empty;

        public bool Standard = false;

        public bool AutoCopy = false;

        public string[] SMDirectories = new string[0];
        public string CopyDirectory = string.Empty;
        public string ServerFile = string.Empty;
        public string ServerArgs = string.Empty;

        public string PostCmd = string.Empty;
        public string PreCmd = string.Empty;

        public bool DeleteAfterCopy = false;

        public int OptimizeLevel = 2;
        public int VerboseLevel = 1;

        public string FTPHost = "ftp://localhost/";
        public string FTPUser = string.Empty;
        public string FTPPassword = string.Empty; //securestring? No! Because it's saved in plaintext and if you want to keep it a secret, you shouldn't automaticly uploade it anyways...
        public string FTPDir = string.Empty;

        public bool RConUseSourceEngine = true;
        public string RConIP = "127.0.0.1";
        public ushort RConPort = 27015;
        public string RConPassword = string.Empty;
        public string RConCommands = string.Empty;

        private SMDefinition SMDef;

        public SMDefinition GetSMDef()
        {
            if (SMDef == null)
            {
                LoadSMDef();
            }
            return SMDef;
        }

        public void InvalidateSMDef()
        {
            this.SMDef = null;
        }

        public void LoadSMDef()
        {
            if (this.SMDef != null)
            {
                return;
            }
            try
            {
				SMDefinition def = new SMDefinition();
				def.AppendFiles(SMDirectories);
				SMDef = def;
            }
            catch (Exception)
            {
                this.SMDef = new SMDefinition(); //this could be dangerous...
            }
        }
    }
}
