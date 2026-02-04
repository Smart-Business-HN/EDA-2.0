using EDA.APPLICATION.DTOs;
using EDA.APPLICATION.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

namespace EDA.INFRAESTRUCTURE.Services
{
    public class UpdateCheckerService : IUpdateCheckerService
    {
        private readonly HttpClient _httpClient;
        private readonly string? _versionCheckUrl;
        private readonly bool _checkOnStartup;

        public UpdateCheckerService(IConfiguration configuration)
        {
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(10);
            _versionCheckUrl = configuration["UpdateSettings:VersionCheckUrl"];
            var checkOnStartupStr = configuration["UpdateSettings:CheckOnStartup"];
            _checkOnStartup = string.IsNullOrEmpty(checkOnStartupStr) || bool.Parse(checkOnStartupStr);
        }

        public string GetCurrentVersion()
        {
            var assembly = Assembly.GetEntryAssembly();
            var version = assembly?.GetName().Version;
            return version?.ToString() ?? "1.0.0.0";
        }

        public async Task<UpdateCheckResult> CheckForUpdatesAsync()
        {
            var result = new UpdateCheckResult
            {
                UpdateAvailable = false,
                CurrentVersion = GetCurrentVersion()
            };

            if (string.IsNullOrEmpty(_versionCheckUrl) || !_checkOnStartup)
            {
                return result;
            }

            try
            {
                var response = await _httpClient.GetStringAsync(_versionCheckUrl);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                var versionInfo = JsonSerializer.Deserialize<AppVersionInfo>(response, options);

                if (versionInfo == null)
                {
                    return result;
                }

                result.LatestVersion = versionInfo.Version;
                result.DownloadUrl = versionInfo.DownloadUrl;
                result.StoreUrl = versionInfo.StoreUrl;
                result.Changelog = versionInfo.Changelog;
                result.IsMandatory = versionInfo.IsMandatory;

                // Compare versions
                if (Version.TryParse(result.CurrentVersion, out var current) &&
                    Version.TryParse(versionInfo.Version, out var latest))
                {
                    result.UpdateAvailable = latest > current;
                }
            }
            catch (Exception)
            {
                // Silently fail - don't interrupt app startup for update check failures
            }

            return result;
        }
    }
}
