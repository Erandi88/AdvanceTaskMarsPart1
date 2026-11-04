using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

namespace MarsAdvanced.Tests.Pages;

public class ProfileSettingsPage
{
    private readonly IWebDriver _driver;
    private readonly WebDriverWait _wait;

    private readonly By AvailabilityEditButton = By.XPath(
    "//div[contains(concat(' ', normalize-space(@class), ' '), ' item ')]" +
    "[.//text()[normalize-space(.)='Availability']]" +
    "//i[contains(@class,'write')]"
);

    private readonly By AvailabilityValue = By.XPath(
        "//div[contains(concat(' ', normalize-space(@class), ' '), ' item ')]" +
        "[.//text()[normalize-space(.)='Availability']]" +
        "//span[i[contains(@class,'write')]]"
    );

    private readonly By AvailabilityDropdown = By.Name("availabiltyType");
    private readonly By HoursValue = By.XPath(
        "//div[contains(concat(' ', normalize-space(@class), ' '), ' item ')]" +
        "[.//text()[normalize-space(.)='Hours']]" +
        "//span[i[contains(@class,'write')]]"
    );

    private readonly By EarnTargetValue = By.XPath(
        "//div[contains(concat(' ', normalize-space(@class), ' '), ' item ')]" +
        "[.//text()[normalize-space(.)='Earn Target']]" +
        "//span[i[contains(@class,'write')]]"
    );

    private readonly By AvailabilityUpdatedMessage = By.XPath(
        "//*[normalize-space()='Availability updated']"
    );

    private readonly By AvailabilityCancelButton = By.XPath(
        "//select[@name='availabiltyType']" +
        "/following-sibling::i[contains(@class,'remove')]"
    );

    public ProfileSettingsPage(IWebDriver driver)
    {
        _driver = driver;
        _wait = new WebDriverWait(
            driver,
            TimeSpan.FromSeconds(30)
        );
    }

    public string GetAvailability()
    {
        return _wait.Until(
            ExpectedConditions.ElementIsVisible(AvailabilityValue)
        ).Text.Trim();
    }

    public void OpenAvailabilityEditor()
    {
        _wait.Until(
            ExpectedConditions.ElementToBeClickable(
                AvailabilityEditButton
            )
        ).Click();

        _wait.Until(
            ExpectedConditions.ElementIsVisible(
                AvailabilityDropdown
            )
        );
    }

    public void SelectAvailability(string availability)
    {
        // Wait for any previous save notification to disappear.
        _wait.Until(
            ExpectedConditions.InvisibilityOfElementLocated(
                AvailabilityUpdatedMessage
            )
        );

        var dropdown = _wait.Until(
            ExpectedConditions.ElementToBeClickable(
                AvailabilityDropdown
            )
        );

        new SelectElement(dropdown).SelectByText(availability);
    }

    public bool WaitForAvailability(string expectedValue)
    {
        try
        {
            return _wait.Until(_ =>
                GetAvailability() == expectedValue
            );
        }
        catch (WebDriverTimeoutException)
        {
            return false;
        }
    }

    public string GetHours()
    {
        return _wait.Until(
            ExpectedConditions.ElementIsVisible(HoursValue)
        ).Text.Trim();
    }

    public string GetEarnTarget()
    {
        return _wait.Until(
            ExpectedConditions.ElementIsVisible(EarnTargetValue)
        ).Text.Trim();
    }

    public bool IsAvailabilityUpdatedMessageDisplayed()
    {
        try
        {
            return _wait.Until(
                ExpectedConditions.ElementIsVisible(
                    AvailabilityUpdatedMessage
                )
            ).Displayed;
        }
        catch (WebDriverTimeoutException)
        {
            return false;
        }
    }

    public void CloseAvailabilityEditor()
    {
        _wait.Until(
            ExpectedConditions.ElementToBeClickable(
                AvailabilityCancelButton
            )
        ).Click();

        _wait.Until(
            ExpectedConditions.InvisibilityOfElementLocated(
                AvailabilityDropdown
            )
        );
    }
}