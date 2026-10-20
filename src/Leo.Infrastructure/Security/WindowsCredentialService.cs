using System;
using Windows.Security.Credentials;

namespace Leo.Infrastructure.Security;

/// <summary>
/// Secure credential storage using Windows Credential Manager (PasswordVault).
/// Invariant: Secrets are NEVER written in plaintext to project files, configs, or logs.
/// </summary>
public static class WindowsCredentialService
{
    private const string ResourceName = "Leo_Windows_AI_Assistant";
    private const string DefaultUsername = "GeminiApiKey";

    public static void SaveSecret(string key, string secret)
    {
        if (string.IsNullOrWhiteSpace(secret))
        {
            RemoveSecret(key);
            return;
        }

        try
        {
            var vault = new PasswordVault();
            // Remove existing credential if present
            RemoveSecret(key);

            var credential = new PasswordCredential(ResourceName, string.IsNullOrWhiteSpace(key) ? DefaultUsername : key, secret);
            vault.Add(credential);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to save credential to PasswordVault: {ex.Message}");
        }
    }

    public static string? GetSecret(string key)
    {
        try
        {
            var vault = new PasswordVault();
            string username = string.IsNullOrWhiteSpace(key) ? DefaultUsername : key;
            var credential = vault.Retrieve(ResourceName, username);
            if (credential != null)
            {
                credential.RetrievePassword();
                return credential.Password;
            }
        }
        catch
        {
            // Expected if no secret is stored yet
        }
        return null;
    }

    public static void RemoveSecret(string key)
    {
        try
        {
            var vault = new PasswordVault();
            string username = string.IsNullOrWhiteSpace(key) ? DefaultUsername : key;
            var credential = vault.Retrieve(ResourceName, username);
            if (credential != null)
            {
                vault.Remove(credential);
            }
        }
        catch
        {
            // Nothing to remove
        }
    }
}
