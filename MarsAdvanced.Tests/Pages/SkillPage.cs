using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

namespace MarsAdvanced.Tests.Pages
{
    public class SkillPage
    {
        private readonly IWebDriver _driver;
        private readonly WebDriverWait _wait;

        private readonly By SkillsTab = By.XPath("//a[normalize-space()='Skills']");
        private readonly By AddNewButton = By.XPath("//div[contains(@class,'active')]//div[contains(@class,'ui teal button') and normalize-space()='Add New']");
        private readonly By SkillField = By.XPath("//div[contains(@class,'active')]//input[@placeholder='Add Skill']");
        private readonly By SkillLevelDropdown = By.XPath("//div[contains(@class,'active')]//select[@name='level']");
        private readonly By AddButton = By.XPath("//div[contains(@class,'active')]//input[@value='Add']");
        private readonly By UpdateButton = By.XPath("//input[@value='Update'] | //button[normalize-space()='Update']");
        private readonly By CancelButton = By.XPath("//div[contains(@class,'active')]//input[@value='Cancel']");

        public SkillPage(IWebDriver driver)
        {
            _driver = driver;
            _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(30));
        }

        public bool IsProfilePageDisplayed()
        {
            try
            {
                return _wait.Until(driver =>
                    driver.Url.Contains("/Account/Profile")
                );
            }
            catch (WebDriverTimeoutException)
            {
                return false;
            }
        }

        public void ClickSkillsTab()
        {
            var skillsTab = _wait.Until(ExpectedConditions.ElementToBeClickable(SkillsTab));
            skillsTab.Click();
        }

        public void ClickAddNewButton()
        {
            var addNewButton = _wait.Until(ExpectedConditions.ElementToBeClickable(AddNewButton));
            addNewButton.Click();
        }

        public void EnterSkill(string skill)
        {
            var skillInput = _wait.Until(ExpectedConditions.ElementIsVisible(SkillField));
            skillInput.Clear();
            skillInput.SendKeys(skill);
        }

        public void SelectSkillLevel(string level)
        {
            var dropdownElement = _wait.Until(ExpectedConditions.ElementIsVisible(SkillLevelDropdown));
            var selectElement = new SelectElement(dropdownElement);
            selectElement.SelectByText(level);
        }

        public void ClickAddButton()
        {
            var addButton = _wait.Until(ExpectedConditions.ElementToBeClickable(AddButton));
            addButton.Click();
        }

        public void AddSkill(string skill, string level)
        {
            ClickAddNewButton();
            EnterSkill(skill);
            SelectSkillLevel(level);
            ClickAddButton();
        }

        public bool IsSkillDisplayed(string skill)
        {
            try
            {
                var skillRow = By.XPath($"//div[contains(@class,'active')]//td[normalize-space()='{skill}']");
                return _wait.Until(ExpectedConditions.ElementIsVisible(skillRow)).Displayed;
            }
            catch
            {
                return false;
            }
        }

        private By DeleteButtonForSkill(string skill)
        {
            return By.XPath($"//div[contains(@class,'active')]//td[normalize-space()='{skill}']/following-sibling::td//i[contains(@class,'remove')]");
        }

        public void DeleteSkill(string skill)
        {
            var deleteButton = _wait.Until(ExpectedConditions.ElementToBeClickable( DeleteButtonForSkill(skill)));

            deleteButton.Click();
        }

        
        private By EditButtonForSkill(string skill)
        {
            return By.XPath($"//div[contains(@class,'active')]//td[normalize-space()='{skill}']/following-sibling::td//i[contains(@class,'write')]");
            
        }

        public void ClickEditSkill(string skill)
        {
            var editButton = _wait.Until(ExpectedConditions.ElementToBeClickable(EditButtonForSkill(skill)));
            editButton.Click();
        }

        public void ClickUpdateButton()
        {
            var updateButton = _wait.Until(ExpectedConditions.ElementToBeClickable(UpdateButton));
            updateButton.Click();
        }

        public void EditSkill(string currentSkill, string newSkill, string newLevel)
        {
            ClickEditSkill(currentSkill);
            EnterSkill(newSkill);
            SelectSkillLevel(newLevel);
            ClickUpdateButton();
        }

        public void AddSkillWithoutLevel(string skill)
        {
            ClickAddNewButton();
            EnterSkill(skill);
            ClickAddButton();
        }


        public void DeleteSkillIfExists(string skill)
        {
            if (IsSkillDisplayed(skill))
            {
                DeleteSkill(skill);

                if (!IsSkillRemoved(skill))
                {
                    throw new WebDriverTimeoutException(
                        $"The skill '{skill}' was not removed during cleanup."
                    );
                }
            }
        }

        // Add a new skill

        public bool IsSkillAndLevelDisplayed(string skill, string level)
        {
            try
            {
                var skillRow = By.XPath(
                    $"//div[contains(@class,'active')]" +
                    $"//td[normalize-space()='{skill}']" +
                    $"/following-sibling::td[normalize-space()='{level}']"
                );

                return _wait
                    .Until(ExpectedConditions.ElementIsVisible(skillRow))
                    .Displayed;
            }
            catch
            {
                return false;
            }
        }

