using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows;
using Pulse.Core;
using Pulse.Core.WinAPI;
using Pulse.UI;

namespace Pulse.Patcher
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            Log.Message("The app launched.");

            try
            {
                InteractionService.SetGamePart(FFXIIIGamePart.Part1);

                String[] args = Environment.GetCommandLineArgs();
                if (args.Length > 1)
                {
                    switch (args[1])
                    {
                        case "/u":
                            Log.Message("Update: " + args[2]);
                            Update(args[2]);
                            Environment.Exit(0);
                            break;
                        case "/d":
                            Log.Message("Delete: " + args[2]);
                            Delete(args[2]);
                            break;
                        default:
                            throw new NotImplementedException(args[0]);
                    }
                }

                StringsToZtrArchiveEntryInjector.ReplaceAnimatedText = true;
                MainWindow main = new MainWindow();
                main.Show();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unable to launch the application.");
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            InteractionService.Configuration.Provide().Save();
            base.OnExit(e);
        }

        private static void Delete(string destination)
        {
            Thread.Sleep(1000);

            try
            {
                Directory.Delete(destination, true);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to clean up the directory: [{0}]", destination);
            }
        }

        private void Update(string destination)
        {
            Thread.Sleep(1000);
            string source = AppDomain.CurrentDomain.BaseDirectory;
            try
            {
                List<string> sourceFiles = new List<string>(256);
                List<string> targetFiles = new List<string>(256);
                CreateFileTree(source, destination, sourceFiles, targetFiles);

                Process[] lockers = RestartManagerHelper.GetFileLockers(targetFiles.ToArray());
                if (!lockers.IsNullOrEmpty())
                {
                    Wildcard wildcard = new Wildcard("*Pulse.Patcher*");
                    List<Process> processes = new List<Process>(lockers.Length);
                    foreach (Process locker in lockers)
                    {
                        if (wildcard.IsMatch(locker.ProcessName))
                            locker.Kill();
                        else
                            processes.Add(locker);
                    }

                    if (!processes.IsNullOrEmpty())
                    {
                        StringBuilder sb = new StringBuilder(512);
                        sb.AppendLine("The following processes are blocking the update. destroy them?");
                        foreach (Process locker in processes)
                        {
                            sb.Append(locker.ProcessName);
                            sb.Append(": ");
                            sb.AppendLine(locker.GetExecutablePath());
                        }

                        if (MessageBox.Show(sb.ToString(), "Attention!", MessageBoxButton.YesNo, MessageBoxImage.Error) != MessageBoxResult.Yes)
                            throw new OperationCanceledException();

                        foreach (Process locker in processes)
                            locker.Kill();
                    }
                }

                for (int i = 0; i < sourceFiles.Count; i++)
                    File.Copy(sourceFiles[i], targetFiles[i], true);

                ProcessStartInfo procInfo = new ProcessStartInfo(Path.Combine(destination, "Pulse.Patcher.exe"), $"/d \"{source.TrimEnd('\\')}\"")
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    WorkingDirectory = destination
                };
                Process.Start(procInfo);
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                StringBuilder sb = new StringBuilder(512);
                sb.AppendLine("Unable to install the update program installation.");
                sb.AppendLine("Please copy the contents of a directory:");
                sb.AppendLine(source);
                sb.AppendLine("In the catalog:");
                sb.AppendLine(destination);
                sb.AppendLine("With the replacement of all the files . We apologize for any inconvenience caused.");
                sb.AppendLine();
                sb.AppendLine("Prinichna error:");
                sb.AppendLine(ex.ToString());
                MessageBox.Show(sb.ToString(), "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CreateFileTree(String source, String destination, List<string> sourceFiles, List<string> targetFiles)
        {
            Directory.CreateDirectory(destination);

            foreach (string file in Directory.EnumerateFiles(source))
            {
                sourceFiles.Add(file);
                targetFiles.Add(Path.Combine(destination, Path.GetFileName(file)));
            }

            foreach (string directory in Directory.EnumerateDirectories(source))
                CreateFileTree(directory, Path.Combine(destination, Path.GetFileName(directory)), sourceFiles, targetFiles);
        }
    }
}