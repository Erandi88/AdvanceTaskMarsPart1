using NUnit.Framework;
using OpenQA.Selenium;
using MarsAdvanced.Tests.Helpers;
using MarsAdvanced.Tests.Models;
using MarsAdvanced.Tests.Pages;

namespace MarsAdvanced.Tests;

[TestFixture]
public class ProfileSettingsTests
{
    private IWebDriver? driver;
    private ProfileSettingsPage? profileSettingsPage;

    [SetUp]
    public void SetUp()
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

    

    [Test]
    public void UpdateAvailabilityPersistsAfterRefresh()
    {
        var page = profileSettingsPage
            ?? throw new InvalidOperationException(
                "Profile settings page was not created."
            );

        var browser = driver
            ?? throw new InvalidOperationException(
                "Browser was not created."
            );

        string filePath = Path.Combine(
            TestContext.CurrentContext.TestDirectory,
            "TestData",
            "ProfileSettings",
            "UpdateAvailabilityPersistsAfterRefresh.json"
        );

        AvailabilityData testData =
            JsonReader.Read<AvailabilityData>(filePath);

        Assert.That(
            testData.InitialAvailability,
            Is.EqualTo("Part Time")
        );

        Assert.That(
            testData.UpdatedAvailability,
            Is.EqualTo("Full Time")
        );

        // Capture the original settings before changing anything.
        string originalAvailability = page.GetAvailability();
        string originalHours = page.GetHours();
        string originalEarnTarget = page.GetEarnTarget();

        Console.WriteLine("original Availability : " + originalAvailability);
        Console.WriteLine("original Hours : " + originalHours);
        Console.WriteLine("original Earn Target : " + originalEarnTarget);


        Assert.That(
            originalAvailability,
            Is.AnyOf("Part Time", "Full Time"),
            "An existing Availability value is required for restoration."
        );

        try
        {
            // Arrange: establish the required starting value.
            if (originalAvailability != testData.InitialAvailability)
            {
                page.OpenAvailabilityEditor();
                page.SelectAvailability(testData.InitialAvailability);

                Assert.That(
                    page.IsAvailabilityUpdatedMessageDisplayed(),
                    Is.True
                );

                Assert.That(
                    page.WaitForAvailability(
                        testData.InitialAvailability
                    ),
                    Is.True,
                    "Setup should save Part Time."
                );
            }

            // Act: change Part Time to Full Time.
            page.OpenAvailabilityEditor();
            page.SelectAvailability(testData.UpdatedAvailability);

            Assert.That(
                page.IsAvailabilityUpdatedMessageDisplayed(),
                Is.True,
                "The 'Availability updated' message should appear."
            );

            Assert.That(
                page.WaitForAvailability(testData.UpdatedAvailability),
                Is.True,
                "Full Time should be displayed before refresh."
            );

            browser.Navigate().Refresh();

            // Assert: the update persists and other settings are preserved.
            Assert.That(
                page.WaitForAvailability(testData.UpdatedAvailability),
                Is.True,
                "Full Time should remain after refresh."
            );

            Assert.That(
                page.GetHours(),
                Is.EqualTo(originalHours),
                "Changing Availability must not change Hours."
            );

            Assert.That(
                page.GetEarnTarget(),
                Is.EqualTo(originalEarnTarget),
                "Changing Availability must not change Earn Target."
            );
        }
        finally
        {
            // Reload to close any open editor and read the saved value.
            browser.Navigate().Refresh();
            Console.WriteLine("Page Availability at finally : " + page.GetAvailability());
            if (page.GetAvailability() != originalAvailability)
            {
                page.OpenAvailabilityEditor();
                page.SelectAvailability(originalAvailability);

                Assert.That(
                    page.IsAvailabilityUpdatedMessageDisplayed(),
                    Is.True,
                    "Restoring Availability should show a save message."
                );

                Assert.That(
                    page.WaitForAvailability(originalAvailability),
                    Is.True,
                    "Cleanup should restore the original Availability."
                );

                browser.Navigate().Refresh();
            }

            Assert.That(
                page.WaitForAvailability(originalAvailability),
                Is.True,
                "The original Availability should remain after cleanup."
            );
        }
    }

