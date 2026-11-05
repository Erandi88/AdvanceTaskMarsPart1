using NUnit.Framework;
using OpenQA.Selenium;
using MarsAdvanced.Tests.Helpers;
using MarsAdvanced.Tests.Models;
using MarsAdvanced.Tests.Pages;

namespace MarsAdvanced.Tests;

[TestFixture]
public class ShareSkillTests
{
    private IWebDriver? driver;

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
            "Login should succeed before testing Share Skill."
        );
    }


    [Test]
    public void AddShareSkillWithValidDetails()
    {
        var browser = driver
            ?? throw new InvalidOperationException(
                "Browser was not created."
            );

        string filePath = Path.Combine(
            TestContext.CurrentContext.TestDirectory,
            "TestData",
            "ShareSkill",
            "AddShareSkillWithValidDetails.json"
        );

        ShareSkillData testData =
            JsonReader.Read<ShareSkillData>(filePath);

        Assert.That(testData.Title, Is.Not.Null.And.Not.Empty);
        Assert.That(testData.Description, Is.Not.Null.And.Not.Empty);
        Assert.That(testData.Category, Is.Not.Null.And.Not.Empty);
        Assert.That(testData.Subcategory, Is.Not.Null.And.Not.Empty);

        Assert.That(testData.Tags, Is.Not.Null.And.Not.Empty);
        Assert.That(testData.SkillExchangeTags,Is.Not.Null.And.Not.Empty);

        Assert.That(testData.ServiceType, Is.EqualTo("Hourly basis service"));
        Assert.That(testData.LocationType, Is.EqualTo("Online"));
        Assert.That(testData.SkillTrade, Is.EqualTo("Skill-exchange"));
        Assert.That(testData.ActiveStatus, Is.EqualTo("Active"));

        foreach (string tag in testData.Tags.Concat(
            testData.SkillExchangeTags))
        {
            Assert.That(tag, Is.Not.Null.And.Not.Empty);
        }

        // Give this test's listing a unique, recognisable title.
        string suffix = Guid.NewGuid().ToString("N")[..8];
        string title = $"{testData.Title} {suffix}";

        Assert.That(
            title.Length,
            Is.LessThanOrEqualTo(100),
            "The complete title must fit the 100-character limit."
        );

        Assert.That(
            testData.Description.Length,
            Is.LessThanOrEqualTo(600)
        );

        var form = new ShareSkillPage(browser);
        var listings = new ManageListingsPage(browser);

        // Protect existing data before attempting any creation.
        listings.Open();

        Assert.That(
            listings.FindListingByTitle(title),
            Is.False,
            "The generated test title must not already exist."
        );

        bool saveAttempted = false;

        try
        {
            form.OpenShareSkill();

            form.FillForm(testData, title);

            // Calendar and Work Samples remain empty for this flow.
            saveAttempted = true;
            form.ClickSave();
            form.WaitForSaveToComplete();

            listings.Open();

            Assert.That(
                listings.FindListingByTitle(title),
                Is.True,
                "The saved listing should appear in Manage Listings."
            );

            // Verify the listing survives a page reload.
            browser.Navigate().Refresh();
            listings.Open();

            Assert.That(
                listings.FindListingByTitle(title),
                Is.True,
                "The saved listing should remain after refresh."
            );

            // Open the saved listing and verify its field values.
            listings.OpenListingForEdit(title);

            Assert.That(
                form.GetTitle(),
                Is.EqualTo(title),
                "The complete saved title should match the submitted title."
            );

            Assert.That(
                form.GetDescription(),
                Is.EqualTo(testData.Description),
                "The complete saved description should match the JSON."
            );

            Assert.Multiple(() =>
            {
                Assert.That(
                    form.GetCategory(),
                    Is.EqualTo(testData.Category),
                    "The saved category should match."
                );

                Assert.That(
                    form.GetSubcategory(),
                    Is.EqualTo(testData.Subcategory),
                    "The saved subcategory should match."
                );

                Assert.That(
                    form.GetTags(),
                    Is.EquivalentTo(testData.Tags),
                    "The saved tags should match."
                );

                Assert.That(
                    form.GetSkillExchangeTags(),
                    Is.EquivalentTo(testData.SkillExchangeTags),
                    "The saved exchange tags should match."
                );

                Assert.That(
                    form.GetServiceType(),
                    Is.EqualTo(testData.ServiceType),
                    "The saved service type should match."
                );

                Assert.That(
                    form.GetLocationType(),
                    Is.EqualTo(testData.LocationType),
                    "The saved location type should match."
                );

                Assert.That(
                    form.GetSkillTrade(),
                    Is.EqualTo(testData.SkillTrade),
                    "The saved skill trade should match."
                );

                Assert.That(
                    form.GetActiveStatus(),
                    Is.EqualTo(testData.ActiveStatus),
                    "The saved active status should match."
                );
            });

           
            // Leave editing without saving changes.
            form.ClickCancel();
        }
        finally
        {
            // Save might have succeeded even if later verification failed.
            if (saveAttempted)
            {
                listings.Open();
                listings.DeleteListingIfExists(title);
            }
        }
    }


    [TearDown]
    public void TearDown()
    {
        driver?.Dispose();
        driver = null;
    }
}