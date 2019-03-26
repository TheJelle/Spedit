using MahApps.Metro.Controls;
using SourcepawnCondenser.SourcemodDefinition;
using System.Text;
using System.Collections.Generic;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MahApps.Metro;

namespace Spedit.UI.Windows
{
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class SPDefinitionWindow : MetroWindow
    {
        SMDefinition def;

        SPDefEntry[] defArray;
        ListViewItem[] items;
        Timer searchTimer = new Timer(1000.0);

        public SPDefinitionWindow()
        {
            InitializeComponent();
			Language_Translate();
			if (Program.OptionsObject.Program_AccentColor != "Red" || Program.OptionsObject.Program_Theme != "BaseDark")
			{ ThemeManager.ChangeAppStyle(this, ThemeManager.GetAccent(Program.OptionsObject.Program_AccentColor), ThemeManager.GetAppTheme(Program.OptionsObject.Program_Theme)); }
			errorSearchBoxBrush.Freeze();
            def = Program.Configs[Program.SelectedConfig].GetSMDef();
            if (def == null)
            {
                MessageBox.Show(Program.Translations.GetLanguage("ConfigWrongPars"), Program.Translations.GetLanguage("Error"), MessageBoxButton.OK, MessageBoxImage.Warning);
                this.Close();
                return;
            }
            List<SPDefEntry> defList = new List<SPDefEntry>();
            for (int i = 0; i < def.Functions.Count; ++i) { defList.Add((SPDefEntry)def.Functions[i]); }
            for (int i = 0; i < def.Constants.Count; ++i) { defList.Add((SPDefEntry)def.Constants[i]); }
			for (int i = 0; i < def.Enums.Count; ++i) { defList.Add((SPDefEntry)def.Enums[i]); }
			for (int i = 0; i < def.Defines.Count; ++i) { defList.Add((SPDefEntry)def.Defines[i]); }
			for (int i = 0; i < def.Structs.Count; ++i) { defList.Add((SPDefEntry)def.Structs[i]); }
			for (int i = 0; i < def.Methodmaps.Count; ++i) { defList.Add((SPDefEntry)def.Methodmaps[i]); }
			for (int i = 0; i < def.Typedefs.Count; ++i) { defList.Add((SPDefEntry)def.Typedefs[i]); }
			foreach (var mm in def.Methodmaps)
			{
				for (int i = 0; i < mm.Methods.Count; ++i)
				{
					defList.Add((SPDefEntry)mm.Methods[i]);
				}
				for (int i = 0; i < mm.Fields.Count; ++i)
				{
					defList.Add((SPDefEntry)mm.Fields[i]);
				}
			}
			foreach (var e in defList)
			{
				if (string.IsNullOrWhiteSpace(e.Name))
				{
					e.Name = $"--{Program.Translations.GetLanguage("NoName")}--";
				}
			}
			defList.Sort((a, b) => { return string.Compare(a.Name, b.Name); });
            defArray = defList.ToArray();
            int defArrayLength = defArray.Length;
            items = new ListViewItem[defArrayLength];
            for (int i = 0; i < defArrayLength; ++i)
            {
                items[i] = new ListViewItem() { Content = defArray[i].Name, Tag = defArray[i].Entry };
                SPBox.Items.Add(items[i]);
            }
            searchTimer.Elapsed += searchTimer_Elapsed;
        }

        void searchTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            DoSearch();
			searchTimer.Stop();
        }

