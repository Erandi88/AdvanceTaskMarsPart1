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

    [Test]
    public void AddSkillPersistsAfterRefresh()
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
            "AddSkillPersistsAfterRefresh.json"
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
            // Arrange: add and verify the record.
            page.AddSkill(testData.Skill, testData.Level);

            Assert.That(
                page.IsSkillAndLevelDisplayed(
                    testData.Skill,
                    testData.Level
                ),
                Is.True,
                "The skill should appear before refresh."
            );

            Assert.That(
                page.WaitForSkillRowCount(countBefore + 1),
                Is.True
            );

            // Act: reload and reopen the Skills tab.
            browser.Navigate().Refresh();
            page.ClickSkillsTab();

            // Assert: the saved record survives the reload.
            Assert.That(
                page.IsSkillAndLevelDisplayed(
                    testData.Skill,
                    testData.Level
                ),
                Is.True,
                "The saved skill and level should remain after refresh."
            );

            Assert.That(
                page.WaitForSkillRowCount(countBefore + 1),
                Is.True,
                "The row count should remain unchanged after refresh."
            );

            Assert.That(
                page.GetSkillRecordCount(testData.Skill),
                Is.EqualTo(1),
                "Exactly one record with this skill should remain."
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

    [Test]
    public void EditSkillLevelOnly()
    {
        var page = skillPage
            ?? throw new InvalidOperationException(
                "Skill page was not created."
            );

        string filePath = Path.Combine(
            TestContext.CurrentContext.TestDirectory,
            "TestData",
            "Skill",
            "EditSkillLevelOnly.json"
        );

        EditSkillData testData =
            JsonReader.Read<EditSkillData>(filePath);

        Assert.That(testData.OriginalSkill, Is.Not.Null.And.Not.Empty);
        Assert.That(testData.OriginalLevel, Is.Not.Null.And.Not.Empty);
        Assert.That(testData.UpdatedLevel, Is.Not.Null.And.Not.Empty);

        Assert.That(
            testData.UpdatedSkill,
            Is.EqualTo(testData.OriginalSkill),
            "This test must keep the skill name unchanged."
        );

        Assert.That(
            testData.UpdatedLevel,
            Is.Not.EqualTo(testData.OriginalLevel),
            "This test requires a different level."
        );

        Assert.That(
            page.IsSkillDisplayed(testData.OriginalSkill),
            Is.False,
            "The test skill must not already exist."
        );

        int countBefore = page.GetSkillRowCount();

        try
        {
            // Arrange: create the original record.
            page.AddSkill(
                testData.OriginalSkill,
                testData.OriginalLevel
            );

            Assert.That(
                page.IsSkillAndLevelDisplayed(
                    testData.OriginalSkill,
                    testData.OriginalLevel
                ),
                Is.True
            );

            Assert.That(
                page.WaitForSkillRowCount(countBefore + 1),
                Is.True,
                "Adding the setup record should increase the row count by one."
            );

            // Act: submit the same name with a different level.
            page.EditSkill(
                testData.OriginalSkill,
                testData.UpdatedSkill,
                testData.UpdatedLevel
            );

            // Assert: only the level changed.
            Assert.That(
                page.IsSkillAndLevelDisplayed(
                    testData.OriginalSkill,
                    testData.UpdatedLevel
                ),
                Is.True,
                "The skill should display its updated level."
            );

            Assert.That(
                page.GetSkillRecordCount(
                    testData.OriginalSkill,
                    testData.OriginalLevel
                ),
                Is.Zero,
                "The original level should no longer appear."
            );

            Assert.That(
                page.GetSkillRecordCount(testData.OriginalSkill),
                Is.EqualTo(1),
                "Exactly one record with this skill should remain."
            );

            Assert.That(
                page.GetSkillRowCount(),
                Is.EqualTo(countBefore + 1),
                "Editing should not create an additional record."
            );
        }
        finally
        {
            page.DeleteSkillIfExists(testData.OriginalSkill);
        }
    }

    [Test]
    public void EditSkillNameOnly()
    {
        var page = skillPage
            ?? throw new InvalidOperationException(
                "Skill page was not created."
            );

        string filePath = Path.Combine(
            TestContext.CurrentContext.TestDirectory,
            "TestData",
            "Skill",
            "EditSkillNameOnly.json"
        );

        EditSkillData testData =
            JsonReader.Read<EditSkillData>(filePath);

        Assert.That(testData.OriginalSkill, Is.Not.Null.And.Not.Empty);
        Assert.That(testData.UpdatedSkill, Is.Not.Null.And.Not.Empty);
        Assert.That(testData.OriginalLevel, Is.Not.Null.And.Not.Empty);

        Assert.That(
            testData.UpdatedSkill,
            Is.Not.EqualTo(testData.OriginalSkill),
            "This test requires a different skill name."
        );

        Assert.That(
            testData.UpdatedLevel,
            Is.EqualTo(testData.OriginalLevel),
            "The proficiency level must remain unchanged."
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

        try
        {
            // Arrange: create the original record.
            page.AddSkill(
                testData.OriginalSkill,
                testData.OriginalLevel
            );

            Assert.That(
                page.IsSkillAndLevelDisplayed(
                    testData.OriginalSkill,
                    testData.OriginalLevel
                ),
                Is.True
            );

            Assert.That(
                page.WaitForSkillRowCount(countBefore + 1),
                Is.True,
                "Adding the setup record should increase the row count by one."
            );

            // Act: change the name while retaining the same level.
            page.EditSkill(
                testData.OriginalSkill,
                testData.UpdatedSkill,
                testData.UpdatedLevel
            );

            // Assert: the renamed record retains its level.
            Assert.That(
                page.IsSkillAndLevelDisplayed(
                    testData.UpdatedSkill,
                    testData.OriginalLevel
                ),
                Is.True,
                "The updated name should retain the original level."
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
            page.DeleteSkillIfExists(testData.OriginalSkill);
            page.DeleteSkillIfExists(testData.UpdatedSkill);
        }
    }

    [Test]
    public void EditSkillWithEmptyName()
    {
        var page = skillPage
            ?? throw new InvalidOperationException(
                "Skill page was not created."
            );

        string filePath = Path.Combine(
            TestContext.CurrentContext.TestDirectory,
            "TestData",
            "Skill",
            "EditSkillWithEmptyName.json"
        );

        EditSkillData testData =
            JsonReader.Read<EditSkillData>(filePath);

        Assert.That(testData.OriginalSkill, Is.Not.Null.And.Not.Empty);
        Assert.That(testData.OriginalLevel, Is.Not.Null.And.Not.Empty);
        Assert.That(testData.UpdatedSkill, Is.EqualTo(string.Empty));

        Assert.That(
            testData.UpdatedLevel,
            Is.EqualTo(testData.OriginalLevel),
            "The level must remain unchanged for this test."
        );

        Assert.That(
            page.IsSkillDisplayed(testData.OriginalSkill),
            Is.False,
            "The test skill must not already exist."
        );

        int countBefore = page.GetSkillRowCount();

        try
        {
            // Arrange: create the original record.
            page.AddSkill(
                testData.OriginalSkill,
                testData.OriginalLevel
            );

            Assert.That(
                page.IsSkillAndLevelDisplayed(
                    testData.OriginalSkill,
                    testData.OriginalLevel
                ),
                Is.True
            );

            Assert.That(
                page.WaitForSkillRowCount(countBefore + 1),
                Is.True,
                "Adding the setup record should increase the row count by one."
            );

            // Act: clear the name and attempt to update.
            page.ClickEditSkill(testData.OriginalSkill);
            page.ClearSkillField();

            Assert.That(
                page.GetSkillFieldValue(),
                Is.EqualTo(testData.UpdatedSkill),
                "The skill name must be empty before submitting."
            );

            page.ClickUpdateButton();

            // Assert: validation appears and editing remains open.
            Assert.That(
                page.IsSkillValidationMessageDisplayed(),
                Is.True,
                "The required-field validation message should appear."
            );

            Assert.That(
                page.GetSkillFieldValue(),
                Is.EqualTo(string.Empty),
                "The edit form should remain open with an empty name."
            );

            page.ClickCancelButton();

            Assert.That(
                page.IsSkillAndLevelDisplayed(
                    testData.OriginalSkill,
                    testData.OriginalLevel
                ),
                Is.True,
                "The original skill and level should remain unchanged."
            );

            Assert.That(
                page.GetSkillRecordCount(testData.OriginalSkill),
                Is.EqualTo(1),
                "Exactly one original record should remain."
            );

            Assert.That(
                page.GetSkillRowCount(),
                Is.EqualTo(countBefore + 1),
                "The rejected update should not add or remove records."
            );
        }
        finally
        {
            page.DeleteSkillIfExists(testData.OriginalSkill);
        }
    }

    [Test]
    public void EditSkillWithEmptyLevel()
    {
        var page = skillPage
            ?? throw new InvalidOperationException(
                "Skill page was not created."
            );

        string filePath = Path.Combine(
            TestContext.CurrentContext.TestDirectory,
            "TestData",
            "Skill",
            "EditSkillWithEmptyLevel.json"
        );

        EditSkillData testData =
            JsonReader.Read<EditSkillData>(filePath);

        Assert.That(testData.OriginalSkill, Is.Not.Null.And.Not.Empty);
        Assert.That(testData.OriginalLevel, Is.Not.Null.And.Not.Empty);
        Assert.That(testData.UpdatedSkill, Is.EqualTo(testData.OriginalSkill));
        Assert.That(testData.UpdatedLevel, Is.EqualTo(string.Empty));

        Assert.That(
            page.IsSkillDisplayed(testData.OriginalSkill),
            Is.False,
            "The test skill must not already exist."
        );

        int countBefore = page.GetSkillRowCount();

        try
        {
            // Arrange: create the original record.
            page.AddSkill(
                testData.OriginalSkill,
                testData.OriginalLevel
            );

            Assert.That(
                page.IsSkillAndLevelDisplayed(
                    testData.OriginalSkill,
                    testData.OriginalLevel
                ),
                Is.True
            );

            Assert.That(
                page.WaitForSkillRowCount(countBefore + 1),
                Is.True,
                "Adding the setup record should increase the row count by one."
            );

            // Act: select the default level option and submit.
            page.EditSkillWithEmptyLevel(testData.OriginalSkill);

            // Assert: the invalid update is rejected.
            Assert.That(
                page.IsSkillValidationMessageDisplayed(),
                Is.True,
                "The required-field validation message should appear."
            );

            Assert.That(
                page.GetSkillFieldValue(),
                Is.EqualTo(testData.OriginalSkill),
                "The edit form should remain open with the original name."
            );

            page.ClickCancelButton();

            Assert.That(
                page.IsSkillAndLevelDisplayed(
                    testData.OriginalSkill,
                    testData.OriginalLevel
                ),
                Is.True,
                "The original skill and level should remain unchanged."
            );

            Assert.That(
                page.GetSkillRecordCount(testData.OriginalSkill),
                Is.EqualTo(1),
                "Exactly one original record should remain."
            );

            Assert.That(
                page.GetSkillRowCount(),
                Is.EqualTo(countBefore + 1),
                "The rejected update should not add or remove records."
            );
        }
        finally
        {
            page.DeleteSkillIfExists(testData.OriginalSkill);
        }
    }

    [Test]
    public void EditSkillToExistingRecord()
    {
        var page = skillPage
            ?? throw new InvalidOperationException(
                "Skill page was not created."
            );

        string filePath = Path.Combine(
            TestContext.CurrentContext.TestDirectory,
            "TestData",
            "Skill",
            "EditSkillToExistingRecord.json"
        );

        EditSkillData testData =
            JsonReader.Read<EditSkillData>(filePath);

        Assert.That(testData.OriginalSkill, Is.Not.Null.And.Not.Empty);
        Assert.That(testData.OriginalLevel, Is.Not.Null.And.Not.Empty);
        Assert.That(testData.UpdatedSkill, Is.Not.Null.And.Not.Empty);
        Assert.That(testData.UpdatedLevel, Is.Not.Null.And.Not.Empty);

        Assert.That(
            string.Equals(
                testData.OriginalSkill,
                testData.UpdatedSkill,
                StringComparison.OrdinalIgnoreCase
            ),
            Is.False,
            "The setup requires two different skill names."
        );

        Assert.That(
            page.IsSkillDisplayed(testData.OriginalSkill),
            Is.False,
            "The first test skill must not already exist."
        );

        Assert.That(
            page.IsSkillDisplayed(testData.UpdatedSkill),
            Is.False,
            "The second test skill must not already exist."
        );

        int countBefore = page.GetSkillRowCount();

        try
        {
            // Arrange: create the first record.
            page.AddSkill(
                testData.OriginalSkill,
                testData.OriginalLevel
            );

            Assert.That(
                page.IsSkillAndLevelDisplayed(
                    testData.OriginalSkill,
                    testData.OriginalLevel
                ),
                Is.True
            );

            Assert.That(
                page.WaitForSkillRowCount(countBefore + 1),
                Is.True,
                "Adding the setup record should increase the row count by one."
            );

            // Create the record that the edit will attempt to duplicate.
            page.AddSkill(
                testData.UpdatedSkill,
                testData.UpdatedLevel
            );

            Assert.That(
                page.IsSkillAndLevelDisplayed(
                    testData.UpdatedSkill,
                    testData.UpdatedLevel
                ),
                Is.True
            );

            Assert.That(
                page.WaitForSkillRowCount(countBefore + 2),
                Is.True,
                "Adding the setup record should increase the row count by two."

            );

            // Act: change the first record to match the second.
            page.EditSkill(
                testData.OriginalSkill,
                testData.UpdatedSkill,
                testData.UpdatedLevel
            );

            Assert.That(
                page.IsSkillAlreadyAddedMessageDisplayed(),
                Is.True,
                "The duplicate update should display a validation message."
            );

            // Cancel the rejected edit to inspect the saved records.
            page.ClickCancelButton();

            // Assert: both original records remain unchanged.
            Assert.That(
                page.IsSkillAndLevelDisplayed(
                    testData.OriginalSkill,
                    testData.OriginalLevel
                ),
                Is.True,
                "The first record should remain unchanged."
            );

            Assert.That(
                page.IsSkillAndLevelDisplayed(
                    testData.UpdatedSkill,
                    testData.UpdatedLevel
                ),
                Is.True,
                "The second record should remain unchanged."
            );

            Assert.That(
                page.GetSkillRecordCount(testData.OriginalSkill),
                Is.EqualTo(1),
                "Exactly one original skill should remain."
            );

            Assert.That(
                page.GetSkillRecordCount(testData.UpdatedSkill),
                Is.EqualTo(1),
                "Exactly one target skill should remain."
            );

            Assert.That(
                page.GetSkillRowCount(),
                Is.EqualTo(countBefore + 2),
                "The rejected edit should not add or remove records."
            );
        }
        finally
        {
            page.DeleteSkillIfExists(testData.OriginalSkill);
            page.DeleteSkillIfExists(testData.UpdatedSkill);
        }
    }

    [Test]
    public void CancelEditSkill()
    {
        var page = skillPage
            ?? throw new InvalidOperationException(
                "Skill page was not created."
            );

        string filePath = Path.Combine(
            TestContext.CurrentContext.TestDirectory,
            "TestData",
            "Skill",
            "CancelEditSkill.json"
        );

        EditSkillData testData =
            JsonReader.Read<EditSkillData>(filePath);

        Assert.That(testData.OriginalSkill, Is.Not.Null.And.Not.Empty);
        Assert.That(testData.UpdatedSkill, Is.Not.Null.And.Not.Empty);
        Assert.That(testData.OriginalLevel, Is.Not.Null.And.Not.Empty);
        Assert.That(testData.UpdatedLevel, Is.Not.Null.And.Not.Empty);

        Assert.That(
            testData.UpdatedSkill,
            Is.Not.EqualTo(testData.OriginalSkill)
        );

        Assert.That(
            testData.UpdatedLevel,
            Is.Not.EqualTo(testData.OriginalLevel)
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

        try
        {
            // Arrange: create the original record.
            page.AddSkill(
                testData.OriginalSkill,
                testData.OriginalLevel
            );

            Assert.That(
                page.IsSkillAndLevelDisplayed(
                    testData.OriginalSkill,
                    testData.OriginalLevel
                ),
                Is.True
            );

            Assert.That(
                page.WaitForSkillRowCount(countBefore + 1),
                Is.True
            );

            // Act: enter changes, then cancel without clicking Update.
            page.ClickEditSkill(testData.OriginalSkill);
            page.EnterSkill(testData.UpdatedSkill);
            page.SelectSkillLevel(testData.UpdatedLevel);

            Assert.That(
                page.GetSkillFieldValue(),
                Is.EqualTo(testData.UpdatedSkill),
                "The edited name should be entered before cancelling."
            );

            page.ClickCancelButton();

            // Assert: the saved record remains unchanged.
            Assert.That(
                page.IsSkillAndLevelDisplayed(
                    testData.OriginalSkill,
                    testData.OriginalLevel
                ),
                Is.True,
                "Cancel should preserve the original skill and level."
            );

            Assert.That(
                page.GetSkillRecordCount(testData.UpdatedSkill),
                Is.Zero,
                "The unsaved skill name should not appear."
            );

            Assert.That(
                page.GetSkillRecordCount(testData.OriginalSkill),
                Is.EqualTo(1),
                "Exactly one original record should remain."
            );

            Assert.That(
                page.GetSkillRowCount(),
                Is.EqualTo(countBefore + 1),
                "Cancelling should not add or remove records."
            );
        }
        finally
        {
            page.DeleteSkillIfExists(testData.OriginalSkill);
            page.DeleteSkillIfExists(testData.UpdatedSkill);
        }
    }

    [Test]
    public void EditSkillPersistsAfterRefresh()
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
            "EditSkillPersistsAfterRefresh.json"
        );

        EditSkillData testData =
            JsonReader.Read<EditSkillData>(filePath);

        Assert.That(testData.OriginalSkill, Is.Not.Null.And.Not.Empty);
        Assert.That(testData.UpdatedSkill, Is.Not.Null.And.Not.Empty);
        Assert.That(testData.OriginalLevel, Is.Not.Null.And.Not.Empty);
        Assert.That(testData.UpdatedLevel, Is.Not.Null.And.Not.Empty);

        Assert.That(
            testData.UpdatedSkill,
            Is.Not.EqualTo(testData.OriginalSkill)
        );

        Assert.That(
            testData.UpdatedLevel,
            Is.Not.EqualTo(testData.OriginalLevel)
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

        try
        {
            // Arrange: create the original record.
            page.AddSkill(
                testData.OriginalSkill,
                testData.OriginalLevel
            );

            Assert.That(
                page.IsSkillAndLevelDisplayed(
                    testData.OriginalSkill,
                    testData.OriginalLevel
                ),
                Is.True
            );

            Assert.That(
                page.WaitForSkillRowCount(countBefore + 1),
                Is.True
            );

            // Act: edit the record.
            page.EditSkill(
                testData.OriginalSkill,
                testData.UpdatedSkill,
                testData.UpdatedLevel
            );

            Assert.That(
                page.IsSkillAndLevelDisplayed(
                    testData.UpdatedSkill,
                    testData.UpdatedLevel
                ),
                Is.True,
                "The updated record should appear before refresh."
            );

            Assert.That(
                page.IsSkillRemoved(testData.OriginalSkill),
                Is.True
            );

            browser.Navigate().Refresh();
            page.ClickSkillsTab();

            // Assert: the update survives the reload.
            Assert.That(
                page.IsSkillAndLevelDisplayed(
                    testData.UpdatedSkill,
                    testData.UpdatedLevel
                ),
                Is.True,
                "The updated skill and level should remain after refresh."
            );

            Assert.That(
                page.IsSkillRemoved(testData.OriginalSkill),
                Is.True,
                "The original name should not reappear after refresh."
            );

            Assert.That(
                page.GetSkillRecordCount(testData.UpdatedSkill),
                Is.EqualTo(1),
                "Exactly one updated record should remain."
            );

            Assert.That(
                page.WaitForSkillRowCount(countBefore + 1),
                Is.True,
                "Editing and refreshing should not change the row count."
            );
        }
        finally
        {
            page.DeleteSkillIfExists(testData.OriginalSkill);
            page.DeleteSkillIfExists(testData.UpdatedSkill);
        }
    }

    [Test]
    public void EditSkillLeavesOtherRecordUnchanged()
    {
        var page = skillPage
            ?? throw new InvalidOperationException(
                "Skill page was not created."
            );

        string filePath = Path.Combine(
            TestContext.CurrentContext.TestDirectory,
            "TestData",
            "Skill",
            "EditSkillLeavesOtherRecordUnchanged.json"
        );

        EditSkillIsolationData testData =
            JsonReader.Read<EditSkillIsolationData>(filePath);

        var edit = testData.RecordToEdit;
        var other = testData.OtherRecord;

        Assert.That(edit.OriginalSkill, Is.Not.Null.And.Not.Empty);
        Assert.That(edit.OriginalLevel, Is.Not.Null.And.Not.Empty);
        Assert.That(edit.UpdatedSkill, Is.Not.Null.And.Not.Empty);
        Assert.That(edit.UpdatedLevel, Is.Not.Null.And.Not.Empty);
        Assert.That(other.Skill, Is.Not.Null.And.Not.Empty);
        Assert.That(other.Level, Is.Not.Null.And.Not.Empty);

        string[] testNames =
        {
        edit.OriginalSkill,
        edit.UpdatedSkill,
        other.Skill
    };

        Assert.That(
            testNames.Distinct(StringComparer.OrdinalIgnoreCase).Count(),
            Is.EqualTo(3),
            "This test requires three different skill names."
        );

        foreach (string name in testNames)
        {
            Assert.That(
                page.IsSkillDisplayed(name),
                Is.False,
                $"'{name}' must not already exist."
            );
        }

        int countBefore = page.GetSkillRowCount();

        try
        {
            // Arrange: create the record to edit.
            page.AddSkill(edit.OriginalSkill, edit.OriginalLevel);

            Assert.That(
                page.IsSkillAndLevelDisplayed(
                    edit.OriginalSkill,
                    edit.OriginalLevel
                ),
                Is.True
            );

            Assert.That(
                page.WaitForSkillRowCount(countBefore + 1),
                Is.True
            );

            // Create the record that must remain unchanged.
            page.AddSkill(other.Skill, other.Level);

            Assert.That(
                page.IsSkillAndLevelDisplayed(other.Skill, other.Level),
                Is.True
            );

            Assert.That(
                page.WaitForSkillRowCount(countBefore + 2),
                Is.True
            );

            // Act: edit only the first record.
            page.EditSkill(
                edit.OriginalSkill,
                edit.UpdatedSkill,
                edit.UpdatedLevel
            );

            // Assert: the selected record changed correctly.
            Assert.That(
                page.IsSkillAndLevelDisplayed(
                    edit.UpdatedSkill,
                    edit.UpdatedLevel
                ),
                Is.True,
                "The selected skill should display its updated values."
            );

            Assert.That(
                page.IsSkillRemoved(edit.OriginalSkill),
                Is.True,
                "The original skill name should no longer appear."
            );

            // The second record retains its name and level.
            Assert.That(
                page.IsSkillAndLevelDisplayed(other.Skill, other.Level),
                Is.True,
                "The other skill and level should remain unchanged."
            );

            Assert.That(
                page.GetSkillRecordCount(other.Skill),
                Is.EqualTo(1),
                "Exactly one unchanged record should remain."
            );

            Assert.That(
                page.GetSkillRecordCount(edit.UpdatedSkill),
                Is.EqualTo(1),
                "Exactly one updated record should exist."
            );

            Assert.That(
                page.GetSkillRowCount(),
                Is.EqualTo(countBefore + 2),
                "Editing should not add or remove records."
            );
        }
        finally
        {
            page.DeleteSkillIfExists(edit.OriginalSkill);
            page.DeleteSkillIfExists(edit.UpdatedSkill);
            page.DeleteSkillIfExists(other.Skill);
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

    [Test]
    public void DeleteSkillPersistsAfterRefresh()
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
            "DeleteSkillPersistsAfterRefresh.json"
        );

        DeleteSkillPersistenceData testData =
            JsonReader.Read<DeleteSkillPersistenceData>(filePath);

        var target = testData.RecordToDelete;
        var other = testData.OtherRecord;

        Assert.That(target.Skill, Is.Not.Null.And.Not.Empty);
        Assert.That(target.Level, Is.Not.Null.And.Not.Empty);
        Assert.That(other.Skill, Is.Not.Null.And.Not.Empty);
        Assert.That(other.Level, Is.Not.Null.And.Not.Empty);

        Assert.That(
            string.Equals(
                target.Skill,
                other.Skill,
                StringComparison.OrdinalIgnoreCase
            ),
            Is.False,
            "This test requires two different skill names."
        );

        Assert.That(
            page.IsSkillDisplayed(target.Skill),
            Is.False,
            "The deletion target must not already exist."
        );

        Assert.That(
            page.IsSkillDisplayed(other.Skill),
            Is.False,
            "The other test skill must not already exist."
        );

        int countBefore = page.GetSkillRowCount();

        try
        {
            // Arrange: create both temporary records.
            page.AddSkill(target.Skill, target.Level);

            Assert.That(
                page.IsSkillAndLevelDisplayed(target.Skill, target.Level),
                Is.True
            );

            Assert.That(
                page.WaitForSkillRowCount(countBefore + 1),
                Is.True
            );

            page.AddSkill(other.Skill, other.Level);

            Assert.That(
                page.IsSkillAndLevelDisplayed(other.Skill, other.Level),
                Is.True
            );

            Assert.That(
                page.WaitForSkillRowCount(countBefore + 2),
                Is.True
            );

            // Act: delete the selected skill.
            page.DeleteSkill(target.Skill);

            Assert.That(
                page.IsSkillRemoved(target.Skill),
                Is.True,
                "The selected skill should disappear before refresh."
            );

            Assert.That(
                page.WaitForSkillRowCount(countBefore + 1),
                Is.True,
                "Deletion should reduce the row count by one."
            );

            browser.Navigate().Refresh();
            page.ClickSkillsTab();

            // Wait for saved Skill data to load after refresh.
            Assert.That(
                page.IsSkillAndLevelDisplayed(other.Skill, other.Level),
                Is.True,
                "The other skill and its level should remain unchanged."
            );

            // Assert: the deleted record has not returned.
            Assert.That(
                page.IsSkillRemoved(target.Skill),
                Is.True,
                "The deleted skill must not reappear after refresh."
            );

            Assert.That(
                page.GetSkillRecordCount(target.Skill),
                Is.Zero,
                "No record with the deleted skill name should remain."
            );

            Assert.That(
                page.GetSkillRecordCount(other.Skill),
                Is.EqualTo(1),
                "Exactly one copy of the other skill should remain."
            );

            Assert.That(
                page.WaitForSkillRowCount(countBefore + 1),
                Is.True,
                "The row count should remain unchanged after refresh."
            );
        }
        finally
        {
            page.DeleteSkillIfExists(target.Skill);
            page.DeleteSkillIfExists(other.Skill);
        }
    }

    [Test]
    public void DeleteLastSkill()
    {
        var page = skillPage
            ?? throw new InvalidOperationException(
                "Skill page was not created."
            );

        string filePath = Path.Combine(
            TestContext.CurrentContext.TestDirectory,
            "TestData",
            "Skill",
            "DeleteLastSkill.json"
        );

        SkillData testData =
            JsonReader.Read<SkillData>(filePath);

        Assert.That(testData.Skill, Is.Not.Null.And.Not.Empty);
        Assert.That(testData.Level, Is.Not.Null.And.Not.Empty);

        Assert.That(
            page.GetSkillRowCount(),
            Is.Zero,
            "This test requires an empty skill list."
        );

        try
        {
            // Arrange: create the only skill record.
            page.AddSkill(testData.Skill, testData.Level);

            Assert.That(
                page.IsSkillAndLevelDisplayed(
                    testData.Skill,
                    testData.Level
                ),
                Is.True
            );

            Assert.That(
                page.WaitForSkillRowCount(1),
                Is.True,
                "Exactly one skill should exist before deletion."
            );

            // Act: delete the last record.
            page.DeleteSkill(testData.Skill);

            Assert.That(
                page.IsSkillRemoved(testData.Skill),
                Is.True,
                "The last skill should disappear."
            );

            Assert.That(
                page.WaitForSkillRowCount(0),
                Is.True,
                "No skill records should remain."
            );

            // Verify Add New still opens the form.
            page.ClickAddNewButton();

            Assert.That(
                page.GetSkillFieldValue(),
                Is.EqualTo(string.Empty),
                "Add New should open an empty skill form."
            );

            page.ClickCancelButton();

            Assert.That(
                page.GetSkillRowCount(),
                Is.Zero,
                "Opening and cancelling the form should not create a record."
            );
        }
        finally
        {
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