    [Test]
    public void UpdateAvailabilityToPartTimePersistsAfterRefresh()
    {
        var page = profileSettingsPage
            ?? throw new InvalidOperationException(
                "Profile settings page was not created."
            );

        var browser = driver
            ?? throw new InvalidOperationException(
                "Browser was not created."
            );

        string filePath = Path.Combine(
            TestContext.CurrentContext.TestDirectory,
            "TestData",
            "ProfileSettings",
            "UpdateAvailabilityToPartTimePersistsAfterRefresh.json"
        );

        AvailabilityData testData =
            JsonReader.Read<AvailabilityData>(filePath);

        Assert.That(
            testData.InitialAvailability,
            Is.EqualTo("Full Time")
        );

        Assert.That(
            testData.UpdatedAvailability,
            Is.EqualTo("Part Time")
        );

        // Capture the original settings before changing anything.
        string originalAvailability = page.GetAvailability();
        string originalHours = page.GetHours();
        string originalEarnTarget = page.GetEarnTarget();

        Console.WriteLine("original Availability : " + originalAvailability);
        Console.WriteLine("original Hours : " + originalHours);
        Console.WriteLine("original Earn Target : " + originalEarnTarget);


        Assert.That(
            originalAvailability,
            Is.AnyOf("Part Time", "Full Time"),
            "An existing Availability value is required for restoration."
        );

        try
        {
            // Arrange: establish the required starting value.
            if (originalAvailability != testData.InitialAvailability)
            {
                page.OpenAvailabilityEditor();
                page.SelectAvailability(testData.InitialAvailability);

                Assert.That(
                    page.IsAvailabilityUpdatedMessageDisplayed(),
                    Is.True
                );

                Assert.That(
                    page.WaitForAvailability(
                        testData.InitialAvailability
                    ),
                    Is.True,
                    "Setup should save Part Time."
                );
            }

            // Act: change Part Time to Full Time.
            page.OpenAvailabilityEditor();
            page.SelectAvailability(testData.UpdatedAvailability);

            Assert.That(
                page.IsAvailabilityUpdatedMessageDisplayed(),
                Is.True,
                "The 'Availability updated' message should appear."
            );

            Assert.That(
                page.WaitForAvailability(testData.UpdatedAvailability),
                Is.True,
                "Full Time should be displayed before refresh."
            );

            browser.Navigate().Refresh();

            // Assert: the update persists and other settings are preserved.
            Assert.That(
                page.WaitForAvailability(testData.UpdatedAvailability),
                Is.True,
                "Full Time should remain after refresh."
            );

            Assert.That(
                page.GetHours(),
                Is.EqualTo(originalHours),
                "Changing Availability must not change Hours."
            );

            Assert.That(
                page.GetEarnTarget(),
                Is.EqualTo(originalEarnTarget),
                "Changing Availability must not change Earn Target."
            );
        }
        finally
        {
            // Reload to close any open editor and read the saved value.
            browser.Navigate().Refresh();
            Console.WriteLine("Page Availability at finally : " + page.GetAvailability());
            if (page.GetAvailability() != originalAvailability)
            {
                page.OpenAvailabilityEditor();
                page.SelectAvailability(originalAvailability);

                Assert.That(
                    page.IsAvailabilityUpdatedMessageDisplayed(),
                    Is.True,
                    "Restoring Availability should show a save message."
                );

                Assert.That(
                    page.WaitForAvailability(originalAvailability),
                    Is.True,
                    "Cleanup should restore the original Availability."
                );

                browser.Navigate().Refresh();
            }

            Assert.That(
                page.WaitForAvailability(originalAvailability),
                Is.True,
                "The original Availability should remain after cleanup."
            );
        }
    }

    [Test]
    public void CloseAvailabilityWithoutChanges()
    {
        var page = profileSettingsPage
            ?? throw new InvalidOperationException(
                "Profile settings page was not created."
            );

        var browser = driver
            ?? throw new InvalidOperationException(
                "Browser was not created."
            );

        string filePath = Path.Combine(
            TestContext.CurrentContext.TestDirectory,
            "TestData",
            "ProfileSettings",
            "CloseAvailabilityWithoutChanges.json"
        );

        AvailabilityData testData =
            JsonReader.Read<AvailabilityData>(filePath);

        Assert.That(
            testData.InitialAvailability,
            Is.EqualTo("Part Time")
        );

        string originalAvailability = page.GetAvailability();
        string originalHours = page.GetHours();
        string originalEarnTarget = page.GetEarnTarget();

        Assert.That(
            originalAvailability,
            Is.AnyOf("Part Time", "Full Time"),
            "An existing Availability value is required for restoration."
        );

        try
        {
            // Arrange: establish Part Time if necessary.
            if (originalAvailability != testData.InitialAvailability)
            {
                page.OpenAvailabilityEditor();
                page.SelectAvailability(testData.InitialAvailability);

                Assert.That(
                    page.IsAvailabilityUpdatedMessageDisplayed(),
                    Is.True
                );

                Assert.That(
                    page.WaitForAvailability(testData.InitialAvailability),
                    Is.True,
                    "Setup should save Part Time."
                );
            }

            // Act: open and close the editor without selecting anything.
            page.OpenAvailabilityEditor();
            page.CloseAvailabilityEditor();

            // Assert: closing the editor preserves all three values.
            Assert.That(
                page.GetAvailability(),
                Is.EqualTo(testData.InitialAvailability),
                "Availability should remain unchanged."
            );

            Assert.That(
                page.GetHours(),
                Is.EqualTo(originalHours),
                "Hours should remain unchanged."
            );

            Assert.That(
                page.GetEarnTarget(),
                Is.EqualTo(originalEarnTarget),
                "Earn Target should remain unchanged."
            );

            // Also verify that the saved values survive a reload.
            browser.Navigate().Refresh();

            Assert.That(
                page.WaitForAvailability(testData.InitialAvailability),
                Is.True,
                "Availability should remain unchanged after refresh."
            );

            Assert.That(
                page.GetHours(),
                Is.EqualTo(originalHours)
            );

            Assert.That(
                page.GetEarnTarget(),
                Is.EqualTo(originalEarnTarget)
            );
        }
        finally
        {
            browser.Navigate().Refresh();

            if (page.GetAvailability() != originalAvailability)
            {
                page.OpenAvailabilityEditor();
                page.SelectAvailability(originalAvailability);

                Assert.That(
                    page.IsAvailabilityUpdatedMessageDisplayed(),
                    Is.True,
                    "Restoration should show a save message."
                );

                Assert.That(
                    page.WaitForAvailability(originalAvailability),
                    Is.True,
                    "Cleanup should restore the original Availability."
                );

                browser.Navigate().Refresh();
            }

            Assert.That(
                page.WaitForAvailability(originalAvailability),
                Is.True,
                "The original Availability should remain after cleanup."
            );
        }
    }

    [TearDown]
    public void TearDown()
    {
        driver?.Dispose();
        driver = null;
        profileSettingsPage = null;
    }
}