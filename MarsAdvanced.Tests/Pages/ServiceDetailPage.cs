using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace MarsAdvanced.Tests.Pages;

public class ServiceDetailPage
{
    private readonly IWebDriver _driver;
    private readonly WebDriverWait _wait;

    private readonly By Title =
        By.CssSelector("span.skill-title");

    public ServiceDetailPage(IWebDriver driver)
    {
        _driver = driver;
        _wait = new WebDriverWait(
            driver,
            TimeSpan.FromSeconds(30)
        );
    }

    public string GetUrl()
    {
        return _driver.Url;
    }

    public string WaitForTitle()
    {
        return _wait.Until(driver =>
        {
            try
            {
                bool isDetailPage = new Uri(driver.Url)
                    .AbsolutePath.Equals(
                        "/Home/ServiceDetail",
                        StringComparison.OrdinalIgnoreCase
                    );

                if (!isDetailPage)
                {
                    return null;
                }

                var title = driver.FindElements(Title)
                    .FirstOrDefault(element => element.Displayed);

                string text = title?.Text.Trim() ?? string.Empty;

                return text.Length > 0 ? text : null;
            }
            catch (StaleElementReferenceException)
            {
                return null;
            }
        })!;
    }

    public string GetCategory()
    {
        return GetDetailValue("Category");
    }

    public string GetSubcategory()
    {
        return GetDetailValue("Subcategory");
    }

    public string GetLocationType()
    {
        return GetDetailValue("Location Type");
    }

    private string GetDetailValue(string heading)
    {
        var locator = By.XPath(
            "//div[@class='content']" +
            $"/div[@class='header' and normalize-space()='{heading}']" +
            "/following-sibling::div[@class='description'][1]"
        );

        return _wait.Until(driver =>
        {
            try
            {
                var element = driver.FindElements(locator)
                    .FirstOrDefault(item => item.Displayed);

                string value = element?.Text.Trim() ?? string.Empty;

                return value.Length > 0 ? value : null;
            }
            catch (StaleElementReferenceException)
            {
                return null;
            }
        })!;
    }
}