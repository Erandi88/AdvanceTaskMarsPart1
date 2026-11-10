using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

namespace MarsAdvanced.Tests.Pages;

public class ManageListingsPage
{
    private readonly IWebDriver _driver;
    private readonly WebDriverWait _wait;

    private readonly By ManageListingsLink = By.CssSelector(
        "a[href='/Home/ListingManagement']"
    );

    private readonly By ListingRows =
        By.CssSelector("table tbody tr");

    private readonly By CurrentPage = By.CssSelector(
        ".semantic-ui-react-button-pagination button.currentPage"
    );

    private readonly By PreviousButton = By.XPath(
        "//div[contains(@class,'semantic-ui-react-button-pagination')]" +
        "/button[normalize-space()='<']"
    );

    private readonly By NextButton = By.XPath(
        "//div[contains(@class,'semantic-ui-react-button-pagination')]" +
        "/button[normalize-space()='>']"
    );

    private readonly By DeleteDialog = By.CssSelector(
        "div.ui.modal.visible.active"
    );

    private readonly By ConfirmDeleteButton = By.CssSelector(
        "button.ui.positive"
    );

    public ManageListingsPage(IWebDriver driver)
    {
        _driver = driver;
        _wait = new WebDriverWait(
            driver,
            TimeSpan.FromSeconds(30)
        );
    }

    // Open the management page and wait for its table.
    public void Open()
    {
        _wait.Until(
            ExpectedConditions.ElementToBeClickable(
                ManageListingsLink
            )
        ).Click();

        _wait.Until(
            ExpectedConditions.ElementIsVisible(
                By.XPath(
                    "//table/thead/tr/th[normalize-space()='Title']"
                )
            )
        );

        _wait.Until(
            ExpectedConditions.ElementIsVisible(CurrentPage)
        );
    }

    // Search every page, starting from the first.
    public bool FindListingByTitle(string title)
    {
        GoToFirstPage();

        while (true)
        {
            if (FindRowOnCurrentPage(title) != null)
                return true;

            if (!IsButtonEnabled(NextButton))
                return false;

            ChangePage(NextButton);
        }
    }

    // Match the third column: Title.
    private IWebElement? FindRowOnCurrentPage(string title)
    {
        foreach (var row in _driver.FindElements(ListingRows))
        {
            var cells = row.FindElements(By.TagName("td"));

            if (cells.Count >= 3 &&
                cells[2].Text.Trim() == title)
            {
                return row;
            }
        }

        return null;
    }

    // Reset the search so earlier pages are not missed.
    private void GoToFirstPage()
    {
        while (IsButtonEnabled(PreviousButton))
        {
            ChangePage(PreviousButton);
        }
    }

    private bool IsButtonEnabled(By locator)
    {
        var buttons = _driver.FindElements(locator);

        return buttons.Any(button =>
            button.Displayed &&
            button.Enabled &&
            !(button.GetAttribute("class") ?? string.Empty)
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Contains("disabled")
        );
    }

    // Capture row content to detect the new page's data.
    private string GetRowsSnapshot()
    {
        return string.Join(
            "\n",
            _driver.FindElements(ListingRows)
                .Select(row => row.Text)
        );
    }

    private void ChangePage(By buttonLocator)
    {
        string previousPage = _wait.Until(
            ExpectedConditions.ElementIsVisible(CurrentPage)
        ).Text;

        string previousRows = GetRowsSnapshot();

        _wait.Until(
            ExpectedConditions.ElementToBeClickable(buttonLocator)
        ).Click();

        // Wait for both the page indicator and table data to change.
        _wait.Until(driver =>
        {
            try
            {
                string newPage =
                    driver.FindElement(CurrentPage).Text;

                string newRows = GetRowsSnapshot();

                return newPage != previousPage &&
                       !string.IsNullOrWhiteSpace(newRows) &&
                       newRows != previousRows;
            }
            catch (StaleElementReferenceException)
            {
                return false;
            }
            catch (NoSuchElementException)
            {
                return false;
            }
        });
    }

    // Find the exact listing, then delete it.
    public void DeleteListing(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException(
                "A listing title is required.",
                nameof(title)
            );

        if (!FindListingByTitle(title))
        {
            throw new NoSuchElementException(
                $"Listing '{title}' was not found."
            );
        }

        DeleteListingOnCurrentPage(title);
    }

    // Cleanup: remove only the specified test listing if present.
    public void DeleteListingIfExists(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException(
                "A listing title is required.",
                nameof(title)
            );

        if (FindListingByTitle(title))
        {
            DeleteListingOnCurrentPage(title);
        }
    }

