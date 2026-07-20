#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ====================================================================

namespace TelegramBotFramework.Utilities;

using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using TelegramBotFramework.Integration;

/// <summary>
/// Helper class for downloading files from Telegram's servers.
/// </summary>
public static class FileDownloadHelper
{
    /// <summary>
    /// Downloads a file from Telegram's servers to the specified destination.
    /// </summary>
    /// <param name="apiClient">Telegram API client instance</param>
    /// <param name="fileId">File identifier to download</param>
    /// <param name="destinationPath">Full path where the file should be saved</param>
    /// <param name="maxSizeBytes">Maximum file size in bytes (0 = no limit)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if download succeeded, false otherwise</returns>
    /// <exception cref="ArgumentException">Thrown when fileId or destinationPath is invalid</exception>
    public static async Task<bool> DownloadFileAsync(
        ITelegramApiClient apiClient,
        string fileId,
        string destinationPath,
        long maxSizeBytes = 0,
        CancellationToken cancellationToken = default)
    {
        if (apiClient == null)
            throw new ArgumentNullException(nameof(apiClient));

        if (string.IsNullOrWhiteSpace(fileId))
            throw new ArgumentException("File ID cannot be empty", nameof(fileId));

        if (string.IsNullOrWhiteSpace(destinationPath))
            throw new ArgumentException("Destination path cannot be empty", nameof(destinationPath));

        try
        {
            // Step 1: Get file information
            var fileInfo = await apiClient.GetFileAsync(fileId, cancellationToken).ConfigureAwait(false);

            if (fileInfo == null)
            {
                Console.Error.WriteLine($"Failed to get file info for file_id: {fileId}");
                return false;
            }

            // Step 2: Check file size against limit
            if (maxSizeBytes > 0 && fileInfo.FileSize > maxSizeBytes)
            {
                Console.Error.WriteLine($"File size {fileInfo.FileSize} bytes exceeds maximum allowed {maxSizeBytes} bytes");
                return false;
            }

            // Step 3: Construct download URL
            var baseUrl = "https://api.telegram.org/file/bot";
            var fileUrl = $"{baseUrl}{GetBotTokenFromClient(apiClient)}/{fileInfo.FilePath}";

            // Step 4: Download the file
            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(fileUrl, cancellationToken).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                // Ensure destination directory exists
                var directory = Path.GetDirectoryName(destinationPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Save the file
                var fileBytes = await response.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
                await File.WriteAllBytesAsync(destinationPath, fileBytes, cancellationToken).ConfigureAwait(false);

                return true;
            }

            Console.Error.WriteLine($"Failed to download file: Status={response.StatusCode}, FilePath={fileInfo.FilePath}");
            return false;
        }
        catch (OperationCanceledException)
        {
            Console.Error.WriteLine("File download operation was cancelled");
            return false;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error downloading file: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Extracts the bot token from an ITelegramApiClient instance.
    /// This is a workaround since the token is private in the implementation.
    /// </summary>
    /// <param name="apiClient">Telegram API client</param>
    /// <returns>Bot token string</returns>
    private static string GetBotTokenFromClient(ITelegramApiClient apiClient)
    {
        if (apiClient is TelegramApiClient telegramApiClient)
        {
            // Use reflection to get the private _botToken field
            var field = typeof(TelegramApiClient).GetField("_botToken",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                return field.GetValue(telegramApiClient) as string ?? string.Empty;
            }
        }

        throw new InvalidOperationException("Could not extract bot token from ITelegramApiClient");
    }
}