using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

namespace MarsAdvanced.Tests.Pages
{
    public class LoginPage
    {
        private readonly IWebDriver _driver;
        private readonly WebDriverWait _wait;

        public IWebDriver Driver => _driver;

        // Mars Login Locators
        private readonly By SignInLink = By.XPath("//a[normalize-space()='Sign In']");
        private readonly By EmailField = By.Name("email");
        private readonly By PasswordField = By.Name("password");
        private readonly By LoginButton = By.XPath("//button[normalize-space()='Login']");
        private readonly By SignOutButton = By.XPath("//button[normalize-space()='Sign Out']");

        public LoginPage(IWebDriver driver)
        {
            _driver = driver;
            _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(30));
        }

        public void OpenSignInForm()
        {
            var signInElement = _wait.Until(ExpectedConditions.ElementToBeClickable(SignInLink));
            signInElement.Click();
        }

        public void EnterEmail(string email)
        {
            var emailElement = _wait.Until(ExpectedConditions.ElementIsVisible(EmailField));
            emailElement.Clear();
            emailElement.SendKeys(email);
        }

        public void EnterPassword(string password)
        {
            var passwordElement = _wait.Until(ExpectedConditions.ElementIsVisible(PasswordField));
            passwordElement.Clear();
            passwordElement.SendKeys(password);
        }

        public void ClickLoginButton()
        {
            var loginButtonElement = _wait.Until(ExpectedConditions.ElementToBeClickable(LoginButton));
            loginButtonElement.Click();
        }

        public void Login(string email, string password)
        {
            OpenSignInForm();
            EnterEmail(email);
            EnterPassword(password);
            ClickLoginButton();
        }

        public bool IsLoggedInSuccessfully()
        {
            try
            {
                _wait.Until(driver => driver.Url.Contains("/Account/Profile"));

                var signOutElement = _wait.Until(ExpectedConditions.ElementIsVisible(SignOutButton));

                return _driver.Url.Contains("/Account/Profile") && signOutElement.Displayed;
            }
            catch
            {
                return false;
            }
        }
    }
}