    private void DeleteListingOnCurrentPage(string title)
    {
        WaitForNotificationsToDisappear();

        var row = FindRowOnCurrentPage(title)
            ?? throw new NoSuchElementException(
                $"Listing '{title}' is no longer on the current page."
            );

        // Click the remove button belonging to this row only.
        var deleteButton = row.FindElement(
            By.XPath(
                ".//button[i[contains(" +
                "concat(' ', normalize-space(@class), ' ')," +
                "' remove ')]]"
            )
        );

        _wait.Until(_ =>
            deleteButton.Displayed && deleteButton.Enabled
        );

        deleteButton.Click();

        var dialog = _wait.Until(
            ExpectedConditions.ElementIsVisible(DeleteDialog)
        );

        // Check the dialog before confirming a destructive action.
        string heading = dialog.FindElement(
            By.CssSelector(".header")
        ).Text.Trim();

        bool titleMatches = dialog.FindElements(
            By.CssSelector(".content p")
        ).Any(paragraph => paragraph.Text.Trim() == title);

        if (heading != "Delete Your Service" || !titleMatches)
        {
            throw new InvalidOperationException(
                $"The delete dialog did not identify listing '{title}'. " +
                "Deletion was not confirmed."
            );
        }

        // Find Yes inside this dialog, not elsewhere on the page.
        _wait.Until(_ =>
        {
            var button = dialog.FindElement(ConfirmDeleteButton);

            return button.Displayed && button.Enabled
                ? button
                : null;
        }).Click();

        _wait.Until(
            ExpectedConditions.InvisibilityOfElementLocated(
                DeleteDialog
            )
        );

        // Wait for the deleted row to disappear from this page.
        _wait.Until(_ =>
        {
            try
            {
                return FindRowOnCurrentPage(title) == null;
            }
            catch (StaleElementReferenceException)
            {
                return false;
            }
        });

        // Check all pages to verify removal.
        if (FindListingByTitle(title))
        {
            throw new InvalidOperationException(
                $"Listing '{title}' still exists after deletion."
            );
        }
    }

    private void WaitForNotificationsToDisappear()
    {
        _wait.Until(driver =>
        {
            try
            {
                return driver.FindElements(By.CssSelector(".ns-box"))
                    .All(notification => !notification.Displayed);
            }
            catch (StaleElementReferenceException)
            {
                return false;
            }
        });
    }

    // Open the edit form for the exact test listing.
    public void OpenListingForEdit(string title)
    {
        if (!FindListingByTitle(title))
        {
            throw new NoSuchElementException(
                $"Listing '{title}' was not found."
            );
        }

        var row = FindRowOnCurrentPage(title)
            ?? throw new NoSuchElementException(
                $"Listing '{title}' is no longer on the current page."
            );

        var editButton = row.FindElement(
            By.XPath(
                ".//button[i[contains(" +
                "concat(' ', normalize-space(@class), ' ')," +
                "' write ')]]"
            )
        );

        _wait.Until(_ =>
            editButton.Displayed && editButton.Enabled
        );

        editButton.Click();

        // Wait for this listing's data, not just an empty form.
        _wait.Until(driver =>
        {
            try
            {
                var field = driver.FindElement(By.Name("title"));

                return field.Displayed &&
                       field.GetDomProperty("value") == title;
            }
            catch (NoSuchElementException)
            {
                return false;
            }
            catch (StaleElementReferenceException)
            {
                return false;
            }
        });
    }

    // Capture titles across all pages, retaining duplicate and blank titles.
    public List<string> GetAllListingTitles()
    {
        GoToFirstPage();

        var titles = new List<string>();

        while (true)
        {
            foreach (var row in _driver.FindElements(ListingRows))
            {
                var cells = row.FindElements(By.TagName("td"));

                if (cells.Count >= 3)
                {
                    titles.Add(cells[2].Text.Trim());
                }
            }

            if (!IsButtonEnabled(NextButton))
                return titles;

            ChangePage(NextButton);
        }
    }

    public void CancelDeleteListing(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException(
                "A listing title is required.",
                nameof(title)
            );
        }

        if (!FindListingByTitle(title))
        {
            throw new InvalidOperationException(
                $"Listing was not found: {title}"
            );
        }

        var row = FindRowOnCurrentPage(title)
            ?? throw new InvalidOperationException(
                $"Listing row was not found: {title}"
            );

        // Open the deletion dialog for this listing.
        row.FindElement(
            By.XPath(
                ".//button[i[" +
                "contains(concat(' ', normalize-space(@class), ' '), ' remove ')" +
                "]]"
            )
        ).Click();

        var dialogLocator = By.CssSelector(
            "div.ui.modal.visible.active"
        );

        var dialog = _wait.Until(
            ExpectedConditions.ElementIsVisible(dialogLocator)
        );

        // Check the dialog identifies the intended listing.
        bool correctHeading =
            dialog.FindElement(By.CssSelector(".header"))
                .Text.Trim() == "Delete Your Service";

        bool correctTitle = dialog
            .FindElements(By.CssSelector(".content p"))
            .Any(element => element.Text.Trim() == title);

        if (!correctHeading || !correctTitle)
        {
            throw new InvalidOperationException(
                "The delete dialog did not identify the expected listing."
            );
        }

        // Choose No and wait for the dialog to close.
        _wait.Until(driver =>
        {
            var button = driver
                .FindElement(dialogLocator)
                .FindElement(By.CssSelector("button.ui.negative"));

            return button.Displayed && button.Enabled
                ? button
                : null;
        }).Click();

        _wait.Until(
            ExpectedConditions.InvisibilityOfElementLocated(
                dialogLocator
            )
        );
    }
}