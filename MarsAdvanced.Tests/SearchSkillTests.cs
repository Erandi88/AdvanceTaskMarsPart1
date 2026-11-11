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

        [Test]
        public void OpenSearchResultShowsCorrectListing()
        {
            var browser = driver
                ?? throw new InvalidOperationException("Browser was not created.");

            string filePath = Path.Combine(
                TestContext.CurrentContext.TestDirectory,
                "TestData", "SearchSkill",
                "OpenSearchResultShowsCorrectListing.json"
            );
            var data = JsonReader.Read<ShareSkillData>(filePath);
            string title = $"{data.Title} {Guid.NewGuid():N}";
            var form = new ShareSkillPage(browser);
            var listings = new ManageListingsPage(browser);
            var search = new SearchSkillPage(browser);
            var detail = new ServiceDetailPage(browser);
            bool saveAttempted = false;

            try
            {
                form.OpenShareSkill();
                form.FillForm(data, title);
                saveAttempted = true;
                SaveAndVerifyListing(form, listings, title);

                search.SearchUsingEnter(title);
                Assert.That(search.WaitForListingTitle(title), Is.True);
                string expectedUrl = search.OpenListingByTitle(title);

                Assert.That(detail.WaitForTitle(), Is.EqualTo(title),
                    "The detail page should display the selected listing title.");
                Assert.That(detail.GetUrl(), Is.EqualTo(expectedUrl),
                    "The browser should open the selected result's destination.");

                Assert.Multiple(() =>
                {
                    Assert.That(
                        detail.GetCategory(),
                        Is.EqualTo(data.Category),
                        "The detail category should match the created listing."
                    );

                    Assert.That(
                        detail.GetSubcategory(),
                        Is.EqualTo(data.Subcategory),
                        "The detail subcategory should match the created listing."
                    );

                    Assert.That(
                        detail.GetLocationType(),
                        Is.EqualTo(data.LocationType),
                        "The detail location should match the created listing."
                    );
                });
            }
            finally
            {
                if (saveAttempted)
                {
                    listings.Open();
                    listings.DeleteListingIfExists(title);
                }
            }
        }

        [Test]
        [NonParallelizable]
        public void FilterSearchResultsByCombinedFilters()
        {
            var browser = driver
                ?? throw new InvalidOperationException("Browser was not created.");

            string filePath = Path.Combine(
                TestContext.CurrentContext.TestDirectory,
                "TestData", "SearchSkill",
                "FilterSearchResultsByCombinedFilters.json"
            );

            var data = JsonReader.Read<List<ShareSkillData>>(filePath);

            Assert.That(data, Has.Count.EqualTo(4));

            string keyword = $"QA Combined {Guid.NewGuid():N}";
            var form = new ShareSkillPage(browser);
            var listings = new ManageListingsPage(browser);
            var search = new SearchSkillPage(browser);
            var attemptedTitles = new List<string>();

            try
            {
                foreach (var item in data)
                {
                    string title = $"{keyword} {item.Title}";
                    form.OpenShareSkill();
                    form.FillForm(item, title);
                    attemptedTitles.Add(title);
                    SaveAndVerifyListing(form, listings, title);
                }

                VerifyCombinedSearchFilters(search, keyword, attemptedTitles);
            }
            finally
            {
                listings.Open();
                foreach (string title in attemptedTitles)
                    listings.DeleteListingIfExists(title);
            }
        }

        //Helper - FilterSearchResultsByCombinedFilters()
        private static void VerifyCombinedSearchFilters(SearchSkillPage search,string keyword, List<string> titles)
        {
            search.SearchUsingEnter(keyword);
            Assert.That(search.GetSubmittedKeyword(), Is.EqualTo(keyword));
            Assert.That(
                search.WaitForResultTitles(titles.ToArray()),
                Is.True,
                "All four setup listings should appear before filtering."
            );

            search.SelectTestAutomationCategory();
            AssertSelectedSearchResults(
                search, false, titles[0], titles[1], titles[2]
            );

            search.SelectSeleniumSubcategory();
            AssertSelectedSearchResults(search, true, titles[0], titles[1]);

            search.SelectLocationFilter("Onsite");
            AssertSelectedSearchResults(search, true, titles[0]);

            search.SelectLocationFilter("ShowAll");
            AssertSelectedSearchResults(search, true, titles[0], titles[1]);
        }

        private static void AssertSelectedSearchResults(SearchSkillPage search,bool seleniumSelected, params string[] expectedTitles)
        {
            Assert.That(
                search.WaitForResultTitles(expectedTitles),
                Is.True,
                "The selected filters should return exactly the expected listings."
            );

            Assert.That(
                search.IsCategorySelected("Test Automation"),
                Is.True,
                "Test Automation should remain visibly selected."
            );

            if (seleniumSelected)
            {
                Assert.That(
                    search.IsSubcategorySelected("Selenium"),
                    Is.True,
                    "Selenium should remain visibly selected."
                );
            }
        }

        [TestCase(
            "SearchCategoryWithNoMatchingListings.json",
            "Software Development",
            "",
            TestName = "SearchSkill_EmptyCategoryResults"
        )]

        [TestCase(
            "SearchSubcategoryWithNoMatchingListings.json",
            "Test Automation",
            "API Testing",
            TestName = "SearchSkill_EmptySubcategoryResults"
        )]

        [NonParallelizable]
        public void SearchFilterWithNoMatchingListings(string jsonFileName, string category, string subcategory)
        {
            Console.WriteLine("Category : "+category);
            Console.WriteLine("Sub-category : " + subcategory);
            var browser = driver
                ?? throw new InvalidOperationException("Browser was not created.");
            
            string filePath = Path.Combine(
                TestContext.CurrentContext.TestDirectory,
                "TestData", "SearchSkill", jsonFileName
            );
            
            var data = JsonReader.Read<ShareSkillData>(filePath);
            
            Assert.That(data.Category, Is.EqualTo("Test Automation"));
            Assert.That(data.Subcategory, Is.EqualTo("Selenium"));
            Assert.That(data.ActiveStatus, Is.EqualTo("Active"));
            
            string title = $"{data.Title} {Guid.NewGuid():N}";
            
            Assert.That(title.Length, Is.LessThanOrEqualTo(100));
            var form = new ShareSkillPage(browser);
            var listings = new ManageListingsPage(browser);
            var search = new SearchSkillPage(browser);
            bool saveAttempted = false;

            try
            {
                form.OpenShareSkill();
                form.FillForm(data, title);
                saveAttempted = true;
                SaveAndVerifyListing(form, listings, title);
                search.SearchUsingEnter(title);
                
                Assert.That(search.GetSubmittedKeyword(), Is.EqualTo(title));
                Assert.That(search.WaitForResultTitles(title), Is.True);
                
                VerifyEmptySearchFilter(search, category, subcategory, title);
            }
            finally
            {
                if (saveAttempted)
                {
                    listings.Open();
                    listings.DeleteListingIfExists(title);
                }
            }
        }

        private static void VerifyEmptySearchFilter(SearchSkillPage search, string category,string subcategory,string title)
        {
            search.SelectCategory(category);

            if (!string.IsNullOrEmpty(subcategory))
            {
                Assert.That(
                    search.WaitForResultTitles(title),
                    Is.True,
                    "The Selenium listing should remain under Test Automation."
                );

                search.SelectSubcategory(subcategory);
            }

            Assert.That(
                search.WaitForNoResults(),
                Is.True,
                "The selected filter should display the empty-results message."
            );

            Assert.That(
                search.GetVisibleResultTitles(),
                Is.Empty,
                "No listing cards should remain."
            );
        }


        [TestCase(
            "SearchPaginationWith9ResultsPerPage.json",
            TestName = "SearchSkill_PaginationWith9ResultsPerPage"
        )]

        [TestCase(
            "SearchWith12ResultsPerPage.json",
            TestName = "SearchSkill_12ResultsPerPage"
        )]

        [TestCase(
            "SearchWith18ResultsPerPage.json",
            TestName = "SearchSkill_18ResultsPerPage"
        )]

        [NonParallelizable]
        public void SearchResultsPagination(string jsonFileName)
        {
            var browser = driver
                ?? throw new InvalidOperationException("Browser was not created.");
            
            string filePath = Path.Combine(
                TestContext.CurrentContext.TestDirectory,
                "TestData", "SearchSkill", jsonFileName
            );
            var data = JsonReader.Read<SearchPaginationData>(filePath);
            
            Assert.That(data.ListingCount, Is.EqualTo(12));
            Assert.That(data.PageSize, Is.AnyOf(9, 12, 18));
            Assert.That(data.Listing.ActiveStatus, Is.EqualTo("Active"));
            
            string keyword = $"{data.Listing.Title} {Guid.NewGuid():N}";
            var form = new ShareSkillPage(browser);
            var listings = new ManageListingsPage(browser);
            var search = new SearchSkillPage(browser);
            var attemptedTitles = new List<string>();

            try
            {
                CreatePaginationListings(form, listings, data, keyword, attemptedTitles);
                search.SearchUsingEnter(keyword);
                
                Assert.That(search.GetSubmittedKeyword(), Is.EqualTo(keyword));
                search.SelectResultsPerPage(data.PageSize);
                
                VerifySearchPagination(search, attemptedTitles, data.PageSize);
            }
            finally
            {
                listings.Open();
                RemovePaginationListings(listings, attemptedTitles);
            }
        }

        //helper - creates the test listings using your existing save helper

        private void CreatePaginationListings(ShareSkillPage form,ManageListingsPage listings,
            SearchPaginationData data, string keyword,List<string> attemptedTitles)
        {
            for (int number = 1; number <= data.ListingCount; number++)
            {
                string title = $"{keyword} {number:D2}";
                Assert.That(title.Length, Is.LessThanOrEqualTo(100));

                form.OpenShareSkill();
                form.FillForm(data.Listing, title);
                attemptedTitles.Add(title);
                SaveAndVerifyListing(form, listings, title);
            }
        }

        //helper - verifies the results and navigation
        private static void VerifySearchPagination(SearchSkillPage search, List<string> expectedTitles,int pageSize)
        {
            var firstPage = search.WaitForVisibleResultCount(
                Math.Min(pageSize, expectedTitles.Count)
            );
            Assert.That(search.GetCurrentResultsPage(), Is.EqualTo(1));

            if (pageSize == 9)
            {
                Assert.That(search.IsNextResultsPageDisabled(), Is.False);
                search.GoToNextResultsPage();
                var secondPage = search.WaitForVisibleResultCount(3);

                Assert.That(search.GetCurrentResultsPage(), Is.EqualTo(2));
                Assert.That(search.IsNextResultsPageDisabled(), Is.True);
                Assert.That(firstPage.Intersect(secondPage), Is.Empty,
                    "The two pages must not repeat any listing.");
                Assert.That(firstPage.Concat(secondPage),
                    Is.EquivalentTo(expectedTitles),
                    "Both pages together must contain exactly the 12 test listings.");

                search.GoToPreviousResultsPage();
                Assert.That(search.GetCurrentResultsPage(), Is.EqualTo(1));
                Assert.That(search.WaitForResultTitles(firstPage.ToArray()), Is.True,
                    "Returning should restore the original first-page results.");
                Assert.That(search.IsNextResultsPageDisabled(), Is.False);
            }
            else
            {
                Assert.That(firstPage, Is.EquivalentTo(expectedTitles),
                    "All 12 listings should fit on one page.");
                Assert.That(search.IsNextResultsPageDisabled(), Is.True,
                    "A second page should not be available.");
            }
        }

        //helper - cleanup
        private static void RemovePaginationListings( ManageListingsPage listings,List<string> attemptedTitles)
        {
            var failures = new List<Exception>();

            foreach (string title in attemptedTitles)
            {
                try
                {
                    listings.DeleteListingIfExists(title);
                }
                catch (Exception exception)
                {
                    failures.Add(new InvalidOperationException(
                        $"Cleanup failed for listing '{title}'.",
                        exception
                    ));
                }
            }

            if (failures.Count > 0)
            {
                throw new AggregateException(
                    "Some pagination test listings could not be removed.",
                    failures
                );
            }
        }

        [Test]
        [NonParallelizable]
        public void SearchSkillByTitleUsingIcon()
        {
            var browser = driver
                ?? throw new InvalidOperationException("Browser was not created.");
            string filePath = Path.Combine(
                TestContext.CurrentContext.TestDirectory,
                "TestData", "SearchSkill", "SearchSkillByTitleUsingIcon.json"
            );

            var data = JsonReader.Read<ShareSkillData>(filePath);
            
            Assert.That(data.ActiveStatus, Is.EqualTo("Active"));
            
            string title = $"{data.Title} {Guid.NewGuid():N}";
            Assert.That(title.Length, Is.LessThanOrEqualTo(100));
            
            var form = new ShareSkillPage(browser);
            var listings = new ManageListingsPage(browser);
            var search = new SearchSkillPage(browser);
            bool saveAttempted = false;

            try
            {
                form.OpenShareSkill();
                form.FillForm(data, title);
                saveAttempted = true;
                SaveAndVerifyListing(form, listings, title);

                search.SearchUsingIcon(title);
                Assert.That(
                    search.GetSubmittedKeyword(),
                    Is.EqualTo(title),
                    "Clicking the search icon should submit the entered keyword."
                );
                Assert.That(
                    search.WaitForResultTitles(title),
                    Is.True,
                    "Searching the unique title should return only that listing."
                );
            }
            finally
            {
                if (saveAttempted)
                {
                    listings.Open();
                    listings.DeleteListingIfExists(title);
                }
            }
        }

        [Test]
        [NonParallelizable]
        public void SearchSkillWithNoMatchingTitleUsingIcon()
        {
            var browser = driver
                ?? throw new InvalidOperationException("Browser was not created.");

            string filePath = Path.Combine(
                TestContext.CurrentContext.TestDirectory,
                "TestData", "SearchSkill",
                "SearchSkillWithNoMatchingTitleUsingIcon.json"
            );

            var data = JsonReader.Read<SearchSkillData>(filePath);
            Assert.That(data.Keyword, Is.Not.Null.And.Not.Empty);
            string keyword = $"{data.Keyword} {Guid.NewGuid():N}";

            var listings = new ManageListingsPage(browser);
            var search = new SearchSkillPage(browser);
            listings.Open();

            search.SearchUsingIcon(keyword);

            Assert.That(
                search.GetSubmittedKeyword(),
                Is.EqualTo(keyword),
                "The search icon should submit the unmatched keyword."
            );

            Assert.That(
                search.WaitForNoResults(),
                Is.True,
                "An unmatched keyword should display the empty-results message."
            );

            Assert.That(
                search.GetVisibleResultTitles(),
                Is.Empty,
                "An unmatched keyword should return no listing cards."
            );
        }

        [Test]
        [NonParallelizable]
        public void BrowseAllCategoriesShowsCorrectCount()
        {
            var browser = driver
                ?? throw new InvalidOperationException("Browser was not created.");

            string filePath = Path.Combine(
                TestContext.CurrentContext.TestDirectory,
                "TestData", "SearchSkill", "BrowseAllCategories.json"
            );
            var data = JsonReader.Read<SearchBrowseData>(filePath);
            Assert.That(data.PageSize, Is.EqualTo(18));

            var search = new SearchSkillPage(browser);
            search.OpenSearchWithoutKeyword();
            search.SelectAllCategories();
            search.SelectLocationFilter("ShowAll");
            search.SelectResultsPerPage(data.PageSize);

            Assert.That(search.GetSubmittedKeyword(), Is.Empty);
            int displayedTotal = search.GetAllCategoriesCount();
            int actualTotal = CountAllSearchResults(
                search, displayedTotal, data.PageSize
            );

            Console.WriteLine($"All Categories displayed total: {displayedTotal}");
            Console.WriteLine($"Actual result cards across all pages: {actualTotal}");

            Assert.That(
                actualTotal,
                Is.EqualTo(displayedTotal),
                "The displayed total should match the actual result-card count."
            );
            Assert.That(search.GetAllCategoriesCount(), Is.EqualTo(displayedTotal),
                "The displayed total should remain stable during pagination.");
        }

        //Helper - all category search
        private static int CountAllSearchResults(SearchSkillPage search,int displayedTotal,int pageSize)
        {
            Assert.That(displayedTotal, Is.GreaterThanOrEqualTo(0));

            if (displayedTotal == 0)
            {
                Assert.That(search.WaitForNoResults(), Is.True);
                Assert.That(search.GetVisibleResultTitles(), Is.Empty);
                return 0;
            }

            Assert.That(search.GetCurrentResultsPage(), Is.EqualTo(1));
            int pageCount = (displayedTotal + pageSize - 1) / pageSize;
            int actualTotal = 0;

            for (int page = 1; page <= pageCount; page++)
            {
                int expectedOnPage = Math.Min(
                    pageSize, displayedTotal - actualTotal
                );
                var results = search.WaitForVisibleResultCount(expectedOnPage);
                Assert.That(search.GetCurrentResultsPage(), Is.EqualTo(page));
                actualTotal += results.Count;

                bool isLastPage = page == pageCount;
                Assert.That(
                    search.IsNextResultsPageDisabled(),
                    Is.EqualTo(isLastPage),
                    $"Next-button state should be correct on page {page}."
                );

                if (!isLastPage)
                    search.GoToNextResultsPage();
            }

            return actualTotal;
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