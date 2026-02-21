using System.Security.Cryptography;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using VRCX.Core.WebLocalFileAccess.Services.Abstraction;

namespace VRCX.Core.WebLocalFileAccess.Services;

public sealed class WebLocalFileAccessService
{
    private readonly Aes _aes;

    private readonly ILogger _logger = Log.ForContext<WebLocalFileAccessService>();
    private readonly IWebLocalFileUriProvider _uriProvider;

    public WebLocalFileAccessService(IServiceProvider serviceProvider)
    {
        _uriProvider = serviceProvider.GetService<IWebLocalFileUriProvider>() ?? new MockWebLocalFileUriProvider();

        _aes = Aes.Create();
        _aes.GenerateIV();
        _aes.GenerateKey();
    }

    public Uri GetEncryptedUri(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException("File not found", filePath);

        if (!_uriProvider.SupportEncryption)
        {
            return _uriProvider.GetFileUri(filePath);
        }

        var encryptedFilePath = GetEncryptedFilePathAsync(filePath);
        return _uriProvider.GetFileUri(encryptedFilePath);
    }

    public string? GetDecryptedFilePath(Uri uri)
    {
        if (!_uriProvider.SupportEncryption)
        {
            return _uriProvider.ParseFileKey(uri);
        }

        try
        {
            var encryptedBase64 = _uriProvider.ParseFileKey(uri);

            return GetDecryptedFilePath(encryptedBase64);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to parse file key from URI: {Uri}", uri);
            return null;
        }
    }

    private string GetEncryptedFilePathAsync(string filePath)
    {
        var encryptor = _aes.CreateEncryptor(_aes.Key, _aes.IV);

        using var msEncrypt = new MemoryStream();
        using var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
        using (var swEncrypt = new StreamWriter(csEncrypt))
        {
            swEncrypt.Write(filePath);
        }

        var encryptedBytes = msEncrypt.ToArray();
        var encryptedBase64 = Convert.ToHexStringLower(encryptedBytes);
        return encryptedBase64;
    }

    private string? GetDecryptedFilePath(string encryptedBase64)
    {
        try
        {
            var encryptedBytes = Convert.FromHexString(encryptedBase64);
            var decryptor = _aes.CreateDecryptor(_aes.Key, _aes.IV);

            using var msDecrypt = new MemoryStream(encryptedBytes);
            using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
            using var srDecrypt = new StreamReader(csDecrypt);
            return srDecrypt.ReadToEnd();
        }
        catch (Exception ex)
        {
            _logger.Warning(ex, "Got invalid encrypted file path: {EncryptedBase64}", encryptedBase64);
            return null;
        }
    }
}