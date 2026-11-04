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

    private readonly By HoursDropdown =
    By.Name("availabiltyHour");

    private readonly By HoursEditButton = By.XPath(
        "//div[contains(concat(' ', normalize-space(@class), ' '), ' item ')]" +
        "[.//text()[normalize-space(.)='Hours']]" +
        "//i[contains(@class,'write')]"
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

    private readonly By HoursCancelButton = By.XPath(
        "//select[@name='availabiltyHour']" +
        "/following-sibling::i[contains(@class,'remove')]"
    );

    private readonly By EarnTargetDropdown =
    By.Name("availabiltyTarget");

    private readonly By EarnTargetEditButton = By.XPath(
        "//div[contains(concat(' ', normalize-space(@class), ' '), ' item ')]" +
        "[.//text()[normalize-space(.)='Earn Target']]" +
        "//i[contains(@class,'write')]"
    );

    private readonly By EarnTargetCancelButton = By.XPath(
        "//select[@name='availabiltyTarget']" +
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

    public void OpenHoursEditor()
    {
        _wait.Until(
            ExpectedConditions.ElementToBeClickable(HoursEditButton)
        ).Click();

        _wait.Until(
            ExpectedConditions.ElementIsVisible(HoursDropdown)
        );
    }

    public void SelectHours(string hours)
    {
        // Avoid mistaking an earlier notification for this save.
        _wait.Until(
            ExpectedConditions.InvisibilityOfElementLocated(
                AvailabilityUpdatedMessage
            )
        );

        var dropdown = _wait.Until(
            ExpectedConditions.ElementToBeClickable(HoursDropdown)
        );

        new SelectElement(dropdown).SelectByText(hours);
    }

    public bool WaitForHours(string expectedValue)
    {
        try
        {
            return _wait.Until(driver =>
            {
                var element = driver.FindElement(HoursValue);

                return element.Displayed &&
                       element.Text.Trim() == expectedValue;
            });
        }
        catch (WebDriverTimeoutException)
        {
            return false;
        }
    }

    public void CloseHoursEditor()
    {
        _wait.Until(
            ExpectedConditions.ElementToBeClickable(HoursCancelButton)
        ).Click();

        _wait.Until(
            ExpectedConditions.InvisibilityOfElementLocated(HoursDropdown)
        );
    }

    /* Earn Target */
    public void OpenEarnTargetEditor()
    {
        _wait.Until(
            ExpectedConditions.ElementToBeClickable(EarnTargetEditButton)
        ).Click();

        _wait.Until(
            ExpectedConditions.ElementIsVisible(EarnTargetDropdown)
        );
    }

    public void SelectEarnTarget(string earnTarget)
    {
        _wait.Until(
            ExpectedConditions.InvisibilityOfElementLocated(
                AvailabilityUpdatedMessage
            )
        );

        var dropdown = _wait.Until(
            ExpectedConditions.ElementToBeClickable(EarnTargetDropdown)
        );

        new SelectElement(dropdown).SelectByText(earnTarget);
    }

    public bool WaitForEarnTarget(string expectedValue)
    {
        try
        {
            return _wait.Until(driver =>
            {
                var element = driver.FindElement(EarnTargetValue);

                return element.Displayed &&
                       element.Text.Trim() == expectedValue;
            });
        }
        catch (WebDriverTimeoutException)
        {
            return false;
        }
    }

    public void CloseEarnTargetEditor()
    {
        _wait.Until(
            ExpectedConditions.ElementToBeClickable(
                EarnTargetCancelButton
            )
        ).Click();

        _wait.Until(
            ExpectedConditions.InvisibilityOfElementLocated(
                EarnTargetDropdown
            )
        );
    }
}