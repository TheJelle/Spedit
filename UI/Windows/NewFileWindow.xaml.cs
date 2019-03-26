using MahApps.Metro.Controls;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml;
using MahApps.Metro;

namespace Spedit.UI.Windows
{
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class NewFileWindow : MetroWindow
    {
        string PathStr = "sourcepawn\\scripts";
        Dictionary<string, TemplateInfo> TemplateDictionary;
        public NewFileWindow()
        {
            InitializeComponent();
			Language_Translate();
			if (Program.OptionsObject.Program_AccentColor != "Red" || Program.OptionsObject.Program_Theme != "BaseDark")
			{ ThemeManager.ChangeAppStyle(this, ThemeManager.GetAccent(Program.OptionsObject.Program_AccentColor), ThemeManager.GetAppTheme(Program.OptionsObject.Program_Theme)); }
			ParseTemplateFile();
            TemplateListBox.SelectedIndex = 0;
        }

        private void ParseTemplateFile()
        {
            TemplateDictionary = new Dictionary<string, TemplateInfo>();
            if (File.Exists("sourcepawn\\templates\\Templates.xml"))
            {
                using (Stream stream = File.OpenRead("sourcepawn\\templates\\Templates.xml"))
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(stream);
                    if (doc.ChildNodes.Count > 0)
                    {
                        if (doc.ChildNodes[0].Name == "Templates")
                        {
                            XmlNode mainNode = doc.ChildNodes[0];
                            for (int i = 0; i < mainNode.ChildNodes.Count; ++i)
                            {
                                if (mainNode.ChildNodes[i].Name == "Template")
                                {
                                    XmlAttributeCollection attributes = mainNode.ChildNodes[i].Attributes;
                                    string NameStr = attributes["Name"].Value;
                                    string FileNameStr = attributes["File"].Value;
                                    string NewNameStr = attributes["NewName"].Value;
                                    string FilePathStr = Path.Combine("sourcepawn\\templates\\", FileNameStr);
                                    if (File.Exists(FilePathStr))
                                    {
                                        TemplateDictionary.Add(NameStr, new TemplateInfo() { Name = NameStr, FileName = FileNameStr, Path = FilePathStr, NewName = NewNameStr });
                                        TemplateListBox.Items.Add(NameStr);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void TemplateListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TemplateInfo templateInfo = TemplateDictionary[(string)TemplateListBox.SelectedItem];
            PrevieBox.Text = File.ReadAllText(templateInfo.Path);
            PathBox.Text = Path.Combine(PathStr, templateInfo.NewName);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            FileInfo destFile = new FileInfo(PathBox.Text);
            TemplateInfo templateInfo = TemplateDictionary[(string)TemplateListBox.SelectedItem];
            File.Copy(templateInfo.Path, destFile.FullName, true);
            Program.MainWindow.TryLoadSourceFile(destFile.FullName, true, true, true);
            this.Close();
        }

		private void Language_Translate()
		{
			if (Program.Translations.IsDefault)
			{
				return;
			}
			PreviewBlock.Text = $"{Program.Translations.GetLanguage("Preview")}:";
			SaveButton.Content = Program.Translations.GetLanguage("Save");
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
                            var dialog = new SaveFileDialog();
                            dialog.AddExtension = true;
                            dialog.Filter = "Sourcepawn Files (*.sp *.inc)|*.sp;*.inc|All Files (*.*)|*.*";
                            dialog.OverwritePrompt = true;
                            dialog.Title = Program.Translations.GetLanguage("NewFile");
                            var result = dialog.ShowDialog();
                            if (result.Value)
                            {
                                ((TextBox)o).Text = dialog.FileName;
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

    public class TemplateInfo
    {
        public string Name;
        public string FileName;
        public string Path;
        public string NewName;
    }
}
