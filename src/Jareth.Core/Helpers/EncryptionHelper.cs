using System;
using System.Text;

namespace Jareth.Core.Helpers;

/// <summary>
/// Encrypts/decrypts strings using Windows DPAPI via P/Invoke.
/// Encryption is tied to the current Windows user account.
/// </summary>
public static class EncryptionHelper
{
    public static string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText)) return string.Empty;
        try
        {
            var data = Encoding.UTF8.GetBytes(plainText);
            var encrypted = System.Security.Cryptography.ProtectedData.Protect(
                data, null, System.Security.Cryptography.DataProtectionScope.CurrentUser);
            return Convert.ToBase64String(encrypted);
        }
        catch
        {
            return string.Empty;
        }
    }

    public static string Decrypt(string encryptedText)
    {
        if (string.IsNullOrEmpty(encryptedText)) return string.Empty;
        try
        {
            var data = Convert.FromBase64String(encryptedText);
            var decrypted = System.Security.Cryptography.ProtectedData.Unprotect(
                data, null, System.Security.Cryptography.DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(decrypted);
        }
        catch
        {
            return string.Empty;
        }
    }
}
