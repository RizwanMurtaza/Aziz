using System.Net;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Logging;

namespace Nop.Plugin.Misc.SpireProductImport.Services;

/// <summary>
/// Represents FTP service implementation
/// </summary>
public class FtpService : IFtpService
{
    #region Fields

    private readonly ILogger<FtpService> _logger;

    #endregion

    #region Ctor

    public FtpService(ILogger<FtpService> logger)
    {
        _logger = logger;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Downloads a file from FTP server
    /// </summary>
    public async Task<bool> DownloadFileAsync(string ftpServer, string username, string password, string remoteFileName, string localFilePath)
    {
        // Use passive mode first (this works with ftp.spire.co.uk)
        var modes = new[] { true, false }; // true = passive, false = active
        using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(2)); // 2 minute timeout

        foreach (var usePassive in modes)
        {
            try
            {
                _logger.LogInformation("Starting FTP download: {RemoteFile} from {FtpServer} using {Mode} mode",
                    remoteFileName, ftpServer, usePassive ? "passive" : "active");

                var ftpUrl = $"ftp://{ftpServer}/{remoteFileName}";
                var request = (FtpWebRequest)WebRequest.Create(ftpUrl);

                request.Method = WebRequestMethods.Ftp.DownloadFile;
                request.Credentials = new NetworkCredential(username, password);
                request.UseBinary = true;
                request.UsePassive = usePassive;
                request.KeepAlive = false;
                request.Timeout = 60000; // 1 minute timeout
                request.ReadWriteTimeout = 60000;
                request.ServicePoint.ConnectionLimit = 1;

                // Enable FTPS (explicit FTPS - starts as plain FTP then upgrades to TLS)
                request.EnableSsl = true;

                // Set up certificate validation for FTPS
                ServicePointManager.ServerCertificateValidationCallback = ValidateServerCertificate;

                // Additional FTPS settings for better compatibility
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;

                using var response = await GetResponseWithTimeoutAsync(request, cts.Token);
                using var responseStream = response.GetResponseStream();
                using var fileStream = new FileStream(localFilePath, FileMode.Create, FileAccess.Write);

                await responseStream.CopyToAsync(fileStream);

                _logger.LogInformation("Successfully downloaded {RemoteFile} to {LocalFile} using {Mode} mode. Size: {Size} bytes",
                    remoteFileName, localFilePath, usePassive ? "passive" : "active", new FileInfo(localFilePath).Length);

                return true;
            }
            catch (TimeoutException)
            {
                _logger.LogWarning("FTP download timed out for {RemoteFile} from {FtpServer} using {Mode} mode",
                    remoteFileName, ftpServer, usePassive ? "passive" : "active");

                if (usePassive)
                {
                    _logger.LogError("Failed to download file {RemoteFile} from {FtpServer} - both modes timed out",
                        remoteFileName, ftpServer);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Failed to download file {RemoteFile} from {FtpServer} using {Mode} mode: {Error}",
                    remoteFileName, ftpServer, usePassive ? "passive" : "active", ex.Message);

                // If this was the last mode to try, log as error
                if (usePassive)
                {
                    _logger.LogError("Failed to download file {RemoteFile} from {FtpServer} using both active and passive modes",
                        remoteFileName, ftpServer);
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Tests FTP connection with progressive approach
    /// </summary>
    public async Task<bool> TestConnectionAsync(string ftpServer, string username, string password)
    {
        _logger.LogInformation("Starting comprehensive FTP connection test to {FtpServer}", ftpServer);

        // First, try to resolve DNS and basic connectivity
        try
        {
            var hostEntry = await Dns.GetHostEntryAsync(ftpServer);
            _logger.LogInformation("DNS resolution successful for {FtpServer}. IP: {IpAddress}",
                ftpServer, hostEntry.AddressList.FirstOrDefault()?.ToString() ?? "Unknown");
        }
        catch (Exception ex)
        {
            _logger.LogError("DNS resolution failed for {FtpServer}: {Error}", ftpServer, ex.Message);
            return false;
        }

        // Try different connection strategies
        var strategies = new[]
        {
            new { UseSsl = false, UsePassive = false, Name = "Plain FTP Active" },
            new { UseSsl = false, UsePassive = true, Name = "Plain FTP Passive" },
            new { UseSsl = true, UsePassive = false, Name = "FTPS Active" },
            new { UseSsl = true, UsePassive = true, Name = "FTPS Passive" }
        };

        foreach (var strategy in strategies)
        {
            try
            {
                _logger.LogInformation("Attempting {Strategy} connection to {FtpServer}", strategy.Name, ftpServer);

                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15)); // Shorter timeout per attempt
                var ftpUrl = $"ftp://{ftpServer}/";
                var request = (FtpWebRequest)WebRequest.Create(ftpUrl);

                request.Method = WebRequestMethods.Ftp.ListDirectory;
                request.Credentials = new NetworkCredential(username, password);
                request.UsePassive = strategy.UsePassive;
                request.KeepAlive = false;
                request.Timeout = 10000; // 10 seconds per request
                request.ReadWriteTimeout = 10000;
                request.ServicePoint.ConnectionLimit = 1;

                if (strategy.UseSsl)
                {
                    // Configure ServicePointManager before making request
                    ServicePointManager.ServerCertificateValidationCallback = ValidateServerCertificate;
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;

                    // Enable explicit FTPS
                    request.EnableSsl = true;
                    request.ServicePoint.Expect100Continue = false;
                    request.ServicePoint.UseNagleAlgorithm = false;

                    _logger.LogInformation("SSL enabled for this attempt");
                }
                else
                {
                    _logger.LogInformation("Plain FTP (no SSL) for this attempt");
                }

                var startTime = DateTime.UtcNow;
                using var response = await GetResponseWithTimeoutAsync(request, cts.Token);
                var elapsed = DateTime.UtcNow - startTime;

                _logger.LogInformation("{Strategy} connection successful in {ElapsedMs}ms. Status: {Status}",
                    strategy.Name, elapsed.TotalMilliseconds, response.StatusDescription);

                var success = response.StatusCode == FtpStatusCode.OpeningData ||
                             response.StatusCode == FtpStatusCode.DataAlreadyOpen ||
                             response.StatusCode == FtpStatusCode.ClosingData;

                if (success)
                {
                    _logger.LogInformation("Connection test PASSED using {Strategy}", strategy.Name);
                    return true;
                }
            }
            catch (TimeoutException)
            {
                _logger.LogWarning("{Strategy} connection timed out for {FtpServer}", strategy.Name, ftpServer);
            }
            catch (WebException webEx)
            {
                _logger.LogWarning("{Strategy} connection failed for {FtpServer}: {Error} (Status: {Status})",
                    strategy.Name, ftpServer, webEx.Message, webEx.Status);
            }
            catch (Exception ex)
            {
                _logger.LogWarning("{Strategy} connection failed for {FtpServer}: {Error}",
                    strategy.Name, ftpServer, ex.Message);
            }
        }

        _logger.LogError("All connection strategies failed for {FtpServer}", ftpServer);
        return false;
    }

    /// <summary>
    /// Gets the list of files from FTP server
    /// </summary>
    public async Task<List<string>> GetFileListAsync(string ftpServer, string username, string password)
    {
        var fileList = new List<string>();
        // Use passive mode first (this works with ftp.spire.co.uk)
        var modes = new[] { true, false }; // true = passive, false = active
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30)); // 30 second timeout

        foreach (var usePassive in modes)
        {
            try
            {
                _logger.LogInformation("Getting file list from {FtpServer} using {Mode} mode", ftpServer, usePassive ? "passive" : "active");

                var ftpUrl = $"ftp://{ftpServer}/";
                var request = (FtpWebRequest)WebRequest.Create(ftpUrl);

                request.Method = WebRequestMethods.Ftp.ListDirectory;
                request.Credentials = new NetworkCredential(username, password);
                request.UsePassive = usePassive;
                request.KeepAlive = false;
                request.Timeout = 30000;
                request.ReadWriteTimeout = 30000;
                request.ServicePoint.ConnectionLimit = 1;

                // Enable FTPS (explicit FTPS - starts as plain FTP then upgrades to TLS)
                request.EnableSsl = true;

                // Set up certificate validation for FTPS
                ServicePointManager.ServerCertificateValidationCallback = ValidateServerCertificate;

                // Additional FTPS settings for better compatibility
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;

                using var response = await GetResponseWithTimeoutAsync(request, cts.Token);
                using var responseStream = response.GetResponseStream();
                using var reader = new StreamReader(responseStream);

                string line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    if (!string.IsNullOrWhiteSpace(line))
                        fileList.Add(line.Trim());
                }

                _logger.LogInformation("Retrieved {Count} files from {FtpServer} using {Mode} mode", fileList.Count, ftpServer, usePassive ? "passive" : "active");
                return fileList; // Success, return immediately
            }
            catch (TimeoutException)
            {
                _logger.LogWarning("FTP file list operation timed out for {FtpServer} using {Mode} mode", ftpServer, usePassive ? "passive" : "active");

                if (usePassive)
                {
                    _logger.LogError("Failed to get file list from {FtpServer} - both modes timed out", ftpServer);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Failed to get file list from {FtpServer} using {Mode} mode: {Error}", ftpServer, usePassive ? "passive" : "active", ex.Message);

                if (usePassive)
                {
                    _logger.LogError(ex, "Failed to get file list from {FtpServer} using both modes: {Error}", ftpServer, ex.Message);
                }
            }
        }

        return fileList; // Return empty list if all attempts failed
    }

