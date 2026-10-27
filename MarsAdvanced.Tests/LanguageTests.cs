using MarsAdvanced.Tests.Pages;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace MarsAdvanced.Tests;

public class LanguageTests
{
    [Test]
    public void AddLanguageWithValidDetails()
    {
        string? email = Environment.GetEnvironmentVariable("MARS_EMAIL");
        string? password = Environment.GetEnvironmentVariable("MARS_PASSWORD");

        Assert.That(email, Is.Not.Null.And.Not.Empty, "MARS_EMAIL is not set.");
        Assert.That(password, Is.Not.Null.And.Not.Empty, "MARS_PASSWORD is not set.");

        // A different name for each run helps us identify our test record.
        string language = "Auto" + Guid.NewGuid().ToString("N")[..8];
        string level = "Basic";

        using IWebDriver driver = new ChromeDriver();
        driver.Navigate().GoToUrl("http://localhost:5003/Home");

        var loginPage = new LoginPage(driver);
        loginPage.Login(email!, password!);

        Assert.That(
            loginPage.IsLoggedInSuccessfully(),
            Is.True,
            "Login should succeed before testing languages."
        );

        var languagePage = new LanguagePage(driver);
        int countBefore = languagePage.GetLanguageRowCount();
        Console.WriteLine("Count BEFORE : " + countBefore);
        //TestContext.Progress.WriteLine($"Language rows before: {countBefore}");

        Assert.That(
            countBefore,
            Is.LessThan(4),
            "The profile needs a free language slot for this test."
        );

        Assert.That(
            languagePage.IsLanguageDisplayed(language),
            Is.False,
            "The test language must not already exist."
        );

        try
        {
            languagePage.AddLanguage(language, level);

            Assert.That(
                languagePage.IsLanguageAndLevelDisplayed(language, level),
                Is.True,
                "The new language and its level should appear."
            );

            /*TestContext.Progress.WriteLine(
                $"Language rows after adding: {languagePage.GetLanguageRowCount()}"
            );*/

            Console.WriteLine("Row count AFTER : "+languagePage.GetLanguageRowCount());

            Assert.That(
                languagePage.WaitForLanguageRowCount(countBefore + 1),
                Is.True,
                "The language row count should increase by one."
            );
        }
        finally
        {
            // Attempt cleanup even if an assertion fails after adding.
            languagePage.DeleteLanguageIfExists(language);
        }
    }

    [Test]
    public void EditLanguageWithValidDetails()
    {
        string? email = Environment.GetEnvironmentVariable("MARS_EMAIL");
        string? password = Environment.GetEnvironmentVariable("MARS_PASSWORD");

        Assert.That(email, Is.Not.Null.And.Not.Empty, "MARS_EMAIL is not set.");
        Assert.That(password, Is.Not.Null.And.Not.Empty, "MARS_PASSWORD is not set.");

        string suffix = Guid.NewGuid().ToString("N")[..8];
        string originalLanguage = "AutoA" + suffix;
        string updatedLanguage = "AutoB" + suffix;

        using IWebDriver driver = new ChromeDriver();
        driver.Navigate().GoToUrl("http://localhost:5003/Home");

        var loginPage = new LoginPage(driver);
        loginPage.Login(email!, password!);

        Assert.That(
            loginPage.IsLoggedInSuccessfully(),
            Is.True,
            "Login should succeed before testing languages."
        );

        var languagePage = new LanguagePage(driver);
        int countBefore = languagePage.GetLanguageRowCount();

        Assert.That(
            countBefore,
            Is.LessThan(4),
            "The profile needs a free language slot for this test."
        );

        Assert.That(languagePage.IsLanguageDisplayed(originalLanguage), Is.False);
        Assert.That(languagePage.IsLanguageDisplayed(updatedLanguage), Is.False);

        try
        {
            // Arrange: create and verify the record we will edit.
            languagePage.AddLanguage(originalLanguage, "Basic");

            Assert.That(
                languagePage.IsLanguageAndLevelDisplayed(originalLanguage, "Basic"),
                Is.True,
                "The original language should exist before editing."
            );

            // Act: edit the record.
            languagePage.EditLanguage(originalLanguage, updatedLanguage, "Fluent");

            // Assert: the updated row replaces the original.
            Assert.That(
                languagePage.IsLanguageAndLevelDisplayed(updatedLanguage, "Fluent"),
                Is.True,
                "The updated language and level should appear."
            );

            Assert.That(
                languagePage.IsLanguageRemoved(originalLanguage),
                Is.True,
                "The original language should no longer appear."
            );

            Assert.That(
                languagePage.GetLanguageRowCount(),
                Is.EqualTo(countBefore + 1),
                "Editing should not create an extra row."
            );
        }
        finally
        {
            // The record may have either name if the test fails during editing.
            languagePage.DeleteLanguageIfExists(originalLanguage);
            languagePage.DeleteLanguageIfExists(updatedLanguage);
        }
    }

    [Test]
    public void DeleteLanguageWithValidDetails()
    {
        string? email = Environment.GetEnvironmentVariable("MARS_EMAIL");
        string? password = Environment.GetEnvironmentVariable("MARS_PASSWORD");

        Assert.That(email, Is.Not.Null.And.Not.Empty, "MARS_EMAIL is not set.");
        Assert.That(password, Is.Not.Null.And.Not.Empty, "MARS_PASSWORD is not set.");

        string language = "Auto" + Guid.NewGuid().ToString("N")[..8];
        string level = "Basic";

        using IWebDriver driver = new ChromeDriver();
        driver.Navigate().GoToUrl("http://localhost:5003/Home");

        var loginPage = new LoginPage(driver);
        loginPage.Login(email!, password!);

        Assert.That(
            loginPage.IsLoggedInSuccessfully(),
            Is.True,
            "Login should succeed before testing languages."
        );

        var languagePage = new LanguagePage(driver);
        int countBefore = languagePage.GetLanguageRowCount();

        Assert.That(
            countBefore,
            Is.LessThan(4),
            "The profile needs a free language slot for this test."
        );

        Assert.That(
            languagePage.IsLanguageDisplayed(language),
            Is.False,
            "The test language must not already exist."
        );

        try
        {
            // Arrange: create the record that this test will delete.
            languagePage.AddLanguage(language, level);

            Assert.That(
                languagePage.IsLanguageAndLevelDisplayed(language, level),
                Is.True,
                "The test language should exist before deletion."
            );

            // Act: delete the record.
            languagePage.DeleteLanguage(language);

            // Assert: check both removal and row count.
            Assert.That(
                languagePage.IsLanguageRemoved(language),
                Is.True,
                "The deleted language should no longer appear."
            );

            Assert.That(
                languagePage.WaitForLanguageRowCount(countBefore),
                Is.True,
                "The row count should return to its original value."
            );
        }
        finally
        {
            // If deletion failed, attempt to remove our test record.
            languagePage.DeleteLanguageIfExists(language);
        }
    }
}