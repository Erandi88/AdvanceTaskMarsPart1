using MarsAdvanced.Tests.Helpers;
using MarsAdvanced.Tests.Models;
using MarsAdvanced.Tests.Pages;
using NUnit.Framework;
using OpenQA.Selenium;

// Add the same model/helper using statements as ShareSkillTests.cs.

namespace MarsAdvanced.Tests
{
    [TestFixture]
    [NonParallelizable]
    public class SearchSkillTests
    {
        private IWebDriver? driver;

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
                "Login should succeed before testing search."
            );
        }

        [Test]
        public void SearchSkillByTitleUsingEnter()
        {
            var browser = driver
                ?? throw new InvalidOperationException(
                    "Browser was not created."
                );

            string filePath = Path.Combine(
                TestContext.CurrentContext.TestDirectory,
                "TestData",
                "SearchSkill",
                "SearchSkillByTitleUsingEnter.json"
            );

            var testData = JsonReader.Read<ShareSkillData>(filePath);

            Assert.That(testData.Title, Is.Not.Null.And.Not.Empty);
            Assert.That(
                testData.Description,
                Is.Not.Null.And.Not.Empty
            );
            Assert.That(testData.ActiveStatus, Is.EqualTo("Active"));

            string suffix = Guid.NewGuid().ToString("N")[..8];
            string title = $"{testData.Title} {suffix}";

            Assert.That(title.Length, Is.LessThanOrEqualTo(100));
            Assert.That(
                testData.Description.Length,
                Is.LessThanOrEqualTo(600)
            );

            var form = new ShareSkillPage(browser);
            var listings = new ManageListingsPage(browser);
            var searchPage = new SearchSkillPage(browser);

            listings.Open();

            Assert.That(
                listings.FindListingByTitle(title),
                Is.False,
                "The generated title must not already exist."
            );

            bool creationAttempted = false;

            try
            {
                // Arrange: create a unique active listing to search for.
                form.OpenShareSkill();
                form.FillForm(testData, title);

                creationAttempted = true;
                form.ClickSave();
                form.WaitForSaveToComplete();

                listings.Open();

                Assert.That(
                    listings.FindListingByTitle(title),
                    Is.True,
                    "The listing must exist before searching."
                );

                Console.WriteLine($"Created test listing: [{title}]");

                // Act: search from Manage Listings using Enter.
                searchPage.SearchUsingEnter(title);

                // Verify the application submitted the actual keyword.
                Assert.That(
                    searchPage.GetSubmittedKeyword(),
                    Is.EqualTo(title),
                    "The URL should contain the entered search keyword."
                );

                Assert.That(
                    searchPage.WaitForListingTitle(title),
                    Is.True,
                    "The created listing should appear in search results."
                );

                var resultTitles = searchPage.GetVisibleResultTitles();

                Console.WriteLine(
                    $"Visible result count: {resultTitles.Count}"
                );

                foreach (string resultTitle in resultTitles)
                {
                    Console.WriteLine(
                        $"Visible result title: [{resultTitle}]"
                    );
                }

                Assert.That(
                    resultTitles,
                    Is.EquivalentTo(new[] { title }),
                    "Searching the unique full title should show only that listing."
                );
            }
            finally
            {
                // Remove only the listing created by this test.
                if (creationAttempted)
                {
                    listings.Open();
                    listings.DeleteListingIfExists(title);
                }
            }
        }

        [Test]
        public void SearchSkillWithNoMatchingTitle()
        {
            var browser = driver
                ?? throw new InvalidOperationException(
                    "Browser was not created."
                );

            string filePath = Path.Combine(
                TestContext.CurrentContext.TestDirectory,
                "TestData",
                "SearchSkill",
                "SearchSkillWithNoMatchingTitle.json"
            );

            var testData = JsonReader.Read<SearchSkillData>(filePath);

            Assert.That(
                testData.Keyword,
                Is.Not.Null.And.Not.Empty
            );

            // Make an accidental match with existing listings unlikely.
            string keyword =
                $"{testData.Keyword}{Guid.NewGuid():N}";

            var listings = new ManageListingsPage(browser);
            var searchPage = new SearchSkillPage(browser);

            // Start outside the search page.
            listings.Open();

            Console.WriteLine($"Unmatched search keyword: [{keyword}]");

            searchPage.SearchUsingEnter(keyword);

            Assert.That(
                searchPage.GetSubmittedKeyword(),
                Is.EqualTo(keyword),
                "The URL should contain the submitted keyword."
            );

            Assert.That(
                searchPage.WaitForNoResults(),
                Is.True,
                "An unmatched keyword should show the empty-results message without result cards."
            );

            Assert.That(
                searchPage.GetVisibleResultTitles(),
                Is.Empty,
                "No listing titles should appear for the unmatched keyword."
            );
        }


        [Test]
        public void FilterSearchResultsBySeleniumSubcategory()
        {
            var browser = driver
                ?? throw new InvalidOperationException(
                    "Browser was not created."
                );

            string filePath = Path.Combine(
                TestContext.CurrentContext.TestDirectory,
                "TestData",
                "SearchSkill",
                "FilterSearchResultsBySeleniumSubcategory.json"
            );

            var testData = JsonReader.Read<SearchSkillFilterData>(filePath);

            Assert.That(testData.Keyword, Is.Not.Null.And.Not.Empty);
            Assert.That(testData.MatchingListing, Is.Not.Null);
            Assert.That(testData.NonMatchingListing, Is.Not.Null);

            Assert.Multiple(() =>
            {
                Assert.That(
                    testData.MatchingListing.Category,
                    Is.EqualTo("Test Automation")
                );
                Assert.That(
                    testData.NonMatchingListing.Category,
                    Is.EqualTo("Test Automation")
                );
                Assert.That(
                    testData.MatchingListing.Subcategory,
                    Is.EqualTo("Selenium")
                );
                Assert.That(
                    testData.NonMatchingListing.Subcategory,
                    Is.EqualTo("API Testing")
                );
                Assert.That(
                    testData.MatchingListing.ActiveStatus,
                    Is.EqualTo("Active")
                );
                Assert.That(
                    testData.NonMatchingListing.ActiveStatus,
                    Is.EqualTo("Active")
                );
            });

            // Both titles share one unique search keyword.
            string suffix = Guid.NewGuid().ToString("N")[..8];
            string keyword = $"{testData.Keyword} {suffix}";

            string matchingTitle =
                $"{keyword} {testData.MatchingListing.Title}";

            string nonMatchingTitle =
                $"{keyword} {testData.NonMatchingListing.Title}";

            Assert.Multiple(() =>
            {
                Assert.That(matchingTitle.Length, Is.LessThanOrEqualTo(100));
                Assert.That(nonMatchingTitle.Length, Is.LessThanOrEqualTo(100));
                Assert.That(nonMatchingTitle, Is.Not.EqualTo(matchingTitle));
            });

            var form = new ShareSkillPage(browser);
            var listings = new ManageListingsPage(browser);
            var searchPage = new SearchSkillPage(browser);

            listings.Open();

            Assert.That(
                listings.FindListingByTitle(matchingTitle),
                Is.False
            );
            Assert.That(
                listings.FindListingByTitle(nonMatchingTitle),
                Is.False
            );

            bool matchingCreationAttempted = false;
            bool nonMatchingCreationAttempted = false;

            try
            {
                // Arrange: create the Selenium listing.
                form.OpenShareSkill();
                form.FillForm(testData.MatchingListing, matchingTitle);

                matchingCreationAttempted = true;
                form.ClickSave();
                form.WaitForSaveToComplete();

                listings.Open();

                Assert.That(
                    listings.FindListingByTitle(matchingTitle),
                    Is.True,
                    "The Selenium listing must exist before searching."
                );

                // Create the listing that Selenium filtering should exclude.
                form.OpenShareSkill();
                form.FillForm(testData.NonMatchingListing, nonMatchingTitle);

                nonMatchingCreationAttempted = true;
                form.ClickSave();
                form.WaitForSaveToComplete();

                listings.Open();

                Assert.That(
                    listings.FindListingByTitle(nonMatchingTitle),
                    Is.True,
                    "The API Testing listing must exist before searching."
                );

                // Verify both listings appear before applying filters.
                searchPage.SearchUsingEnter(keyword);

                Assert.That(
                    searchPage.GetSubmittedKeyword(),
                    Is.EqualTo(keyword)
                );

                Assert.That(
                    searchPage.WaitForResultTitles(
                        matchingTitle,
                        nonMatchingTitle
                    ),
                    Is.True,
                    "Both test listings should appear before filtering."
                );

                searchPage.SelectTestAutomationCategory();

                Assert.That(
                    searchPage.WaitForResultTitles(
                        matchingTitle,
                        nonMatchingTitle
                    ),
                    Is.True,
                    "Both listings belong to Test Automation and should remain."
                );

                // Act: narrow the results to Selenium.
                searchPage.SelectSeleniumSubcategory();

                Assert.That(
                    searchPage.WaitForResultTitles(matchingTitle),
                    Is.True,
                    "Selenium filtering should retain only the Selenium listing."
                );

                Assert.Multiple(() =>
                {
                    Assert.That(
                        searchPage.GetQueryParameter("cat"),
                        Is.EqualTo("TestAutomation")
                    );

                    Assert.That(
                        searchPage.GetQueryParameter("subcat"),
                        Is.EqualTo("1")
                    );

                    Assert.That(
                        searchPage.GetVisibleResultTitles(),
                        Is.EquivalentTo(new[] { matchingTitle }),
                        "The API Testing listing should be excluded."
                    );
                });

                Console.WriteLine($"Included: [{matchingTitle}]");
                Console.WriteLine($"Excluded: [{nonMatchingTitle}]");
            }
            finally
            {
                // Attempt both cleanups even if the first cleanup fails.
                try
                {
                    if (matchingCreationAttempted)
                    {
                        listings.Open();
                        listings.DeleteListingIfExists(matchingTitle);
                    }
                }
                finally
                {
                    if (nonMatchingCreationAttempted)
                    {
                        listings.Open();
                        listings.DeleteListingIfExists(nonMatchingTitle);
                    }
                }
            }
        }

        [TearDown]
        public void TearDown()
        {
            driver?.Dispose();
            driver = null;
        }
    }
}