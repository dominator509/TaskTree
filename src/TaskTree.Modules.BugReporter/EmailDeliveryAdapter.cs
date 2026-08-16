// SPEC-DERIVED-PHASE3E  HALT #3/#4/#5
// Architecture.md Sections 4.8 and 9.2.6 live SMTP delivery.
// Runtime configuration is supplied by the deployment environment; missing
// values fail closed and never become source-controlled settings.

using System;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using TaskTree.Core.Models;

namespace TaskTree.Modules.BugReporter
{
    /// <summary>SMTP delivery adapter with runtime-only configuration.</summary>
    public sealed class EmailDeliveryAdapter : IBugReportDeliveryAdapter
    {
        private static readonly TimeSpan DeliveryTimeout = TimeSpan.FromSeconds(5);
        public string Channel => "Email";
        public async Task<BugReportDeliveryResult> DeliverAsync(BugReport report)
        {
            ArgumentNullException.ThrowIfNull(report);
            if (!report.Redacted)
                return new BugReportDeliveryResult(false, Channel, "Report is not marked redacted.");

            var host = Environment.GetEnvironmentVariable("TASKTREE_SMTP_HOST");
            var from = Environment.GetEnvironmentVariable("TASKTREE_SMTP_FROM");
            var to = Environment.GetEnvironmentVariable("TASKTREE_SMTP_TO");
            if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(from) || string.IsNullOrWhiteSpace(to))
                return new BugReportDeliveryResult(false, Channel, "SMTP configuration is unavailable.");
            if (string.Equals(Environment.GetEnvironmentVariable("TASKTREE_SMTP_TLS"), "false", StringComparison.OrdinalIgnoreCase))
                return new BugReportDeliveryResult(false, Channel, "SMTP TLS is required.");

            if (!int.TryParse(Environment.GetEnvironmentVariable("TASKTREE_SMTP_PORT"), out var port)) port = 587;
            if (port is < 1 or > 65535)
                return new BugReportDeliveryResult(false, Channel, "SMTP port is invalid.");

            var username = Environment.GetEnvironmentVariable("TASKTREE_SMTP_USERNAME");
            var password = Environment.GetEnvironmentVariable("TASKTREE_SMTP_PASSWORD");
            if (string.IsNullOrWhiteSpace(username) != string.IsNullOrWhiteSpace(password))
                return new BugReportDeliveryResult(false, Channel, "SMTP credentials are incomplete.");

            try
            {
                using var client = new SmtpClient(host, port)
                {
                    EnableSsl = true,
                    UseDefaultCredentials = false,
                    Timeout = (int)DeliveryTimeout.TotalMilliseconds,
                };
                if (!string.IsNullOrWhiteSpace(username))
                    client.Credentials = new NetworkCredential(username, password);

                using var message = new MailMessage(from, to)
                {
                    Subject = $"[TaskTree] {report.Title}",
                    Body = FormatBody(report),
                    IsBodyHtml = false,
                };
                using var timeout = new CancellationTokenSource(DeliveryTimeout);
                await client.SendMailAsync(message, timeout.Token).ConfigureAwait(false);
                return new BugReportDeliveryResult(true, Channel, "Delivered via SMTP.");
            }
            catch (OperationCanceledException)
            {
                return new BugReportDeliveryResult(false, Channel, "SMTP delivery timed out.");
            }
            catch (Exception ex)
            {
                return new BugReportDeliveryResult(false, Channel, $"SMTP delivery failed: {ex.GetType().Name}.");
            }
        }

        private static string FormatBody(BugReport report) =>
            $"TaskTree bug report {report.Id}{Environment.NewLine}" +
            $"Severity: {report.Severity}{Environment.NewLine}" +
            $"Title: {report.Title}{Environment.NewLine}" +
            $"Expected: {report.Description.Expected}{Environment.NewLine}" +
            $"Actual: {report.Description.Actual}{Environment.NewLine}" +
            $"OS: {report.Environment.Os}{Environment.NewLine}" +
            $"App version: {report.Environment.AppVersion}{Environment.NewLine}" +
            $"Build: {report.Environment.Build}{Environment.NewLine}" +
            $"Fingerprint: {report.Fingerprint}";
    }
}
