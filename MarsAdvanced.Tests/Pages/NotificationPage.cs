using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

namespace MarsAdvanced.Tests.Pages;

public class NotificationPage
{
    private readonly WebDriverWait _wait;

    private readonly By NotificationMenu = By.XPath(
        "//div[contains(concat(' ', normalize-space(@class), ' ')," +
        " ' dropdown ')][text()[normalize-space()='Notification']]"
    );

    private readonly By MenuItems = By.XPath(
        "//div[contains(concat(' ', normalize-space(@class), ' ')," +
        " ' dropdown ')][text()[normalize-space()='Notification']]" +
        "//div[contains(concat(' ', normalize-space(@class), ' ')," +
        " ' menu ')]//div[@class='item']"
    );

    public NotificationPage(IWebDriver driver)
    {
        _wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
    }

    public void Open()
    {
        _wait.Until(
            ExpectedConditions.ElementToBeClickable(NotificationMenu)
        ).Click();
    }

    public string WaitForMenuText()
    {
        return _wait.Until(driver =>
        {
            try
            {
                var item = driver.FindElements(MenuItems)
                    .FirstOrDefault(element => element.Displayed);

                string text = item?.Text.Trim() ?? string.Empty;
                return text.Length > 0 ? text : null;
            }
            catch (StaleElementReferenceException)
            {
                return null;
            }
        })!;
    }
}