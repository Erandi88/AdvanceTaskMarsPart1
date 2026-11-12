using AventStack.ExtentReports;
using MarsAdvanced.Tests.Helpers;
using MarsAdvanced.Tests.Pages;
using NUnit.Framework;
using OpenQA.Selenium;

namespace MarsAdvanced.Tests;

public class LoginTests
{
    private IWebDriver? driver;
    private ExtentTest? reportTest;

    [SetUp]
    public void SetUp()
    {
        // Create the report entry before browser setup.
        reportTest = ExtentReportManager.CreateTest(
            TestContext.CurrentContext.Test.FullName
        );

        driver = CommonDriver.CreateChromeDriver();
        driver.Navigate().GoToUrl("http://localhost:5003/Home");
    }

    [Test]
    public void UserCanLogInWithValidCredentials()
    {
        string? email =
            Environment.GetEnvironmentVariable("MARS_EMAIL");

        string? password =
            Environment.GetEnvironmentVariable("MARS_PASSWORD");

        Assert.That(
            email,
            Is.Not.Null.And.Not.Empty,
            "MARS_EMAIL is not set."
        );

        Assert.That(
            password,
            Is.Not.Null.And.Not.Empty,
            "MARS_PASSWORD is not set."
        );

        var browser = driver
            ?? throw new InvalidOperationException(
                "Browser was not created."
            );

        // Login remains the action under test.
        var loginPage = new LoginPage(browser);
        loginPage.Login(email!, password!);

        Assert.That(
            loginPage.IsLoggedInSuccessfully(),
            Is.True,
            "The user should reach the Profile page and see Sign Out."
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