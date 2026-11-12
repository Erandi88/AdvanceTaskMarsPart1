using AventStack.ExtentReports;
using MarsAdvanced.Tests.Helpers;
using MarsAdvanced.Tests.Models;
using MarsAdvanced.Tests.Pages;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using AventStack.ExtentReports;

namespace MarsAdvanced.Tests;

[TestFixture]
public class SkillTests
{
    private IWebDriver? driver;
    private SkillPage? skillPage;
    private ExtentTest? reportTest;

    [SetUp]
    public void SetUp()
    {
        // Create the report entry before setup can fail.
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
            ?? throw new InvalidOperationException("Skill page was not created.");

        string filePath = Path.Combine(
            TestContext.CurrentContext.TestDirectory,
            "TestData", "Skill", "AddSkillWithValidDetails.json");
        var data = JsonReader.Read<SkillData>(filePath);

        Assert.That(data.Skill, Is.Not.Null.And.Not.Empty);
        Assert.That(data.Level, Is.Not.Null.And.Not.Empty);
        Assert.That(page.IsSkillDisplayed(data.Skill), Is.False,
            "The test skill must not already exist.");

        int countBefore = page.GetSkillRowCount();

        try
        {
            page.AddSkill(data.Skill, data.Level);

            Assert.That(
                page.IsSkillAndLevelDisplayed(data.Skill, data.Level),
                Is.True, "The new skill and level should appear.");
            Assert.That(page.WaitForSkillRowCount(countBefore + 1), Is.True,
                "Adding a skill should increase the row count by one.");
            Assert.That(page.GetSkillRecordCount(data.Skill), Is.EqualTo(1),
                "Exactly one record with this skill should exist.");
        }
        finally
        {
            page.DeleteSkillIfExists(data.Skill);
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
            ?? throw new InvalidOperationException("Skill page was not created.");
        var browser = driver
            ?? throw new InvalidOperationException("Browser was not created.");

        string filePath = Path.Combine(
            TestContext.CurrentContext.TestDirectory,
            "TestData", "Skill", "EditSkillWithValidDetails.json");
        var data = JsonReader.Read<EditSkillData>(filePath);

        ValidateSkillEditData(data);

        Assert.That(page.IsSkillDisplayed(data.OriginalSkill), Is.False,
            "The original test skill must not already exist.");
        Assert.That(page.IsSkillDisplayed(data.UpdatedSkill), Is.False,
            "The updated test skill must not already exist.");
        int countBefore = page.GetSkillRowCount();

        try
        {
            page.AddSkill(data.OriginalSkill, data.OriginalLevel);
            Assert.That(
                page.IsSkillAndLevelDisplayed(data.OriginalSkill, data.OriginalLevel),
                Is.True, "The original skill should exist before editing.");
            Assert.That(page.WaitForSkillRowCount(countBefore + 1), Is.True,
                "Adding the setup record should increase the row count by one.");

            page.EditSkill(data.OriginalSkill, data.UpdatedSkill, data.UpdatedLevel);
            Assert.That(
                page.IsSkillAndLevelDisplayed(data.UpdatedSkill, data.UpdatedLevel),
                Is.True, "The updated skill and level should appear.");
            Assert.That(page.IsSkillRemoved(data.OriginalSkill), Is.True,
                "The original skill name should no longer appear.");
            Assert.That(page.GetSkillRecordCount(data.UpdatedSkill), Is.EqualTo(1),
                "Exactly one record with the updated name should exist.");
            Assert.That(page.GetSkillRowCount(), Is.EqualTo(countBefore + 1),
                "Editing should not create an additional record.");
        }
        finally
        {
            CleanupOwnedSkillNames(browser, page, data.OriginalSkill, data.UpdatedSkill);
        }
    }

    private static void ValidateSkillEditData(EditSkillData data)
    {
        Assert.That(data.OriginalSkill, Is.Not.Null.And.Not.Empty);
        Assert.That(data.OriginalLevel, Is.Not.Null.And.Not.Empty);
        Assert.That(data.UpdatedSkill, Is.Not.Null.And.Not.Empty);
        Assert.That(data.UpdatedLevel, Is.Not.Null.And.Not.Empty);
        Assert.That(data.UpdatedSkill, Is.Not.EqualTo(data.OriginalSkill),
            "This test requires a different skill name.");
        Assert.That(data.UpdatedLevel, Is.Not.EqualTo(data.OriginalLevel),
            "This test requires a different level.");
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
            ?? throw new InvalidOperationException("Skill page was not created.");

        string filePath = Path.Combine(
            TestContext.CurrentContext.TestDirectory,
            "TestData", "Skill", "DeleteSkillWithValidDetails.json");
        var data = JsonReader.Read<SkillData>(filePath);

        Assert.That(data.Skill, Is.Not.Null.And.Not.Empty);
        Assert.That(data.Level, Is.Not.Null.And.Not.Empty);
        Assert.That(page.IsSkillDisplayed(data.Skill), Is.False,
            "The test skill must not already exist.");

        int countBefore = page.GetSkillRowCount();

        try
        {
            page.AddSkill(data.Skill, data.Level);

            Assert.That(
                page.IsSkillAndLevelDisplayed(data.Skill, data.Level),
                Is.True, "The skill should exist before deletion.");
            Assert.That(page.WaitForSkillRowCount(countBefore + 1), Is.True,
                "Setup should create exactly one record.");

            page.DeleteSkill(data.Skill);

            Assert.That(page.IsSkillRemoved(data.Skill), Is.True,
                "The deleted skill should no longer appear.");
            Assert.That(page.GetSkillRecordCount(data.Skill), Is.Zero,
                "No record with the deleted skill name should remain.");
            Assert.That(page.WaitForSkillRowCount(countBefore), Is.True,
                "Deletion should restore the original row count.");
        }
        finally
        {
            page.DeleteSkillIfExists(data.Skill);
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

    [Test]
    [NonParallelizable]
    public void AddFifthSkillWithValidDetails()
    {
        var page = skillPage
            ?? throw new InvalidOperationException("Skill page was not created.");
        
        var browser = driver
            ?? throw new InvalidOperationException("Browser was not created.");
        
        string filePath = Path.Combine(
            TestContext.CurrentContext.TestDirectory,
            "TestData", "Skill", "AddFifthSkillWithValidDetails.json"
        );
        
        var skills = JsonReader.Read<List<SkillData>>(filePath);
        ValidateFiveNewSkills(page, skills);
        int countBefore = page.GetSkillRowCount();
        var attemptedSkills = new List<SkillData>();

        try
        {
            foreach (var skill in skills)
            {
                attemptedSkills.Add(skill);
                page.AddSkill(skill.Skill, skill.Level);
                Assert.That(
                    page.WaitForSkillRowCount(countBefore + attemptedSkills.Count),
                    Is.True,
                    $"Adding {skill.Skill} should increase the row count."
                );
            }

            VerifyFiveSkills(page, skills, countBefore + 5);
            browser.Navigate().Refresh();
            page.ClickSkillsTab();
            VerifyFiveSkills(page, skills, countBefore + 5);
        }
        finally
        {
            foreach (var skill in attemptedSkills)
                page.DeleteSkillIfExists(skill.Skill);
        }
    }

    //Helpers
    private static void ValidateFiveNewSkills(SkillPage page, List<SkillData> skills)
    {
        Assert.That(skills, Has.Count.EqualTo(5));

        foreach (var skill in skills)
        {
            Assert.That(skill.Skill, Is.Not.Null.And.Not.Empty);
            Assert.That(skill.Level, Is.Not.Null.And.Not.Empty);
        }

        Assert.That(
            skills.Select(skill => skill.Skill)
                .Distinct(StringComparer.OrdinalIgnoreCase).Count(),
            Is.EqualTo(5),
            "The JSON must contain five different skill names."
        );

        foreach (var skill in skills)
        {
            Assert.That(
                page.IsSkillDisplayed(skill.Skill),
                Is.False,
                $"Setup requires {skill.Skill} to be absent."
            );
        }
    }

    private static void VerifyFiveSkills(SkillPage page, List<SkillData> skills,
    int expectedCount)
    {
        Assert.That(
            page.WaitForSkillRowCount(expectedCount),
            Is.True,
            "All five added records should remain."
        );

        foreach (var skill in skills)
        {
            Assert.That(
                page.IsSkillAndLevelDisplayed(skill.Skill, skill.Level),
                Is.True,
                $"{skill.Skill} should retain its saved level."
            );

            Assert.That(
                page.GetSkillRecordCount(skill.Skill),
                Is.EqualTo(1),
                $"{skill.Skill} should appear exactly once."
            );
        }
    }

    [Test]
    [NonParallelizable]
    public void SkillRowsShowEditAndDeleteIcons()
    {
        var page = skillPage
            ?? throw new InvalidOperationException("Skill page was not created.");

        string filePath = Path.Combine(
            TestContext.CurrentContext.TestDirectory,
            "TestData", "Skill", "SkillRowsShowEditAndDeleteIcons.json"
        );

        var data = JsonReader.Read<List<SkillData>>(filePath);
        ValidateSkillIconSetup(page, data);

        int countBefore = page.GetSkillRowCount();
        var attemptedSkills = new List<string>();

        try
        {
            foreach (var item in data)
            {
                attemptedSkills.Add(item.Skill);
                page.AddSkill(item.Skill, item.Level);

                Assert.That(
                    page.IsSkillAndLevelDisplayed(item.Skill, item.Level),
                    Is.True,
                    $"Setup should create {item.Skill} with its selected level."
                );
            }

            Assert.That(
                page.WaitForSkillRowCount(countBefore + data.Count),
                Is.True
            );

            Assert.That(
                page.WaitForSkillRowIcons(countBefore + data.Count),
                Is.True,
                "Every Skill row should show exactly one Edit and one Delete icon."
            );
        }
        finally
        {
            foreach (string skill in attemptedSkills)
                page.DeleteSkillIfExists(skill);
        }
    }

    private static void ValidateSkillIconSetup(SkillPage page, List<SkillData> data)
    {
        Assert.That(data, Has.Count.EqualTo(2));

        foreach (var item in data)
        {
            Assert.That(item.Skill, Is.Not.Null.And.Not.Empty);
            Assert.That(item.Level, Is.Not.Null.And.Not.Empty);
        }

        Assert.That(
            data.Select(item => item.Skill)
                .Distinct(StringComparer.OrdinalIgnoreCase).Count(),
            Is.EqualTo(2),
            "The setup requires two different skill names."
        );

        foreach (var item in data)
        {
            Assert.That(
                page.IsSkillDisplayed(item.Skill),
                Is.False,
                $"Preserve existing data: {item.Skill} must be absent before setup."
            );
        }
    }

    //add space only
    [Test]
    [NonParallelizable]
    public void AddSkillRejectsSpacesOnlyName()
    {
        var page = skillPage
            ?? throw new InvalidOperationException("Skill page was not created.");
        var browser = driver
            ?? throw new InvalidOperationException("Browser was not created.");
        string filePath = Path.Combine(
            TestContext.CurrentContext.TestDirectory,
            "TestData", "Skill", "AddSkillWithSpacesOnlyName.json"
        );
        var data = JsonReader.Read<SkillData>(filePath);

        Assert.That(data.Skill, Is.EqualTo(new string(' ', 5)));
        Assert.That(data.Level, Is.EqualTo("Beginner"));
        Assert.That(page.GetSkillRowCount(), Is.Zero,
            "This scenario requires an empty Skills table; preserve existing data.");
        bool addAttempted = false;

        try
        {
            page.ClickAddNewButton();
            page.EnterSkill(data.Skill);
            page.SelectSkillLevel(data.Level);
            Assert.That(page.GetSkillFieldValue(), Is.EqualTo(data.Skill),
                "The input must contain exactly five spaces.");

            addAttempted = true;
            page.ClickAddAndWaitForResponse();

            Assert.That(page.GetSkillRowCount(), Is.Zero,
                "Submitting five spaces must not create a blank-looking Skill record.");

            browser.Navigate().Refresh();
            page.ClickSkillsTab();
            Assert.That(page.GetSkillRowCount(), Is.Zero,
                "No Skill record should exist after refresh.");
        }
        catch
        {
            CaptureSkillFailureBeforeCleanup(browser);
            throw;
        }
        finally
        {
            if (addAttempted)
                page.DeleteOnlyBlankSkillIfExists();
        }
    }

    private void CaptureSkillFailureBeforeCleanup(IWebDriver browser, string caption = "Failure state before Skill cleanup")
    {
        try
        {
            if (browser is ITakesScreenshot screenshotDriver)
            {
                string screenshot = screenshotDriver
                    .GetScreenshot().AsBase64EncodedString;

                reportTest?.AddScreenCaptureFromBase64String(
                    screenshot,
                    caption
                );
            }
        }
        catch (Exception exception)
        {
            Console.WriteLine(
                $"Could not capture the failure screenshot: {exception.Message}"
            );
        }
    }

    //Edit - space only
    [Test]
    [NonParallelizable]
    public void EditSkillRejectsSpacesOnlyName()
    {
        var page = skillPage
            ?? throw new InvalidOperationException("Skill page was not created.");
        var browser = driver
            ?? throw new InvalidOperationException("Browser was not created.");
        string filePath = Path.Combine(
            TestContext.CurrentContext.TestDirectory,
            "TestData", "Skill", "EditSkillWithSpacesOnlyName.json"
        );
        var data = JsonReader.Read<EditSkillData>(filePath);

        Assert.That(data.OriginalSkill, Is.EqualTo("Selenium"));
        Assert.That(data.OriginalLevel, Is.EqualTo("Beginner"));
        Assert.That(data.UpdatedSkill, Is.EqualTo(new string(' ', 5)));
        Assert.That(data.UpdatedLevel, Is.EqualTo(data.OriginalLevel));
        Assert.That(page.GetSkillRowCount(), Is.Zero,
            "This scenario requires an empty Skills table; preserve existing data.");

        bool creationAttempted = false;

        try
        {
            creationAttempted = true;
            page.AddSkill(data.OriginalSkill, data.OriginalLevel);

            Assert.That(page.IsSkillAndLevelDisplayed(
                data.OriginalSkill, data.OriginalLevel), Is.True);
            Assert.That(page.WaitForSkillRowCount(1), Is.True);

            page.ClickEditSkill(data.OriginalSkill);
            page.EnterSkill(data.UpdatedSkill);

            Assert.That(page.GetSkillFieldValue(), Is.EqualTo(data.UpdatedSkill));
            Assert.That(page.GetSelectedSkillLevel(), Is.EqualTo(data.UpdatedLevel));

            Console.WriteLine(page.SubmitSkillUpdateAndWaitForResponse());
            CaptureSkillFailureBeforeCleanup(
                browser, "Spaces-only Skill Edit before refresh and cleanup");
            VerifyRejectedSkillNameEdit(browser, page, data);
        }
        finally
        {
            if (creationAttempted)
            {
                browser.Navigate().Refresh();
                page.ClickSkillsTab();
                page.DeleteSkillIfExists(data.OriginalSkill);
                page.DeleteOnlyBlankSkillIfExists();
            }
        }
    }

    private static void VerifyRejectedSkillNameEdit(IWebDriver browser, SkillPage page, EditSkillData data)
    {
        browser.Navigate().Refresh();
        page.ClickSkillsTab();

        bool originalRetained = page.IsSkillAndLevelDisplayed(
            data.OriginalSkill,
            data.OriginalLevel
        );

        Assert.Multiple(() =>
        {
            Assert.That(
                originalRetained,
                Is.True,
                "The rejected edit must preserve the original Skill and level."
            );

            Assert.That(
                page.GetSkillRecordCount(data.OriginalSkill),
                Is.EqualTo(1),
                "Exactly one original Skill record should remain."
            );

            Assert.That(
                page.GetSkillRowCount(),
                Is.EqualTo(1),
                "The rejected edit must not add or remove records."
            );
        });
    }

    //duplicate edit with different level
    [Test]
    [NonParallelizable]
    public void EditSkillRejectsExistingNameWithDifferentLevel()
    {
        var page = skillPage
            ?? throw new InvalidOperationException("Skill page was not created.");
        var browser = driver
            ?? throw new InvalidOperationException("Browser was not created.");
        string filePath = Path.Combine(
            TestContext.CurrentContext.TestDirectory,
            "TestData", "Skill",
            "EditSkillToExistingNameWithDifferentLevel.json"
        );

        var data = JsonReader.Read<DuplicateSkillEditData>(filePath);
        ValidateDifferentLevelSkillSetup(page, data);
        int countBefore = page.GetSkillRowCount();
        bool creationAttempted = false;

        try
        {
            creationAttempted = true;
            page.AddSkill(data.ExistingSkill, data.ExistingLevel);
            
            Assert.That(page.IsSkillAndLevelDisplayed(
                data.ExistingSkill, data.ExistingLevel), Is.True);
           
            page.AddSkill(data.OriginalSkill, data.OriginalLevel);
            
            Assert.That(page.IsSkillAndLevelDisplayed(
                data.OriginalSkill, data.OriginalLevel), Is.True);
            Assert.That(page.WaitForSkillRowCount(countBefore + 2), Is.True);

            page.ClickEditSkill(data.OriginalSkill);
            page.EnterSkill(data.UpdatedSkill);
            page.SelectSkillLevel(data.UpdatedLevel);
            
            Assert.That(page.GetSkillFieldValue(), Is.EqualTo(data.UpdatedSkill));
            Assert.That(page.GetSelectedSkillLevel(), Is.EqualTo(data.UpdatedLevel));
            Console.WriteLine(page.SubmitSkillUpdateAndWaitForResponse());

            CaptureSkillFailureBeforeCleanup(
                browser, "Different-level duplicate Skill Edit before cleanup");
            VerifyDuplicateSkillEditRejected(browser, page, data, countBefore);
        }
        finally
        {
            if (creationAttempted)
                CleanupOwnedSkillNames(browser, page,
                    data.ExistingSkill, data.OriginalSkill, data.UpdatedSkill);
        }
    }

    private static void ValidateDifferentLevelSkillSetup(SkillPage page,DuplicateSkillEditData data)
    {
        Assert.That(data.OriginalSkill, Is.Not.Null.And.Not.Empty);
        Assert.That(data.ExistingSkill, Is.Not.Null.And.Not.Empty);
        Assert.That(data.OriginalLevel, Is.Not.Null.And.Not.Empty);
        Assert.That(data.ExistingLevel, Is.Not.Null.And.Not.Empty);
        Assert.That(data.UpdatedSkill, Is.EqualTo(data.ExistingSkill));
        Assert.That(data.UpdatedLevel, Is.EqualTo(data.OriginalLevel));
        Assert.That(data.ExistingLevel, Is.Not.EqualTo(data.UpdatedLevel));
        Assert.That(
            string.Equals(data.OriginalSkill, data.ExistingSkill,
                StringComparison.OrdinalIgnoreCase),
            Is.False,
            "Setup requires two different Skill names."
        );

        foreach (string name in new[]
        {
            data.OriginalSkill,
            data.ExistingSkill
        })
        {
            Assert.That(page.IsSkillDisplayed(name), Is.False,
                $"Preserve existing data: '{name}' must be absent before setup.");
        }
    }

    private static void VerifyDuplicateSkillEditRejected(IWebDriver browser,SkillPage page,
        DuplicateSkillEditData data,int countBefore)
    {
        browser.Navigate().Refresh();
        page.ClickSkillsTab();

        bool existingRetained = page.IsSkillAndLevelDisplayed(
            data.ExistingSkill, data.ExistingLevel
        );
        bool originalRetained = page.IsSkillAndLevelDisplayed(
            data.OriginalSkill, data.OriginalLevel
        );

        Assert.Multiple(() =>
        {
            Assert.That(existingRetained, Is.True,
                "The existing Skill and level must remain unchanged.");
            Assert.That(originalRetained, Is.True,
                "The rejected edit must preserve the first original record.");
            Assert.That(page.GetSkillRecordCount(data.ExistingSkill),
                Is.EqualTo(1),
                "The existing Skill name must appear exactly once.");
            Assert.That(page.GetSkillRecordCount(data.OriginalSkill),
                Is.EqualTo(1));
            Assert.That(page.GetSkillRecordCount(
                data.UpdatedSkill, data.UpdatedLevel), Is.Zero,
                "The attempted duplicate with a different level must not be saved.");
            Assert.That(page.GetSkillRowCount(), Is.EqualTo(countBefore + 2));
        });
    }

    private static void CleanupOwnedSkillNames(IWebDriver browser, SkillPage page, params string[] names)
    {
        browser.Navigate().Refresh();
        ProfileDataWait.WaitForResponse(browser, "getSkill");
        page.ClickSkillsTab();
        //page.IsSkillDisplayed(names[0]);

        var failures = new List<Exception>();

        foreach (string name in names.Distinct(StringComparer.Ordinal))
        {
            try
            {
                page.DeleteOwnedSkillRecords(name);
            }
            catch (Exception exception)
            {
                failures.Add(new InvalidOperationException(
                    $"Cleanup failed for Skill '{name}'.", exception
                ));
            }
        }

        if (failures.Count > 0)
            throw new AggregateException("Skill cleanup failed.", failures);
    }

    //clicking Update without changing the Skill or level.
    [Test]
    [NonParallelizable]
    public void UpdateSkillWithoutChanges()
    {
        var page = skillPage
            ?? throw new InvalidOperationException("Skill page was not created.");
        var browser = driver
            ?? throw new InvalidOperationException("Browser was not created.");

        string filePath = Path.Combine(
            TestContext.CurrentContext.TestDirectory,
            "TestData", "Skill", "UpdateSkillWithoutChanges.json"
        );
        var data = JsonReader.Read<SkillData>(filePath);

        Assert.That(data.Skill, Is.Not.Null.And.Not.Empty);
        Assert.That(data.Level, Is.Not.Null.And.Not.Empty);
        Assert.That(page.IsSkillDisplayed(data.Skill), Is.False,
            "Preserve existing data: the test Skill must be absent.");
        int countBefore = page.GetSkillRowCount();
        bool creationAttempted = false;

        try
        {
            creationAttempted = true;
            page.AddSkill(data.Skill, data.Level);
            Assert.That(page.IsSkillAndLevelDisplayed(
                data.Skill, data.Level), Is.True);
            Assert.That(page.WaitForSkillRowCount(countBefore + 1), Is.True);

            page.ClickEditSkill(data.Skill);
            Assert.That(page.GetSkillFieldValue(), Is.EqualTo(data.Skill));
            Assert.That(page.GetSelectedSkillLevel(), Is.EqualTo(data.Level));

            string response = page.SubmitSkillUpdateAndWaitForResponse();
            VerifyUnchangedSkillUpdate(browser, page, data, countBefore, response);
        }
        finally
        {
            if (creationAttempted)
                CleanupOwnedSkillNames(browser, page, data.Skill);
        }
    }

    //trimming surrounding spaces when adding a Skill
    [Test]
    [NonParallelizable]
    public void AddSkillTrimsSurroundingSpaces()
    {
        var page = skillPage
            ?? throw new InvalidOperationException("Skill page was not created.");
        var browser = driver
            ?? throw new InvalidOperationException("Browser was not created.");

        string filePath = Path.Combine(
            TestContext.CurrentContext.TestDirectory,
            "TestData", "Skill", "AddSkillTrimsSurroundingSpaces.json"
        );
        var data = JsonReader.Read<SkillData>(filePath);

        Assert.That(data.Skill, Is.EqualTo("  JMeter  "));
        Assert.That(data.Level, Is.EqualTo("Beginner"));
        
        string expectedName = data.Skill.Trim();
        
        Assert.That(page.IsSkillDisplayed(expectedName), Is.False,
            "Preserve existing data: JMeter must be absent before setup.");
        
        int countBefore = page.GetSkillRowCount();
        bool creationAttempted = false;

        try
        {
            page.ClickAddNewButton();
            page.EnterSkill(data.Skill);
            page.SelectSkillLevel(data.Level);
            
            Assert.That(page.GetSkillFieldValue(), Is.EqualTo(data.Skill),
                "The submitted Skill must contain two spaces on each side.");

            creationAttempted = true;
            
            page.ClickAddAndWaitForResponse();
            
            Assert.That(page.WaitForSkillRowCount(countBefore + 1), Is.True);
            
            VerifySavedSkillTrimming(
                browser, page, expectedName, data.Level, countBefore + 1);
        }
        catch
        {
            CaptureSkillFailureBeforeCleanup(browser);
            throw;
        }
        finally
        {
            if (creationAttempted)
                CleanupOwnedSkillNames(browser, page, expectedName);
        }
    }

    private static void VerifySavedSkillTrimming(IWebDriver browser,SkillPage page,
    string expectedName, string expectedLevel,int expectedRowCount)
    {
        browser.Navigate().Refresh();
        page.ClickSkillsTab();

        Assert.That(
            page.IsSkillAndLevelDisplayed(expectedName, expectedLevel),
            Is.True,
            "The saved Skill and level should appear after refresh."
        );
        Assert.That(page.GetSkillRecordCount(expectedName), Is.EqualTo(1));
        Assert.That(page.GetSkillRowCount(), Is.EqualTo(expectedRowCount));

        page.ClickEditSkill(expectedName);
        string savedValue = page.GetSkillFieldValue();

        Console.WriteLine(
            $"Saved Skill: [{savedValue}], length: {savedValue.Length}"
        );

        Assert.Multiple(() =>
        {
            Assert.That(
                savedValue,
                Is.EqualTo(expectedName),
                "The saved Skill must not retain leading or trailing spaces."
            );
            Assert.That(page.GetSelectedSkillLevel(), Is.EqualTo(expectedLevel));
        });
    }

    private void VerifyUnchangedSkillUpdate( IWebDriver browser,SkillPage page,
    SkillData data,int countBefore,string response)
    {
        string[] duplicateMessages =
        {
        "This skill is already added to your skill list",
        "This skill is already exist in your skill list",
        "Duplicated data"
    };

        bool duplicateReported = duplicateMessages.Any(message =>
            response.Contains(message, StringComparison.OrdinalIgnoreCase)
        );

        Console.WriteLine($"Unchanged Skill Update response: [{response}]");

        if (duplicateReported)
            CaptureSkillFailureBeforeCleanup(browser);

        browser.Navigate().Refresh();
        page.ClickSkillsTab();

        bool originalRetained = page.IsSkillAndLevelDisplayed(
            data.Skill, data.Level
        );

        Assert.Multiple(() =>
        {
            Assert.That(originalRetained, Is.True,
                "The original Skill and level should persist after refresh.");
            Assert.That(page.GetSkillRecordCount(data.Skill), Is.EqualTo(1),
                "Updating unchanged values must not create a duplicate.");
            Assert.That(page.GetSkillRowCount(), Is.EqualTo(countBefore + 1),
                "Updating unchanged values must preserve the total row count.");
            Assert.That(duplicateReported, Is.False,
                "An unchanged Skill must not be incorrectly reported as a duplicate.");
        });
    }

    //trimming surrounding spaces during editing
    [Test]
    [NonParallelizable]
    public void EditSkillTrimsSurroundingSpaces()
    {
        var page = skillPage
            ?? throw new InvalidOperationException("Skill page was not created.");
        var browser = driver
            ?? throw new InvalidOperationException("Browser was not created.");

        string filePath = Path.Combine(
            TestContext.CurrentContext.TestDirectory,
            "TestData", "Skill", "EditSkillTrimsSurroundingSpaces.json"
        );
        var data = JsonReader.Read<EditSkillData>(filePath);

        Assert.That(data.OriginalSkill, Is.EqualTo("Postman"));
        Assert.That(data.OriginalLevel, Is.EqualTo("Beginner"));
        Assert.That(data.UpdatedSkill, Is.EqualTo("  JMeter  "));
        Assert.That(data.UpdatedLevel, Is.EqualTo(data.OriginalLevel));

        string expectedName = data.UpdatedSkill.Trim();

        Assert.That(page.IsSkillDisplayed(data.OriginalSkill), Is.False,
            "Preserve existing data: Postman must be absent before setup.");
        Assert.That(page.IsSkillDisplayed(expectedName), Is.False,
            "Preserve existing data: JMeter must be absent before setup.");

        int countBefore = page.GetSkillRowCount();
        bool creationAttempted = false;

        try
        {
            creationAttempted = true;
            page.AddSkill(data.OriginalSkill, data.OriginalLevel);

            Assert.That(
                page.IsSkillAndLevelDisplayed(
                    data.OriginalSkill, data.OriginalLevel),
                Is.True
            );
            Assert.That(page.WaitForSkillRowCount(countBefore + 1), Is.True);

            page.ClickEditSkill(data.OriginalSkill);
            page.EnterSkill(data.UpdatedSkill);

            Assert.That(
                page.GetSkillFieldValue(),
                Is.EqualTo(data.UpdatedSkill),
                "The input must contain two spaces on each side."
            );
            Assert.That(
                page.GetSelectedSkillLevel(),
                Is.EqualTo(data.UpdatedLevel)
            );

            Console.WriteLine(page.SubmitSkillUpdateAndWaitForResponse());

            // This helper refreshes, checks the saved record, and reopens Edit.
            VerifySavedSkillTrimming(
                browser,
                page,
                expectedName,
                data.UpdatedLevel,
                countBefore + 1
            );

            // Close Edit before checking the displayed table records.
            page.ClickCancelButton();

            Assert.That(
                page.GetSkillRecordCount(data.OriginalSkill),
                Is.Zero,
                "The old Skill name should no longer exist after the rename."
            );
        }
        catch
        {
            CaptureSkillFailureBeforeCleanup(browser);
            throw;
        }
        finally
        {
            if (creationAttempted)
            {
                CleanupOwnedSkillNames(
                    browser,
                    page,
                    expectedName,
                    data.OriginalSkill
                );
            }
        }
    }

    //Add numbers and spcial chracters
    [TestCase(
        "AddSkillWithNumericOnlyName.json",
        "34567892",
        "Beginner",
        TestName = "AddSkillRejectsNumericOnlyName"
    )]

    [TestCase(
        "AddSkillWithSpecialCharactersOnlyName.json",
        "##@*!@#",
        "Beginner",
        TestName = "AddSkillRejectsSpecialCharactersOnlyName"
    )]

    [NonParallelizable]
    [Category("RequirementClarification")]
    public void AddSkillRejectsInvalidName(string jsonFileName, string expectedInput, string expectedLevel)
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
            jsonFileName
        );

        var data = JsonReader.Read<SkillData>(filePath);

        Assert.That(data.Skill, Is.EqualTo(expectedInput));
        Assert.That(data.Level, Is.EqualTo(expectedLevel));

        Assert.That(
            page.IsSkillDisplayed(data.Skill),
            Is.False,
            "Preserve existing data: the test Skill must be absent."
        );

        int countBefore = page.GetSkillRowCount();
        bool addAttempted = false;

        try
        {
            page.ClickAddNewButton();
            page.EnterSkill(data.Skill);
            page.SelectSkillLevel(data.Level);

            Assert.That(
                page.GetSkillFieldValue(),
                Is.EqualTo(data.Skill),
                "The submitted input must match the scenario JSON."
            );

            Assert.That(
                page.GetSelectedSkillLevel(),
                Is.EqualTo(data.Level)
            );

            addAttempted = true;
            page.ClickAddAndWaitForResponse();

            Assert.Multiple(() =>
            {
                Assert.That(
                    page.GetSkillRecordCount(data.Skill),
                    Is.Zero,
                    "Under the workbook expectation, the invalid Skill name must not be saved."
                );

                Assert.That(
                    page.GetSkillRowCount(),
                    Is.EqualTo(countBefore),
                    "The rejected attempt must preserve the original row count."
                );
            });

            browser.Navigate().Refresh();
            page.ClickSkillsTab();

            Assert.Multiple(() =>
            {
                Assert.That(
                    page.GetSkillRecordCount(data.Skill),
                    Is.Zero,
                    "The invalid Skill must remain absent after refresh."
                );

                Assert.That(
                    page.GetSkillRowCount(),
                    Is.EqualTo(countBefore),
                    "The original row count must remain unchanged after refresh."
                );
            });
        }
        catch
        {
            CaptureSkillFailureBeforeCleanup(browser);
            throw;
        }
        finally
        {
            if (addAttempted)
                CleanupOwnedSkillNames(browser, page, data.Skill);
        }
    }

    //case-only duplicate Add.

    [Test]
    [NonParallelizable]
    [Category("RequirementClarification")]
    public void AddSkillRejectsCaseOnlyDuplicate()
    {
        var page = skillPage
            ?? throw new InvalidOperationException("Skill page was not created.");
        var browser = driver
            ?? throw new InvalidOperationException("Browser was not created.");

        string filePath = Path.Combine(
            TestContext.CurrentContext.TestDirectory,
            "TestData", "Skill", "AddSkillWithCaseOnlyDuplicate.json"
        );
        var data = JsonReader.Read<List<SkillData>>(filePath);

        Assert.That(data, Has.Count.EqualTo(2));
        Assert.That(data[0].Skill, Is.EqualTo("Selenium"));
        Assert.That(data[1].Skill, Is.EqualTo("selenium"));
        Assert.That(data[0].Level, Is.EqualTo("Beginner"));
        Assert.That(data[1].Level, Is.EqualTo(data[0].Level));

        foreach (var item in data)
        {
            Assert.That(
                page.IsSkillDisplayed(item.Skill),
                Is.False,
                $"Preserve existing data: '{item.Skill}' must be absent."
            );
        }

        int countBefore = page.GetSkillRowCount();
        bool creationAttempted = false;

        try
        {
            creationAttempted = true;
            page.AddSkill(data[0].Skill, data[0].Level);

            Assert.That(
                page.IsSkillAndLevelDisplayed(data[0].Skill, data[0].Level),
                Is.True
            );
            Assert.That(page.WaitForSkillRowCount(countBefore + 1), Is.True);

            page.ClickAddNewButton();
            page.EnterSkill(data[1].Skill);
            page.SelectSkillLevel(data[1].Level);

            Assert.That(page.GetSkillFieldValue(), Is.EqualTo(data[1].Skill));
            Assert.That(page.GetSelectedSkillLevel(), Is.EqualTo(data[1].Level));

            page.ClickAddAndWaitForResponse();

            CaptureSkillFailureBeforeCleanup(
                browser,
                "Case-only duplicate Skill Add before refresh and cleanup"
            );

            VerifyCaseOnlySkillAddRejected(browser, page, data, countBefore);
        }
        finally
        {
            if (creationAttempted)
            {
                CleanupOwnedSkillNames(
                    browser,
                    page,
                    data[0].Skill,
                    data[1].Skill
                );
            }
        }
    }

    private static void VerifyCaseOnlySkillAddRejected(IWebDriver browser, SkillPage page,
    List<SkillData> data, int countBefore)
    {
        browser.Navigate().Refresh();
        page.ClickSkillsTab();

        bool originalRetained = page.IsSkillAndLevelDisplayed(
            data[0].Skill,
            data[0].Level
        );

        Assert.Multiple(() =>
        {
            Assert.That(
                originalRetained,
                Is.True,
                "The original Selenium record and level must remain unchanged."
            );

            Assert.That(
                page.GetSkillRecordCount(data[0].Skill),
                Is.EqualTo(1),
                "Exactly one original Selenium record should remain."
            );

            Assert.That(
                page.GetSkillRecordCount(data[1].Skill),
                Is.Zero,
                "The lowercase selenium duplicate must not be created."
            );

            Assert.That(
                page.GetSkillRowCount(),
                Is.EqualTo(countBefore + 1),
                "The duplicate attempt must not increase the row count."
            );
        });
    }

    //case only - duplicate edit
    [Test]
    [NonParallelizable]
    [Category("RequirementClarification")]
    public void EditSkillRejectsCaseOnlyDuplicate()
    {
        var page = skillPage
            ?? throw new InvalidOperationException("Skill page was not created.");
        var browser = driver
            ?? throw new InvalidOperationException("Browser was not created.");

        string filePath = Path.Combine(
            TestContext.CurrentContext.TestDirectory,
            "TestData", "Skill", "EditSkillWithCaseOnlyDuplicate.json"
        );
        var data = JsonReader.Read<DuplicateSkillEditData>(filePath);
        ValidateCaseDuplicateSkillSetup(page, data);

        int countBefore = page.GetSkillRowCount();
        bool creationAttempted = false;

        try
        {
            creationAttempted = true;

            page.AddSkill(data.ExistingSkill, data.ExistingLevel);
            Assert.That(
                page.IsSkillAndLevelDisplayed(
                    data.ExistingSkill, data.ExistingLevel),
                Is.True
            );

            page.AddSkill(data.OriginalSkill, data.OriginalLevel);
            Assert.That(
                page.IsSkillAndLevelDisplayed(
                    data.OriginalSkill, data.OriginalLevel),
                Is.True
            );
            Assert.That(page.WaitForSkillRowCount(countBefore + 2), Is.True);

            page.ClickEditSkill(data.OriginalSkill);
            page.EnterSkill(data.UpdatedSkill);

            Assert.That(
                page.GetSkillFieldValue(),
                Is.EqualTo(data.UpdatedSkill)
            );
            Assert.That(
                page.GetSelectedSkillLevel(),
                Is.EqualTo(data.UpdatedLevel)
            );

            Console.WriteLine(page.SubmitSkillUpdateAndWaitForResponse());

            CaptureSkillFailureBeforeCleanup(
                browser,
                "Case-only duplicate Skill Edit before refresh and cleanup"
            );

            VerifyDuplicateSkillEditRejected(browser, page, data, countBefore);
        }
        finally
        {
            if (creationAttempted)
            {
                CleanupOwnedSkillNames(
                    browser,
                    page,
                    data.ExistingSkill,
                    data.UpdatedSkill,
                    data.OriginalSkill
                );
            }
        }
    }

    private static void ValidateCaseDuplicateSkillSetup(SkillPage page,DuplicateSkillEditData data)
    {
        Assert.That(data.OriginalSkill, Is.EqualTo("Postman"));
        Assert.That(data.ExistingSkill, Is.EqualTo("Selenium"));
        Assert.That(data.UpdatedSkill, Is.EqualTo("selenium"));
        Assert.That(data.OriginalLevel, Is.EqualTo("Beginner"));
        Assert.That(data.ExistingLevel, Is.EqualTo(data.OriginalLevel));
        Assert.That(data.UpdatedLevel, Is.EqualTo(data.OriginalLevel));

        foreach (string name in new[]
        {
        data.OriginalSkill,
        data.ExistingSkill,
        data.UpdatedSkill
    })
        {
            Assert.That(
                page.IsSkillDisplayed(name),
                Is.False,
                $"Preserve existing data: '{name}' must be absent before setup."
            );
        }
    }

    //Edit - Numbers only
    [Test]
    [NonParallelizable]
    [Category("RequirementClarification")]
    public void EditSkillRejectsNumbersAndSpecialCharacters()
    {
        var page = skillPage
            ?? throw new InvalidOperationException("Skill page was not created.");

        var browser = driver
            ?? throw new InvalidOperationException("Browser was not created.");

        string filePath = Path.Combine(
            TestContext.CurrentContext.TestDirectory,
            "TestData",
            "Skill",
            "EditSkillWithNumbersAndSpecialCharacters.json"
        );

        var data = JsonReader.Read<EditSkillData>(filePath);

        Assert.That(data.OriginalSkill, Is.EqualTo("Postman"));
        Assert.That(data.OriginalLevel, Is.EqualTo("Beginner"));
        Assert.That(data.UpdatedSkill, Is.EqualTo("123@#$"));
        Assert.That(data.UpdatedLevel, Is.EqualTo(data.OriginalLevel));

        Assert.That(
            page.GetSkillRowCount(),
            Is.Zero,
            "This scenario requires an empty Skills table; preserve existing data."
        );

        bool creationAttempted = false;

        try
        {
            creationAttempted = true;
            page.AddSkill(data.OriginalSkill, data.OriginalLevel);

            Assert.That(
                page.IsSkillAndLevelDisplayed(
                    data.OriginalSkill,
                    data.OriginalLevel
                ),
                Is.True
            );

            Assert.That(page.WaitForSkillRowCount(1), Is.True);

            page.ClickEditSkill(data.OriginalSkill);
            page.EnterSkill(data.UpdatedSkill);

            Assert.That(
                page.GetSkillFieldValue(),
                Is.EqualTo(data.UpdatedSkill),
                "The input must match the numbers/special-character scenario."
            );

            Assert.That(
                page.GetSelectedSkillLevel(),
                Is.EqualTo(data.UpdatedLevel)
            );

            Console.WriteLine(page.SubmitSkillUpdateAndWaitForResponse());

            CaptureSkillFailureBeforeCleanup(
                browser,
                "Numbers/special-character Skill Edit before cleanup"
            );

            VerifyRejectedSkillNameEdit(browser, page, data);
        }
        finally
        {
            if (creationAttempted)
            {
                CleanupOwnedSkillNames(
                    browser,
                    page,
                    data.OriginalSkill,
                    data.UpdatedSkill
                );
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
                skillPage = null;
                reportTest = null;

                ExtentReportManager.Flush();
            }
        }
    }
}