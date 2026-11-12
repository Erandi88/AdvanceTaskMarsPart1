using AventStack.ExtentReports;
using AventStack.ExtentReports.Reporter;
using NUnit.Framework;

namespace MarsAdvanced.Tests.Helpers
{
    public static class ExtentReportManager
    {
        private static ExtentReports? _report;

        public static string ReportPath { get; private set; }
            = string.Empty;

        public static void Initialize()
        {
            if (_report != null)
            {
                return;
            }

            // Give each run its own folder.
            string runFolder = Path.Combine(
                TestContext.CurrentContext.WorkDirectory,
                "Reports",
                $"{DateTime.Now:yyyyMMdd_HHmmss}_{Guid.NewGuid():N}"
            );

            Directory.CreateDirectory(runFolder);

            ReportPath = Path.Combine(
                runFolder,
                "ExtentReport.html"
            );

            var reporter = new ExtentSparkReporter(ReportPath);

            reporter.Config.DocumentTitle = "Mars Test Results";
            reporter.Config.ReportName =
                "Mars Advanced Part 1 Automation";

            _report = new ExtentReports();
            _report.AttachReporter(reporter);

            _report.AddSystemInfo("Application", "Mars");
            _report.AddSystemInfo("Browser", "Chrome");

            TestContext.Progress.WriteLine(
                $"Extent report: {ReportPath}"
            );
        }

        public static ExtentTest CreateTest(string testName)
        {
            if (_report == null)
            {
                throw new InvalidOperationException(
                    "Initialize the report before creating a test entry."
                );
            }

            return _report.CreateTest(testName);
        }

        public static void Flush()
        {
            // Write the collected results to the HTML file.
            _report?.Flush();
        }
    }
}