using MarsAdvanced.Tests.Pages;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using MarsAdvanced.Tests.Helpers;

namespace MarsAdvanced.Tests;

public class LoginTests
{
    [Test]
    public void UserCanLogInWithValidCredentials()
    {
        string? email = Environment.GetEnvironmentVariable("MARS_EMAIL");
        string? password = Environment.GetEnvironmentVariable("MARS_PASSWORD");

        Assert.That(email, Is.Not.Null.And.Not.Empty, "MARS_EMAIL is not set.");
        Assert.That(password, Is.Not.Null.And.Not.Empty, "MARS_PASSWORD is not set.");

        using IWebDriver driver = CommonDriver.CreateChromeDriver();

        driver.Navigate().GoToUrl("http://localhost:5003/Home");

        var loginPage = new LoginPage(driver);
        loginPage.Login(email!, password!);

        Assert.That(
            loginPage.IsLoggedInSuccessfully(),
            Is.True,
            "The user should reach the Profile page and see Sign Out."
        );
    }
}