using Spedit.UI.Components;
using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using Xceed.Wpf.AvalonDock.Layout;

namespace Spedit.UI
{
    public partial class MainWindow
    {
        bool IsSearchFieldOpen = false;

        public void ToggleSearchField()
        {
            EditorElement ee = GetCurrentEditorElement();
            if (IsSearchFieldOpen)
            {
                if (ee != null)
                {
                    if (ee.IsKeyboardFocusWithin)
                    {
                        if (ee.editor.SelectionLength > 0)
                        {
                            FindBox.Text = ee.editor.SelectedText;
                        }
                        FindBox.SelectAll();
                        FindBox.Focus();
                        return;
                    }
                }
                IsSearchFieldOpen = false;
                FindReplaceGrid.IsHitTestVisible = false;
                if (Program.OptionsObject.UI_Animations)
                {
                    FadeFindReplaceGridOut.Begin();
                }
                else
                {
                    FindReplaceGrid.Opacity = 0.0;
                }
                if (ee == null)
                {
                    return;
                }
                ee.editor.Focus();
            }
            else
            {
                IsSearchFieldOpen = true;
                FindReplaceGrid.IsHitTestVisible = true;
                if (ee == null)
                {
                    return;
                }
                if (ee.editor.SelectionLength > 0)
                {
                    FindBox.Text = ee.editor.SelectedText;
                }
                FindBox.SelectAll();
                if (Program.OptionsObject.UI_Animations)
                {
                    FadeFindReplaceGridIn.Begin();
                }
                else
                {
                    FindReplaceGrid.Opacity = 1.0;
                }
                FindBox.Focus();
            }
        }

