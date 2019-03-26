using MahApps.Metro;
using MahApps.Metro.Controls;
using Spedit.UI.Components;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Xceed.Wpf.AvalonDock.Layout;
using Spedit.Interop.Updater; //not delete!

namespace Spedit.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public List<EditorElement> EditorsReferences = new List<EditorElement>();

        Storyboard BlendOverEffect;
        Storyboard FadeFindReplaceGridIn;
        Storyboard FadeFindReplaceGridOut;
        Storyboard EnableServerAnim;
        Storyboard DisableServerAnim;

		private bool FullyInitialized = false;

        public MainWindow()
        {
            InitializeComponent();
        }
        public MainWindow(SplashScreen sc)
        {
            InitializeComponent();
			if (Program.OptionsObject.Program_AccentColor != "Red" || Program.OptionsObject.Program_Theme != "BaseDark")
			{ ThemeManager.ChangeAppStyle(this, ThemeManager.GetAccent(Program.OptionsObject.Program_AccentColor), ThemeManager.GetAppTheme(Program.OptionsObject.Program_Theme)); }
			ObjectBrowserColumn.Width = new GridLength(Program.OptionsObject.Program_ObjectbrowserWidth, GridUnitType.Pixel);
			var heightDescriptor = DependencyPropertyDescriptor.FromProperty(ColumnDefinition.WidthProperty, typeof(ItemsControl));
			heightDescriptor.AddValueChanged(EditorObjectBrowserGrid.ColumnDefinitions[1], EditorObjectBrowserGridRow_WidthChanged);
			FillConfigMenu();
            CompileButton.ItemsSource = compileButtonDict;
			CompileButton.SelectedIndex = 0;
            CActionButton.ItemsSource = actionButtonDict;
			CActionButton.SelectedIndex = 0;
            ReplaceButton.ItemsSource = findReplaceButtonDict;
			ReplaceButton.SelectedIndex = 0;
            if (Program.OptionsObject.UI_ShowToolBar)
            {
                Win_ToolBar.Height = double.NaN;
            }
            this.MetroDialogOptions.AnimateHide = this.MetroDialogOptions.AnimateShow = false;
            BlendOverEffect = (Storyboard)this.Resources["BlendOverEffect"];
            FadeFindReplaceGridIn = (Storyboard)this.Resources["FadeFindReplaceGridIn"];
            FadeFindReplaceGridOut = (Storyboard)this.Resources["FadeFindReplaceGridOut"];
            EnableServerAnim = (Storyboard)this.Resources["EnableServerAnim"];
            DisableServerAnim = (Storyboard)this.Resources["DisableServerAnim"];
			ChangeObjectBrowserToDirectory(Program.OptionsObject.Program_ObjectBrowserDirectory);
			Language_Translate(true);
#if DEBUG
            TryLoadSourceFile(@"C:\Users\Jelle\Desktop\scripting\AeroControler.sp", false);
#endif
            if (Program.OptionsObject.LastOpenFiles != null)
            {
                for (int i = 0; i < Program.OptionsObject.LastOpenFiles.Length; ++i)
                {
                    TryLoadSourceFile(Program.OptionsObject.LastOpenFiles[i], false, true, false);
                }
            }
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; ++i)
            {
                if (!args[i].EndsWith("exe"))
                {
                    TryLoadSourceFile(args[i], false, true, (i == 0));
                }
            }
            sc.Close(TimeSpan.FromMilliseconds(500.0));
			StartBackgroundParserThread();
			FullyInitialized = true;
		}

        public bool TryLoadSourceFile(string filePath, bool UseBlendoverEffect = true, bool TryOpenIncludes = true, bool SelectMe = false)
        {
            FileInfo fileInfo = new FileInfo(filePath);
            if (fileInfo.Exists)
            {
                string extension = fileInfo.Extension.ToLowerInvariant().Trim(new char[] { '.', ' ' });
                if (extension == "sp" || extension == "inc" || extension == "txt" || extension == "cfg" || extension == "ini")
                {
                    string finalPath = fileInfo.FullName;
                    try
                    {
                        File.GetAccessControl(finalPath);
                    }
                    catch (UnauthorizedAccessException)
                    {
                        return false;
                    }
                    EditorElement[] editors = GetAllEditorElements();
                    if (editors != null)
                    {
                        for (int i = 0; i < editors.Length; ++i)
                        {
                            if (editors[i].FullFilePath == finalPath)
                            {
								if (SelectMe)
								{
									editors[i].Parent.IsSelected = true;
								}
                                return true;
                            }
                        }
                    }
                    AddEditorElement(finalPath, fileInfo.Name, SelectMe);
                    if (TryOpenIncludes && Program.OptionsObject.Program_OpenCustomIncludes)
                    {
                        using (var textReader = fileInfo.OpenText())
                        {
                            string source = Regex.Replace(textReader.ReadToEnd(), @"/\*.*?\*/", string.Empty, RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.Singleline);
                            Regex regex = new Regex(@"^\s*\#include\s+((\<|"")(?<name>.+?)(\>|""))", RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.Multiline);
                            MatchCollection mc = regex.Matches(source);
                            for (int i = 0; i < mc.Count; ++i)
                            {
                                try
                                {
                                    string fileName = mc[i].Groups["name"].Value;
                                    if (!(fileName.EndsWith(".inc", StringComparison.InvariantCultureIgnoreCase) || fileName.EndsWith(".sp", StringComparison.InvariantCultureIgnoreCase)))
                                    {
                                        fileName = fileName + ".inc";
                                    }
                                    fileName = System.IO.Path.Combine(fileInfo.DirectoryName, fileName);
                                    TryLoadSourceFile(fileName, false, Program.OptionsObject.Program_OpenIncludesRecursively);
                                }
                                catch (Exception) { }
                            }
                        }
                    }
                }
                else if (extension == "smx")
                {
                    LayoutDocument layoutDocument = new LayoutDocument();
                    layoutDocument.Title = "DASM: " + fileInfo.Name;
                    DASMElement dasmElement = new DASMElement(fileInfo);
                    layoutDocument.Content = dasmElement;
                    DockingPane.Children.Add(layoutDocument);
                    DockingPane.SelectedContentIndex = DockingPane.ChildrenCount - 1;
                }
                if (UseBlendoverEffect)
                {
                    BlendOverEffect.Begin();
                }
                return true;
            }
            return false;
        }

        public void AddEditorElement(string filePath, string name, bool SelectMe)
        {
            LayoutDocument layoutDocument = new LayoutDocument();
            layoutDocument.Title = name;
            layoutDocument.Closing += layoutDocument_Closing;
            layoutDocument.ToolTip = filePath;
            EditorElement editor = new EditorElement(filePath);
            editor.Parent = layoutDocument;
            layoutDocument.Content = editor;
            EditorsReferences.Add(editor);
            DockingPane.Children.Add(layoutDocument);
            if (SelectMe)
            {
				layoutDocument.IsSelected = true;
            }
        }

        private void DockingManager_ActiveContentChanged(object sender, EventArgs e)
        {
            UpdateWindowTitle();
            EditorElement ee = GetCurrentEditorElement();
            if (ee != null)
            {
                ee.editor.Focus();
            }
        }

        private void DockingManager_DocumentClosing(object sender, Xceed.Wpf.AvalonDock.DocumentClosingEventArgs e)
        {
            if (e.Document.Content is EditorElement)
            {
                ((EditorElement)e.Document.Content).Close();
            }
            UpdateWindowTitle();
        }

        private void layoutDocument_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
			e.Cancel = true;
        }

        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
			if (backgroundParserThread != null)
			{
				backgroundParserThread.Abort();
			}
			if (parseDistributorTimer != null)
			{
				parseDistributorTimer.Stop();
			}
            if (ServerCheckThread != null)
            {
                ServerCheckThread.Abort(); //a join would not work, so we have to be..forcefully...
            }
            List<string> lastOpenFiles = new List<string>();
            EditorElement[] editors = GetAllEditorElements();
            bool? SaveUnsaved = null;
			if (editors != null)
			{
				for (int i = 0; i < editors.Length; ++i)
				{
					if (File.Exists(editors[i].FullFilePath))
					{
						lastOpenFiles.Add(editors[i].FullFilePath);
						if (editors[i].NeedsSave)
						{
							if (SaveUnsaved == null)
							{
                                var result = MessageBox.Show(this, Program.Translations.GetLanguage("SavingUFiles"), Program.Translations.GetLanguage("Saving"), MessageBoxButton.YesNo, MessageBoxImage.Question);
								if (result == MessageBoxResult.Yes)
								{
									SaveUnsaved = true;
								}
								else
								{
									SaveUnsaved = false;
								}
							}
							if (SaveUnsaved.Value)
							{
								editors[i].Close(true, true);
							}
							else
							{
								editors[i].Close(false, false);
							}
						}
						else
						{
							editors[i].Close(false, false);
						}
					}
				}
			}
            Program.OptionsObject.LastOpenFiles = lastOpenFiles.ToArray();
