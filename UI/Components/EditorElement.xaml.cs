using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit.Rendering;
using ICSharpCode.AvalonEdit.Utils;
using MahApps.Metro.Controls.Dialogs;
using Spedit.Utils.SPSyntaxTidy;
using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Xceed.Wpf.AvalonDock.Layout;

namespace Spedit.UI.Components
{
    /// <summary>
    /// Interaction logic for EditorElement.xaml
    /// </summary>
    public partial class EditorElement : UserControl
    {
        public new LayoutDocument Parent;

        public FoldingManager foldingManager;
        SPFoldingStrategy foldingStrategy;
        ColorizeSelection colorizeSelection;
        SPBracketSearcher bracketSearcher;
        BracketHighlightRenderer bracketHighlightRenderer;

        FileSystemWatcher fileWatcher;

        Timer regularyTimer;
        bool WantFoldingUpdate = false;
        bool SelectionIsHighlited = false;

        Storyboard FadeJumpGridIn;
        Storyboard FadeJumpGridOut;

        double LineHeight = 0.0;

        public string FullFilePath
        {
            get { return _FullFilePath; }
            set
            {
                FileInfo fInfo = new FileInfo(value);
                _FullFilePath = fInfo.FullName;
                Parent.Title = fInfo.Name;
                if (fileWatcher != null)
                {
                    fileWatcher.Path = fInfo.DirectoryName;
                }
            }
        }
        private string _FullFilePath = "";

        private bool _NeedsSave = false;
        public bool NeedsSave
        {
            get
            {
                return _NeedsSave;
            }
            set
            {
                if (!(value ^ _NeedsSave)) //when not changed
                {
                    return;
                }
                _NeedsSave = value;
                if (Parent != null)
                {
                    if (_NeedsSave)
                    {
                        Parent.Title = "*" + Parent.Title;
                    }
                    else
                    {
                        Parent.Title = Parent.Title.Trim(new char[] { '*' });
                    }
                }
            }
        }

        public EditorElement()
        {
            InitializeComponent();
        }
        public EditorElement(string filePath)
        {
            InitializeComponent();

            bracketSearcher = new SPBracketSearcher();
            bracketHighlightRenderer = new BracketHighlightRenderer(editor.TextArea.TextView);
            editor.TextArea.IndentationStrategy = new EditorIndetationStrategy();

            FadeJumpGridIn = (Storyboard)this.Resources["FadeJumpGridIn"];
            FadeJumpGridOut = (Storyboard)this.Resources["FadeJumpGridOut"];

            this.KeyDown += EditorElement_KeyDown;

            editor.TextArea.Caret.PositionChanged += Caret_PositionChanged;
            editor.TextArea.SelectionChanged += TextArea_SelectionChanged;
            editor.TextArea.PreviewKeyDown += TextArea_PreviewKeyDown;
            editor.PreviewMouseWheel += PrevMouseWheel;
            editor.MouseDown += editor_MouseDown;
            editor.TextArea.TextEntered += TextArea_TextEntered;

            FileInfo fInfo = new FileInfo(filePath);
            if (fInfo.Exists)
            {
                fileWatcher = new FileSystemWatcher(fInfo.DirectoryName) { IncludeSubdirectories = false };
                fileWatcher.NotifyFilter = NotifyFilters.Size | NotifyFilters.LastWrite;
                fileWatcher.Filter = "*" + fInfo.Extension;
                fileWatcher.Changed += fileWatcher_Changed;
                fileWatcher.EnableRaisingEvents = true;
            }
            else
            {
                fileWatcher = null;
            }
            _FullFilePath = filePath;
            editor.Options.ConvertTabsToSpaces = false;
            editor.Options.EnableHyperlinks = false;
            editor.Options.HighlightCurrentLine = true;
            editor.Options.AllowScrollBelowDocument = true;
            editor.TextArea.SelectionCornerRadius = 0.0;

            editor.FontFamily = new FontFamily(Program.OptionsObject.Editor_FontFamily);
            editor.WordWrap = Program.OptionsObject.Editor_WordWrap;
            UpdateFontSize(Program.OptionsObject.Editor_FontSize, false);

            colorizeSelection = new ColorizeSelection();
            editor.TextArea.TextView.LineTransformers.Add(colorizeSelection);
            editor.SyntaxHighlighting = new AeonEditorHighlighting();

            LoadAutoCompletes();

            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (StreamReader reader = FileReader.OpenStream(fs, Encoding.UTF8))
                {
                    string source = reader.ReadToEnd();
                    source = ((source.Replace("\r\n", "\n")).Replace("\r", "\n")).Replace("\n", "\r\n"); //normalize line endings
                    editor.Text = source;
                }
            }
            _NeedsSave = false;