        private void SPFunctionsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            object obj = SPBox.SelectedItem;
            if (obj == null) { return; }
            ListViewItem item = (ListViewItem)obj;
            object TagValue = item.Tag;
            if (TagValue != null)
            {
                if (TagValue is SMFunction)
                {
                    var sm = (SMFunction)TagValue;
                    SPNameBlock.Text = sm.Name;
                    SPFullNameBlock.Text = sm.FullName;
					SPFileBlock.Text = sm.File + ".inc" + $" ({string.Format(Program.Translations.GetLanguage("PosLen"), sm.Index, sm.Length)})";
					SPTypeBlock.Text = "Function";
					SPCommentBox.Text = sm.CommentString;
                    return;
				}
				else if (TagValue is SMConstant)
				{
					var sm = (SMConstant)TagValue;
					SPNameBlock.Text = sm.Name;
					SPFullNameBlock.Text = string.Empty;
					SPFileBlock.Text = sm.File + ".inc" + $" ({string.Format(Program.Translations.GetLanguage("PosLen"), sm.Index, sm.Length)})";
					SPTypeBlock.Text = "Constant";
					SPCommentBox.Text = string.Empty;
					return;
				}
				else if (TagValue is SMEnum)
				{
					var sm = (SMEnum)TagValue;
					SPNameBlock.Text = sm.Name;
					SPFullNameBlock.Text = string.Empty;
					SPFileBlock.Text = sm.File + ".inc" + $" ({string.Format(Program.Translations.GetLanguage("PosLen"), sm.Index, sm.Length)})";
					SPTypeBlock.Text = "Enum " + sm.Entries.Length.ToString() + " entries";
					StringBuilder outString = new StringBuilder();
					for (int i = 0; i < sm.Entries.Length; ++i)
					{
						outString.Append((i.ToString() + ".").PadRight(5, ' '));
						outString.AppendLine(sm.Entries[i]);
					}
					SPCommentBox.Text = outString.ToString();
					return;
				}
				else if (TagValue is SMStruct)
				{
					var sm = (SMStruct)TagValue;
					SPNameBlock.Text = sm.Name;
					SPFullNameBlock.Text = string.Empty;
					SPFileBlock.Text = sm.File + ".inc" + $" ({string.Format(Program.Translations.GetLanguage("PosLen"), sm.Index, sm.Length)})";
					SPTypeBlock.Text = "Struct";
					SPCommentBox.Text = string.Empty;
					return;
				}
				else if (TagValue is SMDefine)
				{
					var sm = (SMDefine)TagValue;
					SPNameBlock.Text = sm.Name;
					SPFullNameBlock.Text = string.Empty;
					SPFileBlock.Text = sm.File + ".inc" + $" ({string.Format(Program.Translations.GetLanguage("PosLen"), sm.Index, sm.Length)})";
					SPTypeBlock.Text = "Definition";
					SPCommentBox.Text = string.Empty;
					return;
				}
				else if (TagValue is SMMethodmap)
				{
					var sm = (SMMethodmap)TagValue;
					SPNameBlock.Text = sm.Name;
					SPFullNameBlock.Text = $"{Program.Translations.GetLanguage("TypeStr")}: " + sm.Type + $" - {Program.Translations.GetLanguage("InheritedFrom")}: {sm.InheritedType}";
					SPFileBlock.Text = sm.File + ".inc" + $" ({string.Format(Program.Translations.GetLanguage("PosLen"), sm.Index, sm.Length)})";
					SPTypeBlock.Text = "Methodmap " + sm.Methods.Count.ToString() + " methods - " + sm.Fields.Count.ToString() + " fields";
					StringBuilder outString = new StringBuilder();
					outString.AppendLine("Methods:");
					foreach (var m in sm.Methods)
					{
						outString.AppendLine(m.FullName);
					}
					outString.AppendLine();
					outString.AppendLine("Fields:");
					foreach (var f in sm.Fields)
					{
						outString.AppendLine(f.FullName);
					}
					SPCommentBox.Text = outString.ToString();
					return;
				}
				else if (TagValue is SMMethodmapMethod)
				{
					var sm = (SMMethodmapMethod)TagValue;
					SPNameBlock.Text = sm.Name;
					SPFullNameBlock.Text = sm.FullName;
					SPFileBlock.Text = sm.File + ".inc" + $" ({string.Format(Program.Translations.GetLanguage("PosLen"), sm.Index, sm.Length)})";
					SPTypeBlock.Text = $"{Program.Translations.GetLanguage("MethodFrom")} {sm.MethodmapName}";
					SPCommentBox.Text = sm.CommentString;
					return;
				}
				else if (TagValue is SMMethodmapField)
				{
					var sm = (SMMethodmapField)TagValue;
					SPNameBlock.Text = sm.Name;
					SPFullNameBlock.Text = sm.FullName;
					SPFileBlock.Text = sm.File + ".inc" + $" ({string.Format(Program.Translations.GetLanguage("PosLen"), sm.Index, sm.Length)})";
					SPTypeBlock.Text = $"{Program.Translations.GetLanguage("PropertyFrom")} {sm.MethodmapName}";
					SPCommentBox.Text = string.Empty;
					return;
				}
				else if (TagValue is SMTypedef)
				{
					var sm = (SMTypedef)TagValue;
					SPNameBlock.Text = sm.Name;
					SPFullNameBlock.Text = string.Empty;
					SPFileBlock.Text = sm.File + ".inc" + $" ({string.Format(Program.Translations.GetLanguage("PosLen"), sm.Index, sm.Length)})";
					SPTypeBlock.Text = "Typedef/Typeset";
					SPCommentBox.Text = sm.FullName;
					return;
				}
				else if (TagValue is string)
                {
                    SPNameBlock.Text = (string)item.Content;
                    SPFullNameBlock.Text = (string)TagValue;
					SPFileBlock.Text = string.Empty;
					SPCommentBox.Text = string.Empty;
                    return;
                }
            }
            SPNameBlock.Text = (string)item.Content;
            SPFullNameBlock.Text = string.Empty;
			SPFileBlock.Text = string.Empty;
			SPTypeBlock.Text = string.Empty;
			SPCommentBox.Text = string.Empty;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            SPProgress.IsIndeterminate = true;
            searchTimer.Stop();
            searchTimer.Start();
        }

		Brush errorSearchBoxBrush = new SolidColorBrush(Color.FromArgb(0x50, 0xA0, 0x30, 0));
        private void DoSearch()
        {
            this.Dispatcher.Invoke(() =>
                {
                    int itemCount = defArray.Length;
                    string searchString = SPSearchBox.Text.ToLowerInvariant();
                    bool foundOccurence = false;
                    SPBox.Items.Clear();
                    for (int i = 0; i < itemCount; ++i)
                    {
                        if (defArray[i].Name.ToLowerInvariant().Contains(searchString))
                        {
                            foundOccurence = true;
                            SPBox.Items.Add(items[i]);
                        }
                    }
                    if (foundOccurence)
                    {
                        SPSearchBox.Background = Brushes.Transparent;
                    }
                    else
                    {
                        SPSearchBox.Background = errorSearchBoxBrush;
                    }
                    SPProgress.IsIndeterminate = false;
                });
        }

		private void Language_Translate()
		{
			TextBoxHelper.SetWatermark(SPSearchBox, Program.Translations.GetLanguage("Search"));
			/*if (Program.Translations.GetLanguage("IsDefault)
			{
				return;
			}*/
		}

		private class SPDefEntry
        {
            public string Name;
            public object Entry;

			public static explicit operator SPDefEntry(SMFunction func)
			{
				return new SPDefEntry() { Name = func.Name, Entry = func };
			}
			public static explicit operator SPDefEntry(SMConstant sm)
			{
				return new SPDefEntry() { Name = sm.Name, Entry = sm };
			}
			public static explicit operator SPDefEntry(SMDefine sm)
			{
				return new SPDefEntry() { Name = sm.Name, Entry = sm };
			}
			public static explicit operator SPDefEntry(SMEnum sm)
			{
				return new SPDefEntry() { Name = sm.Name, Entry = sm };
			}
			public static explicit operator SPDefEntry(SMStruct sm)
			{
				return new SPDefEntry() { Name = sm.Name, Entry = sm };
			}
			public static explicit operator SPDefEntry(SMMethodmap sm)
			{
				return new SPDefEntry() { Name = sm.Name, Entry = sm };
			}
			public static explicit operator SPDefEntry(SMMethodmapMethod sm)
			{
				return new SPDefEntry() { Name = sm.Name, Entry = sm };
			}
			public static explicit operator SPDefEntry(SMMethodmapField sm)
			{
				return new SPDefEntry() { Name = sm.Name, Entry = sm };
			}
			public static explicit operator SPDefEntry(SMTypedef sm)
			{
				return new SPDefEntry() { Name = sm.Name, Entry = sm };
			}
		}
    }
}
