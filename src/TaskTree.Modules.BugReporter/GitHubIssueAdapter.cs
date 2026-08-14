// SPEC-DERIVED-PHASE3E  HALT #6/#7/#8
// Architecture.md Sections 4.8, 9.2.4, and 9.2.6 live GitHub Issues delivery.
// Runtime repository/token configuration is environment-only and fail-closed.

using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TaskTree.Core.Enums;
using TaskTree.Core.Models;

namespace TaskTree.Modules.BugReporter
{
    /// <summary>GitHub Issues delivery adapter with runtime-only authentication.</summary>
    public sealed class GitHubIssueAdapter : IBugReportDeliveryAdapter
    {
        public string Channel => "GitHub";
        public string GetLabel(BugSeverity severity) => severity switch
        {
            BugSeverity.Critical => "critical",
            BugSeverity.High => "high",
            BugSeverity.Normal => "bug",
            BugSeverity.Low => "enhancement",
            _ => "bug",
        };
        public async Task<BugReportDeliveryResult> DeliverAsync(BugReport report)
        {
            ArgumentNullException.ThrowIfNull(report);
            if (!report.Redacted)
                return new BugReportDeliveryResult(false, Channel, "Report is not marked redacted.");

            var repository = Environment.GetEnvironmentVariable("TASKTREE_GITHUB_REPOSITORY");
            var token = Environment.GetEnvironmentVariable("TASKTREE_GITHUB_TOKEN");
            if (string.IsNullOrWhiteSpace(repository) || string.IsNullOrWhiteSpace(token))
                return new BugReportDeliveryResult(false, Channel, "GitHub configuration is unavailable.");

            var segments = repository.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (segments.Length != 2 || segments.Any(string.IsNullOrWhiteSpace))
                return new BugReportDeliveryResult(false, Channel, "GitHub repository must use owner/name format.");

            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, $"https://api.github.com/repos/{segments[0]}/{segments[1]}/issues");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                request.Headers.UserAgent.ParseAdd("TaskTree/1.0");
                var payload = new
                {
                    title = report.Title,
                    body = FormatBody(report),
                    labels = new[] { GetLabel(report.Severity) },
                };
                request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                using var response = await HttpClientHolder.Instance.SendAsync(request).ConfigureAwait(false);
                if (response.IsSuccessStatusCode)
                    return new BugReportDeliveryResult(true, Channel, "Delivered as a GitHub issue.");
                return new BugReportDeliveryResult(false, Channel, $"GitHub delivery failed with HTTP {(int)response.StatusCode}.");
            }
            catch (Exception ex)
            {
                return new BugReportDeliveryResult(false, Channel, $"GitHub delivery failed: {ex.GetType().Name}.");
            }
        }

        private static string FormatBody(BugReport report) =>
            $"TaskTree bug report {report.Id}\n" +
            $"Severity: {report.Severity}\n" +
            $"Expected: {report.Description.Expected}\n" +
            $"Actual: {report.Description.Actual}\n" +
            $"OS: {report.Environment.Os}\n" +
            $"App version: {report.Environment.AppVersion}\n" +
            $"Build: {report.Environment.Build}\n" +
            $"Fingerprint: {report.Fingerprint}";

        private static class HttpClientHolder
        {
            internal static readonly HttpClient Instance = new();
        }
    }
}
