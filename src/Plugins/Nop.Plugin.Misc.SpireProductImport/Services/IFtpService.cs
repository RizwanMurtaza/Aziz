namespace Nop.Plugin.Misc.SpireProductImport.Services;

/// <summary>
/// Represents a service for FTP operations
/// </summary>
public interface IFtpService
{
    /// <summary>
    /// Downloads a file from FTP server
    /// </summary>
    /// <param name="ftpServer">FTP server address</param>
    /// <param name="username">FTP username</param>
    /// <param name="password">FTP password</param>
    /// <param name="remoteFileName">Remote file name to download</param>
    /// <param name="localFilePath">Local path to save the downloaded file</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains true if successful, false otherwise</returns>
    Task<bool> DownloadFileAsync(string ftpServer, string username, string password, string remoteFileName, string localFilePath);

    /// <summary>
    /// Tests FTP connection
    /// </summary>
    /// <param name="ftpServer">FTP server address</param>
    /// <param name="username">FTP username</param>
    /// <param name="password">FTP password</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains true if connection is successful, false otherwise</returns>
    Task<bool> TestConnectionAsync(string ftpServer, string username, string password);

    /// <summary>
    /// Gets the list of files from FTP server
    /// </summary>
    /// <param name="ftpServer">FTP server address</param>
    /// <param name="username">FTP username</param>
    /// <param name="password">FTP password</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains list of file names</returns>
    Task<List<string>> GetFileListAsync(string ftpServer, string username, string password);

    /// <summary>
    /// Gets file information from FTP server
    /// </summary>
    /// <param name="ftpServer">FTP server address</param>
    /// <param name="username">FTP username</param>
    /// <param name="password">FTP password</param>
    /// <param name="fileName">File name to get info for</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains file info (size, last modified)</returns>
    Task<(long size, DateTime lastModified)> GetFileInfoAsync(string ftpServer, string username, string password, string fileName);

    /// <summary>
    /// Downloads a file from FTP server with 24-hour local caching
    /// </summary>
    /// <param name="ftpServer">FTP server address</param>
    /// <param name="username">FTP username</param>
    /// <param name="password">FTP password</param>
    /// <param name="remoteFileName">Remote file name to download</param>
    /// <param name="cacheDirectory">Local directory to cache files</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the local file path if successful, null otherwise</returns>
    Task<string> DownloadFileWithCachingAsync(string ftpServer, string username, string password, string remoteFileName, string cacheDirectory);
}