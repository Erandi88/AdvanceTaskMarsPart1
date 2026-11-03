using MarsAdvanced.Tests.Helpers;
using MarsAdvanced.Tests.Models;
using MarsAdvanced.Tests.Pages;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace MarsAdvanced.Tests;

[TestFixture]
public class SkillTests
{
    private IWebDriver? driver;
    private SkillPage? skillPage;

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
            "Login should succeed before testing skills."
        );

        skillPage = new SkillPage(driver);
        skillPage.ClickSkillsTab();

        // Wait until the Skills section is ready.
        skillPage.ClickAddNewButton();
        skillPage.ClickCancelButton();
    }

    /* Add Skills */

    [Test]
    public void AddSkillWithValidDetails()
    {
        var page = skillPage
            ?? throw new InvalidOperationException(
                "Skill page was not created."
            );

        var browser = driver
            ?? throw new InvalidOperationException(
                "Browser was not created."
            );

        string filePath = Path.Combine(
            TestContext.CurrentContext.TestDirectory,
            "TestData",
            "Skill",
            "AddSkillWithValidDetails.json"
        );

        SkillData testData =
            JsonReader.Read<SkillData>(filePath);

        Assert.That(testData.Skill, Is.Not.Null.And.Not.Empty);
        Assert.That(testData.Level, Is.Not.Null.And.Not.Empty);

        Assert.That(
            page.IsSkillDisplayed(testData.Skill),
            Is.False,
            "The test skill must not already exist."
        );

        int countBefore = page.GetSkillRowCount();

        try
        {
            // Act: add the skill using JSON data.
            page.AddSkill(testData.Skill, testData.Level);

            // Assert: verify the saved name and level.
            Assert.That(
                page.IsSkillAndLevelDisplayed(
                    testData.Skill,
                    testData.Level
                ),
                Is.True,
                "The new skill and level should appear."
            );

            var wait = new WebDriverWait(
                browser,
                TimeSpan.FromSeconds(30)
            );

            Assert.That(
                 page.WaitForSkillRowCount(countBefore + 1),
                 Is.True,
                 "Adding the setup record should increase the row count by one."
            );

            Assert.That(
                page.GetSkillRecordCount(testData.Skill),
                Is.EqualTo(1),
                "Exactly one record with this skill should exist."
            );
        }
        finally
        {
            page.DeleteSkillIfExists(testData.Skill);
        }
    }

    [Test]
    public void AddSkillWithEmptyName()
    {
        var page = skillPage
            ?? throw new InvalidOperationException(
                "Skill page was not created."
            );

        string filePath = Path.Combine(
            TestContext.CurrentContext.TestDirectory,
            "TestData",
            "Skill",
            "AddSkillWithEmptyName.json"
        );

        SkillData testData =
            JsonReader.Read<SkillData>(filePath);

        Assert.That(
            testData.Skill,
            Is.EqualTo(string.Empty),
            "This test requires a completely empty skill name."
        );

        Assert.That(testData.Level, Is.Not.Null.And.Not.Empty);

        int countBefore = page.GetSkillRowCount();

        // Act: submit an empty skill name with a valid level.
        page.AddSkill(testData.Skill, testData.Level);

        Assert.That(
            page.IsSkillValidationMessageDisplayed(),
            Is.True,
            "The required-field validation message should appear."
        );

        Assert.That(
            page.GetSkillRowCount(),
            Is.EqualTo(countBefore),
            "An empty skill name should not create a record."
        );

        Assert.That(
            page.GetSkillFieldValue(),
            Is.EqualTo(string.Empty),
            "The Add form should remain open with an empty name."
        );

        page.ClickCancelButton();

        Assert.That(
            page.GetSkillRowCount(),
            Is.EqualTo(countBefore),
            "Cancelling should leave the row count unchanged."
        );
    }

    [Test]
    public void AddSkillWithoutLevel()
    {
        var page = skillPage
            ?? throw new InvalidOperationException(
                "Skill page was not created."
            );

        string filePath = Path.Combine(
            TestContext.CurrentContext.TestDirectory,
            "TestData",
            "Skill",
            "AddSkillWithoutLevel.json"
        );

        SkillData testData =
            JsonReader.Read<SkillData>(filePath);

        Assert.That(testData.Skill, Is.Not.Null.And.Not.Empty);

        Assert.That(
            testData.Level,
            Is.EqualTo(string.Empty),
            "This test requires an unselected level."
        );

        Assert.That(
            page.IsSkillDisplayed(testData.Skill),
            Is.False,
            "The test skill must not already exist."
        );

        int countBefore = page.GetSkillRowCount();

        try
        {
            // Act: leave the level at its default option.
            page.AddSkillWithoutLevel(testData.Skill);

            Assert.That(
                page.IsSkillValidationMessageDisplayed(),
                Is.True,
                "The required-field validation message should appear."
            );

            Assert.That(
                page.GetSkillRowCount(),
                Is.EqualTo(countBefore),
                "No skill record should be created."
            );

            Assert.That(
                page.GetSkillFieldValue(),
                Is.EqualTo(testData.Skill),
                "The form should remain open with the entered skill."
            );

            page.ClickCancelButton();

            Assert.That(
                page.GetSkillRecordCount(testData.Skill),
                Is.Zero,
                "The attempted skill should not be saved."
            );

            Assert.That(
                page.GetSkillRowCount(),
                Is.EqualTo(countBefore),
                "Cancelling should leave the row count unchanged."
            );
        }
        finally
        {
            // Clean up if the application unexpectedly saved the skill.
            page.DeleteSkillIfExists(testData.Skill);
        }
    }

    [Test]
    public void AddDuplicateSkillWithSameLevel()
    {
        var page = skillPage
            ?? throw new InvalidOperationException(
                "Skill page was not created."
            );

        string filePath = Path.Combine(
            TestContext.CurrentContext.TestDirectory,
            "TestData",
            "Skill",
            "AddDuplicateSkillWithSameLevel.json"
        );

        SkillData testData =
            JsonReader.Read<SkillData>(filePath);

        Assert.That(testData.Skill, Is.Not.Null.And.Not.Empty);
        Assert.That(testData.Level, Is.Not.Null.And.Not.Empty);

        Assert.That(
            page.IsSkillDisplayed(testData.Skill),
            Is.False,
            "The test skill must not already exist."
        );

        int countBefore = page.GetSkillRowCount();

        try
        {
            // Arrange: create the original record.
            page.AddSkill(testData.Skill, testData.Level);

            Assert.That(
                page.IsSkillAndLevelDisplayed(
                    testData.Skill,
                    testData.Level
                ),
                Is.True,
                "The original skill should exist before the duplicate attempt."
            );

            Assert.That(
                page.WaitForSkillRowCount(countBefore + 1),
                Is.True,
                "Adding the setup record should increase the row count by one."
           );

            // Act: submit the same skill and level again.
            page.AddSkill(testData.Skill, testData.Level);

            // Assert: check both the message and saved data.
            Assert.That(
                page.IsDuplicateSkillMessageDisplayed(),
                Is.True,
                "The duplicate validation message should appear."
            );

            Assert.That(
                page.GetSkillFieldValue(),
                Is.EqualTo(testData.Skill),
                "The Add form should remain open after rejection."
            );

            page.ClickCancelButton();

            Assert.That(
                page.GetSkillRecordCount(testData.Skill),
                Is.EqualTo(1),
                "Only one record with this skill should remain."
            );

            Assert.That(
                page.IsSkillAndLevelDisplayed(
                    testData.Skill,
                    testData.Level
                ),
                Is.True,
                "The original skill and level should remain unchanged."
            );

            Assert.That(
                page.GetSkillRowCount(),
                Is.EqualTo(countBefore + 1),
                "The duplicate attempt should not create another record."
            );
        }
        finally
        {
            page.DeleteSkillIfExists(testData.Skill);
        }
    }

    [Test]
    public void AddDuplicateSkillWithDifferentLevel()
    {
        var page = skillPage
            ?? throw new InvalidOperationException(
                "Skill page was not created."
            );

        string filePath = Path.Combine(
            TestContext.CurrentContext.TestDirectory,
            "TestData",
            "Skill",
            "AddDuplicateSkillWithDifferentLevel.json"
        );

        DuplicateSkillData testData =
            JsonReader.Read<DuplicateSkillData>(filePath);

        Assert.That(testData.Skill, Is.Not.Null.And.Not.Empty);
        Assert.That(testData.OriginalLevel, Is.Not.Null.And.Not.Empty);
        Assert.That(testData.AttemptedLevel, Is.Not.Null.And.Not.Empty);

        Assert.That(
            testData.AttemptedLevel,
            Is.Not.EqualTo(testData.OriginalLevel),
            "This test requires two different levels."
        );

        Assert.That(
            page.IsSkillDisplayed(testData.Skill),
            Is.False,
            "The test skill must not already exist."
        );

        int countBefore = page.GetSkillRowCount();

        try
        {
            // Arrange: create the original record.
            page.AddSkill(testData.Skill, testData.OriginalLevel);

            Assert.That(
                page.IsSkillAndLevelDisplayed(
                    testData.Skill,
                    testData.OriginalLevel
                ),
                Is.True
            );

            Assert.That(
                page.WaitForSkillRowCount(countBefore + 1),
                Is.True,
                "Adding the setup record should increase the row count by one."
            );

            // Act: try the same skill with a different level.
            page.AddSkill(testData.Skill, testData.AttemptedLevel);

            Assert.That(
                page.IsDuplicatedDataMessageDisplayed(),
                Is.True,
                "The 'Duplicated data' message should appear."
            );

            page.ClickCancelButton();

            // Assert: the original remains and no duplicate was saved.
            Assert.That(
                page.GetSkillRecordCount(testData.Skill),
                Is.EqualTo(1),
                "Only one record with this skill should remain."
            );

            Assert.That(
                page.IsSkillAndLevelDisplayed(
                    testData.Skill,
                    testData.OriginalLevel
                ),
                Is.True,
                "The original level should remain unchanged."
            );

            Assert.That(
                page.GetSkillRecordCount(
                    testData.Skill,
                    testData.AttemptedLevel
                ),
                Is.Zero,
                "The attempted level should not be saved."
            );

            Assert.That(
                page.GetSkillRowCount(),
                Is.EqualTo(countBefore + 1),
                "The duplicate attempt should not add another record."
            );
        }
        finally
        {
            page.DeleteSkillIfExists(testData.Skill);
        }
    }

    /* Edit Skill*/
    [Test]
    public void EditSkillWithValidDetails()
    {
        var page = skillPage
            ?? throw new InvalidOperationException(
                "Skill page was not created."
            );

        var browser = driver
            ?? throw new InvalidOperationException(
                "Browser was not created."
            );

        string filePath = Path.Combine(
            TestContext.CurrentContext.TestDirectory,
            "TestData",
            "Skill",
            "EditSkillWithValidDetails.json"
        );

        EditSkillData testData =
            JsonReader.Read<EditSkillData>(filePath);

        Assert.That(testData.OriginalSkill, Is.Not.Null.And.Not.Empty);
        Assert.That(testData.OriginalLevel, Is.Not.Null.And.Not.Empty);
        Assert.That(testData.UpdatedSkill, Is.Not.Null.And.Not.Empty);
        Assert.That(testData.UpdatedLevel, Is.Not.Null.And.Not.Empty);

        Assert.That(
            testData.UpdatedSkill,
            Is.Not.EqualTo(testData.OriginalSkill),
            "This test requires a different skill name."
        );

        Assert.That(
            testData.UpdatedLevel,
            Is.Not.EqualTo(testData.OriginalLevel),
            "This test requires a different level."
        );

        Assert.That(
            page.IsSkillDisplayed(testData.OriginalSkill),
            Is.False,
            "The original test skill must not already exist."
        );

        Assert.That(
            page.IsSkillDisplayed(testData.UpdatedSkill),
            Is.False,
            "The updated test skill must not already exist."
        );

        int countBefore = page.GetSkillRowCount();

        var wait = new WebDriverWait(
            browser,
            TimeSpan.FromSeconds(30)
        );

        try
        {
            // Arrange: create the record to edit.
            page.AddSkill(
                testData.OriginalSkill,
                testData.OriginalLevel
            );

            Assert.That(
                page.IsSkillAndLevelDisplayed(
                    testData.OriginalSkill,
                    testData.OriginalLevel
                ),
                Is.True,
                "The original skill should exist before editing."
            );

            Assert.That(
            page.WaitForSkillRowCount(countBefore + 1),
            Is.True,
            "Adding the setup record should increase the row count by one."
        );

            // Act: change the skill name and level.
            page.EditSkill(
                testData.OriginalSkill,
                testData.UpdatedSkill,
                testData.UpdatedLevel
            );

            // Assert: the updated record replaces the original.
            Assert.That(
                page.IsSkillAndLevelDisplayed(
                    testData.UpdatedSkill,
                    testData.UpdatedLevel
                ),
                Is.True,
                "The updated skill and level should appear."
            );

            Assert.That(
                page.IsSkillRemoved(testData.OriginalSkill),
                Is.True,
                "The original skill name should no longer appear."
            );

            Assert.That(
                page.GetSkillRecordCount(testData.UpdatedSkill),
                Is.EqualTo(1),
                "Exactly one record with the updated name should exist."
            );

            Assert.That(
                page.GetSkillRowCount(),
                Is.EqualTo(countBefore + 1),
                "Editing should not create an additional record."
            );
        }
        finally
        {
            // The record may have either name if editing fails.
            page.DeleteSkillIfExists(testData.OriginalSkill);
            page.DeleteSkillIfExists(testData.UpdatedSkill);
        }
    }

    /* Delete Skills*/

    [Test]
    public void DeleteSkillWithValidDetails()
    {
        var page = skillPage
            ?? throw new InvalidOperationException(
                "Skill page was not created."
            );

        string filePath = Path.Combine(
            TestContext.CurrentContext.TestDirectory,
            "TestData",
            "Skill",
            "DeleteSkillWithValidDetails.json"
        );

        SkillData testData = JsonReader.Read<SkillData>(filePath);

        Assert.That(testData.Skill, Is.Not.Null.And.Not.Empty);
        Assert.That(testData.Level, Is.Not.Null.And.Not.Empty);

        Assert.That(
            page.IsSkillDisplayed(testData.Skill),
            Is.False,
            "The test skill must not already exist."
        );

        int countBefore = page.GetSkillRowCount();

        try
        {
            // Arrange: create the record to delete.
            page.AddSkill(testData.Skill, testData.Level);

            Assert.That(
                page.IsSkillAndLevelDisplayed(
                    testData.Skill,
                    testData.Level
                ),
                Is.True,
                "The skill should exist before deletion."
            );

            Assert.That(
                page.WaitForSkillRowCount(countBefore + 1),
                Is.True,
                "Setup should create exactly one record."
            );

            // Act: delete the record.
            page.DeleteSkill(testData.Skill);

            // Assert: verify deletion before cleanup runs.
            Assert.That(
                page.IsSkillRemoved(testData.Skill),
                Is.True,
                "The deleted skill should no longer appear."
            );

            Assert.That(
                page.GetSkillRecordCount(testData.Skill),
                Is.Zero,
                "No record with the deleted skill name should remain."
            );

            Assert.That(
                page.WaitForSkillRowCount(countBefore),
                Is.True,
                "Deletion should restore the original row count."
            );
        }
        finally
        {
            // Remove the temporary record if deletion failed.
            page.DeleteSkillIfExists(testData.Skill);
        }
    }

    [TearDown]
    public void TearDown()
    {
        driver?.Dispose();
        driver = null;
        skillPage = null;
    }
}