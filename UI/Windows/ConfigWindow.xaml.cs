using MahApps.Metro;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Spedit.Interop;
using Spedit.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;
using Microsoft.CSharp;
using Microsoft.Win32;

namespace Spedit.UI.Windows
{
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class ConfigWindow : MetroWindow
    {
        private bool NeedsSMDefInvalidation = false;
        private bool AllowChange = false;

        public ConfigWindow()
        {
            InitializeComponent();
			Language_Translate();
			if (Program.OptionsObject.Program_AccentColor != "Red" || Program.OptionsObject.Program_Theme != "BaseDark")
			{ ThemeManager.ChangeAppStyle(this, ThemeManager.GetAccent(Program.OptionsObject.Program_AccentColor), ThemeManager.GetAppTheme(Program.OptionsObject.Program_Theme)); }
			for (int i = 0; i < Program.Configs.Length; ++i)
            {
                ConfigListBox.Items.Add(new ListBoxItem() { Content = Program.Configs[i].Name });
            }
            ConfigListBox.SelectedIndex = Program.SelectedConfig;
        }

        private void ConfigListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadConfigToUI(ConfigListBox.SelectedIndex);
        }

        private void LoadConfigToUI(int index)
        {
            if (index < 0 || index >= Program.Configs.Length)
            {
                return;
            }
            AllowChange = false;
            Config c = Program.Configs[index];
            C_Name.Text = c.Name;
            StringBuilder SMDirOut = new StringBuilder();
            foreach (string dir in c.SMDirectories) { SMDirOut.Append(dir.Trim() + ";"); }
            C_SMDir.Text = SMDirOut.ToString();
            C_AutoCopy.IsChecked = c.AutoCopy;
            C_CopyDir.Text = c.CopyDirectory;
            C_ServerFile.Text = c.ServerFile;
            C_ServerArgs.Text = c.ServerArgs;
            C_PreBuildCmd.Text = c.PreCmd;
            C_PostBuildCmd.Text = c.PostCmd;
            C_OptimizationLevel.Value = c.OptimizeLevel;
            C_VerboseLevel.Value = c.VerboseLevel;
            C_DeleteAfterCopy.IsChecked = c.DeleteAfterCopy;
            C_FTPHost.Text = c.FTPHost;
            C_FTPUser.Text = c.FTPUser;
            C_FTPPW.Password = c.FTPPassword;
            C_FTPDir.Text = c.FTPDir;
            C_RConEngine.SelectedIndex = (c.RConUseSourceEngine) ? 0 : 1;
            C_RConIP.Text = c.RConIP;
            C_RConPort.Text = c.RConPort.ToString();
            C_RConPW.Password = c.RConPassword;
            C_RConCmds.Text = c.RConCommands;
            AllowChange = true;
        }

        private void NewButton_Clicked(object sender, RoutedEventArgs e)
        {
            Config c = new Config() { Name = "New Config", Standard = false, OptimizeLevel = 2, VerboseLevel = 1 };
            List<Config> configList = new List<Config>(Program.Configs);
            configList.Add(c);
            Program.Configs = configList.ToArray();
            ConfigListBox.Items.Add(new ListBoxItem() { Content = Program.Translations.GetLanguage("NewConfig")});
        }

        private void DeleteButton_Clicked(object sender, RoutedEventArgs e)
        {
            int index = ConfigListBox.SelectedIndex;
            Config c = Program.Configs[index];
            if (c.Standard)
            {
                this.ShowMessageAsync(Program.Translations.GetLanguage("CannotDelConf"), Program.Translations.GetLanguage("YCannotDelConf"), MessageDialogStyle.Affirmative, this.MetroDialogOptions);
                return;
            }
            List<Config> configList = new List<Config>(Program.Configs);
            configList.RemoveAt(index);
            Program.Configs = configList.ToArray();
            ConfigListBox.Items.RemoveAt(index);
            if (index == Program.SelectedConfig)
            {
                Program.SelectedConfig = 0;
            }
            ConfigListBox.SelectedIndex = 0;
        }

