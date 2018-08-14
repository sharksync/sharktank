using NUnit.Framework;

namespace SharkSync.IntegrationTests.UIAutomationTests
{
    public partial class Tests
    {
        [Test]
        public void LoginTests_Github()
        {
            AppHelpers.GotoAppUrl(driver, wait);

            LoginHelpers.SignIsUsingGithub(driver, wait, secrets);
        }

        [Test]
        public void LoginTests_Google()
        {
            AppHelpers.GotoAppUrl(driver, wait);

            LoginHelpers.SignIsUsingGoogle(driver, wait, secrets);
        }

        //[Test]
        //public void LoginTests_Microsoft()
        //{
        //    driver.Navigate().GoToUrl(TestingUrl);

        //    driver.FindElement(By.CssSelector(".navbar-nav")).Click();
        //    wait.Until(ExpectedConditions.UrlToBe(LoginUrl));

        //    driver.FindElement(By.CssSelector(".microsoft-login")).Click();
        //    wait.Until(ExpectedConditions.UrlContains("https://login.microsoftonline.com"));

        //    driver.FindElement(By.CssSelector("input[type=email]")).SendKeys(Secrets.MicrosoftUsername);
        //    driver.FindElement(By.CssSelector("input[type=submit]")).Click();

        //    wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector("input[type=password]")));
        //    driver.FindElement(By.CssSelector("input[type=password]")).SendKeys(Secrets.MicrosoftPassword);
        //    driver.FindElement(By.CssSelector("input[type=submit]")).Click();

        //    try
        //    {
        //        wait.Until(ExpectedConditions.UrlToBe(AppsUrl));
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception("Ended on URL: " + driver.Url, ex);
        //    }
        //}
    }
}
