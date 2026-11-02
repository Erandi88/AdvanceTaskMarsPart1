using MarsAdvanced.Tests.Helpers;
using MarsAdvanced.Tests.Models;
using MarsAdvanced.Tests.Pages;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace MarsAdvanced.Tests;

public class LanguageTests
{
    //These fields will let the setup, tests and teardown access the same browser and page object for each test.
    private IWebDriver? driver;
    private LanguagePage? languagePage;

    [SetUp]
    public void SetUp()
    {
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

    [TearDown]
    public void TearDown()
    {
        driver?.Dispose();
        driver = null;
        languagePage = null;
    }
}