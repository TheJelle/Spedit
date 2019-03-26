using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Windows;

namespace Spedit.Interop
{
	public class TranslationProvider
	{
		public string[] AvailableLanguageIDs;
		public string[] AvailableLanguages;

		public bool IsDefault = true;


        private Dictionary<string, string> language = new Dictionary<string, string>();

        public string GetLanguage(string langID)
        {
            //Sucks, someone have to do it better
            try
            {
                return language[langID];
            }
            catch (Exception e)
            {
                throw new Exception($"{langID} is not a known language-phrase");
            }
        }

        public void LoadLanguage(string lang, bool Initial = false)
		{
			FillToEnglishDefaults();
            Dictionary<string, string> language = new Dictionary<string, string>();
			List<string> languageList = new List<string>();
			List<string> languageIDList = new List<string>();
			languageList.Add("English");
			languageIDList.Add("");
			lang = lang.Trim().ToLowerInvariant();
			IsDefault = (string.IsNullOrEmpty(lang) || lang.ToLowerInvariant() == "en") && Initial;
			if (File.Exists("lang_0_spedit.xml"))
			{
				try
				{
					XmlDocument document = new XmlDocument();
					document.Load("lang_0_spedit.xml");
					if (document.ChildNodes.Count < 1)
					{
						throw new Exception("No Root-Node: \"translations\" found");
					}
					XmlNode rootLangNode = null;
					foreach (XmlNode childNode in document.ChildNodes[0].ChildNodes)
					{
						string lID = childNode.Name;
						string lNm = lID;
						if (childNode.Name.ToLowerInvariant() == lang)
						{
							rootLangNode = childNode;
						}
						if (childNode.FirstChild.Name.ToLowerInvariant() == "language")
						{
							lNm = childNode.FirstChild.InnerText;
						}
						languageList.Add(lNm);
						languageIDList.Add(lID);
					}
					if (rootLangNode != null)
					{
						foreach (XmlNode node in rootLangNode.ChildNodes)
						{
							if (node.NodeType == XmlNodeType.Comment)
							{
								continue;
							}
							string nn = node.Name.ToLowerInvariant();
							string nv = node.InnerText;
                            language.Add(nn, nv);
						}
					}
				}
				catch (Exception e)
				{
					MessageBox.Show("An error occured while reading the language-file. Without them, the editor wont show translations." + Environment.NewLine + "Details: " + e.Message
						, "Error while reading configs."
						, MessageBoxButton.OK
						, MessageBoxImage.Warning);
				}
			}
			AvailableLanguages = languageList.ToArray();
			AvailableLanguageIDs = languageIDList.ToArray();
		}