        private void C_Name_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!AllowChange) { return; }
            string Name = C_Name.Text;
            Program.Configs[ConfigListBox.SelectedIndex].Name = Name;
            ((ListBoxItem)ConfigListBox.SelectedItem).Content = Name;
        }

        private void C_SMDir_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!AllowChange) { return; }
            string[] SMDirs = C_SMDir.Text.Split(';');
            List<string> dirs = new List<string>();
            foreach (string dir in SMDirs)
            {
                string d = dir.Trim();
                dirs.Add(d);
            }
            Program.Configs[ConfigListBox.SelectedIndex].SMDirectories = dirs.ToArray();
            NeedsSMDefInvalidation = true;
        }

        private void C_CopyDir_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!AllowChange) { return; }
            Program.Configs[ConfigListBox.SelectedIndex].CopyDirectory = C_CopyDir.Text;
        }

        private void C_ServerFile_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!AllowChange) { return; }
            Program.Configs[ConfigListBox.SelectedIndex].ServerFile = C_ServerFile.Text;
        }

        private void C_ServerArgs_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!AllowChange) { return; }
            Program.Configs[ConfigListBox.SelectedIndex].ServerArgs = C_ServerArgs.Text;
        }

        private void C_PostBuildCmd_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!AllowChange) { return; }
            Program.Configs[ConfigListBox.SelectedIndex].PostCmd = C_PostBuildCmd.Text;
        }

        private void C_PreBuildCmd_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!AllowChange) { return; }
            Program.Configs[ConfigListBox.SelectedIndex].PreCmd = C_PreBuildCmd.Text;
        }

        private void C_OptimizationLevel_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!AllowChange) { return; }
            Program.Configs[ConfigListBox.SelectedIndex].OptimizeLevel = (int)C_OptimizationLevel.Value;
        }

        private void C_VerboseLevel_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!AllowChange) { return; }
            Program.Configs[ConfigListBox.SelectedIndex].VerboseLevel = (int)C_VerboseLevel.Value;
        }

        private void C_AutoCopy_Changed(object sender, RoutedEventArgs e)
        {
            if (!AllowChange) { return; }
            Program.Configs[ConfigListBox.SelectedIndex].AutoCopy = C_AutoCopy.IsChecked.Value;
        }

        private void C_DeleteAfterCopy_Changed(object sender, RoutedEventArgs e)
        {
            if (!AllowChange) { return; }
            Program.Configs[ConfigListBox.SelectedIndex].DeleteAfterCopy = C_DeleteAfterCopy.IsChecked.Value;
        }

        private void C_FTPHost_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!AllowChange) { return; }
            Program.Configs[ConfigListBox.SelectedIndex].FTPHost = C_FTPHost.Text;
        }

        private void C_FTPUser_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!AllowChange) { return; }
            Program.Configs[ConfigListBox.SelectedIndex].FTPUser = C_FTPUser.Text;
        }

        private void C_FTPPW_TextChanged(object sender, RoutedEventArgs e)
        {
            if (!AllowChange) { return; }
            Program.Configs[ConfigListBox.SelectedIndex].FTPPassword = C_FTPPW.Password;
        }

        private void C_FTPDir_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!AllowChange) { return; }
            Program.Configs[ConfigListBox.SelectedIndex].FTPDir = C_FTPDir.Text;
        }

        private void C_RConEngine_Changed(object sender, RoutedEventArgs e)
        {
            if (!AllowChange) { return; }
            if (ConfigListBox.SelectedIndex >= 0)
            {
                Program.Configs[ConfigListBox.SelectedIndex].RConUseSourceEngine = (C_RConEngine.SelectedIndex == 0);
            }
        }

        private void C_RConIP_TextChanged(object sender, RoutedEventArgs e)
        {
            if (!AllowChange) { return; }
            Program.Configs[ConfigListBox.SelectedIndex].RConIP = C_RConIP.Text;
        }

        private void C_RConPort_TextChanged(object sender, RoutedEventArgs e)
        {
            if (!AllowChange) { return; }
            ushort newPort;
            if (!ushort.TryParse(C_RConPort.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out newPort))
            {
                newPort = 27015;
                C_RConPort.Text = "27015";
            }
            Program.Configs[ConfigListBox.SelectedIndex].RConPort = newPort;
        }

        private void C_RConPW_TextChanged(object sender, RoutedEventArgs e)
        {
            if (!AllowChange) { return; }
            Program.Configs[ConfigListBox.SelectedIndex].RConPassword = C_RConPW.Password;
        }

        private void C_RConCmds_TextChanged(object sender, RoutedEventArgs e)
        {
            if (!AllowChange) { return; }
            Program.Configs[ConfigListBox.SelectedIndex].RConCommands = C_RConCmds.Text;
        }

        private void MetroWindow_Closed(object sender, EventArgs e)
        {
            if (NeedsSMDefInvalidation)
            {
                for (int i = 0; i < Program.Configs.Length; ++i)
                {
                    Program.Configs[i].InvalidateSMDef();
                }
            }
            Program.MainWindow.FillConfigMenu();
            Program.MainWindow.ChangeConfig(Program.SelectedConfig);
            StringBuilder outString = new StringBuilder();
            XmlWriterSettings settings = new XmlWriterSettings() { Indent = true, IndentChars = "\t", NewLineOnAttributes = false, OmitXmlDeclaration = true };
            using (XmlWriter writer = XmlWriter.Create(outString, settings))
            {
                writer.WriteStartElement("Configurations");
                for (int i = 0; i < Program.Configs.Length; ++i)
                {
                    Config c = Program.Configs[i];
                    writer.WriteStartElement("Config");
                    writer.WriteAttributeString("Name", c.Name);
                    StringBuilder SMDirOut = new StringBuilder();
                    foreach (string dir in c.SMDirectories) { SMDirOut.Append(dir.Trim() + ";"); }
                    writer.WriteAttributeString("SMDirectory", SMDirOut.ToString());
                    writer.WriteAttributeString("Standard", (c.Standard) ? "1" : "0");
                    writer.WriteAttributeString("CopyDirectory", c.CopyDirectory);
                    writer.WriteAttributeString("AutoCopy", (c.AutoCopy) ? "1" : "0");
                    writer.WriteAttributeString("ServerFile", c.ServerFile);
                    writer.WriteAttributeString("ServerArgs", c.ServerArgs);
                    writer.WriteAttributeString("PostCmd", c.PostCmd);
                    writer.WriteAttributeString("PreCmd", c.PreCmd);
                    writer.WriteAttributeString("OptimizationLevel", c.OptimizeLevel.ToString());
                    writer.WriteAttributeString("VerboseLevel", c.VerboseLevel.ToString());
                    writer.WriteAttributeString("DeleteAfterCopy", (c.DeleteAfterCopy) ? "1" : "0");
                    writer.WriteAttributeString("FTPHost", c.FTPHost);
                    writer.WriteAttributeString("FTPUser", c.FTPUser);
                    writer.WriteAttributeString("FTPPassword", ManagedAES.Encrypt(c.FTPPassword));
                    writer.WriteAttributeString("FTPDir", c.FTPDir);
                    writer.WriteAttributeString("RConSourceEngine", (c.RConUseSourceEngine) ? "1" : "0");
                    writer.WriteAttributeString("RConIP", c.RConIP);
                    writer.WriteAttributeString("RConPort", c.RConPort.ToString());
                    writer.WriteAttributeString("RConPassword", ManagedAES.Encrypt(c.RConPassword));
                    writer.WriteAttributeString("RConCommands", c.RConCommands);
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
                writer.Flush();
            }
            File.WriteAllText("sourcepawn\\configs\\Configs.xml", outString.ToString());
        }

		private void Language_Translate()
		{
			if (Program.Translations.IsDefault)
			{
				return;
			}
			NewButton.Content = Program.Translations.GetLanguage("New");
			DeleteButton.Content = Program.Translations.GetLanguage("Delete");
			NameBlock.Text = Program.Translations.GetLanguage("Name");
			ScriptingDirBlock.Text = Program.Translations.GetLanguage("ScriptDir");
			DelimitWiBlock.Text = $"({Program.Translations.GetLanguage("DelimiedWi")}";
			CopyDirBlock.Text = Program.Translations.GetLanguage("CopyDir");
			ServerExeBlock.Text = Program.Translations.GetLanguage("ServerExe");
			ServerStartArgBlock.Text = Program.Translations.GetLanguage("serverStartArgs");
			PreBuildBlock.Text = Program.Translations.GetLanguage("PreBuildCom");
			PostBuildBlock.Text = Program.Translations.GetLanguage("PostBuildCom");
			OptimizeBlock.Text = Program.Translations.GetLanguage("OptimizeLvl");
			VerboseBlock.Text = Program.Translations.GetLanguage("VerboseLvl");
			C_AutoCopy.Content = Program.Translations.GetLanguage("AutoCopy");
			C_DeleteAfterCopy.Content = Program.Translations.GetLanguage("DeleteOldSMX");
			FTPHostBlock.Text = Program.Translations.GetLanguage("FTPHost");
			FTPUserBlock.Text = Program.Translations.GetLanguage("FTPUser");
			FTPPWBlock.Text = Program.Translations.GetLanguage("FTPPw");
			FTPDirBlock.Text = Program.Translations.GetLanguage("FTPDir");
			CMD_ItemC.Text = Program.Translations.GetLanguage("CMDLineCom");
			ItemC_EditorDir.Content = "{editordir} - " + Program.Translations.GetLanguage("ComEditorDir");
			ItemC_ScriptDir.Content = "{scriptdir} - " + Program.Translations.GetLanguage("ComScriptDir");
			ItemC_CopyDir.Content = "{copydir} - " + Program.Translations.GetLanguage("ComCopyDir");
			ItemC_ScriptFile.Content = "{scriptfile} - " + Program.Translations.GetLanguage("ComScriptFile");
			ItemC_ScriptName.Content = "{scriptname} - " + Program.Translations.GetLanguage("ComScriptName");
			ItemC_PluginFile.Content = "{pluginfile} - " + Program.Translations.GetLanguage("ComPluginFile");
			ItemC_PluginName.Content = "{pluginname} - " + Program.Translations.GetLanguage("ComPluginName");
			RConEngineBlock.Text = Program.Translations.GetLanguage("RConEngine");
			RConIPBlock.Text = Program.Translations.GetLanguage("RConIP");
			RConPortBlock.Text = Program.Translations.GetLanguage("RconPort");
			RConPWBlock.Text = Program.Translations.GetLanguage("RconPw");
			RConComBlock.Text = Program.Translations.GetLanguage("RconCom");
			Rcon_MenuC.Text = Program.Translations.GetLanguage("RConCMDLineCom");
			MenuC_PluginsReload.Content = "{plugins_reload} - " + Program.Translations.GetLanguage("ComPluginsReload");
			MenuC_PluginsLoad.Content = "{plugins_load} - " + Program.Translations.GetLanguage("ComPluginsLoad");
			MenuC_PluginsUnload.Content = "{plugins_unload} - " + Program.Translations.GetLanguage("ComPluginsUnload");
		}

		private ICommand textBoxButtonFolderCmd;

        public ICommand TextBoxButtonFolderCmd
        {
            set { }
            get
            {
                if (this.textBoxButtonFolderCmd == null)
                {
                    var cmd = new SimpleCommand();
                    cmd.CanExecutePredicate = o =>
                    {
                        return true;
                    };
                    cmd.ExecuteAction = o =>
                    {
                        if (o is TextBox)
                        {
                            var dialog = new System.Windows.Forms.FolderBrowserDialog();
                            var result = dialog.ShowDialog();
                            if (result == System.Windows.Forms.DialogResult.OK)
                            {
                                ((TextBox)o).Text = dialog.SelectedPath;
                            }
                        }
                    };
                    this.textBoxButtonFolderCmd = cmd;
                    return cmd;
                }
                else
                {
                    return textBoxButtonFolderCmd;
                }
            }
        }

        private ICommand textBoxButtonFileCmd;

        public ICommand TextBoxButtonFileCmd
        {
            set { }
            get
            {
                if (this.textBoxButtonFileCmd == null)
                {
                    var cmd = new SimpleCommand();
                    cmd.CanExecutePredicate = o =>
                    {
                        return true;
                    };
                    cmd.ExecuteAction = o =>
                    {
                        if (o is TextBox)
                        {
                            var dialog = new OpenFileDialog();
                            dialog.Filter = "Executables *.exe|*.exe|All Files *.*|*.*";
                            dialog.Multiselect = false;
                            dialog.CheckFileExists = true; dialog.CheckPathExists = true;
                            dialog.Title = Program.Translations.GetLanguage("SelectExe");
                            var result = dialog.ShowDialog();
                            if (result.Value)
                            {
                                FileInfo fInfo = new FileInfo(dialog.FileName);
                                if (fInfo.Exists)
                                {
                                    ((TextBox)o).Text = fInfo.FullName;
                                }
                            }
                        }
                    };
                    this.textBoxButtonFileCmd = cmd;
                    return cmd;
                }
                else
                {
                    return textBoxButtonFileCmd;
                }
            }
        }


        private class SimpleCommand : ICommand
        {
            public Predicate<object> CanExecutePredicate { get; set; }
            public Action<object> ExecuteAction { get; set; }

            public bool CanExecute(object parameter)
            {
                return true;
            }

            public event EventHandler CanExecuteChanged
            {
                add { CommandManager.RequerySuggested += value; }
                remove { CommandManager.RequerySuggested -= value; }
            }

            public void Execute(object parameter)
            {
                if (ExecuteAction != null)
                {
                    ExecuteAction(parameter);
                }
            }
        }

    }
}
