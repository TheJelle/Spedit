using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Spedit.UI.Components;
using Spedit.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;

namespace Spedit.UI
{
    public partial class MainWindow : MetroWindow
    {
        private List<string> compiledFiles = new List<string>();
        private List<string> nonUploadedFiles = new List<string>();
        private List<string> compiledFileNames = new List<string>();

        private bool InCompiling = false;
        private async void Compile_SPScripts(bool All = true)
        {
            if (InCompiling) { return; }
            Command_SaveAll();
            InCompiling = true;
            compiledFiles.Clear();
            compiledFileNames.Clear();
            nonUploadedFiles.Clear();
            var c = Program.Configs[Program.SelectedConfig];
            FileInfo spCompInfo = null;
            bool SpCompFound = false;
			bool PressedEscape = false;
            foreach (string dir in c.SMDirectories)
            {
                spCompInfo = new FileInfo(Path.Combine(dir, "spcomp.exe"));
                if (spCompInfo.Exists)
                {
                    SpCompFound = true;
                    break;
                }
            }
            if (SpCompFound)
            {
                List<string> filesToCompile = new List<string>();
                if (All)
                {
                    EditorElement[] editors = GetAllEditorElements();
                    if (editors == null)
					{
						InCompiling = false;
						return;
                    }
                    for (int i = 0; i < editors.Length; ++i)
                    {
                        if (editors[i].CompileBox.IsChecked.Value)
                        {
                            filesToCompile.Add(editors[i].FullFilePath);
                        }
                    }
                }
                else
                {
                    EditorElement ee = GetCurrentEditorElement();
                    if (ee == null)
					{
						InCompiling = false;
						return;
                    }
                    /*
                    ** I've struggled a bit here. Should i check, if the CompileBox is checked 
                    ** and only compile if it's checked or should it be ignored and compiled anyway?
                    ** I decided, to compile anyway but give me feedback/opinions.
                    */
                    if (ee.FullFilePath.EndsWith(".sp"))
                    {
                        filesToCompile.Add(ee.FullFilePath);
                    }
                }
                int compileCount = filesToCompile.Count;
                if (compileCount > 0)
                {
                    ErrorResultGrid.Items.Clear();
                    var progressTask = await this.ShowProgressAsync(Program.Translations.Compiling, "", false, this.MetroDialogOptions);
                    progressTask.SetProgress(0.0);
                    StringBuilder stringOutput = new StringBuilder();
                    Regex errorFilterRegex = new Regex(@"^(?<file>.+?)\((?<line>[0-9]+(\s*--\s*[0-9]+)?)\)\s*:\s*(?<type>[a-zA-Z]+\s+([a-zA-Z]+\s+)?[0-9]+)\s*:(?<details>.+)", RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.Multiline);
                    string destinationFileName = null;
                    FileInfo fileInfo = null;
                    string outFile = null;
                    for (int i = 0; i < compileCount; ++i)
                    {
						if (!InCompiling) //pressed escape
						{
							PressedEscape = true;
							break;
						}
                        string file = filesToCompile[i];
                        progressTask.SetMessage(file);
                        MainWindow.ProcessUITasks();
                        fileInfo = new FileInfo(file);
                        stringOutput.AppendLine(fileInfo.Name);
                        if (fileInfo.Exists)
                        {
                            using (Process process = new Process())
                            {
                                process.StartInfo.WorkingDirectory = fileInfo.DirectoryName;
                                process.StartInfo.UseShellExecute = true;
                                process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                                process.StartInfo.CreateNoWindow = true;
                                process.StartInfo.FileName = spCompInfo.FullName;
                                destinationFileName = ShortenScriptFileName(fileInfo.Name) + ".smx";
                                outFile = Path.Combine(fileInfo.DirectoryName, destinationFileName);
                                if (File.Exists(outFile)) { File.Delete(outFile); }
                                string errorFile = Environment.CurrentDirectory + @"\sourcepawn\errorfiles\error_" + Environment.TickCount.ToString() + "_" + file.GetHashCode().ToString("X") + "_" + i.ToString() + ".txt";
                                if (File.Exists(errorFile)) { File.Delete(errorFile); }

                                StringBuilder includeDirectories = new StringBuilder();
                                foreach (string dir in c.SMDirectories)
                                {
                                    includeDirectories.Append(" -i=\"" + dir + "\"");
                                }

                                string includeStr = string.Empty;
                                includeStr = includeDirectories.ToString();

                                process.StartInfo.Arguments = "\"" + fileInfo.FullName + "\" -o=\"" + outFile + "\" -e=\"" + errorFile + "\"" + includeStr + " -O=" + c.OptimizeLevel.ToString() + " -v=" + c.VerboseLevel.ToString();
                                progressTask.SetProgress((((double)(i + 1)) - 0.5d) / ((double)compileCount));
                                string execResult = ExecuteCommandLine(c.PreCmd, fileInfo.DirectoryName, c.CopyDirectory, fileInfo.FullName, fileInfo.Name, outFile, destinationFileName);
                                if (!string.IsNullOrWhiteSpace(execResult))
                                {
                                    stringOutput.AppendLine(execResult.Trim(new char[] { '\n', '\r' }));
                                }
                                MainWindow.ProcessUITasks();
                                try
                                {
                                    process.Start();
                                    process.WaitForExit();
                                }
                                catch (Exception)
                                {
                                    InCompiling = false;
                                }
                                if (!InCompiling) //cannot await in catch
                                {
                                    await progressTask.CloseAsync();
                                    await this.ShowMessageAsync(Program.Translations.SPCompNotStarted, Program.Translations.Error, MessageDialogStyle.Affirmative, this.MetroDialogOptions);
									return;
                                }
                                if (File.Exists(errorFile))
                                {
                                    string errorStr = File.ReadAllText(errorFile);
                                    stringOutput.AppendLine(errorStr.Trim(new char[] { '\n', '\r' } ));
                                    MatchCollection mc = errorFilterRegex.Matches(errorStr);
                                    for (int j = 0; j < mc.Count; ++j)
                                    {
                                        ErrorResultGrid.Items.Add(new ErrorDataGridRow()
                                        {
                                            compiledfileInfo = fileInfo,
                                            file = mc[j].Groups["file"].Value.Trim(),
                                            line = mc[j].Groups["line"].Value.Trim(),
                                            type = mc[j].Groups["type"].Value.Trim(),
                                            details = mc[j].Groups["details"].Value.Trim()
                                        });
                                    }
                                    File.Delete(errorFile);
                                }
                                stringOutput.AppendLine(Program.Translations.Done);
                                if (File.Exists(outFile))
                                {
                                    compiledFiles.Add(outFile);
                                    nonUploadedFiles.Add(outFile);
                                    compiledFileNames.Add(destinationFileName);
                                }
                                string execResult_Post = ExecuteCommandLine(c.PostCmd, fileInfo.DirectoryName, c.CopyDirectory, fileInfo.FullName, fileInfo.Name, outFile, destinationFileName);
                                if (!string.IsNullOrWhiteSpace(execResult_Post))
                                {
                                    stringOutput.AppendLine(execResult_Post.Trim(new char[] { '\n', '\r' }));
                                }
                                stringOutput.AppendLine();
                                progressTask.SetProgress(((double)(i + 1)) / ((double)compileCount));
                                MainWindow.ProcessUITasks();
                            }
                        }
                    }
					if (!PressedEscape)
					{
						progressTask.SetProgress(1.0);
						CompileOutput.Text = stringOutput.ToString();
						if (c.AutoCopy)
						{
							Copy_Plugins(true);
						}
						if (CompileOutputRow.Height.Value < 11.0)
						{
							CompileOutputRow.Height = new GridLength(200.0);
						}
					}
                    await progressTask.CloseAsync();
                }
            }
            else
            {
                await this.ShowMessageAsync(Program.Translations.Error, Program.Translations.SPCompNotFound, MessageDialogStyle.Affirmative, this.MetroDialogOptions);
            }
            InCompiling = false;
        }

