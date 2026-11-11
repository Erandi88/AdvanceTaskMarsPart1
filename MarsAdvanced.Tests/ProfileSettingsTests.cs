using NUnit.Framework;
using OpenQA.Selenium;
using MarsAdvanced.Tests.Helpers;
using MarsAdvanced.Tests.Models;
using MarsAdvanced.Tests.Pages;
using AventStack.ExtentReports;

namespace MarsAdvanced.Tests;

[TestFixture]
public class ProfileSettingsTests
{
    private IWebDriver? driver;
    private ProfileSettingsPage? profileSettingsPage;
    private ExtentTest? reportTest;

    [SetUp]
    public void SetUp()
    {
        reportTest = ExtentReportManager.CreateTest(
            TestContext.CurrentContext.Test.FullName
        );

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

        driver = CommonDriver.CreateChromeDriver();
        driver.Navigate().GoToUrl("http://localhost:5003/Home");

        var loginPage = new LoginPage(driver);
        loginPage.Login(email!, password!);

        Assert.That(
            loginPage.IsLoggedInSuccessfully(),
            Is.True,
            "Login should succeed before testing profile settings."
        );

        profileSettingsPage = new ProfileSettingsPage(driver);
    }



    [TestCase(
    "UpdateAvailabilityPersistsAfterRefresh.json",
    TestName = "Availability_PartTime_To_FullTime")]

    [TestCase(
    "UpdateAvailabilityToPartTimePersistsAfterRefresh.json",
    TestName = "Availability_FullTime_To_PartTime")]
    public void UpdateAvailabilityPersistsAfterRefresh(string jsonFileName)
    {
        var page = profileSettingsPage
            ?? throw new InvalidOperationException("Profile settings page was not created.");
        var browser = driver
            ?? throw new InvalidOperationException("Browser was not created.");

        string filePath = Path.Combine(
            TestContext.CurrentContext.TestDirectory,
            "TestData", "ProfileSettings", jsonFileName);
        var data = JsonReader.Read<AvailabilityData>(filePath);

        string originalAvailability = page.GetAvailability();
        string originalHours = page.GetHours();
        string originalEarnTarget = page.GetEarnTarget();
        ValidateAvailabilityChange(data, originalAvailability);

        try
        {
            if (originalAvailability != data.InitialAvailability)
                SaveAvailabilityAndVerify(page, data.InitialAvailability, "Setup");

            SaveAvailabilityAndVerify(page, data.UpdatedAvailability, "Update");
            browser.Navigate().Refresh();

            Assert.That(page.WaitForAvailability(data.UpdatedAvailability), Is.True,
                "The updated Availability should remain after refresh.");
            Assert.That(page.GetHours(), Is.EqualTo(originalHours),
                "Changing Availability must not change Hours.");
            Assert.That(page.GetEarnTarget(), Is.EqualTo(originalEarnTarget),
                "Changing Availability must not change Earn Target.");
        }
        finally
        {
            RestoreAvailability(browser, page, originalAvailability);
        }
    }

    private static void ValidateAvailabilityChange(
        AvailabilityData data,
        string originalAvailability)
    {
        string[] supportedValues = { "Part Time", "Full Time" };

        Assert.That(supportedValues, Does.Contain(data.InitialAvailability),
            "The initial Availability must be a supported option.");
        Assert.That(supportedValues, Does.Contain(data.UpdatedAvailability),
            "The updated Availability must be a supported option.");
        Assert.That(data.UpdatedAvailability, Is.Not.EqualTo(data.InitialAvailability),
            "This scenario requires a change in Availability.");
        Assert.That(supportedValues, Does.Contain(originalAvailability),
            "A supported original value is required for restoration.");
    }

    private static void SaveAvailabilityAndVerify(
        ProfileSettingsPage page,
        string value,
        string stage)
    {
        page.OpenAvailabilityEditor();
        page.SelectAvailability(value);

        Assert.That(page.IsAvailabilityUpdatedMessageDisplayed(), Is.True,
            $"{stage}: the save notification should appear.");
        Assert.That(page.WaitForAvailability(value), Is.True,
            $"{stage}: Availability should become '{value}'.");
    }

    private static void RestoreAvailability(
        IWebDriver browser,
        ProfileSettingsPage page,
        string originalAvailability)
    {
        browser.Navigate().Refresh();

        if (page.GetAvailability() != originalAvailability)
        {
            SaveAvailabilityAndVerify(page, originalAvailability, "Cleanup");
            browser.Navigate().Refresh();
        }

        Assert.That(page.WaitForAvailability(originalAvailability), Is.True,
            "The original Availability should remain after cleanup.");
    }

