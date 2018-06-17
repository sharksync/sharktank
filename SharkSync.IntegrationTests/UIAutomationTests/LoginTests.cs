using NUnit.Framework;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharkSync.IntegrationTests.UIAutomationTests
{
    public partial class Tests
    {
        [Test]
        public void LoginTests_Github()
        {
            driver.Navigate().GoToUrl("https://www.testingallthethings.net");

            driver.FindElement(By.CssSelector(".navbar-nav")).Click();
            
            driver.FindElement(By.CssSelector(".github-login")).Click();

            Assert.True(driver.Url.Contains("https://github.com/login"));
        }
    }
}
