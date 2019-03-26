using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Spedit.UI.Components;
using Spedit.UI;
using System.Windows;
using System.Windows.Media;
using MahApps.Metro;
using System;

namespace Spedit.UI.Windows
{
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class OptionsWindow : MetroWindow
    {
		string[] AvailableAccents = { "Red", "Green", "Blue", "Purple", "Orange", "Lime", "Emerald", "Teal", "Cyan", "Cobalt", "Indigo", "Violet", "Pink", "Magenta", "Crimson", "Amber",
			"Yellow", "Brown", "Olive", "Steel", "Mauve", "Taupe", "Sienna" };
        bool RestartTextIsShown = false;
        bool AllowChanging = false;
        public OptionsWindow()
        {
            InitializeComponent();
			Language_Translate();
			if (Program.OptionsObject.Program_AccentColor != "Red" || Program.OptionsObject.Program_Theme != "BaseDark")
			{ ThemeManager.ChangeAppStyle(this, ThemeManager.GetAccent(Program.OptionsObject.Program_AccentColor), ThemeManager.GetAppTheme(Program.OptionsObject.Program_Theme)); }
			LoadSettings();
            AllowChanging = true;
        }

        private async void RestoreButton_Clicked(object sender, RoutedEventArgs e)
        {
            var result = await this.ShowMessageAsync(Program.Translations.GetLanguage("ResetOptions"), Program.Translations.GetLanguage("ResetOptQues"), MessageDialogStyle.AffirmativeAndNegative);
            if (result == MessageDialogResult.Affirmative)
            {
                Program.OptionsObject = new OptionsControl();
				Program.OptionsObject.ReCreateCryptoKey();
                Program.MainWindow.OptionMenuEntry.IsEnabled = false;
                await this.ShowMessageAsync(Program.Translations.GetLanguage("RestartEditor"), Program.Translations.GetLanguage("YRestartEditor"), MessageDialogStyle.Affirmative, this.MetroDialogOptions);
                this.Close();
            }
        }

        private void HardwareAcc_Changed(object sender, RoutedEventArgs e)
        {
            if (!AllowChanging) { return; }
            Program.OptionsObject.Program_UseHardwareAcceleration = HardwareAcc.IsChecked.Value;
            ToggleRestartText();
        }

        private void UIAnimation_Changed(object sender, RoutedEventArgs e)
        {
            if (!AllowChanging) { return; }
            Program.OptionsObject.UI_Animations = UIAnimation.IsChecked.Value;
            ToggleRestartText();
        }

        private void AutoOpenInclude_Changed(object sender, RoutedEventArgs e)
        {
            if (!AllowChanging) { return; }
            Program.OptionsObject.Program_OpenCustomIncludes = OpenIncludes.IsChecked.Value;
            OpenIncludesRecursive.IsEnabled = OpenIncludes.IsChecked.Value;
        }
        private void OpenIncludeRecursively_Changed(object sender, RoutedEventArgs e)
        {
            if (!AllowChanging) { return; }
            Program.OptionsObject.Program_OpenIncludesRecursively = OpenIncludesRecursive.IsChecked.Value;
        }

        private void AutoUpdate_Changed(object sender, RoutedEventArgs e)
        {
            if (!AllowChanging) { return; }
            Program.OptionsObject.Program_CheckForUpdates = AutoUpdate.IsChecked.Value;
        }

        private void ShowToolbar_Changed(object sender, RoutedEventArgs e)
        {
            if (!AllowChanging) { return; }
            Program.OptionsObject.UI_ShowToolBar = ShowToolBar.IsChecked.Value;
            if (Program.OptionsObject.UI_ShowToolBar)
            {
                Program.MainWindow.Win_ToolBar.Height = double.NaN;
            }
            else
            {
                Program.MainWindow.Win_ToolBar.Height = 0.0;
            }
        }

		private void DynamicISAC_Changed(object sender, RoutedEventArgs e)
		{
			if (!AllowChanging) { return; }
			Program.OptionsObject.Program_DynamicISAC = DynamicISAC.IsChecked.Value;
		}

		private void DarkTheme_Changed(object sender, RoutedEventArgs e)
		{
			if (!AllowChanging) { return; }
			if (DarkTheme.IsChecked.Value)
			{ Program.OptionsObject.Program_Theme = "BaseDark"; }
			else
			{ Program.OptionsObject.Program_Theme = "BaseLight"; }
			ThemeManager.ChangeAppStyle(this, ThemeManager.GetAccent(Program.OptionsObject.Program_AccentColor), ThemeManager.GetAppTheme(Program.OptionsObject.Program_Theme));
			ThemeManager.ChangeAppStyle(Program.MainWindow, ThemeManager.GetAccent(Program.OptionsObject.Program_AccentColor), ThemeManager.GetAppTheme(Program.OptionsObject.Program_Theme));
			ToggleRestartText(true);
		}

		private void AccentColor_Changed(object sender, RoutedEventArgs e)
		{
			if (!AllowChanging) { return; }
			Program.OptionsObject.Program_AccentColor = (string)AccentColor.SelectedItem;
			ThemeManager.ChangeAppStyle(this, ThemeManager.GetAccent(Program.OptionsObject.Program_AccentColor), ThemeManager.GetAppTheme(Program.OptionsObject.Program_Theme));
			ThemeManager.ChangeAppStyle(Program.MainWindow, ThemeManager.GetAccent(Program.OptionsObject.Program_AccentColor), ThemeManager.GetAppTheme(Program.OptionsObject.Program_Theme));
		}

		private void FontSize_Changed(object sender, RoutedEventArgs e)
        {
            if (!AllowChanging) { return; }
            double size = FontSizeD.Value;
            Program.OptionsObject.Editor_FontSize = size;
            EditorElement[] editors = Program.MainWindow.GetAllEditorElements();
			if (editors != null)
			{
				for (int i = 0; i < editors.Length; ++i)
				{
					editors[i].UpdateFontSize(size);
				}
			}
        }

        private void ScrollSpeed_Changed(object sender, RoutedEventArgs e)
        {
            if (!AllowChanging) { return; }
            Program.OptionsObject.Editor_ScrollLines = ScrollSpeed.Value;
        }

        private void WordWrap_Changed(object sender, RoutedEventArgs e)
        {
            if (!AllowChanging) { return; }
            bool wrapping = WordWrap.IsChecked.Value;
            Program.OptionsObject.Editor_WordWrap = wrapping;
            EditorElement[] editors = Program.MainWindow.GetAllEditorElements();
			if (editors != null)
			{
				for (int i = 0; i < editors.Length; ++i)
				{
					editors[i].editor.WordWrap = wrapping;
				}
			}
        }

        private void AIndentation_Changed(object sender, RoutedEventArgs e)
        {
            if (!AllowChanging) { return; }
            Program.OptionsObject.Editor_AgressiveIndentation = AgressiveIndentation.IsChecked.Value;
        }

        private void LineReformat_Changed(object sender, RoutedEventArgs e)
        {
            if (!AllowChanging) { return; }
            Program.OptionsObject.Editor_ReformatLineAfterSemicolon = LineReformatting.IsChecked.Value;
        }

        private void TabToSpace_Changed(object sender, RoutedEventArgs e)
        {
            if (!AllowChanging) { return; }
            bool replaceTabs = TabToSpace.IsChecked.Value;
            Program.OptionsObject.Editor_ReplaceTabsToWhitespace = replaceTabs;
            EditorElement[] editors = Program.MainWindow.GetAllEditorElements();
            if (editors != null)
            {
                for (int i = 0; i < editors.Length; ++i)
                {
                    editors[i].editor.Options.ConvertTabsToSpaces = replaceTabs;
                }
            }
        }

		private void AutoCloseBrackets_Changed(object sender, RoutedEventArgs e)
		{
			if (!AllowChanging) { return; }
			Program.OptionsObject.Editor_AutoCloseBrackets = AutoCloseBrackets.IsChecked.Value;
		}

		private void AutoCloseStringChars_Changed(object sender, RoutedEventArgs e)
		{
			if (!AllowChanging) { return; }
			Program.OptionsObject.Editor_AutoCloseStringChars = AutoCloseStringChars.IsChecked.Value;
		}

		private void ShowSpaces_Changed(object sender, RoutedEventArgs e)
		{
			if (!AllowChanging) { return; }
			bool showSpacesValue = Program.OptionsObject.Editor_ShowSpaces = ShowSpaces.IsChecked.Value;
			EditorElement[] editors = Program.MainWindow.GetAllEditorElements();
			if (editors != null)
			{
				for (int i = 0; i < editors.Length; ++i)
				{
					editors[i].editor.Options.ShowSpaces = showSpacesValue;
				}
			}
		}

		private void ShowTabs_Changed(object sender, RoutedEventArgs e)
		{
			if (!AllowChanging) { return; }
			bool showTabsValue = Program.OptionsObject.Editor_ShowTabs = ShowTabs.IsChecked.Value;
			EditorElement[] editors = Program.MainWindow.GetAllEditorElements();
			if (editors != null)
			{
				for (int i = 0; i < editors.Length; ++i)
				{
					editors[i].editor.Options.ShowTabs = showTabsValue;
				}
			}
		}

		private void FontFamily_Changed(object sender, RoutedEventArgs e)
        {
            if (!AllowChanging) { return; }
            FontFamily family = (FontFamily)FontFamilyCB.SelectedItem;
            string FamilyString = family.Source;
            Program.OptionsObject.Editor_FontFamily = FamilyString;
            FontFamilyTB.Text = "Font (" + FamilyString + "):";
            EditorElement[] editors = Program.MainWindow.GetAllEditorElements();
			if (editors != null)
			{
				for (int i = 0; i < editors.Length; ++i)
				{
					editors[i].editor.FontFamily = family;
				}
			}
        }

		private void IndentationSize_Changed(object sender, RoutedEventArgs e)
		{
			if (!AllowChanging) { return; }
			int indentationSizeValue = Program.OptionsObject.Editor_IndentationSize = (int)System.Math.Round(IndentationSize.Value);
			EditorElement[] editors = Program.MainWindow.GetAllEditorElements();
			if (editors != null)
			{
				for (int i = 0; i < editors.Length; ++i)
				{
					editors[i].editor.Options.IndentationSize = indentationSizeValue;
				}
			}
		}


		private void HighlightDeprecateds_Changed(object sender, RoutedEventArgs e)
        {
            if (!AllowChanging) { return; }
            Program.OptionsObject.SH_HighlightDeprecateds = HighlightDeprecateds.IsChecked.Value;
            ToggleRestartText();
        }

		private void LanguageBox_Changed(object sender, RoutedEventArgs e)
		{
			if (!AllowChanging) { return; }
			string selectedString = (string)LanguageBox.SelectedItem;
			for (int i = 0; i < Program.Translations.AvailableLanguages.Length; ++i)
			{
				if (Program.Translations.AvailableLanguages[i] == selectedString)
				{
					Program.Translations.LoadLanguage(Program.Translations.AvailableLanguageIDs[i]);
					Program.OptionsObject.Language = Program.Translations.AvailableLanguageIDs[i];
					Program.MainWindow.Language_Translate();
					break;
				}
			}
			Language_Translate();
			ToggleRestartText(true);
		}

		private void HardwareSalts_Changed(object sender, RoutedEventArgs e)
		{
			if (!AllowChanging) { return; }
			Program.OptionsObject.Program_UseHardwareSalts = HardwareSalts.IsChecked.Value;
			Program.RCCKMade = false;
			Program.OptionsObject.ReCreateCryptoKey();
			Program.MakeRCCKAlert();
		}

		private void AutoSave_Changed(object sender, RoutedEventArgs e)
		{
			if (!AllowChanging) { return; }
			int newIndex = AutoSave.SelectedIndex;
			EditorElement[] editors = Program.MainWindow.GetAllEditorElements();
			if (newIndex == 0)
			{
				Program.OptionsObject.Editor_AutoSave = false;
				if (editors != null)
				{
					for (int i = 0; i < editors.Length; ++i)
					{
						if (editors[i].AutoSaveTimer.Enabled)
						{
							editors[i].AutoSaveTimer.Stop();
						}
					}
				}
			}
			else
			{
				Program.OptionsObject.Editor_AutoSave = true;
				if (newIndex == 1)
					Program.OptionsObject.Editor_AutoSaveInterval = 30;
				else if (newIndex == 7)
					Program.OptionsObject.Editor_AutoSaveInterval = 600;
				else if (newIndex == 8)
					Program.OptionsObject.Editor_AutoSaveInterval = 900;
				else
					Program.OptionsObject.Editor_AutoSaveInterval = (newIndex - 1) * 60;
				if (editors != null)
				{
					for (int i = 0; i < editors.Length; ++i)
					{
						editors[i].StartAutoSaveTimer();
					}
				}
			}
		}

		private void LoadSettings()
        {
			for (int i = 0; i < AvailableAccents.Length; ++i)
			{
				AccentColor.Items.Add(AvailableAccents[i]);
			}
            HardwareAcc.IsChecked = Program.OptionsObject.Program_UseHardwareAcceleration;
            UIAnimation.IsChecked = Program.OptionsObject.UI_Animations;
            OpenIncludes.IsChecked = Program.OptionsObject.Program_OpenCustomIncludes;
            OpenIncludesRecursive.IsChecked = Program.OptionsObject.Program_OpenIncludesRecursively;
            AutoUpdate.IsChecked = Program.OptionsObject.Program_CheckForUpdates;
            if (!Program.OptionsObject.Program_OpenCustomIncludes)
            {
                OpenIncludesRecursive.IsEnabled = false;
            }
            ShowToolBar.IsChecked = Program.OptionsObject.UI_ShowToolBar;
			DynamicISAC.IsChecked = Program.OptionsObject.Program_DynamicISAC;
			DarkTheme.IsChecked = (Program.OptionsObject.Program_Theme == "BaseDark");
			for (int i = 0; i < AvailableAccents.Length; ++i)
			{
				if (AvailableAccents[i] == Program.OptionsObject.Program_AccentColor)
				{
					AccentColor.SelectedIndex = i;
					break;
				}
			}
			for (int i = 0; i < Program.Translations.AvailableLanguages.Length; ++i)
			{
				LanguageBox.Items.Add(Program.Translations.AvailableLanguages[i]);
				if (Program.OptionsObject.Language == Program.Translations.AvailableLanguageIDs[i])
				{
					LanguageBox.SelectedIndex = i;
				}
			}
			if (Program.OptionsObject.Editor_AutoSave)
			{
				int seconds = Program.OptionsObject.Editor_AutoSaveInterval;
				if (seconds < 60)
					AutoSave.SelectedIndex = 1;
				else if (seconds <= 300)
					AutoSave.SelectedIndex = Math.Max(1, Math.Min(seconds / 60, 5)) + 1;
				else if (seconds == 600)
					AutoSave.SelectedIndex = 7;
				else
					AutoSave.SelectedIndex = 8;
			}
			else
			{
				AutoSave.SelectedIndex = 0;
			}
            HighlightDeprecateds.IsChecked = Program.OptionsObject.SH_HighlightDeprecateds;
            FontSizeD.Value = Program.OptionsObject.Editor_FontSize;
            ScrollSpeed.Value = Program.OptionsObject.Editor_ScrollLines;
            WordWrap.IsChecked = Program.OptionsObject.Editor_WordWrap;
            AgressiveIndentation.IsChecked = Program.OptionsObject.Editor_AgressiveIndentation;
            LineReformatting.IsChecked = Program.OptionsObject.Editor_ReformatLineAfterSemicolon;
            TabToSpace.IsChecked = Program.OptionsObject.Editor_ReplaceTabsToWhitespace;
			AutoCloseBrackets.IsChecked = Program.OptionsObject.Editor_AutoCloseBrackets;
			AutoCloseStringChars.IsChecked = Program.OptionsObject.Editor_AutoCloseStringChars;
			ShowSpaces.IsChecked = Program.OptionsObject.Editor_ShowSpaces;
			ShowTabs.IsChecked = Program.OptionsObject.Editor_ShowTabs;
			FontFamilyTB.Text = $"{Program.Translations.GetLanguage("FontFamily")} ({Program.OptionsObject.Editor_FontFamily}):";
            FontFamilyCB.SelectedValue = new FontFamily(Program.OptionsObject.Editor_FontFamily);
			IndentationSize.Value = (double)Program.OptionsObject.Editor_IndentationSize;
			HardwareSalts.IsChecked = Program.OptionsObject.Program_UseHardwareSalts;
            LoadSH();
        }

        private void ToggleRestartText(bool FullEffect = false)
        {
            if (AllowChanging)
            {
                if (!RestartTextIsShown)
                {
                    StatusTextBlock.Content = (FullEffect) ? Program.Translations.GetLanguage("RestartEdiFullEff") : Program.Translations.GetLanguage("RestartEdiEff");
                    RestartTextIsShown = true;
                }
            }
        }

        private void Language_Translate()
        {
            if (Program.Translations.IsDefault)
            {
                return;
            }
            ResetButton.Content = Program.Translations.GetLanguage("ResetOptions");
            ProgramHeader.Header = $" {Program.Translations.GetLanguage("Program")}";

            HardwareAcc.Content = Program.Translations.GetLanguage("HardwareAcc");
            UIAnimation.Content = Program.Translations.GetLanguage("UIAnim");
            OpenIncludes.Content = Program.Translations.GetLanguage("OpenInc");
            OpenIncludesRecursive.Content = Program.Translations.GetLanguage("OpenIncRec");
            AutoUpdate.Content = Program.Translations.GetLanguage("AutoUpdate");
            ShowToolBar.Content = Program.Translations.GetLanguage("ShowToolbar");
            DynamicISAC.Content = Program.Translations.GetLanguage("DynamicISAC");
            DarkTheme.Content = Program.Translations.GetLanguage("DarkTheme");
            ThemeColorLabel.Content = Program.Translations.GetLanguage("ThemeColor");
            LanguageLabel.Content = Program.Translations.GetLanguage("LanguageStr");
            EditorHeader.Header = $" {Program.Translations.GetLanguage("Editor")}";
            FontSizeBlock.Text = Program.Translations.GetLanguage("FontSize");
            ScrollSpeedBlock.Text = Program.Translations.GetLanguage("ScrollSpeed");
            WordWrap.Content = Program.Translations.GetLanguage("WordWrap");
            AgressiveIndentation.Content = Program.Translations.GetLanguage("AggIndentation");
            LineReformatting.Content = Program.Translations.GetLanguage("ReformatAfterSem");
            TabToSpace.Content = Program.Translations.GetLanguage("TabsToSpace");
            AutoCloseBrackets.Content = $"{Program.Translations.GetLanguage("AutoCloseBrack")} (), [], {{}}";
            AutoCloseStringChars.Content = $"{Program.Translations.GetLanguage("AutoCloseStrChr")} \"\", ''";
			ShowSpaces.Content = Program.Translations.GetLanguage("ShowSpaces");
			ShowTabs.Content = Program.Translations.GetLanguage("ShowTabs");
			IndentationSizeBlock.Text = Program.Translations.GetLanguage("IndentationSize");
			SyntaxHighBlock.Text = Program.Translations.GetLanguage("SyntaxHigh");
			HighlightDeprecateds.Content = Program.Translations.GetLanguage("HighDeprecat");
			AutoSaveBlock.Text = Program.Translations.GetLanguage("AutoSaveMin");
		}
    }
}
