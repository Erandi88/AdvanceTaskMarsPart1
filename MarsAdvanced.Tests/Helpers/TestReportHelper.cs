using AventStack.ExtentReports;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using OpenQA.Selenium;
using System.Net;

namespace MarsAdvanced.Tests.Helpers
{
    public static class TestReportHelper
    {
        public static void RecordResult(
            ExtentTest? reportTest,
            IWebDriver? driver)
        {
            if (reportTest == null)
            {
                return;
            }

            var result = TestContext.CurrentContext.Result;
            string message = WebUtility.HtmlEncode(result.Message);

            switch (result.Outcome.Status)
            {
                case TestStatus.Passed:
                    reportTest.Pass("Test passed.");
                    break;

                case TestStatus.Failed:
                    reportTest.Fail(message);

                    if (!string.IsNullOrWhiteSpace(result.StackTrace))
                    {
                        reportTest.Info(
                            WebUtility.HtmlEncode(result.StackTrace)
                        );
                    }

                    CaptureScreenshot(reportTest, driver);
                    break;

                case TestStatus.Skipped:
                    reportTest.Skip(message);
                    break;

                case TestStatus.Inconclusive:
                    reportTest.Skip("Inconclusive: " + message);
                    break;

                case TestStatus.Warning:
                    reportTest.Warning(message);
                    break;

                default:
                    reportTest.Info(
                        $"NUnit outcome: {result.Outcome}"
                    );
                    break;
            }
        }

        private static void CaptureScreenshot(
            ExtentTest reportTest,
            IWebDriver? driver)
        {
            if (driver is not ITakesScreenshot screenshotDriver)
            {
                reportTest.Info(
                    "No browser was available for a screenshot."
                );

                return;
            }

            try
            {
                string base64 = screenshotDriver
                    .GetScreenshot()
                    .AsBase64EncodedString;

                reportTest.AddScreenCaptureFromBase64String(
                    base64,
                    "Browser at test failure"
                );
            }
            catch (Exception exception)
            {
                // Preserve the original failure if screenshot capture fails.
                reportTest.Info(
                    "Screenshot could not be captured: " +
                    WebUtility.HtmlEncode(exception.Message)
                );
            }
        }
    }
}