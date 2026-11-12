using NUnit.Framework;
using OpenQA.Selenium;
using MarsAdvanced.Tests.Helpers;
using MarsAdvanced.Tests.Models;
using MarsAdvanced.Tests.Pages;
using AventStack.ExtentReports;

namespace MarsAdvanced.Tests;

[TestFixture]
public class ShareSkillTests
{
    private IWebDriver? driver;
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
            "Login should succeed before testing Share Skill."
        );
    }


    [TestCase(
    "AddShareSkillWithValidDetails.json",
    TestName = "ShareSkill_CreateHourlyOnlineActive")]

    [TestCase(
    "AddShareSkillWithOneOffService.json",
    TestName = "ShareSkill_CreateOneOff")]

    [TestCase(
    "AddShareSkillWithOnSiteLocation.json",
    TestName = "ShareSkill_CreateOnSite")]

    [TestCase(
    "AddShareSkillWithHiddenStatus.json",
    TestName = "ShareSkill_CreateHidden")]

    [TestCase(
    "AddShareSkillWithCredit.json",
    TestName = "ShareSkill_CreateCredit")]
    public void AddShareSkillWithValidDetails(string jsonFileName)
    {
        var browser = driver
            ?? throw new InvalidOperationException("Browser was not created.");

        string filePath = Path.Combine(
            TestContext.CurrentContext.TestDirectory,
            "TestData", "ShareSkill", jsonFileName);
        var data = JsonReader.Read<ShareSkillData>(filePath);
        ValidateShareSkillCreationData(data);

        string title = $"{data.Title} {Guid.NewGuid().ToString("N")[..8]}";
        Assert.That(title.Length, Is.LessThanOrEqualTo(100),
            "The complete title must fit the 100-character limit.");

        var form = new ShareSkillPage(browser);
        var listings = new ManageListingsPage(browser);
        listings.Open();
        Assert.That(listings.FindListingByTitle(title), Is.False,
            "The generated test title must not already exist.");

        bool saveAttempted = false;
        try
        {
            form.OpenShareSkill();
            form.FillForm(data, title);
            saveAttempted = true;
            form.ClickSave();
            form.WaitForSaveToComplete();

            VerifyCreatedListingPersists(browser, listings, title);
            listings.OpenListingForEdit(title);
            VerifyCreatedShareSkillValues(form, data, title);
            form.ClickCancel();
        }
        finally
        {
            if (saveAttempted)
            {
                listings.Open();
                listings.DeleteListingIfExists(title);
            }
        }
    }

    private static void ValidateShareSkillCreationData(ShareSkillData data)
    {
        Assert.That(data.Title, Is.Not.Null.And.Not.Empty);
        Assert.That(data.Description, Is.Not.Null.And.Not.Empty);
        Assert.That(data.Category, Is.Not.Null.And.Not.Empty);
        Assert.That(data.Subcategory, Is.Not.Null.And.Not.Empty);
        Assert.That(data.Tags, Is.Not.Null.And.Not.Empty);
        Assert.That(data.ServiceType,
            Is.AnyOf("Hourly basis service", "One-off service"));
        Assert.That(data.LocationType, Is.AnyOf("Online", "On-site"));
        Assert.That(data.SkillTrade, Is.AnyOf("Skill-exchange", "Credit"));
        Assert.That(data.ActiveStatus, Is.AnyOf("Active", "Hidden"));
        Assert.That(data.Description.Length, Is.LessThanOrEqualTo(600));

        if (data.SkillTrade == "Skill-exchange")
        {
            Assert.That(data.SkillExchangeTags, Is.Not.Null.And.Not.Empty);
        }
        else
        {
            Assert.That(data.CreditAmount, Is.Not.Null.And.Not.Empty);
            Assert.That(data.CreditAmount.Length, Is.LessThanOrEqualTo(4),
                "The credit amount must fit the input's four-character limit.");
        }

        foreach (string tag in data.Tags.Concat(data.SkillExchangeTags))
        {
            Assert.That(tag, Is.Not.Null.And.Not.Empty);
        }
    }

    private static void VerifyCreatedListingPersists(
        IWebDriver browser,
        ManageListingsPage listings,
        string title)
    {
        listings.Open();
        Assert.That(listings.FindListingByTitle(title), Is.True,
            "The saved listing should appear in Manage Listings.");

        browser.Navigate().Refresh();
        listings.Open();
        Assert.That(listings.FindListingByTitle(title), Is.True,
            "The saved listing should remain after refresh.");
    }

    private static void VerifyCreatedShareSkillValues(
        ShareSkillPage form,
        ShareSkillData data,
        string title)
    {
        Assert.Multiple(() =>
        {
            Assert.That(form.GetTitle(), Is.EqualTo(title),
                "The complete saved title should match.");
            Assert.That(form.GetDescription(), Is.EqualTo(data.Description),
                "The complete saved description should match.");
            Assert.That(form.GetCategory(), Is.EqualTo(data.Category),
                "The saved category should match.");
            Assert.That(form.GetSubcategory(), Is.EqualTo(data.Subcategory),
                "The saved subcategory should match.");
            Assert.That(form.GetTags(), Is.EquivalentTo(data.Tags),
                "The saved tags should match.");

            if (data.SkillTrade == "Skill-exchange")
            {
                Assert.That(form.GetSkillExchangeTags(),
                    Is.EquivalentTo(data.SkillExchangeTags),
                    "The saved exchange tags should match.");
            }
            else
            {
                Assert.That(form.GetCreditAmount(), Is.EqualTo(data.CreditAmount),
                    "The saved credit amount should match.");
            }

            Assert.That(form.GetServiceType(), Is.EqualTo(data.ServiceType),
                "The saved service type should match.");
            Assert.That(form.GetLocationType(), Is.EqualTo(data.LocationType),
                "The saved location type should match.");
            Assert.That(form.GetSkillTrade(), Is.EqualTo(data.SkillTrade),
                "The saved skill trade should match.");
            Assert.That(form.GetActiveStatus(), Is.EqualTo(data.ActiveStatus),
                "The saved active status should match.");
        });
    }




    [Test]
    public void CancelShareSkill()
    {
        var browser = driver
            ?? throw new InvalidOperationException(
                "Browser was not created."
            );

        string filePath = Path.Combine(
            TestContext.CurrentContext.TestDirectory,
            "TestData",
            "ShareSkill",
            "CancelShareSkill.json"
        );

        ShareSkillData testData =
            JsonReader.Read<ShareSkillData>(filePath);

        Assert.That(testData.Title, Is.Not.Null.And.Not.Empty);
        Assert.That(testData.Description, Is.Not.Null.And.Not.Empty);

        string suffix = Guid.NewGuid().ToString("N")[..8];
        string title = $"{testData.Title} {suffix}";

        Assert.That(title.Length, Is.LessThanOrEqualTo(100));
        Assert.That(testData.Description.Length, Is.LessThanOrEqualTo(600));

        var form = new ShareSkillPage(browser);
        var listings = new ManageListingsPage(browser);

        // Confirm this test cannot target an existing listing.
        listings.Open();

        Assert.That(
            listings.FindListingByTitle(title),
            Is.False,
            "The generated test title must not already exist."
        );

        try
        {
            // Arrange: complete the form without saving.
            form.OpenShareSkill();
            form.FillForm(testData, title);

            Assert.That(
                form.GetTitle(),
                Is.EqualTo(title),
                "The form should contain the test title before Cancel."
            );

            // Act: cancel instead of saving.
            form.ClickCancel();
            form.WaitForFormToClose();

            // Assert: no listing was created.
            listings.Open();

            Assert.That(
                listings.FindListingByTitle(title),
                Is.False,
                "Cancel must not create a listing."
            );

            browser.Navigate().Refresh();
            listings.Open();

            Assert.That(
                listings.FindListingByTitle(title),
                Is.False,
                "The cancelled listing must remain absent after refresh."
            );
        }
        finally
        {
            // Remove the test listing if a defect unexpectedly created it.
            listings.Open();
            listings.DeleteListingIfExists(title);
        }
    }

    [TestCase(
    "AddShareSkillWithEmptyTitle.json", "Title", "Title is required",
    TestName = "ShareSkill_RejectsEmptyTitle")]

    [TestCase(
    "AddShareSkillWithEmptyDescription.json",
    "Description", "Description is required",
    TestName = "ShareSkill_RejectsEmptyDescription")]

    [TestCase(
    "AddShareSkillWithMissingCategory.json", "Category", "Category is required",
    TestName = "ShareSkill_RejectsMissingCategory")]

    [TestCase(
    "AddShareSkillWithMissingSubcategory.json",
    "Subcategory", "Subcategory is required",
    TestName = "ShareSkill_RejectsMissingSubcategory")]

    [TestCase(
    "AddShareSkillWithMissingTags.json", "Tags", "Tags are required",
    TestName = "ShareSkill_RejectsMissingTags")]

    [TestCase(
    "AddShareSkillWithMissingExchangeTags.json",
    "Skill-Exchange", "Tag is required",
    TestName = "ShareSkill_RejectsMissingExchangeTags")]

    public void AddShareSkillWithMissingRequiredField(
    string jsonFileName, string missingField, string expectedMessage)
    {
        var browser = driver
            ?? throw new InvalidOperationException("Browser was not created.");

        string filePath = Path.Combine(
            TestContext.CurrentContext.TestDirectory,
            "TestData", "ShareSkill", jsonFileName);
        var data = JsonReader.Read<ShareSkillData>(filePath);

        string title = missingField == "Title"
            ? string.Empty
            : $"{data.Title} {Guid.NewGuid().ToString("N")[..8]}";
        ValidateMissingFieldScenario(data, title, missingField);

        var form = new ShareSkillPage(browser);
        var listings = new ManageListingsPage(browser);
        listings.Open();
        List<string> titlesBefore = listings.GetAllListingTitles();

        if (!string.IsNullOrEmpty(title))
            Assert.That(titlesBefore, Does.Not.Contain(title),
                "The generated title must not already exist.");

        try
        {
            form.OpenShareSkill();
            form.FillFormWithMissingField(data, title, missingField);
            VerifyMissingFieldRejection(form, data, title, missingField, expectedMessage);
            VerifyListingCollectionUnchanged(browser, listings, titlesBefore);
        }
        finally
        {
            if (!string.IsNullOrEmpty(title))
            {
                listings.Open();
                listings.DeleteListingIfExists(title);
            }
        }
    }

    private static void ValidateMissingFieldScenario(
        ShareSkillData data,
        string title,
        string missingField)
    {
        Assert.That(title.Length, Is.LessThanOrEqualTo(100));

        if (missingField != "Title")
        {
            Assert.That(data.Title, Is.Not.Null.And.Not.Empty,
                "Other scenarios require a valid title.");
        }

        if (missingField != "Description")
        {
            Assert.That(data.Description, Is.Not.Null.And.Not.Empty,
                "Other scenarios require a valid description.");
        }
    }

    private static void VerifyMissingFieldRejection(
        ShareSkillPage form,
        ShareSkillData data,
        string title,
        string missingField,
        string expectedMessage)
    {
        if (missingField == "Title")
        {
            Assert.That(form.GetTitle(), Is.EqualTo(string.Empty));
        }
        else if (missingField == "Description")
        {
            Assert.That(form.GetDescription(), Is.EqualTo(string.Empty));
        }

        form.ClickSave();

        Assert.That(form.IsValidationMessageDisplayed(expectedMessage), Is.True,
            $"Missing {missingField} should display '{expectedMessage}'.");
        Assert.That(form.GetTitle(), Is.EqualTo(title),
            "The form should remain open with the submitted title.");
        Assert.That(form.GetDescription(), Is.EqualTo(
            missingField == "Description" ? string.Empty : data.Description),
            "The form should retain the submitted description.");

        form.ClickCancel();
        form.WaitForFormToClose();
    }

    private static void VerifyListingCollectionUnchanged(
        IWebDriver browser,
        ManageListingsPage listings,
        List<string> titlesBefore)
    {
        listings.Open();
        Assert.That(listings.GetAllListingTitles(), Is.EquivalentTo(titlesBefore),
            "The rejected Save must leave the listing collection unchanged.");

        browser.Navigate().Refresh();
        listings.Open();
        Assert.That(listings.GetAllListingTitles(), Is.EquivalentTo(titlesBefore),
            "The listing collection should remain unchanged after refresh.");
    }


    /* EDIT */

    [Test]
    public void EditShareSkillTitleAndDescription()
    {
        var browser = driver
            ?? throw new InvalidOperationException("Browser was not created.");
        string filePath = Path.Combine(
            TestContext.CurrentContext.TestDirectory,
            "TestData", "ShareSkill", "EditShareSkillTitleAndDescription.json");
        var data = JsonReader.Read<EditShareSkillData>(filePath);

        string suffix = Guid.NewGuid().ToString("N")[..8];
        ValidateShareSkillEditData(data, suffix);
        string originalTitle = $"{data.Original.Title} {suffix}";
        string updatedTitle = $"{data.UpdatedTitle} {suffix}";

        var form = new ShareSkillPage(browser);
        var listings = new ManageListingsPage(browser);
        listings.Open();
        Assert.That(listings.FindListingByTitle(originalTitle), Is.False);
        Assert.That(listings.FindListingByTitle(updatedTitle), Is.False);

        bool creationAttempted = false;
        try
        {
            form.OpenShareSkill();
            form.FillForm(data.Original, originalTitle);
            creationAttempted = true;
            var expectedTitles = SaveListingAndOpenForEdit(form, listings, data, originalTitle);

            form.EnterTitle(updatedTitle);
            form.EnterDescription(data.UpdatedDescription);
            form.ClickSave();
            form.WaitForSaveToComplete();
            expectedTitles.Remove(originalTitle);
            expectedTitles.Add(updatedTitle);

            VerifyEditedListingPersists(browser, listings, expectedTitles);
            listings.OpenListingForEdit(updatedTitle);
            VerifyEditedShareSkillValues(form, data, updatedTitle);
            form.ClickCancel();
        }
        finally
        {
            if (creationAttempted)
            {
                listings.Open();
                try
                {
                    listings.DeleteListingIfExists(originalTitle);
                }
                finally
                {
                    listings.DeleteListingIfExists(updatedTitle);
                }
            }
        }
    }

    private static void ValidateShareSkillEditData(
        EditShareSkillData data,string suffix)
    {
        Assert.That(data.Original, Is.Not.Null);
        Assert.That(data.Original.Title, Is.Not.Null.And.Not.Empty);
        Assert.That(data.Original.Description, Is.Not.Null.And.Not.Empty);
        Assert.That(data.UpdatedTitle, Is.Not.Null.And.Not.Empty);
        Assert.That(data.UpdatedDescription, Is.Not.Null.And.Not.Empty);

        string originalTitle = $"{data.Original.Title} {suffix}";
        string updatedTitle = $"{data.UpdatedTitle} {suffix}";

        Assert.Multiple(() =>
        {
            Assert.That(originalTitle.Length, Is.LessThanOrEqualTo(100));
            Assert.That(updatedTitle.Length, Is.LessThanOrEqualTo(100));
            Assert.That(data.Original.Description.Length, Is.LessThanOrEqualTo(600));
            Assert.That(data.UpdatedDescription.Length, Is.LessThanOrEqualTo(600));
            Assert.That(updatedTitle, Is.Not.EqualTo(originalTitle));
            Assert.That(data.UpdatedDescription,
                Is.Not.EqualTo(data.Original.Description));
        });
    }

    private static List<string> SaveListingAndOpenForEdit(
        ShareSkillPage form,ManageListingsPage listings,EditShareSkillData data,string originalTitle)
    {
        form.ClickSave();
        form.WaitForSaveToComplete();
        listings.Open();

        Assert.That(listings.FindListingByTitle(originalTitle), Is.True,
            "The original listing should exist before editing.");

        var expectedTitles = listings.GetAllListingTitles().ToList();
        Assert.That(expectedTitles.Count(title => title == originalTitle),
            Is.EqualTo(1), "Exactly one original listing should exist.");

        listings.OpenListingForEdit(originalTitle);
        Assert.Multiple(() =>
        {
            Assert.That(form.GetTitle(), Is.EqualTo(originalTitle));
            Assert.That(form.GetDescription(), Is.EqualTo(data.Original.Description));
        });

        return expectedTitles;
    }

    private static void VerifyEditedListingPersists(
        IWebDriver browser, ManageListingsPage listings,List<string> expectedTitles)
    {
        listings.Open();
        Assert.That(listings.GetAllListingTitles(), Is.EquivalentTo(expectedTitles),
            "Editing should replace the title without adding or removing listings.");

        browser.Navigate().Refresh();
        listings.Open();
        Assert.That(listings.GetAllListingTitles(), Is.EquivalentTo(expectedTitles),
            "The updated listing should remain after refresh.");
    }

    private static void VerifyEditedShareSkillValues(
        ShareSkillPage form, EditShareSkillData data,string updatedTitle)
    {
        var original = data.Original;

        Assert.Multiple(() =>
        {
            Assert.That(form.GetTitle(), Is.EqualTo(updatedTitle),
                "The updated title should be saved.");
            Assert.That(form.GetDescription(), Is.EqualTo(data.UpdatedDescription),
                "The updated description should be saved.");
            Assert.That(form.GetCategory(), Is.EqualTo(original.Category));
            Assert.That(form.GetSubcategory(), Is.EqualTo(original.Subcategory));
            Assert.That(form.GetTags(), Is.EquivalentTo(original.Tags));
            Assert.That(form.GetSkillExchangeTags(),
                Is.EquivalentTo(original.SkillExchangeTags));
            Assert.That(form.GetServiceType(), Is.EqualTo(original.ServiceType));
            Assert.That(form.GetLocationType(), Is.EqualTo(original.LocationType));
            Assert.That(form.GetSkillTrade(), Is.EqualTo(original.SkillTrade));
            Assert.That(form.GetActiveStatus(), Is.EqualTo(original.ActiveStatus));
        });
    }

    [Test]
    public void CancelEditShareSkill()
    {
        var browser = driver
            ?? throw new InvalidOperationException("Browser was not created.");
        string filePath = Path.Combine(
            TestContext.CurrentContext.TestDirectory,
            "TestData", "ShareSkill", "CancelEditShareSkill.json");
        var data = JsonReader.Read<EditShareSkillData>(filePath);

        string suffix = Guid.NewGuid().ToString("N")[..8];
        ValidateShareSkillEditData(data, suffix);
        string originalTitle = $"{data.Original.Title} {suffix}";
        string updatedTitle = $"{data.UpdatedTitle} {suffix}";

        var form = new ShareSkillPage(browser);
        var listings = new ManageListingsPage(browser);
        listings.Open();
        Assert.That(listings.FindListingByTitle(originalTitle), Is.False);
        Assert.That(listings.FindListingByTitle(updatedTitle), Is.False);

        bool creationAttempted = false;
        try
        {
            form.OpenShareSkill();
            form.FillForm(data.Original, originalTitle);
            creationAttempted = true;
            var titlesBefore = SaveListingAndOpenForEdit(form, listings, data, originalTitle);

            EnterAndVerifyShareSkillChanges(form, updatedTitle, data.UpdatedDescription);
            form.ClickCancel();
            form.WaitForFormToClose();
            VerifyCancelledShareSkillEdit(
                browser, form, listings, data, originalTitle, titlesBefore);
        }
        finally
        {
            if (creationAttempted)
                CleanupShareSkillEditTitles(listings, originalTitle, updatedTitle);
        }
    }

    private static void EnterAndVerifyShareSkillChanges(ShareSkillPage form,string title,string description)
    {
        form.EnterTitle(title);
        form.EnterDescription(description);

        Assert.Multiple(() =>
        {
            Assert.That(form.GetTitle(), Is.EqualTo(title));
            Assert.That(form.GetDescription(), Is.EqualTo(description));
        });
    }

    private static void VerifyCancelledShareSkillEdit(
        IWebDriver browser,ShareSkillPage form,
        ManageListingsPage listings, EditShareSkillData data,string originalTitle,List<string> titlesBefore)
    {
        listings.Open();
        Assert.That(listings.GetAllListingTitles(), Is.EquivalentTo(titlesBefore),
            "Cancelling should leave the listing titles unchanged.");

        browser.Navigate().Refresh();
        listings.Open();
        Assert.That(listings.GetAllListingTitles(), Is.EquivalentTo(titlesBefore),
            "No changed listing should appear after refresh.");

        listings.OpenListingForEdit(originalTitle);
        Assert.Multiple(() =>
        {
            Assert.That(form.GetTitle(), Is.EqualTo(originalTitle),
                "Cancelling should preserve the original title.");
            Assert.That(form.GetDescription(), Is.EqualTo(data.Original.Description),
                "Cancelling should preserve the original description.");
        });

        form.ClickCancel();
        form.WaitForFormToClose();
    }

    private static void CleanupShareSkillEditTitles(
        ManageListingsPage listings,string originalTitle,string updatedTitle)
    {
        listings.Open();

        try
        {
            listings.DeleteListingIfExists(originalTitle);
        }
        finally
        {
            listings.DeleteListingIfExists(updatedTitle);
        }
    }


    [Test]
    public void EditShareSkillWithEmptyTitle()
    {
        var browser = driver
            ?? throw new InvalidOperationException("Browser was not created.");
        string filePath = Path.Combine(
            TestContext.CurrentContext.TestDirectory,
            "TestData", "ShareSkill", "EditShareSkillWithEmptyTitle.json");
        var data = JsonReader.Read<EditShareSkillData>(filePath);

        string originalTitle =
            $"{data.Original?.Title} {Guid.NewGuid().ToString("N")[..8]}";
        ValidateEmptyTitleEditData(data, originalTitle);

        var form = new ShareSkillPage(browser);
        var listings = new ManageListingsPage(browser);
        listings.Open();
        Assert.That(listings.FindListingByTitle(originalTitle), Is.False,
            "The generated title must not already exist.");

        bool creationAttempted = false;
        try
        {
            form.OpenShareSkill();
            form.FillForm(data.Original, originalTitle);
            creationAttempted = true;
            var titlesBefore = SaveListingAndOpenForEdit(form, listings, data, originalTitle);

            form.ClearTitle();
            Assert.That(form.GetTitle(), Is.EqualTo(string.Empty),
                "The title must be empty before saving.");
            form.ClickSave();
            Assert.That(form.IsValidationMessageDisplayed("Please type a Title."), Is.True,
                "An empty title should display 'Please type a Title.'.");

            form.ClickCancel();
            form.WaitForFormToClose();
            VerifyCancelledShareSkillEdit(
                browser, form, listings, data, originalTitle, titlesBefore);
        }
        finally
        {
            if (creationAttempted)
            {
                listings.Open();
                listings.DeleteListingIfExists(originalTitle);
            }
        }
    }

    private static void ValidateEmptyTitleEditData(
        EditShareSkillData data,
        string originalTitle)
    {
        Assert.That(data.Original, Is.Not.Null);
        Assert.That(data.Original.Title, Is.Not.Null.And.Not.Empty);
        Assert.That(data.Original.Description, Is.Not.Null.And.Not.Empty);
        Assert.That(data.UpdatedTitle, Is.EqualTo(string.Empty));
        Assert.That(originalTitle.Length, Is.LessThanOrEqualTo(100));
        Assert.That(data.Original.Description.Length, Is.LessThanOrEqualTo(600));
    }

    [Test]
    public void EditShareSkillWithEmptyTitleAndDescription()
    {
        var browser = driver
            ?? throw new InvalidOperationException("Browser was not created.");
        string filePath = Path.Combine(
            TestContext.CurrentContext.TestDirectory,
            "TestData", "ShareSkill", "EditShareSkillWithEmptyTitleAndDescription.json");
        var data = JsonReader.Read<EditShareSkillData>(filePath);

        string originalTitle =
            $"{data.Original?.Title} {Guid.NewGuid().ToString("N")[..8]}";
        ValidateEmptyTitleEditData(data, originalTitle);
        Assert.That(data.UpdatedDescription, Is.EqualTo(string.Empty));

        var form = new ShareSkillPage(browser);
        var listings = new ManageListingsPage(browser);
        listings.Open();
        Assert.That(listings.FindListingByTitle(originalTitle), Is.False,
            "The generated title must not already exist.");

        bool creationAttempted = false;
        try
        {
            form.OpenShareSkill();
            form.FillForm(data.Original, originalTitle);
            creationAttempted = true;
            var titlesBefore = SaveListingAndOpenForEdit(form, listings, data, originalTitle);

            SubmitAndVerifyEmptyTitleAndDescription(form);
            form.ClickCancel();
            form.WaitForFormToClose();
            VerifyCancelledShareSkillEdit(
                browser, form, listings, data, originalTitle, titlesBefore);
        }
        finally
        {
            if (creationAttempted)
            {
                listings.Open();
                listings.DeleteListingIfExists(originalTitle);
            }
        }
    }

    private static void SubmitAndVerifyEmptyTitleAndDescription(ShareSkillPage form)
    {
        form.ClearTitle();
        form.ClearDescription();

        Assert.Multiple(() =>
        {
            Assert.That(form.GetTitle(), Is.EqualTo(string.Empty),
                "The title must be empty before saving.");
            Assert.That(form.GetDescription(), Is.EqualTo(string.Empty),
                "The description must be empty before saving.");
        });

        form.ClickSave();

        Assert.Multiple(() =>
        {
            Assert.That(form.IsValidationMessageDisplayed("Please type a Title."),
                Is.True, "An empty title should display its validation message.");
            Assert.That(form.IsValidationMessageDisplayed("Please type a description."),
                Is.True, "An empty description should display its validation message.");
        });
    }

    /* DELETE */

    [Test]
    public void DeleteShareSkillPersistsAfterRefresh()
    {
        var browser = driver
            ?? throw new InvalidOperationException("Browser was not created.");
        string filePath = Path.Combine(
            TestContext.CurrentContext.TestDirectory,
            "TestData", "ShareSkill", "DeleteShareSkillPersistsAfterRefresh.json");
        var data = JsonReader.Read<ShareSkillData>(filePath);

        Assert.That(data.Title, Is.Not.Null.And.Not.Empty);
        Assert.That(data.Description, Is.Not.Null.And.Not.Empty);
        string title = $"{data.Title} {Guid.NewGuid().ToString("N")[..8]}";
        Assert.That(title.Length, Is.LessThanOrEqualTo(100));
        Assert.That(data.Description.Length, Is.LessThanOrEqualTo(600));

        var form = new ShareSkillPage(browser);
        var listings = new ManageListingsPage(browser);
        listings.Open();
        var titlesBefore = listings.GetAllListingTitles().ToList();
        Assert.That(titlesBefore, Does.Not.Contain(title),
            "The generated title must not already exist.");

        bool creationAttempted = false;
        try
        {
            form.OpenShareSkill();
            form.FillForm(data, title);
            creationAttempted = true;
            SaveAndVerifyListingAdded(form, listings, title, titlesBefore);

            listings.DeleteListing(title);
            VerifyListingDeletedAfterRefresh(browser, listings, title, titlesBefore);
        }
        finally
        {
            if (creationAttempted)
            {
                listings.Open();
                listings.DeleteListingIfExists(title);
            }
        }
    }

    private static void SaveAndVerifyListingAdded(ShareSkillPage form, ManageListingsPage listings,
        string title,List<string> titlesBefore)
    {
        form.ClickSave();
        form.WaitForSaveToComplete();
        listings.Open();

        var expectedTitles = titlesBefore.ToList();
        expectedTitles.Add(title);

        Assert.That(listings.GetAllListingTitles(), Is.EquivalentTo(expectedTitles),
            "Creation should add exactly this test's listing.");
    }

    private static void VerifyListingDeletedAfterRefresh(IWebDriver browser,ManageListingsPage listings,
        string title,List<string> titlesBefore)
    {
        Assert.That(listings.FindListingByTitle(title), Is.False,
            "The deleted listing should no longer appear.");
        Assert.That(listings.GetAllListingTitles(), Is.EquivalentTo(titlesBefore),
            "Deletion should preserve all other listing titles.");

        browser.Navigate().Refresh();
        listings.Open();

        Assert.That(listings.FindListingByTitle(title), Is.False,
            "The deleted listing should not return after refresh.");
        Assert.That(listings.GetAllListingTitles(), Is.EquivalentTo(titlesBefore),
            "Other listing titles should remain unchanged after refresh.");
    }

    [Test]
    public void CancelDeleteShareSkill()
    {
        var browser = driver
            ?? throw new InvalidOperationException("Browser was not created.");
        string filePath = Path.Combine(
            TestContext.CurrentContext.TestDirectory,
            "TestData", "ShareSkill", "CancelDeleteShareSkill.json");
        var data = JsonReader.Read<ShareSkillData>(filePath);

        Assert.That(data.Title, Is.Not.Null.And.Not.Empty);
        Assert.That(data.Description, Is.Not.Null.And.Not.Empty);
        string title = $"{data.Title} {Guid.NewGuid().ToString("N")[..8]}";
        Assert.That(title.Length, Is.LessThanOrEqualTo(100));
        Assert.That(data.Description.Length, Is.LessThanOrEqualTo(600));

        var form = new ShareSkillPage(browser);
        var listings = new ManageListingsPage(browser);
        listings.Open();
        var titlesBefore = listings.GetAllListingTitles().ToList();
        Assert.That(titlesBefore, Does.Not.Contain(title),
            "The generated title must not already exist.");

        bool creationAttempted = false;
        try
        {
            form.OpenShareSkill();
            form.FillForm(data, title);
            creationAttempted = true;
            SaveAndVerifyListingAdded(form, listings, title, titlesBefore);

            var expectedTitles = titlesBefore.ToList();
            expectedTitles.Add(title);
            listings.CancelDeleteListing(title);
            VerifyCancelledListingDeletion(
                browser, form, listings, data, title, expectedTitles);
        }
        finally
        {
            if (creationAttempted)
            {
                listings.Open();
                listings.DeleteListingIfExists(title);
            }
        }
    }

    private static void VerifyCancelledListingDeletion(
        IWebDriver browser, ShareSkillPage form, ManageListingsPage listings,
        ShareSkillData data, string title, List<string> expectedTitles)
    {
        Assert.That(listings.FindListingByTitle(title), Is.True,
            "Cancelling deletion should preserve the listing.");
        Assert.That(listings.GetAllListingTitles(), Is.EquivalentTo(expectedTitles),
            "Cancelling deletion should leave all listing titles unchanged.");

        browser.Navigate().Refresh();
        listings.Open();

        Assert.That(listings.GetAllListingTitles(), Is.EquivalentTo(expectedTitles),
            "All listing titles should remain after refresh.");

        listings.OpenListingForEdit(title);
        Assert.Multiple(() =>
        {
            Assert.That(form.GetTitle(), Is.EqualTo(title),
                "The original title should remain saved.");
            Assert.That(form.GetDescription(), Is.EqualTo(data.Description),
                "The original description should remain saved.");
        });

        form.ClickCancel();
        form.WaitForFormToClose();
    }

    [Test]
    [NonParallelizable]
    public void AddShareSkillWithEmptyForm()
    {
        var browser = driver
            ?? throw new InvalidOperationException("Browser was not created.");
        
        string filePath = Path.Combine(
            TestContext.CurrentContext.TestDirectory,
            "TestData", "ShareSkill", "AddShareSkillWithEmptyForm.json"
        );
        
        var data = JsonReader.Read<ValidationMessageData>(filePath);
        
        Assert.That(data.ExpectedMessage, Is.Not.Null.And.Not.Empty);
        
        var form = new ShareSkillPage(browser);
        var listings = new ManageListingsPage(browser);

        listings.Open();
        var titlesBefore = listings.GetAllListingTitles();
        form.OpenShareSkill();
        Assert.That(form.GetTitle(), Is.Empty);
        Assert.That(form.GetDescription(), Is.Empty);

        // Submit without filling any fields or changing defaults.
        form.ClickSave();
        Assert.That(
            form.IsFormErrorDisplayed(data.ExpectedMessage),
            Is.True,
            "Submitting an empty form should display validation."
        );
        Assert.That(form.GetTitle(), Is.Empty);
        Assert.That(form.GetDescription(), Is.Empty);

        form.ClickCancel();
        form.WaitForFormToClose();
        listings.Open();
        Assert.That(listings.GetAllListingTitles(), Is.EquivalentTo(titlesBefore));

        browser.Navigate().Refresh();
        listings.Open();
        Assert.That(listings.GetAllListingTitles(), Is.EquivalentTo(titlesBefore));
    }


    [TestCase(
        "AddShareSkillWithSpacesOnlyTitle.json",
        "     ",
        TestName = "ShareSkill_RejectsSpacesOnlyTitle"
    )]

    [TestCase(
        "AddShareSkillWithSpecialCharactersTitle.json",
        "@#$%",
        TestName = "ShareSkill_RejectsSpecialCharactersTitle"
    )]
    [NonParallelizable]
    public void AddShareSkillWithInvalidTitle(string jsonFileName, string expectedInput)
    {
        var browser = driver
            ?? throw new InvalidOperationException("Browser was not created.");
        
        string filePath = Path.Combine(
            TestContext.CurrentContext.TestDirectory,
            "TestData", "ShareSkill", jsonFileName
        );
        
        var data = JsonReader.Read<ShareSkillValidationData>(filePath);
        
        Assert.That(data.Title, Is.EqualTo(expectedInput));
        Assert.That(data.ExpectedMessage, Is.Not.Null.And.Not.Empty);
        
        var form = new ShareSkillPage(browser);
        var listings = new ManageListingsPage(browser);

        listings.Open();
        var titlesBefore = listings.GetAllListingTitles();
        form.OpenShareSkill();
        form.FillForm(data, data.Title);

        Assert.That(form.GetTitle(), Is.EqualTo(data.Title),
            "The title should match the invalid input before Save.");
        
        form.ClickSave();
        
        Assert.That(
            form.IsValidationMessageDisplayed(data.ExpectedMessage),
            Is.True,
            "The invalid title should be rejected."
        );
        
        Assert.That(form.GetTitle(), Is.EqualTo(data.Title));

        form.ClickCancel();
        form.WaitForFormToClose();
        listings.Open();
        
        Assert.That(listings.GetAllListingTitles(), Is.EquivalentTo(titlesBefore));

        browser.Navigate().Refresh();
        listings.Open();
        
        Assert.That(listings.GetAllListingTitles(), Is.EquivalentTo(titlesBefore));
    }


    [TestCase(
        "AddShareSkillWithSpacesOnlyDescription.json",
        "     ",
        TestName = "ShareSkill_RejectsSpacesOnlyDescription"
    )]

    [TestCase(
        "AddShareSkillWithSpecialCharactersDescription.json",
        "Selenium # and @ testing",
        TestName = "ShareSkill_RejectsSpecialCharactersDescription"
    )]

    [NonParallelizable]
    public void AddShareSkillWithInvalidDescription(string jsonFileName,string expectedInput)
    {
        var browser = driver
            ?? throw new InvalidOperationException("Browser was not created.");
        
        string filePath = Path.Combine(
            TestContext.CurrentContext.TestDirectory,
            "TestData", "ShareSkill", jsonFileName
        );
        
        var data = JsonReader.Read<ShareSkillValidationData>(filePath);
        
        Assert.That(data.Description, Is.EqualTo(expectedInput));
        Assert.That(data.ExpectedMessage, Is.Not.Null.And.Not.Empty);
        
        string title = $"{data.Title} {Guid.NewGuid():N}";
        var form = new ShareSkillPage(browser);
        var listings = new ManageListingsPage(browser);

        listings.Open();
        var titlesBefore = listings.GetAllListingTitles();
        try
        {
            form.OpenShareSkill();
            form.FillForm(data, title);
            
            Assert.That(form.GetDescription(), Is.EqualTo(data.Description));
            
            form.ClickSave();
            
            Assert.That(form.IsValidationMessageDisplayed(data.ExpectedMessage),
                Is.True, "The invalid description should be rejected.");

            form.ClickCancel();
            form.WaitForFormToClose();
            listings.Open();
            
            Assert.That(listings.GetAllListingTitles(), Is.EquivalentTo(titlesBefore));
            
            browser.Navigate().Refresh();
            listings.Open();
            
            Assert.That(listings.GetAllListingTitles(), Is.EquivalentTo(titlesBefore));
        }
        finally
        {
            listings.Open();
            listings.DeleteListingIfExists(title);
        }
    }


    [TestCase(
        "AddShareSkillWith99CharacterTitle.json",
        TestName = "ShareSkill_Accepts99CharacterTitle"
    )]

    [TestCase(
        "AddShareSkillWith100CharacterTitle.json",
        TestName = "ShareSkill_Accepts100CharacterTitle"
    )]

    [TestCase(
        "AddShareSkillWith101CharacterTitle.json",
        TestName = "ShareSkill_Restricts101CharacterTitleTo100"
    )]

    [NonParallelizable]
    public void AddShareSkillWithTitleBoundary(string jsonFileName)
    {
        var browser = driver
            ?? throw new InvalidOperationException("Browser was not created.");
        
        string filePath = Path.Combine(
            TestContext.CurrentContext.TestDirectory,
            "TestData", "ShareSkill", jsonFileName
        );
        
        var data = JsonReader.Read<ShareSkillTitleBoundaryData>(filePath);
        
        Assert.That(data.InputTitleLength, Is.AnyOf(99, 100, 101));
        Assert.That(data.ExpectedTitleLength,
            Is.EqualTo(Math.Min(data.InputTitleLength, 100)));

        string prefix = $"{data.Title} {Guid.NewGuid():N} ";
        
        Assert.That(prefix.Length, Is.LessThan(data.InputTitleLength));
        
        string attemptedTitle = prefix.PadRight(data.InputTitleLength, 'A');
        string expectedTitle = attemptedTitle[..data.ExpectedTitleLength];
        var form = new ShareSkillPage(browser);
        var listings = new ManageListingsPage(browser);

        listings.Open();
        
        Assert.That(listings.FindListingByTitle(expectedTitle), Is.False);
        bool saveAttempted = false;
        try
        {
            form.OpenShareSkill();
            form.FillForm(data, attemptedTitle);
            Assert.That(attemptedTitle.Length, Is.EqualTo(data.InputTitleLength));
            Assert.That(form.GetTitle(), Is.EqualTo(expectedTitle),
                "The title input should enforce its 100-character maximum.");

            saveAttempted = true;
            VerifySavedBoundaryTitle(browser, form, listings, expectedTitle);
        }
        finally
        {
            if (saveAttempted)
            {
                listings.Open();
                listings.DeleteListingIfExists(expectedTitle);
            }
        }
    }

    //helper - AddShareSkillWithTitleBoundary()
    private static void VerifySavedBoundaryTitle(IWebDriver browser, ShareSkillPage form, ManageListingsPage listings, string expectedTitle)
    {
        form.ClickSave();
        form.WaitForSaveToComplete();

        listings.Open();
        Assert.That(
            listings.FindListingByTitle(expectedTitle),
            Is.True,
            "The boundary listing should be saved."
        );

        browser.Navigate().Refresh();
        listings.Open();
        Assert.That(
            listings.FindListingByTitle(expectedTitle),
            Is.True,
            "The listing should remain after refresh."
        );

        listings.OpenListingForEdit(expectedTitle);
        string savedTitle = form.GetTitle();

        Assert.That(savedTitle, Is.EqualTo(expectedTitle),
            "The complete saved title should match the expected text.");
        Assert.That(savedTitle.Length, Is.EqualTo(expectedTitle.Length));

        form.ClickCancel();
        form.WaitForFormToClose();
    }

    //boundary -  description
    [TestCase(
        "AddShareSkillWith599CharacterDescription.json",
        TestName = "ShareSkill_Accepts599CharacterDescription"
    )]

    [TestCase(
        "AddShareSkillWith600CharacterDescription.json",
        TestName = "ShareSkill_Accepts600CharacterDescription"
    )]

    [TestCase(
        "AddShareSkillWith601CharacterDescription.json",
        TestName = "ShareSkill_Restricts601CharacterDescriptionTo600"
    )]

    [NonParallelizable]
    public void AddShareSkillWithDescriptionBoundary(string jsonFileName)
    {
        var browser = driver
            ?? throw new InvalidOperationException("Browser was not created.");
        
        string filePath = Path.Combine(
            TestContext.CurrentContext.TestDirectory,
            "TestData", "ShareSkill", jsonFileName
        
            );
        var data = JsonReader.Read<ShareSkillDescriptionBoundaryData>(filePath);
        
        Assert.That(data.InputDescriptionLength, Is.AnyOf(599, 600, 601));
        Assert.That(data.ExpectedDescriptionLength,
            Is.EqualTo(Math.Min(data.InputDescriptionLength, 600)));
        Assert.That(data.Description, Is.Not.Null.And.Not.Empty);
        Assert.That(data.Description.Length, Is.LessThan(data.InputDescriptionLength));

        string title = $"{data.Title} {Guid.NewGuid():N}";
        
        Assert.That(title.Length, Is.LessThanOrEqualTo(100));
        
        string attempted = data.Description.PadRight(data.InputDescriptionLength, 'A');
        string expected = attempted[..data.ExpectedDescriptionLength];
        data.Description = attempted;
        var form = new ShareSkillPage(browser);
        var listings = new ManageListingsPage(browser);
        bool saveAttempted = false;

        try
        {
            form.OpenShareSkill();
            form.FillForm(data, title);
            
            Assert.That(attempted.Length, Is.EqualTo(data.InputDescriptionLength));
            Assert.That(form.GetDescription(), Is.EqualTo(expected),
                "The Description input should enforce its 600-character maximum.");
            saveAttempted = true;
            VerifySavedBoundaryDescription(browser, form, listings, title, expected);
        }
        finally
        {
            if (saveAttempted)
            {
                listings.Open();
                listings.DeleteListingIfExists(title);
            }
        }
    }

    //Helper 
    private static void VerifySavedBoundaryDescription(IWebDriver browser,
    ShareSkillPage form,ManageListingsPage listings, string title, string expectedDescription)
    {
        form.ClickSave();
        form.WaitForSaveToComplete();

        listings.Open();
        
        Assert.That(
            listings.FindListingByTitle(title),
            Is.True,
            "The boundary listing should be saved."
        );

        browser.Navigate().Refresh();
        listings.Open();
        
        Assert.That(
            listings.FindListingByTitle(title),
            Is.True,
            "The listing should remain after refresh."
        );

        listings.OpenListingForEdit(title);
        
        Assert.That(form.GetTitle(), Is.EqualTo(title));
        string savedDescription = form.GetDescription();

        Assert.That(savedDescription, Is.EqualTo(expectedDescription),
            "The complete saved Description should match the expected text.");
        Assert.That(savedDescription.Length,
            Is.EqualTo(expectedDescription.Length));

        form.ClickCancel();
        form.WaitForFormToClose();
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
                //languagePage = null;
                reportTest = null;

                ExtentReportManager.Flush();
            }
        }
    }
}