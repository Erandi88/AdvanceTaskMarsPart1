using MarsAdvanced.Tests.Helpers;
using MarsAdvanced.Tests.Models;
using MarsAdvanced.Tests.Pages;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using AventStack.ExtentReports;

namespace MarsAdvanced.Tests;

public class LanguageTests
{
    //These fields will let the setup, tests and teardown access the same browser and page object for each test.
    private IWebDriver? driver;
    private LanguagePage? languagePage;
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
            "Login should succeed before testing languages."
        );

        languagePage = new LanguagePage(driver);
    }

    

    [Test]
    public void AddLanguageWithValidDetails()
    {
        var page = languagePage
            ?? throw new InvalidOperationException("Language page was not created.");

        //string language = "Auto" + Guid.NewGuid().ToString("N")[..8];
        //string level = "Basic";

        string filePath = Path.Combine(
                                TestContext.CurrentContext.TestDirectory,
                                "TestData",
                                "Language",
                                "AddLanguageWithValidDetails.json"
                                );

        LanguageData testData = JsonReader.Read<LanguageData>(filePath);

        Assert.That(
            testData.Language,
            Is.Not.Null.And.Not.Empty,
            "The language must be provided in the JSON file."
        );

        Assert.That(
            testData.Level,
            Is.Not.Null.And.Not.Empty,
            "The language level must be provided in the JSON file."
        );

        string language = testData.Language;
        string level = testData.Level;

        int countBefore = page.GetLanguageRowCount();
        Console.WriteLine("Row count BEFORE : " + countBefore);

        Assert.That(
            countBefore,
            Is.LessThan(4),
            "The profile needs a free language slot for this test."
        );

        Assert.That(
            page.IsLanguageDisplayed(language),
            Is.False,
            "The test language must not already exist."
        );

        try
        {
            page.AddLanguage(language, level);

            Assert.That(
                page.IsLanguageAndLevelDisplayed(language, level),
                Is.True,
                "The new language and its level should appear."
            );
            Console.WriteLine("Row count AFTER : " + page.GetLanguageRowCount());
            Assert.That(
                page.WaitForLanguageRowCount(countBefore + 1),
                Is.True,
                "The language row count should increase by one."
            );
        }
        finally
        {
            page.DeleteLanguageIfExists(language);
        }
    }

    
    [Test]
    public void AddLanguageWithEmptyName()
    {
        var page = languagePage ?? throw new InvalidOperationException(
                "Language page was not created."
            );

        string filePath = Path.Combine(TestContext.CurrentContext.TestDirectory,
                                        "TestData",
                                        "Language",
                                        "AddLanguageWithEmptyName.json"
                                    );

        LanguageData testData = JsonReader.Read<LanguageData>(filePath);

        
        int countBefore = page.GetLanguageRowCount();

        Assert.That(
            countBefore,
            Is.LessThan(4),
            "The profile needs a free language slot for this test."
        );

        Assert.That(
            testData.Language,
            Is.EqualTo(string.Empty),
            "This test requires a completely empty language name."
        );

        page.AddLanguage(testData.Language, testData.Level);

        Assert.That(
            page.IsLanguageValidationMessageDisplayed(),
            Is.True,
            "The required-field validation message should appear."
        );

        Assert.That(
            page.GetLanguageRowCount(),
            Is.EqualTo(countBefore),
            "No record should be added; the row count should remain unchanged"
        );

        // Reading the field confirms the form is still open.
        Assert.That(
            page.GetLanguageFieldValue(),
            Is.EqualTo(string.Empty),
            "The Add form should remain open with an empty name."
        );

        page.ClickCancelButton();

        Assert.That(
            page.IsAddNewButtonAvailable(),
            Is.True,
            "Add New should be available after cancelling."
        );

        Assert.That(
            page.GetLanguageRowCount(),
            Is.EqualTo(countBefore),
            "The new language record should not be created after cancelling."
        );
    }

    [Test]
    public void AddLanguageWithoutLevel()
    {
        var page = languagePage ?? throw new InvalidOperationException(
                                        "Language page was not created."
                                    );

        string filePath = Path.Combine( TestContext.CurrentContext.TestDirectory,
                                    "TestData",
                                    "Language",
                                    "AddLanguageWithoutLevel.json"
                                );

        LanguageData testData = JsonReader.Read<LanguageData>(filePath);

        Assert.That(testData.Language, Is.Not.Null.And.Not.Empty);
        Assert.That(testData.Level, Is.EqualTo(string.Empty));

        int countBefore = page.GetLanguageRowCount();

        Assert.That(
            countBefore,
            Is.LessThan(4),
            "The profile needs a free language slot."
        );

        Assert.That(
            page.IsLanguageDisplayed(testData.Language),
            Is.False,
            "The test language must not already exist."
        );

        // Leave the level dropdown at its default option.
        page.AddLanguageWithoutLevel(testData.Language);

        Assert.That(
            page.IsLanguageValidationMessageDisplayed(),
            Is.True,
            "The required-field validation message should appear."
        );

        Assert.That(
            page.GetLanguageRowCount(),
            Is.EqualTo(countBefore),
            "No language record should be created."
        );

        Assert.That(
            page.GetLanguageFieldValue(),
            Is.EqualTo(testData.Language),
            "The form should remain open with the entered language."
        );

        page.ClickCancelButton();

        Assert.That(
            page.IsAddNewButtonAvailable(),
            Is.True,
            "Add New should be available after cancelling."
        );

        Assert.That(
            page.GetLanguageRowCount(),
            Is.EqualTo(countBefore),
            "Cancelling should leave the row count unchanged."
        );
    }


    [Test]
    public void AddDuplicateLanguageWithSameLevel()
    {
        var page = languagePage
            ?? throw new InvalidOperationException(
                "Language page was not created."
            );

        string filePath = Path.Combine(
            TestContext.CurrentContext.TestDirectory,
            "TestData",
            "Language",
            "AddDuplicateLanguageWithSameLevel.json"
        );

        LanguageData testData = JsonReader.Read<LanguageData>(filePath);

        Assert.That(testData.Language, Is.Not.Null.And.Not.Empty);
        Assert.That(testData.Level, Is.Not.Null.And.Not.Empty);

        int countBefore = page.GetLanguageRowCount();

        // After creating the first record, Add New must remain available.
        Assert.That(
            countBefore,
            Is.LessThan(3),
            "This test needs two free language slots."
        );

        Assert.That(
            page.IsLanguageDisplayed(testData.Language),
            Is.False,
            "The test language must not already exist."
        );

        try
        {
            // Arrange: create the original record.
            page.AddLanguage(testData.Language, testData.Level);

            Assert.That(
                page.IsLanguageAndLevelDisplayed(
                    testData.Language,
                    testData.Level
                ),
                Is.True,
                "The original record should be created."
            );

            Assert.That(
                page.WaitForLanguageRowCount(countBefore + 1),
                Is.True,
                "Setup should add exactly one record."
            );

            // Act: submit the same language and level again.
            page.AddLanguage(testData.Language, testData.Level);

            // Assert: verify both the message and the saved data.
            Assert.That(
                page.IsDuplicateLanguageMessageDisplayed(),
                Is.True,
                "A duplicate validation message should appear."
            );

            Assert.That(
                page.GetLanguageRecordCount(testData.Language),
                Is.EqualTo(1),
                "Only one record with this language should exist."
            );

            Assert.That(
                page.IsLanguageAndLevelDisplayed(
                    testData.Language,
                    testData.Level
                ),
                Is.True,
                "The original language and level should remain."
            );

            Assert.That(
                page.GetLanguageRowCount(),
                Is.EqualTo(countBefore + 1),
                "The duplicate attempt should not add another record."
            );

            page.ClickCancelButton();
        }
        finally
        {
            page.DeleteLanguageIfExists(testData.Language);
        }
    }

    [Test]
    public void AddDuplicateLanguageWithDifferentLevel()
    {
        var page = languagePage
            ?? throw new InvalidOperationException(
                "Language page was not created."
            );

        string filePath = Path.Combine(
            TestContext.CurrentContext.TestDirectory,
            "TestData",
            "Language",
            "AddDuplicateLanguageWithDifferentLevel.json"
        );

        DuplicateLanguageData testData = JsonReader.Read<DuplicateLanguageData>(filePath);

        Assert.That(testData.Language, Is.Not.Null.And.Not.Empty);
        Assert.That(testData.OriginalLevel, Is.Not.Null.And.Not.Empty);
        Assert.That(testData.AttemptedLevel, Is.Not.Null.And.Not.Empty);

        Assert.That(
            testData.AttemptedLevel,
            Is.Not.EqualTo(testData.OriginalLevel),
            "This test requires two different levels."
        );

        int countBefore = page.GetLanguageRowCount();

        Assert.That(
            countBefore,
            Is.LessThan(3),
            "This test needs two free language slots."
        );

        Assert.That(
            page.IsLanguageDisplayed(testData.Language),
            Is.False,
            "The test language must not already exist."
        );

        try
        {
            // Arrange: create the original record.
            page.AddLanguage(testData.Language, testData.OriginalLevel);

            Assert.That(
                page.IsLanguageAndLevelDisplayed( testData.Language, testData.OriginalLevel),
                Is.True,
                "The original record should be created."
            );

            Assert.That(
                page.WaitForLanguageRowCount(countBefore + 1),
                Is.True,
                "Setup should create exactly one record."
            );

            // Act: try the same language with a different level.
            page.AddLanguage(
                testData.Language,
                testData.AttemptedLevel
            );

            // Assert: the duplicate is rejected.
            Assert.That(
                page.IsDuplicatedDataMessageDisplayed(),
                Is.True,
                "The 'Duplicated data' message should appear."
            );

            Assert.That(
                page.GetLanguageRecordCount(testData.Language),
                Is.EqualTo(1),
                "Only one record with this language should remain."
            );

            Assert.That(
                page.IsLanguageAndLevelDisplayed(
                    testData.Language,
                    testData.OriginalLevel
                ),
                Is.True,
                "The original level should remain unchanged."
            );

            Assert.That(
                page.GetLanguageRecordCount(
                    testData.Language,
                    testData.AttemptedLevel
                ),
                Is.Zero,
                "The attempted level should not be saved."
            );

            Assert.That(
                page.GetLanguageRowCount(),
                Is.EqualTo(countBefore + 1),
                "The duplicate attempt should not add a record."
            );

            page.ClickCancelButton();
        }
        finally
        {
            page.DeleteLanguageIfExists(testData.Language);
        }
    }

    [Test]
    public void AddLanguagePersistsAfterRefresh()
    {
        var page = languagePage
            ?? throw new InvalidOperationException(
                "Language page was not created."
            );

        var browser = driver
            ?? throw new InvalidOperationException(
                "Browser was not created."
            );

        string filePath = Path.Combine(
            TestContext.CurrentContext.TestDirectory,
            "TestData",
            "Language",
            "AddLanguagePersistsAfterRefresh.json"
        );

        LanguageData testData =
            JsonReader.Read<LanguageData>(filePath);

        Assert.That(testData.Language, Is.Not.Null.And.Not.Empty);
        Assert.That(testData.Level, Is.Not.Null.And.Not.Empty);

        // Wait for the profile's language controls.
        Assert.That(
            page.IsAddNewButtonAvailable(),
            Is.True,
            "Add New must be available."
        );

        int countBefore = page.GetLanguageRowCount();

        Assert.That(
            countBefore,
            Is.LessThan(4),
            "The profile needs a free language slot."
        );

        Assert.That(
            page.IsLanguageDisplayed(testData.Language),
            Is.False,
            "The test language must not already exist."
        );

        try
        {
            // Arrange: add and verify the record.
            page.AddLanguage(testData.Language, testData.Level);

            Assert.That(
                page.IsLanguageAndLevelDisplayed(
                    testData.Language,
                    testData.Level
                ),
                Is.True,
                "The language should appear before refresh."
            );

            Assert.That(
                page.WaitForLanguageRowCount(countBefore + 1),
                Is.True,
                "Adding should increase the row count by one."
            );

            // Act: reload the page.
            browser.Navigate().Refresh();

            // Assert: wait for the saved record after reload.
            Assert.That(
                page.IsLanguageAndLevelDisplayed(
                    testData.Language,
                    testData.Level
                ),
                Is.True,
                "The saved language and level should remain after refresh."
            );

            Assert.That(
                page.WaitForLanguageRowCount(countBefore + 1),
                Is.True,
                "The row count should remain unchanged after refresh."
            );

            Assert.That(
                page.GetLanguageRecordCount(testData.Language),
                Is.EqualTo(1),
                "Exactly one record with this language should remain."
            );
        }
        finally
        {
            page.DeleteLanguageIfExists(testData.Language);
        }
    }

    /*Edit Language*/

    [Test]
    public void EditLanguageWithValidDetails()
    {
        var page = languagePage
           ?? throw new InvalidOperationException("Language page was not created.");

        /*string suffix = Guid.NewGuid().ToString("N")[..8];
        string originalLanguage = "AutoA" + suffix;
        string updatedLanguage = "AutoB" + suffix;*/

        string filePath = Path.Combine(
                            TestContext.CurrentContext.TestDirectory,
                            "TestData",
                            "Language",
                            "EditLanguageWithValidDetails.json"
                        );

        EditLanguageData testData = JsonReader.Read<EditLanguageData>(filePath);

        Assert.That(
            testData.OriginalLanguage,
            Is.Not.Null.And.Not.Empty,
            "The original language must be provided."
        );

        Assert.That(
            testData.OriginalLevel,
            Is.Not.Null.And.Not.Empty,
            "The original level must be provided."
        );

        Assert.That(
            testData.UpdatedLanguage,
            Is.Not.Null.And.Not.Empty,
            "The updated language must be provided."
        );

        Assert.That(
            testData.UpdatedLevel,
            Is.Not.Null.And.Not.Empty,
            "The updated level must be provided."
        );

        string originalLanguage = testData.OriginalLanguage;
        string originalLevel = testData.OriginalLevel;
        string updatedLanguage = testData.UpdatedLanguage;
        string updatedLevel = testData.UpdatedLevel;
        int countBefore = page.GetLanguageRowCount();

        Console.WriteLine("Row count BEFORE Edit : " + countBefore);

        Assert.That(
            countBefore,
            Is.LessThan(4),
            "The profile needs a free language slot for this test."
        );

        Assert.That(page.IsLanguageDisplayed(originalLanguage), Is.False);
        Assert.That(page.IsLanguageDisplayed(updatedLanguage), Is.False);

        try
        {
            // Arrange: create and verify the record we will edit.
            page.AddLanguage(originalLanguage, originalLevel);

            Assert.That(
                page.IsLanguageAndLevelDisplayed(originalLanguage, originalLevel),
                Is.True,
                "The original language should exist before editing."
            );

            // Act: edit the record.
            page.EditLanguage(originalLanguage, updatedLanguage, updatedLevel);

            // Assert: the updated row replaces the original.
            Assert.That(
                page.IsLanguageAndLevelDisplayed(updatedLanguage, updatedLevel),
                Is.True,
                "The updated language and level should appear."
            );

            Assert.That(
                page.IsLanguageRemoved(originalLanguage),
                Is.True,
                "The original language should no longer appear."
            );
            Console.WriteLine("Row count AFTER Edit : " + page.GetLanguageRowCount());
            Assert.That(
                page.GetLanguageRowCount(),
                Is.EqualTo(countBefore + 1),
                "Editing should not create an extra row."
            );
        }
        finally
        {
            // The record may have either name if the test fails during editing.
            page.DeleteLanguageIfExists(originalLanguage);
            page.DeleteLanguageIfExists(updatedLanguage);
        }
    }

    [Test]
    public void EditLanguageLevelOnly()
    {
        var page = languagePage
            ?? throw new InvalidOperationException(
                "Language page was not created."
            );

        string filePath = Path.Combine(
            TestContext.CurrentContext.TestDirectory,
            "TestData",
            "Language",
            "EditLanguageLevelOnly.json"
        );

        EditLanguageData testData =
            JsonReader.Read<EditLanguageData>(filePath);

        Assert.That(testData.OriginalLanguage, Is.Not.Null.And.Not.Empty);
        Assert.That(testData.OriginalLevel, Is.Not.Null.And.Not.Empty);
        Assert.That(testData.UpdatedLevel, Is.Not.Null.And.Not.Empty);

        Assert.That(
            testData.UpdatedLanguage,
            Is.EqualTo(testData.OriginalLanguage),
            "This test must keep the language name unchanged."
        );

        Assert.That(
            testData.UpdatedLevel,
            Is.Not.EqualTo(testData.OriginalLevel),
            "This test requires a different proficiency level."
        );

        Assert.That(page.IsAddNewButtonAvailable(), Is.True);

        int countBefore = page.GetLanguageRowCount();
        Console.WriteLine("Count before for Edit Level only : "+countBefore);

        Assert.That(
            countBefore,
            Is.LessThan(4),
            "The profile needs a free language slot."
        );

        Assert.That(
            page.IsLanguageDisplayed(testData.OriginalLanguage),
            Is.False,
            "The test language must not already exist."
        );

        try
        {
            // Arrange: create the record to edit.
            page.AddLanguage(
                testData.OriginalLanguage,
                testData.OriginalLevel
            );

            Assert.That(
                page.IsLanguageAndLevelDisplayed(
                    testData.OriginalLanguage,
                    testData.OriginalLevel
                ),
                Is.True,
                "The original record should exist before editing."
            );

            Console.WriteLine("Count After for Edit Level only : " +page.GetLanguageRowCount());

            Assert.That(
                page.WaitForLanguageRowCount(countBefore + 1),
                Is.True,
                "Adding should increase the row count by one."
            );

            // Act: submit the same name with a different level.
            page.EditLanguage(
                testData.OriginalLanguage,
                testData.UpdatedLanguage,
                testData.UpdatedLevel
            );

            // Assert: only the level changed.
            Assert.That(
                page.IsLanguageAndLevelDisplayed(
                    testData.OriginalLanguage,
                    testData.UpdatedLevel
                ),
                Is.True,
                "The same language should display the updated level."
            );

            Assert.That(
                page.GetLanguageRecordCount(
                    testData.OriginalLanguage,
                    testData.OriginalLevel
                ),
                Is.Zero,
                "The original level should no longer be displayed."
            );

            Assert.That(
                page.GetLanguageRecordCount(testData.OriginalLanguage),
                Is.EqualTo(1),
                "Exactly one record with this language should remain."
            );

            Assert.That(
                page.GetLanguageRowCount(),
                Is.EqualTo(countBefore + 1),
                "Editing should not create an additional record."
            );
        }
        finally
        {
            page.DeleteLanguageIfExists(testData.OriginalLanguage);
        }
    }

    [Test]
    public void EditLanguageNameOnly()
    {
        var page = languagePage
            ?? throw new InvalidOperationException(
                "Language page was not created."
            );

        string filePath = Path.Combine(
            TestContext.CurrentContext.TestDirectory,
            "TestData",
            "Language",
            "EditLanguageNameOnly.json"
        );

        EditLanguageData testData =
            JsonReader.Read<EditLanguageData>(filePath);

        Assert.That(testData.OriginalLanguage, Is.Not.Null.And.Not.Empty);
        Assert.That(testData.UpdatedLanguage, Is.Not.Null.And.Not.Empty);
        Assert.That(testData.OriginalLevel, Is.Not.Null.And.Not.Empty);

        Assert.That(
            testData.UpdatedLanguage,
            Is.Not.EqualTo(testData.OriginalLanguage),
            "This test requires a different language name."
        );

        Assert.That(
            testData.UpdatedLevel,
            Is.EqualTo(testData.OriginalLevel),
            "This test must keep the proficiency level unchanged."
        );

        Assert.That(page.IsAddNewButtonAvailable(), Is.True);

        int countBefore = page.GetLanguageRowCount();

        Console.WriteLine("BEFORE Row count for edit language name only : " + page.GetLanguageRowCount());

        Assert.That(
            countBefore,
            Is.LessThan(4),
            "The profile needs a free language slot."
        );

        Assert.That(
            page.IsLanguageDisplayed(testData.OriginalLanguage),
            Is.False,
            "The original test language must not already exist."
        );

        Assert.That(
            page.IsLanguageDisplayed(testData.UpdatedLanguage),
            Is.False,
            "The updated test language must not already exist."
        );

        try
        {
            // Arrange: create the record to edit.
            page.AddLanguage(
                testData.OriginalLanguage,
                testData.OriginalLevel
            );

            Assert.That(
                page.IsLanguageAndLevelDisplayed(
                    testData.OriginalLanguage,
                    testData.OriginalLevel
                ),
                Is.True,
                "The original record should exist before editing."
            );

            Console.WriteLine("AFTER Row count for edit language name only : " + page.GetLanguageRowCount());

            Assert.That(
                page.WaitForLanguageRowCount(countBefore + 1),
                Is.True,
                "Adding should increase the row count by one"
            );

            // Act: change the name while retaining the same level.
            page.EditLanguage(
                testData.OriginalLanguage,
                testData.UpdatedLanguage,
                testData.UpdatedLevel
            );

            // Assert: the renamed record retains its level.
            Assert.That(
                page.IsLanguageAndLevelDisplayed(
                    testData.UpdatedLanguage,
                    testData.OriginalLevel
                ),
                Is.True,
                "The new language name should retain the original level."
            );

            Assert.That(
                page.IsLanguageRemoved(testData.OriginalLanguage),
                Is.True,
                "The original language name should no longer appear."
            );

            Assert.That(
                page.GetLanguageRecordCount(testData.UpdatedLanguage),
                Is.EqualTo(1),
                "Exactly one record with the updated name should exist."
            );

            Assert.That(
                page.GetLanguageRowCount(),
                Is.EqualTo(countBefore + 1),
                "Editing should not create an additional record."
            );
        }
        finally
        {
            // The record may have either name if editing fails.
            page.DeleteLanguageIfExists(testData.OriginalLanguage);
            page.DeleteLanguageIfExists(testData.UpdatedLanguage);
        }
    }

    [Test]
    public void EditLanguageWithEmptyName()
    {
        var page = languagePage
            ?? throw new InvalidOperationException(
                "Language page was not created."
            );

        string filePath = Path.Combine(
            TestContext.CurrentContext.TestDirectory,
            "TestData",
            "Language",
            "EditLanguageWithEmptyName.json"
        );

        EditLanguageData testData =
            JsonReader.Read<EditLanguageData>(filePath);

        Assert.That(testData.OriginalLanguage, Is.Not.Null.And.Not.Empty);
        Assert.That(testData.OriginalLevel, Is.Not.Null.And.Not.Empty);
        Assert.That(testData.UpdatedLanguage, Is.EqualTo(string.Empty));
        Assert.That(testData.UpdatedLevel, Is.EqualTo(testData.OriginalLevel));

        Assert.That(page.IsAddNewButtonAvailable(), Is.True);

        int countBefore = page.GetLanguageRowCount();

        Assert.That(
            countBefore,
            Is.LessThan(4),
            "The profile needs a free language slot."
        );

        Assert.That(
            page.IsLanguageDisplayed(testData.OriginalLanguage),
            Is.False,
            "The test language must not already exist."
        );

        try
        {
            // Arrange: create the original record.
            page.AddLanguage(
                testData.OriginalLanguage,
                testData.OriginalLevel
            );

            Assert.That(
                page.IsLanguageAndLevelDisplayed(
                    testData.OriginalLanguage,
                    testData.OriginalLevel
                ),
                Is.True
            );

            Assert.That(
                page.WaitForLanguageRowCount(countBefore + 1),
                Is.True,
                "The row count should be creased."
            );

            // Act: clear the name and attempt to update.
            page.ClickEditLanguage(testData.OriginalLanguage);
            page.ClearLanguageField();

            Assert.That(
                page.GetLanguageFieldValue(),
                Is.EqualTo(testData.UpdatedLanguage),
                "The language field must be empty before submitting."
            );

            page.ClickUpdateButton();

            // Assert: validation appears and editing remains open.
            Assert.That(
                page.IsLanguageValidationMessageDisplayed(),
                Is.True,
                "The required-field validation message should appear."
            );

            Assert.That(
                page.GetLanguageFieldValue(),
                Is.EqualTo(string.Empty),
                "The edit form should remain open with an empty name."
            );

            page.ClickCancelButton();

            Assert.That(
                page.IsLanguageAndLevelDisplayed(
                    testData.OriginalLanguage,
                    testData.OriginalLevel
                ),
                Is.True,
                "The original record should remain unchanged."
            );

            Assert.That(
                page.GetLanguageRowCount(),
                Is.EqualTo(countBefore + 1),
                "The rejected update should not add or remove a record."
            );
        }
        finally
        {
            page.DeleteLanguageIfExists(testData.OriginalLanguage);
        }
    }

    [Test]
    public void EditLanguageWithEmptyLevel()
    {
        var page = languagePage
            ?? throw new InvalidOperationException(
                "Language page was not created."
            );

        string filePath = Path.Combine(
            TestContext.CurrentContext.TestDirectory,
            "TestData",
            "Language",
            "EditLanguageWithEmptyLevel.json"
        );

        EditLanguageData testData =
            JsonReader.Read<EditLanguageData>(filePath);

        Assert.That(testData.OriginalLanguage, Is.Not.Null.And.Not.Empty);
        Assert.That(testData.OriginalLevel, Is.Not.Null.And.Not.Empty);
        Assert.That(testData.UpdatedLanguage, Is.EqualTo(testData.OriginalLanguage));
        Assert.That(testData.UpdatedLevel, Is.EqualTo(string.Empty));

        Assert.That(page.IsAddNewButtonAvailable(), Is.True);

        int countBefore = page.GetLanguageRowCount();

        Assert.That(
            countBefore,
            Is.LessThan(4),
            "The profile needs a free language slot."
        );

        Assert.That(
            page.IsLanguageDisplayed(testData.OriginalLanguage),
            Is.False,
            "The test language must not already exist."
        );

        try
        {
            // Arrange: create the original record.
            page.AddLanguage(
                testData.OriginalLanguage,
                testData.OriginalLevel
            );

            Assert.That(
                page.IsLanguageAndLevelDisplayed(
                    testData.OriginalLanguage,
                    testData.OriginalLevel
                ),
                Is.True
            );

            Assert.That(
                page.WaitForLanguageRowCount(countBefore + 1),
                Is.True,
                "Row count should be increased."
            );

            // Act: select the default level option and submit.
            page.EditLanguageWithEmptyLevel(testData.OriginalLanguage);

            // Assert: the invalid update is rejected.
            Assert.That(
                page.IsLanguageValidationMessageDisplayed(),
                Is.True,
                "The required-field validation message should appear."
            );

            Assert.That(
                page.GetLanguageFieldValue(),
                Is.EqualTo(testData.OriginalLanguage),
                "The edit form should remain open with the original name."
            );

            page.ClickCancelButton();

            Assert.That(
                page.IsLanguageAndLevelDisplayed(
                    testData.OriginalLanguage,
                    testData.OriginalLevel
                ),
                Is.True,
                "The original language and level should remain unchanged."
            );

            Assert.That(
                page.GetLanguageRowCount(),
                Is.EqualTo(countBefore + 1),
                "The rejected update should not add or remove a record."
            );
        }
        finally
        {
            page.DeleteLanguageIfExists(testData.OriginalLanguage);
        }
    }

    [Test]
    public void EditLanguageToExistingRecord()
    {
        var page = languagePage
            ?? throw new InvalidOperationException(
                "Language page was not created."
            );

        string filePath = Path.Combine(
            TestContext.CurrentContext.TestDirectory,
            "TestData",
            "Language",
            "EditLanguageToExistingRecord.json"
        );

        EditLanguageData testData =
            JsonReader.Read<EditLanguageData>(filePath);

        Assert.That(testData.OriginalLanguage, Is.Not.Null.And.Not.Empty);
        Assert.That(testData.OriginalLevel, Is.Not.Null.And.Not.Empty);
        Assert.That(testData.UpdatedLanguage, Is.Not.Null.And.Not.Empty);
        Assert.That(testData.UpdatedLevel, Is.Not.Null.And.Not.Empty);

        Assert.That(
            testData.UpdatedLanguage,
            Is.Not.EqualTo(testData.OriginalLanguage),
            "The setup requires two different languages."
        );

        Assert.That(page.IsAddNewButtonAvailable(), Is.True);

        int countBefore = page.GetLanguageRowCount();

        Assert.That(
            countBefore,
            Is.LessThanOrEqualTo(2),
            "This test needs space to create two records."
        );

        Assert.That(
            page.IsLanguageDisplayed(testData.OriginalLanguage),
            Is.False,
            "The first test language must not already exist."
        );

        Assert.That(
            page.IsLanguageDisplayed(testData.UpdatedLanguage),
            Is.False,
            "The second test language must not already exist."
        );

        try
        {
            // Arrange: create both records.
            page.AddLanguage(
                testData.OriginalLanguage,
                testData.OriginalLevel
            );

            Assert.That(
                page.IsLanguageAndLevelDisplayed(
                    testData.OriginalLanguage,
                    testData.OriginalLevel
                ),
                Is.True
            );

            page.AddLanguage(
                testData.UpdatedLanguage,
                testData.UpdatedLevel
            );

            Assert.That(
                page.IsLanguageAndLevelDisplayed(
                    testData.UpdatedLanguage,
                    testData.UpdatedLevel
                ),
                Is.True
            );

            Assert.That(
                page.WaitForLanguageRowCount(countBefore + 2),
                Is.True,
                "The row count should be increased."
            );

            // Act: change the first record to match the second.
            page.EditLanguage(
                testData.OriginalLanguage,
                testData.UpdatedLanguage,
                testData.UpdatedLevel
            );

            Assert.That(
                page.IsLanguageAlreadyAddedMessageDisplayed(),
                Is.True,
                "The duplicate update should display a validation message."
            );

            page.ClickCancelButton();

            // Assert: both original records remain unchanged.
            Assert.That(
                page.IsLanguageAndLevelDisplayed(
                    testData.OriginalLanguage,
                    testData.OriginalLevel
                ),
                Is.True,
                "The first record should remain unchanged."
            );

            Assert.That(
                page.IsLanguageAndLevelDisplayed(
                    testData.UpdatedLanguage,
                    testData.UpdatedLevel
                ),
                Is.True,
                "The second record should remain unchanged."
            );

            Assert.That(
                page.GetLanguageRecordCount(testData.OriginalLanguage),
                Is.EqualTo(1)
            );

            Assert.That(
                page.GetLanguageRecordCount(testData.UpdatedLanguage),
                Is.EqualTo(1)
            );

            Assert.That(
                page.GetLanguageRowCount(),
                Is.EqualTo(countBefore + 2),
                "The rejected edit should not add or remove records."
            );
        }
        finally
        {
            page.DeleteLanguageIfExists(testData.OriginalLanguage);
            page.DeleteLanguageIfExists(testData.UpdatedLanguage);
        }
    }

    [Test]
    public void CancelEditLanguage()
    {
        var page = languagePage
            ?? throw new InvalidOperationException(
                "Language page was not created."
            );

        string filePath = Path.Combine(
            TestContext.CurrentContext.TestDirectory,
            "TestData",
            "Language",
            "CancelEditLanguage.json"
        );

        EditLanguageData testData =
            JsonReader.Read<EditLanguageData>(filePath);

        Assert.That(testData.OriginalLanguage, Is.Not.Null.And.Not.Empty);
        Assert.That(testData.UpdatedLanguage, Is.Not.Null.And.Not.Empty);
        Assert.That(testData.OriginalLevel, Is.Not.Null.And.Not.Empty);
        Assert.That(testData.UpdatedLevel, Is.Not.Null.And.Not.Empty);

        Assert.That(
            testData.UpdatedLanguage,
            Is.Not.EqualTo(testData.OriginalLanguage)
        );

        Assert.That(
            testData.UpdatedLevel,
            Is.Not.EqualTo(testData.OriginalLevel)
        );

        Assert.That(page.IsAddNewButtonAvailable(), Is.True);

        int countBefore = page.GetLanguageRowCount();

        Assert.That(countBefore, Is.LessThan(4));

        Assert.That(
            page.IsLanguageDisplayed(testData.OriginalLanguage),
            Is.False,
            "The original test language must not already exist."
        );

        Assert.That(
            page.IsLanguageDisplayed(testData.UpdatedLanguage),
            Is.False,
            "The updated test language must not already exist."
        );

        try
        {
            // Arrange: create the original record.
            page.AddLanguage(
                testData.OriginalLanguage,
                testData.OriginalLevel
            );

            Assert.That(
                page.IsLanguageAndLevelDisplayed(
                    testData.OriginalLanguage,
                    testData.OriginalLevel
                ),
                Is.True
            );

            Assert.That(
                page.WaitForLanguageRowCount(countBefore + 1),
                Is.True,
                "The row count should be increased."
            );

            // Act: enter changes, then cancel without clicking Update.
            page.ClickEditLanguage(testData.OriginalLanguage);
            page.EnterLanguage(testData.UpdatedLanguage);
            page.SelectLanguageLevel(testData.UpdatedLevel);

            Assert.That(
                page.GetLanguageFieldValue(),
                Is.EqualTo(testData.UpdatedLanguage),
                "The edited name should be entered before cancelling."
            );

            page.ClickCancelButton();

            // Assert: the saved record remains unchanged.
            Assert.That(
                page.IsLanguageAndLevelDisplayed(
                    testData.OriginalLanguage,
                    testData.OriginalLevel
                ),
                Is.True,
                "Cancel should preserve the original name and level."
            );

            Assert.That(
                page.GetLanguageRecordCount(testData.UpdatedLanguage),
                Is.Zero,
                "The unsaved language name should not appear."
            );

            Assert.That(
                page.GetLanguageRecordCount(testData.OriginalLanguage),
                Is.EqualTo(1)
            );

            Assert.That(
                page.GetLanguageRowCount(),
                Is.EqualTo(countBefore + 1),
                "Cancelling should not add or remove records."
            );
        }
        finally
        {
            page.DeleteLanguageIfExists(testData.OriginalLanguage);
            page.DeleteLanguageIfExists(testData.UpdatedLanguage);
        }
    }

    [Test]
    public void EditLanguagePersistsAfterRefresh()
    {
        var page = languagePage
            ?? throw new InvalidOperationException(
                "Language page was not created."
            );

        var browser = driver
            ?? throw new InvalidOperationException(
                "Browser was not created."
            );

        string filePath = Path.Combine(
            TestContext.CurrentContext.TestDirectory,
            "TestData",
            "Language",
            "EditLanguagePersistsAfterRefresh.json"
        );

        EditLanguageData testData =
            JsonReader.Read<EditLanguageData>(filePath);

        Assert.That(testData.OriginalLanguage, Is.Not.Null.And.Not.Empty);
        Assert.That(testData.UpdatedLanguage, Is.Not.Null.And.Not.Empty);
        Assert.That(testData.OriginalLevel, Is.Not.Null.And.Not.Empty);
        Assert.That(testData.UpdatedLevel, Is.Not.Null.And.Not.Empty);

        Assert.That(
            testData.UpdatedLanguage,
            Is.Not.EqualTo(testData.OriginalLanguage)
        );

        Assert.That(
            testData.UpdatedLevel,
            Is.Not.EqualTo(testData.OriginalLevel)
        );

        Assert.That(page.IsAddNewButtonAvailable(), Is.True);

        int countBefore = page.GetLanguageRowCount();

        Assert.That(countBefore, Is.LessThan(4));

        Assert.That(
            page.IsLanguageDisplayed(testData.OriginalLanguage),
            Is.False,
            "The original test language must not already exist."
        );

        Assert.That(
            page.IsLanguageDisplayed(testData.UpdatedLanguage),
            Is.False,
            "The updated test language must not already exist."
        );

        try
        {
            // Arrange: create the original record.
            page.AddLanguage(
                testData.OriginalLanguage,
                testData.OriginalLevel
            );

            Assert.That(
                page.IsLanguageAndLevelDisplayed(
                    testData.OriginalLanguage,
                    testData.OriginalLevel
                ),
                Is.True
            );

            Assert.That(
                page.WaitForLanguageRowCount(countBefore + 1),
                Is.True
            );

            // Act: edit and verify before refreshing.
            page.EditLanguage(
                testData.OriginalLanguage,
                testData.UpdatedLanguage,
                testData.UpdatedLevel
            );

            Assert.That(
                page.IsLanguageAndLevelDisplayed(
                    testData.UpdatedLanguage,
                    testData.UpdatedLevel
                ),
                Is.True,
                "The updated record should appear before refresh."
            );

            Assert.That(
                page.IsLanguageRemoved(testData.OriginalLanguage),
                Is.True
            );

            browser.Navigate().Refresh();

            // Assert: the saved update survives the reload.
            Assert.That(
                page.IsLanguageAndLevelDisplayed(
                    testData.UpdatedLanguage,
                    testData.UpdatedLevel
                ),
                Is.True,
                "The updated name and level should remain after refresh."
            );

            Assert.That(
                page.IsLanguageRemoved(testData.OriginalLanguage),
                Is.True,
                "The original name should not reappear after refresh."
            );

            Assert.That(
                page.GetLanguageRecordCount(testData.UpdatedLanguage),
                Is.EqualTo(1),
                "Exactly one updated record should remain."
            );

            Assert.That(
                page.WaitForLanguageRowCount(countBefore + 1),
                Is.True,
                "Editing and refreshing should not change the row count."
            );
        }
        finally
        {
            page.DeleteLanguageIfExists(testData.OriginalLanguage);
            page.DeleteLanguageIfExists(testData.UpdatedLanguage);
        }
    }

    [Test]
    public void EditLanguageLeavesOtherRecordUnchanged()
    {
        var page = languagePage
            ?? throw new InvalidOperationException(
                "Language page was not created."
            );

        string filePath = Path.Combine(
            TestContext.CurrentContext.TestDirectory,
            "TestData",
            "Language",
            "EditLanguageLeavesOtherRecordUnchanged.json"
        );

        EditLanguageIsolationData testData =
            JsonReader.Read<EditLanguageIsolationData>(filePath);

        var edit = testData.RecordToEdit;
        var other = testData.OtherRecord;

        Assert.That(edit.OriginalLanguage, Is.Not.Null.And.Not.Empty);
        Assert.That(edit.UpdatedLanguage, Is.Not.Null.And.Not.Empty);
        Assert.That(edit.OriginalLevel, Is.Not.Null.And.Not.Empty);
        Assert.That(edit.UpdatedLevel, Is.Not.Null.And.Not.Empty);
        Assert.That(other.Language, Is.Not.Null.And.Not.Empty);
        Assert.That(other.Level, Is.Not.Null.And.Not.Empty);

        // Keep all three names distinct so cleanup is safe.
        string[] testNames =
        {
            edit.OriginalLanguage,
            edit.UpdatedLanguage,
            other.Language
        };

        Assert.That(
            testNames.Distinct(StringComparer.OrdinalIgnoreCase).Count(),
            Is.EqualTo(3),
            "This test requires three different language names."
        );

        Assert.That(page.IsAddNewButtonAvailable(), Is.True);

        int countBefore = page.GetLanguageRowCount();

        Assert.That(
            countBefore,
            Is.LessThanOrEqualTo(2),
            "This test needs two free language slots."
        );

        foreach (string name in testNames)
        {
            Assert.That(
                page.IsLanguageDisplayed(name),
                Is.False,
                $"'{name}' must not already exist."
            );
        }

        try
        {
            // Arrange: create the record to edit.
            page.AddLanguage(edit.OriginalLanguage, edit.OriginalLevel);

            Assert.That(
                page.IsLanguageAndLevelDisplayed(
                    edit.OriginalLanguage,
                    edit.OriginalLevel
                ),
                Is.True
            );

            // Create the record that must remain unchanged.
            page.AddLanguage(other.Language, other.Level);

            Assert.That(
                page.IsLanguageAndLevelDisplayed(
                    other.Language,
                    other.Level
                ),
                Is.True
            );

            Assert.That(
                page.WaitForLanguageRowCount(countBefore + 2),
                Is.True,
                "The row count should be increased."
            );

            // Act: edit only the first record.
            page.EditLanguage(
                edit.OriginalLanguage,
                edit.UpdatedLanguage,
                edit.UpdatedLevel
            );

            // Assert: the selected record changed correctly.
            Assert.That(
                page.IsLanguageAndLevelDisplayed(
                    edit.UpdatedLanguage,
                    edit.UpdatedLevel
                ),
                Is.True,
                "The selected record should have its updated values."
            );

            Assert.That(
                page.IsLanguageRemoved(edit.OriginalLanguage),
                Is.True,
                "The original name should no longer appear."
            );

            // The other record retains its name and level.
            Assert.That(
                page.IsLanguageAndLevelDisplayed(
                    other.Language,
                    other.Level
                ),
                Is.True,
                "Editing another record must not change this record."
            );

            Assert.That(
                page.GetLanguageRecordCount(other.Language),
                Is.EqualTo(1),
                "Exactly one unchanged record should remain."
            );

            Assert.That(
                page.GetLanguageRecordCount(edit.UpdatedLanguage),
                Is.EqualTo(1),
                "Exactly one updated record should exist."
            );

            Assert.That(
                page.GetLanguageRowCount(),
                Is.EqualTo(countBefore + 2),
                "Editing should not add or remove records."
            );
        }
        finally
        {
            page.DeleteLanguageIfExists(edit.OriginalLanguage);
            page.DeleteLanguageIfExists(edit.UpdatedLanguage);
            page.DeleteLanguageIfExists(other.Language);
        }
    }

    [Test]
    public void EditLanguageAtFourRecordLimit()
    {
        var page = languagePage
            ?? throw new InvalidOperationException(
                "Language page was not created."
            );

        string filePath = Path.Combine(
            TestContext.CurrentContext.TestDirectory,
            "TestData",
            "Language",
            "EditLanguageAtFourRecordLimit.json"
        );

        EditLanguageLimitData testData =
            JsonReader.Read<EditLanguageLimitData>(filePath);

        var edit = testData.RecordToEdit;

        // Validate the setup data.
        Assert.That(testData.Languages, Has.Count.EqualTo(4));

        foreach (var item in testData.Languages)
        {
            Assert.That(item.Language, Is.Not.Null.And.Not.Empty);
            Assert.That(item.Level, Is.Not.Null.And.Not.Empty);
        }

        Assert.That(edit.OriginalLanguage, Is.Not.Null.And.Not.Empty);
        Assert.That(edit.OriginalLevel, Is.Not.Null.And.Not.Empty);
        Assert.That(edit.UpdatedLanguage, Is.Not.Null.And.Not.Empty);
        Assert.That(edit.UpdatedLevel, Is.Not.Null.And.Not.Empty);

        Assert.That(
            testData.Languages
                .Select(item => item.Language)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count(),
            Is.EqualTo(4),
            "The starting languages must have different names."
        );

        Assert.That(
            testData.Languages.Count(item =>
                item.Language == edit.OriginalLanguage &&
                item.Level == edit.OriginalLevel),
            Is.EqualTo(1),
            "The record to edit must match one starting record."
        );

        Assert.That(
            testData.Languages.Any(item =>
                string.Equals(
                    item.Language,
                    edit.UpdatedLanguage,
                    StringComparison.OrdinalIgnoreCase
                )),
            Is.False,
            "The updated name must differ from all starting names."
        );

        Assert.That(page.IsAddNewButtonAvailable(), Is.True);

        Assert.That(
            page.GetLanguageRowCount(),
            Is.Zero,
            "This test requires an empty language list."
        );

        var attemptedLanguages = new List<string>();

        try
        {
            // Arrange: fill all four slots.
            foreach (var item in testData.Languages)
            {
                attemptedLanguages.Add(item.Language);
                page.AddLanguage(item.Language, item.Level);

                Assert.That(
                    page.IsLanguageAndLevelDisplayed(
                        item.Language,
                        item.Level
                    ),
                    Is.True,
                    $"'{item.Language}' should be created."
                );

                Assert.That(
                    page.WaitForLanguageRowCount(attemptedLanguages.Count),
                    Is.True
                );
            }

            Assert.That(
                page.IsAddNewButtonDisplay(),
                Is.False,
                "Add New should be unavailable at four records."
            );

            // Act: edit an existing record while the list is full.
            attemptedLanguages.Add(edit.UpdatedLanguage);

            page.EditLanguage(
                edit.OriginalLanguage,
                edit.UpdatedLanguage,
                edit.UpdatedLevel
            );

            // Assert: the edit succeeds without creating another row.
            Assert.That(
                page.IsLanguageAndLevelDisplayed(
                    edit.UpdatedLanguage,
                    edit.UpdatedLevel
                ),
                Is.True,
                "Editing should succeed at the four-record limit."
            );

            Assert.That(
                page.IsLanguageRemoved(edit.OriginalLanguage),
                Is.True,
                "The original name should no longer appear."
            );

            Assert.That(
                page.GetLanguageRecordCount(edit.UpdatedLanguage),
                Is.EqualTo(1)
            );

            Assert.That(
                page.GetLanguageRowCount(),
                Is.EqualTo(4),
                "Editing should keep the row count at four."
            );

            Assert.That(
                page.IsAddNewButtonDisplay(),
                Is.False,
                "Add New should remain unavailable."
            );

            // Check the other three records.
            foreach (var item in testData.Languages.Where(
                item => item.Language != edit.OriginalLanguage))
            {
                Assert.That(
                    page.IsLanguageAndLevelDisplayed(
                        item.Language,
                        item.Level
                    ),
                    Is.True,
                    $"'{item.Language}' and its level should remain unchanged."
                );

                Assert.That(
                    page.GetLanguageRecordCount(item.Language),
                    Is.EqualTo(1)
                );
            }
        }
        finally
        {
            foreach (string name in attemptedLanguages)
            {
                page.DeleteLanguageIfExists(name);
            }
        }
    }

    /*Delete Language*/

    [Test]
    public void DeleteLanguageWithValidDetails()
    {
        var page = languagePage
           ?? throw new InvalidOperationException("Language page was not created.");

        //string language = "Auto" + Guid.NewGuid().ToString("N")[..8];
        //string level = "Basic";

        string filePath = Path.Combine(
                            TestContext.CurrentContext.TestDirectory,
                            "TestData",
                            "Language",
                            "DeleteLanguageWithValidDetails.json"
                        );

        LanguageData testData = JsonReader.Read<LanguageData>(filePath);

        Assert.That(
            testData.Language,
            Is.Not.Null.And.Not.Empty,
            "The language must be provided."
        );
        Assert.That(
            testData.Level,
            Is.Not.Null.And.Not.Empty,
            "The language level must be provided."
        );

        string language = testData.Language;
        string level = testData.Level;

        int countBefore = page.GetLanguageRowCount();

        Console.WriteLine("Row count delete BEFORE : " + countBefore);

        Assert.That(
            countBefore,
            Is.LessThan(4),
            "The profile needs a free language slot for this test."
        );

        Assert.That(
            page.IsLanguageDisplayed(language),
            Is.False,
            "The test language must not already exist."
        );

        try
        {
            // Arrange: create the record that this test will delete.
            page.AddLanguage(language, level);

            Assert.That(
                page.IsLanguageAndLevelDisplayed(language, level),
                Is.True,
                "The test language should exist before deletion."
            );

            // Act: delete the record.
            page.DeleteLanguage(language);

            // Assert: check both removal and row count.
            Assert.That(
                page.IsLanguageRemoved(language),
                Is.True,
                "The deleted language should no longer appear."
            );

            Console.WriteLine("Row count Delete AFTER : " + page.GetLanguageRowCount());

            Assert.That(
                page.WaitForLanguageRowCount(countBefore),
                Is.True,
                "The row count should return to its original value."
            );
        }
        finally
        {
            // If deletion failed, attempt to remove our test record.
            page.DeleteLanguageIfExists(language);
        }
    }

    [Test]
    public void DeleteLanguagePersistsAfterRefresh()
    {
        var page = languagePage
            ?? throw new InvalidOperationException(
                "Language page was not created."
            );

        var browser = driver
            ?? throw new InvalidOperationException(
                "Browser was not created."
            );

        string filePath = Path.Combine(
            TestContext.CurrentContext.TestDirectory,
            "TestData",
            "Language",
            "DeleteLanguagePersistsAfterRefresh.json"
        );

        DeleteLanguagePersistenceData testData =
            JsonReader.Read<DeleteLanguagePersistenceData>(filePath);

        var target = testData.RecordToDelete;
        var other = testData.OtherRecord;

        Assert.That(target.Language, Is.Not.Null.And.Not.Empty);
        Assert.That(target.Level, Is.Not.Null.And.Not.Empty);
        Assert.That(other.Language, Is.Not.Null.And.Not.Empty);
        Assert.That(other.Level, Is.Not.Null.And.Not.Empty);

        Assert.That(
            string.Equals(
                target.Language,
                other.Language,
                StringComparison.OrdinalIgnoreCase
            ),
            Is.False,
            "The test requires two different language names."
        );

        Assert.That(page.IsAddNewButtonAvailable(), Is.True);

        int countBefore = page.GetLanguageRowCount();

        Assert.That(
            countBefore,
            Is.LessThanOrEqualTo(2),
            "This test needs two free language slots."
        );

        Assert.That(page.IsLanguageDisplayed(target.Language), Is.False);
        Assert.That(page.IsLanguageDisplayed(other.Language), Is.False);

        try
        {
            // Arrange: create both temporary records.
            page.AddLanguage(target.Language, target.Level);

            Assert.That(
                page.IsLanguageAndLevelDisplayed(
                    target.Language,
                    target.Level
                ),
                Is.True
            );

            page.AddLanguage(other.Language, other.Level);

            Assert.That(
                page.IsLanguageAndLevelDisplayed(
                    other.Language,
                    other.Level
                ),
                Is.True
            );

            Assert.That(
                page.WaitForLanguageRowCount(countBefore + 2),
                Is.True,
                "The row count should be increased."
            );

            // Act: delete the selected record.
            page.DeleteLanguage(target.Language);

            Assert.That(
                page.IsLanguageRemoved(target.Language),
                Is.True,
                "The selected record should disappear before refresh."
            );

            Assert.That(
                page.WaitForLanguageRowCount(countBefore + 1),
                Is.True,
                "Deleting should reduce the row count by one."
            );

            browser.Navigate().Refresh();

            // Wait for saved language data to load after refresh.
            Assert.That(
                page.IsLanguageAndLevelDisplayed(
                    other.Language,
                    other.Level
                ),
                Is.True,
                "The remaining record should retain its name and level."
            );

            Assert.That(
                page.IsLanguageRemoved(target.Language),
                Is.True,
                "The deleted language must not reappear after refresh."
            );

            Assert.That(
                page.WaitForLanguageRowCount(countBefore + 1),
                Is.True,
                "The row count should remain unchanged after refresh."
            );

            Assert.That(
                page.GetLanguageRecordCount(other.Language),
                Is.EqualTo(1),
                "Exactly one copy of the remaining record should exist."
            );
        }
        finally
        {
            page.DeleteLanguageIfExists(target.Language);
            page.DeleteLanguageIfExists(other.Language);
        }
    }

    [Test]
    public void DeleteLastLanguage()
    {
        var page = languagePage
            ?? throw new InvalidOperationException(
                "Language page was not created."
            );

        string filePath = Path.Combine(
            TestContext.CurrentContext.TestDirectory,
            "TestData",
            "Language",
            "DeleteLastLanguage.json"
        );

        LanguageData testData =
            JsonReader.Read<LanguageData>(filePath);

        Assert.That(testData.Language, Is.Not.Null.And.Not.Empty);
        Assert.That(testData.Level, Is.Not.Null.And.Not.Empty);

        Assert.That(page.IsAddNewButtonAvailable(), Is.True);

        Assert.That(
            page.GetLanguageRowCount(),
            Is.Zero,
            "This test requires an empty language list."
        );

        try
        {
            // Arrange: create the only language record.
            page.AddLanguage(testData.Language, testData.Level);

            Assert.That(
                page.IsLanguageAndLevelDisplayed(
                    testData.Language,
                    testData.Level
                ),
                Is.True
            );

            Assert.That(
                page.WaitForLanguageRowCount(1),
                Is.True,
                "Exactly one record should exist before deletion."
            );

            // Act: delete the last record.
            page.DeleteLanguage(testData.Language);

            // Assert: the list is empty and adding remains available.
            Assert.That(
                page.IsLanguageRemoved(testData.Language),
                Is.True,
                "The last language should disappear."
            );

            Assert.That(
                page.WaitForLanguageRowCount(0),
                Is.True,
                "No language records should remain."
            );

            Assert.That(
                page.IsAddNewButtonAvailable(),
                Is.True,
                "Add New should remain available with an empty list."
            );
        }
        finally
        {
            page.DeleteLanguageIfExists(testData.Language);
        }
    }

    [Test]
    public void DeleteLanguageRestoresAddNew()
    {
        var page = languagePage
            ?? throw new InvalidOperationException(
                "Language page was not created."
            );

        string filePath = Path.Combine(
            TestContext.CurrentContext.TestDirectory,
            "TestData",
            "Language",
            "DeleteLanguageRestoresAddNew.json"
        );

        LanguageLimitData testData =
            JsonReader.Read<LanguageLimitData>(filePath);

        Assert.That(testData.Languages, Has.Count.EqualTo(4));
        Assert.That(testData.LanguageToDelete, Is.Not.Null.And.Not.Empty);

        foreach (var language in testData.Languages)
        {
            Assert.That(language.Language, Is.Not.Null.And.Not.Empty);
            Assert.That(language.Level, Is.Not.Null.And.Not.Empty);
        }

        Assert.That(
            testData.Languages
                .Select(item => item.Language)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count(),
            Is.EqualTo(4),
            "The JSON must contain four different language names."
        );

        Assert.That(
            testData.Languages.Count(
                item => item.Language == testData.LanguageToDelete
            ),
            Is.EqualTo(1),
            "The deletion target must match one language in the JSON."
        );

        Assert.That(page.IsAddNewButtonAvailable(), Is.True);

        Assert.That(
            page.GetLanguageRowCount(),
            Is.Zero,
            "This test requires an empty language list."
        );

        // Track only records whose creation the test attempted.
        var attemptedLanguages = new List<string>();

        try
        {
            // Arrange: fill all four language slots.
            foreach (var language in testData.Languages)
            {
                attemptedLanguages.Add(language.Language);

                page.AddLanguage(language.Language, language.Level);

                Assert.That(
                    page.IsLanguageAndLevelDisplayed(
                        language.Language,
                        language.Level
                    ),
                    Is.True,
                    $"'{language.Language}' should be created."
                );

                Assert.That(
                    page.WaitForLanguageRowCount(attemptedLanguages.Count),
                    Is.True
                );
            }

            Assert.That(
                page.IsAddNewButtonDisplay(),
                Is.False,
                "Add New should be unavailable when four records exist."
            );

            // Act: delete one record from the full list.
            page.DeleteLanguage(testData.LanguageToDelete);

            Assert.That(
                page.IsLanguageRemoved(testData.LanguageToDelete),
                Is.True,
                "The selected language should be removed."
            );

            Assert.That(
                page.WaitForLanguageRowCount(3),
                Is.True,
                "Three records should remain."
            );

            Assert.That(
                page.IsAddNewButtonAvailable(),
                Is.True,
                "Add New should reappear after a slot becomes free."
            );

            // Assert: the other records remain unchanged.
            foreach (var language in testData.Languages.Where(
                item => item.Language != testData.LanguageToDelete))
            {
                Assert.That(
                    page.IsLanguageAndLevelDisplayed(
                        language.Language,
                        language.Level
                    ),
                    Is.True,
                    $"'{language.Language}' and its level should remain unchanged."
                );

                Assert.That(
                    page.GetLanguageRecordCount(language.Language),
                    Is.EqualTo(1)
                );
            }
        }
        finally
        {
            foreach (string language in attemptedLanguages)
            {
                page.DeleteLanguageIfExists(language);
            }
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
                languagePage = null;
                reportTest = null;

                ExtentReportManager.Flush();
            }
        }
    }
}