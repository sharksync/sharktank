using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using OtpNet;
using SeleniumExtras.WaitHelpers;
using System.Threading;

namespace SharkSync.IntegrationTests.UIAutomationTests
{
    public static class LoginHelpers
    {
        public static void SignIsUsingGithub(this IWebDriver driver, WebDriverWait wait, SecretsViewModel secrets)
        {
            driver.FindElement(By.CssSelector(".navbar-nav")).Click();
            wait.Until(ExpectedConditions.UrlToBe(Tests.LoginUrl));

            driver.FindElement(By.CssSelector(".github-login")).Click();
            wait.Until(ExpectedConditions.UrlContains("https://github.com/login"));

            driver.FindElement(By.Id("login_field")).SendKeys(secrets.GithubUsername);
            driver.FindElement(By.Id("password")).SendKeys(secrets.GithubPassword);
            driver.FindElement(By.CssSelector("input[type=submit]")).Click();

            wait.Until(ExpectedConditions.ElementIsVisible(By.Id("otp")));

            byte[] secretKey = Base32Encoding.ToBytes(secrets.GithubTwoFactorCode);
            Totp totp = new Totp(secretKey);
            string code = totp.ComputeTotp();

            driver.FindElement(By.Id("otp")).SendKeys(code);
            driver.FindElement(By.CssSelector("button[type=submit]")).Click();

            wait.Until(ExpectedConditions.UrlToBe(Tests.AppsUrl));

            AppHelpers.WaitForAppContainerToLoad(driver, wait);
        }

        public static void SignIsUsingGoogle(this IWebDriver driver, WebDriverWait wait, SecretsViewModel secrets)
        {
            driver.FindElement(By.CssSelector(".navbar-nav")).Click();
            wait.Until(ExpectedConditions.UrlToBe(Tests.LoginUrl));

            driver.FindElement(By.CssSelector(".google-login")).Click();
            wait.Until(ExpectedConditions.UrlContains("https://accounts.google.com/signin"));

            driver.FindElement(By.CssSelector("input[type=email]")).SendKeys(secrets.GoogleUsername);
            driver.FindElement(By.Id("identifierNext")).Click();

            wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector("input[type=password]")));
            driver.FindElement(By.CssSelector("input[type=password]")).SendKeys(secrets.GooglePassword);
            driver.FindElement(By.Id("passwordNext")).Click();

            wait.Until(ExpectedConditions.ElementIsVisible(By.Id("totpPin")));

            byte[] secretKey = Base32Encoding.ToBytes(secrets.GoogleTwoFactorCode);
            Totp totp = new Totp(secretKey);
            string code = totp.ComputeTotp();

            driver.FindElement(By.Id("totpPin")).SendKeys(code);

            // The button takes a second or so to enable
            Thread.Sleep(500);

            driver.FindElement(By.Id("totpNext")).Click();

            wait.Until(ExpectedConditions.UrlToBe(Tests.AppsUrl));

            AppHelpers.WaitForAppContainerToLoad(driver, wait);
        }
    }
}