#if !DEBUG
            if (Program.UpdateStatus.IsAvailable)
            {
                UpdateWindow updateWin = new UpdateWindow(Program.UpdateStatus) { Owner = this };
                updateWin.ShowDialog();
            }
#endif
        }

        private void MetroWindow_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                this.Activate();
                this.Focus();
                for (int i = 0; i < files.Length; ++i)
                {
                    TryLoadSourceFile(files[i], (i == 0), true, (i == 0));
                }
            }
        }

        public static void ProcessUITasks()
        {
            DispatcherFrame frame = new DispatcherFrame();
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background, new DispatcherOperationCallback(delegate(object parameter)
            {
                frame.Continue = false;
                return null;
            }), null);
            Dispatcher.PushFrame(frame);
        }

        private void ErrorResultGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var row = ((ErrorDataGridRow)ErrorResultGrid.SelectedItem);
            if (row == null)
            {
                return;
            }
            string fileName = row.file;
            EditorElement[] editors = GetAllEditorElements();
            if (editors == null)
            {
                return;
            }
            for (int i = 0; i < editors.Length; ++i)
            {
                if (editors[i].FullFilePath == fileName)
                {
                    ((LayoutDocument)editors[i].Parent).IsSelected = true;
                    int line = GetLineInteger(row.line);
                    if (line > 0 && line <= editors[i].editor.LineCount)
                    {
                        var lineObj = editors[i].editor.Document.Lines[line - 1];
                        editors[i].editor.ScrollToLine(line - 1);
                        editors[i].editor.Select(lineObj.Offset, lineObj.Length);
                    }
                }
            }
        }

        private void CloseErrorResultGrid(object sender, RoutedEventArgs e)
        {
            CompileOutputRow.Height = new GridLength(8.0);
        }

		private void EditorObjectBrowserGridRow_WidthChanged(object sender, EventArgs e)
		{
			if (FullyInitialized)
			{
				Program.OptionsObject.Program_ObjectbrowserWidth = ObjectBrowserColumn.Width.Value;
			}
		}

		private void UpdateWindowTitle()
        {
            EditorElement ee = GetCurrentEditorElement();
            string outString;
            if (ee == null)
            {
                outString = "SPEdit";
            }
            else
            {
                outString = ee.FullFilePath + " - SPEdit";
            }
            if (ServerIsRunning)
            {
                outString = $"{outString} ({Program.Translations.GetLanguage("ServerRunning")})";
            }
            this.Title = outString;
        }

        private int GetLineInteger(string lineStr)
        {
            int end = 0;
            for (int i = 0; i < lineStr.Length; ++i)
            {
                if (lineStr[i] >= '0' && lineStr[i] <= '9')
                {
                    end = i;
                }
                else
                {
                    break;
                }
            }
            int line;
            if (int.TryParse(lineStr.Substring(0, end + 1), out line))
            {
                return line;
            }
            return -1;
        }

        private ObservableCollection<string> compileButtonDict = new ObservableCollection<string>() { Program.Translations.GetLanguage("CompileAll"), Program.Translations.GetLanguage("CompileCurr") };
        private ObservableCollection<string> actionButtonDict = new ObservableCollection<string>() { Program.Translations.GetLanguage("Copy"), Program.Translations.GetLanguage("FTPUp"), Program.Translations.GetLanguage("StartServer") };
        private ObservableCollection<string> findReplaceButtonDict = new ObservableCollection<string>() { Program.Translations.GetLanguage("Replace"), Program.Translations.GetLanguage("ReplaceAll") };
    }
}
