using NUnit.Framework;
using MarsAdvanced.Tests.Helpers;

namespace MarsAdvanced.Tests
{
    [SetUpFixture]
    public class TestRunSetup
    {
        [OneTimeSetUp]
        public void BeforeTestRun()
        {
            // Start one shared report before the test fixtures run.
            ExtentReportManager.Initialize();
        }

        [OneTimeTearDown]
        public void AfterTestRun()
        {
            // Save the report after the test fixtures finish.
            ExtentReportManager.Flush();

            TestContext.Progress.WriteLine(
                $"Report saved to: {ExtentReportManager.ReportPath}"
            );

            TestContext.AddTestAttachment(
                ExtentReportManager.ReportPath,
                "Extent HTML report"
            );
        }
    }
}