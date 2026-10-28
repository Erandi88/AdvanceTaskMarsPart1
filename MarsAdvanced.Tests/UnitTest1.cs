using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using MarsAdvanced.Tests.Helpers;


namespace MarsAdvanced.Tests;

public class BrowserTests
{
    [Test]
    public void MarsHomePageCanBeOpened()
    {
        using IWebDriver driver = CommonDriver.CreateChromeDriver();


        driver.Navigate().GoToUrl("http://localhost:5003/Home");

        Assert.That(
            driver.Url,
            Does.Contain("localhost:5003"),
            "The browser should open the local MARS application."
        );
    }
}