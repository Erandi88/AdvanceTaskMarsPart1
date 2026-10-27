using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

namespace MarsAdvanced.Tests.Pages
{
    public class LanguagePage
    {
        private readonly IWebDriver _driver;
        private readonly WebDriverWait _wait;

        private readonly By AddNewButton = By.XPath("//div[contains(@class,'active')]//div[contains(@class,'ui teal button') and normalize-space()='Add New']");
        private readonly By LanguageField = By.XPath("//input[@placeholder='Add Language']");
        private readonly By LanguageLevelDropdown = By.XPath("//select[@name='level']");
        private readonly By AddButton = By.XPath("//input[@value='Add']");
        private readonly By UpdateButton = By.XPath("//input[@value='Update']");
        private readonly By CancelButton = By.XPath("//div[contains(@class,'active')]//input[@value='Cancel']");
        public LanguagePage(IWebDriver driver)
        {
            _driver = driver;
            _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(30));
        }

        public void ClickAddNewButton()
        {
            var addNewButton = _wait.Until(ExpectedConditions.ElementToBeClickable(AddNewButton));
            addNewButton.Click();
        }

        public void EnterLanguage(string language)
        {
            var languageInput = _wait.Until(ExpectedConditions.ElementIsVisible(LanguageField));
            languageInput.Clear();
            languageInput.SendKeys(language);
        }

        public void SelectLanguageLevel(string level)
        {
            var dropdownElement = _wait.Until(ExpectedConditions.ElementIsVisible(LanguageLevelDropdown));
            var selectElement = new SelectElement(dropdownElement);
            selectElement.SelectByText(level);
        }

        public void ClickAddButton()
        {
            var addButton = _wait.Until(ExpectedConditions.ElementToBeClickable(AddButton));
            addButton.Click();
        }

        public void AddLanguage(string language, string level)
        {
            ClickAddNewButton();
            EnterLanguage(language);
            SelectLanguageLevel(level);
            ClickAddButton();
        }

        public void AddLanguageWithoutLevel(string language)
        {
            ClickAddNewButton();
            EnterLanguage(language);
            ClickAddButton();
        }

        public bool IsLanguageDisplayed(string language)
        {
            try
            {
                var languageRow = By.XPath($"//td[normalize-space()='{language}']");
                return _wait.Until(ExpectedConditions.ElementIsVisible(languageRow)).Displayed;
            }
            catch
            {
                return false;
            }
        }

        public bool IsLanguageAndLevelDisplayed(string language, string level)
        {
            try
            {
                var languageRow = By.XPath(
                    $"//div[contains(@class,'active')]//tr[" + 
                    $"td[normalize-space()='{language}'] and " +
                    $"td[normalize-space()='{level}']]"
                );

                return _wait
                    .Until(ExpectedConditions.ElementIsVisible(languageRow)).Displayed;
            }
            catch (WebDriverTimeoutException)
            {
                return false;
            }
        }

        //Builds the XPath for the correct language row
        private By DeleteButtonForLanguage(string language)
        {
            return By.XPath($"//div[contains(@class,'active')]//td[normalize-space()='{language}']/following-sibling::td//i[contains(@class,'remove')]");

        }

        //Waits for that delete icon and clicks it
        public void DeleteLanguage(string language)
        {
            var deleteButton = _wait.Until(ExpectedConditions.ElementToBeClickable(DeleteButtonForLanguage(language)));
            deleteButton.Click();
        }

        //Check the language removed from the table
        public bool IsLanguageRemoved(string language)
        {
            try
            {
               
                var languageRow = By.XPath($"//div[contains(@class,'active')]//td[normalize-space()='{language}']");
                _wait.Until(ExpectedConditions.InvisibilityOfElementLocated(languageRow));
                return true;
            }
            catch
            {
                return false;
            }
        }

        private By EditButtonForLanguage(string language)
        {
            return By.XPath($"//div[contains(@class,'active')]//td[normalize-space()='{language}']/following-sibling::td//i[contains(@class,'write')]");
            
        }

        public void ClickEditLanguage(string language)
        {
            var editButton = _wait.Until(ExpectedConditions.ElementToBeClickable(EditButtonForLanguage(language)));
            editButton.Click();
        }

        public void ClickUpdateButton()
        {
            var updateButton = _wait.Until(ExpectedConditions.ElementToBeClickable(UpdateButton));
            updateButton.Click();
        }

        public void EditLanguage(string currentLanguage, string newLanguage, string newLevel)
        {
            ClickEditLanguage(currentLanguage);
            EnterLanguage(newLanguage);
            SelectLanguageLevel(newLevel);
            ClickUpdateButton();
        }

