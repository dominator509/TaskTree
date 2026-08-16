// SPEC-DERIVED-PHASE3E  HALT #9/#10/#11/#12
// Architecture.md Sections 9.2.4-9.2.5 local file drop for trivial reports.
// Gap #267/#268/#269: file-drop path/schema and unredacted-write validation need Phase 5C/Architecture documentation.

using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using TaskTree.Core.Models;

namespace TaskTree.Modules.BugReporter
{
    /// <summary>Writes redacted bug reports to local file drop output.</summary>
    public sealed class FileDropAdapter : IBugReportDeliveryAdapter
    {
        private readonly string _outputRoot;
        private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true };
        public FileDropAdapter() : this(DefaultRoot()) { }
        public FileDropAdapter(string outputRoot) => _outputRoot = string.IsNullOrWhiteSpace(outputRoot) ? throw new ArgumentException("Output root required.", nameof(outputRoot)) : outputRoot;
        public string Channel => "FileDrop";
        public async Task<BugReportDeliveryResult> DeliverAsync(BugReport report)
        {
            if (report is null) throw new ArgumentNullException(nameof(report));
            if (!report.Redacted) return new BugReportDeliveryResult(false, Channel, "Report is not marked redacted.");
            Directory.CreateDirectory(_outputRoot);
            var path = Path.Combine(_outputRoot, $"{report.Id}.json");
            var temporaryPath = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
            try
            {
                await File.WriteAllTextAsync(temporaryPath, JsonSerializer.Serialize(report, JsonOptions)).ConfigureAwait(false);
                File.Move(temporaryPath, path, overwrite: true);
            }
            catch
            {
                TryDelete(temporaryPath);
                throw;
            }
            return new BugReportDeliveryResult(true, Channel, path);
        }
        private static void TryDelete(string path){try{if(File.Exists(path))File.Delete(path);}catch{} }
        private static string DefaultRoot(){var local=Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);return Path.Combine(local,"TaskTree","bugreports","out");}
    }
}
