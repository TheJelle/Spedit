using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Spedit.UI.Components;
using Spedit.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Controls;
using System.Collections.ObjectModel;

namespace Spedit.UI
{
	public partial class MainWindow : MetroWindow
	{
		public void Language_Translate(bool Initial = false)
		{
			if (Initial && Program.Translations.IsDefault)
			{
				return;
			}
			if (!Initial)
			{
				compileButtonDict = new ObservableCollection<string>() { Program.Translations.GetLanguage("CompileAll"), Program.Translations.GetLanguage("CompileCurr")};
                actionButtonDict = new ObservableCollection<string>() { Program.Translations.GetLanguage("Copy"), Program.Translations.GetLanguage("FTPUp"), Program.Translations.GetLanguage("StartServer") };
                findReplaceButtonDict = new ObservableCollection<string>() { Program.Translations.GetLanguage("Replace"), Program.Translations.GetLanguage("ReplaceAll") };
				((MenuItem)ConfigMenu.Items[ConfigMenu.Items.Count - 1]).Header = Program.Translations.GetLanguage("EditConfig");
				EditorElement[] ee = GetAllEditorElements();
				if (ee != null)
				{
					for (int i = 0; i < ee.Length; ++i)
					{
						if (ee[i] != null)
						{
							ee[i].Language_Translate();
						}
					}
				}
			}
			MenuI_File.Header = Program.Translations.GetLanguage("FileStr");
			MenuI_New.Header = Program.Translations.GetLanguage("New");
			MenuI_Open.Header = Program.Translations.GetLanguage("Open");
			MenuI_Save.Header = Program.Translations.GetLanguage("Save");
			MenuI_SaveAll.Header = Program.Translations.GetLanguage("SaveAll");
			MenuI_SaveAs.Header = Program.Translations.GetLanguage("SaveAs");
			MenuI_Close.Header = Program.Translations.GetLanguage("Close");
			MenuI_CloseAll.Header = Program.Translations.GetLanguage("CloseAll");

			MenuI_Edit.Header = Program.Translations.GetLanguage("Edit");
			MenuI_Undo.Header = Program.Translations.GetLanguage("Undo");
			MenuI_Redo.Header = Program.Translations.GetLanguage("Redo");
			MenuI_Cut.Header = Program.Translations.GetLanguage("Cut");
			MenuI_Copy.Header = Program.Translations.GetLanguage("Copy");
			MenuI_Paste.Header = Program.Translations.GetLanguage("Paste");
			MenuI_Folding.Header = Program.Translations.GetLanguage("Folding");
			MenuI_ExpandAll.Header = Program.Translations.GetLanguage("ExpandAll");
			MenuI_CollapseAll.Header = Program.Translations.GetLanguage("CollapseAll");
			MenuI_JumpTo.Header = Program.Translations.GetLanguage("JumpTo");
			MenuI_ToggleComment.Header = Program.Translations.GetLanguage("TogglComment");
			MenuI_SelectAll.Header = Program.Translations.GetLanguage("SelectAll");
			MenuI_FindReplace.Header = Program.Translations.GetLanguage("FindReplace");

			MenuI_Build.Header = Program.Translations.GetLanguage("Build");
			MenuI_CompileAll.Header = Program.Translations.GetLanguage("CompileAll");
			MenuI_Compile.Header = Program.Translations.GetLanguage("CompileCurr");
			MenuI_CopyPlugin.Header = Program.Translations.GetLanguage("CopyPlugin");
			MenuI_FTPUpload.Header = Program.Translations.GetLanguage("FTPUp");
			MenuI_StartServer.Header = Program.Translations.GetLanguage("StartServer");
			MenuI_SendRCon.Header = Program.Translations.GetLanguage("SendRCon");

			ConfigMenu.Header = Program.Translations.GetLanguage("Config");

			MenuI_Tools.Header = Program.Translations.GetLanguage("Tools");
			OptionMenuEntry.Header = Program.Translations.GetLanguage("Options");
			MenuI_ParsedIncDir.Header = Program.Translations.GetLanguage("ParsedIncDir");
			MenuI_OldApiWeb.Header = Program.Translations.GetLanguage("OldAPIWeb");
			MenuI_NewApiWeb.Header = Program.Translations.GetLanguage("NewAPIWeb");
			MenuI_Reformatter.Header = Program.Translations.GetLanguage("Reformatter");
			MenuI_ReformattCurr.Header = Program.Translations.GetLanguage("ReformatCurr");
			MenuI_ReformattAll.Header = Program.Translations.GetLanguage("ReformatAll");
			MenuI_Decompile.Header = $"{Program.Translations.GetLanguage("Decompile")}  .smx (Lysis)";
			MenuI_ReportBugGit.Header = Program.Translations.GetLanguage("ReportBugGit");
			UpdateCheckItem.Header = Program.Translations.GetLanguage("CheckUpdates");
			MenuI_About.Header = Program.Translations.GetLanguage("About");

			MenuC_FileName.Header = Program.Translations.GetLanguage("FileName");
			MenuC_Line.Header = Program.Translations.GetLanguage("Line");
			MenuC_Type.Header = Program.Translations.GetLanguage("TypeStr");
			MenuC_Details.Header = Program.Translations.GetLanguage("Details");

			NSearch_RButton.Content = Program.Translations.GetLanguage("NormalSearch");
			WSearch_RButton.Content = Program.Translations.GetLanguage("MatchWholeWords");
			ASearch_RButton.Content = $"{Program.Translations.GetLanguage("AdvancSearch")} (\\r, \\n, \\t, ...)";
			RSearch_RButton.Content = Program.Translations.GetLanguage("RegexSearch");
			MenuFR_CurrDoc.Content = Program.Translations.GetLanguage("CurrDoc");
			MenuFR_AllDoc.Content = Program.Translations.GetLanguage("AllDoc");

			Find_Button.Content = $"{Program.Translations.GetLanguage("Find")} (F3)";
			Count_Button.Content = Program.Translations.GetLanguage("Count");
			CCBox.Content = Program.Translations.GetLanguage("CaseSen");
			MLRBox.Content = Program.Translations.GetLanguage("MultilineRegex");

            OBItemText_File.Text = Program.Translations.GetLanguage("OBTextFile");
            OBItemText_Config.Text = Program.Translations.GetLanguage("OBTextConfig");
            OBItemText_Item.Text = Program.Translations.GetLanguage("OBTextItem");
		}
	}
}