    [Test]
    public void CloseAvailabilityWithoutChanges()
    {
        var page = profileSettingsPage
            ?? throw new InvalidOperationException("Profile settings page was not created.");
        var browser = driver
            ?? throw new InvalidOperationException("Browser was not created.");

        string filePath = Path.Combine(
            TestContext.CurrentContext.TestDirectory,
            "TestData", "ProfileSettings", "CloseAvailabilityWithoutChanges.json");
        var data = JsonReader.Read<AvailabilityData>(filePath);

        Assert.That(data.InitialAvailability, Is.EqualTo("Part Time"));

        string originalAvailability = page.GetAvailability();
        string originalHours = page.GetHours();
        string originalEarnTarget = page.GetEarnTarget();

        Assert.That(originalAvailability, Is.AnyOf("Part Time", "Full Time"),
            "An existing Availability value is required for restoration.");

        try
        {
            if (originalAvailability != data.InitialAvailability)
                SaveAvailabilityAndVerify(page, data.InitialAvailability, "Setup");

            page.OpenAvailabilityEditor();
            page.CloseAvailabilityEditor();

            Assert.That(page.GetAvailability(), Is.EqualTo(data.InitialAvailability),
                "Availability should remain unchanged.");
            Assert.That(page.GetHours(), Is.EqualTo(originalHours),
                "Hours should remain unchanged.");
            Assert.That(page.GetEarnTarget(), Is.EqualTo(originalEarnTarget),
                "Earn Target should remain unchanged.");

            browser.Navigate().Refresh();

            Assert.That(page.WaitForAvailability(data.InitialAvailability), Is.True,
                "Availability should remain unchanged after refresh.");
            Assert.That(page.GetHours(), Is.EqualTo(originalHours));
            Assert.That(page.GetEarnTarget(), Is.EqualTo(originalEarnTarget));
        }
        finally
        {
            RestoreAvailability(browser, page, originalAvailability);
        }
    }

    /* Hours */

    [TestCase(
    "UpdateHoursPersistsAfterRefresh.json",
    TestName = "Hours_MoreThan30_To_LessThan30")]
    [TestCase(
    "UpdateHoursToAsNeededPersistsAfterRefresh.json",
    TestName = "Hours_LessThan30_To_AsNeeded")]
    [TestCase(
    "UpdateHoursToMoreThan30PersistsAfterRefresh.json",
    TestName = "Hours_AsNeeded_To_MoreThan30")]
    public void UpdateHoursPersistsAfterRefresh(string jsonFileName)
    {
        var page = profileSettingsPage
            ?? throw new InvalidOperationException("Profile settings page was not created.");
        var browser = driver
            ?? throw new InvalidOperationException("Browser was not created.");

        string filePath = Path.Combine(
            TestContext.CurrentContext.TestDirectory,
            "TestData", "ProfileSettings", jsonFileName);
        var data = JsonReader.Read<HoursData>(filePath);

        string originalHours = page.GetHours();
        string originalAvailability = page.GetAvailability();
        string originalEarnTarget = page.GetEarnTarget();
        ValidateHoursChange(data, originalHours);

        try
        {
            if (originalHours != data.InitialHours)
                SaveHoursAndVerify(page, data.InitialHours, "Setup");

            SaveHoursAndVerify(page, data.UpdatedHours, "Update");
            browser.Navigate().Refresh();

            Assert.That(page.WaitForHours(data.UpdatedHours), Is.True,
                "The updated Hours should remain after refresh.");
            Assert.That(page.GetAvailability(), Is.EqualTo(originalAvailability),
                "Changing Hours must not change Availability.");
            Assert.That(page.GetEarnTarget(), Is.EqualTo(originalEarnTarget),
                "Changing Hours must not change Earn Target.");
        }
        finally
        {
            RestoreHours(browser, page, originalHours);
        }
    }

    private static void ValidateHoursChange(
        HoursData data,
        string originalHours)
    {
        string[] supportedHours =
        {
        "Less than 30hours a week",
        "More than 30hours a week",
        "As needed"
    };

        Assert.That(supportedHours, Does.Contain(data.InitialHours),
            "The initial Hours must be a supported option.");
        Assert.That(supportedHours, Does.Contain(data.UpdatedHours),
            "The updated Hours must be a supported option.");
        Assert.That(data.UpdatedHours, Is.Not.EqualTo(data.InitialHours),
            "This scenario requires a change in Hours.");
        Assert.That(supportedHours, Does.Contain(originalHours),
            "An existing supported Hours value is required for restoration.");
    }

