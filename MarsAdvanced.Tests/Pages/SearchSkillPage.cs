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

        public void SelectLocationFilter(string filter)
        {
            if (filter != "Online" &&
                filter != "Onsite" &&
                filter != "ShowAll")
            {
                throw new ArgumentException(
                    $"Unsupported location filter: {filter}",
                    nameof(filter)
                );
            }

            var buttonLocator = By.XPath(
                "//div[contains(concat(' ', normalize-space(@class), ' '), " +
                "' buttons ')]/button[normalize-space()='" + filter + "']"
            );

            _wait.Until(
                ExpectedConditions.ElementToBeClickable(buttonLocator)
            ).Click();

            Console.WriteLine($"Location filter clicked: [{filter}]");
        }

        public string OpenListingByTitle(string title)
        {
            var locator = By.XPath(
                "//div[contains(concat(' ', normalize-space(@class), ' '), ' card ')]" +
                "//a[contains(concat(' ', normalize-space(@class), ' '), ' service-info ')]" +
                $"[normalize-space()={XPathLiteral(title)}]"
            );

            var link = _wait.Until(
                ExpectedConditions.ElementToBeClickable(locator)
            );

            string destination = link.GetAttribute("href")
                ?? throw new InvalidOperationException(
                    "The search result has no destination URL."
                );

            link.Click();

            return destination;
        }

        private static string XPathLiteral(string value)
        {
            if (!value.Contains("'"))
            {
                return $"'{value}'";
            }

            if (!value.Contains("\""))
            {
                return $"\"{value}\"";
            }

            return "concat(" +
                string.Join(
                    ", \"'\", ",
                    value.Split('\'').Select(part => $"'{part}'")
                ) +
                ")";
        }

        public void SelectCategory(string category)
        {
            SelectSearchFilter("category", category);
        }

        public void SelectSubcategory(string subcategory)
        {
            SelectSearchFilter("subcategory", subcategory);
        }

        private void SelectSearchFilter(string filterClass, string text)
        {
            var locator = By.XPath(
                "//a[@role='listitem' and " +
                "contains(concat(' ', normalize-space(@class), ' '), " +
                $"' {filterClass} ')]" +
                $"[.//text()[normalize-space()={XPathLiteral(text)}]]"
            );

            _wait.Until(
                ExpectedConditions.ElementToBeClickable(locator)
            ).Click();

            _wait.Until(driver =>
            {
                try
                {
                    var element = driver.FindElement(locator);
                    string classes = element.GetAttribute("class") ?? string.Empty;

                    return element.Displayed &&
                        classes.Split(
                            ' ',
                            StringSplitOptions.RemoveEmptyEntries
                        ).Contains("active");
                }
                catch (StaleElementReferenceException)
                {
                    return false;
                }
            });
        }

        //Pagination
        public void SelectResultsPerPage(int pageSize)
        {
            if (pageSize != 9 && pageSize != 12 && pageSize != 18)
            {
                throw new ArgumentException(
                    "Page size must be 9, 12, or 18.",
                    nameof(pageSize)
                );
            }

            var locator = By.XPath(
                "//div[text()[contains(., 'Results Per Page')]]" +
                $"/button[normalize-space()='{pageSize}']"
            );

            _wait.Until(
                ExpectedConditions.ElementToBeClickable(locator)
            ).Click();
        }

        public int GetCurrentResultsPage()
        {
            var locator = By.CssSelector(
                ".semantic-ui-react-button-pagination button.currentPage"
            );

            var button = _wait.Until(
                ExpectedConditions.ElementIsVisible(locator)
            );

            return int.Parse(button.Text.Trim());
        }

        public void GoToNextResultsPage()
        {
            ChangeResultsPage(">");
        }

        public void GoToPreviousResultsPage()
        {
            ChangeResultsPage("<");
        }

        private void ChangeResultsPage(string direction)
        {
            int previousPage = GetCurrentResultsPage();

            var locator = By.XPath(
                "//div[contains(concat(' ', normalize-space(@class), ' ')," +
                " ' semantic-ui-react-button-pagination ')]" +
                $"/button[normalize-space()='{direction}']"
            );

            _wait.Until(
                ExpectedConditions.ElementToBeClickable(locator)
            ).Click();

            _wait.Until(_ =>
            {
                try
                {
                    return GetCurrentResultsPage() != previousPage;
                }
                catch (StaleElementReferenceException)
                {
                    return false;
                }
            });
        }

        public bool IsNextResultsPageDisabled()
        {
            var locator = By.XPath(
                "//div[contains(concat(' ', normalize-space(@class), ' ')," +
                " ' semantic-ui-react-button-pagination ')]" +
                "/button[normalize-space()='>']"
            );

            var button = _wait.Until(
                ExpectedConditions.ElementIsVisible(locator)
            );

            return !button.Enabled;
        }

        public List<string> WaitForVisibleResultCount(int expectedCount)
        {
            return _wait.Until(_ =>
            {
                try
                {
                    var titles = GetVisibleResultTitles();
                    return titles.Count == expectedCount ? titles : null;
                }
                catch (StaleElementReferenceException)
                {
                    return null;
                }
            })!;
        }

        public void SearchUsingIcon(string keyword)
        {
            Console.WriteLine("KEYWORD : " + keyword);
            if (string.IsNullOrWhiteSpace(keyword))
            {
                throw new ArgumentException(
                    "A search keyword is required.",
                    nameof(keyword)
                );
            }

            var input = _wait.Until(
                ExpectedConditions.ElementToBeClickable(SearchInput)
            );

            input.Click();
            input.SendKeys(Keys.Control + "a");
            input.SendKeys(Keys.Backspace);
            input.SendKeys(keyword);

            Console.WriteLine(
                $"Search input before clicking icon: [{input.GetDomProperty("value")}]"
            );

            var icon = By.CssSelector(".search-box i.search.link.icon");
            _wait.Until(
                ExpectedConditions.ElementToBeClickable(icon)
            ).Click();

            _wait.Until(driver =>
                new Uri(driver.Url).AbsolutePath.Equals(
                    "/Home/Search",
                    StringComparison.OrdinalIgnoreCase
                )
            );

            Console.WriteLine($"URL after clicking search icon: {_driver.Url}");
        }

        //open the search without search keyword
        public void OpenSearchWithoutKeyword()
        {
            string url = new Uri(new Uri(_driver.Url),
                "/Home/Search?searchString=" ).AbsoluteUri;

            _driver.Navigate().GoToUrl(url);

            _wait.Until(driver =>
            {
                try
                {
                    return driver.FindElements(ResultTitles)
                               .Any(element => element.Displayed) ||
                           driver.FindElements(NoResultsMessage)
                               .Any(element => element.Displayed);
                }
                catch (StaleElementReferenceException)
                {
                    return false;
                }
            });
        }

        //Select All Categories and wait until it becomes active
        public void SelectAllCategories()
        {
            var locator = By.XPath(
                "//a[@role='listitem']" +
                "[.//text()[normalize-space()='All Categories']]"
            );

            _wait.Until(
                ExpectedConditions.ElementToBeClickable(locator)
            ).Click();

            _wait.Until(driver =>
            {
                try
                {
                    string classes = driver.FindElement(locator)
                        .GetAttribute("class") ?? string.Empty;

                    return classes.Split(
                        ' ',
                        StringSplitOptions.RemoveEmptyEntries
                    ).Contains("active");
                }
                catch (StaleElementReferenceException)
                {
                    return false;
                }
            });
        }

        //Read the displayed total
        public int GetAllCategoriesCount()
        {
            var locator = By.XPath(
                "//a[@role='listitem']" +
                "[.//text()[normalize-space()='All Categories']]/span"
            );

            var count = _wait.Until(
                ExpectedConditions.ElementIsVisible(locator)
            );

            return int.Parse(count.Text.Trim());
        }

        public bool IsCategorySelected(string category)
        {
            return IsSearchFilterSelected("category", category);
        }

        public bool IsSubcategorySelected(string subcategory)
        {
            return IsSearchFilterSelected("subcategory", subcategory);
        }

        private bool IsSearchFilterSelected(string filterClass, string text)
        {
            var locator = By.XPath(
                "//a[@role='listitem' and " +
                "contains(concat(' ', normalize-space(@class), ' '), " +
                $"' {filterClass} ')]" +
                $"[.//text()[normalize-space()={XPathLiteral(text)}]]"
            );

            try
            {
                return _wait.Until(driver =>
                {
                    try
                    {
                        return driver.FindElements(locator).Any(element =>
                            element.Displayed &&
                            (element.GetAttribute("class") ?? string.Empty)
                                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                                .Contains("active")
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
    }

}