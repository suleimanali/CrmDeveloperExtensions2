﻿using CrmDeveloperExtensions2.Core.Models;
using Newtonsoft.Json;
using NLog;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows;
using Process = System.Diagnostics.Process;
using StatusBar = CrmDeveloperExtensions2.Core.StatusBar;

namespace TemplateWizards
{
    public class NpmProcessor
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static void InstallPackage(string package, string version, string path)
        {
            try
            {
                if (!string.IsNullOrEmpty(version))
                    version = $"@{version}";

                StatusBar.SetStatusBarValue($"{Resources.Resource.NpmPackageInstallingStatusBarMessage}: {package}{version}");

                var processStartInfo = CreateProcessStartInfo();
                processStartInfo.WorkingDirectory = path;

                var process = Process.Start(processStartInfo);
                if (process == null)
                {
                    MessageBox.Show($"{Resources.Resource.NpmPackageInstallFailureMessage}");
                    return;
                }

                process.StandardInput.WriteLine($"npm install --save {package}{version}");
                process.StandardInput.Flush();
                process.StandardInput.Close();
                process.WaitForExit();

                if (process.ExitCode != 0)
                    MessageBox.Show($"{Resources.Resource.NpmPackageInstallFailureMessage}: {process.StandardError.ReadToEnd()}");
            }
            finally
            {
                StatusBar.ClearStatusBarValue();
            }
        }

        public static NpmHistory GetPackageHistory(string package)
        {
            var processStartInfo = CreateProcessStartInfo();

            var process = Process.Start(processStartInfo);
            if (process == null)
            {
                MessageBox.Show($"{Resources.Resource.NpmPackageInstallFailureMessage}");
                return null;
            }

            process.StandardInput.WriteLine($"npm view {package}");
            process.StandardInput.Flush();
            process.StandardInput.Close();
            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            Regex regEx = new Regex(@"\{(.|\s)*\}");
            var m = regEx.Match(output);

            string json = m.Value;

            NpmHistory history = JsonConvert.DeserializeObject<NpmHistory>(json);

            return history;
        }

        private static ProcessStartInfo CreateProcessStartInfo()
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = "cmd",
                RedirectStandardInput = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                UseShellExecute = false
            };
            return processStartInfo;
        }
    }
}