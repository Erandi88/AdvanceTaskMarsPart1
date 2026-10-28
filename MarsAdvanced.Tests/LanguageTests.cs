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

        Console.WriteLine("Row count BEFORE Edit : "+countBefore);

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
    public void DeleteLanguageWithValidDetails()
    {
        var page = languagePage
           ?? throw new InvalidOperationException("Language page was not created.");

        //string language = "Auto" + Guid.NewGuid().ToString("N")[..8];
        //string level = "Basic";

        string filePath = Path.Combine(
                            TestContext.CurrentContext.TestDirectory,
                            "TestData",
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


    [TearDown]
    public void TearDown()
    {
        driver?.Dispose();
        driver = null;
        languagePage = null;
    }
}