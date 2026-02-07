namespace EDA.APPLICATION.DTOs
{
    public class AppVersionInfo
    {
        public string Version { get; set; } = null!;
        public DateTime ReleaseDate { get; set; }
        public string? DownloadUrl { get; set; }
        public string? StoreUrl { get; set; }
        public string? Changelog { get; set; }
        public bool IsMandatory { get; set; }
    }

    public class UpdateCheckResult
    {
        public bool UpdateAvailable { get; set; }
        public string? CurrentVersion { get; set; }
        public string? LatestVersion { get; set; }
        public string? DownloadUrl { get; set; }
        public string? StoreUrl { get; set; }
        public string? Changelog { get; set; }
        public bool IsMandatory { get; set; }
    }
}
