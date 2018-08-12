using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharkSync.IntegrationTests.UIAutomationTests
{
    public static class AppHelpers
    {
        public static void GotoAppUrl(this IWebDriver driver, WebDriverWait wait)
        {
            driver.Navigate().GoToUrl(Tests.TestingUrl);

            WaitForAppContainerToLoad(driver, wait);
        }

        public static void WaitForAppContainerToLoad(this IWebDriver driver, WebDriverWait wait)
        {
            var reactAppContainer = driver.FindElement(By.Id("react-app"));
            wait.Until(e => reactAppContainer.Text != "Loading...");
        }
    }
}
