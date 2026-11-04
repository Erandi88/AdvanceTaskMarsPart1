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



    [TestCase(
        "UpdateAvailabilityPersistsAfterRefresh.json",
        TestName = "Availability_PartTime_To_FullTime"
    )]

    [TestCase(
        "UpdateAvailabilityToPartTimePersistsAfterRefresh.json",
        TestName = "Availability_FullTime_To_PartTime"
    )]
    public void UpdateAvailabilityPersistsAfterRefresh(string jsonFileName)
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
            jsonFileName
        );

        AvailabilityData testData =
            JsonReader.Read<AvailabilityData>(filePath);

        string[] supportedValues = { "Part Time", "Full Time" };

        Assert.That(
            supportedValues,
            Does.Contain(testData.InitialAvailability),
            "The initial Availability must be a supported option."
        );

        Assert.That(
            supportedValues,
            Does.Contain(testData.UpdatedAvailability),
            "The updated Availability must be a supported option."
        );

        Assert.That(
            testData.UpdatedAvailability,
            Is.Not.EqualTo(testData.InitialAvailability),
            "This scenario requires a change in Availability."
        );

        // Capture the original settings.
        string originalAvailability = page.GetAvailability();
        string originalHours = page.GetHours();
        string originalEarnTarget = page.GetEarnTarget();

        Assert.That(
            supportedValues,
            Does.Contain(originalAvailability),
            "A supported original value is required for restoration."
        );

        try
        {
            // Arrange: establish this scenario's starting value.
            if (originalAvailability != testData.InitialAvailability)
            {
                page.OpenAvailabilityEditor();
                page.SelectAvailability(testData.InitialAvailability);

                Assert.That(
                    page.IsAvailabilityUpdatedMessageDisplayed(),
                    Is.True,
                    "Setup should display the save notification."
                );

                Assert.That(
                    page.WaitForAvailability(
                        testData.InitialAvailability
                    ),
                    Is.True,
                    $"Setup should save '{testData.InitialAvailability}'."
                );
            }

            // Act: select this scenario's updated value.
            page.OpenAvailabilityEditor();
            page.SelectAvailability(testData.UpdatedAvailability);

            Assert.That(
                page.IsAvailabilityUpdatedMessageDisplayed(),
                Is.True,
                "The save notification should appear."
            );

            Assert.That(
                page.WaitForAvailability(testData.UpdatedAvailability),
                Is.True,
                $"'{testData.UpdatedAvailability}' should appear before refresh."
            );

            browser.Navigate().Refresh();

            // Assert: verify persistence and preserve other settings.
            Assert.That(
                page.WaitForAvailability(testData.UpdatedAvailability),
                Is.True,
                $"'{testData.UpdatedAvailability}' should remain after refresh."
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
            browser.Navigate().Refresh();

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

    /* Hours */

    [TestCase(
        "UpdateHoursPersistsAfterRefresh.json",
        TestName = "Hours_MoreThan30_To_LessThan30"
    )]

    [TestCase(
        "UpdateHoursToAsNeededPersistsAfterRefresh.json",
        TestName = "Hours_LessThan30_To_AsNeeded"
    )]

    [TestCase(
        "UpdateHoursToMoreThan30PersistsAfterRefresh.json",
        TestName = "Hours_AsNeeded_To_MoreThan30"
    )]
    public void UpdateHoursPersistsAfterRefresh(string jsonFileName)
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
            jsonFileName
        );

        HoursData testData = JsonReader.Read<HoursData>(filePath);

        string[] supportedHours =
        {
        "Less than 30hours a week",
        "More than 30hours a week",
        "As needed"
    };

        Assert.That(
            supportedHours,
            Does.Contain(testData.InitialHours),
            "The initial Hours must be a supported option."
        );

        Assert.That(
            supportedHours,
            Does.Contain(testData.UpdatedHours),
            "The updated Hours must be a supported option."
        );

        Assert.That(
            testData.UpdatedHours,
            Is.Not.EqualTo(testData.InitialHours),
            "This scenario requires a change in Hours."
        );

        // Capture the original settings for comparison and cleanup.
        string originalHours = page.GetHours();
        string originalAvailability = page.GetAvailability();
        string originalEarnTarget = page.GetEarnTarget();

        Assert.That(
            supportedHours,
            Does.Contain(originalHours),
            "An existing supported Hours value is required for restoration."
        );

        try
        {
            // Arrange: establish this scenario's starting value.
            if (originalHours != testData.InitialHours)
            {
                page.OpenHoursEditor();
                page.SelectHours(testData.InitialHours);

                Assert.That(
                    page.IsAvailabilityUpdatedMessageDisplayed(),
                    Is.True,
                    "Setup should display the save notification."
                );

                Assert.That(
                    page.WaitForHours(testData.InitialHours),
                    Is.True,
                    $"Setup should save '{testData.InitialHours}'."
                );
            }

            // Act: select this scenario's updated value.
            page.OpenHoursEditor();
            page.SelectHours(testData.UpdatedHours);

            Assert.That(
                page.IsAvailabilityUpdatedMessageDisplayed(),
                Is.True,
                "The save notification should appear."
            );

            Assert.That(
                page.WaitForHours(testData.UpdatedHours),
                Is.True,
                $"'{testData.UpdatedHours}' should appear before refresh."
            );

            browser.Navigate().Refresh();

            // Assert: verify persistence and preserve other settings.
            Assert.That(
                page.WaitForHours(testData.UpdatedHours),
                Is.True,
                $"'{testData.UpdatedHours}' should remain after refresh."
            );

            Assert.That(
                page.GetAvailability(),
                Is.EqualTo(originalAvailability),
                "Changing Hours must not change Availability."
            );

            Assert.That(
                page.GetEarnTarget(),
                Is.EqualTo(originalEarnTarget),
                "Changing Hours must not change Earn Target."
            );
        }
        finally
        {
            browser.Navigate().Refresh();

            if (page.GetHours() != originalHours)
            {
                page.OpenHoursEditor();
                page.SelectHours(originalHours);

                Assert.That(
                    page.IsAvailabilityUpdatedMessageDisplayed(),
                    Is.True,
                    "Restoring Hours should display a save notification."
                );

                Assert.That(
                    page.WaitForHours(originalHours),
                    Is.True,
                    "Cleanup should restore the original Hours."
                );

                browser.Navigate().Refresh();
            }

            Assert.That(
                page.WaitForHours(originalHours),
                Is.True,
                "The original Hours should remain after cleanup."
            );
        }
    }

    [Test]
    public void CloseHoursWithoutChanges()
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
            "CloseHoursWithoutChanges.json"
        );

        HoursData testData = JsonReader.Read<HoursData>(filePath);

        Assert.That(
            testData.InitialHours,
            Is.EqualTo("More than 30hours a week")
        );

        // Remember the original settings before changing anything.
        string originalHours = page.GetHours();
        string originalAvailability = page.GetAvailability();
        string originalEarnTarget = page.GetEarnTarget();

        Assert.That(
            originalHours,
            Is.AnyOf(
                "Less than 30hours a week",
                "More than 30hours a week",
                "As needed"
            ),
            "A supported original Hours value is required for restoration."
        );

        try
        {
            // Arrange: establish the required starting value.
            if (originalHours != testData.InitialHours)
            {
                page.OpenHoursEditor();
                page.SelectHours(testData.InitialHours);

                Assert.That(
                    page.IsAvailabilityUpdatedMessageDisplayed(),
                    Is.True,
                    "Setup should display the save notification."
                );

                Assert.That(
                    page.WaitForHours(testData.InitialHours),
                    Is.True,
                    "Setup should save the initial Hours value."
                );
            }

            // Act: open and close Hours without selecting another option.
            page.OpenHoursEditor();
            page.CloseHoursEditor();

            // Assert: closing preserves all three settings.
            Assert.That(
                page.GetHours(),
                Is.EqualTo(testData.InitialHours),
                "Hours should remain unchanged after closing the editor."
            );

            Assert.That(
                page.GetAvailability(),
                Is.EqualTo(originalAvailability),
                "Availability should remain unchanged."
            );

            Assert.That(
                page.GetEarnTarget(),
                Is.EqualTo(originalEarnTarget),
                "Earn Target should remain unchanged."
            );

            browser.Navigate().Refresh();

            // Verify the same values after reload.
            Assert.That(
                page.WaitForHours(testData.InitialHours),
                Is.True,
                "Hours should remain unchanged after refresh."
            );

            Assert.That(
                page.GetAvailability(),
                Is.EqualTo(originalAvailability),
                "Availability should remain unchanged after refresh."
            );

            Assert.That(
                page.GetEarnTarget(),
                Is.EqualTo(originalEarnTarget),
                "Earn Target should remain unchanged after refresh."
            );
        }
        finally
        {
            browser.Navigate().Refresh();

            if (page.GetHours() != originalHours)
            {
                page.OpenHoursEditor();
                page.SelectHours(originalHours);

                Assert.That(
                    page.IsAvailabilityUpdatedMessageDisplayed(),
                    Is.True,
                    "Restoring Hours should display a save notification."
                );

                Assert.That(
                    page.WaitForHours(originalHours),
                    Is.True,
                    "Cleanup should restore the original Hours."
                );

                browser.Navigate().Refresh();
            }

            Assert.That(
                page.WaitForHours(originalHours),
                Is.True,
                "The original Hours should remain after cleanup."
            );
        }
    }

    /* Earn Target */

    [TestCase(
        "UpdateEarnTargetToBetween500And1000.json",
        TestName = "EarnTarget_LessThan500_To_Between500And1000"
    )]

    [TestCase(
        "UpdateEarnTargetToMoreThan1000.json",
        TestName = "EarnTarget_Between500And1000_To_MoreThan1000"
    )]

    [TestCase(
        "UpdateEarnTargetToLessThan500.json",
        TestName = "EarnTarget_MoreThan1000_To_LessThan500"
    )]

    public void UpdateEarnTargetPersistsAfterRefresh(string jsonFileName)
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
            jsonFileName
        );

        EarnTargetData testData =
            JsonReader.Read<EarnTargetData>(filePath);

        string[] supportedValues =
        {
        "Less than $500 per month",
        "Between $500 and $1000 per month",
        "More than $1000 per month"
    };

        Assert.That(
            supportedValues,
            Does.Contain(testData.InitialEarnTarget),
            "The initial Earn Target must be a supported option."
        );

        Assert.That(
            supportedValues,
            Does.Contain(testData.UpdatedEarnTarget),
            "The updated Earn Target must be a supported option."
        );

        Assert.That(
            testData.UpdatedEarnTarget,
            Is.Not.EqualTo(testData.InitialEarnTarget),
            "This scenario requires a change in Earn Target."
        );

        // Capture the original settings.
        string originalEarnTarget = page.GetEarnTarget();
        string originalAvailability = page.GetAvailability();
        string originalHours = page.GetHours();

        Assert.That(
            supportedValues,
            Does.Contain(originalEarnTarget),
            "A supported original value is required for restoration."
        );

        try
        {
            // Arrange: establish this scenario's starting value.
            if (originalEarnTarget != testData.InitialEarnTarget)
            {
                page.OpenEarnTargetEditor();
                page.SelectEarnTarget(testData.InitialEarnTarget);

                Assert.That(
                    page.IsAvailabilityUpdatedMessageDisplayed(),
                    Is.True,
                    "Setup should display the save notification."
                );

                Assert.That(
                    page.WaitForEarnTarget(testData.InitialEarnTarget),
                    Is.True,
                    $"Setup should save '{testData.InitialEarnTarget}'."
                );
            }

            // Act: select the updated value.
            page.OpenEarnTargetEditor();
            page.SelectEarnTarget(testData.UpdatedEarnTarget);

            Assert.That(
                page.IsAvailabilityUpdatedMessageDisplayed(),
                Is.True,
                "The save notification should appear."
            );

            Assert.That(
                page.WaitForEarnTarget(testData.UpdatedEarnTarget),
                Is.True,
                "The updated Earn Target should appear before refresh."
            );

            browser.Navigate().Refresh();

            // Assert: verify persistence and preserve other settings.
            Assert.That(
                page.WaitForEarnTarget(testData.UpdatedEarnTarget),
                Is.True,
                $"'{testData.UpdatedEarnTarget}' should remain after refresh."
            );

            Assert.That(
                page.GetAvailability(),
                Is.EqualTo(originalAvailability),
                "Changing Earn Target must not change Availability."
            );

            Assert.That(
                page.GetHours(),
                Is.EqualTo(originalHours),
                "Changing Earn Target must not change Hours."
            );
        }
        finally
        {
            browser.Navigate().Refresh();

            if (page.GetEarnTarget() != originalEarnTarget)
            {
                page.OpenEarnTargetEditor();
                page.SelectEarnTarget(originalEarnTarget);

                Assert.That(
                    page.IsAvailabilityUpdatedMessageDisplayed(),
                    Is.True,
                    "Restoring Earn Target should show a save message."
                );

                Assert.That(
                    page.WaitForEarnTarget(originalEarnTarget),
                    Is.True,
                    "Cleanup should restore the original Earn Target."
                );

                browser.Navigate().Refresh();
            }

            Assert.That(
                page.WaitForEarnTarget(originalEarnTarget),
                Is.True,
                "The original Earn Target should remain after cleanup."
            );
        }
    }

    [Test]
    public void CloseEarnTargetWithoutChanges()
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
            "CloseEarnTargetWithoutChanges.json"
        );

        EarnTargetData testData =
            JsonReader.Read<EarnTargetData>(filePath);

        Assert.That(
            testData.InitialEarnTarget,
            Is.EqualTo("Less than $500 per month")
        );

        // Capture the original settings before changing anything.
        string originalEarnTarget = page.GetEarnTarget();
        string originalAvailability = page.GetAvailability();
        string originalHours = page.GetHours();

        Assert.That(
            originalEarnTarget,
            Is.AnyOf(
                "Less than $500 per month",
                "Between $500 and $1000 per month",
                "More than $1000 per month"
            ),
            "A supported original value is required for restoration."
        );

        try
        {
            // Arrange: establish the required starting value.
            if (originalEarnTarget != testData.InitialEarnTarget)
            {
                page.OpenEarnTargetEditor();
                page.SelectEarnTarget(testData.InitialEarnTarget);

                Assert.That(
                    page.IsAvailabilityUpdatedMessageDisplayed(),
                    Is.True,
                    "Setup should display the save notification."
                );

                Assert.That(
                    page.WaitForEarnTarget(testData.InitialEarnTarget),
                    Is.True,
                    "Setup should save the initial Earn Target."
                );
            }

            // Act: open and close without selecting another option.
            page.OpenEarnTargetEditor();
            page.CloseEarnTargetEditor();

            // Assert: closing preserves all three settings.
            Assert.That(
                page.GetEarnTarget(),
                Is.EqualTo(testData.InitialEarnTarget),
                "Earn Target should remain unchanged after closing."
            );

            Assert.That(
                page.GetAvailability(),
                Is.EqualTo(originalAvailability),
                "Availability should remain unchanged."
            );

            Assert.That(
                page.GetHours(),
                Is.EqualTo(originalHours),
                "Hours should remain unchanged."
            );

            browser.Navigate().Refresh();

            Assert.That(
                page.WaitForEarnTarget(testData.InitialEarnTarget),
                Is.True,
                "Earn Target should remain unchanged after refresh."
            );

            Assert.That(
                page.GetAvailability(),
                Is.EqualTo(originalAvailability),
                "Availability should remain unchanged after refresh."
            );

            Assert.That(
                page.GetHours(),
                Is.EqualTo(originalHours),
                "Hours should remain unchanged after refresh."
            );
        }
        finally
        {
            browser.Navigate().Refresh();

            if (page.GetEarnTarget() != originalEarnTarget)
            {
                page.OpenEarnTargetEditor();
                page.SelectEarnTarget(originalEarnTarget);

                Assert.That(
                    page.IsAvailabilityUpdatedMessageDisplayed(),
                    Is.True,
                    "Restoring Earn Target should show a save message."
                );

                Assert.That(
                    page.WaitForEarnTarget(originalEarnTarget),
                    Is.True,
                    "Cleanup should restore the original Earn Target."
                );

                browser.Navigate().Refresh();
            }

            Assert.That(
                page.WaitForEarnTarget(originalEarnTarget),
                Is.True,
                "The original Earn Target should remain after cleanup."
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