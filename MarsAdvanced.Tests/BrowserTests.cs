using AventStack.ExtentReports;
using MarsAdvanced.Tests.Helpers;
using NUnit.Framework;
using OpenQA.Selenium;

namespace MarsAdvanced.Tests;

public class BrowserTests
{
    private IWebDriver? driver;
    private ExtentTest? reportTest;

    [SetUp]
    public void SetUp()
    {
        reportTest = ExtentReportManager.CreateTest(
            TestContext.CurrentContext.Test.FullName
        );

        driver = CommonDriver.CreateChromeDriver();
    }

    [Test]
    public void MarsHomePageCanBeOpened()
    {
        var browser = driver
            ?? throw new InvalidOperationException(
                "Browser was not created."
            );

        browser.Navigate().GoToUrl("http://localhost:5003/Home");

        Assert.That(
            browser.Url,
            Does.Contain("localhost:5003"),
            "The browser should open the local MARS application."
        );
    }

    [TearDown]
    public void TearDown()
    {
        try
        {
            // Record the result before closing the browser.
            TestReportHelper.RecordResult(reportTest, driver);
        }
        finally
        {
            try
            {
                driver?.Dispose();
            }
            catch (Exception exception)
            {
                reportTest?.Fail(
                    "Browser disposal failed: " +
                    System.Net.WebUtility.HtmlEncode(
                        exception.Message
                    )
                );

                throw;
            }
            finally
            {
                driver = null;
                reportTest = null;

                ExtentReportManager.Flush();
            }
        }
    }
}