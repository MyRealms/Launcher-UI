using System;
using System.Runtime.InteropServices;

using GitCredentialManager;

using Launcher.Models;

using NLog;

namespace Launcher.Helpers;

public static class CredentialHelper
{
    private const string LinuxCredStoreEnv = "GCM_CREDENTIAL_STORE";

    private const string PasswordService = "passwords";

    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
    private static readonly ICredentialStore _store = CreateStore();

    private static ICredentialStore CreateStore()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) &&
            string.IsNullOrEmpty(Environment.GetEnvironmentVariable(LinuxCredStoreEnv)))
        {
            // Almost all Linux DEs use credential managers that implement the secret service
            Environment.SetEnvironmentVariable(LinuxCredStoreEnv, "secretservice");
        }

        return CredentialManager.Create("OSFRLauncher");
    }

    public static string? GetPassword(ServerInfo server)
    {
        try
        {
            return _store.Get(PasswordService, server.SavePath)?.Password;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to read the password from the OS credential store.");

            return null;
        }
    }

    public static void SavePassword(ServerInfo server, string password)
    {
        try
        {
            _store.AddOrUpdate(PasswordService, server.SavePath, password);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to write the password to the OS credential store.");
        }
    }

    public static void Clear(ServerInfo server)
    {
        try
        {
            _store.Remove(PasswordService, server.SavePath);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to remove the password from the OS credential store.");
        }
    }
}
