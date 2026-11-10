using MarsAdvanced.Tests.Helpers;
using MarsAdvanced.Tests.Models;
using MarsAdvanced.Tests.Pages;
using NUnit.Framework;
using OpenQA.Selenium;
using AventStack.ExtentReports;
using NUnit.Framework.Interfaces;

// Add the same model/helper using statements as ShareSkillTests.cs.

namespace MarsAdvanced.Tests
{
    [TestFixture]
    [NonParallelizable]
    public class SearchSkillTests
    {
        private IWebDriver? driver;
        private ExtentTest? reportTest;

        [SetUp]
        public void SetUp()
        {
            // Create the report entry before browser setup, so setup failures are recorded.
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


        [TestCase(
            "FilterSearchResultsByCategory.json",
            false,
            TestName = "FilterSearchResultsByCategory"
        )]

        [TestCase(
            "FilterSearchResultsBySeleniumSubcategory.json",
            true,
            TestName = "FilterSearchResultsBySeleniumSubcategory"
        )]
        public void FilterSearchResults(string jsonFileName, bool filterBySubcategory)
        {
            var browser = driver
                ?? throw new InvalidOperationException(
                    "Browser was not created."
                );

            string filePath = Path.Combine(
                TestContext.CurrentContext.TestDirectory,
                "TestData",
                "SearchSkill",
                jsonFileName
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
                    testData.MatchingListing.Subcategory,
                    Is.EqualTo("Selenium")
                );

                Assert.That(
                    testData.NonMatchingListing.Category,
                    Is.EqualTo(
                        filterBySubcategory
                            ? "Test Automation"
                            : "Software Development"
                    )
                );

                Assert.That(
                    testData.NonMatchingListing.Subcategory,
                    Is.EqualTo(
                        filterBySubcategory
                            ? "API Testing"
                            : "Programming"
                    )
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

            // Both listings share a unique search keyword.
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
                // Arrange: create both listings using the same helper.
                form.OpenShareSkill();
                form.FillForm(testData.MatchingListing, matchingTitle);

                matchingCreationAttempted = true;
                SaveAndVerifyListing(form, listings, matchingTitle);

                form.OpenShareSkill();
                form.FillForm(testData.NonMatchingListing, nonMatchingTitle);

                nonMatchingCreationAttempted = true;
                SaveAndVerifyListing(form, listings, nonMatchingTitle);

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
                    "Both listings should appear before filtering."
                );

                // Both scenarios begin by selecting Test Automation.
                searchPage.SelectTestAutomationCategory();

                if (filterBySubcategory)
                {
                    // Both belong to this category, so both should remain.
                    Assert.That(
                        searchPage.WaitForResultTitles(
                            matchingTitle,
                            nonMatchingTitle
                        ),
                        Is.True,
                        "Both Test Automation listings should remain."
                    );

                    searchPage.SelectSeleniumSubcategory();
                }

                // Either filter should leave only the matching listing.
                Assert.That(
                    searchPage.WaitForResultTitles(matchingTitle),
                    Is.True,
                    "Filtering should retain only the matching listing."
                );

                Assert.Multiple(() =>
                {
                    Assert.That(
                        searchPage.GetQueryParameter("cat"),
                        Is.EqualTo("TestAutomation")
                    );

                    Assert.That(
                        searchPage.GetQueryParameter("subcat"),
                        Is.EqualTo(filterBySubcategory ? "1" : string.Empty)
                    );

                    Assert.That(
                        searchPage.GetVisibleResultTitles(),
                        Is.EquivalentTo(new[] { matchingTitle }),
                        "The nonmatching listing should be excluded."
                    );
                });

                Console.WriteLine($"Scenario data: {jsonFileName}");
                Console.WriteLine($"Included: [{matchingTitle}]");
                Console.WriteLine($"Excluded: [{nonMatchingTitle}]");
            }
            finally
            {
                // Attempt both cleanups even if the first one fails.
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

        //Helper
        private void SaveAndVerifyListing(ShareSkillPage form,ManageListingsPage listings, string title)
        {
            // Save the completed form and verify the listing exists.
            form.ClickSave();
            form.WaitForSaveToComplete();

            listings.Open();

            Assert.That(
                listings.FindListingByTitle(title),
                Is.True,
                $"The test listing must exist before searching: {title}"
            );
        }

        [Test]
        public void FilterSearchResultsByLocation()
        {
            var browser = driver
                ?? throw new InvalidOperationException(
                    "Browser was not created."
                );

            string filePath = Path.Combine(
                TestContext.CurrentContext.TestDirectory,
                "TestData",
                "SearchSkill",
                "FilterSearchResultsByLocation.json"
            );

            var testData = JsonReader.Read<SearchSkillFilterData>(filePath);

            Assert.That(testData.Keyword, Is.Not.Null.And.Not.Empty);
            Assert.That(testData.MatchingListing, Is.Not.Null);
            Assert.That(testData.NonMatchingListing, Is.Not.Null);

            var onlineData = testData.MatchingListing;
            var onsiteData = testData.NonMatchingListing;

            Assert.Multiple(() =>
            {
                Assert.That(onlineData.LocationType, Is.EqualTo("Online"));
                Assert.That(onsiteData.LocationType, Is.EqualTo("On-site"));

                Assert.That(onlineData.ActiveStatus, Is.EqualTo("Active"));
                Assert.That(onsiteData.ActiveStatus, Is.EqualTo("Active"));

                Assert.That(
                    onlineData.Category,
                    Is.EqualTo(onsiteData.Category)
                );
                Assert.That(
                    onlineData.Subcategory,
                    Is.EqualTo(onsiteData.Subcategory)
                );
            });

            // Use one unique keyword to find both test listings.
            string suffix = Guid.NewGuid().ToString("N")[..8];
            string keyword = $"{testData.Keyword} {suffix}";

            string onlineTitle = $"{keyword} {onlineData.Title}";
            string onsiteTitle = $"{keyword} {onsiteData.Title}";

            Assert.Multiple(() =>
            {
                Assert.That(onlineTitle.Length, Is.LessThanOrEqualTo(100));
                Assert.That(onsiteTitle.Length, Is.LessThanOrEqualTo(100));
                Assert.That(onsiteTitle, Is.Not.EqualTo(onlineTitle));
            });

            var form = new ShareSkillPage(browser);
            var listings = new ManageListingsPage(browser);
            var searchPage = new SearchSkillPage(browser);

            listings.Open();

            Assert.That(listings.FindListingByTitle(onlineTitle), Is.False);
            Assert.That(listings.FindListingByTitle(onsiteTitle), Is.False);

            bool onlineCreationAttempted = false;
            bool onsiteCreationAttempted = false;

            try
            {
                // Arrange: create one listing for each location.
                form.OpenShareSkill();
                form.FillForm(onlineData, onlineTitle);

                onlineCreationAttempted = true;
                SaveAndVerifyListing(form, listings, onlineTitle);

                form.OpenShareSkill();
                form.FillForm(onsiteData, onsiteTitle);

                onsiteCreationAttempted = true;
                SaveAndVerifyListing(form, listings, onsiteTitle);

                searchPage.SearchUsingEnter(keyword);

                Assert.That(
                    searchPage.GetSubmittedKeyword(),
                    Is.EqualTo(keyword)
                );

                Assert.That(
                    searchPage.WaitForResultTitles(onlineTitle, onsiteTitle),
                    Is.True,
                    "Both listings should appear before location filtering."
                );

                // Online should exclude the On-site listing.
                searchPage.SelectLocationFilter("Online");

                Assert.That(
                    searchPage.WaitForResultTitles(onlineTitle),
                    Is.True,
                    "Online should show only the Online test listing."
                );

                Assert.That(
                    searchPage.GetVisibleResultTitles(),
                    Is.EquivalentTo(new[] { onlineTitle })
                );

                Console.WriteLine($"Online filter retained: [{onlineTitle}]");

                // Onsite should replace the Online result.
                searchPage.SelectLocationFilter("Onsite");

                Assert.That(
                    searchPage.WaitForResultTitles(onsiteTitle),
                    Is.True,
                    "Onsite should show only the On-site test listing."
                );

                Assert.That(
                    searchPage.GetVisibleResultTitles(),
                    Is.EquivalentTo(new[] { onsiteTitle })
                );

                Console.WriteLine($"Onsite filter retained: [{onsiteTitle}]");

                // ShowAll should restore both locations within the search.
                searchPage.SelectLocationFilter("ShowAll");

                Assert.That(
                    searchPage.WaitForResultTitles(onlineTitle, onsiteTitle),
                    Is.True,
                    "ShowAll should restore both test listings."
                );

                Assert.That(
                    searchPage.GetVisibleResultTitles(),
                    Is.EquivalentTo(new[] { onlineTitle, onsiteTitle })
                );

                Console.WriteLine("ShowAll restored both test listings.");
            }
            finally
            {
                // Attempt cleanup of both listings.
                try
                {
                    if (onlineCreationAttempted)
                    {
                        listings.Open();
                        listings.DeleteListingIfExists(onlineTitle);
                    }
                }
                finally
                {
                    if (onsiteCreationAttempted)
                    {
                        listings.Open();
                        listings.DeleteListingIfExists(onsiteTitle);
                    }
                }
            }
        }

        [TearDown]
        public void TearDown()
        {
            try
            {
                // Record the result before closing the browser.
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
                    driver = null;
                    reportTest = null;

                    ExtentReportManager.Flush();
                }
            }
        }
    }
}