        private void CloseFindReplaceGrid(object sender, RoutedEventArgs e)
        {
            ToggleSearchField();
        }
        private void SearchButtonClicked(object sender, RoutedEventArgs e)
        {
            Search();
        }
        private void ReplaceButtonClicked(object sender, RoutedEventArgs e)
        {
            if (ReplaceButton.SelectedIndex == 1)
            {
                ReplaceAll();
            }
            else
            {
                Replace();
            }
        }
        private void CountButtonClicked(object sender, RoutedEventArgs e)
        {
            Count();
        }
        private void SearchBoxTextChanged(object sender, RoutedEventArgs e)
        {
            FindResultBlock.Text = string.Empty;
        }
        private void SearchBoxKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Search();
            }
        }
        private void ReplaceBoxKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Replace();
            }
        }
        private void FindReplaceGrid_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Escape)
            {
                ToggleSearchField();
            }
        }

        private void Search()
        {
            int editorIndex = 0;
            EditorElement[] editors = GetEditorElementsForFRAction(out editorIndex);
            if (editors == null) { return; }
            if (editors.Length < 1) { return; }
            if (editors[0] == null) { return; }
            Regex regex = GetSearchRegex();
            if (regex == null) { return; }
            int startFileCaretOffset = 0;
            bool foundOccurence = false;
            for (int i = editorIndex; i < (editors.Length + editorIndex + 1); ++i)
            {
                int index = ValueUnderMap(i, editors.Length);
                string searchText;
                int addToOffset = 0;
                if (i == editorIndex)
                {
                    startFileCaretOffset = editors[index].editor.CaretOffset;
                    addToOffset = startFileCaretOffset;
                    if (startFileCaretOffset < 0) { startFileCaretOffset = 0; }
                    searchText = editors[index].editor.Text.Substring(startFileCaretOffset);
                }
                else if (i == (editors.Length + editorIndex))
                {
                    if (startFileCaretOffset == 0)
                    {
                        searchText = string.Empty;
                    }
                    else
                    {
                        searchText = editors[index].editor.Text.Substring(0, startFileCaretOffset);
                    }
                }
                else
                {
                    searchText = editors[index].editor.Text;
                }
                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    Match m = regex.Match(searchText);
                    if (m != null) //can this happen?
                    {
                        if (m.Success)
                        {
                            foundOccurence = true;
                            ((LayoutDocument)editors[index].Parent).IsSelected = true;
                            editors[index].editor.CaretOffset = m.Index + addToOffset + m.Length;
                            editors[index].editor.Select(m.Index + addToOffset, m.Length);
                            var location = editors[index].editor.Document.GetLocation(m.Index + addToOffset);
                            editors[index].editor.ScrollTo(location.Line, location.Column);
							//FindResultBlock.Text = "Found in offset " + (m.Index + addToOffset).ToString() + " with length " + m.Length.ToString();
							FindResultBlock.Text = string.Format(Program.Translations.GetLanguage("FoundInOff"), m.Index + addToOffset, m.Length);
                            break;
                        }
                    }
                }
            }
            if (!foundOccurence)
            {
                FindResultBlock.Text = Program.Translations.GetLanguage("FoundNothing");
            }
        }

        private void Replace()
        {
            int editorIndex = 0;
            EditorElement[] editors = GetEditorElementsForFRAction(out editorIndex);
            if (editors == null) { return; }
            if (editors.Length < 1) { return; }
            if (editors[0] == null) { return; }
            Regex regex = GetSearchRegex();
            if (regex == null) { return; }
            string replaceString = ReplaceBox.Text;
            int startFileCaretOffset = 0;
            bool foundOccurence = false;
            for (int i = editorIndex; i < (editors.Length + editorIndex + 1); ++i)
            {
                int index = ValueUnderMap(i, editors.Length);
                string searchText;
                int addToOffset = 0;
                if (i == editorIndex)
                {
                    startFileCaretOffset = editors[index].editor.CaretOffset;
                    addToOffset = startFileCaretOffset;
                    if (startFileCaretOffset < 0) { startFileCaretOffset = 0; }
                    searchText = editors[index].editor.Text.Substring(startFileCaretOffset);
                }
                else if (i == (editors.Length + editorIndex))
                {
                    if (startFileCaretOffset == 0)
                    {
                        searchText = string.Empty;
                    }
                    else
                    {
                        searchText = editors[index].editor.Text.Substring(0, startFileCaretOffset);
                    }
                }
                else
                {
                    searchText = editors[index].editor.Text;
                }
                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    Match m = regex.Match(searchText);
                    if (m != null)
                    {
                        if (m.Success)
                        {
                            foundOccurence = true;
                            ((LayoutDocument)editors[index].Parent).IsSelected = true;
                            string result = m.Result(replaceString);
                            editors[index].editor.Document.Replace(m.Index + addToOffset, m.Length, result);
                            editors[index].editor.CaretOffset = m.Index + addToOffset + result.Length;
                            editors[index].editor.Select(m.Index + addToOffset, result.Length);
                            var location = editors[index].editor.Document.GetLocation(m.Index + addToOffset);
                            editors[index].editor.ScrollTo(location.Line, location.Column);
                            FindResultBlock.Text = string.Format(Program.Translations.GetLanguage("ReplacedOff"), MinHeight + addToOffset);
                            break;
                        }
                    }
                }
            }
            if (!foundOccurence)
            {
                FindResultBlock.Text = Program.Translations.GetLanguage("FoundNothing");
            }
        }

        private void ReplaceAll()
        {
            int editorIndex = 0;
            EditorElement[] editors = GetEditorElementsForFRAction(out editorIndex);
            if (editors == null) { return; }
            if (editors.Length < 1) { return; }
            if (editors[0] == null) { return; }
            Regex regex = GetSearchRegex();
            if (regex == null) { return; }
            int count = 0;
            int fileCount = 0;
            string replaceString = ReplaceBox.Text;
            for (int i = 0; i < editors.Length; ++i)
            {
                MatchCollection mc = regex.Matches(editors[i].editor.Text);
                if (mc.Count > 0)
                {
                    fileCount++;
                    count += mc.Count;
                    editors[i].editor.BeginChange();
                    for (int j = mc.Count - 1; j >= 0; --j)
                    {
                        string replace = mc[j].Result(replaceString);
                        editors[i].editor.Document.Replace(mc[j].Index, mc[j].Length, replace);
                    }
                    editors[i].editor.EndChange();
                    editors[i].NeedsSave = true;
                }
            }
			//FindResultBlock.Text = "Replaced " + count.ToString() + " occurences in " + fileCount.ToString() + " documents";
			FindResultBlock.Text = string.Format(Program.Translations.GetLanguage("ReplacedOcc, count, fileCount"));
        }

        private void Count()
        {
            int editorIndex = 0;
            EditorElement[] editors = GetEditorElementsForFRAction(out editorIndex);
            if (editors == null) { return; }
            if (editors.Length < 1) { return; }
            if (editors[0] == null) { return; }
            Regex regex = GetSearchRegex();
            if (regex == null) { return; }
            int count = 0;
            for (int i = 0; i < editors.Length; ++i)
            {
                MatchCollection mc = regex.Matches(editors[i].editor.Text);
                count += mc.Count;
            }
            FindResultBlock.Text = count.ToString() + Program.Translations.GetLanguage("OccFound");
        }

        private Regex GetSearchRegex()
        {
            string findString = FindBox.Text;
            if (string.IsNullOrEmpty(findString))
            {
                FindResultBlock.Text = Program.Translations.GetLanguage("EmptyPatt");
                return null;
            }
            Regex regex;
            RegexOptions regexOptions = RegexOptions.Compiled | RegexOptions.CultureInvariant;
            if (!CCBox.IsChecked.Value)
            { regexOptions |= RegexOptions.IgnoreCase; }
            if (NSearch_RButton.IsChecked.Value)
            {
                regex = new Regex(Regex.Escape(findString), regexOptions);
            }
            else if (WSearch_RButton.IsChecked.Value)
            {
                regex = new Regex("\\b" + Regex.Escape(findString) + "\\b", regexOptions);
            }
            else if (ASearch_RButton.IsChecked.Value)
            {
                findString = findString.Replace("\\t", "\t").Replace("\\r", "\r").Replace("\\n", "\n");
                Regex rx = new Regex(@"\\[uUxX]([0-9A-F]{4})");
                findString = rx.Replace(findString, delegate(Match match) { return ((char)Int32.Parse(match.Value.Substring(2), NumberStyles.HexNumber)).ToString(); });
                regex = new Regex(Regex.Escape(findString), regexOptions);
            }
            else //if (RSearch_RButton.IsChecked.Value)
            {
                regexOptions |= RegexOptions.Multiline;
                if (MLRBox.IsChecked.Value)
                { regexOptions |= RegexOptions.Singleline; } //paradox, isn't it? ^^
                try
                {
                    regex = new Regex(findString, regexOptions);
                }
                catch (Exception) { FindResultBlock.Text = Program.Translations.GetLanguage("NoValidRegex"); return null; }
            }
            return regex;
        }

        private EditorElement[] GetEditorElementsForFRAction(out int editorIndex)
        {
            int editorStartIndex = 0;
            EditorElement[] editors = null;
            if (FindDestinies.SelectedIndex == 0)
            { editors = new EditorElement[] { GetCurrentEditorElement() }; }
            else
            {
                editors = GetAllEditorElements();
                if (DockingPane.SelectedContent != null)
                {
                    object checkElement = DockingPane.SelectedContent.Content;
                    if (checkElement != null)
                    {
                        if (checkElement is EditorElement)
                        {
                            for (int i = 0; i < editors.Length; ++i)
                            {
                                if (editors[i] == checkElement)
                                {
                                    editorStartIndex = i;
                                }
                            }
                        }
                    }
                }
            }
            editorIndex = editorStartIndex;
            return editors;
        }

        private int ValueUnderMap(int value, int map)
        {
            while (value >= map)
            {
                value -= map;
            }
            return value;
        }
    }
}