        public void Copy_Plugins(bool OvertakeOutString = false)
        {
            if (compiledFiles.Count > 0)
            {
                nonUploadedFiles.Clear();
                int copyCount = 0;
                var c = Program.Configs[Program.SelectedConfig];
                if (!string.IsNullOrWhiteSpace(c.CopyDirectory))
                {
                    StringBuilder stringOutput = new StringBuilder();
                    for (int i = 0; i < compiledFiles.Count; ++i)
                    {
                        try
                        {
                            FileInfo destFile = new FileInfo(compiledFiles[i]);
                            if (destFile.Exists)
                            {
                                string destinationFileName = destFile.Name;
                                string copyFileDestination = Path.Combine(c.CopyDirectory, destinationFileName);
                                File.Copy(compiledFiles[i], copyFileDestination, true);
                                nonUploadedFiles.Add(copyFileDestination);
                                stringOutput.AppendLine($"{Program.Translations.Copied}: " + compiledFiles[i]);
                                ++copyCount;
                                if (c.DeleteAfterCopy)
                                {
                                    File.Delete(compiledFiles[i]);
                                    stringOutput.AppendLine($"{Program.Translations.Deleted}: " + compiledFiles[i]);
                                }
                            }
                        }
                        catch (Exception)
                        {
                            stringOutput.AppendLine($"{Program.Translations.FailCopy}: " + compiledFiles[i]);
                        }
                    }
                    if (copyCount == 0)
                    {
                        stringOutput.AppendLine(Program.Translations.NoFilesCopy);
                    }
                    if (OvertakeOutString)
                    {
                        CompileOutput.AppendText(stringOutput.ToString());
                    }
                    else
                    {
                        CompileOutput.Text = stringOutput.ToString();
                    }
                    if (CompileOutputRow.Height.Value < 11.0)
                    {
                        CompileOutputRow.Height = new GridLength(200.0);
                    }
                }
            }
        }

