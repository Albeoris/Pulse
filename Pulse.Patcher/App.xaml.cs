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
            Log.Message("Приложение запущено.");

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
                Log.Error(ex, "Не удалось запустить приложение.");
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
                Log.Error(ex, "Не удалось очистить каталог: [{0}]", destination);
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
                        sb.AppendLine("Следующие процессы блокируют обновление. Уничтожить их?");
                        foreach (Process locker in processes)
                        {
                            sb.Append(locker.ProcessName);
                            sb.Append(": ");
                            sb.AppendLine(locker.GetExecutablePath());
                        }

                        if (MessageBox.Show(sb.ToString(), "Внимание!", MessageBoxButton.YesNo, MessageBoxImage.Error) != MessageBoxResult.Yes)
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
                sb.AppendLine("Не удалось установить обновление программы установки.");
                sb.AppendLine("Пожалуйста, скопируйте содержимое каталога:");
                sb.AppendLine(source);
                sb.AppendLine("В каталог:");
                sb.AppendLine(destination);
                sb.AppendLine("С заменой всех файлов. Приносим извинения за доставленные неудобства.");
                sb.AppendLine();
                sb.AppendLine("Принична ошибки:");
                sb.AppendLine(ex.ToString());
                MessageBox.Show(sb.ToString(), "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
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