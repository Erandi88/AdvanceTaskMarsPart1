using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace MarsAdvanced.Tests.Helpers;

public static class CommonDriver
{
    public static IWebDriver CreateChromeDriver()
    {
        var driver = new ChromeDriver();
        driver.Manage().Window.Maximize();

        return driver;
    }
}