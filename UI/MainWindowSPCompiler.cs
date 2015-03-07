using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Windows;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Spedit.UI.Components;
using System.Globalization;
using Spedit.Utils;

namespace Spedit.UI
{
    public partial class MainWindow : MetroWindow
    {
        private List<string> compiledFiles = new List<string>();

        private bool InCompiling = false;
        private async void Compile_SPScripts(bool All = true)
        {
            Command_SaveAll();
            if (InCompiling) { return; }
            InCompiling = true;
            compiledFiles.Clear();
            var c = Program.Configs[Program.SelectedConfig];
            //string spCompPath = Path.Combine(Environment.CurrentDirectory + @"\sourcepawn\spcomp.exe");
            FileInfo spCompInfo = new FileInfo(Path.Combine(c.SMDirectory, "spcomp.exe"));
            if (spCompInfo.Exists)
            {
                List<string> filesToCompile = new List<string>();
                if (All)
                {
                    EditorElement[] editors = GetAllEditorElements();
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
                    var conf = Program.Configs[Program.SelectedConfig];
                    var progressTask = await this.ShowProgressAsync("Compiling", "", false, this.MetroDialogOptions);
                    progressTask.SetProgress(0.0);
                    StringBuilder stringOutput = new StringBuilder();
                    Regex errorFilterRegex = new Regex(@"^(?<file>.+?)\((?<line>[0-9]+(\s*--\s*[0-9]+)?)\)\s*:\s*(?<type>[a-zA-Z]+\s+([a-zA-Z]+\s+)?[0-9]+)\s*:(?<details>.+)", RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.Multiline);
                    for (int i = 0; i < compileCount; ++i)
                    {
                        string file = filesToCompile[i];
                        progressTask.SetMessage(file);
                        MainWindow.ProcessUITasks();
                        FileInfo fileInfo = new FileInfo(file);
                        stringOutput.AppendLine(fileInfo.Name);
                        if (fileInfo.Exists)
                        {
                            using (Process process = new Process())
                            {
                                process.StartInfo.WorkingDirectory = Environment.CurrentDirectory + @"\sourcepawn";
                                process.StartInfo.UseShellExecute = true;
                                process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                                process.StartInfo.CreateNoWindow = true;
                                process.StartInfo.FileName = spCompInfo.FullName;
                                string destinationFileName = ShortenScriptFileName(fileInfo.Name) + ".smx";
                                string outFile = Path.Combine(fileInfo.DirectoryName, destinationFileName);
                                if (File.Exists(outFile)) { File.Delete(outFile); }
                                string errorFile = Environment.CurrentDirectory + @"\sourcepawn\errorfiles\error_" + Environment.TickCount.ToString() + "_" + file.GetHashCode().ToString("X") + "_" + i.ToString() + ".txt";
                                if (File.Exists(errorFile)) { File.Delete(errorFile); }
                                process.StartInfo.Arguments = "\"" + fileInfo.FullName + "\" -o=\"" + outFile + "\" -e=\"" + errorFile + "\" -i=" + c.SMDirectory + " -O=" + c.OptimizeLevel.ToString() + " -v=" + c.VerboseLevel.ToString();
                                progressTask.SetProgress((((double)(i + 1)) - 0.5d) / ((double)compileCount));
                                string execResult = ExecuteCommandLine(conf.PreCmd, fileInfo.DirectoryName, conf.CopyDirectory, fileInfo.FullName, fileInfo.Name, outFile, destinationFileName);
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
                                    await this.ShowMessageAsync("The 'spcomp.exe' compiler did not started correctly", "Error", MessageDialogStyle.Affirmative, this.MetroDialogOptions);
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
                                            file = mc[j].Groups["file"].Value.Trim(),
                                            line = mc[j].Groups["line"].Value.Trim(),
                                            type = mc[j].Groups["type"].Value.Trim(),
                                            details = mc[j].Groups["details"].Value.Trim()
                                        });
                                    }
                                    File.Delete(errorFile);
                                }
                                stringOutput.AppendLine("Done");
                                if (File.Exists(outFile))
                                {
                                    compiledFiles.Add(outFile);
                                }
                                stringOutput.AppendLine();
                                progressTask.SetProgress(((double)(i + 1)) / ((double)compileCount));
                                execResult = ExecuteCommandLine(conf.PostCmd, fileInfo.DirectoryName, conf.CopyDirectory, fileInfo.FullName, fileInfo.Name, outFile, destinationFileName);
                                if (!string.IsNullOrWhiteSpace(execResult))
                                {
                                    stringOutput.AppendLine(execResult.Trim(new char[] { '\n', '\r' }));
                                }
                                MainWindow.ProcessUITasks();
                            }
                        }
                    }
                    progressTask.SetProgress(1.0);
                    CompileOutput.Text = stringOutput.ToString();
                    if (CompileOutputRow.Height.Value < 11.0)
                    {
                        CompileOutputRow.Height = new GridLength(200.0);
                    }
                    await progressTask.CloseAsync();

                }
            }
            else
            {
                await this.ShowMessageAsync("The 'spcomp.exe' compiler could not be found.", "Error", MessageDialogStyle.Affirmative, this.MetroDialogOptions);
            }
            InCompiling = false;
        }

        public void Copy_Plugins()
        {
            if (compiledFiles.Count > 0)
            {
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
                                string destinationFileName = ShortenScriptFileName(destFile.Name);
                                string copyFileDestination = Path.Combine(c.CopyDirectory, destinationFileName);
                                File.Copy(compiledFiles[i], copyFileDestination, true);
                                stringOutput.AppendLine("Copied: " + compiledFiles[i]);
                                if (c.DeleteAfterCopy)
                                {
                                    File.Delete(compiledFiles[i]);
                                    stringOutput.AppendLine("Deleted: " + compiledFiles[i]);
                                }
                            }
                        }
                        catch (Exception)
                        {
                            stringOutput.AppendLine("Failed to copy: " + compiledFiles[i]);
                        }
                    }
                    CompileOutput.Text = stringOutput.ToString();
                    if (CompileOutputRow.Height.Value < 11.0)
                    {
                        CompileOutputRow.Height = new GridLength(200.0);
                    }
                }
            }
        }

        public void FTPUpload_Plugins()
        {
            if (compiledFiles.Count <= 0)
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
                for (int i = 0; i < compiledFiles.Count; ++i)
                {
                    FileInfo fileInfo = new FileInfo(compiledFiles[i]);
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
                            ftp.upload(uploadDir, compiledFiles[i]);
                            stringOutput.AppendLine("Uploaded: " + compiledFiles[i]);
                        }
                        catch (Exception e)
                        {
                            stringOutput.AppendLine("Error while uploading file: " + compiledFiles[i]);
                            stringOutput.AppendLine("Details: " + e.Message);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                stringOutput.AppendLine("Error while uploading files");
                stringOutput.AppendLine("Details: " + e.Message);
            }
            stringOutput.AppendLine("Done");
            CompileOutput.Text = stringOutput.ToString();
            if (CompileOutputRow.Height.Value < 11.0)
            {
                CompileOutputRow.Height = new GridLength(200.0);
            }
        }

        public async void Server_Start()
        {
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
            using (Process process = new Process())
            {
                var progressTask = await this.ShowProgressAsync("Running local server", "Waiting for server to terminating.", false, this.MetroDialogOptions);
                process.StartInfo.UseShellExecute = true;
                process.StartInfo.FileName = serverExec.FullName;
                process.StartInfo.WorkingDirectory = serverExec.DirectoryName;
                process.StartInfo.Arguments = c.ServerArgs;
                process.Start();
                process.WaitForExit();
                await progressTask.CloseAsync();
            }
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
        public string file { set; get; }
        public string line { set; get; }
        public string type { set; get; }
        public string details { set; get; }
    }
}