        public bool IsSkillRemoved(string skill)
        {
            try
            {
                var skillRow = By.XPath(
                    $"//div[contains(@class,'active')]" +
                    $"//td[normalize-space()='{skill}']"
                );

                _wait.Until(
                    ExpectedConditions.InvisibilityOfElementLocated(skillRow)
                );

                return true;
            }
            catch (WebDriverTimeoutException)
            {
                return false;
            }
        }

        //record count
        public int GetSkillRecordCount(string skill, string level)
        {
            var skillRows = By.XPath(
                $"//div[contains(@class,'active')]//tr[" +
                $"td[normalize-space()='{skill}'] and " +
                $"td[normalize-space()='{level}']]"
            );

            return _driver.FindElements(skillRows).Count;
        }

        //duplicate msg
        public bool IsDuplicateSkillMessageDisplayed()
        {
            try
            {
                var message = By.XPath(
                    "//*[normalize-space()='This skill is already exist in your skill list.']"
                );

                return _wait
                    .Until(ExpectedConditions.ElementIsVisible(message))
                    .Displayed;
            }
            catch (WebDriverTimeoutException)
            {
                return false;
            }
        }

        //row count
        public int GetSkillRowCount()
        {
            var skillRows = By.XPath(
                "//div[contains(@class,'active')]//table/tbody/tr"
            );

            return _driver.FindElements(skillRows).Count;
        }

        //empty skill & level msg
        public bool IsSkillValidationMessageDisplayed()
        {
            try
            {
                return _wait.Until(driver =>
                {
                    var messages = driver.FindElements(
                        By.XPath(
                            "//*[contains(normalize-space(.), " +
                            "'Please enter skill and experience level')]"
                        )
                    );

                    return messages.Any(message =>
                        message.Displayed &&
                        message.Text.Contains(
                            "Please enter skill and experience level"
                        )
                    );
                });
            }
            catch (WebDriverTimeoutException)
            {
                return false;
            }
        }

        //record count skill name only
        public int GetSkillRecordCount(string skill)
        {
            var skillRows = By.XPath(
                $"//div[contains(@class,'active')]//tr[" +
                $"td[normalize-space()='{skill}']]"
            );

            return _driver.FindElements(skillRows).Count;
        }

        //duplicate msg
        public bool IsDuplicatedDataMessageDisplayed()
        {
            try
            {
                var message = By.XPath(
                    "//*[normalize-space()='Duplicated data']"
                );

                return _wait
                    .Until(ExpectedConditions.ElementIsVisible(message))
                    .Displayed;
            }
            catch (WebDriverTimeoutException)
            {
                return false;
            }
        }

        //Check that the record remains interactive
        public bool IsSkillDeleteButtonClickable(string skill)
        {
            try
            {
                var deleteButton = _wait.Until(
                    ExpectedConditions.ElementToBeClickable(
                        DeleteButtonForSkill(skill)
                    )
                );

                return deleteButton.Displayed && deleteButton.Enabled;
            }
            catch (WebDriverTimeoutException)
            {
                return false;
            }
        }

        //Update a skill to an existing skill
        public bool IsSkillAlreadyAddedMessageDisplayed()
        {
            try
            {
                var message = By.XPath(
                    "//*[normalize-space()='This skill is already added to your skill list.']"
                );

                return _wait
                    .Until(ExpectedConditions.ElementIsVisible(message))
                    .Displayed;
            }
            catch (WebDriverTimeoutException)
            {
                return false;
            }
        }

        public void ClickCancelButton()
        {
            var cancelButton = _wait.Until(
                ExpectedConditions.ElementToBeClickable(CancelButton)
            );

            cancelButton.Click();
        }

        //Update a skill with an empty skill field
        public void ClearSkillField()
        {
            var skillInput = _wait.Until(
                ExpectedConditions.ElementIsVisible(SkillField)
            );

            skillInput.Click();

            skillInput.SendKeys(Keys.Control + "a");
            skillInput.SendKeys(Keys.Backspace);

            _wait.Until(driver =>
                string.IsNullOrEmpty(skillInput.GetAttribute("value"))
            );
        }

        public void EditSkillWithEmptySkill(string currentSkill)
        {
            ClickEditSkill(currentSkill);
            ClearSkillField();
            ClickUpdateButton();
        }

        public string GetSkillFieldValue()
        {
            var skillInput = _wait.Until(
                ExpectedConditions.ElementIsVisible(SkillField)
            );

            return skillInput.GetAttribute("value") ?? string.Empty;
        }


        //update a skill with empty level
        public void SelectEmptySkillLevel()
        {
            var dropdownElement = _wait.Until(
                ExpectedConditions.ElementIsVisible(SkillLevelDropdown)
            );

            var selectElement = new SelectElement(dropdownElement);

            selectElement.SelectByIndex(0);
        }

        public void EditSkillWithEmptyLevel(string currentSkill)
        {
            ClickEditSkill(currentSkill);
            SelectEmptySkillLevel();
            ClickUpdateButton();
        }

        public bool WaitForSkillRowCount(int expectedCount)
        {
            try
            {
                return _wait.Until(_ =>
                    GetSkillRowCount() == expectedCount
                );
            }
            catch (WebDriverTimeoutException)
            {
                return false;
            }
        }


    }

}