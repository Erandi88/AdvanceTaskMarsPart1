using AventStack.ExtentReports;
using MarsAdvanced.Tests.Helpers;
using MarsAdvanced.Tests.Models;
using MarsAdvanced.Tests.Pages;
using NUnit.Framework;
using OpenQA.Selenium;

namespace MarsAdvanced.Tests;

[TestFixture]
[NonParallelizable]
public class NotificationTests
{
    private IWebDriver? driver;
    private ExtentTest? reportTest;

    [SetUp]
    public void SetUp()
    {
        reportTest = ExtentReportManager.CreateTest(
            TestContext.CurrentContext.Test.FullName
        );

        string? email = Environment.GetEnvironmentVariable("MARS_EMAIL");
        string? password = Environment.GetEnvironmentVariable("MARS_PASSWORD");

        Assert.That(email, Is.Not.Null.And.Not.Empty, "MARS_EMAIL is not set.");
        Assert.That(password, Is.Not.Null.And.Not.Empty, "MARS_PASSWORD is not set.");

        driver = CommonDriver.CreateChromeDriver();
        driver.Navigate().GoToUrl("http://localhost:5003/Home");

        var loginPage = new LoginPage(driver);
        loginPage.Login(email!, password!);

        Assert.That(
            loginPage.IsLoggedInSuccessfully(),
            Is.True,
            "Login should succeed before testing notifications."
        );
    }

    [Test]
    public void OpenEmptyNotificationMenu()
    {
        var browser = driver
            ?? throw new InvalidOperationException("Browser was not created.");

        string filePath = Path.Combine(
            TestContext.CurrentContext.TestDirectory,
            "TestData", "Notification", "OpenEmptyNotificationMenu.json"
        );

        var data = JsonReader.Read<NotificationData>(filePath);

        Assert.That(data.ExpectedMessage, Is.Not.Null.And.Not.Empty);

        var notifications = new NotificationPage(browser);
        notifications.Open();

        Assert.That(
            notifications.WaitForMenuText(),
            Is.EqualTo(data.ExpectedMessage),
            "An empty notification menu should display its empty-state message."
        );
    }

    [TearDown]
    public void TearDown()
    {
        try
        {
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
                    System.Net.WebUtility.HtmlEncode(exception.Message)
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