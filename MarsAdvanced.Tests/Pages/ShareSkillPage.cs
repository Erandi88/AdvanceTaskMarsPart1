using MarsAdvanced.Tests.Models;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

namespace MarsAdvanced.Tests.Pages;

public class ShareSkillPage
{
    private readonly IWebDriver _driver;
    private readonly WebDriverWait _wait;

    private readonly By TitleField = By.Name("title");
    private readonly By DescriptionField = By.Name("description");
    private readonly By CategoryDropdown = By.Name("categoryId");
    private readonly By SubcategoryDropdown = By.Name("subcategoryId");

    // tag inputs to their field headings.
    private readonly By TagsInput = TagInputFor("Tags");
    private readonly By SkillExchangeInput = TagInputFor("Skill-Exchange");

    // Form actions.
    private readonly By SaveButton =
        By.CssSelector("input[type='button'][value='Save']");

    private readonly By CancelButton =
        By.CssSelector("input[type='button'][value='Cancel']");

    public ShareSkillPage(IWebDriver driver)
    {
        _driver = driver;
        _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(30));
    }

    // Clear the existing title and enter the supplied value.
    public void EnterTitle(string title)
    {
        var field = _wait.Until(
            ExpectedConditions.ElementIsVisible(TitleField)
        );

        field.Clear();
        field.SendKeys(title);
    }

    // Clear the existing description and enter the supplied value.
    public void EnterDescription(string description)
    {
        var field = _wait.Until(
            ExpectedConditions.ElementIsVisible(DescriptionField)
        );

        field.Clear();
        field.SendKeys(description);
    }

    // Select a category using its visible option text.
    public void SelectCategory(string category)
    {
        var dropdown = _wait.Until(
            ExpectedConditions.ElementToBeClickable(CategoryDropdown)
        );

        new SelectElement(dropdown).SelectByText(category);
    }

    // Subcategory options load after selecting a category.
    public void SelectSubcategory(string subcategory)
    {
        _wait.Until(driver =>
        {
            try
            {
                var dropdown =
                    driver.FindElement(SubcategoryDropdown);

                if (!dropdown.Displayed || !dropdown.Enabled)
                    return false;

                var select = new SelectElement(dropdown);

                bool optionExists = select.Options.Any(
                    option => option.Text.Trim() == subcategory
                );

                if (!optionExists)
                    return false;

                select.SelectByText(subcategory);

                return true;
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

    // Add a tag to the service's Tags field.
    public void AddTag(string tag)
    {
        AddTagToField(TagsInput, "Tags", tag);
    }

    // Add a skill the user wants to receive in exchange.
    public void AddSkillExchangeTag(string tag)
    {
        AddTagToField(
            SkillExchangeInput,
            "Skill-Exchange",
            tag
        );
    }

    // Shared tag entry: Enter commits the text as a tag.
    private void AddTagToField(
        By inputLocator,
        string heading,
        string tag)
    {
        var input = _wait.Until(
            ExpectedConditions.ElementToBeClickable(inputLocator)
        );

        input.SendKeys(tag);
        input.SendKeys(Keys.Enter);

        var committedTags = By.XPath(
            FieldColumnXPath(heading) +
            "//span[" +
            "contains(concat(' ', normalize-space(@class), ' '), " +
            "' ReactTags__tag ')]"
        );

        // Compare tag text directly, supporting quotes in tag names.
        _wait.Until(driver =>
            driver.FindElements(committedTags).Any(element =>
                element.Displayed &&
                element.Text.TrimEnd('×').Trim() == tag.Trim()
            )
        );
    }

    // Choose Hourly basis service or One-off service.
    public void SelectServiceType(string serviceType)
    {
        SelectRadioOption("serviceType", serviceType);
    }

    // Choose On-site or Online.
    public void SelectLocationType(string locationType)
    {
        SelectRadioOption("locationType", locationType);
    }

    // Choose Skill-exchange or Credit.
    public void SelectSkillTrade(string skillTrade)
    {
        SelectRadioOption("skillTrades", skillTrade);
    }

    // Choose Active or Hidden.
    public void SelectActiveStatus(string status)
    {
        SelectRadioOption("isActive", status);
    }

    // Click the styled radio label, then verify selection.
    private void SelectRadioOption(
        string groupName,
        string optionText)
    {
        var labelLocator = By.XPath(
            $"//input[@type='radio' and @name='{groupName}']" +
            "/following-sibling::label[" +
            $"normalize-space()={XPathLiteral(optionText)}]"
        );

        _wait.Until(
            ExpectedConditions.ElementToBeClickable(labelLocator)
        ).Click();

        var inputLocator = By.XPath(
            $"//input[@type='radio' and @name='{groupName}']" +
            "[following-sibling::label[" +
            $"normalize-space()={XPathLiteral(optionText)}]]"
        );

        _wait.Until(driver =>
            driver.FindElement(inputLocator).Selected
        );
    }

    // Submit the listing; tests verify the save result.
    public void ClickSave()
    {
        _wait.Until(
            ExpectedConditions.ElementToBeClickable(SaveButton)
        ).Click();
    }

    // Close the form without submitting the listing.
    public void ClickCancel()
    {
        _wait.Until(
            ExpectedConditions.ElementToBeClickable(CancelButton)
        ).Click();
    }

    // Find the input in the column beside its heading.
    private static By TagInputFor(string heading)
    {
        return By.XPath(
            FieldColumnXPath(heading) +
            "//input[@placeholder='Add new tag']"
        );
    }

    private static string FieldColumnXPath(string heading)
    {
        return "//h3[normalize-space()=" +
               XPathLiteral(heading) +
               "]/parent::div/following-sibling::div[1]";
    }

    // Safely include text containing quotation marks in XPath.
    private static string XPathLiteral(string value)
    {
        if (!value.Contains("'"))
            return $"'{value}'";

        if (!value.Contains("\""))
            return $"\"{value}\"";

        return "concat(" +
               string.Join(
                   ", \"'\", ",
                   value.Split('\'').Select(part => $"'{part}'")
               ) +
               ")";
    }

    // Open the Share Skill form.
    public void OpenShareSkill()
    {
        var link = By.CssSelector(
            "a[href='/Home/ServiceListing']"
        );

        _wait.Until(
            ExpectedConditions.ElementToBeClickable(link)
        ).Click();

        _wait.Until(
            ExpectedConditions.ElementIsVisible(TitleField)
        );
    }

    // Navigate to the listings management page.
    public void OpenManageListings()
    {
        var link = By.CssSelector(
            "a[href='/Home/ListingManagement']"
        );

        _wait.Until(
            ExpectedConditions.ElementToBeClickable(link)
        ).Click();

        var titleHeader = By.XPath(
            "//table/thead/tr/th[normalize-space()='Title']"
        );

        _wait.Until(
            ExpectedConditions.ElementIsVisible(titleHeader)
        );
    }


    // Wait for Save to navigate to Manage Listings.
    public void WaitForSaveToComplete()
    {
        _wait.Until(driver =>
            new Uri(driver.Url).AbsolutePath.Equals(
                "/Home/ListingManagement",
                StringComparison.OrdinalIgnoreCase
            )
        );
    }

    // Fill the valid Skill-exchange form without submitting it.
    public void FillForm(ShareSkillData data, string title)
    {
        if (data.SkillTrade != "Skill-exchange")
        {
            throw new ArgumentException(
                "FillForm currently supports Skill-exchange listings only.",
                nameof(data)
            );
        }

        EnterTitle(title);
        EnterDescription(data.Description);

        SelectCategory(data.Category);
        SelectSubcategory(data.Subcategory);

        foreach (string tag in data.Tags)
        {
            AddTag(tag);
        }

        SelectServiceType(data.ServiceType);
        SelectLocationType(data.LocationType);
        SelectSkillTrade(data.SkillTrade);

        foreach (string tag in data.SkillExchangeTags)
        {
            AddSkillExchangeTag(tag);
        }

        SelectActiveStatus(data.ActiveStatus);
    }

}