    /// <summary>
    /// Gets file information from FTP server
    /// </summary>
    public async Task<(long size, DateTime lastModified)> GetFileInfoAsync(string ftpServer, string username, string password, string fileName)
    {
        // Use passive mode first (this works with ftp.spire.co.uk)
        var modes = new[] { true, false }; // true = passive, false = active
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30)); // 30 second timeout

        foreach (var usePassive in modes)
        {
            try
            {
                _logger.LogDebug("Getting file info for {FileName} from {FtpServer} using {Mode} mode", fileName, ftpServer, usePassive ? "passive" : "active");

                var ftpUrl = $"ftp://{ftpServer}/{fileName}";

                // Get file size
                var sizeRequest = (FtpWebRequest)WebRequest.Create(ftpUrl);
                sizeRequest.Method = WebRequestMethods.Ftp.GetFileSize;
                sizeRequest.Credentials = new NetworkCredential(username, password);
                sizeRequest.UsePassive = usePassive;
                sizeRequest.KeepAlive = false;
                sizeRequest.Timeout = 30000;
                sizeRequest.ReadWriteTimeout = 30000;
                sizeRequest.ServicePoint.ConnectionLimit = 1;

                // Enable FTPS (explicit FTPS - starts as plain FTP then upgrades to TLS)
                sizeRequest.EnableSsl = true;

                // Set up certificate validation for FTPS
                ServicePointManager.ServerCertificateValidationCallback = ValidateServerCertificate;

                long fileSize;
                using (var sizeResponse = await GetResponseWithTimeoutAsync(sizeRequest, cts.Token))
                {
                    fileSize = sizeResponse.ContentLength;
                }

                // Get last modified date
                var dateRequest = (FtpWebRequest)WebRequest.Create(ftpUrl);
                dateRequest.Method = WebRequestMethods.Ftp.GetDateTimestamp;
                dateRequest.Credentials = new NetworkCredential(username, password);
                dateRequest.UsePassive = usePassive;
                dateRequest.KeepAlive = false;
                dateRequest.Timeout = 30000;
                dateRequest.ReadWriteTimeout = 30000;
                dateRequest.ServicePoint.ConnectionLimit = 1;

                // Enable FTPS (explicit FTPS - starts as plain FTP then upgrades to TLS)
                dateRequest.EnableSsl = true;

                // Set up certificate validation for FTPS
                ServicePointManager.ServerCertificateValidationCallback = ValidateServerCertificate;

                DateTime lastModified;
                using (var dateResponse = await GetResponseWithTimeoutAsync(dateRequest, cts.Token))
                {
                    lastModified = dateResponse.LastModified;
                }

                _logger.LogDebug("File info for {FileName}: Size={Size} bytes, LastModified={LastModified} using {Mode} mode",
                    fileName, fileSize, lastModified, usePassive ? "passive" : "active");

                return (fileSize, lastModified);
            }
            catch (TimeoutException)
            {
                _logger.LogWarning("FTP file info operation timed out for {FileName} from {FtpServer} using {Mode} mode", fileName, ftpServer, usePassive ? "passive" : "active");

                if (usePassive)
                {
                    _logger.LogError("Failed to get file info for {FileName} from {FtpServer} - both modes timed out", fileName, ftpServer);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Failed to get file info for {FileName} from {FtpServer} using {Mode} mode: {Error}",
                    fileName, ftpServer, usePassive ? "passive" : "active", ex.Message);

                if (usePassive)
                {
                    _logger.LogError(ex, "Failed to get file info for {FileName} from {FtpServer} using both modes: {Error}",
                        fileName, ftpServer, ex.Message);
                }
            }
        }

        return (0, DateTime.MinValue); // Return default values if all attempts failed
    }

    /// <summary>
    /// Downloads a file from FTP server with 24-hour local caching
    /// </summary>
    public async Task<string> DownloadFileWithCachingAsync(string ftpServer, string username, string password, string remoteFileName, string cacheDirectory)
    {
        try
        {
            // Create cache directory if it doesn't exist
            if (!Directory.Exists(cacheDirectory))
            {
                Directory.CreateDirectory(cacheDirectory);
                _logger.LogInformation("Created cache directory: {CacheDirectory}", cacheDirectory);
            }

            // Generate cache file path
            var cacheFileName = $"{remoteFileName}_{DateTime.UtcNow:yyyyMMdd}";
            var cacheFilePath = Path.Combine(cacheDirectory, cacheFileName);

            // Check if cached file exists and is less than 24 hours old
            if (File.Exists(cacheFilePath))
            {
                var fileInfo = new FileInfo(cacheFilePath);
                var ageHours = (DateTime.UtcNow - fileInfo.CreationTimeUtc).TotalHours;

                if (ageHours < 24)
                {
                    _logger.LogInformation("Using cached file {CacheFile}, age: {Age:F1} hours", cacheFilePath, ageHours);
                    return cacheFilePath;
                }
                else
                {
                    _logger.LogInformation("Cache file {CacheFile} is {Age:F1} hours old, re-downloading", cacheFilePath, ageHours);

                    // Delete old cache file
                    try
                    {
                        File.Delete(cacheFilePath);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning("Failed to delete old cache file {CacheFile}: {Error}", cacheFilePath, ex.Message);
                    }
                }
            }

            // Clean up old cache files (older than 48 hours)
            CleanupOldCacheFiles(cacheDirectory, remoteFileName);

            // Download fresh file from FTP
            _logger.LogInformation("Downloading fresh file {RemoteFile} from {FtpServer} to cache", remoteFileName, ftpServer);

            var tempFilePath = cacheFilePath + ".tmp";
            var downloadSuccess = await DownloadFileAsync(ftpServer, username, password, remoteFileName, tempFilePath);

            if (downloadSuccess && File.Exists(tempFilePath))
            {
                // Move temp file to final cache location
                File.Move(tempFilePath, cacheFilePath);
                _logger.LogInformation("Successfully cached file {RemoteFile} to {CacheFile}", remoteFileName, cacheFilePath);
                return cacheFilePath;
            }
            else
            {
                _logger.LogError("Failed to download file {RemoteFile} from {FtpServer}", remoteFileName, ftpServer);

                // Clean up temp file if it exists
                if (File.Exists(tempFilePath))
                {
                    try
                    {
                        File.Delete(tempFilePath);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning("Failed to clean up temp file {TempFile}: {Error}", tempFilePath, ex.Message);
                    }
                }

                return null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to download file with caching {RemoteFile} from {FtpServer}: {Error}", remoteFileName, ftpServer, ex.Message);
            return null;
        }
    }

    /// <summary>
    /// Cleans up old cache files for a specific remote file
    /// </summary>
    private void CleanupOldCacheFiles(string cacheDirectory, string remoteFileName)
    {
        try
        {
            if (!Directory.Exists(cacheDirectory))
                return;

            var pattern = $"{remoteFileName}_*";
            var cacheFiles = Directory.GetFiles(cacheDirectory, pattern);

            foreach (var file in cacheFiles)
            {
                var fileInfo = new FileInfo(file);
                var ageHours = (DateTime.UtcNow - fileInfo.CreationTimeUtc).TotalHours;

                if (ageHours > 48) // Delete files older than 48 hours
                {
                    try
                    {
                        File.Delete(file);
                        _logger.LogDebug("Deleted old cache file {File}, age: {Age:F1} hours", file, ageHours);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning("Failed to delete old cache file {File}: {Error}", file, ex.Message);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Failed to cleanup old cache files in {CacheDirectory}: {Error}", cacheDirectory, ex.Message);
        }
    }

    /// <summary>
    /// Gets FTP response with timeout handling to prevent hanging
    /// </summary>
    private async Task<FtpWebResponse> GetResponseWithTimeoutAsync(FtpWebRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var task = request.GetResponseAsync();
            var completedTask = await Task.WhenAny(task, Task.Delay(Timeout.Infinite, cancellationToken));

            if (completedTask == task)
            {
                return (FtpWebResponse)await task;
            }
            else
            {
                // Timeout occurred
                request.Abort();
                throw new TimeoutException("FTP operation timed out");
            }
        }
        catch (WebException ex) when (ex.Status == WebExceptionStatus.RequestCanceled)
        {
            throw new TimeoutException("FTP operation was cancelled due to timeout");
        }
    }

    /// <summary>
    /// Validates the server certificate for FTPS connections
    /// </summary>
    private bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
    {
        // For ftp.spire.co.uk, we'll accept the certificate even with policy errors
        // In production, you might want to be more restrictive
        if (sslPolicyErrors == SslPolicyErrors.None)
        {
            _logger.LogInformation("SSL certificate validation successful");
            return true;
        }

        // Check if it's specifically ftp.spire.co.uk
        var request = sender as FtpWebRequest;
        var host = request?.RequestUri?.Host ?? "unknown";

        if (host.Contains("spire.co.uk", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Accepting certificate for {Host} with SSL policy errors: {Errors}. Certificate Subject: {Subject}",
                host, sslPolicyErrors, certificate?.Subject ?? "Unknown");
            return true;
        }

        _logger.LogError("Certificate validation failed for {Host}: {Errors}. Certificate Subject: {Subject}",
            host, sslPolicyErrors, certificate?.Subject ?? "Unknown");
        return false;
    }

    #endregion
}