            var encoding = new UTF8Encoding(false);
            editor.Encoding = encoding; //let them read in whatever encoding they want - but save in UTF8

            foldingManager = FoldingManager.Install(editor.TextArea);
            foldingStrategy = new SPFoldingStrategy();
            foldingStrategy.UpdateFoldings(foldingManager, editor.Document);

            regularyTimer = new Timer(2000.0);
            regularyTimer.Elapsed += regularyTimer_Elapsed;
            regularyTimer.Start();

            CompileBox.IsChecked = (bool?)filePath.EndsWith(".sp");
        }

        private void EditorElement_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.G)
            {
                if (Keyboard.IsKeyDown(Key.LeftCtrl))
                {
                    ToggleJumpGrid();
                    e.Handled = true;
                }
            }
        }

        private bool JumpGridIsOpen = false;

        public void ToggleJumpGrid()
        {
            if (JumpGridIsOpen)
            {
                FadeJumpGridOut.Begin();
                JumpGridIsOpen = false;
            }
            else
            {
                FadeJumpGridIn.Begin();
                JumpGridIsOpen = true;
                JumpNumber.Focus();
                JumpNumber.SelectAll();
            }
        }

        private void JumpNumberKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                JumpToNumber(null, null);
                e.Handled = true;
            }
        }

        private void JumpToNumber(object sender, RoutedEventArgs e)
        {
            int num;
            if (int.TryParse(JumpNumber.Text, out num))
            {
                if (LineJump.IsChecked.Value)
                {
                    num = Math.Max(1, Math.Min(num, editor.LineCount));
                    var line = editor.Document.GetLineByNumber(num);
                    if (line != null)
                    {
                        editor.ScrollToLine(num);
                        editor.Select(line.Offset, line.Length);
                        editor.CaretOffset = line.Offset;
                    }
                }
                else
                {
                    num = Math.Max(0, Math.Min(num, editor.Text.Length));
                    var line = editor.Document.GetLineByOffset(num);
                    if (line != null)
                    {
                        editor.ScrollTo(line.LineNumber, 0);
                        editor.CaretOffset = num;
                    }
                }
            }
            ToggleJumpGrid();
            editor.Focus();
        }

        private void fileWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (e == null) { return; }
            if (e.FullPath == _FullFilePath)
            {
                bool ReloadFile = false;
                if (_NeedsSave)
                {
                    var result = MessageBox.Show(_FullFilePath + " has changed." + Environment.NewLine + "Try reloading file?", "File Changed", MessageBoxButton.YesNo, MessageBoxImage.Asterisk);
                    ReloadFile = (result == MessageBoxResult.Yes);
                }
                else //when the user didnt changed anything, we just reload the file since we are intelligent...
                {
                    ReloadFile = true;
                }
                if (ReloadFile)
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        FileStream stream;
                        bool IsNotAccessed = true;
                        while (IsNotAccessed)
                        {
                            try
                            {
                                using (stream = new FileStream(_FullFilePath, FileMode.OpenOrCreate))
                                {
                                    editor.Load(stream);
                                    NeedsSave = false;
                                    IsNotAccessed = false;
                                }
                            }
                            catch (Exception) { }
                            System.Threading.Thread.Sleep(100); //dont include System.Threading in the using directives, cause its onlyused once and the Timer class will double
                        }
                    });
                }
            }
        }

        private void regularyTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
                    {
                        if (editor.SelectionLength > 0 && editor.SelectionLength < 50)
                        {
                            string selectionString = editor.SelectedText;
                            if (IsValidSearchSelectionString(selectionString))
                            {
                                colorizeSelection.SelectionString = selectionString;
                                colorizeSelection.HighlightSelection = true;
                                SelectionIsHighlited = true;
                                editor.TextArea.TextView.Redraw();
                            }
                            else
                            {
                                colorizeSelection.HighlightSelection = false;
                                colorizeSelection.SelectionString = string.Empty;
                                if (SelectionIsHighlited)
                                {
                                    editor.TextArea.TextView.Redraw();
                                    SelectionIsHighlited = false;
                                }
                            }
                        }
                        else
                        {
                            colorizeSelection.HighlightSelection = false;
                            colorizeSelection.SelectionString = string.Empty;
                            if (SelectionIsHighlited)
                            {
                                editor.TextArea.TextView.Redraw();
                                SelectionIsHighlited = false;
                            }
                        }
                    });
            if (WantFoldingUpdate)
            {
                WantFoldingUpdate = false;
                try //this "solves" a racing-conditions error - i wasnt able to fix it till today.. 
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        foldingStrategy.UpdateFoldings(foldingManager, editor.Document);
                    });
                }
                catch (Exception) { }
            }
        }

        public void Save(bool Force = false)
        {
            if (_NeedsSave || Force)
            {
                if (fileWatcher != null)
                {
                    fileWatcher.EnableRaisingEvents = false;
                }

                try
                {
                    using (FileStream fs = new FileStream(_FullFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        editor.Save((Stream)fs);
                    }

                    NeedsSave = false;
                }
                catch (IOException)
                {
                    System.Windows.MessageBox.Show("Error while saving file: " + _FullFilePath);
                }
                
                if (fileWatcher != null)
                {
                    fileWatcher.EnableRaisingEvents = true;
                }
            }
        }

        public void UpdateFontSize(double size, bool UpdateLineHeight = true)
        {
            if (size > 2 && size < 31)
            {
                editor.FontSize = size;
                StatusLine_FontSize.Text = size.ToString("n0") + " pt";
            }
            if (UpdateLineHeight)
            {
                LineHeight = editor.TextArea.TextView.DefaultLineHeight;
            }
        }

        public async void Close(bool ForcedToSave = false, bool CheckSavings = true)
        {
            regularyTimer.Stop();
            regularyTimer.Close();
            if (fileWatcher != null)
            {
                fileWatcher.EnableRaisingEvents = false;
                fileWatcher.Dispose();
                fileWatcher = null;
            }
            if (CheckSavings)
            {
                if (_NeedsSave)
                {
                    if (ForcedToSave)
                    {
                        Save();
                    }
                    else
                    {
                        var Result = await Program.MainWindow.ShowMessageAsync("Saving File '" + Parent.Title.Trim(new char[] { '*' }) + "'", "", MessageDialogStyle.AffirmativeAndNegative, Program.MainWindow.MetroDialogOptions);
                        if (Result == MessageDialogResult.Affirmative)
                        {
                            Save();
                        }
                    }
                }
            }
            Program.MainWindow.EditorsReferences.Remove(this);
            Parent = null; //to prevent a ring depency which disables the GC from work

        }

        private void editor_TextChanged(object sender, EventArgs e)
        {
            WantFoldingUpdate = true;
            NeedsSave = true;
        }

        private void Caret_PositionChanged(object sender, EventArgs e)
        {
            StatusLine_Coloumn.Text = "Col " + editor.TextArea.Caret.Column.ToString();
            StatusLine_Line.Text = "Ln " + editor.TextArea.Caret.Line.ToString();
            EvaluateIntelliSense();
            var result = bracketSearcher.SearchBracket(editor.Document, editor.CaretOffset);
            bracketHighlightRenderer.SetHighlight(result);
        }

        private void TextArea_TextEntered(object sender, TextCompositionEventArgs e)
        {
            if (Program.OptionsObject.Editor_ReformatLineAfterSemicolon)
            {
                if (e.Text == ";")
                {
                    if (editor.CaretOffset >= 0)
                    {
                        var line = editor.Document.GetLineByOffset(editor.CaretOffset);
                        var leadingIndentation = editor.Document.GetText(TextUtilities.GetLeadingWhitespace(editor.Document, line));
                        string newLineStr = leadingIndentation + SPSyntaxTidy.TidyUp(editor.Document.GetText(line)).Trim();
                        editor.Document.Replace(line, newLineStr);
                    }
                }
            }
            if (e.Text == "}") //force indentate line so we can evaluate the indentation
            {
                editor.TextArea.IndentationStrategy.IndentLine(editor.Document, editor.Document.GetLineByOffset(editor.CaretOffset));
                foldingStrategy.UpdateFoldings(foldingManager, editor.Document);
            }
            else if (e.Text == "{")
            {
                foldingStrategy.UpdateFoldings(foldingManager, editor.Document);
            }
        }

        private void TextArea_SelectionChanged(object sender, EventArgs e)
        {
            StatusLine_SelectionLength.Text = "Len " + editor.SelectionLength.ToString();
        }

        private void PrevMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                UpdateFontSize(editor.FontSize + Math.Sign(e.Delta));
                e.Handled = true;
            }
            else
            {
                if (LineHeight == 0.0)
                {
                    LineHeight = editor.TextArea.TextView.DefaultLineHeight;
                }
                editor.ScrollToVerticalOffset(editor.VerticalOffset - (Math.Sign((double)e.Delta) * LineHeight * Program.OptionsObject.Editor_ScrollLines));
                //editor.ScrollToVerticalOffset(editor.VerticalOffset - ((double)e.Delta * editor.FontSize * Program.OptionsObject.Editor_ScrollSpeed));
                e.Handled = true;
            }
            HideISAC();
        }

        private void editor_MouseDown(object sender, MouseButtonEventArgs e)
        {
            HideISAC();
        }

        private void TextArea_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = ISAC_EvaluateKeyDownEvent(e.Key);
        }

        private void HandleContextMenuCommand(object sender, RoutedEventArgs e)
        {
            switch ((string)((MenuItem)sender).Tag)
            {
                case "0": { editor.Undo(); break; }
                case "1": { editor.Redo(); break; }
                case "2": { editor.Cut(); break; }
                case "3": { editor.Copy(); break; }
                case "4": { editor.Paste(); break; }
                case "5": { editor.SelectAll(); break; }
            }
        }

        private void ContextMenu_Opening(object sender, RoutedEventArgs e)
        {
            ((MenuItem)((ContextMenu)sender).Items[0]).IsEnabled = editor.CanUndo;
            ((MenuItem)((ContextMenu)sender).Items[1]).IsEnabled = editor.CanRedo;
        }

        private bool IsValidSearchSelectionString(string s)
        {
            int length = s.Length;
            for (int i = 0; i < length; ++i)
            {
                if (!((s[i] >= 'a' && s[i] <= 'z') || (s[i] >= 'A' && s[i] <= 'Z') || (s[i] >= '0' && s[i] <= '9') || (s[i] == '_')))
                {
                    return false;
                }
            }
            return true;
        }
    }

    public class ColorizeSelection : DocumentColorizingTransformer
    {
        public string SelectionString = string.Empty;
        public bool HighlightSelection = false;

        protected override void ColorizeLine(DocumentLine line)
        {
            if (HighlightSelection)
            {
                if (string.IsNullOrWhiteSpace(SelectionString))
                {
                    return;
                }
                int lineStartOffset = line.Offset;
                string text = CurrentContext.Document.GetText(line);
                int start = 0;
                int index;
                while ((index = text.IndexOf(SelectionString, start)) >= 0)
                {
                    base.ChangeLinePart(
                        lineStartOffset + index,
                        lineStartOffset + index + SelectionString.Length,
                        (VisualLineElement element) =>
                        {
                            element.BackgroundBrush = Brushes.LightGray;
                        });
                    start = index + 1;
                }
            }
        }
    }

}
