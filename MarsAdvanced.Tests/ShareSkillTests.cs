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
        TestName = "ShareSkill_CreateHourlyOnlineActive"
    )]

    [TestCase(
        "AddShareSkillWithOneOffService.json",
        TestName = "ShareSkill_CreateOneOff"
    )]

    [TestCase(
        "AddShareSkillWithOnSiteLocation.json",
        TestName = "ShareSkill_CreateOnSite"
    )]

    [TestCase(
        "AddShareSkillWithHiddenStatus.json",
        TestName = "ShareSkill_CreateHidden"
    )]

    [TestCase(
        "AddShareSkillWithCredit.json",
        TestName = "ShareSkill_CreateCredit"
    )]

    public void AddShareSkillWithValidDetails(string jsonFileName)
    {
        var browser = driver
            ?? throw new InvalidOperationException(
                "Browser was not created."
            );

        string filePath = Path.Combine(
            TestContext.CurrentContext.TestDirectory,
            "TestData",
            "ShareSkill",
            jsonFileName
        );

        ShareSkillData testData =
            JsonReader.Read<ShareSkillData>(filePath);

        Assert.That(testData.Title, Is.Not.Null.And.Not.Empty);
        Assert.That(testData.Description, Is.Not.Null.And.Not.Empty);
        Assert.That(testData.Category, Is.Not.Null.And.Not.Empty);
        Assert.That(testData.Subcategory, Is.Not.Null.And.Not.Empty);

        Assert.That(testData.Tags, Is.Not.Null.And.Not.Empty);
      

        // Allow the supported variations supplied by each JSON file.
        Assert.That(
            testData.ServiceType,
            Is.AnyOf("Hourly basis service", "One-off service")
        );

        Assert.That(
            testData.LocationType,
            Is.AnyOf("Online", "On-site")
        );

        Assert.That(
            testData.SkillTrade,
            Is.AnyOf("Skill-exchange", "Credit")
        );

        if (testData.SkillTrade == "Skill-exchange")
        {
            Assert.That(
                testData.SkillExchangeTags,
                Is.Not.Null.And.Not.Empty
            );
        }
        else
        {
            Assert.That(
                testData.CreditAmount,
                Is.Not.Null.And.Not.Empty
            );

            Assert.That(
                testData.CreditAmount.Length,
                Is.LessThanOrEqualTo(4),
                "The credit amount must fit the input's four-character limit."
            );
        }

        Assert.That(
            testData.ActiveStatus,
            Is.AnyOf("Active", "Hidden")
        );

        foreach (string tag in testData.Tags.Concat(
            testData.SkillExchangeTags))
        {
            Assert.That(tag, Is.Not.Null.And.Not.Empty);
        }

        // Generate a unique title for safe verification and cleanup.
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

            // Calendar and Work Samples remain empty for these cases.
            saveAttempted = true;
            form.ClickSave();
            form.WaitForSaveToComplete();

            listings.Open();

            Assert.That(
                listings.FindListingByTitle(title),
                Is.True,
                "The saved listing should appear in Manage Listings."
            );

            // Verify persistence after reloading.
            browser.Navigate().Refresh();
            listings.Open();

            Assert.That(
                listings.FindListingByTitle(title),
                Is.True,
                "The saved listing should remain after refresh."
            );

            listings.OpenListingForEdit(title);

            // Compare the saved values with this scenario's data.
            Assert.Multiple(() =>
            {
                Assert.That(
                    form.GetTitle(),
                    Is.EqualTo(title),
                    "The complete saved title should match."
                );

                Assert.That(
                    form.GetDescription(),
                    Is.EqualTo(testData.Description),
                    "The complete saved description should match."
                );

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

                // Verify the saved value for the selected trade type.
                if (testData.SkillTrade == "Skill-exchange")
                {
                    Assert.That(
                        form.GetSkillExchangeTags(),
                        Is.EquivalentTo(testData.SkillExchangeTags),
                        "The saved exchange tags should match."
                    );
                }
                else
                {
                    Assert.That(
                        form.GetCreditAmount(),
                        Is.EqualTo(testData.CreditAmount),
                        "The saved credit amount should match."
                    );
                }

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
        "AddShareSkillWithEmptyTitle.json",
        "Title",
        "Title is required",
        TestName = "ShareSkill_RejectsEmptyTitle"
    )]

    [TestCase(
        "AddShareSkillWithEmptyDescription.json",
        "Description",
        "Description is required",
        TestName = "ShareSkill_RejectsEmptyDescription"
    )]

    [TestCase(
        "AddShareSkillWithMissingCategory.json",
        "Category",
        "Category is required",
        TestName = "ShareSkill_RejectsMissingCategory"
    )]

    [TestCase(
        "AddShareSkillWithMissingSubcategory.json",
        "Subcategory",
        "Subcategory is required",
        TestName = "ShareSkill_RejectsMissingSubcategory"
    )]

    [TestCase(
        "AddShareSkillWithMissingTags.json",
        "Tags",
        "Tags are required",
        TestName = "ShareSkill_RejectsMissingTags"
    )]

    [TestCase(
        "AddShareSkillWithMissingExchangeTags.json",
        "Skill-Exchange",
        "Tag is required",
        TestName = "ShareSkill_RejectsMissingExchangeTags"
    )]

    public void AddShareSkillWithMissingRequiredField(string jsonFileName,string missingField, string expectedMessage)
    {
        var browser = driver
            ?? throw new InvalidOperationException(
                "Browser was not created."
            );

        string filePath = Path.Combine(
            TestContext.CurrentContext.TestDirectory,
            "TestData",
            "ShareSkill",
            jsonFileName
        );

        ShareSkillData testData = JsonReader.Read<ShareSkillData>(filePath);

        // Preserve an empty title; otherwise generate a unique title.
        string title = missingField == "Title"
            ? string.Empty
            : $"{testData.Title} {Guid.NewGuid().ToString("N")[..8]}";

        Assert.That(title.Length, Is.LessThanOrEqualTo(100));

        if (missingField != "Title")
        {
            Assert.That(
                testData.Title,
                Is.Not.Null.And.Not.Empty,
                "Other scenarios require a valid title."
            );
        }

        if (missingField != "Description")
        {
            Assert.That(
                testData.Description,
                Is.Not.Null.And.Not.Empty,
                "Other scenarios require a valid description."
            );
        }

        var form = new ShareSkillPage(browser);
        var listings = new ManageListingsPage(browser);

        // Capture all existing listing titles, including duplicates.
        listings.Open();
        List<string> titlesBefore = listings.GetAllListingTitles();

        if (!string.IsNullOrEmpty(title))
        {
            Assert.That(
                titlesBefore,
                Does.Not.Contain(title),
                "The generated title must not already exist."
            );
        }

        try
        {
            form.OpenShareSkill();

            form.FillFormWithMissingField(
                testData,
                title,
                missingField
            );

            // Confirm the intended input is empty before submitting.
            if (missingField == "Title")
            {
                Assert.That(form.GetTitle(), Is.EqualTo(string.Empty));
            }
            else if (missingField == "Description")
            {
                Assert.That(
                    form.GetDescription(),
                    Is.EqualTo(string.Empty)
                );
            }

            form.ClickSave();

            Assert.That(
                form.IsValidationMessageDisplayed(expectedMessage),
                Is.True,
                $"Missing {missingField} should display '{expectedMessage}'."
            );

            // Reading the fields confirms the form remains open.
            Assert.That(form.GetTitle(), Is.EqualTo(title));

            Assert.That(
                form.GetDescription(),
                Is.EqualTo(
                    missingField == "Description"
                        ? string.Empty
                        : testData.Description
                )
            );

            form.ClickCancel();
            form.WaitForFormToClose();

            listings.Open();

            Assert.That(
                listings.GetAllListingTitles(),
                Is.EquivalentTo(titlesBefore),
                "The rejected Save must leave the listing collection unchanged."
            );

            browser.Navigate().Refresh();
            listings.Open();

            Assert.That(
                listings.GetAllListingTitles(),
                Is.EquivalentTo(titlesBefore),
                "The listing collection should remain unchanged after refresh."
            );
        }
        finally
        {
            // Never delete by an empty title.
            if (!string.IsNullOrEmpty(title))
            {
                listings.Open();
                listings.DeleteListingIfExists(title);
            }
        }
    }


    /* EDIT */

    [Test]
    public void EditShareSkillTitleAndDescription()
    {
        var browser = driver
            ?? throw new InvalidOperationException(
                "Browser was not created."
            );

        string filePath = Path.Combine(
            TestContext.CurrentContext.TestDirectory,
            "TestData",
            "ShareSkill",
            "EditShareSkillTitleAndDescription.json"
        );

        var testData = JsonReader.Read<EditShareSkillData>(filePath);

        Assert.That(testData.Original, Is.Not.Null);
        Assert.That(
            testData.Original.Title,
            Is.Not.Null.And.Not.Empty
        );
        Assert.That(
            testData.Original.Description,
            Is.Not.Null.And.Not.Empty
        );
        Assert.That(
            testData.UpdatedTitle,
            Is.Not.Null.And.Not.Empty
        );
        Assert.That(
            testData.UpdatedDescription,
            Is.Not.Null.And.Not.Empty
        );

        // Use unique titles to identify only this test's listing.
        string suffix = Guid.NewGuid().ToString("N")[..8];
        string originalTitle = $"{testData.Original.Title} {suffix}";
        string updatedTitle = $"{testData.UpdatedTitle} {suffix}";

        Assert.Multiple(() =>
        {
            Assert.That(originalTitle.Length, Is.LessThanOrEqualTo(100));
            Assert.That(updatedTitle.Length, Is.LessThanOrEqualTo(100));

            Assert.That(
                testData.Original.Description.Length,
                Is.LessThanOrEqualTo(600)
            );
            Assert.That(
                testData.UpdatedDescription.Length,
                Is.LessThanOrEqualTo(600)
            );

            Assert.That(updatedTitle, Is.Not.EqualTo(originalTitle));
            Assert.That(
                testData.UpdatedDescription,
                Is.Not.EqualTo(testData.Original.Description)
            );
        });

        var form = new ShareSkillPage(browser);
        var listings = new ManageListingsPage(browser);

        listings.Open();

        Assert.That(listings.FindListingByTitle(originalTitle), Is.False);
        Assert.That(listings.FindListingByTitle(updatedTitle), Is.False);

        bool creationAttempted = false;

        try
        {
            // Arrange: create the listing that this test will edit.
            form.OpenShareSkill();
            form.FillForm(testData.Original, originalTitle);

            creationAttempted = true;
            form.ClickSave();
            form.WaitForSaveToComplete();

            listings.Open();

            Assert.That(
                listings.FindListingByTitle(originalTitle),
                Is.True,
                "The original listing should exist before editing."
            );

            // Capture all titles to detect an extra or missing listing.
            var expectedTitles = listings.GetAllListingTitles().ToList();

            Assert.That(
                expectedTitles.Count(title => title == originalTitle),
                Is.EqualTo(1),
                "Exactly one original listing should exist."
            );

            listings.OpenListingForEdit(originalTitle);

            Assert.Multiple(() =>
            {
                Assert.That(form.GetTitle(), Is.EqualTo(originalTitle));
                Assert.That(
                    form.GetDescription(),
                    Is.EqualTo(testData.Original.Description)
                );
            });

            // Act: change only the title and description.
            form.EnterTitle(updatedTitle);
            form.EnterDescription(testData.UpdatedDescription);
            form.ClickSave();
            form.WaitForSaveToComplete();

            // Expect the updated title to replace the original title.
            expectedTitles.Remove(originalTitle);
            expectedTitles.Add(updatedTitle);

            listings.Open();

            Assert.That(
                listings.GetAllListingTitles(),
                Is.EquivalentTo(expectedTitles),
                "Editing should replace the title without adding or removing listings."
            );

            // Assert: the changes remain after refreshing.
            browser.Navigate().Refresh();
            listings.Open();

            Assert.That(
                listings.GetAllListingTitles(),
                Is.EquivalentTo(expectedTitles),
                "The updated listing should remain after refresh."
            );

            listings.OpenListingForEdit(updatedTitle);

            Assert.Multiple(() =>
            {
                Assert.That(
                    form.GetTitle(),
                    Is.EqualTo(updatedTitle),
                    "The updated title should be saved."
                );

                Assert.That(
                    form.GetDescription(),
                    Is.EqualTo(testData.UpdatedDescription),
                    "The updated description should be saved."
                );

                // Other fields should retain their original values.
                Assert.That(
                    form.GetCategory(),
                    Is.EqualTo(testData.Original.Category)
                );
                Assert.That(
                    form.GetSubcategory(),
                    Is.EqualTo(testData.Original.Subcategory)
                );
                Assert.That(
                    form.GetTags(),
                    Is.EquivalentTo(testData.Original.Tags)
                );
                Assert.That(
                    form.GetSkillExchangeTags(),
                    Is.EquivalentTo(testData.Original.SkillExchangeTags)
                );
                Assert.That(
                    form.GetServiceType(),
                    Is.EqualTo(testData.Original.ServiceType)
                );
                Assert.That(
                    form.GetLocationType(),
                    Is.EqualTo(testData.Original.LocationType)
                );
                Assert.That(
                    form.GetSkillTrade(),
                    Is.EqualTo(testData.Original.SkillTrade)
                );
                Assert.That(
                    form.GetActiveStatus(),
                    Is.EqualTo(testData.Original.ActiveStatus)
                );
            });

            form.ClickCancel();
        }
        finally
        {
            // Either title may exist if creation or editing fails.
            if (creationAttempted)
            {
                listings.Open();
                listings.DeleteListingIfExists(originalTitle);
                listings.DeleteListingIfExists(updatedTitle);
            }
        }
    }

    [Test]
    public void CancelEditShareSkill()
    {
        var browser = driver
            ?? throw new InvalidOperationException(
                "Browser was not created."
            );

        string filePath = Path.Combine(
            TestContext.CurrentContext.TestDirectory,
            "TestData",
            "ShareSkill",
            "CancelEditShareSkill.json"
        );

        var testData = JsonReader.Read<EditShareSkillData>(filePath);

        Assert.That(testData.Original, Is.Not.Null);
        Assert.That(testData.Original.Title, Is.Not.Null.And.Not.Empty);
        Assert.That(testData.Original.Description, Is.Not.Null.And.Not.Empty);
        Assert.That(testData.UpdatedTitle, Is.Not.Null.And.Not.Empty);
        Assert.That(testData.UpdatedDescription, Is.Not.Null.And.Not.Empty);

        string suffix = Guid.NewGuid().ToString("N")[..8];
        string originalTitle = $"{testData.Original.Title} {suffix}";
        string updatedTitle = $"{testData.UpdatedTitle} {suffix}";

        Assert.Multiple(() =>
        {
            Assert.That(originalTitle.Length, Is.LessThanOrEqualTo(100));
            Assert.That(updatedTitle.Length, Is.LessThanOrEqualTo(100));
            Assert.That(
                testData.Original.Description.Length,
                Is.LessThanOrEqualTo(600)
            );
            Assert.That(
                testData.UpdatedDescription.Length,
                Is.LessThanOrEqualTo(600)
            );

            Assert.That(updatedTitle, Is.Not.EqualTo(originalTitle));
            Assert.That(
                testData.UpdatedDescription,
                Is.Not.EqualTo(testData.Original.Description)
            );
        });

        var form = new ShareSkillPage(browser);
        var listings = new ManageListingsPage(browser);

        listings.Open();

        Assert.That(listings.FindListingByTitle(originalTitle), Is.False);
        Assert.That(listings.FindListingByTitle(updatedTitle), Is.False);

        bool creationAttempted = false;

        try
        {
            // Arrange: create a listing owned by this test.
            form.OpenShareSkill();
            form.FillForm(testData.Original, originalTitle);

            creationAttempted = true;
            form.ClickSave();
            form.WaitForSaveToComplete();

            listings.Open();

            Assert.That(
                listings.FindListingByTitle(originalTitle),
                Is.True,
                "The original listing should exist before editing."
            );

            var titlesBefore = listings.GetAllListingTitles().ToList();

            listings.OpenListingForEdit(originalTitle);

            Assert.Multiple(() =>
            {
                Assert.That(form.GetTitle(), Is.EqualTo(originalTitle));
                Assert.That(
                    form.GetDescription(),
                    Is.EqualTo(testData.Original.Description)
                );
            });

            // Enter changes and confirm they reached the form.
            form.EnterTitle(updatedTitle);
            form.EnterDescription(testData.UpdatedDescription);

            Assert.Multiple(() =>
            {
                Assert.That(form.GetTitle(), Is.EqualTo(updatedTitle));
                Assert.That(
                    form.GetDescription(),
                    Is.EqualTo(testData.UpdatedDescription)
                );
            });

            // Act: discard the changes.
            form.ClickCancel();
            form.WaitForFormToClose();

            listings.Open();

            Assert.That(
                listings.GetAllListingTitles(),
                Is.EquivalentTo(titlesBefore),
                "Cancelling should leave the listing titles unchanged."
            );

            // Assert: original values remain after refresh.
            browser.Navigate().Refresh();
            listings.Open();

            Assert.That(
                listings.GetAllListingTitles(),
                Is.EquivalentTo(titlesBefore),
                "No changed listing should appear after refresh."
            );

            listings.OpenListingForEdit(originalTitle);

            Assert.Multiple(() =>
            {
                Assert.That(
                    form.GetTitle(),
                    Is.EqualTo(originalTitle),
                    "Cancelling should preserve the original title."
                );

                Assert.That(
                    form.GetDescription(),
                    Is.EqualTo(testData.Original.Description),
                    "Cancelling should preserve the original description."
                );
            });

            form.ClickCancel();
            form.WaitForFormToClose();
        }
        finally
        {
            // Check both titles in case changes were saved unexpectedly.
            if (creationAttempted)
            {
                listings.Open();
                listings.DeleteListingIfExists(originalTitle);
                listings.DeleteListingIfExists(updatedTitle);
            }
        }
    }


    [Test]
    public void EditShareSkillWithEmptyTitle()
    {
        var browser = driver
            ?? throw new InvalidOperationException(
                "Browser was not created."
            );

        string filePath = Path.Combine(
            TestContext.CurrentContext.TestDirectory,
            "TestData",
            "ShareSkill",
            "EditShareSkillWithEmptyTitle.json"
        );

        var testData = JsonReader.Read<EditShareSkillData>(filePath);

        Assert.That(testData.Original, Is.Not.Null);
        Assert.That(testData.Original.Title, Is.Not.Null.And.Not.Empty);
        Assert.That(testData.Original.Description, Is.Not.Null.And.Not.Empty);
        Assert.That(testData.UpdatedTitle, Is.EqualTo(string.Empty));

        string suffix = Guid.NewGuid().ToString("N")[..8];
        string originalTitle = $"{testData.Original.Title} {suffix}";

        Assert.That(originalTitle.Length, Is.LessThanOrEqualTo(100));
        Assert.That(
            testData.Original.Description.Length,
            Is.LessThanOrEqualTo(600)
        );

        var form = new ShareSkillPage(browser);
        var listings = new ManageListingsPage(browser);

        listings.Open();

        Assert.That(
            listings.FindListingByTitle(originalTitle),
            Is.False,
            "The generated title must not already exist."
        );

        bool creationAttempted = false;

        try
        {
            // Arrange: create the listing we will attempt to edit.
            form.OpenShareSkill();
            form.FillForm(testData.Original, originalTitle);

            creationAttempted = true;
            form.ClickSave();
            form.WaitForSaveToComplete();

            listings.Open();

            Assert.That(
                listings.FindListingByTitle(originalTitle),
                Is.True,
                "The original listing should exist before editing."
            );

            var titlesBefore = listings.GetAllListingTitles().ToList();

            listings.OpenListingForEdit(originalTitle);

            Assert.Multiple(() =>
            {
                Assert.That(form.GetTitle(), Is.EqualTo(originalTitle));
                Assert.That(
                    form.GetDescription(),
                    Is.EqualTo(testData.Original.Description)
                );
            });

            // Act: clear the title and attempt to save.
            form.ClearTitle();

            Assert.That(
                form.GetTitle(),
                Is.EqualTo(string.Empty),
                "The title must be empty before saving."
            );

            form.ClickSave();

            Assert.That(
                form.IsValidationMessageDisplayed("Please type a Title."),
                Is.True,
                "An empty title should display 'Please type a Title.'."
            );

            // Leave the rejected edit, then check the saved data.
            form.ClickCancel();
            form.WaitForFormToClose();

            listings.Open();

            Assert.That(
                listings.GetAllListingTitles(),
                Is.EquivalentTo(titlesBefore),
                "The rejected edit should not change the listing titles."
            );

            browser.Navigate().Refresh();
            listings.Open();

            Assert.That(
                listings.GetAllListingTitles(),
                Is.EquivalentTo(titlesBefore),
                "Listing titles should remain unchanged after refresh."
            );

            listings.OpenListingForEdit(originalTitle);

            Assert.Multiple(() =>
            {
                Assert.That(
                    form.GetTitle(),
                    Is.EqualTo(originalTitle),
                    "The original title should remain saved."
                );

                Assert.That(
                    form.GetDescription(),
                    Is.EqualTo(testData.Original.Description),
                    "The original description should remain saved."
                );
            });

            form.ClickCancel();
            form.WaitForFormToClose();
        }
        finally
        {
            // Remove only the listing identified by this test's title.
            if (creationAttempted)
            {
                listings.Open();
                listings.DeleteListingIfExists(originalTitle);
            }
        }
    }

    [Test]
    public void EditShareSkillWithEmptyTitleAndDescription()
    {
        var browser = driver
            ?? throw new InvalidOperationException(
                "Browser was not created."
            );

        string filePath = Path.Combine(
            TestContext.CurrentContext.TestDirectory,
            "TestData",
            "ShareSkill",
            "EditShareSkillWithEmptyTitleAndDescription.json"
        );

        var testData = JsonReader.Read<EditShareSkillData>(filePath);

        Assert.That(testData.Original, Is.Not.Null);
        Assert.That(testData.Original.Title, Is.Not.Null.And.Not.Empty);
        Assert.That(testData.Original.Description, Is.Not.Null.And.Not.Empty);

        Assert.Multiple(() =>
        {
            Assert.That(testData.UpdatedTitle, Is.EqualTo(string.Empty));
            Assert.That(testData.UpdatedDescription, Is.EqualTo(string.Empty));
        });

        string suffix = Guid.NewGuid().ToString("N")[..8];
        string originalTitle = $"{testData.Original.Title} {suffix}";

        Assert.That(originalTitle.Length, Is.LessThanOrEqualTo(100));
        Assert.That(
            testData.Original.Description.Length,
            Is.LessThanOrEqualTo(600)
        );

        var form = new ShareSkillPage(browser);
        var listings = new ManageListingsPage(browser);

        listings.Open();

        Assert.That(
            listings.FindListingByTitle(originalTitle),
            Is.False,
            "The generated title must not already exist."
        );

        bool creationAttempted = false;

        try
        {
            // Arrange: create a listing owned by this test.
            form.OpenShareSkill();
            form.FillForm(testData.Original, originalTitle);

            creationAttempted = true;
            form.ClickSave();
            form.WaitForSaveToComplete();

            listings.Open();

            Assert.That(
                listings.FindListingByTitle(originalTitle),
                Is.True,
                "The original listing should exist before editing."
            );

            var titlesBefore = listings.GetAllListingTitles().ToList();

            listings.OpenListingForEdit(originalTitle);

            Assert.Multiple(() =>
            {
                Assert.That(form.GetTitle(), Is.EqualTo(originalTitle));
                Assert.That(
                    form.GetDescription(),
                    Is.EqualTo(testData.Original.Description)
                );
            });

            // Act: clear both fields through keyboard input.
            form.ClearTitle();
            form.ClearDescription();

            Assert.Multiple(() =>
            {
                Assert.That(
                    form.GetTitle(),
                    Is.EqualTo(testData.UpdatedTitle),
                    "The title must be empty before saving."
                );

                Assert.That(
                    form.GetDescription(),
                    Is.EqualTo(testData.UpdatedDescription),
                    "The description must be empty before saving."
                );
            });

            form.ClickSave();

            // Both required-field messages should appear.
            Assert.Multiple(() =>
            {
                Assert.That(
                    form.IsValidationMessageDisplayed("Please type a Title."),
                    Is.True,
                    "An empty title should display its validation message."
                );

                Assert.That(
                    form.IsValidationMessageDisplayed("Please type a description."),
                    Is.True,
                    "An empty description should display its validation message."
                );
            });

            form.ClickCancel();
            form.WaitForFormToClose();

            listings.Open();

            Assert.That(
                listings.GetAllListingTitles(),
                Is.EquivalentTo(titlesBefore),
                "The rejected edit should leave listing titles unchanged."
            );

            // Verify the saved values remain unchanged after refresh.
            browser.Navigate().Refresh();
            listings.Open();

            Assert.That(
                listings.GetAllListingTitles(),
                Is.EquivalentTo(titlesBefore),
                "Listing titles should remain unchanged after refresh."
            );

            listings.OpenListingForEdit(originalTitle);

            Assert.Multiple(() =>
            {
                Assert.That(
                    form.GetTitle(),
                    Is.EqualTo(originalTitle),
                    "The original title should remain saved."
                );

                Assert.That(
                    form.GetDescription(),
                    Is.EqualTo(testData.Original.Description),
                    "The original description should remain saved."
                );
            });

            form.ClickCancel();
            form.WaitForFormToClose();
        }
        finally
        {
            // Delete only the listing identified by this test's unique title.
            if (creationAttempted)
            {
                listings.Open();
                listings.DeleteListingIfExists(originalTitle);
            }
        }
    }

    /* DELETE */

    [Test]
    public void DeleteShareSkillPersistsAfterRefresh()
    {
        var browser = driver
            ?? throw new InvalidOperationException(
                "Browser was not created."
            );

        string filePath = Path.Combine(
            TestContext.CurrentContext.TestDirectory,
            "TestData",
            "ShareSkill",
            "DeleteShareSkillPersistsAfterRefresh.json"
        );

        var testData = JsonReader.Read<ShareSkillData>(filePath);

        Assert.That(testData.Title, Is.Not.Null.And.Not.Empty);
        Assert.That(testData.Description, Is.Not.Null.And.Not.Empty);

        string suffix = Guid.NewGuid().ToString("N")[..8];
        string title = $"{testData.Title} {suffix}";

        Assert.That(title.Length, Is.LessThanOrEqualTo(100));
        Assert.That(
            testData.Description.Length,
            Is.LessThanOrEqualTo(600)
        );

        var form = new ShareSkillPage(browser);
        var listings = new ManageListingsPage(browser);

        listings.Open();

        // Capture existing titles before creating this test's listing.
        var titlesBefore = listings.GetAllListingTitles().ToList();

        Assert.That(
            titlesBefore,
            Does.Not.Contain(title),
            "The generated title must not already exist."
        );

        bool creationAttempted = false;

        try
        {
            // Arrange: create the listing we will delete.
            form.OpenShareSkill();
            form.FillForm(testData, title);

            creationAttempted = true;
            form.ClickSave();
            form.WaitForSaveToComplete();

            listings.Open();

            var expectedAfterCreation = titlesBefore.ToList();
            expectedAfterCreation.Add(title);

            Assert.That(
                listings.GetAllListingTitles(),
                Is.EquivalentTo(expectedAfterCreation),
                "Creation should add exactly this test's listing."
            );

            // Act: delete the listing and confirm Yes.
            listings.DeleteListing(title);

            // Assert deletion before finally performs any cleanup.
            Assert.That(
                listings.FindListingByTitle(title),
                Is.False,
                "The deleted listing should no longer appear."
            );

            Assert.That(
                listings.GetAllListingTitles(),
                Is.EquivalentTo(titlesBefore),
                "Deletion should preserve all other listing titles."
            );

            // Verify the deletion remains after refreshing.
            browser.Navigate().Refresh();
            listings.Open();

            Assert.That(
                listings.FindListingByTitle(title),
                Is.False,
                "The deleted listing should not return after refresh."
            );

            Assert.That(
                listings.GetAllListingTitles(),
                Is.EquivalentTo(titlesBefore),
                "Other listing titles should remain unchanged after refresh."
            );
        }
        finally
        {
            // Remove a leftover only after the test's assertions.
            if (creationAttempted)
            {
                listings.Open();
                listings.DeleteListingIfExists(title);
            }
        }
    }

    [Test]
    public void CancelDeleteShareSkill()
    {
        var browser = driver
            ?? throw new InvalidOperationException(
                "Browser was not created."
            );

        string filePath = Path.Combine(
            TestContext.CurrentContext.TestDirectory,
            "TestData",
            "ShareSkill",
            "CancelDeleteShareSkill.json"
        );

        var testData = JsonReader.Read<ShareSkillData>(filePath);

        Assert.That(testData.Title, Is.Not.Null.And.Not.Empty);
        Assert.That(testData.Description, Is.Not.Null.And.Not.Empty);

        string suffix = Guid.NewGuid().ToString("N")[..8];
        string title = $"{testData.Title} {suffix}";

        Assert.That(title.Length, Is.LessThanOrEqualTo(100));
        Assert.That(
            testData.Description.Length,
            Is.LessThanOrEqualTo(600)
        );

        var form = new ShareSkillPage(browser);
        var listings = new ManageListingsPage(browser);

        listings.Open();

        var titlesBefore = listings.GetAllListingTitles().ToList();

        Assert.That(
            titlesBefore,
            Does.Not.Contain(title),
            "The generated title must not already exist."
        );

        bool creationAttempted = false;

        try
        {
            // Arrange: create a listing owned by this test.
            form.OpenShareSkill();
            form.FillForm(testData, title);

            creationAttempted = true;
            form.ClickSave();
            form.WaitForSaveToComplete();

            listings.Open();

            var expectedTitles = titlesBefore.ToList();
            expectedTitles.Add(title);

            Assert.That(
                listings.GetAllListingTitles(),
                Is.EquivalentTo(expectedTitles),
                "Creation should add exactly this test's listing."
            );

            // Act: open the delete dialog and choose No.
            listings.CancelDeleteListing(title);

            Assert.That(
                listings.FindListingByTitle(title),
                Is.True,
                "Cancelling deletion should preserve the listing."
            );

            Assert.That(
                listings.GetAllListingTitles(),
                Is.EquivalentTo(expectedTitles),
                "Cancelling deletion should leave all listing titles unchanged."
            );

            // Verify the listing still exists after refresh.
            browser.Navigate().Refresh();
            listings.Open();

            Assert.That(
                listings.GetAllListingTitles(),
                Is.EquivalentTo(expectedTitles),
                "All listing titles should remain after refresh."
            );

            listings.OpenListingForEdit(title);

            Assert.Multiple(() =>
            {
                Assert.That(
                    form.GetTitle(),
                    Is.EqualTo(title),
                    "The original title should remain saved."
                );

                Assert.That(
                    form.GetDescription(),
                    Is.EqualTo(testData.Description),
                    "The original description should remain saved."
                );
            });

            form.ClickCancel();
            form.WaitForFormToClose();
        }
        finally
        {
            // Clean up only after verifying cancellation.
            if (creationAttempted)
            {
                listings.Open();
                listings.DeleteListingIfExists(title);
            }
        }
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
    public void AddShareSkillWithInvalidTitle(
    string jsonFileName,
    string expectedInput)
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