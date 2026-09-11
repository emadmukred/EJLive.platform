using System;
using System.Diagnostics;
using System.IO;
using System.Security.Principal;
using Microsoft.Win32;

namespace EJLive.Client.WinForms.Services
{
    public sealed class StartupRegistrationResult
    {
        public bool Success { get; set; }
        public bool RequiresAdministrator { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public static class WindowsStartupService
    {
        public const string ClientTaskName = "EJLive Client AutoStart";
        public const string CompanionRunValueName = "EJLiveClientCompanion";

        public static StartupRegistrationResult RegisterClientAutostart(string clientExePath)
        {
            if (string.IsNullOrWhiteSpace(clientExePath) || !File.Exists(clientExePath))
            {
                return new StartupRegistrationResult
                {
                    Success = false,
                    Message = "Client executable was not found: " + clientExePath
                };
            }

            if (!IsAdministrator())
            {
                return new StartupRegistrationResult
                {
                    Success = false,
                    RequiresAdministrator = true,
                    Message = "Auto-start registration requires running EJLive Installer as Administrator."
                };
            }

            var taskCommand = $"\\\"{clientExePath}\\\" --background";
            var args = $"/Create /TN \"{ClientTaskName}\" /TR \"{taskCommand}\" /SC ONSTART /RU SYSTEM /RL HIGHEST /F";
            var run = RunHidden("schtasks.exe", args);
            return new StartupRegistrationResult
            {
                Success = run.ExitCode == 0,
                Message = run.ExitCode == 0
                    ? $"Windows startup task registered: {ClientTaskName}"
                    : $"Startup task registration failed: {run.Output}"
            };
        }

        public static StartupRegistrationResult UnregisterClientAutostart()
        {
            if (!IsAdministrator())
            {
                return new StartupRegistrationResult
                {
                    Success = false,
                    RequiresAdministrator = true,
                    Message = "Auto-start removal requires Administrator rights."
                };
            }

            var run = RunHidden("schtasks.exe", $"/Delete /TN \"{ClientTaskName}\" /F");
            return new StartupRegistrationResult
            {
                Success = run.ExitCode == 0,
                Message = run.ExitCode == 0
                    ? $"Windows startup task removed: {ClientTaskName}"
                    : $"Startup task removal failed: {run.Output}"
            };
        }

        public static bool IsClientAutostartRegistered()
        {
            var run = RunHidden("schtasks.exe", $"/Query /TN \"{ClientTaskName}\"");
            return run.ExitCode == 0;
        }

        public static StartupRegistrationResult RegisterUserSessionCompanion(string clientExePath)
        {
            if (string.IsNullOrWhiteSpace(clientExePath) || !File.Exists(clientExePath))
            {
                return new StartupRegistrationResult
                {
                    Success = false,
                    Message = "Client executable was not found: " + clientExePath
                };
            }

            if (!IsAdministrator())
            {
                return new StartupRegistrationResult
                {
                    Success = false,
                    RequiresAdministrator = true,
                    Message = "Companion startup registration requires Administrator rights."
                };
            }

            try
            {
                var view = Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView.Default;
                using var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, view);
                using var runKey = baseKey.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
                if (runKey == null)
                {
                    return new StartupRegistrationResult
                    {
                        Success = false,
                        Message = "Failed to open HKLM Run key for companion registration."
                    };
                }

                var command = $"\"{clientExePath}\" --background --companion";
                runKey.SetValue(CompanionRunValueName, command, RegistryValueKind.String);
                return new StartupRegistrationResult
                {
                    Success = true,
                    Message = "User-session companion startup registered in HKLM Run."
                };
            }
            catch (Exception ex)
            {
                return new StartupRegistrationResult
                {
                    Success = false,
                    Message = "Companion startup registration failed: " + ex.Message
                };
            }
        }

        public static StartupRegistrationResult UnregisterUserSessionCompanion()
        {
            if (!IsAdministrator())
            {
                return new StartupRegistrationResult
                {
                    Success = false,
                    RequiresAdministrator = true,
                    Message = "Companion startup removal requires Administrator rights."
                };
            }

            try
            {
                var view = Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView.Default;
                using var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, view);
                using var runKey = baseKey.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
                if (runKey == null)
                {
                    return new StartupRegistrationResult
                    {
                        Success = true,
                        Message = "Companion startup was already absent."
                    };
                }

                runKey.DeleteValue(CompanionRunValueName, false);
                return new StartupRegistrationResult
                {
                    Success = true,
                    Message = "User-session companion startup removed."
                };
            }
            catch (Exception ex)
            {
                return new StartupRegistrationResult
                {
                    Success = false,
                    Message = "Companion startup removal failed: " + ex.Message
                };
            }
        }

        public static bool IsUserSessionCompanionRegistered()
        {
            try
            {
                var view = Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView.Default;
                using var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, view);
                using var runKey = baseKey.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", false);
                var value = runKey?.GetValue(CompanionRunValueName) as string;
                return !string.IsNullOrWhiteSpace(value);
            }
            catch
            {
                return false;
            }
        }

        private static bool IsAdministrator()
        {
            try
            {
                using (var identity = WindowsIdentity.GetCurrent())
                {
                    return new WindowsPrincipal(identity).IsInRole(WindowsBuiltInRole.Administrator);
                }
            }
            catch
            {
                return false;
            }
        }

        private static CommandRun RunHidden(string fileName, string arguments)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = arguments,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                using (var process = Process.Start(psi))
                {
                    if (process == null) return new CommandRun { ExitCode = -1, Output = fileName + " did not start." };
                    if (!process.WaitForExit(15000))
                    {
                        try { process.Kill(); } catch { }
                        return new CommandRun { ExitCode = -2, Output = fileName + " timed out." };
                    }

                    var output = (process.StandardOutput.ReadToEnd() + " " + process.StandardError.ReadToEnd()).Trim();
                    return new CommandRun { ExitCode = process.ExitCode, Output = output };
                }
            }
            catch (Exception ex)
            {
                return new CommandRun { ExitCode = -3, Output = ex.Message };
            }
        }

        private sealed class CommandRun
        {
            public int ExitCode { get; set; }
            public string Output { get; set; } = string.Empty;
        }
    }
}