    private static void SaveHoursAndVerify(
        ProfileSettingsPage page,
        string value,
        string stage)
    {
        page.OpenHoursEditor();
        page.SelectHours(value);

        Assert.That(page.IsAvailabilityUpdatedMessageDisplayed(), Is.True,
            $"{stage}: the save notification should appear.");
        Assert.That(page.WaitForHours(value), Is.True,
            $"{stage}: Hours should become '{value}'.");
    }

    private static void RestoreHours(
        IWebDriver browser,
        ProfileSettingsPage page,
        string originalHours)
    {
        browser.Navigate().Refresh();

        if (page.GetHours() != originalHours)
        {
            SaveHoursAndVerify(page, originalHours, "Cleanup");
            browser.Navigate().Refresh();
        }

        Assert.That(page.WaitForHours(originalHours), Is.True,
            "The original Hours should remain after cleanup.");
    }

    [Test]
    public void CloseHoursWithoutChanges()
    {
        var page = profileSettingsPage
            ?? throw new InvalidOperationException("Profile settings page was not created.");
        var browser = driver
            ?? throw new InvalidOperationException("Browser was not created.");

        string filePath = Path.Combine(
            TestContext.CurrentContext.TestDirectory,
            "TestData", "ProfileSettings", "CloseHoursWithoutChanges.json");
        var data = JsonReader.Read<HoursData>(filePath);

        Assert.That(data.InitialHours, Is.EqualTo("More than 30hours a week"));

        string originalHours = page.GetHours();
        string originalAvailability = page.GetAvailability();
        string originalEarnTarget = page.GetEarnTarget();

        Assert.That(originalHours, Is.AnyOf(
            "Less than 30hours a week", "More than 30hours a week", "As needed"),
            "A supported original Hours value is required for restoration.");

        try
        {
            if (originalHours != data.InitialHours)
                SaveHoursAndVerify(page, data.InitialHours, "Setup");

            page.OpenHoursEditor();
            page.CloseHoursEditor();

            Assert.That(page.GetHours(), Is.EqualTo(data.InitialHours),
                "Hours should remain unchanged after closing the editor.");
            Assert.That(page.GetAvailability(), Is.EqualTo(originalAvailability),
                "Availability should remain unchanged.");
            Assert.That(page.GetEarnTarget(), Is.EqualTo(originalEarnTarget),
                "Earn Target should remain unchanged.");

            browser.Navigate().Refresh();

            Assert.That(page.WaitForHours(data.InitialHours), Is.True,
                "Hours should remain unchanged after refresh.");
            Assert.That(page.GetAvailability(), Is.EqualTo(originalAvailability),
                "Availability should remain unchanged after refresh.");
            Assert.That(page.GetEarnTarget(), Is.EqualTo(originalEarnTarget),
                "Earn Target should remain unchanged after refresh.");
        }
        finally
        {
            RestoreHours(browser, page, originalHours);
        }
    }

    /* Earn Target */

    [TestCase(
    "UpdateEarnTargetToBetween500And1000.json",
    TestName = "EarnTarget_LessThan500_To_Between500And1000")]

    [TestCase(
    "UpdateEarnTargetToMoreThan1000.json",
    TestName = "EarnTarget_Between500And1000_To_MoreThan1000")]

    [TestCase(
    "UpdateEarnTargetToLessThan500.json",
    TestName = "EarnTarget_MoreThan1000_To_LessThan500")]
    public void UpdateEarnTargetPersistsAfterRefresh(string jsonFileName)
    {
        var page = profileSettingsPage
            ?? throw new InvalidOperationException("Profile settings page was not created.");
        var browser = driver
            ?? throw new InvalidOperationException("Browser was not created.");

        string filePath = Path.Combine(
            TestContext.CurrentContext.TestDirectory,
            "TestData", "ProfileSettings", jsonFileName);
        var data = JsonReader.Read<EarnTargetData>(filePath);

        string originalEarnTarget = page.GetEarnTarget();
        string originalAvailability = page.GetAvailability();
        string originalHours = page.GetHours();
        ValidateEarnTargetChange(data, originalEarnTarget);

        try
        {
            if (originalEarnTarget != data.InitialEarnTarget)
                SaveEarnTargetAndVerify(page, data.InitialEarnTarget, "Setup");

            SaveEarnTargetAndVerify(page, data.UpdatedEarnTarget, "Update");
            browser.Navigate().Refresh();

            Assert.That(page.WaitForEarnTarget(data.UpdatedEarnTarget), Is.True,
                "The updated Earn Target should remain after refresh.");
            Assert.That(page.GetAvailability(), Is.EqualTo(originalAvailability),
                "Changing Earn Target must not change Availability.");
            Assert.That(page.GetHours(), Is.EqualTo(originalHours),
                "Changing Earn Target must not change Hours.");
        }
        finally
        {
            RestoreEarnTarget(browser, page, originalEarnTarget);
        }
    }

