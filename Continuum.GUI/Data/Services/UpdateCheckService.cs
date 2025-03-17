using Continuum.Common.Logging;
using Continuum.GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Continuum.GUI.Services
{
    public class UpdateCheckService
    {
        const string REPO_OWNER_NAME = "dasorik";
        const string REPO_NAME = "continuum-mod-manager";

        public bool HasCheckedForUpdate { get; set; }

        public Version? GetCurrentVersion()
        {
            return Assembly.GetExecutingAssembly().GetName().Version;
        }

        public string GetRepoURL()
        {
            return $"https://github.com/{REPO_OWNER_NAME}/{REPO_NAME}/releases/latest";
        }

        public async Task<Version?> GetLatestGitHubReleaseVersion()
        {
            using HttpClient client = new();
            client.DefaultRequestHeaders.Add("User-Agent", "CSharpApp");

            try
            {
                string url = $"https://api.github.com/repos/{REPO_OWNER_NAME}/{REPO_NAME}/releases/latest";
                string json = await client.GetStringAsync(url);
                using JsonDocument doc = JsonDocument.Parse(json);

                string? tagName = doc.RootElement.GetProperty("tag_name").GetString();
                Version? version = ExtractVersionFromTag(tagName);

                return version;
            }
            catch (Exception ex)
            {
                Logger.Log($"Error fetching latest version: {ex.Message}", LogSeverity.Error);
            }

            return null;
        }

        private Version? ExtractVersionFromTag(string? tag)
        {
            if (tag == null)
            {
                return null;
            }

            Match match = Regex.Match(tag, @"(\d+\.\d+\.\d+)");
            return match.Success ? new Version(match.Groups[1].Value) : null;
        }
    }
}
