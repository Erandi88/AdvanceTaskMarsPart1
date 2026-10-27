using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace MarsAdvanced.Tests;

public class BrowserTests
{
    [Test]
    public void MarsHomePageCanBeOpened()
    {
        using IWebDriver driver = new ChromeDriver();

        driver.Navigate().GoToUrl("http://localhost:5003/Home");

        Assert.That(
            driver.Url,
            Does.Contain("localhost:5003"),
            "The browser should open the local MARS application."
        );
    }
}