		private void FillToEnglishDefaults()
		{
            language.Add("Language", "English");
            language.Add("ServerRunning", "Server running");
            language.Add("Saving", "Saving");
            language.Add("SavingUFiles", "Save all unsaved files?");
            language.Add("CompileAll", "Compile all");
            language.Add("CompileCurr", "Compile current");
            language.Add("Copy", "Copy");
            language.Add("FTPUp", "FTP Upload");
            language.Add("StartServer", "Start server");
            language.Add("Replace", "Replace");
            language.Add("ReplaceAll", "Replace all");
            language.Add("OpenNewFile", "Open new file");
            language.Add("NoFileOpened", "No files opened");
            language.Add("NoFileOpenedCap", "None of the selected files could be opened.");
            language.Add("SaveFileAs", "Save file as");
            language.Add("SaveFollow", "Save following files");
            language.Add("ChDecomp", "Select plugin to decompile");
            language.Add("Decompiling", "Decompiling");
            language.Add("EditConfig", "Edit Configurations");
            language.Add("FoundInOff", "Found in offset {0} with length {1}");
            language.Add("FoundNothing", "Found nothing");
            language.Add("ReplacedOff", "Replaced in offset {0}");
            language.Add("ReplacedOcc", "Replaced {0} occurences in {1} documents");
            language.Add("OccFound", "occurences found");
            language.Add("EmptyPatt", "Empty search pattern");
            language.Add("NoValidRegex", "No valid regex pattern");
            language.Add("FailedCheck", "Failed to check");
            language.Add("ErrorUpdate", "Error while checking for updates.");
            language.Add("VersUpToDate", "Version up to date");
            language.Add("VersionYour", "Your program version {0} is up to date.");
            language.Add("Details", "Details");
            language.Add("Compiling", "Compiling");
            language.Add("Error", "Error");
            language.Add("SPCompNotStarted", "The spcomp.exe compiler did not started correctly.");
            language.Add("SPCompNotFound", "The spcomp.exe compiler could not be found.");
            language.Add("Copied", "Copied");
            language.Add("Deleted", "Deleted");
            language.Add("FailCopy", "Failed to copy");
            language.Add("NoFilesCopy", "No files copied");
            language.Add("Uploaded", "Uploaded");
            language.Add("ErrorUploadFile", "Error while uploading file: {0} to {1}");
            language.Add("ErrorUpload", "Error while uploading files");
            language.Add("Done", "Done");
            language.Add("FileStr", "File");
            language.Add("New", "New");
            language.Add("Open", "Open");
            language.Add("Save", "Save");
            language.Add("SaveAll", "Save all");
            language.Add("SaveAs", "Save as");
            language.Add("Close", "Close");
            language.Add("CloseAll", "Close all");
            language.Add("Build", "Build");
            language.Add("CopyPlugin", "Copy Plugins");
            language.Add("SendRCon", "Senc RCon commands");
            language.Add("Config", "Configuration");
            language.Add("Edit", "Edit");
            language.Add("Undo", "Undo");
            language.Add("Redo", "Redo");
            language.Add("Cut", "Cut");
            language.Add("Paste", "Paste");
            language.Add("Folding", "Foldings");
            language.Add("ExpandAll", "Expand all");
            language.Add("CollapseAll", "Collapse all");
            language.Add("JumpTo", "Jump to");
            language.Add("TogglComment", "Toggle comment");
            language.Add("SelectAll", "Select all");
            language.Add("FindReplace", "Find & Replace");
            language.Add("Tools", "Tools");
            language.Add("Options", "Options");
            language.Add("ParsedIncDir", "Parsed from include directory");
            language.Add("OldAPIWeb", "Old API webside");
            language.Add("NewAPIWeb", "New API webside");
            language.Add("Reformatter", "Syntax reformatter");
            language.Add("ReformatCurr", "Reformat current");
            language.Add("ReformatAll", "Reformat all");
            language.Add("Decompile", "Decompile");
            language.Add("ReportBugGit", "Report bug on GitHub");
            language.Add("CheckUpdates", "Check for updates");
            language.Add("About", "About");
            language.Add("FileName", "File Name");
            language.Add("Line", "Line");
            language.Add("TypeStr", "Type");
            language.Add("NormalSearch", "Normal search");
            language.Add("MatchWholeWords", "Match whole words");
            language.Add("AdvancSearch", "Advanced search");
            language.Add("RegexSearch", "Regex search");
            language.Add("CurrDoc", "Current document");
            language.Add("AllDoc", "All open documents");
            language.Add("Find", "Find");
            language.Add("Count", "Count");
            language.Add("CaseSen", "Case sensitive");
            language.Add("MultilineRegex", "Multiline Regex");
            language.Add("ErrorFileLoadProc", "Error while loading and processing the file.");
            language.Add("NotDissMethod", "Could not disassemble method {0}: {1}");
            language.Add("DFileChanged", "{0} has changed.");
            language.Add("FileChanged", "File changed");
            language.Add("FileTryReload", "Try reloading file?");
            language.Add("DSaveError", "An error occured while saving.");
            language.Add("SaveError", "Save error");
            language.Add("SavingFile", "Saving file");
            language.Add("PtAbb", "pt");
            language.Add("ColAbb", "Col");
            language.Add("LnAbb", "Ln");
            language.Add("LenAbb", "Len");
            language.Add("SPEditCap", "a lightweight sourcepawn editor");
            language.Add("WrittenBy", "written by: {0}");
            language.Add("License", "License");
            language.Add("PeopleInv", "People involved");
            language.Add("Preview", "Preview");
            language.Add("NewFile", "New file");
            language.Add("ConfigWrongPars", "The config was not able to parse a sourcepawn definiton.");
            language.Add("NoName", "no name");
            language.Add("PosLen", "(pos: {0} - len: {1})");
            language.Add("InheritedFrom", "inherited from");
            language.Add("MethodFrom", "Method from");
            language.Add("PropertyFrom", "Property from");
            language.Add("Search", "search");
            language.Add("Delete", "Delete");
            language.Add("Name", "Name");
            language.Add("ScriptDir", "Scripting directories");
            language.Add("DelimiedWi", "delimit with");
            language.Add("CopyDir", "Copy directory");
            language.Add("ServerExe", "Server executable");
            language.Add("serverStartArgs", "Server-start arguments");
            language.Add("PreBuildCom", "Pre-Build commandline");
            language.Add("PostBuildCom", "Post-Build commandline");
            language.Add("OptimizeLvl", "Optimization level");
            language.Add("VerboseLvl", "Verbose level");
            language.Add("AutoCopy", "Auto copy after compile");
            language.Add("DeleteOldSMX", "Delete old .smx after copy");
            language.Add("FTPHost", "FTP host");
            language.Add("FTPUser", "FTP user");
            language.Add("FTPPw", "FTP password");
            language.Add("FTPDir", "FTP directory");
            language.Add("ComEditorDir", "Directory of the SPEdit binary");
            language.Add("ComScriptDir", "Directory of the Compiling script");
            language.Add("ComCopyDir", "Directory where the smx should be copied");
            language.Add("ComScriptFile", "Full Directory and Name of the script");
            language.Add("ComScriptName", "File Name of the script");
            language.Add("ComPluginFile", "Full Directory and Name of the compiled script");
            language.Add("ComPluginName", "File Name of the compiled script");
            language.Add("RConEngine", "RCon server engine");
            language.Add("RConIP", "RCon server IP");
            language.Add("RconPort", "RCon server port");
            language.Add("RconPw", "RCon server password");
            language.Add("RconCom", "RCon Server commands");
            language.Add("ComPluginsReload", "Reloads all compiled plugins");
            language.Add("ComPluginsLoad", "Loads all compiled plugins");
            language.Add("ComPluginsUnload", "Unloads all compiled plugins");
            language.Add("NewConfig", "New config");
            language.Add("CannotDelConf", "Cannot delete config");
            language.Add("YCannotDelConf", "You cannot delete this config.");
            language.Add("SelectExe", "Select executable");
            language.Add("CMDLineCom", "Commandline variables");
            language.Add("RConCMDLineCom", "Rcon commandline variables");
            language.Add("ResetOptions", "Reset options");
            language.Add("ResetOptQues", "Are you sure, you want to reset the options?");
            language.Add("RestartEditor", "Restart Editor");
            language.Add("YRestartEditor", "You have to restart the editor for the changes to have effect.");
            language.Add("RestartEdiFullEff", "Restart editor to take full effect...");
            language.Add("RestartEdiEff", "Restart editor to take effect...");
            language.Add("Program", "Program");
            language.Add("HardwareAcc", "Use hardware acceleration (if available)");
            language.Add("UIAnim", "UI animations");
            language.Add("OpenInc", "Auto open includes");
            language.Add("OpenIncRec", "Open Includes Recursively");
            language.Add("AutoUpdate", "Search automatically for updates");
            language.Add("ShowToolbar", "Show toolbar");
            language.Add("DynamicISAC", "Dynamic Autocompletition/Intellisense");
            language.Add("DarkTheme", "Dark theme");
            language.Add("ThemeColor", "Theme Color");
            language.Add("LanguageStr", "Language");
            language.Add("Editor", "Editor");
            language.Add("FontSize", "Font size");
            language.Add("ScrollSpeed", "Scroll speed");
            language.Add("WordWrap", "Word wrap");
            language.Add("AggIndentation", "Agressive Indentation");
            language.Add("ReformatAfterSem", "Reformatting line after semicolon");
            language.Add("TabsToSpace", "Replace tabs with spaces");
            language.Add("AutoCloseBrack", "Auto close brackets");
            language.Add("AutoCloseStrChr", "Auto close Strings, Chars");
            language.Add("ShowSpaces", "Show spaces");
            language.Add("ShowTabs", "Show tabs");
            language.Add("IndentationSize", "Indentation size");
            language.Add("FontFamily", "Font");
            language.Add("SyntaxHigh", "Syntaxhighlighting");
            language.Add("HighDeprecat", "Highlight deprecated (<1.7) syntax");
            language.Add("Compile", "Compile");
            language.Add("AutoSaveMin", "Auto save (min)");
            language.Add("OBTextFile", "File Dir.");
            language.Add("OBTextConfig", "Config Dir.");
            language.Add("OBTextItem", "Item Dir.");
        }
	}
}