        public void FTPUpload_Plugins()
        {
            if (nonUploadedFiles.Count <= 0)
            {
                return;
            }
            var c = Program.Configs[Program.SelectedConfig];
            if ((string.IsNullOrWhiteSpace(c.FTPHost)) || (string.IsNullOrWhiteSpace(c.FTPUser)))
            {
                return;
            }
            StringBuilder stringOutput = new StringBuilder();
            try
            {
                FTP ftp = new FTP(c.FTPHost, c.FTPUser, c.FTPPassword);
                for (int i = 0; i < nonUploadedFiles.Count; ++i)
                {
                    FileInfo fileInfo = new FileInfo(nonUploadedFiles[i]);
                    if (fileInfo.Exists)
                    {
                        string uploadDir;
                        if (string.IsNullOrWhiteSpace(c.FTPDir))
                        {
                            uploadDir = fileInfo.Name;
                        }
                        else
                        {
                            uploadDir = c.FTPDir.TrimEnd(new char[] { '/' }) + "/" + fileInfo.Name;
                        }
						try
						{
							ftp.upload(uploadDir, nonUploadedFiles[i]);
							stringOutput.AppendLine($"{Program.Translations.Uploaded}: " + nonUploadedFiles[i]);
						}
						catch (Exception e)
						{
							stringOutput.AppendLine(string.Format(Program.Translations.ErrorUploadFile, nonUploadedFiles[i], uploadDir));
                            stringOutput.AppendLine($"{Program.Translations.Details}: " + e.Message);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                stringOutput.AppendLine(Program.Translations.ErrorUpload);
                stringOutput.AppendLine($"{Program.Translations.Details}: " + e.Message);
            }
            stringOutput.AppendLine(Program.Translations.Done);
            CompileOutput.Text = stringOutput.ToString();
            if (CompileOutputRow.Height.Value < 11.0)
            {
                CompileOutputRow.Height = new GridLength(200.0);
            }
        }

        public bool ServerIsRunning = false;
        public Process ServerProcess;
        public Thread ServerCheckThread;

        public void Server_Start()
        {
            if (ServerIsRunning)
            { return; }
            var c = Program.Configs[Program.SelectedConfig];
            string serverOptionsPath = c.ServerFile;
            if (string.IsNullOrWhiteSpace(serverOptionsPath))
            {
                return;
            }
            FileInfo serverExec = new FileInfo(serverOptionsPath);
            if (!serverExec.Exists)
            {
                return;
            }
            try
            {
                ServerProcess = new Process();
                ServerProcess.StartInfo.UseShellExecute = true;
                ServerProcess.StartInfo.FileName = serverExec.FullName;
                ServerProcess.StartInfo.WorkingDirectory = serverExec.DirectoryName;
                ServerProcess.StartInfo.Arguments = c.ServerArgs;
                ServerCheckThread = new Thread(new ThreadStart(ProcessCheckWorker));
                ServerCheckThread.Start();
            }
            catch (Exception)
            {
                if (ServerProcess != null)
                {
                    ServerProcess.Dispose();
                }
                return;
            }

        }

        private void ProcessCheckWorker()
        {
            try
            {
                ServerProcess.Start();
            }
            catch (Exception)
            {
                return;
            }
            ServerIsRunning = true;
            Program.MainWindow.Dispatcher.Invoke(() =>
            {
                EnableServerAnim.Begin();
                UpdateWindowTitle();
            });
            ServerProcess.WaitForExit();
            ServerProcess.Dispose();
            ServerIsRunning = false;
            Program.MainWindow.Dispatcher.Invoke(() =>
            {
                if (Program.MainWindow.IsLoaded)
                {
                    DisableServerAnim.Begin();
                    UpdateWindowTitle();
                }
            });
        }


        private string ShortenScriptFileName(string fileName)
        {
            if (fileName.EndsWith(".sp", StringComparison.InvariantCultureIgnoreCase))
            {
                return fileName.Substring(0, fileName.Length - 3);
            }
            return fileName;
        }

        private string ExecuteCommandLine(string code, string directory, string copyDir, string scriptFile, string scriptName, string pluginFile, string pluginName)
        {
            code = ReplaceCMDVaraibles(code, directory, copyDir, scriptFile, scriptName, pluginFile, pluginName);
            if (string.IsNullOrWhiteSpace(code))
            {
                return null;
            }
            string batchFile = (new FileInfo(Path.Combine("sourcepawn\\temp\\", Environment.TickCount.ToString() + "_" + ((uint)code.GetHashCode() ^ (uint)directory.GetHashCode()).ToString() + "_temp.bat"))).FullName;
            File.WriteAllText(batchFile, code);
            string result = null;
            using (Process process = new Process())
            {
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.FileName = "cmd.exe";
                process.StartInfo.WorkingDirectory = directory;
                process.StartInfo.Arguments = "/c \"" + batchFile + "\"";
                process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.RedirectStandardOutput = true;
                process.Start();
                process.WaitForExit();
                using (StreamReader reader = process.StandardOutput)
                {
                    result = reader.ReadToEnd();
                }
            }
            File.Delete(batchFile);
            return result;
        }

        private string ReplaceCMDVaraibles(string CMD, string scriptDir, string copyDir, string scriptFile, string scriptName, string pluginFile, string pluginName)
        {
            CMD = CMD.Replace("{editordir}", Environment.CurrentDirectory.Trim('\\'));
            CMD = CMD.Replace("{scriptdir}", scriptDir);
            CMD = CMD.Replace("{copydir}", copyDir);
            CMD = CMD.Replace("{scriptfile}", scriptFile);
            CMD = CMD.Replace("{scriptname}", scriptName);
            CMD = CMD.Replace("{pluginfile}", pluginFile);
            CMD = CMD.Replace("{pluginname}", pluginName);
            return CMD;
        }
    }

    public class ErrorDataGridRow
    {
        public FileInfo compiledfileInfo { set; get; }
        public string file { set; get; }
        public string line { set; get; }
        public string type { set; get; }
        public string details { set; get; }
    }
}