    private static void ValidateEarnTargetChange(EarnTargetData data, string originalEarnTarget)
    {
        string[] supportedValues =
        {
        "Less than $500 per month",
        "Between $500 and $1000 per month",
        "More than $1000 per month"
        };

        Assert.That(supportedValues, Does.Contain(data.InitialEarnTarget),
            "The initial Earn Target must be a supported option.");
        Assert.That(supportedValues, Does.Contain(data.UpdatedEarnTarget),
            "The updated Earn Target must be a supported option.");
        Assert.That(data.UpdatedEarnTarget, Is.Not.EqualTo(data.InitialEarnTarget),
            "This scenario requires a change in Earn Target.");
        Assert.That(supportedValues, Does.Contain(originalEarnTarget),
            "A supported original value is required for restoration.");
    }

    private static void SaveEarnTargetAndVerify(ProfileSettingsPage page, string value, string stage)
    {
        page.OpenEarnTargetEditor();
        page.SelectEarnTarget(value);

        Assert.That(page.IsAvailabilityUpdatedMessageDisplayed(), Is.True,
            $"{stage}: the save notification should appear.");
        Assert.That(page.WaitForEarnTarget(value), Is.True,
            $"{stage}: Earn Target should become '{value}'.");
    }

    private static void RestoreEarnTarget(IWebDriver browser, ProfileSettingsPage page, string originalEarnTarget)
    {
        browser.Navigate().Refresh();

        if (page.GetEarnTarget() != originalEarnTarget)
        {
            SaveEarnTargetAndVerify(page, originalEarnTarget, "Cleanup");
            browser.Navigate().Refresh();
        }

        Assert.That(page.WaitForEarnTarget(originalEarnTarget), Is.True,
            "The original Earn Target should remain after cleanup.");
    }

    [Test]
    public void CloseEarnTargetWithoutChanges()
    {
        var page = profileSettingsPage
            ?? throw new InvalidOperationException("Profile settings page was not created.");
        var browser = driver
            ?? throw new InvalidOperationException("Browser was not created.");

        string filePath = Path.Combine(
            TestContext.CurrentContext.TestDirectory,
            "TestData", "ProfileSettings", "CloseEarnTargetWithoutChanges.json");
        var data = JsonReader.Read<EarnTargetData>(filePath);

        Assert.That(data.InitialEarnTarget, Is.EqualTo("Less than $500 per month"));

        string originalEarnTarget = page.GetEarnTarget();
        string originalAvailability = page.GetAvailability();
        string originalHours = page.GetHours();

        Assert.That(originalEarnTarget, Is.AnyOf(
            "Less than $500 per month",
            "Between $500 and $1000 per month",
            "More than $1000 per month"),
            "A supported original value is required for restoration.");

        try
        {
            if (originalEarnTarget != data.InitialEarnTarget)
                SaveEarnTargetAndVerify(page, data.InitialEarnTarget, "Setup");

            page.OpenEarnTargetEditor();
            page.CloseEarnTargetEditor();

            Assert.That(page.GetEarnTarget(), Is.EqualTo(data.InitialEarnTarget),
                "Earn Target should remain unchanged after closing.");
            Assert.That(page.GetAvailability(), Is.EqualTo(originalAvailability),
                "Availability should remain unchanged.");
            Assert.That(page.GetHours(), Is.EqualTo(originalHours),
                "Hours should remain unchanged.");

            browser.Navigate().Refresh();

            Assert.That(page.WaitForEarnTarget(data.InitialEarnTarget), Is.True,
                "Earn Target should remain unchanged after refresh.");
            Assert.That(page.GetAvailability(), Is.EqualTo(originalAvailability),
                "Availability should remain unchanged after refresh.");
            Assert.That(page.GetHours(), Is.EqualTo(originalHours),
                "Hours should remain unchanged after refresh.");
        }
        finally
        {
            RestoreEarnTarget(browser, page, originalEarnTarget);
        }
    }

    [TearDown]
    public void TearDown()
    {
        try
        {
            // Record the result and capture a screenshot if failed.
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
                // Preserve the existing field cleanup.
                driver = null;
                profileSettingsPage = null;
                reportTest = null;

                ExtentReportManager.Flush();
            }
        }
    }
}