        public void DeleteLanguageIfExists(string language)
        {
            if (IsLanguageDisplayed(language))
            {
                DeleteLanguage(language);

                if (!IsLanguageRemoved(language))
                {
                    throw new WebDriverTimeoutException(
                        $"The language '{language}' was not removed during cleanup."
                    );
                }
            }
        }


        public bool IsDuplicateLanguageMessageDisplayed()
        {
            try
            {
                var duplicateMessage = By.XPath(
                    "//*[normalize-space()='This language is already exist in your language list.']"
                );

                return _wait
                    .Until(ExpectedConditions.ElementIsVisible(duplicateMessage))
                    .Displayed;
            }
            catch (WebDriverTimeoutException)
            {
                return false;
            }
        }

        public int GetLanguageRecordCount(string language, string level)
        {
            var matchingRows = By.XPath(
                $"//div[contains(@class,'active')]//tr[" +
                $"td[normalize-space()='{language}'] and " +
                $"td[normalize-space()='{level}']]"
            );

            return _driver.FindElements(matchingRows).Count;
        }

        // returns the current number of language records.
        public int GetLanguageRowCount()
        {
            var languageRows = By.XPath(
                "//div[contains(@class,'active')]//table/tbody/tr"
            );

            return _driver.FindElements(languageRows).Count;
        }

        public bool IsLanguageValidationMessageDisplayed()
        {
            try
            {
                var validationMessage = By.XPath(
                    "//*[normalize-space()='Please enter language and level']"
                );

                return _wait
                    .Until(ExpectedConditions.ElementIsVisible(validationMessage))
                    .Displayed;
            }
            catch (WebDriverTimeoutException)
            {
                return false;
            }
        }

        public int GetLanguageRecordCount(string language)
        {
            var matchingRows = By.XPath(
                $"//div[contains(@class,'active')]//tr[" +
                $"td[normalize-space()='{language}']]"
            );

            return _driver.FindElements(matchingRows).Count;
        }

        public bool IsDuplicatedDataMessageDisplayed()
        {
            try
            {
                var duplicatedDataMessage = By.XPath(
                    "//*[normalize-space()='Duplicated data']"
                );

                return _wait
                    .Until(ExpectedConditions.ElementIsVisible(duplicatedDataMessage))
                    .Displayed;
            }
            catch (WebDriverTimeoutException)
            {
                return false;
            }
        }

        public bool IsAddNewButtonAvailable()
        {
            try
            {
                return _wait
                    .Until(ExpectedConditions.ElementToBeClickable(AddNewButton))
                    .Displayed;
            }
            catch (WebDriverTimeoutException)
            {
                return false;
            }
        }

        public bool IsLanguageAlreadyAddedMessageDisplayed()
        {
            try
            {
                var message = By.XPath(
                    "//*[normalize-space()='This language is already added to your language list.']"
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

        public void ClearLanguageField()
        {
            var languageInput = _wait.Until(
                ExpectedConditions.ElementIsVisible(LanguageField)
            );

            languageInput.Click();
            languageInput.SendKeys(Keys.Control + "a");
            languageInput.SendKeys(Keys.Backspace);

            _wait.Until(driver =>
                string.IsNullOrEmpty(languageInput.GetAttribute("value"))
            );
        }

        public string GetLanguageFieldValue()
        {
            var languageInput = _wait.Until(
                ExpectedConditions.ElementIsVisible(LanguageField)
            );

            return languageInput.GetAttribute("value") ?? string.Empty;
        }

        public void SelectEmptyLanguageLevel()
        {
            var dropdownElement = _wait.Until(
                ExpectedConditions.ElementIsVisible(LanguageLevelDropdown)
            );

            var selectElement = new SelectElement(dropdownElement);

            selectElement.SelectByIndex(0);
        }

        public void EditLanguageWithEmptyLevel(string currentLanguage)
        {
            ClickEditLanguage(currentLanguage);
            SelectEmptyLanguageLevel();
            ClickUpdateButton();
        }

        /* Boundary test */

        public bool WaitForLanguageRowCount(int expectedCount)
        {
            try
            {
                return _wait.Until(driver =>
                    GetLanguageRowCount() == expectedCount
                );
            }
            catch (WebDriverTimeoutException)
            {
                return false;
            }
        }

        public bool IsAddNewButtonDisplay()
        {
            var buttons = _driver.FindElements(AddNewButton);

            return buttons.Any(button =>
                button.Displayed && button.Enabled
            );
        }

    }
}