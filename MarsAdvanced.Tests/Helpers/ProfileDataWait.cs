using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace MarsAdvanced.Tests.Helpers;

public static class ProfileDataWait
{
    public static void WaitForResponse(
        IWebDriver driver,
        string requestName)
    {
        var wait = new WebDriverWait(
            driver,
            TimeSpan.FromSeconds(30)
        );

        const string script = """
            const endpoint = "/" + arguments[0].toLowerCase();

            return performance.getEntriesByType("resource")
                .some(entry => {
                    const path = new URL(entry.name)
                        .pathname.toLowerCase();

                    return path.endsWith(endpoint)
                        && entry.responseEnd > 0;
                });
            """;

        wait.Until(browser =>
            (bool)((IJavaScriptExecutor)browser)
                .ExecuteScript(script, requestName)
        );
    }
}