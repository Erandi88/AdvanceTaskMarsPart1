using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

namespace MarsAdvanced.Tests.Pages
{
    public class SearchSkillPage
    {
        private readonly IWebDriver _driver;
        private readonly WebDriverWait _wait;

        private readonly By SearchInput = By.CssSelector(
            ".search-box input[placeholder='Search skills']"
        );

        private readonly By ResultTitles = By.CssSelector(
            "div.ui.card a.service-info p"
        );

        private readonly By NoResultsMessage = By.XPath(
            "//h3[normalize-space()='No results found, please select a new category!']"
        );


        public SearchSkillPage(IWebDriver driver)
        {
            _driver = driver;
            _wait = new WebDriverWait(
                driver,
                TimeSpan.FromSeconds(30)
            );
        }

        public void SearchUsingEnter(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                throw new ArgumentException(
                    "A search keyword is required.",
                    nameof(keyword)
                );
            }

            Console.WriteLine($"Search keyword: [{keyword}]");
            Console.WriteLine($"URL before searching: {_driver.Url}");

            var input = _wait.Until(
                ExpectedConditions.ElementToBeClickable(SearchInput)
            );

            input.Click();
            input.SendKeys(Keys.Control + "a");
            input.SendKeys(Keys.Backspace);
            input.SendKeys(keyword);

            Console.WriteLine(
                $"Search input contains: [{input.GetDomProperty("value")}]"
            );

            input.SendKeys(Keys.Enter);

            _wait.Until(driver =>
                new Uri(driver.Url).AbsolutePath.Equals(
                    "/Home/Search",
                    StringComparison.OrdinalIgnoreCase
                )
            );

            Console.WriteLine($"URL after navigation: {_driver.Url}");
        }

        public string GetSubmittedKeyword()
        {
            Console.WriteLine($"Current browser URL: {_driver.Url}");

            // Step 1: extract the query and remove the leading question mark.
            string query = new Uri(_driver.Url).Query.TrimStart('?');

            Console.WriteLine($"Query after removing '?': [{query}]");

            // Step 2: examine each parameter separated by &.
            foreach (string parameter in query.Split(
                '&',
                StringSplitOptions.RemoveEmptyEntries))
            {
                Console.WriteLine($"Examining parameter: [{parameter}]");

                // Step 3: separate the parameter name from its value.
                string[] parts = parameter.Split('=', 2);

                string name = Uri.UnescapeDataString(parts[0]);

                Console.WriteLine($"Parameter name: [{name}]");

                // Step 4: find the searchString parameter.
                if (name.Equals(
                    "searchString",
                    StringComparison.OrdinalIgnoreCase))
                {
                    string encodedValue = parts.Length == 2
                        ? parts[1]
                        : string.Empty;

                    Console.WriteLine(
                        $"Encoded search value: [{encodedValue}]"
                    );

                    // Step 5: convert URL encoding into readable text.
                    string keyword = Uri.UnescapeDataString(
                        encodedValue.Replace("+", " ")
                    );

                    Console.WriteLine(
                        $"Decoded keyword returned to the test: [{keyword}]"
                    );

                    return keyword;
                }
            }

            Console.WriteLine(
                "No searchString parameter was found. Returning an empty string."
            );

            return string.Empty;
        }

        public bool WaitForListingTitle(string expectedTitle)
        {
            try
            {
                // Re-find result elements while search results load.
                return _wait.Until(driver =>
                {
                    try
                    {
                        return driver.FindElements(ResultTitles)
                            .Any(element =>
                                element.Displayed &&
                                element.Text.Trim() == expectedTitle
                            );
                    }
                    catch (StaleElementReferenceException)
                    {
                        return false;
                    }
                });
            }
            catch (WebDriverTimeoutException)
            {
                return false;
            }
        }

        public List<string> GetVisibleResultTitles()
        {
            return _driver.FindElements(ResultTitles)
                .Where(element => element.Displayed)
                .Select(element => element.Text.Trim())
                .ToList();
        }

        public bool WaitForNoResults()
        {
            try
            {
                // Wait for the message and the absence of visible result cards.
                return _wait.Until(driver =>
                {
                    try
                    {
                        bool messageVisible = driver
                            .FindElements(NoResultsMessage)
                            .Any(element => element.Displayed);

                        bool resultCardsVisible = driver
                            .FindElements(By.CssSelector("div.ui.card"))
                            .Any(element => element.Displayed);

                        return messageVisible && !resultCardsVisible;
                    }
                    catch (StaleElementReferenceException)
                    {
                        return false;
                    }
                });
            }
            catch (WebDriverTimeoutException)
            {
                return false;
            }
        }

        public void SelectTestAutomationCategory()
        {
            var category = By.XPath(
            "//a[@role='listitem' and " +
            "contains(concat(' ', normalize-space(@class), ' '), ' category ')]" +
            "[.//text()[normalize-space()='Test Automation']]"
        );

            _wait.Until(
                ExpectedConditions.ElementToBeClickable(category)
            ).Click();

            // Verify that clicking the category submitted its filter.
            _wait.Until(driver =>
                GetQueryParameter("cat") == "TestAutomation"
            );

            Console.WriteLine($"Category selected. URL: {_driver.Url}");
        }

        public void SelectSeleniumSubcategory()
        {
            var subcategory = By.XPath(
                "//a[@role='listitem' and " +
                "contains(concat(' ', normalize-space(@class), ' '), ' subcategory ')]" +
                "[.//text()[normalize-space()='Selenium']]"
            );

            _wait.Until(
                ExpectedConditions.ElementToBeClickable(subcategory)
            ).Click();

            // Both filters should remain in the URL.
            _wait.Until(driver =>
                GetQueryParameter("cat") == "TestAutomation" &&
                GetQueryParameter("subcat") == "1"
            );

            Console.WriteLine($"Subcategory selected. URL: {_driver.Url}");
        }

        public string GetQueryParameter(string parameterName)
        {
            Console.WriteLine("Parameter Name  : "+ parameterName);
            string query = new Uri(_driver.Url).Query.TrimStart('?');

            foreach (string parameter in query.Split(
                '&',
                StringSplitOptions.RemoveEmptyEntries))
            {
                string[] parts = parameter.Split('=', 2);

                string name = Uri.UnescapeDataString(
                    parts[0].Replace("+", " ")
                );

                Console.WriteLine("Name  : " + name);

                if (name.Equals(
                    parameterName,
                    StringComparison.OrdinalIgnoreCase))
                {
                    return parts.Length == 2
                        ? Uri.UnescapeDataString(parts[1].Replace("+", " "))
                        : string.Empty;
                }
            }

            return string.Empty;
        }

        public bool WaitForResultTitles(params string[] expectedTitles)
        {
            try
            {
                return _wait.Until(driver =>
                {
                    try
                    {
                        var actualTitles = GetVisibleResultTitles();

                        // Compare all visible titles, including duplicate counts.
                        return actualTitles
                            .OrderBy(title => title, StringComparer.Ordinal)
                            .SequenceEqual(
                                expectedTitles.OrderBy(
                                    title => title,
                                    StringComparer.Ordinal
                                ),
                                StringComparer.Ordinal
                            );
                    }
                    catch (StaleElementReferenceException)
                    {
                        return false;
                    }
                });
            }
            catch (WebDriverTimeoutException)
            {
                Console.WriteLine(
                    "Expected result titles were not displayed before timeout."
                );

                return false;
            }
        }
    }


}